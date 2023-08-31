using HQConnector.Dto.DTO.Enums.Exchange;
using HQConnector.Dto.DTO.Symbol;
using HQConnector.Dto.DTO.Trade;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using ProximaTrader.Infrastructure.Helpers_WPF;
using ProximaTrader.Infrastructure.Prism.Interfaces;
using ProximaTrader.Managers.Core.InstanceClasses;
using ProximaTrader.Settings.SettingsClasses;
using ProximaTrader.SubEvents;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace ProximaTrader.Module.Tables.Tables.RecentTrades
{
    public class RecentTradesControlViewModel : FullBindableBase, INavigationAware
    {
        private readonly Dispatcher dispatcher;
        public RecentTradesControlViewModel(IEventAggregator eventAggregator, IRegionManager regionManager, IUnityContainer container, IShellService shellService)
            : base(eventAggregator, regionManager, container, shellService)
        {
            dispatcher = Dispatcher.CurrentDispatcher;
            ViewModel = new ExchangeSymbolFilterViewModel
            {
                CancelSymbolSelectionCommand = new DelegateCommand(() =>
                {
                    IsOpenSymbolFilter = false;
                    ViewModel.SelectedSymbol = previousSymbol;
                    ViewModel.SelectedExchange = previousExchange;
                }),
                SelectedExchangeAndSymbolCommand = new DelegateCommand
                (async () => await SetExchangeAndSymbol(), CanUpdatedExchangeAndSymbol),
            };
            TableLimit = 1000;
            EventAggregator.GetEvent<DisposeSubEvent>().Subscribe(WindowClosed);
        }

        public ICommand ClearTradesCommand
        {
            get => new DelegateCommand(() => Trades = new ObservableCollection<Trade>());
        }

        public ExchangeSymbolFilterViewModel ViewModel { get; set; }
        /// <summary>
        /// Выбранная биржа
        /// </summary>
        private Exchange? previousExchange;
        public Exchange? SelectedExchange
        {
            get => ViewModel.SelectedExchange;
        }

        /// <summary>
        /// Выбранный символ
        /// </summary>
        private Symbol previousSymbol;
        public Symbol SelectedSymbol
        {
            get => ViewModel.SelectedSymbol;
        }

        /// <summary>
        /// Трейды
        /// </summary>
        private ObservableCollection<Trade> _trades;
        public ObservableCollection<Trade> Trades
        {
            get => _trades;
            set => SetProperty(ref _trades, value);
        }

        private void UnsubscribeFromTrades(string symbol)
        {
            if (!string.IsNullOrEmpty(symbol))
            {
                MarketDataManager.Instance.UnsubscribeToTrades(SelectedExchange.Value, symbol, NewTradeReceived);
                Trades = new ObservableCollection<Trade>();
            }
        }

        private string selectedExchangeAndSymbol;
        public string SelectedExchangeAndSymbol
        {
            get => selectedExchangeAndSymbol;
            set => SetProperty(ref selectedExchangeAndSymbol, value);
        }

        /// <summary>
        /// Флаг открытия панели выбора символа
        /// </summary>
        private bool _isOpenSymbolFilter;
        public bool IsOpenSymbolFilter
        {
            get => _isOpenSymbolFilter;
            set => SetProperty(ref _isOpenSymbolFilter, value);
        }

        private int tableLimit;
        public int TableLimit
        {
            get => tableLimit;
            set => SetProperty(ref tableLimit, value);
        }

        private bool CanUpdatedExchangeAndSymbol()
        {
            return ViewModel.CanUpdateExchangeAndSymbol(previousExchange, previousSymbol);
        }

        private async Task SubscribeToTrades()
        {
            if (SelectedExchange.HasValue && SelectedSymbol != null)
            {
                var trades = await MarketDataManager.Instance.GetTrades
                    (SelectedExchange.Value, SelectedSymbol.SymbolCode, 100);
                Trades = new ObservableCollection<Trade>(trades);
                MarketDataManager.Instance.SubscribeToTrades
                    (SelectedExchange.Value, SelectedSymbol.SymbolCode, NewTradeReceived);
            }
        }

        private async Task SetExchangeAndSymbol()
        {
            IsOpenSymbolFilter = false;
            if (previousSymbol != null)
                UnsubscribeFromTrades(previousSymbol.SymbolCode);
            await SubscribeToTrades();
            SelectedExchangeAndSymbol = $"{SelectedExchange} : {SelectedSymbol?.SymbolCode}";
            EventAggregator.GetEvent<UpdateWindowPropertiesEvent>().
                Publish((FormID, new ExchangeSymbolPair(SelectedExchange.Value, SelectedSymbol.SymbolCode)));
        }

        public ICommand OpenSymbolFilterCommand
        {
            get => new DelegateCommand(() =>
            {
                IsOpenSymbolFilter = true;
                previousSymbol = ViewModel.SelectedSymbol;
                previousExchange = ViewModel.SelectedExchange;
            });
        }

        private void NewTradeReceived(IEnumerable<Trade> trades)
        {
            if (!dispatcher.HasShutdownStarted)
                dispatcher.Invoke(() =>
                {
                    foreach (var tr in trades)
                        if (!Trades.Any(x => x.Id == tr.Id))
                            Trades.Insert(0, tr);
                        else
                        {
                            var trade = Trades.FirstOrDefault(x => x.Id == tr.Id);
                            Trades[Trades.IndexOf(trade)] = tr;
                        }
                    if (Trades.Count >= TableLimit)
                        for (var i = TableLimit; i < Trades.Count; i++)
                            Trades.RemoveAt(i);
                });
        }

        private void WindowClosed(string formId)
        {
            if (FormID == formId)
                Dispose();
        }

        protected override void Dispose()
        {
            UnsubscribeFromTrades(SelectedSymbol?.SymbolCode);
            base.Dispose();
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return navigationContext.Uri.OriginalString.Contains("RecentTrades");
        }

        public void OnNavigatedFrom(NavigationContext navigationContext) { }

        public async void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (navigationContext.Parameters != null)
            {
                if (navigationContext.Parameters["params"] is WindowSettings param)
                {
                    FormID = param.Id;
                    var pair = param.ExchangeSymbols.FirstOrDefault();
                    ViewModel.SelectedExchange = pair.Exchange;
                    if (!string.IsNullOrEmpty(pair.SymbolCode))
                        ViewModel.SelectedSymbol = ViewModel.Symbols.FirstOrDefault(x => x.SymbolCode == pair.SymbolCode);
                    if (ViewModel.CanUpdateExchangeAndSymbol(previousExchange, previousSymbol))
                        await SetExchangeAndSymbol();
                }
                else if (navigationContext.Parameters["id"] is string id)
                {
                    FormID = id;
                }
            }
        }
    }
}
