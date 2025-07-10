using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class InvoiceLineChargeValidationTest : TestCaseWithFactory
	{
		public void TestCheckJ7_AdjustedCharge()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.ApportionmentDirty = false;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 10000m;
			invoice.JZ_RX_NKInvoice_Currency = JobDeclaration.LocalCurrencyConstantCode;
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 10000m;
			InvoiceLineCharge charge = invoiceLine.Charges.AddNew();
			charge.J7_ChargeType = USCustomsChargeTypeList.Codes.OverseasFreight;
			charge.J7_AdjustedCharge = true;
			declaration.ApportionmentDirty = false;
			charge.Validation.ValidateJ7_AdjustedCharge();
			AssertHasMessageError(charge.J7_AdjustedChargeInfo, ChargeValidationExtension.NoCorrespondingChargeExistForAdjustedCharge);
			InvoiceLineCharge charge2 = invoiceLine.Charges.AddNew();
			charge2.J7_ChargeType = USCustomsChargeTypeList.Codes.OverseasFreight;
			charge.J7_AdjustedCharge = true;
			declaration.ResumeApportionment();
			declaration.ApportionmentDirty = false;
			invoice.Charges.ResumeValidation();
			AssertNoMessageError(charge.J7_AdjustedChargeInfo, ChargeValidationExtension.NoCorrespondingChargeExistForAdjustedCharge);
		}

		public void TestIsCIFComponentUsed()
		{
			Assert(new InvoiceLineChargeValidationForTest(Factory.New<InvoiceLineCharge>()).IsCIFComponentUsedExposed);
		}

		sealed class InvoiceLineChargeValidationForTest : InvoiceLineChargeValidation
		{
			public InvoiceLineChargeValidationForTest(InvoiceLineCharge invoiceLineCharge) : base(invoiceLineCharge)
			{
			}

			internal bool IsCIFComponentUsedExposed => IsCIFComponentUsed;
		}
	}
}
