#nullable enable
using System.Threading.Tasks;
using CargoWise.Blazor.Client.Integration.Menus;
using CargoWise.NetworkVisualisation.GUI.Extensions;
using Microsoft.AspNetCore.Components;
using WinzorFramework.JSInterop;

namespace CargoWise.NetworkVisualisation.GUI.Components;

[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Referenced in DynamicTextArea.razor")]
/// <summary>
///Represents a component for dynamic text areas, which supports
///custom styling, placeholder text, and manages user interactions
///such as input events.
/// </summary>
public partial class DynamicTextArea : ComponentBase
{
	/// <summary>
	/// Gets or sets the CSS class of the text area.
	/// </summary>
	[Parameter]
	public string? Class { get; set; }

	/// <summary>
	/// Gets or sets the inline CSS styles for the text area.
	/// </summary>
	[Parameter]
	public string? Style { get; set; }

	/// <summary>
	/// Gets or sets the placeholder text for the text area.
	/// </summary>
	[Parameter]
	public string? Placeholder { get; set; }

	/// <summary>
	/// Gets or sets the value of the text area.
	/// </summary>
	[Parameter]
	public string? Value { get; set; }

	/// <summary>
	/// Event that is triggered when the value of the text area is changed.
	/// </summary>
	[Parameter]
	public EventCallback<string> ValueChanged { get; set; }

	/// <summary>
	/// Gets or sets the ARIA label for accessibility.
	/// </summary>
	[Parameter]
	public string? AriaLabel { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the text area is read-only.
	/// </summary>
	[Parameter]
	public bool? Readonly { get; set; }

	/// <summary>
	/// Gets or sets the maximum length of the text area value.
	/// </summary>
	[Parameter]
	public int? Maxlength { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether the text area is currently being moved.
	/// </summary>
	[Parameter]
	public bool IsMoving { get; set; }

	/// <summary>
	/// Gets or sets the service used to display menus.
	/// </summary>
	[Inject]
	public IMenuDisplayer? MenuDisplayer { get; set; }

	/// <summary>
	/// Gets or sets the service used for clipboard interactions.
	/// </summary>
	[Inject]
	public IClipboardJSInterop? ClipboardInterop { get; set; }

	async Task OnValueChanged()
	{
		if (Readonly == true)
		{
			return;
		}
		if (Maxlength is not null && Value?.Length > Maxlength)
		{
			return;
		}
		await ValueChanged.InvokeAsync(Value);
	}

	async Task OpenContextMenuAsync(WebMouseEventArgs args)
	{
		await MenuDisplayer.ShowClipboardContextMenuAsync(args, ClipboardInterop, elementReference);
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "Analyzer runner incorrectly detecting.")]
	ElementReference elementReference;
}
