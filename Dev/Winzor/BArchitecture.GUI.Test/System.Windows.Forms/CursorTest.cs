using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Bunit;
using NUnit.Framework;
using WinzorTestFramework;

namespace System.Windows.Forms;

class CursorTest
{
	[Test]
	public async Task SettingCursorCurrentToWaitCursorShowsNotRespondingOverlay()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			var buttonWait = new Button { Text = "button wait cursor" };
			var buttonDefault = new Button { Text = "button wait cursor" };

			buttonWait.Click += (sender, args) => Cursor.Current = Cursors.WaitCursor;
			buttonDefault.Click += (sender, args) => Cursor.Current = Cursors.Default;

			form.Controls.Add(buttonWait);
			form.Controls.Add(buttonDefault);
			return form;
		});

		rendered.WaitForElement(".overlay--container");
		Assert.That(rendered.FindAll(".overlay--notresponding").Count, Is.EqualTo(0));

		rendered.FindAll("button")[0].Click(new WebMouseEventArgs());
		Assert.That(() => rendered.FindAll(".overlay--notresponding").Count, Is.EqualTo(1).After(3000, 100));

		rendered.FindAll("button")[1].Click(new WebMouseEventArgs());
		Assert.That(() => rendered.FindAll(".overlay--notresponding").Count, Is.EqualTo(0).After(3000, 100));
	}

	[Test]
	public async Task SettingCursorCurrentToWaitCursorMultipleTimes()
	{
		using var ctx = new WinzorTestContext();
		Button button = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			button = new Button { Text = "buttontext" };
			return button;
		});
		rendered.WaitForElement(".overlay--container");
		Assert.That(rendered.FindAll(".overlay--notresponding").Count, Is.EqualTo(0));
		await button.InvokeWinzorDispatcherAsync(() => Cursor.Current = Cursors.WaitCursor);
		rendered.WaitForElement(".overlay--notresponding");
		Assert.That(() => rendered.FindAll(".overlay--notresponding").Count, Is.EqualTo(1).After(100));
		await button.InvokeWinzorDispatcherAsync(() => Cursor.Current = Cursors.WaitCursor);
		await Task.Delay(100);
		Assert.That(() => rendered.FindAll(".overlay--notresponding").Count, Is.EqualTo(1));
		await Task.Delay(100);
		Assert.That(() => rendered.FindAll(".overlay--notresponding").Count, Is.EqualTo(1));
		await button.InvokeWinzorDispatcherAsync(() => Cursor.Current = Cursors.Default);
		await Task.Delay(100);
		Assert.That(() => rendered.FindAll(".overlay--notresponding").Count, Is.EqualTo(0).After(100));
	}

	[Test]
	public async Task SettingCursorCurrentToWaitCursorAndNormalNotRespondingOverlay()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var button = new Button { Text = "buttontext" };
			button.Click += (sender, args) =>
			{
				Cursor.Current = Cursors.WaitCursor;
				Thread.Sleep(3000);
				Cursor.Current = Cursors.Default;
			};
			return button;
		});
		rendered.WaitForElement(".overlay--container");
		Assert.That(rendered.FindAll(".overlay--notresponding").Count, Is.EqualTo(0));
		rendered.Find("button").Click(new WebMouseEventArgs());
		Assert.That(() => rendered.FindAll(".overlay--notresponding").Count, Is.EqualTo(1).After(1000, 100));
		Assert.That(() => rendered.FindAll(".overlay--notresponding").Count, Is.EqualTo(1).After(1000));
		Assert.That(() => rendered.FindAll(".overlay--notresponding").Count, Is.EqualTo(0).After(2000));
	}

	[Test, TestCaseSource(nameof(SettingCursorCurrentToOtherCursorReportsErrorTestCaseData))]
	public async Task<bool> SettingCursorCurrentToOtherCursorReportsError(Cursor cursor)
	{
		using var ctx = new WinzorTestContext();
		Exception threadException = null;
		ctx.ThreadExceptionExceptionRaised += ex =>
		{
			threadException = ex;
			return true;
		};
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var button = new Button { Text = "buttontext" };
			button.Click += (sender, args) =>
			{
				Cursor.Current = cursor;
			};
			return button;
		});

		try
		{
			await rendered.Find("button").ClickAsync(new WebMouseEventArgs());
			return threadException != null;
		}
		finally
		{
			await ctx.WinzorDispatcher.InvokeAsync(() => Cursor.Current = null);
		}
	}

	public static IEnumerable<TestCaseData> SettingCursorCurrentToOtherCursorReportsErrorTestCaseData
	{
		get
		{
			yield return new TestCaseData(null) { ExpectedResult = false };
			yield return new TestCaseData(Cursors.Default) { ExpectedResult = false };
			yield return new TestCaseData(Cursors.Arrow) { ExpectedResult = false, TestName = "SettingCursorCurrentToOtherCursorReportsError(Arrow)" };
			yield return new TestCaseData(Cursors.AppStarting) { ExpectedResult = true };				
			yield return new TestCaseData(Cursors.Cross) { ExpectedResult = true };				
			yield return new TestCaseData(Cursors.Hand) { ExpectedResult = true };
			yield return new TestCaseData(Cursors.Help) { ExpectedResult = true };
			yield return new TestCaseData(Cursors.HSplit) { ExpectedResult = true };
			yield return new TestCaseData(Cursors.IBeam) { ExpectedResult = true };
			yield return new TestCaseData(Cursors.No) { ExpectedResult = true };
			yield return new TestCaseData(Cursors.SizeWE) { ExpectedResult = true };
			yield return new TestCaseData(Cursors.SizeNWSE) { ExpectedResult = true };
			yield return new TestCaseData(Cursors.SizeNESW) { ExpectedResult = true };
			yield return new TestCaseData(Cursors.SizeNS) { ExpectedResult = true };
			yield return new TestCaseData(Cursors.SizeAll) { ExpectedResult = true };
			yield return new TestCaseData(Cursors.VSplit) { ExpectedResult = true };
			yield return new TestCaseData(Cursors.NoMoveVert) { ExpectedResult = true };
			yield return new TestCaseData(Cursors.NoMoveHoriz) { ExpectedResult = true };
			yield return new TestCaseData(Cursors.NoMove2D) { ExpectedResult = true };
			yield return new TestCaseData(Cursors.PanSW) { ExpectedResult = true };
			yield return new TestCaseData(Cursors.PanSouth) { ExpectedResult = true };
			yield return new TestCaseData(Cursors.PanSE) { ExpectedResult = true };
			yield return new TestCaseData(Cursors.PanNW) { ExpectedResult = true };
			yield return new TestCaseData(Cursors.PanNorth) { ExpectedResult = true };
			yield return new TestCaseData(Cursors.PanNE) { ExpectedResult = true };
			yield return new TestCaseData(Cursors.PanEast) { ExpectedResult = true };
			yield return new TestCaseData(Cursors.PanWest) { ExpectedResult = true };
			yield return new TestCaseData(Cursors.UpArrow) { ExpectedResult = true };
		}
	}
}
