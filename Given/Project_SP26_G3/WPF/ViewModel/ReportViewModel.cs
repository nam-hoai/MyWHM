using Microsoft.EntityFrameworkCore.Diagnostics;
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
    public class ReportViewModel : BaseViewModel
    {
        private readonly ReportService _service;
        private readonly IBackLogService _backlog;

        public ObservableCollection<ReportEntry> Reports => _service.Reports;
        private ReportEntry _selectedReport = null!;
        private void LoadFormReport(ReportEntry report)
        {
            FormReport = new ReportEntry
            {
                FileName = report.FileName,
                FilePath = report.FilePath,
                CreatedAt = report.CreatedAt,
                FileSize = report.FileSize,
                Content = report.Content
            };
        }
        public ReportEntry SelectedReport
        {
            get => _selectedReport;
            set
            {
                _selectedReport = value;
                (ExportCommand as RelayCommand<ReportEntry>)?.RaiseCanExecuteChanged();
                (DeleteCommand as RelayCommand<ReportEntry>)?.RaiseCanExecuteChanged();
                OnPropertyChanged();
                if (value != null)
                {
                    LoadFormReport(value);
                }
            }
        }
        private ReportEntry _formReport = new ReportEntry();
        public ReportEntry FormReport
        {
            get => _formReport;
            set
            {
                _formReport = value;
                OnPropertyChanged();
            }
        }
        private void RegisterReportFormatters()
        {
            // === Product ===

            _service.RegisterFormatter<Product>(products =>
            {
                var sb = new StringBuilder();
                foreach (var p in products)
                {
                    sb.AppendLine($"Product: {p.ProductName} ({p.ProductCode})");
                    sb.AppendLine($"Quantity: {p.Quantity}");
                    sb.AppendLine($"Sender: {p.Sender}");
                    sb.AppendLine($"Location: {p.Location}");
                    sb.AppendLine($"Category: {p.Cat.CatName}");
                    sb.AppendLine($"Date In: {p.DateIn:dd/MM/yyyy}");
                    sb.AppendLine($"Date Out: {(p.DateOut.HasValue ? p.DateOut.Value.ToString("dd/MM/yyyy") : "N/A")}");
                    sb.AppendLine($"Status: {(p.Status ? "Active" : "Disabled")}");
                    sb.AppendLine(new string('-', 60));
                }
                return sb.ToString();
            });

            // === Warehouse ===
            _service.RegisterFormatter<Warehouse>(warehouses =>
            {
                var sb = new StringBuilder();
                foreach (var w in warehouses)
                {
                    sb.AppendLine($"Warehouse: {w.WarehouseName}");
                    sb.AppendLine($"Size: {w.Size}");
                    sb.AppendLine($"Status: {(w.Status ? "Enable" : "Disable")}");
                    sb.AppendLine(new string('-', 60));
                }
                return sb.ToString();
            });

            // === Person ===
            _service.RegisterFormatter<Person>(people =>
            {
                var sb = new StringBuilder();
                using MyContext context = new MyContext();
                foreach (var person in people)
                {
                    sb.AppendLine($"Name: {person.PersonName}");
                    sb.AppendLine($"Address: {person.Address}");
                    sb.AppendLine($"Birtday: {person.Birthdate:dd/MM/yyyy}");
                    sb.AppendLine($"Phone: {person.Phone}");
                    //sb.AppendLine($"Role: {person.Role.RoleId}");
                    sb.AppendLine($"Warehouse Sender: {(context.Warehouses.FirstOrDefault(p => p.WarehouseName == (context.Products.FirstOrDefault(p => p.Sender == person.PersonName)).Location).WarehouseName)}");
                    sb.AppendLine(new string('-', 60));
                }
                return sb.ToString();
            });
        }
        public ICommand ExportCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ResetCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand BacklogCommand { get; }

        public ReportViewModel()
        {
            _service =  ReportService.Instance;
            _backlog = new BackLogService();
            RegisterReportFormatters();
            LoadData();

            ExportCommand = new RelayCommand<ReportEntry>(Export, r => r != null);
            DeleteCommand = new RelayCommand<ReportEntry>(Delete, r => r != null);
            ResetCommand = new RelayCommand(Reset);
            SearchCommand = new RelayCommand(Search);
            BacklogCommand = new RelayCommand(Backlog);
        }
        private void LoadData()
        {
            // Load data lần đầu khi mở app
            var result = _service.SearchReports("Reports");
            if (result.IsSuccess)
            {
                MessageBox.Show(result.Message, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(result.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void Export(ReportEntry report)
        {
            if (report == null) return;
             var result = _service.ExportReport(SelectedReport, @"ExportedReports");
            // ViewModel quyết định hiển thị thông báo
            if (result.IsSuccess)
            {
                MessageBox.Show(result.Message, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(result.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            Reset();
        }

        private void Delete(ReportEntry report)
        {
            if (report == null) return;
            var result = _service.DeleteReport(SelectedReport);
            if (result.IsSuccess)
            {
                MessageBox.Show(result.Message, "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show(result.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            Reset();
        }

        private void Reset()
        {
            LoadData();
            FormReport = new ReportEntry();
            SelectedReport = null!;
        }

        private void Search()
        {
            string keyword = $"{FormReport.FileName}";

            if (string.IsNullOrEmpty(keyword))
            {
                LoadData();
            }
            else
            {
                var result = _service.SearchReports(@"Reports", keyword);
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

        private void Backlog()
        {
                _service.UndoDelete();   
        }
    }
}
