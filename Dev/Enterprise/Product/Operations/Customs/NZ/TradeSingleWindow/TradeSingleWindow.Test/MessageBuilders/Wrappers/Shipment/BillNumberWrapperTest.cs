using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders.Testing
{
	class BillNumberWrapperTest : TestCaseWithFactory
	{
		public void TestBillNumberWrapper()
		{
			var houseBill = "292039482X";
			var wrappedBillNumber = new BillNumberWrapper(houseBill);
			AssertEquals("Bill Number returned using HB String constructor", "292039482X", wrappedBillNumber.BillNumber);
			AssertEquals("Bill Type", "HWB", wrappedBillNumber.BillType);
			var testShipment = Factory.NewWithValidTestData<ForwardingShipment>();
			testShipment.JS_HouseBill = "HB0029385";
			wrappedBillNumber = new BillNumberWrapper(testShipment);
			AssertEquals("BIll Number returned using Shipment constructor", "HB0029385", wrappedBillNumber.BillNumber);
			AssertEquals("Bill Type", "HWB", wrappedBillNumber.BillType);
		}
	}
}
