using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	public class USNHTSADetailsAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestVINIsMandatoryForAllVehicles()
		{
			DetailsLine.Header.US_NHTProgramCode = NHTSAProgramCodeList.Codes.MVS;
			DetailsLine.Validation.ValidateAll();
			AssertHasRowMessageErrorContaining(DetailsLine, ValidationConstants.NHTSA.VINIsMandatoryForAllVehicles);

			DetailsLine.US_NHTIdentityNumQualifier = ItemIdentityNumberQualifierList.Codes.EngineNumber;
			DetailsLine.Validation.ValidateAll();
			AssertHasRowMessageErrorContaining(DetailsLine, ValidationConstants.NHTSA.VINIsMandatoryForAllVehicles);

			DetailsLine.US_NHTIdentityNumQualifier = ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN;
			DetailsLine.Validation.ValidateAll();
			AssertHasRowMessageErrorContaining(DetailsLine, ValidationConstants.NHTSA.VINIsMandatoryForAllVehicles);

			DetailsLine.US_NHTIdentityNumber = "ZFFVA40B00000000";
			DetailsLine.Validation.ValidateAll();
			AssertNoRowMessageErrorContaining(DetailsLine, ValidationConstants.NHTSA.VINIsMandatoryForAllVehicles);

			DetailsLine.AdditionalNumbers.RemoveAndDeleteAll();
			DetailsLine.Header.US_NHTProgramCode = NHTSAProgramCodeList.Codes.REI;
			DetailsLine.Validation.ValidateAll();
			AssertNoRowMessageErrorContaining(DetailsLine, ValidationConstants.NHTSA.VINIsMandatoryForAllVehicles);
		}

		public void TestCheckUS_NHTIdentityNumQualifier()
		{
			DetailsLine.Header.US_NHTProgramCode = NHTSAProgramCodeList.Codes.MVS;
			DetailsLine.US_NHTIdentityNumQualifier = ZString.Empty;
			AssertHasMessageErrorContaining(DetailsLine.US_NHTIdentityNumQualifierInfo, MandatoryValidation.YouHaveNotEntered);

			DetailsLine.US_NHTIdentityNumQualifier = "XXX";
			AssertHasMessageErrorContaining(DetailsLine.US_NHTIdentityNumQualifierInfo, ListValidation.InvalidCodeMessageError);

			DetailsLine.US_NHTIdentityNumQualifier = ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN;
			AssertNoMessageErrors(DetailsLine.US_NHTIdentityNumQualifierInfo);
		}

		public void TestCheckUS_NHTIdentityNumber()
		{
			DetailsLine.Header.US_NHTProgramCode = NHTSAProgramCodeList.Codes.MVS;
			DetailsLine.Header.US_NHTBoxNumber = DepartmentOfTransportBoxNumberList.Codes._01;
			DetailsLine.US_NHTIdentityNumQualifier = ItemIdentityNumberQualifierList.Codes.ModelNumber;
			AssertHasWarningContaining(DetailsLine.US_NHTIdentityNumberInfo, MandatoryValidation.YouHaveNotEntered);

			DetailsLine.US_NHTIdentityNumber = "123456";
			AssertNoWarnings(DetailsLine.US_NHTIdentityNumberInfo);
			AssertNoMessageErrors(DetailsLine.US_NHTIdentityNumberInfo);

			DetailsLine.US_NHTIdentityNumQualifier = ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN;
			AssertHasWarningContaining(DetailsLine.US_NHTIdentityNumberInfo, ValidationConstants.NHTSA.InvalidAdditionalNumberFormat);

			DetailsLine.US_NHTIdentityNumber = "ZFFVA40B000000001";
			AssertNoMessageErrors(DetailsLine.US_NHTIdentityNumberInfo);

			DetailsLine.Header.US_NHTProgramCode = NHTSAProgramCodeList.Codes.MVS;
			DetailsLine.AdditionalNumbers.AddNew().US_NHTAdditionalIdentityNumQualifier = ItemIdentityNumberQualifierList.Codes.VehicleIdentificationNumberVIN;
			DetailsLine.Header.US_NHTBoxNumber = DepartmentOfTransportBoxNumberList.Codes._2B;
			DetailsLine.US_NHTIdentityNumber = "!!!@@%^^^&&*";

			DetailsLine.Validation.ValidateUS_NHTIdentityNumber();
			AssertHasMessageErrorContaining(DetailsLine.US_NHTIdentityNumberInfo, ValidationConstants.NHTSA.InvalidAdditionalNumberFormat);
		}

		public void TestCheckUS_NHTLPCOType()
		{
			DetailsLine.US_NHTLPCOType = "XXX";
			AssertHasMessageErrorContaining(DetailsLine.US_NHTLPCOTypeInfo, ListValidation.InvalidCodeMessageError);

			DetailsLine.US_NHTLPCOType = ZString.Empty;
			DetailsLine.US_NHTLPCOQuantity = 12m;
			DetailsLine.Validation.ValidateUS_NHTLPCOType();
			AssertHasWarningContaining(DetailsLine.US_NHTLPCOTypeInfo, MandatoryValidation.YouHaveNotEntered);

			DetailsLine.US_NHTLPCOType = NHTSALPCOTypeList.Codes.NH0;
			AssertNoWarnings(DetailsLine.US_NHTLPCOTypeInfo);
			AssertNoMessageErrors(DetailsLine.US_NHTLPCOTypeInfo);
		}

		public void TestCheckUS_NHTLPCONumber()
		{
			DetailsLine.US_NHTLPCOType = NHTSALPCOTypeList.Codes.NH0;
			DetailsLine.US_NHTLPCONumber = "XXXXX";
			AssertHasMessageErrorContaining(DetailsLine.US_NHTLPCONumberInfo, ValidationConstants.NHTSA.InvalidLPCONumberFormatForRegisteredImporterNumber);

			DetailsLine.US_NHTLPCONumber = "R-90-007";
			AssertNoMessageErrors(DetailsLine.US_NHTLPCONumberInfo);

			DetailsLine.US_NHTLPCOType = NHTSALPCOTypeList.Codes.NH2;
			AssertHasMessageErrorContaining(DetailsLine.US_NHTLPCONumberInfo, ValidationConstants.NHTSA.InvalidLPCONumberFormatForNHTSAImportPermissionLetterr);

			DetailsLine.US_NHTLPCONumber = "07-1401-0001";
			AssertNoMessageErrors(DetailsLine.US_NHTLPCONumberInfo);

			DetailsLine.US_NHTLPCOType = NHTSALPCOTypeList.Codes.NH3;
			AssertHasMessageErrorContaining(DetailsLine.US_NHTLPCONumberInfo, ValidationConstants.NHTSA.InvalidLPCONumberFormatForVehicleEligbilityNumber);

			DetailsLine.US_NHTLPCONumber = "VSA-080";
			AssertNoMessageErrors(DetailsLine.US_NHTLPCONumberInfo);
		}

		public void TestCheckUS_NHTLPCODateType()
		{
			DetailsLine.US_NHTLPCODateType = "X";
			AssertHasMessageErrorContaining(DetailsLine.US_NHTLPCODateTypeInfo, ListValidation.InvalidCodeMessageError);

			DetailsLine.US_NHTLPCODateType = LPCODateQualifierList.Codes.DateApplicationReceived;
			AssertNoMessageErrorContaining(DetailsLine.US_NHTLPCODateTypeInfo, ListValidation.InvalidCodeMessageError);

			DetailsLine.US_NHTLPCODateType = ZString.Empty;
			DetailsLine.US_NHTLPCODate = ZDateTime.Today;
			AssertHasMessageErrorContaining(DetailsLine.US_NHTLPCODateTypeInfo, MandatoryValidation.YouHaveNotEntered);

			DetailsLine.US_NHTLPCODateType = LPCODateQualifierList.Codes.DateApplicationReceived;
			AssertNoMessageErrorContaining(DetailsLine.US_NHTLPCODateTypeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_NHTLPCODate()
		{
			DetailsLine.US_NHTLPCODateType = LPCODateQualifierList.Codes.DateApplicationReceived;
			AssertHasMessageErrorContaining(DetailsLine.US_NHTLPCODateInfo, MandatoryValidation.YouHaveNotEntered);

			DetailsLine.US_NHTLPCODate = ZDateTime.Today;
			AssertNoMessageErrorContaining(DetailsLine.US_NHTLPCODateInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_NHTBrandName()
		{
			Header.US_NHTProgramCode = NHTSAProgramCodeList.Codes.MVS;
			DetailsLine.AddInfo.Validation.ValidateUS_NHTBrandName();
			AssertHasMessageErrorContaining(DetailsLine.US_NHTBrandNameInfo, MandatoryValidation.YouHaveNotEntered);

			DetailsLine.US_NHTBrandName = "ABC";
			AssertNoMessageErrors(DetailsLine.US_NHTBrandNameInfo);

			Header.US_NHTProgramCode = NHTSAProgramCodeList.Codes.OFF;
			DetailsLine.US_NHTBrandName = ZString.Empty;
			AssertNoMessageErrors(DetailsLine.US_NHTBrandNameInfo);
		}

		public void TestCheckUS_NHTModel()
		{
			DetailsLine.AddInfo.Validation.ValidateUS_NHTModel();
			AssertNoMessageErrors(DetailsLine.US_NHTModelInfo);

			Header.US_NHTProgramCode = NHTSAProgramCodeList.Codes.MVS;
			DetailsLine.AddInfo.Validation.ValidateUS_NHTModel();
			AssertHasMessageErrorContaining(DetailsLine.US_NHTModelInfo, MandatoryValidation.YouHaveNotEntered);

			DetailsLine.US_NHTModel = "ABC";
			AssertNoMessageErrors(DetailsLine.US_NHTModelInfo);
		}

		[TestDate(2015, 07, 01)]
		public void TestCheckUS_NHTYearOfMFR()
		{
			DetailsLine.AddInfo.Validation.ValidateUS_NHTYearOfMFR();
			AssertNoMessageErrors(DetailsLine.US_NHTYearOfMFRInfo);

			Header.US_NHTProgramCode = NHTSAProgramCodeList.Codes.MVS;
			Header.US_NHTBoxNumber = DepartmentOfTransportBoxNumberList.Codes._01;
			DetailsLine.AddInfo.Validation.ValidateUS_NHTYearOfMFR();
			AssertHasMessageErrorContaining(DetailsLine.US_NHTYearOfMFRInfo, MandatoryValidation.YouHaveNotEntered);

			Header.US_NHTProgramCode = NHTSAProgramCodeList.Codes.OEI;
			DetailsLine.AddInfo.Validation.ValidateUS_NHTYearOfMFR();
			AssertNoMessageErrorContaining(DetailsLine.US_NHTYearOfMFRInfo, MandatoryValidation.YouHaveNotEntered);

			Header.US_NHTProgramCode = NHTSAProgramCodeList.Codes.OFF;
			DetailsLine.US_NHTYearOfMFR = "2017";
			AssertHasMessageErrorContaining(DetailsLine.US_NHTYearOfMFRInfo, ListValidation.InvalidCodeMessageError);

			DetailsLine.US_NHTYearOfMFR = "2014";
			AssertNoMessageErrors(DetailsLine.US_NHTYearOfMFRInfo);

			Header.US_NHTBoxNumber = DepartmentOfTransportBoxNumberList.Codes._03;
			DetailsLine.US_NHTYearOfMFR = "2017";
			AssertHasMessageErrorContaining(DetailsLine.US_NHTYearOfMFRInfo, ListValidation.InvalidCodeMessageError);

			DetailsLine.US_NHTMonthOfMFR = MonthList.Codes._01;
			DetailsLine.US_NHTYearOfMFR = ZString.Empty;
			AssertHasMessageErrorContaining(DetailsLine.US_NHTYearOfMFRInfo, MandatoryValidation.YouHaveNotEntered);

			DetailsLine.US_NHTYearOfMFR = "2015";
			AssertNoMessageErrors(DetailsLine.US_NHTYearOfMFRInfo);
		}

		public void TestCheckUS_NHTMonthOfMFR()
		{
			DetailsLine.AddInfo.Validation.ValidateUS_NHTMonthOfMFR();
			AssertNoMessageErrors(DetailsLine.US_NHTMonthOfMFRInfo);

			Header.US_NHTProgramCode = NHTSAProgramCodeList.Codes.REI;
			Header.US_NHTBoxNumber = DepartmentOfTransportBoxNumberList.Codes._01;
			DetailsLine.AddInfo.Validation.ValidateUS_NHTMonthOfMFR();
			AssertHasMessageErrorContaining(DetailsLine.US_NHTMonthOfMFRInfo, MandatoryValidation.YouHaveNotEntered);

			Header.US_NHTProgramCode = NHTSAProgramCodeList.Codes.OEI;
			DetailsLine.AddInfo.Validation.ValidateUS_NHTMonthOfMFR();
			AssertNoMessageErrorContaining(DetailsLine.US_NHTMonthOfMFRInfo, MandatoryValidation.YouHaveNotEntered);

			Header.US_NHTProgramCode = NHTSAProgramCodeList.Codes.TPE;
			DetailsLine.US_NHTMonthOfMFR = "XX";
			AssertHasMessageErrorContaining(DetailsLine.US_NHTMonthOfMFRInfo, ListValidation.InvalidCodeMessageError);

			DetailsLine.US_NHTMonthOfMFR = MonthList.Codes._01;
			AssertNoMessageErrors(DetailsLine.US_NHTMonthOfMFRInfo);

			DetailsLine.US_NHTYearOfMFR = "2015";
			DetailsLine.US_NHTMonthOfMFR = ZString.Empty;
			AssertHasMessageErrorContaining(DetailsLine.US_NHTMonthOfMFRInfo, MandatoryValidation.YouHaveNotEntered);

			DetailsLine.US_NHTMonthOfMFR = MonthList.Codes._01;
			AssertNoMessageErrors(DetailsLine.US_NHTMonthOfMFRInfo);
		}

		public void TestCheckUS_NHTCategoryCode()
		{
			DetailsLine.AddInfo.Validation.ValidateUS_NHTCategoryCode();
			AssertHasMessageErrorContaining(DetailsLine.US_NHTCategoryCodeInfo, MandatoryValidation.YouHaveNotEntered);

			DetailsLine.US_NHTCategoryCode = NHTSACategoryCode_MVSTYPList.Codes.MVS1;
			AssertHasMessageErrorContaining(DetailsLine.US_NHTCategoryCodeInfo, ListValidation.InvalidCodeMessageError);

			Header.US_NHTProgramCode = NHTSAProgramCodeList.Codes.MVS;
			DetailsLine.AddInfo.Validation.ValidateUS_NHTCategoryCode();
			AssertNoMessageErrors(DetailsLine.US_NHTCategoryCodeInfo);

			DetailsLine.US_NHTCategoryCode = NHTSACategoryCode_REITYPList.Codes.REI1;
			AssertHasMessageErrorContaining(DetailsLine.US_NHTCategoryCodeInfo, ListValidation.InvalidCodeMessageError);

			Header.US_NHTProgramCode = NHTSAProgramCodeList.Codes.REI;
			DetailsLine.AddInfo.Validation.ValidateUS_NHTCategoryCode();
			AssertNoMessageErrors(DetailsLine.US_NHTCategoryCodeInfo);

			DetailsLine.US_NHTCategoryCode = NHTSACategoryCode_TPETYPList.Codes.TPE1;
			AssertHasMessageErrorContaining(DetailsLine.US_NHTCategoryCodeInfo, ListValidation.InvalidCodeMessageError);

			Header.US_NHTProgramCode = NHTSAProgramCodeList.Codes.TPE;
			DetailsLine.AddInfo.Validation.ValidateUS_NHTCategoryCode();
			AssertNoMessageErrors(DetailsLine.US_NHTCategoryCodeInfo);

			DetailsLine.US_NHTCategoryCode = NHTSACategoryCode_OEITYPList.Codes.OEI1;
			AssertHasMessageErrorContaining(DetailsLine.US_NHTCategoryCodeInfo, ListValidation.InvalidCodeMessageError);

			Header.US_NHTProgramCode = NHTSAProgramCodeList.Codes.OEI;
			DetailsLine.AddInfo.Validation.ValidateUS_NHTCategoryCode();
			AssertNoMessageErrors(DetailsLine.US_NHTCategoryCodeInfo);

			DetailsLine.US_NHTCategoryCode = NHTSACategoryCode_OFFTYPList.Codes.OFF1;
			AssertHasMessageErrorContaining(DetailsLine.US_NHTCategoryCodeInfo, ListValidation.InvalidCodeMessageError);

			Header.US_NHTProgramCode = NHTSAProgramCodeList.Codes.OFF;
			DetailsLine.AddInfo.Validation.ValidateUS_NHTCategoryCode();
			AssertNoMessageErrors(DetailsLine.US_NHTCategoryCodeInfo);
		}

		public void TestCheckUS_NHTCategoryCodeOnProduct()
		{
			var product = Factory.New<OrgSupplierPart>();
			var pivot = product.PivotsForBinding.AddNew();
			var header = pivot.NHTSALines.AddNew();
			var details = header.NHTSADetails.AddNew();
			details.AddInfo.Validation.ValidateUS_NHTCategoryCode();
			AssertNoMessageErrors(details.US_NHTCategoryCodeInfo);

			header.US_NHTProgramCode = NHTSAProgramCodeList.Codes.MVS;
			details.US_NHTCategoryCode = "~";
			AssertHasMessageErrorContaining(details.US_NHTCategoryCodeInfo, ListValidation.InvalidCodeMessageError);

			details.US_NHTCategoryCode = NHTSACategoryCode_MVSTYPList.Codes.MVS1;
			AssertNoMessageErrors(details.US_NHTCategoryCodeInfo);
		}

		[TestDate(2015, 07, 01)]
		public void TestCheckUS_NHTModelYear()
		{
			DetailsLine.AddInfo.Validation.ValidateUS_NHTModelYear();
			AssertNoWarnings(DetailsLine.US_NHTModelYearInfo);

			DetailsLine.US_NHTModelYear = "2017";
			AssertHasWarningContaining(DetailsLine.US_NHTModelYearInfo, ListValidation.InvalidCodeMessage);

			DetailsLine.US_NHTModelYear = "2016";
			AssertNoWarnings(DetailsLine.US_NHTYearOfMFRInfo);
		}

		public void TestCheckUS_NHTDriveSide()
		{
			DetailsLine.AddInfo.Validation.ValidateUS_NHTDriveSide();
			AssertNoMessageErrors(DetailsLine.US_NHTDriveSideInfo);

			DetailsLine.US_NHTDriveSide = "X";
			AssertHasWarningContaining(DetailsLine.US_NHTDriveSideInfo, ListValidation.InvalidCodeMessage);

			DetailsLine.US_NHTDriveSide = DriverSideList.Codes.Left;
			AssertNoWarnings(DetailsLine.US_NHTDriveSideInfo);

			Header.US_NHTBoxNumber = DepartmentOfTransportBoxNumberList.Codes._2B;
			DetailsLine.US_NHTDriveSide = ZString.Empty;
			AssertHasMessageErrorContaining(DetailsLine.US_NHTDriveSideInfo, MandatoryValidation.YouHaveNotEntered);

			DetailsLine.US_NHTDriveSide = "X";
			AssertHasMessageErrorContaining(DetailsLine.US_NHTDriveSideInfo, ListValidation.InvalidCodeMessageError);

			DetailsLine.US_NHTDriveSide = DriverSideList.Codes.Left;
			AssertNoMessageErrors(DetailsLine.US_NHTDriveSideInfo);

			Header.US_NHTBoxNumber = DepartmentOfTransportBoxNumberList.Codes._03;
			DetailsLine.US_NHTDriveSide = ZString.Empty;
			AssertHasMessageErrorContaining(DetailsLine.US_NHTDriveSideInfo, MandatoryValidation.YouHaveNotEntered);

			DetailsLine.US_NHTDriveSide = "X";
			AssertHasMessageErrorContaining(DetailsLine.US_NHTDriveSideInfo, ListValidation.InvalidCodeMessageError);

			DetailsLine.US_NHTDriveSide = DriverSideList.Codes.Left;
			AssertNoMessageErrors(DetailsLine.US_NHTDriveSideInfo);
		}

		NHTSAHeader Header
		{
			get
			{
				if (header == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					declaration.US_EnableENS = true;
					declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

					var invoice = declaration.Invoices.AddNew();
					var invoiceLine = invoice.InvoiceLines.AddNew();
					header = invoiceLine.NHTSALines.AddNew();
				}

				return header;
			}
		}

		NHTSAHeader header;

		NHTSADetails DetailsLine
		{
			get { return fDetailsLine ?? (fDetailsLine = Header.NHTSADetails.AddNew()); }
		}

		NHTSADetails fDetailsLine;
	}
}
