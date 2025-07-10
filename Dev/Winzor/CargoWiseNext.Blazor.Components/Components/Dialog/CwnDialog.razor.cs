using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace CargoWiseNext.Blazor.Components;

[SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Referenced in CwnDialog.razor")]
public partial class CwnDialog : CwnComponentBase
{
	public ElementReference DialogRef;
	[Parameter] public RenderFragment? ChildContent { get; set; }
	[Parameter] public string? ClassContent { get; set; }
	[Parameter] public string? StyleContent { get; set; }

	[Inject] public IJSRuntime? JsRuntime { get; set; }

	string? Classname =>
		new CssBuilder()
			.AddClass("cwn-dialog")
			.AddClass(Class)
			.Build();

	string? Stylename =>
		new StyleBuilder()
			.AddStyle(Style)
			.Build();

	string? ClassnameContent =>
		new CssBuilder()
			.AddClass("cwn-dialog__content")
			.AddClass(ClassContent)
			.Build();

#pragma warning disable CW1161 // these are method names so shouldn't be translated
	public ValueTask ShowModalAsync() => JsRuntime?.InvokeVoidAsync("showModal", DialogRef) ?? ValueTask.CompletedTask;

	public ValueTask CloseAsync() => JsRuntime?.InvokeVoidAsync("close", DialogRef) ?? ValueTask.CompletedTask;

	public ValueTask<bool> IsOpen() => JsRuntime?.InvokeAsync<bool>("isOpen", DialogRef) ?? ValueTask.FromResult(false);
#pragma warning restore CW1161
}
