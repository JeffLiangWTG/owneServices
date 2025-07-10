using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using NUnit.Framework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace Enterprise.Winzor.Architecture.Test;

class KSplitContainerTest
{
	[Test, WithPlaywrightPage]
	public async Task KSplitContainerBackgroundColorBehavior()
	{
		var renderCount = 0;
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new KForm();
			form.Size = new Size(800, 600);
			var splitContainer1 = new KSplitContainer();
			splitContainer1.Dock = DockStyle.None;
			splitContainer1.Size = new Size(400, 600);
			splitContainer1.Location = new Point(0, 0);
			splitContainer1.Paint += (_, _) => renderCount++;

			var splitContainer2 = new KSplitContainer();
			splitContainer2.Dock = DockStyle.None;
			splitContainer2.Size = new Size(400, 600);
			splitContainer2.Location = new Point(400, 0);

			form.Controls.Add(splitContainer1);
			form.Controls.Add(splitContainer2);

			return form;
		});

		var splitContainer1Element = page.Locator("body > div.form > div:nth-child(1)");
		var splitContainer2Element = page.Locator("body > div.form > div:nth-child(2)");
		var splitter1Element = page.Locator("body > div.form > div:nth-child(1) > .splitter--draggable--vertical");
		var splitter2Element = page.Locator("body > div.form > div:nth-child(2) > .splitter--draggable--vertical");

		// initial state.
		await splitter1Element.WaitForAsync();
		Assert.That(
			async () => await splitContainer1Element.GetComputedStyleAsync("background-color"),
			Is.EqualTo("rgb(240, 240, 240)"));
		Assert.That(
			async () => await splitContainer2Element.GetComputedStyleAsync("background-color"),
			Is.EqualTo("rgb(240, 240, 240)"));
		Assert.That(
			async () => await splitter1Element.GetComputedStyleAsync("background-color"),
			Is.EqualTo("rgba(0, 0, 0, 0)"));
		Assert.That(
			async () => await splitter2Element.GetComputedStyleAsync("background-color"),
			Is.EqualTo("rgba(0, 0, 0, 0)"));
		Assert.That(renderCount, Is.EqualTo(1));

		// hover on splitter.
		// when hovering over a splitter,
		// the current splitter's background colour should change to grey and the other splitters should remain unaffected.
		// and the background color of the split containers should not change.
		var boundingBox = await splitter1Element.BoundingBoxAsync();
		await page.Mouse.MoveAsync(boundingBox.X, boundingBox.Y);
		Assert.That(
			async () => await splitContainer1Element.GetComputedStyleAsync("background-color"),
			Is.EqualTo("rgb(240, 240, 240)").After(3000, 300));
		Assert.That(
			async () => await splitContainer2Element.GetComputedStyleAsync("background-color"),
			Is.EqualTo("rgb(240, 240, 240)"));
		Assert.That(
			async () => await splitter1Element.GetComputedStyleAsync("background-color"),
			Is.EqualTo("rgb(169, 169, 169)").After(3000, 300));
		Assert.That(
			async () => await splitter2Element.GetComputedStyleAsync("background-color"),
			Is.EqualTo("rgba(0, 0, 0, 0)"));
		Assert.That(renderCount, Is.EqualTo(1));

		// leave on splitter.
		await page.Mouse.MoveAsync(0, 0);
		Assert.That(
			async () => await splitContainer1Element.GetComputedStyleAsync("background-color"),
			Is.EqualTo("rgb(240, 240, 240)"));
		Assert.That(
			async () => await splitContainer2Element.GetComputedStyleAsync("background-color"),
			Is.EqualTo("rgb(240, 240, 240)"));
		Assert.That(
			async () => await splitter1Element.GetComputedStyleAsync("background-color"),
			Is.EqualTo("rgba(0, 0, 0, 0)"));
		Assert.That(
			async () => await splitter2Element.GetComputedStyleAsync("background-color"),
			Is.EqualTo("rgba(0, 0, 0, 0)"));
		Assert.That(renderCount, Is.EqualTo(1));
	}
}
