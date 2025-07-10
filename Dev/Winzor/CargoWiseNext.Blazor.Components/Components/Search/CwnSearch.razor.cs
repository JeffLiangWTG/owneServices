using Microsoft.AspNetCore.Components;

namespace CargoWiseNext.Blazor.Components;

public partial class CwnSearch : CwnComponentBase
{
	protected string Classname =>
		new CssBuilder()
		.AddClass("cwn-search")
		.AddClass(Class)
		.Build();

	[Parameter]
	public string? Placeholder { get; set; }

	[Parameter]
	public EventCallback<string?> OnInput { get; set; }

	[Parameter]
	public EventCallback OnFocus { get; set; }

	[Parameter]
	public List<string> Shortcut { get; set; } = new List<string>();

	protected CwnTextField _inputRef = default!;

	public ValueTask FocusAsync() => _inputRef.FocusAsync();

	public void Clear() => _inputRef.Value = string.Empty;
}
