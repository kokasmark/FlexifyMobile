using System.ComponentModel;
using Microsoft.Maui.Controls.Xaml;
using Microsoft.Maui.Controls;

namespace FlexifyMobile;

public partial class WorkoutPage : ContentPage
{
    private string timerString;
    FlexifyDatabase database;
    int seconds = 0;
    string token;
    Template workout;
    public WorkoutPageViewModel ViewModel { get; set; }
    private bool isPressed = false;
    bool fromCalendar;
    
    public WorkoutPage(Template template, bool fromCalendar)
	{
		InitializeComponent();
        database = new FlexifyDatabase();
        List<User> users = database.GetItemsAsync();
        token = users[users.Count - 1].token;
        PageWorkout.Title = template.name;
        workout = template;
        ViewModel = new WorkoutPageViewModel();
        BindingContext = ViewModel;
        ViewModel.CurrentWorkout = template.json[0];
        ViewModel.SetCounter = $"1/{ViewModel.CurrentWorkout.set_data.Length}";
        ViewModel.CurrentSet = ViewModel.CurrentWorkout.set_data[0];
        ViewModel.Paused = false;
        ViewModel.Title = template.name;

        this.fromCalendar = fromCalendar;

        MuscleViewAnimation();
		IncreaseSeconds();
        AnimateGlow();
    }
    public void Next()
    {
        if (!ViewModel.Paused)
        {
            try
            {
                if (ViewModel.SetIndex < workout.json[ViewModel.ExerciseIndex].set_data.Length - 1) ViewModel.SetIndex++;
                else
                {
                    ViewModel.SetIndex = 0;
                    if (ViewModel.ExerciseIndex < workout.json.Length - 1)
                    {
                        ViewModel.ExerciseIndex++;
                    }
                    else
                    {
                        FinishWorkout();
                    }
                }
                ViewModel.CurrentWorkout = workout.json[ViewModel.ExerciseIndex];
                ViewModel.SetCounter = $"{ViewModel.SetIndex + 1}/{ViewModel.CurrentWorkout.set_data.Length}";
                ViewModel.CurrentSet = ViewModel.CurrentWorkout.set_data[ViewModel.SetIndex];
            }
            catch
            {

            }
        }
    }
    public void Previous()
    {
        if (!ViewModel.Paused)
        {
            try
            {
                if (ViewModel.SetIndex > 0) ViewModel.SetIndex--;
                else
                {
                    if (ViewModel.ExerciseIndex > 0)
                    {
                        ViewModel.ExerciseIndex--;
                    }
                    ViewModel.SetIndex = workout.json[ViewModel.ExerciseIndex].set_data.Length;

                }
                ViewModel.CurrentWorkout = workout.json[ViewModel.ExerciseIndex];
                ViewModel.SetCounter = $"{ViewModel.SetIndex + 1}/{ViewModel.CurrentWorkout.set_data.Length}";
                ViewModel.CurrentSet = ViewModel.CurrentWorkout.set_data[ViewModel.SetIndex];
            }
            catch
            {

            }
        }
    }
    public async Task FinishWorkout()
    {
        ViewModel.Paused = true;
        await Task.Delay(100);
        done_gif.IsAnimationPlaying = false;
        await Task.Delay(100);
        done_gif.IsAnimationPlaying = true;
        done_grid.FadeTo(1, 300);
        await Task.Delay(3000);

        if (fromCalendar)
        {
            var client = new HttpClient();

            var request = new HttpRequestMessage(HttpMethod.Post, $"http://{Constants.hostname}:3001/api/workouts/finish");
            var jsonBody = $"{{ \"id\": \"{workout.id}\"}}";
            var content = new StringContent(jsonBody, null, "application/json");
            request.Content = content;
            request.Headers.Add("X-Token", $"{token}");

            var response = await client.SendAsync(request).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
        }
        else
        {

        }
    }
    public void SaveCopy()
    {

    }
    protected async override void OnAppearing()
    {
        base.OnAppearing();
        hide.IsVisible = false;
        done_grid.Opacity = 0;
        seconds = 0;
        await Task.Delay(100);
        imgLoader.IsAnimationPlaying = false;
        await Task.Delay(100);
        imgLoader.IsAnimationPlaying = true;
        await Task.Delay(100);
    }

