using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Organisation.CommissionAgreement.Testing
{
	[TestedType(typeof(CommissionAgreementsModuleFilter))]
	sealed class CommissionAgreementsModuleFilterTest : ModuleFilterTestCase<CommissionAgreementsModuleFilter>
	{
		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		protected override CommissionAgreementsModuleFilter GetNewModuleFilter()
		{
			return new CommissionAgreementsModuleFilter("moo", OrgOpportunitySchema.PK, OrgCommissionAgreementSchema.CA0_P8, new OrgCommissionAgreementCollection(Factory), typeof(OrgOpportunity));
		}

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.Other;
	}
}
