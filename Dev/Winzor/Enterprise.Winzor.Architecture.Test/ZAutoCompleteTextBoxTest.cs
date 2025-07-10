using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.Playwright;
using Moq;
using NUnit.Framework;
using WTG.PlaywrightTesting;

namespace Enterprise.Winzor.Architecture.Test;

class ZAutoCompleteTextBoxTest
{
	#region CW1 Tests

#pragma warning disable CW1050 // allow declaring as int instead of TimeSpan
	readonly int delayInMilliseconds = 3000;
#pragma warning restore CW1050
	[Test, WithPlaywrightPage]
	public async Task TestAtTwiceDoesntReshowForm()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var (page, editor, autocomplete) = await GetInitializedObjectsAsync(ctx);

		await editor.PressAsync("@");
		var original = default(ZDropForm);
		Assert.That(() => original = autocomplete.DropForm_Exposed, Is.Not.Null.After(delayInMilliseconds, 100), "PRE: Drop form exists");
		Assert.That(() => page.Locator(".zdropform").IsVisibleAsync(), Is.True.After(delayInMilliseconds, 100));

		await editor.PressAsync("@");
		Assert.That(() => autocomplete.DropForm_Exposed, Is.Not.Null.After(delayInMilliseconds, 100), "Form still open");
		Assert.That(() => page.Locator(".zdropform").IsVisibleAsync(), Is.True.After(delayInMilliseconds, 100));
		Assert.That(() => original.IsDisposed, Is.True.After(delayInMilliseconds, 100), "Original should be no longer visible");
	}

	[Test, WithPlaywrightPage]
	public async Task TestHittingEnterWhenThereIsNoItemThere()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var (page, editor, autocomplete) = await GetInitializedObjectsAsync(ctx);

		var eventFired = false;
		autocomplete.ItemSelected += (s, e) => eventFired = true;

		await editor.PressAsync("@");
		await editor.PressAsync("f");
		await editor.PressAsync("f");
		await editor.PressAsync("f");
		await editor.PressAsync("Enter");

		await autocomplete.InvokeWinzorDispatcherAsync(() => Application.DoEvents());

		Assert.That(eventFired, Is.False, "There was nothing valid to select so the event should not fire");
	}

	[Test, WithPlaywrightPage, ExpectNoExceptions]
	public async Task TestSelectAllWhenThereIsOnlyAtSymbol()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var (page, editor, autocomplete) = await GetInitializedObjectsAsync(ctx);

		await editor.PressAsync("@");
		await editor.PressAsync("Control+A");
		await autocomplete.InvokeWinzorDispatcherAsync(() => Application.DoEvents());
	}

	[Test, WithPlaywrightPage]
	public async Task TestSelectHighlightedOrFirstItem()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var (page, editor, autocomplete) = await GetInitializedObjectsAsync(ctx);

		await editor.PressAsync("@");
		Assert.That(await editor.InnerTextAsync(), Is.EqualTo("@"), "Pre-Condition");

		await autocomplete.InvokeWinzorDispatcherAsync(() => autocomplete.SelectHighlightedOrFirstItem());
		Assert.That(() => editor.InnerTextAsync(), Is.EqualTo("@Apple").After(delayInMilliseconds, 100), "Should select the first item");

		await editor.PressAsync("Control+A");
		await editor.PressAsync("Delete");

		await editor.PressAsync("@");
		Assert.That(await editor.InnerTextAsync(), Is.EqualTo("@"), "Pre-Condition");

		var banana = new FruitsField().GetList("Banana")[0] as ICodeDescription;
		await autocomplete.InvokeWinzorDispatcherAsync(() => autocomplete.SelectHighlightedOrFirstItem(banana));
		Assert.That(() => editor.InnerTextAsync(), Is.EqualTo("@Banana").After(delayInMilliseconds, 100), "Should select the first item");
	}

	[Test, WithPlaywrightPage]
	public async Task TestCloseWhenFocusLost()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var autocomplete = default(ZAutoCompleteTextBox);
		var somethingElse = default(ZTextBox);
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new ZForm();
			autocomplete = new ZAutoCompleteTextBox() { AutocompleteManager = new FruitsField() };
			autocomplete.Dock = DockStyle.Fill;
			form.Controls.Add(autocomplete);
			form.Controls.Add(somethingElse = new ZTextBox());
			return form;
		});
		var editor = await GetInitializedEditorAsync(page);

		await autocomplete.InvokeWinzorDispatcherAsync(() => autocomplete.Focus());
		await editor.PressAsync("@");
		Assert.That(() => autocomplete.DropForm_Exposed, Is.Not.Null.After(delayInMilliseconds, 100));
		Assert.That(() => autocomplete.DropForm_Exposed.Visible, Is.True.After(delayInMilliseconds, 100), "Form should be open");
		Assert.That(() => page.Locator(".zdropform").IsVisibleAsync(), Is.True.After(delayInMilliseconds, 100), "Form should be open");

		await autocomplete.InvokeWinzorDispatcherAsync(() => somethingElse.Focus());
		Assert.That(() => autocomplete.DropForm_Exposed, Is.Null.After(delayInMilliseconds, 100), "Form should be closed when we lose focus");
		Assert.That(() => page.Locator(".zdropform").IsVisibleAsync(), Is.False.After(delayInMilliseconds, 100), "Form should be closed when we lose focus");
	}

	[Test, WithPlaywrightPage]
	public async Task TestArrowKeys()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var (page, editor, autocomplete) = await GetInitializedObjectsAsync(ctx);

		await editor.PressAsync("@");
		Assert.That(() => autocomplete.DropForm_Exposed, Is.Not.Null.After(delayInMilliseconds, 100));
		Assert.That(() => autocomplete.DropForm_Exposed.Visible, Is.True.After(delayInMilliseconds, 100), "PRE: Drop form is shown");
		Assert.That(() => page.Locator(".zdropform").IsVisibleAsync(), Is.True.After(delayInMilliseconds, 100), "PRE: Drop form is shown");

		await editor.PressAsync("ArrowDown");
		Assert.That(() => autocomplete.DropForm_Exposed, Is.Not.Null.After(delayInMilliseconds, 100));
		Assert.That(() => autocomplete.DropForm_Exposed.HighlightedItem_Exposed, Is.EqualTo(0).After(delayInMilliseconds, 100), "Arrow should move selectedIndex");
		Assert.That(() => page.Locator(".zdropform__item:nth-child(1) > td").GetAttributeAsync("class"), Is.EqualTo("selected__description").After(delayInMilliseconds, 100), "Arrow should move selectedIndex");

		await editor.PressAsync("ArrowDown");
		Assert.That(() => autocomplete.DropForm_Exposed, Is.Not.Null.After(delayInMilliseconds, 100));
		Assert.That(() => autocomplete.DropForm_Exposed.HighlightedItem_Exposed, Is.EqualTo(1).After(delayInMilliseconds, 100), "Arrow should move selectedIndex");
		Assert.That(() => page.Locator(".zdropform__item:nth-child(2) > td").GetAttributeAsync("class"), Is.EqualTo("selected__description").After(delayInMilliseconds, 100), "Arrow should move selectedIndex");

		await editor.PressAsync("ArrowUp");
		Assert.That(() => autocomplete.DropForm_Exposed, Is.Not.Null.After(delayInMilliseconds, 100));
		Assert.That(() => autocomplete.DropForm_Exposed.HighlightedItem_Exposed, Is.EqualTo(0).After(delayInMilliseconds, 100), "Arrow should move selectedIndex");
		Assert.That(() => page.Locator(".zdropform__item:nth-child(1) > td").GetAttributeAsync("class"), Is.EqualTo("selected__description").After(delayInMilliseconds, 100), "Arrow should move selectedIndex");

		await editor.PressAsync("ArrowUp");
		Assert.That(() => autocomplete.DropForm_Exposed, Is.Not.Null.After(delayInMilliseconds, 100));
		Assert.That(() => autocomplete.DropForm_Exposed.HighlightedItem_Exposed, Is.EqualTo(0).After(delayInMilliseconds, 100), "Should not be able to move beyond zero");
		Assert.That(() => page.Locator(".zdropform__item:nth-child(1) > td").GetAttributeAsync("class"), Is.EqualTo("selected__description").After(delayInMilliseconds, 100), "Should not be able to move beyond zero");

		await WaitForDropFormItemAsync(page, "Apple");
		await editor.PressAsync("Enter");
		Assert.That(() => autocomplete.DropForm_Exposed, Is.Null.After(delayInMilliseconds, 100), "Should close the form");
		Assert.That(() => page.Locator(".zdropform").IsVisibleAsync(), Is.False.After(delayInMilliseconds, 100), "Should close the form");

		Assert.That(() => editor.InnerTextAsync(), Is.EqualTo("@Apple").After(delayInMilliseconds, 100), "Should have selected the text");
	}

	[Test, WithPlaywrightPage]
	public async Task TestCloseWhenNavigating()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var (page, editor, autocomplete) = await GetInitializedObjectsAsync(ctx);

		var keys = new[] { "Home", "End", "Escape", "ArrowLeft", "ArrowRight" };
		foreach (var key in keys)
		{
			await editor.ClearAsync();
			await editor.PressAsync("@");
			Assert.That(() => autocomplete.DropForm_Exposed, Is.Not.Null.After(delayInMilliseconds, 100));
			Assert.That(() => autocomplete.DropForm_Exposed.Visible, Is.True.After(delayInMilliseconds, 100), "Form should be open");
			Assert.That(() => page.Locator(".zdropform").IsVisibleAsync(), Is.True.After(delayInMilliseconds, 100), "Form should be open");

			await editor.PressAsync(key);
			Assert.That(() => autocomplete.DropForm_Exposed, Is.Null.After(delayInMilliseconds, 100), "Drop Form should be closed when we press " + key);
			Assert.That(() => page.Locator(".zdropform").IsVisibleAsync(), Is.False.After(delayInMilliseconds, 100), "Drop Form should be closed when we press " + key);
			Assert.That(() => editor.InnerTextAsync(), Is.EqualTo("@").After(delayInMilliseconds, 100), "Should not have effected the text " + key);
		}
	}

	[Test, WithPlaywrightPage]
	public async Task TestAutocompleteHasItems()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var (page, editor, autocomplete) = await GetInitializedObjectsAsync(ctx);
		var fruits = new FruitsField();

		await editor.PressAsync("@");
		Assert.That(() => page.Locator(".zdropform").IsVisibleAsync(), Is.True.After(delayInMilliseconds, 100));
		var expectedList = fruits.GetList(string.Empty).OfType<ICodeDescription>().Select(f => f.Code);
		Assert.That(() => page.Locator(".zdropform__item > td").AllTextContentsAsync(), Is.EquivalentTo(expectedList).After(delayInMilliseconds, 100));

		await editor.PressAsync("a");
		expectedList = fruits.GetList("a").OfType<ICodeDescription>().Select(f => f.Code);
		Assert.That(() => page.Locator(".zdropform__item > td").AllTextContentsAsync(), Is.EquivalentTo(expectedList).After(delayInMilliseconds, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task TestTwoAtSymbols()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var (page, editor, autocomplete) = await GetInitializedObjectsAsync(ctx);

		await editor.PressAsync("@");
		await editor.PressAsync("@");
		Assert.That(() => editor.InnerTextAsync(), Is.EqualTo("@@").After(delayInMilliseconds, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task TestFormIsClosedOnItemSelected()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var (page, editor, autocomplete) = await GetInitializedObjectsAsync(ctx);

		await editor.PressAsync("@");
		await WaitForDropFormItemAsync(page, "Apple");
		await editor.PressAsync("Enter");

		Assert.That(() => autocomplete.DropForm_Exposed, Is.Null.After(delayInMilliseconds, 100), "Form should be closed on select");
		Assert.That(() => page.Locator(".zdropform").IsVisibleAsync(), Is.False.After(delayInMilliseconds, 100), "Form should be closed on select");
		Assert.That(() => editor.InnerTextAsync(), Is.EqualTo("@Apple").After(delayInMilliseconds, 100), "We actually selected the value on our end");
	}

	[Test, WithPlaywrightPage]
	public async Task TestBackSpace()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var (page, editor, autocomplete) = await GetInitializedObjectsAsync(ctx);
		var fruits = new FruitsField();

		await editor.PressAsync("@");
		await editor.PressAsync("a");
		await editor.PressAsync("p");

		var expectedList = fruits.GetList("ap").OfType<ICodeDescription>().Select(f => f.Code);
		Assert.That(() => page.Locator(".zdropform__item > td").AllTextContentsAsync(), Is.EquivalentTo(expectedList).After(delayInMilliseconds, 100));

		await editor.PressAsync("Backspace");
		expectedList = fruits.GetList("a").OfType<ICodeDescription>().Select(f => f.Code);
		Assert.That(() => page.Locator(".zdropform__item > td").AllTextContentsAsync(), Is.EquivalentTo(expectedList).After(delayInMilliseconds, 100));

		await editor.PressAsync("Backspace");
		expectedList = fruits.GetList(string.Empty).OfType<ICodeDescription>().Select(f => f.Code);
		Assert.That(() => page.Locator(".zdropform__item > td").AllTextContentsAsync(), Is.EquivalentTo(expectedList).After(delayInMilliseconds, 100));

		await editor.PressAsync("Backspace");
		Assert.That(() => autocomplete.DropForm_Exposed, Is.Null.After(delayInMilliseconds, 100), "Form should now be hidden, we backspaced the @ symbol");
		Assert.That(() => page.Locator(".zdropform").IsVisibleAsync(), Is.False.After(delayInMilliseconds, 100), "Form should now be hidden, we backspaced the @ symbol");
	}

	[Test, WithPlaywrightPage]
	public async Task TestDoesNotAutoCompleteOnWhiteSpace()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var (page, editor, autocomplete) = await GetInitializedObjectsAsync(ctx);

		var expectedOutput = "I like @Apple and @Banana";
		await editor.PressSequentiallyAsync("I like @ap");
		await WaitForDropFormItemAsync(page, "Apple");
		await editor.PressAsync("Tab");

		await editor.PressSequentiallyAsync(" and @ba");
		await WaitForDropFormItemAsync(page, "Banana");
		await editor.PressAsync("Enter");
		Assert.That(() => editor.InnerTextAsync(), Is.EqualTo(expectedOutput).After(delayInMilliseconds, 100));

		await editor.ClearAsync();

		var expectedOutput1 = "I like @ap  and @Banana";
		await editor.PressSequentiallyAsync("I like @ap  and @ba");
		await WaitForDropFormItemAsync(page, "Banana");
		await editor.PressAsync("Enter");
		Assert.That(() => editor.InnerTextAsync(), Is.EqualTo(expectedOutput1).After(delayInMilliseconds, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task TestAutocompleteDoesntDieOnItemNotInList()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var (page, editor, autocomplete) = await GetInitializedObjectsAsync(ctx);

		var textToType = "Here is some @baaaaaad stuff";
		await editor.PressSequentiallyAsync(textToType);
		Assert.That(await editor.InnerTextAsync(), Is.EqualTo(textToType));
	}

	[Test, WithPlaywrightPage]
	public async Task TestStartAndEnd()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var (page, editor, autocomplete) = await GetInitializedObjectsAsync(ctx);

		var expectedOutput = "@Apple and @Banana";

		await editor.PressSequentiallyAsync("@Ap");
		await WaitForDropFormItemAsync(page, "Apple");
		await editor.PressAsync("Tab");

		await editor.PressSequentiallyAsync(" and @ba");
		await WaitForDropFormItemAsync(page, "Banana");
		await editor.PressAsync("Enter");

		Assert.That(() => editor.InnerTextAsync(), Is.EqualTo(expectedOutput).After(delayInMilliseconds, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task TestWeDontLoseFocusWhenPopupIsShown()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var (page, editor, autocomplete) = await GetInitializedObjectsAsync(ctx);

		await editor.PressAsync("@");
		Assert.That(() => autocomplete.DropForm_Exposed, Is.Not.Null.After(delayInMilliseconds, 100));
		Assert.That(autocomplete.DropForm_Exposed.ContainsFocus, Is.False, "Drop down shouldn't have focus");
		Assert.That(autocomplete.ContainsFocus, Is.True, "Autocomplete should have focus");
	}

	[Test, WithPlaywrightPage]
	public async Task TestFormIsShownWhenAtSymbolIsPressed()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var (page, editor, autocomplete) = await GetInitializedObjectsAsync(ctx);

		await editor.PressAsync("h");
		await editor.PressAsync("i");
		Assert.That(autocomplete.DropForm_Exposed, Is.Null, "Form should not be visible");
		Assert.That(await page.Locator(".zdropform").IsVisibleAsync(), Is.False, "Form should not be visible");

		await editor.PressAsync(" ");
		await editor.PressAsync("@");
		Assert.That(() => autocomplete.DropForm_Exposed, Is.Not.Null.After(delayInMilliseconds, 100), "Form should be visible");
		Assert.That(() => page.Locator(".zdropform").IsVisibleAsync(), Is.True.After(delayInMilliseconds, 100), "Form should be visible");

		await editor.PressAsync("m");
		await editor.PressAsync("u");
		Assert.That(() => autocomplete.DropForm_Exposed, Is.Not.Null.After(delayInMilliseconds, 100), "Form should still be visible");
		Assert.That(() => page.Locator(".zdropform").IsVisibleAsync(), Is.True.After(delayInMilliseconds, 100), "Form should still be visible");

		await editor.PressAsync(" ");
		Assert.That(() => autocomplete.DropForm_Exposed, Is.Null.After(delayInMilliseconds, 100), "Form should no longer be visible");
		Assert.That(() => page.Locator(".zdropform").IsVisibleAsync(), Is.False.After(delayInMilliseconds, 100), "Form should no longer be visible");
	}

	[Test, WithPlaywrightPage]
	public async Task TestFormAutosizesProperly()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var (page, editor, autocomplete) = await GetInitializedObjectsAsync(ctx);

		await editor.PressAsync("@");
		Assert.That(() => autocomplete.DropForm_Exposed, Is.Not.Null.After(delayInMilliseconds, 100));

		var previousHeight = autocomplete.DropForm_Exposed.Height;
		await editor.PressAsync("a");
		Assert.That(autocomplete.DropForm_Exposed, Is.Not.Null);
		Assert.That(() => autocomplete.DropForm_Exposed.Height, Is.LessThan(previousHeight).After(delayInMilliseconds, 100), "Height should have shrunk because we have less items now");
	}

	[Test, WithPlaywrightPage]
	public async Task TestDeletesTagProperlyAtEnd()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var (page, editor, autocomplete) = await GetInitializedObjectsAsync(ctx);

		await editor.PressSequentiallyAsync("@ap");
		await WaitForDropFormItemAsync(page, "Apple");
		await editor.PressAsync("Enter");
		Assert.That(() => editor.InnerTextAsync(), Is.EqualTo("@Apple").After(delayInMilliseconds, 100));

		await editor.PressAsync(" ");
		await editor.PressAsync("Backspace");
		await editor.PressAsync("Backspace");
		Assert.That(() => editor.InnerTextAsync(), Is.EqualTo(string.Empty).After(delayInMilliseconds, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task TestDeletesTagProperlyInside()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var (page, editor, autocomplete) = await GetInitializedObjectsAsync(ctx);

		await editor.PressSequentiallyAsync("@ap");
		await WaitForDropFormItemAsync(page, "Apple");
		await editor.PressAsync("Enter");
		Assert.That(() => editor.InnerTextAsync(), Is.EqualTo("@Apple").After(delayInMilliseconds, 100));

		await editor.PressAsync(" ");
		await SetSelectionAsync(page, editor, 3, 0);
		await editor.PressAsync("Backspace");
		Assert.That(() => editor.InnerTextAsync(), Is.EqualTo(" ").After(delayInMilliseconds, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task TestDoesNotDeleteTagJustBefore()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var (page, editor, autocomplete) = await GetInitializedObjectsAsync(ctx);

		await editor.PressSequentiallyAsync("a@ap");
		await WaitForDropFormItemAsync(page, "Apple");
		await editor.PressAsync("Enter");
		Assert.That(() => editor.InnerTextAsync(), Is.EqualTo("a@Apple").After(delayInMilliseconds, 100));

		await editor.PressAsync(" ");
		await SetSelectionAsync(page, editor, 1, 0);
		await editor.PressAsync("Backspace");
		Assert.That(() => editor.InnerTextAsync(), Is.EqualTo("@Apple ").After(delayInMilliseconds, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task TestDeletesTagAmpersand()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var (page, editor, autocomplete) = await GetInitializedObjectsAsync(ctx);
		await editor.PressSequentiallyAsync("a@ap");
		await WaitForDropFormItemAsync(page, "Apple");
		await editor.PressAsync("Enter");
		Assert.That(() => editor.InnerTextAsync(), Is.EqualTo("a@Apple").After(delayInMilliseconds, 100));

		await editor.PressAsync(" ");
		await SetSelectionAsync(page, editor, 2, 0);
		await editor.PressAsync("Backspace");
		Assert.That(() => editor.InnerTextAsync(), Is.EqualTo("a ").After(delayInMilliseconds, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task TestDeletesTagDeleteKeyAtStart()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var (page, editor, autocomplete) = await GetInitializedObjectsAsync(ctx);

		await editor.PressSequentiallyAsync("a@ap");
		await WaitForDropFormItemAsync(page, "Apple");
		await page.Locator(".zdropform__item:first-child>td").IsVisibleAsync();
		await editor.PressAsync("Enter", new() { Delay = 50 });
		Assert.That(() => editor.InnerTextAsync(), Is.EqualTo("a@Apple").After(delayInMilliseconds, 100));

		await editor.PressAsync(" ");
		await SetSelectionAsync(page, editor, 2, 0);
		await editor.PressAsync("Delete", new() { Delay = 50 });
		Assert.That(() => editor.InnerTextAsync(), Is.EqualTo("a ").After(delayInMilliseconds, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task TestDoesNotDeleteTagDeleteKeyAtEnd()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var (page, editor, autocomplete) = await GetInitializedObjectsAsync(ctx);

		await editor.PressSequentiallyAsync("a@ap");
		await WaitForDropFormItemAsync(page, "Apple");
		await editor.PressAsync("Enter");
		Assert.That(() => editor.InnerTextAsync(), Is.EqualTo("a@Apple").After(delayInMilliseconds, 100));

		await editor.PressSequentiallyAsync(" ap");
		await SetSelectionAsync(page, editor, 7, 0);
		await editor.PressAsync("Delete");
		Assert.That(() => editor.InnerTextAsync(), Is.EqualTo("a@Appleap").After(delayInMilliseconds, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task TestDeletesTagHighlightingInside()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var (page, editor, autocomplete) = await GetInitializedObjectsAsync(ctx);

		await editor.PressSequentiallyAsync("a@ap");
		await WaitForDropFormItemAsync(page, "Apple");
		await page.Locator(".zdropform__item:first-child>td").IsVisibleAsync();
		await editor.PressAsync("Enter", new() { Delay = 50 });
		Assert.That(() => editor.InnerTextAsync(), Is.EqualTo("a@Apple").After(delayInMilliseconds, 100));

		await editor.PressAsync(" ");
		await SetSelectionAsync(page, editor, 3, 2);
		await editor.PressAsync("Backspace", new() { Delay = 50 });
		Assert.That(() => editor.InnerTextAsync(), Is.EqualTo("a ").After(delayInMilliseconds, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task TestDeletesTagHighlightingPartTextOnRight()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var (page, editor, autocomplete) = await GetInitializedObjectsAsync(ctx);

		await editor.PressSequentiallyAsync("a@ap");
		await WaitForDropFormItemAsync(page, "Apple");
		await editor.PressAsync("Enter");
		Assert.That(() => editor.InnerTextAsync(), Is.EqualTo("a@Apple").After(delayInMilliseconds, 100));

		await editor.PressSequentiallyAsync(" cp");
		await SetSelectionAsync(page, editor, 4, 5);
		await editor.PressAsync("Backspace");
		Assert.That(() => editor.InnerTextAsync(), Is.EqualTo("ap").After(delayInMilliseconds, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task TestDeletesTagHighlightingPartTextOnLeft()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var (page, editor, autocomplete) = await GetInitializedObjectsAsync(ctx);

		await editor.PressSequentiallyAsync("ac@ap");
		await WaitForDropFormItemAsync(page, "Apple");
		await editor.PressAsync("Enter");
		Assert.That(() => editor.InnerTextAsync(), Is.EqualTo("ac@Apple").After(delayInMilliseconds, 100));

		await editor.PressSequentiallyAsync(" p");
		await SetSelectionAsync(page, editor, 1, 4);
		await editor.PressAsync("Backspace");
		Assert.That(() => editor.InnerTextAsync(), Is.EqualTo("a p").After(delayInMilliseconds, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task TestDeletesTagsHighlightingTwoTags()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var (page, editor, autocomplete) = await GetInitializedObjectsAsync(ctx);

		await GenerateTextAsync(page, editor);
		await SetSelectionAsync(page, editor, 5, 9);
		await editor.PressAsync("Backspace");
		Assert.That(() => editor.InnerTextAsync(), Is.EqualTo("ac qr").After(delayInMilliseconds, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task TestDoesNotDeleteTagsHighlightingBetweenTwoTags()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var (page, editor, autocomplete) = await GetInitializedObjectsAsync(ctx);

		await GenerateTextAsync(page, editor);
		await SetSelectionAsync(page, editor, 8, 3);
		await editor.PressAsync("Backspace");
		Assert.That(() => editor.InnerTextAsync(), Is.EqualTo("ac@Apple@Banana qr").After(delayInMilliseconds, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task TestKeyDownChangingSelectionStart()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var autocomplete = default(ZAutoCompleteTextBox);
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new ZForm();
			autocomplete = new ZAutoCompleteTextBox() { AutocompleteManager = new FruitsField() };
			autocomplete.Dock = DockStyle.Fill;
			autocomplete.HideSelection = false;
			autocomplete.Text = "Here is some text so that we have something to select";
			form.Controls.Add(autocomplete);
			return form;
		});
		var editor = await GetInitializedEditorAsync(page);

		async Task SelectWordBackwardsAsync() => await editor.PressAsync("Control+Shift+ArrowLeft");
		async Task<string> GetSelectedTextAsync() => await editor.EvaluateAsync<string>("window.getSelection().toString()");

		var start = autocomplete.Text.IndexOf("something") - 1;
		await SetSelectionAsync(page, editor, start, 0);

		await SelectWordBackwardsAsync();
		Assert.That(GetSelectedTextAsync, Is.EqualTo("have"));

		await SelectWordBackwardsAsync();
		Assert.That(GetSelectedTextAsync, Is.EqualTo("we have"));

		await SelectWordBackwardsAsync();
		Assert.That(GetSelectedTextAsync, Is.EqualTo("that we have"));
	}

	[Test, WithPlaywrightPage]
	public async Task TestDistinctTags()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var (page, editor, autocomplete) = await GetInitializedObjectsAsync(ctx);

		await GenerateTextAsync(page, editor);
		await GenerateAdditionalDuplicateTextAsync(page, editor);

		Assert.That(async () => (await GetDistinctTagsAsync(editor)).Count(t => t == "Apple"), Is.EqualTo(1).After(delayInMilliseconds, 100));
		Assert.That(async () => (await GetDistinctTagsAsync(editor)).Count(t => t == "Banana"), Is.EqualTo(1).After(delayInMilliseconds, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task TestDistinctTagsAfterDelete()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var (page, editor, autocomplete) = await GetInitializedObjectsAsync(ctx);

		await GenerateTextAsync(page, editor);
		await GenerateAdditionalDuplicateTextAsync(page, editor);

		await SetSelectionAsync(page, editor, 14, 0);
		await editor.PressAsync("Backspace");

		await SetSelectionAsync(page, editor, 4, 0);
		await editor.PressAsync("Backspace");

		Assert.That(async () => (await GetDistinctTagsAsync(editor)).Count(t => t == "Apple"), Is.EqualTo(1).After(delayInMilliseconds, 100));
		Assert.That(async () => (await GetDistinctTagsAsync(editor)).Count(t => t == "Banana"), Is.EqualTo(0).After(delayInMilliseconds, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task TestTags()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var (page, editor, autocomplete) = await GetInitializedObjectsAsync(ctx);

		await GenerateTextAsync(page, editor);
		await GenerateAdditionalDuplicateTextAsync(page, editor);

		Assert.That(async () => (await GetTagsAsync(editor)).Count(t => t == "Apple"), Is.EqualTo(2).After(delayInMilliseconds, 100));
		Assert.That(async () => (await GetTagsAsync(editor)).Count(t => t == "Banana"), Is.EqualTo(1).After(delayInMilliseconds, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task TestTagsAfterDelete()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var (page, editor, autocomplete) = await GetInitializedObjectsAsync(ctx);

		await GenerateTextAsync(page, editor);
		await GenerateAdditionalDuplicateTextAsync(page, editor);

		await SetSelectionAsync(page, editor, 4, 0);
		await editor.PressAsync("Backspace");

		Assert.That(async () => (await GetTagsAsync(editor)).Count(t => t == "Apple"), Is.EqualTo(1).After(delayInMilliseconds, 100));
		Assert.That(async () => (await GetTagsAsync(editor)).Count(t => t == "Banana"), Is.EqualTo(1).After(delayInMilliseconds, 100));
	}

	#endregion

	[Test, WithPlaywrightPage]
	public async Task TagsAreCorrectlyColored()
	{
		// Arrange
		await using var ctx = new InMemoryAppServerTestContext();
		var (page, editor, autocomplete) = await GetInitializedObjectsAsync(ctx);

		// Act
		await GenerateTextAsync(page, editor);
		await GenerateAdditionalDuplicateTextAsync(page, editor);

		// Assert
		var expectedInnerHtml = "<p>ac<span style=\"color: rgb(0, 0, 255);\">@Apple</span> np" +
			"<span style=\"color: rgb(0, 0, 255);\">@Banana</span> qr " +
			"<span style=\"color: rgb(0, 0, 255);\">@Apple </span></p>";
		Assert.That(() => editor.InnerHTMLAsync(), Is.EqualTo(expectedInnerHtml).After(delayInMilliseconds, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task DoesNotRemoveTagOnSelectionKeys([Values("Enter", "Tab")] string selectionKey)
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var (page, editor, autocomplete) = await GetInitializedObjectsAsync(ctx);
		var interop = new Mock<IAutoCompleteTextBoxJSInterop>();
		interop.Setup(i => i.InsertTagAndUpdateTagsAsync(It.IsAny<ElementReference>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>()));
		autocomplete.InteropForTest = interop.Object;

		await editor.PressSequentiallyAsync("@App");
		await WaitForDropFormItemAsync(page, "Apple");
		await editor.PressAsync(selectionKey);
		Assert.That(() => editor.InnerTextAsync(), Is.EqualTo("@App").After(100));
	}

	[Test, WithPlaywrightPage]
	public async Task DoesNotRemoveTagOnDropFormClick()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var (page, editor, autocomplete) = await GetInitializedObjectsAsync(ctx);
		var interop = new Mock<IAutoCompleteTextBoxJSInterop>();
		interop.Setup(i => i.InsertTagAndUpdateTagsAsync(It.IsAny<ElementReference>(), It.IsAny<string>(), It.IsAny<IEnumerable<string>>()));
		autocomplete.InteropForTest = interop.Object;

		await editor.PressSequentiallyAsync("@App");
		await WaitForDropFormItemAsync(page, "Apple");
		await page.Locator(".zdropform__item:first-child").ClickAsync();
		Assert.That(() => editor.InnerTextAsync(), Is.EqualTo("@App").After(100));
	}

	[Test, WithPlaywrightPage]
	public async Task WhitespaceRemainsIntact()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var (page, editor, autocomplete) = await GetInitializedObjectsAsync(ctx);
		await editor.EvaluateAsync("editor => editor.innerHTML = `<p>Times\nNew Roman</p>`;");
		Assert.That(await editor.InnerHTMLAsync(), Is.EqualTo("<p>Times<br>New Roman</p>"));
	}

	async Task<(IPage, ILocator, ZAutoCompleteTextBoxForTest)> GetInitializedObjectsAsync(InMemoryAppServerTestContext ctx)
	{
		var autocomplete = default(ZAutoCompleteTextBoxForTest);
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new ZForm();
			autocomplete = new ZAutoCompleteTextBoxForTest() { AutocompleteManager = new FruitsField() };
			autocomplete.Dock = DockStyle.Fill;
			form.Controls.Add(autocomplete);
			return form;
		});
		var editor = await GetInitializedEditorAsync(page);
		await autocomplete.InvokeWinzorDispatcherAsync(() => autocomplete.Focus());
		return (page, editor, autocomplete);
	}

	async Task<ILocator> GetInitializedEditorAsync(IPage page)
	{
		var editor = page.Locator(".richtextbox__editoranchor");
		await editor.WaitForAsync();
		await page.WaitForFunctionAsync("editor => editor.autoCompleteInitialized", await editor.ElementHandleAsync(), new () { PollingInterval = 100, Timeout = 1000 });
		return editor;
	}

	async Task WaitForDropFormItemAsync(IPage page, string code)
	{
		await page.Locator(".zdropform__item:first-child > td").GetByText(code).WaitForAsync();
	}

	async Task SetSelectionAsync(IPage page, ILocator editor, int start, int length)
	{
		await editor.FocusAsync();
		await editor.PressAsync("Home");
		while (start-- > 0)
		{
			await editor.PressAsync("ArrowRight");
		}
		if (length > 0)
		{
			await page.Keyboard.DownAsync("Shift");
			while (length-- > 0)
			{
				await editor.PressAsync("ArrowRight");
			}
			await page.Keyboard.UpAsync("Shift");
		}
	}

	async Task GenerateTextAsync(IPage page, ILocator editor)
	{
		var text = await editor.InnerTextAsync();
		await editor.PressSequentiallyAsync("ac@ap");
		await WaitForDropFormItemAsync(page, "Apple");
		await editor.PressAsync("Enter");
		Assert.That(() => editor.InnerTextAsync(), Is.EqualTo(text + "ac@Apple").After(delayInMilliseconds, 100));

		await editor.PressSequentiallyAsync(" np@ba");
		await WaitForDropFormItemAsync(page, "Banana");
		await editor.PressAsync("Enter");
		Assert.That(() => editor.InnerTextAsync(), Is.EqualTo(text + "ac@Apple np@Banana").After(delayInMilliseconds, 100));

		await editor.PressSequentiallyAsync(" qr");
		Assert.That(() => editor.InnerTextAsync(), Is.EqualTo(text + "ac@Apple np@Banana qr").After(delayInMilliseconds, 100));
	}

	async Task GenerateAdditionalDuplicateTextAsync(IPage page, ILocator editor)
	{
		var text = await editor.InnerTextAsync();
		await editor.PressSequentiallyAsync(" @ap");
		await WaitForDropFormItemAsync(page, "Apple");
		await editor.PressAsync("Enter");
		Assert.That(() => editor.InnerTextAsync(), Is.EqualTo(text + " @Apple").After(delayInMilliseconds, 100));

		await editor.PressAsync(" ");
		Assert.That(() => editor.InnerTextAsync(), Is.EqualTo(text + " @Apple ").After(delayInMilliseconds, 100));
	}

	async Task<string[]> GetDistinctTagsAsync(ILocator editor) => await editor.EvaluateAsync<string[]>("editor => editor.distinctTags");

	async Task<string[]> GetTagsAsync(ILocator editor) => await editor.EvaluateAsync<string[]>("editor => editor.tags");

	class ZAutoCompleteTextBoxForTest : ZAutoCompleteTextBox
	{
		public ZAutoCompleteTextBoxForTest() { }

		public IAutoCompleteTextBoxJSInterop InteropForTest { get; set; }

		public override IAutoCompleteTextBoxJSInterop Interop => InteropForTest ?? base.Interop;
	}
}
