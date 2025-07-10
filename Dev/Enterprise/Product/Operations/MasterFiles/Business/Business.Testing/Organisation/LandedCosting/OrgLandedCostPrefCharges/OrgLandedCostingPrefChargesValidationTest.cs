using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgLandedCostingPrefChargesValidationTest : BusinessObjectValidationTestCase
	{
		#region Charge Code

		public void TestChargeCode()
		{
			AccChargeCode fRT = Factory.New<AccChargeCode>();
			fRT.AC_Code = "FRT";
			fRT.AC_ChargeGroup = IncoTermChargeCodeGroupList.Codes.Freight;

			AccChargeCode oRG = Factory.New<AccChargeCode>();
			oRG.AC_Code = "ORG";
			oRG.AC_ChargeGroup = IncoTermChargeCodeGroupList.Codes.Origin;

			OrgHeader org = OrgHeader.New(Factory);
			OrgLandedCostingPrefs prefs = org.LandedCostingPreferences.AddNew();
			OrgLandedCostingPrefCharges charges = prefs.Charges.AddNew();
			charges.O0_Excluded = true;
			charges.O0_ChargeGroup = IncoTermChargeCodeGroupList.Codes.Freight;
			charges.O0_AC_ChargeCode = fRT.PK;
			AssertNoErrors("No errors as FRT is in the Freight charge code group", charges.O0_AC_ChargeCodeInfo);

			charges.O0_AC_ChargeCode = oRG.PK;
			AssertHasErrors("Errors as ORG is NOT in the Freight charge code group", charges.O0_AC_ChargeCodeInfo);

			charges.O0_Excluded = false;
			AssertNoErrors("No errors as we are including the ORG charge with the Freight charge code group", charges.O0_AC_ChargeCodeInfo);

			charges.O0_AC_ChargeCode = fRT.PK;
			AssertHasErrors("Errors as FRT is already in the Freight charge code group so can't be included", charges.O0_AC_ChargeCodeInfo);
		}

		#endregion

		#region Exclude Charge Code

		public void TestExcludeChargeCode()
		{
			AccChargeCode fRT = Factory.New<AccChargeCode>();
			fRT.AC_Code = "FRT";
			fRT.AC_ChargeGroup = IncoTermChargeCodeGroupList.Codes.Freight;

			OrgHeader org = OrgHeader.New(Factory);
			OrgLandedCostingPrefs prefs = org.LandedCostingPreferences.AddNew();
			OrgLandedCostingPrefCharges charges = prefs.Charges.AddNew();
			charges.O0_ChargeGroup = IncoTermChargeCodeGroupList.Codes.Freight;
			charges.O0_AC_ChargeCode = fRT.PK;
			charges.O0_Excluded = true;
			AssertNoErrors("No errors on exclude flag as group specified", charges.O0_ExcludedInfo);

			charges.O0_ChargeGroup = "";
			AssertHasErrors("Error on exclude flag as group is not specified", charges.O0_ExcludedInfo);

			charges.O0_Excluded = false;
			AssertNoErrors("No errors on exclude flag as exclude flag is not ticked", charges.O0_ExcludedInfo);

			charges.O0_ChargeGroup = IncoTermChargeCodeGroupList.Codes.Freight;
			charges.O0_AC_ChargeCode = ZGuid.Empty;
			charges.O0_Excluded = true;
			AssertHasErrors("Error on exclude flag as charge code not specified", charges.O0_ExcludedInfo);

			charges.O0_AC_ChargeCode = fRT.PK;
			AssertNoErrors("No errors on exclude flag as charge code specified", charges.O0_ExcludedInfo);
		}

		#endregion

		#region Charge Group

		public void TestChargeGroup()
		{
			OrgHeader org = OrgHeader.New(Factory);
			OrgLandedCostingPrefs prefs = org.LandedCostingPreferences.AddNew();
			OrgLandedCostingPrefCharges charges = prefs.Charges.AddNew();
			charges.O0_ChargeGroup = IncoTermChargeCodeGroupList.Codes.Freight;
			AssertNoErrors("No errors on charge group as valid group specified", charges.O0_ChargeGroupInfo);

			charges.O0_ChargeGroup = "";
			AssertNoErrors("No errors on charge group as it is not mandatory", charges.O0_ChargeGroupInfo);

			charges.O0_ChargeGroup = ChargeCodeGroupList.Codes.CFSLoadList;
			AssertHasErrors("Errors on charge group as it is not a valid INCOTERM charge group", charges.O0_ChargeGroupInfo);
		}

		#endregion

		#region Uniqueness Across Groups

		public void TestIfExcludedWithOtherChargeNotExcludedPassesValidation()
		{
			AccChargeCode fRT = Factory.New<AccChargeCode>();
			fRT.AC_Code = "FRT";
			fRT.AC_ChargeGroup = IncoTermChargeCodeGroupList.Codes.Freight;

			AccChargeCode bAF = Factory.New<AccChargeCode>();
			bAF.AC_Code = "BAF";
			bAF.AC_ChargeGroup = IncoTermChargeCodeGroupList.Codes.Freight;

			OrgHeader org = OrgHeader.New(Factory);
			OrgLandedCostingPrefs prefs1 = org.LandedCostingPreferences.AddNew();
			OrgLandedCostingPrefs prefs2 = org.LandedCostingPreferences.AddNew();

			OrgLandedCostingPrefCharges charges1 = prefs1.Charges.AddNew();
			charges1.O0_ChargeGroup = IncoTermChargeCodeGroupList.Codes.Freight;
			charges1.O0_AC_ChargeCode = fRT.PK;
			charges1.O0_Excluded = true;

			OrgLandedCostingPrefCharges charges2 = prefs1.Charges.AddNew();
			charges2.O0_ChargeGroup = IncoTermChargeCodeGroupList.Codes.Freight;
			charges2.O0_AC_ChargeCode = bAF.PK;
			charges2.O0_Excluded = true;

			OrgLandedCostingPrefCharges charges3 = prefs2.Charges.AddNew();
			charges3.O0_AC_ChargeCode = fRT.PK;

			charges3.Validation.ValidateAll();
			AssertNoErrors(charges3);
		}

		public void TestEachChargeExistsInOneLandedCostGroupOnly()
		{
			AccChargeCode fRT = Factory.New<AccChargeCode>();
			fRT.AC_Code = "FRT";
			fRT.AC_ChargeGroup = IncoTermChargeCodeGroupList.Codes.Freight;

			OrgHeader org = OrgHeader.New(Factory);
			OrgLandedCostingPrefs prefs1 = org.LandedCostingPreferences.AddNew();
			OrgLandedCostingPrefs prefs2 = org.LandedCostingPreferences.AddNew();

			OrgLandedCostingPrefCharges charges1 = prefs1.Charges.AddNew();
			charges1.O0_ChargeGroup = IncoTermChargeCodeGroupList.Codes.Freight;

			OrgLandedCostingPrefCharges charges2 = prefs2.Charges.AddNew();
			charges2.O0_ChargeGroup = IncoTermChargeCodeGroupList.Codes.Origin;
			charges2.O0_Excluded = false;
			charges2.O0_AC_ChargeCode = fRT.PK;

			charges2.Validation.ValidateAll();
			Assert("Errors on charge as this charge is included in the first group", charges2.HasRowErrors);

			charges1.O0_Excluded = true;
			charges1.O0_AC_ChargeCode = fRT.PK;

			charges2.Validation.ValidateAll();
			Assert("Error cleared on charge as first group now excludes this same charge", !charges2.HasRowErrors);
		}

		#endregion
	}
}
