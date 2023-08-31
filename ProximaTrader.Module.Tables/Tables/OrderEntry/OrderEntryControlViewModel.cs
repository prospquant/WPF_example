using HQConnector.Dto.DTO.Enums.Exchange;
using HQConnector.Dto.DTO.Enums.Orders;
using HQConnector.Dto.DTO.Order;
using HQConnector.Dto.DTO.Symbol;
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

namespace ProximaTrader.Module.Tables.Tables.OrderEntry
{
    public class OrderEntryControlViewModel : FullBindableBase, INavigationAware
    {
        public DelegateCommand<string> PlaceOrderCommand { get; set; }
        public ExchangeSymbolFilterViewModel ViewModel { get; set; }
        public OrderEntryControlViewModel(IEventAggregator eventAggregator, 
            IRegionManager regionManager, 
            IUnityContainer container, 
            IShellService shellService) : 
            base(eventAggregator, regionManager, container, shellService)
        {
            PlaceOrderCommand =
                new DelegateCommand<string>(async side => await PlaceOrder(side), OrderPlacementAllowed);
            InitUpDownControls();
            ViewModel = new ExchangeSymbolFilterViewModel
            {
                CancelSymbolSelectionCommand = new DelegateCommand(() =>
                {
                    IsOpenSymbolFilter = false;
                    ViewModel.SelectedSymbol = previousSymbol;
                    ViewModel.SelectedExchange = previousExchange;
                }),
                SelectedExchangeAndSymbolCommand = new DelegateCommand(() =>
                    SetExchangeAndSymbol(), CanUpdateExchangeAndSymbol)
            };
            SelectedTimeInForce = MarketDataManager.Instance.SupportedOrderTypes.First();
            SelectedOrderType = OrderTypes.First();
        }

