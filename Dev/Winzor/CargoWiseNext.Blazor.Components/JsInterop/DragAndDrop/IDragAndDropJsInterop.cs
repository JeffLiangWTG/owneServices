using Microsoft.JSInterop;

namespace CargoWiseNext.Blazor.Components;

public interface IDragAndDropJsInterop
{
	ValueTask InitAsync(string dropAreaClassname, string dragOverClassname, DotNetObjectReference<IDragAndDropInvokables> invokablesRef);
	ValueTask TeardownAsync();
}
