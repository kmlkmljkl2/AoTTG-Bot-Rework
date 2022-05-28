using System;
using Windows.UI.Xaml.Media;

namespace AoTTG_Bot_Rework
{
    public class AoTTGMessage
    {
        public int ID { get; set; }

        //public string Name { get; set; }
        public string Message { get; set; }

        // public string Test => new Run() { Text = Message, Foreground = new SolidColorBrush(Colors.Red) }.ToString();

        // public List<Run> Test { get; set; }

        // public Paragraph Paragraph { get; set; } = new Paragraph();

        public AoTTGMessage()
        {
            //string[] test = Message.Split("<color=");
            //if (test.Length > 0)
            //{
            //    foreach (var i in test)
            //    {
            //        if (i.Trim() == "") continue;

            //        if (i.Length > 6 && i.StartsWith("#") && i[7] == '>')
            //        {
            //            Paragraph.Inlines.Add(new Run()
            //            {
            //                Text = i.Remove(7, 1).Replace("</color>", ""),
            //                Foreground = GetSolidColorBrush(i.Remove(7))
            //            });

            //        }
            //        else if (i.Length > 5 && i[6] == '>')
            //        {
            //            Paragraph.Inlines.Add(new Run()
            //            {
            //                Text = i.Remove(6, 1).Replace("</color>", ""),
            //                Foreground = GetSolidColorBrush(i.Remove(6))
            //            });
            //        }
            //        else
            //        {
            //            Paragraph.Inlines.Add(new Run()
            //            {
            //                Text = i.RemoveAll(),
            //                Foreground = new SolidColorBrush(Colors.White)
            //            });

            //        }

            //    }

            //}
            //else
            //{
            //    Paragraph.Inlines.Add(new Run() { Text = Message, Foreground = new SolidColorBrush(Colors.White) });
            //   // Test.Add(new Run() { Text = Message, Foreground = new SolidColorBrush(Colors.White) });
            //}
        }

        public SolidColorBrush GetSolidColorBrush(string hex)
        {
            hex = hex.Replace("#", string.Empty);
            byte a = (byte)(Convert.ToUInt32(hex.Substring(0, 2), 16));
            byte r = (byte)(Convert.ToUInt32(hex.Substring(2, 2), 16));
            byte g = (byte)(Convert.ToUInt32(hex.Substring(4, 2), 16));
            byte b = (byte)(Convert.ToUInt32(hex.Substring(6, 2), 16));
            SolidColorBrush myBrush = new SolidColorBrush(Windows.UI.Color.FromArgb(a, r, g, b));
            return myBrush;
        }

        public override string ToString()
        {
            return string.Format("{0, -3}{1}", "[" + ID + "]", Message);
        }
    }
}