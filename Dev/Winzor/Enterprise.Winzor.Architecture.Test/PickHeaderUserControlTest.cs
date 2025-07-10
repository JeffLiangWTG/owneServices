using System.Threading.Tasks;
using System.Windows.Forms;
using Enterprise.Warehouse.Transactions.GUI;
using NUnit.Framework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace Enterprise.Winzor.Architecture.Test;
class PickHeaderUserControlTest
{
	[Test]
	public async Task NewPickHeaderUserControlDoesNotThrowException()
	{
		using var ctx = new EnterpriseTestContext();
		_ = await ctx.RenderControlOnFormAsync(() => new PickHeaderUserControl());
	}

	[Test, WithPlaywrightPage]
	public async Task PieChartShouldRenderCorrectStyle()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var pickHeaderUserControl = new PickHeaderUserControl();
			form.Controls.Add(pickHeaderUserControl);

			return form;
		});

		var pieChartContainer = await page.WaitForSelectorAsync(".chart");
		Assert.That(await pieChartContainer.GetComputedStyleAsync("flex-direction"), Is.EqualTo("row"));
		Assert.That(await pieChartContainer.GetComputedStyleAsync("background-color"), Is.EqualTo("rgb(244, 246, 247)"));
		Assert.That(await pieChartContainer.GetComputedStyleAsync("display"), Is.EqualTo("flex"));
		Assert.That(await pieChartContainer.GetComputedStyleAsync("align-items"), Is.EqualTo("center"));
		Assert.That(await pieChartContainer.GetComputedStyleAsync("justify-content"), Is.EqualTo("normal"));

		var pieChartLegend = await page.WaitForSelectorAsync(".chart__legend");
		Assert.That(await pieChartLegend.GetComputedStyleAsync("flex-direction"), Is.EqualTo("column"));
		Assert.That(await pieChartLegend.GetComputedStyleAsync("background-color"), Is.EqualTo("rgb(244, 246, 247)"));
		Assert.That(await pieChartLegend.GetComputedStyleAsync("display"), Is.EqualTo("flex"));
		Assert.That(await pieChartLegend.GetComputedStyleAsync("align-items"), Is.EqualTo("center"));
		Assert.That(await pieChartLegend.GetComputedStyleAsync("justify-content"), Is.EqualTo("center"));
	}
}
