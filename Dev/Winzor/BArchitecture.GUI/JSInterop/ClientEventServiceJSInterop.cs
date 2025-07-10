using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace WinzorFramework.JSInterop;

public interface IClientEventServiceJSInterop : IJSInterop
{
	Task<RegisteredClientEvent> RegisterEventListenerAsync(DotNetObjectReference<ClientEventCallback> clientEventCallback, string eventType, ElementReference? element, object eventData);
	Task<RegisteredClientEvent> RegisterEventListenerAsync<T>(DotNetObjectReference<ClientEventCallback<T>> clientEventCallback, string eventType, ElementReference? element, object? eventData);
	Task<RegisteredClientEvent> RegisterGlobalEventListenerAsync(DotNetObjectReference<ClientEventCallback> clientEventCallback, string eventType, object eventData);
	Task<RegisteredClientEvent> RegisterGlobalEventListenerAsync<T>(DotNetObjectReference<ClientEventCallback<T>> clientEventCallback, string eventType, object? eventData);
}

public sealed class ClientEventServiceJSInterop : JSInteropBase, IClientEventServiceJSInterop
{
	public ClientEventServiceJSInterop(IJSRuntimeWithMonitor jsRuntime, IFileVersionHash fileVersionHash)
		: base(jsRuntime, "/_content/WinzorFramework/js/module/clientEventService.js", fileVersionHash)
	{
	}

	public async Task<RegisteredClientEvent> RegisterEventListenerAsync(DotNetObjectReference<ClientEventCallback> clientEventCallback, string eventType, ElementReference? element, object eventData)
	{
		var jsObjectReference = await InvokeJsAsync<IJSObjectReference>("registerEventListener", clientEventCallback, eventType, element, eventData);
		return new RegisteredClientEvent(jsObjectReference);
	}

	public async Task<RegisteredClientEvent> RegisterEventListenerAsync<T>(DotNetObjectReference<ClientEventCallback<T>> clientEventCallback, string eventType, ElementReference? element, object? eventData = null)
	{
		var jsObjectReference = await InvokeJsAsync<IJSObjectReference>("registerEventListener", clientEventCallback, eventType, element, eventData);
		return new RegisteredClientEvent(jsObjectReference);
	}

	public async Task<RegisteredClientEvent> RegisterGlobalEventListenerAsync(DotNetObjectReference<ClientEventCallback> clientEventCallback, string eventType, object eventData)
	{
		var jsObjectReference = await InvokeJsAsync<IJSObjectReference>("registerGlobalEventListener", clientEventCallback, eventType, eventData);
		return new RegisteredClientEvent(jsObjectReference);
	}

	public async Task<RegisteredClientEvent> RegisterGlobalEventListenerAsync<T>(DotNetObjectReference<ClientEventCallback<T>> clientEventCallback, string eventType, object? eventData = null)
	{
		var jsObjectReference = await InvokeJsAsync<IJSObjectReference>("registerGlobalEventListener", clientEventCallback, eventType, eventData);
		return new RegisteredClientEvent(jsObjectReference);
	}
}
