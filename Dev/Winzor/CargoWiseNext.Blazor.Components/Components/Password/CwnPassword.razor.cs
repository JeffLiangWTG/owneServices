using Microsoft.AspNetCore.Components;

namespace CargoWiseNext.Blazor.Components;

public partial class CwnPassword : CwnComponentBase
{
	protected string Classname =>
		new CssBuilder()
			.AddClass(Class)
			.Build();

	string? Value { get; set; }

	[Parameter]
	public string? Placeholder { get; set; }

	[Parameter]
	public InputType InputType { get; set; } = InputType.Password;

	[Parameter]
	public EventCallback<string?> OnInput { get; set; }

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Referenced in CwnPassword.razor")]
	async Task OnInputChangeAsync(ChangeEventArgs e)
	{
		Value = e.Value?.ToString();
		if (OnInput.HasDelegate)
		{
			await OnInput.InvokeAsync(Value);
		}
	}
}
