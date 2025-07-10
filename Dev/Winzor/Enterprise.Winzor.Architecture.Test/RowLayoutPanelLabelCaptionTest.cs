using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Layout;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Microsoft.Playwright;
using NUnit.Framework;
using WTG.PlaywrightTesting;
using Res = CargoWiseOne.ResourceStrings.Res;

namespace Enterprise.Winzor.Architecture.Test;

using static PlaywrightTestContext;

class RowLayoutPanelLabelCaptionTest
{
	async Task<IPage> CreatePanelAndLabel(InMemoryAppServerTestContext ctx,
		int textBoxLeft, int panelLeft, string originalCaptionText, bool hasObstacle = false)
	{
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new WinzorTestForm();
			var textBox1 = new ZTextBox()
			{
				Width = 100, Height = 20,
				Top = 50, Left = textBoxLeft,
				Text = "TextBox1"
			};
			var rowLayoutPanel = new RowLayoutPanel()
			{
				Width = 300, Height = 200,
				Top = 10, Left = panelLeft
			};
			rowLayoutPanel.Controls.Add(textBox1);
			form.Controls.Add(rowLayoutPanel);
			textBox1.CaptionResourceString = Res.GetData("88b9879f-d0cb-455d-ab60-721e18bd855a",
				originalCaptionText);
			form.CaptionRenderingEnabled = true;

			if (hasObstacle)
			{
				ZTextBox obstacleTb = new ZTextBox { Width = 80, Height = 20,
					Text = "The Obstacle", Top = 5, Left = 15 };
				form.Controls.Add(obstacleTb);
			}

			return form;
		});
		return page;
	}

	async Task<IPage> CreatePanelAndMultipleLabels(InMemoryAppServerTestContext ctx, int controlLeft, int panelLeft, int rowHeight)
	{
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new WinzorTestForm();
			var rowLayoutPanel = new RowLayoutPanel();
			var textBox1 = new ZTextBox();
			var dropEdit1 = new ZDropEdit();
			var dateEdit1 = new ZDateEdit();

			rowLayoutPanel.Controls.Add(textBox1);
			rowLayoutPanel.Controls.Add(dropEdit1);
			rowLayoutPanel.Controls.Add(dateEdit1);
			rowLayoutPanel.Location = ControlDpiScalingHelper.NewScaledPoint(panelLeft, 0, true);
			rowLayoutPanel.Size = ControlDpiScalingHelper.NewScaledSize(300, 200, true);
			rowLayoutPanel.RowHeight = ControlDpiScalingHelper.ScaleToCurrentDpiY(rowHeight);

			textBox1.Location = ControlDpiScalingHelper.NewScaledPoint(controlLeft, 0, true);
			textBox1.Size = ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			rowLayoutPanel.SetRow(textBox1, 1);
			textBox1.CaptionResourceString = Res.GetData("7b318dd6-c238-4140-b19f-2a707e20d18e",
				"Second Row");

			dropEdit1.Location = ControlDpiScalingHelper.NewScaledPoint(controlLeft, 0, true);
			dropEdit1.Size = ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			rowLayoutPanel.SetRow(dropEdit1, 0);
			dropEdit1.CaptionResourceString = Res.GetData("56586169-d90f-4606-b3a1-b486a08dabd8",
				"First Row");

			dateEdit1.Location = ControlDpiScalingHelper.NewScaledPoint(controlLeft, 0, true);
			dateEdit1.Size = ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			rowLayoutPanel.SetRow(dateEdit1, 2);
			dateEdit1.CaptionResourceString = Res.GetData("b0ec2b43-36b2-4cc2-9125-10e489fa2b6d",
				"Third Row");

			form.Controls.Add(rowLayoutPanel);
			form.CaptionRenderingEnabled = true;

			return form;
		});
		return page;
	}

	async Task<IPage> CreatePanelAndMultipleLabelsWithLabelControl(InMemoryAppServerTestContext ctx, int controlLeft, int panelLeft, int rowHeight)
	{
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new WinzorTestForm();
			var rowLayoutPanel = new RowLayoutPanel();
			var textBox1 = new ZTextBox();
			var dropEdit1 = new ZDropEdit();
			var label = new ZLabel();

			rowLayoutPanel.Controls.Add(textBox1);
			rowLayoutPanel.Controls.Add(dropEdit1);
			rowLayoutPanel.Controls.Add(label);
			rowLayoutPanel.Location = ControlDpiScalingHelper.NewScaledPoint(panelLeft, 0, true);
			rowLayoutPanel.Size = ControlDpiScalingHelper.NewScaledSize(300, 200, true);
			rowLayoutPanel.RowHeight = ControlDpiScalingHelper.ScaleToCurrentDpiY(rowHeight);

			dropEdit1.Location = ControlDpiScalingHelper.NewScaledPoint(controlLeft, 0, true);
			dropEdit1.Size = ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			rowLayoutPanel.SetRow(dropEdit1, 0);
			dropEdit1.CaptionResourceString = Res.GetData("56586169-d90f-4606-b3a1-b486a08dabd8",
				"Drop Edit");

			textBox1.Location = ControlDpiScalingHelper.NewScaledPoint(controlLeft, 0, true);
			textBox1.Size = ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			rowLayoutPanel.SetRow(textBox1, 1);
			textBox1.CaptionResourceString = Res.GetData("7b318dd6-c238-4140-b19f-2a707e20d18e",
				"Text Box");

			label.Location = ControlDpiScalingHelper.NewScaledPoint(controlLeft + 60, 0, true);
			label.Size = ControlDpiScalingHelper.NewScaledSize(20, 20, true);
			rowLayoutPanel.SetRow(label, 1);
			label.Text = "M";
			form.Controls.Add(rowLayoutPanel);
			form.CaptionRenderingEnabled = true;

			return form;
		});
		return page;
	}

	[Test, WithPlaywrightPage]
	[TestCase(100, 0, 43, 56, TestName = "TextBoxCaptions-OnParent-WriteOk")]
	[TestCase(50, 0, 3, 46, "First L...", TestName = "TextBoxCaptions-OnParent-Truncate")]
	[TestCase(50, 60, 3, 56, TestName = "TextBoxCaptions-OnClient-WriteOk")]
	[TestCase(50, 50, 3, 46, "First L...", TestName = "TextBoxCaptions-OnClient-Truncate")]
	public async Task RowLayoutPanelWithLabelsAndCaptions(int panelLeft, int textBoxLeft,
			int expectedCaptionLeft, int expectedCaptionWidth, string expectedCaption = null)
	{
		var originalCaptionText = expectedCaption ??= "First Label";
		await using var ctx = new InMemoryAppServerTestContext();

		var page = await CreatePanelAndLabel(ctx, textBoxLeft, panelLeft, originalCaptionText);

		var label = await page.WaitForSelectorAsync("div.label");
		var labelStyle = await label.GetAttributeAsync("style");
		var labelInnerText = await label.InnerTextAsync();

		Assert.That(labelStyle, Does.Contain($"left:{expectedCaptionLeft}px"));
		Assert.That(labelStyle, Does.Contain($"width:{expectedCaptionWidth}px"));
		Assert.That(labelInnerText, Is.EqualTo(originalCaptionText));
	}

	// For some reason, the CW system does not display Label Captions in certain
	// circumstances.  Like when there is not enough room to fit them in.
	// This will test those circumstances work as in CW orginal.
	[Test, WithPlaywrightPage]
	[TestCase(5, 0, TestName = "TextBoxCaptions-Disappear-NoRoom")]
	[TestCase(100, 0, true, TestName = "TextBoxCaptions-Disappear-Obstacle")]
	[TestCase(50, 20, TestName = "TextBoxCaptions-Disappear-ParentChildGap-TooBig")]
	public async Task RowLayoutPanelWithLabelsAndVanishedCaptions(int panelLeft, int textBoxLeft, bool obstacle = false)
	{
		var originalCaptionText = "First Label";
		await using var ctx = new InMemoryAppServerTestContext();

		var page = await CreatePanelAndLabel(ctx, textBoxLeft, panelLeft, originalCaptionText, obstacle);

		Assert.That(async () => await page.IsHiddenAsync("div.label"), Is.True.After(1000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task RowLayoutPanelLabelShouldBeVisibleOnScroll()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new WinzorTestForm();

			form.Size = new Size(350, 250);

			var textBoxArr = new ZTextBox[20];

			for (var i = 0; i < 20; i++)
			{
				var text = "TextBox: " + i.ToString();
				textBoxArr[i] = new ZTextBox()
				{
					Width = 100,
					Height = 20,
					Top = 10 + i * 30,
					Left = 100,
					Text = text
				};

				textBoxArr[i].CaptionResourceString = Res.GetData("88b9879f-d0cb-455d-ab60-721e18bd855a", text);
			}

			var rowLayoutPanel = new RowLayoutPanel()
			{
				Width = 300,
				Height = 200,
				Top = 10,
				AutoScroll = true
			};

			for (var i = 0; i < 20; i++)
			{
				rowLayoutPanel.Controls.Add(textBoxArr[i]);
			}

			form.Controls.Add(rowLayoutPanel);

			form.CaptionRenderingEnabled = true;
			return form;
		});

		await MouseMoveWithDelayAsync(page, 290, 50, 100);
		await MouseDownWithDelayAsync(page, 100);
		await MouseMoveWithDelayAsync(page, 290, 150, 100);
		await MouseUpWithDelayAsync(page, 100);

		IReadOnlyList<IElementHandle> labels = null;
		Assert.That(async () => labels = await page.QuerySelectorAllAsync("div.label"), Has.Count.EqualTo(20).After(1000, 100));

		var lastLabel = labels.Last();

		Assert.That(async () => await lastLabel.IsVisibleAsync(), Is.True);
	}

	[Test, WithPlaywrightPage]
	[TestCase(100, 0, 21, TestName = "TextBoxCaptions-MultipleRows-OnClient")]
	[TestCase(0, 100, 21, TestName = "TextBoxCaptions-MultipleRows-OnParent")]
	public async Task RowLayoutWithMultipleLabelsAndCaptions(int controlLeft, int panelLeft, int rowHeight)
	{
		await using var ctx = new InMemoryAppServerTestContext();

		var page = await CreatePanelAndMultipleLabels(ctx, controlLeft, panelLeft, rowHeight);

		var offset = 3; // floor (20 - 13) / 2

		var firstLabel = await page.WaitForSelectorAsync("div.label >> nth=1");
		var firstLabelStyle = await firstLabel.GetAttributeAsync("style");

		Assert.That(firstLabelStyle, Does.Contain($"top:{offset}px"));

		var secondLabel = await page.WaitForSelectorAsync("div.label >> nth=0");
		var secondLabelStyle = await secondLabel.GetAttributeAsync("style");

		Assert.That(secondLabelStyle, Does.Contain($"top:{rowHeight + offset}px"));

		var thirdLabel = await page.WaitForSelectorAsync("div.label >> nth=2");
		var thirdLabelStyle = await thirdLabel.GetAttributeAsync("style");

		Assert.That(thirdLabelStyle, Does.Contain($"top:{2 * rowHeight + offset}px"));
	}

	[Test, WithPlaywrightPage]
	[TestCase(100, 0, 23)]
	public async Task RowLayoutWithLableControlAndCaptions(int controlLeft, int panelLeft, int rowHeight)
	{
		await using var ctx = new InMemoryAppServerTestContext();

		var page = await CreatePanelAndMultipleLabelsWithLabelControl(ctx, controlLeft, panelLeft, rowHeight);

		var offset = 3; // floor (20 - 13) / 2

		var firstLabel = await page.WaitForSelectorAsync("div.label >> nth=1");
		var firstLabelStyle = await firstLabel.GetAttributeAsync("style");

		Assert.That(firstLabelStyle, Does.Contain($"top:{rowHeight + offset}px"));

		var secondLabel = await page.WaitForSelectorAsync("div.label >> nth=2");
		var secondLabelStyle = await secondLabel.GetAttributeAsync("style");

		Assert.That(secondLabelStyle, Does.Contain($"top:{offset}px"));

		var thirdLabel = await page.WaitForSelectorAsync("div.label >> nth=0");
		var thirdLabelStyle = await thirdLabel.GetAttributeAsync("style");

		Assert.That(thirdLabelStyle, Does.Contain($"top:{rowHeight}px"));
	}

	async Task MouseDownWithDelayAsync(IPage page, int delay)
	{
		await page.Mouse.DownAsync();
		await Task.Delay(delay);
	}

	async Task MouseMoveWithDelayAsync(IPage page, float x, float y, int delay)
	{
		await page.Mouse.MoveAsync(x, y, new () { Steps = 10 });
		await Task.Delay(delay);
	}

	async Task MouseUpWithDelayAsync(IPage page, int delay)
	{
		await page.Mouse.UpAsync();
		await Task.Delay(delay);
	}
}
