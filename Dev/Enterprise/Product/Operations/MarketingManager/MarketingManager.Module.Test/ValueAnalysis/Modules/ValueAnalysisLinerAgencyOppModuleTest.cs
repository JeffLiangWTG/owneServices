using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Module.Testing
{
	[TestedType(typeof(ValueAnalysisLinerAgencyOppModule))]
	public class ValueAnalysisLinerAgencyOppModuleTest : ValueAnalysisOpportunityModuleTest
	{
		protected override IValueAnalysisModuleForTest GetValueAnalysisModuleForTest() => new ValueAnalysisLinerAgencyOppModuleForTest();
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.ValueAnalysisLinerAgencyOpp;
	}
}
