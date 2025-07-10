namespace CargoWise.NetworkVisualisation.GUI.Controls.NodeResizing;

/// <summary>
/// A factory class for creating instances of <see cref="NodeResizeControl"/>
/// </summary>
public class NodeResizeControlFactory
{
	readonly IGlobalEventProvider globalEventProvider;

	/// <summary>
	/// Initializes a new instance of the NodeResizeControlFactory class.
	/// </summary>
	/// <param name="globalEventProvider">The global event provider to use when creating <see cref="NodeResizeControl"/> instances.</param>
	public NodeResizeControlFactory(IGlobalEventProvider globalEventProvider)
	{
		this.globalEventProvider = globalEventProvider;
	}

	/// <summary>
	/// Creates a new instance of NodeResizeControl.
	/// </summary>
	/// <param name="resizerProvider">The resizer provider to use for the new <see cref="NodeResizeControl"/> instance.</param>
	/// <returns>A new instance of <see cref="NodeResizeControl"/>.</returns>
	public virtual NodeResizeControl CreateControl(NodeResizerProvider nodeResizerProvider)
	{
		return new NodeResizeControl(nodeResizerProvider, globalEventProvider);
	}
}
