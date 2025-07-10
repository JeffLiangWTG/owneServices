#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Blazor.Diagrams.Core.Models.Base;
using Blazor.Diagrams.Extensions;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.GUI.Controls.NodeResizing;
using CargoWise.NetworkVisualisation.GUI.Events.Scroll;
using CargoWise.NetworkVisualisation.GUI.Extensions;
using CargoWise.NetworkVisualisation.GUI.JSInterop;
using CargoWise.NetworkVisualisation.GUI.Models;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using WinzorFramework;
using WinzorFramework.Extensions;
using Point = Blazor.Diagrams.Core.Geometry.Point;
using Rectangle = Blazor.Diagrams.Core.Geometry.Rectangle;
using Size = Blazor.Diagrams.Core.Geometry.Size;

namespace CargoWise.NetworkVisualisation.GUI;

public interface IDiagramAreaUserControl
{
	Task ExecuteContextMenuActionAsync(INetworkAction action, WebMouseEventArgs args);
}

public interface IGlobalEventProvider
{
	Task RegisterGlobalPointerEvents(Func<PointerEventArgs, Task> onPointerMove, Func<PointerEventArgs, Task> onPointerUp);
}

public interface IDiagramChannelsControl
{
	IEnumerable<IDiagramChannel> Channels { get; }
}

[SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Referenced in DiagramAreaUserControl.razor")]
[SuppressMessage("CodeQuality", "IDE0052:Remove unread private member", Justification = "Referenced in DiagramAreaUserControl.razor")]
/// <summary>
/// Represents the control for creating a Diagram Area Canvas. Used to create both Schedule and Non-Schedule Diagrams.
/// </summary>
public partial class DiagramAreaUserControl : UserControl, IDiagramAreaUserControl, IGlobalEventProvider, IDiagramChannelsControl
{
	readonly NetworkUserControl parentNetworkControl;
	readonly NodeResizeControlFactory nodeResizeControlFactory;
	internal NCNDiagramModel? DiagramModel { get; private set; }

	public override bool UseParentDivForLayout => false;

	public DiagramAreaUserControl(NetworkUserControl parentNetworkControl, bool isNonScheduled)
	{
		IsNonScheduled = isNonScheduled;
		this.parentNetworkControl = parentNetworkControl;
		this.parentNetworkControl.Wheel += OnWheelAsync;
		nodeResizeControlFactory = new NodeResizeControlFactory(this);
	}

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (firstRender)
		{
			await ScrollToAsync(scrollLeft, scrollTop);

			var scrollSize = await GetScrollWidthAndHeight();
			if (scrollSize != null && scrollSize.Length > 1)
			{
				scrollWidth = scrollSize[0];
				scrollHeight = scrollSize[1];
			}

			await InvokeWinzorDispatcherAsync(() =>
			{
				RefreshColumns();
			});
		}

		if (JSRuntime != null)
		{
			boundingClientRect = await JSRuntime.GetBoundingClientRect(ElementReference);
		}

		await base.OnAfterRenderAsync(firstRender);
	}

	internal void SetDataContext(DiagramAreaUserControlViewModel viewModel)
	{
		if (DataContext is not null)
		{
			viewModel.ContentViewportWidth = ViewModel.ContentViewportWidth;
			ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
		}

		DataContext = viewModel;
		ViewModel.PropertyChanged += ViewModel_PropertyChanged;

		AdjustViewWidthAndHeight();
		UpdateProperties();
		UpdateDiagramModel();
		RefreshScale();
		NotifyRenderRequired();
	}

	void UpdateDiagramModel()
	{
		if (DiagramModel is null)
		{
			DiagramModel = new NCNDiagramModel(parentNetworkControl, nodeResizeControlFactory);
			DiagramModel.SetContainer(Rectangle.Zero);
			DiagramModel.PointerDown += async (_, _) => await DiagramArea_OnPointerDownAsync();
			DiagramModel.ContainerChanged += OnContainerChanged;
			DiagramModel.SelectionChanged += OnSelectionChanged;
		}

		DiagramModel.UpdateSources(Nodes, Connections);
	}

	void UpdateProperties()
	{
		diagramName = ViewModel.NetworkControlViewModel.DiagramName;
		jobName = ViewModel.NetworkControlViewModel.JobName;
		jobNameReadOnly = !ViewModel.NetworkControlViewModel.JobNameEnabled;
		jobNumber = ViewModel.NetworkControlViewModel.JobNumber;
		completionCriteria = ViewModel.NetworkControlViewModel.DiagramEntityViewModel.CompletionCriteria;
		completionCriteriaPlaceholder = ViewModel.NetworkControlViewModel.DiagramEntityViewModel.CompletionCriteriaPlaceholder;
		supportsDiagramVisualStyles = ViewModel.NetworkControlViewModel.SupportsDiagramVisualStyles;
		diagramBackgroundColor = ViewModel.NetworkControlViewModel.DiagramBackgroundColor;
		diagramForegroundColor = ViewModel.NetworkControlViewModel.DiagramForegroundColor;
		contentWidth = ViewModel.ContentWidth;
		contentHeight = ViewModel.ContentHeight;
		contentZoom = ViewModel.NetworkControlViewModel.ContentScale;
		contentLeftMargin = ViewModel.ContentLeftMargin;
		scaleUnitPixelSize = ViewModel.NetworkControlViewModel.DiagramEntity.ScaleUnitPixelSize;
		isDiagramScaled = ViewModel.NetworkControlViewModel.DiagramEntity.IsDiagramScaled;
		scrollLeft = ContentOffsetX;
		scrollTop = ContentOffsetY;
		scaleDescriptor = ViewModel.NetworkControlViewModel.ScaleDescriptor;
		startDate = ViewModel.NetworkControlViewModel.DiagramEntityViewModel.StartDateReadableText;
		finishDate = ViewModel.NetworkControlViewModel.DiagramEntityViewModel.FinishDateReadableText;
	}

	async void OnSelectionChanged(SelectableModel nodeModel)
	{
		if (nodeModel is null || !nodeModel.Selected || nodeModel is not NetworkNodeModel || nodeModel is NetworkNodeModel { IsSettingSelectionOnClient: true })
		{
			return;
		}

		await InvokeWinzorDispatcherAsync(() =>
		{
			if (parentNetworkControl.ViewModel.DiagramEntity.ShouldShowNonScheduledSection)
			{
				if (ViewModel.IsNonScheduled)
				{
					foreach (var node in NetworkViewModel.ScheduledNodes)
					{
						node.IsSelected = false;
					}
				}
				else
				{
					foreach (var node in NetworkViewModel.NonScheduledNodes)
					{
						node.IsSelected = false;
					}
				}
			}
		});
	}

	async void OnContainerChanged()
	{
		if (DataContext is not null)
		{
			await InvokeWinzorDispatcherAsync(() =>
			{
				if (DiagramModel == null || DiagramModel.Container == null)
				{
					return;
				}

				var container = DiagramModel.Container;
				ViewModel.ContentViewportWidth = container.Width;
				ViewModel.ContentViewportHeight = container.Height;
				RefreshColumns();
			});
		}
	}

	async void OnWheelAsync(ThrottledWheelEventArgs args)
	{
		if (!args.CtrlKey)
		{
			return;
		}

		const double scaleFactor = 0.05;
		await InvokeWinzorDispatcherAsync(() =>
		{
			if (args.DeltaY > 0)
			{
				parentNetworkControl.ZoomOut(scaleFactor);
			}
			else
			{
				parentNetworkControl.ZoomIn(scaleFactor);
			}
		});
	}

	async Task OnScrollAsync(ThrottledScrollEventArgs args)
	{
		var deltaX = scrollLeft - args.ScrollLeft;
		var deltaY = scrollTop - args.ScrollTop;
		await UpdateViewModelPositionAsync(args.ScrollLeft, args.ScrollTop);
		DiagramModel?.UpdatePan(deltaX, deltaY);
	}

	public async Task ScrollToMaintainView()
	{
		var newScrollSize = await JSRuntime!.InvokeAsync<double[]>("getScrollWidthAndHeight", new object[1] { ElementReference });
		if (newScrollSize != null && newScrollSize.Length > 1)
		{
			var deltaSizeRatioNeededForScroll = 0.5;
			var deltaWidth = newScrollSize[0] - scrollWidth;
			var deltaHeight = newScrollSize[1] - scrollHeight;
			scrollLeft = double.Max(ContentOffsetX + (deltaWidth * deltaSizeRatioNeededForScroll), 0);
			scrollTop = double.Max(ContentOffsetY + (deltaHeight * deltaSizeRatioNeededForScroll), 0);
			scrollWidth = newScrollSize[0];
			scrollHeight = newScrollSize[1];

			await UpdateViewModelPositionAsync(scrollLeft, scrollTop);
			await ScrollToAsync(scrollLeft, scrollTop);
		}
	}

	void OnKeyUp(WebKeyboardEventArgs e)
	{
		if (e.Key == "Shift")
		{
			DiagramModel?.OnKeyUp();
		}
	}

	internal ElementReference diagramCanvasRef;

	internal double ContentOffsetX
	{
		get => ViewModel.NetworkControlViewModel.ContentOffsetX;
		private set
		{
			if (ViewModel.NetworkControlViewModel.ContentOffsetX != value)
			{
				ViewModel.NetworkControlViewModel.ContentOffsetX = value;
			}
		}
	}

	internal double ContentOffsetY
	{
		get => ViewModel.NetworkControlViewModel.ContentOffsetY;
		private set
		{
			if (ViewModel.NetworkControlViewModel.ContentOffsetY != value)
			{
				ViewModel.NetworkControlViewModel.ContentOffsetY = value;
			}
		}
	}

	async Task UpdateViewModelPositionAsync(double scrollLeft, double scrollTop)
	{
		await InvokeWinzorDispatcherAsync(() =>
		{
			ContentOffsetX = scrollLeft;
			ContentOffsetY = scrollTop;
		});
	}

	bool UpdateScrollPosition()
	{
		var positionHasChanged = scrollLeft != ContentOffsetX || scrollTop != ContentOffsetY;

		scrollLeft = ContentOffsetX;
		scrollTop = ContentOffsetY;

		return positionHasChanged;
	}

	bool OnContentOffsetChange()
	{
		ViewModel.ResetLastCreatedNodeLocation();
		return UpdateScrollPosition();
	}

	double leftMarginForBackgrounds;
	readonly List<string> backgroundStyleStrings = new();

	internal void PopulateBackgrounds()
	{
		PopulateBackgroundsIfNeeded();
	}

	internal void PopulateBackgroundsIfNeeded()
	{
		var diagramEntity = (DataContext as DiagramAreaUserControlViewModel)?.NetworkControlViewModel?.DiagramEntity;
		if (diagramEntity == null)
		{
			return;
		}

		if (scaleDescriptor != null)
		{
			scaleDescriptor.ClearCachedData();
		}

		if (numberOfColumnsRequired > 0)
		{
			InitScaleSet(Ceiling(numberOfColumnsRequired, numberOfMergedColumns));
		}

		leftMarginForBackgrounds = hasChannels ? columnWidth * contentZoom : 0;
		PopulateBackgrounds(columnWidth, numberOfColumnsRequired);
	}

	void PopulateBackgrounds(double columnWidth, int lastColumn)
	{
		var tempBackgroundStyleStrings = new List<string>();
		var backgrounds = scaleSet?.BackgroundPoints;

		if (backgrounds == null)
		{
			return;
		}

		foreach (var background in backgrounds)
		{
			var backgroundWidth = columnWidth * (background.EndColumn - background.StartColumn);
			backgroundWidth *= contentZoom;

			var backgroundLeft = columnWidth * background.StartColumn;
			backgroundLeft *= contentZoom;

			tempBackgroundStyleStrings.Add($"width: {backgroundWidth}px; left: {backgroundLeft}px; background-color: {background.Color.GetColorStyleValue()}");
		}

		if (!backgroundStyleStrings.SequenceEqual(tempBackgroundStyleStrings))
		{
			backgroundStyleStrings.Clear();
			backgroundStyleStrings.AddRange(tempBackgroundStyleStrings);
		}
	}

	public void SelectAndScrollToEntity(INetworkEntity entity, bool animated = false)
	{
		SelectEntity(entity);
		ScrollToEntity(entity, animated);
	}

	void SelectEntity(INetworkEntity entity)
	{
		parentNetworkControl.NetworkViewModel.SelectSingleEntity(entity);
	}

	public void ScrollToEntity(INetworkEntity entity, bool animated = false)
	{
		var x = entity.X;
		var y = entity.Y;
		RegisterAfterRenderAction(() => ScrollToAsync(x * contentZoom, y * contentZoom, animated));
		NotifyRenderRequired();
	}

	public object? DataContext { get; set; }

	public DiagramAreaUserControlViewModel ViewModel => DataContext as DiagramAreaUserControlViewModel ?? throw new InvalidOperationException("null DataContext cannot be used as a ViewModel");

	public NetworkViewModel NetworkViewModel => ViewModel.NetworkControlViewModel.NetworkViewModel;

	public bool IsNonScheduled { get; init; }

	public ObservableCollection<NodeViewModel> Nodes => IsNonScheduled ? NetworkViewModel.NonScheduledNodes : NetworkViewModel.ScheduledNodes;

	ObservableCollection<ConnectionViewModel> Connections => IsNonScheduled ? NetworkViewModel.NonScheduledConnections : NetworkViewModel.ScheduledConnections;

	IEnumerable<NodeViewModel> SelectedNodes => NetworkViewModel.SelectedNodes;

	Rectangle? boundingClientRect;

	public void ExecuteContextMenuAction(INetworkAction action)
	{
	}

	public object? FindName(string name)
	{
		return null;
	}

	public void ExecuteRibbonAction(INetworkAction action)
	{
		ExecuteGenericNonPositionalAction(action,
			locationGetter: () =>
			{
				var lastNodeLocation = new Location(ViewModel.LastCreatedNodeLocationRelativeToViewport.X, ViewModel.LastCreatedNodeLocationRelativeToViewport.Y);
				var viewport = new ViewportRect(ContentOffsetX / contentZoom, ContentOffsetY / contentZoom, ViewModel.ContentViewportWidth, ViewModel.ContentViewportHeight);
				return GetPositionForNewEntities(lastNodeLocation, viewport, NetworkViewModel);
			},
			executeForRibbon: true);
	}

	double GetStaggeringXOffset(INetworkViewModel networkViewModel) => networkViewModel.Network.DiagramEntity.IsDiagramScaled ? networkViewModel.Network.DiagramEntity.ScaleUnitPixelSize : 30;
	double GetStaggeringYOffset() => 25;

	public Location GetPositionForNewEntities(Location lastCreatedNodeLocationRelativeToViewport, ViewportRect viewport, INetworkViewModel networkViewModel)
	{
		var newNodeLocationRelativeToViewport = new Location(lastCreatedNodeLocationRelativeToViewport.X + GetStaggeringXOffset(networkViewModel), lastCreatedNodeLocationRelativeToViewport.Y + GetStaggeringYOffset());

		var newNodeAbsLocation = new Location(viewport.X + newNodeLocationRelativeToViewport.X, viewport.Y + newNodeLocationRelativeToViewport.Y);
		return newNodeAbsLocation;
	}
	async Task OpenContextMenuAsync(WebMouseEventArgs args)
	{
#pragma warning disable SA1011
		NetworkActionMenuItem[]? items = null;
#pragma warning restore SA1011
		await SafeClipboard.FetchClipboardDataAsync(JSRuntime);
		await InvokeWinzorDispatcherAsync(() =>
		{
			parentNetworkControl.ViewModel.ReloadMenuItems();
			items = parentNetworkControl.ViewModel.MenuItems.ToArray();
		});

		await (FindForm(throwIfNotOnForm: true).CargoWiseClientServices?.MenuDisplayer.ShowContextMenuAsync(args, items, ExecuteContextMenuActionAsync) ?? Task.CompletedTask);
	}

	public async Task ExecuteContextMenuActionAsync(INetworkAction? action, WebMouseEventArgs args)
	{
		await InvokeWinzorDispatcherAsync(() => ExecuteContextMenuAction(action, args));
	}

	public void ExecuteContextMenuAction(INetworkAction? action, WebMouseEventArgs args)
	{
		if (DiagramModel is null)
		{
			throw new InvalidOperationException("Cannot perform action without a data context");
		}

		if (DiagramModel.Container is null)
		{
			var ex = new Exception("Container can not be null");
			var message = $"Action Name: {action!.GetName()} | Network Active Entity: {NetworkViewModel.ActiveEntity} | Diagram Name: {diagramName} | IsNonScheduled: {IsNonScheduled}";
			ExceptionReporter.Instance.ReportDeveloperException((NoResString)"Failed to Execute Context Menu on NCN", message, ex);
			throw ex;
		}

		var mousePositionInDiagram = DiagramModel.GetRelativeMousePoint(args.ClientX, args.ClientY);

		if (action is PositionalNetworkAction positionAction)
		{
			ExecuteGenericPositionalAction(positionAction, new Location(mousePositionInDiagram.X, mousePositionInDiagram.Y));
		}
		else if (action != null)
		{
			ExecuteGenericNonPositionalAction(action, () => new Location(mousePositionInDiagram.X, mousePositionInDiagram.Y), executeForRibbon: false);
		}
	}

	void ExecuteGenericPositionalAction(PositionalNetworkAction action, Location location)
	{
		action.Execute(location);
	}

	void ExecuteGenericNonPositionalAction(INetworkAction action, Func<Location> locationGetter, bool executeForRibbon)
	{
		INetworkActionResult result;

		using (NetworkViewModel.Network.SuspendRefreshingOnEntityCountChanged())
		{
			result = action.Execute();
		}

		if (result != null && result.IsHandledByVisualiser)
		{
			var entity = result as INetworkEntity;
			var collection = result as INetworkEntityCollection;
			var results = Array.Empty<INetworkEntity>();

			if (entity != null)
			{
				if (IsNonScheduled)
				{
					entity.IsNonScheduled = true;
				}

				ViewModel.CreateNewNode(locationGetter(), entity, ViewModel.RememberLastCreatedNodeLocationRelativeToZoom, disableCentering: executeForRibbon);
				results = new[] { entity };
			}
			else if (collection != null)
			{
				ViewModel.CreateNewNodes(collection.Entities, locationGetter());
				results = collection.Entities.ToArray();
			}

			NetworkViewModel.SelectEntities(results);
			NetworkViewModel.Network.Refresh(RefreshType.EntitiesReloaded, results);
			AdjustViewWidthAndHeight();
		}
	}

	public void AdjustViewWidthAndHeight()
	{
		var viewModel = ViewModel;

		if (viewModel.NetworkControlViewModel != null)
		{
			var cornerRadius = viewModel.NetworkControlViewModel.DiagramCornerRadius;

			var diagramEntity = viewModel.NetworkControlViewModel.DiagramEntity;

			var isDiagramSurfaceFixed = diagramEntity.IsDiagramSurfaceFixed;

			if (AdjustEntitiesLocationToChannelBounds(diagramEntity) || (isDiagramSurfaceFixed && AdjustEntitiesLocationToDiagramBounds(diagramEntity)))
			{
				parentNetworkControl.ReloadNetwork();
			}

			var nodes = NetworkViewModel.GetAppropriateNodeCollection(IsNonScheduled);
			var widthForContent = nodes.Select(a => a.X >= 0 ? a.X + a.Width : 0).Append(0).Max();
			var widthForChannelHeaders = diagramEntity.DiagramChannels.Any() ? diagramEntity.ScaleUnitPixelSize : 0;

			viewModel.ContentLeftMargin = widthForChannelHeaders;

			viewModel.ContentWidth = widthForContent + widthForChannelHeaders;

			if (isDiagramSurfaceFixed && !diagramEntity.IsDeleted)
			{
				var diagramWidth = diagramEntity.Width + widthForChannelHeaders;

				if (viewModel.ContentWidth != diagramWidth)
				{
					viewModel.ContentWidth = diagramWidth;
				}
			}

			parentNetworkControl.AdjustDiagramControlHeights();
		}
	}

	internal double GetHeightForContent()
	{
		var diagramEntity = ViewModel.NetworkControlViewModel.DiagramEntity;
		var cornerRadius = ViewModel.NetworkControlViewModel.DiagramCornerRadius;
		var heightForChannels = NetworkViewModel.GetChannelYAxisRanges().LastOrDefault()?.Item2.Maximum ?? 0;

		double heightForContent;

		if (diagramEntity.IsDiagramSurfaceFixed && diagramEntity.Height > 0d)
		{
			heightForContent = diagramEntity.Height;
		}
		else
		{
			var nodes = NetworkViewModel.GetAppropriateNodeCollection(IsNonScheduled);
			heightForContent = nodes.Select(a => a.Y > 0 ? a.Y + a.Height : 0).Append(0).Max();
		}

		return Math.Max(heightForContent, heightForChannels);
	}

	internal void AdjustContentHeight(double height)
	{
		if (DataContext != null)
		{
			ViewModel.ContentHeight = height;
		}
	}

	async Task ScrollToAsync(double x, double y, bool animated = false)
	{
		if (JSRuntime is null)
		{
			return;
		}

		await ElementReference.ScrollToAsync(JSRuntime, x, y, animated);
	}

	public async Task<double[]> GetScrollWidthAndHeight()
	{
		return await JSRuntime!.InvokeAsync<double[]>("getScrollWidthAndHeight", new object[1] { ElementReference });
	}

	static bool AdjustEntitiesLocationToDiagramBounds(IDiagramEntity diagramEntity, bool useOffset = false)
	{
		var lastShapes = diagramEntity.Children.OrderByDescending(node => node.X).Where(shape => shape.X + shape.Width > diagramEntity.Width);
		var result = false;

		foreach (var shape in lastShapes)
		{
			result |= shape.AdjustEntityLocationToDiagramBounds(diagramEntity, useOffset);

			if (shape.Children.Any())
			{
				AdjustEntitiesLocationToDiagramBounds((IDiagramEntity)shape, true);
			}
		}
		return result;
	}

	bool AdjustEntitiesLocationToChannelBounds(IDiagramEntity diagramEntity)
	{
		if (!diagramEntity.DiagramChannels.Any())
		{
			return false;
		}

		var shapes = diagramEntity.Children.OrderByDescending(node => node.X);
		var result = false;

		foreach (var shape in shapes)
		{
			if (shape.AdjustLocationToChannels(diagramEntity, NetworkViewModel))
			{
				result = true;
			}
		}

		return result;
	}

	internal bool RefreshScale()
	{
		RefreshColumns();
		return true;
	}

	void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		var changed = e.PropertyName switch
		{
			nameof(DiagramAreaUserControlViewModel.ContentWidth) => UpdateProperty(ref contentWidth, ViewModel.ContentWidth) && RefreshScale(),
			nameof(DiagramAreaUserControlViewModel.ContentHeight) => UpdateProperty(ref contentHeight, ViewModel.ContentHeight),
			nameof(DiagramAreaUserControlViewModel.ContentLeftMargin) => UpdateProperty(ref contentLeftMargin, ViewModel.ContentLeftMargin),
			_ => false,
		};
	}

	internal void NetworkViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		var changed = e.PropertyName switch
		{
			nameof(NetworkUserControlViewModel.ContentScale) => UpdateProperty(ref contentZoom, ViewModel.NetworkControlViewModel.ContentScale) && RefreshScale(),
			nameof(NetworkUserControlViewModel.ContentOffsetX) => OnContentOffsetChange(),
			nameof(NetworkUserControlViewModel.ContentOffsetY) => OnContentOffsetChange(),
			_ => false,
		};
	}

	public Task InvokeWinzorDispatcherAsync(Action action) => base.InvokeWinzorDispatcherAsync(action);
	string DiagramAreaUserControlStyleString => @$"--diagram-background-color: {diagramBackgroundColor.GetColorStyleValue()};
