using System;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NO.Business.Testing
{
	sealed class JobComInvoiceHeaderFunctionalTest : Customs.Business.Testing.BaseJobComInvoiceHeaderFunctionalTest
	{
		protected override BaseJobDeclaration GetNewDeclaration() => Factory.New<JobDeclaration>();

		protected override ZString OFTChargeDescription => NOInvoiceChargeTypesExport.Descriptions.OverseasFreight.ToString().ToUpper();

		#region Overseas carges (OFT/ONS) are dutiable in NO

		public override void TestCalculateFOB_CIFNonDutiablePreFOB()
		{
			using (CustomsDataRegistry.Instance.InvoiceChargesForExport.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, Common.ChargeDistributeByList.Codes.Value))
			{
				var currencyCode = declaration.LocalCurrencyCode;
				var currency = RefCurrency.LoadFromCurrencyCode(Factory, currencyCode);
				if (currency is null)
				{
					Assert("Can't run this test when currency code " + currencyCode + " is not in RefCurrency", false);
				}
				if (currency.RX_SubUnitRatio == 1)
				{
					currency.RX_SubUnitRatio = 100; // If the local country currency has 1 for the sub unit, then this test will fail (out by 10 cents) because of rounding to the nearest whole unit.  For the purpose of this test, pretend that minor units are allowed.  DJC.
				}
				var invoice = declaration.Invoices.AddNew();
				invoice.JZ_InvoiceNumber = "INV1";
				invoice.JZ_InvoiceAmount = 180848.58m;
				invoice.JZ_RX_NKInvoice_Currency = currencyCode;
				invoice.JZ_IncoTerm = invoice.IncotermEquivalentToCFRForTesting;

				var oFT = invoice.Charges.AddNew(CustomsChargeTypeList.Codes.OverseasFreight);
				oFT.J7_Amount = 9280m;

				var invoiceLine = declaration.FilteredInvoiceLines.AddNew();
				invoiceLine.JI_Calc_Invoice = invoice.JZ_InvoiceNumber;
				invoiceLine.JI_LinePrice = 171568.58m;

				ZDecimal fOBExpected = invoice.JZ_InvoiceAmount;
				declaration.ResumeApportionment();
				CombineAssertions(() =>
				{
					AssertEquals("FOB value", fOBExpected, invoice.JZ_Calc_FOBAmount);
					AssertEquals("FOB line", fOBExpected, invoiceLine.JI_Calc_FOB);
				});
			}
		}

		#endregion
	}
}
