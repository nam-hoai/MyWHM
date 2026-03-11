using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPF.ViewModel
{
    public class AdminViewModel
    {
        public AccountViewModel AccountVM { get; set; }
        public WarehouseViewModel WarehouseVM { get; set; }
        public ProductViewModel ProductVM { get; set; }
        public ReportViewModel ReportVM { get; set; }

        public AdminViewModel()
        {
            AccountVM = new AccountViewModel();
            WarehouseVM = new WarehouseViewModel();
            ProductVM = new ProductViewModel();
            ReportVM = new ReportViewModel();
        }
    }
}
