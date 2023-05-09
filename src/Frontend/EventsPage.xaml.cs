using DataModel;

namespace Frontend;

public partial class EventsPage : ContentPage
{
    int userID;

    public EventsPage(int userID)
    {
        InitializeComponent();

        this.userID = userID;

        IEnumerable<EventInfo> events;
        lock (App.EventsFile)
        {
            events = App.LoadEvents();
        }
        this.ListOfEvents.ItemsSource = events;
    }

    /* TODO
        async Task<Dictionary<int, int>> GetEvents()
        {
            var response = await instance.GetEvents(new GetEventsRequest(userID));
            return response.Events;
        }
    */

    async void ListOfEventsItemSelected(object sender, TappedEventArgs evArgs)
    {
        DateTime utcDateKey = (DateTime)evArgs.Parameter;

        await Navigation.PushAsync(new ClosenessPage(utcDateKey));
    }

}
