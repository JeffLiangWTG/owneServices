using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Module.Testing
{
	[TestedType(typeof(CartageLegPlannerFilterStripBusinessObject))]
	public class CartageLegPlannerFilterStripBusinessObjectTest : CartageLegFilterStripBusinessObjectTest
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new CartageLegPlannerFilterStripBusinessObject();
		}
	}
}
