using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	internal class USInvoiceLineFSISLineAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_ExportingEstNo()
		{
			line.US_UC_NKCertificateIssuerCountry = Core.Constants.CountryCodes.Mexico;
			line.US_HealthCertificateNumber = "2";
			line.US_ExportingEstNo = "555A";
			line.US_ExportingEstNo = ZString.Empty;
			AssertHasMessageErrorContaining(line.US_ExportingEstNoInfo, MandatoryValidation.YouHaveNotEntered);
			line.US_ExportingEstNo = "555A";
			AssertNoMessageErrorContaining(line.US_ExportingEstNoInfo, MandatoryValidation.YouHaveNotEntered);

			line.US_ExportingEstNo = ZString.Empty;
			var cert = (USDeclarationFSISLine)declaration.FSISLines.FirstOrDefault(line => ((USDeclarationFSISLine)line).US_HealthCertificateNumber == "2");
			cert.US_HealthCertificateNumber = "2";
			cert.US_ExportingEstNo = "555A";

			AssertNoMessageErrorContaining(line.US_ExportingEstNoInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(cert.US_ExportingEstNoInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_CommercialDescription()
		{
			line.US_HealthCertificateNumber = "2";
			line.US_CommercialDescription = "555A";
			line.US_CommercialDescription = ZString.Empty;
			AssertHasMessageErrorContaining(line.US_CommercialDescriptionInfo, MandatoryValidation.YouHaveNotEntered);
			line.US_CommercialDescription = "555A";
			AssertNoMessageErrorContaining(line.US_CommercialDescriptionInfo, MandatoryValidation.YouHaveNotEntered);

			line.US_CommercialDescription = ZString.Empty;
			var cert = (USDeclarationFSISLine)declaration.FSISLines.FirstOrDefault(line => ((USDeclarationFSISLine)line).US_HealthCertificateNumber == "2");
			cert.US_CommercialDescription = "555A";

			AssertNoMessageErrorContaining(line.US_CommercialDescriptionInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(cert.US_CommercialDescriptionInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_UC_NKCountryOfOrigin()
		{
			line.US_HealthCertificateNumber = "2";
			line.US_UC_NKCountryOfOrigin = "XX";
			line.US_UC_NKCountryOfOrigin = ZString.Empty;
			AssertHasMessageErrorContaining(line.US_UC_NKCountryOfOriginInfo, MandatoryValidation.YouHaveNotEntered);
			line.US_UC_NKCountryOfOrigin = "XX";
			AssertNoMessageErrorContaining(line.US_UC_NKCountryOfOriginInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(line.US_UC_NKCountryOfOriginInfo, ListValidation.InvalidCodeMessageError);
			line.US_UC_NKCountryOfOrigin = Core.Constants.CountryCodes.Australia;
			AssertNoMessageErrorContaining(line.US_UC_NKCountryOfOriginInfo, ListValidation.InvalidCodeMessageError);

			line.US_UC_NKCountryOfOrigin = ZString.Empty;
			var cert = (USDeclarationFSISLine)declaration.FSISLines.FirstOrDefault(line => ((USDeclarationFSISLine)line).US_HealthCertificateNumber == "2");
			cert.US_UC_NKCountryOfOrigin = "XX";

			AssertNoMessageErrorContaining(line.US_UC_NKCountryOfOriginInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(cert.US_UC_NKCountryOfOriginInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_UC_NKCertificateIssuerCountry()
		{
			line.US_HealthCertificateNumber = "2";
			line.US_UC_NKCertificateIssuerCountry = "XX";
			line.US_UC_NKCertificateIssuerCountry = ZString.Empty;
			AssertHasMessageErrorContaining(line.US_UC_NKCertificateIssuerCountryInfo, MandatoryValidation.YouHaveNotEntered);
			line.US_UC_NKCertificateIssuerCountry = "XX";
			AssertNoMessageErrorContaining(line.US_UC_NKCertificateIssuerCountryInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(line.US_UC_NKCertificateIssuerCountryInfo, ListValidation.InvalidCodeMessageError);
			line.US_UC_NKCertificateIssuerCountry = Core.Constants.CountryCodes.Australia;
			AssertNoMessageErrorContaining(line.US_UC_NKCertificateIssuerCountryInfo, ListValidation.InvalidCodeMessageError);

			line.US_UC_NKCertificateIssuerCountry = ZString.Empty;
			var cert = (USDeclarationFSISLine)declaration.FSISLines.FirstOrDefault(line => ((USDeclarationFSISLine)line).US_HealthCertificateNumber == "2");
			cert.US_UC_NKCertificateIssuerCountry = "XX";

			AssertNoMessageErrorContaining(line.US_UC_NKCertificateIssuerCountryInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(cert.US_UC_NKCertificateIssuerCountryInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_ImportingEstNo()
		{
			line.US_HealthCertificateNumber = "2";
			line.US_ImportingEstNo = "XX";
			line.US_ImportingEstNo = ZString.Empty;
			AssertHasMessageErrorContaining(line.US_ImportingEstNoInfo, MandatoryValidation.YouHaveNotEntered);
			line.US_ImportingEstNo = "XX";
			AssertNoMessageErrorContaining(line.US_ImportingEstNoInfo, MandatoryValidation.YouHaveNotEntered);

			line.US_ImportingEstNo = ZString.Empty;
			var cert = declaration.FSISLines.AddNew();
			cert.US_HealthCertificateNumber = "2";
			cert.US_ImportingEstNo = "XX";

			AssertNoMessageErrorContaining(line.US_ImportingEstNoInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(cert.US_ImportingEstNoInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_ProductID()
		{
			line.US_HealthCertificateNumber = "2";
			line.US_ProductID = ZString.Empty;
			AssertNoMessageErrorContaining(line.US_ProductIDInfo, MandatoryValidation.YouHaveNotEntered);
			line.US_ProductID = "555A";
			AssertNoMessageErrorContaining(line.US_ProductIDInfo, MandatoryValidation.YouHaveNotEntered);
			line.US_ProductIDQualifier = "AI";
			line.US_ProductID = ZString.Empty;
			AssertHasMessageErrorContaining(line.US_ProductIDInfo, MandatoryValidation.YouHaveNotEntered);

			line.US_ProductID = ZString.Empty;
			var cert = (USDeclarationFSISLine)declaration.FSISLines.FirstOrDefault(line => ((USDeclarationFSISLine)line).US_HealthCertificateNumber == "2");
			cert.US_ProductID = "555A";

			AssertNoMessageErrorContaining(line.US_ProductIDInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(cert.US_ProductIDInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_ProductIDQualifier()
		{
			line.US_HealthCertificateNumber = "2";
			line.US_ProductIDQualifier = "XX";
			AssertHasMessageErrorContaining(line.US_ProductIDQualifierInfo, ListValidation.InvalidCodeMessageError);
			line.US_ProductIDQualifier = ZString.Empty;
			AssertNoMessageErrorContaining(line.US_ProductIDQualifierInfo, ListValidation.InvalidCodeMessageError);

			var cert = (USDeclarationFSISLine)declaration.FSISLines.FirstOrDefault(line => ((USDeclarationFSISLine)line).US_HealthCertificateNumber == "2");
			cert.US_ProductIDQualifier = "!";
			AssertHasMessageErrorContaining(cert.US_ProductIDQualifierInfo, ListValidation.InvalidCodeMessageError);

			line.US_ProductIDQualifier = GlobalUniqueProductCodeQualifierList.Codes.SRV;
			AssertNoMessageErrorContaining(line.US_ProductIDQualifierInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(cert.US_ProductIDQualifierInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_IntendedUseCode()
		{
			line.US_HealthCertificateNumber = "2";
			line.US_IntendedUseCode = "XX";
			line.US_IntendedUseCode = ZString.Empty;
			AssertHasMessageErrorContaining(line.US_IntendedUseCodeInfo, MandatoryValidation.YouHaveNotEntered);
			line.US_IntendedUseCode = "XX";
			AssertNoMessageErrorContaining(line.US_IntendedUseCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(line.US_IntendedUseCodeInfo, ListValidation.InvalidCodeMessageError);

			line.US_IntendedUseCode = ZString.Empty;
			var cert = (USDeclarationFSISLine)declaration.FSISLines.FirstOrDefault(line => ((USDeclarationFSISLine)line).US_HealthCertificateNumber == "2");
			cert.US_HealthCertificateNumber = "2";
			cert.US_IntendedUseCode = "XX";

			AssertNoMessageErrorContaining(line.US_IntendedUseCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(cert.US_IntendedUseCodeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			line = declaration.Invoices.AddNew().InvoiceLines.AddNew().FSISLines.AddNew();
		}
		USInvoiceLineFSISLine line;
		JobDeclaration declaration;
	}
}
