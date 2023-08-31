using HQConnector.Dto.DTO.Enums.Exchange;
using HQConnector.Dto.DTO.Ticker;
using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using ProximaTrader.Infrastructure.Helpers_WPF;
using ProximaTrader.Infrastructure.Prism.Interfaces;
using ProximaTrader.Managers.Core.InstanceClasses;
using ProximaTrader.Settings.SettingsClasses;
using ProximaTrader.SubEvents;
using ProximaTrader.WPF.Helpers.Classes_For_Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace ProximaTrader.Module.Tables.Tables.MarketAnalyzer
{
    public class MarketAnalyzerControlViewModel : FullBindableBase, INavigationAware
    {
        private readonly Dispatcher dispatcher;
        public MarketAnalyzerControlViewModel(IEventAggregator eventAggregator, IRegionManager regionManager, IUnityContainer container, IShellService shellService) 
            : base(eventAggregator, regionManager, container, shellService)
        {
            Tickers = new ObservableCollection<TickerTableRow>();
            dispatcher = Dispatcher.CurrentDispatcher;
            ViewModel = new ExchangeSymbolFilterViewModel
            {
                SelectedExchangeAndSymbolCommand = new DelegateCommand(() =>
                {
                    AddCommand.Execute(null);
                    IsOpenPopup = false;
                }),
                CancelSymbolSelectionCommand = new DelegateCommand(() => IsOpenPopup = false)
            };
            EventAggregator.GetEvent<DisposeSubEvent>().Subscribe(WindowClosed);
        }

        public ExchangeSymbolFilterViewModel ViewModel { get; set; }

        public ICommand ClearCommand
        {
            get => new DelegateCommand(() =>
            {
                Unsubscribe();
                EventAggregator.GetEvent<UpdateWindowPropertiesListEvent>()
                    .Publish((FormID, new List<ExchangeSymbolPair>()));
                Tickers = new ObservableCollection<TickerTableRow>();
            });
        }

        private void Unsubscribe()
        {
            foreach (var tick in Tickers)
                MarketDataManager.Instance.UnsubscribeToTicker(tick.Exchange, tick.Pair, UpdateTicker);
        }

        public ICommand AddCommand
        {
            get => new DelegateCommand(() =>
            {
                MarketDataManager.Instance.SubscribeToTicker(ViewModel.SelectedExchange.Value, ViewModel.SelectedSymbol, UpdateTicker);
                EventAggregator.GetEvent<UpdateWindowPropertiesListEvent>()
                    .Publish((FormID, GetTickerPairs(new ExchangeSymbolPair(ViewModel.SelectedExchange.Value, ViewModel.SelectedSymbol.SymbolCode))));
                ViewModel.SelectedSymbol = null;
                IsOpenPopup = false;
            });
        }

        public ICommand RemoveCommand
        {
            get => new DelegateCommand(() =>
            {
                if (SelectedTicker != null)
                {
                    MarketDataManager.Instance.UnsubscribeToTicker(ViewModel.SelectedExchange.Value, SelectedTicker.Pair, UpdateTicker);
                    EventAggregator.GetEvent<UpdateWindowPropertiesListEvent>()
                        .Publish((FormID, GetTickerPairs(new ExchangeSymbolPair(ViewModel.SelectedExchange.Value, SelectedTicker.Pair), true)));
                    Tickers.Remove(SelectedTicker);
                }
            });
        }

        public ICommand MoveUpCommand
        {
            get => new DelegateCommand(() =>
            {
                if (Tickers.First() == SelectedTicker || SelectedTicker == null)
                    return;
                var indexOf = Tickers.IndexOf(SelectedTicker);
                var prev = Tickers[indexOf - 1];
                Tickers[indexOf - 1] = SelectedTicker;
                Tickers[indexOf] = prev;
            });
        }

        public ICommand MoveDownCommand
        {
            get => new DelegateCommand(() =>
            {
                if (Tickers.Last() == SelectedTicker || SelectedTicker == null)
                    return;
                var indexOf = Tickers.IndexOf(SelectedTicker);
                var prev = Tickers[indexOf + 1];
                Tickers[indexOf + 1] = SelectedTicker;
                Tickers[indexOf] = prev;
            });
        }

        public ICommand OpenPopupCommand
        {
            get => new DelegateCommand(() =>
            {
                IsOpenPopup = true;
            });
        }

        public ICommand CancelSymbolSelectionCommand
        {
            get => new DelegateCommand(() =>
            {
                IsOpenPopup = false;
            });
        }

        private List<ExchangeSymbolPair> GetTickerPairs(ExchangeSymbolPair newPair, bool removed = false)
        {
            var list = Tickers.Select(x => new ExchangeSymbolPair(x.Exchange, x.Pair)).ToList();
            if (!removed)
                list.Add(newPair);
            return list;
        }

        private void UpdateTicker(Ticker ticker)
        {
            if (!Tickers.Any(x => x.Exchange == ticker.Exchange && x.Pair == ticker.Pair))
            {
                dispatcher.Invoke(() => Tickers.Add(new TickerTableRow(ticker)));
            }
            else
            {
                Tickers.First(x => x.Exchange == ticker.Exchange && x.Pair == ticker.Pair).Update(ticker);
            }
        }

        private bool _isOpenPopup;
        public bool IsOpenPopup
        {
            get => _isOpenPopup;
            set => SetProperty(ref _isOpenPopup, value);
        }
        
        /// <summary>
        /// Тикеры
        /// </summary>
        private ObservableCollection<TickerTableRow> _tickers;
        public ObservableCollection<TickerTableRow> Tickers
        {
            get => _tickers;
            set => SetProperty(ref _tickers, value);
        }

        /// <summary>
        /// Выбранный тикер
        /// </summary>
        private TickerTableRow _selectedTicker;
        public TickerTableRow SelectedTicker
        {
            get => _selectedTicker;
            set => SetProperty(ref _selectedTicker, value); 
        }

        private void WindowClosed(string formId)
        {
            if (FormID == formId)
                Dispose();
        }

        protected override void Dispose()
        {
            Unsubscribe();
            base.Dispose();
        }

        public void OnNavigatedFrom(NavigationContext navigationContext) { }

        public async void OnNavigatedTo(NavigationContext navigationContext)
        {
            if (navigationContext.Parameters != null)
            {
                if (navigationContext.Parameters["params"] is WindowSettings param)
                {
                    FormID = param.Id;
                    await Task.Delay(1750).ConfigureAwait(false);
                    foreach (var pair in param.ExchangeSymbols)
                        MarketDataManager.Instance.SubscribeToTicker(pair.Exchange, pair.SymbolCode, UpdateTicker);
                }
                else if (navigationContext.Parameters["id"] is string id)
                {
                    FormID = id;
                }
            }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return navigationContext.Uri.OriginalString.Contains("MarketAnalyzerControl");
        }
    }
}
