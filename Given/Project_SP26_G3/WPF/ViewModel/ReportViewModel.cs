using Microsoft.EntityFrameworkCore.Diagnostics;
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
    public class ReportViewModel : BaseViewModel
    {
        private readonly IReportService _service;
        private readonly IBackLogService _backlog;

        public ObservableCollection<ReportEntry> Reports { get; set; }
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
        public ICommand ExportCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand ResetCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand BacklogCommand { get; }

        public ReportViewModel()
        {
            _service = new ReportService();
            _backlog = new BackLogService();
            Reports = new ObservableCollection<ReportEntry>();
            LoadData();

            ExportCommand = new RelayCommand(Export);
            DeleteCommand = new RelayCommand(Delete);
            ResetCommand = new RelayCommand(Reset);
            SearchCommand = new RelayCommand(Search);
            BacklogCommand = new RelayCommand(Backlog);
        }
        private void LoadData()
        {
            Reports = new ObservableCollection<ReportEntry>(_service.Report);
            OnPropertyChanged(nameof(Reports));
        }
        private void Export()
        {
            throw new NotImplementedException();
        }

        private void Delete()
        {
            throw new NotImplementedException();
        }

        private void Reset()
        {
            throw new NotImplementedException();
        }

        private void Search()
        {
            throw new NotImplementedException();
        }

        private void Backlog()
        {
            throw new NotImplementedException();
        }
    }
}
