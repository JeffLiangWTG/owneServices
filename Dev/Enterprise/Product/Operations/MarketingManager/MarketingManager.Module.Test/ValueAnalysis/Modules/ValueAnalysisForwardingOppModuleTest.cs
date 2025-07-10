using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Module.Testing
{
	[TestedType(typeof(ValueAnalysisForwardingOppModule))]
	public class ValueAnalysisForwardingOppModuleTest : ValueAnalysisOpportunityModuleTest
	{
		protected override IValueAnalysisModuleForTest GetValueAnalysisModuleForTest() => new ValueAnalysisForwardingOppModuleForTest();
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.ValueAnalysisForwardingOpp;
	}
}
