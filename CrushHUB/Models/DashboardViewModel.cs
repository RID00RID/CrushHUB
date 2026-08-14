using System.Globalization;

namespace CrushHUB.Models;

public class DashboardViewModel
{
    public int ProjectId { get; init; }

    public string ProjectName { get; init; } = string.Empty;

    public int CrashCount { get; init; }

    public int OpenReportCount { get; init; }

    public ChartViewModel Crashes { get; init; } = new();

    public ChartViewModel Reports { get; init; } = new();
}

public class ChartPoint
{
    public double X { get; init; }

    public double Y { get; init; }

    public string XText => X.ToString("0.##", CultureInfo.InvariantCulture);

    public string YText => Y.ToString("0.##", CultureInfo.InvariantCulture);
}

public class ChartGridLine
{
    public int Value { get; init; }

    public double Y { get; init; }

    public string YText => Y.ToString("0.##", CultureInfo.InvariantCulture);
}

/// <summary>
/// График за N дней. Координаты считаем здесь, чтобы разметка осталась разметкой;
/// система координат — та же, что в макете: viewBox 640×180, поле графика по вертикали 20…160.
/// </summary>
public class ChartViewModel
{
    private const double Left = 20;
    private const double Width = 600;
    private const double Bottom = 160;
    private const double Height = 140;
    private const int GridSteps = 4;

    public string Title { get; init; } = string.Empty;

    /// <summary>Цветовой тон линии: danger для крашей, warning для обращений.</summary>
    public string Tone { get; init; } = "danger";

    public IReadOnlyList<ChartPoint> Points { get; init; } = [];

    public IReadOnlyList<ChartGridLine> Grid { get; init; } = [];

    public IReadOnlyList<string> Labels { get; init; } = [];

    public IReadOnlyList<int> Values { get; init; } = [];

    public bool IsEmpty => Values.All(v => v == 0);

    public string Polyline => string.Join(' ', Points.Select(p => $"{p.XText},{p.YText}"));

    public static ChartViewModel Build(string title, string tone, IReadOnlyList<DateTime> days,
        IReadOnlyDictionary<DateTime, int> countByDay)
    {
        List<int> values = days.Select(d => countByDay.TryGetValue(d, out int count) ? count : 0).ToList();

        int step = NiceStep(values.Count == 0 ? 0 : values.Max());
        int max = step * GridSteps;
        double dayStep = days.Count > 1 ? Width / (days.Count - 1) : 0;

        return new ChartViewModel
        {
            Title = title,
            Tone = tone,
            Values = values,
            Labels = days.Select(d => d.ToString("dd.MM")).ToList(),
            Grid = Enumerable.Range(0, GridSteps + 1)
                .Select(level => new ChartGridLine
                {
                    Value = level * step,
                    Y = Bottom - level * step / (double)max * Height
                })
                .ToList(),
            Points = values
                .Select((value, index) => new ChartPoint
                {
                    X = Left + index * dayStep,
                    Y = Bottom - value / (double)max * Height
                })
                .ToList()
        };
    }

    /// <summary>Подбирает «круглый» шаг сетки (1, 2, 5, 10, 20…), чтобы подписи были читаемыми.</summary>
    private static int NiceStep(int max)
    {
        if (max <= 0)
            return 1;

        double raw = max / (double)GridSteps;
        double magnitude = Math.Pow(10, Math.Floor(Math.Log10(raw)));
        double normalized = raw / magnitude;

        double nice = normalized switch
        {
            <= 1 => 1,
            <= 2 => 2,
            <= 5 => 5,
            _ => 10
        };

        return Math.Max(1, (int)Math.Round(nice * magnitude));
    }
}
