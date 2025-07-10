using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Module.Testing
{
	[TestedType(typeof(ValueAnalysisWarehouseOppModule))]
	public class ValueAnalysisWarehouseOpportunityModuleTest : ValueAnalysisOpportunityModuleTest
	{
		protected override IValueAnalysisModuleForTest GetValueAnalysisModuleForTest() => new ValueAnalysisWarehouseOppModuleForTest();
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.ValueAnalysisWarehouseOpp;
	}
}
