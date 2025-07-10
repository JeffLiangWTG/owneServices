using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefAirlineCommodityCodeFilterBusinessObject))]
	sealed class RefAirlineCommodityCodeFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		#region Implementation

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new RefAirlineCommodityCodeFilterBusinessObject();
		}

		#endregion
	}
}
