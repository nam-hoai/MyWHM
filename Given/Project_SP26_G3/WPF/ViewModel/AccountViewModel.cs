using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
                        Status = value.Status
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

            LoadData();
        }
        private void LoadData()
        {
            Accounts = new ObservableCollection<Person>(_service.GetAll());
        }
        private void Add()
        {
            _service.Add(FormAccount);
            LoadData();
            Reset();
        }
        private void Edit()
        {
            if (SelectedAccount == null) return;

            _service.Update(SelectedAccount);
            LoadData();
            Reset();
        }
        private void Delete()
        {
            if (SelectedAccount == null) return;
            _service.Delete(SelectedAccount.PersonId);
            LoadData();
            Reset();
        }
        //search by name - i'll update it to search by all thing soon
        private void Search()
        {
            string key = $"{FormAccount.PersonName} {FormAccount.Phone} {FormAccount.Address}";
            var result = _service.Search(key);

            Accounts = new ObservableCollection<Person>(result);

            OnPropertyChanged(nameof(Accounts));
        }
        private void Reset()
        {
            FormAccount = new Person();
            SelectedAccount = null!;
        }
    }
}
