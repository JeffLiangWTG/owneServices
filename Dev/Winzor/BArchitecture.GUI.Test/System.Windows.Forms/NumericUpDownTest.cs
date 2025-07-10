using System.Collections.Generic;
using System.Threading.Tasks;
using Bunit;
using Microsoft.Playwright;
using NUnit.Framework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace System.Windows.Forms;

using static PlaywrightTestContext;

internal class NumericUpDownTest
{
	static readonly IEnumerable<HorizontalAlignment> TextAlignments = new[] { HorizontalAlignment.Left, HorizontalAlignment.Center, HorizontalAlignment.Right };

	[Test]
	public async Task NumericUpDownValueChangedEvent()
	{
		using var ctx = new WinzorTestContext();

		var valueChangeCalled = 0;

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var numericUpDown = new NumericUpDown();
			numericUpDown.ValueChanged += (_, _) => valueChangeCalled++;
			return numericUpDown;
		});

		var numericUpDownInput = rendered.Find(".numericupdown__input");
		var upButton = rendered.Find(".numericupdown__upbutton");

		await numericUpDownInput.ChangeAsync(new Microsoft.AspNetCore.Components.ChangeEventArgs() { Value = "10" });
		Assert.That(valueChangeCalled, Is.EqualTo(1));
		await upButton.ClickAsync(new WebMouseEventArgs());
		Assert.That(valueChangeCalled, Is.EqualTo(2));
	}

	[Test]
	public async Task NumericUpDownHasStyleClassSet()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var numericUpDown = new NumericUpDown();
			return numericUpDown;
		});

		var numericUpDown = rendered.Find(".numericupdown");

		Assert.That(numericUpDown, Is.Not.Null);
	}

	[Test]
	public async Task NumericUpDownHasCorrectSizeAndPosition()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var numericUpDown = new NumericUpDown();
			numericUpDown.Width = 50;
			numericUpDown.Height = 25;
			return numericUpDown;
		});

		var numericUpDown = rendered.Find(".numericupdown");

		Assert.That(numericUpDown.GetAttribute("style"), Is.EqualTo("position:absolute;width:50px;height:25px;top:0px;left:0px;background-color:var(--color-control);overflow:hidden;"));
	}

	[Test]
	public async Task NumericUpDownInputParametersAreSet()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var numericUpDown = new NumericUpDown();
			numericUpDown.Value = 10;
			numericUpDown.Minimum = 5;
			numericUpDown.Maximum = 20;
			numericUpDown.Increment = 1;
			return numericUpDown;
		});

		var numericUpDownInput = rendered.Find(".numericupdown__input");

		Assert.That(numericUpDownInput.GetAttribute("value"), Is.EqualTo("10"));
		Assert.That(numericUpDownInput.GetAttribute("type"), Is.EqualTo("number"));
		Assert.That(numericUpDownInput.GetAttribute("min"), Is.EqualTo("5"));
		Assert.That(numericUpDownInput.GetAttribute("max"), Is.EqualTo("20"));
		Assert.That(numericUpDownInput.GetAttribute("step"), Is.EqualTo("1"));
		Assert.That(numericUpDownInput.GetAttribute("readonly"), Is.Null);
		Assert.That(numericUpDownInput.GetAttribute("disabled"), Is.Null);
	}

	[Test]
	public async Task NumericUpDownUpButtonIncreasesValueByIncrement()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var numericUpDown = new NumericUpDown();
			numericUpDown.Value = 50;
			numericUpDown.Minimum = 0;
			numericUpDown.Maximum = 100;
			numericUpDown.Increment = 10;
			return numericUpDown;
		});

		var numericUpDownInput = rendered.Find(".numericupdown__input");
		var upButton = rendered.Find(".numericupdown__upbutton");

		Assert.That(numericUpDownInput.Attributes["value"].Value, Is.EqualTo("50"));
		await upButton.ClickAsync(new WebMouseEventArgs());
		Assert.That(numericUpDownInput.Attributes["value"].Value, Is.EqualTo("60"));
	}

	[Test]
	public async Task NumericUpDownDownButtonDecreasesValueByIncrement()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var numericUpDown = new NumericUpDown();
			numericUpDown.Value = 50;
			numericUpDown.Minimum = 0;
			numericUpDown.Maximum = 100;
			numericUpDown.Increment = 10;
			return numericUpDown;
		});

		var numericUpDownInput = rendered.Find(".numericupdown__input");
		var downButton = rendered.Find(".numericupdown__downbutton");

		Assert.That(numericUpDownInput.Attributes["value"].Value, Is.EqualTo("50"));
		await downButton.ClickAsync(new WebMouseEventArgs());
		Assert.That(numericUpDownInput.Attributes["value"].Value, Is.EqualTo("40"));
	}

	[Test]
	[TestCase(0, "5", "5", TestName = "NumericUpDownRoundsAndDisplaysCorrectDecimalPlaces_NoDecimalPlaceNoRounding")]
	[TestCase(0, "5.5", "6", TestName = "NumericUpDownRoundsAndDisplaysCorrectDecimalPlaces_NoDecimalPlaceWithRounding")]
	[TestCase(1, "5", "5.0", TestName = "NumericUpDownRoundsAndDisplaysCorrectDecimalPlaces_OneDecimalPlaceNoRounding")]
	[TestCase(1, "5.55", "5.6", TestName = "NumericUpDownRoundsAndDisplaysCorrectDecimalPlaces_OneDecimalPlaceWithRounding")]
	[TestCase(2, "5", "5.00", TestName = "NumericUpDownRoundsAndDisplaysCorrectDecimalPlaces_TwoDecimalPlaceNoRounding")]
	[TestCase(2, "5.555", "5.56", TestName = "NumericUpDownRoundsAndDisplaysCorrectDecimalPlaces_TwoDecimalPlaceWithRounding")]
	public async Task NumericUpDownRoundsAndDisplaysCorrectDecimalPlaces(int decimalPlaces, string value, string expectedValue)
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var numericUpDown = new NumericUpDown();
			numericUpDown.DecimalPlaces = decimalPlaces;
			return numericUpDown;
		});

		var numericUpDownInput = rendered.Find(".numericupdown__input");
		await numericUpDownInput.ChangeAsync(new Microsoft.AspNetCore.Components.ChangeEventArgs() { Value = value });
		Assert.That(numericUpDownInput.Attributes["value"].Value, Is.EqualTo(expectedValue));
	}

	[Test]
	public async Task NumericUpDownReadOnlyAppliedToControlInput()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var numericUpDown = new NumericUpDown();
			numericUpDown.ReadOnly = true;
			return numericUpDown;
		});

		var numericUpDownInput = rendered.Find(".numericupdown__input");

		Assert.That(numericUpDownInput.GetAttribute("readonly"), Is.Not.Null);
	}

	[Test]
	public async Task NumericUpDownNotEnabledDisablesControlAndButtons()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var numericUpDown = new NumericUpDown();
			numericUpDown.Enabled = false;
			return numericUpDown;
		});

		var numericUpDownInput = rendered.Find(".numericupdown__input");
		var upButton = rendered.Find(".numericupdown__upbutton");
		var downButton = rendered.Find(".numericupdown__downbutton");

		Assert.That(numericUpDownInput.GetAttribute("disabled"), Is.Not.Null);
		Assert.That(upButton.GetAttribute("disabled"), Is.Not.Null);
		Assert.That(downButton.GetAttribute("disabled"), Is.Not.Null);
	}

	[Test]
	public async Task NumericUpDownValueCannotBeSetAboveMax()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var numericUpDown = new NumericUpDown();
			numericUpDown.Value = 15;
			numericUpDown.Minimum = 10;
			numericUpDown.Maximum = 20;
			numericUpDown.Increment = 10;
			return numericUpDown;
		});

		var numericUpDownInput = rendered.Find(".numericupdown__input");
		var upButton = rendered.Find(".numericupdown__upbutton");

		Assert.That(numericUpDownInput.Attributes["value"].Value, Is.EqualTo("15"));
		await upButton.ClickAsync(new WebMouseEventArgs());
		Assert.That(numericUpDownInput.Attributes["value"].Value, Is.EqualTo("20"));
		await numericUpDownInput.ChangeAsync(new Microsoft.AspNetCore.Components.ChangeEventArgs() { Value = "25" });
		Assert.That(numericUpDownInput.Attributes["value"].Value, Is.EqualTo("20"));
	}

	[Test]
	public async Task NumericUpDownValueCannotBeSetBelowMin()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var numericUpDown = new NumericUpDown();
			numericUpDown.Value = 15;
			numericUpDown.Minimum = 10;
			numericUpDown.Maximum = 20;
			numericUpDown.Increment = 10;
			return numericUpDown;
		});

		var numericUpDownInput = rendered.Find(".numericupdown__input");
		var downButton = rendered.Find(".numericupdown__downbutton");

		Assert.That(numericUpDownInput.Attributes["value"].Value, Is.EqualTo("15"));
		await downButton.ClickAsync(new WebMouseEventArgs());
		Assert.That(numericUpDownInput.Attributes["value"].Value, Is.EqualTo("10"));
		await numericUpDownInput.ChangeAsync(new Microsoft.AspNetCore.Components.ChangeEventArgs() { Value = "5" });
		Assert.That(numericUpDownInput.Attributes["value"].Value, Is.EqualTo("10"));
	}

	[Test]
	public async Task NumericUpDownSetAboveMaximumSetsToMaximum()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var numericUpDown = new NumericUpDown();
			numericUpDown.Maximum = 100;
			return numericUpDown;
		});

		var numericUpDownInput = rendered.Find(".numericupdown__input");

		await numericUpDownInput.ChangeAsync(new Microsoft.AspNetCore.Components.ChangeEventArgs() { Value = "110" });
		Assert.That(numericUpDownInput.Attributes["value"].Value, Is.EqualTo("100"));
		await numericUpDownInput.ChangeAsync(new Microsoft.AspNetCore.Components.ChangeEventArgs() { Value = "1100" });
		Assert.That(numericUpDownInput.Attributes["value"].Value, Is.EqualTo("100"));
		await numericUpDownInput.ChangeAsync(new Microsoft.AspNetCore.Components.ChangeEventArgs() { Value = "11000" });
		Assert.That(numericUpDownInput.Attributes["value"].Value, Is.EqualTo("100"));
	}

	[Test]
	public async Task NumericUpDownSetBelowMinimumSetsToMinimum()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var numericUpDown = new NumericUpDown();
			numericUpDown.Minimum = 0;
			return numericUpDown;
		});

		var numericUpDownInput = rendered.Find(".numericupdown__input");

		await numericUpDownInput.ChangeAsync(new Microsoft.AspNetCore.Components.ChangeEventArgs() { Value = "-10" });
		Assert.That(numericUpDownInput.Attributes["value"].Value, Is.EqualTo("0"));
		await numericUpDownInput.ChangeAsync(new Microsoft.AspNetCore.Components.ChangeEventArgs() { Value = "-100" });
		Assert.That(numericUpDownInput.Attributes["value"].Value, Is.EqualTo("0"));
		await numericUpDownInput.ChangeAsync(new Microsoft.AspNetCore.Components.ChangeEventArgs() { Value = "-1000" });
		Assert.That(numericUpDownInput.Attributes["value"].Value, Is.EqualTo("0"));
	}

	[Test, WithPlaywrightPage]
	[TestCaseSource(nameof(TextAlignments))]
	public async Task NumericUpDownTextAlign(HorizontalAlignment alignment)
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new NumericUpDown { TextAlign = alignment });

		var numericUpDownNode = page.Locator(".numericupdown__input");
		var parentNode = page.Locator("body");
		var parentStyle = await parentNode.GetComputedStyleAsync("text-align");

		if (alignment == HorizontalAlignment.Left)
		{
			Assert.That(await numericUpDownNode.GetComputedStyleAsync("text-align"), Is.EqualTo(parentStyle));
		}
		else
		{
			Assert.That(await numericUpDownNode.GetComputedStyleAsync("text-align"), Is.EqualTo(alignment.ToString().ToLower()));
		}
	}

	[Test, WithPlaywrightPage]
	[TestCase(".numericupdown__input")]
	[TestCase(".numericupdown__upbutton")]
	[TestCase(".numericupdown__downbutton")]
	public async Task NumericUpDownFocus(string selector)
	{
		await using var ctx = new InMemoryTestServerContext();
		NumericUpDown numericUpDown = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			numericUpDown = new NumericUpDown { Top = 30 };
			form.Controls.Add(new TextBox());
			form.Controls.Add(numericUpDown);
			return form;
		});

		Assert.That(numericUpDown.Focused, Is.False);

		await page.Locator(selector).ClickAsync();
		Assert.That(() => numericUpDown.Focused, Is.True.After(2000, 100));

		await page.Locator(".textbox").Nth(0).ClickAsync();
		Assert.That(() => numericUpDown.Focused, Is.False.After(2000, 100));
	}

	[Explicit]
	[Test, WithPlaywrightPage(Headless = false)]
	public async Task NumericUpDownControlDemo()
	{
		await using var ctx = new InMemoryTestServerContext();

		var gotFocusCalled = new TaskCompletionSource<bool>();
		var lostFocusCalled = new TaskCompletionSource<bool>();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var numericUpDown = new NumericUpDown()
			{
				Width = 50,
				Height = 25,
			};
			form.Controls.Add(numericUpDown);
			return form;
		});

		await page.WaitForTimeoutAsync(120000);
	}

	[Test]
	public async Task NumericUpDownHasInitalTextZero()
	{
		NumericUpDown numericUpDown = null;
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			numericUpDown = new NumericUpDown();
			return numericUpDown;
		});

		Assert.That(numericUpDown.Text, Is.EqualTo("0"));
	}

	[Test]
	public async Task NumericUpDownDisplaysEmptyContentWhenValueIsNotSetAndTextIsEmpty()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var numericUpDown = new NumericUpDown();
			numericUpDown.Text = string.Empty;
			return numericUpDown;
		});

		var numericUpDownInput = rendered.Find(".numericupdown__input");
		Assert.That(numericUpDownInput.Attributes["value"].Value, Is.EqualTo(string.Empty));
	}

	[Test]
	public async Task NumericUpDownDisplaysValueDespiteReadonlyAndDisabled()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var numericUpDown = new NumericUpDown();
			numericUpDown.Value = 1;
			numericUpDown.ReadOnly = true;
			numericUpDown.Enabled = false;
			return numericUpDown;
		});

		var numericUpDownInput = rendered.Find(".numericupdown__input");
		Assert.That(numericUpDownInput.Attributes["value"].Value, Is.EqualTo("1"));
	}

	[Test]
	public async Task NumericUpDownDisplaysValueAfterValueIsSetEvenIfTextWasEmptyBefore()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new NumericUpDown
		{
			Text = string.Empty,
			Value = 1
		});

		var numericUpDownInput = rendered.Find(".numericupdown__input");
		Assert.That(numericUpDownInput.Attributes["value"].Value, Is.EqualTo("1"));

		var upButton = rendered.Find(".numericupdown__upbutton");
		await upButton.ClickAsync(new WebMouseEventArgs());
		Assert.That(numericUpDownInput.Attributes["value"].Value, Is.EqualTo("2"));
	}

	[TestCase(false, "123", "123")]
	[TestCase(false, "12345", "12345")]
	[TestCase(false, "1234567", "1234567")]
	[TestCase(true, "123", "123")]
	[TestCase(true, "12345", "12,345")]
	[TestCase(true, "1234567", "1,234,567")]
	public async Task NumericUpDownThousandsSeparator(bool thousandsSeparator, string value, string expectedValue)
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var numericUpDown = new NumericUpDown();
			Assert.That(numericUpDown.ThousandsSeparator, Is.EqualTo(false));

			numericUpDown.Maximum = int.MaxValue;
			numericUpDown.ThousandsSeparator = thousandsSeparator;
			return numericUpDown;
		});

		var numericUpDownInput = rendered.Find(".numericupdown__input");
		await numericUpDownInput.ChangeAsync(new Microsoft.AspNetCore.Components.ChangeEventArgs() { Value = value });
		Assert.That(numericUpDownInput.Attributes["value"].Value, Is.EqualTo(expectedValue));
	}

	[Test]
	public async Task NumericUpDownShouldBeDisabledWhenReadonly()
	{
		using var ctx = new WinzorTestContext();
		NumericUpDown numericUpDown = null;
		var rendered = await ctx.RenderControlOnFormAsync(() => numericUpDown = new NumericUpDown() { ReadOnly = true, Enabled = true, });
		var input = rendered.Find("input.numericupdown__input");
		Assert.That(input.Attributes["disabled"], Is.Not.Null);
		Assert.That(input.Attributes["readonly"], Is.Not.Null);

		await numericUpDown.InvokeWinzorDispatcherAsync(() => numericUpDown.ReadOnly = false);
		Assert.That(input.Attributes["disabled"], Is.Null);
		Assert.That(input.Attributes["readonly"], Is.Null);
	}

	[Test]
	public async Task NumericUpDownReadonlyUpdatesFromServer()
	{
		await ControlAssert.ImplementsChangeFromServerAsync<NumericUpDown>(c => c.ReadOnly = true);
	}

	[Test]
	public async Task NumericUpDownDoesNotAllowChangeWhenDisabled()
	{
		using var ctx = new WinzorTestContext();
		NumericUpDown numericUpDown = null;
		var rendered = await ctx.RenderControlOnFormAsync(() => numericUpDown = new NumericUpDown() { Value = 10, Enabled = false, });

		await rendered.Find(".numericupdown__upbutton").ClickAsync(new WebMouseEventArgs());
		Assert.That(numericUpDown.Value, Is.EqualTo(10));

		await rendered.Find(".numericupdown__downbutton").ClickAsync(new WebMouseEventArgs());
		Assert.That(numericUpDown.Value, Is.EqualTo(10));

		var input = rendered.Find(".numericupdown__input");
		await input.ChangeAsync(new Microsoft.AspNetCore.Components.ChangeEventArgs() { Value = 15 });
		Assert.That(numericUpDown.Value, Is.EqualTo(10));
	}

	[Test]
	public async Task NumericUpDownDoesNotAllowChangeWhenReadOnly()
	{
		using var ctx = new WinzorTestContext();
		NumericUpDown numericUpDown = null;
		var rendered = await ctx.RenderControlOnFormAsync(() => numericUpDown = new NumericUpDown() { Value = 10, ReadOnly = true, });

		await rendered.Find(".numericupdown__upbutton").ClickAsync(new WebMouseEventArgs());
		Assert.That(numericUpDown.Value, Is.EqualTo(10));

		await rendered.Find(".numericupdown__downbutton").ClickAsync(new WebMouseEventArgs());
		Assert.That(numericUpDown.Value, Is.EqualTo(10));

		var input = rendered.Find(".numericupdown__input");
		await input.ChangeAsync(new Microsoft.AspNetCore.Components.ChangeEventArgs() { Value = 15 });
		Assert.That(numericUpDown.Value, Is.EqualTo(10));
	}

	[Test, WithPlaywrightPage]
	[TestCase(".numericupdown__upbutton")]
	[TestCase(".numericupdown__downbutton")]
	public async Task NumericUpDownFocusOnElementClick(string selector)
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var numericUpDown = new NumericUpDown();
			form.Controls.Add(numericUpDown);
			return form;
		});

		await page.Locator(selector).ClickAsync();
		var inputElement = page.Locator(".numericupdown__input");

		var isFocused = await inputElement.EvaluateAsync<bool>("element => element === document.activeElement");
		Assert.That(isFocused, Is.True);
	}

	[Test, WithPlaywrightPage]
	[TestCase(10, 5)]
	[TestCase(400, 405)]
	public async Task NumericUpDownInputValueOutOfBounds(int expectedValue, int inputValue)
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new NumericUpDown
		{
			Minimum = 10,
			Maximum = 400,
			Value = 100
		});

		var inputElement = page.Locator(".numericupdown__input");
		Assert.That(() => inputElement.EvaluateAsync<int>("element => element.value"), Is.EqualTo(100));
		await AssertDisplayedValueWhenInputValueOutOfBounds();
		await AssertDisplayedValueWhenInputValueOutOfBounds();

		async Task AssertDisplayedValueWhenInputValueOutOfBounds()
		{
			await inputElement.FillAsync(inputValue.ToString());
			Assert.That(() => inputElement.EvaluateAsync<int>("element => element.value"), Is.EqualTo(inputValue).After(1000, 100));

			await inputElement.BlurAsync();
			Assert.That(() => inputElement.EvaluateAsync<int>("element => element.value"), Is.EqualTo(expectedValue).After(1000, 100));
		}
	}

	[Test, WithPlaywrightPage]
	public async Task NumericUpDownButtonsDoNotFocusOnClick()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new NumericUpDown());

		var numericUpDownElement = page.Locator(".numericupdown__input");
		await numericUpDownElement.FocusAsync();

		var upButton = page.Locator(".numericupdown__upbutton");
		await upButton.ClickAsync();

		var upButtonHasFocus = await upButton.EvaluateAsync<bool>("element => element === document.activeElement");
		Assert.That(upButtonHasFocus, Is.False, "Button should not be focused");

		var controlHasFocus = await numericUpDownElement.EvaluateAsync<bool>("element => element === document.activeElement");
		Assert.That(controlHasFocus, Is.True, "control should be focused");

		var downButton = page.Locator(".numericupdown__downbutton");
		await downButton.ClickAsync();

		var downButtonHasFocus = await downButton.EvaluateAsync<bool>("element => element === document.activeElement");
		Assert.That(downButtonHasFocus, Is.False, "Button should not be focused");

		var isControlFocused = await numericUpDownElement.EvaluateAsync<bool>("element => element === document.activeElement");
		Assert.That(isControlFocused, Is.True,"control should be focused");
	}

	[Test, WithPlaywrightPage]
	public async Task NumericUpDownUpButtonDontHaveTabStop()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new NumericUpDown());

		var inputElement = page.Locator(".numericupdown__input");
		var upButton = page.Locator(".numericupdown__upbutton");

		await inputElement.FocusAsync();
		await inputElement.PressAsync("Tab");

		var isFocused = await upButton.EvaluateAsync<bool>("element => element === document.activeElement");
		Assert.That(isFocused, Is.False, "button should not be focused");
	}

	[Test, WithPlaywrightPage]
	public async Task NumericUpDownDownButtonDoesNotHaveTabStop()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new NumericUpDown());

		var inputElement = page.Locator(".numericupdown__input");
		var downButton = page.Locator(".numericupdown__downbutton");

		await inputElement.FocusAsync();
		await inputElement.PressAsync("Tab");
		await inputElement.PressAsync("Tab");

		var isFocused = await downButton.EvaluateAsync<bool>("element => element === document.activeElement");
		Assert.That(isFocused, Is.False, "Button should not be focused");
	}
}
