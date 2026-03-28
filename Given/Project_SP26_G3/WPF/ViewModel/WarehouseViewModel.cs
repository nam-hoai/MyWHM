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
    public class WarehouseViewModel : BaseViewModel
    {
        private readonly IWareHouseService _service;
        private readonly IBackLogService _backlog;
        public ObservableCollection<Warehouse> Warehouses { get; set; }

        private Warehouse _selectedWarehouse = null!;

        private void LoadFromWarehouse(Warehouse house)
        {
            FormWarehouse = new Warehouse
            {
                WarehouseId = house.WarehouseId,
                WarehouseName = house.WarehouseName,
                Size = house.Size,
                Status = house.Status
            };
        }
        public Warehouse SelectedWarehouse
        {
            get => _selectedWarehouse;
            set
            {
                _selectedWarehouse = value;
                (EditCommand as RelayCommand)?.RaiseCanExecuteChanged();
                (DeleteCommand as RelayCommand)?.RaiseCanExecuteChanged();
                OnPropertyChanged();

                if (value != null) LoadFromWarehouse(value);
            }
        }

        private Warehouse _formWarehouse = new Warehouse();
        public Warehouse FormWarehouse
        {
            get => _formWarehouse;
            set
            {
                _formWarehouse = value;
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
        //public ICommand ReportCommand { get; }

        public WarehouseViewModel()
        {
            _service = new WarehouseService();
            _backlog = new BackLogService();
            Warehouses = new ObservableCollection<Warehouse>();
            LoadData();

            AddCommand = new RelayCommand(Add);
            EditCommand = new RelayCommand(Edit, () => SelectedWarehouse != null);
            DeleteCommand = new RelayCommand(Delete, () => SelectedWarehouse != null);
            ResetCommand = new RelayCommand(Reset);
            SearchCommand = new RelayCommand(Search);
            BacklogCommand = new RelayCommand(Backlog);
            //ReportCommand = new RelayCommand(Report);

        }

        private void LoadData()
        {
            Warehouses = new ObservableCollection<Warehouse>(_service.GetAll());
            OnPropertyChanged(nameof(Warehouses));
        }
        private void Add()
        {
            try
            {
                //validation
                if (string.IsNullOrWhiteSpace(FormWarehouse.WarehouseName))
                {
                    MessageBox.Show("Name is required");
                    return;
                }
                //for size
                if (FormWarehouse.Size == null)
                {
                    MessageBox.Show("Size is required");
                    return;
                }
                if(FormWarehouse.Size < 0)
                {
                    MessageBox.Show("Size must be greater than or equal 0 ");
                    FormWarehouse.Size = 0;
                    return;
                }
                _service.Add(FormWarehouse);
                MessageBox.Show($"Added {FormWarehouse.WarehouseName} success");
                Reset();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}; Detail: {ex.InnerException}");
            }
        }

        private void Edit()
        {
            if (SelectedWarehouse == null) return;
            try
            {
                if (string.IsNullOrWhiteSpace(FormWarehouse.WarehouseName))
                {
                    MessageBox.Show("Warehouse name is required");
                    return;
                }

                var oldData = _backlog.CloneEntity(SelectedWarehouse);
                var newData = _backlog.CloneEntity(FormWarehouse); // set data update

                var updated = new Warehouse
                {
                    WarehouseId = SelectedWarehouse.WarehouseId,
                    WarehouseName = FormWarehouse.WarehouseName,
                    Size = FormWarehouse.Size,
                    Status = FormWarehouse.Status,
                };

                _service.Update(updated);
                _backlog.Push(oldData, newData, "Update", SelectedWarehouse.WarehouseId);
                MessageBox.Show($"Update {FormWarehouse.WarehouseName} success");
                Reset();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message} - Detail: {ex.InnerException?.Message ?? "N/A"}");
            }
        }

        private void Delete()
        {
            if (SelectedWarehouse == null) return;
            try
            {
                var oldData = _backlog.CloneEntity(SelectedWarehouse);
                
                _service.Delete(SelectedWarehouse);
                _backlog.Push(oldData, null, "Delete", oldData.WarehouseId);
                MessageBox.Show($"Deleted {SelectedWarehouse.WarehouseName} sucess");
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
            FormWarehouse = new Warehouse();
            SelectedWarehouse = null!;
        }
        private void Search()
        {
            string key = $"{FormWarehouse.WarehouseName} {FormWarehouse.Size} {FormWarehouse.Status}";
            var result = _service.Search(key);

            Warehouses = new ObservableCollection<Warehouse>(result);

            OnPropertyChanged(nameof(Warehouses));
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
    }
}
