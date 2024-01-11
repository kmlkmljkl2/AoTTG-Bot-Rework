using AoTTG_Bot_Rework.AoTTG_Related;
using AoTTG_Bot_Rework.Games;
using AoTTG_Bot_Rework.Handlers;
using ExitGames.Client.Photon;
using ExitGames.Client.Photon.StructWrapping;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Core;

namespace AoTTG_Bot_Rework
{
    public class AoTTG_Bot : LoadBalancingClient, IDisposable
    {
        private ClientState BState = ClientState.Authenticated;
        private System.Timers.Timer _Timer = new System.Timers.Timer();

        public AoTTG_Bot(ConnectionProtocol protocolType) : base(protocolType)
        {
            LocalPlayer.SetCustomProperties(new Hashtable()
            {
                { "name", PlayerName },
                { "dead", true },
                { "kills",  0 },
                { "deaths", 0 },
                { "max_dmg", 0 },
                { "total_dmg" , 0 },
                { "RCteam", 0 }
            });

            AuthValues = new AuthenticationValues()
            {
                UserId = UserID
            };
            Handler = new RPCHandler(this);

            AddCallbackTarget(lobbyCallbacks);
            AddCallbackTarget(roomCallbacks);

            SendAndReceive();

            Handler.AddCallback("Chat", async (args) =>
            {
                object[] message = args.Arguments;
                string content = message[0] as string ?? "";
                string sender = message[1] as string ?? "";

                string Total = (sender == "" ? content : sender + ": " + content).RemoveAll();
                AoTTGMessage messageInfo = new AoTTGMessage()
                {
                    ID = args.CallInfo.Sender.ActorNumber,
                    Message = Total
                };

                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    Messages.Add(messageInfo);
                });
            });
            Logger.Add(new LoggerInfo() { Player = new AoTTGPlayer() });
            _Timer.AutoReset = true;
            _Timer.Interval = 5000;
            _Timer.Elapsed += CollectTrash;
            _Timer.Start();

        }

        private async void CollectTrash(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (LogTime == 0) return;
            if (State != ClientState.Joined) return;

            var LowestAllowed = DateTime.Now.AddSeconds(-LogTime);


            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                lock(Logger)
                {
                    foreach(var i in Logger.ToList())
                    {
                        if (i.Player.ID == 0) continue;
                        TimeSpan time = i.Time - LowestAllowed;
                        if (Math.Abs(time.TotalSeconds) > LogTime) continue;
                        Logger.Remove(i);
                    }
                }
            });
        }

        public enum PhotonTargets
        {
            All,
            Others
        }
        /// <summary>
        /// Time the Logger keeps the Data in Seconds
        /// </summary>
        public int LogTime => Config_Page.LoggerTime;
        public bool LeaveWhenMcDoes => Config_Page.McToggleSetting.Data;

        public IBaseGame Game { get; set; } = new AoTTG();
        public RPCHandler Handler { get; private set; }
        public ObservableCollection<LoggerInfo> Logger { get; } = new ObservableCollection<LoggerInfo>();
        public ObservableCollection<AoTTGMessage> Messages { get; } = new ObservableCollection<AoTTGMessage>();

        public ObservableCollection<AoTTGPlayer> PlayerList
        {
            get
            {
                return roomCallbacks.PlayerList;
            }
        }

        public PhotonRegion Region { get; set; }

        public ObservableCollection<AoTTGRoomInfo> RoomList
        {
            get
            {
                return lobbyCallbacks.Rooms;
            }
        }

        /// <summary>
        /// Connection Protocol (UDP by default)
        /// </summary>
        public ConnectionProtocol TransportProtocol
        {
            get
            {
                return LoadBalancingPeer.TransportProtocol;
            }
            set
            {
                LoadBalancingPeer.TransportProtocol = value;
            }
        }

        private bool IsRunning { get; set; } = true;
        private LobbyCallbacks lobbyCallbacks { get; } = new LobbyCallbacks();
        private ApplicationDataContainer localSettings { get; } = ApplicationData.Current.LocalSettings;
        private string PlayerName => localSettings.Values.ContainsKey(Settings.PlayerName) ? localSettings.Values[Settings.PlayerName] as string : "GUEST" + new Random().Next(0, short.MaxValue);

        private RoomCallbacks roomCallbacks { get; } = new RoomCallbacks();
        private string UserID => localSettings.Values.ContainsKey(Settings.UserId) ? localSettings.Values[Settings.UserId] as string : SetUserId();
        // public ObservableCollection<RoomInfo> RoomList => lobbyCallbacks.RoomList;

        /// <summary>
        /// Connects to region
        /// </summary>
        /// <returns></returns>
        public async Task<bool> ConnectToMasterAsync()
        {
            Task toWait = WhenAny(3020, WaitForConnectAsync());

            if (!ConnectToMaster())
            {
                return false;
            }

            await toWait;
            await Task.Delay(50);

            return State == ClientState.JoinedLobby;
        }

        public void Disconnect()
        {
            IsRunning = false;
            base.Disconnect();
            Dispose();
        }

        public void Dispose()
        {
            IsRunning = false;
            //RoomList.Clear();
            GC.SuppressFinalize(this);
        }

        public async Task<bool> JoinRoomAsync(RoomInfo room)
        {
            if (State != ClientState.JoinedLobby)
            {
                return false;
            }
            Task toWait = WhenAny(3000, WaitForJoinRoomAsync());

            bool canConnect = OpJoinRoom(new EnterRoomParams()
            {
                Lobby = null,
                PlayerProperties = LocalPlayer.CustomProperties,
                RoomName = room.Name
            });

            if (!canConnect)
            {
                return false;
            }

            await toWait;

            return State == ClientState.Joined;
        }

        public override async void OnEvent(EventData photonEvent)
        {
            base.OnEvent(photonEvent);

            if (photonEvent.Parameters.ContainsKey(254))
            {
                try
                {
                    if (!MainPage.LogAoTTG) return;

                    var Info = new LoggerInfo()
                    {
                        Data = photonEvent.ToStringFull(),
                        //ID = (int)photonEvent[254],
                        Time = DateTime.Now,
                        Event = photonEvent.Code,
                        Player = new AoTTGPlayer(CurrentRoom.Players[(int)photonEvent[254]])
                    };
                    await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        Logger.Add(Info);
                    });
                }
                catch
                {
                }
            }
        }

        public override async void OnOperationResponse(OperationResponse operationResponse)
        {
            if (operationResponse.ReturnCode != 0)
            {
                AoTTGMessage messageInfo = new AoTTGMessage()
                {
                    ID = 0,
                    Message = operationResponse.ToStringFull()
                };

                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    Messages.Add(messageInfo);
                });
            }

            //Log.LogInfo(operationResponse.ToStringFull());

            if (operationResponse.OperationCode == 226 || operationResponse.OperationCode == 227)
            {
                if (operationResponse.ReturnCode == 32747)
                {
                    // AuthName = "cunt" + new Random().Next(short.MaxValue);
                    SetUserId();
                }

                if (Game.CustomCloud)
                {
                    if (operationResponse.Parameters.ContainsKey(230))
                    {
                        string address = operationResponse.Parameters[(byte)230] as string;
                        int port = int.Parse(address.Split(':')[1]);
                        operationResponse.Parameters[230] = $"{Game.Region[Region]}:{port}";
                    }
                }
            }
            base.OnOperationResponse(operationResponse);
        }

        public override void OnStatusChanged(StatusCode statusCode)
        {
            base.OnStatusChanged(statusCode);
            switch (statusCode)
            {
                case StatusCode.EncryptionFailedToEstablish:
                case StatusCode.Exception:
                case StatusCode.ExceptionOnConnect:
                case StatusCode.ExceptionOnReceive:
                case StatusCode.SecurityExceptionOnConnect:
                case StatusCode.SendError:
                case StatusCode.DisconnectByServerLogic:
                case StatusCode.DisconnectByServerReasonUnknown:
                case StatusCode.DisconnectByServerTimeout:
                case StatusCode.DisconnectByServerUserLimit:
                case StatusCode.TimeoutDisconnect:

                    break;

                case StatusCode.Disconnect:
                    break;
            }
        }

        public virtual bool SendChatMessage(string content, Player target = null)
        {
            bool result;

            if (target == null)
            {
                result = SendRPC(2, "Chat", new object[] { content, PlayerName.ToHTMLFormat() }, PhotonTargets.All);
            }
            else
            {
                result = SendRPC(2, "Chat", new object[] { content, PlayerName.ToHTMLFormat() }, target.ActorNumber);
            }
            return result;
        }

        public bool SendRPC(int viewId, string rpcName, object[] arguments, int target)
        {
            Hashtable data = PrepareRPCData(viewId, rpcName, arguments);

            RaiseEventOptions options = new RaiseEventOptions
            {
                TargetActors = new int[] { target }
            };

            return OpRaiseEvent(200, data, options, SendOptions.SendReliable);
        }

        public bool SendRPC(int viewId, string rpcName, object[] arguments, PhotonTargets targets)
        {
            Hashtable data = PrepareRPCData(viewId, rpcName, arguments);

            switch (targets)
            {
                case PhotonTargets.All:
                    Handler.OnRPCReceived(data, LocalPlayer);
                    break;

                case PhotonTargets.Others:
                    break;
            }

            return OpRaiseEvent(200, data, RaiseEventOptions.Default, SendOptions.SendReliable);
        }

        private bool ConnectToMaster()
        {
            string port = (TransportProtocol == ConnectionProtocol.Tcp ? "4530" : "5055");
            if (Game.CustomCloud == true)
            {
                if (Game.Region == null || !Game.Region.ContainsKey(Region))
                    throw new Exception("Missing regions");

                MasterServerAddress = $"{Game.Region[Region]}:{port}";
            }
            else
            {
                MasterServerAddress = $"app-{PhotonRegions.Shortcuts[Region]}.exitgamescloud.com:{port}";
            }
            base.AppId = Game.AppId;
            base.AppVersion = Game.GameVersion;
            SendAndReceive();
            return base.ConnectToMasterServer();
        }

        private Hashtable PrepareRPCData(int viewId, string rpcName, object[] args)
        {
            Hashtable result = new Hashtable
            {
                { (byte)0, viewId },
                { (byte)2, LoadBalancingPeer.ServerTimeInMilliSeconds },
                { (byte)3, rpcName },
                { (byte)4, args ?? new object[0] }
            };

            return result;
        }

        private void SendAndReceive()
        {
            IsRunning = true;
            new Thread(() =>
            {
                while (IsRunning)
                {
                    base.Service();
                    Thread.Sleep(50);
                }
                LoadBalancingPeer.StopThread();
            }).Start();
        }

        private string SetUserId()
        {
            localSettings.Values[Settings.UserId] = "cunt" + new Random().Next(0, short.MaxValue);

            return localSettings.Values[Settings.UserId] as string;
        }

        private async Task WaitForConnectAsync()
        {
            for (int i = 0; i < 1500; i++)
            {
                BState = State;
                await Task.Delay(1);
                if (State == ClientState.ConnectedToMasterServer)
                {
                    OpJoinLobby(null);
                    i = 0;
                }
                else if (State == ClientState.JoinedLobby)
                {
                    break;
                }
            }
        }

        private async Task WaitForJoinRoomAsync()
        {
            for (int i = 0; i < 2000; i++)
            {
                await Task.Delay(1);
                if (State == ClientState.Joined)
                {
                    break;
                }
            }
        }

        private Task WhenAny(int milliseconds, Task action)
        {
            Task[] array = new Task[]
            {
                Task.Run(() => Task.Delay(milliseconds)),
                Task.Run(() => action)
            };

            return Task.WhenAny(array);
        }
    }
}