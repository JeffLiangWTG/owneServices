using System.Drawing;
using System.Windows.Forms;
using CargoWise.NetworkVisualisation.GUI.JSInterop;
using Enterprise.Winzor.Architecture.Test;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Microsoft.JSInterop.Infrastructure;
using Moq;
using WinzorFramework.JSInterop;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace NetworkVisualisation.GUI.Winzor.Test.JSInterop;

class GlobalEventInteropTest
{
	[Test, WithPlaywrightPage]
	public async Task PointerMoveInsideElementShouldNotTriggerCallbackAsync()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		ControlWithGlobalEventInterop? control = null;
		var page = await ctx.LoadControlOnFormAsync(() => control = new ControlWithGlobalEventInterop());

		var pointerMoveTcs = new TaskCompletionSource<PointerEventArgs>();
		await control!.GetJSInterop<IGlobalEventJSInterop>()!.SubscribeToPointerEventsOutsideElement(control.ElementReference, (args) => {
			pointerMoveTcs.SetResult(args);
			return Task.CompletedTask;
		}, (_) => Task.CompletedTask);

		var element = page.Locator(".ControlWithGlobalEventInterop");
		await element!.DispatchEventAsync("pointermove", new Dictionary<string, string> {
			{ "clientX", "123" },
			{ "clientY", "456" },
		});

		Assert.That(await pointerMoveTcs.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.False);
	}

	[Test, WithPlaywrightPage]
	public async Task PointerMoveOutsideElementShouldTriggerCallbackAsync()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		ControlWithGlobalEventInterop? control = null;
		var page = await ctx.LoadControlOnFormAsync(() => control = new ControlWithGlobalEventInterop());

		var pointerMoveTcs = new TaskCompletionSource<PointerEventArgs>();
		await control!.GetJSInterop<IGlobalEventJSInterop>()!.SubscribeToPointerEventsOutsideElement(control.ElementReference, (args) => {
			pointerMoveTcs.SetResult(args);
			return Task.CompletedTask;
		}, (_) => Task.CompletedTask);

		var formElement = page.Locator(".form");
		await formElement!.DispatchEventAsync("pointermove", new Dictionary<string, string> {
			{ "clientX", "123" },
			{ "clientY", "456" },
		});

		Assert.That(() => pointerMoveTcs.Task.IsCompleted, Is.True.After(3000, 100));
		var eventArgs = await pointerMoveTcs.Task;
		Assert.That(eventArgs.ClientX, Is.EqualTo(123));
		Assert.That(eventArgs.ClientY, Is.EqualTo(456));
	}

	[Test, WithPlaywrightPage]
	public async Task PointerUpInsideElementShouldNotTriggerCallbackAsync()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		ControlWithGlobalEventInterop? control = null;
		var page = await ctx.LoadControlOnFormAsync(() => control = new ControlWithGlobalEventInterop());

		var pointerUpTcs = new TaskCompletionSource<PointerEventArgs>();
		await control!.GetJSInterop<IGlobalEventJSInterop>()!.SubscribeToPointerEventsOutsideElement(control.ElementReference, (_) => Task.CompletedTask, (args) => {
			pointerUpTcs.SetResult(args);
			return Task.CompletedTask;
		});

		var element = page.Locator(".ControlWithGlobalEventInterop");
		await element!.DispatchEventAsync("pointerup", new Dictionary<string, string> {
			{ "clientX", "123" },
			{ "clientY", "456" },
		});

		Assert.That(await pointerUpTcs.Task.WithTimeout(TimeSpan.FromSeconds(3)), Is.False);
	}

	[Test, WithPlaywrightPage]
	public async Task PointerUpOutsideElementShouldTriggerCallbackAsync()
	{
		await using var ctx = new InMemoryAppServerTestContext();
		ControlWithGlobalEventInterop? control = null;
		var page = await ctx.LoadControlOnFormAsync(() => control = new ControlWithGlobalEventInterop());

		var pointerUpTcs = new TaskCompletionSource<PointerEventArgs>();
		await control!.GetJSInterop<IGlobalEventJSInterop>()!.SubscribeToPointerEventsOutsideElement(control.ElementReference, (_) => Task.CompletedTask, (args) => {
			pointerUpTcs.SetResult(args);
			return Task.CompletedTask;
		});

		var formElement = page.Locator(".form");
		await formElement!.DispatchEventAsync("pointerup", new Dictionary<string, string> {
			{ "clientX", "123" },
			{ "clientY", "456" },
		});

		Assert.That(() => pointerUpTcs.Task.IsCompleted, Is.True.After(3000, 100));
		var eventArgs = await pointerUpTcs.Task;
		Assert.That(eventArgs.ClientX, Is.EqualTo(123));
		Assert.That(eventArgs.ClientY, Is.EqualTo(456));
	}

	[Test, WithPlaywrightPage]
	public async Task PointerUpOutsideElementShouldNotThrowTaskCancelledExceptionAsync()
	{
		var mockJsObjectReference = new Mock<IJSObjectReference>();
		mockJsObjectReference.Setup(x => x.InvokeAsync<IJSVoidResult>(It.IsAny<string>(), It.IsAny<object[]>())).ThrowsAsync(new TaskCanceledException());
		var jsRuntimeMock = new Mock<IJSRuntimeWithMonitor>();
		jsRuntimeMock.Setup(js => js.InvokeAsync<IJSObjectReference?>("import", It.IsAny<object[]>())).ReturnsAsync(mockJsObjectReference.Object);

		var interop = new GlobalEventJSInterop(jsRuntimeMock.Object, Mock.Of<IFileVersionHash>());

		Assert.DoesNotThrowAsync(async () => await interop.SubscribeToPointerEventsOutsideElement(new ElementReference(), (_) => Task.CompletedTask, (args) => Task.CompletedTask));
		await interop.DisposeAsync();
	}

	class ControlWithGlobalEventInterop : Control
	{
		protected override bool ShouldRender => true;

		public override bool CaptureElementReference => true;

		protected override string ClassName => "ControlWithGlobalEventInterop";

		protected override Size DefaultSize => new Size(150, 150);
	}
}
