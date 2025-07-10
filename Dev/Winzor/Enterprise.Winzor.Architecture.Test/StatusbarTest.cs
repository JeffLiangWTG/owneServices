using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bunit;
using CargoWise.EntityFramework.Testing;
using Microsoft.Playwright;
using NUnit.Framework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace Enterprise.Winzor.Architecture.Test;

using static InMemoryAppServerTestContextExtensions;
using static PlaywrightTestContext;

internal class StatusbarTest
{
	[Test]
	public async Task StatusBarShouldRenderedWithDefaultSize()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new StatusBar());
		Assert.That(rendered.Find(".statusbar"), Is.Not.Null);
	}

	[Test, WithPlaywrightPage]
	public async Task StatusBarBorderShouldHaveCorrectLeftPadding()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() => new StatusBar());

		var statusbar = page.Locator(".statusbar__border");
		await statusbar.WaitForAsync();
		Assert.That(async () => await statusbar.GetComputedStyleAsync("padding-left"), Is.EqualTo("3.5px"));
	}

	[Test]
	public async Task StausBarDisplaysText()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new WinzorTestForm();
			form.MainStatusBar.Panels.Add(new StatusBarPanel { Text = "STATUSBAR" });
			form.BindingSource.DataSourceType = typeof(DummyBusinessObject);
			return form;
		});
		Assert.That(rendered.Markup, Contains.Substring("STATUSBAR"));
	}

	[Test]
	public async Task StausBarDisplaysColors()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new WinzorTestForm();
			form.MainStatusBar.Panels.Add(new StatusBarPanel { Text = "STATUSBAR", BackColor = Color.Blue, ForeColor = Color.Red });
			form.BindingSource.DataSourceType = typeof(DummyBusinessObject);
			return form;
		});
		Assert.That(rendered.Markup, Contains.Substring("background-color:#0000FFFF;"));
		Assert.That(rendered.Markup, Contains.Substring("color:#FF0000FF;"));
	}

	[Test]
	public async Task StatusBarInvokedDrawItem()
	{
		using var ctx = new EnterpriseTestContext();
		var drawItemInvoked = false;
		await ctx.RenderFormAsync(() =>
		{
			var form = new WinzorTestForm();
			form.MainStatusBar.Panels.Add(new StatusBarPanel { Text = "STATUSBAR" });
			form.MainStatusBar.DrawItem +=
				delegate(object sender, StatusBarDrawItemEventArgs sbdevent)
				{
					drawItemInvoked = true;
				};
			return form;
		});
		Assert.That(drawItemInvoked, Is.True);
	}

	[TestCase(FormBorderStyle.FixedToolWindow, FormBorderStyle.Sizable, true, TestName = "{m}_ChangedToTrue")]
	[TestCase(FormBorderStyle.Sizable, FormBorderStyle.FixedToolWindow, false, TestName = "{m}_ChangedToFalse")]
	[TestCase(FormBorderStyle.SizableToolWindow, FormBorderStyle.SizableToolWindow, true, TestName = "{m}_NotChangedTrue")]
	[TestCase(FormBorderStyle.FixedToolWindow, FormBorderStyle.FixedToolWindow, false, TestName = "{m}_NotChangedFalse")]
	public async Task StatusBarSizingGripUpdatedWhenFormBorderStyle(FormBorderStyle formBorderStyleInit, FormBorderStyle formBorderStyleUpdated, bool sizingGripExpect)
	{
		WinzorTestForm form = null;
		using var ctx = new EnterpriseTestContext();
		await ctx.RenderFormAsync(() =>
		{
			form = new WinzorTestForm { FormBorderStyle = formBorderStyleInit };
			form.MainStatusBar.Panels.Add(new StatusBarPanel());
			form.FormBorderStyle = formBorderStyleUpdated;
			return form;
		});

		Assert.That(form.MainStatusBar.SizingGrip, Is.EqualTo(sizingGripExpect));
	}
}
