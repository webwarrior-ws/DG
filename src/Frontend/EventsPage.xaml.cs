using DataModel;

namespace Frontend;

public partial class EventsPage : ContentPage
{
    Dictionary<int, string> events = new Dictionary<int, string>();
    int userID;

    public EventsPage(int userID)
    {
        InitializeComponent();
        this.userID = userID;
    }

    /* TODO
        async Task<Dictionary<int, int>> GetEvents()
        {
            var response = await instance.GetEvents(new GetEventsRequest(userID));
            return response.Events;
        }

        async void ListOfEventsItemSelected(object sender, TappedEventArgs e)
        {
            int eventID = (int)e.Parameter;

            if (!events.ContainsKey(eventID))
                return;

            await Navigation.PushAsync(new ClosenessPage(userID, eventID));
        }
    */

}
