using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;

namespace CargoWiseNext.Blazor.Components;

[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard coded property names")]
public partial class CwnButton : CwnBaseButton, IHandleEvent, IDisposable
{
	protected string Classname => new CssBuilder()
		.AddClass("cwn-button")
		.AddClass($"cwn-button--{Variant.GetDescription()}")
		.AddClass($"cwn-{Color?.GetDescription()}", Color.HasValue)
		.AddClass("cwn-button--full-width", GetRealFullWidth())
		.AddClass("cwn-button--disable-elevation", !DropShadow)
		.AddClass(Class)
		.Build();

	[SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Referenced in CwnButton.razor")]
	string ClassnameCaption => new CssBuilder()
		.AddClass("cwn-button__caption")
		.AddClass(ClassCaption)
		.Build();

	[Parameter]
	public string? ClassCaption { get; set; }

	[Parameter]
	public Icon? StartIcon { get; set; }

	[Parameter]
	public Icon? EndIcon { get; set; }

	[Parameter]
	public Size IconSize { get; set; } = Size.Small;

	[Parameter]
	public Variant Variant { get; set; } = Variant.Text;

	[Parameter]
	public RenderFragment? ChildContent { get; set; }

	[Parameter]
	public Color? Color { get; set; }

	/// <summary>
	/// Expands the button to 100% of the container width.
	/// </summary>
	/// <remarks>
	/// Defaults to <c>false</c>.
	/// </remarks>
	[Parameter]
	public bool FullWidth { get; set; }

	Task IHandleEvent.HandleEventAsync(EventCallbackWorkItem callback, object? arg) => callback.InvokeAsync(arg);

	/// <summary>
	/// The buton group which owns this button.
	/// </summary>
	[CascadingParameter]
#if DEBUG
	public
#endif
	CwnButtonGroup? ButtonGroup { get; set; }

	protected override void OnInitialized()
	{
		base.OnInitialized();
		ButtonGroup?.AddButton(this);
	}

	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposing)
		{
			ButtonGroup?.RemoveButton(this);
		}
	}

	internal bool GetRealFullWidth()
	{
		if (FullWidth)
		{
			return true;
		}
		// If the button is in a group, the group is stretched and none button is explicitly stretched,
		// then the button need to be streched
		// See https://github.com/MudBlazor/MudBlazor/issues/9710
		return ButtonGroup != null && ButtonGroup.FullWidth && ButtonGroup.NoneButtonIsStreched();
	}
}
