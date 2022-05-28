using System;

namespace AoTTG_Bot_Rework.AoTTG_Related
{
    public class LoggerInfo
    {
        public DateTime Time { get; set; }
        public int Event { get; set; }

        //public int ID { get; set; }
        public string Data { get; set; }

        public AoTTGPlayer Player { get; set; }
    }
}