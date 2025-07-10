using Microsoft.AspNetCore.Components;

namespace WinzorFramework.JSInterop;

public interface ITextBoxBaseJSInterop : IJSInterop
{
	public Task SetSelectionAsync(ElementReference reference, int start, int end);
	public Task ScrollToCaretAsync(ElementReference reference, int start, int end, int textLength);
}
