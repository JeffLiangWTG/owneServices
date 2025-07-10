using System.Threading.Tasks;
using Bunit;
using NUnit.Framework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace System.Windows.Forms;

internal class ToolStripSeparatorTest
{
	[Test]
	public async Task ToolStripSeparatorShouldHaveAssociatedStylingClass()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			toolStrip.Items.Add(new ToolStripSeparator());
			form.Controls.Add(toolStrip);
			return form;
		});

		var toolStripSeparator = rendered.Find(".toolstrip__separator");
		Assert.That(toolStripSeparator, Is.Not.Null);
	}

	[Test]
	public async Task ToolStripSeparatorShouldHaveSizeSet()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			toolStrip.Items.Add(new ToolStripSeparator() { AutoSize = false, Width = 10, Height = 15 });
			form.Controls.Add(toolStrip);
			return form;
		});

		var toolStripSeparator = rendered.Find(".toolstrip__separator");
		Assert.That(toolStripSeparator, Is.Not.Null);
		Assert.That(toolStripSeparator.GetAttribute("style"), Does.Contain("width:10px;"));
		Assert.That(toolStripSeparator.GetAttribute("style"), Does.Contain("height:15px;"));
	}
	[Test]
	public async Task ToolStripSeparatorShouldHaveCorrectPadding()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip();
			toolStrip.Items.Add(new ToolStripSeparator { Margin = new Padding(5), Padding = new Padding(3, 2, 1, 0) });
			form.Controls.Add(toolStrip);
			return form;
		});

		var toolStripSeparator = rendered.Find(".toolstrip__separator");
		Assert.That(toolStripSeparator.ClassList, Contains.Item("pl-3"));
		Assert.That(toolStripSeparator.ClassList, Contains.Item("pt-2"));
		Assert.That(toolStripSeparator.ClassList, Contains.Item("pr-1"));
		Assert.That(toolStripSeparator.ClassList, Contains.Item("pb-0"));
	}

	[Test, WithPlaywrightPage]
	public async Task ToolStripSeparatorDivShouldHaveCorrectStyle()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var toolStrip = new ToolStrip() { Width = 6, Height = 25 };
			toolStrip.Items.Add(new ToolStripSeparator());
			form.Controls.Add(toolStrip);
			return form;
		});

		var toolStripSeparatorDiv = page.Locator(".toolstrip__separator > div");

		Assert.That((await toolStripSeparatorDiv.GetComputedStyleAsync("height")).AsPixels, Is.EqualTo(15));

		Assert.That((await toolStripSeparatorDiv.GetComputedStyleAsync("border-left-width")).AsPixels, Is.GreaterThan(0).Within(1));
		Assert.That((await toolStripSeparatorDiv.GetComputedStyleAsync("border-right-width")).AsPixels, Is.GreaterThan(0).Within(1));

		Assert.That((await toolStripSeparatorDiv.GetComputedStyleAsync("border-left-color")), Is.EqualTo("rgb(171, 173, 177)"));
		Assert.That((await toolStripSeparatorDiv.GetComputedStyleAsync("border-right-color")), Is.EqualTo("rgb(255, 255, 255)"));

		Assert.That((await toolStripSeparatorDiv.GetComputedStyleAsync("border-left-style")), Is.EqualTo("solid"));
		Assert.That((await toolStripSeparatorDiv.GetComputedStyleAsync("border-right-style")), Is.EqualTo("solid"));
	}
}
