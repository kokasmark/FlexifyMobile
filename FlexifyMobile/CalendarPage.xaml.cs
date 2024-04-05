using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Maui.Controls;

namespace FlexifyMobile;

public partial class CalendarPage : ContentPage
{
    public ObservableCollection<DayViewModel> Days { get; set; }
    public ObservableCollection<DayViewModel> Workouts { get; set; }
    FlexifyDatabase database;
    WorkoutDatesResult workoutDatesResult;
    DateTime selected;
    string token;

    string selectedDay;
    Template selectedTemplate;
    public CalendarPage()
	{
		InitializeComponent();
        selected = DateTime.Now;
        database = new FlexifyDatabase();
        List<User> users = database.GetItemsAsync();
        token = users[users.Count - 1].token;
        getDates(token, $"{DateTime.Now.Year}-{DateTime.Now.Month:D2}");//DateTime.Now.Month
        InitializeTemplates();
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

            Template template = Translator.TranslateToTemplate(data);

            DayViewModel d = new DayViewModel
            {
                Day = monthsShort[int.Parse(date_parsed[1]) - 1] + "\n" + date_parsed[2],
                Description = data != null ? timeString : "No data",
                Title = data != null ? data.data[0].name : "No data",
                Color = data.data[0].isFinished == 1 ? Color.Parse("#22cc33") : Color.Parse("#3f8de6"),
                IsFinished = data.data[0].isFinished == 1,
                IsVisible = int.Parse(date_parsed[2]) >= DateTime.Today.Day ? data.data[0].isFinished != 1 : false,
                Opacity = (data.data[0].isFinished != 1 && int.Parse(date_parsed[2]) >= DateTime.Today.Day) ? 1 : 0.5,
                Data = template
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
    void InitializeTemplates()
    {
        Workouts = new ObservableCollection<DayViewModel>();
        WorkoutTemplate workoutData = getTemplates(token).Result;
        for (int i = 0; i < workoutData.templates.Length; i++)
        {

            Template template = workoutData.templates[i];
            string description = "";
            var ii = 0;
            foreach (Json exercise in template.json)
            {
                description += $"{exercise.set_data.Length}x {exercise.name}";
                if (ii < template.json.Length - 1)
                {
                    description += ", ";
                }
                ii++;
            }
            Workouts.Add(new DayViewModel
            {
                Day = workoutData != null ? $"{template.json.Length.ToString()}\nSet" : "No data",
                Title = workoutData != null ? template.name : "No data",
                Description = workoutData != null ? description : "No data",
                Data = template,
                Color = Color.FromRgba(0, 0, 0, 0)
            }) ;

        }

        workoutCollectionView.ItemsSource = Workouts;
    }
    async Task<WorkoutTemplate> getTemplates(string token)
    {
        var client = new HttpClient();

        var request = new HttpRequestMessage(HttpMethod.Get, $"http://{Constants.hostname}/api/templates");
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
    void OnDaySelected(object sender, SelectionChangedEventArgs e)
    {

    }

    async void getDates(string token, string d)
    {
        var client = new HttpClient();

        var jsonBody = $"{{\"date\": \"{d}\", \"location\": \"{"mobile"}\" }}";
        var content = new StringContent(jsonBody, null, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, $"http://{Constants.hostname}/api/workouts/dates");
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

        var request = new HttpRequestMessage(HttpMethod.Post, $"http://{Constants.hostname}/api/workouts/data");
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
    private void AddWorkout(object sender, EventArgs e)
    {
        selectedDay = DateTime.Now.Day.ToString();
        SelectedDay.Text = selectedDay;
        AddView.IsVisible = true;    
    }
    private void Start_Workout(object sender, EventArgs e)
    {
        hide.IsVisible = true;
        var button = sender as ImageButton;
        var dayViewModel = button.CommandParameter as DayViewModel;
        var template = dayViewModel.Data;

        // Proceed with navigating to the WorkoutPage or any other action with the selected workout data
        if (template != null)
        {
            Navigation.PushAsync(new WorkoutPage(template,true));
        }
    }

    private void cancelAdd_Clicked(object sender, EventArgs e)
    {
        AddView.IsVisible = false;
    }

    private void incrementDay_Clicked(object sender, EventArgs e)
    {
        if ((int.Parse(selectedDay)) < DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month)) {
            incrementDay.Opacity = 1;
            selectedDay = (int.Parse(selectedDay) + 1).ToString();
            SelectedDay.Text = selectedDay;
        }
        else
        {
            incrementDay.Opacity = 0.5;
        }
    }

    private void decreaseDay_Clicked(object sender, EventArgs e)
    {
        if ((int.Parse(selectedDay)) > 0)
        {
            decreaseDay.Opacity = 1;
            selectedDay = (int.Parse(selectedDay) - 1).ToString();
            SelectedDay.Text = selectedDay;
        }
        else
        {
            decreaseDay.Opacity = 0.5;
        }
    }

    private void workoutCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        selectedTemplate = (e.CurrentSelection[0] as DayViewModel).Data;
    }

    public static string ConvertJsonArrayToJson(Json[] jsonArray)
    {
        // Define serialization options
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) } // Convert enums to strings with camel case
        };

        // Serialize the JSON array to JSON using System.Text.Json
        string jsonString = JsonSerializer.Serialize(jsonArray, options);

        // Return the JSON string
        return jsonString;
    }
    private async void confirmAdd_Clicked(object sender, EventArgs e)
    {
        var client = new HttpClient();
        DateTime selectedDateTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, int.Parse(selectedDay));

        // Constructing the time string
        TimeSpan startPicker = start.Time;
        TimeSpan endPicker = end.Time;
        var startTime = $"{selectedDateTime.ToString("ddd MMM dd yyyy HH:mm:ss")} GMT+0100 (közép-európai téli idõ)";
        var endTime = $"{selectedDateTime.AddDays(1).ToString("ddd MMM dd yyyy HH:mm:ss")} GMT+0100 (közép-európai téli idõ)";
        var timeJson = $"{{\"start\":\"{startTime}\",\"end\":\"{endTime}\"}}";

        // Constructing the JSON body
        var jsonBody = $"{{" +
                        $"\"token\": \"{token}\"," +
                        $"\"date\": \"{DateTime.Now.Year}-{DateTime.Now.Month:D2}-{selectedDay:D2}\"," +
                        $"\"time\": {timeJson}," +
                        $"\"location\": \"mobile\"," + // Assuming "mobile" is the value of location
                        $"\"name\": \"{selectedTemplate.name}\"," + // Inserting the name variable
                        $"\"json\": {ConvertJsonArrayToJson(selectedTemplate.json)}" + // Inserting the JSON variable
                    $"}}";

        var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, $"http://{Constants.hostname}/api/workouts/save");
        request.Headers.Add("X-Token", token);
        request.Content = content;

        var response = await client.SendAsync(request).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        if (response.IsSuccessStatusCode)
        {
            var responseData = await response.Content.ReadAsStringAsync();
            if (!responseData.Contains("false"))
            {
                // Success handling
            }
        }

    }
}

