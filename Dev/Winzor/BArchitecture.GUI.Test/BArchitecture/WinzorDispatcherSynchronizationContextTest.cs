using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Bunit;
using CargoWise.Blazor.Client.Integration.Messaging;
using Moq;
using NUnit.Framework;
using WinzorTestFramework;

namespace WinzorFramework;

class WinzorDispatcherSynchronizationContextTest
{
	[Test]
	public async Task UpdateControlFromPost()
	{
		using var ctx = new WinzorTestContext();
		Label label = null;
		var rendered = await ctx.RenderControlOnFormAsync(() => label = new Label() { Text = "Foo" });
		Assert.That(rendered.Find(".label__text").InnerHtml, Is.EqualTo("Foo"));
		SynchronizationContext syncContext = null;
		await ctx.WinzorDispatcher.InvokeAsync(() => syncContext = SynchronizationContext.Current);
		syncContext.Post(_ => label.Text = "Bar", null);
		await ctx.WinzorDispatcher.InvokeAsync(Application.DoEvents);
		Assert.That(rendered.Find(".label__text").InnerHtml, Is.EqualTo("Bar"));
	}

	[Test]
	public async Task UpdateControlFromSend()
	{
		using var ctx = new WinzorTestContext();
		Label label = null;
		var rendered = await ctx.RenderControlOnFormAsync(() => label = new Label() { Text = "Foo" });
		Assert.That(rendered.Find(".label__text").InnerHtml, Is.EqualTo("Foo"));
		SynchronizationContext syncContext = null;
		await ctx.WinzorDispatcher.InvokeAsync(() => syncContext = SynchronizationContext.Current);
		syncContext.Send(_ => label.Text = "Bar", null);
		Assert.That(rendered.Find(".label__text").InnerHtml, Is.EqualTo("Bar"));
	}

	[Test]
	public async Task UpdateControlFromTaskCallbackAfterShowDialog()
	{
		var formInstanceRegister = new Mock<IFormInstanceRegister>();
		using var ctx = new WinzorTestContext(new FormOpener(), formInstanceRegister.Object);
		var expectedLoadRequestUrl = new Uri(TestNavigationManager.BaseServerUri, "foo");
		formInstanceRegister.Setup(o => o.Add(TestNavigationManager.BaseServerUri, It.IsAny<Form>())).Returns(expectedLoadRequestUrl);
		var windowService = new Mock<IWindowService>();
		var loadRequestSent = new TaskCompletionSource();
		windowService
			.Setup(o => o.RequestCreateAndShowWindowAsync(It.IsAny<CreateWindowOptions>(), It.IsAny<ShowWindowOptions>(), It.IsAny<WindowStyleOptions>()))
			.Callback<CreateWindowOptions, ShowWindowOptions, WindowStyleOptions>((createWindowOptions, showWindowOptions, windowStyleOptions) =>
			{
				loadRequestSent.SetResult();
			});
		var clientServices = MockCargoWiseClientServices.MakeMock(windowService: windowService.Object);

		Form newForm = null;
		Button button = null;
		var rendered = await ctx.RenderFormAsync(() =>
		{
			var form = new Form();
			button = new Button();
			button.Text = "Foo";
			button.Click += (s, e) => (newForm = new Form()).ShowDialog();
			form.Controls.Add(button);
			return form;
		}, clientServices);

		Assert.That(rendered.Find(".button__text").InnerHtml, Is.EqualTo("Foo"));

		SynchronizationContext syncContext = null;
		await ctx.WinzorDispatcher.InvokeAsync(() => syncContext = SynchronizationContext.Current);

		try
		{
			rendered.Find("button").Click();
			Assert.That(await loadRequestSent.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.True);

			syncContext.Send(_ => button.Text = "Bar", null);
			Assert.That(rendered.Find(".button__text").InnerHtml, Is.EqualTo("Bar"));
		}
		finally
		{
			newForm?.Dispose();
		}
	}

	[Test]
	public async Task SendDoNothingWhenWinzorDispatcherIsDisposed()
	{
		var winzorDispatcher = new WinzorDispatcher(Mock.Of<IFormOpener>(), Mock.Of<IFormInstanceRegister>());
		var executed = false;
		SynchronizationContext syncContext = null;
		await winzorDispatcher.InvokeAsync(() => syncContext = SynchronizationContext.Current);

		winzorDispatcher.Dispose();
		syncContext.Send(_ => executed = true, null);

		Assert.That(executed, Is.False);
	}

	[Test]
	public async Task PostDoNothingWhenWinzorDispatcherIsDisposed()
	{
		var winzorDispatcher = new WinzorDispatcher(Mock.Of<IFormOpener>(), Mock.Of<IFormInstanceRegister>());
		var executed = false;
		SynchronizationContext syncContext = null;
		await winzorDispatcher.InvokeAsync(() => syncContext = SynchronizationContext.Current);

		winzorDispatcher.Dispose();
		syncContext.Post(_ => executed = true, null);

		Assert.That(executed, Is.False);
	}

	[Test]
	public async Task PostExceptionHandling()
	{
		using var ctx = new WinzorTestContext();
		var threadExceptionThrown = new TaskCompletionSource<Exception>();
		ctx.ThreadExceptionExceptionRaised += ex =>
		{
			threadExceptionThrown.SetResult(ex);
			return true;
		};

		SynchronizationContext syncContext = null;
		await ctx.WinzorDispatcher.InvokeAsync(() => syncContext = SynchronizationContext.Current);

		const string exMsg = "Test Exception";
		syncContext.Post(_ => throw new InvalidOperationException(exMsg), null);

		var exception = await threadExceptionThrown.Task;
		Assert.That(exception.Message, Is.EqualTo(exMsg));
	}
}
