using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Module.Testing
{
	[TestedType(typeof(ValueAnalysisLinerAgencyOrgModule))]
	public class ValueAnalysisLinerAgencyOrgModuleTest : ValueAnalysisOrganisationModuleTest
	{
		protected override IValueAnalysisModuleForTest GetValueAnalysisModuleForTest() => new ValueAnalysisLinerAgencyOrgModuleForTest();
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.ValueAnalysisLinerAgencyOrg;
	}
}
