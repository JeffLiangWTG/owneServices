using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USExportATFAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_FFLNumber()
		{
			ExportATF.US_FFLNumber = "TEST NUMBER";
			ExportATF.US_FFLExemptionCode = ExemptionCodesCodeList.Codes._1;
			AssertHasMessageErrorContaining(ExportATF.US_FFLNumberInfo, USExportATFAddInfoValidation.EitherFFLNumberOrExemptionCodeCanBeSpecified);

			ExportATF.US_FFLExemptionCode = ZString.Empty;
			ExportATF.AddInfoValidation.ValidateUS_FFLNumber();
			AssertNoMessageErrorContaining(ExportATF.US_FFLNumberInfo, USExportATFAddInfoValidation.EitherFFLNumberOrExemptionCodeCanBeSpecified);
		}

		public void TestCheckUS_FFLExemptionCode()
		{
			ExportATF.US_FFLNumber = "TEST NUMBER";
			ExportATF.US_FFLExemptionCode = ExemptionCodesCodeList.Codes._1;
			AssertHasMessageErrorContaining(ExportATF.US_FFLExemptionCodeInfo, USExportATFAddInfoValidation.EitherFFLNumberOrExemptionCodeCanBeSpecified);
			AssertNoMessageErrorContaining(ExportATF.US_FFLExemptionCodeInfo, ListValidation.InvalidCodeMessageError);

			ExportATF.US_FFLExemptionCode = "~";
			AssertHasMessageErrorContaining(ExportATF.US_FFLExemptionCodeInfo, ListValidation.InvalidCodeMessageError);

			ExportATF.US_FFLNumber = ZString.Empty;
			ExportATF.AddInfoValidation.ValidateUS_FFLExemptionCode();
			AssertNoMessageErrorContaining(ExportATF.US_FFLExemptionCodeInfo, USExportATFAddInfoValidation.EitherFFLNumberOrExemptionCodeCanBeSpecified);

			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivot.CD_ATFIndicator = OGAIndicatorList.Codes.Declared;
			var productExportATF = pivot.ExportATF;
			AssertNoMessageErrorContaining(productExportATF.US_FFLExemptionCodeInfo, ListValidation.InvalidCodeMessageError);
			productExportATF.US_FFLExemptionCode = "~";
			AssertHasMessageErrorContaining(productExportATF.US_FFLExemptionCodeInfo, ListValidation.InvalidCodeMessageError);
			productExportATF.US_FFLExemptionCode = ExemptionCodesCodeList.Codes._1;
			AssertNoMessageErrorContaining(productExportATF.US_FFLExemptionCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_PermitNumber()
		{
			ExportATF.US_PermitNumber = "TEST NUM";
			ExportATF.US_PermitExemptionCode = ExemptionCodesCodeList.Codes._2;
			AssertHasMessageErrorContaining(ExportATF.US_PermitNumberInfo, USExportATFAddInfoValidation.EitherPermitNumberOrExemptionCodeCanBeSpecified);

			ExportATF.US_PermitExemptionCode = ZString.Empty;
			ExportATF.AddInfoValidation.ValidateUS_PermitNumber();
			AssertNoMessageErrorContaining(ExportATF.US_PermitNumberInfo, USExportATFAddInfoValidation.EitherPermitNumberOrExemptionCodeCanBeSpecified);
		}

		public void TestCheckUS_PermitExemptionCode()
		{
			ExportATF.US_PermitNumber = "TEST NUM";
			ExportATF.US_PermitExemptionCode = ExemptionCodesCodeList.Codes._2;
			AssertHasMessageErrorContaining(ExportATF.US_PermitExemptionCodeInfo, USExportATFAddInfoValidation.EitherPermitNumberOrExemptionCodeCanBeSpecified);
			AssertNoMessageErrorContaining(ExportATF.US_PermitExemptionCodeInfo, ListValidation.InvalidCodeMessageError);

			ExportATF.US_PermitExemptionCode = "~";
			AssertHasMessageErrorContaining(ExportATF.US_PermitExemptionCodeInfo, ListValidation.InvalidCodeMessageError);

			ExportATF.US_PermitNumber = ZString.Empty;
			ExportATF.AddInfoValidation.ValidateUS_PermitExemptionCode();
			AssertNoMessageErrorContaining(ExportATF.US_PermitExemptionCodeInfo, USExportATFAddInfoValidation.EitherPermitNumberOrExemptionCodeCanBeSpecified);

			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			pivot.CD_ATFIndicator = OGAIndicatorList.Codes.Declared;
			var productExportATF = pivot.ExportATF;
			AssertNoMessageErrorContaining(productExportATF.US_PermitExemptionCodeInfo, ListValidation.InvalidCodeMessageError);
			productExportATF.US_PermitExemptionCode = "~";
			AssertHasMessageErrorContaining(productExportATF.US_PermitExemptionCodeInfo, ListValidation.InvalidCodeMessageError);
			productExportATF.US_PermitExemptionCode = ExemptionCodesCodeList.Codes._1;
			AssertNoMessageErrorContaining(productExportATF.US_PermitExemptionCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_Quantity()
		{
			ExportATF.AddInfoValidation.ValidateUS_Quantity();
			AssertHasMessageErrorContaining(ExportATF.US_QuantityInfo, MandatoryValidation.YouHaveNotEntered);

			ExportATF.US_Quantity = 123m;
			AssertNoMessageErrorContaining(ExportATF.US_QuantityInfo, MandatoryValidation.YouHaveNotEntered);

			ExportATF.InvoiceLine.US_ATFInd = ZString.Empty;
			ExportATF.US_Quantity = 0m;
			ExportATF.AddInfoValidation.ValidateUS_Quantity();
			AssertNoMessageErrorContaining(ExportATF.US_QuantityInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_CategoryCode()
		{
			ExportATF.US_CategoryCode = "~";
			AssertHasMessageErrorContaining(ExportATF.US_CategoryCodeInfo, ListValidation.InvalidCodeMessageError);

			ExportATF.US_CategoryCode = ATFCategoryCodeList.Codes.AW;
			AssertNoMessageErrorContaining(ExportATF.US_CategoryCodeInfo, ListValidation.InvalidCodeMessageError);
		}

		#region Implementation

		JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (fInvoiceLine == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
					var invoice = declaration.Invoices.AddNew();
					fInvoiceLine = invoice.InvoiceLines.AddNew();
					fInvoiceLine.US_ATFInd = OGAIndicatorList.Codes.Declared;
				}

				return fInvoiceLine;
			}
		}
		JobComInvoiceLine fInvoiceLine;

		ATF ExportATF
		{
			get
			{
				if (fExportATF == null || fExportATF.IsDeleted || fExportATF.IsDeleting)
				{
					fExportATF = InvoiceLine.ExportATF;
				}

				return fExportATF;
			}
		}
		ATF fExportATF;

		#endregion
	}
}
