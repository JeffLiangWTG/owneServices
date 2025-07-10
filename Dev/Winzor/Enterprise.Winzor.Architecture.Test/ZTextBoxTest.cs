using System.Threading.Tasks;
using System.Windows.Forms;
using Enterprise.ZArchitecture;
using Microsoft.Playwright;
using NUnit.Framework;
using WTG.PlaywrightTesting;

namespace Enterprise.Winzor.Architecture.Test;

class ZTextBoxTest
{
	[Test, WithPlaywrightPage]
	public async Task TestFocusedOnMouseClick()
	{
		var textBox1 = default(ZTextBox);
		var textBox2 = default(ZTextBox);
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			textBox1 = new ZTextBox();
			textBox2 = new ZTextBox();
			form.Controls.Add(textBox1);
			form.Controls.Add(textBox2);
			return form;
		});

		Assert.That(textBox1.Focused, Is.True);
		Assert.That(textBox2.Focused, Is.False);

		var textBox2Locator = page.Locator(".textbox").Nth(1);
		await textBox2Locator.ClickAsync();
		await Assertions.Expect(textBox2Locator).ToBeFocusedAsync();

		Assert.That(textBox1.Focused, Is.False);
		Assert.That(textBox2.Focused, Is.True);
	}

	[Test, WithPlaywrightPage]
	public async Task DynamicMultilineTextBoxExpandWhenExpandArrowClicked()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var textBox = new ZTextBox { IsDynamicMultiline = true };
			form.Controls.Add(textBox);
			return form;
		});

		var expandArrow = await page.WaitForSelectorAsync(".textbox--dynamic-expand-arrow");
		Assert.That(expandArrow, Is.Not.Null);

		await expandArrow.ClickAsync();
		var dynamicTextArea = await page.WaitForSelectorAsync(".dynamic-multiline-textbox > textarea");
		Assert.That(dynamicTextArea, Is.Not.Null);
		Assert.That(async () => await dynamicTextArea.EvaluateAsync<int>("e => window.getComputedStyle(e).getPropertyValue('z-index')"), Is.EqualTo(100));
		Assert.That(async () => await dynamicTextArea.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('height')"), Is.EqualTo("200px"));
		Assert.That(async () => await dynamicTextArea.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('overflow-y')"), Is.EqualTo("scroll"));
	}

	[Test, WithPlaywrightPage]
	public async Task DynamicMultilineTextBoxContainLineBreaksWhenNotExpand()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var textBox = new ZTextBox { IsDynamicMultiline = true, Multiline = true };
			form.Controls.Add(textBox);
			return form;
		});

		const string inputText = "ELA=B\nC\nD\n\nB\n";
		var pressSequentiallyOptions = new LocatorPressSequentiallyOptions { Delay = 50 };
		var pressOptions = new LocatorPressOptions { Delay = 50 };

		var textBox = page.Locator(".form > textarea");
		await textBox.PressSequentiallyAsync("ELA=B", pressSequentiallyOptions);
		await textBox.PressAsync(nameof(Keys.Enter), pressOptions);
		await textBox.PressAsync("C", pressOptions);
		await textBox.PressAsync(nameof(Keys.Enter), pressOptions);
		await textBox.PressAsync("D", pressOptions);
		await textBox.PressAsync(nameof(Keys.Enter), pressOptions);
		await textBox.PressAsync(nameof(Keys.Enter), pressOptions);
		await textBox.PressAsync("B", pressOptions);
		await textBox.PressAsync(nameof(Keys.Enter), pressOptions);
		await textBox.EvaluateAsync("e => e.blur()");

		await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
		textBox = page.Locator(".form > textarea");
		Assert.That(await textBox.InputValueAsync(), Is.EqualTo(inputText));
	}

	[Test, WithPlaywrightPage]
	public async Task DynamicMultilineTextBoxContainLineBreaksWhenExpand()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			var textBox = new ZTextBox { IsDynamicMultiline = true, Multiline = true };
			form.Controls.Add(textBox);
			return form;
		});

		const string inputText = "ELA=B\nC\nD\n\nB\n";
		var pressSequentiallyOptions = new LocatorPressSequentiallyOptions { Delay = 50 };
		var pressOptions = new LocatorPressOptions { Delay = 50 };

		var textBox = page.Locator(".form > textarea");
		await textBox.PressSequentiallyAsync("ELA=B", pressSequentiallyOptions);
		await textBox.PressAsync(nameof(Keys.Enter), pressOptions);
		await textBox.PressAsync("C", pressOptions);
		await textBox.PressAsync(nameof(Keys.Enter), pressOptions);
		await textBox.PressAsync("D", pressOptions);
		await textBox.PressAsync(nameof(Keys.Enter), pressOptions);
		await textBox.PressAsync(nameof(Keys.Enter), pressOptions);
		await textBox.PressAsync("B", pressOptions);
		await textBox.PressAsync(nameof(Keys.Enter), pressOptions);
		await textBox.EvaluateAsync("e => e.blur()");

		var expandArrow = await page.WaitForSelectorAsync(".textbox--dynamic-expand-arrow");
		Assert.That(expandArrow, Is.Not.Null);
		await expandArrow.ClickAsync();

		await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
		var dynamicTextArea = await page.WaitForSelectorAsync(".dynamic-multiline-textbox > textarea");
		Assert.That(dynamicTextArea, Is.Not.Null);
		Assert.That(await dynamicTextArea.InputValueAsync(), Is.EqualTo(inputText));
		await dynamicTextArea.EvaluateAsync("e => e.blur()");

		await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
		textBox = page.Locator(".form > textarea");
		Assert.That(await textBox.InputValueAsync(), Is.EqualTo(inputText));
	}
}
