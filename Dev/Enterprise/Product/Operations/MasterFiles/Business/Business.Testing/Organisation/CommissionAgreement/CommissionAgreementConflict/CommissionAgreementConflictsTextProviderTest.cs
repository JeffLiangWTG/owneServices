using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CommissionAgreementConflictsTextProviderTest : TestCaseWithFactory
	{
		public void TestNew()
		{
			AssertType(typeof(CommissionAgreementConflictsTextProvider), CommissionAgreementConflictsTextProvider.New());
		}

		public void TestToDisplayList()
		{
			using (CommissionLookupsForTest.TemporarilyOverrideShouldShowServicesAndSubModulesForTesting(true))
			{
				var opportunity = Factory.New<OrgOpportunity>();
				var agreement = opportunity.ApprovedCommissionAgreements.AddNew();
				var agreementItem_XXX_XXX_XXX = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(agreement, "XXX", "XXX", "XXX");
				var agreementItem_XXX_XXX_ALL = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(agreement, "XXX", "XXX", "ALL");
				var agreementItem_YYY_YYY_YYY = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(agreement, "YYY", "YYY", "YYY");
				var agreementItem_YYY_YYY_ALL = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(agreement, "YYY", "YYY", "ALL");

				var conflictXXX = new CommissionAgreementItemConflict(agreementItem_XXX_XXX_XXX, agreementItem_XXX_XXX_ALL);
				var conflictYYY = new CommissionAgreementItemConflict(agreementItem_YYY_YYY_YYY, agreementItem_YYY_YYY_ALL);

				var textProvider = CommissionAgreementConflictsTextProvider.New();

				AssertMultilineASCIIEquals("ToDisplayList",
	@"   XXX > XXX > XXX
   YYY > YYY > YYY",
					textProvider.ToDisplayList(new[] { conflictXXX, conflictYYY }, 1, false));

				AssertMultilineASCIIEquals("ToDisplayList html",
	"<li>XXX &gt; XXX &gt; XXX</li>"
	+ "<li>YYY &gt; YYY &gt; YYY</li>",
					textProvider.ToDisplayList(new[] { conflictXXX, conflictYYY }, 1, true));
			}
		}

		public void TestToDisplayList_WithConditions()
		{
			var opportunity = Factory.New<OrgOpportunity>();
			var agreement = opportunity.ApprovedCommissionAgreements.AddNew();
			var agreementItem_XXX_XXX_AUSYD_UAIEV = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItemWithConditions(agreement, "XXX", "XXX", "AUSYD", "UAIEV");
			var agreementItem_XXX_XXX_AUSYD_ALL = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItemWithConditions(agreement, "XXX", "XXX", "AUSYD", "");
			var agreementItem_YYY_YYY_USLAX_GBLON = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItemWithConditions(agreement, "YYY", "YYY", "USLAX", "GBLON");
			var agreementItem_YYY_YYY_USLAX_ALL = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItemWithConditions(agreement, "YYY", "YYY", "AUSYD", "");

			var conflictXXX = new CommissionAgreementItemConflict(agreementItem_XXX_XXX_AUSYD_UAIEV, agreementItem_XXX_XXX_AUSYD_ALL);
			var conflictYYY = new CommissionAgreementItemConflict(agreementItem_YYY_YYY_USLAX_GBLON, agreementItem_YYY_YYY_USLAX_ALL);

			var textProvider = CommissionAgreementConflictsTextProvider.New();

			AssertMultilineASCIIEquals("ToDisplayList",
@"   XXX | AUSYD > ALL | XXX 
   XXX | AUSYD > UAIEV | XXX
   YYY | AUSYD > ALL | YYY 
   YYY | USLAX > GBLON | YYY",
				textProvider.ToDisplayList(new[] { conflictXXX, conflictYYY }, 1, false));

			AssertMultilineASCIIEquals("ToDisplayList html",
@"<li>XXX | AUSYD &gt; ALL | XXX 
 XXX | AUSYD &gt; UAIEV | XXX</li><li>YYY | AUSYD &gt; ALL | YYY 
 YYY | USLAX &gt; GBLON | YYY</li>",
				textProvider.ToDisplayList(new[] { conflictXXX, conflictYYY }, 1, true));
		}

		public void TestToErrorMessage()
		{
			var opportunity = Factory.New<OrgOpportunity>();
			opportunity.P8_OpportunityID = "O00010023";
			var agreement = opportunity.ApprovedCommissionAgreements.AddNew();
			agreement.CA0_Name = "#1";
			var agreementItem_XXX_XXX_XXX = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(agreement, "XXX", "XXX", "XXX");
			var agreementItem_YYY_YYY_YYY = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(agreement, "YYY", "YYY", "YYY");

			var duplicationXXX = new CommissionAgreementItemDuplication(agreementItem_XXX_XXX_XXX);
			var duplicationYYY = new CommissionAgreementItemDuplication(agreementItem_YYY_YYY_YYY);

			var textProvider = CommissionAgreementConflictsTextProvider.New();

			AssertMultilineASCIIEquals("ToDisplayList",
@"Other agreement(s) have a duplicate item. (Commission Agreement O00010023#1)",
				textProvider.ToErrorMessage(new[] { duplicationXXX, duplicationYYY }));
		}
	}
}
