using System.Drawing;
using System.Threading.Tasks;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Playwright;
using Moq;
using NUnit.Framework;
using WinzorFramework;
using WinzorFramework.Extensions;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace System.Windows.Forms;

using static PlaywrightTestContext;

class CheckBoxTest
{
	[Test]
	public async Task CheckBoxChangedEvent()
	{
		await ControlAssert.ImplementsEventAsync<CheckBox, EventHandler>(nameof(CheckBox.CheckedChanged), a => new EventHandler((o, e) => a()), "input", e => e.Change(true));
	}

	[Test]
	public async Task EnterEvent()
	{
		await ControlAssert.ImplementsEventAsync<CheckBox, EventHandler>(nameof(CheckBox.Enter), a => new EventHandler((s, e) => a()), "label", e => e.TriggerEvent("onwinzorfocusin", new WinzorFocusInEventArgs()));
	}

	[Test]
	public async Task CheckBoxChangedFromServer()
	{
		await ControlAssert.ImplementsChangeFromServerAsync<CheckBox>(c => {
			// When OnCheckStateChanged() is run, Refresh() is called when OwnerDraw is true.
			// This performs an additional render, causing ImplementsChangeFromServerAsync to fail.
			// Setting FlatStyle to System causes OwnerDraw to return false, preventing this issue.
			c.FlatStyle = FlatStyle.System;

			c.Checked = true;
		});
	}

