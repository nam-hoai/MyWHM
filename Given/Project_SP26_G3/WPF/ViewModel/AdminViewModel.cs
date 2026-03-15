using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace WPF.ViewModel
{
    public class AdminViewModel
    {
        public AccountViewModel AccountVM { get; set; }
        public WarehouseViewModel WarehouseVM { get; set; }
        public ProductViewModel ProductVM { get; set; }
        public ReportViewModel ReportVM { get; set; }

        public ICommand OutCammand { get; }

        public AdminViewModel()
        {
            AccountVM = new AccountViewModel();
            WarehouseVM = new WarehouseViewModel();
            ProductVM = new ProductViewModel();
            ReportVM = new ReportViewModel();

            OutCammand = new RelayCommand(Out);
        }
        private void Out()
        {
            MessageBoxResult rs = MessageBox.Show(
                "Do you want exit",
                "Comfirm",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
                );
            if (rs == MessageBoxResult.Yes)
            {
                MessageBox.Show("Thank for using program");
                frmLogin login = new frmLogin();
                //_backlog.SaveToFile(BACKLOG_FILE);
                login.Show();
                Application.Current.MainWindow.Close();
            }
        }
    }
}
