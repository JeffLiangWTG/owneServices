using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Blazor.Client.Integration.Messaging;
using Moq;
using NUnit.Framework;
using WinzorTestFramework;

namespace WinzorFramework;

public class FormOpenerTest
{
	[Test]
	public async Task TestOpenFormCallsRequestShowWindowAsyncWithCorrectClientWindowOpenOptionsAsync([Values] bool isModal)
	{
		using var ctx = new WinzorTestContext();

		var component1 = await ctx.RenderFormAsync(() => new Form());
		var form = component1.GetForm();

		ShowWindowOptions expectedShowWindowOptions = null;
		WindowStyleOptions expectedWindowStyleOptions = null;
		Uri uri = null;

		await form.InvokeWinzorDispatcherAsync(() =>
		{
			var form2 = new Form() { ModalWindow = isModal };
			uri = form.WinzorDispatcher.FormInstanceRegister.Add(form.CargoWiseClientServices.ServerBaseUri, form2, out var isNewForm, out _);
			var formOpener = new FormOpener();
			formOpener.OpenForm(WinzorDispatcher.Current.CurrentContext, uri, form, form2);

			expectedShowWindowOptions = form2.GenerateShowWindowOptions();
			expectedWindowStyleOptions = form2.GenerateWindowStyleOptions();
		});

		ctx.MockCargoWiseClientServices.WindowService.Verify(s => s.RequestCreateAndShowWindowAsync(
			It.Is<CreateWindowOptions>(o => o.Uri == uri),
			expectedShowWindowOptions,
			expectedWindowStyleOptions), Times.Once);
	}

	[Test]
	public async Task TestOpenFormThrowsIfCargoWiseClientServicesAreNull()
	{
		var formRegister = new RegisteredFormInstances();
		var mockContext = new Mock<ServerInitiatedCallbackContext>();
		using var dispatcher = new WinzorDispatcher(new FormOpener(), formRegister);

		await dispatcher.InvokeAsync(() =>
		{
			using var context = dispatcher.WithContext(mockContext.Object);
			var serverBaseUri = new Uri("https://test.wisecloud.com/");
			var formOpener = new FormOpener();
			var form = new Form();
			var form2 = new Form();
			var uri = new RegisteredFormInstances().Add(serverBaseUri, form2, out var isNewForm, out _);
			Assert.That(() => formOpener.OpenForm(dispatcher.CurrentContext, uri, form, form2), Throws.ArgumentException);
		});
	}

	[Test]
	public async Task RequestShowWindowAsyncWithCorrectArguments([Values] bool modal)
	{
		using var ctx = new WinzorTestContext();
		var serverBaseUri = new Uri("https://test.wisecloud.com/");
		var register = new Mock<IFormInstanceRegister>();
		var formOpener = new FormOpener();
		var form = (await ctx.RenderFormAsync(() => new Form())).GetForm();
		var isNewForm = false;
		var isNewActiveForm = false;
		var formUri = new UriBuilder(serverBaseUri)
		{
			Path = "form"
		}.Uri;
		register.Setup(a => a.Add(serverBaseUri, form, out isNewActiveForm, out isNewForm)).Returns(formUri);

		ShowWindowOptions expectedShowWindowOptions = null;
		WindowStyleOptions expectedWindowStyleOptions = null;

		Uri uri = null;
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			form.ModalWindow = modal;
			expectedShowWindowOptions = form.GenerateShowWindowOptions();
			expectedWindowStyleOptions = form.GenerateWindowStyleOptions();
			uri = register.Object.Add(serverBaseUri, form, out isNewActiveForm, out isNewForm);
		});

		await formOpener.OpenFormAsync(uri, ctx.MockCargoWiseClientServices.WindowService.Object, form);

		ctx.MockCargoWiseClientServices.WindowService.Verify(s => s.RequestCreateAndShowWindowAsync(
			It.Is<CreateWindowOptions>(o => o.Uri == formUri),
			expectedShowWindowOptions,
			expectedWindowStyleOptions), Times.Once);
		register.Verify(r => r.Add(serverBaseUri, form, out isNewActiveForm, out isNewForm), Times.Once);
	}

	[Test]
	public async Task TestOpenFormAsyncDoesNotSendRequestForDisposedForm()
	{
		using var ctx = new WinzorTestContext();
		var serverBaseUri = new Uri("https://test.wisecloud.com/");
		var register = new Mock<IFormInstanceRegister>();
		var formOpener = new FormOpener();
		var windowService = new Mock<IWindowService>();
		var form = (await ctx.RenderFormAsync(
			() =>
			{
				var form = new Form();
				return form;
			})).GetForm();

		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			form.Dispose();
		});

		Uri uri = null;
		var isNewForm = false;
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			uri = register.Object.Add(serverBaseUri, form, out isNewForm, out _);
		});

		await formOpener.OpenFormAsync(uri, windowService.Object, form);

		ctx.MockCargoWiseClientServices.WindowService.Verify(s => s.RequestCreateAndShowWindowAsync(
			It.IsAny<CreateWindowOptions>(), It.IsAny<ShowWindowOptions>(), It.IsAny<WindowStyleOptions>()), Times.Never);
		register.Verify(r => r.Add(It.IsAny<Uri>(), It.IsAny<Form>()), Times.Never);
	}

	[Test]
	public async Task TestOpenFormAsyncDoesNotSendRequestForClosingForm()
	{
		using var ctx = new WinzorTestContext();
		var serverBaseUri = new Uri("https://test.wisecloud.com/");
		var register = new Mock<IFormInstanceRegister>();
		var formOpener = new FormOpener();
		var windowService = new Mock<IWindowService>();
		using var testCompleteCts = new CancellationTokenSource();

		try
		{
			var form = (await ctx.RenderFormAsync(
				() =>
				{
					var form = new Form();
					form.Closing += (sender, args) => ctx.WinzorDispatcher.RunMessageLoop(testCompleteCts);
					return form;
				})).GetForm();

			ctx.WinzorDispatcher.Queue(() =>
			{
				form.Close();
			});

			Uri uri = null;
			var isNewForm = false;
			await ctx.WinzorDispatcher.InvokeAsync(() =>
			{
				uri = register.Object.Add(serverBaseUri, form, out isNewForm, out _);
			});

			await formOpener.OpenFormAsync(uri, windowService.Object, form);

			ctx.MockCargoWiseClientServices.WindowService.Verify(s => s.RequestCreateAndShowWindowAsync(
				It.IsAny<CreateWindowOptions>(), It.IsAny<ShowWindowOptions>(), It.IsAny<WindowStyleOptions>()), Times.Never);
			register.Verify(r => r.Add(It.IsAny<Uri>(), It.IsAny<Form>()), Times.Never);
		}
		finally
		{
			await testCompleteCts.CancelAsync();
		}
	}
}