    private async Task NextAnimation(View view)
    {
        await view.ScaleYTo(0.999, 100, Easing.SinOut);
        await view.ScaleYTo(1.1, 100, Easing.SinOut);
        await view.ScaleYTo(1, 100, Easing.SinOut);
    }
    private async Task AlertAnimation()
    {
        await alert_grid.FadeTo(1.0, 300);
        await Task.Delay(1000);
        await alert_grid.FadeTo(0, 300);
    }
    private async Task AnimateGlow()
    {
        while (true)
        {
            // Fade in
            glowIndicatorRight.FadeTo(0.2, 500); // Adjust the duration as needed
             glowIndicatorLeft.FadeTo(0.2, 500); // Adjust the duration as needed
            await Task.Delay(500); // Adjust the delay between animations as needed

            // Fade out
             glowIndicatorRight.FadeTo(0, 500); // Adjust the duration as needed
             glowIndicatorLeft.FadeTo(0, 500); // Adjust the duration as needed
            await Task.Delay(5000); // Adjust the delay between animations as needed
        }
    }
    async Task MuscleViewAnimation()
    {
        bool front = true;
		while (true)
		{
			await Task.Delay(3000);
			front = !front;
			muscleView_front.FadeTo(front ? 1 : 0, 300);
			muscleView_back.FadeTo(front ? 0 : 1, 300);
		}
	}
    async Task IncreaseSeconds()
	{
        while (true)
		{
            
            await Task.Delay(1000);
            if (!ViewModel.Paused)
            {
                seconds++;
                TimeSpan timer = TimeSpan.FromSeconds(seconds);
                ViewModel.TimerString = $"{timer.Hours:D2}:{timer.Minutes:D2}:{timer.Seconds:D2}";
            }
        }
	}
    private void Home_Clicked(object sender, EventArgs e)
    {
        hide.IsVisible = true;
        Shell.Current.GoToAsync($"//HomePage?Token={token}", false);
    }
    private void Calendar_Clicked(object sender, EventArgs e)
    {
        hide.IsVisible = true;
        Shell.Current.GoToAsync($"//CalendarPage", false);
    }
    private async void Swipe(object sender, SwipedEventArgs e)
    {

        switch (e.Direction)
        {
            case SwipeDirection.Left:
                NextAnimation(HeaderBg);
                NextAnimation(ControlsBg);
                AlertAnimation();
                Next();
                break;
            case SwipeDirection.Right:
                NextAnimation(HeaderBg);
                NextAnimation(ControlsBg);
                AlertAnimation();
                Previous();
                break;
            case SwipeDirection.Up:
                
                break;
            case SwipeDirection.Down:
                
                break;
        }

    }

    private void PauseResume(object sender, EventArgs e)
    {
        ViewModel.Paused = !ViewModel.Paused;
        pause_btn.IsVisible = !ViewModel.Paused;
        resume_btn.IsVisible = ViewModel.Paused;
    }

    private void FinishEarly(object sender, EventArgs e)
    {
        FinishWorkout();
    }
    private async void DisplayAlert(object sender, EventArgs e)
    {
        isPressed = true;
        await Task.Delay(500);
        if (isPressed)
        {
            alert_grid.FadeTo(1,300);
        }
    }
    private void HideAlert(object sender, EventArgs e)
    {
        isPressed = false;
        alert_grid.FadeTo(0, 300);
    }
}
public class WorkoutPageViewModel : INotifyPropertyChanged
{
    string title;
    public string Title
    {
        get { return title; }
        set
        {
            if (value != title)
            {
                title = value;
                OnPropertyChanged(nameof(Title));
            }
        }
    }
    string timerString;
    public string TimerString
    {
        get { return timerString; }
        set
        {
            if (value != timerString)
            {
                timerString = value;
                OnPropertyChanged(nameof(TimerString));
            }
        }
    }
    private bool paused;
    public bool Paused
    {
        get { return paused; }
        set
        {
            if (paused != value)
            {
                paused = value;
                OnPropertyChanged(nameof(Paused));
            }
        }
    }
    private bool details;
    public bool Details
    {
        get { return details; }
        set
        {
            if (details != value)
            {
                details = value;
                OnPropertyChanged(nameof(Details));
            }
        }
    }

    private Json currentWorkout;
    public Json CurrentWorkout
    {
        get { return currentWorkout; }
        set
        {
            if (currentWorkout != value)
            {
                currentWorkout = value;
                OnPropertyChanged(nameof(CurrentWorkout));
            }
        }
    }
    private Set_Data currentSet;
    public Set_Data CurrentSet
    {
        get { return currentSet; }
        set
        {
            if (currentSet != value)
            {
                currentSet = value;
                OnPropertyChanged(nameof(CurrentSet));
            }
        }
    }
    int exerciseIndex = 0;
    public int ExerciseIndex
    {
        get { return exerciseIndex; }
        set
        {
            if (exerciseIndex != value)
            {
                exerciseIndex = value;
                OnPropertyChanged(nameof(ExerciseIndex));
            }
        }
    }
    string setCounter = "";
    public string SetCounter
    {
        get { return setCounter; }
        set
        {
            if (setCounter != value)
            {
                setCounter = value;
                OnPropertyChanged(nameof(SetCounter));
            }
        }
    }
    int setIndex = 0;
    public int SetIndex
    {
        get { return setIndex; }
        set
        {
            if (setIndex != value)
            {
                setIndex = value;
                OnPropertyChanged(nameof(SetIndex));
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}