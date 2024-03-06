using Microsoft.Maui.Controls;
using SkiaSharp;
using SkiaSharp.Views.Maui;
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