using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Module.Testing
{
	[TestedType(typeof(ValueAnalysisCustomsBrokerageOrgModule))]
	public class ValueAnalysisCustomsBrokerageOrgModuleTest : ValueAnalysisOrganisationModuleTest
	{
		protected override IValueAnalysisModuleForTest GetValueAnalysisModuleForTest() => new ValueAnalysisCustomsBrokerageOrgModuleForTest();
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.ValueAnalysisCustomsBrokerageOrg;
	}
}
