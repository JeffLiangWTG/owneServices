using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;
using Microsoft.Playwright;
using NUnit.Framework;
using OpenTelemetry;
using OpenTelemetry.Trace;
using WinzorFramework.JSInterop;
using WinzorTestFramework;
using WTG.PlaywrightTesting;

namespace WinzorFramework;

using static PlaywrightTestContext;

internal sealed class ClientEventServiceTest
{
	#region Client EventService Tests

	[Test, WithPlaywrightPage]
	public async Task ClientEventServiceRegisterEventAddsEventListenerToElement()
	{
		await using var ctx = new InMemoryTestServerContext();

		FormForTest form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new FormForTest();
			return form;
		});

		var formElement = await LoadPageAndClearDefaultEventData(page);
		Assert.That(await IsEventListenerAttached(page, "keydown", form.ControlId), Is.False);
		await form.CargoWiseClientServices.ClientEventService.RegisterKeyEventListenerAsync(() => Task.CompletedTask, ClientKeyEvent.KeyDown, new ClientKeyEventData("a", false), form.ElementReference);
		Assert.That(async () => await IsEventListenerAttached(page, "keydown", form.ControlId), Is.True.After(1000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task ClientEventServiceRegisterEventStoresEventDataOnClient()
	{
		await using var ctx = new InMemoryTestServerContext();

		FormForTest form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new FormForTest();
			return form;
		});

		var formElement = await LoadPageAndClearDefaultEventData(page);
		Assert.That(await GetClientEventData(page, "keydown", form.ControlId, 1), Is.Null);
		await form.CargoWiseClientServices.ClientEventService.RegisterKeyEventListenerAsync(() => Task.CompletedTask, ClientKeyEvent.KeyDown, new ClientKeyEventData("a", false), form.ElementReference);
		Assert.That(async () => await GetClientEventData(page, "keydown", form.ControlId, 1), Is.Not.Null.After(1000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task ClientEventServiceRegisterGlobalEventAddsEventListenerToDocument()
	{
		await using var ctx = new InMemoryTestServerContext();

		FormForTest form = null;
		var page = await ctx.LoadFormAsync(() => form = new FormForTest());

		var formElement = await LoadPageAndClearDefaultEventData(page);
		Assert.That(await IsEventListenerAttached(page, "keydown", "document"), Is.False);

		await form.CargoWiseClientServices.ClientEventService.RegisterGlobalKeyEventListenerAsync(() => Task.CompletedTask, ClientKeyEvent.KeyDown, new ClientKeyEventData("a", false));
		Assert.That(async () => await IsEventListenerAttached(page, "keydown", "document"), Is.True.After(1000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task ClientEventServiceRegisterElementMouseEvent([Values] ClientMouseEvent mouseEvent)
	{
		await using var ctx = new InMemoryTestServerContext();

		FormForTest form = null;
		var page = await ctx.LoadFormAsync(() => form = new FormForTest());

		Assert.That(await IsEventListenerAttached(page, mouseEvent.ToString().ToLower(), form.ControlId), Is.False);
		await form.CargoWiseClientServices.ClientEventService.RegisterMouseEventListenerAsync(e => Task.CompletedTask, mouseEvent, form.ElementReference);
		Assert.That(async () => await IsEventListenerAttached(page, mouseEvent.ToString().ToLower(), form.ControlId), Is.True.After(1000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task ClientEventServiceRegisterGlobalMouseEvent([Values] ClientMouseEvent mouseEvent)
	{
		await using var ctx = new InMemoryTestServerContext();

		Form form = null;
		var page = await ctx.LoadFormAsync(() => form = new Form());

		Assert.That(await IsEventListenerAttached(page, mouseEvent.ToString().ToLower(), "document"), Is.False);
		await form.CargoWiseClientServices.ClientEventService.RegisterGlobalMouseEventListenerAsync(e => Task.CompletedTask, mouseEvent);
		Assert.That(async () => await IsEventListenerAttached(page, mouseEvent.ToString().ToLower(), "document"), Is.True.After(1000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task ClientEventServiceRegisterGlobalEventStoresEventDataOnClient()
	{
		await using var ctx = new InMemoryTestServerContext();

		FormForTest form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new FormForTest();
			return form;
		});
		var formElement = await LoadPageAndClearDefaultEventData(page);
		Assert.That(await GetClientEventData(page, "keydown", "document", 1), Is.Null);
		await form.CargoWiseClientServices.ClientEventService.RegisterGlobalKeyEventListenerAsync(() => Task.CompletedTask, ClientKeyEvent.KeyDown, new ClientKeyEventData("a", false));
		Assert.That(async () => await GetClientEventData(page, "keydown", "document", 1), Is.Not.Null.After(1000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task ClientEventServiceRegisterEventElementWithoutWinzorIdThrowsException()
	{
		await using var ctx = new InMemoryTestServerContext();

		FormForTest form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new FormForTest();
			form.ControlId = string.Empty;
			return form;
		});

		var formElement = await LoadPageAndClearDefaultEventData(page);
		var exception = Assert.ThrowsAsync<JSException>(async () =>
		{
			await form.CargoWiseClientServices.ClientEventService.RegisterKeyEventListenerAsync(() => Task.CompletedTask, ClientKeyEvent.KeyDown, new ClientKeyEventData("a", false), form.ElementReference);
		});
		Assert.That(exception.Message, Does.StartWith("Element does not have a Winzor Control Id."));
	}

	[Test, WithPlaywrightPage]
	public async Task ClientEventServiceRegisterEventNonRegisteredEventTypeThrowsException()
	{
		await using var ctx = new InMemoryTestServerContext();

		FormForTest form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new FormForTest();
			return form;
		});

		var formElement = await LoadPageAndClearDefaultEventData(page);
		// Need to use the interop directly for this test rather than the ClientEventService as the client event service is strongly typed to avoid these issues
		// This test will ensure that a helpful error message is provided if in the future someone bypasses the interface provided by ClientEventService
#pragma warning disable CA2000 // Dispose objects before losing scope - disposal results in exceptions and this should not leak
		var clientEventServiceInterop = new ClientEventServiceJSInterop(new DummyJSRunTimeWithMonitor(form.CargoWiseClientServices.JSRuntime), new DummyFileVersionHash());
#pragma warning restore CA2000 // Dispose objects before losing scope - disposal results in exceptions and this should not leak
		var exception = Assert.ThrowsAsync<JSException>(async () =>
		{
			await clientEventServiceInterop.RegisterEventListenerAsync(null, "invalideventtype", null, new object());
		});
		Assert.That(exception.Message, Does.StartWith("There is not event handler registered for event type invalideventtype."));
	}

	[Test, WithPlaywrightPage]
	public async Task ClientEventServiceEventListenerWithNoWinzorIdThrowsException()
	{
		await using var ctx = new InMemoryTestServerContext();

		FormForTest form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new FormForTest();
			return form;
		});

		var formElement = await LoadPageAndClearDefaultEventData(page);
		await form.CargoWiseClientServices.ClientEventService.RegisterKeyEventListenerAsync(() => Task.CompletedTask, ClientKeyEvent.KeyDown, new ClientKeyEventData("a", false), form.ElementReference);
		await form.InvokeWinzorDispatcherAsync(() => form.ControlId = string.Empty);
		formElement = await page.WaitForSelectorAsync(".form[data-winzor-control-id='']");

		var jsError = new TaskCompletionSource<string>();
		page.PageError += (_, error) =>
		{
			jsError.SetResult(error);
			PageErrors.Remove(error);
		};

		await page.PressAsync("input", "KeyA");
		Assert.That(await jsError.Task.WaitAsync(TimeSpan.FromSeconds(3)), Does.StartWith("Error: Element does not have a Winzor Control Id."));
	}

	[Test, WithPlaywrightPage]
	public async Task ClientEventServiceEventListenerWithNoRegisteredEventsRemovesEventListener()
	{
		await using var ctx = new InMemoryTestServerContext();

		FormForTest form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new FormForTest();
			return form;
		});

		var formElement = await LoadPageAndClearDefaultEventData(page);
		await form.CargoWiseClientServices.ClientEventService.RegisterKeyEventListenerAsync(() => Task.CompletedTask, ClientKeyEvent.KeyDown, new ClientKeyEventData("a", false), form.ElementReference);
		Assert.That(async () => await IsEventListenerAttached(page, "keydown", form.ControlId), Is.True.After(1000, 100));

		// Force remove the stored key data from the client
		await page.EvaluateAsync($"delete window.winzor.events.keydown['{form.ControlId}'].registeredEvents['1'];");
		await page.PressAsync("input", "KeyA");
		Assert.That(async () => await IsEventListenerAttached(page, "keydown", form.ControlId), Is.False.After(1000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task ClientEventServiceEventDisposeEventRemovesEventData()
	{
		await using var ctx = new InMemoryTestServerContext();

		FormForTest form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new FormForTest();
			return form;
		});

		var formElement = await LoadPageAndClearDefaultEventData(page);
		var registeredClientEvent = await form.CargoWiseClientServices.ClientEventService.RegisterKeyEventListenerAsync(() => Task.CompletedTask, ClientKeyEvent.KeyDown, new ClientKeyEventData("a", false), form.ElementReference);
		Assert.That(async () => await GetClientEventData(page, "keydown", form.ControlId, 1), Is.Not.Null.After(1000, 100));

		await registeredClientEvent.DisposeAsync();
		Assert.That(async () => await GetClientEventData(page, "keydown", form.ControlId, 1), Is.Null.After(1000, 100));
	}

	[Test, WithPlaywrightPage]
	public async Task ClientEventServiceEventDisposeEventRemovesEventListenerIfNoOthersRegistered()
	{
		await using var ctx = new InMemoryTestServerContext();

		FormForTest form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new FormForTest();
			return form;
		});

		var formElement = await LoadPageAndClearDefaultEventData(page);
		var registeredClientEventOne = await form.CargoWiseClientServices.ClientEventService.RegisterKeyEventListenerAsync(() => Task.CompletedTask, ClientKeyEvent.KeyDown, new ClientKeyEventData("a", false), form.ElementReference);
		var registeredClientEventTwo = await form.CargoWiseClientServices.ClientEventService.RegisterKeyEventListenerAsync(() => Task.CompletedTask, ClientKeyEvent.KeyDown, new ClientKeyEventData("b", false), form.ElementReference);
		Assert.That(async () => await GetClientEventData(page, "keydown", form.ControlId, 1), Is.Not.Null.After(1000, 100));
		Assert.That(async () => await GetClientEventData(page, "keydown", form.ControlId, 2), Is.Not.Null.After(1000, 100));
		Assert.That(async () => await IsEventListenerAttached(page, "keydown", form.ControlId), Is.True.After(1000, 100));

		await registeredClientEventOne.DisposeAsync();
		Assert.That(async () => await GetClientEventData(page, "keydown", form.ControlId, 1), Is.Null.After(1000, 100));
		Assert.That(async () => await GetClientEventData(page, "keydown", form.ControlId, 2), Is.Not.Null.After(1000, 100));
		Assert.That(async () => await IsEventListenerAttached(page, "keydown", form.ControlId), Is.True.After(1000, 100));

		await registeredClientEventTwo.DisposeAsync();
		Assert.That(async () => await GetClientEventData(page, "keydown", form.ControlId, 2), Is.Null.After(1000, 100));
		Assert.That(async () => await IsEventListenerAttached(page, "keydown", form.ControlId), Is.False.After(1000, 100));
	}

	#endregion

	#region Event Handler Tests

	[Test, WithPlaywrightPage]
	public async Task ClientEventServiceKeyDownEventHandlerFiresCallbackIfKeyIsEqual()
	{
		await using var ctx = new InMemoryTestServerContext();

		FormForTest form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new FormForTest();
			return form;
		});

		var formElement = await LoadPageAndClearDefaultEventData(page);
		var callbackFired = new TaskCompletionSource<bool>();
		await form.CargoWiseClientServices.ClientEventService.RegisterKeyEventListenerAsync(() => {
			callbackFired.SetResult(true);
			return Task.CompletedTask;
		}, ClientKeyEvent.KeyDown, new ClientKeyEventData("a", false), form.ElementReference);

		await page.PressAsync("input", "KeyA");

		Assert.That(await callbackFired.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.True);
	}

	[Test, WithPlaywrightPage]
	public async Task TestClientServiceCallBackTraces()
	{
		var traces = new List<Activity>();
		using var tracerProvider = Sdk.CreateTracerProviderBuilder()
		.AddSource("WinzorFramework")
		.AddInMemoryExporter(traces)
		.SetSampler(new AlwaysOnSampler())
		.Build();

		await using var ctx = new InMemoryTestServerContext();

		FormForTest form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new FormForTest();
			return form;
		});

		var formElement = await LoadPageAndClearDefaultEventData(page);
		var callbackFired = new TaskCompletionSource<bool>();
		await form.CargoWiseClientServices.ClientEventService.RegisterKeyEventListenerAsync(() => {
			callbackFired.SetResult(true);
			return Task.CompletedTask;
		}, ClientKeyEvent.KeyDown, new ClientKeyEventData("a", false), form.ElementReference);

		await page.PressAsync("input", "KeyA");

		Assert.That(traces, Has.Count.GreaterThan(0));
		Assert.That(traces.Any(trace => trace.OperationName.Equals("ClientEventCallback_KeyDown")), Is.True);
	}

	[Test, WithPlaywrightPage]
	public async Task ClientEventServiceKeyDownEventHandlerDoesNotFireCallbackIfKeyIsNotEqual()
	{
		await using var ctx = new InMemoryTestServerContext();

		FormForTest form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new FormForTest();
			return form;
		});

		var formElement = await LoadPageAndClearDefaultEventData(page);
		var callbackFired = new TaskCompletionSource<bool>();
		await form.CargoWiseClientServices.ClientEventService.RegisterKeyEventListenerAsync(() => {
			callbackFired.SetResult(true);
			return Task.CompletedTask;
		}, ClientKeyEvent.KeyDown, new ClientKeyEventData("a", false), form.ElementReference);

		await page.PressAsync("input", "KeyB");

		Assert.That(await callbackFired.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.False);
	}

	[Test, WithPlaywrightPage]
	public async Task ClientEventServiceKeyDownEventHandlerFiresCallbackIfAltKeyPressed()
	{
		await using var ctx = new InMemoryTestServerContext();

		FormForTest form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new FormForTest();
			return form;
		});

		var formElement = await LoadPageAndClearDefaultEventData(page);
		var callbackFired = new TaskCompletionSource<bool>();
		await form.CargoWiseClientServices.ClientEventService.RegisterKeyEventListenerAsync(() => {
			callbackFired.SetResult(true);
			return Task.CompletedTask;
		}, ClientKeyEvent.KeyDown, new ClientKeyEventData("a", true), form.ElementReference);

		await page.PressAsync("input", "Alt+KeyA");

		Assert.That(await callbackFired.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.True);
	}

	[Test, WithPlaywrightPage]
	public async Task ClientEventServiceKeyDownEventHandlerDoesNotFireCallbackIfAltKeyNotPressed()
	{
		await using var ctx = new InMemoryTestServerContext();

		FormForTest form = null;
		var page = await ctx.LoadFormAsync(() =>
		{
			form = new FormForTest();
			return form;
		});

		var formElement = await LoadPageAndClearDefaultEventData(page);
		var callbackFired = new TaskCompletionSource<bool>();
		await form.CargoWiseClientServices.ClientEventService.RegisterKeyEventListenerAsync(() => {
			callbackFired.SetResult(true);
			return Task.CompletedTask;
		}, ClientKeyEvent.KeyDown, new ClientKeyEventData("a", true), form.ElementReference);

		await page.PressAsync("input", "KeyA");

		Assert.That(await callbackFired.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.False);
	}

	[Test, WithPlaywrightPage]
	public async Task ClientEventServiceMouseClickEventHandler([Values] ClientMouseEvent mouseEvent)
	{
		await using var ctx = new InMemoryTestServerContext();

		Form form = null;
		var page = await ctx.LoadFormAsync(() => form = new Form());

		var callbackFired = new TaskCompletionSource<WebMouseEventArgs>();
		await form.CargoWiseClientServices.ClientEventService.RegisterGlobalMouseEventListenerAsync(e =>
		{
			callbackFired.SetResult(e);
			return Task.CompletedTask;
		}, mouseEvent);

		switch (mouseEvent)
		{
			case ClientMouseEvent.Click:
				await page.Mouse.ClickAsync(10, 20);
				break;
			case ClientMouseEvent.MouseDown:
				await page.Mouse.MoveAsync(10, 20);
				await page.Mouse.DownAsync();
				break;
			case ClientMouseEvent.MouseMove:
				await page.Mouse.MoveAsync(10, 20);
				break;
			case ClientMouseEvent.MouseUp:
				await page.Mouse.MoveAsync(10, 20);
				await page.Mouse.UpAsync();
				break;
		}

		Assert.That(await callbackFired.Task.WithTimeout(TimeSpan.FromSeconds(1)), Is.True);
		var eventArgs = await callbackFired.Task;
		Assert.That(eventArgs.ClientX, Is.EqualTo(10));
		Assert.That(eventArgs.ClientY, Is.EqualTo(20));
		Assert.That(eventArgs.Button, Is.EqualTo(0));
	}

	[Test, WithPlaywrightPage]
	public async Task TestAttachDocumentDrag()
	{
		await using var ctx = new InMemoryTestServerContext();

		Form form = null;
		var page = await ctx.LoadFormAsync(() => form = new Form());

		var mouseDown = new TaskCompletionSource();
		var mouseMoveCount = 0;
		var mouseUpCount = 0;
		var trigger = form.CargoWiseClientServices.ClientEventService.AttachDocumentDrag(
			e => { mouseDown.SetResult(); return Task.CompletedTask; },
			e => { Interlocked.Increment(ref mouseMoveCount); return Task.CompletedTask; },
			e => { Interlocked.Increment(ref mouseUpCount); return Task.CompletedTask; });

		Assert.That(async () => await IsEventListenerAttached(page, "mousemove", "document"), Is.EqualTo(false));
		Assert.That(async () => await IsEventListenerAttached(page, "mouseup", "document"), Is.EqualTo(false));

		await trigger(null);
		Assert.That(mouseDown.Task.IsCompleted);
		Assert.That(async () => await IsEventListenerAttached(page, "mousemove", "document"), Is.EqualTo(true));
		Assert.That(async () => await IsEventListenerAttached(page, "mouseup", "document"), Is.EqualTo(true));

		await page.Mouse.DownAsync();
		await page.Mouse.MoveAsync(20, 20);
		Assert.That(() => mouseMoveCount, Is.EqualTo(1).After(1000, 100));

		await page.Mouse.UpAsync();
		Assert.That(() => mouseUpCount, Is.EqualTo(1).After(1000, 100));
		Assert.That(async () => await IsEventListenerAttached(page, "mousemove", "document"), Is.EqualTo(false));
		Assert.That(async () => await IsEventListenerAttached(page, "mouseup", "document"), Is.EqualTo(false));
	}

	[Test, WithPlaywrightPage]
	public async Task TestDragWithLeftButtonUp()
	{
		await using var ctx = new InMemoryTestServerContext();

		Form form = null;
		var page = await ctx.LoadFormAsync(() => form = new Form());

		var mouseDown = new TaskCompletionSource();
		var mouseMoveCount = 0;
		var mouseUpCount = 0;
		var trigger = form.CargoWiseClientServices.ClientEventService.AttachDocumentDrag(
			e => { mouseDown.SetResult(); return Task.CompletedTask; },
			e => { Interlocked.Increment(ref mouseMoveCount); return Task.CompletedTask; },
			e => { Interlocked.Increment(ref mouseUpCount); return Task.CompletedTask; });

		Assert.That(async () => await IsEventListenerAttached(page, "mousemove", "document"), Is.EqualTo(false));
		Assert.That(async () => await IsEventListenerAttached(page, "mouseup", "document"), Is.EqualTo(false));

		await trigger(null);
		Assert.That(mouseDown.Task.IsCompleted);
		Assert.That(async () => await IsEventListenerAttached(page, "mousemove", "document"), Is.EqualTo(true));
		Assert.That(async () => await IsEventListenerAttached(page, "mouseup", "document"), Is.EqualTo(true));

		await page.Mouse.MoveAsync(20, 20);
		await page.Mouse.MoveAsync(40, 40);
		await page.Mouse.MoveAsync(60, 60);
		Assert.That(() => mouseMoveCount, Is.EqualTo(0));
		Assert.That(async () => await IsEventListenerAttached(page, "mousemove", "document"), Is.EqualTo(false));
		Assert.That(async () => await IsEventListenerAttached(page, "mouseup", "document"), Is.EqualTo(false));
	}

	#endregion

	#region Helper Methods

	async Task<bool> IsEventListenerAttached(IPage page, string eventType, string controlId)
	{
		return await page.EvaluateAsync<bool>($"window.winzor?.events?.{eventType}?.['{controlId}']?._isEventListenerAttached ?? false");
	}

	async Task<object> GetClientEventData(IPage page, string eventType, string controlId, int eventId)
	{
		return await page.EvaluateAsync<object>($"window.winzor?.events?.{eventType}?.['{controlId}']?.registeredEvents?.['{eventId}'] ?? null");
	}

	async Task<IElementHandle> LoadPageAndClearDefaultEventData(IPage page)
	{
		var formElement = await page.WaitForSelectorAsync(".form");
		Assert.That(async () => await IsEventListenerAttached(page, "keydown", "document"), Is.True.After(3000, 100));
		Assert.That(async () => await GetClientEventData(page, "keydown", "document", 0), Is.Not.Null.After(3000, 100));
		// Trigger the form to show the mnemonic keys and remove its event listeners so they don't interfere with the test
		await page.Keyboard.PressAsync("Alt");
		Assert.That(async () => await IsEventListenerAttached(page, "keydown", "document"), Is.False.After(3000, 100), "Failed to clean up the event listener added by the form.");
		Assert.That(async () => await GetClientEventData(page, "keydown", "document", 0), Is.Null.After(3000, 100), "Failed to clean up the event data added by the form.");
		return formElement;
	}

	class FormForTest : Form
	{
		public string ControlId
		{
			get => controlId ?? WinzorControlId;
			set => UpdateProperty(ref controlId, value);
		}
		string controlId;

		protected override void BuildRenderTree(RenderTreeBuilder builder)
		{
			builder.OpenElement(0, "div");
			builder.AddAttribute(1, "class", "form");
			builder.AddAttribute(2, "style", "absolute;width:300px;height:300px;");
			builder.AddAttribute(3, "data-winzor-control-id", ControlId);
			builder.AddElementReferenceCapture(4, reference => ElementReference = reference);
			builder.OpenElement(5, "input");
			builder.CloseElement();
			builder.CloseElement();
		}
	}

	#endregion
}
