#nullable enable
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Core.Models.Base;
using CargoWise.NetworkVisualisation.Business;
using CargoWise.NetworkVisualisation.GUI.Controls.NodeResizing;
using CargoWise.NetworkVisualisation.GUI.Services.Events;
using CargoWise.NetworkVisualisation.GUI.Services.NetworkNode;
using CargoWise.NetworkVisualisation.GUI.Services.Utils;
using CargoWise.NetworkVisualisation.Integration;
using DiagramsPoint = Blazor.Diagrams.Core.Geometry.Point;
using Size = Blazor.Diagrams.Core.Geometry.Size;

namespace CargoWise.NetworkVisualisation.GUI.Models;

/// <summary>
/// Represents the model for the NetworkNode Component.
/// It contains data and behaviors to render a node on the network.
/// </summary>
/// <remarks>
/// Inherits <see cref="NodeModel"/> and implements <see cref="IDisposable"/>.
/// It is used in <see cref="Components.NetworkNodeBase{T}"/>.
/// </remarks>
[DebuggerDisplay("PK: {EntityPK}", Name = "{DiagramName}")]
public abstract class NetworkNodeModel : NodeModel
{
	INetworkNodeService Service { get; }
	NetworkNodeData Data { get; set; }
	/// <summary>
	/// Gets the network model primary key.
	/// </summary>

	public Guid EntityPK => Data.EntityPK;

	/// <summary>
	/// Event triggered when a property changes on the server.
	/// </summary>
	/// <remarks>
	/// This event is nullable and may not always have subscribers.
	/// </remarks>
	public event EventHandler<PropertyChangedEventArgs>? PropertyChangedFromServer;

	/// <summary>
	/// Event triggered when the object is disposed.
	/// </summary>
	/// <remarks>
	/// This event can be used to perform custom actions when the object is disposed.
	/// </remarks>
	public event EventHandler? Disposed;

	internal JobNodeModel? ParentNode;

	protected NetworkNodeModel(INetworkNodeService service)
	{
		Service = service;

		Data = Service.GetNetworkNodeData();

		Position = new (Data.X, Data.Y);
		Size = new (Data.Width, Data.Height);
		Locked = Data.IsLocked;

		Service.PropertyValueChanged += OnPropertyValueChanged;
		Resized += Node_Resized;
		Moved += Node_Moved;
		Updater = CreatePropertiesUpdater();
		MinimumDimensions = new(Data.MinimunWidth, Data.MinimumHeight);
		ControlledSize = true;
	}

	/// </summary>
	/// <returns>
	/// The network entity associated with the ViewModel.
	/// </returns>
	public INetworkEntity GetEntity()
	{
		return Data.Entity;
	}

	/// <summary>
	/// Sets the position of the node on the network diagram.
	/// </summary>
	/// <param name="x">The x-coordinate of the new position.</param>
	/// <param name="y">The y-coordinate of the new position.</param>
	/// <remarks>
	/// This method updates the Position property with a new DiagramsPoint.
	/// </remarks>
	public override void SetPosition(double x, double y)
	{
		Position = new DiagramsPoint(x, y);
		base.SetPosition(x, y);
	}

	/// <summary>
	/// Set the size of the node on the network diagram.
	/// </summary>
	/// <param name="size">The new size of the node.</param>
	public virtual void SetSize(Size size)
	{
		Size = size;
	}

	async void Node_Resized(Model node)
	{
		await Service.ResizeAsync(
			Position.X,
			Position.Y,
			Size?.Width ?? MinimumDimensions.Width,
			Size?.Height ?? MinimumDimensions.Height);
	}

	async void Node_Moved(Model node)
	{
		isMoving = false;

		await Service.MoveAsync(Position.X, Position.Y);
	}

	internal async Task Diagram_SelectedChangedAsync()
	{
		if (Selected != IsSelected)
		{
			await Service.UpdateIsSelectedAsync(Selected);
		}
	}

	internal void ShowNodeAsSelected(bool value)
	{
		if (IsSelected == value)
		{
			return;
		}

		Data.IsSelected = value;
		Refresh();
	}

	internal Task Diagram_DiagramNameChangedAsync() => Service.UpdateDiagramNameAsync(DiagramName);

	internal Task Diagram_NotesChangedAsync() => Service.UpdateNotesAsync(Notes);

	/// <summary>
	/// Disposes this node and its associated event handlers to prevent memory leaks.
	/// </summary>
	public virtual void Dispose()
	{
		Resized -= Node_Resized;
		Moved -= Node_Moved;
		Service.PropertyValueChanged -= OnPropertyValueChanged;
		Service.Dispose();
		Disposed?.Invoke(this, EventArgs.Empty);
	}

	/// <summary>
	/// Gets or sets the diagram name.
	/// </summary>
	/// <remarks>
	/// This property is only settable within the assembly (internal set).
	/// </remarks>
	public string DiagramName
	{
		get => Data.DiagramName;
		internal set => Data.DiagramName = value;
	}

	/// <summary>
	/// Gets the node status colors of the node.
	/// </summary>
	public NodeColors StatusColors => Data.StatusColors;

