using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class InvoiceChargeValidationTest : TestCaseWithFactory
	{
		public void TestCheckJ7_AdjustedCharge()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.ApportionmentDirty = false;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			InvoiceCharge charge = invoice.Charges.AddNew();
			charge.J7_ChargeType = Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight;
			charge.J7_AdjustedCharge = true;
			declaration.ApportionmentDirty = false;
			invoice.Charges.ResumeValidation();
			charge.Validation.ValidateJ7_AdjustedCharge();
			AssertHasMessageError(charge.J7_AdjustedChargeInfo, ChargeValidationExtension.NoCorrespondingChargeExistForAdjustedCharge);
			InvoiceCharge charge2 = invoice.Charges.AddNew();
			charge2.J7_ChargeType = Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight;
			charge.J7_AdjustedCharge = true;
			declaration.ApportionmentDirty = false;
			invoice.Charges.ResumeValidation();
			charge.Validation.ValidateJ7_AdjustedCharge();
			AssertNoMessageError(charge.J7_AdjustedChargeInfo, ChargeValidationExtension.NoCorrespondingChargeExistForAdjustedCharge);
		}

		public void TestExchangeRateValidation()
		{
			var audCurr = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Core.Constants.CurrencyCodes.Australia);
			audCurr.ExchangeRates.DeleteAll();
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_DateOfExport = new ZDateTime(2012, 1, 1);
			Assert(declaration.ValuationDatesChanged);
			var invoice = declaration.Invoices.AddNew();
			var charge = invoice.Charges.AddNew();
			charge.J7_Amount = 100m;
			charge.J7_RX_NKCurrency = audCurr.RX_Code;
			Assert(charge.J7_RX_NKCurrencyInfo.HasMessageErrors());
			audCurr.SetUpExchangeRates(new ZDateTime(2012, 1, 1), 1.05m);
			declaration.RefreshExchangeRates();
			Assert(!charge.J7_RX_NKCurrencyInfo.HasMessageErrors());
		}

		public void TestIsCIFComponentUsed()
		{
			Assert(new InvoiceChargeValidationForTest(Factory.New<InvoiceCharge>()).IsCIFComponentUsedExposed);
		}

		sealed class InvoiceChargeValidationForTest : InvoiceChargeValidation
		{
			public InvoiceChargeValidationForTest(InvoiceCharge invoiceLineCharge) : base(invoiceLineCharge)
			{
			}

			internal bool IsCIFComponentUsedExposed => IsCIFComponentUsed;
		}
	}
}
