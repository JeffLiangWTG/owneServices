using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Manifest.Business.Testing;

[TestedType(typeof(PreviousDocumentValidation))]
sealed class PreviousDocumentValidationTest : BusinessObjectValidationTestCase
{
	public void TestGoodsRegistrationNumberMandatoryValidation()
	{
		CombineAssertions(() =>
		{
			var previousDocument = Factory.New<PreviousDocument>();
			previousDocument.CSI_Code = "N820";
			AssertNoMessageErrorContaining("When CSI_Code is not empty", previousDocument.CSI_CodeInfo, MandatoryValidation.YouHaveNotEntered);

			previousDocument.CSI_Code = ZString.Empty;
			AssertHasMessageErrorContaining("When CSI_Code is empty", previousDocument.CSI_CodeInfo, MandatoryValidation.YouHaveNotEntered);
		});
	}

	public void TestCheckCSI_Code()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var previousDocumentType = NO.Business.UniversalReferenceConstants.RefCusCodeListType.PreviousDocumentsForManifestBill;
		helper.CreateNewOrGetExistingCusCodeType(previousDocumentType, "Previous Documents");
		helper.CreateCusCodeList(Core.Constants.CountryCodes.Norway, previousDocumentType, "N820", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));
		helper.CreateCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, previousDocumentType, "AAA", ZDateTime.Today.AddMonths(-1), ZDateTime.Today.AddMonths(3));
		Factory.Save();

		var previousDocument = Factory.New<PreviousDocument>();
		CombineAssertions(() =>
		{
			previousDocument.CSI_Code = "N820";
			AssertNoMessageErrors("No message errors when code is in the Norway list", previousDocument.CSI_CodeInfo);
			previousDocument.CSI_Code = "AAA";
			AssertNoMessageErrors("No message errors when code is in the European list", previousDocument.CSI_CodeInfo);
			previousDocument.CSI_Code = "XYZ";
			AssertHasMessageErrorContaining(previousDocument.CSI_CodeInfo, ListValidation.InvalidCodeMessageError);
		});
	}

	public void TestCSI_ReferenceNumberFormat()
	{
		const string expectedMessageError = "Document number for type CUDE requires the following structure: Declarant(9)-Date(8 YYYYMMDD)-Sequence(1-6 long). Example: 123456789-20240928-123";
		var bill = Factory.New<AsycudaBill>();
		var previousDocument = bill.PreviousDocuments.AddNew();
		var validation = previousDocument.Validation;

		CombineAssertions(() =>
		{
			bill.ABL_BolType = AsycudaBill.ChildBolCode;
			previousDocument.CSI_Code = PreviousDocumentConstants.Codes.CUDE;
			previousDocument.CSI_ReferenceNumber = "123456-12-1";
			validation.ValidateCSI_ReferenceNumber();
			AssertNoMessageError($"When HouseBill: {bill.IsHouseBill}, CSI_Code: {previousDocument.CSI_Code} and wrong format entered",
				previousDocument.CSI_ReferenceNumberInfo, expectedMessageError);

			bill.ABL_BolType = Core.Constants.ShipmentTypes.StandardHouse;
			previousDocument.CSI_Code = "XYZ";
			validation.ValidateCSI_ReferenceNumber();
			AssertNoMessageError($"When HouseBill: {bill.IsHouseBill}, CSI_Code: {previousDocument.CSI_Code} and wrong format entered",
				previousDocument.CSI_ReferenceNumberInfo, expectedMessageError);

			previousDocument.CSI_Code = PreviousDocumentConstants.Codes.CUDE;
			validation.ValidateCSI_ReferenceNumber();
			AssertHasMessageError($"When HouseBill: {bill.IsHouseBill}, CSI_Code: {previousDocument.CSI_Code} and wrong format entered",
				previousDocument.CSI_ReferenceNumberInfo, expectedMessageError);

			previousDocument.CSI_ReferenceNumber = "123456789-20241201-123";
			validation.ValidateCSI_ReferenceNumber();
			AssertNoMessageError($"When HouseBill: {bill.IsHouseBill}, CSI_Code: {previousDocument.CSI_Code} and correct format entered",
				previousDocument.CSI_ReferenceNumberInfo, expectedMessageError);

			previousDocument.CSI_ReferenceNumber = "123456789-20241232-123";
			validation.ValidateCSI_ReferenceNumber();
			AssertHasMessageError($"When HouseBill: {bill.IsHouseBill}, CSI_Code: {previousDocument.CSI_Code} and wrong format (wrong date) entered",
				previousDocument.CSI_ReferenceNumberInfo, expectedMessageError);
		});
	}
}
