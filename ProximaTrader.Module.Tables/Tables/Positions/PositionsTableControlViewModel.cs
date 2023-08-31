using Microsoft.Practices.Unity;
using Prism.Events;
using Prism.Regions;
using ProximaTrader.Infrastructure.Helpers_WPF;
using ProximaTrader.Infrastructure.Prism.Interfaces;

namespace ProximaTrader.Module.Tables.Tables.Positions
{
    public class PositionsTableControlViewModel : FullBindableBase
    {
        #region ctor
        public PositionsTableControlViewModel(IEventAggregator eventAggregator, 
            IRegionManager regionManager, 
            IUnityContainer container, 
            IShellService shellService) : 
            base(eventAggregator, regionManager, container, shellService)
        {
        }
        #endregion
    }
}
