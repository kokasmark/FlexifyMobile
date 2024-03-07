using System.Linq;
using System.Text.Json;

namespace FlexifyMobile
{
    public partial class MainPage : ContentPage
    {
        FlexifyDatabase database;
        string token;
        public MainPage()
        {
            InitializeComponent();
            database = new FlexifyDatabase();
            List<User> users = database.GetItemsAsync();
            token = users[users.Count - 1].token;
            this.BindingContext = this;
        }
        protected async override void OnAppearing()
        {
            base.OnAppearing();
            await Task.Delay(100);
            imgLoader.IsAnimationPlaying = false;
            await Task.Delay(100);
            imgLoader.IsAnimationPlaying = true;
            await Task.Delay(100);
            TryLoggingIn(token);
        }
        async void TryLoggingIn(string token)
        {
            try
            {
                var client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Get, $"http://{Constants.hostname}:3001/api/user");
                request.Headers.Add("X-Token", $"{token}");
                var response = await client.SendAsync(request).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                Console.WriteLine(await response.Content.ReadAsStringAsync());

                if (response.IsSuccessStatusCode)
                {
                    await Device.InvokeOnMainThreadAsync(async () =>
                    {
                        Shell.Current.GoToAsync($"//HomePage?Token={token}", true);
                    });
                }
            }
            catch
            {
                hide.IsVisible = false;
            }
        }
        private async void login_Btn_Clicked(object sender, EventArgs e)
        {
            var client = new HttpClient();

            string username = username_Ent.Text;
            string password = password_Ent.Text;

            var jsonBody = $"{{ \"user\": \"{username}\", \"password\": \"{password}\", \"location\": \"{"mobile"}\" }}";
            var content = new StringContent(jsonBody, null, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Post, $"http://{Constants.hostname}:3001/api/login");
            request.Content = content;

            var response = await client.SendAsync(request).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            LoginResult token = new LoginResult();

            if (response.IsSuccessStatusCode)
            {
                var resdata = await response.Content.ReadAsStringAsync();
                token = JsonSerializer.Deserialize<LoginResult>(resdata);
            }
            
            await database.SaveItemAsync(new User(username,token.token));
            // Use Device.InvokeOnMainThreadAsync to update the UI on the main thread
            await Device.InvokeOnMainThreadAsync(async () =>
            {
                Console.WriteLine("flexify_token_home:" + token.token);
                Shell.Current.GoToAsync($"//HomePage?Token={token.token}", true);
            });
        }
    }
}