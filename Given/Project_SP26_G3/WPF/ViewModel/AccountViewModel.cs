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
        private readonly IBackLogService _backlog;
        private readonly ReportService _report;
        public ObservableCollection<Person> Accounts { get; set; }

        private Person _selectedAccount = null!;
        private void LoadFormAccount(Person person)
        {
            FormAccount = new Person
            {
                PersonId = person.PersonId,
                PersonName = person.PersonName,
                Password = person.Password,
                Birthdate = person.Birthdate,
                Address = person.Address,
                Phone = person.Phone,
                Status = person.Status,
                RoleId = person.RoleId
            };
        }
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
                    LoadFormAccount(value);
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
        public ICommand BacklogCommand { get; }
        public ICommand ReportCommand { get; }

        public AccountViewModel()
        {
            _service = new AccountService();
            _backlog = new BackLogService();
            _report = ReportService.Instance;
            Accounts = new ObservableCollection<Person>();
            LoadData();

            AddCommand = new RelayCommand(Add);
            EditCommand = new RelayCommand(Edit, () => SelectedAccount != null);
            DeleteCommand = new RelayCommand(Delete, () => SelectedAccount != null);
            ResetCommand = new RelayCommand(Reset);
            SearchCommand = new RelayCommand(Search);
            BacklogCommand = new RelayCommand(Backlog);
            ReportCommand = new RelayCommand(Report);

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

                if (_backlog.CheckDuplicateInBacklog(FormAccount))
                {
                    MessageBox.Show($"This {FormAccount.PersonName} has been deleted and stored in Backlog to restore. Please click 'BACKLOG DATA'!", "Dupplication Found",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
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
                MessageBox.Show($"{ex.Message}; Detail: {ex.InnerException?.Message??"N/A"}");
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

                var oldData = _backlog.CloneEntity(SelectedAccount);
                var newData = _backlog.CloneEntity(FormAccount); // set data update

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
                _backlog.Push(oldData, newData, "Update", SelectedAccount.PersonId);
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
                var oldData = _backlog.CloneEntity(SelectedAccount);

                _service.Delete(SelectedAccount);
                _backlog.Push(oldData, null, "Delete", oldData.PersonId);
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

        private void Backlog()
        {
            try
            {
                if (!_backlog.HasBacklog())
                {
                    MessageBox.Show("No backlog to restore!");
                    return;
                }
                var res = MessageBox.Show("Undo all? Yes=all, No=last", "Undo", MessageBoxButton.YesNoCancel);
                if (res == MessageBoxResult.Cancel) return;
                if (res == MessageBoxResult.Yes)
                {
                    _backlog.UndoAll();
                    LoadData();
                    MessageBox.Show("All undone");
                }
                else if (res == MessageBoxResult.No)
                {
                    _backlog.UndoLast();
                    LoadData();
                    MessageBox.Show("Last undone");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Undo failed: {ex.Message} - Detail: {ex.InnerException?.Message??"N/A"}");
            }
        }
        private void Report()
        {
             var result = _report.GenerateReport(Accounts,"Persons", "Reports");
            if (result.IsSuccess)
            {
                MessageBox.Show(result.Message, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(result.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
