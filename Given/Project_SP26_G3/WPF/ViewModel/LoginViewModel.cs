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

        LoginCommand = new RelayCommand(Login);
        CloseCommand = new RelayCommand(Close);
        ForgetCommand = new RelayCommand(ForgetPassword);
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
    private string _password = null!;
    public string Password
    {
        get => _password;
        set
        {
            _password = value;
            OnPropertyChanged();
        }
    }
    public ICommand LoginCommand { get; }

    public ICommand CloseCommand { get; }

    public ICommand ForgetCommand { get; }

    private void Login()
    {
        if (string.IsNullOrEmpty(Username))
        {
            MessageBox.Show("Username is required");
            return;
        }

        if (string.IsNullOrEmpty(Password))
        {
            MessageBox.Show("Password is required");
            return;
        }

        try
        {
            var user = _authenService.Login(Username, Password);

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

    private void ForgetPassword()
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

    private void Close()
    {
        MessageBox.Show("Good Bye!");
        Application.Current.Shutdown();
    }
}