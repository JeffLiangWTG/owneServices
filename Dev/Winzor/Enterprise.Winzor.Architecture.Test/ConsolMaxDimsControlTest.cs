using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Enterprise.Freight.Forwarding.GUI;
using NUnit.Framework;
using WTG.PlaywrightTesting;
using static WTG.PlaywrightTesting.PlaywrightTestContext;

namespace Enterprise.Winzor.Architecture.Test;

class ConsolMaxDimsControlTest
{
	[Test, WithPlaywrightPage]
	public async Task ConsolMaxDimsControlXLabelOverlapCalcEdit()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var maxDimsControl = new ConsolMaxDimsControl();
			form.Controls.Add(maxDimsControl);
			return form;
		});

		var consolMaxDimsControl = page.Locator("[data-name='ConsolMaxDimsControl']");
		Assert.That(consolMaxDimsControl, Is.Not.Null);

		int ConvertZIndex(string value) => value == "auto" ? 0 : Convert.ToInt32(value);

		var heightXLabel = await Page.EvaluateAsync<string>("window.getComputedStyle(document.querySelectorAll('div')[7]).getPropertyValue('z-index')");
		var heightCalcEdit = await Page.EvaluateAsync<string>("window.getComputedStyle(document.querySelectorAll('input')[2]).getPropertyValue('z-index')");
		var widthXLabel = await Page.EvaluateAsync<string>("window.getComputedStyle(document.querySelectorAll('div')[6]).getPropertyValue('z-index')");
		var widthCalcEdit = await Page.EvaluateAsync<string>("window.getComputedStyle(document.querySelectorAll('input')[1]).getPropertyValue('z-index')");

		Assert.That(ConvertZIndex(heightXLabel), Is.LessThan(ConvertZIndex(heightCalcEdit)));
		Assert.That(ConvertZIndex(widthXLabel), Is.LessThan(ConvertZIndex(widthCalcEdit)));
	}
}
