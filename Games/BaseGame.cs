using System.Collections.Generic;

namespace AoTTG_Bot_Rework.Games
{
    public interface IBaseGame
    {
        string Name { get; }
        string AppId { get; }
        string GameVersion { get; }
        bool CustomCloud { get; }
        Dictionary<PhotonRegion, string> Region { get; }
    }
}