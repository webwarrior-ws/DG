namespace Frontend;

public partial class App : Application
{
    internal static FileInfo EventsFile = new FileInfo(Path.Combine(FileSystem.AppDataDirectory, "events.json"));
    internal static FileInfo EventsFileBackup = new FileInfo(Path.Combine(FileSystem.AppDataDirectory, "events.json.bak"));

    internal static FileInfo NonEventsFile = new FileInfo(Path.Combine(FileSystem.AppDataDirectory, "nonEvents.json"));
    internal static FileInfo NonEventsFileBackup = new FileInfo(Path.Combine(FileSystem.AppDataDirectory, "nonEvents.json.bak"));

    FileInfo everOpenedFile = new FileInfo(Path.Combine(FileSystem.AppDataDirectory, "zero.ini"));
    internal static string DefaultDateTimeFormat = "dddd, dd/MMM/yyyy HH:mm";


    internal static HashSet<string> MyAppearanceCompletionWords = new HashSet<string>();
    internal static HashSet<string> AppearanceCompletionWords = new HashSet<string>();

    public App()
    {
        InitializeComponent();

        MainPage = CheckIfAppIsOpenedFirstTime() ? new MainPage() : new WelcomePage();
    }

    bool CheckIfAppIsOpenedFirstTime()
    {
        if (everOpenedFile.Exists)
        {
            return true;
        }
        else
        {
            File.WriteAllText(everOpenedFile.FullName, String.Empty);
            return false;
        }
    }

    internal static DataModel.EventInfo[] LoadEvents()
    {
        if (!Monitor.IsEntered(App.EventsFile))
            throw new Exception("Access to LoadEvents() without lock");
        if (!App.EventsFile.Exists)
            return Array.Empty<DataModel.EventInfo>();

        var eventsJson = File.ReadAllText(App.EventsFile.FullName);
        if (eventsJson is null)
            throw new Exception("Reading events file returned null");
        if (eventsJson.Trim() == string.Empty)
            throw new Exception("The events file had no content");

        DataModel.EventInfo[] persistedNonEvents =
            DataModel.Marshaller.Deserialize<DataModel.EventInfo[]>(eventsJson);
        EventsFile.CopyTo(EventsFileBackup.FullName, true);
        return persistedNonEvents;
    }

    internal static void SaveEvents(DataModel.EventInfo[] events)
    {
        if (!Monitor.IsEntered(App.EventsFile))
            throw new Exception("Access to SaveNonEvents() without lock");
        var json = DataModel.Marshaller.Serialize(events);
        File.WriteAllText(App.EventsFile.FullName, json);
        EventsFile.Refresh();
    }

    internal static DataModel.NonEvent[] LoadNonEvents()
    {
        if (!Monitor.IsEntered(NonEventsFile))
            throw new Exception("Access to LoadNonEvents() without lock");
        if (!NonEventsFile.Exists)
            return Array.Empty<DataModel.NonEvent>();

        var nonEventsJson = File.ReadAllText(NonEventsFile.FullName);
        if (nonEventsJson is null)
            throw new Exception("Reading nonEvents file returned null");
        if (nonEventsJson.Trim() == string.Empty)
            throw new Exception("The nonEvents file had no content");

        DataModel.NonEvent[] persistedNonEvents =
            DataModel.Marshaller.Deserialize<DataModel.NonEvent[]>(nonEventsJson);
        NonEventsFile.CopyTo(NonEventsFileBackup.FullName, true);
        return persistedNonEvents;
    }

    internal static void SaveNonEvents(DataModel.NonEvent[] nonEvents)
    {
        if (!Monitor.IsEntered(NonEventsFile))
            throw new Exception("Access to SaveNonEvents() without lock");
        var json = DataModel.Marshaller.Serialize(nonEvents);
        File.WriteAllText(NonEventsFile.FullName, json);
        NonEventsFile.Refresh();
    }
}
