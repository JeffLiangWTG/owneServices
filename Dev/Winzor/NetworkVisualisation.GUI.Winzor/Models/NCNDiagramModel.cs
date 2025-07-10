#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Blazor.Diagrams;
using Blazor.Diagrams.Core;
using Blazor.Diagrams.Core.Anchors;
using Blazor.Diagrams.Core.Behaviors;
using Blazor.Diagrams.Core.Controls;
using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Core.Models.Base;
using CargoWise.Common;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.GUI.Components;
using CargoWise.NetworkVisualisation.GUI.Controls;
using CargoWise.NetworkVisualisation.GUI.Controls.NodeResizing;
using CargoWise.NetworkVisualisation.GUI.Extensions;
using CargoWise.NetworkVisualisation.GUI.Services.Connection;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using NetworkVisualisation.GUI.Winzor.Models.Factories;
using static CargoWise.NetworkVisualisation.GUI.Models.NetworkPortModel;

namespace CargoWise.NetworkVisualisation.GUI.Models;

/// <summary>
/// Represents and manages what the Blazor.Diagrams `DiagramCanvas` displays by converting CargoWise diagram view models into Blazor.Diagrams models.
/// It sets up some of the UI features such as adding resizers to nodes, adding keyboard shortcuts and configuring the diagram behaviors.
/// </summary>
public class NCNDiagramModel : BlazorDiagram, IDisposable
{
	internal readonly Guid DiagramId = Guid.NewGuid();
	readonly INetworkUserControl networkUserControl;
	internal INetworkAction linkFromClipboardAction;
	internal bool isMovingNodesByShiftArrowKeys;

	ObservableCollection<NodeViewModel> nodesSource = new ();
	ObservableCollection<ConnectionViewModel> connectionsSource = new ();

	/// <summary>
	/// Updates the connections between nodes in the Diagram Canvas whenever the DiagramModel is updated.
	/// </summary>
	public NCNDiagramModel(
		ObservableCollection<NodeViewModel> nodesSource,
		ObservableCollection<ConnectionViewModel> connectionsSource,
		INetworkUserControl networkUserControl,
		NodeResizeControlFactory? nodeResizeControlFactory = null) : this(networkUserControl, nodeResizeControlFactory)
	{
		UpdateSources(nodesSource, connectionsSource);
	}

	/// <summary>
	/// Registers custom nodes with the diagram component and setups behaviours and event handlers for the DiagramCanvas.
	/// </summary>
	public NCNDiagramModel(INetworkUserControl networkUserControl, NodeResizeControlFactory? nodeResizeControlFactory = null)
	{
		this.networkUserControl = networkUserControl;
		NodeResizeControlFactory = nodeResizeControlFactory;

		Nodes.Added += AddResizersToNode;
		Links.Removed += OnLinkRemoved;
		ContainerChanged += OnContainerChanged;

		linkFromClipboardAction = new LinkFromClipboardAction(networkUserControl.NetworkViewModel);

		RegisterComponents();
		SetupDiagramOptions();
		SetupBehaviors();
		SelectionChanged += Diagram_SelectionChangedAsync;
		InitializeShortcuts();
		SetZoomFromViewModel();
	}

	public bool IsDisposed { get; private set; }
	public void Dispose()
	{
		if (IsDisposed)
		{
			return;
		}

		ContainerChanged -= OnContainerChanged;
		Links.Removed -= OnLinkRemoved;
		Nodes.Added -= AddResizersToNode;

		nodesSource.CollectionChanged -= NodesSource_CollectionChanged;
		connectionsSource.CollectionChanged -= ConnectionsSource_CollectionChanged;

		IsDisposed = true;
	}

	void OnContainerChanged()
	{
		SetPan(0, 0);
	}

	public void UpdateSources(ObservableCollection<NodeViewModel> nodes, ObservableCollection<ConnectionViewModel> connections)
	{
		SetNodesSource(nodes);
		SetConnectionsSource(connections);
		Reset();
	}

	void SetNodesSource(ObservableCollection<NodeViewModel> value)
	{
		if (nodesSource != null)
		{
			nodesSource.CollectionChanged -= NodesSource_CollectionChanged;
		}
		nodesSource = value;
		nodesSource.CollectionChanged += NodesSource_CollectionChanged;
	}

