using Microsoft.Practices.Unity;
using NLog;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using ProximaTrader.Infrastructure.Helpers_WPF;
using ProximaTrader.Infrastructure.Prism.Interfaces;
using ProximaTrader.Logger;
using ProximaTrader.Managers.Core.InstanceClasses;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using ResourceStrings = ProximaTrader.Styles.Resources.Localization.Strings;

namespace ProximaTrader.Module.Tables.Tables.LogViewer
{
    public class LogViewerControlViewModel : FullBindableBase
    {
        private readonly ObservableCollection<LogRecord> records;

        public LogViewerControlViewModel(IEventAggregator eventAggregator, IRegionManager regionManager, IUnityContainer container, IShellService shellService)
            : base(eventAggregator, regionManager, container, shellService)
        {
            Exchanges = new ObservableCollection<string> { ResourceStrings.common_AllExchanges };
            Exchanges.AddRange(MarketDataManager.Instance.MarketExchanges.Select(x => x.ToString()));
            Accounts = new ObservableCollection<string> { ResourceStrings.common_AllAccounts };
            Accounts.AddRange(ConnectorManager.AllAccounts());
            IsTableView = true; SelectedTabPageIndex = 0;
            var logFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}{Logger.Logger.FileName}";
            records = new ObservableCollection<LogRecord>(Logger.Logger.ReadLogRecords(logFilePath));
            LogRecords = new ObservableCollection<LogRecord>(records);
            DateTo = DateTime.Today.AddDays(1).Date;
            DateFrom = DateTime.Today.Date;
            SelectedExchange = Exchanges.First();
            SelectedAccount = Accounts.First();
            SelectedLogLevel = LogLevels.First();
            Logger.Logger.OnLogEntryAdded += Logger_OnLogEntryAdded;
            records.CollectionChanged += Records_CollectionChanged;
            Records_CollectionChanged(this, null);
        }

