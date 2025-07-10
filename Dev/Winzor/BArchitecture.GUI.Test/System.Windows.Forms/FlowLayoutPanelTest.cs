using System.Collections;
using System.Drawing;
using System.Threading.Tasks;
using NUnit.Framework;
using WinzorTestFramework;
using Point = System.Drawing.Point;

namespace System.Windows.Forms;

public class FlowLayoutPanelTest
{
	const int DefaultMargin = 3;

	static IEnumerable FlowLayoutBasicTestCaseData
	{
		get
		{
			yield return new TestCaseData(FlowDirection.LeftToRight, true, -1, new Point(3, 3), new Point(59, 3), new Point(115, 3), new Point(3, 19)) { TestName = "{m}-LtoR-Wrap-NoBreak" };
			yield return new TestCaseData(FlowDirection.RightToLeft, true, -1, new Point(147, 3), new Point(91, 3), new Point(35, 3), new Point(147, 19)) { TestName = "{m}-RtoL-Wrap-NoBreak" };
			yield return new TestCaseData(FlowDirection.LeftToRight, false, -1, new Point(3, 3), new Point(59, 3), new Point(115, 3), new Point(171, 3)) { TestName = "{m}-LtoR-NoWrap-NoBreak" };
			yield return new TestCaseData(FlowDirection.RightToLeft, false, -1, new Point(147, 3), new Point(91, 3), new Point(35, 3), new Point(-21, 3)) { TestName = "{m}-RtoL-NoWrap-NoBreak" };
			yield return new TestCaseData(FlowDirection.LeftToRight, true, 0, new Point(3, 3), new Point(3, 19), new Point(59, 19), new Point(115, 19)) { TestName = "{m}-LtoR-Wrap-Break-0" };
			yield return new TestCaseData(FlowDirection.RightToLeft, true, 0, new Point(147, 3), new Point(147, 19), new Point(91, 19), new Point(35, 19)) { TestName = "{m}-RtoL-Wrap-Break-0" };
			yield return new TestCaseData(FlowDirection.LeftToRight, false, 2, new Point(3, 3), new Point(59, 3), new Point(115, 3), new Point(171, 3)) { TestName = "{m}-LtoR-NoWrap-Break-2" };
			yield return new TestCaseData(FlowDirection.RightToLeft, false, 2, new Point(147, 3), new Point(91, 3), new Point(35, 3), new Point(-21, 3)) { TestName = "{m}-RtoL-NoWrap-Break-2" };
		}
	}

