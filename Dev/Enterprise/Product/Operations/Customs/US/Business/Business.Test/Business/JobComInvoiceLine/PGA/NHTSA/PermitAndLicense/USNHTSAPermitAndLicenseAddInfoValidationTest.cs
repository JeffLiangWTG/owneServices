using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	public class USNHTSAPermitAndLicenseAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_NHTLPCOType()
		{
			PermitAndLiceses.AddInfo.Validation.ValidateUS_NHTLPCOType();
			AssertNoMessageErrors(PermitAndLiceses.US_NHTLPCOTypeInfo);
			AssertNoWarnings(PermitAndLiceses.US_NHTLPCOTypeInfo);

			PermitAndLiceses.US_NHTLPCONumber = "XXX";
			PermitAndLiceses.AddInfo.Validation.ValidateUS_NHTLPCOType();
			AssertHasWarningContaining(PermitAndLiceses.US_NHTLPCOTypeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrors(PermitAndLiceses.US_NHTLPCOTypeInfo);

			PermitAndLiceses.US_NHTLPCOType = "XXX";
			AssertNoWarnings(PermitAndLiceses.US_NHTLPCOTypeInfo);
			AssertHasMessageErrorContaining(PermitAndLiceses.US_NHTLPCOTypeInfo, ListValidation.InvalidCodeMessageError);

			PermitAndLiceses.US_NHTLPCOType = NHTSALPCOTypeList.Codes.NH0;
			AssertNoMessageErrors(PermitAndLiceses.US_NHTLPCOTypeInfo);
			AssertNoWarnings(PermitAndLiceses.US_NHTLPCOTypeInfo);
		}

		public void TestCheckUS_NHTLPCONumber()
		{
			PermitAndLiceses.US_NHTLPCOType = NHTSALPCOTypeList.Codes.NH0;
			PermitAndLiceses.US_NHTLPCONumber = "XXXXX";
			AssertHasMessageErrorContaining(PermitAndLiceses.US_NHTLPCONumberInfo, ValidationConstants.NHTSA.InvalidLPCONumberFormatForRegisteredImporterNumber);

			PermitAndLiceses.US_NHTLPCONumber = "R-90-007";
			AssertNoMessageErrors(PermitAndLiceses.US_NHTLPCONumberInfo);

			PermitAndLiceses.US_NHTLPCOType = NHTSALPCOTypeList.Codes.NH2;
			AssertHasMessageErrorContaining(PermitAndLiceses.US_NHTLPCONumberInfo, ValidationConstants.NHTSA.InvalidLPCONumberFormatForNHTSAImportPermissionLetterr);

			PermitAndLiceses.US_NHTLPCONumber = "07-1401-0001";
			AssertNoMessageErrors(PermitAndLiceses.US_NHTLPCONumberInfo);

			PermitAndLiceses.US_NHTLPCOType = NHTSALPCOTypeList.Codes.NH3;
			AssertHasMessageErrorContaining(PermitAndLiceses.US_NHTLPCONumberInfo, ValidationConstants.NHTSA.InvalidLPCONumberFormatForVehicleEligbilityNumber);

			PermitAndLiceses.US_NHTLPCONumber = "VSA-080";
			AssertNoMessageErrors(PermitAndLiceses.US_NHTLPCONumberInfo);
		}

		public void TestCheckUS_NHTLPCODateType()
		{
			PermitAndLiceses.US_NHTLPCODateType = "X";
			AssertHasMessageErrorContaining(PermitAndLiceses.US_NHTLPCODateTypeInfo, ListValidation.InvalidCodeMessageError);

			PermitAndLiceses.US_NHTLPCODateType = LPCODateQualifierList.Codes.DateApplicationReceived;
			AssertNoMessageErrorContaining(PermitAndLiceses.US_NHTLPCODateTypeInfo, ListValidation.InvalidCodeMessageError);

			PermitAndLiceses.US_NHTLPCODateType = ZString.Empty;
			PermitAndLiceses.US_NHTLPCODate = ZDateTime.Today;
			AssertHasMessageErrorContaining(PermitAndLiceses.US_NHTLPCODateTypeInfo, MandatoryValidation.YouHaveNotEntered);

			PermitAndLiceses.US_NHTLPCODateType = LPCODateQualifierList.Codes.DateApplicationReceived;
			AssertNoMessageErrorContaining(PermitAndLiceses.US_NHTLPCODateTypeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_NHTLPCODate()
		{
			PermitAndLiceses.US_NHTLPCODateType = LPCODateQualifierList.Codes.DateApplicationReceived;
			AssertHasMessageErrorContaining(PermitAndLiceses.US_NHTLPCODateInfo, MandatoryValidation.YouHaveNotEntered);

			PermitAndLiceses.US_NHTLPCODate = ZDateTime.Today;
			AssertNoMessageErrorContaining(PermitAndLiceses.US_NHTLPCODateInfo, MandatoryValidation.YouHaveNotEntered);
		}

		#region Implementation

		NHTSAHeader Header
		{
			get
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
				declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
				declaration.US_EnableENS = true;
				declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

				var invoice = declaration.Invoices.AddNew();
				var invoiceLine = invoice.InvoiceLines.AddNew();
				header = invoiceLine.NHTSALines.AddNew();
				return header;
			}
		}
		NHTSAHeader header;

		NHTSADetails DetailsLine
		{
			get { return fDetailsLine ?? (fDetailsLine = Header.NHTSADetails.AddNew()); }
		}
		NHTSADetails fDetailsLine;

		NHTSAPermitAndLicenses PermitAndLiceses
		{
			get { return fPermitAndLiceses ?? (fPermitAndLiceses = DetailsLine.PermitAndLicenses.AddNew()); }
		}
		NHTSAPermitAndLicenses fPermitAndLiceses;

		#endregion
	}
}
