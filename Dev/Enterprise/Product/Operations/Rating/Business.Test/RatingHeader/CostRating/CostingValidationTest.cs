using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business.Testing
{
	public class CostingValidationTest : RatingHeaderValidationTest
	{
		public void TestCheckOrgHeader_OrgHeaderIsNotEmpty_ShouldNotAddErrors()
		{
			var header = Helper.NewOrgHeader();
			header.OH_IsCreditor = true;
			Factory.Save();

			var costingToTest = Helper.NewCosting(null);
			costingToTest.TH_OH = header.PK;
			costingToTest.RunPreSaveValidation();

			AssertNoErrors(costingToTest.TH_OHInfo);
		}

		public void TestCheckOrgHeader_OrgHeaderIsEmptyAndStandardConstingNotExists_ShouldNotAddErrors()
		{
			var costingToTest = Helper.NewCosting(null);
			costingToTest.TH_OH = ZGuid.Empty;
			costingToTest.RunPreSaveValidation();

			AssertNoErrors(costingToTest.TH_OHInfo);
		}

		public void TestCheckOrgHeader_OrgHeaderIsEmptyAndStandardConstingExists_AddErrorAboutOneAllowedStandardCosting()
		{
			var standardCosting = Helper.NewCosting(null);
			standardCosting.TH_OH = ZGuid.Empty;

			var costingToTest = Helper.NewCosting(null);
			costingToTest.TH_OH = ZGuid.Empty;
			costingToTest.RunPreSaveValidation();

			AssertHasError(costingToTest.TH_OHInfo, ErrorMessages.StandardCostAlreadyExists);
		}

		public void TestCheckOrgHeader_OrgHeaderIsEmptyAndStandardConstingNotExists_GlobalCosting()
		{
			var localCosting = Helper.NewCosting(null);
			localCosting.Validation.ValidateTH_OH();

			var globalCosting = Helper.NewGlobalCosting(null);
			globalCosting.Validation.ValidateTH_OH();

			AssertNoErrors("Local Standard Costing does not overlap with Global Standard Costing", localCosting.TH_OHInfo);
			AssertNoErrors("Global Standard Costing does not overlap with Local Standard Costing", globalCosting.TH_OHInfo);

			var globalCosting2 = Helper.NewGlobalCosting(null);
			globalCosting2.Validation.ValidateTH_OH();

			AssertHasError("There are now two standard global costings", globalCosting2.TH_OHInfo, ErrorMessages.StandardGlobalCostAlreadyExists);
		}

		public void TestOrgValidation()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_IsCreditor = true;

			Factory.Save();

			var genericCost = Helper.NewCosting(null);
			genericCost.TH_OH = ZGuid.Empty;
			genericCost.RunPreSaveValidation();
			AssertNoErrors(genericCost.TH_OHInfo);
			AssertEquals(true, genericCost.IsStandardCostRate());

			var specificCost = Helper.NewCosting(null);
			specificCost.TH_OH = header.PK;
			specificCost.RunPreSaveValidation();
			AssertNoErrors(specificCost.TH_OHInfo);
			AssertEquals(false, specificCost.IsStandardCostRate());
		}

		public void TestCostingHeaderValidation()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();

			var cost = Helper.NewCosting(header);
			cost.TH_OH = header.PK;

			cost.RunPreSaveValidation();
			AssertHasError(cost.TH_OHInfo, ErrorMessages.InvalidCostingHeader);

			header.OH_IsCreditor = true;
			Factory.Save();

			cost.RunPreSaveValidation();
			AssertNoError(cost.TH_OHInfo, ErrorMessages.InvalidCostingHeader);

			var cost2 = Helper.NewCosting(header);
			cost2.RunPreSaveValidation();

			AssertHasError(cost2.TH_OHInfo, ErrorMessages.CostForThisSupplierAlreadyExists);
			cost2.TH_GC = ZGuid.Empty;

			cost2.RunPreSaveValidation();
			AssertNoError(cost2.TH_OHInfo, ErrorMessages.CostForThisSupplierAlreadyExists);
			Factory.Save();

			var globalCost = Helper.NewGlobalCosting(header);
			globalCost.RunPreSaveValidation();

			AssertHasError(globalCost.TH_OHInfo, ErrorMessages.GlobalCostForThisSupplierAlreadyExists);

			var header2 = Factory.NewWithValidTestData<OrgHeader>();
			globalCost.TH_OH = header2.PK;

			AssertNoError(globalCost.TH_OHInfo, ErrorMessages.GlobalCostForThisSupplierAlreadyExists);
		}
	}
}
