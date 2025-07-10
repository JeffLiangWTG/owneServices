using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	public class USAMSOR2AddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_CertNumber()
		{
			var or2Line = testAMS.AMSLines.AddNew();
			or2Line.AddInfo.Validation.ValidateUS_CertNumber();
			AssertHasMessageErrorContaining(or2Line.US_CertNumberInfo, MandatoryValidation.YouHaveNotEntered);

			or2Line.US_CertNumber = "1234";
			AssertHasWarning(or2Line.US_CertNumberInfo, USAMSOR2AddInfoValidation.CertNumberFormatWarning);
			AssertNoMessageErrorContaining(or2Line.US_CertNumberInfo, MandatoryValidation.YouHaveNotEntered);

			or2Line.US_CertNumber = "123-1234567890-123456";
			AssertNoWarning(or2Line.US_CertNumberInfo, USAMSOR2AddInfoValidation.CertNumberFormatWarning);

			or2Line.US_CertNumber = "123-123-A";
			AssertNoWarning(or2Line.US_CertNumberInfo, USAMSOR2AddInfoValidation.CertNumberFormatWarning);
		}

		public void TestCheckUS_CertType()
		{
			var or2Line = testAMS.AMSLines.AddNew();
			or2Line.US_CertType = "~";
			AssertHasMessageErrorContaining(or2Line.US_CertTypeInfo, ListValidation.InvalidCodeMessageError);

			or2Line.US_CertType = LPCOTransactionTypeList.Codes.SingleUse;
			AssertNoMessageErrorContaining(or2Line.US_CertTypeInfo, ListValidation.InvalidCodeMessageError);

			or2Line.US_CertType = LPCOTransactionTypeList.Codes.Continuous;
			AssertNoMessageErrorContaining(or2Line.US_CertTypeInfo, ListValidation.InvalidCodeMessageError);

			or2Line.US_CertType = LPCOTransactionTypeList.Codes.General;
			AssertHasMessageErrorContaining(or2Line.US_CertTypeInfo, ListValidation.InvalidCodeMessageError);

			or2Line.US_CertType = ZString.Empty;
			AssertNoMessageErrorContaining(or2Line.US_CertTypeInfo, MandatoryValidation.YouHaveNotEntered);
			or2Line.US_CertNumber = "1234";
			or2Line.AddInfo.Validation.ValidateUS_CertType();
			AssertHasMessageErrorContaining(or2Line.US_CertTypeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		AMS testAMS;
		protected override void SetUp()
		{
			base.SetUp();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.ACE;
			declaration.US_EnableENS = true;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			testAMS = invoiceLine.AMSLines.AddNew();
			testAMS.US_Program = AMSProgramList.Codes.OR2;
		}
	}
}
