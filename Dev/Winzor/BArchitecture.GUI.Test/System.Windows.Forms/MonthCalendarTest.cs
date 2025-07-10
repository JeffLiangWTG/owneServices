using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using Microsoft.Playwright;
using Moq;
using NUnit.Framework;
using WinzorFramework.JSInterop;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace System.Windows.Forms;
class MonthCalendarTest
{
	[Test]
	public async Task TestOpen()
	{
		using var ctx = new WinzorTestContext();
		MonthCalendar calendar = null;

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			calendar = new MonthCalendar()
			{
				MinDate = new DateTime(2076, 7, 7),
				MaxDate = new DateTime(2078, 7, 7),
				ShowTime = false
			};
			calendar.SetDate(new DateTime(2077, 7, 7));
			return calendar;
		});

		await calendar.ShowCalendarAsync();

		var input = rendered.Find("input[type=date]");
		Assert.That(input, Is.Not.Null);
		Assert.That(input.GetAttribute("value"), Is.EqualTo("2077-07-07"));
		Assert.That(input.GetAttribute("min"), Is.EqualTo("2076-07-07"));
		Assert.That(input.GetAttribute("max"), Is.EqualTo("2078-07-07"));
	}

	[Test]
	public async Task PreloadMonthCalendarJSInterop()
	{
		using var ctx = new WinzorTestContext();
		var services = ctx.MockCargoWiseClientServices;
		var interop = new Mock<IMonthCalendarJSInterop>();
		interop.Setup(i => i.PreloadInterop());
		ctx.Services.AddScoped(_ => interop.Object);

		var rendered = await ctx.RenderControlOnFormAsync(() => new MonthCalendar()
		{
			MinDate = new DateTime(2076, 7, 7),
			MaxDate = new DateTime(2078, 7, 7)
		});

		interop.Verify(e => e.PreloadInterop(), Times.Once());
	}

	[Test]
	public async Task TestOpenShowTime()
	{
		using var ctx = new WinzorTestContext();
		MonthCalendar calendar = null;

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			calendar = new MonthCalendar()
			{
				MinDate = new DateTime(2076, 7, 7, 7, 7, 0),
				MaxDate = new DateTime(2078, 7, 7, 7, 7, 0),
				ShowTime = false
			};
			calendar.SetDate(new DateTime(2077, 7, 7, 7, 7, 0));
			return calendar;
		});

		calendar.ShowTime = true;
		await calendar.ShowCalendarAsync();

		var input = rendered.Find("input[type=datetime-local]");
		Assert.That(input, Is.Not.Null);
		Assert.That(input.GetAttribute("value"), Is.EqualTo("2077-07-07T07:07:00"));
		Assert.That(input.GetAttribute("min"), Is.EqualTo("2076-07-07T07:07"));
		Assert.That(input.GetAttribute("max"), Is.EqualTo("2078-07-07T07:07"));
	}

	[Test, WithPlaywrightPage]
	public async Task TestClose()
	{
		await using var ctx = new InMemoryTestServerContext();

		MonthCalendar calendar = null;
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			calendar = new MonthCalendar() { ShowTime = false };
			calendar.SetDate(new DateTime(2077, 7, 7));
			return calendar;
		});

		var waitForHidden = new LocatorWaitForOptions() { State = WaitForSelectorState.Hidden };

		await calendar.ShowCalendarAsync();

		var input = page.Locator("input[type=date][data-calendar-open]");
		await input.WaitForAsync(waitForHidden);

		var formElement = page.Locator(".form");
		await formElement.ClickAsync();

		input = page.Locator("input[type=date]:not([data-calendar-open])");
		await input.WaitForAsync(waitForHidden);
		Assert.That(input, Is.Not.Null);
	}

	[Test]
	public async Task PopupMonthCalendarShouldNotCalledInsideOnAfterRenderAsync()
	{
		using var ctx = new WinzorTestContext();
		MonthCalendar calendar = null;
		var interop = new Mock<IMonthCalendarJSInterop>();
		interop.Setup(i => i.ShowCalendarAsync(It.IsAny<ElementReference>(), It.IsAny<DotNetObjectReference<MonthCalendar>>()))
					   .Callback<ElementReference, DotNetObjectReference<MonthCalendar>>((element, dotnet) =>
					   {
						   var stackTrace = new StackTrace();
						   var frames = stackTrace.GetFrames();
						   var invokedByOnAfterRender = frames.Any(frame => frame.GetMethod().Name == "OnAfterRenderAsync");
						   Assert.That(invokedByOnAfterRender, Is.False, "ShowCalendarAsync was not invoked by OnAfterRenderAsync.");
						   calendar.SetDate(new DateTime(2077, 8, 8));
					   });
		ctx.Services.AddScoped(_ => interop.Object);

		var tcs = new TaskCompletionSource();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			calendar = new MonthCalendar();
			var textBox = new TextBox();
			var button = new Button() { Text = "Open Calender", Location = new Drawing.Point(0, 30) };
			form.Controls.Add(textBox);
			form.Controls.Add(button);
			form.Controls.Add(calendar);

			calendar.DateChanged += (sender, value) =>
			{
				textBox.Text = value.End.ToString();
			};
			button.Click += (sender, e) =>
			{
				calendar.ShowTime = true;
				calendar.MinDate = new DateTime(2076, 7, 7, 7, 7, 0);
				calendar.MaxDate = new DateTime(2078, 7, 7, 7, 7, 0);
				calendar.SetDate(new DateTime(2077, 7, 7));
				calendar.ShowCalendar();
				tcs.SetResult();
			};
			return form;
		});
		rendered.Find("button").Click();
		await tcs.Task;
		await calendar.InvokeStateHasChangedAsync();
		Assert.That(rendered.Find("input[type=datetime-local]").GetAttribute("value"), Is.EqualTo("2077-08-08T00:00:00"));
	}

	[Test]
	public async Task TestValueChanged()
	{
		using var ctx = new WinzorTestContext();
		MonthCalendar calendar = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			calendar = new MonthCalendar();
			return calendar;
		});

		DateTime date = default;
		calendar.DateChanged += (sender, value) => date = value.End;

		await rendered.Find("input[type=date]").InputAsync(new ChangeEventArgs() { Value = "2077-07-07" });

		var input = new DateTime(2077, 7, 7);
		Assert.That(calendar.DateTime, Is.EqualTo(input));
		Assert.That(date, Is.EqualTo(input));
	}

	[Test]
	public async Task TestMinMax()
	{
		using var ctx = new WinzorTestContext();

		MonthCalendar calendar = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			calendar = new MonthCalendar()
			{
				MinDate = new DateTime(2076, 7, 7),
				MaxDate = new DateTime(2078, 7, 7)
			};
			return calendar;
		});

		DateTime date = default;
		calendar.DateChanged += (sender, value) => date = value.End;

		var input = rendered.Find("input[type=date]");

		await input.InputAsync(new ChangeEventArgs() { Value = "1111-01-01" });
		Assert.That(calendar.DateTime, Is.EqualTo(calendar.MinDate));
		Assert.That(date, Is.EqualTo(calendar.MinDate));

		await input.InputAsync(new ChangeEventArgs() { Value = "2111-01-01" });
		Assert.That(calendar.DateTime, Is.EqualTo(calendar.MaxDate));
		Assert.That(date, Is.EqualTo(calendar.MaxDate));
	}
}
