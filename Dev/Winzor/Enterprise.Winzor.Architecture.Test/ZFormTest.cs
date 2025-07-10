using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Blazor.Client.Integration.Messaging;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Microsoft.Playwright;
using Moq;
using NUnit.Framework;
using WinzorFramework.JSInterop;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace Enterprise.Winzor.Architecture.Test;
public class ZFormTest
{
	[Test]
	public async Task TestTriggerResizeCompleteOnBrowserSizeChangedAsyncEventDispatcher()
	{
		ZForm form = null;
		var resizeComplete = false;
		using var ctx = new EnterpriseTestContext();
		var rendered = await ctx.RenderFormAsync(() =>
		{
			form = new ZForm { ClientSize = new Size(1000, 1000) };
			form.ResizeComplete += (o, e) => resizeComplete = true;
			form.ClientSize = new Size(2000, 2000);
			return form;
		});
		await form.OnBrowserSizeChangedAsync(2000, 2000);

		Assert.That(resizeComplete, Is.True);
	}

	[Test, WithPlaywrightPage]
	public async Task TestPressControlShiftFShouldShowControlInformationOverlayComponentAsync()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() => CreateTestZForm());
		var formElement = page.Locator(".form");
		var parentElement = page.Locator("body > div.form > div:nth-child(2)");
		var childElement = page.Locator("body > div.form > div:nth-child(2) > button");

		await formElement.WaitForAsync();
		await formElement.ClickAsync();

		await AssertElementOverlayAsync(page, childElement, parentElement);
		await AssertElementOverlayAsync(page, parentElement, formElement);
		await AssertElementOverlayAsync(page, formElement, formElement);
	}

	[Test, WithPlaywrightPage]
	public async Task TestOverlayComponentClickedShouldShowInfoDiggerFormAsync()
	{
		var openFormRequest = new TaskCompletionSource();
		var clientServiceProvider = new MockCargoWiseClientSeviceProvider();
		clientServiceProvider.MockWindowService
			.Setup(o => o.RequestCreateAndShowWindowAsync(It.IsAny<CreateWindowOptions>(), It.IsAny<ShowWindowOptions>(), It.IsAny<WindowStyleOptions>()))
			.Callback<CreateWindowOptions, ShowWindowOptions, WindowStyleOptions>((createWindowOptions, showWindowOptions, windowStyleOptions) => openFormRequest.SetResult());

		await using var ctx = new InMemoryAppServerTestContext(clientServiceProvider);
		var page = await ctx.LoadFormAsync(() => CreateTestZForm());
		var childElement = page.Locator("body > div.form > div:nth-child(2) > button");
		var formElement = page.Locator(".form");

		await formElement.ClickAsync();
		await PressControlShiftFAsync(page);
		await MouseMoveToElementAsync(page, childElement);
		await page.Locator(".form--overlay--mask").WaitForAsync();
		await page.Mouse.DownAsync();

		Assert.That(await openFormRequest.Task.WithTimeout(TimeSpan.FromSeconds(10)), Is.True);

		await page.Mouse.UpAsync();
		await ReleaseControlShiftFAsync(page);
	}

	[Test, WithPlaywrightPage]
	public async Task TestPressControlShiftFWithUnsetMousePositionDoesNotThrow()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(CreateTestZForm);
		Assert.That(async () => await page.Locator(".form").PressAsync("Control+Shift+F"), Throws.Nothing);
	}

	[Test, WithPlaywrightPage]
	public async Task TestDropDangerousOrLargeFilesOntoFormShouldShowWarningBox()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var form = default(ZForm);
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new ZForm();
			form.AllowDrop = true;
			return form;
		});

		var fileServiceMock = new Mock<IFileService>();
		fileServiceMock
			.Setup(i => i.UploadFilesToServerAsync(It.IsAny<BrowserFile[]>(), It.IsAny<long>(), It.IsAny<CancellationToken>()))
			.Returns<BrowserFile[], long, CancellationToken>((files, _, _) => Task.FromResult(files.Select(f => f.Name).ToArray()));
		form.CargoWiseClientServices.FileService = fileServiceMock.Object;

		var files = new[]
		{
			new BrowserFile { Name = "LargeFile.txt", Size = long.MaxValue },
			new BrowserFile { Name = "DangerousFile.exe", Size = 1024 }
		};
		var formLocator = page.Locator(".form");
		await form.OnDropFilesAsync(files, 0, 0, form.WinzorControlId);

		Assert.That(UnitTestUserNotification.Instance.LastMessage.Text, Is.EqualTo("The following files were not added because they are potentially dangerous file types:\r\nDangerousFile.exe\r\n\r\nThe following files are larger than the maximum file size (10MB) specified in the registry 'System -> DocManager -> eDocs Maximum File Size':\r\nLargeFile.txt\r\n"));
	}

	[Test, WithPlaywrightPage, UseSnapshotProtection, Explicit]
	public async Task TestLastActiveFormInOpenedFormCacheShouldPopulateCorrectly()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		var page = await ctx.LoadFormAsync(() =>
		{
			var factory = new BusinessObjectFactory();
			var cashAccount = factory.NewWithValidTestData<AccBankAccount>();
			cashAccount.AB_AccountType = AccountTypeCodeDescriptionPairList.Codes.CSH;
			cashAccount.AB_Desc = "Test Cash Account";
			factory.Save();

			var form = new AccBankAccountForm(cashAccount);
			return form;
		});

		var newButton = page.Locator("div[data-name=\"ButtonsUserControl\"] .button__button").First;
		await newButton.ClickAsync();
		Assert.That(() => (AccBankAccountForm)OpenedFormCache.LastActiveForm.Target, Is.Not.Null.After(2000, 100));
	}

	async Task AssertElementOverlayAsync(IPage page, ILocator element, ILocator parentElement)
	{
		var containerElement = page.Locator(".form--overlay--container");
		var maskElement = page.Locator(".form--overlay--mask");
		var borderElement = page.Locator(".form--overlay--border");
		var infocardElement = page.Locator(".form--overlay--infocard");
		var elementBounding = await element.BoundingBoxAsync();
		Assert.That((await containerElement.GetComputedStyleAsync("display")).Raw, Is.EqualTo("none"));

		await PressControlShiftFAsync(page);
		await MouseMoveToElementAsync(page, element);
		Assert.That(async () => (await containerElement.GetComputedStyleAsync("display")).Raw, Is.EqualTo("block").After(3000, 300));
		Assert.That(async () => (await maskElement.GetComputedStyleAsync("background-color")).Raw, Is.EqualTo("rgb(32, 178, 170)"));
		Assert.That(async () => (await maskElement.GetComputedStyleAsync("border-color")).Raw, Is.EqualTo("rgb(50, 205, 50)"));
		Assert.That(async () => (await maskElement.GetComputedStyleAsync("opacity")).Raw, Is.EqualTo("0.8"));
		Assert.That(async () => (await borderElement.GetComputedStyleAsync("border-color")).Raw, Is.EqualTo("rgb(255, 0, 255)"));
		Assert.That(async () => await CompareBoundingBoxAsync(maskElement, element), Is.EqualTo(true));
		Assert.That(async () => await CompareBoundingBoxAsync(borderElement, parentElement), Is.EqualTo(true));

		// Info Card is not fully implemented at the moment, so this is just a test to see if Info Card is displayed,
		// and will continue to be refined in subsequent WIs.
		Assert.That(async () => await infocardElement.TextContentAsync(), Does.Contain("Name: "));
		Assert.That(async () => await infocardElement.TextContentAsync(), Does.Contain("Type: "));
		Assert.That(async () => await infocardElement.TextContentAsync(), Does.Contain("Location: "));
		Assert.That(async () => await infocardElement.TextContentAsync(), Does.Contain("Size: "));
		Assert.That(async () => await infocardElement.TextContentAsync(), Does.Contain("Padding (NESW): "));
		Assert.That(async () => await infocardElement.TextContentAsync(), Does.Contain("Margin (NESW): "));
		Assert.That(async () => await infocardElement.TextContentAsync(), Does.Contain("Click for more details..."));

		await ReleaseControlShiftFAsync(page);
		Assert.That(async () => (await containerElement.GetComputedStyleAsync("display")).Raw, Is.EqualTo("none").After(3000, 300));
	}

	async Task<bool> CompareBoundingBoxAsync(ILocator overlay, ILocator element)
	{
		var overlayBox = await overlay.BoundingBoxAsync();
		var elementBox = await element.BoundingBoxAsync();

		return elementBox.X == overlayBox.X && elementBox.Y == overlayBox.Y &&
			elementBox.Width == overlayBox.Width && elementBox.Height == overlayBox.Height;
	}

	async Task MouseMoveToElementAsync(IPage page, ILocator element)
	{
		var boundingBox = await element.BoundingBoxAsync();
		await page.Mouse.MoveAsync(boundingBox.X + 15, boundingBox.Y + 15);
		await Task.Delay(500);
	}

	async Task PressControlShiftFAsync(IPage page)
	{
		await page.Keyboard.DownAsync("Control");
		await page.Keyboard.DownAsync("Shift");
		await page.Keyboard.DownAsync("F");
		await Task.Delay(500);
	}

	async Task ReleaseControlShiftFAsync(IPage page)
	{
		await page.Keyboard.UpAsync("Control");
		await page.Keyboard.UpAsync("Shift");
		await page.Keyboard.UpAsync("F");
	}

	ZForm CreateTestZForm()
	{
		var form = new ZForm()
		{
			Name = "Form",
			Size = new Size(500, 500)
		};

		var parentControl = new Control()
		{
			BackColor = Color.Black,
			Name = "Parent",
			Size = new Size(400, 400),
			Location = new Point(50, 50)
		};

		var childControl = new Button()
		{
			Name = "Child",
			Text = "Button",
			Size = new Size(300, 300),
			Location = new Point(50, 50)
		};

		parentControl.Controls.Add(childControl);
		form.Controls.Add(parentControl);

		return form;
	}
}
