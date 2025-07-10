using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace CargoWiseNext.Blazor.Components;

[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Referenced in CwnListItem.razor")]
public partial class CwnListItem : CwnComponentBase
{
	[Parameter]
	public RenderFragment? ChildContent { get; set; }

	[Parameter]
	public bool IsSelected { get; set; }

	[CascadingParameter]
	public CwnList? List { get; set; }

	[Parameter]
	public EventCallback<MouseEventArgs> OnClick { get; set; }

	[Parameter]
	public EventCallback<MouseEventArgs> OnRightClick { get; set; }

	[Parameter]
	public EventCallback<MouseEventArgs> OnMouseDown { get; set; }

	protected virtual string Classname => new CssBuilder()
		.AddClass("cwn-list-item")
		.AddClass("cwn-list-item--selected", IsSelected)
		.AddClass(Class)
		.Build();

	protected virtual string? Stylename => new StyleBuilder()
		.AddStyle(Style)
		.Build();

	protected override async Task OnInitializedAsync()
	{
		await base.OnInitializedAsync();
		List?.AddItem(this);
	}

	async Task OnClickHandlerAsync(WebMouseEventArgs e)
	{
		List?.UpdateSelected(this);
		if (OnClick.HasDelegate)
		{
			await OnClick.InvokeAsync(e);
		}
	}

	async Task OnContextMenuHandlerAsync(WebMouseEventArgs e)
	{
		if (OnRightClick.HasDelegate)
		{
			await OnRightClick.InvokeAsync(e);
		}
	}

	async Task OnMouseDownHandlerAsync(MouseEventArgs e)
	{
		if (OnMouseDown.HasDelegate)
		{
			await OnMouseDown.InvokeAsync(e);
		}
	}

	public void UnSelect()
	{
		IsSelected = false;
		StateHasChanged();
	}

	public void Select()
	{
		IsSelected = true;
		StateHasChanged();
	}
}
