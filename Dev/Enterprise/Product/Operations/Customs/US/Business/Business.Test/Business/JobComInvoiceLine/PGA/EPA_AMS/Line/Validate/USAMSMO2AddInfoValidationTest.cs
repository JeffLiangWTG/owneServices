using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	internal class USAMSMO2AddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestUS_InspeCertType()
		{
			var mo2Line = testAMS.AMSLines.AddNew();
			mo2Line.US_CertType = "~";
			AssertHasMessageErrorContaining(mo2Line.US_CertTypeInfo, ListValidation.InvalidCodeMessageError);

			mo2Line.US_CertType = LPCOTypeList.Codes.AM6;
			AssertNoMessageErrorContaining(mo2Line.US_CertTypeInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(mo2Line.US_CertTypeInfo, MandatoryValidation.YouHaveNotEntered);

			mo2Line.US_CertType = ZString.Empty;
			AssertHasMessageErrorContaining(mo2Line.US_CertTypeInfo, MandatoryValidation.YouHaveNotEntered);

			var mo2LineForProduct = MASLineForProduct.AMSLines.AddNew();
			mo2LineForProduct.US_CertType = ZString.Empty;
			AssertNoMessageErrorContaining(mo2LineForProduct.US_CertTypeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestUS_CertNumber()
		{
			var mo2Line = testAMS.AMSLines.AddNew();
			mo2Line.US_CertNumber = "E0000998";
			AssertNoMessageErrorContaining(mo2Line.US_CertNumberInfo, MandatoryValidation.YouHaveNotEntered);

			mo2Line.US_CertNumber = "";
			AssertHasMessageErrorContaining(mo2Line.US_CertNumberInfo, MandatoryValidation.YouHaveNotEntered);

			var mo2LineForProduct = MASLineForProduct.AMSLines.AddNew();
			mo2LineForProduct.US_CertNumber = "";
			AssertNoMessageErrorContaining(mo2LineForProduct.US_CertNumberInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestUS_IsDocSubmitted()
		{
			var mo2Line = testAMS.AMSLines.AddNew();
			mo2Line.US_IsDocSubmitted = false;
			AssertHasMessageError(mo2Line.US_IsDocSubmittedInfo, USAMSMO2AddInfoValidation.ConfirmSubmittedALLDocument);

			mo2Line.US_IsDocSubmitted = true;
			AssertNoMessageError(mo2Line.US_IsDocSubmittedInfo, USAMSMO2AddInfoValidation.ConfirmSubmittedALLDocument);
			var mo2LineForProduct = MASLineForProduct.AMSLines.AddNew();

			mo2LineForProduct.US_IsDocSubmitted = true;
			AssertNoMessageError(mo2LineForProduct.US_IsDocSubmittedInfo, USAMSMO2AddInfoValidation.ConfirmSubmittedALLDocument);
		}

		public void TestCheckUS_CanadianProvinces()
		{
			var mo2Line = testAMS.AMSLines.AddNew();
			mo2Line.US_Party = FoodInspectionAgencyList.Codes.CA;
			mo2Line.US_InspectionLocation = CanadaStatesList.Codes.AB;
			AssertNoMessageErrorContaining(mo2Line.US_InspectionLocationInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(mo2Line.US_InspectionLocationInfo, ListValidation.InvalidCodeMessageError);

			mo2Line.US_InspectionLocation = "";
			AssertHasMessageErrorContaining(mo2Line.US_InspectionLocationInfo, MandatoryValidation.YouHaveNotEntered);

			mo2Line.US_InspectionLocation = "~";
			AssertHasMessageErrorContaining(mo2Line.US_InspectionLocationInfo, ListValidation.InvalidCodeMessageError);

			var mo2LineForProduct = MASLineForProduct.AMSLines.AddNew();
			mo2LineForProduct.US_InspectionLocation = "";
			AssertNoMessageErrorContaining(mo2LineForProduct.US_InspectionLocationInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestUS_Pary()
		{
			var mo2Line = testAMS.AMSLines.AddNew();
			mo2Line.US_Party = FoodInspectionAgencyList.Codes.CA;
			AssertNoMessageErrorContaining(mo2Line.US_PartyInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(mo2Line.US_PartyInfo, ListValidation.InvalidCodeMessageError);

			mo2Line.US_Party = "";
			AssertHasMessageErrorContaining(mo2Line.US_PartyInfo, MandatoryValidation.YouHaveNotEntered);

			mo2Line.US_Party = "~";
			AssertHasMessageErrorContaining(mo2Line.US_PartyInfo, ListValidation.InvalidCodeMessageError);

			var mo2LineForProduct = MASLineForProduct.AMSLines.AddNew();
			mo2LineForProduct.US_Party = "";
			AssertNoMessageErrorContaining(mo2LineForProduct.US_PartyInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestUS_WeightAndUQ()
		{
			var mo2Line = testAMS.AMSLines.AddNew();
			mo2Line.US_Weight = 100m;
			AssertNoMessageErrorContaining(mo2Line.US_WeightInfo, MandatoryValidation.YouHaveNotEntered);

			mo2Line.US_Weight = ZDecimal.Zero;
			AssertHasMessageErrorContaining(mo2Line.US_WeightInfo, MandatoryValidation.YouHaveNotEntered);

			mo2Line.US_Weight = -2m;
			AssertHasMessageErrorContaining(mo2Line.US_WeightInfo, USAMSLineAddInfoValidation.EnterNumberGreaterThanZero);

			mo2Line.US_Weight = 2m;
			AssertNoMessageErrorContaining(mo2Line.US_WeightInfo, USAMSLineAddInfoValidation.EnterNumberGreaterThanZero);

			mo2Line.US_WeightUQ = "KG";
			AssertNoMessageErrorContaining(mo2Line.US_WeightUQInfo, MandatoryValidation.YouHaveNotEntered);

			mo2Line.US_WeightUQ = "";
			AssertHasMessageErrorContaining(mo2Line.US_WeightUQInfo, MandatoryValidation.YouHaveNotEntered);

			mo2Line.US_WeightUQ = "~";
			AssertHasMessageErrorContaining(mo2Line.US_WeightUQInfo, ListValidation.InvalidCodeMessageError);

			mo2Line.US_WeightUQ = "KG";
			AssertNoMessageErrorContaining(mo2Line.US_WeightUQInfo, ListValidation.InvalidCodeMessageError);

			var mo2LineForProduct = MASLineForProduct.AMSLines.AddNew();
			mo2LineForProduct.US_WeightUQ = "";
			AssertNoMessageErrorContaining(mo2LineForProduct.US_WeightUQInfo, MandatoryValidation.YouHaveNotEntered);
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
			testAMS.US_Program = AMSProgramList.Codes.MO2;
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
					fAMSForProduct.US_Program = AMSProgramList.Codes.MO2;
				}
				return fAMSForProduct;
			}
		}
		AMS fAMSForProduct;

		#endregion
	}
}
