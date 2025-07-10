using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	internal class USAMSEG2AddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestUS_PermitNumber()
		{
			var eg2Line = testAMS.AMSLines.AddNew();
			var eg2LineForProduct = MASLineForProduct.AMSLines.AddNew();

			eg2Line.US_PermitNumber = "1233333";
			AssertNoMessageErrorContaining(eg2Line.US_PermitNumberInfo, MandatoryValidation.YouHaveNotEntered);

			eg2Line.US_PermitNumber = "";
			AssertHasMessageErrorContaining(eg2Line.US_PermitNumberInfo, MandatoryValidation.YouHaveNotEntered);

			eg2LineForProduct.US_PermitNumber = "";
			AssertNoMessageErrorContaining(eg2LineForProduct.US_PermitNumberInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_IsDocSubmitted()
		{
			var eg2Line = testAMS.AMSLines.AddNew();
			var eg2LineForProduct = MASLineForProduct.AMSLines.AddNew();

			eg2Line.US_IsDocSubmitted = false;
			AssertHasMessageError(eg2Line.US_IsDocSubmittedInfo, USAMSEG2AddInfoValidation.ConfirmSubmittedALLDocument);

			eg2LineForProduct.US_IsDocSubmitted = false;
			AssertNoMessageError(eg2LineForProduct.US_IsDocSubmittedInfo, USAMSEG2AddInfoValidation.ConfirmSubmittedALLDocument);

			eg2Line.US_IsDocSubmitted = true;
			AssertNoMessageError(eg2Line.US_IsDocSubmittedInfo, USAMSEG2AddInfoValidation.ConfirmSubmittedALLDocument);
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
			testAMS.US_Program = AMSProgramList.Codes.EG2;
		}

		#region TestForProductLines

		AMS MASLineForProduct
		{
			get
			{
				if (fAMSForProduct == null)
				{
					var lookup = Factory.New<CusClassification>();
					lookup.CC_LookupCode = "LOOK555";
					var product = Factory.New<OrgSupplierPart>();
					product.OP_PartNum = "PART555";

					var pivot = Factory.New<CusClassPartPivot>();
					pivot.CI_CC = lookup.PK;
					pivot.CI_OP = product.PK;

					fAMSForProduct = pivot.AMSLines.AddNew();
					fAMSForProduct.US_Program = AMSProgramList.Codes.EG2;
				}
				return fAMSForProduct;
			}
		}
		AMS fAMSForProduct;

		#endregion
	}
}
