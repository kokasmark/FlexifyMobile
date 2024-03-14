namespace FlexifyMobile;

public partial class SettingsPage : ContentPage
{
    FlexifyDatabase database;
	public SettingsPage()
	{
        InitializeComponent();
        database = new FlexifyDatabase();
        List<User> users = database.GetItemsAsync();

        if(users[users.Count - 1].anatomy)
        {
            female.BackgroundColor = Colors.Transparent;
            male.BackgroundColor = Color.Parse("#3f8de6");
        }
        else
        {
            male.BackgroundColor = Colors.Transparent;
            female.BackgroundColor = Color.Parse("#3f8de6");
        }
    }

    private async void male_Clicked(object sender, EventArgs e)
    {
        List<User> users = database.GetItemsAsync();
        User userToUpdate = users[users.Count - 1];
        userToUpdate.anatomy = true;
        await database.UpdateItemAsync(userToUpdate);

        female.BackgroundColor = Colors.Transparent;
        male.BackgroundColor = Color.Parse("#3f8de6");
    }

    private async void female_Clicked(object sender, EventArgs e)
    {
        List<User> users = database.GetItemsAsync();
        User userToUpdate = users[users.Count - 1];
        userToUpdate.anatomy = false;
        await database.UpdateItemAsync(userToUpdate);

        male.BackgroundColor = Colors.Transparent;
        female.BackgroundColor = Color.Parse("#3f8de6");
    }

    private async void logout_Clicked(object sender, EventArgs e)
    {
        List<User> users = database.GetItemsAsync();
        User userToUpdate = users[users.Count - 1];
        userToUpdate.token = ""; // Update the token
        await database.UpdateItemAsync(userToUpdate);

        await Device.InvokeOnMainThreadAsync(async () =>
        {
            await Shell.Current.GoToAsync($"//MainPage", true);
        });
    }
}