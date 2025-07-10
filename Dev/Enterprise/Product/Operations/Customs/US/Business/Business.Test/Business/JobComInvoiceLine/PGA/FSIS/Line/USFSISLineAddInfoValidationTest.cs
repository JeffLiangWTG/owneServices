using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	internal class USFSISLineAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_ExportingEstNo()
		{
			line.US_UC_NKCertificateIssuerCountry = Core.Constants.CountryCodes.Mexico;
			line.US_ExportingEstNo = "555A";
			line.US_ExportingEstNo = ZString.Empty;
			AssertHasMessageErrorContaining(line.US_ExportingEstNoInfo, MandatoryValidation.YouHaveNotEntered);
			line.US_ExportingEstNo = "555A";
			AssertNoMessageErrorContaining(line.US_ExportingEstNoInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_CommercialDescription()
		{
			line.US_CommercialDescription = "555A";
			line.US_CommercialDescription = ZString.Empty;
			AssertHasMessageErrorContaining(line.US_CommercialDescriptionInfo, MandatoryValidation.YouHaveNotEntered);
			line.US_CommercialDescription = "555A";
			AssertNoMessageErrorContaining(line.US_CommercialDescriptionInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_DateOfInspection()
		{
			line.US_DateOfInspection = new ZDateTime(ZDateTime.Now);
			line.US_DateOfInspection = ZDateTime.Empty;
			AssertHasMessageErrorContaining(line.US_DateOfInspectionInfo, MandatoryValidation.YouHaveNotEntered);
			line.US_DateOfInspection = new ZDateTime(ZDateTime.Now);
			AssertNoMessageErrorContaining(line.US_DateOfInspectionInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_UC_NKCountryOfOrigin()
		{
			line.US_UC_NKCountryOfOrigin = "XX";
			line.US_UC_NKCountryOfOrigin = ZString.Empty;
			AssertHasMessageErrorContaining(line.US_UC_NKCountryOfOriginInfo, MandatoryValidation.YouHaveNotEntered);
			line.US_UC_NKCountryOfOrigin = "XX";
			AssertNoMessageErrorContaining(line.US_UC_NKCountryOfOriginInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(line.US_UC_NKCountryOfOriginInfo, ListValidation.InvalidCodeMessageError);
			line.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Australia;
			AssertNoMessageErrorContaining(line.US_UC_NKCountryOfOriginInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_UC_NKCertificateIssuerCountry()
		{
			line.US_UC_NKCertificateIssuerCountry = "XX";
			line.US_UC_NKCertificateIssuerCountry = ZString.Empty;
			AssertHasMessageErrorContaining(line.US_UC_NKCertificateIssuerCountryInfo, MandatoryValidation.YouHaveNotEntered);
			line.US_UC_NKCertificateIssuerCountry = "XX";
			AssertNoMessageErrorContaining(line.US_UC_NKCertificateIssuerCountryInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(line.US_UC_NKCertificateIssuerCountryInfo, ListValidation.InvalidCodeMessageError);
			line.US_UC_NKCertificateIssuerCountry = Core.Constants.CountryCodes.Australia;
			AssertNoMessageErrorContaining(line.US_UC_NKCertificateIssuerCountryInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_ImportingEstNo()
		{
			line.US_ImportingEstNo = "XX";
			line.US_ImportingEstNo = ZString.Empty;
			AssertHasMessageErrorContaining(line.US_ImportingEstNoInfo, MandatoryValidation.YouHaveNotEntered);
			line.US_ImportingEstNo = "XX";
			AssertNoMessageErrorContaining(line.US_ImportingEstNoInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(line.US_ImportingEstNoInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_ProductID()
		{
			line.US_ProductID = ZString.Empty;
			AssertNoMessageErrorContaining(line.US_ProductIDInfo, MandatoryValidation.YouHaveNotEntered);
			line.US_ProductID = "555A";
			AssertNoMessageErrorContaining(line.US_ProductIDInfo, MandatoryValidation.YouHaveNotEntered);
			line.US_ProductIDQualifier = "AI";
			line.US_ProductID = ZString.Empty;
			AssertHasMessageErrorContaining(line.US_ProductIDInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_ProductIDQualifier()
		{
			line.US_ProductIDQualifier = "XX";
			AssertHasMessageErrorContaining(line.US_ProductIDQualifierInfo, ListValidation.InvalidCodeMessageError);
			line.US_ProductIDQualifier = ZString.Empty;
			AssertNoMessageErrorContaining(line.US_ProductIDQualifierInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_IntendedUseCode()
		{
			line.US_IntendedUseCode = "XX";
			line.US_IntendedUseCode = ZString.Empty;
			AssertHasMessageErrorContaining(line.US_IntendedUseCodeInfo, MandatoryValidation.YouHaveNotEntered);
			line.US_IntendedUseCode = "XX";
			AssertNoMessageErrorContaining(line.US_IntendedUseCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(line.US_IntendedUseCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_PGAContactName()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			Declaration.US_EnableENS = true;
			Declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			line.US_PGAContactName = ZString.Empty;
			line.AddInfoValidation.ValidateUS_PGAContactName();
			AssertHasMessageErrorContaining(line.US_PGAContactNameInfo, MandatoryValidation.YouHaveNotEntered);
			line.US_PGAContactName = "TEST NAME";
			AssertNoMessageErrorContaining(line.US_PGAContactNameInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_PGAContactPhoneNo()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			Declaration.US_EnableENS = true;
			Declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			line.US_PGAContactPhoneNo = "";
			line.AddInfoValidation.ValidateUS_PGAContactPhoneNo();
			AssertHasMessageErrorContaining(line.US_PGAContactPhoneNoInfo, DomesticPhoneNoValidator.DomesticPhoneNoFormat);
			line.US_PGAContactPhoneNo = "0122232323";
			AssertNoMessageErrorContaining(line.US_PGAContactPhoneNoInfo, DomesticPhoneNoValidator.DomesticPhoneNoFormat);
		}

		public void TestCheckUS_PGAContactEmail()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			Declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			Declaration.US_EnableENS = true;
			Declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;

			line.US_PGAContactEmail = ZString.Empty;
			line.AddInfoValidation.ValidateUS_PGAContactEmail();
			AssertHasMessageErrorContaining(line.US_PGAContactEmailInfo, MandatoryValidation.YouHaveNotEntered);
			line.US_PGAContactEmail = "~";
			AssertNoMessageErrorContaining(line.US_PGAContactEmailInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasWarningContaining(line.US_PGAContactEmailInfo, "Invalid email format");
			line.US_PGAContactEmail = "test.abc@def.com";
			AssertNoWarningContaining(line.US_PGAContactEmailInfo, "Invalid email format");
		}

		USFSISLine line;
		protected override void SetUp()
		{
			base.SetUp();
			line = Declaration.Invoices.AddNew().InvoiceLines.AddNew().FSISLines.AddNew();
		}

		JobDeclaration Declaration
		{
			get { return declaration ?? (declaration = Factory.New<JobDeclaration>()); }
		}
		JobDeclaration declaration;
	}
}
