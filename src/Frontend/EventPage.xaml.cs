using Grpc.Core;
using GrpcService;
using GrpcClient;

using ZXing.Net.Maui;
using ZXing.QrCode.Internal;

using DataModel;
using ZXing.Net.Maui.Controls;

namespace Frontend;

public partial class EventPage : ContentPage
{
    private (DateTime, DateTime) creationTime;
    private decimal[] scores =
        { 10m, 9.5m, 9m, 8.5m, 8m, 7.5m, 7m, 6.5m, 6m, 5.5m, 5m, 4.5m, 4m, 3.5m, 3m, 2.5m, 2m, 1.5m, 1m, 0.5m, 0m };
    private Location location = null;
    private bool? solo = null;
    private EventInfo ev = null;

    public EventPage()
    {
        InitializeComponent();
    }

    public EventPage(Location location, bool solo) : this()
    {
        creationTime = (DateTime.UtcNow, DateTime.Now);
        this.BindingContext = this;

        this.location = location;
        this.solo = solo;
    }

    public EventPage(EventInfo ev) : this()
    {
        creationTime = (ev.DateTimeUtc, ev.DateTime);
        this.BindingContext = this;

        this.location = null;
        this.ev = ev;
        this.Title = "View/Update event";

        this.racePicker.SelectedItem = ev.Race.ToString();
        this.ratePicker.SelectedItem = ev.Rate;
        this.notesEditor.Text = ev.Notes;
        this.ageEntry.Text = ev.Age.ToString();
        this.ageSwitch.IsToggled = ev.AgeIsExact;

        this.saveButton.Text = "Update";
    }

    public string CreationTime {
        get {
            var thisCreationTime = creationTime;
            return thisCreationTime.Item2.ToString(App.DefaultDateTimeFormat);
        }
    }

    public IEnumerable<string> Races {
        get {
            return Enum.GetNames(typeof(Race));
        }
    }

    public IEnumerable<decimal> Scores {
        get {
            return scores;
        }
    }

    void SaveClicked(object sender, EventArgs evArgs)
    {
        lock (App.EventsFile)
        {
            var age = int.Parse(ageEntry.Text);
            var ageIsExact = ageSwitch.IsToggled;
            var race = (Race)Enum.Parse(typeof(Race), (string)racePicker.SelectedItem);
            var rate = (decimal)ratePicker.SelectedItem;

            var events = App.LoadEvents();
            if (this.ev is null)
            {
                if (this.location == null)
                    throw new Exception("if this.ev is null then this.location should not be null");
                var gpsLocation =
                    new DataModel.GpsLocation(location.Latitude, location.Longitude);
                var newEv = new DataModel.EventInfo(
                    creationTime.Item1,
                    creationTime.Item2,
                    gpsLocation,
                    race,
                    rate,
                    age,
                    ageIsExact,
                    notesEditor.Text,
                    this.solo.Value
                );

                var newEventsList = new List<DataModel.EventInfo>(events);
                newEventsList.Add(newEv);
                App.SaveEvents(newEventsList.ToArray());
                MainThread.BeginInvokeOnMainThread(() => {
                    Navigation.PopAsync();
                });
            }
            else
            {
                var newEventsList = new List<DataModel.EventInfo>();
                foreach (var ev in events)
                {
                    if (ev.DateTimeUtc == this.ev.DateTimeUtc)
                    {
                        var newEv = new DataModel.EventInfo(
                            this.ev.DateTimeUtc,
                            this.ev.DateTime,
                            this.ev.GpsLocation,
                            race,
                            rate,
                            age,
                            ageIsExact,
                            notesEditor.Text,
                            this.ev.Solo
                        );
                        newEventsList.Add(newEv);
                    }
                    else
                    {
                        newEventsList.Add(ev);
                    }
                }
                App.SaveEvents(newEventsList.ToArray());
                MainThread.BeginInvokeOnMainThread(() => {
                    Navigation.PopAsync();
                });
            }
        }
    }
}
