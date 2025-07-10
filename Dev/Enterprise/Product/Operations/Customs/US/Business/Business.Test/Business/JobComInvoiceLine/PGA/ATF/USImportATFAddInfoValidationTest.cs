using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USImportATFAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestNotRequiredDataByCategoryCode()
		{
			ATFLine.US_CategoryCode = ATFCategoryCodeList.Codes.NW;
			ATFLineForProduct.US_CategoryCode = ATFCategoryCodeList.Codes.NW;

			ATFLine.US_FELExemptionCode = "1";
			ATFLine.US_FELNumber = "1";
			AssertHasMessageError(ATFLine.US_FELExemptionCodeInfo, USImportATFAddInfoValidation.NotRequiredDataByCategoryCode);
			AssertHasMessageError(ATFLine.US_FELNumberInfo, USImportATFAddInfoValidation.NotRequiredDataByCategoryCode);

			ATFLine.US_FELExemptionCode = "";
			ATFLine.US_FELNumber = "";
			AssertNoMessageError(ATFLine.US_FELExemptionCodeInfo, USImportATFAddInfoValidation.NotRequiredDataByCategoryCode);
			AssertNoMessageError(ATFLine.US_FELNumberInfo, USImportATFAddInfoValidation.NotRequiredDataByCategoryCode);

			ATFLine.US_FFLExemptionCode = "1";
			ATFLine.US_FFLNumber = "1";
			AssertHasMessageError(ATFLine.US_FFLExemptionCodeInfo, USImportATFAddInfoValidation.NotRequiredDataByCategoryCode);
			AssertHasMessageError(ATFLine.US_FFLNumberInfo, USImportATFAddInfoValidation.NotRequiredDataByCategoryCode);

			ATFLine.US_FFLExemptionCode = "";
			ATFLine.US_FFLNumber = "";
			AssertNoMessageError(ATFLine.US_FFLExemptionCodeInfo, USImportATFAddInfoValidation.NotRequiredDataByCategoryCode);
			AssertNoMessageError(ATFLine.US_FFLNumberInfo, USImportATFAddInfoValidation.NotRequiredDataByCategoryCode);

			ATFLine.US_PermitExemptionCode = "1";
			ATFLine.US_PermitNumber = "1";
			AssertHasMessageError(ATFLine.US_PermitExemptionCodeInfo, USImportATFAddInfoValidation.NotRequiredDataByCategoryCode);
			AssertHasMessageError(ATFLine.US_PermitNumberInfo, USImportATFAddInfoValidation.NotRequiredDataByCategoryCode);

			ATFLine.US_PermitExemptionCode = "";
			ATFLine.US_PermitNumber = "";
			AssertNoMessageError(ATFLine.US_PermitExemptionCodeInfo, USImportATFAddInfoValidation.NotRequiredDataByCategoryCode);
			AssertNoMessageError(ATFLine.US_PermitNumberInfo, USImportATFAddInfoValidation.NotRequiredDataByCategoryCode);

			ATFLine.US_AECAExemptionCode = "1";
			ATFLine.US_AECANumber = "1";
			AssertHasMessageError(ATFLine.US_AECAExemptionCodeInfo, USImportATFAddInfoValidation.NotRequiredDataByCategoryCode);
			AssertHasMessageError(ATFLine.US_AECANumberInfo, USImportATFAddInfoValidation.NotRequiredDataByCategoryCode);

			ATFLine.US_AECAExemptionCode = "";
			ATFLine.US_AECANumber = "";
			AssertNoMessageError(ATFLine.US_AECAExemptionCodeInfo, USImportATFAddInfoValidation.NotRequiredDataByCategoryCode);
			AssertNoMessageError(ATFLine.US_AECANumberInfo, USImportATFAddInfoValidation.NotRequiredDataByCategoryCode);

			ATFLine.US_PermitExemptionCode = "1";
			ATFLine.US_PermitNumber = "1";
			AssertHasMessageError(ATFLine.US_PermitExemptionCodeInfo, USImportATFAddInfoValidation.NotRequiredDataByCategoryCode);
			AssertHasMessageError(ATFLine.US_PermitNumberInfo, USImportATFAddInfoValidation.NotRequiredDataByCategoryCode);

			ATFLineForProduct.US_FELExemptionCode = "KKK";
			AssertHasMessageErrorContaining(ATFLineForProduct.US_FELExemptionCodeInfo, ListValidation.InvalidCodeMessageError);

			ATFLineForProduct.US_PermitExemptionCode = "KKK";
			AssertHasMessageErrorContaining(ATFLineForProduct.US_PermitExemptionCodeInfo, ListValidation.InvalidCodeMessageError);
			ATFLineForProduct.US_AECAExemptionCode = "KKK";
			AssertHasMessageErrorContaining(ATFLineForProduct.US_AECAExemptionCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_Quantity()
		{
			ATFLine.US_Quantity = 1000m;
			AssertNoMessageErrorContaining(ATFLine.US_QuantityInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageError(ATFLine.US_QuantityInfo, USImportATFAddInfoValidation.EnterNumberGreaterThanZero);
			ATFLine.US_Quantity = ZDecimal.Zero;
			AssertHasMessageErrorContaining(ATFLine.US_QuantityInfo, MandatoryValidation.YouHaveNotEntered);

			ATFLine.US_Quantity = -2m;
			AssertHasMessageError(ATFLine.US_QuantityInfo, USImportATFAddInfoValidation.EnterNumberGreaterThanZero);
			ATFLineForProduct.US_Quantity = ZDecimal.Zero;
			AssertNoMessageErrorContaining(ATFLineForProduct.US_QuantityInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_CategoryCode()
		{
			ATFLine.US_CategoryCode = ATFCategoryCodeList.Codes.TRA;
			AssertNoMessageErrorContaining(ATFLine.US_CategoryCodeInfo, MandatoryValidation.YouHaveNotEntered);
			AssertNoMessageErrorContaining(ATFLine.US_CategoryCodeInfo, ListValidation.InvalidCodeMessageError);

			ATFLine.US_CategoryCode = "";
			AssertHasMessageErrorContaining(ATFLine.US_CategoryCodeInfo, MandatoryValidation.YouHaveNotEntered);

			ATFLine.US_CategoryCode = "~";
			AssertHasMessageErrorContaining(ATFLine.US_CategoryCodeInfo, ListValidation.InvalidCodeMessageError);

			ATFLineForProduct.US_CategoryCode = ATFCategoryCodeList.Codes.TRA;
			AssertNoMessageErrorContaining(ATFLineForProduct.US_CategoryCodeInfo, MandatoryValidation.YouHaveNotEntered);
			ATFLineForProduct.US_CategoryCode = "";
			AssertNoMessageErrorContaining(ATFLineForProduct.US_CategoryCodeInfo, MandatoryValidation.YouHaveNotEntered);
			ATFLineForProduct.US_CategoryCode = "~";
			AssertHasMessageErrorContaining(ATFLineForProduct.US_CategoryCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_FFLNumber()
		{
			ATFLine.US_CategoryCode = ATFCategoryCodeList.Codes.NSA;
			ATFLine.US_FFLNumber = "123";
			ATFLine.US_FFLExemptionCode = "";
			AssertNoMessageErrorContaining(ATFLine.US_FFLNumberInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(ATFLine.US_FFLNumberInfo, USImportATFAddInfoValidation.FFLNumberFormat);

			ATFLine.US_FFLNumber = "";
			AssertHasMessageErrorContaining(ATFLine.US_FFLNumberInfo, MandatoryValidation.YouHaveNotEntered);

			ATFLine.US_FFLNumber = "6-45-234-37-8Y-40512";
			AssertNoMessageErrorContaining(ATFLine.US_FFLNumberInfo, USImportATFAddInfoValidation.FFLNumberFormat);
			ATFLine.US_FFLNumber = "645234378Y40512";
			AssertNoMessageErrorContaining(ATFLine.US_FFLNumberInfo, USImportATFAddInfoValidation.FFLNumberFormat);

			ATFLine.US_CategoryCode = ATFCategoryCodeList.Codes.NW;
			ATFLine.AddInfoValidation.ValidateUS_FFLNumber();
			AssertNoMessageErrorContaining(ATFLine.US_FFLNumberInfo, MandatoryValidation.YouHaveNotEntered);

			ATFLine.US_CategoryCode = ATFCategoryCodeList.Codes.NSA;
			ATFLine.US_FFLExemptionCode = ExemptionCodesCodeList.Codes._1;
			ATFLine.AddInfoValidation.ValidateUS_FFLNumber();
			AssertNoMessageErrorContaining(ATFLine.US_FFLNumberInfo, MandatoryValidation.YouHaveNotEntered);

			ATFLine.US_FFLExemptionCode = ExemptionCodesCodeList.Codes._2;
			AssertNoMessageErrorContaining(ATFLine.US_FFLExemptionCodeInfo, ListValidation.InvalidCodeMessageError);
			ATFLine.US_FFLExemptionCode = "~";
			AssertHasMessageErrorContaining(ATFLine.US_FFLExemptionCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_FELNumber()
		{
			ATFLine.US_CategoryCode = ATFCategoryCodeList.Codes.INC;
			ATFLine.US_FELNumber = "123";
			ATFLine.US_FELExemptionCode = "";
			AssertNoMessageErrorContaining(ATFLine.US_FELNumberInfo, MandatoryValidation.YouHaveNotEntered);

			ATFLine.US_FELNumber = "";
			AssertHasMessageErrorContaining(ATFLine.US_FELNumberInfo, MandatoryValidation.YouHaveNotEntered);

			ATFLine.US_CategoryCode = ATFCategoryCodeList.Codes.HTZ;
			ATFLine.AddInfoValidation.ValidateUS_FELNumber();
			AssertNoMessageErrorContaining(ATFLine.US_FELNumberInfo, MandatoryValidation.YouHaveNotEntered);

			ATFLine.US_CategoryCode = ATFCategoryCodeList.Codes.INC;
			ATFLine.US_FELExemptionCode = ExemptionCodesCodeList.Codes._1;
			ATFLine.AddInfoValidation.ValidateUS_FELNumber();
			AssertNoMessageErrorContaining(ATFLine.US_FELNumberInfo, MandatoryValidation.YouHaveNotEntered);

			ATFLine.US_FELExemptionCode = ExemptionCodesCodeList.Codes._2;
			AssertNoMessageErrorContaining(ATFLine.US_FELExemptionCodeInfo, ListValidation.InvalidCodeMessageError);
			ATFLine.US_FELExemptionCode = "~";
			AssertHasMessageErrorContaining(ATFLine.US_FELExemptionCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_PermitNumber()
		{
			ATFLine.US_CategoryCode = ATFCategoryCodeList.Codes.WHD;
			ATFLine.US_PermitNumber = "123";
			ATFLine.US_PermitExemptionCode = "";
			AssertNoMessageErrorContaining(ATFLine.US_PermitNumberInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(ATFLine.US_PermitNumberInfo, USImportATFAddInfoValidation.PermitNumberFormat);

			ATFLine.US_PermitNumber = "";
			AssertHasMessageErrorContaining(ATFLine.US_PermitNumberInfo, MandatoryValidation.YouHaveNotEntered);

			ATFLine.US_PermitNumber = "2016-12345";
			AssertNoMessageErrorContaining(ATFLine.US_PermitNumberInfo, USImportATFAddInfoValidation.PermitNumberFormat);
			ATFLine.US_PermitNumber = "201612345";
			AssertNoMessageErrorContaining(ATFLine.US_PermitNumberInfo, USImportATFAddInfoValidation.PermitNumberFormat);

			ATFLine.US_PermitNumber = "";
			ATFLine.US_CategoryCode = ATFCategoryCodeList.Codes.NW;
			ATFLine.AddInfoValidation.ValidateUS_PermitNumber();
			AssertNoMessageErrorContaining(ATFLine.US_PermitNumberInfo, MandatoryValidation.YouHaveNotEntered);

			ATFLine.US_CategoryCode = ATFCategoryCodeList.Codes.WHD;
			ATFLine.US_PermitExemptionCode = ExemptionCodesCodeList.Codes._1;
			ATFLine.AddInfoValidation.ValidateUS_PermitNumber();
			AssertNoMessageErrorContaining(ATFLine.US_PermitNumberInfo, MandatoryValidation.YouHaveNotEntered);

			ATFLine.US_PermitExemptionCode = ExemptionCodesCodeList.Codes._2;
			AssertNoMessageErrorContaining(ATFLine.US_PermitExemptionCodeInfo, ListValidation.InvalidCodeMessageError);
			ATFLine.US_PermitExemptionCode = "~";
			AssertHasMessageErrorContaining(ATFLine.US_PermitExemptionCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_AECANumber()
		{
			ATFLine.US_CategoryCode = ATFCategoryCodeList.Codes.WHD;
			ATFLine.US_AECANumber = "123";
			ATFLine.US_AECAExemptionCode = "";
			AssertNoMessageErrorContaining(ATFLine.US_AECANumberInfo, MandatoryValidation.YouHaveNotEntered);
			AssertHasMessageErrorContaining(ATFLine.US_AECANumberInfo, USImportATFAddInfoValidation.AECANumberFormat);

			ATFLine.US_AECANumber = "";
			AssertHasMessageErrorContaining(ATFLine.US_AECANumberInfo, MandatoryValidation.YouHaveNotEntered);

			ATFLine.US_AECANumber = "A-45-265-4528";
			AssertNoMessageErrorContaining(ATFLine.US_AECANumberInfo, USImportATFAddInfoValidation.AECANumberFormat);

			ATFLine.US_AECANumber = "A452654528";
			AssertNoMessageErrorContaining(ATFLine.US_AECANumberInfo, USImportATFAddInfoValidation.AECANumberFormat);

			ATFLine.US_AECANumber = "";
			ATFLine.US_CategoryCode = ATFCategoryCodeList.Codes.WHP;
			ATFLine.AddInfoValidation.ValidateUS_AECANumber();
			AssertNoMessageErrorContaining(ATFLine.US_AECANumberInfo, MandatoryValidation.YouHaveNotEntered);

			ATFLine.US_CategoryCode = ATFCategoryCodeList.Codes.WHD;
			ATFLine.US_AECAExemptionCode = ExemptionCodesCodeList.Codes._1;
			ATFLine.AddInfoValidation.ValidateUS_AECANumber();
			AssertNoMessageErrorContaining(ATFLine.US_AECANumberInfo, MandatoryValidation.YouHaveNotEntered);

			ATFLine.US_PermitExemptionCode = ExemptionCodesCodeList.Codes._2;
			AssertNoMessageErrorContaining(ATFLine.US_AECAExemptionCodeInfo, ListValidation.InvalidCodeMessageError);
			ATFLine.US_AECAExemptionCode = "~";
			AssertHasMessageErrorContaining(ATFLine.US_AECAExemptionCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		ATF ATFLineForProduct
		{
			get
			{
				if (fATFLineForProduct == null)
				{
					var lookup = Factory.New<CusClassification>();
					lookup.CC_LookupCode = "LOOK434";
					var product = Factory.New<OrgSupplierPart>();
					product.OP_PartNum = "PART434";

					var pivot = Factory.New<CusClassPartPivot>();
					pivot.CI_CC = lookup.PK;
					pivot.CI_OP = product.PK;

					fATFLineForProduct = pivot.ATFLines.AddNew();
				}
				return fATFLineForProduct;
			}
		}
		ATF fATFLineForProduct;

		ATF ATFLine
		{
			get
			{
				if (atfLine == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					declaration.US_EnableCRL = true;
					declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
					var invoiceHeader = declaration.Invoices.AddNew();
					var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
					atfLine = invoiceLine.ATFLines.AddNew();
				}
				return atfLine;
			}
		}

		ATF atfLine;
	}
}
