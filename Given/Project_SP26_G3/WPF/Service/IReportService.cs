using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF.Service
{
    public interface IReportService
    {
        ObservableCollection<ReportEntry> Reports { get;}
        void RegisterFormatter<T>(Func<IEnumerable<T>, string> formatter);
        (bool IsSuccess,string Message) GenerateReport<T>(IEnumerable<T> data, string title, string folder); //Tuple
        (bool IsSuccess,string Message) ExportReport(ReportEntry report, string outputFolder);
        (bool IsSuccess, String Message) DeleteReport(ReportEntry report);
        (bool IsSuccess, string Message) SearchReports(string folderPath);
        (bool IsSuccess, string Message) SearchReports(string folderPath, string keyword);
    }
}
