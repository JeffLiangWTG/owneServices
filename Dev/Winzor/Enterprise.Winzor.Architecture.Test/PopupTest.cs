using System.Threading.Tasks;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using Microsoft.Playwright;
using NUnit.Framework;
using WTG.PlaywrightTesting;

namespace Enterprise.Winzor.Architecture.Test;
sealed class PopupTest
{
	[Test, WithPlaywrightPage]
	public async Task PopupAnchorRichTextBoxCursorPosition()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var (page, editor) = await GetInitializedObjectsAsync(ctx);
		var zdropform = page.Locator(".zdropform");

		await editor.PressSequentiallyAsync("first@");
		await zdropform.WaitForAsync();
		Assert.That(getZDropFormXPosition, Is.GreaterThan(0));
		Assert.That(getZDropFormYPosition, Is.GreaterThan(0));

		var initialXPosition = await getZDropFormXPosition();
		var initialYPosition = await getZDropFormYPosition();
		await editor.PressSequentiallyAsync(" second@");
		await zdropform.WaitForAsync();
		Assert.That(getZDropFormXPosition, Is.GreaterThan(initialXPosition), "DropForm should have moved right");
		Assert.That(getZDropFormYPosition, Is.EqualTo(initialYPosition), "DropForm should not have moved down");

		var nextXPosition = await getZDropFormXPosition();
		var nextYPosition = await getZDropFormYPosition();
		await editor.PressAsync(" ");
		await editor.PressAsync("Enter");
		await editor.PressSequentiallyAsync("a new line with some text@");
		await zdropform.WaitForAsync();
		Assert.That(getZDropFormXPosition, Is.GreaterThan(nextXPosition), "DropForm should have moved right");
		Assert.That(getZDropFormYPosition, Is.GreaterThan(nextYPosition), "DropForm should have moved down");

		async Task<float> getZDropFormXPosition() => (await zdropform.BoundingBoxAsync()).X;
		async Task<float> getZDropFormYPosition() => (await zdropform.BoundingBoxAsync()).Y;
	}

	[Test, WithPlaywrightPage]
	public async Task UpdateContentAfterTypeMatchPopupList()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var (page, editor) = await GetInitializedObjectsAsync(ctx);
		var dropForm = page.Locator(".zdropform");

		await editor.PressSequentiallyAsync("@a");
		await WaitForDropFormItemAsync(dropForm, "Apple");
		await editor.PressAsync("Enter");
		Assert.That(() => editor.InnerTextAsync(), Is.EqualTo("@Apple").After(2000, 100));

		await editor.PressSequentiallyAsync("234");
		await editor.PressAsync("ArrowLeft");
		await editor.PressSequentiallyAsync("@a");
		await WaitForDropFormItemAsync(dropForm, "Apple");
		await editor.PressAsync("Enter");
		Assert.That(() => editor.InnerTextAsync(), Is.EqualTo("@Apple23@Apple4").After(2000, 100));

		await editor.PressAsync("ArrowRight");
		await editor.PressAsync("Enter");
		await editor.PressAsync("Enter");
		await editor.PressAsync("Enter");
		await editor.PressSequentiallyAsync("@stop");
		await page.Mouse.ClickAsync(20, 20);

		Assert.That(() => dropForm.IsVisibleAsync(), Is.False.After(2000, 100));

		await editor.PressAsync("@");
		Assert.That(() => dropForm.IsVisibleAsync(), Is.True.After(2000, 100));

		await editor.EvaluateAsync("e => e.scrollTop = 0");
		Assert.That(() => dropForm.IsVisibleAsync(), Is.False.After(2000, 100));
	}

	async Task<(IPage, ILocator)> GetInitializedObjectsAsync(InMemoryAppServerTestContext ctx)
	{
		var autocomplete = default(ZAutoCompleteTextBox);
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new ZForm { Width = 200, Height = 60 };
			autocomplete = new ZAutoCompleteTextBox { AutocompleteManager = new FruitsField() };
			autocomplete.Dock = DockStyle.Fill;
			form.Controls.Add(autocomplete);
			return form;
		});
		var editor = page.Locator(".richtextbox__editoranchor");
		await editor.WaitForAsync();
		await page.WaitForFunctionAsync("editor => editor.autoCompleteInitialized", await editor.ElementHandleAsync(), new () { PollingInterval = 100, Timeout = 1000 });
		return (page, editor);
	}

	async Task WaitForDropFormItemAsync(ILocator dropForm, string code)
	{
		if (!string.IsNullOrEmpty(code))
		{
			await dropForm.Locator(".zdropform__item:first-child > td").GetByText(code).WaitForAsync();
		}
	}
}
