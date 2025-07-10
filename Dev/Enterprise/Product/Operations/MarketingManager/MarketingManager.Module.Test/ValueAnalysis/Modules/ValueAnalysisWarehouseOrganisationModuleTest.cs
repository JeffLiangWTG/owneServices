using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Module.Testing
{
	[TestedType(typeof(ValueAnalysisWarehouseOrgModule))]
	public class ValueAnalysisWarehouseOrganisationModuleTest : ValueAnalysisOrganisationModuleTest
	{
		protected override IValueAnalysisModuleForTest GetValueAnalysisModuleForTest() => new ValueAnalysisWarehouseOrgModuleForTest();
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.ValueAnalysisWarehouseOrg;
	}
}
