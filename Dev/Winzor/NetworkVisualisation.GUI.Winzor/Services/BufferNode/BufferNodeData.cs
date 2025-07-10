using CargoWise.PAVE.Common.Interfaces;

namespace CargoWise.NetworkVisualisation.GUI.Services.BufferNode;

public class BufferNodeData
{
	public string Description { get; internal set; }
	public string AdditionalDetail { get; internal set; }
	public string StatusTooltip { get; internal set; }
	public string PenetrationPercentLabel { get; internal set; }
	public WorkStatus Status { get; internal set; }
}
