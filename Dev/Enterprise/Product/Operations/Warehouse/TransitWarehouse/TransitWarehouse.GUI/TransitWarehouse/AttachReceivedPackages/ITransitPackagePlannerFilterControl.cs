using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transit.GUI
{
	public interface ITransitPackagePlannerFilterControl
	{
		ZToolStripButton AssignToolStripButton { get; }
		ZToolStripButton RemoveToolStripButton { get; }
	}
}
