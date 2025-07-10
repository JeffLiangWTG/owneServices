#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Core;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using WinzorFramework;
using WinzorFramework.JSInterop;

namespace CargoWise.NetworkVisualisation.GUI;

public interface INetworkUserControl : IWinzorDispatch
{
	string? StatusMessage { get; }
	string? StatusTooltip { get; }
	NetworkViewModel NetworkViewModel { get; }
	NetworkUserControlViewModel ViewModel { get; }
	IJSRuntime? JSRuntime { get; }
	void ExecuteRibbonAction(INetworkAction action);
	public void ZoomIn(double scaleFactor = 0.1);
	public void ZoomOut(double scaleFactor = 0.1);
	void SetZoom(double zoom);
	void SavePrevZoom();
	void FitNodes();
}

public interface IWinzorDispatch : IWinzorThreadInfo
{
	Task InvokeWinzorDispatcherAsync(Action action);
	void InvokeRenderDispatcher(Func<Task> workItem, bool needElementRendered = false);
}

public interface IWinzorThreadInfo
{
	bool IsWinzorThread { get; }
	bool IsRenderThread { get; }
}

[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Referenced in NetworkUserControl.razor")]
/// <summary>
/// The top level control for displaying a Network Diagram. All GUI components for a Network Diagram are created either directly or indirectly within this control.
/// </summary>
public partial class NetworkUserControl : UserControl, IWinzorSupportedWPFContent, INetworkUserControl
{
	public new IJSRuntime? JSRuntime { get => base.JSRuntime; }
	public object? DataContext { get; set; }
	public NetworkUserControlViewModel ViewModel => DataContext as NetworkUserControlViewModel ?? throw new InvalidOperationException("null DataContext cannot be used as a ViewModel");
	public DiagramAreaUserControl MainDiagramControl { get; }
	public DiagramAreaUserControl? NonScheduledDiagramControl { get; private set; }
	public NetworkViewModel NetworkViewModel => ViewModel.NetworkViewModel;
	public string? StatusMessage { get; private set; }
	public string? StatusTooltip { get; private set; }
	public override bool UseParentDivForLayout => false;
	public bool IsWinzorThread => !IsInRenderInvoke && WinzorDispatcher.IsCurrent;
	public bool IsRenderThread => !IsWinzorThread;
	public event Action<ThrottledWheelEventArgs>? Wheel;
	public bool IsWRDPopout { get; set; }

#if DEBUG
	public bool RibbonControlIsVisible_ForTesting { get; }
#endif
#pragma warning disable CS0067
	// used in testing
	public event EventHandler<RefreshArgs>? SelectionChanged;
#pragma warning restore CS0067

	internal Ribbon? RibbonReference { get; set; }
	INetworkAction refreshAction => DiagramNetworkActionProvider.GetRefreshAction(NetworkViewModel);
	INetworkAction popoutAction => DiagramNetworkActionProvider.GetPopOutAction(NetworkViewModel, this);
	INetworkAction fitAction => new FitNodesAction(NetworkViewModel, this);
	INetworkAction fillAction => DiagramNetworkActionProvider.GetFillAction(NetworkViewModel, this);
	INetworkAction oneHundredPercentAction => DiagramNetworkActionProvider.GetOneHundredPercentAction(NetworkViewModel, this);
	INetworkAction zoomOutAction => DiagramNetworkActionProvider.GetZoomOutActionButton(this);
	INetworkAction zoomInAction => DiagramNetworkActionProvider.GetZoomInActionButton(this);
	string nonScheduledSectionStyleString => $"width: {nonScheduledSectionWidth}px";
	IEnumerable<DiagramAreaUserControl> DiagramAreaControls
	{
		get
		{
			yield return MainDiagramControl;

			if (NonScheduledDiagramControl is not null)
			{
				yield return NonScheduledDiagramControl;
			}
		}
	}

	readonly INetworkRefresher networkRefresher;
	readonly NodeViewModelProvider? viewModelProvider;
	readonly IRibbonDataProvider? ribbonDataProvider;
	readonly NetworkRibbonResourcesProvider resourcesProvider;
	readonly List<Form> popUpChildren = new();
	double nonScheduledSectionWidth;
	SearchFinderForm? SearchForm;
	double prevZoomScale;
	bool prevZoomSet;
	int zoomPercent;
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0044:Add readonly modifier", Justification = "Analyzer runner incorrectly detecting.")]
	ElementReference splitterReference;
	const double defaultScaleOperand = 0.1;

	public NetworkUserControl(IDiagramEntity? diagramEntity, INetworkRefresher networkRefresher, NodeViewModelProvider? viewModelProvider = null, IRibbonDataProvider? ribbonDataProvider = null, bool isWRDPopout = false)
	{
		this.networkRefresher = networkRefresher;
		this.networkRefresher.Refreshed += Network_Refreshed;
		this.viewModelProvider = viewModelProvider;
		this.ribbonDataProvider = ribbonDataProvider;
		this.IsWRDPopout = isWRDPopout;
		resourcesProvider = new NetworkRibbonResourcesProvider();

		MainDiagramControl = new DiagramAreaUserControl(this, isNonScheduled: false);
		Controls.Add(MainDiagramControl);
		if (diagramEntity != null)
		{
			if (diagramEntity.ShouldShowNonScheduledSection)
			{
				NonScheduledDiagramControl = new DiagramAreaUserControl(this, isNonScheduled: true);
				Controls.Add(NonScheduledDiagramControl);
			}
		}
	}

	/// <summary>
	/// Uses <paramref name="network"/> to set the data context for the control. Expected to be used when creating a new <see cref="NetworkUserControl"/> or when the network is reloaded.
	/// Initializes all properties, controls and events so that <paramref name="network"/> is displayed by the control.
	/// </summary>
	/// <param name="network">The network that will be used to set the data context.</param>
	/// <param name="isReloading">This value is eventually used to determine whether calculation should be suspended when building the network.</param>
	public void SetDataContext(INetwork network, bool isReloading)
	{
		if (DataContext is not null)
		{
			ViewModel.PropertyChanged -= NetworkUserControlViewModel_OnPropertyChanged;
			NetworkViewModel?.Deactivate();
		}
		UnsubscribeFromNetworkViewModelEvents();

		var dataContext = new NetworkUserControlViewModel(network, viewModelProvider, isReloading, ribbonDataProvider, resourcesProvider, this, network.DiagramEntity.ShapeInspectorVisible);
		dataContext.SetupNetworkActions(this);
		dataContext.PropertyChanged += NetworkUserControlViewModel_OnPropertyChanged;

		if (DataContext is not null)
		{
			dataContext.ContentScale = ViewModel.ContentScale;
			dataContext.ContentOffsetY = ViewModel.ContentOffsetY;
			dataContext.ContentOffsetX = ViewModel.ContentOffsetX;
		}

		DataContext = dataContext;

		if (DataContext is not null)
		{
			UpdateProperty(ref zoomPercent, (int)(ViewModel.ContentScale * 100));
		}

		StatusMessage = ViewModel.StatusMessage;
		StatusTooltip = ViewModel.StatusTooltip;

		SetUpDiagramAreaControls();
		SubscribeToNetworkViewModelEvents();
		NotifyRenderRequired();
	}

	public Task InvokeWinzorDispatcherAsync(Action action) => base.InvokeWinzorDispatcherAsync(action);

	public SearchFinderForm OpenFinderForm()
	{
		if (SearchForm is null || SearchForm.IsDisposed)
		{
			SearchForm = new SearchFinderForm(this);
			SearchForm.Show();
		}
		SearchForm.ReFocus();
		return SearchForm;
	}

	public void CloseFinderForm()
	{
		SearchForm?.Close();
	}

	/// <summary>
	/// Creates and shows a new form with a new <see cref="NetworkUserControl"/> for the same network.
	/// </summary>
	public void PopOut()
	{
		var viewModel = ViewModel;
		Debug.Assert(NetworkViewModel != null, nameof(NetworkViewModel) + (NoResString)" != null");
		var popoutControl = new NetworkUserControl(viewModel.DiagramEntity, networkRefresher,
			NetworkViewModel.NodeViewModelProvider, ribbonDataProvider, IsWRDPopout);
		popoutControl.SetDataContext(networkRefresher.GetReloadedNetwork(), isReloading: false);
		var form = new PopoutForm(popoutControl);

		Control? backgroundColorSource = Parent;
		while (backgroundColorSource is { BackColor.A: 0 })
		{
			backgroundColorSource = backgroundColorSource.Parent;
		}
		if (backgroundColorSource is not null)
		{
			form.BackColor = backgroundColorSource.BackColor;
		}

		popUpChildren.Add(form);
		form.Show();
	}

	/// <summary>
	/// Close all pop up forms created by this <see cref="NetworkUserControl"/>.
	/// </summary>
	public void CloseAllChildren()
	{
		var formsToClose = popUpChildren.ToArray();
		popUpChildren.Clear();
		foreach (var form in formsToClose)
		{
			form.Close();
		}
	}

	/// <summary>
	/// Set zoom to 100%. Saves previous zoom amount first. Saves current zoom first to enable jump back to previous zoom functionality.
	/// </summary>
	public void OneHundredPercent()
	{
		SavePrevZoom();
		ViewModel.ContentScale = 1;
	}

	/// <summary>
	/// Zoom out by amount specified in <paramref name="scaleSubtrahend"/>. Saves current zoom first to enable jump back to previous zoom functionality.
	/// </summary>
	/// <param name="scaleSubtrahend">The amount to zoom out by as a decimal.</param>
	public void ZoomOut(double scaleSubtrahend = defaultScaleOperand)
	{
		SetContentScale(ViewModel.ContentScale - scaleSubtrahend);
		ScrollToMaintainView();
		UpdateZoomInDiagram(ViewModel.ContentScale);
	}

	/// <summary>
	/// Zoom in by amount specified in <paramref name="scaleAddend"/>. Saves current zoom first to enable jump back to previous zoom functionality.
	/// </summary>
	/// <param name="scaleAddend">The amount to zoom in by as a decimal.</param>
	public void ZoomIn(double scaleAddend = defaultScaleOperand)
	{
		SetContentScale(ViewModel.ContentScale + scaleAddend);
		ScrollToMaintainView();
		UpdateZoomInDiagram(ViewModel.ContentScale);
	}

	/// <summary>
	/// Scroll the diagram to maintain the current view after zoom in/out.
	/// </summary>
	void ScrollToMaintainView()
	{
		RegisterAfterRenderAction(() => MainDiagramControl.ScrollToMaintainView());
	}

	/// <summary>
	/// If the zoom has been previously adjusted and the zoom level saved, sets the zoom to the saved amount.
	/// Resets saved zoom therefore immediate subsequent calls to this method have no effect.
	/// </summary>
	public void JumpBackToPrevZoom()
	{
		if (prevZoomSet)
		{
			ViewModel.ContentScale = prevZoomScale;
		}
		prevZoomSet = false;
	}

	/// <summary>
	/// Sets the zoom level to amount specifed in <paramref name="zoom"/>.
	/// </summary>
	/// <param name="zoom">The amount to set zoom to as a decimal.</param>
	public void SetZoom(double zoom)
	{
		ViewModel.ContentScale = zoom;
	}

	/// <summary>
	/// Save current zoom level to enable jump back to previous zoom functionality.
	/// </summary>
	public void SavePrevZoom()
	{
		prevZoomScale = ViewModel.ContentScale;
		prevZoomSet = true;
	}

	/// <summary>
	/// Invokes <see cref="DiagramAreaUserControl.FillNodes"/> on the <see cref="MainDiagramControl"/>. Saves current zoom first to enable jump back to previous zoom functionality.
	/// </summary>
	public void Fill()
	{
		SavePrevZoom();
		MainDiagramControl.FillNodes();
	}

	/// <summary>
	/// Invokes <see cref="DiagramAreaUserControl.FitNodes"/> on the <see cref="MainDiagramControl"/>. Saves current zoom first to enable jump back to previous zoom functionality.
	/// </summary>
	public void FitNodes()
	{
		SavePrevZoom();
		MainDiagramControl.FitNodes();
	}

	public void SetNonScheduledSectionWidth(double width)
	{
		ViewModel.DiagramEntity.NonScheduledSectionWidth = width;
		nonScheduledSectionWidth = width;
	}

	/// <summary>
	/// Focuses on the search box within the diagram ribbon if it exists.
	/// </summary>
	public void SwitсhFocusToFinder()
	{
		if (RibbonReference is not null)
		{
			RibbonReference.FocusOnSearchBox();
		}
	}

	public void ExecuteRibbonAction(INetworkAction action)
	{
		MainDiagramControl.ExecuteRibbonAction(action);
	}

	public void ScrollToEntity(INetworkEntity entity, bool animated = false)
	{
		MainDiagramControl.ScrollToEntity(entity, animated);
	}

	void NetworkUserControlViewModel_OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		DoOnDiagramControls(c => c.NetworkViewModel_PropertyChanged(sender, e));
		switch (e.PropertyName)
		{
			case nameof(ViewModel.ShapeInspectorVisible):
			case nameof(ViewModel.ShapeInspectorNodeViewModel):
				NotifyRenderRequired();
				break;
			case nameof(ViewModel.ContentScale):
				UpdateProperty(ref zoomPercent, (int)(ViewModel.ContentScale * 100));
				UpdateZoomInDiagram(ViewModel.ContentScale);
				if (DataContext is not null)
				{
					ViewModel.RibbonViewModel?.RefreshActions(new RefreshArgs(RefreshType.None));
				}
				break;
		}
	}

	void UpdateZoomInDiagram(double contentScale)
	{
		InvokeRenderDispatcher(() =>
		{
			DoOnDiagramControls(control => control.DiagramModel?.SetZoom(contentScale));
			return Task.CompletedTask;
		});
	}

	bool disposed;
	protected override void Dispose(bool disposing)
	{
		if (disposed)
		{
			return;
		}

		if (disposing && DataContext is not null)
		{
			CloseFinderForm();
			UnsubscribeFromNetworkViewModelEvents();
			ViewModel.PropertyChanged -= NetworkUserControlViewModel_OnPropertyChanged;
		}

		base.Dispose(disposing);
		disposed = disposing;
	}

	void DoOnDiagramControls(Action<DiagramAreaUserControl> action)
	{
		foreach (var control in DiagramAreaControls.Where(x => x.DataContext is not null))
		{
			action.Invoke(control);
		}
	}

	void SetUpDiagramAreaControls()
	{
		var diagramEntity = ViewModel.DiagramEntity;

		if (diagramEntity.ShouldShowNonScheduledSection)
		{
			if (NonScheduledDiagramControl == null)
			{
				NonScheduledDiagramControl = new DiagramAreaUserControl(this, isNonScheduled: true);
				Controls.Add(NonScheduledDiagramControl);
			}
			nonScheduledSectionWidth = ViewModel.DiagramEntity.NonScheduledSectionWidth;
			Debug.Assert(NonScheduledDiagramControl != null, nameof(NonScheduledDiagramControl) + (NoResString)" != null");
			var nonScheduledViewModel = new DiagramAreaUserControlViewModel(ViewModel, isNonScheduled: true);
			nonScheduledViewModel.PropertyChanged += NonScheduledViewModelOnPropertyChanged;

			NonScheduledDiagramControl.SetDataContext(nonScheduledViewModel);
		}
		else
		{
			SetNonScheduledSectionWidth(0d);
			NonScheduledDiagramControl = null;
		}
		MainDiagramControl.SetDataContext(new DiagramAreaUserControlViewModel(ViewModel, isNonScheduled: diagramEntity.IsNonScheduled));
	}

	void NonScheduledViewModelOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == nameof(DiagramAreaUserControlViewModel.ContentViewportWidth) && ViewModel.DiagramEntity.ShouldShowNonScheduledSection)
		{
			UpdateStoredNonScheduledSectionWidthValue(ViewModel.DiagramEntity.NonScheduledSectionWidth);
		}
	}

	void UpdateStoredNonScheduledSectionWidthValue(double width)
	{
		ViewModel.DiagramEntity.NonScheduledSectionWidth = width;
	}
	void SubscribeToNetworkViewModelEvents()
	{
		if (DataContext != null)
		{
			NetworkViewModel.SelectionChanged += NetworkViewModel_SelectionChanged;
			NetworkViewModel.FocusedEntityChanged += NetworkViewModel_FocusedEntityChanged;
		}
	}

	void UnsubscribeFromNetworkViewModelEvents()
	{
		if (DataContext != null)
		{
			NetworkViewModel.SelectionChanged -= NetworkViewModel_SelectionChanged;
			NetworkViewModel.FocusedEntityChanged -= NetworkViewModel_FocusedEntityChanged;
		}
	}

	internal void AdjustDiagramControlHeights()
	{
		var maxHeight = DiagramAreaControls
			.Where(i => i.DataContext is not null && i.ViewModel.NetworkViewModel is not null)
			.Max(i => i.GetHeightForContent());
		DoOnDiagramControls(control => control.AdjustContentHeight(maxHeight));
	}

	void Network_Refreshed(object? sender, RefreshArgs e)
	{
		switch (e.RefreshType)
		{
			case RefreshType.AffinitiesRefreshRequired:
			case RefreshType.RedrawDiagram:
			case RefreshType.RefreshButton:
			case RefreshType.EntityEdited:
			case RefreshType.EntityRemoved:
			case RefreshType.Saved:
				ReloadNetwork();
				break;

			case RefreshType.EntitySize:
			case RefreshType.EntityCoordinates:
				DoOnDiagramControls(control => control.AdjustViewWidthAndHeight());
				RefreshScale();
				break;

			case RefreshType.TextColorUpdated:
				RefreshScale();
				ReloadNetwork();
				break;

			case RefreshType.Scale:
				RefreshScale();
				break;

			case RefreshType.EntitiesReloaded:
				ViewModel.RefreshNodes();
				break;

			case RefreshType.EntityAdded:
				ViewModel.NetworkViewModel.AddRelationships(e.Entities);
				break;

			case RefreshType.Close:
				CloseAllChildren();
				break;

			case RefreshType.ResourceDependencyAdded:
				DisplayResourceDependencyLink(e.Entities);
				break;

			case RefreshType.EntitiesMovedToDiagramSection:
				ReloadNetwork();
				ViewModel.RefreshNodes();
				OnEntitiesMovedToDiagramSection(e.Entities.ToArray());
				break;

			case RefreshType.RelationshipAdded:
			case RefreshType.RelationshipRemoved:
			case RefreshType.EntityPinnedOrUnpinned:
			case RefreshType.EntityApprovedOrUnapproved:
			case RefreshType.EntityStatusChanged:
			case RefreshType.ConnectionAdded:
			case RefreshType.Saving:
			case RefreshType.ToogleFreezeChannelHeadersAndTimeLabels:
			case RefreshType.ChannelView:
				break;

			case RefreshType.ShapeInspectorVisibilityChanged:
				ViewModel.ShapeInspectorVisible = !ViewModel.ShapeInspectorVisible;
				ViewModel.ShapeInspectorNodeViewModel = ViewModel.NetworkViewModel.GetNodeForEntity(e.Entities.FirstOrDefault());
				break;

			case RefreshType.Affinities:
				PopulateBackgrounds();
				break;

			default:
				throw new NotImplementedException(string.Format(CultureInfo.InvariantCulture, "Refresh Type not implemented: {0}", e.RefreshType.ToString()));
		}
	}

	internal void ReloadNetwork()
	{
		var controller = ViewModel.NetworkModel.Controller;

		// avoid recreating the ribbon after form disposal (after form disposal the controller is deactivated)
		// controller is null when using a dummy network
		if (controller == null || !controller.IsDeactivated)
		{
			SetDataContext(networkRefresher.GetReloadedNetwork(), isReloading: true);
		}
	}

	void RefreshScale()
	{
		DoOnDiagramControls(control => control.RefreshScale());
	}

	void PopulateBackgrounds()
	{
		DoOnDiagramControls(control => control.PopulateBackgrounds());
	}

	void NetworkViewModel_SelectionChanged(object? sender, RefreshArgs e)
	{
		if (e.RefreshType != RefreshType.Close)
		{
			if (DataContext is not null)
			{
				ViewModel.RibbonViewModel?.RefreshActions(e);
			}
			SelectionChanged?.Invoke(this, e);
		}
	}

	void NetworkViewModel_FocusedEntityChanged(object? sender, FocusedEntityChangedEventArgs e)
	{
		if (e.FocusedEntity != null)
		{
			ScrollToEntity(e.FocusedEntity);
		}
	}

	async Task ControlBar_BeforeZoomChanged()
	{
		if (ViewModel is not null)
		{
			await InvokeWinzorDispatcherAsync(() =>
			{
				SavePrevZoom();
			});
		}
	}

	async Task ControlBar_ZoomChanged()
	{
		if (ViewModel is not null)
		{
			await InvokeWinzorDispatcherAsync(() =>
			{
				ViewModel.ContentScale = zoomPercent / 100f;
			});
		}
	}

	async Task BeginSplitterMoveAsync()
	{
		await (GetJSInterop<ISplitterJSInterop>()?.MoveSplitterAsync(splitterReference, true, (int)Constants.NonScheduledSectionMinWidth, (int)Constants.SectionBaseMaxWidthBuffer) ?? Task.CompletedTask);
	}

	async Task HandleSplitterMoveAsync(SplitterMovedEventArgs args)
	{
		await InvokeWinzorDispatcherAsync(() =>
		{
			SplitMove(args.xOffset, args.yOffset);
		});
	}

	void SplitMove(int xOffset, int yOffset)
	{
		var newWidth = ViewModel.DiagramEntity.NonScheduledSectionWidth - xOffset;
		var nonScheduledSectionMinWidth = Constants.NonScheduledSectionMinWidth;
		var nonScheduledSectionMaxWidth = Width - Constants.SectionBaseMaxWidthBuffer;

		newWidth = Math.Clamp(newWidth, nonScheduledSectionMinWidth, nonScheduledSectionMaxWidth);

		ViewModel.DiagramEntity.NonScheduledSectionWidth = newWidth;
		nonScheduledSectionWidth = ViewModel.DiagramEntity.NonScheduledSectionWidth;

		NotifyRenderRequired();
	}

	void OnEntitiesMovedToDiagramSection(INetworkEntity[] entities)
	{
		var areEntitiesMovingToNonScheduledSection = entities.First().IsNonScheduled;
		var destinationDiagramControl = areEntitiesMovingToNonScheduledSection ? NonScheduledDiagramControl : MainDiagramControl;

		foreach (var entity in entities)
		{
			destinationDiagramControl?.OnEntityMovedToDiagramSection(entity);
		}
	}

	void DisplayResourceDependencyLink(IEnumerable<INetworkEntity> entities)
	{
		var fromEntity = entities.First();
		var toEntity = entities.Last();

		var sourceNode = ViewModel.NetworkViewModel.Nodes.FirstOrDefault(n => n.Entity.IsSameEntity(fromEntity));
		var destNode = ViewModel.NetworkViewModel.Nodes.FirstOrDefault(n => n.Entity.IsSameEntity(toEntity));

		if (sourceNode != null && destNode != null)
		{
			var network = sourceNode.Network;

			var sourceEntity = sourceNode.Entity;
			var destEntity = destNode.Entity;
			if (sourceEntity != null && destEntity != null)
			{
				var diagramRelationship = network.GetRelationship(sourceEntity, destEntity);
				if (diagramRelationship != null)
				{
					var connection = new ConnectionViewModel
					{
						Relationship = diagramRelationship,
						SourceConnector = sourceNode.OutputConnectors[0],
						DestConnector = destNode.InputConnectors[0],
					};

					ViewModel.NetworkViewModel.AddConnection(connection);
				}
			}
		}
	}

	void OnWheel(ThrottledWheelEventArgs args)
	{
		Wheel?.Invoke(args);
	}

	void SetContentScale(double scale)
	{
		var newScale = Math.Clamp(scale, min: 0.1, max: 10);
		if (ViewModel.ContentScale != newScale)
		{
			SavePrevZoom();
			ViewModel.ContentScale = newScale;
		}
	}

	public void ToggleFreezeChannelHeaders()
	{
		DoOnDiagramControls(control => control.ToggleFreezeChannelHeaders());
	}

	internal bool IsChannelHeadersFrozen()
	{
		return DiagramAreaControls.Any(control => control.IsChannelHeadersFrozen);
	}

	public void ToggleFreezeTimeLabels()
	{
		DoOnDiagramControls(control => control.ToggleFreezeTimeLabels());
	}

	internal bool IsTimeLabelsFrozen()
	{
		return DiagramAreaControls.Any(control => control.IsTimeLabelsFrozen);
	}

	internal bool HasChannels()
	{
		return DiagramAreaControls.Any(control => control.HasChannels);
	}
	internal bool IsDiagramScaled()
	{
		return DiagramAreaControls.Any(control => control.IsDiagramScaled);
	}
}
