using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WPF.ViewModel;

namespace WPF.Service
{
    public class ReportEntry : BaseViewModel
    {
        private string _fileName = null!;
        public string FileName
        {
            get => _fileName;
            set { _fileName = value; OnPropertyChanged(); }
        }

        private string _filePath = null!;
        public string FilePath
        {
            get => _filePath;
            set { _filePath = value; OnPropertyChanged(); }
        }

        private DateTime _createdAt;
        public DateTime CreatedAt
        {
            get => _createdAt;
            set { _createdAt = value; OnPropertyChanged(); }
        }

        private long _fileSize;
        public long FileSize
        {
            get => _fileSize;
            set { _fileSize = value; OnPropertyChanged(); }
        }

        private string _content = null!;
        public string Content
        {
            get => _content;
            set { _content = value; OnPropertyChanged(); }
        }
    }
    public class ReportService : IReportService
    {
        // Giữ instance duy nhất nếu bạn không dùng DI Container (như Autofac/Microsoft DI)
        private static ReportService _instance = null!;
        public static ReportService Instance => _instance ??= new ReportService();

        private readonly Dictionary<Type, Func<IEnumerable<object>, string>> _formatters = new();
        private readonly Stack<ReportEntry> _backlog = new(); // dùng cho undo delete
        public ObservableCollection<ReportEntry> Reports { get; } = new ObservableCollection<ReportEntry>();
        private ReportService()
        {

        }
        public void RegisterFormatter<T>(Func<IEnumerable<T>, string> formatter)
        {
            _formatters[typeof(T)] = objs => formatter(objs.Cast<T>());
        }
        //generare report using formatter customize
        public (bool IsSuccess, String Message) GenerateReport<T>(IEnumerable<T> data, string title, string folder)
        {
            try
            {
                if (data == null || !data.Any())
                {
                    return (false, "No data to generate report.");
                }
                //create folder if not exist
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                string text = GenerateReportText(data, title);

                string fileName = $"{title}_{DateTime.Now:yyyyMMdd_HHmmss}.txt";

                string filePath = Path.Combine(folder, fileName);

                File.WriteAllText(filePath, text, Encoding.UTF8);

                // thêm vào backlog (chưa load vào datagrid)
                var newReport = new ReportEntry
                {
                    FileName = fileName,
                    FilePath = filePath,
                    CreatedAt = DateTime.Now,
                    FileSize = new FileInfo(filePath).Length,
                    Content = text
                };
                _backlog.Push(newReport);

                // Tự động thêm vào danh sách hiển thị
                Reports.Add(newReport);

                return (true, $"Report [{fileName}] generated successfully.");
            }
            catch (Exception ex)
            {
                return (false, $"Failed to generate report: {ex.Message}");
            }
        }
        //support method: using custom formatter
        private string GenerateReportText<T>(IEnumerable<T> data, string title)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"=== {title.ToUpper()} REPORT ===");
            sb.AppendLine($"Generated: {DateTime.Now}");
            sb.AppendLine(new string('-', 60));

            if (_formatters.TryGetValue(typeof(T), out var formatter))
            {
                sb.AppendLine(formatter(data.Cast<object>()));
            }
            else
            {
                foreach (var item in data)
                {
                    foreach (var prop in typeof(T).GetProperties())
                    {
                        var val = prop.GetValue(item) ?? "";
                        sb.AppendLine($"{prop.Name}: {val}");
                    }
                    sb.AppendLine(new string('-', 60));
                }
            }

            sb.AppendLine($"Total: {data.Count()} records");
            return sb.ToString();
        }
        //Search + Load
        public (bool IsSuccess, string Message) SearchReports(string folderPath)
        {
            Reports.Clear();
            if (!Directory.Exists(folderPath))
            {
                return (false, "Report folder not found!");
            }

            var files = Directory.GetFiles(folderPath, "*.txt");
            if (files.Length == 0)
            {
                return (false, "No report files found.");
            }

            foreach (var file in files)
            {
                var fi = new FileInfo(file);
                Reports.Add(new ReportEntry
                {
                    FileName = fi.Name,
                    FilePath = fi.FullName,
                    CreatedAt = fi.CreationTime,
                    FileSize = fi.Length,
                    Content = File.ReadAllText(fi.FullName)
                });
            }
            return (true, "Already");
        }
        public (bool IsSuccess, String Message) SearchReports(string folderPath, string keyword)
        {
            Reports.Clear();
            if (!Directory.Exists(folderPath))
                return (false, "Report folder not found!");

            var files = Directory.GetFiles(folderPath, "*.txt");

            // lọc theo keyword trong tên file
            files = files
                .Where(f => Path.GetFileName(f)
                .Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            if (files.Length == 0)
                return (false, "No matching report files found.");
            

            foreach (var file in files)
            {
                var fi = new FileInfo(file);
                Reports.Add(new ReportEntry
                {
                    FileName = fi.Name,
                    FilePath = fi.FullName,
                    CreatedAt = fi.CreationTime,
                    FileSize = fi.Length,
                    Content = File.ReadAllText(fi.FullName)
                });
            }
            return (true, "Already");
        }
        //Export report
        public (bool IsSuccess, String Message) ExportReport(ReportEntry report, string outputFolder)
        {
            try
            {
                if (!Directory.Exists(outputFolder))
                    Directory.CreateDirectory(outputFolder);

                string path = Path.Combine(outputFolder, report.FileName);
                File.WriteAllText(path, report.Content, Encoding.UTF8);
                return (true, $"Report exported to {path}");
            }
            catch (Exception ex)
            {
                return (false, $"Export failed: {ex.Message}");
            }
        }
        //delete
        public (bool IsSuccess, String Message) DeleteReport(ReportEntry report)
        {
            if (report == null) return (false, "No found file to delete");

            _backlog.Push(report); // backup để undo
            Reports.Remove(report);
            
            try
            {
                if (File.Exists(report.FilePath))
                    File.Delete(report.FilePath);
                return (true, "Delete success");
            }
            catch (Exception ex)
            {
                return (false, $"Failed to delete file: {ex.Message}");
            }
        }
        //read content
        public string GetReportContent(ReportEntry? selected)
        {
            if (selected == null) return "";
            if (File.Exists(selected.FilePath))
                return File.ReadAllText(selected.FilePath);
            else
                return selected.Content ?? "";
        }
        //backlog
        public void UndoDelete()
        {
            if (_backlog.Count == 0)
                MessageBox.Show("No deleted reports to restore.");

            var entry = _backlog.Pop();
            Reports.Add(entry);

            // Nếu file bị xóa, ghi lại file để khôi phục vật lý
            try
            {
                File.WriteAllText(entry.FilePath, entry.Content, Encoding.UTF8);
                MessageBox.Show($"Report [{entry.FileName}] restored.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Restore failed: {ex.Message}");
            }
        }
    }
}
