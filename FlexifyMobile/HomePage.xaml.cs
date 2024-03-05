using System.Text.Json;
using System.ComponentModel;
using System.Globalization;
using System.Collections.ObjectModel;

namespace FlexifyMobile;

public class DayViewModel
{
    public string Title { get; set; }
    public string Day { get; set; }
    public string Description { get; set; }
    public Template Data;
}

public partial class HomePage : ContentPage, INotifyPropertyChanged
{
    bool front = true;
    public DayViewModel dayViewModel;
    WorkoutDatesResult workoutDatesResult;
    FlexifyDatabase database;
    public ObservableCollection<DayViewModel> Days { get; set; }
    string token;
    
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
            });

        }

        calendarCollectionView.ItemsSource = Days;
    }
    public HomePage()
	{
        InitializeComponent();
        database = new FlexifyDatabase();
        List<User> users = database.GetItemsAsync();
        token = users[users.Count - 1].token;
        GetUserInformation(token);
        getDates(token, $"{DateTime.Now.Year}-{DateTime.Now.Month:D2}");
        InitializeWorkouts();
        StartAnim(500);
    }
    async void StartAnim(uint speed)
    {
        muscleView_front.FadeTo(1f, speed);
        muscleView_back.FadeTo(0.1f, speed);
        muscleView_back.TranslateTo(280, 225, speed);
        muscleView_front.TranslateTo(0, 200, speed);
    }
    async void Rotate(uint speed)
    {
        front = !front;
        if (front == false)
        {
            muscleView_front.FadeTo(0.1f, speed);
            muscleView_back.FadeTo(1, speed);
            muscleView_back.TranslateTo(170, 225, speed);
            muscleView_front.TranslateTo(110, 200, speed);
        }
        else
        {
            muscleView_front.FadeTo(1f, speed);
            muscleView_back.FadeTo(0.1f, speed);
            muscleView_back.TranslateTo(280, 225, speed);
            muscleView_front.TranslateTo(0, 200, speed);
        }
    }
    async void GetUserInformation(string token)
	{
        var client = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Get, $"http://{Constants.hostname}:3001/api/user");
        request.Headers.Add("X-Token", $"{token}");
        var response = await client.SendAsync(request).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        Console.WriteLine(await response.Content.ReadAsStringAsync());

		if (response.IsSuccessStatusCode)
		{
            var resdata = await response.Content.ReadAsStringAsync();
            User user = JsonSerializer.Deserialize<User>(resdata);
            await Device.InvokeOnMainThreadAsync(() =>
            {
                Page.Title = $"Welcome {user.username}!";
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
            string[] monthsShort = new string[] {"Jan", "Feb", "Mar", "Apr",
            "Maj","Jun", "Jul", "Aug", "Sep", "Okt", "Nov", "Dec"};
            Device.BeginInvokeOnMainThread(() =>
            {
                if (workoutDatesResult.dates.ToList().Contains($"{DateTime.Now.Year}-{DateTime.Now.Month:D2}-{DateTime.Now.Day:D2}"))
                {
                    WorkoutDataResult workoutData = getData(token, $"{DateTime.Now.Year}-{DateTime.Now.Month:D2}-{DateTime.Now.Day:D2}").Result;
                    TimeJson time = JsonSerializer.Deserialize<TimeJson>(workoutData.data[0].time);
                    string format = "ddd MMM dd yyyy HH:mm:ss 'GMT'zzz '(''közép-európai téli idõ'')'";

                    // Parse the string into a DateTime object
                    DateTime start_time = DateTime.ParseExact(time.start, format, CultureInfo.InvariantCulture);
                    DateTime end_time = DateTime.ParseExact(time.end, format, CultureInfo.InvariantCulture);
                    string timeString = $"{start_time.Hour+1}:{start_time.Minute:D2} - {end_time.Hour+1}:{end_time.Minute:D2}";
                    dayViewModel = new DayViewModel {
                        Day = monthsShort[DateTime.Now.Month - 1] + "\n" + DateTime.Now.Day.ToString(),
                        Description = workoutData != null ? timeString : "No data",
                        Title = workoutData != null ? workoutData.data[0].name : "No data"
                    };
                    BindingContext = dayViewModel;
                }
                else
                {
                    todays_workout.IsVisible = false;
                }
            });
        }
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

    void WorkoutsToggle(bool toggle)
    {
        if (toggle)
        {
            calendarCollectionView.TranslateTo(0, -300, 300);
            calendarCollectionView.FadeTo(1, 300);
            muscleView_front.FadeTo(0, 300);
            muscleView_back.FadeTo(0, 300);

            moreGrid.FadeTo(0, 300);
            todays_workout.FadeTo(0, 300);
            timeSpan.FadeTo(0, 300);
        }
        else
        {
            calendarCollectionView.TranslateTo(0, 230, 300);
            calendarCollectionView.FadeTo(0.5, 300);
            muscleView_front.FadeTo(front ? 1: 0.1, 300);
            muscleView_back.FadeTo(front ?  0.1 : 1, 300);

            moreGrid.FadeTo(1, 300);
            todays_workout.FadeTo(1, 300);
            timeSpan.FadeTo(1, 300);
        }
    }
    private void Swipe(object sender, SwipedEventArgs e)
    {

        switch (e.Direction)
        {
            case SwipeDirection.Left:
                Rotate(500);
                //muscleView_front.Tips(front);
                break;
            case SwipeDirection.Right:
                Rotate(500);
                //muscleView_front.Tips(front);
                break;
            case SwipeDirection.Up:
                WorkoutsToggle(true);
                break;
            case SwipeDirection.Down:
                WorkoutsToggle(false);
                break;
        }
        
    }

}
