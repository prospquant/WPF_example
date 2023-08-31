using Microsoft.Practices.Unity;
using Prism.Events;
using Prism.Regions;
using ProximaTrader.Module.Tables.Tables.Balance;
using ProximaTrader.Module.Tables.Tables.LogViewer;
using ProximaTrader.Module.Tables.Tables.MarketAnalyzer;
using ProximaTrader.Module.Tables.Tables.MyTrades;
using ProximaTrader.Module.Tables.Tables.OrderEntry;
using ProximaTrader.Module.Tables.Tables.Orders;
using ProximaTrader.Module.Tables.Tables.Positions;
using ProximaTrader.Module.Tables.Tables.RecentTrades;
using ProximaTrader.Module.Tables.Tables.StopOrders;
using ProximaTrader.Infrastructure;
using ProximaTrader.Infrastructure.Helpers_WPF;
using ProximaTrader.Settings;

namespace ProximaTrader.Module.Tables.Module
{
    public class TerminalTableModule : PrismModule
    {

        #region ctor

        public TerminalTableModule(IRegionManager regionManager, IUnityContainer container, IEventAggregator eventAggregator) : base(regionManager, container, eventAggregator)
        {
        }

        #endregion


        #region Interface Methods

        public override void Initialize()
        {
            RegionManager.RegisterViewWithRegion(TerminalRegions.TableRegion, typeof(OrdersTableControl));
            RegionManager.RegisterViewWithRegion(TerminalRegions.TableRegion, typeof(PositionsTableControl));
            RegionManager.RegisterViewWithRegion(TerminalRegions.TableRegion, typeof(StopOrdersTableControl));
            RegionManager.RegisterViewWithRegion(TerminalRegions.TableRegion, typeof(MyTradesTableControl));
            RegionManager.RegisterViewWithRegion(TerminalRegions.TableRegion, typeof(BalanceTableControl));
            RegionManager.RegisterViewWithRegion(TerminalRegions.MainRegion,  typeof(LogViewerControl));
            RegionManager.RegisterViewWithRegion(TerminalRegions.MainRegion,  typeof(RecentTradesControl));
            RegionManager.RegisterViewWithRegion(TerminalRegions.MainRegion,  typeof(MarketAnalyzerControl));
            RegionManager.RegisterViewWithRegion(TerminalRegions.MainRegion,  typeof(OrderEntryControl));
            RegionManager.RegisterViewWithRegion(TerminalRegions.MainRegion,  typeof(HotkeySettingsControl));
            RegionManager.RegisterViewWithRegion(TerminalRegions.MainRegion,  typeof(MainSettingsControl));
        }

        #endregion


    }
}
