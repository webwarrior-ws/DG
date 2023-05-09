using DataModel;

namespace Frontend;

public partial class EventsPage : ContentPage
{
    public EventsPage()
    {
        InitializeComponent();

        IEnumerable<EventInfo> events;
        lock (App.EventsFile)
        {
            events = App.LoadEvents();
        }
        this.ListOfEvents.ItemsSource = events;
    }

    async void ListOfEventsItemSelected(object sender, TappedEventArgs evArgs)
    {
        EventInfo ev = (EventInfo)evArgs.Parameter;

        await Navigation.PushAsync(new EventPage(ev));
    }

}
