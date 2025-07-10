using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ChargeValidationExtensionTest : TestCaseWithFactory
	{
		public void TestValidateAdjustedChargeHasAnotherChargeToAdjust()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			declaration.ResumeApportionment();
			InvoiceCharge charge = invoice.Charges.AddNew();
			charge.J7_ChargeType = USCustomsChargeTypeList.Codes.OverseasFreight;
			charge.J7_AdjustedCharge = true;
			declaration.ApportionmentDirty = false;
			charge.Validation.ValidateJ7_AdjustedCharge();
			AssertHasMessageError(charge.J7_AdjustedChargeInfo, ChargeValidationExtension.NoCorrespondingChargeExistForAdjustedCharge);
			InvoiceCharge charge2 = invoice.Charges.AddNew();
			charge2.J7_ChargeType = USCustomsChargeTypeList.Codes.OverseasFreight;
			charge.J7_AdjustedCharge = true;
			AssertNoMessageError(charge.J7_AdjustedChargeInfo, ChargeValidationExtension.NoCorrespondingChargeExistForAdjustedCharge);
		}

		public void TestValidateOverridenChargeIsDutiable()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			InvoiceCharge charge = invoice.Charges.AddNew();
			charge.J7_ChargeType = USCustomsChargeTypeList.Codes.OverseasFreight;
			InvoiceCharge charge2 = invoice.Charges.AddNew();
			charge2.J7_ChargeType = USCustomsChargeTypeList.Codes.OverseasFreight;
			charge.J7_AdjustedCharge = true;
			AssertHasMessageError(charge.J7_AdjustedChargeInfo, ChargeValidationExtension.CorrespondingChargeNotDutiable);
			InvoiceCharge charge3 = invoice.Charges.AddNew();
			charge3.J7_ChargeType = USCustomsChargeTypeList.Codes.OverseasFreight;
			charge3.J7_IsDutiable = true;
			charge.J7_AdjustedCharge = true;
			AssertNoMessageError(charge.J7_AdjustedChargeInfo, ChargeValidationExtension.CorrespondingChargeNotDutiable);
		}
	}
}
