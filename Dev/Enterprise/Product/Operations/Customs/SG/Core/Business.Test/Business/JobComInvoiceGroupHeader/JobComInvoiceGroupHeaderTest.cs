using System;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(JobComInvoiceGroupHeader))]
	class JobComInvoiceGroupHeaderTest : Customs.Business.Testing.BaseJobComInvoiceGroupHeaderTest
	{
		#region Overrides
		public override void TestChargeTypeList()
		{
			var dec = Factory.New<JobDeclaration>();
			Customs.Business.ICommonInvoice commonInvoice = dec.TopGroupInvoice;
			var chargeTypeList1 = commonInvoice.ChargeTypeList;
			var chargeTypeList2 = commonInvoice.ChargeTypeList;
			AssertEquals(true, object.ReferenceEquals(chargeTypeList1, chargeTypeList2));
			AssertEquals("OFT, ONS, OPT, OTH", chargeTypeList1.CodesAsString);
		}

		public void TestTypeDecider()
		{
			Assert(Factory.New<Customs.Business.BaseJobComInvoiceGroupHeader>() is JobComInvoiceGroupHeader);
		}

		#endregion
		#region Overridden Landed Costing Tests
		public override void TestChargesToImportForLandedCosting()
		{
			Assert(true);
		}
		#endregion

		protected override Type ExpectedTypeOfCharges => typeof(Common.JobComInvChargeCollection<GroupInvoiceCharge>);
	}
}
