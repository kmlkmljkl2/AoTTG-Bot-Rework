using System.Collections.Generic;

namespace AoTTG_Bot_Rework
{
    public enum PhotonRegion
    {
        Europe,
        USA,
        Asia,
        SA
    }

    public class PhotonRegions
    {
        public static Dictionary<PhotonRegion, string> Shortcuts = new Dictionary<PhotonRegion, string>()
        {
            { PhotonRegion.Europe, "eu" },
            { PhotonRegion.USA, "us" },
            { PhotonRegion.Asia, "asia" },
            { PhotonRegion.SA, "jp" }
        };
    }
}