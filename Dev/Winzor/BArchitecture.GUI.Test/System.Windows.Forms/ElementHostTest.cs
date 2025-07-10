using System.Threading.Tasks;
using System.Windows.Forms.Integration;
using Bunit;
using NUnit.Framework;
using WinzorTestFramework;

namespace System.Windows.Forms;

public class ElementHostTest
{
	[Test]
	public async Task PositionAndSizeAreAsExpected()
	{
		using var ctx = new WinzorTestContext();

		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var panel = new Panel { Width = 200, Height = 300, Left = 50, Top = 100 };
			form.Controls.Add(panel);
			var elementHost = new ElementHost();
			elementHost.Dock = DockStyle.Fill;
			panel.Controls.Add(elementHost);
			return form;
		});

		Assert.That(rendered.Find("div.form > div > div").GetAttribute("style"), Contains.Substring("position:absolute;width:200px;height:300px;top:0px;left:0px;"));
	}

	[Test]
	public async Task PositionAndSizeAreAsExpectedAfterParentHeightChange()
	{
		using var ctx = new WinzorTestContext();

		Panel panel = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			panel = new Panel { Width = 200, Height = 300, Left = 50, Top = 100 };
			form.Controls.Add(panel);
			var elementHost = new ElementHost();
			elementHost.Dock = DockStyle.Fill;
			panel.Controls.Add(elementHost);
			return form;
		});

		panel.Parent.Invoke(() => panel.Height = 600);
		Assert.That(rendered.Find("div.form > div > div").GetAttribute("style"), Contains.Substring("position:absolute;width:200px;height:600px;top:0px;left:0px;"));
	}

	[Test]
	public async Task WPFSupportedControlIsRendered()
	{
		using var ctx = new WinzorTestContext();

		Panel panel = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			panel = new Panel { Width = 200, Height = 300, Left = 50, Top = 100 };
			form.Controls.Add(panel);
			var elementHost = new ElementHost();
			elementHost.Dock = DockStyle.Fill;
			panel.Controls.Add(elementHost);
			return form;
		});

		Assert.That(() => rendered.Find(".elementhost__inprogressmessage"), Throws.TypeOf(typeof(ElementNotFoundException)));
		Assert.That(rendered.Find("div.form > div > div").GetAttribute("style"), Contains.Substring("position:absolute;width:200px;height:300px;top:0px;left:0px;"));
	}

	[Test]
	public async Task MultipleWPFSupportedControlsAreRendered()
	{
		using var ctx = new WinzorTestContext();

		Panel panel = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			panel = new Panel { Width = 200, Height = 300, Left = 50, Top = 100 };
			form.Controls.Add(panel);
			var elementHost = new ElementHost();
			elementHost.Dock = DockStyle.Top;
			elementHost.Size = new Drawing.Size(200, 150);
			panel.Controls.Add(elementHost);
			var elementHost2 = new ElementHost();
			elementHost2.Dock = DockStyle.Bottom;
			elementHost2.Size = new Drawing.Size(200, 150);
			panel.Controls.Add(elementHost2);
			return form;
		});

		Assert.That(() => rendered.Find(".elementhost__inprogressmessage"), Throws.TypeOf(typeof(ElementNotFoundException)));
		Assert.That(rendered.Find("div.form > div > div:nth-child(1)").GetAttribute("style"), Contains.Substring("position:absolute;width:200px;height:150px;top:0px;left:0px;"));
		Assert.That(rendered.Find("div.form > div > div:nth-child(2)").GetAttribute("style"), Contains.Substring("position:absolute;width:200px;height:150px;top:150px;left:0px;"));
	}

	[Test]
	public async Task WPFUnsupportedControlIsNotRendered()
	{
		using var ctx = new WinzorTestContext();

		Panel panel = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			panel = new Panel { Width = 200, Height = 300, Left = 50, Top = 100 };
			form.Controls.Add(panel);
			var elementHost = new Integration.ElementHost();
			elementHost.Dock = DockStyle.Fill;
			panel.Controls.Add(elementHost);
			return form;
		});

		Assert.That(() => rendered.Find(".elementhost__inprogressmessage"), Throws.Nothing);
		Assert.That(rendered.Find("div.form > div > div").GetAttribute("style"), Contains.Substring("position:absolute;width:200px;height:300px;top:0px;left:0px;"));
	}

	[Test]
	public async Task MultipleWPFUnSupportedControlsAreNotRendered()
	{
		using var ctx = new WinzorTestContext();

		Panel panel = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			panel = new Panel { Width = 200, Height = 300, Left = 50, Top = 100 };
			form.Controls.Add(panel);
			var elementHost = new Integration.ElementHost();
			elementHost.Dock = DockStyle.Top;
			elementHost.Size = new Drawing.Size(200, 150);
			panel.Controls.Add(elementHost);
			var elementHost2 = new Integration.ElementHost();
			elementHost2.Dock = DockStyle.Bottom;
			elementHost2.Size = new Drawing.Size(200, 150);
			panel.Controls.Add(elementHost2);
			return form;
		});

		Assert.That(rendered.Find("div.form > div > div:nth-child(1)").GetAttribute("style"), Contains.Substring("position:absolute;width:200px;height:150px;top:0px;left:0px;"));
		Assert.That(() => rendered.Find("div.form > div > div:nth-child(1) > div.elementhost__inprogressmessage"), Throws.Nothing);
		Assert.That(rendered.Find("div.form > div > div:nth-child(2)").GetAttribute("style"), Contains.Substring("position:absolute;width:200px;height:150px;top:150px;left:0px;"));
		Assert.That(() => rendered.Find("div.form > div > div:nth-child(2) > div.elementhost__inprogressmessage"), Throws.Nothing);
	}

	[Test]
	public async Task ElementHostChildAddedToWinzorSpecificCollection()
	{
		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var elementHost = new ElementHost();
			elementHost.Child = new UserControl();
			Assert.That(elementHost.Child, Is.Not.Null);
			Assert.That(elementHost.Children.Count, Is.EqualTo(0));
			Assert.That(elementHost.WinzorSpecificControls.Count, Is.EqualTo(1));
		});
	}
}

class ElementHost : Integration.ElementHost
{
	public ElementHost()
	{
		Child = new WPFContent();
	}

	public class WPFContent : UserControl, IWinzorSupportedWPFContent
	{
	}
}
