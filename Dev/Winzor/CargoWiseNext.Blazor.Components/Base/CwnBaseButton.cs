using Microsoft.AspNetCore.Components;

namespace CargoWiseNext.Blazor.Components;

public abstract class CwnBaseButton : CwnComponentBase
{
	[CascadingParameter]
	protected IActivatable? Activatable { get; set; }

	[CascadingParameter(Name = "ParentDisabled")]
	bool ParentDisabled { get; set; }

	[Parameter]
	public bool Disabled { get; set; }

	[Parameter]
	public bool DropShadow { get; set; } = true;

	[Parameter]
	public string? AccessKey { get; set; }

	[Parameter]
	public EventCallback<WebMouseEventArgs> OnClick { get; set; }

	[Parameter]
	public EventCallback<WebMouseEventArgs> OnMouseDown { get; set; }

	[Parameter]
	public EventHandler<WebMouseEventArgs>? OnButtonClicked { get; set; }

	[Parameter]
	public EventCallback OnFocusIn { get; set; }

	protected bool GetDisabledState() => Disabled || ParentDisabled;

	protected virtual async Task OnClickHandlerAsync(WebMouseEventArgs ev)
	{
		if (GetDisabledState())
		{
			return;
		}

		if (OnClick.HasDelegate)
		{
			await OnClick.InvokeAsync(ev);
		}
		OnButtonClicked?.Invoke(this, ev);
		Activatable?.Activate(this, ev);
	}

	protected virtual async Task OnMouseDownHandlerAsync(WebMouseEventArgs ev)
	{
		if (OnMouseDown.HasDelegate)
		{
			await OnMouseDown.InvokeAsync(ev);
		}
	}

	protected async Task HandleOnFocusInAsync()
	{
		if (OnFocusIn.HasDelegate)
		{
			await OnFocusIn.InvokeAsync();
		}
	}

	protected ElementReference _elementReference;

	public ElementReference ElementReference
	{
		get => _elementReference;
	}

	public ValueTask FocusAsync() => _elementReference.FocusAsync();
}
