using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using WinzorFramework.JSInterop;

namespace CargoWiseNext.Blazor.Components.JsInterop;

public interface IDropDownListInterop
{
	Task Initialize(string lastSelectedCode,
		ElementReference dropDownElementReference,
		ElementReference inputElementReerence,
		CwnDropDownList dropDownList);

	Task ScrollToAsync(ElementReference selectedElement);
}

public class DropDownListInterop : JSInteropBase, IDropDownListInterop
{
	public DropDownListInterop(IJSRuntimeWithMonitor jsRuntime, IFileVersionHash fileVersionHash)
		: base(jsRuntime, "/_content/CargoWiseNext.Blazor.Components/js/dropDownList.js", fileVersionHash)
	{
	}

	public async Task Initialize(string lastSelectedCode,
		ElementReference dropDownElementReference,
		ElementReference inputElementReerence,
		CwnDropDownList dropDownList)
	{
		await InvokeJsAsync("initialize",
						lastSelectedCode,
						dropDownElementReference,
						inputElementReerence,
						DotNetObjectReference.Create(dropDownList));
	}

	public async Task ScrollToAsync(ElementReference selectedElement)
	{
		await InvokeJsAsync("scrollToId", selectedElement);
	}
}
