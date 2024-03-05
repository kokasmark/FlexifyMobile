using Microsoft.Maui.Controls;
using SkiaSharp.Views.Maui;
using SkiaSharp;
using SkiaSharp.Views.Maui.Controls;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Text.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel;

namespace FlexifyMobile
{
    public partial class ColoredSvgContentView : ContentView
    {
        SKCanvasView canvasView;
        string musclesWorkedOn = "";
        FlexifyDatabase database;
        public static readonly BindableProperty IsFrontProperty =
             BindableProperty.Create(nameof(IsFront), typeof(bool), typeof(ColoredSvgContentView), default(bool));
        Dictionary<string,int> frontTips = new Dictionary<string, int>();
        Dictionary<string, int> backTips = new Dictionary<string, int>();
        public bool IsFront
        {
            get { return (bool)GetValue(IsFrontProperty); }
            set { SetValue(IsFrontProperty, value);}
        }

        public ColoredSvgContentView()
        {
            InitializeComponent();
            

            canvasView = new SKCanvasView();
            canvasView.PaintSurface += OnPaintSurface;

            Content = canvasView;
        }
        void InitializeComponent()
        {
            database = new FlexifyDatabase();
            List<User> users = database.GetItemsAsync();
            string t = users[users.Count-1].token;
            GetUserMusclesWorked(t,30);
        }
        public void Tips(bool front)
        {
            for(int ii = 0; ii < 4; ii++)
            {
                Label tip = FindByName($"tip_{ii}") as Label;
                tip.Text = "";
                tip.IsVisible = false;
                Image tip_icon = FindByName($"tip_{ii}_icon") as Image;
                tip_icon.IsVisible = false;
            }
            if (front)
            {
                var i = 0;
                foreach (var t in frontTips)
                {
                    try
                    {
                        Label tip = FindByName($"tip_{i}") as Label;
                        string[] tips = new string[] { $"Consider incorporating more exercises targeting your {t.Key} for balanced development.",
                            $"You are maintaining a good balance in exercising your {t.Key}. Keep it up!",
                            $"Your {t.Key} are over exercised! Let it rest a bit to enchance muscle growth!" };
                        tip.Text = tips[t.Value];
                        Color[] brushes = new Color[] {
                        Color.FromHex("#FBBB21"),
                        Color.FromHex("#F68838"),
                        Color.FromHex("#EE3E32")};
                        tip.TextColor = brushes[t.Value];
                        Image tip_icon = FindByName($"tip_{i}_icon") as Image;
                        tip_icon.IsVisible = true;
                        tip.IsVisible = true;
                        i++;
                    }
                    catch
                    {

                    }

                }
            }
            else
            {
                var i = 0;
                foreach (var t in backTips)
                {
                    try
                    {
                        Label tip = FindByName($"tip_{i}") as Label;
                        string[] tips = new string[] { $"Consider incorporating more exercises targeting your {t.Key} for balanced development.",
                            $"You are maintaining a good balance in exercising your {t.Key}. Keep it up!",
                            $"Your {t.Key} are over exercised! Let it rest a bit to enchance muscle growth!" };
                        tip.Text = tips[t.Value];
                        Color[] brushes = new Color[] {
                        Color.FromHex("#FBBB21"),
                        Color.FromHex("#F68838"),
                        Color.FromHex("#EE3E32")};
                        tip.TextColor = brushes[t.Value];
                        tip.IsVisible = true;
                    }
                    catch
                    {

                    }
                    i++;
                }
            }
        }
        void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;

            BindingContext = this;
            // Load your SVG file
            bool isFront = ((ColoredSvgContentView)BindingContext).IsFront;
            string svgFilePath = "Resources.Images.muscles.svg";
            if (!isFront)
            {
                svgFilePath = "Resources.Images.muscles-back.svg";
                canvas.Scale(0.1f);
            }
            string svgContent = new StreamReader( GetStreamFromFile(svgFilePath)).ReadToEnd();

