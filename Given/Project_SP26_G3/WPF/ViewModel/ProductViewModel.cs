using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;
using WPF.Models;
using WPF.Service;

namespace WPF.ViewModel
{
    public class ProductViewModel : BaseViewModel
    {
        private readonly IProductService _service;
        private readonly IBackLogService _backlog;
        private readonly ReportService _report;
        public ObservableCollection<Product> Products { get; set; }

        private Product _selectedProduct = null!;
        private void LoadFromProduct(Product product)
        {
            FormProduct = new Product
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                ProductCode = product.ProductCode,
                Sender = product.Sender,
                Location = product.Location,
                Quantity = product.Quantity,
                CatId = product.CatId,
                DateIn = product.DateIn,
                DateOut = product.DateOut,
                Status = product.Status
            };
        }
        public Product SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                _selectedProduct = value;
                (EditCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (DeleteCommand as RelayCommand)?.RaiseCanExecuteChanged();
                OnPropertyChanged();

                if (value != null) LoadFromProduct(value);
            }
        }

        private Product _formProduct = new Product();

        public Product FormProduct
        {
            get => _formProduct;
            set
            {
                _formProduct = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Category> _categories = null!;
        public ObservableCollection<Category> Categories
        {
            get => _categories;
            set
            {
                _categories = value;
                OnPropertyChanged(nameof(Categories));
            }
        }
        private ObservableCollection<Person> _person = null!;
        public ObservableCollection<Person> Persons
        {
            get => _person;
            set
            {
                _person = value;
                OnPropertyChanged(nameof(Persons));
            }
        }
        private ObservableCollection<Warehouse> _locations = null!;
        public ObservableCollection<Warehouse> Locations
        {
            get => _locations;
            set
            {
                _locations = value;
                OnPropertyChanged(nameof(Locations));
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

        public ProductViewModel()
        {
            _service = new ProductService();
            _backlog = new BackLogService();
            _report = ReportService.Instance;
            Products = new ObservableCollection<Product>();
            Categories = new ObservableCollection<Category>();
            Persons = new ObservableCollection<Person>();
            Locations = new ObservableCollection<Warehouse>();
            LoadData();

            AddCommand = new RelayCommand(Add);
            EditCommand = new RelayCommand(Edit, () => SelectedProduct != null);
            DeleteCommand = new RelayCommand(Delete, () => SelectedProduct != null);
            ResetCommand = new RelayCommand(Reset);
            SearchCommand = new RelayCommand(Search);
            BacklogCommand = new RelayCommand(Backlog);
            ReportCommand = new RelayCommand(Report);

        }

        private void LoadData()
        {
            Products = new ObservableCollection<Product>(_service.GetAllProduct());
            OnPropertyChanged(nameof(Products));
            Categories = new ObservableCollection<Category>(_service.GetAllCategory());
            OnPropertyChanged(nameof(Categories));
            Persons = new ObservableCollection<Person>(_service.GetAllUser());
            OnPropertyChanged(nameof(Persons));
            Locations = new ObservableCollection<Warehouse>(_service.GetAllLocation());
            OnPropertyChanged(nameof(Locations));
        }

        private void Add()
        {
            using MyContext context = new MyContext();
            try
            {
                //validation
                if (string.IsNullOrWhiteSpace(FormProduct.ProductName))
                {
                    MessageBox.Show("Name required");
                    return;
                }
                //for quantity
                if (FormProduct.Quantity == null)
                {
                    MessageBox.Show("Quantity is required");
                    return;
                }

                if (FormProduct.Quantity < 0)
                {
                    MessageBox.Show("Quantity must be greater than or equal 0");
                    FormProduct.Quantity = 0;
                    return;
                }
                //for date in stock
                if (FormProduct.DateIn == null)
                {
                    MessageBox.Show("Date of Stockin is required");
                    return;
                }
                if (FormProduct.DateIn > DateTime.Today)
                {
                    MessageBox.Show("You can not stock in product in the puture");
                    return;
                }
                //for date out stock
                if (FormProduct.DateOut < DateTime.Today && FormProduct.DateOut != null)
                {
                    MessageBox.Show("You can not stock out product in the past");
                    return;
                }
                //for location
                if (FormProduct.Location==null)
                {
                    MessageBox.Show("Location is required");
                    return;
                }
                var warehouse = context.Warehouses.FirstOrDefault(x => x.WarehouseName == FormProduct.Location);
                if (warehouse == null)
                {
                    MessageBox.Show($"Location {FormProduct.Location} is not exist.");
                    return;
                }
                if (warehouse.Status == false)
                {
                    MessageBox.Show($"Location {FormProduct.Location} is disabled, try with another");
                    return;
                }
                //for category
                if (FormProduct.CatId==0)
                {
                    MessageBox.Show("Category is required");
                    return;
                }
                // for sender
                if (FormProduct.Sender==null)
                {
                    MessageBox.Show("Sender is required");
                    return;
                }
                else
                {
                    var s = context.Persons.FirstOrDefault(p=>p.PersonName.Equals(FormProduct.Sender));
                    if (s == null)
                    {
                        MessageBox.Show($"{FormProduct.Sender} not exist in database, try with another");
                        return;
                    }
                }
                //generate product code
                var initials = string.Join("", FormProduct.ProductName.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                                        .Select(word => char.ToUpper(word[0])));
                FormProduct.ProductCode = $"{initials}{FormProduct.Quantity}{FormProduct.Location.ToUpper()}";
                _service.Add(FormProduct);
                MessageBox.Show($"Added {FormProduct.ProductName} success");
                Reset();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}; Detail: {ex.InnerException?.Message ?? "N/A"}");
            }
        }

        private void Edit()
        {
            using MyContext context = new MyContext();
            if (SelectedProduct == null) return;
            try
            {
                //validation
                if (string.IsNullOrWhiteSpace(FormProduct.ProductName))
                {
                    MessageBox.Show("Name required");
                    return;
                }
                //for quantity
                if (FormProduct.Quantity == null)
                {
                    MessageBox.Show("Quantity is required");
                    return;
                }

                if (FormProduct.Quantity < 0)
                {
                    MessageBox.Show("Quantity must be greater than or equal 0");
                    FormProduct.Quantity = 0;
                    return;
                }
                //for date in stock
                if (FormProduct.DateIn == null)
                {
                    MessageBox.Show("Date of Stockin is required");
                    return;
                }
                if (FormProduct.DateIn > DateTime.Today)
                {
                    MessageBox.Show("You can not stock in product in the puture");
                    FormProduct.DateIn = DateTime.Today;
                    return;
                }
                if (FormProduct.DateOut!=null && FormProduct.DateOut < FormProduct.DateIn)
                {
                    MessageBox.Show("Date out stock must after date in stock");
                    FormProduct.DateOut = DateTime.Today;
                    return;
                }
                //for location
                if (FormProduct.Location == null)
                {
                    MessageBox.Show("Location is required");
                    return;
                }
                var warehouse = context.Warehouses.FirstOrDefault(x => x.WarehouseName == FormProduct.Location);
                if (warehouse == null)
                {
                    MessageBox.Show($"Location {FormProduct.Location} is not exist.");
                    return;
                }
                if (warehouse.Status == false)
                {
                    MessageBox.Show($"Location {FormProduct.Location} is disabled, try with another");
                    return;
                }
                //for category
                if (FormProduct.CatId == 0)
                {
                    MessageBox.Show("Category is required");
                    return;
                }
                // for sender
                if (FormProduct.Sender == null)
                {
                    MessageBox.Show("Sender is required");
                    return;
                }
                var sender = context.Persons.FirstOrDefault(x=>x.PersonName==FormProduct.Sender);
                if (sender == null)
                {
                    MessageBox.Show($"Sender {FormProduct.Sender} is not exist.");
                    return;
                }
                if (sender.Status == false)
                {
                    MessageBox.Show($"Sender {FormProduct.Sender} is disabled, try with another.");
                    return;
                }
                var oldData = _backlog.CloneEntity(SelectedProduct);
                var newData = _backlog.CloneEntity(FormProduct); // set data update

                var updated = new Product
                {
                    ProductId = SelectedProduct.ProductId,
                    ProductName = FormProduct.ProductName,
                    ProductCode = SelectedProduct.ProductCode,
                    Sender = FormProduct.Sender,
                    Location = FormProduct.Location,
                    Quantity = FormProduct.Quantity,
                    DateIn = FormProduct.DateIn,
                    DateOut = FormProduct.DateOut,
                    CatId = FormProduct.CatId,
                    Status = FormProduct.Status,
                };

                _service.Update(updated);
                _backlog.Push(oldData, newData, "Update", SelectedProduct.ProductId);
                MessageBox.Show($"Update {FormProduct.ProductName} success");
                Reset();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message} - Detail: {ex.InnerException?.Message ?? "N/A"}");
            }
        }

        private void Delete()
        {
            if (SelectedProduct == null) return;
            try
            {
                var oldData = _backlog.CloneEntity(SelectedProduct);
                
                _service.Delete(SelectedProduct);
                _backlog.Push(oldData, null, "Delete", oldData.ProductId);
                MessageBox.Show($"Deleted {SelectedProduct.ProductName} sucess");
                Reset();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message} - Detail: {ex.InnerException?.Message ?? "N/A"}");
            }
        }

        private void Reset()
        {
            LoadData();
            FormProduct = new Product();
            SelectedProduct = null!;
        }
        private void Search()
        {
            string key = $"{FormProduct.ProductName} {FormProduct.ProductCode} {FormProduct.Location} {FormProduct.Sender} {FormProduct.Status}";
            var result = _service.Search(key,FormProduct.DateIn,FormProduct.DateOut);

            Products = new ObservableCollection<Product>(result);

            OnPropertyChanged(nameof(Products));
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
                MessageBox.Show("Undo failed: " + ex.Message);
            }
        }
        private void Report()
        {
            var result = _report.GenerateReport(Products, "Products", "Reports");
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
