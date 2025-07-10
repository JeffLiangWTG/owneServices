using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(SalesEnquiryActionSupporter))]
	sealed class SalesEnquiryActionSupporterTest : OperationalActionSupporterTest<SalesEnquiryActionSupporter>
	{
		protected override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.SalesEnquiry; }
		}

		public void TestSingularElementNoun()
		{
			AssertEquals("inquiry", Supporter.SingularElementNoun);
		}

		public void TestPluralElementNoun()
		{
			AssertEquals("inquiries", Supporter.PluralElementNoun);
		}

		public override bool ShouldSupportDocuments
		{
			get { return false; }
		}
	}
}
