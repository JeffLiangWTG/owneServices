using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class UNDGCountryReferencePivotValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckDCP_StorageInstruction()
		{
			var errorMessage = "Enter a valid Storage Instruction Category.";
			var substance = DGSubstanceTestHelper.Create("0000", "A", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO);
			var countryReference = Factory.New<UNDGCountryReference>();
			countryReference.DCR_Type = Core.Constants.UNDGCountryReference.Type.ICPE;
			countryReference.DCR_RN_NKCountry = Core.Constants.CountryCodes.France;
			countryReference.DCR_Code = "1111";

			var pivot = Factory.New<UNDGCountryReferencePivot>();
			pivot.DCP_DCR = countryReference.PK;
			pivot.DCP_Standard = substance.DG_Standard;
			pivot.DCP_Variant = substance.DG_Variant;
			pivot.DCP_UNNO = substance.DG_UNNO;

			pivot.DCP_StorageInstruction = "XXX";
			AssertHasError(pivot.DCP_StorageInstructionInfo, errorMessage);

			pivot.DCP_StorageInstruction = StorageInstructionList.Codes.ToBeConfirmedBySafetyDataSheet;
			AssertNoError(pivot.DCP_StorageInstructionInfo, errorMessage);
		}
	}
}
