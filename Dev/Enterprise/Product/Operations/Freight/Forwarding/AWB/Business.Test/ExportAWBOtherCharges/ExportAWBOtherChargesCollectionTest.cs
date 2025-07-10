using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.AWB.Business.Testing
{
	[TestedType(typeof(ExportAWBOtherChargesCollection))]
	sealed class ExportAWBOtherChargesCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestSumOfByChargesDue()
		{
			var header = Factory.New<ExportAWBHeader>();
			var otherCharges = new ExportAWBOtherChargesCollection(header);

			var otherCharge = otherCharges.AddNew();
			otherCharge.EO_EntitlementCode = Core.Constants.AWB.EntitlementCode.Carrier;
			otherCharge.EO_Amount = 56.32M;

			otherCharge = otherCharges.AddNew();
			otherCharge.EO_EntitlementCode = Core.Constants.AWB.EntitlementCode.Agent;
			otherCharge.EO_Amount = 22.42M;

			otherCharge = otherCharges.AddNew();
			otherCharge.EO_EntitlementCode = Core.Constants.AWB.EntitlementCode.Carrier;
			otherCharge.EO_Amount = 99.99M;

			AssertEquals(156.31M, otherCharges.SumOf(ChargesDue.Carrier));
			AssertEquals(22.42M, otherCharges.SumOf(ChargesDue.Agent));
		}

		public void TestSumOfByPaymentTermAndChargesDue()
		{
			var aWBHeader = Factory.New<ExportAWBHeader>();
			var otherCharges = new ExportAWBOtherChargesCollection(aWBHeader);

			var otherCharge = otherCharges.AddNew();
			otherCharge.EO_PPDCLT = ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid;
			otherCharge.EO_EntitlementCode = Core.Constants.AWB.EntitlementCode.Agent;
			otherCharge.EO_Amount = 56.32M;

			otherCharge = otherCharges.AddNew();
			otherCharge.EO_PPDCLT = ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect;
			otherCharge.EO_EntitlementCode = Core.Constants.AWB.EntitlementCode.Agent;
			otherCharge.EO_Amount = 22.42M;

			otherCharge = otherCharges.AddNew();
			otherCharge.EO_PPDCLT = ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Prepaid;
			otherCharge.EO_EntitlementCode = Core.Constants.AWB.EntitlementCode.Agent;
			otherCharge.EO_Amount = 99.99M;

			otherCharge = otherCharges.AddNew();
			otherCharge.EO_PPDCLT = ExportAWBHeader.Constants.PrepaidCollect3CharCodes.Collect;
			otherCharge.EO_EntitlementCode = Core.Constants.AWB.EntitlementCode.Carrier;
			otherCharge.EO_Amount = 33.38M;

			AssertEquals(156.31M, otherCharges.SumOf(PaymentTerm.Prepaid, ChargesDue.Agent));
			AssertEquals(0M, otherCharges.SumOf(PaymentTerm.Prepaid, ChargesDue.Carrier));
			AssertEquals(22.42M, otherCharges.SumOf(PaymentTerm.Collect, ChargesDue.Agent));
			AssertEquals(33.38M, otherCharges.SumOf(PaymentTerm.Collect, ChargesDue.Carrier));
		}

		public void TestAllowNew()
		{
			var header = Factory.New<ExportAWBHeader>();
			var otherCharges = new ExportAWBOtherChargesCollection(header);

			Assert("No charges added, should always allow charges when writable", otherCharges.AllowNew);

			var otherCharge = otherCharges.AddNew();

			Assert("1 charge added, should always allow charges when writable", otherCharges.AllowNew);

			otherCharges.AddNew();
			otherCharges.AddNew();
			otherCharges.AddNew();
			otherCharges.AddNew();

			Assert("5 charges added, should always allow charges when writable", otherCharges.AllowNew);

			otherCharges.AddNew();

			Assert("6 charges added, should always allow charges when writable", otherCharges.AllowNew);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ExportAWBOtherChargesCollection(Factory.New<ExportAWBHeader>());
		}
	}
}
