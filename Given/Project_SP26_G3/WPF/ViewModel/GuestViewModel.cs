using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using WPF.Models;
using System.Collections.ObjectModel;

namespace WPF.ViewModel
{
    public class GuestViewModel : BaseViewModel
    {
        public ObservableCollection<Product> Products { get; set; }
        public ICommand AddCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand ResetCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand OutCommand { get; }

        public GuestViewModel()
        {
            Products = new ObservableCollection<Product>();

            AddCommand = new RelayCommand(Add);
            ExportCommand = new RelayCommand(Export);
            ResetCommand = new RelayCommand(Reset);
            SearchCommand = new RelayCommand(Search);
            OutCommand = new RelayCommand<Window>(Out);
        }

        private void Add()
        {
            //code
        }

        private void Export()
        {
            //code
        }

        private void Reset()
        {
            //code
        }

        private void Search()
        {
            //code
        }

        private void Out(Window window)
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

                login.Show();
                window.Close();
            }
        }
    }
}
