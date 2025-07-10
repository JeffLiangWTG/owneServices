using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

class NctsBillAdditionalDocumentPhase5ValidationTest : TestCaseWithFactory
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

	public void TestCSI_Code_HouseConsignmentLevel()
	{
		SetupCusCodeList("POW01", "PCS01");
		const string errorMessage = "[RP51] The POW01/PCS01 code should be declared only at the Consignment level.";

		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.BH_HeaderType = NctsMovementType.Codes.Departure;
		var bill = nctsHeader.Bills.AddNew();
		var billAdditionalDocument = bill.AdditionalDocuments.AddNew();

		billAdditionalDocument.CSI_SubType = AdditionalInfoKindList.Codes.INF;
		billAdditionalDocument.CSI_Code = Constants.AdditionalInfoCodes._PCS01;
		billAdditionalDocument.Validation.ValidateCSI_Code();
		AssertHasMessageError("PCS01 declared at the goods item level", billAdditionalDocument.CSI_CodeInfo, errorMessage);

		billAdditionalDocument.CSI_Code = Constants.AdditionalInfoCodes._POW01;
		billAdditionalDocument.Validation.ValidateCSI_Code();
		AssertHasMessageError("POW01 declared at the goods item level", billAdditionalDocument.CSI_CodeInfo, errorMessage);
	}
}
