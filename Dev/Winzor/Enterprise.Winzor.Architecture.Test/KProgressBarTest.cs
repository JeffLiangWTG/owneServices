using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bunit;
using CargoWise.Windows.UI;
using Microsoft.Playwright;
using NUnit.Framework;
using WinzorFramework.Extensions;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace Enterprise.Winzor.Architecture.Test;

using static PlaywrightTestContext;

class KProgressBarTest
{
	[Test]
	public async Task CheckProgressBarColorStyleSetCorrectlyWithoutChangingWidthStyle()
	{
		using var ctx = new EnterpriseTestContext();
		KProgressBar kProgressBar = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			kProgressBar = new KProgressBar()
			{
				Width = 50,
				Height = 50,
				Value = 50,
			};
			return kProgressBar;
		});
		await kProgressBar.InvokeWinzorDispatcherAsync(() => kProgressBar.SetForeGroundColor(Color.Green));
		Assert.That(rendered.Find(".progressbar-filled").GetAttribute("style"), Does.Contain($"background-color:{Color.Green.GetColorStyleValue()};"));
		Assert.That(rendered.Find(".progressbar-filled").GetAttribute("style"), Does.Contain($"width:50%;"));

		await kProgressBar.InvokeWinzorDispatcherAsync(() => kProgressBar.SetForeGroundColor(Color.Red));
		Assert.That(rendered.Find(".progressbar-filled").GetAttribute("style"), Does.Contain($"background-color:{Color.Red.GetColorStyleValue()};"));
		Assert.That(rendered.Find(".progressbar-filled").GetAttribute("style"), Does.Contain($"width:50%;"));

		await kProgressBar.InvokeWinzorDispatcherAsync(() => kProgressBar.SetForeGroundColor(Color.Empty));
		Assert.That(rendered.Find(".progressbar-filled").GetAttribute("style"), Does.Not.Contain("background-color:"));
		Assert.That(rendered.Find(".progressbar-filled").GetAttribute("style"), Does.Contain($"width:50%;"));
	}

	[Test]
	public async Task CheckSetForegroundColorTriggersRender()
	{
		using var ctx = new EnterpriseTestContext();
		KProgressBar kProgressBar = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			kProgressBar = new KProgressBar()
			{
				Width = 50,
				Height = 50
			};
			return kProgressBar;
		});
		Assert.That(rendered.RenderCount, Is.EqualTo(1));

		await kProgressBar.InvokeWinzorDispatcherAsync(() => kProgressBar.SetForeGroundColor(Color.Green));
		Assert.That(rendered.RenderCount, Is.EqualTo(2));
	}

	[Explicit]
	[Test, WithPlaywrightPage(Headless = false)]
	public async Task KProgressBarDemo()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var kProgressBar = new KProgressBar()
			{
				Width = 500,
				Height = 20,
			};
			var stepSizeTextBox = new TextBox()
			{
				Text = "10",
				Width = 100,
				Height = 30,
				Left = 50,
				Top = 50
			};
			var setStepSizeButton = new Button()
			{
				Text = "Set step size",
				Width = 200,
				Height = 30,
				Left = 150,
				Top = 50
			};

			var incrementButton = new Button()
			{
				Text = "Click to increment Progress Bar",
				Width = 200,
				Height = 30,
				Left = 100,
				Top = 100,
			};

			var changeColorToBlackButton = new Button()
			{
				Text = "Click to change color of Progress Bar to black",
				Width = 150,
				Height = 30,
				Left = 250,
				Top = 200,
			};

			var changeColorToWhiteButton = new Button()
			{
				Text = "Click to change color of Progress Bar to white",
				Width = 150,
				Height = 30,
				Left = 50,
				Top = 200,
			};

			setStepSizeButton.Click += new EventHandler((object sender, EventArgs e) => kProgressBar.Step = int.Parse(stepSizeTextBox.Text));
			incrementButton.Click += new EventHandler((object sender, EventArgs e) => kProgressBar.PerformStep());
			changeColorToBlackButton.Click += new EventHandler((object sender, EventArgs e) => kProgressBar.SetForeGroundColor(Color.Black));
			changeColorToWhiteButton.Click += new EventHandler((object sender, EventArgs e) => kProgressBar.SetForeGroundColor(Color.White));

			form.Controls.Add(kProgressBar);
			form.Controls.Add(stepSizeTextBox);
			form.Controls.Add(setStepSizeButton);
			form.Controls.Add(incrementButton);
			form.Controls.Add(changeColorToBlackButton);
			form.Controls.Add(changeColorToWhiteButton);

			return form;
		});
		var pageClosed = new TaskCompletionSource<bool>();
		page.Close += (sender, args) => pageClosed.SetResult(true);
		await pageClosed.Task.WithTimeout(TimeSpan.FromMinutes(5));
	}
}
