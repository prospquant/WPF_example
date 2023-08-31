using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using ProximaTrader.Infrastructure.Helpers_WPF;
using ProximaTrader.Infrastructure.Prism.Interfaces;
using System.Diagnostics;

namespace ProximaTrader.Module.Tables.Tables.Positions
{
    public class BalanceTableControlViewModel : FullBindableBase
    {
        #region ctor
        public BalanceTableControlViewModel(IEventAggregator eventAggregator,
            IRegionManager regionManager,
            IUnityContainer container,
            IShellService shellService) :
            base(eventAggregator, regionManager, container, shellService)
        {
       
        }

        #endregion

    
    }
}
