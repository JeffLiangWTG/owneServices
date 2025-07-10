using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Microsoft.Playwright;
using NUnit.Framework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;
using ResString = Enterprise.ZArchitecture.GUI.ResString;

namespace Enterprise.Winzor.Architecture.Test;

class TranslationFeedbackManagerTest
{
	readonly LocatorAssertionsToHaveCSSOptions ToHaveCSSOptions = new() { Timeout = 1000 };
	readonly LocatorAssertionsToBeFocusedOptions ToBeFocusedOptions = new() { Timeout = 1000 };
	readonly LocatorDispatchEventOptions DispatchEventOptions = new() { Timeout = 1000 };
	readonly LocatorClickOptions ClickOptions = new() { Timeout = 1000 };

	[Test, WithPlaywrightPage]
	public async Task TestZLabel()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var key = "ZL";
		var caption = "Zee Label";
		var form = default(ZDummyForm);
		var page = await ctx.LoadFormAsync(() =>
		{
			var label = new ZLabel() { CaptionResourceString = new ResourceStringData(key, caption) };
			return form = SetupTestForm(label);
		});
		await AssertThatTranslationFeedbackWorksAsync(page, form, page.Locator(".label"), key, caption);
	}

	[Test, WithPlaywrightPage]
	public async Task TestZLinkLabel()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var key = "ZLL";
		var caption = "Zee Link Label";
		var form = default(ZDummyForm);
		var page = await ctx.LoadFormAsync(() =>
		{
			var linkLabel = new ZLinkLabel() { CaptionResourceString = new ResourceStringData(key, caption) };
			linkLabel.LinkClicked += ControlAction;
			return form = SetupTestForm(linkLabel);
		});
		await AssertThatTranslationFeedbackWorksAsync(page, form, page.Locator(".linklabel"), key, caption);
	}

	[Test, WithPlaywrightPage]
	public async Task TestZButton()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var key = "ZB";
		var caption = "Zee Button";
		var form = default(ZDummyForm);
		var page = await ctx.LoadFormAsync(() =>
		{
			var button = new ZButton() { CaptionResourceString = new ResourceStringData(key, caption) };
			button.Click += ControlAction;
			return form = SetupTestForm(button);
		});
		await AssertThatTranslationFeedbackWorksAsync(page, form, page.Locator(".button"), key, caption);
	}

	[Test, WithPlaywrightPage]
	public async Task TestZRadioButton()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var key = "ZRB";
		var caption = "Zee Radio Button";
		var form = default(ZDummyForm);
		var page = await ctx.LoadFormAsync(() =>
		{
			var button = new ZRadioButton() { CaptionResourceString = new ResourceStringData(key, caption) };
			button.Click += ControlAction;
			return form = SetupTestForm(button);
		});
		await AssertThatTranslationFeedbackWorksAsync(page, form, page.Locator(".radiobutton"), key, caption);
	}

	[Test, WithPlaywrightPage]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1199:Do Not Use Unnecessary Resource String In Unit Tests", Justification = "Test Translation Feedback")]
	public async Task TestZDropEdit()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var disposableAction = Db.DisposableActionForDbConnection();
		var dummyWithList = new BusinessObjectFactory().New<DummyWithSettableCodeDescriptionPairList>();
		var list = new CodeDescriptionPairList();
		list.AddPair("ONE", ResString.GetMultilingualString("111", "First Item"));
		list.AddPair("TWO", ResString.GetMultilingualString("222", "Second Item"));
		dummyWithList.SetDummyList(list);
		var form = default(TestDropEditForm);
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new TestDropEditForm(dummyWithList);
			form.Disposed += (s, e) => disposableAction.Dispose();
			return form;
		});
		var button = page.Locator(".zdropbutton button");

		await button.ClickAsync(options: ClickOptions);
		await page.Locator(".zdropform tr").Nth(0).ClickAsync(options: ClickOptions);
		await AssertThatTranslationFeedbackWorksAsync(page, form, page.Locator(".zdropedit .textbox").Nth(0), "111", "First Item");
		await AssertThatTranslationFeedbackWorksAsync(page, form, page.Locator(".zdropedit .textbox").Nth(1), "111", "First Item");

		await button.ClickAsync(options: ClickOptions);
		await page.Locator(".zdropform tr").Nth(1).ClickAsync(options: ClickOptions);
		await AssertThatTranslationFeedbackWorksAsync(page, form, page.Locator(".zdropedit .textbox").Nth(0), "222", "Second Item");
		await AssertThatTranslationFeedbackWorksAsync(page, form, page.Locator(".zdropedit .textbox").Nth(1), "222", "Second Item");
	}

	[Test, WithPlaywrightPage]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1199:Do Not Use Unnecessary Resource String In Unit Tests", Justification = "Test Translation Feedback")]
	public async Task TestDropEditWithoutDescriptions()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var disposableAction = Db.DisposableActionForDbConnection();
		var dummyWithList = new BusinessObjectFactory().New<DummyWithSettableCodeDescriptionPairList>();
		var list = new CodeDescriptionPairList();
		list.AddPair(ResString.GetMultilingualString("1", "ONE"), "First Item");
		list.AddPair(ResString.GetMultilingualString("2", "TWO"), "Second Item");
		dummyWithList.SetDummyList(list);
		var form = default(TestDropEditForm);
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new TestDropEditForm(dummyWithList);
			form.DropEdit.ShowDescriptionBox = false;
			form.DropEdit.ShowInDropDown = ZDropEdit.ShowInDropDownList.OnlyShowCode;
			form.Disposed += (s, e) => disposableAction.Dispose();
			return form;
		});
		var button = page.Locator(".zdropbutton button");

		await button.ClickAsync(options: ClickOptions);
		await page.Locator(".zdropform tr").Nth(0).ClickAsync(options: ClickOptions);
		await AssertThatTranslationFeedbackWorksAsync(page, form, page.Locator(".zdropedit .textbox").Nth(0), "1", "ONE");

		await button.ClickAsync(options: ClickOptions);
		await page.Locator(".zdropform tr").Nth(1).ClickAsync(options: ClickOptions);
		await AssertThatTranslationFeedbackWorksAsync(page, form, page.Locator(".zdropedit .textbox").Nth(0), "2", "TWO");
	}

	[Test, WithPlaywrightPage]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1199:Do Not Use Unnecessary Resource String In Unit Tests", Justification = "Test Translation Feedback")]
	public async Task TestZMessageBox()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var key = "ZMB";
		var caption = "Zee Message Box";
		var form = default(Form);
		var page = await ctx.LoadFormAsync(() => form = new ZMessageBox(ResString.GetMultilingualString(key, caption), "Test", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk));
		await AssertThatTranslationFeedbackWorksAsync(page, form, page.Locator(".zmessagebox .textbox"), key, caption);
	}

	[Explicit("TranslationFeedback for ZCheckBox is not working for now. There will be work item(s) to fix it.")]
	[Test, WithPlaywrightPage]
	public async Task TestZCheckBox()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var key = "ZC";
		var caption = "Zee CheckBox";
		var form = default(ZDummyForm);
		var page = await ctx.LoadFormAsync(() =>
		{
			var checkBox = new ZCheckBox() { CaptionResourceString = new ResourceStringData(key, caption) };
			checkBox.Click += ControlAction;
			return form = SetupTestForm(checkBox);
		});
		await AssertThatTranslationFeedbackWorksAsync(page, form, page.Locator(".checkbox"), key, caption);
	}

	[Explicit("TranslationFeedback for TabPage is not working for now. There will be work item(s) to fix it.")]
	[Test, WithPlaywrightPage]
	public async Task TestZTabControl()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var form = default(ZDummyForm);
		var page = await ctx.LoadFormAsync(() =>
		{
			var tabControl = new ZTabControl();
			tabControl.TabIndexChanged += ControlAction;
			tabControl.TabPages.Add(new ZTabPage() { CaptionResourceString = new ResourceStringData("1", "First Tab") });
			tabControl.TabPages.Add(new ZTabPage() { CaptionResourceString = new ResourceStringData("2", "Second Tab") });
			return form = SetupTestForm(tabControl);
		});
		await AssertThatTranslationFeedbackWorksAsync(page, form, page.Locator(".tabcontrol__button").Nth(0), "1", "First Tab");
		await AssertThatTranslationFeedbackWorksAsync(page, form, page.Locator(".tabcontrol__button").Nth(1), "2", "Second Tab");
	}

	async Task AssertThatTranslationFeedbackWorksAsync(IPage page, Form form, ILocator locator, string key, string caption)
	{
		await HoldOnF2KeyAsync(page);
		await AssertThatMouseEnterAddsHighlightAsync(locator);
		await AssertThatMouseLeaveRemovesHighlightAsync(locator);
		await ReleaseF2KeyAsync(page);

		await HoldOnF2KeyAsync(page);
		await AssertThatMouseEnterAddsHighlightAsync(locator);
		await AssertThatClickOpensFeedbackAndRemovesHighlightAsync(form, locator, key, caption);
		await ReleaseF2KeyAsync(page);
	}

	async Task AssertThatMouseEnterAddsHighlightAsync(ILocator locator)
	{
		await locator.DispatchEventAsync("mouseenter", options: DispatchEventOptions);
		await Assertions.Expect(locator).ToHaveCSSAsync("cursor", "default", options: ToHaveCSSOptions);
		await Assertions.Expect(locator).ToHaveCSSAsync("background-image", "linear-gradient(rgba(32, 178, 170, 0.5), rgba(32, 178, 170, 0.5))", options: ToHaveCSSOptions);
	}

	async Task AssertThatMouseLeaveRemovesHighlightAsync(ILocator locator)
	{
		await locator.DispatchEventAsync("mouseleave", options: DispatchEventOptions);
		await Assertions.Expect(locator).ToHaveCSSAsync("background-image", "none", options: ToHaveCSSOptions);
		Assert.That(() => locator.GetComputedStyleAsync("cursor"), Is.EqualTo("text").Or.EqualTo("auto").After(1000, 100));
	}

	async Task AssertThatClickOpensFeedbackAndRemovesHighlightAsync(Form form, ILocator locator, string key, string caption)
	{
		var provider = new MockTranslationFeedbackProvider();
		using (ObjectFactory.Substitute<ITranslationFeedbackProvider>(provider))
		{
			var disposable = default(IDisposable);
			await form.InvokeWinzorDispatcherAsync(() => disposable = TranslationFeedbackManager.EnableTranslationFeedbackModeForTest());

			await locator.ClickAsync(options: ClickOptions);

			await Assertions.Expect(locator).ToHaveCSSAsync("background-image", "none", options: ToHaveCSSOptions);
			Assert.That(() => locator.GetComputedStyleAsync("cursor"), Is.EqualTo("text").Or.EqualTo("auto").After(1000, 100));
			Assert.That(() => provider.LastFeedbackKey, Is.EqualTo(key).After(1000, 100));
			Assert.That(() => provider.LastFeedbackCaption, Is.EqualTo(caption).After(1000, 100));

			await form.InvokeWinzorDispatcherAsync(disposable.Dispose);
		}
	}

	async Task HoldOnF2KeyAsync(IPage page)
	{
		await page.FocusAsync(".form");
		await Assertions.Expect(page.Locator(".form")).ToBeFocusedAsync(options: ToBeFocusedOptions);
		await Task.Delay(100);
		await page.Keyboard.DownAsync("F2");
		await Task.Delay(100);
	}

	async Task ReleaseF2KeyAsync(IPage page)
	{
		await page.Keyboard.UpAsync("F2");
	}

	ZDummyForm SetupTestForm(Control control)
	{
		var testForm = new ZDummyForm(new BusinessObjectFactory().New<DummyBusinessObject>());
		testForm.CaptionRenderingEnabled = true;
		testForm.EnableTranslationFeedbackHighlight = true;
		if (control != null)
		{
			testForm.Controls.Add(control);
		}
		return testForm;
	}

	void ControlAction(object sender, EventArgs e)
	{
		throw new Exception("Control should not fire in translation feedback mode");
	}
}
