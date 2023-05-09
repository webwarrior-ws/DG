﻿using Grpc.Core;
using GrpcService;
using GrpcClient;

using ZXing.Net.Maui;
using ZXing.QrCode.Internal;

using DataModel;
using ZXing.Net.Maui.Controls;

namespace Frontend;

public partial class AddEventPage : ContentPage
{
    private (DateTime, DateTime) creationTime = (DateTime.Now, DateTime.UtcNow);
    private decimal[] scores =
        { 10m, 9.5m, 9m, 8.5m, 8m, 7.5m, 7m, 6.5m, 6m, 5.5m, 5m, 4.5m, 4m, 3.5m, 3m, 2.5m, 2m, 1.5m, 1m, 0.5m, 0m };
    private Location location;

    public AddEventPage(Location location)
    {
        InitializeComponent();

        this.BindingContext = this;
        this.location = location;
    }

    public string CreationTime {
        get {
            var thisCreationTime = creationTime;
            return thisCreationTime.Item1.ToString();
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
        var gpsLocation =
            new DataModel.GpsLocation(location.Latitude, location.Longitude);
        lock (App.EventsFile)
        {
            var age = int.Parse(ageEntry.Text);
            var ageIsExact = ageSwitch.IsToggled;

            var ev = new DataModel.EventInfo(
                DateTime.UtcNow,
                DateTime.Now,
                gpsLocation,
                (Race)Enum.Parse(typeof(Race), (string)racePicker.SelectedItem),
                (decimal)ratePicker.SelectedItem,
                age,
                ageIsExact,
                notesEditor.Text
            );

            var events = App.LoadEvents();
            var newEventsList = new List<DataModel.EventInfo>(events);
            newEventsList.Add(ev);
            App.SaveEvents(newEventsList.ToArray());

            MainThread.BeginInvokeOnMainThread(() => {
                Navigation.PopAsync();
            });
        }
    }
}
