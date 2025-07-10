using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace CargoWiseNext.Blazor.Components;

public partial class CwnInput
{
	[Parameter] public string Value { get; set; } = string.Empty;
	[Parameter] public EventCallback<string> ValueChanged { get; set; }

	[Parameter] public string? Placeholder { get; set; }
	[Parameter] public InputType InputType { get; set; } = InputType.Text;
	[Parameter] public int MaxLength { get; set; } = 250;
	[Parameter] public bool Disabled { get; set; }

	[Parameter] public Icon? StartIcon { get; set; }
	[Parameter] public Icon? EndIcon { get; set; }
	[Parameter] public Size IconSize { get; set; } = Size.Small;
	[Parameter] public Color IconColor { get; set; } = Color.Default;

	[Parameter] public EventCallback OnFocus { get; set; }
	[Parameter] public EventCallback OnBlur { get; set; }
	[Parameter] public EventCallback<KeyboardEventArgs> OnKeyDown { get; set; }

	public bool IsFocused { get; private set; }

	string internalValue { get; set; } = string.Empty;

	string? Classname => new CssBuilder()
		.AddClass("cwn-input")
		.AddClass("cwn-input--disabled", Disabled)
		.AddClass(Class)
		.Build();

	string? Stylename => new StyleBuilder()
		.AddStyle(Style)
		.Build();

	async Task HandleOnFocusedAsync(FocusEventArgs e)
	{
		IsFocused = true;
		if (OnFocus.HasDelegate)
		{
			await OnFocus.InvokeAsync();
		}
	}

	async Task HandleOnBlurAsync(FocusEventArgs e)
	{
		IsFocused = false;
		if (OnBlur.HasDelegate)
		{
			await OnBlur.InvokeAsync();
		}
	}

	string? newValue;
	async Task HandleOnInputAsync(ChangeEventArgs e)
	{
		newValue = e.Value?.ToString() ?? string.Empty;
		await ValueChanged.InvokeAsync(newValue);
	}

	async Task HandleKeyDownAsync(KeyboardEventArgs e)
	{
		await OnKeyDown.InvokeAsync(e);
	}

	protected override void OnParametersSet()
	{
		// To avoid swallowing chars when typing rapidly
		if (Value == newValue)
		{
			return;
		}

		internalValue = Value;
	}
}
