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
    public bool IsFinished { get; set; }
    public bool IsVisible { get; set; }

    public double Opacity { get; set; } 

    public Color Color { get; set; }
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

    bool swipedUp = false;
    WorkoutTemplate workoutData;



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
        StartBounceAnimation(calendarCollectionView);
        AutoRotate();
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
        Days = new ObservableCollection<DayViewModel>();
        workoutData = getTemplates(token).Result;
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
            Days.Add(new DayViewModel
            {
                Day = workoutData != null ? $"{template.json.Length.ToString()}\nSet" : "No data",
                Title = workoutData != null ? template.name : "No data",
                Description = workoutData != null ? description : "No data",
                Data = template
            });

        }

        calendarCollectionView.ItemsSource = Days;
    }
    private  async Task BounceAnimation(View view)
    {
        await view.ScaleYTo(0.999, 100, Easing.SinOut);
        await view.ScaleYTo(1.01, 100, Easing.SinOut);
        await view.ScaleYTo(1, 100, Easing.SinOut);
    }
    public  async Task StartBounceAnimation(View view)
    {
        while (true)
        {
            if (!swipedUp)
            {
                await BounceAnimation(view);
                await BounceAnimation(view);
            }
            await Task.Delay(2000); // Wait for 2 seconds before repeating
        }
    }

    async void StartAnim(uint speed)
    {
        muscleView_front.FadeTo(1f, speed);
        muscleView_back.FadeTo(0.1f, speed);
        muscleView_back.TranslateTo(280, 225, speed);
        muscleView_back.ScaleTo(1.2, speed);
        muscleView_front.TranslateTo(0, 200, speed);
    }
    async Task AutoRotate()
    {
        while (true)
        {
            if (!swipedUp)
            {
                Rotate(1000);
            }
            await Task.Delay(5000); 
        }
    }
    async void Rotate(uint speed)
    {
        if (!swipedUp)
        {
            front = !front;
            if (front == false)
            {
                muscleView_front.FadeTo(0.1f, speed);
                muscleView_back.FadeTo(1, speed);
                muscleView_back.ScaleTo(1.4, speed);
                muscleView_front.ScaleTo(1.3, speed);
                muscleView_back.TranslateTo(170, 225, speed);
                muscleView_front.TranslateTo(110, 200, speed);
            }
            else
            {
                muscleView_front.FadeTo(1f, speed);
                muscleView_back.FadeTo(0.1f, speed);
                muscleView_back.ScaleTo(1.2, speed);
                muscleView_front.ScaleTo(1.5, speed);
                muscleView_back.TranslateTo(280, 225, speed);
                muscleView_front.TranslateTo(0, 200, speed);
            }
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
                Page.Title = $"Hey, {user.username}!";
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

                    DateTime start_time = DateTime.Today;
                    DateTime end_time = DateTime.Today;
                    // Parse the string into a DateTime object
                    try
                    {
                        start_time = DateTime.ParseExact(time.start, format, CultureInfo.InvariantCulture);
                        end_time = DateTime.ParseExact(time.end, format, CultureInfo.InvariantCulture);
                    }
                    catch
                    {

                    }
                    string timeString = $"{start_time.Hour+1}:{start_time.Minute:D2} - {end_time.Hour+1}:{end_time.Minute:D2}";
                    dayViewModel = new DayViewModel {
                        Day = monthsShort[DateTime.Now.Month - 1] + "\n" + DateTime.Now.Day.ToString(),
                        Description = workoutData != null ? timeString : "No data",
                        Title = workoutData != null ? workoutData.data[0].name : "No data",
                        Color = workoutData.data[0].isFinished == 1 ? Color.Parse("#22cc33") : Color.Parse("#3f8de6"),
                        IsFinished = workoutData.data[0].isFinished == 1,
                        IsVisible = workoutData.data[0].isFinished != 1
                    };
                    BindingContext = dayViewModel;
                }
                else
                {
                    todays_workout.IsVisible = false;
                    add_workout.IsVisible = true;
                }
            });
        }
    }
    async Task<WorkoutTemplate> getTemplates(string token)
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

            todays_workout.TranslateTo(0, -600, 200);
            add_workout.TranslateTo(0, -600, 200);
            timeSpan.FadeTo(0, 300);
        }
        else
        {
            calendarCollectionView.TranslateTo(0, 240, 200);
            calendarCollectionView.FadeTo(0.5, 300);
            muscleView_front.FadeTo(front ? 1: 0.1, 300);
            muscleView_back.FadeTo(front ?  0.1 : 1, 300);

            todays_workout.TranslateTo(0, -65, 300);
            add_workout.TranslateTo(0, -65, 300);
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
                swipedUp = true;
                break;
            case SwipeDirection.Down:
                WorkoutsToggle(false);
                swipedUp = false;
                break;
        }
        
    }

    private void TimeSpan_Clicked(object sender, EventArgs e)
    {
        foreach (var child in timeSpanGrid.Children)
        {
            if (child is Button b)
            {
                b.Opacity = 0.8;
                b.FontAttributes = FontAttributes.None;
            }
        }
        if (sender is Button button)
        {
            string buttonText = button.Text;
            button.Opacity = 1;
            button.FontAttributes = FontAttributes.Bold;
            switch (buttonText)
            {
                case "Weekly":
                    muscleView_front.ChangeTimeSpan(7);
                    muscleView_back.ChangeTimeSpan(7);
                    break;
                case "Monthly":
                    muscleView_front.ChangeTimeSpan(30);
                    muscleView_back.ChangeTimeSpan(30);
                    break;
                case "6 Months":
                    muscleView_front.ChangeTimeSpan(180);
                    muscleView_back.ChangeTimeSpan(180);
                    break;
                case "A Year":
                    muscleView_front.ChangeTimeSpan(365);
                    muscleView_back.ChangeTimeSpan(365);
                    break;
                case "All":
                    muscleView_front.ChangeTimeSpan(69420);
                    muscleView_back.ChangeTimeSpan(69420);
                    break;
                default:
                    muscleView_front.ChangeTimeSpan(30);
                    muscleView_back.ChangeTimeSpan(30);
                    break;
            }
        }
    }

    private void Diet_Clicked(object sender, EventArgs e)
    {
        hide.IsVisible = true;
        Shell.Current.GoToAsync($"//DietPage", false);
    }
    private void Other_Clicked(object sender, EventArgs e)
    {
        hide.IsVisible = true;
        Shell.Current.GoToAsync($"//WorkoutPage", false);
    }
    private void Calendar_Clicked(object sender, EventArgs e)
    {
        hide.IsVisible = true;
        Shell.Current.GoToAsync($"//CalendarPage", false);
    }
    private void Start_Workout(object sender, EventArgs e)
    {
        var button = sender as ImageButton;
        var dayViewModel = button.CommandParameter as DayViewModel;
        var template = dayViewModel.Data;

        // Proceed with navigating to the WorkoutPage or any other action with the selected workout data
        if (template != null)
        {
            Navigation.PushAsync(new WorkoutPage(template));
        }
    }
}
