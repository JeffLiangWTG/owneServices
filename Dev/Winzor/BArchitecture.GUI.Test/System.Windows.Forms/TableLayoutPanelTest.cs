using System.Collections;
using System.Drawing;
using System.Threading.Tasks;
using NUnit.Framework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;
using Point = System.Drawing.Point;

namespace System.Windows.Forms;

public class TableLayoutPanelTest
{
	const int DefaultMargin = 3;

	static IEnumerable TableLayoutBasicTestCaseData
	{
		get
		{
			yield return new TestCaseData(100, 100, 100, 100, SizeType.Absolute, SizeType.Absolute, SizeType.Absolute, SizeType.Absolute,
											new Point(0, 0), new Point(100, 0), new Point(0, 100), new Point(100, 100)) { TestName = "{m}-100-Same-Absolute" };
			yield return new TestCaseData(50, 100, 60, 100, SizeType.Absolute, SizeType.Absolute, SizeType.Absolute, SizeType.Absolute,
											new Point(0, 0), new Point(50, 0), new Point(0, 60), new Point(50, 60)) { TestName = "{m}-50-60-100-Diff-Absolute" };
			yield return new TestCaseData(50, 50, 50, 50, SizeType.Percent, SizeType.Percent, SizeType.Percent, SizeType.Percent,
											new Point(0, 0), new Point(100, 0), new Point(0, 100), new Point(100, 100)) { TestName = "{m}-50-Same-Percent" };
			yield return new TestCaseData(75, 25, 75, 25, SizeType.Percent, SizeType.Percent, SizeType.Percent, SizeType.Percent,
											new Point(0, 0), new Point(150, 0), new Point(0, 150), new Point(150, 150)) { TestName = "{m}-75-25-Diff-Percent" };
			yield return new TestCaseData(75, 25, 75, 25, SizeType.Absolute, SizeType.Percent, SizeType.Absolute, SizeType.Percent,
											new Point(0, 0), new Point(75, 0), new Point(0, 75), new Point(75, 75)) { TestName = "{m}-75-25-Combo-Absolute-Percent" };
			yield return new TestCaseData(75, 25, 75, 25, SizeType.Percent, SizeType.Absolute, SizeType.Percent, SizeType.Absolute,
											new Point(0, 0), new Point(175, 0), new Point(0, 175), new Point(175, 175)) { TestName = "{m}-75-25-Combo-Percent-Absolute" };
		}
	}

