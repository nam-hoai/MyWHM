using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WPF.Service;

namespace WPF.ViewModel;
public class LoginViewModel : BaseViewModel
{
    private readonly IAuthenService _authenService;

    public LoginViewModel()
    {
        _authenService = new AuthenService();

        LoginCommand = new RelayCommand<object>(Login);
        CloseCommand = new RelayCommand<object>(Close);
        ForgetCommand = new RelayCommand<object>(ForgetPassword);
    }

    private string _username = null!;

    public string Username
    {
        get => _username;
        set
        {
            _username = value;
            OnPropertyChanged(nameof(Username));
        }
    }

    public ICommand LoginCommand { get; }

    public ICommand CloseCommand { get; }

    public ICommand ForgetCommand { get; }

    private void Login(object parameter)
    {
        var passBox = parameter as PasswordBox;

        if (passBox == null)
            return;

        string password = passBox.Password;

        if (string.IsNullOrEmpty(Username))
        {
            MessageBox.Show("Username is required");
            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            MessageBox.Show("Password is required");
            return;
        }

        try
        {
            var user = _authenService.Login(Username, password);

            if (user == null)
            {
                MessageBox.Show("Invalid username or password");
                return;
            }

            MessageBox.Show("Login success");

            if (user.RoleId == 1)
            {
                new WPF.frmAdmin(Username).Show();
            }
            else if (user.RoleId == 2)
            {
                new WPF.frmGuest(Username).Show();
            }

            Application.Current.Windows[0]?.Close();
        }
        catch (System.Exception ex)
        {
            MessageBox.Show("Login fail: " + ex.Message);
        }
    }

    private void ForgetPassword(object parameter)
    {
        if (string.IsNullOrEmpty(Username))
        {
            MessageBox.Show("Enter username first");
            return;
        }

        var user = _authenService.GetUser(Username);

        if (user == null)
        {
            MessageBox.Show("Username does not exist");
        }
        else
        {
            MessageBox.Show("Password: " + user.Password);
        }
    }

    private void Close(object parameter)
    {
        MessageBox.Show("Good Bye!");
        Application.Current.Shutdown();
    }
}