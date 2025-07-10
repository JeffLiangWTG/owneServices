using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Blazor.Client.Integration.Messaging;
using Moq;
using NUnit.Framework;
using WinzorFramework;
using WinzorTestFramework;

namespace Enterprise.Winzor.Architecture.Test;

public class FormOpenerTest
{
	[Test]
	public async Task TestOpenFormThrowsIfCargoWiseClientServicesAreNull()
	{
		using var winzorDispatcher = new WinzorDispatcher(new FormOpener(), new RegisteredFormInstances());
		var mockContext = new Mock<ServerInitiatedCallbackContext>();

		await winzorDispatcher.InvokeAsync(() =>
		{
			using var context = winzorDispatcher.WithContext(mockContext.Object);
			var serverBaseUri = new Uri("https://test.wisecloud.com/");
			var formOpener = new FormOpener();
			var form = new Form();
			var form2 = new Form();
			var uri = form.WinzorDispatcher.FormInstanceRegister.Add(serverBaseUri, form2, out var isNewForm, out _);
			Assert.That(() => formOpener.OpenForm(winzorDispatcher.CurrentContext, uri, form, form2), Throws.ArgumentException);
		});
	}

	[Test]
	public async Task RequestShowWindowAsyncWithCorrectArguments([Values] bool modal)
	{
		using var ctx = new EnterpriseTestContext();
		var serverBaseUri = new Uri("https://test.wisecloud.com/");
		var register = new Mock<IFormInstanceRegister>();
		var formOpener = new FormOpener();
		var form = (await ctx.RenderFormAsync(() => new Form())).GetForm();
		var isNewActiveForm = false;
		var isNewForm = false;
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
			expectedShowWindowOptions = form.GenerateShowWindowOptions();
			expectedWindowStyleOptions = form.GenerateWindowStyleOptions();
			uri = register.Object.Add(serverBaseUri, form, out isNewActiveForm, out isNewForm);
		});

		await formOpener.OpenFormAsync(uri, ctx.MockCargoWiseClientServices.WindowService.Object, form);

		ctx.MockCargoWiseClientServices.WindowService.Verify(s => s.RequestCreateAndShowWindowAsync(
			It.Is<CreateWindowOptions>(o => o.Uri == formUri),
			expectedShowWindowOptions,
			expectedWindowStyleOptions),
			Times.Once);
		register.Verify(r => r.Add(serverBaseUri, form, out isNewActiveForm, out isNewForm), Times.Once);
	}

	[Test]
	public async Task TestOpenFormAsyncDoesNotSendRequestForDisposedForm()
	{
		using var ctx = new EnterpriseTestContext();
		var serverBaseUri = new Uri("https://test.wisecloud.com/");
		var register = new Mock<IFormInstanceRegister>();
		var formOpener = new FormOpener();
		var windowService = new Mock<IWindowService>();
		var form = (await ctx.RenderFormAsync(() => new Form())).GetForm();

		await ctx.WinzorDispatcher.InvokeAsync(() => form.Dispose());

		Uri uri = null;
		var isNewActiveForm = false;
		var isNewForm = false;
		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			uri = register.Object.Add(serverBaseUri, form, out isNewActiveForm, out isNewForm);
		});

		await formOpener.OpenFormAsync(uri, windowService.Object, form);

		ctx.MockCargoWiseClientServices.WindowService.Verify(s => s.RequestCreateAndShowWindowAsync(It.IsAny<CreateWindowOptions>(), It.IsAny<ShowWindowOptions>(), It.IsAny<WindowStyleOptions>()), Times.Never);
		register.Verify(r => r.Add(It.IsAny<Uri>(), It.IsAny<Form>()), Times.Never);
	}

	[Test]
	public async Task TestOpenFormAsyncDoesNotSendRequestForClosingForm()
	{
		using var ctx = new EnterpriseTestContext();
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

			ctx.WinzorDispatcher.Queue(() => form.Close());

			Uri uri = null;
			var isNewActiveForm = false;
			var isNewForm = false;
			await ctx.WinzorDispatcher.InvokeAsync(() =>
			{
				uri = register.Object.Add(serverBaseUri, form, out isNewActiveForm, out isNewForm);
			});

			await formOpener.OpenFormAsync(uri, windowService.Object, form);

			ctx.MockCargoWiseClientServices.WindowService.Verify(s => s.RequestCreateAndShowWindowAsync(It.IsAny<CreateWindowOptions>(), It.IsAny<ShowWindowOptions>(), It.IsAny<WindowStyleOptions>()), Times.Never);
			register.Verify(r => r.Add(It.IsAny<Uri>(), It.IsAny<Form>()), Times.Never);
		}
		finally
		{
			await testCompleteCts.CancelAsync();
		}
	}
}
