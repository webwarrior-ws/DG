
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

    async void SaveButtonClicked(object sender, EventArgs eventArgs)
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

    async Task DisplayContentIsNotJsonAlert()
    {
        await DisplayAlert("Error!", "Content is not Json!", "Ok");
    }

    async Task DisplayJsonStructureIsNotValidAlert()
    {
        await DisplayAlert("Error!", "Json structure is not valid", "Ok");
    }
}
