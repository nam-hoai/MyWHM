using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WPF.Service
{
    public class ReportEntry
    {
        public string FileName { get; set; } = null!;
        public string FilePath { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public long FileSize { get; set; }
        public string Content { get; set; } = null!;
    }

    public class ReportService : IReportService
    {
        private readonly Dictionary<Type, Func<IEnumerable<object>, string>> _formatters = new();
        private readonly Stack<ReportEntry> _backlog = new(); // dùng cho undo delete
        public ObservableCollection<ReportEntry> Reports { get; set; } = new();

        public void RegisterFormatter<T>(Func<IEnumerable<T>, string> formatter)
        {
            _formatters[typeof(T)] = objs => formatter(objs.Cast<T>());
        }
        //generare report using formatter customize
        public bool GenerateReport<T>(IEnumerable<T> data, string title, string folder)
        {
            try
            {
                if (data == null || !data.Any())
                {
                    MessageBox.Show("No data to generate report.");
                    return false;
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
                _backlog.Push(new ReportEntry
                {
                    FileName = fileName,
                    FilePath = filePath,
                    CreatedAt = DateTime.Now,
                    FileSize = new FileInfo(filePath).Length,
                    Content = text
                });

                MessageBox.Show($"Report [{fileName}] generated successfully.");
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to generate report: " + ex.Message);
                return false;
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
        public void SearchReports(string folderPath)
        {
            Reports.Clear();
            if (!Directory.Exists(folderPath))
            {
                MessageBox.Show("Report folder not found!");
                return;
            }

            var files = Directory.GetFiles(folderPath, "*.txt");
            if (files.Length == 0)
            {
                MessageBox.Show("No report files found.");
                return;
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
        }
        public void SearchReports(string folderPath, string keyword)
        {
            Reports.Clear();

            if (!Directory.Exists(folderPath))
            {
                MessageBox.Show("Report folder not found!");
                return;
            }

            var files = Directory.GetFiles(folderPath, "*.txt");

            // lọc theo keyword trong tên file
            files = files
                .Where(f => Path.GetFileName(f)
                .Contains(keyword, StringComparison.OrdinalIgnoreCase))
                .ToArray();

            if (files.Length == 0)
            {
                MessageBox.Show("No matching report files found.");
                return;
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
        }
        //Export report
        public bool ExportReport(ReportEntry report, string outputFolder)
        {
            try
            {
                if (!Directory.Exists(outputFolder))
                    Directory.CreateDirectory(outputFolder);

                string path = Path.Combine(outputFolder, report.FileName);
                File.WriteAllText(path, report.Content, Encoding.UTF8);
                MessageBox.Show($"Report exported to {path}");
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Export failed: " + ex.Message);
                return false;
            }
        }
        //delete
        public void DeleteReport(ReportEntry report)
        {
            if (report == null) return;

            _backlog.Push(report); // backup để undo
            Reports.Remove(report);

            try
            {
                if (File.Exists(report.FilePath))
                    File.Delete(report.FilePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to delete file: " + ex.Message);
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
            {
                MessageBox.Show("No deleted reports to restore.");
                return;
            }

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
                MessageBox.Show("Restore failed: " + ex.Message);
            }
        }
    }
}
