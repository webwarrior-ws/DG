namespace Frontend;

public partial class AboutPage : ContentPage
{
    public AboutPage()
    {
        InitializeComponent();
        VersionNumberLabel.Text = AppInfo.Current.VersionString;
    }
}
