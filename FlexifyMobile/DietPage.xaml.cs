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
    public class MealSummaryViewModel
    {
        public int BreakfastTotalCalories { get; set; }
        public int LunchTotalCalories { get; set; }
        public int DinnerTotalCalories { get; set; }
        public int SnacksTotalCalories { get; set; }
    }
    public DietPage()
	{
		InitializeComponent();
        database = new FlexifyDatabase();
        List<User> users = database.GetItemsAsync();
        token = users[users.Count - 1].token;
        string date = $"{DateTime.Now.Year}-{DateTime.Now.Month:D2}-{DateTime.Now.Day:D2}";
        PageDiet.Title = date;
        getDiet(token,date);
    }
    protected async override void OnAppearing()
    {
        base.OnAppearing();
        hide.IsVisible = false;
        await Task.Delay(100);
        imgLoader.IsAnimationPlaying = false;
        await Task.Delay(100);
        imgLoader.IsAnimationPlaying = true;
        await Task.Delay(100);
    }
    void InatializeDiet()
    {
        ObservableCollection<Meal> breakfast = new ObservableCollection<Meal>();
        ObservableCollection<Meal> lunch = new ObservableCollection<Meal>();
        ObservableCollection<Meal> dinner = new ObservableCollection<Meal>();
        ObservableCollection<Meal> snacks = new ObservableCollection<Meal>();

        MealSummaryViewModel viewModel = new MealSummaryViewModel();
        try
        {
            foreach (Meal meal in dietResult.json.breakfast)
            {
                breakfast.Add(meal);
                viewModel.BreakfastTotalCalories += meal.calories;
            }
            foreach (Meal meal in dietResult.json.lunch)
            {
                lunch.Add(meal);
                viewModel.LunchTotalCalories += meal.calories;
            }
            foreach (Meal meal in dietResult.json.dinner)
            {
                dinner.Add(meal);
                viewModel.DinnerTotalCalories += meal.calories;
            }
            foreach (Meal meal in dietResult.json.snacks)
            {
                snacks.Add(meal);
                viewModel.SnacksTotalCalories += meal.calories;
            }
        }
        catch
        {

        }
        // Set the BindingContext to the viewModel
        this.BindingContext = viewModel;

        foods_breakfast.ItemsSource = breakfast;
        foods_lunch.ItemsSource = lunch;
        foods_dinner.ItemsSource = dinner;
        foods_snacks.ItemsSource = snacks;

        foods_breakfast.IsVisible = breakfast.Count > 0;
        foods_lunch.IsVisible = lunch.Count > 0;
        foods_dinner.IsVisible = dinner.Count > 0;
        foods_snacks.IsVisible = snacks.Count > 0;

        breakfast_no_data.IsVisible = breakfast.Count == 0;
        lunch_no_data.IsVisible = lunch.Count == 0;
        dinner_no_data.IsVisible = dinner.Count == 0;
        snacks_no_data.IsVisible = snacks.Count == 0;

        this.BindingContext = viewModel;
    }
    async void getDiet(string token, string d)
    {
        var client = new HttpClient();

        var jsonBody = $"{{\"date\": \"{d}\", \"location\": \"{"mobile"}\" }}";
        var content = new StringContent(jsonBody, null, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, $"http://{Constants.hostname}/api/diet");
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