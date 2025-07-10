using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Module.Testing
{
	[TestedType(typeof(ValueAnalysisCustomsBrokerageOppModule))]
	public class ValueAnalysisCustomsBrokerageModuleOppTest : ValueAnalysisOpportunityModuleTest
	{
		protected override IValueAnalysisModuleForTest GetValueAnalysisModuleForTest() => new ValueAnalysisCustomsBrokerageOppModuleForTest();
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.ValueAnalysisCustomsBrokerageOpp;
	}
}
