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
        ObservableCollection<ReportEntry> Reports { get;  set; }
    }
}
