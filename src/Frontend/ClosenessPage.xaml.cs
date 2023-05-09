using DataModel;

namespace Frontend;

public partial class ClosenessPage : ContentPage
{
    private readonly int userID;
    private readonly int friendID;

    public ClosenessPage(int userID, int friendID)
    {
        InitializeComponent();

        this.userID = userID;
        this.friendID = friendID;
    }
}

