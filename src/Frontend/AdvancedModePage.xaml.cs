using System.Text.Json;
using System.Text.Json.Nodes;
using DataModel;

namespace Frontend;

public partial class AdvancedModePage : ContentPage
{
    public AdvancedModePage()
    {
        InitializeComponent();
        LoadEventsFiles();
    }

    void LoadEventsFiles()
    {
        if (App.EventsFile.Exists)
            EventsEditor.Text = File.ReadAllText(App.EventsFile.FullName);

        if (App.NonEventsFile.Exists)
            NonEventsEditor.Text = File.ReadAllText(App.NonEventsFile.FullName);
    }

    void SaveButtonClicked(object sender, EventArgs eventArgs)
    {
        if (StatusAppendSwitch.IsToggled)
            SaveForAppend();
        else SaveForStatus();
    }

    async void SaveForStatus()
    {
        try
        {
            if (!string.IsNullOrEmpty(EventsEditor.Text))
                _ = JsonValue.Parse(EventsEditor.Text);

            if (!string.IsNullOrEmpty(NonEventsEditor.Text))
                _ = JsonValue.Parse(NonEventsEditor.Text);
        }
        catch (JsonException)
        {
            await DisplayContentIsNotJsonAlert();
            return;
        }

        try
        {
            IEnumerable<EventInfo> events = string.IsNullOrEmpty(EventsEditor.Text)
                ? new List<EventInfo>()
                : DataModel.Marshaller.Deserialize<IEnumerable<EventInfo>>(EventsEditor.Text);

            IEnumerable<NonEvent> nonEvents = string.IsNullOrEmpty(NonEventsEditor.Text)
                ? new List<NonEvent>()
                : DataModel.Marshaller.Deserialize<IEnumerable<NonEvent>>(NonEventsEditor.Text);

            lock (App.EventsFile)
            {
                App.SaveEvents(events.ToArray());
            }

            lock (App.NonEventsFile)
            {
                App.SaveNonEvents(nonEvents.ToArray());
            }

            await DisplayAlert("Success!", "The changes have been saved", "Ok");
            EventsEditor.Text = File.ReadAllText(App.EventsFile.FullName);
            NonEventsEditor.Text = File.ReadAllText(App.NonEventsFile.FullName);
        }
        catch (JsonException)
        {
            await DisplayJsonStructureIsNotValidAlert();
        }
    }

    async void SaveForAppend()
    {
        if (!string.IsNullOrWhiteSpace(EventsEditor.Text))
        {
            try
            {
                _ = JsonValue.Parse(EventsEditor.Text);
            }
            catch (FormatException)
            {
                await DisplayContentIsNotJsonAlert();
                return;
            }

            IEnumerable<EventInfo> newEvents;

            try
            {
                newEvents = Marshaller.Deserialize<IEnumerable<EventInfo>>(EventsEditor.Text);
            }
            catch (JsonException)
            {
                await DisplayJsonStructureIsNotValidAlert();
                return;
            }

            lock (App.EventsFile)
            {
                var events = App.LoadEvents().ToList();
                events.AddRange(newEvents);
                App.SaveEvents(events.ToArray());
            }
            EventsEditor.Text = string.Empty;
            await DisplayAlert("Success!", "New event(s) appended", "Ok");
        }

        if (!string.IsNullOrWhiteSpace(NonEventsEditor.Text))
        {
            try
            {
                _ = JsonValue.Parse(NonEventsEditor.Text);
            }
            catch (FormatException)
            {
                await DisplayContentIsNotJsonAlert();
                return;
            }

            IEnumerable<NonEvent> newNonEvents;

            try
            {
                newNonEvents = Marshaller.Deserialize<IEnumerable<NonEvent>>(NonEventsEditor.Text);
            }
            catch (JsonException)
            {
                await DisplayJsonStructureIsNotValidAlert();
                return;
            }

            lock (App.NonEventsFile)
            {
                var nonEvents = App.LoadNonEvents().ToList();
                nonEvents.AddRange(newNonEvents);
                App.SaveNonEvents(nonEvents.ToArray());
            }
            NonEventsEditor.Text = string.Empty;
            await DisplayAlert("Success!", "New non-event(s) appended", "Ok");
        }
    }

    void StatusAppendSwitchToggled(object sender, ToggledEventArgs e)
    {
        EventsEditor.Text = string.Empty;
        NonEventsEditor.Text = string.Empty;
        if (!StatusAppendSwitch.IsToggled)
            LoadEventsFiles();
    }

    async Task DisplayContentIsNotJsonAlert()
    {
        await DisplayAlert("Error!", "Content is not Json!", "Ok");
    }

    async Task DisplayJsonStructureIsNotValidAlert()
    {
        await DisplayAlert("Error!", "Json structure is not valid", "Ok");
    }
}
