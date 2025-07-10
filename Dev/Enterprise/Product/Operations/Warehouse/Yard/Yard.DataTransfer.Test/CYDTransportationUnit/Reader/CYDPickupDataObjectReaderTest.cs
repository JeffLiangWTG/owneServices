using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Testing.Core;
using Enterprise.Warehouse.Yard.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.DataTransfer.Universal.Test
{
	[TestedType(typeof(CYDPickupDataObjectReader))]
	public class CYDPickupDataObjectReaderTest : TestCaseWithUniversalObjectFactory
	{
		public void TestBooking_QuantityWithinTheLimit_NoException()
		{
			var shipment = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { IsForPickup = true, ContainerNumbers = new string[] { null, null } });
			var subShipment = shipment.SubShipmentCollection.FirstOrDefault();
			shipment.DataContext.AddDataSource(DataContextType.GateBooking, "GB00001");
			shipment.DataContext.AddDataSource(DataContextType.GateMovementBooking, "GBM00001");
			var containers = subShipment.ContainerCollection;
			var transportationUnit1 = Factory.NewWithValidTestData<CYDTransportationUnit>();
			var logger = new TestLogger(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
			var reader = new CYDPickupDataObjectReader(containers[0], "BKR01", "VHC01", transportationUnit1, shipment, logger, Factory);
			AssertNoExceptionThrown("Receive advice line should be null to throw exception.", () => reader.ReadIntoBusinessObject());
		}

		public void TestBooking_QuantityExceededTheLimit_ThrowException()
		{
			var shipment1 = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { IsForPickup = true, ContainerNumbers = new string[] { null } });
			var subShipment1 = shipment1.SubShipmentCollection.FirstOrDefault();
			shipment1.DataContext.AddDataSource(DataContextType.GateBooking, "GB00001");
			shipment1.DataContext.AddDataSource(DataContextType.GateMovementBooking, "GBM00001");
			var containers1 = subShipment1.ContainerCollection;
			var transportationUnit1 = Factory.NewWithValidTestData<CYDTransportationUnit>();
			var logger1 = new TestLogger(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
			var reader1 = new CYDPickupDataObjectReader(containers1[0], "BKR01", "VHC01", transportationUnit1, shipment1, logger1, Factory);
			reader1.ReadIntoBusinessObject();

			var shipment2 = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { IsForPickup = true, ContainerNumbers = new string[] { null } });
			var subShipment2 = shipment2.SubShipmentCollection.FirstOrDefault();
			shipment1.DataContext.AddDataSource(DataContextType.GateBooking, "GB00002");
			shipment1.DataContext.AddDataSource(DataContextType.GateMovementBooking, "GBM00002");
			var containers2 = subShipment2.ContainerCollection;
			var transportationUnit2 = Factory.NewWithValidTestData<CYDTransportationUnit>();
			var logger2 = new TestLogger(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
			var reader2 = new CYDPickupDataObjectReader(containers2[0], "BKR01", "VHC02", transportationUnit2, shipment2, logger2, Factory);

			AssertExceptionThrown(
				"Receive advice line should be null to throw exception.",
				typeof(DataObjectReadFailureException),
				"Release Advice Line (BKR01 / 20GP) is not found or has 0 balance quantity.",
				() => reader2.ReadIntoBusinessObject());
		}

		public void TestGateIn_WithBoundaryQuantity_NoException()
		{
			var shipment1 = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { IsForPickup = true, ContainerNumbers = new string[] { null } });
			var subShipment1 = shipment1.SubShipmentCollection.FirstOrDefault();
			shipment1.DataContext.AddDataSource(DataContextType.GateBooking, "GB00001");
			shipment1.DataContext.AddDataSource(DataContextType.GateMovementBooking, "GBM00001");
			var containers1 = subShipment1.ContainerCollection;
			var transportationUnit1 = Factory.NewWithValidTestData<CYDTransportationUnit>();
			var logger1 = new TestLogger(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
			var reader1 = new CYDPickupDataObjectReader(containers1[0], "BKR01", "VHC01", transportationUnit1, shipment1, logger1, Factory);
			reader1.ReadIntoBusinessObject();

			var shipment2 = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { IsForPickup = true, ContainerNumbers = new string[] { null }, IsDataSourceGateVehicleMovement = true, DataSourceKeys = ["GBM00005678"] });
			var subShipment2 = shipment1.SubShipmentCollection.FirstOrDefault();
			shipment2.DataContext.AddDataSource(DataContextType.GateVehicleMovement, "GVE00001");
			shipment2.DataContext.AddDataSource(DataContextType.GateBooking, "GB00001");
			shipment2.DataContext.AddDataSource(DataContextType.GateMovementBooking, "GBM00001");
			var containers2 = subShipment2.ContainerCollection;
			var transportationUnit2 = Factory.NewWithValidTestData<CYDTransportationUnit>();
			var logger2 = new TestLogger(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
			var reader2 = new CYDPickupDataObjectReader(containers2[0], "BKR01", "VHC02", transportationUnit2, shipment2, logger2, Factory);

			AssertNoExceptionThrown("Receive advice line should be null to throw exception.", () => reader2.ReadIntoBusinessObject());
		}

		UniversalTestData Data;

		protected override void SetUp()
		{
			base.SetUp();
			Data = new UniversalTestData(Factory, new TestErrorLogger());
			var today = ZDateTime.Today.Date;
			var tomorrow = ZDateTime.Today.AddDays(1).Date;
			Data.SetupDataForPickup("BKR01", "BKR01", today, tomorrow, "20GP", new string[] { "GENL" });
		}
	}
}
