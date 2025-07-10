using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class JobComInvoiceHeaderFunctionalTest : Customs.Business.Testing.BaseJobComInvoiceHeaderFunctionalTest
	{
		#region Implementation
		protected override BaseJobDeclaration GetNewDeclaration()
		{
			return Factory.New<JobDeclaration>();
		}

		public override void TestIsIncludedInLinesForApportionedCharge()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value))
			{
				var groupHeader = declaration.JobComInvoiceGroupHeaders[0];
				var invoice = groupHeader.JobComInvoiceHeaders.AddNew();
				invoice.JZ_InvoiceAmount = 1000m;
				invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
				invoice.JZ_IncoTerm = "CIF";
				var charge = groupHeader.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight, 100, declaration.LocalCurrencyCode);
				declaration.ResumeApportionment();
				AssertEquals("JZ_Calc_FOBAmount", 900m, invoice.JZ_Calc_FOBAmount);
			}
		}

		public override void TestCalculateFOB_CIFNonDutiablePreFOB()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, ChargeDistributeByList.Codes.Value))
			{
				var currencyCode = declaration.LocalCurrencyCode;
				var currency = RefCurrency.LoadFromCurrencyCode(Factory, currencyCode);
				if (currency != null)
				{
					if (currency.RX_SubUnitRatio == 1)
					{
						currency.RX_SubUnitRatio = 100; // If the local country currency has 1 for the sub unit, then this test will fail (out by 10 cents) because of rounding to the nearest whole unit.  For the purpose of this test, pretend that minor units are allowed.  DJC.
					}
				}
				else
				{
					Assert("Can't run this test when currency code " + currencyCode + " is not in RefCurrency", false);
				}
				BaseJobComInvoiceHeader invoice = declaration.Invoices.AddNew();
				invoice.JZ_InvoiceNumber = "INV1";
				invoice.JZ_InvoiceAmount = 180848.58m;
				invoice.JZ_RX_NKInvoice_Currency = currencyCode;
				invoice.JZ_IncoTerm = invoice.IncotermEquivalentToCFRForTesting;

				BaseJobComInvHeaderCharge oFT = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight);
				oFT.J7_Amount = 9280m;
				oFT.J7_IsIncludedInITOT = false;

				BaseJobComInvHeaderCharge nonDutiableFIFT = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.ForeignInlandFreight);
				nonDutiableFIFT.J7_Amount = 2755.90m;
				nonDutiableFIFT.J7_IsDutiable = false;
				nonDutiableFIFT.J7_IsGSTApplicable = true;
				nonDutiableFIFT.J7_IsIncludedInITOT = true;

				var invoiceLine = (JobComInvoiceLine)declaration.FilteredInvoiceLines.AddNew();
				invoiceLine.JI_Calc_Invoice = invoice.JZ_InvoiceNumber;
				invoiceLine.JI_InvoiceQuantity = 1m;
				invoiceLine.JI_EnteredUnitPrice = 171568.58m;

				var fOBExpected = invoice.JZ_InvoiceAmount - oFT.J7_Amount - nonDutiableFIFT.J7_Amount;
				declaration.ResumeApportionment();
				AssertEquals("FOB value", fOBExpected, invoice.JZ_Calc_FOBAmount);
				AssertEquals("FOB line", fOBExpected, invoiceLine.JI_Calc_FOB);
			}
		}
		#endregion
	}
}
