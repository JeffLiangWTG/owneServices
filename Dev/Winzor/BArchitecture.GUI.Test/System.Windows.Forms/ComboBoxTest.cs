using System.Threading.Tasks;
using Bunit;
using Microsoft.Playwright;
using NUnit.Framework;
using WinzorFramework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace System.Windows.Forms;
public class ComboBoxTest
{
	[Test]
	public async Task ComboBoxHasCorrectSizeAndPosition()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new ComboBox());

		var comboBox = rendered.Find(".combobox");
		Assert.That(comboBox, Is.Not.Null);
	}

	[TestCase(ComboBoxStyle.DropDown)]
	[TestCase(ComboBoxStyle.DropDownList)]
	public async Task ComboBoxHasCorrectStyleClass(ComboBoxStyle comboBoxStyle)
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new ComboBox() { Height = 25, Width = 50, Top = 10, Left = 20, DropDownStyle = comboBoxStyle });

		var comboBox = rendered.Find(".combobox");
		Assert.That(comboBox.GetAttribute("style"), Is.EqualTo("position:absolute;width:50px;height:25px;top:10px;left:20px;background-color:var(--color-control);"));
	}

	[TestCase(ComboBoxStyle.DropDown)]
	[TestCase(ComboBoxStyle.DropDownList)]
	public async Task ComboBoxButtonTogglesDropdown(ComboBoxStyle comboBoxStyle)
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => ComboBoxForTest(comboBoxStyle: comboBoxStyle));

		var dropdown = rendered.FindAll(".combobox__dropdown");
		Assert.That(dropdown.Count, Is.EqualTo(0));

		var comboBoxButton = rendered.Find(".combobox__button");
		await comboBoxButton.ClickAsync(new WebMouseEventArgs());

		dropdown = rendered.FindAll(".combobox__dropdown");
		Assert.That(dropdown.Count, Is.EqualTo(1));

		await comboBoxButton.ClickAsync(new WebMouseEventArgs());

		dropdown = rendered.FindAll(".combobox__dropdown");
		Assert.That(dropdown.Count, Is.EqualTo(0));
	}

	[Test, WithPlaywrightPage]
	public async Task ComboBoxEnterKeyShouldHideDropDown()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => ComboBoxForTest(comboBoxStyle: ComboBoxStyle.DropDownList));

		var cbInner = await page.WaitForSelectorAsync(".combobox__button");
		await cbInner.ClickAsync();

		var dropdown = page.Locator(".combobox__dropdown");
		await Assertions.Expect(dropdown).ToBeVisibleAsync();
		await Assertions.Expect(dropdown).ToHaveCountAsync(1);

		await page.Keyboard.PressAsync("Enter");
		await Assertions.Expect(dropdown).ToBeHiddenAsync();
		await Assertions.Expect(dropdown).ToHaveCountAsync(0);
	}

	[TestCase(ComboBoxStyle.DropDown)]
	[TestCase(ComboBoxStyle.DropDownList)]
	public async Task ComboBoxDropdownDisplaysAllItems(ComboBoxStyle comboBoxStyle)
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => ComboBoxForTest(comboBoxStyle: comboBoxStyle));

		var comboBox = rendered.Find(".combobox");

		var comboBoxButton = rendered.Find(".combobox__button");
		await comboBoxButton.ClickAsync(new WebMouseEventArgs());

		var items = rendered.FindAll(".combobox__dropdown-item");
		Assert.That(items.Count, Is.EqualTo(4));
	}

	[TestCase(ComboBoxStyle.DropDown)]
	[TestCase(ComboBoxStyle.DropDownList)]
	public async Task ComboBoxDropdownItemClickSelectsItemAndClosesDropdown(ComboBoxStyle comboBoxStyle)
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => ComboBoxForTest(comboBoxStyle: comboBoxStyle));
		var form = (Form)rendered.Instance.Control;
		var comboBoxComponent = (ComboBox)form.Controls[0];

		var comboBox = rendered.Find(".combobox");

		var comboBoxButton = rendered.Find(".combobox__button");
		await comboBoxButton.ClickAsync(new WebMouseEventArgs());

		var items = rendered.FindAll(".combobox__dropdown-item");
		await items[1].MouseDownAsync(new WebMouseEventArgs());

		var dropdown = rendered.FindAll(".combobox__dropdown");
		Assert.That(dropdown.Count, Is.EqualTo(0));

		object selectedValue = null;
		await comboBoxComponent.InvokeWinzorDispatcherAsync(() =>
		{
			selectedValue = comboBoxComponent.SelectedItem;
		});
		Assert.That(selectedValue, Is.EqualTo(comboBoxItems[1]));
	}

	[Test]
	public async Task ComboBoxSuggestedItemsDropdownOpensOnKeyPress()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => ComboBoxForTest(AutoCompleteMode.Suggest));

		var comboBoxInput = rendered.Find(".combobox__input");
		await comboBoxInput.InputAsync(new Microsoft.AspNetCore.Components.ChangeEventArgs() { Value = "A" });

		var suggestions = rendered.FindAll(".combobox__suggestion");
		Assert.That(suggestions.Count, Is.EqualTo(1));
	}

	[Test]
	public async Task ComboBoxSuggestedItemsDropdownDisplaysFilteredResults()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => ComboBoxForTest(AutoCompleteMode.Suggest));

		var comboBox = rendered.Find(".combobox");

		var comboBoxInput = rendered.Find(".combobox__input");
		await comboBoxInput.InputAsync(new Microsoft.AspNetCore.Components.ChangeEventArgs() { Value = "A" });

		var suggestionItems = rendered.FindAll(".combobox__suggestion-item");
		Assert.That(suggestionItems.Count, Is.EqualTo(4));

		await comboBoxInput.InputAsync(new Microsoft.AspNetCore.Components.ChangeEventArgs() { Value = "AB" });

		suggestionItems = rendered.FindAll(".combobox__suggestion-item");
		Assert.That(suggestionItems.Count, Is.EqualTo(3));

		await comboBoxInput.InputAsync(new Microsoft.AspNetCore.Components.ChangeEventArgs() { Value = "ABC" });

		suggestionItems = rendered.FindAll(".combobox__suggestion-item");
		Assert.That(suggestionItems.Count, Is.EqualTo(2));

		await comboBoxInput.InputAsync(new Microsoft.AspNetCore.Components.ChangeEventArgs() { Value = "ABCZ" });

		suggestionItems = rendered.FindAll(".combobox__suggestion-item");
		Assert.That(suggestionItems.Count, Is.EqualTo(0));
		var suggestions = rendered.FindAll(".combobox__suggestion");
		Assert.That(suggestions.Count, Is.EqualTo(0));
	}

	[Test]
	public async Task ComboBoxSuggestedItemsDropdownHiddenWhenNoResults()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => ComboBoxForTest(AutoCompleteMode.Suggest));

		var comboBoxInput = rendered.Find(".combobox__input");
		await comboBoxInput.InputAsync(new Microsoft.AspNetCore.Components.ChangeEventArgs() { Value = "A" });

		var suggestions = rendered.FindAll(".combobox__suggestion");
		Assert.That(suggestions.Count, Is.EqualTo(1));
		var suggestionItems = rendered.FindAll(".combobox__suggestion-item");
		Assert.That(suggestionItems.Count, Is.EqualTo(4));

		await comboBoxInput.InputAsync(new Microsoft.AspNetCore.Components.ChangeEventArgs() { Value = "AZ" });

		suggestions = rendered.FindAll(".combobox__suggestion");
		Assert.That(suggestions.Count, Is.EqualTo(0));
	}

	[Test]
	public async Task ComboBoxSuggestedItemsDropdownClickSelectsItemAndClosesDropdown()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => ComboBoxForTest(AutoCompleteMode.Suggest));
		var form = (Form)rendered.Instance.Control;
		var comboBoxComponent = (ComboBox)form.Controls[0];

		var suggestions = rendered.FindAll(".combobox__suggestion");
		Assert.That(suggestions.Count, Is.EqualTo(0));

		var comboBoxInput = rendered.Find(".combobox__input");
		await comboBoxInput.InputAsync(new Microsoft.AspNetCore.Components.ChangeEventArgs() { Value = "A" });

		suggestions = rendered.FindAll(".combobox__suggestion");
		Assert.That(suggestions.Count, Is.EqualTo(1));
		var suggestionItems = rendered.FindAll(".combobox__suggestion-item");
		Assert.That(suggestionItems.Count, Is.EqualTo(4));
		await suggestionItems[1].MouseDownAsync(new WebMouseEventArgs());

		suggestions = rendered.FindAll(".combobox__suggestion");
		Assert.That(suggestions.Count, Is.EqualTo(0));

		object selectedValue = null;
		await comboBoxComponent.InvokeWinzorDispatcherAsync(() =>
		{
			selectedValue = comboBoxComponent.SelectedItem;
		});
		Assert.That(selectedValue, Is.EqualTo(comboBoxItems[1]));
	}

	[Test, WithPlaywrightPage]
	public async Task ComboBoxInlineAutoCompleteOnKeyPress()
	{
		await using var ctx = new InMemoryTestServerContext();

		ComboBox comboBox = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			comboBox = ComboBoxForTest(AutoCompleteMode.Append);
			form.Controls.Add(comboBox);
			return form;
		});

		await page.WaitForSelectorAsync(".combobox");
		var input = page.Locator("input").First;
		await input.ClickAsync();
		await page.Keyboard.PressAsync("A");
		await page.WaitForFunctionAsync("document.activeElement.value === 'AAA'");
		Assert.That(await input.EvaluateAsync<int>("i => i.selectionStart"), Is.EqualTo(1));
		Assert.That(await input.EvaluateAsync<int>("i => i.selectionEnd"), Is.EqualTo(3));
		await page.Keyboard.PressAsync("B");
		await page.WaitForFunctionAsync("document.activeElement.value === 'ABA'");
		Assert.That(await input.EvaluateAsync<int>("i => i.selectionStart"), Is.EqualTo(2));
		Assert.That(await input.EvaluateAsync<int>("i => i.selectionEnd"), Is.EqualTo(3));
		await page.Keyboard.PressAsync("C");
		await page.WaitForFunctionAsync("document.activeElement.value === 'ABCDEF'");
		Assert.That(await input.EvaluateAsync<int>("i => i.selectionStart"), Is.EqualTo(3));
		Assert.That(await input.EvaluateAsync<int>("i => i.selectionEnd"), Is.EqualTo(6));
	}

	[WithPlaywrightPage]
	[TestCase(ComboBoxStyle.DropDown)]
	[TestCase(ComboBoxStyle.DropDownList)]
	public async Task ComboBoxTextboxFocusedOnButtonClick(ComboBoxStyle comboBoxStyle)
	{
		await using var ctx = new InMemoryTestServerContext();

		ComboBox comboBox = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			comboBox = ComboBoxForTest(comboBoxStyle: comboBoxStyle);
			form.Controls.Add(comboBox);
			return form;
		});

		await page.WaitForSelectorAsync(".combobox");
		var button = page.Locator("button").First;
		await button.ClickAsync();
		var focusedInput = page.Locator(".combobox__input:focus");
		Assert.That(() => focusedInput.IsVisibleAsync(), Is.True.After(5000, 100));
	}

	[TestCase(ComboBoxStyle.DropDown)]
	[TestCase(ComboBoxStyle.DropDownList)]
	public async Task ComboBoxSelectedIndexChangeInvokesEvent(ComboBoxStyle comboBoxStyle)
	{
		using var ctx = new WinzorTestContext();
		var selectedIndexChangedEventInvoked = new TaskCompletionSource<bool>(false);

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var comboBox = ComboBoxForTest(comboBoxStyle: comboBoxStyle);
			comboBox.SelectedIndexChanged += (sender, eventArgs) => selectedIndexChangedEventInvoked.SetResult(true);
			return comboBox;
		});
		var form = (Form)rendered.Instance.Control;
		var comboBox = (ComboBox)form.Controls[0];

		await comboBox.InvokeWinzorDispatcherAsync(() =>
		{
			comboBox.SelectedIndex = 1;
		});

		Assert.That(await selectedIndexChangedEventInvoked.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);
	}

	static readonly string[] comboBoxItems = { "AAA", "ABA", "ABCDEF", "ABCD" };

	ComboBox ComboBoxForTest(AutoCompleteMode autocompleteMode = AutoCompleteMode.None, ComboBoxStyle comboBoxStyle = ComboBoxStyle.DropDown)
	{
		var comboBox = new ComboBox() { Width = 100, Height = 25, DropDownStyle = comboBoxStyle };
		comboBox.Items.AddRange(comboBoxItems);
		if (autocompleteMode != AutoCompleteMode.None)
		{
			comboBox.AutoCompleteMode = autocompleteMode;
			comboBox.AutoCompleteSource = AutoCompleteSource.ListItems;
		}
		return comboBox;
	}

	ComboBox ComboBoxLargeForTest(int count, AutoCompleteMode autocompleteMode = AutoCompleteMode.None, ComboBoxStyle comboBoxStyle = ComboBoxStyle.DropDown)
	{
		var comboBox = new ComboBox() { Width = 100, Height = 25, DropDownStyle = comboBoxStyle };
		string[] comboBoxItems = new string[count];
		for (int i = 0; i < count; i++)
		{
			comboBoxItems[i] = i.ToString();
		}
		comboBox.Items.AddRange(comboBoxItems);
		if (autocompleteMode != AutoCompleteMode.None)
		{
			comboBox.AutoCompleteMode = autocompleteMode;
			comboBox.AutoCompleteSource = AutoCompleteSource.ListItems;
		}
		return comboBox;
	}

	[TestCase(ComboBoxStyle.DropDown, 0, "AAA")]
	[TestCase(ComboBoxStyle.DropDownList, 1, "ABA")]
	public async Task ComboBoxShowsToolTipOfSelectedItemOnMouseOver(ComboBoxStyle comboBoxStyle, int selectedIndex, string expectedTitle)
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var comboBox = ComboBoxForTest(comboBoxStyle: comboBoxStyle);
			comboBox.SelectedIndex = selectedIndex;
			return comboBox;
		});
		Assert.That(rendered.Find(".combobox").GetAttribute("title"), Is.EqualTo(expectedTitle));
	}

	[TestCase(ComboBoxStyle.DropDown)]
	[TestCase(ComboBoxStyle.DropDownList)]
	public async Task ComboxBoxHasDisabled(ComboBoxStyle comboBoxStyle)
	{
		using var ctx = new WinzorTestContext();

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var comboBox = ComboBoxForTest(comboBoxStyle: comboBoxStyle);
			comboBox.Enabled = false;
			return comboBox;
		});
		var form = (Form)rendered.Instance.Control;
		var comboBoxControl = (ComboBox)form.Controls[0];

		Assert.That(comboBoxControl.Enabled, Is.False);
		var comboBox = rendered.Find(".combobox");
		var comboBoxButton = rendered.Find(".combobox__button");
		Assert.That(comboBox.GetAttribute("style"), Contains.Substring("background-color:var(--color-control);"));
		Assert.That(comboBox.GetAttribute("disabled"), Is.Not.Null);
		Assert.That(rendered.Find(".combobox__input").GetAttribute("disabled"), Is.Not.Null);
		Assert.That(comboBoxButton.GetAttribute("disabled"), Is.Not.Null);

		await comboBoxControl.InvokeWinzorDispatcherAsync(() =>
		{
			comboBoxControl.Enabled = true;
		});

		Assert.That(comboBoxControl.Enabled, Is.True);
		Assert.That(comboBox.GetAttribute("disabled"), Is.Null);
		Assert.That(rendered.Find(".combobox__input").GetAttribute("disabled"), Is.Null);
		Assert.That(comboBoxButton.GetAttribute("disabled"), Is.Null);
	}

	[WithPlaywrightPage]
	[TestCase(false, "0.8", "none", "content-box", "rgb(240, 240, 240)", "rgb(219, 219, 219)")]
	[TestCase(true, "1", "auto", "border-box", "rgb(255, 255, 255)", "rgb(122, 122, 122) rgb(0, 0, 0) rgb(122, 122, 122) rgb(122, 122, 122)")]
	public async Task ComboBoxHasDisabledStyleForInput(bool comboxEnabled, string expectedOpacity, string expectedPointerEvents, string expectedBackgroundClip, string expectedInputBackgroundColor, string expectedBorderColor)
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var comboBox = ComboBoxForTest();
			comboBox.Enabled = comboxEnabled;
			comboBox.SelectedIndex = 1;
			return comboBox;
		});

		var comboBoxInput = page.Locator(".combobox__input");

		Assert.That(await comboBoxInput.GetComputedStyleAsync("pointer-events"), Is.EqualTo(expectedPointerEvents));
		Assert.That(await comboBoxInput.GetComputedStyleAsync("background-color"), Is.EqualTo(expectedInputBackgroundColor));
		Assert.That(await comboBoxInput.GetComputedStyleAsync("background-clip"), Is.EqualTo(expectedBackgroundClip));
		Assert.That(await comboBoxInput.GetComputedStyleAsync("opacity"), Is.EqualTo(expectedOpacity));
		Assert.That((await comboBoxInput.GetComputedStyleAsync("border-color")).ToString(), Contains.Substring(expectedBorderColor));
	}

	[WithPlaywrightPage]
	[TestCase(false, "0.8", "none", "rgb(255, 255, 255)", "rgb(219, 219, 219)")]
	[TestCase(true, "1", "auto", "rgb(255, 255, 255)", "rgb(122, 122, 122) rgb(122, 122, 122) rgb(122, 122, 122) rgb(0, 0, 0)")]
	public async Task ComboBoxHasDisabledStyleForButton(bool comboxEnabled, string expectedOpacity, string expectedPointerEvents, string expectedButtonBackgroundColor, string expectedBorderColor)
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var comboBox = ComboBoxForTest();
			comboBox.Enabled = comboxEnabled;
			comboBox.SelectedIndex = 1;
			return comboBox;
		});

		var comboBoxButton = page.Locator(".combobox__button");

		Assert.That(await comboBoxButton.GetComputedStyleAsync("opacity"), Is.EqualTo(expectedOpacity));
		Assert.That(await comboBoxButton.GetComputedStyleAsync("pointer-events"), Is.EqualTo(expectedPointerEvents));
		Assert.That(await comboBoxButton.GetComputedStyleAsync("background-color"), Is.EqualTo(expectedButtonBackgroundColor));
		Assert.That((await comboBoxButton.GetComputedStyleAsync("border-color")).ToString(), Contains.Substring(expectedBorderColor));
	}

	[TestCase]
	public async Task ComboBoxFireActionWhenEnabledAsync()
	{
		using var ctx = new WinzorTestContext();

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var comboBox = ComboBoxForTest(comboBoxStyle: ComboBoxStyle.DropDown, autocompleteMode: AutoCompleteMode.Suggest);
			comboBox.Enabled = true;
			return comboBox;
		});

		var form = (Form)rendered.Instance.Control;
		var comboBoxControl = (ComboBox)form.Controls[0];

		var comboBoxInput = rendered.Find(".combobox__input");

		comboBoxInput.TriggerEvent("onwinzorfocusin", new WinzorFocusInEventArgs());
		var selected = ctx.JSInterop.VerifyInvoke("selectAll").Arguments;
		Assert.That(selected.Count, Is.Not.Zero);

		await comboBoxInput.InputAsync(new Microsoft.AspNetCore.Components.ChangeEventArgs() { Value = "A" });
		var suggestions = rendered.FindAll(".combobox__suggestion-item");
		Assert.That(suggestions.Count, Is.Not.Zero);

		var comboBoxButton = rendered.Find(".combobox__button");
		await comboBoxButton.ClickAsync(new WebMouseEventArgs());
		Assert.That(comboBoxControl.DroppedDown, Is.True);
	}

	[TestCase]
	public async Task ComboBoxOnInputFocusInDoesNotFireActionWhenDisabledAsync()
	{
		using var ctx = new WinzorTestContext();

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var comboBox = ComboBoxForTest(comboBoxStyle: ComboBoxStyle.DropDown);
			comboBox.Enabled = false;
			return comboBox;
		});

		var comboBoxInput = rendered.Find(".combobox__input");
		var isRemoved = comboBoxInput.RemoveAttribute("disabled");
		Assert.That(isRemoved, Is.True);

		comboBoxInput.TriggerEvent("onwinzorfocusin", new WinzorFocusInEventArgs());
		Assert.Throws<JSInvokeCountExpectedException>(() => ctx.JSInterop.VerifyInvoke("selectAll"));
	}

	[TestCase]
	public async Task ComboBoxOnInputDoesNotFireActionWhenDisabledAsync()
	{
		using var ctx = new WinzorTestContext();

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var comboBox = ComboBoxForTest(comboBoxStyle: ComboBoxStyle.DropDown, autocompleteMode: AutoCompleteMode.Suggest);
			comboBox.Enabled = false;
			return comboBox;
		});

		var form = (Form)rendered.Instance.Control;
		var comboBoxControl = (ComboBox)form.Controls[0];

		var comboBoxInput = rendered.Find(".combobox__input");
		var isRemoved = comboBoxInput.RemoveAttribute("disabled");
		Assert.That(isRemoved, Is.True);

		await comboBoxInput.InputAsync(new Microsoft.AspNetCore.Components.ChangeEventArgs() { Value = "A" });
		var suggestions = rendered.FindAll(".combobox__suggestion-item");
		Assert.That(suggestions.Count, Is.Zero);
	}

	[TestCase]
	public async Task ComboBoxOnDropdownButtonClickDoesNotFireActionWhenDisabledAsync()
	{
		using var ctx = new WinzorTestContext();

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var comboBox = ComboBoxForTest(comboBoxStyle: ComboBoxStyle.DropDown);
			comboBox.Enabled = false;
			return comboBox;
		});

		var form = (Form)rendered.Instance.Control;
		var comboBoxControl = (ComboBox)form.Controls[0];

		var dropdown = rendered.FindAll(".combobox__dropdown");
		Assert.That(dropdown.Count, Is.EqualTo(0));

		var comboBoxButton = rendered.Find(".combobox__button");
		var isRemoved = comboBoxButton.RemoveAttribute("disabled");
		Assert.That(isRemoved, Is.True);

		await comboBoxButton.ClickAsync(new WebMouseEventArgs());
		Assert.That(comboBoxControl.DroppedDown, Is.False);
	}

	[WithPlaywrightPage]
	[TestCase(ComboBoxStyle.DropDown, "rgb(122, 122, 122)")]
	public async Task ComboBoxInputBorderColorAndBorderRadiusTest(ComboBoxStyle comboBoxStyle, string borderColor)
	{
		await using var ctx = new InMemoryTestServerContext();
		ComboBox comboBox = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			comboBox = ComboBoxForTest(comboBoxStyle: comboBoxStyle);
			form.Controls.Add(comboBox);
			return form;
		});

		var comboboxInput = await page.WaitForSelectorAsync(".combobox__input");

		var getComputedStyle = (string p) => comboboxInput.EvaluateAsync<string>($"e => window.getComputedStyle(e).getPropertyValue('{p}')");

		Assert.That(await getComputedStyle("border-radius"), Is.EqualTo("0px"));
		Assert.That(await getComputedStyle("border-top-width"), Is.EqualTo("1px"));
		Assert.That(await getComputedStyle("border-left-width"), Is.EqualTo("1px"));
		Assert.That(await getComputedStyle("border-bottom-width"), Is.EqualTo("1px"));
		Assert.That(await getComputedStyle("border-right-width"), Is.EqualTo("0px"));
		Assert.That(await getComputedStyle("border-top-color"), Is.EqualTo(borderColor));
		Assert.That(await getComputedStyle("border-left-color"), Is.EqualTo(borderColor));
		Assert.That(await getComputedStyle("border-bottom-color"), Is.EqualTo(borderColor));
	}

	[WithPlaywrightPage]
	[TestCase(ComboBoxStyle.DropDown, "rgb(122, 122, 122)")]
	public async Task ComboBoxButtonBorderColorAndBorderRadiusTest(ComboBoxStyle comboBoxStyle, string borderColor)
	{
		await using var ctx = new InMemoryTestServerContext();

		ComboBox comboBox = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			comboBox = ComboBoxForTest(comboBoxStyle: comboBoxStyle);
			form.Controls.Add(comboBox);
			return form;
		});

		var comboboxButton = await page.WaitForSelectorAsync(".combobox__button");

		var getComputedStyle = (string p) => comboboxButton.EvaluateAsync<string>($"e => window.getComputedStyle(e).getPropertyValue('{p}')");

		Assert.That(await getComputedStyle("border-radius"), Is.EqualTo("0px"));
		Assert.That(await getComputedStyle("border-top-width"), Is.EqualTo("1px"));
		Assert.That(await getComputedStyle("border-left-width"), Is.EqualTo("0px"));
		Assert.That(await getComputedStyle("border-bottom-width"), Is.EqualTo("1px"));
		Assert.That(await getComputedStyle("border-right-width"), Is.EqualTo("1px"));
		Assert.That(await getComputedStyle("border-top-color"), Is.EqualTo(borderColor));
		Assert.That(await getComputedStyle("border-right-color"), Is.EqualTo(borderColor));
		Assert.That(await getComputedStyle("border-bottom-color"), Is.EqualTo(borderColor));
	}

	[Test]
	public async Task ComboBoxShouldNotSuggestWhenInputIsEmpty()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => ComboBoxForTest(AutoCompleteMode.Suggest));
		var comboBoxInput = rendered.Find(".combobox__input");

		await comboBoxInput.InputAsync(new Microsoft.AspNetCore.Components.ChangeEventArgs() { Value = string.Empty });

		Assert.That(rendered.FindAll(".combobox__suggestion").Count, Is.EqualTo(0));
	}

	[Test, WithPlaywrightPage]
	public async Task TestScrollToSelectedItem()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var comboBox = new ComboBox() { Width = 100, Height = 25 };
			var items = new string[100];
			for (var i = 0; i < items.Length; i++)
			{
				items[i] = $"item-{i}";
			}
			comboBox.Items.AddRange(items);
			comboBox.SelectedIndex = 30;

			var form = new Form() { Width = 300, Height = 300 };
			form.Controls.Add(comboBox);
			return form;
		});

		await page.WaitForSelectorAsync(".combobox");
		await page.Locator("input").First.ClickAsync();
		await (await page.WaitForSelectorAsync(".combobox__button")).ClickAsync();
		var selectedItem = await page.WaitForSelectorAsync(".combobox__dropdown-item--selected");

		Assert.That(async () => await selectedItem.EvaluateAsync<int>("e => e.getBoundingClientRect().top"), Is.EqualTo(25).Within(1).After(2000, 100));
		Assert.That(async () => await selectedItem.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('background-color')"), Is.EqualTo("rgb(0, 90, 185)").After(2000, 100));
		Assert.That(async () => await selectedItem.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('color')"), Is.EqualTo("rgb(255, 255, 255)").After(2000, 100));
	}

	[Test]
	public async Task ComboBoxWithDropDownListStyleHasDisabledTextBox()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var combo = ComboBoxForTest(comboBoxStyle: ComboBoxStyle.DropDownList);
			return combo;
		});

		var input = rendered.Find(".combobox__input");

		Assert.That(input.NodeName, Is.EqualTo("DIV"));
	}

	[Test, WithPlaywrightPage]
	public async Task ComboBoxWithDropDownListStyleActivatesDropDownOnClick()
	{
		await using var ctx = new InMemoryTestServerContext();
		var combo = default(ComboBox);
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form { Width = 500, Height = 500 };

			combo = ComboBoxForTest(comboBoxStyle: ComboBoxStyle.DropDownList);
			combo.Location = new Drawing.Point(10, 50);
			combo.SelectedIndex = 0;

			form.Controls.Add(combo);

			return form;
		});

		var cbInner = page.Locator(".combobox__input");
		await ElementMouseMoveWithDelayAsync(page, cbInner, 1, 1);
		await MouseDownWithDelayAsync(page);
		await MouseUpWithDelayAsync(page);

		var dropdown = page.Locator(".combobox__dropdown");
		Assert.That(() => dropdown.IsVisibleAsync(), Is.True.After(5000, 100), "Drop down is activated on click");

		await page.ClickAsync(".form");
		Assert.That(() => dropdown.IsVisibleAsync(), Is.False.After(5000, 100), "Drop down is dismissed on click away");
	}

	[Test]
	public async Task ComboBoxWithDropDownListStyleHasAlternateStyle()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var combo = ComboBoxForTest(comboBoxStyle: ComboBoxStyle.DropDownList);
			return combo;
		});

		var comboBox = rendered.Find(".combobox");

		var styles = comboBox.ClassList;
		Assert.That(styles.Contains("combobox--style-dropdownlist"), Is.True);
	}

	[Test]
	public async Task ComboBoxWithDropDownListStyle_CannotAssignText()
	{
		using var ctx = new WinzorTestContext();
		var combo = default(ComboBox);
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			combo = ComboBoxForTest(comboBoxStyle: ComboBoxStyle.DropDownList);
			return combo;
		});

		var text = default(string);
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			combo.Text = "Hello";

			text = combo.Text;
		});

		Assert.That(text, Is.EqualTo(string.Empty));
	}

	[WithPlaywrightPage]
	[TestCase(true, ComboBoxStyle.DropDown)]
	[TestCase(false, ComboBoxStyle.DropDown)]
	[TestCase(true, ComboBoxStyle.DropDownList)]
	[TestCase(false, ComboBoxStyle.DropDownList)]
	public async Task ComboBoxTabStop(bool tabStop, ComboBoxStyle comboBoxStyle)
	{
		await using var ctx = new InMemoryTestServerContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form { Width = 500, Height = 500 };
			var textBox1 = new TextBox() { Text = "First" };
			var comboBox = ComboBoxForTest(comboBoxStyle: comboBoxStyle);
			comboBox.SelectedIndex = 0;
			comboBox.Location = new Drawing.Point(0, 30);
			comboBox.TabStop = tabStop;
			var textBox2 = new TextBox { Text = "Second" };
			textBox2.Location = new Drawing.Point(0, 60);
			form.Controls.Add(textBox1);
			form.Controls.Add(comboBox);
			form.Controls.Add(textBox2);
			return form;
		});

		var isElementFocused = async (ILocator element) => (await element.EvaluateAsync("node => document.activeElement === node")).Value.GetBoolean();

		var textBoxLocator = page.Locator("input.textbox");
		var textBox1 = textBoxLocator.Nth(0);
		var textBox2 = textBoxLocator.Nth(1);

		await textBox1.ClickAsync();
		await page.Keyboard.PressAsync("Tab");

		if (tabStop)
		{
			var comboBoxLocator = page.Locator(".combobox__input");
			Assert.That(() => isElementFocused(comboBoxLocator), Is.True);
		}
		else
		{
			Assert.That(() => isElementFocused(textBox2), Is.True);
		}
	}

	[Test, WithPlaywrightPage]
	[TestCase(40, true, 106, 0, 392)]
	[TestCase(20, true, 106, 0, 262)]
	[TestCase(40, false, 106, 20, 262)]
	[TestCase(40, false, 107, 20, 175)]
	[TestCase(40, true, 106, 31, 405)]
	[TestCase(40, false, 200, 3, 202)]
	[TestCase(40, true, 1200, 3, 522)]
	public async Task ComboBoxDropdownHeight(int count, bool integralHeight, int dropDownHeight, int maxDropDownItems, int expectedHeight)
	{
		await using var ctx = new InMemoryTestServerContext();

		ComboBox comboBox = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form { Width = 300, Height = 1000 };
			comboBox = ComboBoxLargeForTest(count, comboBoxStyle: ComboBoxStyle.DropDownList);
			comboBox.DropDownHeight = dropDownHeight;
			comboBox.MaxDropDownItems = maxDropDownItems;
			comboBox.IntegralHeight = integralHeight;
			form.Controls.Add(comboBox);
			return form;
		});

		var comboboxButton = await page.WaitForSelectorAsync(".combobox__button");
		await comboboxButton.ClickAsync();

		var comboboxDropdown = await page.WaitForSelectorAsync(".combobox__dropdown");

		Assert.That(async () => await comboboxDropdown.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('height')"), Is.EqualTo($"{expectedHeight}px"));
	}

	[Test, WithPlaywrightPage(Headless = false)]
	[Explicit]
	public async Task ZZControlsDemo()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form { Width = 500, Height = 165 };

			var combo = ComboBoxForTest();
			combo.Location = new Drawing.Point(10, 50);
			combo.SelectedIndex = 0;

			var comboLabel = new Label { AutoSize = true, Text = "DropDownStyle = ComboBoxStyle.DropDown" };
			comboLabel.Location = new Drawing.Point(10, 25);

			var combo2 = ComboBoxForTest(comboBoxStyle: ComboBoxStyle.DropDownList);
			combo2.Location = new Drawing.Point(250, 50);
			combo2.SelectedIndex = 0;

			var combo2Label = new Label { AutoSize = true, Text = "DropDownStyle = ComboBoxStyle.DropDownList" };
			combo2Label.Location = new Drawing.Point(250, 25);

			var combo3 = ComboBoxForTest();
			combo3.Location = new Drawing.Point(10, 120);
			combo3.Enabled = false;
			combo3.SelectedIndex = 0;

			var combo3Label = new Label { AutoSize = true, Text = "Disabled" };
			combo3Label.Location = new Drawing.Point(10, 95);

			form.Controls.Add(combo);
			form.Controls.Add(comboLabel);
			form.Controls.Add(combo2);
			form.Controls.Add(combo2Label);
			form.Controls.Add(combo3);
			form.Controls.Add(combo3Label);

			return form;
		});

		while (!page.IsClosed)
		{
			await Task.Delay(1000);
		}
	}

	[Test, WithPlaywrightPage]
	public async Task ComboboxDropdownListHasBlueTransitionOnHover()
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			var comboBox = ComboBoxForTest(comboBoxStyle: ComboBoxStyle.DropDownList);
			form.Controls.Add(comboBox);
			return form;
		});

		await page.WaitForSelectorAsync(".combobox");
		var combobox = await page.WaitForSelectorAsync(".combobox");
		var comboboxInput = await page.WaitForSelectorAsync(".combobox__input");
		var comboboxButton = await page.WaitForSelectorAsync(".combobox__button");
		await combobox.HoverAsync();
		await Task.Delay(2000);
		Assert.Multiple(() =>
		{
			Assert.That(async () => await comboboxInput.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('border')"), Is.EqualTo("0px none rgb(0, 0, 0)"));
			Assert.That(async () => await comboboxButton.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('border')"), Is.EqualTo("0px none rgb(0, 0, 0)"));
			Assert.That(async () => await combobox.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('transition')"), Is.EqualTo("background-color 0.5s, border-color 0.5s"));
			Assert.That(async () => await combobox.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('border-color')"), Is.EqualTo("rgb(0, 120, 215)"));
			Assert.That(async () => await combobox.EvaluateAsync<string>("e => window.getComputedStyle(e).getPropertyValue('background-color')"), Is.EqualTo("rgb(229, 241, 251)"));
		});
	}

	[Test, WithPlaywrightPage]
	[TestCase(ComboBoxStyle.DropDown, 40, true, 106, 0)]
	[TestCase(ComboBoxStyle.DropDown, 20, true, 106, 0)]
	[TestCase(ComboBoxStyle.DropDown, 40, false, 106, 20)]
	[TestCase(ComboBoxStyle.DropDown, 40, false, 107, 20)]
	[TestCase(ComboBoxStyle.DropDown, 40, true, 106, 31)]
	[TestCase(ComboBoxStyle.DropDown, 40, false, 200, 3)]
	[TestCase(ComboBoxStyle.DropDown, 40, true, 1200, 3)]
	[TestCase(ComboBoxStyle.DropDownList, 40, true, 106, 0)]
	[TestCase(ComboBoxStyle.DropDownList, 20, true, 106, 0)]
	[TestCase(ComboBoxStyle.DropDownList, 40, false, 106, 20)]
	[TestCase(ComboBoxStyle.DropDownList, 40, false, 107, 20)]
	[TestCase(ComboBoxStyle.DropDownList, 40, true, 106, 31)]
	[TestCase(ComboBoxStyle.DropDownList, 40, false, 200, 3)]
	[TestCase(ComboBoxStyle.DropDownList, 40, true, 1200, 3)]
	public async Task ComboBoxRendersWithinBounds(ComboBoxStyle comboBoxStyle, int count, bool integralHeight, int dropDownHeight, int maxDropDownItems)
	{
		await using var ctx = new InMemoryTestServerContext();

		var formHeight = 900;
		ComboBox comboBox = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form { Height = formHeight, Width = 300 };
			comboBox = ComboBoxLargeForTest(count, comboBoxStyle: comboBoxStyle);
			comboBox.DropDownHeight = dropDownHeight;
			comboBox.MaxDropDownItems = maxDropDownItems;
			comboBox.IntegralHeight = integralHeight;
			form.Controls.Add(comboBox);
			return form;
		});
		var comboboxButton = await page.WaitForSelectorAsync(".combobox__button");
		await comboboxButton.ClickAsync();

		var comboboxDropdown = await page.WaitForSelectorAsync(".combobox__dropdown");

		Assert.That(async () => await comboboxDropdown.EvaluateAsync<int>("e => e.getBoundingClientRect().height + e.getBoundingClientRect().top"), Is.LessThanOrEqualTo(formHeight));
	}

	[Test, WithPlaywrightPage]
	[TestCase(600, 30, false)]
	[TestCase(300, 10, false)]
	[TestCase(300, 250, true)]
	[TestCase(600, 410, true)]
	public async Task ComboBoxShouldFlipWhenInsufficientHeightAvailable(int formHeight, int comboBoxTop, bool comboBoxShouldFlip)
	{
		await using var ctx = new InMemoryTestServerContext();

		ComboBox comboBox = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form { Height = formHeight, Width = 500 };
			comboBox = ComboBoxLargeForTest(40, comboBoxStyle: ComboBoxStyle.DropDownList);
			comboBox.Height = 20;
			comboBox.Top = comboBoxTop;
			form.Controls.Add(comboBox);
			return form;
		});

		var comboboxButton = await page.WaitForSelectorAsync(".combobox__button");
		await comboboxButton.ClickAsync();
		var comboboxDropdown = await page.WaitForSelectorAsync(".combobox__dropdown");
		var comboboxBoundingBoxResult = await comboboxDropdown.BoundingBoxAsync();

		Assert.That(comboboxBoundingBoxResult, Is.Not.Null);

		var hasFlipped = comboboxBoundingBoxResult.Y < comboBox.Top;

		Assert.That(hasFlipped, Is.EqualTo(comboBoxShouldFlip));
	}

	[Test, WithPlaywrightPage]
	public async Task ComboBoxMenuMovesOnResize()
	{
		await using var ctx = new InMemoryTestServerContext();

		ComboBox comboBox = null;
		Form form = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form { Height = 500, Width = 200 };
			comboBox = ComboBoxLargeForTest(40, comboBoxStyle: ComboBoxStyle.DropDownList);
			comboBox.Height = 20;
			comboBox.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
			comboBox.Top = 400;
			comboBox.Left = 0;
			form.Controls.Add(comboBox);
			return form;
		});

		var comboboxButton = page.Locator(".combobox__button");
		await comboboxButton.ClickAsync();
		var comboboxDropdown = page.Locator(".combobox__dropdown");
		var comboboxBoundingBoxResult = await comboboxDropdown.BoundingBoxAsync();

		Assert.That(comboboxBoundingBoxResult, Is.Not.Null);

		var popup = page.Locator(".popup");
		var combobox = page.Locator(".combobox");

		var comboboxBoundingBoxBefore = await combobox.BoundingBoxAsync();
		var popupBoundingBoxBefore = await popup.BoundingBoxAsync();

		var comboboxPositionBefore = comboboxBoundingBoxBefore?.Y;
		var popupPositionBefore = popupBoundingBoxBefore?.Y;

		await form.InvokeWinzorDispatcherAsync(() => form.Height += 500);

		Assert.That(async () => (await combobox.BoundingBoxAsync())?.Y, Is.Not.EqualTo(comboboxPositionBefore).After(1000, 100));
		Assert.That(async () => (await popup.BoundingBoxAsync())?.Y, Is.Not.EqualTo(popupPositionBefore).After(1000, 100));
	}

	[Test]
	public async Task SelectedItemAfterRemove()
	{
		using var ctx = new WinzorTestContext();
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			var comboBox = new ComboBox();
			comboBox.Items.Add("one");
			comboBox.Items.Add("two");
			comboBox.Items.Add("three");
			comboBox.SelectedItem = "two";
			Assert.That(comboBox.SelectedItem, Is.EqualTo("two"));
			comboBox.Items.Remove("three");
			Assert.That(comboBox.SelectedItem, Is.EqualTo("two"));
			comboBox.Items.Remove("one");
			Assert.That(comboBox.SelectedItem, Is.EqualTo("two"));
		});
	}

	[Test, WithPlaywrightPage]
	public async Task FocusShouldStayOnComboBoxAfterSelectedItem()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var combo = ComboBoxForTest(comboBoxStyle: ComboBoxStyle.DropDownList);
			combo.Location = new Drawing.Point(10, 50);
			combo.SelectedIndex = 0;
			return combo;
		});

		var combobox = page.Locator(".combobox__input");
		await combobox.WaitForAsync();
		await ElementMouseMoveWithDelayAsync(page, combobox, 1, 1);//Click should in div area
		await MouseDownWithDelayAsync(page);
		await MouseUpWithDelayAsync(page);

		var dropdown = page.Locator(".combobox__dropdown");
		await Assertions.Expect(dropdown).ToBeVisibleAsync();
		var dropdownItem = page.Locator("body > div.form > div:nth-child(2) > div > div > button:nth-child(4)");
		await Assertions.Expect(dropdownItem).ToBeVisibleAsync();
		await dropdownItem.ClickAsync();

		await Assertions.Expect(combobox).ToBeFocusedAsync();
	}

	[TestCase(ComboBoxStyle.DropDown)]
	[TestCase(ComboBoxStyle.DropDownList)]
	public async Task TestComboBoxDropdownListEmptyListShowsSingleBlankItem(ComboBoxStyle comboBoxStyle)
	{
		using var ctx = new WinzorTestContext();

		var rendered = await ctx.RenderControlOnFormAsync(() => new ComboBox() { Width = 100, Height = 25, DropDownStyle = comboBoxStyle });

		var comboBox = rendered.Find(".combobox");

		var comboBoxButton = rendered.Find(".combobox__button");
		await comboBoxButton.ClickAsync(new WebMouseEventArgs());

		var items = rendered.FindAll(".combobox__dropdown");
		Assert.That(items.Count, Is.EqualTo(1));
		var dropdown = items[0];
		Assert.That(dropdown, Is.Not.Null);
		var dropdownItems = dropdown.Children;
		Assert.That(dropdownItems.Length, Is.EqualTo(1));
		Assert.That(dropdownItems[0].GetAttribute("class"), Is.EqualTo("combobox__dropdown-blank-item"));
	}

	[TestCase(ComboBoxStyle.DropDown)]
	[TestCase(ComboBoxStyle.DropDownList)]
	public async Task TestComboBoxDropdownBlankItemIsUnselectableAndUnhoverable(ComboBoxStyle comboBoxStyle)
	{
		using var ctx = new WinzorTestContext();

		var rendered = await ctx.RenderControlOnFormAsync(() => new ComboBox() { Width = 100, Height = 25, DropDownStyle = comboBoxStyle });

		var comboBox = rendered.Find(".combobox");

		var comboBoxButton = rendered.Find(".combobox__button");
		await comboBoxButton.ClickAsync(new WebMouseEventArgs());

		var items = rendered.FindAll(".combobox__dropdown");
		Assert.That(items.Count, Is.EqualTo(1));
		var dropdown = items[0];
		Assert.That(dropdown, Is.Not.Null);
		var dropdownItems = dropdown.Children;
		Assert.That(dropdownItems.Length, Is.EqualTo(1));
		Assert.That(dropdownItems[0].GetAttribute(":hover"), Is.EqualTo(null));
	}

	[Test, WithPlaywrightPage]
	public async Task ComboBoxWithDropDownListTextNoWrapAndHidden()
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => ComboBoxForTest(comboBoxStyle: ComboBoxStyle.DropDownList));

		var comboBoxInput = page.Locator(".combobox__input");
		var comboBoxInputText = page.Locator(".combobox__input-text");

		Assert.That(await comboBoxInput.GetComputedStyleAsync("overflow"), Is.EqualTo("auto"));
		Assert.That(await comboBoxInputText.GetComputedStyleAsync("white-space"), Is.EqualTo("nowrap"));
		Assert.That(await comboBoxInputText.GetComputedStyleAsync("overflow-x"), Is.EqualTo("hidden"));
	}

	[Test, WithPlaywrightPage]
	[TestCase(".combobox__input", true, 4)]
	[TestCase(".combobox__input", false, 0)]
	[TestCase(".combobox__button", true, 4)]
	[TestCase(".combobox__button", false, 0)]
	public async Task OpenAndCloseDropdownWithClick(string clickArea, bool enabled, int dropdownItemCount)
	{
		await using var ctx = new InMemoryTestServerContext();

		var combo = default(ComboBox);
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form { Width = 500, Height = 500 };

			combo = ComboBoxForTest(comboBoxStyle: ComboBoxStyle.DropDownList);
			combo.Location = new Drawing.Point(10, 50);
			combo.SelectedIndex = 0;
			combo.Enabled = enabled;
			form.Controls.Add(combo);

			return form;
		});

		var comboboxToClick = page.Locator(clickArea);
		await ElementMouseMoveWithDelayAsync(page, comboboxToClick, 1, 1);//Click should in div area
		await MouseDownWithDelayAsync(page);
		await MouseUpWithDelayAsync(page);

		var dropdownItem = page.Locator(".combobox__dropdown-item");
		Assert.That(dropdownItem.CountAsync, Is.EqualTo(dropdownItemCount));

		await ElementMouseMoveWithDelayAsync(page, comboboxToClick, 150, 50);
		await MouseDownWithDelayAsync(page);
		await MouseUpWithDelayAsync(page);

		dropdownItem = page.Locator(".combobox__dropdown-item");
		Assert.That(dropdownItem.CountAsync, Is.EqualTo(0));
	}

	async Task MouseDownWithDelayAsync(IPage page, MouseDownOptions options = default, int delay = 350)
	{
		await page.Mouse.DownAsync(options);
		await Task.Delay(TimeSpan.FromMilliseconds(delay));
	}

	async Task ElementMouseMoveWithDelayAsync(IPage page, ILocator locator, int x, int y, int steps = 10, int delay = 350)
	{
		var elementRect = await locator.BoundingBoxAsync();
		await page.Mouse.MoveAsync(elementRect.X + x, elementRect.Y + y, new() { Steps = steps });
		await Task.Delay(TimeSpan.FromMilliseconds(delay));
	}

	async Task MouseUpWithDelayAsync(IPage page, MouseUpOptions options = default, int delay = 350)
	{
		await page.Mouse.UpAsync(options);
		await Task.Delay(TimeSpan.FromMilliseconds(delay));
	}
}
