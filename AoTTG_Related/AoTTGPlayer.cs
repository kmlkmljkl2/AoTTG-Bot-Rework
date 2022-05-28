using Photon.Realtime;
using System.Collections.Generic;

namespace AoTTG_Bot_Rework
{
    public class AoTTGPlayer
    {
        public Player Player { get; private set; }
        public string Name { get; private set; } = "Nameless";
        public int Kills { get; private set; } = -1;
        public int Deaths { get; private set; } = -1;
        public int HighestDmg { get; private set; } = -1;
        public int TotalDmg { get; private set; } = -1;
        public bool IsDead { get; private set; } = true;
        public bool IsBot { get; private set; } = false;
        public string PlayerSelection { get; private set; } = "H";
        public int ID => Player == null ? 0 : Player.ActorNumber;
        public int AverageDamage => (int)decimal.Round((TotalDmg == 0 ? 1 : TotalDmg) / (Kills == 0 ? 1 : Kills));
        public double Kda => (double)(Kills == 0 ? 1 : Kills) / (Deaths == 0 ? 1 : Deaths);

        public string Stats
        {
            get
            {
                return $"{Kills}/{Deaths}/{HighestDmg}/{TotalDmg}";
            }
        }

        public string PlayerType
        {
            get
            {
                if (Player.IsMasterClient)
                    return "M";
                else if (IsBot)
                    return "B";
                else
                    return "P";
            }
        }

        public AoTTGPlayer()
        {
        }

        public AoTTGPlayer(Player player)
        {
            Player = player;
            var props = player.CustomProperties;
            if (props.ContainsKey(AoTTG_Properties.Kills) && props[AoTTG_Properties.Kills] is int kills)
            {
                Kills = kills;
            }
            if (props.ContainsKey(AoTTG_Properties.Deaths) && props[AoTTG_Properties.Deaths] is int deaths)
            {
                Deaths = deaths;
            }
            if (props.ContainsKey(AoTTG_Properties.Dead) && props[AoTTG_Properties.Dead] is bool isDead)
            {
                IsDead = isDead;
            }
            if (props.ContainsKey(AoTTG_Properties.Total_dmg) && props[AoTTG_Properties.Total_dmg] is int totaldmg)
            {
                TotalDmg = totaldmg;
            }
            if (props.ContainsKey(AoTTG_Properties.Max_dmg) && props[AoTTG_Properties.Max_dmg] is int highestDMG)
            {
                HighestDmg = highestDMG;
            }
            if (props.ContainsKey(AoTTG_Properties.BotProp) || player.IsLocal)
            {
                IsBot = true;
            }
            if (props.ContainsKey(AoTTG_Properties.IsTitan) && props[AoTTG_Properties.IsTitan] is int isTitan)
            {
                switch (isTitan)
                {
                    case 0:
                    case 1:
                        PlayerSelection = "H";
                        break;

                    case 2:
                        PlayerSelection = "T";
                        break;

                    default:
                        PlayerSelection = "H";
                        break;
                }
            }
            if (props.ContainsKey(AoTTG_Properties.Name) && props[AoTTG_Properties.Name] is string name)
            {
                if (name.RemoveAll().Trim() == "")
                {
                    Name = "Nameless";
                }
                else
                {
                    Name = name.RemoveAll();
                }
            }
        }

        public static IEnumerable<AoTTGPlayer> GetAoTTGPlayers(List<Player> players)
        {
            List<AoTTGPlayer> list = new List<AoTTGPlayer>();
            foreach (var player in players)
            {
                var Player = new AoTTGPlayer(player);
                list.Add(Player);
            }
            return list;
        }

        public override string ToString()
        {
            return string.Format("[{0}] {1, -4} {2} {3}", PlayerSelection, ID, Name, Stats);
        }
    }
}