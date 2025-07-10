using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.AMS.Messaging.Business.StowPlan;
using Enterprise.Freight.Agency.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	[TestedType(typeof(StowPlanShipmentData))]
	class StowPlanShipmentDataTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var bill = Factory.New<BillOfLading>();
			var billData = new StowPlanShipmentData(bill);
			AssertEquals(true, billData.Checked);
		}

		public void TestSailingContainersCountChangedEvent()
		{
			var anotherFactory = new BusinessObjectFactory();
			var shipmentInAnotherFactory = anotherFactory.NewWithValidTestData<BillOfLading>();

			anotherFactory.Save();

			var shipment = Factory.Load<BillOfLading>(shipmentInAnotherFactory.PK);
			var shipmentData = new StowPlanShipmentData(shipment);
			AssertEquals(0, shipmentData.Containers.Count);

			var containerInAnotherFactory = shipmentInAnotherFactory.RealContainers.AddNew();
			containerInAnotherFactory.FillWithValidTestData();
			anotherFactory.Save();

			AssertEquals(1, shipmentData.Containers.Count);
			Assert(shipmentData.Containers[0].IsWrapperOf(containerInAnotherFactory));

			shipmentInAnotherFactory.RealContainers.RemoveAndDeleteAll();
			anotherFactory.Save();

			AssertEquals(0, shipmentData.Containers.Count);
		}

		public void TestIStowPlanShipmentDataMembers()
		{
			var bill = Factory.New<BillOfLading>();
			bill.JS_NKLoadPort = "AUSYD";
			bill.JS_NKDischargePort = "USLAX";
			bill.RealContainers.AddNew();

			IStowPlanShipmentData billData = new StowPlanShipmentData(bill);
			AssertEquals("AUSYD", billData.PortOfLading);
			AssertEquals("USLAX", billData.PortOfDischarge);
			AssertEquals(1, billData.Containers.Count());
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new StowPlanShipmentData(Factory.New<BillOfLading>());
		}
	}
}
