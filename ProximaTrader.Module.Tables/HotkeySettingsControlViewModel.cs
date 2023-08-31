using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using ProximaTrader.Settings.SettingsClasses;
using ProximaTrader.Infrastructure.Helpers_WPF;
using ProximaTrader.Infrastructure.Prism.Interfaces;
using ResourceStrings = ProximaTrader.Styles.Resources.Localization.Strings;

namespace ProximaTrader.Settings
{
    public class HotkeySettingsControlViewModel : FullBindableBase
    {
        public HotkeySettingsControlViewModel(IEventAggregator eventAggregator, 
            IRegionManager regionManager, 
            IUnityContainer container, 
            IShellService shellService) : 
            base(eventAggregator, regionManager, container, shellService)
        {
            ApplyFilterCommand = new DelegateCommand<string>(ApplyFilter);
            HotKeyCommands = new ObservableCollection<HotKeyCommand>(
                SettingsStorage.Instance.ProximaSettings.HotKeys.
                Where(x => x.HotKeyGroup == HotKeyGroup.General && x.IsEnabled));
            SaveCommand = new DelegateCommand<Window>(SaveHotkeys);
        }

        private ObservableCollection<HotKeyCommand> hotKeyCommands;
        public ObservableCollection<HotKeyCommand> HotKeyCommands
        {
            get => hotKeyCommands;
            set => SetProperty(ref hotKeyCommands, value);
        }

        private void ApplyFilter(string filter)
        {
            foreach (var group in Enum.GetValues(typeof(HotKeyGroup)))
            {
                if (group.ToString() == filter)
                {
                    HotKeyCommands = new ObservableCollection<HotKeyCommand>(
                        SettingsStorage.Instance.ProximaSettings.HotKeys.
                        Where(x => x.HotKeyGroup == (HotKeyGroup)group && x.IsEnabled));
                    return;
                }
            }
        }

        private void SaveHotkeys(Window window)
        {
            var keys = SettingsStorage.Instance.ProximaSettings.HotKeys.ToArray();
            foreach (var k in keys)
            {
                var count = keys.Count(x => x.Representation == k.Representation);
                if (count > 1)
                {
                    ShellService.ShowMessageBox(ResourceStrings.settings_ErrorSavingText, ResourceStrings.settings_SaveHotkeysErrorText, ProximaTraderMessageBoxButton.OK);
                    return;
                }
            }
            SettingsStorage.Instance.SaveSettings();
            ExitWindow(window);
        }

        public DelegateCommand<string> ApplyFilterCommand { get; set; }
        public DelegateCommand<Window> SaveCommand { get; set; }
    }
}
