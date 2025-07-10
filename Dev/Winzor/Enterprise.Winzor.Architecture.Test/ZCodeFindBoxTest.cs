using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bunit;
using CargoWise.Blazor.Client.Integration.Messaging;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Internal.Testing;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules.Testing;
using Microsoft.Playwright;
using Moq;
using NUnit.Framework;
using WinzorFramework;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace Enterprise.Winzor.Architecture.Test;

using static Enterprise.ZArchitecture.GUI.Internal.ZFindBoxUserControl;

class ZCodeFindBoxTest
{
	[Test]
	public async Task ZCodeBoxTestPressF4ShowModal()
	{
		Task loadRequestTask = null;
		var windowService = new Mock<IWindowService>();
		windowService
			.Setup(o => o.RequestCreateAndShowWindowAsync(It.IsAny<CreateWindowOptions>(), It.IsAny<ShowWindowOptions>(), It.IsAny<WindowStyleOptions>()))
			.Callback<CreateWindowOptions, ShowWindowOptions, WindowStyleOptions>((createWindowOptions, showWindowOptions, windowStyleOptions) =>
			{
				loadRequestTask = Task.Run(() =>
				{
					using var ctxMock = new EnterpriseTestContext();
					var renderedNewForm = ctxMock.RenderEntryPointComponent(createWindowOptions.Uri.ToString());
					renderedNewForm.WaitForState(() => renderedNewForm.Instance.Form != null);
					Assert.That(renderedNewForm.Instance.Form, Is.InstanceOf<EmbeddedModulePopup>());
					_ = ctxMock.WinzorDispatcher.InvokeAsync(renderedNewForm.Instance.Form.Dispose);
				});
			});

		var clientServices = MockCargoWiseClientServices.MakeMock(windowService: windowService.Object);

		using var ctx = new EnterpriseTestContext();
		ZCodeBox codeFindBox = null;
		var component = await ctx.RenderFormAsync(() =>
		{
			var form = new WinzorTestForm();
			var parentCodeFindBox = new ZCodeFindBox { ModuleID = DummyModuleIDs.Dummy };
			codeFindBox = new ZCodeBox(parentCodeFindBox);
			form.Controls.Add(parentCodeFindBox);
			return form;
		}, clientServices);

		var input = component.Find("input");
		Assert.That(codeFindBox.Hotkeys.IsRegistered(Keys.F4), Is.True);

		await input.TriggerEventAsync("onwinzorfocusin", new WinzorFocusInEventArgs());
		await component.KeyPressAsync(Keys.F4, input);

		Assert.That(await loadRequestTask.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);
	}

	[Test]
	public async Task ZCodeBoxTestPressF3ShowForm()
	{
		Task loadRequestTask = null;
		var windowService = new Mock<IWindowService>();
		windowService
			.Setup(o => o.RequestCreateAndShowWindowAsync(It.IsAny<CreateWindowOptions>(), It.IsAny<ShowWindowOptions>(), It.IsAny<WindowStyleOptions>()))
			.Callback<CreateWindowOptions, ShowWindowOptions, WindowStyleOptions>((createWindowOptions, showWindowOptions, windowStyleOptions) =>
			{
				loadRequestTask = Task.Run(() =>
				{
					using var ctxMock = new EnterpriseTestContext();
					var renderedNewForm = ctxMock.RenderEntryPointComponent(createWindowOptions.Uri.ToString());
					renderedNewForm.WaitForState(() => renderedNewForm.Instance.Form != null);
					Assert.That(renderedNewForm.Instance.Form, Is.InstanceOf<ZDummyForm>());
					_ = ctxMock.WinzorDispatcher.InvokeAsync(renderedNewForm.Instance.Form.Dispose);
				});
			});

		var clientServices = MockCargoWiseClientServices.MakeMock(windowService: windowService.Object);

		using var ctx = new EnterpriseTestContext();
		ZCodeBox codeFindBox = null;
		var component = await ctx.RenderFormAsync(() =>
		{
			var form = new WinzorTestForm();
			var parentCodeFindBox = new ZCodeFindBox { ModuleID = DummyModuleIDs.Dummy };
			codeFindBox = new ZCodeBox(parentCodeFindBox);
			form.Controls.Add(parentCodeFindBox);
			return form;
		}, clientServices);

		var input = component.Find("input");
		Assert.That(codeFindBox.Hotkeys.IsRegistered(Keys.F3), Is.True);

		await input.TriggerEventAsync("onwinzorfocusin", new WinzorFocusInEventArgs());
		await component.KeyPressAsync(Keys.F3, input);

		Assert.That(await loadRequestTask.WithTimeout(TimeSpan.FromSeconds(3)), Is.True);
	}