	[TestCaseSource(nameof(TableLayoutBasicTestCaseData))]
	public async Task DynamicRowsCols(
		int colWidth1, int colWidth2,
		int rowHeight1, int rowHeight2,
		SizeType colWidthType1, SizeType colWidthType2,
		SizeType rowHeightType1, SizeType rowHeightType2,
		Point expButton0Loc,
		Point expButton1Loc,
		Point expButton2Loc,
		Point expButton3Loc
		)
	{
		using var ctx = new WinzorTestContext();

		TableLayoutPanel panel = null;
		Button[] buttons = new Button[4];

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			panel = new TableLayoutPanel();
			panel.Location = new Point(0, 0);
			panel.Name = "TableLayoutPanel1";
			panel.Size = new Size(200, 200);
			panel.TabIndex = 0;
			panel.Padding = new Padding(0);
			panel.Margin = new Padding(0);
			panel.CellBorderStyle = TableLayoutPanelCellBorderStyle.None;
			panel.BorderStyle = BorderStyle.None;

			panel.ColumnStyles.Add(new ColumnStyle(colWidthType1, colWidth1));
			panel.ColumnStyles.Add(new ColumnStyle(colWidthType2, colWidth2));
			panel.RowStyles.Add(new RowStyle(rowHeightType1, rowHeight1));
			panel.RowStyles.Add(new RowStyle(rowHeightType2, rowHeight2));

			for (int i = 0; i < buttons.Length; i++) { buttons[i] = new Button() { Margin = new Padding(DefaultMargin) }; }

			panel.Controls.Add(buttons[0], 0, 0);
			panel.Controls.Add(buttons[1], 1, 0);
			panel.Controls.Add(buttons[2], 0, 1);
			panel.Controls.Add(buttons[3], 1, 1);

			return panel;
		});

		Assert.That(panel.Bounds.X, Is.EqualTo(0));
		Assert.That(buttons[0].Location, Is.EqualTo(PointWithMargin(expButton0Loc)));
		Assert.That(buttons[1].Location, Is.EqualTo(PointWithMargin(expButton1Loc)));
		Assert.That(buttons[2].Location, Is.EqualTo(PointWithMargin(expButton2Loc)));
		Assert.That(buttons[3].Location, Is.EqualTo(PointWithMargin(expButton3Loc)));
	}

	[Test]
	public async Task BasicSizeChange()
	{
		using var ctx = new WinzorTestContext();

		TableLayoutPanel panel = null;
		Button[] buttons = new Button[4];

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			panel = new TableLayoutPanel();
			panel.Location = new Point(0, 0);
			panel.Name = "TableLayoutPanel1";
			panel.Size = new Size(200, 200);
			panel.TabIndex = 0;
			panel.Padding = new Padding(0);
			panel.Margin = new Padding(0);
			panel.CellBorderStyle = TableLayoutPanelCellBorderStyle.None;
			panel.BorderStyle = BorderStyle.None;

			panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
			panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
			panel.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
			panel.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

			for (int i = 0; i < buttons.Length; i++) { buttons[i] = new Button() { Margin = new Padding(DefaultMargin) }; }

			panel.Controls.Add(buttons[0], 0, 0);
			panel.Controls.Add(buttons[1], 1, 0);
			panel.Controls.Add(buttons[2], 0, 1);
			panel.Controls.Add(buttons[3], 1, 1);

			return panel;
		});

		Assert.That(buttons[0].Location, Is.EqualTo(PointWithMargin(0, 0)));
		Assert.That(buttons[1].Location, Is.EqualTo(PointWithMargin(100, 0)));
		Assert.That(buttons[2].Location, Is.EqualTo(PointWithMargin(0, 100)));
		Assert.That(buttons[3].Location, Is.EqualTo(PointWithMargin(100, 100)));

		await panel.InvokeWinzorDispatcherAsync(() => { panel.Size = new Size(400,400); });

		Assert.That(buttons[0].Location, Is.EqualTo(PointWithMargin(0, 0)));
		Assert.That(buttons[1].Location, Is.EqualTo(PointWithMargin(200, 0)));
		Assert.That(buttons[2].Location, Is.EqualTo(PointWithMargin(0, 200)));
		Assert.That(buttons[3].Location, Is.EqualTo(PointWithMargin(200, 200)));
	}

	[Test]
	public async Task VisualBoardIndividualAreaShouldHasBorder()
	{
		using var ctx = new WinzorTestContext();

		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var labels = new[] { new Label(), new Label(), new Label(), new Label() };
			foreach (var label in labels)
			{
				Assert.That(label.ExtraStyleString, Is.Empty);
			}
			var panel = new TableLayoutPanel();
			panel.ColumnCount = 2;
			panel.RowCount = 2;
			panel.CellBorderStyle = TableLayoutPanelCellBorderStyle.Inset;
			panel.Controls.Add(labels[0], 0, 0);
			panel.Controls.Add(labels[1], 1, 0);
			panel.Controls.Add(labels[2], 0, 1);
			panel.Controls.Add(labels[3], 1, 1);

			Assert.That(labels.Length, Is.EqualTo(4));
			foreach (var label in labels)
			{
				Assert.That(label.ExtraStyleString, Does.Contain("outline: 0.1px solid #A0A0A0; outline-offset: 3.9px;"));
			}
		});
	}

	[Test]
	public async Task TableLayoutPanelShouldHaveCorrectBorderForBorderStyleTypeEqualsSingle()
	{
		using var ctx = new WinzorTestContext();

		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var labels = new[] { new Label(), new Label(), new Label(), new Label() };
			foreach (var label in labels)
			{
				Assert.That(label.ExtraStyleString, Is.Empty);
			}
			var panel = new TableLayoutPanel();
			panel.ColumnCount = 2;
			panel.RowCount = 2;
			panel.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
			panel.Controls.Add(labels[0], 0, 0);
			panel.Controls.Add(labels[1], 1, 0);
			panel.Controls.Add(labels[2], 0, 1);
			panel.Controls.Add(labels[3], 1, 1);

			Assert.That(labels.Length, Is.EqualTo(4));
			foreach (var label in labels)
			{
				Assert.That(label.ExtraStyleString, Does.Contain("outline: 0.1px solid #A0A0A0; outline-offset: 0.9px;"));
			}
		});
	}

	[Test, WithPlaywrightPage]
	public async Task TableLayoutPanelShouldShowScrollBarWhenBorderStyleTypeEqualsSingle()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form() { Size = new Size(300, 200) };
			var panel = new TableLayoutPanel() { AutoScroll = true, Width = 200, Height = 100 };
			var button1 = new Button()
			{
				Text = "button1",
				Height = 100,
				Width = 100,
				Top = 0,
				Left = 0,
			};
			var button2 = new Button()
			{
				Text = "button2",
				Height = 100,
				Width = 100,
				Top = 0,
				Left = 100,
			};
			panel.Controls.Add(button1);
			panel.Controls.Add(button2);
			panel.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
			form.Controls.Add(panel);
			return form;
		});

		var panelEl = await page.WaitForSelectorAsync(".panel");
		Assert.That(async () => await panelEl.EvaluateAsync<string>($"e => window.getComputedStyle(e).getPropertyValue('overflow')"), Is.EqualTo("auto"));
		Assert.That(async () => await panelEl.EvaluateAsync<bool>("element => element.scrollHeight > element.clientHeight"), Is.EqualTo(true));
	}

	Point PointWithMargin(int x, int y) => new Point(x + DefaultMargin, y + DefaultMargin);
	Point PointWithMargin(Point point) => new Point(point.X + DefaultMargin, point.Y + DefaultMargin);

	[Test, WithPlaywrightPage]
	[TestCase("#00FFFFFF")]
	[TestCase("#808080FF")]
	public async Task ParentBackColorDoesNotAffectTableLayoutPanel(Color parentBackColor)
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form() { Size = new Size(300, 200), BackColor = parentBackColor };
			var panel = new TableLayoutPanel()
			{
				AutoScroll = true,
				Width = 200,
				Height = 100,
				BackColor = Color.Blue
			};

			form.Controls.Add(panel);
			return form;
		});

		var panelEl = await page.WaitForSelectorAsync(".panel");

		Assert.That(async () => await panelEl.EvaluateAsync<string>("e => window.getComputedStyle(e).backgroundColor"), Is.EqualTo("rgb(0, 0, 255)"));
	}

	[Test]
	public async Task TestSetCellPosition()
	{
		using var ctx = new WinzorTestContext();
		var panel = default(TableLayoutPanel);
		var label = default(Label);

		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			panel = GetTableLayoutPanelWith4CellsOfAbsoluteSize100x100();
			label = new Label { Margin = new Padding(DefaultMargin) };
			panel.Controls.Add(label, 0, 0);
		});
		Assert.That(label.Location, Is.EqualTo(PointWithMargin(0, 0)));

		await panel.InvokeWinzorDispatcherAsync(() => panel.SetCellPosition(label, new TableLayoutPanelCellPosition(1, 1)));
		Assert.That(label.Location, Is.EqualTo(PointWithMargin(100, 100)));
	}

	[Test]
	public async Task TestGetPositionFromControl()
	{
		using var ctx = new WinzorTestContext();
		var panel = default(TableLayoutPanel);
		var label = default(Label);

		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			panel = GetTableLayoutPanelWith4CellsOfAbsoluteSize100x100();
			label = new Label { Margin = new Padding(DefaultMargin) };
			panel.Controls.Add(label, 1, 0);
		});

		Assert.That(panel.GetPositionFromControl(label), Is.EqualTo(new TableLayoutPanelCellPosition(1, 0)));
	}

	[TestCase(TableLayoutPanelGrowStyle.AddRows, 0, 2, TestName = "{m}AddRows")]
	[TestCase(TableLayoutPanelGrowStyle.AddColumns, 2, 0, TestName = "{m}AddColumns")]
	public async Task TestAddingAdditionalCellWhenGrowStyleIs(TableLayoutPanelGrowStyle growStyle, int col, int row)
	{
		using var ctx = new WinzorTestContext();
		var panel = default(TableLayoutPanel);
		var additionalLabel = default(Label);

		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			panel = GetTableLayoutPanelWith4CellsOfAbsoluteSize100x100();
			panel.GrowStyle = growStyle;
			panel.Controls.Add(new Label() { Text = "0,0" }, 0, 0);
			panel.Controls.Add(new Label() { Text = "1,0" }, 1, 0);
			panel.Controls.Add(new Label() { Text = "0,1" }, 0, 1);
			panel.Controls.Add(new Label() { Text = "1,1" }, 1, 1);
			panel.Controls.Add(additionalLabel = new Label());
		});

		Assert.That(panel.GetPositionFromControl(additionalLabel), Is.EqualTo(new TableLayoutPanelCellPosition(col, row)));
	}

	[Test]
	public async Task TestAddingAdditionalCellWhenGrowStyleIsFixedSizeThrowsException()
	{
		using var ctx = new WinzorTestContext();
		var panel = default(TableLayoutPanel);

		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			panel = GetTableLayoutPanelWith4CellsOfAbsoluteSize100x100();
			panel.GrowStyle = TableLayoutPanelGrowStyle.FixedSize;
			panel.Controls.Add(new Label() { Text = "0,0" }, 0, 0);
			panel.Controls.Add(new Label() { Text = "1,0" }, 1, 0);
			panel.Controls.Add(new Label() { Text = "0,1" }, 0, 1);
			panel.Controls.Add(new Label() { Text = "1,1" }, 1, 1);
		});

		var exception = default(ArgumentException);
		await panel.InvokeWinzorDispatcherAsync(() => {
			try
			{
				panel.Controls.Add(new Label());
			}
			catch (ArgumentException e)
			{
				exception = e;
			}
		});
		Assert.That(exception, Is.Not.Null);
		Assert.That(exception.Message, Is.EqualTo(SR.TableLayoutPanelFullDesc));
	}

	TableLayoutPanel GetTableLayoutPanelWith4CellsOfAbsoluteSize100x100()
	{
		var panel = new TableLayoutPanel();
		panel.Location = new Point(0, 0);
		panel.Size = new Size(200, 200);
		panel.TabIndex = 0;
		panel.Padding = new Padding(0);
		panel.Margin = new Padding(0);
		panel.CellBorderStyle = TableLayoutPanelCellBorderStyle.None;
		panel.BorderStyle = BorderStyle.None;
		panel.ColumnCount = 2;
		panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
		panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
		panel.RowCount = 2;
		panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));
		panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));
		return panel;
	}
}
