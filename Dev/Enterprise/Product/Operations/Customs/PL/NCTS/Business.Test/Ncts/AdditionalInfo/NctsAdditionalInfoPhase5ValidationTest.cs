using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

class NctsAdditionalInfoPhase5ValidationTest : TestCaseWithFactory
{
	void SetupCusCodeList(params string[] codesToCreate)
	{
		const string codeType = "AI44N";
		var countryCodePL = Core.Constants.CountryCodes.Poland;
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var groupingEUN = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);
		helper.CreateNewOrGetExistingDataGrouping(countryCodePL, parent: groupingEUN);
		helper.CreateNewOrGetExistingCusCodeType(codeType, $"{codeType} code type");

		foreach (var codeToCreate in codesToCreate)
		{
			var code = helper.CreateCusCodeList(countryCodePL, codeType, codeToCreate, $"{codeToCreate} code", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(code.PK, "Level", "Item");
		}
		Factory.Save();
	}

	public void TestCSI_DescriptionRuleRP51()
	{
		const string errorMessage = "[RP51] Description is required for Additional Information Type 'POW01' or 'PCS01' and must contain IDSISC or email address";

		var (_, additionalInfoHeader) = CreateDepartureData(Factory, phase: CusInBondApplicationCodeList.Codes.NCTS5);

		additionalInfoHeader.CSI_SubType = AdditionalInfoKindList.Codes.REF;
		additionalInfoHeader.CSI_Code = Constants.AdditionalInfoCodes._POW01;
		AssertNoMessageError("CSI SubType is REF", additionalInfoHeader.CSI_DescriptionInfo, errorMessage);

		additionalInfoHeader.CSI_SubType = AdditionalInfoKindList.Codes.INF;
		additionalInfoHeader.CSI_Code = Constants.AdditionalInfoCodes._POW01;
		additionalInfoHeader.CSI_Description = string.Empty;
		AssertHasMessageError("CSI SubType is INF and CSI Code is POW01 and Description is empty",
		additionalInfoHeader.CSI_DescriptionInfo, errorMessage);

		additionalInfoHeader.CSI_Code = Constants.AdditionalInfoCodes._PCS01;
		AssertHasMessageError("CSI SubType is INF and CSI Code is PCS01 and Description is empty",
		additionalInfoHeader.CSI_DescriptionInfo, errorMessage);

		additionalInfoHeader.CSI_Description = "valid@email.com";
		AssertNoMessageError("Valid description provided", additionalInfoHeader.CSI_DescriptionInfo, errorMessage);
	}

	public void TestCSI_Code_GoodsLevel()
	{
		SetupCusCodeList("POW01", "PCS01");
		const string errorMessage = "[RP51] The POW01/PCS01 code should be declared only at the Consignment level.";

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		var bill = nctsHeader.Bills.AddNew();
		var goodsItem = bill.GoodsItems.AddNew();
		var additionalInfo = goodsItem.AdditionalInfos.AddNew();

		additionalInfo.CSI_SubType = AdditionalInfoKindList.Codes.INF;
		additionalInfo.CSI_Code = Constants.AdditionalInfoCodes._PCS01;
		additionalInfo.Validation.ValidateCSI_Code();
		AssertHasMessageError("PCS01 declared at the goods item level", additionalInfo.CSI_CodeInfo, errorMessage);

		additionalInfo.CSI_Code = Constants.AdditionalInfoCodes._POW01;
		additionalInfo.Validation.ValidateCSI_Code();
		AssertHasMessageError("POW01 declared at the goods item level", additionalInfo.CSI_CodeInfo, errorMessage);
	}

	public void TestDuplicatePOW01_PCS01_AtConsignmentLevel()
	{
		SetupCusCodeList("POW01", "PCS01");
		const string errorMessage = "[RP51] The declaration may contain only one occurrence of either 'POW01' or 'PCS01' code.";
		var (nctsHeader, additionalInfo1) = CreateDepartureData(Factory, phase: CusInBondApplicationCodeList.Codes.NCTS5);

		var additionalInfo2 = nctsHeader.AdditionalDocuments.AddNew();

		additionalInfo1.CSI_SubType = AdditionalInfoKindList.Codes.INF;
		additionalInfo1.CSI_Code = Constants.AdditionalInfoCodes._POW01;
		AssertNoMessageError("Single POW01 code", additionalInfo1.CSI_CodeInfo, errorMessage);

		additionalInfo2.CSI_SubType = AdditionalInfoKindList.Codes.INF;
		additionalInfo2.CSI_Code = Constants.AdditionalInfoCodes._POW01;
		AssertHasMessageError("Duplicate POW01 codes", additionalInfo2.CSI_CodeInfo, errorMessage);

		additionalInfo2.CSI_Code = Constants.AdditionalInfoCodes._PCS01;
		additionalInfo1.Validation.ValidateCSI_Code();
		AssertHasMessageError("Mixed POW01 and PCS01 codes", additionalInfo1.CSI_CodeInfo, errorMessage);

		additionalInfo1.CSI_Code = "OTHER_CODE";
		additionalInfo2.CSI_Code = Constants.AdditionalInfoCodes._POW01;
		AssertNoMessageError("Different code types", additionalInfo1.CSI_CodeInfo, errorMessage);
	}

	static (NctsHeader nctsHeader, NctsAdditionalInfo additionalInfoHeader)
		CreateDepartureData(BusinessObjectFactory factory, string phase = CusInBondApplicationCodeList.Codes.NCTS5)
	{
		var nctsHeader = factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		nctsHeader.BH_ApplicationCode = phase;
		var additionalInfoHeader = nctsHeader.AdditionalDocuments.AddNew();
		var bill = nctsHeader.Bills.AddNew();
		return (nctsHeader, additionalInfoHeader);
	}
}

