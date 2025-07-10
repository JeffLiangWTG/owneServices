using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	public class USDEAConstituentAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_ProductCode()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(DEAConstituent.US_ProductCodeInfo);

			DEAConstituent.US_ProductCode = "123";
			AssertHasMessageError(DEAConstituent.US_ProductCodeInfo, USDEAConstituentAddInfoValidation.ProductCodeFormat);

			DEAConstituent.US_ProductCode = "123A";
			AssertHasMessageError(DEAConstituent.US_ProductCodeInfo, USDEAConstituentAddInfoValidation.ProductCodeFormat);

			DEAConstituent.US_ProductCode = "1234";
			AssertNoMessageError(DEAConstituent.US_ProductCodeInfo, USDEAConstituentAddInfoValidation.ProductCodeFormat);

			DEAConstituent.Header.InvoiceLine.Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(DEAConstituent.US_ProductCodeInfo);
		}

		public void TestUS_Weight()
		{
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(DEAConstituent.US_WeightInfo, "value cannot be zero.");
			ValidationTestHelper.AssertValueCannotBeNegativeMessageError(DEAConstituent.US_WeightInfo);

			DEAConstituent.US_Weight = 1m;
			AssertNoMessageErrorContaining(DEAConstituent.US_WeightInfo, USDEAConstituentAddInfoValidation.WeightShouldBeLessThanTenBillion);

			DEAConstituent.US_Weight = 1234567890123m;
			AssertHasMessageErrorContaining(DEAConstituent.US_WeightInfo, USDEAConstituentAddInfoValidation.WeightShouldBeLessThanTenBillion);

			DEAConstituent.Header.InvoiceLine.Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			ValidationTestHelper.AssertYouHaveNotEnteredMessageError(DEAConstituent.US_WeightInfo, "value cannot be zero.");
			ValidationTestHelper.AssertValueCannotBeNegativeMessageError(DEAConstituent.US_WeightInfo);
		}

		public void TestUS_WeightUQ()
		{
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(DEAConstituent.US_WeightUQInfo, "~", "G");

			DEAConstituent.Header.InvoiceLine.Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(DEAConstituent.US_WeightUQInfo, "~", "G");

			var pivot = Factory.New<CusClassPartPivot>();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTE;
			var productExportDEA = pivot.DEAHeaders.AddNew();
			var productExportConstituent = productExportDEA.Constituents.AddNew();
			AssertNoMessageErrorContaining(productExportConstituent.US_WeightUQInfo, ListValidation.InvalidCodeMessageError);
			productExportConstituent.US_WeightUQ = "~";
			AssertHasMessageErrorContaining(productExportConstituent.US_WeightUQInfo, ListValidation.InvalidCodeMessageError);
			productExportConstituent.US_WeightUQ = "G";
			AssertNoMessageErrorContaining(productExportConstituent.US_WeightUQInfo, ListValidation.InvalidCodeMessageError);
		}

		DEAConstituent DEAConstituent
		{
			get
			{
				if (fDEAConstituent == null)
				{
					var declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					declaration.US_EnableCRL = true;
					var invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
					fDEAConstituent = invoiceLine.DEAHeaders.AddNew().Constituents.AddNew();
				}
				return fDEAConstituent;
			}
		}
		DEAConstituent fDEAConstituent;
	}
}