	/// <summary>
	/// Get the font weight of the node.
	/// </summary>
	public NodeFontWeight StatusTextWeight => Data.StatusTextWeight;

	/// <summary>
	/// Gets or set the notes for the node.
	/// </summary>
	/// <remarks>
	/// This property is only settable within the assembly (internal set).
	/// </remarks>
	public string Notes
	{
		get => Data.Notes;
		internal set => Data.Notes = value;
	}

	/// <summary>
	/// Gets the notes placeholder of the node.
	/// </summary>
	public string NotesPlaceholder => Data.NotesPlaceholder;

	/// <summary>
	/// Gets the foreground color of the node.
	/// </summary>
	public Color ForegroundColor => Data.ForegroundColor;

	/// <summary>
	/// Gets the tooltip of the node.
	/// </summary>
	public string Tooltip => Data.ToolTip;

	/// <summary>
	/// Gets the zindex of the node.
	/// </summary>
	public int ZIndex => Data.ZIndex;

	/// <summary>
	/// Gets if the node is selected.
	/// </summary>
	public bool IsSelected => Data.IsSelected;

	/// <summary>
	/// Gets the delete tooltip.
	/// </summary>
	public string DeleteTooltip => Data.DeleteTooltip;

	/// <summary>
	/// Gets the entity state.
	/// </summary>
	/// <remarks>
	/// Acceptable entitystates are None, Inactive, Fixed, Approved, NotApproved, HasErrors, HasWarnings, HasMessages.
	/// </remarks>
	public EntityState EntityState => Data.EntityState;

	/// <summary>
	/// Gets if the node has notifications.
	/// </summary>
	public bool HasNotifications => Data.HasNotifications && Data.HasNotificationsForState;

	/// <summary>
	/// Gets the entity state tooltip.
	/// </summary>
	public string EntityStateTooltip => Data.EntityStateTooltip;

	/// <summary>
	/// Gets if node is moving.
	/// </summary>
	public bool IsMoving => isMoving;
	bool isMoving;

	/// <summary>
	/// Gets or sets the top left resizer.
	/// </summary>
	public NodeResizeControl? TopLeftResizer { get; set; }
	/// <summary>
	/// Gets or sets the top right resizer.
	/// </summary>
	public NodeResizeControl? TopRightResizer { get; set; }
	/// <summary>
	/// Gets or sets the bottom left resizer.
	/// </summary>
	public NodeResizeControl? BottomLeftResizer { get; set; }
	/// <summary>
	/// Gets or sets the bottom right resizer.
	/// </summary>
	public NodeResizeControl? BottomRightResizer { get; set; }

	/// <summary>
	/// Gets or sets if settings selecttion is on client.
	/// </summary>
	public bool IsSettingSelectionOnClient { get; set; }

	internal Task<IEnumerable<NetworkActionMenuItem>> GetContextMenuItemsAsync() => Service.GetContextMenuItemsAsync();

	/// <summary>
	/// Asynchronously views the entity in the network.
	/// </summary>
	/// <returns>
	/// A Task representing the asynchronous operation.
	/// </returns>
	/// <remarks>
	/// This method uses the WinzorDispatch to invoke the ViewEntity method on the NetworkViewModel's Network.
	/// </remarks>
	public Task View() => Service.ShowViewAsync();

	/// <summary>
	/// Event triggered when a NodeModel is resized.
	/// </summary>
	/// <remarks>
	/// This event can be used to perform custom actions when a NodeModel is resized.
	/// </remarks>
	public event Action<NodeModel>? Resized;

	/// <summary>
	/// Triggers the Resized event.
	/// </summary>
	/// <remarks>
	/// This method invokes the Resized event, if it has subscribers.
	/// </remarks>
	public void TriggerResized()
	{
		Resized?.Invoke(this);
	}

	/// <summary>
	/// Initiates the process of moving a node.
	/// </summary>
	/// <remarks>
	/// This method sets the 'isMoving' flag to true and triggers the moving process.
	/// </remarks>
	public void NodeMoving()
	{
		isMoving = true;
		TriggerMoving();
	}

	protected bool HasProgressBar => Data.HasProgressBar;

	#region OnPropertyValueChanged

	readonly PropertiesUpdater Updater;

	PropertiesUpdater CreatePropertiesUpdater()
	{
		var updater = new PropertiesUpdater();
		InitializePropertiesUpdater(updater);
		return updater;
	}