	void SetConnectionsSource(ObservableCollection<ConnectionViewModel> value)
	{
		if (connectionsSource != null)
		{
			connectionsSource.CollectionChanged -= ConnectionsSource_CollectionChanged;
		}
		connectionsSource = value;
		connectionsSource.CollectionChanged += ConnectionsSource_CollectionChanged;
	}

	void AddResizersToNode(NodeModel nodeModel)
	{
		if (nodeModel is NetworkNodeModel networkNodeModel)
		{
			networkNodeModel.TopLeftResizer = NodeResizeControlFactory?.CreateControl(new TopLeftResizerProvider());
			networkNodeModel.TopRightResizer = NodeResizeControlFactory?.CreateControl(new TopRightResizerProvider());
			networkNodeModel.BottomLeftResizer = NodeResizeControlFactory?.CreateControl(new BottomLeftResizerProvider());
			networkNodeModel.BottomRightResizer = NodeResizeControlFactory?.CreateControl(new BottomRightResizerProvider() { IsVisible = true });

			if (!networkNodeModel.Locked &&
				networkNodeModel.TopLeftResizer is not null &&
				networkNodeModel.TopRightResizer is not null &&
				networkNodeModel.BottomLeftResizer is not null &&
				networkNodeModel.BottomRightResizer is not null)
			{
				Controls.AddFor(networkNodeModel, ControlsType.AlwaysOn)
					.Add(networkNodeModel.TopLeftResizer)
					.Add(networkNodeModel.TopRightResizer)
					.Add(networkNodeModel.BottomLeftResizer)
					.Add(networkNodeModel.BottomRightResizer);
			}
		}
	}

	void RegisterComponents()
	{
		RegisterComponent<JobNodeModel, InDiagramNetworkNode<JobNodeModel, JobNode>>();
		RegisterComponent<AnnotationNodeModel, InDiagramNetworkNode<AnnotationNodeModel, AnnotationNode>>();
		RegisterComponent<BufferNodeModel, InDiagramNetworkNode<BufferNodeModel, BufferNode>>();
		RegisterComponent<NetworkLinkModel, NetworkLink>();
		RegisterComponent<NodeResizeControl, NodeResizeControlWidget>();
	}

	void SetupDiagramOptions()
	{
		Options.AllowPanning = false;
		Options.LinksLayerOrder = 100;
		Options.Links.Factory = Diagram_CreateLink;
		Options.Links.TargetAnchorFactory = Diagram_CreateAnchor;
		Options.Zoom.Enabled = false; // disable default zoom behaviour
	}

	void InitializeShortcuts()
	{
		var shortcuts = GetBehavior<KeyboardShortcutsBehavior>()!;
		shortcuts.RemoveShortcut((NoResString)"g", true, false, true);
		shortcuts.SetShortcut((NoResString)"c", true, false, false, async _ =>
		{
			await CopySelectionToClipboardAsync();
		});
		shortcuts.SetShortcut((NoResString)"v", true, false, false, async _ =>
		{
			await PasteFromClipboardAsync();
		});
		shortcuts.SetShortcut((NoResString)"Delete", false, false, false, async _ =>
		{
			await DeleteSelectionAsync();
		});
		shortcuts.SetShortcut((NoResString)"a", true, false, false, _ =>
		{
			SelectAll();
			return ValueTask.CompletedTask;
		});
		shortcuts.SetShortcut((NoResString)"i", true, false, false, async _ =>
		{
			await InvertSelectionAsync();
		});
		shortcuts.SetShortcut((NoResString)"Escape", false, false, false, _ =>
		{
			DeselectAll();
			return ValueTask.CompletedTask;
		});
		shortcuts.SetShortcut("ArrowUp", false, true, false, _ =>
		{
			MoveSelectedNode(0, -5);
			return ValueTask.CompletedTask;
		});
		shortcuts.SetShortcut("ArrowDown", false, true, false, _ =>
		{
			MoveSelectedNode(0, 5);
			return ValueTask.CompletedTask;
		});
		shortcuts.SetShortcut("ArrowLeft", false, true, false, _ =>
		{
			MoveSelectedNode(-5, 0);
			return ValueTask.CompletedTask;
		});
		shortcuts.SetShortcut("ArrowRight", false, true, false, _ =>
		{
			MoveSelectedNode(5, 0);
			return ValueTask.CompletedTask;
		});
	}

	internal void MoveSelectedNode(double deltaX, double deltaY)
	{
		var behavior = GetBehavior<DragNodesBehavior>();
		behavior?.MoveSelectedNodes(deltaX, deltaY);
		isMovingNodesByShiftArrowKeys = true;
	}

