using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;

namespace AoTTG_Bot_Rework.Handlers
{
    public class RPCHandler
    {
        private static readonly Dictionary<byte, string> _defaultShortcuts = new Dictionary<byte, string>()
        {
            { 13, SupportedRPC.NetDie },
            { 37, SupportedRPC.NetGameLose},
            { 50, SupportedRPC.NetDie2 },
            { 53, SupportedRPC.Net3DmgSmoke },
            { 62, SupportedRPC.Chat },
            { 83, SupportedRPC.LoadLevelRPC },
            { 85, SupportedRPC.NetGameWin },
            { 86, SupportedRPC.SomeOneIsDead },
            { 87, SupportedRPC.RequireStatus },
            { 88, SupportedRPC.RefreshStatus },
            { 90, SupportedRPC.OneTitanDown },
            { 94, SupportedRPC.GetRacingResult },
            { 95, SupportedRPC.NetRefreshRacingResult },
            { 98, SupportedRPC.RefreshPvpStatus },
            { 113, SupportedRPC.SetMyTeam },
            { 117, SupportedRPC.RefreshAhssPvpStatus },
            { 93, SupportedRPC.UpdateKillInfo },
        };

        private AoTTG_Bot _bot { get; }
        private Dictionary<string, Action<RPCArguments>> _callbacks { get; } = new Dictionary<string, Action<RPCArguments>>();

        public RPCHandler(AoTTG_Bot bot)
        {
            _bot = bot;
            _bot.EventReceived += CheckForRPC;
        }

        private void CheckForRPC(ExitGames.Client.Photon.EventData data)
        {
            if (data.Code == 200 && data.Parameters.ContainsKey(245) && data[245] is Hashtable hash)
            {
                OnRPCReceived(hash, _bot.CurrentRoom.GetPlayer(data.Sender));
            }
        }

        internal void OnRPCReceived(Hashtable hash, Player sender)
        {
            int viewId;
            string rpcName;
            object[] parameters;

            try
            {
                rpcName = "";
                if (hash.ContainsKey((byte)5))
                {
                    byte shortByte = (byte)hash[(byte)5];
                    if (!_defaultShortcuts.ContainsKey(shortByte))
                    {
                        return;
                    }
                    rpcName = _defaultShortcuts[shortByte];
                }
                else if (hash.ContainsKey((byte)3))
                {
                    rpcName = (string)hash[(byte)3];
                }
                else
                {
                    throw new InvalidOperationException("Not found any RPC Name keys");
                }
                viewId = (int)hash[(byte)0];
                parameters = (object[])hash[(byte)4];
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while parsing RPC occured by ID {sender}\nException: {ex.Message}\n{ex.StackTrace}");
                return;
            }

            if (!_callbacks.TryGetValue(rpcName, out Action<RPCArguments> callback))
            {
                return;
            }

            var callArgs = new RPCArguments(_bot, parameters, new RPCCallInfo { Sender = sender, ViewID = viewId });

            callback(callArgs);
        }

        /// <summary>
        /// Executes <paramref name="callback"/> when RPC with name <paramref name="rpcName"/> received
        /// </summary>
        /// <param name="rpcName">Method name</param>
        /// <param name="callback">Method that will be executed</param>
        public void AddCallback(string rpcName, Action<RPCArguments> callback)
        {
            if (rpcName == null)
            {
                throw new ArgumentNullException(nameof(callback));
            }

            if (_callbacks.ContainsKey(rpcName))
            {
                _callbacks[rpcName] = callback;
            }
            else
            {
                _callbacks.Add(rpcName, callback);
            }
        }
    }
}