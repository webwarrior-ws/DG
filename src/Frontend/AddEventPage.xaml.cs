using Grpc.Core;
using GrpcService;
using GrpcClient;

using ZXing.Net.Maui;
using ZXing.QrCode.Internal;

using DataModel;
using ZXing.Net.Maui.Controls;

namespace Frontend;

public partial class AddEventPage : ContentPage
{
    int userID;

    private const string OkButtonText = "Ok";
    private (DateTime, DateTime) creationTime = (DateTime.Now, DateTime.UtcNow);

    public AddEventPage(int userID)
    {
        InitializeComponent();

        this.BindingContext = this;
        this.userID = userID;
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

    private void SwitcherToggled(object sender, ToggledEventArgs _)
    {
    }
}
