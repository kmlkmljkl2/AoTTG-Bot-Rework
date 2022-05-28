using AoTTG_Bot_Rework.AoTTG_Related;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace AoTTG_Bot_Rework.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoggerPage : Page
    {
        private ObservableCollection<LoggerInfo> _logs => MainPage.Instance.Bot.Logger;
        private ObservableCollection<int> Ids = new ObservableCollection<int>();
        private ObservableCollection<int> EventCodes = new ObservableCollection<int>();

        public LoggerPage()
        {
            this.InitializeComponent();
            Loaded += LoggerPage_Loaded;
            _logs.CollectionChanged += _logs_CollectionChanged;
            Unloaded += LoggerPage_Unloaded;
        }

        private void LoggerPage_Unloaded(object sender, RoutedEventArgs e)
        {
            LoggerGrid = null;
        }

        private void _logs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (!Ids.Contains(((LoggerInfo)e.NewItems[0]).Player.ID))
            {
                Ids.Add(((LoggerInfo)e.NewItems[0]).Player.ID);
            }
            if (!EventCodes.Contains(((LoggerInfo)e.NewItems[0]).Event))
            {
                EventCodes.Add(((LoggerInfo)e.NewItems[0]).Event);
            }
        }

        private void LoggerPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoggerGrid.ItemsSource = _logs.ToList();
            foreach (var i in _logs.ToList().OrderBy(x => x.Player.ID))
            {
                if (!Ids.Contains(i.Player.ID))
                {
                    Ids.Add(i.Player.ID);
                }
                if (!EventCodes.Contains(i.Event))
                {
                    EventCodes.Add(i.Event);
                }
            }
        }

        private void Button_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var button = sender as Button;
            button.FontSize = base.ActualHeight * 0.00972;
        }

        private void RefreshBTN_Click(object sender, RoutedEventArgs e)
        {
            RefreshList();
        }

        private void IdSelection_Changed(object sender, SelectionChangedEventArgs e)
        {
            RefreshList();
        }

        private void EventCodeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshList();
        }

        private void RefreshList()
        {
            LoggerGrid.ItemsSource = _logs.ToList();
            ApplyIdFilter();
        }

        private void ApplyIdFilter()
        {
            var IdFilterComboBox = IdComboBox;
            if (IdFilterComboBox.SelectedItem != null && (int)IdFilterComboBox.SelectedItem != 0)
            {
                int filteredId = (int)IdFilterComboBox.SelectedItem;
                IEnumerable<LoggerInfo> log = (IEnumerable<LoggerInfo>)LoggerGrid.ItemsSource;

                LoggerGrid.ItemsSource = log.Where(x => x.Player.ID == filteredId);
            }

            ApplyEventIdFilter();
        }

        private void ApplyEventIdFilter()
        {
            var EventFilterComboBox = EventCodeComboBox;
            if (EventFilterComboBox.SelectedItem == null || (int)EventFilterComboBox.SelectedItem == 0) return;

            int Event = (int)EventFilterComboBox.SelectedItem;
            IEnumerable<LoggerInfo> log = (IEnumerable<LoggerInfo>)LoggerGrid.ItemsSource;
            LoggerGrid.ItemsSource = log.Where(x => x.Event == Event);
        }
    }
}