using System.Windows;
using ParasiteReplayAnalyzer.Saving;

namespace ParasiteReplayAnalyzer.UI
{
    /// <summary>
    /// Interaction logic for SettingsUI.xaml
    /// </summary>
    public partial class SettingsUI : Window
    {
        private SettingsManager _settingsManager;
        private ParasiteMainUiWindow _mainUiWindow;

        public SettingsUI(SettingsManager settingsManager, ParasiteMainUiWindow mainUiWindow)
        {
            InitializeComponent();
            Show();

            _mainUiWindow = mainUiWindow;
            _settingsManager = settingsManager;
        }

        private void OnOkClicked(object sender, RoutedEventArgs e)
        {
            var newReplaysPath = _pathTextBox.Text;

            _settingsManager.SaveSettings(newReplaysPath);
            _settingsManager.LoadSettings();

            _mainUiWindow.LoadReplaysAsync();
            _mainUiWindow.FillListBoxItems();

            Close();
        }
    }
}