	[Test]
	public async Task ZCodeFindBoxNegativeWidthDescriptionBoxWidth()
	{
		using var ctx = new EnterpriseTestContext();
		ZCodeFindBox parentCodeFindBox = null;
		ZCodeBox codeFindBox = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new WinzorTestForm();
			parentCodeFindBox = new ZCodeFindBox { ModuleID = DummyModuleIDs.Dummy, Width = 70 };
			codeFindBox = new ZCodeBox(parentCodeFindBox);
			codeFindBox.Width = 10;
			parentCodeFindBox.DescriptionBox.Width = -60;
			parentCodeFindBox.ShowDescriptionBox = true;
			form.Controls.Add(parentCodeFindBox);
			return form;
		});

		Assert.That(parentCodeFindBox.Width, Is.GreaterThan(70));
		Assert.That(rendered.Find("div[data-type='Enterprise.ZArchitecture.GUI.ZCodeFindBox']").GetAttribute("style"), Does.Contain($"width:{parentCodeFindBox.Width}px"));
	}

	[Test]
	public async Task ZCodeFindBoxCaptureElementReferenceTrue()
	{
		using var ctx = new EnterpriseTestContext();
		ZCodeFindBox codeFindBox = null;
		var rendered = await ctx.RenderControlOnFormAsync(() =>
		{
			codeFindBox = new ZCodeFindBox { ModuleID = DummyModuleIDs.Dummy, Width = 70 };
			return codeFindBox;
		});

		Assert.That(codeFindBox.CaptureElementReference, Is.EqualTo(true));
	}

	[Test, WithPlaywrightPage]
	public async Task ZCodeFindBoxAutoComplete()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var dataSource = factory.New<FindBoxDummyBusinessObject>();
			var dummy = factory.LoadTop1<DummyBusinessObject>(new ZQuery());
			dummy.Z0_Code = "AABCD";
			var form = new ZForm();
			form.SetDataBinding(dataSource, "");
			var findBox = new ZCodeFindBox();
			findBox.BindTo = "SS_Dummy";
			findBox.BindToList = "Dummies";
			form.Controls.Add(findBox);
			return form;
		});
		var input = await page.WaitForSelectorAsync("input");
		var selectedValue = await input.InputValueAsync();
		Assert.That(selectedValue, Is.Empty);

		await input.PressAsync("=");
		await page.WaitForFunctionAsync("document.querySelector('input').value==='AABCD'");
		await page.Keyboard.PressAsync("Tab", new KeyboardPressOptions { Delay = 500 });
		await page.WaitForFunctionAsync("document.activeElement.value!=='AABCD'");

		input = await page.WaitForSelectorAsync("input");
		selectedValue = await input.InputValueAsync();
		Assert.That(selectedValue, Is.EqualTo("AABCD"));
	}

	[Test, WithPlaywrightPage]
	public async Task ZCodeFindBoxEquationTriggerAutoComplete()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var dataSource = factory.New<FindBoxDummyBusinessObject>();

			var dummy1 = dataSource.DummiesForBindToListChange.AddNew();
			dummy1.Z0_Code = "AABCD";
			dummy1.Z0_Description = "AABCD";

			var dummy2 = dataSource.DummiesForBindToListChange.AddNew();
			dummy2.Z0_Code = "BBDDD";
			dummy2.Z0_Description = "BBDDD";

			var form = new ZForm();
			form.SetDataBinding(dataSource, "");
			var findBox = new ZCodeFindBox();
			findBox.BindTo = "SS_Dummy";
			findBox.BindToList = "Dummies";
			form.Controls.Add(findBox);
			return form;
		});

		var input = await page.WaitForSelectorAsync("input");
		Assert.That(await input.InputValueAsync(), Is.Empty);

		await input.PressAsync("B", new ElementHandlePressOptions { Delay = 500 });
		await input.PressAsync("=");
		Assert.That(async () => await input.InputValueAsync(), Is.EqualTo("BBDDD").After(1000, 200), "autocomplete should works.");

		await page.FillAsync("input", "");

		await input.PressAsync("=", new ElementHandlePressOptions { Delay = 500 });
		await input.PressAsync("A");
		Assert.That(async () => await input.InputValueAsync(), Is.EqualTo("A").After(1000, 200), "autocomplete text overlaped by next typing.");

		await page.FillAsync("input", "");

		await input.PressAsync("=", new ElementHandlePressOptions { Delay = 500 });
		await input.PressAsync("Tab");
		Assert.That(async () => await input.InputValueAsync(), Is.EqualTo("AABCD").After(1000, 200), "autocomplete should works after pressing tab.");
	}

	[Test, WithPlaywrightPage]
	public async Task ZCodeFindBoxCodeChangeShouldTriggerRender()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		ZCodeFindBox findBox = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var dataSource = factory.New<DummyWithCodes>();
			dataSource.Code = "TEST";
			var form = new ZForm();
			findBox = new ZCodeFindBox();
			findBox.BindTo = "Code";
			findBox.BindToForDescription = "Code";
			findBox.SetDataBinding(dataSource, "Code");
			form.Controls.Add(findBox);
			return form;
		});

		await findBox.InvokeWinzorDispatcherAsync(() =>
		{
			findBox.CodeBox.Text = "TEST";
		});

		UserIdleWorker.Flush();

		var input = await page.WaitForSelectorAsync("input");
		var selectedValue = await input.InputValueAsync();
		Assert.That(selectedValue, Is.EqualTo("TEST"));
	}

	[Test, WithPlaywrightPage]
	public async Task ZCodeFindBoxCodeAndDescriptionChangeAreSynchronized()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var dataSource = factory.New<FindBoxDummyBusinessObject>();
			var dummy = factory.LoadTop1<DummyBusinessObject>(new ZQuery());
			dummy.Z0_Code = "Hello";
			dummy.Z0_Description = "Kitty";
			var form = new ZForm();
			form.SetDataBinding(dataSource, "");
			var findBox = new ZCodeFindBox();
			findBox.BindTo = "SS_Dummy";
			findBox.BindToList = "Dummies";
			form.Controls.Add(findBox);
			return form;
		});

		var codeBox = page.Locator("input:nth-of-type(1)");
		var descriptionBox = page.Locator("input:nth-of-type(2)");

		await codeBox.PressSequentiallyAsync("Hello");
		await page.WaitForTimeoutAsync(1000);
		await descriptionBox.FocusAsync();

		await Assertions.Expect(descriptionBox).ToHaveValueAsync("Kitty");

		await codeBox.SelectTextAsync();
		await codeBox.PressAsync("Delete", new () { Delay = 10 });
		await page.WaitForTimeoutAsync(1000);
		await descriptionBox.FocusAsync();

		await Assertions.Expect(descriptionBox).ToHaveValueAsync("{None Selected}");
	}
}
