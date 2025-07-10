using System.ComponentModel;
using System.Drawing;
using System.Text;

namespace System.Windows.Forms.DataVisualization.Charting;

[Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Referenced in Control.razor")]
public partial class Chart : Control, ISupportInitialize, IDisposable
{
	public string ConicGradientOfPieChart
	{
		get
		{
			if (Series.Count > 0 && Series[0].Points.Count > 0)
			{
				var degStringBuilder = new StringBuilder();
				var sum = Series[0].Points.Sum(p => p.y);
				var currentAngle = 0d;
				for (var i = 0; i < Series[0].Points.Count - 1; i++)
				{
					var dataPoint = Series[0].Points[i];
					var angle = (dataPoint.y / sum) * 360;
					var color = GetRgbFromColor(dataPoint.Color);
					degStringBuilder.Append($"{color} {currentAngle}deg");
					currentAngle += angle;
					degStringBuilder.Append($" {currentAngle}deg, ");
				}
				degStringBuilder.Append($"{GetRgbFromColor(Series[0].Points[^1].Color)} {currentAngle}deg");
				return $"conic-gradient({degStringBuilder})";
			}
			return "transparent";
		}
	}
	double DiameterPercentage(double circleAngle) => circleAngle < 180 ? 0 :  50 + 50 * Math.Cos((circleAngle ) * Math.PI / 180);

	public string LinearGradientOfPieChart
	{
		get
		{
			if (Series.Count > 0 && Series[0].Points.Count > 0)
			{
				var degStringBuilder = new StringBuilder();
				var sum = Series[0].Points.Sum(p => p.y);
				var currentAngle = 0d;
				for (var i = 0; i < Series[0].Points.Count - 1; i++)
				{
					var dataPoint = Series[0].Points[i];
					var angle = (dataPoint.y / sum) * 360;
					var color = GetRgbFromColor(dataPoint.Color);
					if (currentAngle + angle < 180)
					{
						currentAngle += angle;
						degStringBuilder.Append($"{color} 0% 0% ,");
						continue;
					}
					currentAngle = 180;
					degStringBuilder.Append($"{color} {DiameterPercentage(currentAngle)}%");
					currentAngle  = currentAngle == 180 ? angle : currentAngle + angle;
					degStringBuilder.Append($" {DiameterPercentage(currentAngle)}% , ");
				}
				degStringBuilder.Append($"{GetRgbFromColor(Series[0].Points[^1].Color)} {DiameterPercentage(currentAngle)}%");
				return $"linear-gradient(to left, {degStringBuilder})";
			}
			return "transparent";
		}
	}

	public string GetRgbFromColor(Color color)
	{
		return $"rgb({color.R}, {color.G}, {color.B})";
	}

	double ExpectedChartWidth => (ChartAreas[0].Position.Width * Size.Width) / 100;

	double ExpectedChartHeight => (ChartAreas[0].Position.Height * Size.Height) / 100;

	double RotatedHeight => ExpectedChartHeight / Math.Sin(Math.PI * 50 / 180.0);

	double ChartTopHeight => RotatedHeight < ExpectedChartWidth ? RotatedHeight : ExpectedChartWidth;

	string PieChartLayoutStyleString
	{
		get
		{
			var position = ChartAreas[0].Position;
			var height = (RotatedHeight < (ExpectedChartWidth * 1.2)) ? RotatedHeight : ExpectedChartWidth * 1.2;

			return $"top: {position.Y}px; left: {position.X}px; width: {ExpectedChartWidth}px; height: {height}px; border-radius: {ExpectedChartWidth / 2}px; background: " + LinearGradientOfPieChart;
		}
	}
}
