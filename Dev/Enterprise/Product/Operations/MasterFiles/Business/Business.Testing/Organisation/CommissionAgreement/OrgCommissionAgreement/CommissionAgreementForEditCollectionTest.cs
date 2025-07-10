using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(CommissionAgreementForEditCollection))]
	class CommissionAgreementForEditCollectionTest : ActiveBusinessObjectCollectionTestCase<CommissionAgreementForEditCollection>
	{
		#region Default Values

		public void TestDefaultValues_IsDraft()
		{
			var opportunity = Factory.New<OrgOpportunity>();
			var agreement = opportunity.CommissionAgreementsForEdit.AddNew();

			AssertEquals(true, agreement.IsDraft);
		}

		public void TestDefaultValues_ProductItems()
		{
			var opportunity = Factory.New<OrgOpportunity>();
			var agreement = opportunity.CommissionAgreementsForEdit.AddNew();

			AssertEquals(1, agreement.ProductItems.Count);
			var productItem = agreement.ProductItems[0];
			AssertEquals(true, productItem.CAI_IsInclude);
			AssertEquals(OrgCommissionAgreementItemLookups.AllProductsCode, productItem.CAI_Code);
		}

		#endregion

		#region AddUncommittedDraftAgreements

		public void TestAddUncommittedDraftAgreements()
		{
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var approvedAgreement = opportunity.ApprovedCommissionAgreements.AddNew();
			approvedAgreement.FillWithValidTestData();
			Factory.Save();

			var anotherFactory = new BusinessObjectFactory();
			var opportunityInAnotherFactory = anotherFactory.Load<OrgOpportunity>(opportunity.PK);
			AssertEquals(1, opportunityInAnotherFactory.CommissionAgreementsForEdit.Count);
			AssertEquals(true, opportunityInAnotherFactory.CommissionAgreementsForEdit[0].IsUncommittedDraft);
			AssertEquals(approvedAgreement.PK, opportunityInAnotherFactory.CommissionAgreementsForEdit[0].UncommittedParentVersion.PK);
		}

		public void TestAddUncommittedDraftAgreements_AfterNewAgreementIsApproved()
		{
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var newAgreement = opportunity.CommissionAgreementsForEdit.AddNew();
			newAgreement.FillWithValidTestData();
			Factory.Save();
			AssertEquals("Precondition", 1, opportunity.CommissionAgreementsForEdit.Count);
			AssertEquals("Precondition", false, opportunity.HasChanges);

			var anotherFactory = new BusinessObjectFactory();
			var newAgreementInAnotherFactory = anotherFactory.Load<OrgCommissionAgreement>(newAgreement.PK);
			newAgreementInAnotherFactory.ApproveDraft();
			anotherFactory.Save();

			AssertEquals("Should re-add another uncommitted draft agreement after original is approved", 1, opportunity.CommissionAgreementsForEdit.Count);
			AssertEquals("Should still not have changes", false, opportunity.HasChanges);
		}

		#endregion

		#region Permissions to Edit Approved and Unapproved Commissions

		public void TestPermissionToEditApprovedAndUnApprovedCommissions()
		{
			var collection = GetCollectionWithApprovedAndUnapprovedCommission();
			void AssertCommissionAgreementForEdit(bool editApprovedCommissionAllowed, bool editUnapprovedCommissionAllowed)
			{
				AssertEquals(editApprovedCommissionAllowed, collection[0].IsEditable);
				AssertEquals(editUnapprovedCommissionAllowed, collection[1].IsEditable);
				AssertEquals(!editApprovedCommissionAllowed, collection[0].ReadOnly);
				AssertEquals(!editUnapprovedCommissionAllowed, collection[1].ReadOnly);
			}

			Env.Security.ApprovedCommissionAgreementEdit.IsAllowed = true;
			Env.Security.UnapprovedCommissionAgreementEdit.IsAllowed = false;
			collection.UpdateAgreementsReadOnlyProperty();
			AssertCommissionAgreementForEdit(Env.Security.ApprovedCommissionAgreementEdit.IsAllowed, Env.Security.UnapprovedCommissionAgreementEdit.IsAllowed);

			Env.Security.ApprovedCommissionAgreementEdit.IsAllowed = false;
			Env.Security.UnapprovedCommissionAgreementEdit.IsAllowed = true;
			collection.UpdateAgreementsReadOnlyProperty();
			AssertCommissionAgreementForEdit(Env.Security.ApprovedCommissionAgreementEdit.IsAllowed, Env.Security.UnapprovedCommissionAgreementEdit.IsAllowed);

			Env.Security.ApprovedCommissionAgreementEdit.IsAllowed = false;
			Env.Security.UnapprovedCommissionAgreementEdit.IsAllowed = false;
			collection.UpdateAgreementsReadOnlyProperty();
			AssertCommissionAgreementForEdit(Env.Security.ApprovedCommissionAgreementEdit.IsAllowed, Env.Security.UnapprovedCommissionAgreementEdit.IsAllowed);

			Env.Security.ApprovedCommissionAgreementEdit.IsAllowed = true;
			Env.Security.UnapprovedCommissionAgreementEdit.IsAllowed = true;
			collection.UpdateAgreementsReadOnlyProperty();
			AssertCommissionAgreementForEdit(Env.Security.ApprovedCommissionAgreementEdit.IsAllowed, Env.Security.UnapprovedCommissionAgreementEdit.IsAllowed);
		}

		#endregion

		#region Overrides

		protected override CommissionAgreementForEditCollection GetCollectionToTest()
		{
			var opportunity = Factory.New<OrgOpportunity>();
			return new CommissionAgreementForEditCollection(opportunity);
		}

		#endregion

		CommissionAgreementForEditCollectionForTest GetCollectionWithApprovedAndUnapprovedCommission()
		{
			var opportunity = Factory.New<OrgOpportunity>();
			var collection = new CommissionAgreementForEditCollectionForTest(opportunity);
			var approved = GetApprovedAgreement();
			var unapproved = GetUnapprovedAgreement();
			collection.Add(approved);
			collection.Add(unapproved);
			return collection;
		}

		OrgCommissionAgreement GetApprovedAgreement()
		{
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var agreement = opportunity.ApprovedCommissionAgreements.AddNew();
			agreement.FillWithValidTestData();
			return agreement;
		}

		OrgCommissionAgreement GetUnapprovedAgreement()
		{
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var agreement = opportunity.CommissionAgreements.AddNew();
			agreement.FillWithValidTestData();
			return agreement;
		}

		class CommissionAgreementForEditCollectionForTest : CommissionAgreementForEditCollection
		{
			public CommissionAgreementForEditCollectionForTest(OrgOpportunity opportunity) : base(opportunity)
			{
			}

			public OrgOpportunity Opportunity_Exposed => Opportunity;
		}
	}
}
