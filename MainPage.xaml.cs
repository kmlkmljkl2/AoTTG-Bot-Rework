using AoTTG_Bot_Rework.CustomClasses;
using AoTTG_Bot_Rework.Pages;
using ExitGames.Client.Photon;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.WindowManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace AoTTG_Bot_Rework
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public static bool LogAoTTG { get; private set; } = false;
        private ApplicationDataContainer localSettings { get; } = ApplicationData.Current.LocalSettings;
        public ObservableString InfoLabel { get; } = new ObservableString() { Data = "Offline" };
        public static MainPage Instance { get; private set; }

        public MainPage()
        {
            foreach (var reg in Enum.GetValues(typeof(PhotonRegion)))
            {
                RegionEnums.Add((PhotonRegion)reg);
            }
            Instance = this;
            InitializeComponent();
            new Thread(UpdatePing) { IsBackground = true }.Start();
            Loaded += MainPage_Loaded;
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (localSettings.Values.ContainsKey(Settings.UseTcp))
            {
                TcpButton.IsChecked = (bool)localSettings.Values[Settings.UseTcp];
            }
        }

        public AoTTG_Bot Bot { get; private set; }

        private ObservableCollection<PhotonRegion> RegionEnums { get; } = new ObservableCollection<PhotonRegion>();

        private void Button_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Button button = sender as Button;

            button.FontSize = base.ActualHeight * 0.025;
        }

        private async void ConnectBTN_Click(object sender, RoutedEventArgs e)
        {
            DisconnectBTN_Click(sender, e);
            Bot = new AoTTG_Bot(TcpButton.IsChecked == true ? ConnectionProtocol.Tcp : ConnectionProtocol.Udp)
            {
                Region = (PhotonRegion)(RegionSelection.SelectedItem ?? RegionEnums.First())
            };
            await Bot.ConnectToMasterAsync();
            MainBox.ItemsSource = Bot.RoomList;
            LoggerBTN.IsEnabled = true;
        }

        private void DisconnectBTN_Click(object sender, RoutedEventArgs e)
        {
            if (Bot == null) return;
            Bot.Disconnect();

            PlayerList.ItemsSource = null;
            MainBox.ItemsSource = null;
        }

        private async void MainBox_DoubleTapped(object sender, Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            ListBox listBox = sender as ListBox;

            if (listBox.SelectedItem != null && listBox.SelectedItem is AoTTGRoomInfo info && await Bot.JoinRoomAsync(info))
            {
                MainBox.ItemsSource = Bot.Messages;
                PlayerList.ItemsSource = Bot.PlayerList;

                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    foreach (var player in Bot.CurrentRoom.Players.Values.OrderBy(x => x.ActorNumber))
                    {
                        Bot.PlayerList.Add(new AoTTGPlayer(player));
                    }
                });
                Bot.Messages.CollectionChanged += Messages_CollectionChanged;
                Bot.PlayerList.CollectionChanged += PlayerList_CollectionChanged;
            }
        }

        private void MainBox_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            ListBox box = sender as ListBox;

            if (e.Key.Equals(VirtualKey.C) && Window.Current.CoreWindow.GetKeyState(VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down) && box.SelectedValue != null)
            {
                string Content = "";

                if (box.SelectedItem is AoTTGMessage message)
                {
                    Content = message.ToString();
                }
                if (box.SelectedItem is AoTTGRoomInfo info)
                {
                    Content = info.ToString();
                }

                DataPackage dataPackage = new DataPackage();
                dataPackage.SetText(Content);
                Clipboard.SetContent(dataPackage);
            }
        }

        private async void Messages_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                MainBox.ScrollIntoView(e.NewItems[0]);
            });
        }

        private async void PlayerList_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                InfoLabel.Data = $"{Bot.CurrentRoom.PlayerCount}/{Bot.CurrentRoom.MaxPlayers}\t{Bot.CurrentRoom.Name.Split('`')[0].RemoveAll()}";
            });
        }

        private void PlayerList_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            var PlayerList = sender as ListBox;

            if (e.Key.Equals(VirtualKey.C) && Window.Current.CoreWindow.GetKeyState(VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down) && PlayerList.SelectedValue != null)
            {
                DataPackage dataPackage = new DataPackage();
                dataPackage.SetText(PlayerList.SelectedValue.ToString());
                Clipboard.SetContent(dataPackage);
            }
        }

        private void TextBox_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            var TextBox = sender as TextBox;
            if (e.Key.Equals(VirtualKey.Enter) && TextBox.Text != null && TextBox.Text.Trim() != "")
            {
                Bot.SendChatMessage(TextBox.Text);
                TextBox.Text = "";
            }
        }

        private void TextBox_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            TextBox box = sender as TextBox;

            box.FontSize = base.ActualHeight * 0.011;
        }

        private async void UpdatePing()
        {
            while (true)
            {
                if (Bot != null)
                {
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        Pinglabel.Text = " " + Bot.LoadBalancingPeer.RoundTripTime.ToString();

                        if (Bot.State == Photon.Realtime.ClientState.JoinedLobby)
                            InfoLabel.Data = $"Rooms: {Bot.RoomsCount}\tPlayers: {(Bot.PlayersInRoomsCount + Bot.PlayersOnMasterCount)}";

                        StatusLabel.Text = Bot.State.ToString();
                    });
                }
                await Task.Delay(100);
            }
        }

        private async void ConfigBTN_Click(object sender, RoutedEventArgs e)
        {
            AppWindow appWindow = await AppWindow.TryCreateAsync();
            Frame appWindowContentFrame = new Frame();
            appWindowContentFrame.Navigate(typeof(Config_Page));
            ElementCompositionPreview.SetAppWindowContent(appWindow, appWindowContentFrame);
            await appWindow.TryShowAsync();
            appWindow.Closed += delegate
            {
                appWindowContentFrame.Content = null;
                appWindow = null;
            };
        }

        private void SmallButtons_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var BTN = sender as Button;

            BTN.FontSize = base.ActualHeight * 0.00972;
        }

        private async void LoggerBTN_Click(object sender, RoutedEventArgs e)
        {
            AppWindow appWindow = await AppWindow.TryCreateAsync();
            Frame appWindowContentFrame = new Frame();
            appWindowContentFrame.Navigate(typeof(LoggerPage));
            ElementCompositionPreview.SetAppWindowContent(appWindow, appWindowContentFrame);
            await appWindow.TryShowAsync();
            appWindow.Closed += delegate
            {
                appWindowContentFrame.Content = null;
                appWindow = null;
            };
        }

        private void TcpButton_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox checkBox = sender as CheckBox;

            if (Bot != null)
            {
                Bot.TransportProtocol = (bool)checkBox.IsChecked ? ConnectionProtocol.Tcp : ConnectionProtocol.Udp;
            }
            localSettings.Values[Settings.UseTcp] = (bool)checkBox.IsChecked;
        }

        private void LoggerButton_Checked(object sender, RoutedEventArgs e)
        {
            LogAoTTG = (bool)(sender as CheckBox).IsChecked;
        }

        private async void PlayerList_DoubleTapped(object sender, Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            var listBox = sender as ListBox;
            if (listBox.SelectedItem == null) return;

            PlayerInfoPage.Player = listBox.SelectedItem as AoTTGPlayer;

            try
            {
                AppWindow appWindow = await AppWindow.TryCreateAsync();
                Frame appWindowContentFrame = new Frame();
                appWindowContentFrame.Navigate(typeof(PlayerInfoPage));
                ElementCompositionPreview.SetAppWindowContent(appWindow, appWindowContentFrame);
                await appWindow.TryShowAsync();
                appWindow.Closed += delegate
                {
                    appWindowContentFrame.Content = null;
                    appWindow = null;
                };
            }
            catch (Exception ex)
            {
                await new MessageDialog("Failed to open Page: " + ex.ToString()).ShowAsync();
            }
        }
    }
}

//copy

//if (e.Key.Equals(VirtualKey.C) && Window.Current.CoreWindow.GetKeyState(VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down) && PlayerList.SelectedValue != null)
//    {
//        DataPackage dataPackage = new DataPackage();
//dataPackage.SetText(PlayerList.SelectedValue.ToString());
//        Clipboard.SetContent(dataPackage);
//    }