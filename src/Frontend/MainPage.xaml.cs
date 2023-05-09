namespace Frontend;

using System;
using System.IO;

using Microsoft.Maui.Devices.Sensors;

#if ANDROID || IOS
using Shiny.Push;
#endif

public partial class MainPage : ContentPage
{
#if ANDROID || IOS
    IPushManager pushManager;
#endif

    public MainPage()
    {
        InitializeComponent();

        MainThread.BeginInvokeOnMainThread(Setup);
    }

    async void Setup()
    {
        if (!await CheckIfLocationPermissionGranted())
            return;

        try
        {
            await GatherLocation(GetUserID());
        }
        catch (Microsoft.Maui.ApplicationModel.FeatureNotEnabledException)
        {
            FallbackLabel.Text = Texts.GpsLocationFeatureIsNeededText;
            MainLayout.IsVisible = false;
        }
    }

    int GetUserID()
    {
        /* no push notifs yet
        #if ANDROID || IOS
                    await SetupPushNotifications(newUserId.ToString());
        #endif
        */

        // no userId yet
        return 0;
    }

    int? lastTapCount = null;
    string[] motivationalTexts = new string[] {
        "Fear? How about going to a medieval war as a warrior!",
        "No repercussions: society is vastly immense and there's no desertion concept nowadays.",
        "Ego: remove that stupid shield that we built as 'sociological protection' which we were not born with.",
        "Belief: picture yourself having success, getting what you want. You've had it before, you are worthy of love.",
        "Mindset: you're like a billionaire handing out cash; if they don't want it, not your fault.",
        "Carpe diem: add a little adventure to your life, you might die tomorrow.",
        "Nothing matters anyway: you will die some day.",
        "The universe doesn't care about your AA, why should you?",
        "She seems in a hurry? Stop that know-it-all inner voice and nod to this other one: 'let's find out'",
        "What they are attracted more is our 6th sense / empathy / capabilities for reading situations; so, does she seem shy at approach? tell her she looks shy, ask if she's shy.",
        "The goal is not to get laid / to get a gf; the goal is to become a more socially calibrated individual.",
        "'I have a boyfriend' ...Oh yeah? How long have you had that problem?",
        "'I have a boyfriend' ...oh then we have something in common for sure, I'm also in a relationship",
        "'Let's go for a coffee/drink...'; 'I have a boyfriend'; ...well he's not invited, this'd be just for the two of us.",
        "Sense of humor: most ppl have it. But their social anxiety, fear of judgment and self-imposed mental filters suppress it. Worried what they say may not be good enough, or they will be judged. Once these barriers (ego) are removed, their humor will often reveal itself naturally.",
        "“It is better to be a fool with the world than to be wise alone.” –Baltasar Gracian",
        "“DG is hard”... WTF is the alternative?",
        "Make experiments to relieve the pressure? e.g. 'I'm not actually hitting on you, I have a gf/married, but thought you look kind of hot'",
        "Most people play not to lose instead of playing to win. Be the 1%.",
    };

    void TapHereTapped(object sender, TappedEventArgs _evArgs)
    {
        var tappedLabel = (Label)sender;
        if (lastTapCount == null)
        {
            lastTapCount = new Random().Next(0, motivationalTexts.Length - 1);
        }
        if (lastTapCount.Value == motivationalTexts.Length - 1)
            lastTapCount = 0;
        else
            lastTapCount = lastTapCount.Value + 1;

        tappedLabel.Text = motivationalTexts[lastTapCount.Value];
    }

    private async Task GatherLocation(int userId)
    {
        MainThread.BeginInvokeOnMainThread(() => {
            CounterLabel.Text =
                $"Our location is...";
            SemanticScreenReader.Announce(CounterLabel.Text);
        });
        var req = new GeolocationRequest(GeolocationAccuracy.Low);
        var location = await Geolocation.GetLocationAsync(req);
        var updateGpsLocationRequest =
            new DataModel.UpdateGpsLocationRequest(
                userId,
                location.Latitude,
                location.Longitude
            );
        CounterLabel.Text =
            $"Our location at{Environment.NewLine}{DateTime.Now.ToString()} is:{Environment.NewLine}{location.Latitude},{location.Longitude}";

        SemanticScreenReader.Announce(CounterLabel.Text);
    }

    #region Permissions
    private async Task<PermissionStatus> RequestAndGetLocationPermission()
    {
        PermissionStatus status = await Permissions.CheckStatusAsync<Permissions.LocationAlways>();

        if (status == PermissionStatus.Granted)
            return status;

        if (status == PermissionStatus.Denied && DeviceInfo.Platform == DevicePlatform.iOS)
            return status;

        if (DeviceInfo.Platform == DevicePlatform.Android)
        {
            await DisplayAlert("Warning", Texts.BackgroundLocationPermissionIsNeededText, "Ok");
            await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        }

        status = await Permissions.RequestAsync<Permissions.LocationAlways>();

        return status;
    }

    private async Task<bool> CheckIfLocationPermissionGranted()
    {
        var locationPermissionStatus = await RequestAndGetLocationPermission();
        if (locationPermissionStatus != PermissionStatus.Granted)
        {
            MainLayout.IsVisible = false;
            return false;
        }

        return true;
    }
    #endregion

    #region Navigations
    void NavigateToAddEventClicked(object sender, EventArgs evArgs)
    {
        Navigation.PushAsync(new AddEventPage(GetUserID()));
    }

    void NavigateToEventsClicked(object sender, EventArgs evArgs)
    {
        Navigation.PushAsync(new EventsPage(GetUserID()));
    }
    #endregion


#if ANDROID || IOS
    async Task SetupPushNotifications(string userID)
    {
        pushManager = this.Handler.MauiContext.Services.GetService<IPushManager>();
        var result = await this.pushManager.RequestAccess();
        if (result.Status == AccessState.Available)
        {
            await pushManager.Tags.AddTag(userID);
        }
    }
#endif
}