	public void OnKeyUp()
	{
		if (isMovingNodesByShiftArrowKeys)
		{
			var behavior = GetBehavior<DragNodesBehavior>();
			behavior?.UpdateMovedNodes();
			isMovingNodesByShiftArrowKeys = false;
		}
	}

	void SetupBehaviors()
	{
		UnregisterBehavior<DragMovablesBehavior>();
		RegisterBehavior(new DragNodesBehavior(this));
		UnregisterBehavior<SelectionBoxBehavior>();
		RegisterBehavior(new NCNSelectionBoxBehavior(this));
		BehaviorOptions.DiagramDragBehavior = GetBehavior<NCNSelectionBoxBehavior>();
		BehaviorOptions.DiagramWheelBehavior = null;
	}

	#region BlazorDiagrams Factory Implementations

	NetworkLinkModel Diagram_CreateLink(Diagram diagram, ILinkable source, Anchor targetAnchor)
	{
		if (source is NetworkPortModel port)
		{
			var anchor = new SinglePortAnchor(port) { MiddleIfNoMarker = true };
			var link = new NetworkLinkModel(anchor, targetAnchor);
			link.TargetAttached += Diagram_LinkAttachedAsync;
			inProgressLink = link;
			return link;
		}

		throw new NotImplementedException();
	}

	async void Diagram_LinkAttachedAsync(BaseLinkModel link)
	{
		link.TargetAttached -= Diagram_LinkAttachedAsync;

		if (link.Source.Model is NetworkPortModel { Type: PortType.Input })
		{
			// The link was drawn backwards, so we need to swap the Source and Target
			var oldSource = link.Source;
			var oldTarget = link.Target;
			link.SetTarget(oldSource);
			link.SetSource(oldTarget);
		}

		if (link.Source.Model is not PortModel { Parent: LinkableNodeModel sourceNode }
			|| link.Target.Model is not PortModel { Parent: LinkableNodeModel targetNode })
		{
			throw new NotImplementedException();
		}

		await AddConnectionToViewModelAsync(sourceNode.EntityPK, targetNode.EntityPK);
		inProgressLink = null;

		Links.Remove(link);
	}

	Anchor Diagram_CreateAnchor(Diagram diagram, BaseLinkModel link, ILinkable model)
	{
		if (model is NetworkPortModel port)
		{
			return new SinglePortAnchor(port) { MiddleIfNoMarker = true };
		}

		throw new NotImplementedException();
	}

	async Task AddConnectionToViewModelAsync(Guid sourceEntityId, Guid targetEntityId)
	{
		await networkUserControl.InvokeWinzorDispatcherAsync(() =>
		{
			var sourceViewModel = nodesSource.FirstOrDefault(n => n.Entity.EntityPK == sourceEntityId);
			var targetViewModel = nodesSource.FirstOrDefault(n => n.Entity.EntityPK == targetEntityId);

			if (sourceViewModel != null && targetViewModel != null)
			{
				var relationship = networkUserControl.NetworkViewModel.Network.CreateRelationship(sourceViewModel.Entity, targetViewModel.Entity);
				if (relationship != null)
				{
					var connection = new ConnectionViewModel()
					{
						SourceConnector = sourceViewModel.OutputConnectors[0],
						DestConnector = targetViewModel.InputConnectors[0],
						Relationship = relationship
					};

					var existingConnection = networkUserControl.NetworkViewModel.Connections.SingleOrDefault(c => c.SourceConnector == sourceViewModel.OutputConnectors[0] && c.DestConnector == targetViewModel.InputConnectors[0]);

					if (existingConnection != null)
					{
						networkUserControl.NetworkViewModel.RemoveConnection(existingConnection);
					}

					networkUserControl.NetworkViewModel.AddConnection(connection);
				}
			}
		});
	}

	#endregion

	async void Diagram_SelectionChangedAsync(SelectableModel model)
	{
		if (model is NetworkNodeModel networkNodeModel)
		{
			if (networkNodeModel.IsSettingSelectionOnClient)
			{
				return;
			}

			await networkNodeModel.Diagram_SelectedChangedAsync();
		}
	}

	void NodesSource_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		if (e.Action == NotifyCollectionChangedAction.Reset)
		{
			Reset();
			return;
		}

		// Must call ToArray() inside Winzor thread.
		var nodesToRemove = GetNodesToRemove(e).ToArray();
		var nodesToAdd = GetNodesToAdd(e).ToArray();

