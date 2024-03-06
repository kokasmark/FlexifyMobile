using Microsoft.Maui.Controls;
using SkiaSharp;
using SkiaSharp.Views.Maui;
namespace FlexifyMobile;

public partial class DietPage : ContentPage
{
    FlexifyDatabase database;
    string token = "";
    public DietPage()
	{
		InitializeComponent();
        database = new FlexifyDatabase();
        List<User> users = database.GetItemsAsync();
        token = users[users.Count - 1].token;
    }

    private void Home_Clicked(object sender, EventArgs e)
    {
        Shell.Current.GoToAsync($"//HomePage?Token={token}", false);
    }
}