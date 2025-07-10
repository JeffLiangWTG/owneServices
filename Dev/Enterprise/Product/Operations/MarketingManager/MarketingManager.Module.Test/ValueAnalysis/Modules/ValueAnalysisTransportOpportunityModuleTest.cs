using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Module.Testing
{
	[TestedType(typeof(ValueAnalysisTransportOppModule))]
	public class ValueAnalysisTransportOpportunityModuleTest : ValueAnalysisOpportunityModuleTest
	{
		protected override IValueAnalysisModuleForTest GetValueAnalysisModuleForTest() => new ValueAnalysisTransportOppModuleForTest();
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.ValueAnalysisTransportOpp;
	}
}
