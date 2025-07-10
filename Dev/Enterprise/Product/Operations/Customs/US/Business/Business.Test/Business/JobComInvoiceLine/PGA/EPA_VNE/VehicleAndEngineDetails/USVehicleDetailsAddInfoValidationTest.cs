using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	internal class USVehicleDetailsAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestIsVehicleDetailsRequired_SomeVehicleDetailsEntered()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var vehicle = invoiceLine.VehicleLines.AddNew();
			var details = vehicle.VehicleAndEngineDetails.AddNew();
			details.US_IdentityNumber = "123456";

			vehicle.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_1;
			AssertVehicleDetailsHasNoEnteredError(details);

			vehicle.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_21;
			AssertVehicleDetailsHasNoEnteredError(details);
		}

		void AssertVehicleDetailsHasNoEnteredError(VehicleDetails details)
		{
			var expectedMessage = "You have not entered a value.";

			details.US_EngineBuildDate = ZDateTime.BrettsBirthday;
			details.US_EngineManufacturer = "TST";
			details.US_EngineNumber = "TST";
			details.US_EngineModel = "TST";

			details.AddInfoValidation.ValidateAll();

			CombineAssertions(() =>
			{
				AssertHasMessageError("BuildMonth", details.US_BuildMonthInfo, expectedMessage);
				AssertHasMessageError("BuildYear", details.US_BuildYearInfo, expectedMessage);
				AssertHasMessageError("IdentityNumberQualifier", details.US_IdentityNumberQualifierInfo, USVehicleDetailsAddInfoValidation.QualifierRequired);
			});

			details.US_EngineBuildDateInfo.ClearValue();
			details.US_EngineManufacturerInfo.ClearValue();
			details.US_EngineNumberInfo.ClearValue();
			details.US_EngineModelInfo.ClearValue();

			details.AddInfoValidation.ValidateAll();

			CombineAssertions(() =>
			{
				AssertHasMessageError("BuildMonth", details.US_BuildMonthInfo, expectedMessage);
				AssertHasMessageError("BuildYear", details.US_BuildYearInfo, expectedMessage);
				AssertHasMessageError("IdentityNumberQualifier", details.US_IdentityNumberQualifierInfo, USVehicleDetailsAddInfoValidation.QualifierRequired);
			});
		}

		public void TestIsVehicleDetailsRequired_AllEngineDetailsEntered()
		{
			var expectedMessage = "You have not entered a value.";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();

			var vehicle = invoiceLine.VehicleLines.AddNew();
			vehicle.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_21;

			var details = vehicle.VehicleAndEngineDetails.AddNew();
			details.AddInfoValidation.ValidateAll();

			CombineAssertions(() =>
			{
				AssertNoMessageError("BuildMonth", details.US_BuildMonthInfo, expectedMessage);
				AssertNoMessageError("BuildYear", details.US_BuildYearInfo, expectedMessage);
				AssertNoMessageError("IdentityNumberQualifier", details.US_IdentityNumberQualifierInfo, USVehicleDetailsAddInfoValidation.QualifierRequired);
			});

			vehicle.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_1;
			details.AddInfoValidation.ValidateAll();

			CombineAssertions(() =>
			{
				AssertHasMessageError("BuildMonth", details.US_BuildMonthInfo, expectedMessage);
				AssertHasMessageError("BuildYear", details.US_BuildYearInfo, expectedMessage);
				AssertHasMessageError("IdentityNumberQualifier", details.US_IdentityNumberQualifierInfo, USVehicleDetailsAddInfoValidation.QualifierRequired);
			});

			details.US_EngineBuildDate = ZDateTime.BrettsBirthday;
			details.US_EngineManufacturer = "TST";
			details.US_EngineNumber = "TST";
			details.US_EngineModel = "TST";
			details.AddInfoValidation.ValidateAll();

			CombineAssertions(() =>
			{
				AssertNoMessageError("BuildMonth", details.US_BuildMonthInfo, expectedMessage);
				AssertNoMessageError("BuildYear", details.US_BuildYearInfo, expectedMessage);
				AssertNoMessageError("IdentityNumberQualifier", details.US_IdentityNumberQualifierInfo, USVehicleDetailsAddInfoValidation.QualifierRequired);
			});
		}

		[TestDate(2014, 01, 01)]
		public void TestVINForm3520_1()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var vehicle = invoiceLine.VehicleLines.AddNew();
			var details = vehicle.VehicleAndEngineDetails.AddNew();

			details.US_IdentityNumberQualifier = "!";
			AssertHasMessageErrorContaining(details.US_IdentityNumberQualifierInfo, ListValidation.InvalidCodeMessageError);
			details.US_BuildMonth = "G";
			AssertHasMessageErrorContaining(details.US_BuildMonthInfo, ListValidation.InvalidCodeMessageError);

			details.US_BuildYear = "A002";
			AssertHasMessageErrorContaining(details.US_BuildYearInfo, USVehicleDetailsAddInfoValidation.YearFormat);

			details.US_BuildYear = "2002";
			AssertNoMessageErrorContaining(details.US_BuildYearInfo, USVehicleDetailsAddInfoValidation.YearFormat);

			details.US_BuildYear = "2015";
			AssertHasMessageErrorContaining(details.US_BuildYearInfo, USVehicleDetailsAddInfoValidation.YearInFuture);

			details.US_BuildYear = "2013";
			AssertNoMessageErrorContaining(details.US_BuildYearInfo, USVehicleDetailsAddInfoValidation.YearInFuture);

			details.US_BuildYear = "1899";
			AssertHasMessageErrorContaining(details.US_BuildYearInfo, USVehicleDetailsAddInfoValidation.YearInPast);

			details.US_BuildYear = "1900";
			AssertNoMessageErrorContaining(details.US_BuildYearInfo, USVehicleDetailsAddInfoValidation.YearInPast);

			vehicle.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_1;

			details.AddInfoValidation.ValidateUS_IdentityNumber();
			details.US_IdentityNumberQualifier = ZString.Empty;
			AssertHasMessageErrorContaining(details.US_IdentityNumberInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(details.US_IdentityNumberQualifierInfo, USVehicleDetailsAddInfoValidation.QualifierRequired);

			details.US_IdentityNumberQualifier = ItemIdentityNumberQualifierList.Codes.EngineNumber;
			AssertHasMessageErrorContaining(details.US_IdentityNumberQualifierInfo, USVehicleDetailsAddInfoValidation.ShouldBeVIN);

			details.US_IdentityNumberQualifier = ItemIdentityNumberQualifierList.Codes.SerialNumber;
			AssertNoMessageErrorContaining(details.US_IdentityNumberQualifierInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(details.US_IdentityNumberQualifierInfo, USVehicleDetailsAddInfoValidation.ShouldBeVIN);
			AssertNoMessageErrorContaining(details.US_IdentityNumberQualifierInfo, USVehicleDetailsAddInfoValidation.QualifierRequired);

			details.US_IdentityNumberQualifier = ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN;
			AssertNoMessageErrorContaining(details.US_IdentityNumberQualifierInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(details.US_IdentityNumberQualifierInfo, USVehicleDetailsAddInfoValidation.ShouldBeVIN);
			AssertNoMessageErrorContaining(details.US_IdentityNumberQualifierInfo, USVehicleDetailsAddInfoValidation.QualifierRequired);

			details.US_IdentityNumber = "123456";
			details.US_BuildYear = "2014";
			AssertNoMessageErrorContaining(details.US_IdentityNumberInfo, MandatoryValidation.YouHaveNotEntered);
			details.US_BuildMonth = ZString.Empty;
			AssertHasMessageErrorContaining(details.US_BuildMonthInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(details.US_BuildYearInfo, MandatoryValidation.YouHaveNotEntered);

			details.US_BuildMonth = MonthList.Codes._08;
			details.US_BuildYear = ZString.Empty;
			AssertNoMessageErrorContaining(details.US_BuildMonthInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(details.US_BuildMonthInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(details.US_BuildYearInfo, MandatoryValidation.YouHaveNotEntered);

			details.US_IdentityNumberQualifier = ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN;
			details.US_IdentityNumber = "0123";
			AssertHasMessageErrorContaining(details.US_IdentityNumberInfo, VNEAdditionalNumberValidation.NumberLengthText);

			details.US_IdentityNumber = "01234567891234567";
			AssertNoMessageErrorContaining(details.US_IdentityNumberInfo, VNEAdditionalNumberValidation.NumberLengthText);
		}

		public void TestUS_EngineNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var vehicle = invoiceLine.VehicleLines.AddNew();
			vehicle.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_1;
			var details = vehicle.VehicleAndEngineDetails.AddNew();

			var additionalNumber2 = details.AdditionalNumbers.AddNew();
			additionalNumber2.CY_Code = ItemIdentityNumberQualifierList.Codes.EngineNumber;
			additionalNumber2.CY_Data = "E0000001";

			details.US_EngineNumber = "E0000002";
			AssertNoMessageError(details.US_EngineNumberInfo, USVehicleDetailsAddInfoValidation.EngineNumberMessageText);

			details.US_EngineNumber = "";
			AssertHasMessageError(details.US_EngineNumberInfo, USVehicleDetailsAddInfoValidation.EngineNumberMessageText);
		}

		public void TestForm3520_21()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var vehicle = invoiceLine.VehicleLines.AddNew();
			vehicle.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_21;
			var details = vehicle.VehicleAndEngineDetails.AddNew();
			details.US_IdentityNumber = "123456";
			details.AddInfoValidation.ValidateAll();
			AssertHasMessageErrorContaining(details.US_EngineManufacturerInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(details.US_EngineModelInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(details.US_EngineNumberInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(details.US_EngineBuildDateInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(details.US_IdentityNumberInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(details.US_IdentityNumberQualifierInfo, USVehicleDetailsAddInfoValidation.QualifierRequired);

			details.US_EngineManufacturer = "test";
			AssertNoMessageErrorContaining(details.US_EngineManufacturerInfo, MandatoryValidation.YouHaveNotEntered);
			details.US_EngineModel = "test";
			AssertNoMessageErrorContaining(details.US_EngineModelInfo, MandatoryValidation.YouHaveNotEntered);
			details.US_EngineNumber = "34524234234";
			AssertNoMessageErrorContaining(details.US_EngineNumberInfo, MandatoryValidation.YouHaveNotEntered);
			details.US_EngineBuildDate = ZDateTime.Today;
			AssertNoMessageErrorContaining(details.US_EngineBuildDateInfo, MandatoryValidation.YouHaveNotEntered);

			details.US_IdentityNumberQualifier = ItemIdentityNumberQualifierList.Codes.SerialNumber;
			AssertNoMessageErrorContaining(details.US_IdentityNumberQualifierInfo, USVehicleDetailsAddInfoValidation.QualifierRequired);

			details.US_EngineNumber = ZString.Empty;
			details.US_IdentityNumber = ZString.Empty;
			AssertHasMessageErrorContaining(details.US_IdentityNumberInfo, MandatoryValidation.YouHaveNotEntered);

			details.US_MfrDateType = "XXX";
			AssertHasMessageErrorContaining(details.US_MfrDateTypeInfo, ListValidation.InvalidCodeMessageError);

			details.US_MfrDateType = ManufactureDateTypeList.Codes.OTH;
			AssertHasMessageErrorContaining(details.US_BuildDateExplanationInfo, MandatoryValidation.YouHaveNotEntered);

			details.US_MfrDateType = ManufactureDateTypeList.Codes.ENG;
			details.US_EngineBuildDate = ZDateTime.Empty;
			AssertHasMessageErrorContaining(details.US_EngineBuildDateInfo, MandatoryValidation.YouHaveNotEntered);

			details.US_MfrDateType = ManufactureDateTypeList.Codes.VEH;
			details.US_BuildYear = ZString.Empty;
			details.US_BuildMonth = ZString.Empty;
			AssertHasMessageErrorContaining(details.US_BuildYearInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(details.US_BuildMonthInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_MfrDateType()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var vehicle = invoiceLine.VehicleLines.AddNew();
			vehicle.US_FormType = EPAVNEDocumentIdentifierList.Codes.EPA3520_21;
			var details = vehicle.VehicleAndEngineDetails.AddNew();

			details.US_MfrDateType = ZString.Empty;
			AssertHasMessageErrorContaining(details.US_MfrDateTypeInfo, MandatoryValidation.YouHaveNotEntered);

			vehicle.US_EnginePower = 0m;
			details.US_MfrDateType = "ENG";
			AssertNoMessageErrorContaining(details.US_MfrDateTypeInfo, MandatoryValidation.YouHaveNotEntered);
		}
	}
}
