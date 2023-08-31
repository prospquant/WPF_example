using Microsoft.Practices.Unity;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using ProximaTrader.Infrastructure.Helpers_WPF;
using ProximaTrader.Infrastructure.Prism.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using ResourceStrings = ProximaTrader.Styles.Resources.Localization.Strings;
using WPFLocalizeExtension.Engine;

namespace ProximaTrader.Settings
{
    public class MainSettingsControlViewModel : FullBindableBase
    {
        public DelegateCommand<Window> SaveCommand { get; set; }
        public MainSettingsControlViewModel(IEventAggregator eventAggregator, 
            IRegionManager regionManager, 
            IUnityContainer container, 
            IShellService shellService) : 
            base(eventAggregator, regionManager, container, shellService)
        {
            SaveCommand = new DelegateCommand<Window>(UpdateLocale);
            CultureInfo = CulturesDictionary.First(x => x.Value == LocalizeDictionary.Instance.Culture.Name).Key;
        }

        private void UpdateLocale(Window window)
        {
            if (SettingsStorage.Instance.ProximaSettings.Locale != CulturesDictionary[CultureInfo])
            {
                SettingsStorage.Instance.ProximaSettings.Locale = CulturesDictionary[CultureInfo];
                SettingsStorage.Instance.SaveSettings();
                ShellService.ShowMessageBox(ResourceStrings.settings_localeText, ResourceStrings.common_LocaleChangedText, ProximaTraderMessageBoxButton.OK);
            }
            ExitWindow(window);
        }

        public IEnumerable<string> Cultures
        { //  
            get => LocalizeDictionary.Instance.MergedAvailableCultures.
                Where(x => !string.IsNullOrEmpty(x.Name) && x.ThreeLetterISOLanguageName != "rus").
                Select(x => ToUpperFirstLetter(x.NativeName));
        }

        private Dictionary<string, string> CulturesDictionary
        {
            get => LocalizeDictionary.Instance.MergedAvailableCultures.
                Where(x => !string.IsNullOrEmpty(x.Name) && x.ThreeLetterISOLanguageName != "rus").
                ToDictionary(x => ToUpperFirstLetter(x.NativeName), y => y.Name);
        }

        private string cultureInfo;
        public string CultureInfo
        {
            get => cultureInfo;
            set => SetProperty(ref cultureInfo, value);
        }

        private string ToUpperFirstLetter(string value)
        {
            return $"{value[0].ToString().ToUpper()}{value.Substring(1)}";
        }
    }
}
