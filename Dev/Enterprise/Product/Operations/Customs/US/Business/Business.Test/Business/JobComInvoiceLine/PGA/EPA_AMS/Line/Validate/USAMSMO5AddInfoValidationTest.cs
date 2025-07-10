using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	internal class USAMSMO5AddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestUS_InspecDateTime()
		{
			var mo5Line = testAMS.AMSLines.AddNew();
			mo5Line.US_InspecDateTime = ZDateTime.Now;
			AssertNoMessageErrorContaining(mo5Line.US_InspecDateTimeInfo, MandatoryValidation.YouHaveNotEntered);

			mo5Line.US_InspecDateTime = ZDateTime.Empty;
			AssertHasMessageErrorContaining(mo5Line.US_InspecDateTimeInfo, MandatoryValidation.YouHaveNotEntered);

			var mo5LineForProduct = MASLineForProduct.AMSLines.AddNew();
			mo5LineForProduct.US_InspecDateTime = ZDateTime.Empty;
			AssertNoMessageErrorContaining(mo5LineForProduct.US_InspecDateTimeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestUS_NetWeight()
		{
			var mo5Line = testAMS.AMSLines.AddNew();
			mo5Line.US_NetWeight = 100m;
			AssertNoMessageErrorContaining(mo5Line.US_NetWeightInfo, MandatoryValidation.YouHaveNotEntered);

			mo5Line.US_NetWeight = ZDecimal.Zero;
			AssertHasMessageErrorContaining(mo5Line.US_NetWeightInfo, MandatoryValidation.YouHaveNotEntered);

			mo5Line.US_NetWeight = -2m;
			AssertHasMessageErrorContaining(mo5Line.US_NetWeightInfo, USAMSLineAddInfoValidation.EnterNumberGreaterThanZero);

			mo5Line.US_NetWeight = 2m;
			AssertNoMessageErrorContaining(mo5Line.US_NetWeightInfo, USAMSLineAddInfoValidation.EnterNumberGreaterThanZero);

			var mo5LineForProduct = MASLineForProduct.AMSLines.AddNew();
			mo5LineForProduct.US_NetWeight = ZDecimal.Zero;
			AssertNoMessageErrorContaining(mo5LineForProduct.US_NetWeightInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestUS_NetWeightUQ()
		{
			var mo5Line = testAMS.AMSLines.AddNew();
			mo5Line.US_NetWeightUQ = "KG";
			AssertNoMessageErrorContaining(mo5Line.US_NetWeightUQInfo, MandatoryValidation.YouHaveNotEntered);

			mo5Line.US_NetWeightUQ = "";
			AssertHasMessageErrorContaining(mo5Line.US_NetWeightUQInfo, MandatoryValidation.YouHaveNotEntered);

			mo5Line.US_NetWeightUQ = "~";
			AssertHasMessageErrorContaining(mo5Line.US_NetWeightUQInfo, ListValidation.InvalidCodeMessageError);

			mo5Line.US_NetWeightUQ = "KG";
			AssertNoMessageErrorContaining(mo5Line.US_NetWeightUQInfo, ListValidation.InvalidCodeMessageError);

			var mo5LineForProduct = MASLineForProduct.AMSLines.AddNew();
			mo5LineForProduct.US_NetWeightUQ = "";
			AssertNoMessageErrorContaining(mo5LineForProduct.US_NetWeightUQInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestUS_OA_Applicant()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "ABC";

			var mo5Line = testAMS.AMSLines.AddNew();
			mo5Line.US_OA_Applicant = orgHeader.MainAddress.PK;
			AssertNoMessageErrorContaining(mo5Line.US_OA_ApplicantInfo, MandatoryValidation.YouHaveNotEntered);

			mo5Line.US_OA_Applicant = ZGuid.Empty;
			AssertHasMessageErrorContaining(mo5Line.US_OA_ApplicantInfo, MandatoryValidation.YouHaveNotEntered);

			var mo5LineForProduct = MASLineForProduct.AMSLines.AddNew();
			mo5LineForProduct.US_OA_Applicant = ZGuid.Empty;
			AssertNoMessageErrorContaining(mo5LineForProduct.US_OA_ApplicantInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestUS_OA_GoodsLocation()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "EFG";

			var mo5Line = testAMS.AMSLines.AddNew();
			mo5Line.US_OA_GoodsLocation = orgHeader.MainAddress.PK;
			AssertNoMessageErrorContaining(mo5Line.US_OA_GoodsLocationInfo, MandatoryValidation.YouHaveNotEntered);

			mo5Line.US_OA_GoodsLocation = ZGuid.Empty;
			AssertHasMessageErrorContaining(mo5Line.US_OA_GoodsLocationInfo, MandatoryValidation.YouHaveNotEntered);

			var mo5LineForProduct = MASLineForProduct.AMSLines.AddNew();
			mo5LineForProduct.US_OA_GoodsLocation = ZGuid.Empty;
			AssertNoMessageErrorContaining(mo5LineForProduct.US_OA_GoodsLocationInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestUS_PackageAndUQ()
		{
			var mo5Line = testAMS.AMSLines.AddNew();
			mo5Line.US_Packages = 100m;
			AssertNoMessageErrorContaining(mo5Line.US_PackagesInfo, MandatoryValidation.YouHaveNotEntered);

			mo5Line.US_Packages = ZDecimal.Zero;
			AssertHasMessageErrorContaining(mo5Line.US_PackagesInfo, MandatoryValidation.YouHaveNotEntered);

			mo5Line.US_Packages = -2m;
			AssertHasMessageErrorContaining(mo5Line.US_PackagesInfo, USAMSLineAddInfoValidation.EnterNumberGreaterThanZero);

			mo5Line.US_Packages = 2m;
			AssertNoMessageErrorContaining(mo5Line.US_PackagesInfo, USAMSLineAddInfoValidation.EnterNumberGreaterThanZero);

			mo5Line.US_PackagesUQ = "AC";
			AssertNoMessageErrorContaining(mo5Line.US_PackagesUQInfo, MandatoryValidation.YouHaveNotEntered);

			mo5Line.US_PackagesUQ = "";
			AssertHasMessageErrorContaining(mo5Line.US_PackagesUQInfo, MandatoryValidation.YouHaveNotEntered);

			mo5Line.US_PackagesUQ = "~";
			AssertHasMessageErrorContaining(mo5Line.US_PackagesUQInfo, ListValidation.InvalidCodeMessageError);

			mo5Line.US_PackagesUQ = "AC";
			AssertNoMessageErrorContaining(mo5Line.US_PackagesUQInfo, ListValidation.InvalidCodeMessageError);

			var mo5LineForProduct = MASLineForProduct.AMSLines.AddNew();
			mo5LineForProduct.US_PackagesUQ = "";
			AssertNoMessageErrorContaining(mo5LineForProduct.US_PackagesUQInfo, MandatoryValidation.YouHaveNotEntered);
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
			testAMS.US_Program = AMSProgramList.Codes.MO5;
			invoiceLine.US_AMSInd = OGAIndicatorList.Codes.Declared;
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
					fAMSForProduct.US_Program = AMSProgramList.Codes.MO5;
				}
				return fAMSForProduct;
			}
		}
		AMS fAMSForProduct;

		#endregion
	}
}
