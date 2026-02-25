using Microsoft.Maui.Graphics;

namespace BMICalculator;

public partial class MainPage : ContentPage
{
    double _bmi;
    string _currentGradientKey = "";
    BoxView _backgroundNext;
    int _heightUnitIndex;
    int _weightUnitIndex;
    int _genderIndex;
    Button[] _heightUnitButtons;
    Button[] _weightUnitButtons;
    Button[] _genderButtons;

    public MainPage()
    {
        InitializeComponent();

        _backgroundNext = new BoxView { Opacity = 0, InputTransparent = true };
        var rootGrid = (Grid)Content;
        rootGrid.Children.Insert(1, _backgroundNext);

        _genderButtons = [GenderMaleBtn, GenderFemaleBtn];
        _heightUnitButtons = [HUnitCm, HUnitM, HUnitFt, HUnitIn];
        _weightUnitButtons = [WUnitKg, WUnitLbs];

        // Default starting values 
        HeightEntry.Text = "170";
        WeightEntry.Text = "65";

        HeightEntry.TextChanged += (_, __) => UpdateAll();
        WeightEntry.TextChanged += (_, __) => UpdateAll();

        UpdateAll();
    }

    void GenderSelected(object sender, EventArgs e)
    {
        int newIndex = Array.IndexOf(_genderButtons, (Button)sender);
        if (newIndex < 0 || newIndex == _genderIndex) return;

        _genderIndex = newIndex;
        AnimateHighlight(GenderHighlight, _genderButtons, _genderIndex, GenderGrid, 8);
        UpdateAll();
    }

    void HeightUp(object sender, EventArgs e)
    {
        double v = 0;
        double.TryParse(HeightEntry.Text, out v);
        HeightEntry.Text = (v + 1).ToString();
    }

    void HeightDown(object sender, EventArgs e)
    {
        double v = 0;
        double.TryParse(HeightEntry.Text, out v);
        HeightEntry.Text = Math.Max(0, v - 1).ToString();
    }

    void WeightUp(object sender, EventArgs e)
    {
        double v = 0;
        double.TryParse(WeightEntry.Text, out v);
        WeightEntry.Text = (v + 1).ToString();
    }

    void WeightDown(object sender, EventArgs e)
    {
        double v = 0;
        double.TryParse(WeightEntry.Text, out v);
        WeightEntry.Text = Math.Max(0, v - 1).ToString();
    }

    void UpdateAll()
    {
        if (!double.TryParse(HeightEntry.Text, out var height) ||
            !double.TryParse(WeightEntry.Text, out var weight) ||
            height <= 0 || weight <= 0)
        {
            _bmi = 0;
            BmiLabel.Text = "—";
            CategoryLabel.Text = "—";
            ResultImage.Source = null;
            SetGradient("#0EA5E9", "#22C55E"); // fallback
            return;
        }

        double heightCm = ConvertHeightToCm(height);
        double weightKg = ConvertWeightToKg(weight);

        double heightM = heightCm / 100.0;
        _bmi = Math.Round(weightKg / (heightM * heightM), 1);

        BmiLabel.Text = _bmi.ToString("0.0");

        var (category, imageKey) = GetCategory(_bmi);
        CategoryLabel.Text = category;

        // gradient per category
        ApplyGradient(imageKey);

        // image per gender + category key
        var genderPrefix = _genderIndex == 1 ? "female" : "male";
        ResultImage.Source = $"{genderPrefix}{imageKey}.png";
    }

    static (string category, string imageKey) GetCategory(double bmi)
    {
        // Your 5 categories
        if (bmi < 18.5) return ("Underweight", "underweight");
        if (bmi < 25.0) return ("Normal", "normal");
        if (bmi < 30.0) return ("Overweight", "overweight");
        if (bmi < 40.0) return ("Obese", "obese");
        return ("Morbidly Obese", "morbidlyobese");
    }

    void ApplyGradient(string key)
    {
        if (key == _currentGradientKey)
            return;

        _currentGradientKey = key;

        var (top, bottom) = key switch
        {
            "underweight" => ("#3A7BD5", "#00D2FF"),
            "normal" => ("#11998E", "#38EF7D"),
            "overweight" => ("#FF8C42", "#FF5E62"),
            "obese" => ("#F46B45", "#EEA849"),
            _ => ("#CB2D3E", "#EF473A")
        };

        SetGradientOn(_backgroundNext, top, bottom);
        AnimateGradientTransition();
    }

