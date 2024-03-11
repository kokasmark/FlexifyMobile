using System.ComponentModel;
using Microsoft.Maui.Controls.Xaml;

namespace FlexifyMobile;

public partial class WorkoutPage : ContentPage
{
    private string timerString;
    FlexifyDatabase database;
    int seconds = 0;
    string token;

    Template workout;
    public WorkoutPageViewModel ViewModel { get; set; }
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

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    public void Next()
    {
        try
        {
            if (ViewModel.SetIndex < workout.json[ViewModel.ExerciseIndex].set_data.Length) ViewModel.SetIndex++;
            else
            {
                ViewModel.SetIndex = 0;
                if (ViewModel.ExerciseIndex < workout.json.Length)
                {
                    ViewModel.ExerciseIndex++;
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
    public void Previous()
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
    public WorkoutPage(Template template)
	{
		InitializeComponent();
        database = new FlexifyDatabase();
        List<User> users = database.GetItemsAsync();
        token = users[users.Count - 1].token;
        PageWorkout.Title = template.name;
        MuscleViewAnimation();
		IncreaseSeconds();
        StartBounceAnimation(ControlsBg);
        workout = template;
        ViewModel = new WorkoutPageViewModel();
        BindingContext = ViewModel;
        ViewModel.CurrentWorkout = template.json[0];
        ViewModel.SetCounter = $"1/{ViewModel.CurrentWorkout.set_data.Length}";
        ViewModel.CurrentSet = ViewModel.CurrentWorkout.set_data[0]; 
        ViewModel.Paused = false;
    }
    protected async override void OnAppearing()
    {
        base.OnAppearing();
        hide.IsVisible = false;
        seconds = 0;
        await Task.Delay(100);
        imgLoader.IsAnimationPlaying = false;
        await Task.Delay(100);
        imgLoader.IsAnimationPlaying = true;
        await Task.Delay(100);
    }
    private async Task BounceAnimation(View view)
    {
        await view.ScaleYTo(0.999, 100, Easing.SinOut);
        await view.ScaleYTo(1.01, 100, Easing.SinOut);
        await view.ScaleYTo(1, 100, Easing.SinOut);
    }
    private async Task NextAnimation(View view)
    {
        await view.ScaleYTo(0.999, 100, Easing.SinOut);
        await view.ScaleYTo(1.1, 100, Easing.SinOut);
        await view.ScaleYTo(1, 100, Easing.SinOut);
    }
    private async Task DetailsAnimation(View view)
    {
        ViewModel.Details = true;
        await Task.Delay(1000);
        ViewModel.Details = false;
    }
    public async Task StartBounceAnimation(View view)
    {
        while (true)
        {
            await BounceAnimation(view);
            await BounceAnimation(view);

            await Task.Delay(2000); // Wait for 2 seconds before repeating
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
			seconds++;
            TimeSpan timer = TimeSpan.FromSeconds(seconds);
            TimerString = $"{timer.Hours:D2}:{timer.Minutes:D2}:{timer.Seconds:D2}";
            timer_lbl.Text = timerString;
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
                DetailsAnimation(exercise_grid);
                Next();
                break;
            case SwipeDirection.Right:
                NextAnimation(HeaderBg);
                NextAnimation(ControlsBg);
                DetailsAnimation(exercise_grid);
                Previous();
                break;
            case SwipeDirection.Up:
                ViewModel.Details = true;
                break;
            case SwipeDirection.Down:
                ViewModel.Details = false;
                break;
        }

    }
}
public class WorkoutPageViewModel : INotifyPropertyChanged
{
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