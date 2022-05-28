using System.Collections.Generic;

namespace AoTTG_Bot_Rework.Games
{
    internal class AoTTG : IBaseGame
    {
        public string Name => "AoTTG";

        public string AppId => "";

        public string GameVersion => "01042015_1.28";

        public bool CustomCloud => true;

        public Dictionary<PhotonRegion, string> Region => new Dictionary<PhotonRegion, string>()
        {
            { PhotonRegion.Europe,   "135.125.239.180" },
            { PhotonRegion.USA,      "142.44.242.29"   },
            { PhotonRegion.SA,       "172.107.193.233" },
            { PhotonRegion.Asia,     "51.79.164.137"   }
        };
    }
}