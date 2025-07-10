using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace CargoWiseNext.Blazor.Components;

[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Referenced in CwnTextField.razor")]
public partial class CwnTextField : CwnComponentBase
{
	protected string Classname =>
		new CssBuilder()
			.AddClass("cwn-text-field")
			.AddClass(Class)
			.Build();

	public string? Value { get; set; }

	[Parameter]
	public string? Placeholder { get; set; }

	[Parameter]
	public InputType InputType { get; set; } = InputType.Text;

	[Parameter]
	public EventCallback<string?> OnInput { get; set; }

	[Parameter]
	public EventCallback OnFocus { get; set; }

	[Parameter]
	public EventCallback OnBlur { get; set; }

	public bool IsFocused { get; private set; }

	/// <summary>
	/// OnInput captures value of the input field.
	/// Avoid assigning to the binding Value property to avoid two way binding problems.
	/// Input value is passed back via OnInput callback.
	/// </summary>
	/// <param name="e">Change event contains the input value</param>
	async Task OnInputChangeAsync(ChangeEventArgs e)
	{
		if (OnInput.HasDelegate)
		{
			await OnInput.InvokeAsync(e.Value?.ToString());
		}
	}
	async Task OnFocusedAsync(FocusEventArgs e)
	{
		IsFocused = true;
		if (OnFocus.HasDelegate)
		{
			await OnFocus.InvokeAsync();
		}
	}

	protected ElementReference _elementReference;

	public ValueTask FocusAsync() => _elementReference.FocusAsync();
}
