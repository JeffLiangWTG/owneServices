using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	class USDeclarationFSISLineAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_HealthCertificateNumber()
		{
			var declaration = Factory.New<JobDeclaration>();
			var line = declaration.FSISLines.AddNew();
			line.US_HealthCertificateNumber = "555A";
			line.US_HealthCertificateNumber = ZString.Empty;
			AssertHasMessageErrorContaining(line.US_HealthCertificateNumberInfo, MandatoryValidation.YouHaveNotEntered);
			line.US_HealthCertificateNumber = "555A";
			AssertNoMessageErrorContaining(line.US_HealthCertificateNumberInfo, MandatoryValidation.YouHaveNotEntered);

			var secondLine = declaration.FSISLines.AddNew();
			secondLine.US_HealthCertificateNumber = "555A";
			AssertHasMessageError(secondLine.US_HealthCertificateNumberInfo, USDeclarationFSISLineAddInfoValidation.CertificateIsDuplicate);
			secondLine.US_HealthCertificateNumber = "555B";
			AssertNoMessageError(secondLine.US_HealthCertificateNumberInfo, USDeclarationFSISLineAddInfoValidation.CertificateIsDuplicate);
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

		public void TestCheckUS_DateOfInspection()
		{
			UsInvLineFSISLine.US_HealthCertificateNumber = new ZString("444");
			UsDeclarationFSISLine.US_HealthCertificateNumber = new ZString("444");
			UsDeclarationFSISLine.US_DateOfInspection = ZDateTime.Empty;
			AssertHasMessageErrorContaining(UsDeclarationFSISLine.US_DateOfInspectionInfo, MandatoryValidation.YouHaveNotEntered);
			UsDeclarationFSISLine.US_DateOfInspection = new ZDateTime(2016, 11, 09);
			AssertNoMessageErrorContaining(UsDeclarationFSISLine.US_DateOfInspectionInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_ImportingEstNo()
		{
			UsInvLineFSISLine.US_HealthCertificateNumber = new ZString("444");
			UsDeclarationFSISLine.US_HealthCertificateNumber = new ZString("444");
			UsDeclarationFSISLine.US_ImportingEstNo = ZString.Empty;
			AssertHasMessageErrorContaining(UsDeclarationFSISLine.US_ImportingEstNoInfo, MandatoryValidation.YouHaveNotEntered);
			UsDeclarationFSISLine.US_ImportingEstNo = new ZString("Test");
			AssertNoMessageErrorContaining(UsDeclarationFSISLine.US_ImportingEstNoInfo, MandatoryValidation.YouHaveNotEntered);
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

		JobComInvoiceLine InvLine
		{
			get { return fInvLine ?? (fInvLine = Declaration.Invoices.AddNew().InvoiceLines.AddNew()); }
		}
		JobComInvoiceLine fInvLine;

		USInvoiceLineFSISLine UsInvLineFSISLine
		{
			get { return fUsInvLineFSISLine ?? (fUsInvLineFSISLine = InvLine.FSISLines.AddNew()); }
		}
		USInvoiceLineFSISLine fUsInvLineFSISLine;

		USDeclarationFSISLine UsDeclarationFSISLine
		{
			get { return fUsDeclarationFSISLine ?? (fUsDeclarationFSISLine = Declaration.FSISLines.AddNew()); }
		}
		USDeclarationFSISLine fUsDeclarationFSISLine;
	}
}
