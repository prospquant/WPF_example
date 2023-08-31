using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Prism.Events;
using Prism.Regions;
using ProximaTrader.Infrastructure.Helpers_WPF;
using ProximaTrader.Infrastructure.Prism.Interfaces;

namespace ProximaTrader.Module.Tables.Tables.StopOrders
{
    public class StopOrdersTableControlViewModel : FullBindableBase
    {
        #region ctor


        public StopOrdersTableControlViewModel(IEventAggregator eventAggregator, IRegionManager regionManager, IUnityContainer container, IShellService shellService) : base(eventAggregator, regionManager, container, shellService)
        {
        }


        #endregion

    }
}
