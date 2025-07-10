using Microsoft.AspNetCore.Components;

namespace CargoWiseNext.Blazor.Components;

public partial class CwnButtonGroup : CwnComponentBase
{
	protected string Classname => new CssBuilder()
		.AddClass("cwn-button-group")
		.AddClass("cwn-button-group--override-styles", OverrideStyles)
		.AddClass($"cwn-button-group--{Variant.GetDescription()}")
		.AddClass($"cwn-{Color?.GetDescription()}", Color.HasValue)
		.AddClass($"cwn-button-group--size-{Size.GetDescription()}")
		.AddClass("cwn-button-group--vertical", Vertical)
		.AddClass("cwn-button-group--horizontal", !Vertical)
		.AddClass("cwn-button-group--disable-elevation", !DropShadow)
		.AddClass("cwn-button-group--full-width", FullWidth)
		.AddClass(Class)
		.Build();

	/// <summary>
	/// Overrides individual button styles with this group's style.
	/// </summary>
	/// <remarks>
	/// Defaults to <c>true</c>.  When <c>true</c>, the button styles are defined by this group.
	/// </remarks>
	[Parameter]
	public bool OverrideStyles { get; set; } = true;

	/// <summary>
	/// The custom content within this group.
	/// </summary>
	/// <remarks>
	/// This property allows for custom content to displayed inside of the group, but it is not required.
	/// </remarks>
	[Parameter]
	public RenderFragment? ChildContent { get; set; }

	/// <summary>
	/// Displays buttons vertically.
	/// </summary>
	/// <remarks>
	/// Defaults to <c>false</c>.  When <c>true</c>, buttons will be displayed vertically, otherwise horizontally.
	/// </remarks>
	[Parameter]
	public bool Vertical { get; set; }

	/// <summary>
	/// Displays a shadow.
	/// </summary>
	/// <remarks>
	/// Defaults to <c>true</c>.
	/// </remarks>
	[Parameter]
	public bool DropShadow { get; set; } = true;

	/// <summary>
	/// The color of all buttons in this group.
	/// </summary>
	/// <remarks>
	/// Defaults to <see cref="Color.Default" />.  Theme colors are supported.
	/// </remarks>
	[Parameter]
	public Color? Color { get; set; }

	/// <summary>
	/// The size of all buttons in the group.
	/// </summary>
	/// <remarks>
	/// Defaults to <see cref="Size.Medium"/>.
	/// </remarks>
	[Parameter]
	public Size Size { get; set; } = Size.Medium;

	/// <summary>
	/// The display variant of all buttons in the group.
	/// </summary>
	/// <remarks>
	/// Defaults to <see cref="Variant.Text"/>.  Other supported values are <see cref="Variant.Outlined"/> and <see cref="Variant.Filled"/>.
	/// </remarks>
	[Parameter]
	public Variant Variant { get; set; } = Variant.Text;

	/// <summary>
	/// If true, the button group will take up 100% of available width.
	/// </summary>
	[Parameter]
	public bool FullWidth { get; set; }

	readonly List<CwnButton> _renderedButtons = [];

	internal void AddButton(CwnButton button)
	{
		_renderedButtons.Add(button);
	}

	internal void RemoveButton(CwnButton button)
	{
		_renderedButtons.Remove(button);
	}

	internal bool NoneButtonIsStreched()
	{
		return !_renderedButtons.Any(b => b.FullWidth);
	}

#if DEBUG
	public void AddButtonForTest(CwnButton button)
	{
		AddButton(button);
	}
#endif
}

