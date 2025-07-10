using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	class FeeCusCodeDataValidationTest : Customs.Business.Testing.CusCodeDataValidationTest
	{
		[NUnit.Framework.TestDate(2009, 1, 1)]
		public void TestDefaultAndValidateCY_SelectedRateType()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryFilerCode = "XJ5";

			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "2204212000";
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_CustomsQuantity = 1500m;

			FeeCusCodeData wineFee = invoiceLine.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.Wines);
			AssertNotNull("Should have defaulted a Fee", wineFee);

			invoiceLine.RunPreSaveValidation();
			//users should select a rate type otherwise no wine fee would be calculated
			AssertHasMessageErrorContaining(wineFee.CY_SelectedRateTypeInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.US_TaxRateT = RateTypeList.Codes.Primary;
			AssertNoMessageErrorContaining(wineFee.CY_SelectedRateTypeInfo, MandatoryValidation.YouHaveNotEntered);

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			AssertNotEquals(0m, invoiceLine.CusEntryLine.WinesAmount);
		}

		public void TestValidateCY_SelectedRateType()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1010101010";
			tariff.UE_DateFrom = ZDateTime.Today.AddDays(-10);
			tariff.UE_DateTo = ZDateTime.Today.AddDays(+10);
			USCTariffDutyRate dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Wines;
			dutyRate.UD_TaxFeeComputationCode = "";
			dutyRate.UD_TaxFeeFlag = "1";
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceLine invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			FeeCusCodeData fee = invoiceLine.FeeCusCodes.AddNew(Core.Constants.USCustoms.FeeCodes.Wines);
			fee.CY_SelectedRateType = "P";
			AssertHasMessageErrorContaining(fee.CY_SelectedRateTypeInfo, MandatoryValidation.DoNotEntered);
			AssertNoMessageErrorContaining(fee.CY_SelectedRateTypeInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(fee.CY_SelectedRateTypeInfo, MandatoryValidation.YouHaveNotEntered);
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			fee = invoiceLine.FeeCusCodes.GetFirstElementHaving(Core.Constants.USCustoms.FeeCodes.Wines);
			fee.CY_SelectedRateType = "P";
			AssertHasMessageErrorContaining(fee.CY_SelectedRateTypeInfo, MandatoryValidation.DoNotEntered);
			AssertNoMessageErrorContaining(fee.CY_SelectedRateTypeInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(fee.CY_SelectedRateTypeInfo, MandatoryValidation.YouHaveNotEntered);
			dutyRate.UD_TaxFeeComputationCode = ComputationCodeList.Codes.SpecificSpecific;
			fee.CY_SelectedRateType = "P";
			AssertNoMessageErrorContaining(fee.CY_SelectedRateTypeInfo, MandatoryValidation.DoNotEntered);
			AssertNoMessageErrorContaining(fee.CY_SelectedRateTypeInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(fee.CY_SelectedRateTypeInfo, MandatoryValidation.YouHaveNotEntered);
			fee.CY_SelectedRateType = "Z";
			AssertNoMessageErrorContaining(fee.CY_SelectedRateTypeInfo, MandatoryValidation.DoNotEntered);
			AssertHasMessageErrorContaining(fee.CY_SelectedRateTypeInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(fee.CY_SelectedRateTypeInfo, MandatoryValidation.YouHaveNotEntered);
			fee.CY_SelectedRateType = "";
			AssertNoMessageErrorContaining(fee.CY_SelectedRateTypeInfo, MandatoryValidation.DoNotEntered);
			AssertNoMessageErrorContaining(fee.CY_SelectedRateTypeInfo, ListValidation.InvalidCodeMessageError);
			AssertHasMessageErrorContaining(fee.CY_SelectedRateTypeInfo, MandatoryValidation.YouHaveNotEntered);
			fee.CY_SelectedRateType = "S";
			AssertNoMessageErrorContaining(fee.CY_SelectedRateTypeInfo, MandatoryValidation.DoNotEntered);
			AssertNoMessageErrorContaining(fee.CY_SelectedRateTypeInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(fee.CY_SelectedRateTypeInfo, MandatoryValidation.YouHaveNotEntered);

			invoiceLine.US_TaxApply = TaxApplyList.Codes.Override;
			fee.CY_SelectedRateType = "";
			AssertNoMessageErrorContaining(fee.CY_SelectedRateTypeInfo, MandatoryValidation.DoNotEntered);
			AssertNoMessageErrorContaining(fee.CY_SelectedRateTypeInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(fee.CY_SelectedRateTypeInfo, MandatoryValidation.YouHaveNotEntered);
			fee.CY_SelectedRateType = "S";
			AssertHasMessageErrorContaining(fee.CY_SelectedRateTypeInfo, MandatoryValidation.DoNotEntered);
			AssertNoMessageErrorContaining(fee.CY_SelectedRateTypeInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(fee.CY_SelectedRateTypeInfo, MandatoryValidation.YouHaveNotEntered);

			dutyRate.UD_TaxFeeComputationCode = ComputationCodeList.Codes.SpecificSpecific;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			fee.CY_SelectedRateType = "";
			AssertNoMessageErrorContaining(fee.CY_SelectedRateTypeInfo, MandatoryValidation.DoNotEntered);
			AssertNoMessageErrorContaining(fee.CY_SelectedRateTypeInfo, ListValidation.InvalidCodeMessageError);
			AssertNoMessageErrorContaining(fee.CY_SelectedRateTypeInfo, MandatoryValidation.YouHaveNotEntered);
		}

		public void TestValidateCY_FeeAmount()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1010101010";
			tariff.UE_DateFrom = ZDateTime.Today.AddDays(-10);
			tariff.UE_DateTo = ZDateTime.Today.AddDays(+10);

			USCTariffDutyRate dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Wines;
			dutyRate.UD_TaxFeeComputationCode = ComputationCodeList.Codes.NoComputationFormulaAvailable;

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			JobComInvoiceLine invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			invoiceLine.US_TaxApply = TaxApplyList.Codes.Yes;
			invoiceLine.US_TaxCode = Core.Constants.USCustoms.FeeCodes.Wines;
			invoiceLine.US_TaxRateS = AppendixBTaxRateList.Codes.Wines_4;
			invoiceLine.US_TaxQty = 0m;

			FeeCusCodeData fee = invoiceLine.FeeCusCodes.AddNew(Core.Constants.USCustoms.FeeCodes.Wines);
			fee.Validation.ValidateCY_FeeAmount();
			AssertHasWarning(fee.CY_FeeAmountInfo, FeeCusCodeDataValidation.NoFeeFormulaAvailableMessage);
		}

		public void TestValidateAll()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "1010101010";
			tariff.UE_DateFrom = ZDateTime.Today.AddDays(-10);
			tariff.UE_DateTo = ZDateTime.Today.AddDays(+10);
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceLine invoiceLine = declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.JI_Tariff = tariff.UE_Tariff;
			USCTariffDutyRate dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Wines;
			dutyRate.UD_TaxFeeComputationCode = ComputationCodeList.Codes.SpecificSpecific;
			FeeCusCodeData fee = invoiceLine.FeeCusCodes.AddNew(Core.Constants.USCustoms.FeeCodes.Wines);
			fee.CY_SelectedRateType = "Z";
			Factory.Save();
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			FeeCusCodeData fee1 = newFactory.Load<FeeCusCodeData>(fee.PK);
			fee1.Validation.ValidateAll();
			AssertHasMessageErrorContaining(fee1.CY_SelectedRateTypeInfo, ListValidation.InvalidCodeMessageError);
			fee.CY_SelectedRateType = "P";
			Factory.Save();
			fee1.Validation.ValidateAll();
			AssertNoMessageErrorContaining(fee1.CY_SelectedRateTypeInfo, ListValidation.InvalidCodeMessageError);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			fee.CY_SelectedRateType = "Z";
			Factory.Save();
			fee1.Validation.ValidateAll();
			AssertNoMessageErrorContaining(fee1.CY_SelectedRateTypeInfo, ListValidation.InvalidCodeMessageError);
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
		}
	}
}
