using Microsoft.AspNetCore.Components;

namespace WinzorFramework.JSInterop;

#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
public record ClientRect(double X, double Y, double Width, double Height, double Top, double Right, double Bottom, double Left);
#pragma warning restore SA1313 // Parameter names should begin with lower-case letter

public interface IElementJSInterop : IJSInterop
{
	Task<ClientRect> GetBoundingClientRectAsync(ElementReference element);
	Task<double> GetOffsetTopAsync(ElementReference element);
}

public class ElementJSInterop : JSInteropBase, IElementJSInterop
{
	public ElementJSInterop(IJSRuntimeWithMonitor jsRuntime, IFileVersionHash fileVersionHash)
		: base(jsRuntime, "/_content/WinzorFramework/js/module/element.js", fileVersionHash)
	{
	}

	public async Task<ClientRect> GetBoundingClientRectAsync(ElementReference element)
	{
		return await InvokeJsAsync<ClientRect>("getBoundingClientRect", element);
	}

	public async Task<double> GetOffsetTopAsync(ElementReference element)
	{
		return await InvokeJsAsync<double>("getOffsetTop", element);
	}
}