        private void Records_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ApplyFilter();
        }

        private readonly LogFilter Filter = new LogFilter();

        private void Logger_OnLogEntryAdded(LogRecord record)
        {
            records.Add(record);
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            var result = records.AsQueryable();
            if (!string.IsNullOrEmpty(Filter.Account) && Filter.Account != ResourceStrings.common_AllAccounts)
            {
                result = result.Where(x => x.Account == Filter.Account);
            }
            if (!string.IsNullOrEmpty(Filter.Exchange) && Filter.Exchange != ResourceStrings.common_AllExchanges)
            {
                result = result.Where(x => x.Exchange == Filter.Exchange);
            }
            if (!string.IsNullOrEmpty(Filter.LogLevel) && Filter.LogLevel != ResourceStrings.common_AllText)
            {
                result = result.Where(x => x.LogLevel == Filter.LogLevel);
            }
            if (!string.IsNullOrEmpty(Filter.SearchText))
            {
                result = result.Where(x =>
                x.Event.ToLower().Contains(Filter.SearchText.ToLower()) ||
                x.SystemMessage.ToLower().Contains(Filter.SearchText.ToLower()));
            }
            if (Filter.From.Date == Filter.To.Date)
                LogRecords = new ObservableCollection<LogRecord>(result.Where(x => x.Date.Date == Filter.From));
            else
                LogRecords = new ObservableCollection<LogRecord>(result.Where(x => x.Date >= Filter.From && x.Date <= Filter.To));
            LogText = string.Join(Environment.NewLine, LogRecords);
        }

        /// <summary>
        /// Выбранная биржа
        /// </summary>
        private string _selectedExchange;
        public string SelectedExchange
        {
            get => _selectedExchange;
            set
            {
                SetProperty(ref _selectedExchange, value);
                Filter.Exchange = value.ToString();
                Accounts = new ObservableCollection<string>
                {
                    ResourceStrings.common_AllAccounts
                };
                if (value == ResourceStrings.common_AllExchanges)
                    Accounts.AddRange(ConnectorManager.AllAccounts());
                else
                    Accounts.AddRange(ConnectorManager.AccountsByExchange(value));
                ApplyFilter();
            }

        }

        public ObservableCollection<string> ViewTypes { get; set; } = new ObservableCollection<string>
        {
            ResourceStrings.common_TableLbl,
            ResourceStrings.common_TextLbl
        };

        /// <summary>
        /// Аккаунты
        /// </summary>
        private ObservableCollection<string> _accounts;
        public ObservableCollection<string> Accounts
        {
            get => _accounts;
            set => SetProperty(ref _accounts, value);
        }

        /// <summary>
        /// Все биржи
        /// </summary>
        private ObservableCollection<string> _exchanges;
        public ObservableCollection<string> Exchanges
        {
            get => _exchanges;
            set => SetProperty(ref _exchanges, value);
        }

        /// <summary>
        /// Уровни логов
        /// </summary>
        public ObservableCollection<string> LogLevels
        {
            get => new ObservableCollection<string>
            {
                ResourceStrings.common_AllText,
                LogLevel.Off.Name,
                LogLevel.Info.Name,
                LogLevel.Trace.Name,
                LogLevel.Debug.Name,
                LogLevel.Warn.Name,
                LogLevel.Error.Name,
                LogLevel.Fatal.Name,
            };
        }

        /// <summary>
        /// Выбранный уровень логов
        /// </summary>
        private string _selectedLogLevel;
        public string SelectedLogLevel
        {
            get => _selectedLogLevel;
            set
            {
                SetProperty(ref _selectedLogLevel, value);
                Filter.LogLevel = value;
                ApplyFilter();
            }
        }

        /// <summary>
        /// Дата начала выборки записей
        /// </summary>
        private DateTime _dateFrom;
        public DateTime DateFrom
        {
            get => _dateFrom;
            set
            {
                if (value <= DateTo)
                {
                    SetProperty(ref _dateFrom, value);
                    Filter.From = value;
                }
                ApplyFilter();
            }
        }

        /// <summary>
        /// Дата окончания выборки записей
        /// </summary>
        private DateTime _dateTo;
        public DateTime DateTo
        {
            get => _dateTo;
            set
            {
                if (value >= DateFrom)
                {
                    SetProperty(ref _dateTo, value);
                    Filter.To = value;
                }
                ApplyFilter();
            }
        }

        /// <summary>
        /// Выбранный аккаунт
        /// </summary>
        private string _selectedAccount;
        public string SelectedAccount
        {
            get => _selectedAccount;
            set
            {
                SetProperty(ref _selectedAccount, value);
                Filter.Account = value;
                ApplyFilter();
            }
        }

        /// <summary>
        /// Текст для поиска
        /// </summary>
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                SetProperty(ref _searchText, value);
                Filter.SearchText = value;
                ApplyFilter();
            }
        }

        /// <summary>
        /// Записи логов для отображения
        /// </summary>
        private ObservableCollection<LogRecord> _logRecords;
        public ObservableCollection<LogRecord> LogRecords
        {
            get => _logRecords;
            set => SetProperty(ref _logRecords, value);
        }

        private bool _tableView, _textView;
        public bool IsTableView
        {
            get => _tableView;
            set
            {
                SetProperty(ref _tableView, value);
                if (IsTextView == value)
                {
                    IsTextView = !value;
                }
                if (value)
                    SelectedTabPageIndex = 0;
            }
        }

        public ICommand SaveLogFileCommand
        {
            get => new DelegateCommand(() =>
            {
                var dialog = new SaveFileDialog()
                {
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory),
                    FileName = $"Log_{DateFrom.ToString("ddMMyyyy")}_{DateTo.ToString("ddMMyyyy")}",
                    Filter = IsTextView ? "Text file (*.txt)|*.txt" : "CSV file(*.csv) | *.csv"
                };
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    var fileName = dialog.FileName;
                    var selectedRecords = records.Where(x => x.Date >= Filter.From && x.Date <= Filter.To).Select(x => x.ToString());
                    File.WriteAllLines(fileName, selectedRecords);
                }
            });
        }

        public bool IsTextView
        {
            get => _textView;
            set
            {
                SetProperty(ref _textView, value);
                if (IsTableView == value)
                {
                    IsTableView = !value;
                }
                if (value)
                    SelectedTabPageIndex = 1;
            }
        }

        private string _logText;
        public string LogText
        {
            get => _logText;
            set => SetProperty(ref _logText, value);
        }

        private int _selectedTabPageIndex;
        public int SelectedTabPageIndex
        {
            get => _selectedTabPageIndex;
            set => SetProperty(ref _selectedTabPageIndex, value);
        }

        internal class LogFilter
        {
            public string Exchange { get; set; }
            public string Account { get; set; }
            public DateTime From { get; set; }
            public DateTime To { get; set; }
            public string LogLevel { get; set; }
            public string SearchText { get; set; }
        }
    }
}
