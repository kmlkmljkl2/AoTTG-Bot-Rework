namespace AoTTG_Bot_Rework.Handlers
{
    /// <summary>
    /// Arguments of RPC call
    /// </summary>
    public sealed class RPCArguments
    {
        /// <summary>
        /// Received method arguments
        /// </summary>
        public object[] Arguments { get; }

        /// <summary>
        /// Meta information
        /// </summary>
        public RPCCallInfo CallInfo { get; }

        /// <summary>
        /// Current client instance
        /// </summary>
        public AoTTG_Bot Client { get; }

        internal RPCArguments(AoTTG_Bot client, object[] args, RPCCallInfo callInfo)
        {
            CallInfo = callInfo;
            Client = client;
            Arguments = args;
        }
    }
}