	[Test]
	public async Task CheckBoxInlineStyles()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new CheckBox() { Top = 100, Left = 200, Width = 300, Height = 400 });

		var checkbox = rendered.Find(".checkbox");

		Assert.That(checkbox.GetAttribute("style"), Is.EqualTo("position:absolute;width:300px;height:400px;top:100px;left:200px;background-color:var(--color-control);"));
	}

	[Test]
	public async Task CheckBoxHasInputTypeCheckbox()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new CheckBox());

		var checkbox = rendered.Find(".checkbox input[type=checkbox].checkbox__input");

		Assert.That(checkbox, Is.Not.Null);
	}

	[Test]
	public async Task CheckboxDefaultTextAlign()
	{
		using var ctx = new WinzorTestContext();
		CheckBox checkbox = null;
		await ctx.RenderControlOnFormAsync(() => checkbox = new CheckBox());
		Assert.That(checkbox.TextAlign, Is.EqualTo(ContentAlignment.MiddleLeft));
	}

	[Test]
	public async Task CheckBoxDisabled()
	{
		using var ctx = new WinzorTestContext();
		CheckBox checkbox = null;
		var rendered = await ctx.RenderControlOnFormAsync(() => checkbox = new CheckBox { Enabled = false });
		Assert.That(rendered.Find(".checkbox input[type=checkbox].checkbox__input").GetAttribute("disabled"), Is.Empty);

		await checkbox.InvokeWinzorDispatcherAsync(() => checkbox.Enabled = true);
		Assert.That(rendered.Find(".checkbox input[type=checkbox].checkbox__input").GetAttribute("disabled"), Is.Null);
	}

	[Test]
	public async Task CheckBoxText()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new CheckBox() { Text = "Do Re Mi" });

		var checkboxText = rendered.Find(".checkbox span");

		Assert.That(checkboxText.TextContent, Is.EqualTo("Do Re Mi"));
	}

	[Test]
	public async Task ElementReferenceShouldOnTheCorrectElement()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new CheckBox() { Text = "Do Re Mi" });

		Assert.That(rendered.Find(".checkbox").GetAttribute("blazor:elementreference"), Is.Null);
		Assert.That(rendered.Find(".checkbox input").GetAttribute("blazor:elementreference"), Is.Not.Null);
	}

	[Test]
	public async Task CheckBoxImage()
	{
		using var ctx = new WinzorTestContext();
		var imageSrc = TestImage.GetImage();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			return new CheckBox() { Image = imageSrc };
		});

		var checkboxImage = rendered.Find(".checkbox .checkbox__image img");
		Assert.That(checkboxImage.GetAttribute("src"), Does.Contain($"{imageSrc.ToBase64()}"));
	}

	[Test]
	public async Task CheckboxAutoSizeGrowsAndShrinksByDefault()
	{
		using var ctx = new WinzorTestContext();

		CheckBox checkBox1 = null;
		CheckBox checkBox2 = null;

		await ctx.RenderControlOnFormAsync(() => checkBox1 = new CheckBox() { AutoSize = true, Width = 1, Text = "This is a test" });
		await ctx.RenderControlOnFormAsync(() => checkBox2 = new CheckBox() { AutoSize = true, Width = 1000, Text = "This is a test" });

		Assert.That(checkBox1.Width, Is.GreaterThan(1));
		Assert.That(checkBox2.Width, Is.LessThan(1000));
	}

	[Test]
	public async Task CheckboxShouldHaveCorrectPadding()
	{
		using var ctx = new WinzorTestContext();

		CheckBox checkBox1 = null;
		var checkBoxText = "Test text";

		await ctx.RenderControlOnFormAsync(() => checkBox1 = new CheckBox() { AutoSize = true, Text = checkBoxText });

		var testTextWidth = TextRenderer.MeasureText(checkBoxText, checkBox1.Font).Width + 18; // AutoSizeExtraWidth
		Assert.That(checkBox1.Width, Is.EqualTo(testTextWidth));
	}

	[Test]
	public async Task CheckBoxAutoSizeMainLabelStyles()
	{
		using var ctx = new WinzorTestContext();
		var rendered = await ctx.RenderControlOnFormAsync(() => new CheckBox { AutoSize = true, Text = "The Label" });
		var checkbox = rendered.Find(".checkbox");

		Assert.That(checkbox.GetAttribute("style"), Is.EqualTo("position:absolute;width:70px;height:17px;top:0px;left:0px;background-color:var(--color-control);"));
	}

	[Test]
	public async Task CheckBoxToString()
	{
		using var ctx = new WinzorTestContext();
		CheckBox checkbox = null;
		var rendered = await ctx.RenderControlOnFormAsync(() => checkbox = new CheckBox());

		Assert.That(checkbox.ToString(), Is.EqualTo("System.Windows.Forms.CheckBox, CheckState: 0"));
	}

	[Test, WithPlaywrightPage]
	public async Task CheckAutoSizeInputAndTextStyles()
	{
		await using var ctx = new InMemoryTestServerContext();
		CheckBox checkbox = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var form = new Form();
			checkbox = new CheckBox { AutoSize = true, Text = "The Label" };
			form.Controls.Add(checkbox);
			return form;
		});

		var checkboxInput = await page.WaitForSelectorAsync(".checkbox__input");
		AssertElementSizeAndPosition(checkboxInput, 13, 13, 0, 2);

		var checkboxText = await page.WaitForSelectorAsync("label span");
		AssertElementSizeAndPosition(checkboxText, 52, 13, 17, 2);
	}

	void AssertElementSizeAndPosition(IElementHandle element, int width, int height, int x, int y)
	{
		Assert.That(element, Is.Not.Null);
		Assert.That(async () => (int)(await element.BoundingBoxAsync()).Width, Is.EqualTo(width).After(2000, 100));
		Assert.That(async () => (int)(await element.BoundingBoxAsync()).Height, Is.EqualTo(height).After(2000, 100));
		Assert.That(async () => (int)(await element.BoundingBoxAsync()).X, Is.EqualTo(x).After(2000, 100));
		Assert.That(async () => (int)(await element.BoundingBoxAsync()).Y, Is.EqualTo(y).After(2000, 100));
	}

	[Test]
	public async Task CheckBoxUseMnemonicUpdatesFromServer()
	{
		await ControlAssert.ImplementsChangeFromServerAsync<CheckBox>(c => c.UseMnemonic = false);
	}

	[Test]
	public async Task CheckBoxProcessMnemonicPerformsClick()
	{
		using var ctx = new WinzorTestContext();
		var clicked = false;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var checkBox = new CheckBox() { Text = "&Test" };
			checkBox.Click += (sender, args) => clicked = true;
			return checkBox;
		});

		Assert.That(clicked, Is.False);
		var checkBox = rendered.Find(".checkbox");
		await rendered.KeyPressAsync(Keys.Alt | Keys.T, checkBox);
		Assert.That(clicked, Is.True);

		clicked = false;

		Assert.That(clicked, Is.False);
		var form = rendered.Find(".form");
		await rendered.KeyPressAsync(Keys.Alt | Keys.T, form);
		Assert.That(clicked, Is.True);
	}

	[Test]
	public async Task CheckBoxUseMnemonicFormatsTestWithMnemonic()
	{
		using var ctx = new WinzorTestContext();
		CheckBox checkBox = null;
		var rendered = await ctx.RenderControlOnFormAsync(() => checkBox = new CheckBox() { Text = "&Test", UseMnemonic = false });

		Assert.That(rendered.Find("span").InnerHtml, Is.EqualTo("Test"));
		await checkBox.InvokeWinzorDispatcherAsync(() => checkBox.UseMnemonic = true);
		Assert.That(rendered.Find("span").InnerHtml, Is.EqualTo("<span class=\"mnemonickey\">T</span>est"));
	}

	[Test]
	public async Task CheckBoxCheckStateAlsoChangeWithChecked()
	{
		using var ctx = new WinzorTestContext();
		CheckBox checkBox = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			checkBox = new CheckBox() { Text = "&Test" };
			return checkBox;
		});

		Assert.That(checkBox.CheckState, Is.EqualTo(CheckState.Unchecked));
		await rendered.Find(".checkbox__input").ChangeAsync(new ChangeEventArgs { Value = true });
		Assert.That(checkBox.CheckState, Is.EqualTo(CheckState.Checked));
	}

	[Test]
	public async Task CheckBoxFiresClickEventWithChecked()
	{
		using var ctx = new WinzorTestContext();
		CheckBox checkBox = null;
		var onGotFocusFired = new TaskCompletionSource();
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			checkBox = new CheckBox() { Text = "&Test" };
			checkBox.Click += (s, e) => onGotFocusFired.SetResult();
			return checkBox;
		});
		Assert.That(checkBox.CheckState, Is.EqualTo(CheckState.Unchecked));
		Assert.That(await onGotFocusFired.Task.WithTimeout(TimeSpan.FromSeconds(1.0)), Is.False);
		await rendered.Find(".checkbox__input").ChangeAsync(new ChangeEventArgs { Value = true });
		Assert.That(checkBox.CheckState, Is.EqualTo(CheckState.Checked));
		Assert.That(await onGotFocusFired.Task.WithTimeout(TimeSpan.FromSeconds(3.0)), Is.True);
	}

	[Test]
	public async Task CheckBoxSetValueBeforeFiresClickEvent()
	{
		using var ctx = new WinzorTestContext();
		var checkStateToAssert = CheckState.Unchecked;
		CheckBox checkBox = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			checkBox = new CheckBox() { CheckState = checkStateToAssert };
			checkBox.Click += (s, e) =>
			{
				checkStateToAssert = checkBox.CheckState;
			};
			return checkBox;
		});

		await rendered.Find(".checkbox__input").ChangeAsync(new ChangeEventArgs { Value = true });
		Assert.That(checkStateToAssert, Is.EqualTo(CheckState.Checked));
	}

	[Test, WithPlaywrightPage]
	[TestCase(true, "none", TestName = "{m}_Colored")]
	[TestCase(false, "grayscale(1)", TestName = "{m}_GrayedOut")]
	public async Task CheckBoxIcon(bool isEnabled, string expectedStyle)
	{
		var imageSrc = TestImage.GetImage();
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			var checkbox = new CheckBox() { Image = imageSrc, Enabled = isEnabled };
			form.Controls.Add(checkbox);
			return form;
		});

		var checkboxImageParent = await page.WaitForSelectorAsync(".checkbox__image");
		var filter = await getComputedStyle(checkboxImageParent, "filter");

		Assert.That(filter, Is.EqualTo(expectedStyle));
	}

	[Test]
	public async Task OnCheckedChangedShouldNotFireEnterOrValidatedEvent()
	{
		using var ctx = new WinzorTestContext();
		var enterFiredCount = 0;
		var validatingFiredCount = 0;
		var validatedFiredCount = 0;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var checkBox = new CheckBox();
			checkBox.Enter += (sender, e) => { enterFiredCount++; };
			checkBox.Validating += (sender, e) => { validatingFiredCount++; };
			checkBox.Validated += (sender, e) => { validatedFiredCount++; };
			return checkBox;
		});

		await rendered.Find(".checkbox__input").ChangeAsync(new ChangeEventArgs { Value = true });
		Assert.That(enterFiredCount, Is.EqualTo(1));
		Assert.That(validatingFiredCount, Is.EqualTo(0));
		Assert.That(validatedFiredCount, Is.EqualTo(0));
	}

	[Test]
	public async Task AppearanceChangedEventIsTriggeredWhenAppearanceChanged()
	{
		using var ctx = new WinzorTestContext();
		CheckBox checkBox = null;
		var eventFired = false;

		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			checkBox = new CheckBox();
			checkBox.Appearance = Appearance.Normal;
			checkBox.AppearanceChanged += (s, e) => eventFired = true;

			form.Controls.Add(checkBox);
			return form;
		});

		await checkBox.InvokeWinzorDispatcherAsync(() =>
		{
			checkBox.Appearance = Appearance.Button;
		});

		Assert.That(eventFired, Is.True, "AppearanceChanged event should be fired when Appearance is updated.");
	}

	[Test]
	[TestCase(FlatStyle.Standard)]
	[TestCase(FlatStyle.System)]
	public async Task CheckStateChangedEventIsTriggeredWhenCheckStateChanged(FlatStyle flatStyle)
	{
		using var ctx = new WinzorTestContext();
		var eventFired = false;
		var refreshCalled = false;

		var mockCheckBox = new Mock<CheckBox>();
		mockCheckBox.Setup(m => m.Refresh()).Callback(() => refreshCalled = true);

		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			mockCheckBox.Object.CheckState = CheckState.Unchecked;
			mockCheckBox.Object.FlatStyle = flatStyle;
			mockCheckBox.Object.CheckStateChanged += (s, e) => eventFired = true;

			form.Controls.Add(mockCheckBox.Object);
			return form;
		});

		await mockCheckBox.Object.InvokeWinzorDispatcherAsync(() =>
		{
			mockCheckBox.Object.CheckState = CheckState.Checked;
		});

		Assert.That(eventFired, Is.True, "CheckStateChanged event should be fired when CheckState is updated.");
		Assert.That(refreshCalled, Is.EqualTo(flatStyle != FlatStyle.System), "Refresh should be called when FlatStyle is Not System.");
	}

	[Test]
	public async Task UpdateCheckStateUpdatesChecked()
	{
		using var ctx = new WinzorTestContext();

		var rendered = await ctx.RenderControlOnFormAsync(() => new CheckBox());

		var checkBox = rendered.GetForm().Controls[0] as CheckBox;

		Assert.That(checkBox.Checked, Is.EqualTo(false));
		Assert.That(checkBox.CheckState, Is.EqualTo(CheckState.Unchecked));

		await checkBox.InvokeWinzorDispatcherAsync(() => checkBox.CheckState = CheckState.Checked);

		Assert.That(checkBox.Checked, Is.EqualTo(true));
		Assert.That(checkBox.CheckState, Is.EqualTo(CheckState.Checked));

		await checkBox.InvokeWinzorDispatcherAsync(() => checkBox.CheckState = CheckState.Indeterminate);

		Assert.That(checkBox.Checked, Is.EqualTo(true));
		Assert.That(checkBox.CheckState, Is.EqualTo(CheckState.Indeterminate));

		await checkBox.InvokeWinzorDispatcherAsync(() => checkBox.CheckState = CheckState.Unchecked);

		Assert.That(checkBox.Checked, Is.EqualTo(false));
		Assert.That(checkBox.CheckState, Is.EqualTo(CheckState.Unchecked));
	}

	[Test]
	public async Task UpdateCheckedUpdatesCheckState()
	{
		using var ctx = new WinzorTestContext();

		var rendered = await ctx.RenderControlOnFormAsync(() => new CheckBox());

		var checkBox = rendered.GetForm().Controls[0] as CheckBox;

		Assert.That(checkBox.Checked, Is.EqualTo(false));
		Assert.That(checkBox.CheckState, Is.EqualTo(CheckState.Unchecked));

		await checkBox.InvokeWinzorDispatcherAsync(() => checkBox.Checked = true);

		Assert.That(checkBox.Checked, Is.EqualTo(true));
		Assert.That(checkBox.CheckState, Is.EqualTo(CheckState.Checked));

		await checkBox.InvokeWinzorDispatcherAsync(() => checkBox.Checked = false);

		Assert.That(checkBox.Checked, Is.EqualTo(false));
		Assert.That(checkBox.CheckState, Is.EqualTo(CheckState.Unchecked));
	}

	[Test]
	public async Task CheckedChangedTriggedWhenCheckStateUpdated()
	{
		using var ctx = new WinzorTestContext();
		var checkedChangedCalledTimes = 0;

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var checkBox = new CheckBox();
			checkBox.CheckedChanged += (s, e) => checkedChangedCalledTimes++;
			return checkBox;
		});

		var checkBox = rendered.GetForm().Controls[0] as CheckBox;

		Assert.That(checkedChangedCalledTimes, Is.EqualTo(expected: 0));

		await checkBox.InvokeWinzorDispatcherAsync(() => checkBox.CheckState = CheckState.Checked);

		Assert.That(checkedChangedCalledTimes, Is.EqualTo(expected: 1));

		await checkBox.InvokeWinzorDispatcherAsync(() => checkBox.CheckState = CheckState.Indeterminate);

		Assert.That(checkedChangedCalledTimes, Is.EqualTo(expected: 1));

		await checkBox.InvokeWinzorDispatcherAsync(() => checkBox.CheckState = CheckState.Unchecked);

		Assert.That(checkedChangedCalledTimes, Is.EqualTo(expected: 2));
	}

	[Test]
	public async Task CheckStateChangedTriggedWhenCheckedUpdated()
	{
		using var ctx = new WinzorTestContext();
		var checkStateChangedCalledTimes = 0;

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var checkBox = new CheckBox();
			checkBox.CheckStateChanged += (s, e) => checkStateChangedCalledTimes++;
			return checkBox;
		});

		var checkBox = rendered.GetForm().Controls[0] as CheckBox;

		Assert.That(checkStateChangedCalledTimes, Is.EqualTo(expected: 0));

		await checkBox.InvokeWinzorDispatcherAsync(() => checkBox.Checked = true);

		Assert.That(checkStateChangedCalledTimes, Is.EqualTo(expected: 1));

		await checkBox.InvokeWinzorDispatcherAsync(() => checkBox.Checked = false);

		Assert.That(checkStateChangedCalledTimes, Is.EqualTo(expected: 2));

		await checkBox.InvokeWinzorDispatcherAsync(() => checkBox.Checked = false);

		Assert.That(checkStateChangedCalledTimes, Is.EqualTo(expected: 2));
	}

	[Test]
	public async Task CheckedChangedTriggedWhenCheckedUpdated()
	{
		using var ctx = new WinzorTestContext();
		var checkedChangedCalledTimes = 0;

		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			var checkBox = new CheckBox();
			checkBox.CheckedChanged += (s, e) => checkedChangedCalledTimes++;
			return checkBox;
		});

		var checkBox = rendered.GetForm().Controls[0] as CheckBox;

		Assert.That(checkedChangedCalledTimes, Is.EqualTo(expected: 0));

		await checkBox.InvokeWinzorDispatcherAsync(() => checkBox.Checked = true);

		Assert.That(checkedChangedCalledTimes, Is.EqualTo(expected: 1));

		await checkBox.InvokeWinzorDispatcherAsync(() => checkBox.Checked = false);

		Assert.That(checkedChangedCalledTimes, Is.EqualTo(expected: 2));

		await checkBox.InvokeWinzorDispatcherAsync(() => checkBox.Checked = false);

		Assert.That(checkedChangedCalledTimes, Is.EqualTo(expected: 2));
	}

	[Test, WithPlaywrightPage]
	[TestCase(true, TestName = "{m}_Checked")]
	[TestCase(false, TestName = "{m}_Unchecked")]
	public async Task CheckBoxStyles(bool isChecked)
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			var checkbox = new CheckBox() { Checked = isChecked };
			form.Controls.Add(checkbox);
			return form;
		});

		var checkboxParent = await page.WaitForSelectorAsync(".checkbox");
		var checkboxItem = await page.WaitForSelectorAsync(".checkbox__input");

		Assert.That(await getComputedStyle(checkboxItem, "background-color"), Is.EqualTo("rgb(255, 255, 255)"));
		Assert.That(await getComputedStyle(checkboxItem, "border-color"), Is.EqualTo("rgb(50, 118, 208)"));
		Assert.That(await getComputedStyle(checkboxItem, "border-radius"), Is.EqualTo("0px"));
		if (isChecked)
		{
			Assert.That(await getComputedStyle(checkboxItem, "background-image"), Does.StartWith("url(\"data:image/svg+xml"));
		}
	}

	[Test, WithPlaywrightPage]
	[TestCase(true, TestName = "{m}_Checked")]
	[TestCase(false, TestName = "{m}_Unchecked")]
	public async Task CheckBoxStylesDisabled(bool isChecked)
	{
		await using var ctx = new InMemoryTestServerContext();
		Form form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new Form();
			var checkbox = new CheckBox() { Checked = isChecked, Enabled = false };
			form.Controls.Add(checkbox);
			return form;
		});

		var checkboxItem = await page.WaitForSelectorAsync(".checkbox__input");

		Assert.That(await getComputedStyle(checkboxItem, "background-color"), Is.EqualTo("rgb(255, 255, 255)"));
		Assert.That(await getComputedStyle(checkboxItem, "border-color"), Is.EqualTo("rgb(204, 204, 204)"));
		Assert.That(await getComputedStyle(checkboxItem, "border-radius"), Is.EqualTo("0px"));
		if (isChecked)
		{
			Assert.That(await getComputedStyle(checkboxItem, "background-image"), Does.StartWith("url(\"data:image/svg+xml"));
		}
	}

	[Test, WithPlaywrightPage]
	public async Task Security_UpdatingValuesForReadonlyCheckboxes()
	{
		await using var ctx = new InMemoryTestServerContext();
		Exception exception = null;
		CheckBox checkBox = null;
		Form form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			Application.ThreadException += Application_ThreadException;
			form = new Form();
			checkBox = new CheckBox() { Text = "disabled checkBox", Enabled = false, Checked = false };
			form.Controls.Add(checkBox);
			return form;
		});

		try
		{
			var checkboxItem = await page.WaitForSelectorAsync(".checkbox__input");
			await checkboxItem.EvaluateAsync("e => { e.disabled = false; e.attributes.removeNamedItem('aria-disabled'); e.attributes.removeNamedItem('readonly'); e.attributes.removeNamedItem('onclick'); }");

			await checkboxItem.CheckAsync();
			Assert.Multiple(() =>
			{
				Assert.That(exception, Is.Not.Null);
				Assert.That(checkBox.Checked, Is.False);
			});
		}
		finally
		{
			Application.ThreadException -= Application_ThreadException;
		}

		void Application_ThreadException(object sender, Threading.ThreadExceptionEventArgs e)
		{
			exception = e.Exception;
		}
	}

	async Task<string> getComputedStyle(IElementHandle e, string property)
	{
		return (await e.EvaluateAsync($"e => window.getComputedStyle(e).getPropertyValue('{property}')")).Value.ToString();
	}

	[Test, WithPlaywrightPage]
	[TestCase(true, "rgb(0, 0, 0)")]
	[TestCase(false, "rgb(160, 160, 160)")]
	public async Task CheckBoxLabelStyle(bool isEnabled, string expectedColor)
	{
		await using var ctx = new InMemoryTestServerContext();
		var page = await ctx.LoadControlOnFormAsync(() => new CheckBox { Text = "This is a checkbox", Width = 200, Height = 30, Enabled = isEnabled });

		var checkboxItem = await page.WaitForSelectorAsync(".checkbox__label");
		Assert.That(await getComputedStyle(checkboxItem, "color"), Is.EqualTo(expectedColor));
	}
}
