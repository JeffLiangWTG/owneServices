using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components;

namespace CargoWiseNext.Blazor.Components;

public partial class CwnTabPanel : CwnComponentBase
{
	protected string Classname => new CssBuilder()
		.AddClass(AliveClass)
		.AddClass(Class)
		.Build();

	bool _disposed;

	[CascadingParameter]
	CwnTabs? Parent { get; set; }

	string? aliveClass;
	[Parameter]
	[SuppressMessage("Blazor", "BL0007", Justification = "'Component parameter 'Class' should be auto property' not true")]
	public string? AliveClass
	{
		get
		{
			return aliveClass + " cwn-tabs__panel" + (Parent?.ActivePanel == this ? " cwn-tabs__panel--show" : " cwn-tabs__panel--hidden");
		}
		set
		{
			aliveClass = value;
		}
	}

	/// <summary>
	/// Reference to the underlying panel element.
	/// </summary>
	public ElementReference PanelRef;

	/// <summary>
	/// Text will be displayed in the TabPanel as TabTitle. Text is no longer rendered
	/// as a MarkupString, so use the TabContent RenderFragment instead for HTML content.
	/// </summary>
	[Parameter]
	public string? Text { get; set; }

	/// <summary>
	/// If true, the tabpanel will be disabled.
	/// </summary>
	[Parameter]
	public bool Disabled { get; set; }

	/// <summary>
	/// Unique TabPanel ID. Useful for activation when Panels are dynamically generated.
	/// </summary>
	[Parameter]
	public object? ID { get; set; }

	/// <summary>
	/// Raised when tab is clicked
	/// </summary>
	[Parameter] public EventCallback<WebMouseEventArgs> OnClick { get; set; }

	/// <summary>
	/// Child content of component.
	/// </summary>
	[Parameter]
	public RenderFragment? ChildContent { get; set; }

	/// <summary>
	/// Tab content of component.
	/// </summary>
	[Parameter]
	public RenderFragment? TabContent { get; set; }

	/// <summary>
	/// Tab content wrapper of component. It is used to wrap the content of a tab heading in a user supplied div or component. 
	/// Use @context in the TabWrapperContent to render the tab header within your custom wrapper. 
	/// This is most useful with tooltips, which must wrap the entire content they refer to.
	/// </summary>
	[Parameter]
	public RenderFragment<RenderFragment>? TabWrapperContent { get; set; }

	/// <summary>
	/// TabPanel Tooltip. It will be ignored if TabContent is provided.
	/// </summary>
	[Parameter]
	public string? ToolTip { get; set; }

	/// <inheritdoc/>
	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		await base.OnAfterRenderAsync(firstRender);
		if (firstRender && Parent is not null)
		{
			Parent.SetPanelRef(PanelRef);
		}
	}

	/// <inheritdoc/>
	protected override void OnInitialized()
	{
		// NOTE: we must not throw here because we need the component to be able to live for the API docs to be able to infer default values
		//if (Parent == null)
		//    throw new ArgumentNullException(nameof(Parent), "TabPanel must exist within a Tabs component");
		base.OnInitialized();

		Parent?.AddPanel(this);
	}

	/// <inheritdoc/>
	public async ValueTask DisposeAsync()
	{
		if (_disposed)
		{
			return;
		}

		_disposed = true;
		if (Parent is not null)
		{
			await Parent.RemovePanel(this);
		}
	}
}