	protected virtual void InitializePropertiesUpdater(PropertiesUpdater updater) => updater
		.Add(nameof(NetworkNodeData.X),                        (v) => TryUpdateX((double)v))
		.Add(nameof(NetworkNodeData.Y),                        (v) => TryUpdateY((double)v))
		.Add(nameof(NetworkNodeData.Width),                    (v) => TryUpdateWidth(Math.Max((double)v, Data.MinimunWidth)))
		.Add(nameof(NetworkNodeData.Height),                   (v) => TryUpdateHeight(Math.Max((double)v, Data.MinimumHeight)))
		.Add(nameof(NetworkNodeData.HasProgressBar),           (v) => UpdateHasProgressBar((bool)v))
		.Add(nameof(NetworkNodeData.ZIndex),                   (v) => Properties.TryUpdate((int)v, Data.ZIndex, (value) => Data.ZIndex = value))
		.Add(nameof(NetworkNodeData.HasNotifications),         (v) => Properties.TryUpdate((bool)v, Data.HasNotifications, (value) => Data.HasNotifications = value))
		.Add(nameof(NetworkNodeData.HasNotificationsForState), (v) => Properties.TryUpdate((bool)v, Data.HasNotificationsForState, (value) => Data.HasNotificationsForState = value))
		.Add(nameof(NetworkNodeData.IsSelected),               (v) => Properties.TryUpdate((bool)v, Data.IsSelected, UpdateIsSelected))
		.Add(nameof(NetworkNodeData.DeleteTooltip),            (v) => Properties.TryUpdate((string)v, Data.DeleteTooltip, (value) => Data.DeleteTooltip = value))
		.Add(nameof(NetworkNodeData.DiagramName),              (v) => Properties.TryUpdate((string)v, Data.DiagramName, (value) => Data.DiagramName = value))
		.Add(nameof(NetworkNodeData.Notes),                    (v) => Properties.TryUpdate((string)v, Data.Notes, (value) => Data.Notes = value))
		.Add(nameof(NetworkNodeData.NotesPlaceholder),         (v) => Properties.TryUpdate((string)v, Data.NotesPlaceholder, (value) => Data.NotesPlaceholder = value))
		.Add(nameof(NetworkNodeData.ToolTip),                  (v) => Properties.TryUpdate((string)v, Data.ToolTip, (value) => Data.ToolTip = value))
		.Add(nameof(NetworkNodeData.EntityState),              (v) => Properties.TryUpdate((EntityState)v, Data.EntityState, (value) => Data.EntityState = value))
		.Add(nameof(NetworkNodeData.EntityStateTooltip),       (v) => Properties.TryUpdate((string)v, Data.EntityStateTooltip, (value) => Data.EntityStateTooltip = value))
		.Add(nameof(NetworkNodeData.ForegroundColor),          (v) => Properties.TryUpdate((Color)v, Data.ForegroundColor, (value) => Data.ForegroundColor = value))
		.Add(nameof(NetworkNodeData.StatusColors),             (v) => Properties.TryUpdate((NodeColors)v, Data.StatusColors, (value) => Data.StatusColors = value))
		.Add(nameof(NetworkNodeData.StatusTextWeight),         (v) => Properties.TryUpdate((NodeFontWeight)v, Data.StatusTextWeight, (value) => Data.StatusTextWeight = value))
		.Add(nameof(NetworkNodeData.IsLocked),                 (v) => TryUpdateIsLocked((bool)v))
	;

	void OnPropertyValueChanged(object? sender, PropertyValueChangedEventArgs e)
	{
		if (Updater.TryUpdate(e))
		{
			PropertyChangedFromServer?.Invoke(this, e);
			Refresh();
		}
	}

	protected virtual bool UpdateHasProgressBar(bool value)
	{
		Data.HasProgressBar = value;
		return true; // Because the progress bar instance may have been replaced in the BizO.
	}

	void UpdateIsSelected(bool value)
	{
		Data.IsSelected = value;

		if (TopLeftResizer is not null)
		{
			TopLeftResizer.IsVisible = value;
		}
		if (TopRightResizer is not null)
		{
			TopRightResizer.IsVisible = value;
		}
		if (BottomLeftResizer is not null)
		{
			BottomLeftResizer.IsVisible = value;
		}
	}

	bool TryUpdateX(double value)
	{
		var updPosition = Properties.TryUpdate(value, Position.X, (value) => SetPosition(value, Position.Y));
		var updData     = Properties.TryUpdate(value, Data.X,     (value) => Data.X = value);

		return updPosition || updData;
	}

	bool TryUpdateY(double value)
	{
		var updPosition = Properties.TryUpdate(value, Position.Y, (value) => SetPosition(Position.X, value));
		var updData     = Properties.TryUpdate(value, Data.Y,     (value) => Data.Y = value);

		return updPosition || updData;
	}

	bool TryUpdateWidth(double value)
	{
		var updSize = Properties.TryUpdate(value, Size?.Width ?? 0, (value) => SetSize(new (value, Size?.Height ?? 0)));
		var updData = Properties.TryUpdate(value, Data.Width,       (value) => Data.Width = value);

		return updSize || updData;
	}

	bool TryUpdateHeight(double value)
	{
		var updSize = Properties.TryUpdate(value, Size?.Height ?? 0, (value) => SetSize(new (Size?.Width ?? 0, value)));
		var updData = Properties.TryUpdate(value, Data.Height, (value) => Data.Height = value);

		return updSize || updData;
	}

	bool TryUpdateIsLocked(bool value)
	{
		var updLocked = Properties.TryUpdate(value, Locked, (value) => Locked = value);
		var updData = Properties.TryUpdate(value, Data.IsLocked, (value) => Data.IsLocked = value);

		return updData || updLocked;
	}

	#endregion
}
