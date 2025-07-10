using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing
{
	[TestedType(typeof(InvoiceCharge))]
	sealed class InvoiceChargeTest : Customs.Business.Testing.BaseInvoiceChargeTest
	{
		public override void TestIsIncludedInLinesReadOnly()
		{
			var invoice = testDec.Invoices.AddNew();
			invoice.JZ_IncoTerm = "FOB";

			var aDD = invoice.Charges.AddNew();
			aDD.J7_ChargeType = CustomsChargeTypeList.Codes.AdditionCharge;

			var oFT = invoice.Charges.AddNew();
			oFT.J7_ChargeType = CustomsChargeTypeList.Codes.OverseasFreight;

			var oTH = invoice.Charges.AddNew();
			oTH.J7_ChargeType = CustomsChargeTypeList.Codes.OtherCharges;

			AssertEquals("ADD Included in ITOT should be readonly", false, aDD.J7_IsIncludedInITOTInfo.ReadOnly);
			AssertEquals("OFT Included in ITOT should be readonly", true, oFT.J7_IsIncludedInITOTInfo.ReadOnly);
			AssertEquals("OTH Included in ITOT should be readonly", true, oTH.J7_IsIncludedInITOTInfo.ReadOnly);
		}
	}
}
