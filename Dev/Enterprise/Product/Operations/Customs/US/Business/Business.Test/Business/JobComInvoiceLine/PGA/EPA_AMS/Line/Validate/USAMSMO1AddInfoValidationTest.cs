using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	internal class USAMSMO1AddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_Packages()
		{
			var mo1Line = testAMS.AMSLines.AddNew();
			var mo1LineForProduct = MASLineForProduct.AMSLines.AddNew();
			mo1Line.US_Packages = 100m;
			AssertNoMessageErrorContaining(mo1Line.US_PackagesInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(mo1Line.US_PackagesInfo, USAMSEG1AddInfoValidation.EnterNumberGreaterThanZero);

			mo1Line.US_Packages = ZDecimal.Zero;
			AssertHasMessageErrorContaining(mo1Line.US_PackagesInfo, MandatoryValidation.YouHaveNotEntered);
			mo1LineForProduct.US_Packages = ZDecimal.Zero;
			AssertNoMessageErrorContaining(mo1LineForProduct.US_PackagesInfo, MandatoryValidation.YouHaveNotEntered);

			mo1Line.US_Packages = -2m;
			AssertHasMessageErrorContaining(mo1Line.US_PackagesInfo, USAMSEG1AddInfoValidation.EnterNumberGreaterThanZero);

			mo1Line.US_Packages = 742m;
			AssertNoMessageErrorContaining(mo1Line.US_PackagesInfo, USAMSEG1AddInfoValidation.EnterNumberGreaterThanZero);
		}

		public void TestCheckUS_InspecDateTime()
		{
			var mo1Line = testAMS.AMSLines.AddNew();
			var mo1LineForProduct = MASLineForProduct.AMSLines.AddNew();
			mo1Line.US_InspecDateTime = ZDateTime.Now.AddDays(4);
			AssertNoWarning(mo1Line.US_InspecDateTimeInfo, USAMSMO1AddInfoValidation.InspecDateTimeMessageError);

			mo1LineForProduct.US_InspecDateTime = ZDateTime.Empty;
			AssertNoWarning(mo1LineForProduct.US_InspecDateTimeInfo, USAMSMO1AddInfoValidation.InspecDateTimeMessageError);

			mo1Line.US_InspecDateTime = ZDateTime.Now.AddDays(1);
			AssertHasWarning(mo1Line.US_InspecDateTimeInfo, USAMSMO1AddInfoValidation.InspecDateTimeMessageError);
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
			testAMS.US_Program = AMSProgramList.Codes.MO1;
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
					fAMSForProduct.US_Program = AMSProgramList.Codes.MO1;
				}
				return fAMSForProduct;
			}
		}
		AMS fAMSForProduct;

		#endregion
	}
}
