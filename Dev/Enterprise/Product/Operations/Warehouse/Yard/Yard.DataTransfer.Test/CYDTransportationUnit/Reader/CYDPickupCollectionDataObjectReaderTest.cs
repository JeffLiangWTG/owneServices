using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.Warehouse.Yard.Business;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.DataTransfer.Universal.Test
{
	[TestedType(typeof(CYDPickupCollectionDataObjectReader))]
	public class CYDPickupCollectionDataObjectReaderTest : DataObjectCollectionReaderTest
	{
		public override void TestReadIntoCollection()
		{
			var today = ZDateTime.Today.Date;
			var tomorrow = ZDateTime.Today.AddDays(1).Date;
			new UniversalTestData(Factory, new TestErrorLogger()).SetupDataForPickup("BKR01", "BKR01", today, tomorrow, "20GP", new string[] { "GENL" });
			var shipment = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { ContainerNumbers = new string[] { "GENL" } });
			var subShipment = shipment.SubShipmentCollection.FirstOrDefault();
			var containers = subShipment.ContainerCollection;
			var transportationUnit = Factory.NewWithValidTestData<CYDTransportationUnit>();
			var logger = new TestLogger(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
			var reader = new CYDPickupCollectionDataObjectReader(containers, transportationUnit, subShipment, logger, Factory);
			reader.ReadIntoCollection();

			AssertMultilineASCIIEquals("logger.Logs", @"
Information - No matching Pickup found, creating new Pickup.
Information - Populating Pickup...
Information - Added CYDPickup from UniversalShipment.
".Trim(), logger.LogOutput);

			var pickupHeader = Factory.Load<CYDPickupHeader>(new ZQuery()).Single();
			AssertEquals("Pickup Header Job Number should be populated.", transportationUnit.YTU_WW_Yard, pickupHeader.YPH_WW_Yard);

			var pickups = Factory.Load<CYDPickup>(new ZQuery());
			AssertEquals(1, pickups.Length);
			AssertEquals("Pickup Transport Reference should be populated", "TREF240419151008", pickups[0].YPL_TransportReference);
			AssertEquals("Dispatch Transportation Unit should be populated", transportationUnit.PK, pickups[0].LinkedYardUnit.YUS_YTU_DispatchTransportationUnit);
		}

		public void TestReadPickUpDataObject_WhenReleaseAdviceLineIsNotFound_ThenThrowDataObjectReadFailureException()
		{
			var today = ZDateTime.Today.Date;
			var tomorrow = ZDateTime.Today.AddDays(1).Date;
			new UniversalTestData(Factory, new TestErrorLogger()).SetupDataForPickup("BKR01", "BKR01", today, tomorrow, "20GP", []);
			var shipment = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { IsForPickup = true, ContainerNumbers = new string[] { null } });
			var subShipment = shipment.SubShipmentCollection.FirstOrDefault();
			var containers = subShipment.ContainerCollection;
			var transportationUnit = Factory.NewWithValidTestData<CYDTransportationUnit>();
			var logger = new TestLogger(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
			var reader = new CYDPickupCollectionDataObjectReader(containers, transportationUnit, subShipment, logger, Factory);

			AssertExceptionThrown(
				"Receive advice line should be null to throw exception.",
				typeof(DataObjectReadFailureException),
				"Release Advice Line (BKR01 / 20GP) is not found or has 0 balance quantity.",
				() => reader.ReadIntoCollection());
		}

		public void TestReadPickUpDataObject_WhenBookingReferenceIsNotFound_ThenThrowDataObjectReadFailureException()
		{
			var today = ZDateTime.Today.Date;
			var tomorrow = ZDateTime.Today.AddDays(1).Date;
			new UniversalTestData(Factory, new TestErrorLogger()).SetupDataForPickup("BKR01", "BKR01", today, tomorrow, "20GP", []);
			var shipment = UniversalTestShipmentBuilder.GetShipment(new TestShipmentOptions { IsForPickup = true, ContainerNumbers = new string[] { null, null } });
			var subShipment = shipment.SubShipmentCollection.FirstOrDefault();
			subShipment.BookingConfirmationReference = "";
			var containers = subShipment.ContainerCollection;
			var transportationUnit = Factory.NewWithValidTestData<CYDTransportationUnit>();
			var logger = new TestLogger(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
			var reader = new CYDPickupCollectionDataObjectReader(containers, transportationUnit, subShipment, logger, Factory);

			AssertExceptionThrown(
				"Booking Confirmation Reference is required in UXML for pickup.",
				typeof(DataObjectReadFailureException),
				"Booking Confirmation Reference is required in UXML for pickup.",
				() => reader.ReadIntoCollection());
		}
	}
}
