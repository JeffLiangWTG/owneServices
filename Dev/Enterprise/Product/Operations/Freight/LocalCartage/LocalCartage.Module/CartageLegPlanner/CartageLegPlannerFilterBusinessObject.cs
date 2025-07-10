
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.LocalCartage.Module
{
	public class CartageLegPlannerFilterStripBusinessObject : CartageLegFilterStripBusinessObject
	{
		public CartageLegPlannerFilterStripBusinessObject() : base()
		{
			((IFilterStripBusinessObjectInternals)this).LayoutContext = ModuleIDs.CartageLegPlanner.Name;
		}
	}
}
