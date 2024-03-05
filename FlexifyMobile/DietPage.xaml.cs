using Microsoft.Maui.Controls;
using SkiaSharp;
using SkiaSharp.Views.Maui;

namespace FlexifyMobile;

public partial class DietPage : ContentPage
{
	public DietPage()
	{
		InitializeComponent();
	}
    void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
    {
        SKImageInfo info = args.Info;
        SKSurface surface = args.Surface;
        SKCanvas canvas = surface.Canvas;

        canvas.Clear();
        float radius = Math.Min(info.Width, info.Height) / 2.0f;
        float centerX = info.Width / 2.0f;
        float centerY = info.Height / 2.0f;

        SKRect rect = new SKRect(centerX - radius, centerY - radius, centerX + radius, centerY + radius);

        using (var paint = new SKPaint())
        {
            paint.Style = SKPaintStyle.Fill;

            paint.TextSize = 30;
            // Draw slices
            paint.Color = SKColor.Parse("#1C1533");
            canvas.DrawArc(rect, 0, 45, true, paint);
            canvas.DrawText("Carbs", centerX, centerY, paint);

            paint.Color = SKColor.Parse("#3C6FAA");
            canvas.DrawArc(rect, 45, 90, true, paint);
            canvas.DrawText("Fat", centerX, centerY, paint);

            paint.Color = SKColor.Parse("#10D8B8");
            canvas.DrawArc(rect, 135, 135, true, paint);
            canvas.DrawText("Protein", centerX, centerY, paint);
        }
    }

    private void home_btn_Clicked(object sender, EventArgs e)
    {
        Shell.Current.GoToAsync("//HomePage", true);
    }
    private void calendar_btn_Clicked(object sender, EventArgs e)
    {
        Shell.Current.GoToAsync("//CalendarPage", true);
    }
}