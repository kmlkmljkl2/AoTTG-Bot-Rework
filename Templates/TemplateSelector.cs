using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace AoTTG_Bot_Rework.Templates
{
    public class Tempselector : DataTemplateSelector
    {
        public DataTemplate StandartTemplate { get; set; }
        public DataTemplate RoomTemplate { get; set; }
        public DataTemplate AoTTGMessages { get; set; }

        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is AoTTGRoomInfo)
                return RoomTemplate;
            if (item is AoTTGMessage)
                return AoTTGMessages;
            else
                return StandartTemplate;
        }
    }
}