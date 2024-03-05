using System;
using System.Collections.ObjectModel;
using System.Text.Json;
using Microsoft.Maui.Controls;

namespace FlexifyMobile;

public partial class CalendarPage : ContentPage
{
    public ObservableCollection<DayViewModel> Days { get; set; }
    FlexifyDatabase database;
    WorkoutDatesResult workoutDatesResult;
    DateTime selected;
    string token;
    public CalendarPage()
	{
		InitializeComponent();
        selected = DateTime.Now;
        database = new FlexifyDatabase();
        List<User> users = database.GetItemsAsync();
        token = users[users.Count - 1].token;
        InitializeWorkouts();
    }
    void InitializeWorkouts()
    {
        Days = new ObservableCollection<DayViewModel>();
        WorkoutTemplate workoutData = getData(token).Result;
        for (int i = 0; i < workoutData.templates.Length; i++)
        {

            Template template = workoutData.templates[i];
            Days.Add(new DayViewModel
            {
                Day = workoutData != null ? $"{template.json.Length.ToString()}\nSet" : "No data",
                Title = workoutData != null ? template.name : "No data",
                Data = template
            }) ;
            
        }

        calendarCollectionView.ItemsSource = Days;
    }

    void OnDaySelected(object sender, SelectionChangedEventArgs e)
    {
        DayViewModel dayViewModel = (DayViewModel)e.CurrentSelection[0];
        Border node = (Border)calendarCollectionView.FindByName("workoutDetailContainer");
        if (node != null)
        {
            node.IsVisible = true;
        }
        /*s.IsVisible = true;
        workoutDetail.ItemsSource = dayViewModel.Data;
        workoutDetail.IsVisible = true;*/
    }

    private void home_btn_Clicked(object sender, EventArgs e)
    {
        Shell.Current.GoToAsync("//HomePage", true);
    }

    private void diet_btn_Clicked(object sender, EventArgs e)
    {
        Shell.Current.GoToAsync("//DietPage", true);
    }
    async Task<WorkoutTemplate> getData(string token)
    {
        var client = new HttpClient();

        var request = new HttpRequestMessage(HttpMethod.Get, $"http://{Constants.hostname}:3001/api/templates");
        request.Headers.Add("X-Token", $"{token}");

        var response = await client.SendAsync(request).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();


        if (response.IsSuccessStatusCode)
        {
           
                var resdata = await response.Content.ReadAsStringAsync();
                if (!resdata.Contains("false"))
                {
                    return JsonSerializer.Deserialize<WorkoutTemplate>(resdata);
                }
            

        }
        return null;
    }
}