	[TestCaseSource(nameof(FlowLayoutBasicTestCaseData))]
	public async Task Basic(
		FlowDirection direction,
		bool wrap,
		int breakButtonIndex,
		Point expButton0Loc,
		Point expButton1Loc,
		Point expButton2Loc,
		Point expButton3Loc
		)
	{
		using var ctx = new WinzorTestContext();

		FlowLayoutPanel panel = null;
		Button[] buttons = new Button[4];

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			panel = new FlowLayoutPanel();
			panel.Location = new Point(0, 0);
			panel.Name = "FlowLayoutPanel1";
			panel.Size = new Size(200, 200);
			panel.FlowDirection = direction;
			panel.WrapContents = wrap;

			for (int i = 0; i < buttons.Length; i++)
			{
				buttons[i] = new Button() { Margin = new Padding(DefaultMargin), Width = 50, Height = 10 };
				panel.Controls.Add(buttons[i]);
			}
			if (breakButtonIndex != -1)
			{
				panel.SetFlowBreak(buttons[breakButtonIndex], true);
			}

			return panel;
		});

		Assert.That(panel.Bounds.X, Is.EqualTo(0));

		Assert.That(buttons[0].Location, Is.EqualTo(expButton0Loc));
		Assert.That(buttons[1].Location, Is.EqualTo(expButton1Loc));
		Assert.That(buttons[2].Location, Is.EqualTo(expButton2Loc));
		Assert.That(buttons[3].Location, Is.EqualTo(expButton3Loc));
	}

	static IEnumerable FlowLayoutDirectionChangeTestCaseData
	{
		get
		{
			yield return new TestCaseData(FlowDirection.LeftToRight, FlowDirection.RightToLeft,
										new Point(3, 3), new Point(59, 3),
										new Point(147, 3), new Point(91, 3))
										{ TestName = "{m}-LtoR-RtoL" };
			yield return new TestCaseData(FlowDirection.RightToLeft, FlowDirection.LeftToRight,
										new Point(147, 3), new Point(91, 3),
										new Point(3, 3), new Point(59, 3))
										{ TestName = "{m}-RtoL-LtoR" };
		}
	}

	[TestCaseSource(nameof(FlowLayoutDirectionChangeTestCaseData))]
	public async Task DirectionChange(
		FlowDirection beforeDirection,
		FlowDirection afterDirection,
		Point expBeforeButton0Loc,
		Point expBeforeButton1Loc,
		Point expAfterButton0Loc,
		Point expAfterButton1Loc
		)
	{
		using var ctx = new WinzorTestContext();
		FlowLayoutPanel panel = null;
		Button[] buttons = new Button[2];

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			panel = new FlowLayoutPanel();
			panel.Location = new Point(0, 0);
			panel.Name = "FlowLayoutPanel1";
			panel.Size = new Size(200, 200);
			panel.FlowDirection = beforeDirection;

			for (int i = 0; i < buttons.Length; i++)
			{
				buttons[i] = new Button() { Margin = new Padding(DefaultMargin), Width = 50, Height = 10 };
				panel.Controls.Add(buttons[i]);
			}
			return panel;
		});

		Assert.That(buttons[0].Location, Is.EqualTo(expBeforeButton0Loc));
		Assert.That(buttons[1].Location, Is.EqualTo(expBeforeButton1Loc));

		await panel.InvokeWinzorDispatcherAsync(() => { panel.FlowDirection = afterDirection; });

		Assert.That(buttons[0].Location, Is.EqualTo(expAfterButton0Loc));
		Assert.That(buttons[1].Location, Is.EqualTo(expAfterButton1Loc));
	}

	static IEnumerable FlowLayoutFlowBreakChangeTestCaseData
	{
		get
		{
			yield return new TestCaseData(0, new Point(3, 3), new Point(59, 3), new Point(115, 3),
											  new Point(3, 3), new Point(3, 19), new Point(59, 19))
			{ TestName = "{m}-Button-0" };
			yield return new TestCaseData(1, new Point(3, 3), new Point(59, 3), new Point(115, 3),
											 new Point(3, 3), new Point(59, 3), new Point(3, 19))
			{ TestName = "{m}-Button-1" };
		}
	}

	[TestCaseSource(nameof(FlowLayoutFlowBreakChangeTestCaseData))]
	public async Task FlowBreakChange(
		int buttonIndexToFlowBreak,
		Point expBeforeButton0Loc,
		Point expBeforeButton1Loc,
		Point expBeforeButton2Loc,
		Point expAfterButton0Loc,
		Point expAfterButton1Loc,
		Point expAfterButton2Loc
		)
	{
		using var ctx = new WinzorTestContext();
		FlowLayoutPanel panel = null;
		Button[] buttons = new Button[3];

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			panel = new FlowLayoutPanel();
			panel.Location = new Point(0, 0);
			panel.Name = "FlowLayoutPanel1";
			panel.Size = new Size(200, 200);
			panel.FlowDirection = FlowDirection.LeftToRight;

			for (int i = 0; i < buttons.Length; i++)
			{
				buttons[i] = new Button() { Margin = new Padding(DefaultMargin), Width = 50, Height = 10 };
				panel.Controls.Add(buttons[i]);
			}
			return panel;
		});

		Assert.That(buttons[0].Location, Is.EqualTo(expBeforeButton0Loc));
		Assert.That(buttons[1].Location, Is.EqualTo(expBeforeButton1Loc));
		Assert.That(buttons[2].Location, Is.EqualTo(expBeforeButton2Loc));

		await panel.InvokeWinzorDispatcherAsync(() => { panel.SetFlowBreak(buttons[buttonIndexToFlowBreak], true); });

		Assert.That(buttons[0].Location, Is.EqualTo(expAfterButton0Loc));
		Assert.That(buttons[1].Location, Is.EqualTo(expAfterButton1Loc));
		Assert.That(buttons[2].Location, Is.EqualTo(expAfterButton2Loc));
	}

	static IEnumerable FlowLayoutWrapChangeTestCaseData
	{
		get
		{
			yield return new TestCaseData(-1, new Point(3, 3), new Point(59, 3), new Point(3, 19),
											  new Point(3, 3), new Point(59, 3), new Point(115, 3))
											  { TestName = "{m}-General" };
			yield return new TestCaseData(0, new Point(3, 3), new Point(3, 19), new Point(59, 19),
											 new Point(3, 3), new Point(59, 3), new Point(115, 3))
											 { TestName = "{m}-Button-0" };
		}
	}

	[TestCaseSource(nameof(FlowLayoutWrapChangeTestCaseData))]
	public async Task WrapChange(
		int buttonIndexToFlowBreak,
		Point expBeforeButton0Loc,
		Point expBeforeButton1Loc,
		Point expBeforeButton2Loc,
		Point expAfterButton0Loc,
		Point expAfterButton1Loc,
		Point expAfterButton2Loc
		)
	{
		using var ctx = new WinzorTestContext();
		FlowLayoutPanel panel = null;
		Button[] buttons = new Button[3];

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			panel = new FlowLayoutPanel();
			panel.Location = new Point(0, 0);
			panel.Name = "FlowLayoutPanel1";
			panel.Size = new Size(150, 200);
			panel.FlowDirection = FlowDirection.LeftToRight;

			for (int i = 0; i < buttons.Length; i++)
			{
				buttons[i] = new Button() { Margin = new Padding(DefaultMargin), Width = 50, Height = 10 };
				panel.Controls.Add(buttons[i]);
			}
			if (buttonIndexToFlowBreak != -1)
			{
				panel.SetFlowBreak(buttons[buttonIndexToFlowBreak], true);
			}
			return panel;
		});

		Assert.That(buttons[0].Location, Is.EqualTo(expBeforeButton0Loc));
		Assert.That(buttons[1].Location, Is.EqualTo(expBeforeButton1Loc));
		Assert.That(buttons[2].Location, Is.EqualTo(expBeforeButton2Loc));

		await panel.InvokeWinzorDispatcherAsync(() => { panel.WrapContents = false; });

		Assert.That(buttons[0].Location, Is.EqualTo(expAfterButton0Loc));
		Assert.That(buttons[1].Location, Is.EqualTo(expAfterButton1Loc));
		Assert.That(buttons[2].Location, Is.EqualTo(expAfterButton2Loc));
	}
}
