using Grpc.Core;
using GrpcService;
using GrpcClient;
using System.Text.Json;

using ZXing.Net.Maui;
using ZXing.QrCode.Internal;

using DataModel;
using ZXing.Net.Maui.Controls;
using Frontend.Controls;

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
        LoadPreviousWords();
    }

    public EventPage(Location location, bool solo) : this()
    {
        creationTime = (DateTime.UtcNow, DateTime.Now);
        this.BindingContext = this;

        this.location = location;
        this.solo = solo;
        AutofillAppearanceEditors();
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
        this.myAppearanceCompEditor.Text =
            string.IsNullOrWhiteSpace(ev.MyClothes) ? String.Empty : ev.MyClothes;
        this.appearanceCompEditor.Text =
            string.IsNullOrWhiteSpace(ev.HerClothes) ? String.Empty : ev.HerClothes;
        this.ageEntry.Text = ev.Age.ToString();
        this.ageSwitch.IsToggled = ev.AgeIsExact;

        this.saveButton.Text = "Update";
        this.saveButton.IsEnabled = true;
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

    void RequiredInputWidgetChanged(object _sender, EventArgs _evArgs)
    {
        if (String.IsNullOrWhiteSpace(ageEntry.Text))
        {
            saveButton.IsEnabled = false;
            return;
        }

        if (ratePicker.SelectedIndex == -1)
        {
            saveButton.IsEnabled = false;
            return;
        }

        if (racePicker.SelectedIndex == -1)
        {
            saveButton.IsEnabled = false;
            return;
        }

        saveButton.IsEnabled = true;
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
                    this.solo.Value,
                    myAppearanceCompEditor.Text,
                    appearanceCompEditor.Text
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
                            this.ev.Solo,
                            myAppearanceCompEditor.Text,
                            appearanceCompEditor.Text
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

            foreach (var text in myAppearanceCompEditor.Text.Split(' '))
                App.MyAppearanceCompletionWords.Add(text.ToLower());
            foreach (var text in appearanceCompEditor.Text.Split(' '))
                App.AppearanceCompletionWords.Add(text.ToLower());

        }
    }

    void LoadPreviousWords()
    {
        if (App.MyAppearanceCompletionWords.Count == 0 || App.AppearanceCompletionWords.Count == 0)
        {
            var myClothesWords = new HashSet<string>();
            var herClothesWords = new HashSet<string>();
            lock (App.EventsFile)
            {
                foreach (var eventInfo in App.LoadEvents())
                {
                    if (eventInfo.MyClothes is not null)
                        foreach (var text in eventInfo.MyClothes.Split(' '))
                            myClothesWords.Add(text.ToLower());

                    if (eventInfo.HerClothes is not null)
                        foreach (var text in eventInfo.HerClothes.Split(' '))
                            herClothesWords.Add(text.ToLower());
                }
            }
            if (App.MyAppearanceCompletionWords.Count == 0)
                App.MyAppearanceCompletionWords = myClothesWords;
            if (App.AppearanceCompletionWords.Count == 0)
                App.AppearanceCompletionWords = herClothesWords;
        }


        myAppearanceCompEditor.AutocompletedWords = App.MyAppearanceCompletionWords.ToList();
        appearanceCompEditor.AutocompletedWords = App.AppearanceCompletionWords.ToList();
    }

    void AutofillAppearanceEditors()
    {
        lock (App.EventsFile)
        {
            var lastEvent = App.LoadEvents().OrderByDescending(x => x.DateTime).FirstOrDefault();
            if (lastEvent != null && DateTime.Now.Date == lastEvent.DateTime.Date)
            {
                myAppearanceCompEditor.Text = lastEvent.MyClothes;
            }
        }
    }

    void CompletionReadyEditorFocused(object sender, System.EventArgs e)
    {
        var completionReadyEditor = (CompletionReadyEditor)sender;
        if (completionReadyEditor.CanSelectAll)
        {
            completionReadyEditor.SelectAllText();
            completionReadyEditor.CanSelectAll = false;
        }
    }
}
