using System.Threading.Tasks;
using Bunit;
using Microsoft.Playwright;
using NUnit.Framework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace System.Windows.Forms;

using static PlaywrightTestContext;

class DateTimePickerTest
{
	[Test]
	public async Task DateTimePickerHasCorrectClass()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new DateTimePicker());
		Assert.That(rendered.Find(".datetimepicker"), Is.Not.Null);
	}

	[Test, WithPlaywrightPage]
	public async Task DateTimePickerFocus()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new DateTimePicker());

		var activeElement = async () => await page.EvaluateAsync<string>("document.activeElement.className");
		Assert.That(await activeElement(), Is.EqualTo(string.Empty));

		await page.Locator(".datetimepicker__dropdownbutton").ClickAsync();
		Assert.That(await activeElement(), Is.EqualTo(string.Empty));

		await page.Locator(".datetimepicker__input").ClickAsync();
		Assert.That(await activeElement(), Is.EqualTo("datetimepicker__input"));
	}

	[Test, WithPlaywrightPage]
	public async Task DateTimePickerStyle()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new DateTimePicker());

		var dropDownButton = page.Locator(".datetimepicker__dropdownbutton");
		var dropDownButtonWithCalendar = page.Locator(".datetimepicker__dropdownbuttonwithcalendar");
		var dateTimePickerInput = page.GetByRole(AriaRole.Textbox);

		await Assertions.Expect(dropDownButton).ToHaveCSSAsync("position", "absolute");
		await Assertions.Expect(dropDownButton).ToHaveCSSAsync("width", "31px");
		await Assertions.Expect(dropDownButton).ToHaveCSSAsync("height", "18px");
		await Assertions.Expect(dropDownButtonWithCalendar).ToHaveCSSAsync("width", "20px");
		await Assertions.Expect(dropDownButtonWithCalendar).ToHaveCSSAsync("height", "20px");
		await Assertions.Expect(dropDownButtonWithCalendar).ToHaveCSSAsync("margin-right", "5px");
		await Assertions.Expect(dropDownButtonWithCalendar).ToHaveCSSAsync("float", "right");
		await Assertions.Expect(dropDownButtonWithCalendar).ToHaveCSSAsync("background-position", "50% 50%");
		await Assertions.Expect(dropDownButtonWithCalendar).ToHaveCSSAsync("background-size", "contain");
		await Assertions.Expect(dropDownButtonWithCalendar).ToHaveCSSAsync("background-repeat", "no-repeat");
		await Assertions.Expect(dropDownButtonWithCalendar).ToHaveCSSAsync("background-image", "url(\"data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAACgAAAAWCAYAAACyjt" +
			"6wAAAAAXNSR0IB2cksfwAAAAlwSFlzAAAk6QAAJOkBUCTn+AAABBVJREFUeJy1lstvG0Ucx7+760eIU9K4hjxaXBvSQCkRz54QqAhxq6AHOMCBv6pIcIJKlXrMoVUrLpx7QIgmagOkURw/gpzuJrbj9XN" +
			"3h9/M7I7XjwRHckay5zezj/ns7/f9/WYioGbu799a/+P3tVq1BsYYxmvD92majlg8hqvvrCK7fEUb80Untgj/KxXya61WC6vvf4B8IY9up+svzxDw8j6AZ4NjJucWF5fQath4ur4+CbYeoNPt4tzL55FZ" +
			"XsZS+rJYUS7q94M2/Xk+peg8OdYNA+W9IsrFnckCyibdEI1EezCQMAFUGE5X0DT2KLyhj3Bc9ywAAZdWKpVKcBwXQBiIo2oK0mO90Avbk/2r8/My7GPr+BSA/J0ugdXrdSwsLArB82WajQYOKxXMzy+oU" +
			"FcOK+iSl+bmkhKaAMv7ZbTbnUmyDQBC6omDJWZmyNIRZGrtqI7pxIzyoG23oLkOpqcTSpeGYWHCzusHRCgB5CISmOtLzoW1yIY0GZ4f1Ta3Ciy/Z54KztD1sAf9RQQUUXHN+bYnEoH1wfAx12wYUvxG1E" +
			"feHNfD2qPHKJUPxoKLGDq+ufnxoAcZeD3c3t5WC/OMbLfaeP58S8212m1hV0VhlwnSIK2emz1/bIhX37qsPf0nz3648wiF//FkPBbFzc8/wne3PtX6NMg9EIvFcelSmotRwNi2jYODQywuXVRJYlkWXPJ" +
			"I8kJKJUmpVOxp8BjIaytp7cmzHfbj3V9R/NcaeU8sFsGXX1zH919/JnaiERqkyWgUsqxwu8NNRER9lB40jAj1jpwL6qBuhPR7fHv37az2ZDPHfiLIQU9yzXHPBXB9gL2i3BO6CB9PEi+sT/h6hIJj/teF" +
			"t78TIa9mtI2/dtntXx5iz9dkoDke1vC9fXUwEH+H9mIZYrkNch12qQ8SxOVjxxH3Bc+5zPM/Yrw6IzT5d57dvvMQL6waee7DIbgBD0oNNilJcrldMcPHXGvNZgM7uZwKYafTEZD1I1ttb/y52dmk2iLHa" +
			"dfeTGt/kiY3t4r49qtPRp5++jzIszEen0I6kyUHSg/W6zYs8wUuvpb27wGNTQGYSr2ivFosFuSWeMpC/R5p8qTrAyGWkBrPCsjnuC1OLgjew4TJEMwxf2+meknQvDZqEzkJDgBKyLDQNaFHL7RreOp6rz" +
			"gfmvt4/NsDCntbZHWEjlyZ1zOTBwwghLao9gVlpkH663YdcYgIDqqtZkt4yrbrOKpVCQq4fuMGlR8dOpWbC6nU7hkAyt/U1EswSWNKTmTwRU3SoToj+p41LRO1ahWJRAJXVlbmkslkZVJg/YCaJvZcvvg" +
			"CHduHDwHDJ+oANqZraFq7OAs4BZhMpfBsYwP37/0sxM6bSsaQwfrG0uT5kH0jexZsov0H+5JPpRGhTREAAAAASUVORK5CYII=\")");
		await Assertions.Expect(dateTimePickerInput).ToHaveCSSAsync("position", "absolute");
		await Assertions.Expect(dateTimePickerInput).ToHaveCSSAsync("height", "20px");
		await Assertions.Expect(dateTimePickerInput).ToHaveCSSAsync("width", "200px");
		await Assertions.Expect(dateTimePickerInput).ToHaveCSSAsync("left", "0px");
		await Assertions.Expect(dateTimePickerInput).ToHaveCSSAsync("top", "0px");
		await Assertions.Expect(dateTimePickerInput).ToHaveCSSAsync("border", "1px solid rgb(165, 167, 168)");
		await Assertions.Expect(dateTimePickerInput).ToHaveCSSAsync("cursor", "pointer");

		await dateTimePickerInput.FocusAsync();
		var focusedDateTimePickerInput = page.Locator(".datetimepicker__input:focus");
		await Assertions.Expect(focusedDateTimePickerInput).ToHaveCSSAsync("border", "1px solid rgb(94, 165, 221)");

		await dateTimePickerInput.HoverAsync();
		await Assertions.Expect(dateTimePickerInput).ToHaveCSSAsync("border", "1px solid rgb(109, 111, 113)");

		await dropDownButton.HoverAsync();
		await Assertions.Expect(dropDownButton).ToHaveCSSAsync("background-color", "rgb(229, 241, 251)");
		await Assertions.Expect(dropDownButton).ToHaveCSSAsync("border-left", "1px solid rgb(96, 166, 221)");
	}

	[Test, WithPlaywrightPage]
	public async Task DateTimePickerCalendarUpdateValueByInput()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new DateTimePicker());

		var dateTimePickerInput = page.GetByRole(AriaRole.Textbox);
		var currentDate = await dateTimePickerInput.InputValueAsync();
		await dateTimePickerInput.PressSequentiallyAsync("11");
		var expectedDateAfterEdit = $"{currentDate[..8]}11";
		await Assertions.Expect(dateTimePickerInput).ToHaveValueAsync(expectedDateAfterEdit);
	}

	[WithPlaywrightPage]
	[TestCase("111", "111111", "1111-01-11", "1753-01-11", TestName = "{m}_MinDateValidation")]
	[TestCase("222", "999999", "9999-02-22", "9998-02-22", TestName = "{m}_MaxDateValidation")]
	public async Task DateTimePickerDateTimeValidation(string inputDayMonth, string inputYear, string expectedDateTime, string expectedDateTimeAfter)
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new DateTimePicker());

		var dateTimePickerInput = Page.GetByRole(AriaRole.Textbox);
		var delay = new LocatorPressSequentiallyOptions { Delay = 500 };
		await dateTimePickerInput.PressSequentiallyAsync(inputDayMonth, delay);
		await dateTimePickerInput.PressAsync("ArrowRight");
		await dateTimePickerInput.PressSequentiallyAsync(inputYear, delay);
		await Assertions.Expect(dateTimePickerInput).ToHaveValueAsync(expectedDateTime);

		await page.Locator(".datetimepicker__dropdownbuttonwithcalendar").ClickAsync(new LocatorClickOptions { Delay = 500 });
		await Assertions.Expect(dateTimePickerInput).ToHaveValueAsync(expectedDateTimeAfter);
	}
}
