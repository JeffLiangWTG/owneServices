using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgCommissionAgreementItemValidationTest : BusinessObjectValidationTestCase
	{
		#region Properties

		public void TestCheckCAI_Code()
		{
			var item = Factory.New<OrgCommissionAgreementItem>();

			item.CAI_Code = "XXX";
			AssertMandatoryValidationError(item.CAI_CodeInfo, false);
			AssertListValidationInvalidCodeError(item.CAI_CodeInfo, true);

			item.CAI_Code = "";
			AssertMandatoryValidationError(item.CAI_CodeInfo, true);
			AssertListValidationInvalidCodeError(item.CAI_CodeInfo, false);

			item.CAI_Code = "ALL";
			AssertMandatoryValidationError(item.CAI_CodeInfo, false);
			AssertListValidationInvalidCodeError(item.CAI_CodeInfo, false);
		}

		public void TestCheckCAI_Code_NoDuplicateItems_WhileShowingServicesAndSubModules()
		{
			var customerA = Factory.New<OrgHeader>();
			var customerB = Factory.New<OrgHeader>();
			var opportunity = Factory.New<OrgOpportunity>();
			opportunity.P8_OpportunityID = "O00001001";
			var agreement1 = opportunity.CommissionAgreements.AddNew();
			agreement1.CA0_Name = "#1";
			agreement1.CA0_OH_Customer = customerA.PK;
			var agreement2 = opportunity.CommissionAgreements.AddNew();
			agreement2.CA0_Name = "#2";
			agreement2.CA0_OH_Customer = customerA.PK;
			var agreement3 = opportunity.CommissionAgreements.AddNew();
			agreement3.CA0_Name = "#3";
			agreement3.CA0_OH_Customer = customerB.PK;

			using (CommissionLookupsForTest.TemporarilyOverrideShouldShowServicesAndSubModulesForTesting(true))
			{
				var item1_AAA_BBB_CCC = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(agreement1, "AAA", "BBB", "CCC");
				var item2_AAA_BBB_CCC = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(agreement2, "AAA", "BBB", "CCC");
				var item3_AAA_BBB_DDD = OrgCommissionAgreementTestHelper.AddInclusionCommissionAgreementOverallItem(agreement3, "AAA", "BBB", "DDD");

				item2_AAA_BBB_CCC.Validation.ValidateCAI_Code();
				AssertHasError(item2_AAA_BBB_CCC.CAI_CodeInfo, "Other agreement(s) have a duplicate item. (Commission Agreement O00001001#1)");

				item2_AAA_BBB_CCC.CAI_Code = "DDD";
				AssertNoError("Is not a duplicate of agreement 3 as it is for a different customer", item2_AAA_BBB_CCC.CAI_CodeInfo, "Other agreement(s) have a duplicate item. (Commission Agreement O00001001#1)");
				AssertNoError("Is not a duplicate of agreement 3 as it is for a different customer", item2_AAA_BBB_CCC.CAI_CodeInfo, "Other agreement(s) have a duplicate item. (Commission Agreement O00001001#3)");

				agreement2.CA0_OH_Customer = customerB.PK;
				item2_AAA_BBB_CCC.Validation.ValidateCAI_Code();
				AssertHasError(item2_AAA_BBB_CCC.CAI_CodeInfo, "Other agreement(s) have a duplicate item. (Commission Agreement O00001001#3)");

				agreement3.Reverse();
				item2_AAA_BBB_CCC.Validation.ValidateCAI_Code();
				AssertNoError("Is not a duplicate of agreement 3 as it has been reversed", item2_AAA_BBB_CCC.CAI_CodeInfo, "Other agreement(s) have a duplicate item. (Commission Agreement O00001001#3)");
			}
		}

		public void TestCheckCAI_Code_NoDuplicateItems()
		{
			var customerA = Factory.New<OrgHeader>();
			var customerB = Factory.New<OrgHeader>();
			var opportunity = Factory.New<OrgOpportunity>();
			opportunity.P8_OpportunityID = "O00001001";
			var agreement1 = opportunity.CommissionAgreements.AddNew();
			agreement1.CA0_Name = "#1";
			agreement1.CA0_OH_Customer = customerA.PK;
			var agreement2 = opportunity.CommissionAgreements.AddNew();
			agreement2.CA0_Name = "#2";
			agreement2.CA0_OH_Customer = customerA.PK;
			var agreement3 = opportunity.CommissionAgreements.AddNew();
			agreement3.CA0_Name = "#3";
			agreement3.CA0_OH_Customer = customerB.PK;

			using (CommissionLookupsForTest.TemporarilyOverrideShouldShowServicesAndSubModulesForTesting(false))
			{
				var item1_AAA = agreement1.ProductItems.AddNew(true, "AAA");
				var item2_AAA = agreement2.ProductItems.AddNew(true, "AAA");
				var item3_BBB = agreement3.ProductItems.AddNew(true, "BBB");

				item2_AAA.Validation.ValidateCAI_Code();
				AssertHasError(item2_AAA.CAI_CodeInfo, "Other agreement(s) have a duplicate item. (Commission Agreement O00001001#1)");

				item2_AAA.CAI_Code = "BBB";
				AssertNoError("Is not a duplicate of agreement 3 as it is for a different customer", item2_AAA.CAI_CodeInfo, "Other agreement(s) have a duplicate item. (Commission Agreement O00001001#1)");
				AssertNoError("Is not a duplicate of agreement 3 as it is for a different customer", item2_AAA.CAI_CodeInfo, "Other agreement(s) have a duplicate item. (Commission Agreement O00001001#3)");

				agreement2.CA0_OH_Customer = customerB.PK;
				item2_AAA.Validation.ValidateCAI_Code();
				AssertHasError(item2_AAA.CAI_CodeInfo, "Other agreement(s) have a duplicate item. (Commission Agreement O00001001#3)");

				agreement3.Reverse();
				item2_AAA.Validation.ValidateCAI_Code();
				AssertNoError("Is not a duplicate of agreement 3 as it has been reversed", item2_AAA.CAI_CodeInfo, "Other agreement(s) have a duplicate item. (Commission Agreement O00001001#3)");
			}
		}

		public void TestCheckCAI_Code_NoDuplicateItemsWithReversedAgreement()
		{
			var customerA = Factory.New<OrgHeader>();
			var customerB = Factory.New<OrgHeader>();
			var opportunity = Factory.New<OrgOpportunity>();
			opportunity.P8_OpportunityID = "O00001001";
			var agreement1 = opportunity.CommissionAgreements.AddNew();
			agreement1.CA0_Name = "#1";
			agreement1.CA0_OH_Customer = customerA.PK;
			var agreement2 = opportunity.CommissionAgreements.AddNew();
			agreement2.CA0_Name = "#2";
			agreement2.CA0_OH_Customer = customerA.PK;
			var agreement3 = opportunity.CommissionAgreements.AddNew();
			agreement3.CA0_Name = "#3";
			agreement3.CA0_OH_Customer = customerB.PK;

			using (CommissionLookupsForTest.TemporarilyOverrideShouldShowServicesAndSubModulesForTesting(false))
			{
				var item1_AAA = agreement1.ProductItems.AddNew(true, "AAA");
				var item2_AAA = agreement2.ProductItems.AddNew(true, "AAA");
				var item3_BBB = agreement3.ProductItems.AddNew(true, "BBB");

				item2_AAA.Validation.ValidateCAI_Code();
				AssertHasError(item2_AAA.CAI_CodeInfo, "Other agreement(s) have a duplicate item. (Commission Agreement O00001001#1)");

				item2_AAA.CAI_Code = "BBB";
				AssertNoError("Is not a duplicate of agreement 3 as it is for a different customer", item2_AAA.CAI_CodeInfo, "Other agreement(s) have a duplicate item. (Commission Agreement O00001001#1)");
				AssertNoError("Is not a duplicate of agreement 3 as it is for a different customer", item2_AAA.CAI_CodeInfo, "Other agreement(s) have a duplicate item. (Commission Agreement O00001001#3)");

				agreement2.CA0_OH_Customer = customerB.PK;
				item2_AAA.Validation.ValidateCAI_Code();
				AssertHasError(item2_AAA.CAI_CodeInfo, "Other agreement(s) have a duplicate item. (Commission Agreement O00001001#3)");

				agreement2.Reverse();
				item2_AAA.Validation.ValidateCAI_Code();
				AssertNoError(item2_AAA.CAI_CodeInfo, "Other agreement(s) have a duplicate item. (Commission Agreement O00001001#3)");
			}
		}

		public void TestCheckCAI_Code_ALLProduct()
		{
			var customerA = Factory.NewWithValidTestData<OrgHeader>();

			var opportunity1 = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity1.P8_OpportunityID = "O00001001";
			var agreement1 = opportunity1.CommissionAgreements.AddNew();
			agreement1.CA0_Name = "#1";
			agreement1.CA0_OH_Customer = customerA.PK;
			agreement1.FillWithValidTestData();

			var opportunity2 = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity2.P8_OpportunityID = "O00001002";
			var agreement2 = opportunity2.CommissionAgreements.AddNew();
			agreement2.CA0_Name = "#2";
			agreement2.CA0_OH_Customer = customerA.PK;
			agreement2.FillWithValidTestData();

			agreement1.ProductItems.AddNew(true, "SHP");

			Factory.Save();

			var item1 = agreement2.ProductItems.AddNew(true, OrgCommissionAgreementItemLookups.AllProductsCode);

			item1.Validation.ValidateCAI_Code();
			AssertHasWarning(item1.CAI_CodeInfo,
$@"This will not include the following subset of commissions as more specific commission agreements already exist:

ALL > ALL > ALL > ALL

{agreement1.AgreementId}
   SHP | ALL > ALL | ALL


");
		}

		public void TestCheckCAI_IsInclude()
		{
			var agreement = Factory.New<OrgCommissionAgreement>();
			agreement.ProductItems.DeleteAll();

			var item1 = agreement.ProductItems.AddNew(true, OrgCommissionAgreementItemLookups.AllProductsCode);
			AssertNoErrors(item1.CAI_IsIncludeInfo);

			item1.CAI_IsInclude = false;
			AssertHasError(item1.CAI_IsIncludeInfo, "'ALL' can not be an exclusion.");

			item1.CAI_Code = "XXX";
			item1.Validation.ValidateCAI_IsInclude();
			AssertHasError(item1.CAI_IsIncludeInfo, "Must include 'ALL' before exclusions can be added.");

			var item2 = agreement.ProductItems.AddNew(true, OrgCommissionAgreementItemLookups.AllProductsCode);
			item1.Validation.ValidateCAI_IsInclude();
			AssertNoErrors(item1.CAI_IsIncludeInfo);
		}

		#endregion

		#region Row Notifications

		public void TestCheckAtLeastOneChildItemIsIncluded()
		{
			var agreement = Factory.New<OrgCommissionAgreement>();
			agreement.ProductItems.DeleteAll();
			agreement.Validation.ValidateAll();
			AssertHasRowError(agreement, "There must be at least one Product item that is included.");

			var item_noAAA = agreement.ProductItems.AddNew(false, "AAA");
			agreement.Validation.ValidateAll();
			AssertHasRowError(agreement, "There must be at least one Product item that is included.");

			var item_BBB = agreement.ProductItems.AddNew(true, "BBB");
			agreement.Validation.ValidateAll();
			AssertNoRowError(agreement, "There must be at least one Product item that is included.");

			item_BBB.ChildServiceItems.DeleteAll();
			item_BBB.Validation.ValidateAll();
			AssertHasRowError(item_BBB, "There must be at least one Service item that is included.");

			var item_BBB_noAAA = item_BBB.ChildServiceItems.AddNew(false, "AAA");
			item_BBB.Validation.ValidateAll();
			AssertHasRowError(item_BBB, "There must be at least one Service item that is included.");

			var item_BBB_BBB = item_BBB.ChildServiceItems.AddNew(true, "BBB");
			item_BBB.Validation.ValidateAll();
			AssertNoRowError(item_BBB, "There must be at least one Service item that is included.");

			item_BBB_BBB.ChildSubModuleItems.DeleteAll();
			item_BBB_BBB.Validation.ValidateAll();
			AssertHasRowError(item_BBB_BBB, "There must be at least one Sub-Module item that is included.");

			var item_BBB_BBB_noAAA = item_BBB_BBB.ChildSubModuleItems.AddNew(false, "AAA");
			item_BBB_BBB.Validation.ValidateAll();
			AssertHasRowError(item_BBB_BBB, "There must be at least one Sub-Module item that is included.");

			var item_BBB_BBB_BBB = item_BBB_BBB.ChildSubModuleItems.AddNew(true, "BBB");
			item_BBB_BBB.Validation.ValidateAll();
			AssertNoRowError(item_BBB_BBB, "There must be at least one Sub-Module item that is included.");
		}

		#endregion
	}
}
