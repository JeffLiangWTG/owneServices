using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bunit;
using CargoWise.Common;
using CargoWiseOne.ResourceStrings;
using Enterprise.ProcessManagement.GUI;
using Enterprise.ZArchitecture.GUI;
using Microsoft.Playwright;
using NUnit.Framework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;
using Res = CargoWiseOne.ResourceStrings.Res;
namespace Enterprise.Winzor.Architecture.Test;

class GroupBoxTest
{
	[Test]
	public async Task RenderedCaptionFromCaptionResourceString()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new ZForm();
			form.Size = new Size(300, 300);
			var groupBox = new ZGroupBox();
			groupBox.AutoSize = true;
			groupBox.CaptionResourceString = Res.GetData("88b9879f-d0cb-455d-ab60-721e18bd855a", "Hello world");
			groupBox.Width = 100;
			form.Controls.Add(groupBox);
			form.CaptionRenderingEnabled = true;
			return form;
		});
		Assert.That(rendered.Find("legend:contains('Hello world')"), Is.Not.Null);
	}

	[Test, WithPlaywrightPage]
	public async Task PressingTabShouldSelectTheFirstControlInsideGroupBox()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var textBox1 = new TextBox() { Width = 100, Height = 20, Top = 0, Left = 0, Text = "TextBox1" };
			var textBox2 = new TextBox() { Width = 100, Height = 20, Top = 100, Left = 20, Text = "TextBox2" };
			var textBox3 = new TextBox() { Width = 100, Height = 20, Top = 150, Left = 20, Text = "TextBox3" };
			var groupBox = new GroupBox() { Width = 200, Height = 200, Top = 50, Left = 0, Text = "GroupBox1" };
			form.Controls.Add(textBox1);
			groupBox.Controls.Add(textBox2);
			groupBox.Controls.Add(textBox3);
			form.Controls.Add(groupBox);
			return form;
		});

		var initialInput = await page.WaitForSelectorAsync("body > div > input");
		Assert.That(await initialInput.InputValueAsync(), Is.EqualTo("TextBox1"));
		await initialInput.ClickAsync();
		await initialInput.FocusAsync();
		await page.Keyboard.PressAsync("Tab");
		var expectedFocusedControl = await page.WaitForSelectorAsync("input:focus");
		Assert.That(await expectedFocusedControl.InputValueAsync(), Is.EqualTo("TextBox2"));
	}

	[Test, WithPlaywrightPage]
	public async Task PressingShiftTabShouldSelectTheLastControlInsideGroupBox()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var textBox1 = new TextBox() { Width = 100, Height = 20, Top = 50, Left = 20, Text = "TextBox1" };
			var textBox2 = new TextBox() { Width = 100, Height = 20, Top = 100, Left = 20, Text = "TextBox2" };
			var textBox3 = new TextBox() { Width = 100, Height = 20, Top = 300, Left = 0, Text = "TextBox3" };
			var groupBox = new GroupBox() { Width = 200, Height = 200, Top = 0, Left = 0, Text = "GroupBox1" };
			groupBox.Controls.Add(textBox1);
			groupBox.Controls.Add(textBox2);
			form.Controls.Add(groupBox);
			form.Controls.Add(textBox3);
			return form;
		});
		var initialInput = await page.WaitForSelectorAsync("body > div > input");
		Assert.That(await initialInput.InputValueAsync(), Is.EqualTo("TextBox3"));
		await initialInput.ClickAsync();
		await initialInput.FocusAsync();
		await page.Keyboard.PressAsync("Shift+Tab");
		var expectedFocusedControl = await page.WaitForSelectorAsync("input:focus");
		Assert.That(await expectedFocusedControl.InputValueAsync(), Is.EqualTo("TextBox2"));
	}

	[Test]
	public async Task ZGroupBoxFontNotChanged()
	{
		using var ctx = new EnterpriseTestContext();
		Task monitorFontChangeTask = null;
		using var monitorFontCancellationTokenSource = new CancellationTokenSource();
		var fontChanged = false;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new ZForm();
			form.Size = new Size(300, 300);
			var groupBox = new ZGroupBox();
			groupBox.AutoSize = true;
			groupBox.CaptionResourceString = Res.GetData("88b9879f-d0cb-455d-ab60-721e18bd855a", "Hello world");
			groupBox.Width = 100;
			form.Controls.Add(groupBox);
			form.CaptionRenderingEnabled = true;
			monitorFontChangeTask = Task.Run(() =>
			{
				var initialFont = groupBox.Font;
				while (!monitorFontCancellationTokenSource.Token.IsCancellationRequested)
				{
					if (initialFont != groupBox.Font)
					{
						fontChanged = true;
					}
				}
			});
			return form;
		});
		Assert.That(rendered.Find("legend:contains('Hello world')"), Is.Not.Null);
		await monitorFontCancellationTokenSource.CancelAsync();
		await monitorFontChangeTask;
		Assert.That(fontChanged, Is.False);
	}

	[Test, WithPlaywrightPage]
	public async Task ZGroupBoxLegendIsBold()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		Form form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			var groupBox = new ZGroupBox
			{
				Text = "123"
			};
			form.Controls.Add(groupBox);
			return form;
		});

		var buttonText = await page.WaitForSelectorAsync($".groupbox__text");

		Assert.That(async () => await buttonText.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('font-weight')"), Is.EqualTo("700"));
	}

	[Test, WithPlaywrightPage]
	public async Task TestLabelCaptionVisibleBasedOnZGroupBoxObstructingCaption()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new WinzorTestForm();
			form.Size = new Size(1200, 1200);

			var groupBox1 = new ZGroupBox() { Size = new Size(500, 146), Location = new Point(10, 10), Name = "GroupBox1" };
			var guidFindBox1 = new ZGuidFindBox() { Size = new Size(350, 46), Location = new Point(150, 60), CaptionResourceString = Res.GetData("FindBox", "GUI Guid Caption") };
			groupBox1.Controls.Add(guidFindBox1);

			var groupBox2 = new ZGroupBox() { Size = new Size(500, 146), Location = new Point(10, 160), Name = "GroupBox2" };
			var guidFindBox2 = new ZGuidFindBox() { Size = new Size(350, 46), Location = new Point(15, 60), CaptionResourceString = Res.GetData("FindBox", "GUI Guid Caption") };
			groupBox2.Controls.Add(guidFindBox2);
			form.Controls.Add(groupBox1);
			form.Controls.Add(groupBox2);
			form.CaptionRenderingEnabled = true;
			return form;
		});
		await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
		var label1 = page.Locator("div[data-name='GroupBox1'] .label");
		var label2 = page.Locator("div[data-name='GroupBox2'] .label");

		Assert.That(async() => await label1.IsVisibleAsync(), Is.True.After(3000, 100));
		Assert.That(async() => await label2.IsVisibleAsync(), Is.False.After(3000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task TestLabelCaptionShouldNotGetRenderedWhenParentNotVisible()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new WinzorTestForm();
			form.Size = new Size(1200, 1200);

			var groupBox = new ZGroupBox() { Size = new Size(500, 146), Location = new Point(10, 10), Name = "GroupBox" };
			var guidFindBox1 = new ZGuidFindBox() { Size = new Size(350, 46), Location = new Point(150, 10), CaptionResourceString = Res.GetData("FindBox", "GUI Guid Caption1") };
			groupBox.Controls.Add(guidFindBox1);
			var guidFindBox2 = new ZGuidFindBox() { Size = new Size(350, 46), Location = new Point(150, 70), CaptionResourceString = Res.GetData("FindBox", "GUI Guid Caption2") };
			groupBox.Controls.Add(guidFindBox2);

			var button = new Button() { Size = new Size(100, 20), Location = new Point(10, 130), Text = "Click" };
			//Trying to replicate the scenario when groupbox visibility set to false and then when we change the visibility of guidFindBox to false then it doesn't remove the associated labelcaption
			//from groupbox and when it is made true again then labelcaption is still visible. This test checks if that issue is resolved.
			button.Click += (s, e) =>
			{
				groupBox.Visible = false;
				guidFindBox1.Visible = false;
				groupBox.Visible = true;
			};
			groupBox.Controls.Add(button);
			form.Controls.Add(groupBox);
			form.CaptionRenderingEnabled = true;
			return form;
		});
		await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
		var label1 = page.Locator(".label").GetByText("GUI Guid Caption1");
		var label2 = page.Locator(".label").GetByText("GUI Guid Caption2");
		var button = page.GetByRole(AriaRole.Button, new() { Name = "Click" });
		Assert.That(async() => await label1.IsVisibleAsync(), Is.True.After(3000, 100));
		await button.ClickAsync();
		Assert.That(async() => await label1.IsVisibleAsync(), Is.False.After(3000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task TestGroupBoxPanelShouldHaveCorrectStyleAndIssueResolved()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new ZForm() { Width = 1000, Height = 1000, BackColor = Color.White };
			var tabControl = new ZTabControl() { Width = 1000, Height = 1000 };
			var tabPage = new TabPage() { Name = "P1", Text = "TestPage" };
			tabControl.TabPages.Add(tabPage);

			var control = new ProjectDetailsControl();
			((ZDynamicControlCreationUserControl)((SplitContainer)control.SplitContainerMain.Panel1.Controls[0]).Panel1.Controls[0].Controls[0]).UserControlType = typeof(ProjectStatusControlTest);
			tabPage.Controls.Add(control);
			form.Controls.Add(tabControl);
			return form;
		});

		var legend = page.Locator("legend");
		var currentTaskLabel = legend.GetByText("Current Task / State");
		await currentTaskLabel.WaitForAsync();
		var clientInformationLabel = legend.GetByText("Client Information");
		await clientInformationLabel.WaitForAsync();

		var currentTaskLabelPosition = await currentTaskLabel.BoundingBoxAsync();
		var clientInformationLabelPosition = await clientInformationLabel.BoundingBoxAsync();

		Assert.That(currentTaskLabelPosition.Y, Is.EqualTo(clientInformationLabelPosition.Y));
		Assert.That(currentTaskLabelPosition.Y, Is.EqualTo(24));
	}

	class ProjectStatusControlTest : ProjectStatusControl
	{
		public ProjectStatusControlTest() : base()
		{
			((SplitContainer)this.Controls[0]).SplitterDistance = 0;
		}
	}
}
