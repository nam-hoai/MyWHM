using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WPF.ViewModel;

namespace WPF
{
    /// <summary>
    /// Interaction logic for frmLogin.xaml
    /// </summary>
    public partial class frmLogin : Window
    {
        public frmLogin()
        {
            InitializeComponent();
            DataContext = new LoginViewModel();
        }
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            txtVisiblePassword.Text = passBox.Password;
        }
        private void ShowPassword_Checked(object sender, RoutedEventArgs e)
        {
            txtVisiblePassword.Text = passBox.Password;

            txtVisiblePassword.Visibility = Visibility.Visible;
            passBox.Visibility = Visibility.Collapsed;
        }

        private void ShowPassword_Unchecked(object sender, RoutedEventArgs e)
        {
            passBox.Password = txtVisiblePassword.Text;

            passBox.Visibility = Visibility.Visible;
            txtVisiblePassword.Visibility = Visibility.Collapsed;
        }
    }
}
