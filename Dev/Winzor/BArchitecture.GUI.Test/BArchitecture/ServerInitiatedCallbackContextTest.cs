using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Blazor.Client.Integration.Messaging;
using Moq;
using NUnit.Framework;
using WinzorTestFramework;

namespace WinzorFramework;

class ServerInitiatedCallbackContextTest
{
	[Test]
	public async Task FormShouldOnlyIncludeRenderedForm()
	{
		using var ctx = new WinzorTestContext();
		Form form = null;
		Form formInContext = null;
		var rendered = await ctx.RenderFormAsync(() => form = new Form());

		await form.InvokeWinzorDispatcherAsync(() => { form.Hide(); });

		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			using (ctx.WinzorDispatcher.WithContext(new ServerInitiatedCallbackContext()))
			{
				formInContext = WinzorDispatcher.Current.CurrentContext.Form;
			}
		});
		Assert.That(formInContext, Is.Null);
	}

	[Test]
	public async Task FormShouldRunMessageLoop()
	{
		using var ctx = new WinzorTestContext();
		Form form = null;
		Form modalForm = null;

		var windowService = new Mock<IWindowService>();
		var closeFormTcs = new TaskCompletionSource();
		windowService
			.Setup(o => o.RequestCreateAndShowWindowAsync(It.IsAny<CreateWindowOptions>(), It.IsAny<ShowWindowOptions>(), It.IsAny<WindowStyleOptions>()))
			.Callback<CreateWindowOptions, ShowWindowOptions, WindowStyleOptions>((createWindowOptions, showWindowOptions, windowStyleOptions) =>
			{
				var form = ((RegisteredFormInstances)ctx.WinzorDispatcher.FormInstanceRegister).Lookup(createWindowOptions.Uri);
				form.Close();
			});
		var clientServices = MockCargoWiseClientServices.MakeMock(windowService: windowService.Object);

		var rendered = await ctx.RenderFormAsync(() => form = new Form(), clientServices);
		var mockContext = new Mock<ServerInitiatedCallbackContext>();
		mockContext.Setup(m => m.OnEnterMessageLoop()).Verifiable();

		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			using (ctx.WinzorDispatcher.WithContext(mockContext.Object))
			{
				modalForm = new Form();
				modalForm.ShowDialog();
			}
		});

		mockContext.Verify(m => m.OnEnterMessageLoop(), Times.Once);
	}

	[Test]
	public async Task IsParentActiveForModelFormWithServerInitiatedCallbackContextOnSeperateThread()
	{
		using var ctx = new WinzorTestContext();
		Form modalForm = null;
		var openWindowTaskComplete = new TaskCompletionSource<bool>();

		var windowService = new Mock<IWindowService>();
		windowService
			.Setup(o => o.RequestCreateAndShowWindowAsync(It.IsAny<CreateWindowOptions>(), It.IsAny<ShowWindowOptions>(), It.IsAny<WindowStyleOptions>()))
			.Callback<CreateWindowOptions, ShowWindowOptions, WindowStyleOptions>((createWindowOptions, showWindowOptions, windowStyleOptions) =>
			{
				openWindowTaskComplete.SetResult(true);
				modalForm.Close();
			});
		var clientServices = MockCargoWiseClientServices.MakeMock(windowService: windowService.Object);

		var rendered = await ctx.RenderFormAsync(() => new Form(), clientServices);
		var form = rendered.GetForm();

		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			using (ctx.WinzorDispatcher.WithContext(new ServerInitiatedCallbackContext()))
			{
				var previousDispatcherContext = WinzorDispatcher.Current.CurrentContext;

				using var winzorDispatcher = new WinzorDispatcher(
					WinzorDispatcher.Current.FormOpener,
					WinzorDispatcher.Current.FormInstanceRegister,
					"Test",
					isBackgroundThread: true);
				var modalFormShowTask = winzorDispatcher.InvokeAsync(() =>
				{
					using var context = winzorDispatcher.WithContext(previousDispatcherContext);
					modalForm = new Form();
					modalForm.Parent = form;
					modalForm.ShowDialog();
				});
				modalFormShowTask.Wait();
			}
		});
		Assert.That(await openWindowTaskComplete.Task.WithTimeout(TimeSpan.FromSeconds(2)), Is.True);
		windowService.Verify(s => s.RequestCreateAndShowWindowAsync(It.IsAny<CreateWindowOptions>(), It.Is<ShowWindowOptions>(o => !o.Modal), It.IsAny<WindowStyleOptions>()), Times.Once);
	}

	[Test]
	public async Task IsParentActiveForModelFormWithServerInitiatedCallbackContextOnSameThread()
	{
		using var ctx = new WinzorTestContext();
		Form modalForm = null;
		var windowService = new Mock<IWindowService>();
		windowService
			.Setup(o => o.RequestCreateAndShowWindowAsync(It.IsAny<CreateWindowOptions>(), It.IsAny<ShowWindowOptions>(), It.IsAny<WindowStyleOptions>()))
			.Callback<CreateWindowOptions, ShowWindowOptions, WindowStyleOptions>((createWindowOptions, showWindowOptions, windowStyleOptions) =>
			{
				modalForm.Close();
			});
		var clientServices = MockCargoWiseClientServices.MakeMock(windowService: windowService.Object);

		await ctx.RenderFormAsync(() => new Form(), clientServices);

		await ctx.WinzorDispatcher.InvokeAsync(() =>
		{
			using (ctx.WinzorDispatcher.WithContext(new ServerInitiatedCallbackContext()))
			{
				modalForm = new Form();
				modalForm.ShowDialog();
			}
		});

		windowService.Verify(s => s.RequestCreateAndShowWindowAsync(It.IsAny<CreateWindowOptions>(), It.Is<ShowWindowOptions>(o => o.Modal), It.IsAny<WindowStyleOptions>()), Times.Once);
	}
}
