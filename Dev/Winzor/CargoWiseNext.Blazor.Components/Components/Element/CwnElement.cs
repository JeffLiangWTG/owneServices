using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace CargoWiseNext.Blazor.Components;

/// <summary>
/// A primitive component which allows dynamically changing the HTML element rendered under the hood.
/// </summary>
public class CwnElement : CwnComponentBase
{
	[Parameter]
	public RenderFragment? ChildContent { get; set; }

	[Parameter]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Html")]
	public string HtmlTag { get; set; } = "span";

	[Parameter]
	public ElementReference? Ref { get; set; }

	[Parameter]
	public string? HRef { get; set; }

	[Parameter]
	public string? TabIndex { get; set; }

	[Parameter]
	public string? Key { get; set; }

	[Parameter]
	public EventCallback<ElementReference> RefChanged { get; set; }

	[Parameter]
	public EventCallback<WebMouseEventArgs> OnSingleClick { get; set; } = default;

	[Parameter]
	public EventCallback<WebMouseEventArgs> OnMouseDown { get; set; } = default;

	[Parameter]
	public EventCallback<WebMouseEventArgs> OnContextMenu { get; set; } = default;

	[Parameter]
	public EventCallback OnFocusIn { get; set; } = default;

	[Parameter]
	public bool ClickStopPropagation { get; set; } = true;

	[Parameter]
	public bool ClickPreventDefault { get; set; } = true;

	[Parameter]
	public bool MouseDownStopPropagation { get; set; } = true;

	[Parameter]
	public bool MouseDownPreventDefault { get; set; } = true;

	[Parameter]
	public bool ContextMenuStopPropagation { get; set; } = true;

	[Parameter]
	public bool ContextMenuPreventDefault { get; set; } = true;

	[Parameter]
	public bool FocusInStopPropagation { get; set; } = true;

	[Parameter]
	public bool FocusInPreventDefault { get; set; } = true;

	[Parameter]
	public string? AccessKey { get; set; }

	[Parameter]
	public bool Disabled { get; set; }

	protected virtual async Task HandleOnFirstClickAsync(WebMouseEventArgs ev)
	{
		if (ev.Detail <= 1)
		{
			await OnSingleClick.InvokeAsync(ev);
		}
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Css")]
	protected override void BuildRenderTree(RenderTreeBuilder builder)
	{
		base.BuildRenderTree(builder);

		// Sequence number.
		// https://learn.microsoft.com/aspnet/core/blazor/advanced-scenarios.

		// Open element.
		builder.OpenElement(0, HtmlTag);

		// Splatted attributes.
		builder.AddMultipleAttributes(1, UserAttributes!);

		// Add class and style attributes.
		builder.AddAttribute(2, "class", Class);
		builder.AddAttribute(3, "style", Style);

		// Add event attributes.
		builder.AddAttribute(4, "onclick", HandleOnFirstClickAsync);
		builder.AddEventStopPropagationAttribute(5, "onclick", OnSingleClick.HasDelegate && ClickStopPropagation);
		builder.AddEventPreventDefaultAttribute(6, "onclick", OnSingleClick.HasDelegate && ClickPreventDefault);

		builder.AddAttribute(7, "onmousedown", OnMouseDown);
		builder.AddEventStopPropagationAttribute(8, "onmousedown", OnMouseDown.HasDelegate && MouseDownStopPropagation);
		builder.AddEventStopPropagationAttribute(9, "onmousedown", OnMouseDown.HasDelegate && MouseDownPreventDefault);

		builder.AddAttribute(10, "oncontextmenu", OnContextMenu);
		builder.AddEventStopPropagationAttribute(11, "oncontextmenu", OnContextMenu.HasDelegate && ContextMenuStopPropagation);
		builder.AddEventPreventDefaultAttribute(12, "oncontextmenu", OnContextMenu.HasDelegate && ContextMenuPreventDefault);

		builder.AddAttribute(13, "onfocusin", OnFocusIn);
		builder.AddEventStopPropagationAttribute(14, "onfocusin", OnFocusIn.HasDelegate && FocusInStopPropagation);
		builder.AddEventPreventDefaultAttribute(15, "onfocusin", OnFocusIn.HasDelegate && FocusInPreventDefault);

		//Add additional attributes
		builder.AddAttribute(16, "accesskey", AccessKey);
		builder.AddAttribute(17, "disabled", Disabled);
		builder.AddAttribute(18, "href", HRef);
		builder.AddAttribute(19, "tabindex", TabIndex);
		builder.AddAttribute(20, "key", Key);

		// Capture the element reference if specified.
		if (Ref != null)
		{
			builder.AddElementReferenceCapture(21, async capturedRef =>
			{
				Ref = capturedRef;
				await RefChanged.InvokeAsync(Ref.Value);
			});
		}

		// Add child content.
		builder.AddContent(22, ChildContent);

		// Close element.
		builder.CloseElement();
	}
}
