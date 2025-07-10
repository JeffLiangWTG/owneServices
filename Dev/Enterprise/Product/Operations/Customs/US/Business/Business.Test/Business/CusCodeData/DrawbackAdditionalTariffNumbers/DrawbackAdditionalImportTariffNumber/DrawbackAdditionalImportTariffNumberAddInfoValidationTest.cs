using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class DrawbackAdditionalImportTariffNumberAddInfoValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckUS_Tariff()
		{
			ZString tariffCode = "111122XX";
			AdditionalImportTariff.US_Tariff = tariffCode;
			AssertNull("Tarriff should not exist", Factory.LoadTop1<USCTariff>(new ZQuery(USCTariffSchema.UE_Tariff, tariffCode)));
			AssertHasMessageErrorContaining(AdditionalImportTariff.US_TariffInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(AdditionalImportTariff.US_TariffInfo, MandatoryValidation.YouHaveNotEntered);
			USCTariff uSCTariff = Factory.New<USCTariff>();
			uSCTariff.UE_Tariff = tariffCode;
			uSCTariff.UE_Unit1 = "KG";
			uSCTariff.UE_ShortDescription = "HTS ITEM";
			AdditionalImportTariff.US_Tariff = "";
			AssertHasMessageErrorContaining(AdditionalImportTariff.US_TariffInfo, MandatoryValidation.YouHaveNotEntered);
			AdditionalImportTariff.US_Tariff = tariffCode;
			AssertNoMessageErrorContaining(AdditionalImportTariff.US_TariffInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckUS_Description()
		{
			AdditionalImportTariff.US_Description = ZString.Empty;
			AssertHasMessageErrorContaining(AdditionalImportTariff.US_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);
			AdditionalImportTariff.US_Description = "~";
			AssertNoMessageErrorContaining(AdditionalImportTariff.US_DescriptionInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestCheckUS_UQ1()
		{
			AdditionalImportTariff.US_UQ1 = "~";
			AssertHasMessageErrorContaining(AdditionalImportTariff.US_UQ1Info, ListValidation.InvalidCodeMessageError);
			AdditionalImportTariff.US_UQ1 = ACEDrawbackUnitOfMeasureList.Codes.AE;
			AssertNoMessageErrorContaining(AdditionalImportTariff.US_UQ1Info, ListValidation.InvalidCodeMessageError);
			AssertHasMessageError(AdditionalImportTariff.US_UQ1Info, DrawbackAdditionalImportTariffNumberAddInfoValidation.UQNotMatch);
			InvoiceLine.DRWImportUQ = ACEDrawbackUnitOfMeasureList.Codes.AE;
			AdditionalImportTariff.AddInfoValidation.ValidateUS_UQ1();
			AssertNoMessageError(AdditionalImportTariff.US_UQ1Info, DrawbackAdditionalImportTariffNumberAddInfoValidation.UQNotMatch);
		}

		public void TestCheckUS_UQ2()
		{
			AdditionalImportTariff.US_UQ2 = "~";
			AssertHasMessageErrorContaining(AdditionalImportTariff.US_UQ2Info, ListValidation.InvalidCodeMessageError);
			AdditionalImportTariff.US_UQ2 = ACEDrawbackUnitOfMeasureList.Codes.AE;
			AssertNoMessageErrorContaining(AdditionalImportTariff.US_UQ2Info, ListValidation.InvalidCodeMessageError);
			AssertHasMessageError(AdditionalImportTariff.US_UQ2Info, DrawbackAdditionalImportTariffNumberAddInfoValidation.UQNotMatch);
			InvoiceLine.DRWImportUQ2 = ACEDrawbackUnitOfMeasureList.Codes.AE;
			AdditionalImportTariff.AddInfoValidation.ValidateUS_UQ2();
			AssertNoMessageError(AdditionalImportTariff.US_UQ2Info, DrawbackAdditionalImportTariffNumberAddInfoValidation.UQNotMatch);
		}

		public void TestCheckUS_UQ3()
		{
			AdditionalImportTariff.US_UQ3 = "~";
			AssertHasMessageErrorContaining(AdditionalImportTariff.US_UQ3Info, ListValidation.InvalidCodeMessageError);
			AdditionalImportTariff.US_UQ3 = ACEDrawbackUnitOfMeasureList.Codes.AE;
			AssertNoMessageErrorContaining(AdditionalImportTariff.US_UQ3Info, ListValidation.InvalidCodeMessageError);
			AssertHasMessageError(AdditionalImportTariff.US_UQ3Info, DrawbackAdditionalImportTariffNumberAddInfoValidation.UQNotMatch);
			InvoiceLine.DRWImportUQ3 = ACEDrawbackUnitOfMeasureList.Codes.AE;
			AdditionalImportTariff.AddInfoValidation.ValidateUS_UQ3();
			AssertNoMessageError(AdditionalImportTariff.US_UQ3Info, DrawbackAdditionalImportTariffNumberAddInfoValidation.UQNotMatch);
		}

		public void TestCheckUS_SubstitutedValue1()
		{
			AdditionalImportTariff.AddInfoValidation.ValidateUS_SubstitutedValue1();
			AssertNoMessageError(AdditionalImportTariff.US_SubstitutedValue1Info, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueRequired);
			AssertNoMessageError(AdditionalImportTariff.US_SubstitutedValue1Info, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueNotRequired);
			Declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._52;
			InvoiceLine.DRWImportQuantity = 100m;
			AdditionalImportTariff.AddInfoValidation.ValidateUS_SubstitutedValue1();
			AssertHasMessageError(AdditionalImportTariff.US_SubstitutedValue1Info, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueRequired);
			AssertNoMessageError(AdditionalImportTariff.US_SubstitutedValue1Info, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueNotRequired);
			AdditionalImportTariff.US_SubstitutedValue1 = 100m;
			AssertNoMessageError(AdditionalImportTariff.US_SubstitutedValue1Info, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueRequired);
			AssertNoMessageError(AdditionalImportTariff.US_SubstitutedValue1Info, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueNotRequired);
			Declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._01;
			AdditionalImportTariff.AddInfoValidation.ValidateUS_SubstitutedValue1();
			AssertNoMessageError(AdditionalImportTariff.US_SubstitutedValue1Info, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueRequired);
			AssertHasMessageError(AdditionalImportTariff.US_SubstitutedValue1Info, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueNotRequired);
			AdditionalImportTariff.US_SubstitutedValue1 = 0m;
			AssertNoMessageError(AdditionalImportTariff.US_SubstitutedValue1Info, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueRequired);
			AssertNoMessageError(AdditionalImportTariff.US_SubstitutedValue1Info, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueNotRequired);
		}

		public void TestCheckUS_SubstitutedValue2()
		{
			AdditionalImportTariff.AddInfoValidation.ValidateUS_SubstitutedValue2();
			AssertNoMessageError(AdditionalImportTariff.US_SubstitutedValue2Info, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueRequired);
			AssertNoMessageError(AdditionalImportTariff.US_SubstitutedValue2Info, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueNotRequired);
			Declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._52;
			InvoiceLine.DRWImportQuantity2 = 100m;
			AdditionalImportTariff.AddInfoValidation.ValidateUS_SubstitutedValue2();
			AssertHasMessageError(AdditionalImportTariff.US_SubstitutedValue2Info, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueRequired);
			AssertNoMessageError(AdditionalImportTariff.US_SubstitutedValue2Info, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueNotRequired);
			AdditionalImportTariff.US_SubstitutedValue2 = 100m;
			AssertNoMessageError(AdditionalImportTariff.US_SubstitutedValue2Info, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueRequired);
			AssertNoMessageError(AdditionalImportTariff.US_SubstitutedValue2Info, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueNotRequired);
			Declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._01;
			AdditionalImportTariff.AddInfoValidation.ValidateUS_SubstitutedValue2();
			AssertNoMessageError(AdditionalImportTariff.US_SubstitutedValue2Info, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueRequired);
			AssertHasMessageError(AdditionalImportTariff.US_SubstitutedValue2Info, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueNotRequired);
			AdditionalImportTariff.US_SubstitutedValue2 = 0m;
			AssertNoMessageError(AdditionalImportTariff.US_SubstitutedValue2Info, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueRequired);
			AssertNoMessageError(AdditionalImportTariff.US_SubstitutedValue2Info, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueNotRequired);
		}

		public void TestCheckUS_SubstitutedValue3()
		{
			AdditionalImportTariff.AddInfoValidation.ValidateUS_SubstitutedValue3();
			AssertNoMessageError(AdditionalImportTariff.US_SubstitutedValue3Info, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueRequired);
			AssertNoMessageError(AdditionalImportTariff.US_SubstitutedValue3Info, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueNotRequired);
			Declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._52;
			InvoiceLine.DRWImportQuantity3 = 100m;
			AdditionalImportTariff.AddInfoValidation.ValidateUS_SubstitutedValue3();
			AssertHasMessageError(AdditionalImportTariff.US_SubstitutedValue3Info, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueRequired);
			AssertNoMessageError(AdditionalImportTariff.US_SubstitutedValue3Info, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueNotRequired);
			AdditionalImportTariff.US_SubstitutedValue3 = 100m;
			AssertNoMessageError(AdditionalImportTariff.US_SubstitutedValue3Info, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueRequired);
			AssertNoMessageError(AdditionalImportTariff.US_SubstitutedValue3Info, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueNotRequired);
			Declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._01;
			AdditionalImportTariff.AddInfoValidation.ValidateUS_SubstitutedValue3();
			AssertNoMessageError(AdditionalImportTariff.US_SubstitutedValue3Info, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueRequired);
			AssertHasMessageError(AdditionalImportTariff.US_SubstitutedValue3Info, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueNotRequired);
			AdditionalImportTariff.US_SubstitutedValue3 = 0m;
			AssertNoMessageError(AdditionalImportTariff.US_SubstitutedValue3Info, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueRequired);
			AssertNoMessageError(AdditionalImportTariff.US_SubstitutedValue3Info, ACEDrawbackAddInfoJobComInvoiceLineValidation.SubstitutedValueNotRequired);
		}

		public void TestCheckUS_Quantity1()
		{
			AdditionalImportTariff.US_Quantity1 = 1m;
			AssertHasMessageError(AdditionalImportTariff.US_Quantity1Info, DrawbackAdditionalImportTariffNumberAddInfoValidation.QuantityShouldNotBeEntered);
			AdditionalImportTariff.US_Quantity1 = 0m;
			AssertNoMessageError(AdditionalImportTariff.US_Quantity1Info, DrawbackAdditionalImportTariffNumberAddInfoValidation.QuantityShouldNotBeEntered);
		}

		public void TestCheckUS_Quantity2()
		{
			AdditionalImportTariff.US_Quantity2 = 1m;
			AssertHasWarning(AdditionalImportTariff.US_Quantity2Info, DrawbackAdditionalImportTariffNumberAddInfoValidation.QuantityShouldNotBeEntered);
			AdditionalImportTariff.US_Quantity2 = 0m;
			AssertNoWarning(AdditionalImportTariff.US_Quantity2Info, DrawbackAdditionalImportTariffNumberAddInfoValidation.QuantityShouldNotBeEntered);
		}

		public void TestCheckUS_Quantity3()
		{
			AdditionalImportTariff.US_Quantity3 = 1m;
			AssertHasWarning(AdditionalImportTariff.US_Quantity3Info, DrawbackAdditionalImportTariffNumberAddInfoValidation.QuantityShouldNotBeEntered);
			AdditionalImportTariff.US_Quantity3 = 0m;
			AssertNoWarning(AdditionalImportTariff.US_Quantity3Info, DrawbackAdditionalImportTariffNumberAddInfoValidation.QuantityShouldNotBeEntered);
		}

		public void TestCheckUS_AllowableQty1()
		{
			AdditionalImportTariff.US_AllowableQty1 = 1m;
			AssertHasMessageError(AdditionalImportTariff.US_AllowableQty1Info, DrawbackAdditionalImportTariffNumberAddInfoValidation.QuantityShouldNotBeEntered);
			AdditionalImportTariff.US_AllowableQty1 = 0m;
			AssertNoMessageError(AdditionalImportTariff.US_AllowableQty1Info, DrawbackAdditionalImportTariffNumberAddInfoValidation.QuantityShouldNotBeEntered);
		}

		public void TestCheckUS_ValuePerUnit1()
		{
			InvoiceLine.JI_Tariff = "910231450";
			InvoiceLine.US_DRWClaimAmountOverriden_New = true;
			InvoiceLine.DRWImportQuantity = 100;
			InvoiceLine.DeclaredVFD = 300;
			InvoiceLine.DRWGoodsValuePerUQ = 1.7m;
			AdditionalImportTariff.US_ValuePerUnit1 = 1m;
			AssertHasWarning(AdditionalImportTariff.US_ValuePerUnit1Info, "Value Per Unit(1.7+1=2.7) doesn't match Entered Value/Import Quantity(300/100=3).");
			AdditionalImportTariff.US_ValuePerUnit1 = 1.3m;
			AssertNoWarning(AdditionalImportTariff.US_ValuePerUnit1Info, "Value Per Unit(1.7+1=2.7) doesn't match Entered Value/Import Quantity(300/100=3).");
		}

		JobDeclaration declaration;
		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
					declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
				}

				return declaration;
			}
		}

		JobComInvoiceLine invoiceLine;
		JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (invoiceLine == null)
				{
					var invoiceHeader = Declaration.Invoices.AddNew();
					invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
				}

				return invoiceLine;
			}
		}

		DrawbackAdditionalImportTariffNumber additionalImportTariff;
		DrawbackAdditionalImportTariffNumber AdditionalImportTariff => additionalImportTariff ?? (additionalImportTariff = InvoiceLine.DrawbackAdditionalImportTariffNumbers.AddNew());
	}
}
