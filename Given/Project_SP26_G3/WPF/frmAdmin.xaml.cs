using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WPF.ViewModel;

namespace WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class frmAdmin : Window
    {
        public frmAdmin(string name)
        {
            InitializeComponent();
            Title = $"Wlecome {name}";
            DataContext = new AdminViewModel();
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text, 0);
        }
        private void NumberOnly_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                var text = (string)e.DataObject.GetData(typeof(string));
                if (!int.TryParse(text, out _))
                    e.CancelCommand();
            }
            else
            {
                e.CancelCommand();
            }
        }

        private void Size_LostFocus(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;

            if (string.IsNullOrWhiteSpace(tb.Text))
            {
                MessageBox.Show("Size is required");
                tb.Text = "0";
            }
        }
    }
}