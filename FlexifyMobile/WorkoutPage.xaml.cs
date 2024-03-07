using System.ComponentModel;

namespace FlexifyMobile;

public partial class WorkoutPage : ContentPage
{
    private string timerString;
    FlexifyDatabase database;
    int seconds = 0;
    string token;
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
    public WorkoutPage()
	{
		InitializeComponent();
        database = new FlexifyDatabase();
        List<User> users = database.GetItemsAsync();
        token = users[users.Count - 1].token;
        MuscleViewAnimation();
		IncreaseSeconds();
        HeaderAnimation();

		this.BindingContext = this;
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
    async Task HeaderAnimation()
    {
        bool f = true;
        while (true)
        {
            await Task.Delay(3000);
            f = !f;
            timer_lbl.FadeTo(f ? 1 : 0, 500);
            exercise_grid.FadeTo(f ? 0 : 1, 500);
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
}