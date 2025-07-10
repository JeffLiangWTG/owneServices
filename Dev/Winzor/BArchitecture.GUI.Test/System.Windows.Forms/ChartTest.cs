using System.Drawing;
using System.Threading.Tasks;
using NUnit.Framework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace System.Windows.Forms.DataVisualization.Charting;
class ChartTest
{
	[Test, WithPlaywrightPage]
	public async Task ChartShouldRenderCorrectStyle()
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			var chart = new Chart();
			chart.Size = new Size(183, 57);
			var chartArea = new ChartArea();
			chartArea.Position.Width = 30;
			chartArea.Position.Height = 100;
			chart.ChartAreas.Add(chartArea);

			var legend = new Legend();
			legend.Position.Width = 70;
			legend.Position.Height = 100;
			chart.Legends.Add(legend);

			var dataPoint1 = new DataPoint(0d, 20d);
			dataPoint1.LegendText = "Test1";
			dataPoint1.Color = Color.Green;

			var dataPoint2 = new DataPoint(0d, 10d);
			dataPoint1.LegendText = "Test2";
			dataPoint1.Color = Color.Pink;

			var series = new Series();
			series.Points.Add(dataPoint1);
			series.Points.Add(dataPoint2);
			series.ChartType = SeriesChartType.Pie;
			chart.Series.Add(series);

			form.Controls.Add(chart);
			return form;
		});

		Assert.That(form, Is.Not.Null);

		var pieChartRendered = await page.WaitForSelectorAsync(".chart__cylinder");
		var pieChartTop = await page.WaitForSelectorAsync(".chart__cylinder-top");
		var box = await pieChartRendered.BoundingBoxAsync();
		Assert.That(await pieChartRendered.GetComputedStyleAsync("border-radius"), Is.EqualTo(Math.Round(box.Width / 2, 2).ToString() + "px"));
		Assert.That(await pieChartRendered.GetComputedStyleAsync("background-color"), Is.EqualTo("rgba(0, 0, 0, 0)"));
		Assert.That(await pieChartRendered.GetComputedStyleAsync("background-image"), Is.EqualTo("linear-gradient(to left, rgb(255, 192, 203) 0%, rgb(255, 192, 203) 25%, rgb(0, 0, 0) 25%)"));

		Assert.That(await pieChartTop.GetComputedStyleAsync("background-image"), Is.EqualTo("conic-gradient(rgb(255, 192, 203) 0deg, rgb(255, 192, 203) 240deg, rgb(0, 0, 0) 240deg)"));

		var pieChartLegendItem = await page.WaitForSelectorAsync(".chart__legend-item");
		Assert.That(await pieChartLegendItem.GetComputedStyleAsync("background-color"), Is.EqualTo("rgb(244, 246, 247)"));

		var pieChartLegendColor = await page.WaitForSelectorAsync(".chart__legend-color");
		Assert.That((await pieChartLegendColor.GetComputedStyleAsync("height")).AsPixels, Is.EqualTo(10));
		Assert.That((await pieChartLegendColor.GetComputedStyleAsync("width")).AsPixels, Is.EqualTo(24));

		var pieChartLegendLabel = await page.WaitForSelectorAsync(".chart__legend-label");
		Assert.That((await pieChartLegendLabel.GetComputedStyleAsync("margin-left")).AsPixels, Is.EqualTo(5));
	}

	[Test, WithPlaywrightPage]
	public async Task ChartWithFullComplete()
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			var chart = new Chart();
			chart.Size = new Size(183, 57);
			var chartArea = new ChartArea();
			chartArea.Position.Width = 30;
			chartArea.Position.Height = 100;
			chart.ChartAreas.Add(chartArea);

			var legend = new Legend();
			legend.Position.Width = 70;
			legend.Position.Height = 100;
			chart.Legends.Add(legend);

			var dataPoint1 = new DataPoint(0d, 20d);
			dataPoint1.LegendText = "Test1";
			dataPoint1.Color = Color.Green;

			var dataPoint2 = new DataPoint(0d, 0d);
			dataPoint1.LegendText = "Test2";
			dataPoint1.Color = Color.Pink;

			var series = new Series();
			series.Points.Add(dataPoint1);
			series.Points.Add(dataPoint2);
			series.ChartType = SeriesChartType.Pie;
			chart.Series.Add(series);

			form.Controls.Add(chart);
			return form;
		});

		Assert.That(form, Is.Not.Null);

		var pieChartRendered = await page.WaitForSelectorAsync(".chart__cylinder");
		var pieChartTop = await page.WaitForSelectorAsync(".chart__cylinder-top");
		var box = await pieChartRendered.BoundingBoxAsync();
		Assert.That(await pieChartRendered.GetComputedStyleAsync("background-image"), Is.EqualTo("linear-gradient(to left, rgb(255, 192, 203) 0%, rgb(255, 192, 203) 100%, rgb(0, 0, 0) 100%)"));
	}

	[Test, WithPlaywrightPage]
	public async Task ChartShouldRenderCorrectStyleWhenFallback()
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			var chart = new Chart();
			chart.Size = new Size(183, 37);
			ChartArea chartArea = new ChartArea();
			chartArea.Position.Width = 30;
			chartArea.Position.Height = 100;
			chart.ChartAreas.Add(chartArea);

			Legend legend = new Legend();
			legend.Position.Width = 70;
			legend.Position.Height = 100;
			chart.Legends.Add(legend);

			Series series = new Series();
			series.ChartType = SeriesChartType.Pie;
			chart.Series.Add(series);

			form.Controls.Add(chart);
			return form;
		});

		Assert.That(form, Is.Not.Null);

		var pieChartRendered = await page.WaitForSelectorAsync(".chart__cylinder");
		Assert.That(await pieChartRendered.GetComputedStyleAsync("background"), Is.EqualTo("rgba(0, 0, 0, 0) none repeat scroll 0% 0% / auto padding-box border-box"));
	}
}
