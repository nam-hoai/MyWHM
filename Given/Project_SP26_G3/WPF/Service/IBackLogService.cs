using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPF.Models;

namespace WPF.Service
{
    public interface IBackLogService
    {
        int Count { get; }
        IEnumerable<BacklogEntry> GetEntriesOfType<T>();
        bool CheckDuplicateInBacklog<T>(T newObj);
        string SerializeScalar<T>(T entity);
        void Push<T>(T before, T? after, string action, int entityId);
        T CloneEntity<T>(T entity);
        bool HasBacklog();
        void RestoreEntry(BacklogEntry entry, MyContext context);
        void UndoLast();
        void UndoAll();
        void CopyProperties(object source, object target);
        void SaveToFile(string path);
        void LoadFromFile(string path);
    }
}
