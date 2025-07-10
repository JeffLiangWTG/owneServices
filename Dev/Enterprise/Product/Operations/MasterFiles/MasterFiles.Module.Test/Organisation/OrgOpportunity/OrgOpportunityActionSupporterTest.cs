using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(OrgOpportunityActionSupporter))]
	sealed class OrgOpportunityActionSupporterTest : OperationalActionSupporterTest<OrgOpportunityActionSupporter>
	{
		protected override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Opportunity; }
		}

		public void TestSingularElementNoun()
		{
			AssertEquals("opportunity", Supporter.SingularElementNoun);
		}

		public void TestPluralElementNoun()
		{
			AssertEquals("opportunities", Supporter.PluralElementNoun);
		}
	}
}
