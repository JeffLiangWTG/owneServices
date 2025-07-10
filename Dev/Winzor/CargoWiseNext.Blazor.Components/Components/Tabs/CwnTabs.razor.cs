using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace CargoWiseNext.Blazor.Components;

[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard coded property names")]
[SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Referenced in CwnTabs.razor")]
public partial class CwnTabs : CwnComponentBase, IAsyncDisposable
{
	protected string Classname => new CssBuilder()
	.AddClass(Class)
	.Build();

	bool isDisposed;
	int activePanelIndex;
	bool isRendered;

	/// <summary>
	/// If true, render all tabs and hide (display:none) every non-active.
	/// </summary>
	[Parameter]
	public bool KeepPanelsAlive { get; set; } = false;

	/// <summary>
	/// If true, sets the border-radius to theme default.
	/// </summary>
	[Parameter]
	public bool Rounded { get; set; } = true;

	/// <summary>
	/// If true, sets a border between the content and the tabHeader depending on the position.
	/// </summary>
	[Parameter]
	public bool Border { get; set; } = false;

	/// <summary>
	/// If true, tabHeader will be outlined.
	/// </summary>
	[Parameter]
	public bool Outlined { get; set; } = false;

	/// <summary>
	/// If true, centers the tabitems.
	/// </summary>
	[Parameter]
	public bool Centered { get; set; } = true;

	/// <summary>
	/// Sets the position of the tabs itself.
	/// </summary>
	[Parameter]
	public Position Position { get; set; } = Position.Top;

	/// <summary>
	/// The color of the component. It supports the theme colors.
	/// </summary>
	[Parameter]
	public Color Color { get; set; } = Color.Default;

	/// <summary>
	/// If true, will apply elevation, rounded, outlined effects to the whole tab component instead of just tabHeader.
	/// </summary>
	[Parameter]
	public bool ApplyEffectsToContainer { get; set; } = false;

	/// <summary>
	/// Child content of component.
	/// </summary>
	[Parameter]
	public RenderFragment? ChildContent { get; set; }

	/// <summary>
	/// Custom class/classes for TabPanel
	/// </summary>
	[Parameter]
	public string? TabPanelClass { get; set; }

	/// <summary>
	/// Custom class/classes for TabHeader
	/// </summary>
	[Parameter]
	public string? TabHeaderClass { get; set; }

	/// <summary>
	/// Custom class/classes for the active tab
	/// </summary>
	[Parameter]
	public string? ActiveTabClass { get; set; }

	/// <summary>
	/// Custom class/classes for Selected Content Panel
	/// </summary>
	[Parameter]
	public string? PanelClass { get; set; }

	public CwnTabPanel? ActivePanel { get; set; }

	/// <summary>
	/// The current active panel index. Also with Bidirectional Binding
	/// </summary>
	[Parameter]
	[SuppressMessage("Blazor", "BL0007", Justification = "'Component parameter 'ActivePanelIndex' should be auto property' not true")]
	public int ActivePanelIndex {
		get
		{
			return activePanelIndex;
		}
		set
		{
			SetActiveIndex(value);
		}
	}

	void SetActiveIndex(int value)
	{
		var validPanel = _panels.Count > 0 && value != -1 && value <= _panels.Count - 1;

		if (activePanelIndex != value)
		{
			activePanelIndex = value;
			if (isRendered)
			{
				ActivePanel = validPanel ? _panels[value] : null;
				ActivePanelIndexChanged.InvokeAsync(value);
			}
		}
		else if (validPanel)
		{
			ActivePanel = _panels[value];
		}
	}

	/// <summary>
	/// Fired when ActivePanelIndex changes.
	/// </summary>
	[Parameter]
	public EventCallback<int> ActivePanelIndexChanged { get; set; }

	/// <summary>
	/// A readonly list of the current panels. Panels should be added or removed through the RenderTree use this collection to get informations about the current panels
	/// </summary>
	public IReadOnlyList<CwnTabPanel> Panels { get; set; }

	readonly List<CwnTabPanel> _panels;

	/// <summary>
	/// A render fragment that is added before or after (based on the value of HeaderPosition) the tabs inside the header panel of the tab control
	/// </summary>
	[Parameter]
	public RenderFragment<CwnTabs>? Header { get; set; }

	/// <summary>
	/// Additional content specified by Header is placed either before the tabs, after or not at all
	/// </summary>
	[Parameter]
	public TabHeaderPosition HeaderPosition { get; set; } = TabHeaderPosition.After;

	/// <summary>
	/// A render fragment that is added before or after (based on the value of HeaderPosition) inside each tab panel
	/// </summary>
	[Parameter]
	public RenderFragment<CwnTabPanel>? TabPanelHeader { get; set; }

	/// <summary>
	/// Additional content specified by Header is placed either before the tabs, after or not at all
	/// </summary>
	[Parameter]
	public TabHeaderPosition TabPanelHeaderPosition { get; set; } = TabHeaderPosition.After;

	/// <summary>
	/// Fired when a panel gets activated. Returned Task will be awaited.
	/// </summary>
	[Parameter]
	public Func<TabInteractionEventArgs, Task>? OnPreviewInteraction { get; set; }

	/// <summary>
	/// Can be used in derived class to add a class to the main container. If not overwritten return an empty string
	/// </summary>
	protected virtual string InternalClassName { get; } = string.Empty;

	#region Life cycle management

	public CwnTabs()
	{
		_panels = new List<CwnTabPanel>();
		Panels = _panels.AsReadOnly();
	}

	protected override Task OnAfterRenderAsync(bool firstRender)
	{
		if (firstRender)
		{
			var items = _panels.Select(x => x.PanelRef).ToList();

			if (activePanelIndex != -1 && _panels.Count > 0)
			{
				ActivePanel = _panels[activePanelIndex];
			}

			StateHasChanged();
			ActivatePanel(ActivePanelIndex);

			isRendered = true;
		}

		return Task.CompletedTask;
	}

	public ValueTask DisposeAsync()
	{
		isDisposed = true;
		return ValueTask.CompletedTask;
	}

	#endregion

	#region Children

	internal void AddPanel(CwnTabPanel tabPanel)
	{
		_panels.Add(tabPanel);
		if (_panels.Count == activePanelIndex + 1 || activePanelIndex == -1 && _panels.Count == 1)
		{
			ActivePanel = tabPanel;
		}
		StateHasChanged();
	}

	internal void SetPanelRef(ElementReference reference)
	{
		if (isRendered)
		{
			StateHasChanged();
		}
	}

	internal async Task RemovePanel(CwnTabPanel tabPanel)
	{
		if (isDisposed)
		{
			return;
		}

		var index = _panels.IndexOf(tabPanel);

		// We're at the right-most tab.
		if (activePanelIndex == index && index == _panels.Count - 1)
		{
			if (_panels.Count == 1)
			{
				SetActiveIndex(-1);
			}
			else if (index > 0)
			{
				SetActiveIndex(index - 1);
			}
			else
			{
				SetActiveIndex(0);
			}
		}

		// Active tab is not necessarily the tab being closed.
		else if (activePanelIndex > index)
		{
			activePanelIndex--;
			await ActivePanelIndexChanged.InvokeAsync(activePanelIndex);
		}

		_panels.Remove(tabPanel);
		StateHasChanged();
	}

	public void ActivatePanel(CwnTabPanel? panel, bool ignoreDisabledState = false)
	{
		if (panel is not null && _panels.IndexOf(panel) > -1)
		{
			ActivatePanel(panel, null, ignoreDisabledState);
		}
	}

	public void ActivatePanel(int index, bool ignoreDisabledState = false)
	{
		if (index > -1 && index <= _panels.Count - 1)
		{
			ActivatePanel(_panels[index], null, ignoreDisabledState);
		}
	}

	public void ActivatePanel(object id, bool ignoreDisabledState = false)
	{
		var panel = _panels.FirstOrDefault(p => Equals(p.ID, id));
		if (panel != null)
		{
			ActivatePanel(panel, null, ignoreDisabledState);
		}
	}

	async void ActivatePanel(CwnTabPanel panel, MouseEventArgs? ev, bool ignoreDisabledState = false)
	{
		if (!panel.Disabled || ignoreDisabledState)
		{
			var index = _panels.IndexOf(panel);
			var previewArgs = new TabInteractionEventArgs
			{
				PanelIndex = index,
				InteractionType = TabInteractionType.Activate
			};

			if (OnPreviewInteraction != null)
			{
				await OnPreviewInteraction.Invoke(previewArgs);
			}

			if (previewArgs.Cancel)
			{
				return;
			}

			SetActiveIndex(previewArgs.PanelIndex);
			if (ActivePanel is not null)
			{
				await ActivePanel.OnClick.InvokeAsync(ev);
			}

			StateHasChanged();
		}
	}

	#endregion

	#region Style and classes

	protected string TabsClassnames =>
		new CssBuilder()
			.AddClass("cwn-tabs")
			.AddClass("cwn-tabs--rounded", ApplyEffectsToContainer && Rounded)
			.AddClass("cwn-tabs--outlined", ApplyEffectsToContainer && Outlined)
			.AddClass("cwn-tabs--centered", Centered)
			.AddClass("cwn-tabs--vertical", IsVerticalTabs())
			.AddClass(InternalClassName)
			.AddClass(Class)
			.Build();

	protected string TabBarClassnames =>
		new CssBuilder()
			.AddClass("cwn-tabs__bar")
			.AddClass("cwn-tabs__bar--rounded", !ApplyEffectsToContainer && Rounded)
			.AddClass($"cwn-tabs__bar--{Color.GetDescription()}", Color != Color.Default)
			.AddClass($"cwn-tabs__bar--border-{ConvertPosition(Position).GetDescription()}", Border)
			.AddClass("cwn-tabs__bar--outlined", !ApplyEffectsToContainer && Outlined)
			.AddClass(TabHeaderClass)
			.Build();

#pragma warning disable CS8603 // Possible null reference return.
	string GetTabStyle(CwnTabPanel panel)
	{
		var tabStyle = new StyleBuilder()
			.AddStyle(panel.Style)
			.Build();

		return tabStyle;
	}

	protected string PanelsClassnames =>
		new CssBuilder()
			.AddClass("cwn-tabs__panels")
			.AddClass("cwn-tabs__scrollable")
			.AddClass(PanelClass)
			.Build();
#pragma warning restore CS8603 // Possible null reference return.

	bool IsVerticalTabs()
	{
		return Position is Position.Left or Position.Right or Position.Start or Position.End;
	}

	Position ConvertPosition(Position position)
	{
		return position switch
		{
			Position.Start => Position.Left,
			Position.End => Position.Right,
			_ => position
		};
	}

	string GetTabClass(CwnTabPanel panel)
	{
		var tabClass = new CssBuilder()
			.AddClass("cwn-tab")
		  .AddClass("cwn-tab--active", panel == ActivePanel)
		  .AddClass("cwn-tab--disabled", panel.Disabled)
		  .AddClass(ActiveTabClass, panel == ActivePanel)
		  .AddClass(TabPanelClass)
		  .AddClass(panel.Class)
		  .Build();

		return tabClass;
	}

	#endregion
}

