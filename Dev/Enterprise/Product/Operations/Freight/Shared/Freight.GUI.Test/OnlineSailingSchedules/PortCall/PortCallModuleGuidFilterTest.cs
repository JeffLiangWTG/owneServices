using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.GUI.Testing
{
	[TestedType(typeof(PortCallModuleGuidFilter))]
	sealed class PortCallModuleGuidFilterTest : ModuleGuidFilterTest
	{
		public void TestHasComparisonOperatorIsFalse()
		{
			var filter = new PortCallModuleGuidFilter("moo", DummyModuleIDs.Dummy, DummyBizoSchema.Z0_Guid, new DummyBusinessObjectCollection(Factory));
			Assert(!filter.HasComparisonOperator);
		}
		protected override ModuleGuidFilter GetNewModuleFilter()
		{
			var result = new PortCallModuleGuidFilter("moo", ModuleIDs.JobShipment, DummyBizoSchema.Z0_Guid, new StmNoteNonDependentCollection(Factory));
			result.SupportsFiltersMatchComparisonOperator = true;

			return result;
		}
	}
}
