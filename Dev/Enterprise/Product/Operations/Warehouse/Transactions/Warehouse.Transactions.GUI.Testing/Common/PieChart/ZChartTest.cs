using System.Drawing;
using System.Windows.Forms.DataVisualization.Charting;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.GUI.Common.Testing
{
	public class ZChartTest : TestCaseWithFactory
	{
		#region TestPieChartCreation

		public void TestPieChartCreation()
		{
			Setup3DPieChart();

			var series = PieChart.Series[0];
			var area = PieChart.ChartAreas[0];
			var legend = PieChart.Legends[0];

			AssertEquals("Not a Pie Chart", SeriesChartType.Pie, series.ChartType);
			AssertEquals("Chart Area needs to be Visible", true, area.Visible);
			AssertEquals("Chart Area Style needs to be 3D", true, area.Area3DStyle.Enable3D);
			AssertEquals("Chart Area Style Inclination needs to be 45", 45, area.Area3DStyle.Inclination);
			AssertEquals("Chart Area Style PointDepth needs to be 200", 200, area.Area3DStyle.PointDepth);

			AssertEquals("Point 1 Y Value", PieChartPoints[0].Data, series.Points[0].YValues[0]);
			AssertEquals("Point 2 Y Value", PieChartPoints[1].Data, series.Points[1].YValues[0]);

			AssertEquals("Legend, Point 1 Legend Text ", PieChartPoints[0].Legend, series.Points[0].LegendText);
			AssertEquals("Legend, Point 2 Legend Text ", PieChartPoints[1].Legend, series.Points[1].LegendText);

			AssertEquals("Point 1 Color", Color.Green, series.Points[0].Color);
			AssertEquals("Point 2 Color", Color.Red, series.Points[1].Color);
		}

		#endregion

		#region TestUserChartCreation

		public void TestUserChartCreation()
		{
			SetupUserDefinedChart();

			var series = UserChart.Series[0];
			var area = UserChart.ChartAreas[0];
			var legend = UserChart.Legends[0];

			AssertEquals("Not a Pie Chart", SeriesChartType.Bar, series.ChartType);
			AssertEquals("Chart Area needs to be Visible", true, area.Visible);

			AssertEquals("Point 1 Y Value", UserChartPoints[0].Data, series.Points[0].YValues[0]);
			AssertEquals("Point 2 Y Value", UserChartPoints[1].Data, series.Points[1].YValues[0]);
			AssertEquals("Point 3 Y Value", UserChartPoints[2].Data, series.Points[2].YValues[0]);

			AssertEquals("Legend, Point 1 Legend Text ", UserChartPoints[0].Legend, series.Points[0].LegendText);
			AssertEquals("Legend, Point 2 Legend Text ", UserChartPoints[1].Legend, series.Points[1].LegendText);
			AssertEquals("Legend, Point 2 Legend Text ", UserChartPoints[2].Legend, series.Points[2].LegendText);

			AssertEquals("Point 1 Color", Color.Green, series.Points[0].Color);
			AssertEquals("Point 2 Color", Color.Red, series.Points[1].Color);
			AssertEquals("Point 3 Color", Color.Blue, series.Points[2].Color);
		}
		#endregion

		#region TestChartUpdate

		public void TestChartUpdate()
		{
			Setup3DPieChart();
			var series1 = PieChart.Series[0];

			AssertEquals("Point 1 Y Value", 100.0, series1.Points[0].YValues[0]);
			AssertEquals("Point 2 Y Value", 0.0, series1.Points[1].YValues[0]);

			PieChartPoints[0].Data = 40;
			PieChartPoints[1].Data = 60;

			PieChart.UpdateLegendColoursAndData(PieChartPoints);

			AssertEquals("Point 1 Y Value", 40.0, series1.Points[0].YValues[0]);
			AssertEquals("Point 2 Y Value", 60.0, series1.Points[1].YValues[0]);
		}

		#endregion

		#region TestInitializeComponentDoesNotCauseException

		[ExpectNoExceptions()]
		public void TestInitializeComponentDoesNotCauseException()
		{
			using (var control = new TestUserControlWithPieChart())
			{
				var pieChart = control.PieChart;
				AssertChartStyle(pieChart, ChartStyle.Pie3D);
				AssertCollectionsCount(pieChart, 1);
				AssertChartHasPieDefaults(pieChart);
			}
		}

		#endregion

		#region TestChartStyle

		public void TestChartStyle()
		{
			using (var control = new TestUserControlWithPieChart())
			{
				var pieChart = control.PieChart;
				control.PieChart.ChartStyle = ChartStyle.UserDefined;
				AssertChartStyle(pieChart, ChartStyle.UserDefined);
				AssertCollectionsCount(pieChart, 1);
				AssertChartHasPieDefaults(pieChart); // should keep existing settings

				pieChart.Series.RemoveAt(0);
				pieChart.ChartAreas.RemoveAt(0);
				pieChart.Legends.RemoveAt(0);
				AssertCollectionsCount(pieChart, 0);

				control.PieChart.ChartStyle = ChartStyle.Pie3D;
				AssertChartStyle(pieChart, ChartStyle.Pie3D);
				AssertCollectionsCount(pieChart, 1);
				AssertChartHasPieDefaults(pieChart);
			}
		}

		#endregion

		#region TestChartStylePersisted

		public void TestChartStylePersisted()
		{
			using (var control = new TestUserControlWithPieChart())
			{
				var pieChart = control.PieChart;
				control.PieChart.ChartStylePersisted = ChartStyle.UserDefined;
				AssertChartStyle(pieChart, ChartStyle.UserDefined);
				AssertCollectionsCount(pieChart, 1);
				AssertChartHasPieDefaults(pieChart); // should keep existing settings

				pieChart.Series.RemoveAt(0);
				pieChart.ChartAreas.RemoveAt(0);
				pieChart.Legends.RemoveAt(0);
				AssertCollectionsCount(pieChart, 0);

				control.PieChart.ChartStylePersisted = ChartStyle.Pie3D;
				AssertChartStyle(pieChart, ChartStyle.Pie3D);
				AssertCollectionsCount(pieChart, 0); // should have no behavior
			}
		}

		#endregion

		#region Assertions

		void AssertChartStyle(ZChart chart, ChartStyle chartStyle)
		{
			AssertEquals(chartStyle, chart.ChartStyle);
			AssertEquals(chartStyle, chart.ChartStylePersisted);
		}

		void AssertCollectionsCount(ZChart pieChart, int count)
		{
			AssertEquals(count, pieChart.Series.Count);
			AssertEquals(count, pieChart.Legends.Count);
			AssertEquals(count, pieChart.ChartAreas.Count);
		}

		void AssertChartHasPieDefaults(ZChart chart)
		{
			var series = chart.Series[0];
			AssertEquals("Not a Pie Chart", SeriesChartType.Pie, series.ChartType);

			var area = chart.ChartAreas[0];
			AssertEquals("Chart Area needs to be Visible", true, area.Visible);
			AssertEquals("Chart Area Style needs to be 3D", true, area.Area3DStyle.Enable3D);
			AssertEquals("Chart Area Style Inclination needs to be 45", 45, area.Area3DStyle.Inclination);
			AssertEquals("Chart Area Style PointDepth needs to be 200", 200, area.Area3DStyle.PointDepth);
		}

		#endregion

		#region Setup3DPieChart

		ZChart PieChart;
		ChartDataPoint[] PieChartPoints;

		void Setup3DPieChart()
		{
			PieChart = new ZChart();
			PieChart.ChartStyle = ChartStyle.Pie3D;

			PieChartPoints = new[] {
				new ChartDataPoint(Color.Green, "100% Completed", 100.00),
				new ChartDataPoint(Color.Red, "0% Pending", 0),
			};

			PieChart.UpdateLegendColoursAndData(PieChartPoints);
		}

		#endregion

		#region SetupUserDefinedChart

		ZChart UserChart;
		ChartDataPoint[] UserChartPoints;

		void SetupUserDefinedChart()
		{
			UserChart = new ZChart();
			UserChart.Series.Add("CW");
			UserChart.ChartAreas.Add("CW");
			UserChart.Legends.Add("CW");

			UserChart.Series[0].ChartType = SeriesChartType.Bar;
			UserChart.ChartStyle = ChartStyle.UserDefined;

			UserChartPoints = new[] {
				new ChartDataPoint(Color.Green, "60% Completed", 60.00),
				new ChartDataPoint(Color.Red, "40% Pending", 40.00),
				new ChartDataPoint(Color.Blue, "10% Pending", 10.00),
			};

			UserChart.UpdateLegendColoursAndData(UserChartPoints);
		}

		#endregion
	}
}
