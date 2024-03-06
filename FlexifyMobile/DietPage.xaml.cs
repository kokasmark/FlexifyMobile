using Microsoft.Maui.Controls;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace FlexifyMobile;

public partial class DietPage : ContentPage
{
    FlexifyDatabase database;
    string token = "";
    DietResult dietResult;
    public DietPage()
	{
		InitializeComponent();
        database = new FlexifyDatabase();
        List<User> users = database.GetItemsAsync();
        token = users[users.Count - 1].token;

        getDiet(token, $"{DateTime.Now.Year}-{DateTime.Now.Month:D2}-{3:D2}");
    }

    private void Home_Clicked(object sender, EventArgs e)
    {
        Shell.Current.GoToAsync($"//HomePage?Token={token}", false);
    }
    void InatializeDiet()
    {
        ObservableCollection<Meal> breakfast = new ObservableCollection<Meal>();
        ObservableCollection<Meal> lunch = new ObservableCollection<Meal>();
        ObservableCollection<Meal> dinner = new ObservableCollection<Meal>();
        ObservableCollection<Meal> snacks = new ObservableCollection<Meal>();

        foreach (Meal meal in dietResult.json.breakfast)
        {
            breakfast.Add(meal);
        }
        foreach (Meal meal in dietResult.json.lunch)
        {
            lunch.Add(meal);
        }
        foreach (Meal meal in dietResult.json.dinner)
        {
            dinner.Add(meal);
        }
        foreach (Meal meal in dietResult.json.snacks)
        {
            snacks.Add(meal);
        }

        foods_breakfast.ItemsSource = breakfast;
        foods_lunch.ItemsSource = lunch;
        foods_dinner.ItemsSource = dinner;
        foods_snacks.ItemsSource = snacks;

    }
    async void getDiet(string token, string d)
    {
        var client = new HttpClient();

        var jsonBody = $"{{\"date\": \"{d}\", \"location\": \"{"mobile"}\" }}";
        var content = new StringContent(jsonBody, null, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, $"http://{Constants.hostname}:3001/api/diet");
        request.Headers.Add("X-Token", $"{token}");
        request.Content = content;

        var response = await client.SendAsync(request).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();


        if (response.IsSuccessStatusCode)
        {
            var resdata = await response.Content.ReadAsStringAsync();
            dietResult = JsonSerializer.Deserialize<DietResult>(resdata);

            Device.BeginInvokeOnMainThread(() =>
            {
                InatializeDiet();
            });
        }
    }
}