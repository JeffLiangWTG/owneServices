using Enterprise.TransportConsignment.Integration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.TransportConsignment.GUI
{
	public class PlannerButton : ZToolStripButton
	{
		public DtbRoutePlannerViewMode ViewMode
		{
			get;
			set;
		}
	}
}
