using System;
using Blazor.Diagrams.Core.Anchors;
using Blazor.Diagrams.Core.Models;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.GUI.Services.Connection;
using CargoWise.NetworkVisualisation.GUI.Services.Events;
using CargoWise.NetworkVisualisation.GUI.Services.Utils;
using CargoWise.NetworkVisualisation.GUI.Utils;
using CargoWise.NetworkVisualisation.Integration;
using Enterprise.BufferManagement.NetworkVisualisation.Business;

namespace CargoWise.NetworkVisualisation.GUI.Models;

public class NetworkLinkModel : LinkModel
{
	public EventHandler Disposed;

	/// <summary>
	/// Constructor for NetworkLinkModel. NetworkLinkModel is a model managing the display of a link between two nodes. <br/>
	/// Will use the constructor <see cref="NetworkLinkModel(Anchor, Anchor)"/> with the source and target anchors created by the link models passed in. <br/>
	/// Along with process of adding values from <paramref name="connectionViewModel"/>.
	/// Setup of <see cref="DeleteTooltip"/> and <see cref="NetworkAttachment"/> will also be performed here.
	/// </summary>
	/// <param name="sourcePort">The source port model the link is created from.</param>
	/// <param name="targetPort">The target port model the link is linked to.</param>
	/// <param name="connectionViewModel">The view model of the connection if applicable, default to null if not passed in.</param>
	/// <param name="connectionModelData">The connection model data as optional. Useful when requried to create an instance out of Winzor Thread.</param>
	public NetworkLinkModel(PortModel sourcePort, PortModel targetPort,
		IConnectionService connectionViewModel = null, ConnectionData connectionModelData = null)
		: this(new SinglePortAnchor(sourcePort) { MiddleIfNoMarker = true },
			  new SinglePortAnchor(targetPort) { MiddleIfNoMarker = true })
	{
		ViewModel = connectionViewModel;
		Data = connectionModelData ?? ViewModel?.GetConnectionModelData() ?? new ConnectionData();

		if (ViewModel is not null)
		{
			ViewModel.PropertyValueChanged += OnPropertyValueChanged;
		}
	}

	/// <summary>
	/// Constructor for NetworkLinkModel. NetworkLinkModel is a model managing the display of a link between two nodes.
	/// </summary>
	/// <param name="source">The source anchor the link is created from.</param>
	/// <param name="target">The target anchor the link is linked to.</param>
	public NetworkLinkModel(Anchor source, Anchor target) : base(source, target)
	{
		PathGenerator = new NetworkLinkPathGenerator();
		Data = new ConnectionData();
	}

	/// <summary>
	/// Constructor for NetworkLinkModel. NetworkLinkModel is a model managing the display of a link between two nodes. <br/>
	/// A variation of <see cref="NetworkLinkModel(PortModel, PortModel, ConnectionViewModel)"/> accepting <see cref="LinkableNodeModel"/> as source and target. <br/>
	/// Will be using <see cref="LinkableNodeModel.Output"/> of the source and <see cref="LinkableNodeModel.Input"/> of the target as the parameters.
	/// </summary>
	/// <param name="source">The source node model the link is created from.</param>
	/// <param name="target">The target node model the link is linked to.</param>
	/// <param name="connectionViewModel">The view model of the connection if applicable, default to null if not passed in.</param>
	/// <param name="connectionModelData">The connection model data as optional. Useful when requried to create an instance out of Winzor Thread.</param>
	public NetworkLinkModel(LinkableNodeModel source, LinkableNodeModel target,
		IConnectionService connectionViewModel = null, ConnectionData connectionModelData = null)
		: this(source.Output, target.Input, connectionViewModel, connectionModelData)
	{
	}

	LinkableNodeModel SourceNode => Source is SinglePortAnchor anchor
		&& anchor.Port is NetworkPortModel port
		&& port.Parent is LinkableNodeModel node ? node : null;

	LinkableNodeModel TargetNode => Target is SinglePortAnchor anchor &&
		anchor.Port is NetworkPortModel port
		&& port.Parent is LinkableNodeModel node ? node : null;

	/// <summary>
	/// Name of the source node.
	/// </summary>
	public string SourceName => SourceNode?.GetName();

	/// <summary>
	/// Name of the target node.
	/// </summary>
	public string TargetName => TargetNode?.GetName();

	/// <summary>
	/// Entity PK of the source node.
	/// </summary>
	public Guid? SourceEntityPK => SourceNode?.EntityPK;

	/// <summary>
	/// Entity PK of the target node.
	/// </summary>
	public Guid? TargetEntityPK => TargetNode?.EntityPK;

	/// <summary>
	/// Refresh the Network Link Model. <br/>
	/// Consecutively call the base class's refresh method and set <see cref="QuickHideDeleteIcon"/> to true.
	/// </summary>
	public override void Refresh()
	{
		base.Refresh();
		QuickHideDeleteIcon = true;
	}

	/// <summary>
	/// The tooltip string for the delete icon.
	/// </summary>
	public string DeleteTooltip => Data.DeleteTooltip;

	protected IConnectionService ViewModel { get; }
	ConnectionData Data { get; }

	/// <summary>
	/// Value of title attribute for NetworkLink.
	/// </summary>
	public string DisplayText => Data.DisplayText;

	/// <summary>
	/// Boolean value indicating if the link is a resource dependency. Being used to determined if the node can be attached.
	/// </summary>
	public bool IsResourceDependency => Data.IsResourceDependency;

	/// <summary>
	/// Boolean value indicating if the link is visible.
	/// </summary>
	public bool IsVisible => Data.IsVisible;

	/// <summary>
	/// String value of the back color of the link.
	/// </summary>
	public string BackColor => Data.BackColor;

	/// <summary>
	/// The appearance of the link. Options are Normal, Dotted, and Dashed.
	/// See <see cref="ArrowAppearance"/>.
	/// </summary>
	public ArrowAppearance Appearance => Data.Appearance;

	/// <summary>
	/// Dispose method for NetworkLinkModel. <br/>
	/// </summary>
	public virtual void Dispose()
	{
		ViewModel.PropertyValueChanged -= OnPropertyValueChanged;
		Disposed.Invoke(this, EventArgs.Empty);
	}

	/// <summary>
	/// Boolean value used for hiding the delete icon when dragging.
	/// </summary>
	public bool QuickHideDeleteIcon { get; set; }

	#region OnPropertyValueChanged

	void OnPropertyValueChanged(object sender, PropertyValueChangedEventArgs e)
	{
		if (TryUpdateProperty(e))
		{
			Refresh();
		}
	}

	bool TryUpdateProperty(PropertyValueChangedEventArgs e) => e.PropertyName switch
	{
		nameof(ConnectionData.IsVisible) => Properties.TryUpdate(
			newValue:     (bool)e.Value,
			currentValue: Data.IsVisible,
			set:          (value) => Data.IsVisible = value),
		nameof(ConnectionData.BackColor) => Properties.TryUpdate((string)e.Value, Data.BackColor, (value) => Data.BackColor = value),
		nameof(ConnectionData.Appearance) => Properties.TryUpdate((ArrowAppearance)e.Value, Data.Appearance, (value) => Data.Appearance = value),
		nameof(ConnectionData.DisplayText) => Properties.TryUpdate((string)e.Value, Data.DisplayText, (value) => Data.DisplayText = value),
		nameof(ConnectionData.IsResourceDependency) => Properties.TryUpdate((bool)e.Value, Data.IsResourceDependency, (value) => Data.IsResourceDependency = value),
		_ => false,
	};

	#endregion

}
