using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WPF.Models;
using WPF.Service;

namespace WPF.ViewModel
{
    public class AccountViewModel : BaseViewModel
    {
        private readonly IAccountService _service;
        public ObservableCollection<Person> Accounts { get; set; }

        private Person _selectedAccount = null!;
        public Person SelectedAccount
        {
            get => _selectedAccount;
            set
            {
                _selectedAccount = value;
                (EditCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (DeleteCommand as RelayCommand)?.RaiseCanExecuteChanged();
                OnPropertyChanged();

                if (value != null)
                {
                    FormAccount = new Person
                    {
                        PersonId = value.PersonId,
                        PersonName = value.PersonName,
                        Password = value.Password,
                        Birthdate = value.Birthdate,
                        Address = value.Address,
                        Phone = value.Phone,
                        Status = value.Status,
                        RoleId = value.RoleId
                    };
                }
            }
        }

        private Person _formAccount = new Person();
        public Person FormAccount
        {
            get => _formAccount;
            set
            {
                _formAccount = value;
                OnPropertyChanged();
            }
        }
        private bool _isStatusFilterEnabled = false;
        public bool IsStatusFilterEnabled
        {
            get => _isStatusFilterEnabled;
            set
            {
                _isStatusFilterEnabled = value;
                OnPropertyChanged();
            }
        }

        private bool _statusFilter = true;   // UI default: Active
        public bool StatusFilter
        {
            get => _statusFilter;
            set
            {
                _statusFilter = value;
                IsStatusFilterEnabled = true;   // active filter if only click radiobutton
                OnPropertyChanged();
            }
        }
        public ICommand AddCommand { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ResetCommand { get; }
        public ICommand SearchCommand { get; }
        //public ICommand BacklogCommand { get; }
        //public ICommand ReportCommand { get; }

        public AccountViewModel()
        {
            _service = new AccountService();
            Accounts = new ObservableCollection<Person>();
            LoadData();

            AddCommand = new RelayCommand(Add);
            EditCommand = new RelayCommand(Edit, () => SelectedAccount != null);
            DeleteCommand = new RelayCommand(Delete, () => SelectedAccount != null);
            ResetCommand = new RelayCommand(Reset);
            SearchCommand = new RelayCommand(Search);
            //BacklogCommand = new RelayCommand(Backlog);
            //ReportCommand = new RelayCommand(Report);

        }
        private void LoadData()
        {
            Accounts = new ObservableCollection<Person>(_service.GetAll());
            OnPropertyChanged(nameof(Accounts));
        }
        private void Add()
        {
            try
            {
                //validation
                if (string.IsNullOrWhiteSpace(FormAccount.PersonName))
                {
                    MessageBox.Show("Name required");
                    return;
                }
                if (FormAccount.Birthdate == null)
                {
                    MessageBox.Show("Birthdate required");
                    return;
                }
                if (FormAccount.Password == null)
                {
                    MessageBox.Show("Password required");
                    return;
                }
                //set role for normal user
                FormAccount.RoleId = 2;

                _service.Add(FormAccount);
                MessageBox.Show($"Added {FormAccount.PersonName} success");
                Reset();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}; Detail: {ex.InnerException}");
            }
        }
        private void Edit()
        {
            if (SelectedAccount == null) return;
            try
            {
                if (string.IsNullOrWhiteSpace(FormAccount.PersonName))
                {
                    MessageBox.Show("Person name is required");
                    return;
                }
                var updated = new Person
                {
                    PersonId = SelectedAccount.PersonId,
                    PersonName = FormAccount.PersonName,
                    Password = FormAccount.Password,
                    Address = FormAccount.Address,
                    Phone = FormAccount.Phone,
                    Birthdate = FormAccount.Birthdate,
                    Status = FormAccount.Status,
                    RoleId = 2 //Default RoleId = 2 for normal user
                };

                _service.Update(updated);
                MessageBox.Show($"Update {FormAccount.PersonName} success");
                Reset();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message} - Detail: {ex.InnerException?.Message ?? "N/A"}");
            }
        }
        private void Delete()
        {
            if (SelectedAccount == null) return;
            try
            {
                _service.Delete(SelectedAccount);
                MessageBox.Show($"Deleted {SelectedAccount.PersonName} sucess");
                Reset();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message} - Detail: {ex.InnerException?.Message ?? "N/A"}");
            }
            
        }
        //search by name - i'll update it to search by all thing soon
        private void Search()
        {
            string key = $"{FormAccount.PersonName} {FormAccount.Phone} {FormAccount.Address} {FormAccount.Status}";
            var result = _service.Search(key);

            Accounts = new ObservableCollection<Person>(result);

            OnPropertyChanged(nameof(Accounts));
        }
        private void Reset()
        {
            LoadData();
            FormAccount = new Person();
            SelectedAccount = null!;
        }
    }
}
