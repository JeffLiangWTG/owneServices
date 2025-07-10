using Microsoft.AspNetCore.Components;

namespace CargoWiseNext.Blazor.Components;

public partial class CwnList : CwnComponentBase
{
	[Parameter]
	public RenderFragment? ChildContent { get; set; }

	protected virtual string Classname => new CssBuilder()
		.AddClass("cwn-list")
		.AddClass(Class)
		.Build();

	protected virtual string? Stylename => new StyleBuilder()
		.AddStyle(Style)
		.Build();

	public List<CwnListItem> Items { get; init; } = new();

	internal void AddItem(CwnListItem item)
	{
		Items.Add(item);
	}

	internal void UpdateSelected(CwnListItem listItem)
	{
		foreach (var item in Items)
		{
			if (item == listItem)
			{
				item.Select();
			}
			else
			{
				item.UnSelect();
			}
		}
	}
}
