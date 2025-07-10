using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	internal class USAMSEG1AddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_InnerWeightUQ()
		{
			var eg1Line = testAMS.AMSLines.AddNew();
			eg1Line.US_InnerWeightUQ = ABIUnitOfMeasureList.Codes.Carat;
			AssertNoMessageErrorContaining(eg1Line.US_InnerWeightUQInfo, ListValidation.InvalidCodeMessageError);

			eg1Line.US_InnerWeightUQ = "~";
			AssertHasMessageErrorContaining(eg1Line.US_InnerWeightUQInfo, ListValidation.InvalidCodeMessageError);

			eg1Line.US_InnerWeight = 120m;
			eg1Line.US_InnerWeightUQ = "";
			AssertHasMessageErrorContaining(eg1Line.US_InnerWeightUQInfo, MandatoryValidation.YouHaveNotEntered);

			eg1Line.US_InnerWeightUQ = ABIUnitOfMeasureList.Codes.Carat;
			AssertNoMessageErrorContaining(eg1Line.US_InnerWeightUQInfo, MandatoryValidation.YouHaveNotEntered);

			var eg1LineForProduct = MASLineForProduct.AMSLines.AddNew();
			eg1LineForProduct.US_InnerWeightUQ = "";
			AssertNoMessageErrorContaining(eg1Line.US_InnerWeightUQInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_TotalWeightAndUQ()
		{
			var eg1Line = testAMS.AMSLines.AddNew();
			var eg1LineForProduct = MASLineForProduct.AMSLines.AddNew();
			eg1Line.US_TotalWeight = 100m;
			AssertNoMessageErrorContaining(eg1Line.US_TotalWeightInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(eg1Line.US_TotalWeightInfo, USAMSEG1AddInfoValidation.EnterNumberGreaterThanZero);

			eg1Line.US_TotalWeight = ZDecimal.Zero;
			AssertHasMessageErrorContaining(eg1Line.US_TotalWeightInfo, MandatoryValidation.YouHaveNotEntered);

			eg1Line.US_TotalWeight = -2m;
			AssertHasMessageErrorContaining(eg1Line.US_TotalWeightInfo, USAMSEG1AddInfoValidation.EnterNumberGreaterThanZero);

			eg1Line.US_TotalWeightUQ = "KG";
			AssertNoMessageErrorContaining(eg1Line.US_TotalWeightUQInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(eg1Line.US_TotalWeightUQInfo, ListValidation.InvalidCodeMessageError);

			eg1Line.US_TotalWeightUQ = "";
			AssertHasMessageErrorContaining(eg1Line.US_TotalWeightUQInfo, MandatoryValidation.YouHaveNotEntered);

			eg1LineForProduct.US_TotalWeightUQ = "";
			AssertNoMessageErrorContaining(eg1LineForProduct.US_TotalWeightUQInfo, MandatoryValidation.YouHaveNotEntered);

			eg1Line.US_TotalWeightUQ = "~";
			AssertHasMessageErrorContaining(eg1Line.US_TotalWeightUQInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestUS_OuterPackageAndUQ()
		{
			var eg1Line = testAMS.AMSLines.AddNew();
			var eg1LineForProduct = MASLineForProduct.AMSLines.AddNew();
			eg1Line.US_OuterPackage = 100m;
			AssertNoMessageErrorContaining(eg1Line.US_OuterPackageInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(eg1Line.US_OuterPackageInfo, USAMSEG1AddInfoValidation.EnterNumberGreaterThanZero);

			eg1Line.US_OuterPackage = ZDecimal.Zero;
			AssertHasMessageErrorContaining(eg1Line.US_OuterPackageInfo, MandatoryValidation.YouHaveNotEntered);

			eg1Line.US_OuterPackage = -2m;
			AssertHasMessageErrorContaining(eg1Line.US_OuterPackageInfo, USAMSEG1AddInfoValidation.EnterNumberGreaterThanZero);

			eg1Line.US_OuterPackageUQ = "AC";
			AssertNoMessageErrorContaining(eg1Line.US_OuterPackageUQInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(eg1Line.US_OuterPackageUQInfo, ListValidation.InvalidCodeMessageError);

			eg1Line.US_OuterPackageUQ = "";
			AssertHasMessageErrorContaining(eg1Line.US_OuterPackageUQInfo, MandatoryValidation.YouHaveNotEntered);

			eg1LineForProduct.US_OuterPackageUQ = "";
			AssertNoMessageErrorContaining(eg1LineForProduct.US_OuterPackageUQInfo, MandatoryValidation.YouHaveNotEntered);

			eg1Line.US_OuterPackageUQ = "~";
			AssertHasMessageErrorContaining(eg1Line.US_OuterPackageUQInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestUS_OA_GoodsLocation()
		{
			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "EFG";

			var eg1Line = testAMS.AMSLines.AddNew();
			var eg1LineForProduct = MASLineForProduct.AMSLines.AddNew();
			eg1Line.US_OA_GoodsLocation = orgHeader.MainAddress.PK;
			AssertNoMessageErrorContaining(eg1Line.US_OA_GoodsLocationInfo, MandatoryValidation.YouHaveNotEntered);

			eg1Line.US_OA_GoodsLocation = ZGuid.Empty;
			AssertHasMessageErrorContaining(eg1Line.US_OA_GoodsLocationInfo, MandatoryValidation.YouHaveNotEntered);

			eg1LineForProduct.US_OA_GoodsLocation = ZGuid.Empty;
			AssertHasMessageErrorContaining(eg1Line.US_OA_GoodsLocationInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestUS_InspecDateTime()
		{
			var eg1Line = testAMS.AMSLines.AddNew();
			var eg1LineForProduct = MASLineForProduct.AMSLines.AddNew();

			eg1Line.US_InspecDateTime = ZDateTime.Now;
			AssertNoMessageErrorContaining(eg1Line.US_InspecDateTimeInfo, MandatoryValidation.YouHaveNotEntered);

			eg1Line.US_InspecDateTime = ZDateTime.Empty;
			AssertHasMessageErrorContaining(eg1Line.US_InspecDateTimeInfo, MandatoryValidation.YouHaveNotEntered);

			eg1LineForProduct.US_InspecDateTime = ZDateTime.Empty;
			AssertNoMessageErrorContaining(eg1LineForProduct.US_InspecDateTimeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestUS_InnerPackageAndUQ()
		{
			var eg1Line = testAMS.AMSLines.AddNew();
			var eg1LineForProduct = MASLineForProduct.AMSLines.AddNew();

			eg1Line.US_InnerPackage = 100m;
			AssertNoMessageErrorContaining(eg1Line.US_InnerPackageInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(eg1Line.US_InnerPackageInfo, USAMSEG1AddInfoValidation.EnterNumberGreaterThanZero);

			eg1Line.US_InnerPackage = ZDecimal.Zero;
			AssertHasMessageErrorContaining(eg1Line.US_InnerPackageInfo, MandatoryValidation.YouHaveNotEntered);

			eg1Line.US_InnerPackage = -2m;
			AssertHasMessageErrorContaining(eg1Line.US_InnerPackageInfo, USAMSEG1AddInfoValidation.EnterNumberGreaterThanZero);

			eg1Line.US_InnerPackageUQ = "";
			AssertNoMessageErrorContaining(eg1LineForProduct.US_InnerPackageUQInfo, MandatoryValidation.YouHaveNotEntered);

			eg1Line.US_InnerPackageUQ = "";
			AssertHasMessageErrorContaining(eg1Line.US_InnerPackageUQInfo, MandatoryValidation.YouHaveNotEntered);

			eg1Line.US_InnerPackageUQ = "~";
			AssertHasMessageErrorContaining(eg1Line.US_InnerPackageUQInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestUS_InnerAmountAndUQ()
		{
			var eg1Line = testAMS.AMSLines.AddNew();
			var eg1LineForProduct = MASLineForProduct.AMSLines.AddNew();

			eg1Line.US_InnerAmount = 100m;
			AssertNoMessageErrorContaining(eg1Line.US_InnerAmountInfo, MandatoryValidation.YouHaveNotEntered);

			eg1Line.US_InnerAmount = ZDecimal.Zero;
			AssertHasMessageErrorContaining(eg1Line.US_InnerAmountInfo, MandatoryValidation.YouHaveNotEntered);

			eg1Line.US_InnerAmount = -2m;
			AssertHasMessageErrorContaining(eg1Line.US_InnerAmountInfo, USAMSEG1AddInfoValidation.EnterNumberGreaterThanZero);

			eg1Line.US_InnerAmount = 2m;
			AssertNoMessageErrorContaining(eg1Line.US_InnerAmountInfo, USAMSEG1AddInfoValidation.EnterNumberGreaterThanZero);

			eg1Line.US_InnerAmountUQ = "AC";
			AssertNoMessageErrorContaining(eg1Line.US_InnerAmountUQInfo, MandatoryValidation.YouHaveNotEntered);

			eg1Line.US_InnerAmountUQ = "";
			AssertHasMessageErrorContaining(eg1Line.US_InnerAmountUQInfo, MandatoryValidation.YouHaveNotEntered);

			eg1LineForProduct.US_InnerAmountUQ = "";
			AssertNoMessageErrorContaining(eg1LineForProduct.US_InnerAmountUQInfo, MandatoryValidation.YouHaveNotEntered);

			eg1Line.US_InnerAmountUQ = "~";
			AssertHasMessageErrorContaining(eg1Line.US_InnerAmountUQInfo, ListValidation.InvalidCodeMessageError);

			eg1Line.US_InnerAmountUQ = "KG";
			AssertNoMessageErrorContaining(eg1Line.US_InnerAmountUQInfo, ListValidation.InvalidCodeMessageError);
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
			testAMS.US_Program = AMSProgramList.Codes.EG1;
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
					fAMSForProduct.US_Program = AMSProgramList.Codes.EG1;
				}
				return fAMSForProduct;
			}
		}
		AMS fAMSForProduct;

		#endregion

	}
}