    async void AnimateGradientTransition()
    {
        await _backgroundNext.FadeTo(1, 500, Easing.CubicInOut);

        BackgroundView.Background = _backgroundNext.Background;
        _backgroundNext.Opacity = 0;
    }

    double ConvertHeightToCm(double value) => HeightToCm(value, _heightUnitIndex);

    double ConvertWeightToKg(double value) => WeightToKg(value, _weightUnitIndex);

    static double HeightToCm(double value, int unitIndex) => unitIndex switch
    {
        0 => value,            // cm
        1 => value * 100.0,    // m → cm
        2 => value * 30.48,    // ft → cm
        3 => value * 2.54,     // in → cm
        _ => value
    };

    static double CmToHeight(double cm, int unitIndex) => unitIndex switch
    {
        0 => cm,               // cm
        1 => cm / 100.0,       // cm → m
        2 => cm / 30.48,       // cm → ft
        3 => cm / 2.54,        // cm → in
        _ => cm
    };

    static double WeightToKg(double value, int unitIndex) => unitIndex switch
    {
        0 => value,              // kg
        1 => value / 2.20462,    // lbs → kg
        _ => value
    };

    static double KgToWeight(double kg, int unitIndex) => unitIndex switch
    {
        0 => kg,                 // kg
        1 => kg * 2.20462,       // kg → lbs
        _ => kg
    };

    void HeightUnitSelected(object sender, EventArgs e)
    {
        int newIndex = Array.IndexOf(_heightUnitButtons, (Button)sender);
        if (newIndex < 0 || newIndex == _heightUnitIndex) return;

        if (double.TryParse(HeightEntry.Text, out var current) && current > 0)
        {
            double cm = HeightToCm(current, _heightUnitIndex);
            _heightUnitIndex = newIndex;
            double converted = CmToHeight(cm, _heightUnitIndex);
            HeightEntry.Text = Math.Round(converted, 2).ToString();
        }
        else
        {
            _heightUnitIndex = newIndex;
        }

        AnimateHighlight(HeightUnitHighlight, _heightUnitButtons, _heightUnitIndex, HeightUnitGrid, 4);
        UpdateAll();
    }

    void WeightUnitSelected(object sender, EventArgs e)
    {
        int newIndex = Array.IndexOf(_weightUnitButtons, (Button)sender);
        if (newIndex < 0 || newIndex == _weightUnitIndex) return;

        if (double.TryParse(WeightEntry.Text, out var current) && current > 0)
        {
            double kg = WeightToKg(current, _weightUnitIndex);
            _weightUnitIndex = newIndex;
            double converted = KgToWeight(kg, _weightUnitIndex);
            WeightEntry.Text = Math.Round(converted, 2).ToString();
        }
        else
        {
            _weightUnitIndex = newIndex;
        }

        AnimateHighlight(WeightUnitHighlight, _weightUnitButtons, _weightUnitIndex, WeightUnitGrid, 4);
        UpdateAll();
    }

    void AnimateHighlight(Border highlight, Button[] buttons, int newIndex, Grid grid, double spacing)
    {
        Microsoft.Maui.Controls.ViewExtensions.CancelAnimations(highlight);

        int cols = buttons.Length;
        double colWidth = (grid.Width - (cols - 1) * spacing) / cols;
        double targetX = newIndex * (colWidth + spacing);

        highlight.TranslateTo(targetX, 0, 250, Easing.CubicInOut);

        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].TextColor = i == newIndex
                ? Color.FromArgb("#333333")
                : Colors.White;
        }
    }

    void SetGradient(string top, string bottom)
    {
        SetGradientOn(BackgroundView, top, bottom);
    }

    static void SetGradientOn(BoxView view, string top, string bottom)
    {
        view.Background = new LinearGradientBrush(
            new GradientStopCollection
            {
                new GradientStop(Color.FromArgb(top).WithAlpha(0.95f), 0),
                new GradientStop(Color.FromArgb(bottom).WithAlpha(0.95f), 1)
            },
            new Point(0, 0),
            new Point(0, 1)
        );
    }
}