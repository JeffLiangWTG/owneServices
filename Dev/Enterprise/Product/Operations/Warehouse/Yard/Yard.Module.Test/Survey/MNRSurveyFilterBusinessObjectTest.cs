using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Module.Test
{
	[TestedType(typeof(MNRSurveyFilterBusinessObject))]
	public class MNRSurveyFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new MNRSurveyFilterBusinessObject();
		}
	}
}
