namespace Frontend;

public partial class App : Application
{
    internal static FileInfo EventsFile = new FileInfo(Path.Combine(FileSystem.AppDataDirectory, "events.json"));
    internal static FileInfo EventsFileBackup = new FileInfo(Path.Combine(FileSystem.AppDataDirectory, "events.json.bak"));

    FileInfo everOpenedFile = new FileInfo(Path.Combine(FileSystem.AppDataDirectory, "zero.ini"));
    internal static string DefaultDateTimeFormat = "dddd, dd/MMM/yyyy HH:mm";


    internal static HashSet<string> MyClothesCompletionWords = new HashSet<string>();
    internal static HashSet<string> HerClothesCompletionWords = new HashSet<string>();

    public App()
    {
        InitializeComponent();

        Page mainPage = CheckIfAppIsOpenedFirstTime() ? new MainPage() : new WelcomePage();
        MainPage = new NavigationPage(mainPage);
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
}
