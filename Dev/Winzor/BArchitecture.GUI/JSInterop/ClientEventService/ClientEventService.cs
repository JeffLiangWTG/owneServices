using Microsoft.AspNetCore.Components;
using WinzorFramework.Telemetry;

namespace WinzorFramework.JSInterop;

public interface IClientEventService
{
	Task<RegisteredClientEvent> RegisterKeyEventListenerAsync(Func<Task> callback, ClientKeyEvent eventType, ClientKeyEventData eventData, ElementReference element);
	Task<RegisteredClientEvent> RegisterGlobalKeyEventListenerAsync(Func<Task> callback, ClientKeyEvent eventType, ClientKeyEventData eventData);
	Task<RegisteredClientEvent> RegisterMouseEventListenerAsync(Func<WebMouseEventArgs, Task> callback, ClientMouseEvent eventType, ElementReference element);
	Task<RegisteredClientEvent> RegisterGlobalMouseEventListenerAsync(Func<WebMouseEventArgs, Task> callback, ClientMouseEvent eventType);
	Func<WebMouseEventArgs, Task> AttachDocumentDrag(Func<WebMouseEventArgs, Task> mouseDown, Func<WebMouseEventArgs, Task> mouseMove, Func<WebMouseEventArgs, Task> mouseUp);
}

public class ClientEventService : IClientEventService, IDisposable
{
	readonly IClientEventServiceJSInterop clientEventServiceInterop;

	public ClientEventService(IClientEventServiceJSInterop clientEventServiceInterop)
	{
		this.clientEventServiceInterop = clientEventServiceInterop;
	}

	public Task<RegisteredClientEvent> RegisterKeyEventListenerAsync(Func<Task> callback, ClientKeyEvent eventType, ClientKeyEventData eventData, ElementReference element)
	{
		return RegisterEventListenerAsync(
			callback,
			Enum.GetName(typeof(ClientKeyEvent), eventType)!,
			eventData,
			element);
	}

	public Task<RegisteredClientEvent> RegisterGlobalKeyEventListenerAsync(Func<Task> callback, ClientKeyEvent eventType, ClientKeyEventData eventData)
	{
		return RegisterEventListenerAsync(
			callback,
			Enum.GetName(typeof(ClientKeyEvent), eventType)!,
			eventData);
	}

	Task<RegisteredClientEvent> RegisterEventListenerAsync(Func<Task> callback, string eventType, object eventData, ElementReference? element = null)
	{
		var clientEventCallback = new ClientEventCallback(callback)
		{
			ActivitySource = TelemetryService.ActivitySource,
			EventType = eventType
		};
		callbacks.Add(clientEventCallback);
		return element != null
			? clientEventServiceInterop.RegisterEventListenerAsync(clientEventCallback.ObjectReference, eventType.ToLower(), element, eventData)
			: clientEventServiceInterop.RegisterGlobalEventListenerAsync(clientEventCallback.ObjectReference, eventType.ToLower(), eventData);
	}

	public Task<RegisteredClientEvent> RegisterMouseEventListenerAsync(Func<WebMouseEventArgs, Task> callback, ClientMouseEvent eventType, ElementReference element)
	{
		return RegisterMouseEventListenerAsync(callback, eventType.ToString().ToLower(), element);
	}

	public Task<RegisteredClientEvent> RegisterGlobalMouseEventListenerAsync(Func<WebMouseEventArgs, Task> callback, ClientMouseEvent eventType)
	{
		return RegisterMouseEventListenerAsync(callback, eventType.ToString().ToLower());
	}

	Task<RegisteredClientEvent> RegisterMouseEventListenerAsync(Func<WebMouseEventArgs, Task> callback, string eventType, ElementReference? element = null, object? eventData = null)
	{
		var clientEventCallback = new ClientEventCallback<WebMouseEventArgs>(callback)
		{
			ActivitySource = TelemetryService.ActivitySource,
			EventType = eventType
		};
		callbacks.Add(clientEventCallback);
		return element != null
			? clientEventServiceInterop.RegisterEventListenerAsync(clientEventCallback.ObjectReference, eventType, element!, eventData)
			: clientEventServiceInterop.RegisterGlobalEventListenerAsync(clientEventCallback.ObjectReference, eventType, eventData);
	}

	public Func<WebMouseEventArgs, Task> AttachDocumentDrag(Func<WebMouseEventArgs, Task> mouseDown, Func<WebMouseEventArgs, Task> mouseMove, Func<WebMouseEventArgs, Task> mouseUp)
	{
		Task MouseDown(WebMouseEventArgs args)
		{
			var lockObj = new object();
			Task<RegisteredClientEvent>? registerMove = null;
			Task<RegisteredClientEvent>? registerUp = null;

			async Task Unregister()
			{
				Task<RegisteredClientEvent>? move = null;
				Task<RegisteredClientEvent>? up = null;
				lock (lockObj)
				{
					move = registerMove;
					up = registerUp;
					registerMove = null;
					registerUp = null;
				}

				var unregisterMove = move is null ? Task.CompletedTask : (await move).DisposeAsync().AsTask();
				var unregisterUp = up is null ? Task.CompletedTask : (await up).DisposeAsync().AsTask();
				await Task.WhenAll(unregisterMove, unregisterUp);
			}

			async Task MouseMove(WebMouseEventArgs args)
			{
				if (registerMove != null && registerUp != null)
				{
					if ((args.Buttons & 1) == 0)
					{
						await Unregister();
					}
					else
					{
						await mouseMove(args);
					}
				}
			}

			Task MouseUp(WebMouseEventArgs args)
			{
				return Task.WhenAll(mouseUp(args), Unregister());
			}

			registerMove = RegisterGlobalMouseEventListenerAsync(MouseMove, ClientMouseEvent.MouseMove);
			registerUp = RegisterGlobalMouseEventListenerAsync(MouseUp, ClientMouseEvent.MouseUp);
			return Task.WhenAll(mouseDown(args), registerMove, registerUp);
		}

		return MouseDown;
	}

	public void Dispose()
	{
		foreach (var callback in callbacks)
		{
			callback?.Dispose();
		}
	}

	readonly List<IDisposable> callbacks = new List<IDisposable>();
}

public enum ClientKeyEvent
{
	KeyDown, KeyPress
}

public enum ClientMouseEvent
{
	Click, MouseDown, MouseMove, MouseUp
}

public record ClientKeyEventData(string key, bool altKey, bool shouldPreventDefault = false);