		networkUserControl.InvokeRenderDispatcher(() => UpdateDiagramNodesAsync(nodesToRemove, nodesToAdd));
	}

	IEnumerable<NetworkNodeModel> GetNodesToRemove(NotifyCollectionChangedEventArgs e)
	{
		var nodes = e.OldItems?.Cast<NodeViewModel>().Select(n => n.Entity.EntityPK) ?? Enumerable.Empty<Guid>();
		return Nodes.OfType<NetworkNodeModel>().Where(n => nodes.Contains(n.EntityPK));
	}

	IEnumerable<NetworkNodeModel> GetNodesToAdd(NotifyCollectionChangedEventArgs eventArgs)
	{
		var factory = new NodeModelFactory(networkUserControl);
		var nodes = eventArgs.NewItems?.Cast<NodeViewModel>().Select(nodeViewModel =>
		{
			var nodeModel = factory.GetNodeModel(nodeViewModel);

			var parent = GetParent(nodeModel);
			parent?.AddChildNode(nodeModel);
			return nodeModel;
		});

		return nodes ?? Enumerable.Empty<NetworkNodeModel>();
	}

	JobNodeModel? GetParent(NetworkNodeModel child)
	{
		if (child.GetEntity().Parent != null)
		{
			return (JobNodeModel?)Nodes.SingleOrDefault(n => n is JobNodeModel node && entityEqualityComparer.Equals(child.GetEntity().Parent, node.GetEntity()));
		}
		return null;
	}
	readonly ProposedNetworkEntityEqualityComparer entityEqualityComparer = new ProposedNetworkEntityEqualityComparer();

	Task UpdateDiagramNodesAsync(NetworkNodeModel[] nodesToRemove, NetworkNodeModel[] nodesToAdd)
	{
		networkUserControl.AssertInRenderThread();

		Batch(() =>
		{
			Nodes.Remove(nodesToRemove);
			Nodes.Add(nodesToAdd);
		});
		return Task.CompletedTask;
	}

	record Link(
		Guid sourceEntityId,
		Guid targetEntityId,
		IConnectionService? connectionViewModel = null,
		ConnectionData? connectionModelData = null);

	void ConnectionsSource_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		networkUserControl.AssertInWinzorThread();

		if (e.Action == NotifyCollectionChangedAction.Reset)
		{
			Reset();
		}

		// must call ToCollection() inside Winzor thread
		var linksToRemove = GetLinksToRemove(e.OldItems).ToHashSet();
		var linksToAdd = GetLinksToAdd(e.NewItems).ToList();

		networkUserControl.InvokeRenderDispatcher(() => UpdateDiagramLinksAsync(linksToRemove, linksToAdd));
	}

	IEnumerable<Link> GetLinksToAdd(IList? newItems)
	{
		if (newItems is null || newItems.Count == 0)
		{
			return Enumerable.Empty<Link>();
		}

		return newItems
			.Cast<ConnectionViewModel>()
			.Select(c => new ConnectionService(c, networkUserControl))
			.Select(c => new Link(
				sourceEntityId: c.GetSourceConnectorPK(),
				targetEntityId: c.GetTargetConnectorPK(),
				connectionViewModel: c,
				connectionModelData: c.GetConnectionModelData()));
	}

	IEnumerable<Link> GetLinksToRemove(IList? oldItems)
	{
		if (oldItems is null || oldItems.Count == 0)
		{
			return Enumerable.Empty<Link>();
		}

		return oldItems
			.Cast<ConnectionViewModel>()
			.Select(c => new Link(c.SourceConnector.ParentNode.Entity.EntityPK, c.DestConnector.ParentNode.Entity.EntityPK));
	}

	Task UpdateDiagramLinksAsync(ICollection<Link> toRemove, IEnumerable<Link> toAdd)
	{
		networkUserControl.AssertInRenderThread();

		var linksToRemove = Links.OfType<NetworkLinkModel>()
			.Where(l => l.SourceEntityPK.HasValue)
			.Where(l => l.TargetEntityPK.HasValue)
			.Where(l => toRemove.Contains(new Link(l.SourceEntityPK!.Value, l.TargetEntityPK!.Value)))
			.ToArray();

		var dicOfNodes = Nodes.OfType<JobNodeModel>().ToDictionary(n => n.EntityPK);
		var linksToAdd = toAdd
			.Where(l => dicOfNodes.ContainsKey(l.sourceEntityId))
			.Where(l => dicOfNodes.ContainsKey(l.targetEntityId))
			.Select(l => new NetworkLinkModel(
				dicOfNodes[l.sourceEntityId],
				dicOfNodes[l.targetEntityId],
				l.connectionViewModel,
				l.connectionModelData))
			.ToArray();

		Batch(() =>
		{
			Links.Remove(linksToRemove);
			Links.Add(linksToAdd);
		});

		return Task.CompletedTask;
	}

	void Reset()
	{
		networkUserControl.AssertInWinzorThread();

		var linkableNodeModels = new Dictionary<Guid, LinkableNodeModel>();
		var nodeModels = new List<NetworkNodeModel>();
		var linkModels = new List<LinkModel>();

		var factory = new NodeModelFactory(networkUserControl);
		foreach (var node in nodesSource)
		{
			var nodeModel = factory.GetNodeModel(node);

			if (nodeModel is LinkableNodeModel linkableNode)
			{
				linkableNodeModels[linkableNode.EntityPK] = linkableNode;
			}
			else if (nodeModel is AnnotationNodeModel annotationNode)
			{
				nodeModels.Add(annotationNode);
			}
		}

		var jobNodes = linkableNodeModels.Where(kv => kv.Value is JobNodeModel).ToDictionary(kv => kv.Key, kv => (JobNodeModel)kv.Value);
		var allNodeModels = linkableNodeModels.Values.Concat(nodeModels);

		foreach (var node in jobNodes.Values)
		{
			node.ResetChildNodes(allNodeModels);
		}

		foreach (var connection in connectionsSource)
		{
			var sourceGuid = connection.SourceConnector.ParentNode.Entity.EntityPK;
			var targetGuid = connection.DestConnector.ParentNode.Entity.EntityPK;
			if (linkableNodeModels.TryGetValue(sourceGuid, out var sourceNode) && linkableNodeModels.TryGetValue(targetGuid, out var targetNode))
			{
				var link = new NetworkLinkModel(linkableNodeModels[sourceGuid], linkableNodeModels[targetGuid], new ConnectionService(connection, networkUserControl));
				linkModels.Add(link);
			}
		}
		nodeModels.AddRange(linkableNodeModels.Values);

		// Must call ToArray() inside Winzor thread
		var nodes = nodeModels.ToArray();
		var links = linkModels.ToArray();

		networkUserControl.InvokeRenderDispatcher(() => ResetDiagramNodesAsync(nodes, links));
	}

	Task ResetDiagramNodesAsync(NetworkNodeModel[] nodes, LinkModel[] links)
	{
		networkUserControl.AssertInRenderThread();

		foreach (var node in Nodes.OfType<IDisposable>())
		{
			node.Dispose();
		}

		Batch(() =>
		{
			Nodes.Clear();
			Links.Clear();
			Nodes.Add(nodes);
			foreach (var node in nodes.Where(n => n.IsSelected))
			{
				node.IsSettingSelectionOnClient = true;
				SelectModel(node, false);
				node.IsSettingSelectionOnClient = false;
			}
			Links.Add(links);
		});

		return Task.CompletedTask;
	}
	
	internal async Task CopySelectionToClipboardAsync()
	{
		await networkUserControl.InvokeWinzorDispatcherAsync(() =>
		{
			var selectedNodes = networkUserControl.NetworkViewModel.SelectedNodes.Select(x => x.Entity);
			if (selectedNodes.Any())
			{
				networkUserControl.NetworkViewModel.Network.TryCopyShapeStateToClipBoard(selectedNodes);
			}
		});
	}

	internal async Task PasteFromClipboardAsync()
	{
		await networkUserControl.InvokeWinzorDispatcherAsync(async () =>
		{
			if (networkUserControl.JSRuntime == null)
			{
				var ex = new Exception("Value cannot be null. (Parameter 'jsRuntime')");
				ex.Data.Add("networkUserControl.JSRuntime", networkUserControl.JSRuntime);
				ex.Data.Add("networkUserControl.NetworkViewModel.DiagramNodeViewModel.DiagramName", networkUserControl.NetworkViewModel.DiagramNodeViewModel?.DiagramName);
				ex.Data.Add("networkUserControl.NetworkViewModel.DiagramNodeViewModel.Status", networkUserControl.NetworkViewModel.DiagramNodeViewModel?.Status);
				ExceptionReporter.Instance.ReportException("13c40174", ex);
				return;
			}

			await SafeClipboard.FetchClipboardDataAsync(networkUserControl.JSRuntime);
			var selectedNodes = networkUserControl.NetworkViewModel.SelectedNodes.Select(x => x.Entity);
			if (!selectedNodes.Any())
			{
				networkUserControl.NetworkViewModel.Network.PasteShapeFromClipBoard(networkUserControl.NetworkViewModel);
				networkUserControl.FitNodes();
				networkUserControl.NetworkViewModel.Refresh();
			}
			else if (selectedNodes.Count() == 1)
			{
				ExecuteLinkingEntityToShapeFromClipboard();
			}
		});
	}

	void ExecuteLinkingEntityToShapeFromClipboard()
	{
		using (networkUserControl.NetworkViewModel.Network.SuspendRefreshingOnEntityCountChanged())
		{
			linkFromClipboardAction.Execute();
		}
	}

	internal async Task DeleteSelectionAsync()
	{
		await networkUserControl.InvokeWinzorDispatcherAsync(() =>
		{
			var selectedNodes = networkUserControl.NetworkViewModel.SelectedNodes;
			foreach (var nodeViewModel in selectedNodes)
			{
				networkUserControl.NetworkViewModel.DeleteNode(nodeViewModel);
			}
		});
	}

	internal void SelectAll()
	{
		Nodes.ForEach(node => SelectModel(node, false));
	}

	internal async Task InvertSelectionAsync()
	{
		await networkUserControl.InvokeWinzorDispatcherAsync(() =>
		{
			var nodes = networkUserControl.NetworkViewModel.Nodes;

			foreach (var nodeViewModel in nodes)
			{
				nodeViewModel.IsSelected = !nodeViewModel.IsSelected;
			}
		});
	}

	internal void DeselectAll()
	{
		Nodes.ForEach(node => UnselectModel(node));
	}

	void SetZoomFromViewModel()
	{
		var contentScale = networkUserControl.ViewModel.ContentScale;
		networkUserControl.InvokeRenderDispatcher(() =>
		{
			if (Zoom != contentScale)
			{
				SetZoom(contentScale);
			}
			return Task.CompletedTask;
		});
	}

	#region Delete Nodes

	internal async Task DeleteNodeFromViewModelAsync(NodeModel model)
	{
		if (model is NetworkNodeModel networkNode)
		{
			var entityId = networkNode.EntityPK;
			await networkUserControl.InvokeWinzorDispatcherAsync(() =>
			{
				var nodeToDelete = nodesSource.FirstOrDefault(n => n.Entity.EntityPK == entityId);
				if (nodeToDelete != null)
				{
					if (nodeToDelete.CanHide)
					{
						networkUserControl.NetworkViewModel.RemoveFromDiagram(nodeToDelete);
					}
					else if (nodeToDelete.CanDelete)
					{
						networkUserControl.NetworkViewModel.DeleteNode(nodeToDelete);
					}
				}
			});
			networkNode.Dispose();
		}
		else
		{
			throw new NotImplementedException();
		}
	}

	internal async Task DeleteLinkFromViewModelAsync(BaseLinkModel model)
	{
		if (model is NetworkLinkModel networklink)
		{
			ArgumentNullException.ThrowIfNull(networklink.SourceEntityPK);
			ArgumentNullException.ThrowIfNull(networklink.TargetEntityPK);
			await networkUserControl.InvokeWinzorDispatcherAsync(() =>
			{
				var connection = connectionsSource.FirstOrDefault(c =>
					c.SourceConnector.ParentNode.Entity.EntityPK == networklink.SourceEntityPK.Value &&
					c.DestConnector.ParentNode.Entity.EntityPK == networklink.TargetEntityPK.Value);

				if (connection is not null)
				{
					if (connection.Relationship is null || networkUserControl.NetworkViewModel.Network.DeleteRelationship(connection.Relationship))
					{
						networkUserControl.NetworkViewModel.RemoveConnection(connection);
					}
				}
			});
		}
		else
		{
			throw new NotImplementedException();
		}
	}

	#endregion

	void OnLinkRemoved(BaseLinkModel link)
	{
		if (link is NetworkLinkModel jobLink && link == inProgressLink)
		{
			inProgressLink = null;
		}
	}

	readonly NodeResizeControlFactory? NodeResizeControlFactory;

	internal virtual bool? TryCanAttachToInProgressLink(NetworkPortModel port) => inProgressLink?.Source.Model?.CanAttachTo(port);
	BaseLinkModel? inProgressLink;
}
