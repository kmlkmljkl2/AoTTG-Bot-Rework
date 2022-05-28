using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Core;

namespace AoTTG_Bot_Rework
{
    internal class RoomCallbacks : IInRoomCallbacks
    {
        public ObservableCollection<AoTTGPlayer> PlayerList = new ObservableCollection<AoTTGPlayer>();

        public async void OnMasterClientSwitched(Player newMasterClient)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var player = PlayerList.FirstOrDefault(x => x.Player == newMasterClient);
                if (player != null)
                {
                    PlayerList[PlayerList.IndexOf(player)] = new AoTTGPlayer(newMasterClient);
                }
            });
        }

        public async void OnPlayerEnteredRoom(Player newPlayer)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                PlayerList.Add(new AoTTGPlayer(newPlayer));
            });
        }

        public async void OnPlayerLeftRoom(Player otherPlayer)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                PlayerList.Remove(PlayerList.FirstOrDefault(x => x.Player == otherPlayer));
            });
        }

        public async void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                var player = PlayerList.FirstOrDefault(x => x.Player == targetPlayer);
                if (player != null)
                {
                    PlayerList[PlayerList.IndexOf(player)] = new AoTTGPlayer(targetPlayer);
                }
            });
        }

        public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
        }
    }
}