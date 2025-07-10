using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bunit;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Balloons;
using Microsoft.Playwright;
using NUnit.Framework;
using WTG.PlaywrightTesting;

namespace Enterprise.Winzor.Architecture.Test;

using static PlaywrightTestContext;
internal class ZDateEditTest
{
	[Test, WithPlaywrightPage]
	public async Task MonthCalendarValueServerClientSyncByChangeInput()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() => new ZDateEdit());

		var calendarButton = page.GetByRole(AriaRole.Button);
		var dateInput = page.GetByRole(AriaRole.Textbox);
		await Assertions.Expect(dateInput).ToHaveValueAsync(string.Empty);

		await dateInput.FillAsync("01-MAY-22");
		await Assertions.Expect(dateInput).ToHaveValueAsync("01-MAY-22");

		await ClickWithDelayAsync(calendarButton, 500);
		await Assertions.Expect(dateInput).ToHaveValueAsync("01-MAY-22");
		//the popup calendar is not in dom
		//the fillAsync not gurantee close the calendar
		//we use this mouse click to ensure the calendar is closed.
		await Page.Mouse.ClickAsync(0, 0);

		await dateInput.FillAsync("19-JUN-22");
		await Page.Mouse.ClickAsync(0, 0);
		await ClickWithDelayAsync(calendarButton, 500);
		await Assertions.Expect(dateInput).ToHaveValueAsync("19-JUN-22");
	}

	[Test, WithPlaywrightPage]
	public async Task MonthCalendarValueClientServerSyncByKeyPress()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadControlOnFormAsync(() => new ZDateEdit());

		var calendarButton = page.GetByRole(AriaRole.Button);
		var dateInput = page.GetByRole(AriaRole.Textbox);
		await Assertions.Expect(dateInput).ToHaveValueAsync(string.Empty);

		await dateInput.PressSequentiallyAsync("19-JUN-22");
		await ClickWithDelayAsync(calendarButton, 500);
		await Assertions.Expect(dateInput).ToHaveValueAsync("19-JUN-22");

		//I use keypress here cause playwright can't click on things out of window
		//this popped up calendar is not part of current window that's why it acts like portal
		await page.Keyboard.PressAsync("ArrowRight");
		await calendarButton.ClickAsync();

		await Assertions.Expect(dateInput).ToHaveValueAsync("20-JUN-22");
	}

	[Test, WithPlaywrightPage]
	public async Task MonthCalendarShowTime([Values] bool showTime)
	{
		await using var ctx = new InMemoryAppServerTestContext();

		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var dateEdit = new ZDateEdit();
			dateEdit.DateTimeFormat = showTime ? ZDateTimePickerFormat.Long : ZDateTimePickerFormat.Short;
			return dateEdit;
		});

		var typeDateTime = showTime ? "24-JUN-22T12:34" : "24-JUN-22";
		var expectedDateTimeInputValue = showTime ? "2022-06-24T12:34" : "2022-06-24";
		var expectedDateTimeCalendarInputValue = showTime ? "24-JUN-22T12:34" : "24-JUN-22";
		var calendarButton = page.GetByRole(AriaRole.Button);
		var dateInput = page.GetByRole(AriaRole.Textbox);

		await Assertions.Expect(dateInput).ToHaveValueAsync(string.Empty);

		await dateInput.PressSequentiallyAsync(typeDateTime);
		await ClickWithDelayAsync(calendarButton, 500);

		await Assertions.Expect(dateInput).ToHaveValueAsync(expectedDateTimeCalendarInputValue);
	}

	[Test, WithPlaywrightPage]
	public async Task BalloonMessageShouldBeFlippedWhenCalendarOpened()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new WinzorTestForm();
			var factory = new BusinessObjectFactory();
			form.SetDataBinding(factory.New<DummyBusinessObject>(), "");

			var dateEdit = new ZDateEdit { Location = new Point(20, 100), BindTo = DummyBusinessObject.Schema.Z0_AnotherDate };
			form.Controls.Add(dateEdit);

			Balloon.Instance.IsShownDuringTesting = true;

			return form;
		});

		var textbox = page.GetByRole(AriaRole.Textbox);
		await textbox.WaitForAsync();
		await textbox.FillAsync("1");
		await textbox.ClickAsync();
		await textbox.BlurAsync();

		var iconElement = page.Locator(".notification");
		await iconElement.WaitForAsync();
		var iconBoundingBox = await iconElement.BoundingBoxAsync();
		await iconElement.HoverAsync();

		var popupElement = page.Locator("div.popup");
		await popupElement.WaitForAsync();
		Assert.That(async () => (await popupElement.BoundingBoxAsync()).Y, Is.GreaterThan(iconBoundingBox.Y).After(1000, 100));

		await iconElement.ClickAsync();
		await page.Mouse.MoveAsync(0, 0);
		var calenderInput = page.Locator("[data-calendar-open]");
		await Assertions.Expect(calenderInput).ToBeAttachedAsync();
		await Assertions.Expect(popupElement).Not.ToBeAttachedAsync();

		await iconElement.HoverAsync();
		await popupElement.WaitForAsync();
		Assert.That(async () => (await popupElement.BoundingBoxAsync()).Y, Is.LessThan(iconBoundingBox.Y).After(1000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task MonthCalenderShowValueWhenOutOfFocus()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		var page = await ctx.LoadControlOnFormAsync(() => new ZDateEdit() { Width = 200 });

		var calendarButton = page.GetByRole(AriaRole.Button);
		var dateInput = page.GetByRole(AriaRole.Textbox);

		await Assertions.Expect(dateInput).ToHaveValueAsync(string.Empty);
		await ClickWithDelayAsync(calendarButton, 500);
		await page.Mouse.ClickAsync(0, 0);

		await Assertions.Expect(dateInput).Not.ToHaveValueAsync(string.Empty);
	}

	[Test, WithPlaywrightPage]
	public async Task BalloonMessageShouldBeFlippedAfterShownFromTextboxWhenCalendarOpened()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new WinzorTestForm();
			var factory = new BusinessObjectFactory();
			form.SetDataBinding(factory.New<DummyBusinessObject>(), "");
			var dateEdit = new ZDateEdit { Location = new Point(20, 100), BindTo = DummyBusinessObject.Schema.Z0_AnotherDate };
			form.Controls.Add(dateEdit);
			var otherEdit = new ZCheckBox { Location = new Point(20, 200), BindTo = DummyBusinessObject.Schema.Z0_Bool };
			form.Controls.Add(otherEdit);
			Balloon.Instance.IsShownDuringTesting = true;
			return form;
		});

		var button = page.GetByRole(AriaRole.Button);
		var textbox = page.GetByRole(AriaRole.Textbox);
		await textbox.FillAsync("1");
		await ClickWithDelayAsync(button, 500);

		var notificationElement = page.Locator(".notification");
		await notificationElement.WaitForAsync();
		var notificationBoundingBox = await notificationElement.BoundingBoxAsync();

		var popup = page.Locator("div.popup");
		await popup.WaitForAsync();
		Assert.That(async () => (await popup.BoundingBoxAsync()).Y, Is.LessThan(notificationBoundingBox.Y).After(1000, 100));
	}

	async Task ClickWithDelayAsync(ILocator locator, int delay)
	{
		await locator.ClickAsync();
		await Task.Delay(delay);
	}

	[Test, WithPlaywrightPage]
	public async Task ReadOnlyMonthCalendarNotShowValueWhenOutOfFocus()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		ZDateEdit dateEdit = null;
		var page = await ctx.LoadControlOnFormAsync(() => dateEdit = new ZDateEdit() { Width = 200 });

		var button = page.GetByRole(AriaRole.Button);
		var dateInput = page.GetByRole(AriaRole.Textbox);
		await Assertions.Expect(dateInput).ToHaveValueAsync(string.Empty);

		await ctx.WinzorDispatcher.InvokeAsync(() => dateEdit.ReadOnly = true);
		await ClickWithDelayAsync(button, 500);
		await page.Mouse.ClickAsync(0, 0);
		await Assertions.Expect(dateInput).ToHaveValueAsync(string.Empty);
	}

	[TestCase(true, TestName = "MonthCalendarNotVisibleWhenCalendarClickedIfReadOnly")]
	[TestCase(false, TestName = "MonthCalendarVisibleWhenCalendarClickedIfNotReadOnly")]
	public async Task MonthCalendarVisibleWhenCalendarClicked(bool readOnly)
	{
		using var ctx = new EnterpriseTestContext();
		ZDateEdit dateEdit = null;

		var rendered = await ctx.RenderControlOnFormAsync(() => dateEdit = new ZDateEdit() { Width = 200 });
		var button = rendered.Find(".button");
		Assert.That(button, Is.Not.Null);

		//We need ReadOnly to be false for the button to be rendered, so it is set after rendering the form.
		await ctx.WinzorDispatcher.InvokeAsync(() => dateEdit.ReadOnly = readOnly);
		Assert.That(dateEdit.ReadOnly, Is.EqualTo(readOnly));

		button.Click();

		Assert.That(ctx.JSInterop.Invocations.Where(x => x.Identifier == "showCalendar").Count, Is.EqualTo(readOnly ? 0 : 1).After(1000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task ReadOnlyDateEditShouldBeTabStopSkipped()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var button1 = new ZButton() { Name = "ButtonOne", Text = "button_one", Location = new Point(20, 100) };
			var dateEdit = new ZDateEdit()
			{
				Width = 200,
				ReadOnly = true,
				Location = new Point(20, 150)
			};
			var button2 = new ZButton() { Name = "ButtonTwo", Text = "button_two", Location = new Point(20, 200) };
			form.Controls.Add(button1);
			form.Controls.Add(dateEdit);
			form.Controls.Add(button2);
			return form;
		});

		var buttonOne = page.GetByTitle("button_one");
		await Assertions.Expect(buttonOne).ToBeFocusedAsync();

		await page.Keyboard.PressAsync("Tab");
		var buttonTwo = page.GetByTitle("button_two");
		await Assertions.Expect(buttonTwo).ToBeFocusedAsync();
	}

	[Test, WithPlaywrightPage]
	public async Task ClickCalendarInGridNoExceptionThrow()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new WinzorTestForm();
			var grid = new ZGrid { Width = 300, Height = 300 };
			form.BindingSource.SetBindingMember(grid, "Collection");
			var columnStyleInfo = new ZDateTimeOffsetEditColumnStyleInfo();
			columnStyleInfo.ColumnName = DummyBusinessObject.Schema.Z0_DateTimeOffset;
			grid.ColumnStyles.Add(columnStyleInfo);
			form.Controls.Add(grid);
			form.DataSourceType = typeof(DummyBusinessObject);
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObject>();
			grid.SetDataBinding(dummyBizo, "Collection", dummyBizo.Collection.GetType().Name);
			form.SetDataBinding(dummyBizo, "");
			return form;
		});

		var button = page.GetByRole(AriaRole.Button);
		await button.ClickAsync();

		var dateInput = page.GetByRole(AriaRole.Textbox);
		await dateInput.WaitForAsync();
		Assert.That(async () => (await dateInput.InputValueAsync()).Contains("GMT"), Is.True.After(1000, 100));

		await button.ClickAsync();
		await dateInput.FillAsync("23-SEP-24 16:52 GMT+10:00");
		await button.ClickAsync();
		await Assertions.Expect(dateInput).ToHaveValueAsync("23-SEP-24 16:52 GMT+10:00");
	}
}
