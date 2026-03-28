using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using WPF.Models;
using Microsoft.EntityFrameworkCore;
using System.IO;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Linq.Expressions;
#pragma warning disable EF1002
namespace WPF.Service
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata;

    public class BacklogEntry
    {
        public string EntityTypeName { get; set; } = null!;
        public int EntityId { get; set; }
        public string? BeforeJson { get; set; }
        public string? AfterJson { get; set; }
        public string Action { get; set; } = null!; // "Update" or "Delete"
        public DateTime Timestamp { get; set; }
    }

    public class BackLogService : IBackLogService
    {

        private readonly Stack<BacklogEntry> _stack = new();
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly string _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "backlog.json");

        public int EntryTtlSeconds { get; set; } = 30;

        public BackLogService()
        {
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = false,
                ReferenceHandler = ReferenceHandler.IgnoreCycles,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
            LoadFromFile(_filePath);
        }

        public int Count => _stack.Count;
        public bool HasBacklog() => Count > 0;

        // Lọc Entry theo loại
        public IEnumerable<BacklogEntry> GetEntriesOfType<T>()
        {
            var typeName = typeof(T).AssemblyQualifiedName;
            return _stack.Where(e => e.EntityTypeName == typeName);
        }

        // Serialize object (chỉ lấy Scalar property)
        public string SerializeScalar<T>(T entity)
        {
            if (entity == null) return "";
            return JsonSerializer.Serialize(entity, _jsonOptions);
        }

        // Thêm vào Backlog
        public void Push<T>(T before, T? after, string action, int entityId)
        {
            // Clone để tách rời entity khỏi sự theo dõi (tracking) của EF Core
            // và loại bỏ các Navigation Properties trước khi Serialize
            var cleanBefore = CloneEntity(before);
            var cleanAfter = after != null ? CloneEntity(after) : default;

            var entry = new BacklogEntry
            {
                EntityTypeName = typeof(T).AssemblyQualifiedName!,
                EntityId = entityId,
                BeforeJson = SerializeScalar(cleanBefore),
                AfterJson = SerializeScalar(cleanAfter),
                Action = action,
                Timestamp = DateTime.Now
            };
            _stack.Push(entry);
        }

        // ==========================================
        // PHẦN XỬ LÝ UNDO & TRANSACTION MỚI
        // ==========================================

        public void UndoLast()
        {
            if (!_stack.Any()) return;

            using var context = new MyContext();
            using var trans = context.Database.BeginTransaction(); // Mở transaction ở ngoài cùng
            try
            {
                var entry = _stack.Pop();
                RestoreEntry(entry, context);

                trans.Commit(); // Hoàn tất an toàn
                SaveToFile(_filePath);
            }
            catch (Exception ex)
            {
                trans.Rollback();
                // Đẩy lại vào stack nếu lỗi để không mất track
                // _stack.Push(entry); 
                Console.WriteLine($"UndoLast failed: {ex.Message}");
            }
        }

        public void UndoAll()
        {
            if (!_stack.Any()) return;

            using var context = new MyContext();
            using var trans = context.Database.BeginTransaction();
            try
            {
                while (_stack.Any())
                {
                    var entry = _stack.Pop();
                    RestoreEntry(entry, context);
                }

                trans.Commit();
                SaveToFile(_filePath);
            }
            catch (Exception ex)
            {
                trans.Rollback();
                Console.WriteLine($"UndoAll failed: {ex.Message}");
            }
        }

        public void RestoreEntry(BacklogEntry entry, MyContext context)
        {
            var type = Type.GetType(entry.EntityTypeName);
            if (type == null) return;

            var entityType = context.Model.FindEntityType(type);
            if (entityType == null) return;

            if (entry.Action == "Update")
            {
                RestoreUpdate(entry, type, entityType, context);
            }
            else if (entry.Action == "Delete")
            {
                RestoreDelete(entry, type, entityType, context);
            }
        }

        private void RestoreUpdate(BacklogEntry entry, Type type, IEntityType entityType, MyContext context)
        {
            if (string.IsNullOrEmpty(entry.BeforeJson)) return;

            var beforeObj = JsonSerializer.Deserialize(entry.BeforeJson, type, _jsonOptions);
            if (beforeObj == null) return;

            var existing = context.Find(type, entry.EntityId);
            if (existing == null) throw new Exception("Không tìm thấy dữ liệu gốc để Undo Update.");

            if (!ValidateForeignKeys(beforeObj, entityType, context))
            {
                throw new Exception("Lỗi Khóa Ngoại (FK): Dữ liệu tham chiếu không còn tồn tại.");
            }

            // SỬ DỤNG COPYPROPERTIES TẠI ĐÂY
            CopyProperties(beforeObj, existing);

            // Đánh dấu entity đã bị thay đổi (đề phòng CopyProperties không kích hoạt change tracker)
            context.Entry(existing).State = EntityState.Modified;

            context.SaveChanges();
        }

        private void RestoreDelete(BacklogEntry entry, Type type, IEntityType entityType, MyContext context)
        {
            if (string.IsNullOrEmpty(entry.BeforeJson)) return;

            var obj = JsonSerializer.Deserialize(entry.BeforeJson, type, _jsonOptions);
            if (obj == null) return;

            var existing = context.Find(type, entry.EntityId);
            if (existing != null) throw new Exception("Entity đã tồn tại, không thể Undo Delete.");

            if (!ValidateForeignKeys(obj, entityType, context))
                throw new Exception("Lỗi Khóa Ngoại (FK): Dữ liệu tham chiếu không còn tồn tại.");

            bool isIdentity = HasIdentityKey(entityType);
            var tableName = entityType.GetTableName();
            var schema = entityType.GetSchema() ?? "dbo";

            try
            {
                if (isIdentity)
                {
                    // Ép mở Connection để đảm bảo ExecuteSqlRaw và SaveChanges xài chung 1 Session
                    context.Database.OpenConnection();
                    context.Database.ExecuteSqlRaw($"SET IDENTITY_INSERT [{schema}].[{tableName}] ON");
                }

                context.Add(obj);
                context.SaveChanges();
            }
            finally
            {
                // FINALLY đảm bảo dù SaveChanges thành công hay thất bại (Exception), 
                // nó VẪN SẼ TẮT Identity Insert đi, trả lại sự bình yên cho Database.
                if (isIdentity)
                {
                    context.Database.ExecuteSqlRaw($"SET IDENTITY_INSERT [{schema}].[{tableName}] OFF");
                    context.Database.CloseConnection();
                }
            }
        }

        // ==========================================
        // CÁC HÀM TIỆN ÍCH (UTILITIES)
        // ==========================================

        // 1. Tạo bản sao entity (chỉ lấy thuộc tính cơ bản)
        public T CloneEntity<T>(T entity)
        {
            if (entity == null) return default!;
            var type = typeof(T);
            var clone = Activator.CreateInstance<T>();

            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!prop.CanRead || !prop.CanWrite) continue;
                // Bỏ qua navigation properties (class type ngoại trừ string)
                if (prop.PropertyType.IsClass && prop.PropertyType != typeof(string)) continue;

                var value = prop.GetValue(entity);
                prop.SetValue(clone, value);
            }
            return clone;
        }

        // 2. Copy dữ liệu từ object này sang object khác (Dùng khi cần update tracked entity)
        public void CopyProperties(object source, object target)
        {
            if (source == null || target == null) return;

            var sourceType = source.GetType();
            var targetType = target.GetType();

            foreach (var prop in sourceType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!prop.CanRead) continue;

                var targetProp = targetType.GetProperty(prop.Name);
                if (targetProp == null || !targetProp.CanWrite) continue;

                var value = prop.GetValue(source);
                if (value == null)
                {
                    targetProp.SetValue(target, null);
                    continue;
                }

                var targetTypeProp = Nullable.GetUnderlyingType(targetProp.PropertyType) ?? targetProp.PropertyType;

                if (!targetTypeProp.IsAssignableFrom(value.GetType()))
                    continue;

                // Bỏ qua navigation properties
                if (targetTypeProp.IsClass && targetTypeProp != typeof(string))
                    continue;

                targetProp.SetValue(target, value);
            }
        }

        // 3. Kiểm tra xem thao tác có bị trùng lặp (spam) trong backlog không
        public bool CheckDuplicateInBacklog<T>(T newObj)
        {
            var type = typeof(T);

            // Dùng switch expression (C# 8.0+) nhìn code sẽ "sạch" và dễ bảo trì hơn chuỗi if-else
            string[] uniqueProps = type.Name switch
            {
                "Product" => new[] { "ProductCode" },
                "Person" => new[] { "PersonName" },
                "Warehouse" => new[] { "WarehouseName" },
                _ => Array.Empty<string>()
            };

            if (uniqueProps.Length == 0) return false;

            foreach (var entry in GetEntriesOfType<T>())
            {
                if (string.IsNullOrEmpty(entry.BeforeJson)) continue;

                // FIX: Đã bổ sung _jsonOptions để Deserialize không bị lỗi format
                var obj = JsonSerializer.Deserialize(entry.BeforeJson, type, _jsonOptions);
                if (obj == null) continue;

                var beforeObj = (T)obj;

                // So sánh các thuộc tính unique
                bool isDuplicate = uniqueProps
                    .Where(propName => type.GetProperty(propName) != null) // Bỏ qua nếu property không tồn tại
                    .All(propName =>
                    {
                        var prop = type.GetProperty(propName)!;
                        var beforeVal = prop.GetValue(beforeObj);
                        var newVal = prop.GetValue(newObj);
                        return Equals(beforeVal, newVal);
                    });

                if (isDuplicate) return true;
            }

            return false;
        }

        // ==========================================
        // PHẦN EXPRESSION TREE XỬ LÝ FK THÔNG MINH
        // ==========================================
        private bool ValidateForeignKeys(object entity, IEntityType entityType, MyContext context)
        {
            foreach (var fk in entityType.GetForeignKeys())
            {
                var principalType = fk.PrincipalEntityType.ClrType;
                var dependentProps = fk.Properties;
                var principalProps = fk.PrincipalKey.Properties;

                var param = Expression.Parameter(principalType, "x");
                Expression? body = null;

                for (int i = 0; i < dependentProps.Count; i++)
                {
                    var fkProp = dependentProps[i];
                    var pkProp = principalProps[i];

                    var value = entity.GetType().GetProperty(fkProp.Name)?.GetValue(entity);

                    // Nếu khóa ngoại cho phép NULL và giá trị đang NULL -> Hợp lệ, bỏ qua check
                    if (value == null) continue;

                    Expression left = Expression.Property(param, pkProp.Name);
                    Expression right = Expression.Constant(value);

                    // FIX LỖI CRASH: Ép kiểu an toàn nếu 2 vế khác nhau (VD: int? và int)
                    if (left.Type != right.Type)
                    {
                        right = Expression.Convert(right, left.Type);
                    }

                    var equal = Expression.Equal(left, right);
                    body = body == null ? equal : Expression.AndAlso(body, equal);
                }

                if (body == null) continue;

                var lambda = Expression.Lambda(body, param);

                // Gọi hàm Set<T>().Any(lambda) qua Reflection
                var setMethod = typeof(DbContext).GetMethod(nameof(DbContext.Set), Type.EmptyTypes)!.MakeGenericMethod(principalType);
                var queryable = setMethod.Invoke(context, null) as IQueryable;

                var anyMethod = typeof(Queryable).GetMethods()
                    .First(m => m.Name == "Any" && m.GetParameters().Length == 2)
                    .MakeGenericMethod(principalType);

                var exists = (bool)anyMethod.Invoke(null, new object[] { queryable!, lambda })!;

                if (!exists) return false;
            }

            return true;
        }

        private bool HasIdentityKey(IEntityType entityType)
        {
            var key = entityType.FindPrimaryKey();
            if (key == null) return false;
            var prop = key.Properties.First();
            return prop.ValueGenerated == ValueGenerated.OnAdd;
        }

        // ==========================================
        // QUẢN LÝ FILE JSON (ĐÃ FIX LỖI ĐẢO NGƯỢC STACK)
        // ==========================================
        public void SaveToFile(string path)
        {
            try
            {
                var list = _stack.ToList();
                var json = JsonSerializer.Serialize(list, _jsonOptions);
                File.WriteAllText(path, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi khi lưu file backlog: " + ex.Message);
            }
        }

        public void LoadFromFile(string path)
        {
            if (!File.Exists(path)) return;
            try
            {
                var json = File.ReadAllText(path);
                var list = JsonSerializer.Deserialize<List<BacklogEntry>>(json, _jsonOptions);
                if (list == null) return;

                // FIX: Reverse list để khi Push lại, thứ tự LIFO được giữ nguyên
                list.Reverse();

                DateTime now = DateTime.Now;
                foreach (var entry in list)
                {
                    var ageSec = (now - entry.Timestamp).TotalSeconds;
                    if (ageSec <= EntryTtlSeconds)
                    {
                        _stack.Push(entry);
                    }
                }
            }
            catch (Exception ex)
            {
                File.Delete(path); // File lỗi thì xóa luôn
                Console.WriteLine("Lỗi khi load file backlog: " + ex.Message);
            }
        }
    }
}
