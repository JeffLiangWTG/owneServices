using Microsoft.AspNetCore.Components;

namespace CargoWiseNext.Blazor.Components;

public partial class CwnPopover : CwnComponentBase
{
	[Inject]
	public IPopoverService? PopoverService { get; set; }

	[Parameter]
	public string Id { get; set; } = new Guid().ToString();

	[Parameter]
	public string TargetId { get; set; } = string.Empty;

	[Parameter]
	public RenderFragment? ChildContent { get; set; }

	[Parameter]
	public PopoverType Type { get; set; } = PopoverType.Manual;

	[Parameter]
	public EventCallback<bool> OnToggle { get; set; }

	protected virtual string? Classname => new CssBuilder()
	.AddClass("cwn-popover")
	.AddClass(Class)
	.Build();

	protected virtual string? Stylename => new StyleBuilder()
		.AddStyle(Style)
		.Build();

	protected ElementReference _elementReference;

	public bool IsOpen { get; protected set; }

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Referenced in CwnPopover.razor")]
	void HandleToggle()
	{
		IsOpen = !IsOpen;
		OnToggle.InvokeAsync(IsOpen);
	}

	public void Show()
	{
		PopoverService?.ShowAsync(Id);
	}

	public void Hide()
	{
		PopoverService?.HideAsync(Id);
	}

	public void Toggle()
	{
		PopoverService?.ToggleAsync(Id);
	}
}

