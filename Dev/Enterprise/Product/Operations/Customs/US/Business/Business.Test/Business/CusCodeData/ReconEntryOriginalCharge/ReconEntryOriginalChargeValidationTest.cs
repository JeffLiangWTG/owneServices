using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ReconEntryOriginalChargeValidationTest : TestCaseWithFactory
	{
		public void TestCheckCY_Code()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "2203.00.00 60";
			invoiceLine.US_UC_NKCountryOfOrigin = "NZ";
			invoiceLine.US_UC_NKCountryOfExport = "NZ";
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_CustomsQuantity = 15000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

			AssertEquals("Excise Tax", 2300.84m, declaration.CustomsEntryHeaders[0].TotalEstimatedTax);
			Factory.Save();

			ReconDeclaration reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.ImportLines(new JobDeclaration[] { declaration });
			AssertEquals("1 original entry created", 1, reconDeclaration.OriginalEntries.Count);
			reconDeclaration.OriginalEntries[0].OriginalCharges.RemoveAndDeleteAll();
			reconDeclaration.OriginalEntries[0].OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.OtherExcise, 100m);
			AssertNoMessageError(reconDeclaration.OriginalEntries[0].OriginalCharges[0].CY_CodeInfo, ReconEntryOriginalChargeValidation.CodeCannotBeDuplicated);

			reconDeclaration.OriginalEntries[0].OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.OtherExcise, 100m);
			AssertHasMessageError(reconDeclaration.OriginalEntries[0].OriginalCharges[1].CY_CodeInfo, ReconEntryOriginalChargeValidation.CodeCannotBeDuplicated);
		}

		public void TestCheckCY_SelectedRateType()
		{
			USCTariff tariff = Factory.New<USCTariff>();
			tariff.UE_Tariff = "01010101";
			tariff.UE_DutyComputationCode = ComputationCodeList.Codes.SpecificSpecific;
			tariff.UE_DateFrom = new ZDateTime(2008, 9, 11);
			tariff.UE_DateTo = ZDateTime.Today.AddDays(1);

			USCTariffDutyRate dutyRate = tariff.DutyRates.AddNew();
			dutyRate.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.Wines;
			dutyRate.UD_TaxFeeComputationCode = ComputationCodeList.Codes.SpecificSpecific;

			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			declaration.US_EnableENS = true;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;

			JobComInvoiceLine invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_Tariff = "01010101";
			invoiceLine.US_UC_NKCountryOfOrigin = "NZ";
			invoiceLine.US_UC_NKCountryOfExport = "NZ";
			invoiceLine.JI_LinePrice = 10000m;
			invoiceLine.JI_CustomsQuantity = 15000m;

			declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());
			Factory.Save();

			ReconDeclaration reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			reconDeclaration.ImportLines(new JobDeclaration[] { declaration });
			AssertEquals("1 original entry created", 1, reconDeclaration.OriginalEntries.Count);

			ReconEntryOriginalCharge charge = invoiceLine.ReconOriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Wines, 100m);
			charge.CY_SelectedRateType = "X";
			AssertHasMessageErrorContaining(charge.CY_SelectedRateTypeInfo, ListValidation.InvalidCodeMessageError);

			invoiceLine.US_R_OrigTaxApply = TaxApplyList.Codes.Override;
			charge.CY_SelectedRateType = "";
			AssertNoMessageErrorContaining(charge.CY_SelectedRateTypeInfo, MandatoryValidation.DoNotEntered);
			charge.CY_SelectedRateType = RateTypeList.Codes.Secondary;
			AssertHasMessageErrorContaining(charge.CY_SelectedRateTypeInfo, MandatoryValidation.DoNotEntered);

			dutyRate.UD_TaxFeeComputationCode = ComputationCodeList.Codes.AdValorem;
			charge.CY_SelectedRateType = RateTypeList.Codes.Primary;
			AssertHasMessageErrorContaining(charge.CY_SelectedRateTypeInfo, MandatoryValidation.DoNotEntered);

			charge.CY_SelectedRateType = "";
			AssertNoMessageErrorContaining(charge.CY_SelectedRateTypeInfo, MandatoryValidation.DoNotEntered);
			AssertNoMessageErrorContaining(charge.CY_SelectedRateTypeInfo, ListValidation.InvalidCodeMessageError);
		}

		public void TestCheckCY_AmountWithNoLineDetailsChecked()
		{
			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			var originalEntry = reconDec.OriginalEntries.AddNew();
			var invoiceLine0 = originalEntry.Invoice.JobComInvoiceLines.AddNew();
			invoiceLine0.US_R_OrigMPFAmount = 60m;
			var originalEntryMPFAmount = originalEntry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 100m);
			originalEntryMPFAmount.CY_Code = Core.Constants.USCustoms.FeeCodes.HMF;
			originalEntryMPFAmount.Validation.ValidateCY_Amount();
			AssertHasMessageErrorContaining(originalEntryMPFAmount.CY_AmountInfo, ReconEntryOriginalChargeValidation.TotalFeeDoesNotMatch);

			originalEntry.US_R_NoLineDetails = ZBool.True;
			originalEntryMPFAmount.Validation.ValidateCY_Amount();
			AssertNoMessageErrorContaining(originalEntryMPFAmount.CY_AmountInfo, ReconEntryOriginalChargeValidation.TotalFeeDoesNotMatch);
		}

		public void TestCheckCY_AmountWhenChangedLinesOnlyChecked()
		{
			var reconDec = new ReconDeclaration(Factory.New<JobDeclaration>());
			var entry = reconDec.OriginalEntries.AddNew();
			var fee = entry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.Blueberry, 60m);
			fee.Validation.ValidateCY_Amount();
			AssertHasMessageErrorContaining(fee.CY_AmountInfo, ReconEntryOriginalChargeValidation.TotalFeeDoesNotMatch);

			fee.CY_Code = Core.Constants.USCustoms.FeeCodes.MPC;
			fee.Validation.ValidateCY_Amount();
			AssertNoMessageErrorContaining(fee.CY_AmountInfo, ReconEntryOriginalChargeValidation.TotalFeeDoesNotMatch);

			entry.US_R_ChangedLinesOnly = ZBool.True;
			fee.CY_Code = Core.Constants.USCustoms.FeeCodes.Coffee;
			fee.Validation.ValidateCY_Amount();
			AssertNoMessageErrorContaining(fee.CY_AmountInfo, ReconEntryOriginalChargeValidation.TotalFeeDoesNotMatch);

			fee.CY_Code = Core.Constants.USCustoms.FeeCodes.MPC;
			fee.CY_Amount = 0m;
			AssertNoMessageErrorContaining(fee.CY_AmountInfo, ReconEntryOriginalChargeValidation.EnterNumberGreaterThanZero);

			entry.OriginalCharges.AddNew(Core.Constants.USCustoms.FeeCodes.MerchandiseProcessing, 60m);
			fee.Validation.ValidateCY_Amount();
			AssertHasMessageErrorContaining(fee.CY_AmountInfo, ReconEntryOriginalChargeValidation.EnterNumberGreaterThanZero);
		}

		protected override void SetUp()
		{
			base.SetUp();
			CusFeeCodeConstantsTestHelper.CreateCusFeeCodeDescriptionPairListForTest();
		}
	}
}
