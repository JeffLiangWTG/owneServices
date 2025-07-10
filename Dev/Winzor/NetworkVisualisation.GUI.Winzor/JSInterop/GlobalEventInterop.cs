#nullable enable
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using WinzorFramework.Extensions;
using WinzorFramework.JSInterop;

namespace CargoWise.NetworkVisualisation.GUI.JSInterop;

/// <summary>
/// Represents an interface that inherits from the IJSInterop interface and provides a method to subscribe to pointer events outside a specified element.
/// </summary>
public interface IGlobalEventJSInterop : IJSInterop
{
	Task SubscribeToPointerEventsOutsideElement(ElementReference parentElement, Func<PointerEventArgs, Task> onPointerMove, Func<PointerEventArgs, Task> onPointerUp);
}

public class GlobalEventJSInterop : JSInteropBase, IGlobalEventJSInterop
{
	/// <summary>
	/// Initializes a new instance of the <see cref="GlobalEventJSInterop"/> class.
	/// </summary>
	/// <param name="jsRuntime">The JavaScript runtime to use for invoking JavaScript functions.</param>
	/// <param name="fileVersionHash">The file version hash used for cache busting.</param>
	public GlobalEventJSInterop(IJSRuntimeWithMonitor jsRuntime, IFileVersionHash fileVersionHash)
		: base(jsRuntime, "/_content/CargoWise.NetworkVisualisation.GUI/js/globalEvents.js", fileVersionHash)
	{
	}

	/// <summary>
	/// Subscribes elements such as <see cref="Controls.NodeResizing.NodeResizeControl"/> and <see cref="DiagramAreaUserControl"/> to pointer events.
	/// </summary>
	/// <param name="parentElement">The element to listen to pointer events outside of.</param>
	public async Task SubscribeToPointerEventsOutsideElement(ElementReference parentElement, Func<PointerEventArgs, Task> onPointerMove, Func<PointerEventArgs, Task> onPointerUp)
	{
		var onPointerMoveCallback = new ClientEventCallback<PointerEventArgs>(onPointerMove);
		ClientEventCallback<PointerEventArgs>? onPointerUpCallback = null;
		onPointerUpCallback = new ClientEventCallback<PointerEventArgs>(async args =>
		{
			await onPointerUp(args);
			onPointerMoveCallback.Dispose();
			onPointerUpCallback?.Dispose();
		});

		await ExceptionHandlerExtension.HandleJSExceptionAsync(async () =>
		{
			await InvokeJsAsync("subscribeToPointerEventsOutsideElement", parentElement, onPointerMoveCallback.ObjectReference, onPointerUpCallback.ObjectReference);
		});
	}
}
