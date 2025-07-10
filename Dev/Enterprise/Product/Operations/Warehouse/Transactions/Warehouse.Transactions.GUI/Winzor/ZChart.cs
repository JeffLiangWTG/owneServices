using System.Windows.Forms.DataVisualization.Charting;
namespace Enterprise.Warehouse.Transactions.GUI.Common
{
	internal class ZChart : Chart
	{
		public ChartStyle ChartStylePersisted { get; set; }

		internal void UpdateLegendColoursAndData(ChartDataPoint[] points)
		{
			var series = Series[0];

			for (var i = 0; i < points.Length; i++)
			{
				var point = points[i];
				var chartPoint = (series.Points.Count <= i) ? series.Points.Add(point.Data) : series.Points[i];
				chartPoint.SetValueY(point.Data);
				chartPoint.Color = point.Color;
				chartPoint.LegendText = point.Legend;
			}
		}
	}
}
