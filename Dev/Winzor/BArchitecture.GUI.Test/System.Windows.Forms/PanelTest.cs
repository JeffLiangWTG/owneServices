using System.Data;
using System.Threading.Tasks;
using Bunit;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Playwright;
using NUnit.Framework;
using WinzorFramework.Extensions;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace System.Windows.Forms;

using static PlaywrightTestContext;

class PanelTest
{
	[Test]
	public async Task ClickEvent()
	{
		await ControlAssert.ImplementsEventAsync<Panel, EventHandler>(nameof(Panel.Click), a => new EventHandler((o, e) => a()), ".panel", e => e.Click());
	}

	[Test]
	public async Task ShouldHaveCorrectSizeWhenAutoSizeGrowAndShrink()
	{
		using var ctx = new WinzorTestContext();
		Panel panel = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			panel = new Panel { Height = 80, Width = 50, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink };
			panel.Controls.Add(new Button { Height = 50, Width = 100, Top = 0, Left = 0, Margin = Padding.Empty });
			return panel;
		});

		var panelEl = rendered.Find(".panel");
		Assert.That(panelEl.GetAttribute("style"), Does.Contain("width:100px;height:50px"));

		await panel.InvokeWinzorDispatcherAsync(() => panel.Controls.Add(new Button { Height = 50, Width = 200, Top = 50, Left = 0, Margin = Padding.Empty }));
		Assert.That(panelEl.GetAttribute("style"), Does.Contain("width:200px;height:100px"));

		await panel.InvokeWinzorDispatcherAsync(() => panel.Controls[1].Dispose());
		Assert.That(panelEl.GetAttribute("style"), Does.Contain("width:100px;height:50px"));
	}

	[Test]
	public async Task UseParentDivForLayoutShouldBeTrue()
	{
		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var control = new Panel();
			Assert.That(control.UseParentDivForLayout, Is.True);
		});
	}

	[TestCase(BorderStyle.FixedSingle, "border: 1px solid black;", TestName = "{m}_FixedSingle")]
	[TestCase(BorderStyle.Fixed3D, "border: inset 2px;", TestName = "{m}_Fixed3D")]
	public async Task HasBorderStyleString(BorderStyle style, string expectedStyleString)
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			return new Panel()
			{
				Width = 100,
				Height = 100,
				BorderStyle = style
			};
		});

		var button = rendered.Find(".panel");
		Assert.That(button.GetAttribute("style"), Does.Contain(expectedStyleString));
	}

	[Test]
	public async Task PanelDefaultValues()
	{
		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var control = new Panel();
			Assert.That(control.CanSelect, Is.False);
		});
	}

	[Test]
	public async Task PanelHasNoBackgroundImageByDefault()
	{
		using var ctx = new WinzorTestContext();
		var imageSrc = TestImage.GetImage();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			return new Panel()
			{
				Width = 100,
				Height = 100,
			};
		});

		var button = rendered.Find(".panel");

		Assert.That(button.GetAttribute("style"), Does.Not.Contain("background-image"));
	}

	[Test]
	public async Task BackgroundImageStyleStringIsApplied()
	{
		using var ctx = new WinzorTestContext();
		var imageSrc = TestImage.GetImage();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			return new Panel()
			{
				Width = 100,
				Height = 100,
				BackgroundImage = imageSrc,
			};
		});

		var button = rendered.Find(".panel");

		Assert.That(button.GetAttribute("style"), Does.Contain($"background-image: url('{imageSrc.ToBase64DataUrl()}'); background-repeat: no-repeat;"));
	}

	[Test, WithPlaywrightPage]
	public async Task MovingMouseOverDataGridInPanelDoesNotTriggerRerender()
	{
		await using var ctx = new InMemoryTestServerContext();

		DataGridWithRenderFullCount dataGrid = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var panel = new Panel();
			panel.Top = 10;
			panel.Left = 10;
			panel.Width = 200;
			panel.Height = 200;

			var table = new DataTable("data");
			var column1 = new DataColumn("one");
			column1.DataType = typeof(string);
			table.Columns.Add(column1);
			var column2 = new DataColumn("two");
			column2.DataType = typeof(string);
			table.Columns.Add(column2);
			table.Rows.Add("1.1", "1.2");
			table.Rows.Add("2.1", "2.2");

			dataGrid = new DataGridWithRenderFullCount();
			dataGrid.ReadOnly = true;
			dataGrid.Dock = DockStyle.Fill;
			dataGrid.DataSource = table;
			dataGrid.AllowNavigation = false;
			var tableStyle = new DataGridTableStyle { MappingName = "data" };
			var columnStyle1 = new DataGridTextBoxColumn { MappingName = "one", HeaderText = "One Header" };
			columnStyle1.Width = 80;
			tableStyle.GridColumnStyles.Add(columnStyle1);
			var columnStyle2 = new DataGridTextBoxColumn { MappingName = "two", HeaderText = "Two Header" };
			columnStyle2.Width = 80;
			tableStyle.GridColumnStyles.Add(columnStyle2);
			dataGrid.TableStyles.Add(tableStyle);

			panel.Controls.Add(dataGrid);
			form.Controls.Add(panel);

			return form;
		});

		var table = await page.WaitForSelectorAsync("table");
		await page.Mouse.MoveAsync(20, 20, new MouseMoveOptions());
		await Task.Delay(100);
		var expectedRenderCount = dataGrid.RenderFullCount;

		await page.Mouse.MoveAsync(100, 100, new MouseMoveOptions() { Steps = 10 });
		await Task.Delay(100);
		Assert.That(dataGrid.RenderFullCount, Is.EqualTo(expectedRenderCount), "No additional renders after moving mouse around within the panel/grid");
	}

	[TestCase(BorderStyle.None, 0)]
	[TestCase(BorderStyle.FixedSingle, 1)]
	[TestCase(BorderStyle.Fixed3D, 2)]

	public async Task TestBorderStyleAdjustForClientSize(BorderStyle bs, int borderSize)
	{
		using var ctx = new WinzorTestContext();
		Panel targetPanel = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			form.Size = new Drawing.Size(500, 500);
			targetPanel = new Panel();
			form.Controls.Add(targetPanel);
			targetPanel.Location = new Drawing.Point(10, 10);
			targetPanel.Size = new Drawing.Size(200, 50);

			targetPanel.BorderStyle = bs;

			return form;
		});

		var b = targetPanel.Bounds;
		Assert.That(b.X, Is.EqualTo(10));
		Assert.That(b.Y, Is.EqualTo(10));
		Assert.That(targetPanel.ClientSize.Width + 2 * borderSize, Is.EqualTo(b.Width));
		Assert.That(targetPanel.ClientSize.Height + 2 * borderSize, Is.EqualTo(b.Height));
		Assert.That(targetPanel.ClientAreaBounds.X, Is.EqualTo(b.X + borderSize));
		Assert.That(targetPanel.ClientAreaBounds.Y, Is.EqualTo(b.Y + borderSize));
	}

	[TestCase(BorderStyle.None, BorderStyle.FixedSingle, true, 2)]
	[TestCase(BorderStyle.None, BorderStyle.Fixed3D, true, 4)]
	[TestCase(BorderStyle.None, BorderStyle.None, false, 0)]
	public async Task BorderStyleChangeTriggersRecalculation(BorderStyle initial, BorderStyle bs, bool expectChange, int expectedDiff)
	{
		using var ctx = new WinzorTestContext();
		Panel targetPanel = null;

		bool clientSizeChanged = false;
		int clientSizeDiff = 0;

		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			form.Size = new Drawing.Size(500, 500);
			targetPanel = new Panel();
			form.Controls.Add(targetPanel);
			targetPanel.Location = new Drawing.Point(10, 10);
			targetPanel.Size = new Drawing.Size(200, 50);

			targetPanel.BorderStyle = BorderStyle.None;

			var clientSizeBefore = targetPanel.ClientSize;
			targetPanel.ClientSizeChanged += (sender, e) =>
			{
				clientSizeChanged = true;
				clientSizeDiff = clientSizeBefore.Width - targetPanel.ClientSize.Width;
			};

			return form;
		});

		clientSizeChanged = false;

		await targetPanel.InvokeWinzorDispatcherAsync(() =>
		{
			targetPanel.BorderStyle = bs;
		});

		Assert.That(clientSizeChanged, Is.EqualTo(expectChange));
		Assert.That(clientSizeDiff, Is.EqualTo(expectedDiff));
	}

	class DataGridWithRenderFullCount : DataGrid
	{
		protected override void RenderFull(RenderTreeBuilder builder)
		{
			RenderFullCount++;
			base.RenderFull(builder);
		}

		public int RenderFullCount { get; private set; }
	}

	[Test, WithPlaywrightPage]
	public async Task CursorStyleChangeWhenOnBorder()
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;
		var panelSelectors = new[]
		{
			"TopBorderPanel",
			"LeftBorderPanel",
			"RightBorderPanel",
			"BottomBorderPanel",
			"TopLeftCornerPanel",
			"TopRightCornerPanel",
			"BottomLeftCornerPanel",
			"BottomRightCornerPanel"
		};
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			foreach (var selector in panelSelectors)
			{
				var panel = new Panel();
				panel.Name = selector;
				form.Controls.Add(panel);
			}
			return form;
		});

		foreach (var selector in panelSelectors)
		{
			var fieldset = await page.WaitForSelectorAsync($"[data-name={selector}]");

			// Assert the CSS property value
			Assert.That(async () => await fieldset.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('cursor')"), Is.EqualTo("default"));
		}
	}
}

