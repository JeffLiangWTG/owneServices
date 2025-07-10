using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Routing.S8.GUI.Test
{
	[TestedType(typeof(RealTimeRoutingModuleNumberFilter))]
	public class RealTimeRoutingModuleNumberFilterTest : ModuleNumberFilterTest
	{
		public void TestHasComparisonOperatorIsFalse()
		{
			var filter = new RealTimeRoutingModuleNumberFilter("moo", DummyBizoSchema.Z0_NVarChar);
			Assert(!filter.HasComparisonOperator);
		}

		protected override ModuleTextFilter GetNewModuleFilter()
		{
			return new RealTimeRoutingModuleNumberFilter("moo", DummyBizoSchema.Z0_NVarChar);
		}
	}
}
