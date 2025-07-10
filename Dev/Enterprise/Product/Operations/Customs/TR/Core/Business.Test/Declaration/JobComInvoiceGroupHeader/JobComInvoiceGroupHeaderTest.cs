using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Business.Declaration.Testing
{
	[TestedType(typeof(JobComInvoiceGroupHeader))]
	class JobComInvoiceGroupHeaderTest : EU.Business.Declaration.Testing.JobComInvoiceGroupHeaderTest
	{
		public void TestCharges()
		{
			var dec = Factory.New<JobDeclaration>();
			AssertType<GroupInvoiceChargeCollection<GroupInvoiceCharge>>(dec.TopGroupInvoice.Charges);
		}

		public override void TestChargesToImportForLandedCosting()
		{
			Assert(true);
		}

		public override void TestChargeTypeList()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;

			ICommonInvoice commonInvoice = dec.TopGroupInvoice;
			var chargeTypeList1 = commonInvoice.ChargeTypeList;

			AssertContainsExactElementsInAnyOrder(
				new[] { "COM", "DEM", "INT", "LBC", "LCC", "LDC", "LEC", "LOT", "LPC", "LRU", "LSC", "LTC", "OBS", "OFT", "ONS", "OTH", "ROY", "SUR", "TFC" },
				chargeTypeList1.GetAllCodes()
			);

			dec.JE_MessageType = JobMessageTypeList.Codes.Export;

			ICommonInvoice commonInvoice2 = dec.TopGroupInvoice;
			var chargeTypeList3 = commonInvoice2.ChargeTypeList;

			AssertContainsExactElementsInAnyOrder(
				new[] { "COM", "DEM", "INT", "LBC", "LCC", "LDC", "LEC", "LOT", "LPC", "LRU", "LSC", "LTC", "OBS", "OFT", "ONS", "OTH", "ROY", "SUR", "TFC" },
				chargeTypeList3.GetAllCodes()
			);
		}

		protected override Type ExpectedTypeOfCharges => typeof(GroupInvoiceChargeCollection<GroupInvoiceCharge>);
	}
}
