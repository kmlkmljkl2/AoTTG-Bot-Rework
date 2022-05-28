using Photon.Realtime;
using System.Text.RegularExpressions;

namespace AoTTG_Bot_Rework
{
    internal static class Helpers
    {
        private const string RemoveAllPattern = @"((\[([0-9a-f]{6})\])|(<(\/|)(color(?(?=\=).*?)>))|(<size=(\\w*)?>?|<\/size>?)|(<\/?[bi]>))";

        /// <summary>
        /// Removes all tags, etc from string
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static string RemoveAll(this string x)
        {
            return Regex.Replace(x, @"((\[([0-9a-f]{6})\])|(<(\/|)(color(?(?=\=).*?)>))|(<size=(\\w*)?>?|<\/size>?)|(<\/?[bi]>))", "", RegexOptions.IgnoreCase);
        }
    }

    public class AoTTGRoomInfo : RoomInfo
    {
        private RoomInfo _oldInfo { get; }

        public AoTTGRoomInfo(RoomInfo info) : base(info.Name, info.CustomProperties)
        {
            isOpen = info.IsOpen;
            isVisible = info.IsVisible;
            RemovedFromList = info.RemovedFromList;

            masterClientId = info.masterClientId;

            _oldInfo = info;
        }

        public override string ToString()
        {
            string[] name = base.Name.Split('`');
            string RoomName = Regex.Replace(name[0], @"\[([\W\w]{6})\]", "");
            if (RoomName.Length > 50)
            {
                RoomName = RoomName.Remove(46) + "...";
            }

            //string Region = NeedRegions ? "[" + TargetBot.Region.ToString().ToUpper() + "] " : "";
            //string Counter = $"[{new string('0', TotalRooms.ToString().Length - Indexx.ToString().Length) + Indexx}]";
            string isOpen = IsOpen ? "" : "Closed ";
            string HasPassword = name[5] != "" ? "[PWD]" : "";

            string FullName = string.Format("{0, -50} | {1, -21} | {2, -2}/{3, -3} | {4}", RoomName, name[1] /*RoomType*/, _oldInfo.PlayerCount, _oldInfo.MaxPlayers, HasPassword);
            return FullName;
        }
    }

    internal static class StringFormatter
    {
        private static readonly Regex hexCode = new Regex(@"\[([0-9a-f]{6})\]", RegexOptions.IgnoreCase);

        public static string ToHTMLFormat(this string str)
        {
            if (hexCode.IsMatch(str))
            {
                str = str.Contains("[-]")
                    ? hexCode.Replace(str, "<color=#$1>").Replace("[-]", "</color>")
                    : hexCode.Replace(str, "<color=#$1>");
                short c = (short)(str.CountWords("<color=") - str.CountWords("</color>"));
                for (short i = 0; i < c; i++)
                {
                    str += "</color>";
                }
            }
            return str;
        }

        public static int CountWords(this string s, string s1)
        {
            return (s.Length - s.Replace(s1, "").Length) / s1.Length;
        }
    }
}