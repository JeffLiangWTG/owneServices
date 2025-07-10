using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	internal class USLicenseAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_DateQualifier()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoiceHeader = declaration.Invoices.AddNew();
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			var laceyActLine = invoiceLine.LaceyActLines.AddNew();
			var license = laceyActLine.Licenses.AddNew();

			license.US_Type = LaceyActLPCOTypeList.Codes.A01;
			license.US_DateQualifier = LPCODateQualifierList.Codes.EffectiveDate;
			var text = string.Format(USLicenseAddInfoValidation.WrongDateQualifier, "3 – Issuance Date", "'A01'");
			AssertHasMessageError(license.US_DateQualifierInfo, text);
			license.US_DateQualifier = LPCODateQualifierList.Codes.DateIssuedOrSigned;
			AssertNoMessageError(license.US_DateQualifierInfo, text);

			license.US_Type = LaceyActLPCOTypeList.Codes.A14;
			license.US_DateQualifier = LPCODateQualifierList.Codes.EffectiveDate;
			text = string.Format(USLicenseAddInfoValidation.WrongDateQualifier, "1 – Expiration Date", "other than 'A01'");
			AssertHasMessageError(license.US_DateQualifierInfo, text);
			license.US_DateQualifier = LPCODateQualifierList.Codes.ExpirationDate;
			AssertNoMessageError(license.US_DateQualifierInfo, text);
			AssertNoMessageError(license.US_DateQualifierInfo, ListValidation.InvalidCodeMessageError);

			license.US_DateQualifier = "5";
			AssertHasMessageError(license.US_DateQualifierInfo, ListValidation.InvalidCodeMessageError);

			license.US_Type = "!";
			AssertHasMessageError(license.US_TypeInfo, ListValidation.InvalidCodeMessageError);

			license.US_Type = LaceyActLPCOTypeList.Codes.A12;
			AssertNoMessageError(license.US_TypeInfo, ListValidation.InvalidCodeMessageError);

			license.US_TransType = "!";
			AssertHasMessageError(license.US_TransTypeInfo, ListValidation.InvalidCodeMessageError);

			license.US_TransType = LPCOTransactionTypeList.Codes.General;
			AssertNoMessageError(license.US_TransTypeInfo, ListValidation.InvalidCodeMessageError);
		}
	}
}