--diagram-foreground-color: {diagramForegroundColor.GetColorStyleValue()};
--diagram-content-min-width: {contentWidth * contentZoom}px;
--diagram-content-min-height: {(contentHeight * contentZoom) + DESIREDPADDING / 2}px;
--header-column-min-width: {columnWidth * contentZoom}px;
--diagram-scale: {contentZoom};";
	string JobNumberStyleString => jobName?.Length > 0 ? (NoResString)"font-weight: 700;" : string.Empty;

	public MultilingualString NonScheduledDiagramName => ResString.GetMultilingualString("7AB6282B-808E-4933-9852-E2B4D478D8DF", "Non-Scheduled Items");

	readonly string jobNamePlaceholder = ResString.GetMultilingualString("0BE310ED-2698-4F9B-845C-E5B95EDFA07A", "Job Name");

	async Task Diagram_DiagramNameChangedAsync() => await InvokeWinzorDispatcherAsync(() => ViewModel.NetworkControlViewModel.DiagramName = diagramName);
	async Task Diagram_JobNameChangedAsync() => await InvokeWinzorDispatcherAsync(() => ViewModel.NetworkControlViewModel.JobName = jobName);
	async Task Diagram_CompletionCriteriaChangedAsync() => await InvokeWinzorDispatcherAsync(() => ViewModel.NetworkControlViewModel.DiagramEntityViewModel.CompletionCriteria = completionCriteria);

	string? diagramName;
	string? jobName;
	bool jobNameReadOnly;
	string? jobNumber;
	string? completionCriteria;
	string? completionCriteriaPlaceholder;
	string? startDate;
	string? finishDate;
	bool supportsDiagramVisualStyles;
	Color diagramBackgroundColor;
	Color diagramForegroundColor;
	double contentWidth;
	double contentHeight;
	double contentZoom;
	int scaleUnitPixelSize;
	bool isDiagramScaled;
	double scrollTop;
	double scrollLeft;
	double scrollWidth;
	double scrollHeight;

	const double DESIREDPADDING = 50;
	Rectangle GetBounds(IEnumerable<NodeViewModel> targets)
	{
		if (!targets.Any())
		{
			return Rectangle.Zero;
		}

		double left = double.MaxValue;
		double right = double.MinValue;
		double top = double.MaxValue;
		double bottom = double.MinValue;

		foreach (NodeViewModel node in targets)
		{
			double width = node.X + node.Width;
			double height = node.Y + node.Height;

			left = Math.Min(left, node.X);
			right = Math.Max(right, width);
			top = Math.Min(top, node.Y);
			bottom = Math.Max(bottom, height);
		}

		return new Rectangle(left, top, right, bottom);
	}

	/// <summary>
	/// Update diagram pan and zoom so that selected nodes (or all nodes if none selected) are within the users view. 
	/// </summary>
	public void FitNodes()
	{
		var nodesToFit = SelectedNodes.Any() ? SelectedNodes : Nodes;

		FitIntoDiagram(nodesToFit);
	}

	/// <summary>
	/// Update diagram pan and zoom so that all nodes are within the users view.
	/// </summary>
	public void FillNodes()
	{
		FitIntoDiagram(Nodes);
	}

	void FitIntoDiagram(IEnumerable<NodeViewModel> nodes)
	{
		if (!nodes.Any() || boundingClientRect is null)
		{
			return;
		}

		var targetBounds = GetBounds(nodes);
		var desiredZoom = CalculateDesiredZoom(targetBounds);
		var desiredPanPosition = CalculateDesiredScrollPosition(targetBounds);

		parentNetworkControl.ViewModel.ContentScale = desiredZoom;
		RegisterAfterRenderAction(() => _ = ScrollToAsync(desiredZoom * desiredPanPosition.X, desiredZoom * desiredPanPosition.Y, animated: true));
	}

	Point CalculateDesiredScrollPosition(Rectangle contentBounds)
	{
		return new Point(contentBounds.Left - DESIREDPADDING, contentBounds.Top - DESIREDPADDING);
	}

	double CalculateDesiredZoom(Rectangle contentBounds)
	{
		if (boundingClientRect is null)
		{
			return parentNetworkControl.ViewModel.ContentScale;
		}

		var spaceForContent = new Size(boundingClientRect.Width, boundingClientRect.Height);
		var idealViewportSize = new Size(contentBounds.Width + (DESIREDPADDING * 2), contentBounds.Height + (DESIREDPADDING * 2));
		return Math.Min(spaceForContent.Width / idealViewportSize.Width, spaceForContent.Height / idealViewportSize.Height);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && DataContext is not null)
		{
			ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
			DiagramModel?.Dispose();
			DataContext = null;
		}
		base.Dispose(disposing);
	}

	void InitScaleSet(int columns)
	{
		scaleSet = scaleDescriptor?.GetScaleSetForColumns(columns);
	}

	bool hasChannels;
	double columnWidth;
	int numberOfColumnsRequired;
	double lastColumnWidth;
	int numberOfMergedColumns;
	double contentLeftMargin;
	int numberOfChannels;
	IReadOnlyList<INetworkScalePoint>? scalePoints;
	ICollection<Tuple<IDiagramChannel, double>>? channelAndHeightTuples;
	int presentColumnIndex;
	double scaleRatio = 1;
	INetworkScaleDescriptor? scaleDescriptor;
	INetworkScaleSet? scaleSet;
	bool isChannelHeadersFrozen;
	bool isTimeLabelsFrozen;

	public bool IsChannelHeadersFrozen => isChannelHeadersFrozen;
	public bool IsTimeLabelsFrozen => scaleRatio == 1 && isTimeLabelsFrozen;
	public bool IsDiagramScaled => isDiagramScaled;

	public bool HasChannels => hasChannels;

	public IEnumerable<IDiagramChannel> Channels => channelAndHeightTuples?.Select(t => t.Item1) ?? Enumerable.Empty<IDiagramChannel>();

	void RefreshColumns()
	{
		if (ViewModel is null || !isDiagramScaled)
		{
			return;
		}

		scaleRatio = ViewModel.NetworkControlViewModel.ContentScale;
		var scaleType = GetScaleType(scaleRatio);
		numberOfMergedColumns = GetNumberOfMergedColumns(scaleType);

		var channelAndRangeTuples = ViewModel.NetworkControlViewModel.NetworkViewModel.GetChannelYAxisRanges().ToArray();
		hasChannels = channelAndRangeTuples.Any();
		var currentWidth = Math.Max(contentWidth, ViewModel.ContentViewportWidth) / scaleRatio;

		columnWidth = scaleUnitPixelSize;
		var channelHeadersWidth = hasChannels ? columnWidth : 0;

		numberOfColumnsRequired = (int)(currentWidth / columnWidth);
		lastColumnWidth = currentWidth % columnWidth;

		if (lastColumnWidth > 0)
		{
			numberOfColumnsRequired++;
		}

		if (hasChannels)
		{
			numberOfColumnsRequired++;
		}

		PopulateBackgroundsIfNeeded();

		if (hasChannels)
		{
			AddChannels(ViewModel, channelHeadersWidth, channelAndRangeTuples);
		}

		if (numberOfColumnsRequired > 0)
		{
			if (scaleSet != null)
			{
				scalePoints = scaleSet.ScalePoints;
				presentColumnIndex = scaleSet.IndexOfColumnInPresent;
			}
		}

		DiagramModel?.Refresh();
		NetworkViewModel.Network.Refresh(RefreshType.ChannelView);
		NetworkViewModel.Network.Refresh(RefreshType.ToogleFreezeChannelHeadersAndTimeLabels);
	}

	static int Ceiling(int num, int multiple)
	{
		var mod = num % multiple;
		if (mod == 0)
		{
			return num;
		}
		else
		{
			return num - mod + multiple;
		}
	}

	static double GetFontSize(double scaleRatio, double font)
	{
		return font * 1 / scaleRatio;
	}

	void AddChannels(DiagramAreaUserControlViewModel viewModel, double channelHeadersWidth, Tuple<IDiagramChannel, Range<int>>[] channelAndRangeTuples)
	{
		if (channelAndHeightTuples is null)
		{
			channelAndHeightTuples = new Collection<Tuple<IDiagramChannel, double>>();
		}
		channelAndHeightTuples.Clear();

		for (var i = 0; i < channelAndRangeTuples.Length; i++)
		{
			var channel = channelAndRangeTuples[i].Item1;
			var range = channelAndRangeTuples[i].Item2;
			var height = (range.Maximum - range.Minimum) * contentZoom;
			channelAndHeightTuples.Add(new Tuple<IDiagramChannel, double>(channel, height));
		}

		numberOfChannels = channelAndHeightTuples.Count;
	}

	string GetWidthForDiagramNode()
	{
		if (isDiagramScaled && !IsNonScheduled && !string.IsNullOrEmpty(startDate) &&
			!string.IsNullOrEmpty(finishDate))
		{
			return $"width: {contentWidth * contentZoom}px";
		}
		return (NoResString)"min-width: 100%";
	}

	public void OnEntityMovedToDiagramSection(INetworkEntity entity)
	{
		var middleOfViewport = GetXCoordinateForMiddleOfViewport();
		entity.X = Math.Max(middleOfViewport - (entity.Width / 2), 0);

		var node = NetworkViewModel.GetNodeForEntity(entity);
		node.OnDragCompleted();
		node.OnResizeCompleted();
	}

	double GetXCoordinateForMiddleOfViewport()
	{
		return (ViewModel.ContentViewportWidth / 2) + ViewModel.ContentOffsetX;
	}

	string GenerateScheduleGridStyle()
	{
		if (numberOfColumnsRequired <= 0)
		{
			return string.Empty;
		}

		var actualNumberOfColumnsRequired = numberOfColumnsRequired;

		if (hasChannels)
		{
			actualNumberOfColumnsRequired--;
		}
		if (lastColumnWidth > 0)
		{
			actualNumberOfColumnsRequired--;
		}

		var scheduleGridStyle = string.Empty;

		scheduleGridStyle += $"grid-template-columns: {string.Concat(Enumerable.Repeat($"{columnWidth}px ", (actualNumberOfColumnsRequired / numberOfMergedColumns)))} {lastColumnWidth}px;";
		scheduleGridStyle += $"grid-column-gap: {(numberOfMergedColumns - 1) * columnWidth}px;";

		if (hasChannels)
		{
			scheduleGridStyle += $"padding-left: {(numberOfMergedColumns - 1) * columnWidth}px;";
		}
		return scheduleGridStyle;
	}

	string GenerateTimeScaleLabelStyle(int columnNumber)
	{
		var actualColumnNumber = columnNumber - (hasChannels ? 1 : 0);
		var labelIndex = actualColumnNumber + numberOfMergedColumns - 1;

		var timeScaleLabelStyle = string.Empty;

		timeScaleLabelStyle += $"left: {(columnWidth * actualColumnNumber) + 4}px;";
		timeScaleLabelStyle += $"width: {(columnWidth * numberOfMergedColumns) - 6}px;";
		timeScaleLabelStyle += $"font-size: {GetFontSize(scaleRatio, 12)}px;";
		timeScaleLabelStyle += $"height: {GetFontSize(scaleRatio, 12) * 1.2}px;";

		if (labelIndex == presentColumnIndex)
		{
			timeScaleLabelStyle += $"font-weight: bold;";
		}

		return timeScaleLabelStyle;
	}

	string? GetTimeScaleLabel(int columnNumber)
	{
		var actualColumnNumber = columnNumber - (hasChannels ? 1 : 0);
		var labelIndex = actualColumnNumber + numberOfMergedColumns - 1;

		return scalePoints?.ElementAtOrDefault(labelIndex)?.Label;
	}

	static ScaleType GetScaleType(double scale)
	{
		if (scale <= 0.25)
		{
			return ScaleType.Eighths;
		}
		else if (scale <= 0.5)
		{
			return ScaleType.Quarters;
		}
		else if (scale <= 0.75)
		{
			return ScaleType.Half;
		}
		else
		{
			return ScaleType.Normal;
		}
	}

	static int GetNumberOfMergedColumns(ScaleType scaleType)
	{
		switch (scaleType)
		{
			case ScaleType.Normal:
				return 1;
			case ScaleType.Half:
				return 2;
			case ScaleType.Quarters:
				return 4;
			case ScaleType.Eighths:
				return 8;
			default:
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Unrecognised [{0}].", scaleType));
		}
	}
	enum ScaleType
	{
		Normal = 0,
		Half,
		Quarters,
		Eighths,
	}

	public async Task RegisterGlobalPointerEvents(Func<PointerEventArgs, Task> onPointerMove, Func<PointerEventArgs, Task> onPointerUp)
	{
		await (GetJSInterop<IGlobalEventJSInterop>()?.SubscribeToPointerEventsOutsideElement(diagramCanvasRef, onPointerMove, onPointerUp) ?? Task.CompletedTask);
	}

	async Task DiagramArea_OnPointerDownAsync()
	{
		await RegisterGlobalPointerEvents(OnPointerMoveAsync, OnPointerUpAsync);
	}

	Task OnPointerMoveAsync(PointerEventArgs args)
	{
		DiagramModel?.TriggerPointerMove(null, args.ToCore());
		return Task.CompletedTask;
	}

	Task OnPointerUpAsync(PointerEventArgs args)
	{
		DiagramModel?.TriggerPointerUp(null, args.ToCore());
		return Task.CompletedTask;
	}

	void OnPointerDownOutsideDiagramCanvas() => DiagramModel?.UnselectAll();

	public void ToggleFreezeChannelHeaders()
	{
		isChannelHeadersFrozen = !isChannelHeadersFrozen;
		NotifyRenderRequired();
	}

	public void ToggleFreezeTimeLabels()
	{
		isTimeLabelsFrozen = !isTimeLabelsFrozen;
		NotifyRenderRequired();
	}

	internal void OnChannelSelected(IDiagramChannel channel)
	{
		isChannelHeadersFrozen = true;
		isTimeLabelsFrozen = true;

		double xCord = ContentOffsetX;
		double yCord = 100;

		var channelsAndHeights = channelAndHeightTuples?.ToList();
		for (int i = 0; i < channelsAndHeights!.Count; i++)
		{
			if (channelsAndHeights[i].Item1 == channel)
			{
				break;
			}
			yCord += channelsAndHeights[i].Item2;
		}

		InvokeRenderDispatcher(async () => await ScrollToAsync(xCord * contentZoom, yCord * contentZoom, false), true);
		NotifyRenderRequired();
	}
}
