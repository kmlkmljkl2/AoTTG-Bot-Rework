using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Core;

namespace AoTTG_Bot_Rework
{
    internal class LobbyCallbacks : ILobbyCallbacks
    {
        private ObservableCollection<AoTTGRoomInfo> _rooms { get; } = new ObservableCollection<AoTTGRoomInfo>();

        public ObservableCollection<AoTTGRoomInfo> Rooms => _rooms;

        //public delegate void RoomE(object sender, RoomEventArgs args);

        //public RoomE RoomEvent;
        //public class RoomEventArgs : EventArgs
        //{
        //    public RoomInfo Room { get; set; }
        //    public bool Remove { get; set; }
        //    public bool Existed { get; set; }

        //}
        public void OnJoinedLobby()
        {
            // _roomlist.Clear();
        }

        public void OnLeftLobby()
        {
        }

        public void OnLobbyStatisticsUpdate(List<TypedLobbyInfo> lobbyStatistics)
        {
            //await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            //{
            //    if (args.Remove)
            //        Rooms.Remove(args.Room);
            //    else if (args.Existed)
            //        Rooms[Rooms.IndexOf(Rooms.FirstOrDefault(x => x.Name == args.Room.Name))] = args.Room;
            //    else
            //        Rooms.Add(args.Room);
            //}
        }

        public async void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            foreach (RoomInfo oldroom in roomList)
            {
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    AoTTGRoomInfo AoTTGRoom = new AoTTGRoomInfo(oldroom);

                    var Room = _rooms.FirstOrDefault(inListRoom => inListRoom.Name == AoTTGRoom.Name);
                    if (Room == null)
                    {
                        _rooms.Add(AoTTGRoom);
                    }
                    else
                    {
                        lock (_rooms)
                        {
                            _rooms[_rooms.IndexOf(Room)] = AoTTGRoom;
                            //_rooms.Remove(Room);
                            //_rooms.Add(room);
                        }
                    }
                });
            }
        }
    }
}