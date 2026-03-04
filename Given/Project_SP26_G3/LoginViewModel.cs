using System.Windows;
using System.Windows.Input;

public class LoginViewModel
{
    public ICommand LoginCommand { get; }
    public ICommand ForgetCommand { get; }
    public ICommand CloseCommand { get; }

    public LoginViewModel()
    {
        LoginCommand = new RelayCommand(Login);
        ForgetCommand = new RelayCommand(Forget);
        CloseCommand = new RelayCommand(Close);
    }

    private void Login(object obj)
    {
        MessageBox.Show("Login logic here");
    }

    private void Forget(object obj)
    {
        MessageBox.Show("Forget logic here");
    }

    private void Close(object obj)
    {
        Application.Current.Shutdown();
    }
}