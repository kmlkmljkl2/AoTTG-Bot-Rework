using AoTTG_Bot_Rework.CustomClasses;
using System;
using Windows.Storage;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace AoTTG_Bot_Rework
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Config_Page : Page
    {
        private ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        public static ObservableBool McToggleSetting = new ObservableBool();
        private ObservableBool leaveWhenMCdoes => McToggleSetting;
        public static int LoggerTime = 60;
        public Config_Page()
        {
            this.InitializeComponent();
            Loaded += Config_Page_Loaded;
        }

        private void Config_Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (localSettings.Values.ContainsKey(Settings.PlayerName))
            {
                NameBox.Text = localSettings.Values[Settings.PlayerName] as string;
            }
            if (localSettings.Values.ContainsKey(Settings.McToggle))
            {
                leaveWhenMCdoes.Data = (bool)localSettings.Values[Settings.McToggle];
            }
            if (localSettings.Values.ContainsKey(Settings.LogTime))
            {
                LoggerTime = (int)localSettings.Values[Settings.LogTime];
                LogTime.Text = LoggerTime.ToString();
            }

        }

        private void TextBox_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var textBox = sender as TextBox;

            textBox.FontSize = base.ActualHeight * 0.044;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox.Text == null) return;

            localSettings.Values[Settings.PlayerName] = textBox.Text;
        }

        private async void BTNNew_Instance(object sender, RoutedEventArgs e)
        {
            AppWindow appWindow = await AppWindow.TryCreateAsync();
            Frame appWindowContentFrame = new Frame();
            appWindowContentFrame.Navigate(typeof(MainPage));
            ElementCompositionPreview.SetAppWindowContent(appWindow, appWindowContentFrame);
            await appWindow.TryShowAsync();
            appWindow.Closed += delegate
            {
                appWindowContentFrame.Content = null;
                appWindow = null;
            };
        }

        private void Button_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var button = sender as Button;

            button.FontSize = base.ActualHeight * 0.0097;
        }

        private void LogTime_TextChanged(object sender, TextChangedEventArgs e)
        {
            var send = (TextBox)sender;
            if(send.Text == null) return;

            if(int.TryParse(send.Text, out var time))
            {
                LoggerTime = time;
                localSettings.Values[Settings.LogTime] = time;
            }
        }

        private void McToggle_Toggled(object sender, RoutedEventArgs e)
        {
            var toggle = (ToggleSwitch)sender;
            if (toggle == null) return;
            localSettings.Values[Settings.McToggle] = toggle.IsOn;
        }
    }
}