using Microsoft.Maui.Graphics;

namespace BMICalculator;

public class BmiGraphDrawable : IDrawable
{
    private readonly Func<double> _getBmi;

    public BmiGraphDrawable(Func<double> getBmi)
    {
        _getBmi = getBmi;
    }

    public void Draw(ICanvas canvas, RectF rect)
    {
        float w = rect.Width;
        float h = rect.Height;

        float baseY = h * 0.75f;

        // Smooth curve
        var path = new PathF();
        path.MoveTo(0, baseY);
        path.QuadTo(w * 0.5f, baseY - 60, w, baseY - 15);

        canvas.StrokeSize = 4;
        canvas.StrokeColor = Colors.White.WithAlpha(0.7f);
        canvas.DrawPath(path);

        double bmi = _getBmi();
        if (bmi <= 0) return;

        float t = (float)Math.Clamp((bmi - 15) / 30, 0, 1);
        float x = t * w;
        float y = baseY - (t * 60);

        // Indicator
        canvas.FillColor = Colors.White;
        canvas.FillColor = Colors.White;
        canvas.FillCircle(x, y, 16);

        canvas.StrokeColor = Colors.Black.WithAlpha(0.2f);
        canvas.StrokeSize = 3;
        canvas.DrawCircle(x, y, 16);
    }
}