            // Parse the SVG content
            string[] elements = svgContent.Split("</path>");
            int i = 0;
            // Iterate through each path element and extract ID
            foreach (var pathElement in elements)
            {

                var idMatch = Regex.Match(pathElement, @"id\s*=\s*""([^""]+)""");
                var dMatch = Regex.Match(pathElement, @"d\s*=\s*""([^""]+)""");

                if (idMatch.Success && dMatch.Success)
                {
                    var id = idMatch.Groups[1].Value;
                    var dAttribute = dMatch.Groups[1].Value;

                    // Set different colors based on the path ID or any other condition
                    using (var paint = new SKPaint())
                    {
                        // Customize the paint color based on path ID
                        paint.Color = GetFillColorBasedOnPathId(id);

                        // Draw the path on the canvas
                        SKPath path = SKPath.ParseSvgPathData(dAttribute);
                        canvas.DrawPath(path, paint);
                    }
                }
            }
            //Tips(true);
        }

        async void GetUserMusclesWorked(string token,int timespan)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Post, $"http://{Constants.hostname}:3001/api/user/muscles");
            var jsonBody = $"{{ \"timespan\": \"{timespan}\"}}";
            request.Headers.Add("X-Token", $"{token}");
            var content = new StringContent(jsonBody, null, "application/json");
            request.Content = content;
            var response = await client.SendAsync(request).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            string result = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                dynamic errorObj = JsonConvert.DeserializeObject<dynamic>(result);
                foreach (JProperty variable in errorObj)
                {
                    if (variable.Name == "muscles")
                    {
                        musclesWorkedOn = variable.Value.ToString();
                    }
                   
                }
               
            }
        }
        public static Stream GetStreamFromFile(string filename)
        {
            var assembly = typeof(App).GetTypeInfo().Assembly;
            var assemblyName = assembly.GetName().Name;

            var stream = assembly.GetManifestResourceStream($"{assemblyName}.{filename}");

            return stream;
        }
        SKColor GetFillColorBasedOnPathId(string pathId)
        {
            var muscleGroups = new MuscleGroups();

            Dictionary<string, int> values = new Dictionary<string, int>();
            try
            {
                if (musclesWorkedOn.Length > 0)
                {
                    string[] musclesTrained = musclesWorkedOn.Replace("{", "").Replace("}", "").Split(',');
                    foreach (string muscle in musclesTrained)
                    {
                        var l = muscle.Replace("\n","").Replace("\\","").Replace('"',' ').Replace(" ","").Split(':');
                        var m = l[0];
                        var v = l[1];
                        Console.WriteLine("FlexifyLog: " + m);
                        values.Add(m, int.Parse(v));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Flexify Error: " + e);
                return SKColor.Parse("#3C6FAA");
            }
            int strength = 0;
            try
            {
                foreach (string m in values.Keys)
                {
                    string mm = m;
                    if (muscleGroups.Men[mm].Contains(int.Parse(pathId)))
                    {
                        strength = values[mm];
                        if (strength > 0)
                        {
                            MuscleGroups groups = new MuscleGroups();
                            if (groups.Front.Contains(mm))
                            {
                                if (!frontTips.ContainsKey(mm))
                                {
                                    frontTips.Add(mm, strength);
                                }
                            }
                            if (groups.Back.Contains(mm))
                            {
                                if (!backTips.ContainsKey(mm))
                                {
                                    backTips.Add(mm, strength);
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                return SKColor.Parse("#3C6FAA");
            }
            switch (strength)
            {
                case 0:
                    return SKColor.Parse("#3C6FAA");
                case 1:
                    return SKColor.Parse("#FBBB21");
                case 2:
                    return SKColor.Parse("#F68838");
                case 3:
                    return SKColor.Parse("#EE3E32");
                default:
                    return SKColor.Parse("#3C6FAA");
            }
        }

    }
}
