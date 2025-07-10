using Enterprise.Freight.GUI.OnlineSailingSchedules;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.OnlineSailingSchedules.Testing
{
	[TestedType(typeof(OnlineSchedulesModuleNumberFilter))]
	public class OnlineSchedulesModuleNumberFilterTest : ModuleNumberFilterTest
	{
		public void TestHasComparisonOperatorIsFalse()
		{
			var filter = new OnlineSchedulesModuleNumberFilter("moo", DummyBizoSchema.Z0_NVarChar);
			Assert(!filter.HasComparisonOperator);
		}
		protected override ModuleTextFilter GetNewModuleFilter()
		{
			return new OnlineSchedulesModuleNumberFilter("moo", DummyBizoSchema.Z0_NVarChar);
		}
	}
}
