using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using AngleSharp.Html.Dom;
using Bunit;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Balloons;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.Playwright;
using NUnit.Framework;
using WinzorFramework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;
using static WTG.PlaywrightTesting.PlaywrightTestContext;

namespace Enterprise.Winzor.Architecture.Test;

class TextBoxTest
{
	[Test]
	public async Task TextTwoWayBinding()
	{
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new WinzorTestForm();
			form.BindingSource.DataSourceType = typeof(DummyBusinessObject);
			var codeTextBox = new ZTextBox();
			form.Controls.Add(codeTextBox);
			form.BindingSource.SetBindingMember(codeTextBox, "Z0_Code");
			var descriptionTextBox = new ZTextBox();
			form.Controls.Add(descriptionTextBox);
			form.BindingSource.SetBindingMember(descriptionTextBox, "Z0_Description");
			form.DataSourceType = typeof(DummyBusinessObject);
			var factory = new BusinessObjectFactory();
			var dummyBizo = factory.New<DummyBusinessObject>();
			dummyBizo.Z0_Code = "FOO";
			dummyBizo.Z0_Description = "";
			dummyBizo.Z0_CodeInfo.ValueChanged += (object sender, EventArgs e) =>
			{
				dummyBizo.Z0_Description = "It's a " + dummyBizo.Z0_Code.ToLower();
			};
			form.SetDataBinding(dummyBizo, "");
			var button = new ZButton() { Text = "Validate", Top = 200 };
			form.Controls.Add(button);
			return form;
		});
		var inputs = rendered.FindAll("input").Cast<IHtmlInputElement>();
		var validateButton = rendered.Find("button:contains('Validate')");

		Assert.That(inputs.Select(e => e.Value), Is.EquivalentTo(new[] { "FOO", "" }));
		await inputs.Single(e => e.Value == "FOO").TriggerEventAsync("oninput", new ChangeEventArgs { Value = "BAR" });
		await validateButton.TriggerEventAsync("onwinzorfocusin", new WinzorFocusInEventArgs());
		Assert.That(rendered.FindAll("input").Cast<IHtmlInputElement>().Select(e => e.Value), Is.EquivalentTo(new[] { "BAR", "IT'S A BAR" }));
	}

	[Test]
	public async Task TextBoxHasFocus()
	{
		using var ctx = new EnterpriseTestContext();
		ZTextBox textbox = null;
		var component = await ctx.RenderControlOnFormAsync(() =>
		{
			textbox = new ZTextBox();
			return textbox;
		});
		var input = component.Find("input");

		await input.TriggerEventAsync("onwinzorfocusin", new WinzorFocusInEventArgs());
		Assert.That(textbox.Focused, Is.True);
		Assert.That(input.Attributes["style"].Value, Does.Contain("background-color:var(--color-info)").IgnoreCase);

		await input.FocusOutAsync();
		Assert.That(textbox.Focused, Is.False);
		Assert.That(input.Attributes["style"].Value, Does.Not.Contain("background-color:var(--color-info)").IgnoreCase);
	}

	[Test]
	public async Task TextBoxEnterMoveToNextTextBox()
	{
		using var ctx = new EnterpriseTestContext();
		ZTextBox textbox1 = null;
		ZTextBox textbox2 = null;

		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new WinzorTestForm();
			textbox1 = new ZTextBox();
			textbox2 = new ZTextBox();
			form.Controls.Add(textbox1);
			form.Controls.Add(textbox2);
			return form;
		});
		var input = rendered.Find("input");

		await input.TriggerEventAsync("onwinzorfocusin", new WinzorFocusInEventArgs());
		Assert.That(textbox1.Focused, Is.True);
		Assert.That(textbox2.Focused, Is.False);

		await rendered.KeyPressAsync(Keys.Enter, input);

		Assert.That(textbox1.Focused, Is.False);
		Assert.That(textbox2.Focused, Is.True);
	}

	[Test, WithPlaywrightPage]
	public async Task TextBoxSelectionShouldBeResetBeforeTextChanged()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		ZTextBox textBox = null;
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var workTimeControl = new GlbWorkTimeControl();
			textBox = workTimeControl.GetControl<ZTextBox>("MondayWorkingHoursBoundText");
			textBox.Text = " ******";
			textBox.Select(4, 3);
			return textBox;
		});

		Assert.Multiple(() =>
		{
			Assert.That(() => textBox.SelectedText, Is.EqualTo("***").After(3000, 100));
			Assert.That(textBox.SelectionStart, Is.EqualTo(4));
			Assert.That(textBox.SelectionLength, Is.EqualTo(3));
		});

		// Make sure the input is focused before pressing a key
		await page.Locator("input").FocusAsync(); // Ensure it gains focus
		await page.Locator("input").PressAsync("0", new() { Delay = 300 });

		Assert.That(() => textBox.SelectedText, Is.EqualTo(string.Empty).After(3000, 100));
		Assert.That(textBox.SelectionStart, Is.EqualTo(4));
		Assert.That(textBox.SelectionLength, Is.EqualTo(0));
	}

	[Test, WithPlaywrightPage]
	public async Task TextBoxCorrespondingTextChangedShouldUpdateTextWhenKeyPress()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		ZTextBox textBox = null;
		var page = await ctx.LoadControlOnFormAsync(() =>
		{
			var workTimeControl = new GlbWorkTimeControl();
			textBox = workTimeControl.GetControl<ZTextBox>("MondayWorkingHoursBoundText");
			textBox.Text = " ******";
			textBox.Select(4, 3);
			return textBox;
		});

		Assert.That(textBox.Text, Is.EqualTo(" ******"));

		await page.Locator("input").PressAsync("0", new() { Delay = 100 });

		Assert.That(() => textBox.Text, Is.EqualTo(" ***").After(3000, 100));
	}

	[Test]
	public async Task TextBoxGetCharIndexFromPosition()
	{
		using var ctx = new EnterpriseTestContext();
		ZTextBox textbox = null;
		var component = await ctx.RenderControlOnFormAsync(() => {
			textbox = new ZTextBox();
			textbox.Text = "this is theeerr";
			textbox.SelectionStart = 9;
			return textbox;
		});

		Assert.That(textbox.GetCharIndexFromPosition(new System.Drawing.Point()), Is.EqualTo(9));
	}

	[Test, WithPlaywrightPage]
	public async Task TextBoxAcceptClickThroughNotificationIcon()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		TextBox textBoxWithNotificationIcon = null;
		TextBox textBoxWithoutNotificationIcon = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			textBoxWithNotificationIcon = new ZTextBox() { Width = 200, Height = 50, Top = 200, Left = 200 };
			var notifications = new NotificationCollection();
			notifications.AddWarning("Warning");
			textBoxWithNotificationIcon.GetExtension<NotificationExtension>().Notifications = notifications;
			textBoxWithNotificationIcon.Text = "ABCDEFGHIJKL";

			textBoxWithoutNotificationIcon = new ZTextBox() { Width = 200, Height = 50, Top = 100, Left = 200 };

			var form = new Form { Width = 1000, Height = 1000 };
			form.Controls.Add(textBoxWithoutNotificationIcon);
			form.Controls.Add(textBoxWithNotificationIcon);

			Balloon.Instance.IsShownDuringTesting = true;

			return form;
		});

		Assert.That(async () => await Page.Locator("input:nth-of-type(1)").EvaluateAsync<bool>("element => document.activeElement === element"), Is.True.After(3000, 100));

		Assert.That(await Page.Locator("div.notification.notification--warning").CountAsync(), Is.EqualTo(1));
		var iconLocation = await Page.Locator("div.notification.notification--warning").BoundingBoxAsync();
		await MouseOverAsync(iconLocation);
		var html = await Page.ContentAsync();
		Assert.That(await Page.Locator(".balloon").CountAsync(), Is.EqualTo(1));

		await SimulateClickEvent(iconLocation);
		Assert.That(async () => await Page.Locator("input:nth-of-type(2)").GetComputedStyleAsync("width"), Is.EqualTo("200px").After(2000, 100));
		Assert.That(async () => await Page.Locator("input:nth-of-type(2)").EvaluateAsync<bool>("element => document.activeElement === element"), Is.True.After(3000, 100));
	}

	async Task SimulateClickEvent(LocatorBoundingBoxResult location)
	{
		await Page.Mouse.MoveAsync(location.X + location.Width / 2, location.Y + location.Height / 2);
		await Task.Delay(500);
		await MouseDownAsync();
		await MouseUpAsync();
	}

	async Task MouseDownAsync()
	{
		await Page.Mouse.DownAsync();
		await Task.Delay(500);
	}

	async Task MouseUpAsync()
	{
		await Page.Mouse.UpAsync();
		await Task.Delay(500);
	}

	async Task MouseOverAsync(LocatorBoundingBoxResult location)
	{
		await Page.Mouse.MoveAsync(location.X + location.Width / 2, location.Y + location.Height / 2);
		await Task.Delay(500);
	}

	async Task KeyPressAsync(string textValue)
	{
		await Page.Keyboard.PressAsync(textValue);
	}

	[Test, WithPlaywrightPage]
	public async Task TextBoxValidateTextOnKeyDown()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		ZTextBox textBoxWithValidation = null;

		var page = await ctx.LoadFormAsync(() =>
		{
			textBoxWithValidation = new ZTextBox() { Width = 200, Height = 50, Top = 200, Left = 200 };
			textBoxWithValidation.SelectionStart = 3;
			textBoxWithValidation.SelectionLength = 1;
			textBoxWithValidation.Focus();

			var form = new Form { Width = 1000, Height = 1000 };
			form.Controls.Add(textBoxWithValidation);

			return form;
		});

		Assert.That(async () => await Page.Locator("input:nth-of-type(1)").EvaluateAsync<bool>("element => document.activeElement === element"), Is.True.After(3000, 100));
		await Page.Locator("input:nth-of-type(1)").PressSequentiallyAsync("******");
		await Task.Delay(500);

		await KeyPressAsync("A");

		var inputVal = await Page.Locator("input:nth-of-type(1)").InputValueAsync();
		Assert.That(inputVal, Is.EqualTo("******A"));

		await page.WaitForSelectorAsync("input:nth-of-type(1)");
		await Page.Locator("input:nth-of-type(1)").ClickAsync();
		await Page.Locator("input:nth-of-type(1)").PressSequentiallyAsync("***AAAA****");
		await Task.Delay(500);

		inputVal = await Page.Locator("input:nth-of-type(1)").InputValueAsync();
		Assert.That(inputVal, Is.EqualTo("******A***AAAA****"));
	}

	[Test, WithPlaywrightPage]
	public async Task TextBoxValidateTextOnKeyDown_GlbWorkTimeControl()
	{
		await using var ctx = new InMemoryAppServerTestContext();

		var page = await ctx.LoadFormAsync(() =>
		{
			var workTimeControl = new GlbWorkTimeControl();
			var form = new Form { Width = 1000, Height = 1000 };
			form.Controls.Add(workTimeControl);

			return form;
		});
		await page.WaitForSelectorAsync("input:nth-of-type(1)");
		await Page.Locator("input:nth-of-type(1)").ClickAsync();
		Assert.That(async () => await Page.Locator("input:nth-of-type(1)").EvaluateAsync<bool>("element => document.activeElement === element"), Is.True.After(3000, 100));

		Assert.That(await Page.Locator("input:nth-of-type(1)").InputValueAsync(), Is.EqualTo(""));

		await Page.Locator("input:nth-of-type(1)").PressSequentiallyAsync("*******");
		await Task.Delay(500);
		await Page.Locator("input:nth-of-type(1)").PressAsync("B");
		await Task.Delay(500);
		Assert.That(await Page.Locator("input:nth-of-type(1)").InputValueAsync(), Is.EqualTo("*******"));

		await page.WaitForSelectorAsync("input:nth-of-type(2)");
		await Page.Locator("input:nth-of-type(2)").ClickAsync();
		await Page.Locator("input:nth-of-type(2)").PressSequentiallyAsync("***AAAA****");
		await Task.Delay(500);
		Assert.That(await Page.Locator("input:nth-of-type(2)").InputValueAsync(), Is.EqualTo("*******"));

		await KeyPressAsync("Backspace");
		await Task.Delay(500);
		Assert.That(await Page.Locator("input:nth-of-type(2)").InputValueAsync(), Is.EqualTo("******"));

		await KeyPressAsync("Backspace");
		await KeyPressAsync("Backspace");
		await Task.Delay(500);
		Assert.That(await Page.Locator("input:nth-of-type(2)").InputValueAsync(), Is.EqualTo("****"));

		await KeyPressAsync("Space");
		await Task.Delay(500);
		Assert.That(await Page.Locator("input:nth-of-type(2)").InputValueAsync(), Is.EqualTo("**** "));

		await Page.Locator("input:nth-of-type(2)").PressSequentiallyAsync(" ***");
		await Task.Delay(500);
		Assert.That(await Page.Locator("input:nth-of-type(2)").InputValueAsync(), Is.EqualTo("****  ***"));
	}
}
