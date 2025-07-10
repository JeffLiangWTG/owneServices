using System.Linq;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	[TestedType(typeof(GatewayConsolProfitShareRedistributionFilterBusinessObject))]
	public class GatewayConsolProfitShareRedistributionFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() =>
			new GatewayConsolProfitShareRedistributionFilterBusinessObject();

		public void TestModuleFilters_Visibilities()
		{
			var filterBizO = new GatewayConsolProfitShareRedistributionFilterBusinessObject();
			var filter = filterBizO.ModuleFilters.Single(x => x.Visibility == FilterVisibility.AlwaysVisible);
			AssertEquals("Batch Number", filter.Description);
		}
	}
}
