using System;
using System.Collections.ObjectModel;
using System.Globalization;
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
        getDates(token, $"{DateTime.Now.Year}-{DateTime.Now.Month:D2}");
    }
    void InitializeWorkouts()
    {
        string[] monthsShort = new string[] {"Jan", "Feb", "Mar", "Apr",
    "Maj","Jun", "Jul", "Aug", "Sep", "Okt", "Nov", "Dec"};

        Days = new ObservableCollection<DayViewModel>();
        ObservableCollection<DayViewModel> thisWeeksWorkouts = new ObservableCollection<DayViewModel>();

        // Get the start and end of the current week
        DateTime startOfWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
        DateTime endOfWeek = startOfWeek.AddDays(7);

        foreach (var date in workoutDatesResult.dates)
        {
            // Parse the date
            DateTime workoutDate = DateTime.Parse(date);
            WorkoutDataResult data = getData(token, date).Result;
            TimeJson time = JsonSerializer.Deserialize<TimeJson>(data.data[0].time);
            string format = "ddd MMM dd yyyy HH:mm:ss 'GMT'zzz '(''közép-európai téli idõ'')'";

            DateTime start_time = new DateTime();
            DateTime end_time = new DateTime();
            // Parse the string into a DateTime object
            try
            {
                start_time = DateTime.ParseExact(time.start, format, CultureInfo.InvariantCulture);
                end_time = DateTime.ParseExact(time.end, format, CultureInfo.InvariantCulture);
            }
            catch
            {

            }
            string timeString = $"{start_time.Hour + 1}:{start_time.Minute:D2} - {end_time.Hour + 1}:{end_time.Minute:D2}";
            string[] date_parsed = date.Split("-");
            DayViewModel d = new DayViewModel
            {
                Day = monthsShort[int.Parse(date_parsed[1]) - 1] + "\n" + date_parsed[2],
                Description = data != null ? timeString : "No data",
                Title = data != null ? data.data[0].name : "No data",
                Color = data.data[0].isFinished == 1 ? Color.Parse("#22cc33") : Color.Parse("#3f8de6"),
                IsFinished = int.Parse(date_parsed[2]) >= DateTime.Today.Day ?  data.data[0].isFinished != 1 : false,
                Opacity = (data.data[0].isFinished != 1 && int.Parse(date_parsed[2]) >= DateTime.Today.Day) ? 1 : 0.5,
            };
            Days.Add(d);

            // Check if the workout date falls within the current week
            if (workoutDate >= startOfWeek && workoutDate < endOfWeek)
            {
                // Is not finished and not in the past
                if (data.data[0].isFinished != 1 && int.Parse(date_parsed[2]) >= DateTime.Today.Day)
                {
                    thisWeeksWorkouts.Add(d);
                }
            }
        }

        // Sort the collections by date
        Days = new ObservableCollection<DayViewModel>(Days.OrderBy(d => DateTime.ParseExact(d.Day.Split('\n')[1], "dd", CultureInfo.InvariantCulture)));
        thisWeeksWorkouts = new ObservableCollection<DayViewModel>(thisWeeksWorkouts.OrderBy(d => DateTime.ParseExact(d.Day.Split('\n')[1], "dd", CultureInfo.InvariantCulture)));

        // Create a separate collection for this week's workouts
        monthView.ItemsSource = Days; // All workouts
        weekView.ItemsSource = thisWeeksWorkouts; // This week's workouts

    }

    void OnDaySelected(object sender, SelectionChangedEventArgs e)
    {

    }

    private void home_btn_Clicked(object sender, EventArgs e)
    {
        Shell.Current.GoToAsync("//HomePage", true);
    }

    private void diet_btn_Clicked(object sender, EventArgs e)
    {
        Shell.Current.GoToAsync("//DietPage", true);
    }
    async void getDates(string token, string d)
    {
        var client = new HttpClient();

        var jsonBody = $"{{\"date\": \"{d}\", \"location\": \"{"mobile"}\" }}";
        var content = new StringContent(jsonBody, null, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, $"http://{Constants.hostname}:3001/api/workouts/dates");
        request.Headers.Add("X-Token", $"{token}");
        request.Content = content;

        var response = await client.SendAsync(request).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();


        if (response.IsSuccessStatusCode)
        {
            var resdata = await response.Content.ReadAsStringAsync();
            workoutDatesResult = JsonSerializer.Deserialize<WorkoutDatesResult>(resdata);
            
            Device.BeginInvokeOnMainThread(() =>
            {
                InitializeWorkouts();
            });
        }
    }
    async Task<WorkoutDataResult> getData(string t, string d)
    {
        var client = new HttpClient();

        var jsonBody = $"{{ \"token\": \"{t}\", \"date\": \"{d}\", \"location\": \"{"mobile"}\" }}";
        var content = new StringContent(jsonBody, null, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, $"http://{Constants.hostname}:3001/api/workouts/data");
        request.Headers.Add("X-Token", $"{token}");
        request.Content = content;

        var response = await client.SendAsync(request).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();


        if (response.IsSuccessStatusCode)
        {

            var resdata = await response.Content.ReadAsStringAsync();
            if (!resdata.Contains("false"))
            {
                return JsonSerializer.Deserialize<WorkoutDataResult>(resdata);
            }


        }
        return null;
    }
    private void Home_Clicked(object sender, EventArgs e)
    {
        Shell.Current.GoToAsync($"//HomePage?Token={token}", false);
    }
}
