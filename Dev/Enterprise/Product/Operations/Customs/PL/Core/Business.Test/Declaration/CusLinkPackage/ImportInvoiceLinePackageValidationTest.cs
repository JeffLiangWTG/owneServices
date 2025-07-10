using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

sealed class ImportInvoiceLinePackageValidationTest : InvoiceLinePackageValidationTest
{
	public void TestCheckForRuleR491()
	{
		const string messageError = "(R491) – For the requested procedure code, number of packs > 0 is required for the Entry Instruction.";
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		invoiceLine.JI_CEI = entryInstruction.PK;

		CombineAssertions(() =>
		{
			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._71;
			linePackage.Validation.ValidatePackQty();
			AssertNoMessageError("CEI_Procedure is 71", linePackage.PackQtyInfo, messageError);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._76;
			linePackage.Validation.ValidatePackQty();
			AssertNoMessageError("CEI_Procedure is 76", linePackage.PackQtyInfo, messageError);

			entryInstruction.CEI_Procedure = Constants.ProcedureCodes._63;
			linePackage.Validation.ValidatePackQty();
			AssertHasMessageError("CEI_Procedure is 63", linePackage.PackQtyInfo, messageError);

			linePackage.IsLinked = false;
			linePackage.Validation.ValidatePackQty();
			AssertNoMessageError("Package is not linked", linePackage.PackQtyInfo, messageError);

			linePackage.IsLinked = true;
			basePackage.CW_PackType = "VQ";
			linePackage.Validation.ValidatePackQty();
			AssertNoMessageError("PackType is Bulk code", linePackage.PackQtyInfo, messageError);

			basePackage.CW_PackType = "AA";
			linePackage.PackQty = 1;
			linePackage.Validation.ValidatePackQty();
			AssertNoMessageError("CEI_Procedure is 63 and CHC_NumberOfPacks = 0", linePackage.PackQtyInfo, messageError);
		});
	}

	void SetupPackageTypes()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "A");
		helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
			"VO", "VO", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, "");
		helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
			"NE", "NE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.BreakBulk, "");
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations);

		Factory.Save();
	}

	protected override void SetUp()
	{
		SetupPackageTypes();

		base.SetUp();
		declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
	}
}