        private void InitUpDownControls()
        {
            AmountViewModel = new UpDownControlViewModel();
            PriceViewModel = new UpDownControlViewModel();
            LimitPriceViewModel = new UpDownControlViewModel();
            AmountViewModel.ValueChanged += PlaceOrderCommand.RaiseCanExecuteChanged;
            PriceViewModel.ValueChanged += PlaceOrderCommand.RaiseCanExecuteChanged;
            LimitPriceViewModel.ValueChanged += PlaceOrderCommand.RaiseCanExecuteChanged;
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

        private bool OrderPlacementAllowed(string side)
        {
            return !string.IsNullOrEmpty(side) &&
                SelectedAmount > 0 &&
                ((SelectedPrice > 0 && SelectedOrderType != OrderType.Market) || 
                (SelectedPrice == 0 && SelectedOrderType == OrderType.Market)) &&
                SelectedAccount != null &&
                SelectedExchange != null &&
                SelectedSymbol != null;
        }

        private void SetExchangeAndSymbol()
        {
            IsOpenSymbolFilter = false;
            SelectedExchangeAndSymbol = $"{SelectedExchange} : {SelectedSymbol.SymbolCode}";
            Accounts = ConnectorManager.AccountsByExchange(SelectedExchange.Value);
            var pair = (SelectedExchange.Value, SelectedSymbol.SymbolCode);
            var priceFormat = MarketDataManager.Instance.GetPriceFormat(MarketDataManager.Instance.SymbolPriceSteps[pair]);
            PriceViewModel.UpdateParams(MarketDataManager.Instance.SymbolPriceSteps[pair], priceFormat);
            LimitPriceViewModel.UpdateParams(MarketDataManager.Instance.SymbolPriceSteps[pair], priceFormat);
            EventAggregator.GetEvent<UpdateWindowPropertiesEvent>().
                Publish((FormID, new ExchangeSymbolPair(SelectedExchange.Value, SelectedSymbol.SymbolCode)));
            OnPropertyChanged(nameof(SelectedTriple));
            PlaceOrderCommand.RaiseCanExecuteChanged();
        }

        private bool CanUpdateExchangeAndSymbol()
        {
            return ViewModel.CanUpdateExchangeAndSymbol(previousExchange, previousSymbol);
        }

        private string selectedTimeInForce;
        public string SelectedTimeInForce
        {
            get => selectedTimeInForce;
            set => SetProperty(ref selectedTimeInForce, value);
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

        /// <summary>
        /// Выбранный символ
        /// </summary>
        private Symbol previousSymbol;
        public Symbol SelectedSymbol
        {
            get => ViewModel.SelectedSymbol;
        }
        
        /// <summary>
        /// Выбранная биржа
        /// </summary>
        private Exchange? previousExchange;
        public Exchange? SelectedExchange
        {
            get => ViewModel.SelectedExchange;
        }
        
        /// <summary>
        /// Аккаунты
        /// </summary>
        private IEnumerable<string> _accounts;
        public IEnumerable<string> Accounts
        {
            get => _accounts;
            set => SetProperty(ref _accounts, value);
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
                EventAggregator.GetEvent<ChangeAccount>().Publish((FormID, _selectedAccount));
                PlaceOrderCommand.RaiseCanExecuteChanged();
                OnPropertyChanged(nameof(SelectedTriple));
            }
        }
        /// <summary>
        /// Выбранное количество
        /// </summary>
        public decimal SelectedAmount
        {
            get => AmountViewModel.Value;
        }

        /// <summary>
        /// Цена
        /// </summary>
        public decimal SelectedPrice
        {
            get => PriceViewModel.Value;
        }

        /// <summary>
        /// Лимитная цена
        /// </summary>
        public decimal SelectedLimitPrice
        {
            get => LimitPriceViewModel.Value;
        }

        private UpDownControlViewModel priceViewModel, limitPriceViewModel, amountViewModel;
        public UpDownControlViewModel PriceViewModel
        {
            get => priceViewModel;
            set => SetProperty(ref priceViewModel, value);
        }

        public UpDownControlViewModel LimitPriceViewModel
        {
            get => limitPriceViewModel;
            set => SetProperty(ref limitPriceViewModel, value);
        }

        public UpDownControlViewModel AmountViewModel
        {
            get => amountViewModel;
            set => SetProperty(ref amountViewModel, value);
        }

        /// <summary>
        /// Выбранный тип ордера
        /// </summary>
        private OrderType _selectedOrderType;
        public OrderType SelectedOrderType
        {
            get => _selectedOrderType;
            set
            {
                SetProperty(ref _selectedOrderType, value);
                PlaceOrderCommand.RaiseCanExecuteChanged();
            }
        }

        public object[] SelectedTriple
        {
            get => new object[] { SelectedExchange ?? 0, SelectedSymbol, SelectedAccount ?? string.Empty };
        }

        public ObservableCollection<OrderType> OrderTypes =>
            new ObservableCollection<OrderType>
            {
                OrderType.Limit,
                OrderType.Market,
                OrderType.StopMarket,
                OrderType.TakeProfitMarket,
                OrderType.StopLimit,
                OrderType.TakeProfitLimit
            };

        private async Task PlaceOrder(string side)
        {
            Order order;
            if (SelectedOrderType == OrderType.StopLimit || SelectedOrderType == OrderType.TakeProfitLimit ||
                SelectedOrderType == OrderType.StopMarket || SelectedOrderType == OrderType.TakeProfitMarket)
            {
                order = new Order
                {
                    Price = SelectedPrice,
                    StopPrice = SelectedOrderType == OrderType.StopLimit || SelectedOrderType == OrderType.TakeProfitLimit ?
                        SelectedLimitPrice : 0,
                    Pair = SelectedSymbol.SymbolCode,
                    Side = side.Equals("sell") ? 
                        Sides.Sell: 
                        Sides.Buy,
                    Amount = SelectedAmount,
                    Exchange = SelectedExchange.Value,
                    Account = SelectedAccount,
                    Type = SelectedOrderType
                };
            }
            else
            {
                order = new Order
                {
                    Price = SelectedPrice,
                    Pair = SelectedSymbol.SymbolCode,
                    Side = side.Equals("sell") ? 
                        Sides.Sell : 
                        Sides.Buy,
                    Amount = SelectedAmount,
                    Exchange = SelectedExchange.Value,
                    Account = SelectedAccount,
                    Type = SelectedOrderType
                };
            }
            await OrderProcessor.Instance.MakeDirectOrder(ConnectorManager.Instance.ReturnConnectorByName(SelectedAccount), order);
        }

        public void OnNavigatedFrom(NavigationContext navigationContext) { }

        public void OnNavigatedTo(NavigationContext navigationContext)
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
                    SelectedAccount = param.Account;
                    if (CanUpdateExchangeAndSymbol())
                        SetExchangeAndSymbol();
                }
                else if (navigationContext.Parameters["id"] is string id)
                {
                    FormID = id;
                }
            }
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return navigationContext.Uri.OriginalString.Contains("OrderEntryControl");
        }
    }
}
