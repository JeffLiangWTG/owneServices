using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CommissionAgreementConflictsFinderTest : TestCaseWithFactory
	{
		#region New

		public void TestNew()
		{
			AssertType(typeof(CommissionAgreementConflictsFinder), CommissionAgreementConflictsFinder.New());
		}

		#endregion

		#region GetMoreGenericCommissionAgreementConflicts

		public void TestGetMoreGenericCommissionAgreementConflicts()
		{
			var customer = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = customer.SalesOpportunities.AddNew();
			var agreement1 = opportunity.CommissionAgreements.AddNew();
			agreement1.CA0_OH_Customer = customer.PK;
			agreement1.FillWithValidTestData();
			var agreement2 = opportunity.CommissionAgreements.AddNew();
			agreement2.CA0_OH_Customer = customer.PK;
			agreement2.FillWithValidTestData();
			var agreement3 = opportunity.CommissionAgreements.AddNew();
			agreement3.CA0_OH_Customer = customer.PK;
			agreement3.FillWithValidTestData();
			var reversedAgreement = opportunity.CommissionAgreements.AddNew();
			reversedAgreement.CA0_OH_Customer = customer.PK;
			reversedAgreement.Reverse();
			reversedAgreement.FillWithValidTestData();
			var wbpStreamAgreement = opportunity.CommissionAgreements.AddNew();
			wbpStreamAgreement.CA0_OH_Customer = customer.PK;
			wbpStreamAgreement.CA0_CommissionStream = "WBP";
			wbpStreamAgreement.FillWithValidTestData();

			var item1_ALL_ALL_ALL = OrgCommissionAgreementTestHelper.SetSingleCommissionAgreementOverallItem(agreement1, OrgCommissionAgreementItemLookups.AllProductsCode, OrgCommissionAgreementItemLookups.AllServicesCode, OrgCommissionAgreementItemLookups.AllSubModulesCode);

			agreement2.ProductItems.DeleteAll();
			var item2_XXX_ALL_ALL = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(agreement2, "XXX", OrgCommissionAgreementItemLookups.AllServicesCode, OrgCommissionAgreementItemLookups.AllSubModulesCode);
			var item2_YYY_ALL_ALL = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(agreement2, "YYY", OrgCommissionAgreementItemLookups.AllServicesCode, OrgCommissionAgreementItemLookups.AllSubModulesCode);

			var item3_XXX_XXX_ALL = OrgCommissionAgreementTestHelper.SetSingleCommissionAgreementOverallItem(agreement3, "XXX", "XXX", OrgCommissionAgreementItemLookups.AllSubModulesCode);

			var reversed4_ALL_ALL_ALL = OrgCommissionAgreementTestHelper.SetSingleCommissionAgreementOverallItem(reversedAgreement, OrgCommissionAgreementItemLookups.AllProductsCode, OrgCommissionAgreementItemLookups.AllServicesCode, OrgCommissionAgreementItemLookups.AllSubModulesCode);
			var wbpStreamItem_ALL_ALL_ALL = OrgCommissionAgreementTestHelper.SetSingleCommissionAgreementOverallItem(wbpStreamAgreement, OrgCommissionAgreementItemLookups.AllProductsCode, OrgCommissionAgreementItemLookups.AllServicesCode, OrgCommissionAgreementItemLookups.AllSubModulesCode);

			Factory.Save();

			var conflictsFinder = CommissionAgreementConflictsFinder.New();
			var actualConflicts = conflictsFinder.GetMoreGenericCommissionAgreementConflicts(opportunity, false).Cast<CommissionAgreementItemConflict>().ToList();
			AssertNotNull(actualConflicts.Single(x => x.LoserAgreementItem == item1_ALL_ALL_ALL.ParentItem.ParentItem && x.WinnerAgreementItem == item2_XXX_ALL_ALL));
			AssertNotNull(actualConflicts.Single(x => x.LoserAgreementItem == item1_ALL_ALL_ALL.ParentItem.ParentItem && x.WinnerAgreementItem == item2_YYY_ALL_ALL));
			AssertNotNull(actualConflicts.Single(x => x.LoserAgreementItem == item2_XXX_ALL_ALL.ParentItem && x.WinnerAgreementItem == item3_XXX_XXX_ALL));

			AssertEquals(3, actualConflicts.Count);

			agreement2.CA0_CommissionBasis = "XXX";
			actualConflicts = conflictsFinder.GetMoreGenericCommissionAgreementConflicts(opportunity, true).Cast<CommissionAgreementItemConflict>().ToList();
			AssertNotNull(actualConflicts.Single(x => x.LoserAgreementItem == item1_ALL_ALL_ALL.ParentItem.ParentItem && x.WinnerAgreementItem == item2_XXX_ALL_ALL));
			AssertNotNull(actualConflicts.Single(x => x.LoserAgreementItem == item1_ALL_ALL_ALL.ParentItem.ParentItem && x.WinnerAgreementItem == item2_YYY_ALL_ALL));
			AssertEquals("Should only include conflicts of agreement2 since it is the only agreement that has been modified", 2, actualConflicts.Count);
		}

		public void TestGetCommissionAgreementConflicts_OnlyConsidersAgreementsInDatabase()
		{
			var customer = Factory.NewWithValidTestData<OrgHeader>();
			var opportunity = customer.SalesOpportunities.AddNew();
			var agreement1 = opportunity.CommissionAgreements.AddNew();
			agreement1.CA0_OH_Customer = customer.PK;
			agreement1.FillWithValidTestData();
			var item1_XXX_ALL_ALL = OrgCommissionAgreementTestHelper.SetSingleCommissionAgreementOverallItem(agreement1, "XXX", OrgCommissionAgreementItemLookups.AllServicesCode, OrgCommissionAgreementItemLookups.AllSubModulesCode);

			Factory.Save();

			var newUnsavedAgreement2 = opportunity.CommissionAgreements.AddNew();
			newUnsavedAgreement2.CA0_OH_Customer = customer.PK;
			var item2_ALL_ALL_ALL = OrgCommissionAgreementTestHelper.SetSingleCommissionAgreementOverallItem(newUnsavedAgreement2, OrgCommissionAgreementItemLookups.AllProductsCode, OrgCommissionAgreementItemLookups.AllServicesCode, OrgCommissionAgreementItemLookups.AllSubModulesCode);

			var conflictsFinder = CommissionAgreementConflictsFinder.New();
			var actualConflicts = conflictsFinder.GetMoreGenericCommissionAgreementConflicts(opportunity, false).Cast<CommissionAgreementItemConflict>().ToList();
			AssertEquals("newUnsavedAgreement2 should not conflict with any other agreements while it is not saved yet.", 0, actualConflicts.Count);
		}

		#endregion

		#region GetNewCommissionAgreementConflicts

		public void TestGetNewCommissionAgreementConflicts()
		{
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			var agreement = opportunity.CommissionAgreements.AddNew();
			agreement.FillWithValidTestData();
			agreement.CA0_OH_Customer = opportunity.P8_OH;
			agreement.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			agreement.CA0_EffectiveDate = new ZDate(2002, 2, 2);
			var item = OrgCommissionAgreementTestHelper.SetSingleCommissionAgreementOverallItem(agreement, OrgCommissionAgreementItemLookups.AllProductsCode, OrgCommissionAgreementItemLookups.AllServicesCode, OrgCommissionAgreementItemLookups.AllSubModulesCode);

			Factory.Save();

			var conflictsFinder = CommissionAgreementConflictsFinder.New();
			var conflicts = conflictsFinder.GetMoreGenericCommissionAgreementConflicts(opportunity, false);
			AssertEquals("Precondition", false, conflicts.Any());

			var agreement2 = opportunity.CommissionAgreements.AddNew();
			agreement2.FillWithValidTestData();
			agreement2.CA0_OH_Customer = opportunity.P8_OH;
			agreement2.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			agreement2.CA0_EffectiveDate = new ZDate(2002, 2, 2);
			var item2 = OrgCommissionAgreementTestHelper.SetSingleCommissionAgreementOverallItem(agreement2, "XXX", OrgCommissionAgreementItemLookups.AllServicesCode, OrgCommissionAgreementItemLookups.AllSubModulesCode);

			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					Tuple.Create(item2, agreement)
				},
				conflictsFinder.GetNewCommissionAgreementConflicts(opportunity, conflicts).Cast<ICommissionAgreementItemConflict>().Select(x => Tuple.Create(x.AgreementItem, x.LoserCommissionAgreement)));

			Factory.Save();
			conflicts = conflictsFinder.GetMoreGenericCommissionAgreementConflicts(opportunity, false);

			agreement2.CA0_EffectiveDate = new ZDate(2002, 2, 3);

			AssertContainsExactElementsInAnyOrder("Effective Date Range was decreased",
				Enumerable.Empty<Tuple<OrgCommissionAgreementItem, OrgCommissionAgreement>>(),
				conflictsFinder.GetNewCommissionAgreementConflicts(opportunity, conflicts).Cast<ICommissionAgreementItemConflict>().Select(x => Tuple.Create(x.AgreementItem, x.LoserCommissionAgreement)));

			agreement2.CA0_EffectiveDate = new ZDate(2002, 2, 1);

			AssertContainsExactElementsInAnyOrder("Effective Date Range was increased",
				new[]
				{
					Tuple.Create(item2, agreement)
				},
				conflictsFinder.GetNewCommissionAgreementConflicts(opportunity, conflicts).Cast<ICommissionAgreementItemConflict>().Select(x => Tuple.Create(x.AgreementItem, x.LoserCommissionAgreement)));

			Factory.Save();
			conflicts = conflictsFinder.GetMoreGenericCommissionAgreementConflicts(opportunity, false);

			item2.Delete();
			AssertContainsExactElementsInAnyOrder("item was deleted",
				Enumerable.Empty<Tuple<OrgCommissionAgreementItem, OrgCommissionAgreement>>(),
				conflictsFinder.GetNewCommissionAgreementConflicts(opportunity, conflicts).Cast<ICommissionAgreementItemConflict>().Select(x => Tuple.Create(x.AgreementItem, x.LoserCommissionAgreement)));
		}

		public void TestGetNewConflictsForEachSpecificAgreement_WithPreviousConflictAgreementApproved()
		{
			var otherFactory = new BusinessObjectFactory();
			var customer = otherFactory.NewWithValidTestData<OrgHeader>();
			var opportunity = OrgCommissionAgreementTestHelper.GetNewEffectiveOpportunity(otherFactory, customer);
			var agreement1 = OrgCommissionAgreementTestHelper.AddNewEffectiveAgreement(opportunity, customer, "ALL", "ALL", "ALL");
			agreement1.CA0_LastApprovedDateUtc = new ZDateTime(2002, 2, 2);
			var agreement2 = OrgCommissionAgreementTestHelper.AddNewEffectiveAgreement(opportunity, customer, "XXX", "XXX", "XXX");
			agreement2.CA0_LastApprovedDateUtc = new ZDateTime(2002, 2, 2);

			var agreement1Draft = agreement1.CreateDraft();
			agreement1Draft.CA0_CommissionBasis = CommissionBasisType.Codes.PRF;

			otherFactory.Save();

			var opportunityInCurrentFactory = Factory.Load<OrgOpportunity>(opportunity.PK);
			var conflictsFinder = CommissionAgreementConflictsFinder.New();
			var previousConflicts = conflictsFinder.GetMoreGenericCommissionAgreementConflicts(opportunityInCurrentFactory, false);

			AssertEquals("Precondition", 1, previousConflicts.Count);
			var previousConflict = previousConflicts.Single();
			AssertEquals("Precondition", agreement1Draft.PK, previousConflict.LoserCommissionAgreement.PK);
			AssertEquals("Precondition", agreement2.PK, previousConflict.WinnerCommissionAgreement.MainVersion.PK);

			agreement1Draft.ApproveDraft();
			otherFactory.Save();

			var currentConflicts = conflictsFinder.GetMoreGenericCommissionAgreementConflicts(opportunityInCurrentFactory, false);
			AssertEquals("Precondition", 1, currentConflicts.Count);
			var currentConflict = currentConflicts.Single();
			AssertEquals("Precondition", agreement1.PK, currentConflict.LoserCommissionAgreement.PK);
			AssertEquals("Precondition", agreement2.PK, currentConflict.WinnerCommissionAgreement.MainVersion.PK);

			AssertContainsExactElementsInAnyOrder("There aren't any 'new' conflicts - previous conflict was for a draft version of the current conflict",
				Enumerable.Empty<ICommissionAgreementConflict>(),
				conflictsFinder.GetNewCommissionAgreementConflicts(opportunity, previousConflicts));
		}

		#endregion

		#region Find Duplicates

		public void TestFindDuplicates()
		{
			var org = Factory.New<OrgHeader>();
			var agreement1 = OrgCommissionAgreementTestHelper.GetNewEffectiveAgreement(Factory, org);
			var agreement1ItemXXXXXXXXX = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(agreement1, "XXX", "XXX", "XXX");
			var agreement1ItemXXXXXXALL = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(agreement1, "XXX", "XXX", "ALL");

			var agreement2 = OrgCommissionAgreementTestHelper.GetNewEffectiveAgreement(Factory, org);
			var agreement2ItemXXXXXXXXX = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(agreement2, "XXX", "XXX", "XXX");

			var agreement3 = OrgCommissionAgreementTestHelper.GetNewEffectiveAgreement(Factory, org);
			var agreement3ItemXXXXXXXXX = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(agreement3, "XXX", "XXX", "XXX");

			var agreementStreamB1 = OrgCommissionAgreementTestHelper.GetNewEffectiveAgreement(Factory, org);
			agreementStreamB1.CA0_CommissionStream = "BBB";
			var agreementStreamB1ItemXXXXXXXXX = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(agreementStreamB1, "XXX", "XXX", "XXX");
			var agreementStreamB1ItemXXXXXXALL = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(agreementStreamB1, "XXX", "XXX", "ALL");

			var agreementStreamB2 = OrgCommissionAgreementTestHelper.GetNewEffectiveAgreement(Factory, org);
			agreementStreamB2.CA0_CommissionStream = "BBB";
			var agreementStreamB2ItemXXXXXXXXX = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(agreementStreamB2, "XXX", "XXX", "XXX");

			var agreementStreamB3 = OrgCommissionAgreementTestHelper.GetNewEffectiveAgreement(Factory, org);
			agreementStreamB3.CA0_CommissionStream = "BBB";
			var agreementStreamB3ItemXXXXXXXXX = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(agreementStreamB3, "XXX", "XXX", "XXX");

			var finder = CommissionAgreementConflictsFinder.New();
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					agreement1ItemXXXXXXXXX,
					agreement3ItemXXXXXXXXX
				},
				finder.FindDuplicates(agreement2ItemXXXXXXXXX).Cast<CommissionAgreementItemDuplication>().Select(x => x.CommissionAgreementItem));

			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					agreementStreamB1ItemXXXXXXXXX,
					agreementStreamB3ItemXXXXXXXXX
				},
				finder.FindDuplicates(agreementStreamB2ItemXXXXXXXXX).Cast<CommissionAgreementItemDuplication>().Select(x => x.CommissionAgreementItem));
		}

		public void TestFindDuplicatesWithReversedAgreement()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var agreement1 = OrgCommissionAgreementTestHelper.GetNewEffectiveAgreement(Factory, org);
			var agreement1Item1 = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(agreement1, "XXX", "XXX", "XXX");

			var agreement2 = OrgCommissionAgreementTestHelper.GetNewEffectiveAgreement(Factory, org);
			var agreement2Item1 = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(agreement2, "XXX", "XXX", "XXX");

			var finder = CommissionAgreementConflictsFinder.New();
			var duplicates = finder.FindDuplicates(agreement2Item1).Cast<CommissionAgreementItemDuplication>().Select(x => x.CommissionAgreementItem).ToArray();

			AssertEquals(1, duplicates.Length);
			AssertEquals(agreement1Item1.PK, duplicates[0].PK);

			agreement1.CA0_LastApprovedDateUtc = new ZDateTime(2002, 2, 2);
			Factory.Save();
			AssertEquals(true, agreement1.IsApproved);

			finder.GetMoreGenericCommissionAgreementConflicts(agreement1.Opportunity, false);
			agreement1.Reverse();
			Factory.Save();
			AssertEquals(true, agreement1.IsReversed);

			var agreements = Factory.Load<OrgCommissionAgreement>(new ZQuery());
			AssertEquals(3, agreements.Length);
			AssertCollectionContains(agreement1, agreements);
			AssertCollectionContains(agreement2, agreements);
			var agreement1Draft = agreements.First(x => x.PK != agreement1.PK && x != agreement2.PK);

			AssertEquals(0, finder.FindDuplicates(agreement2Item1).Count);
		}

		#endregion
	}
}
