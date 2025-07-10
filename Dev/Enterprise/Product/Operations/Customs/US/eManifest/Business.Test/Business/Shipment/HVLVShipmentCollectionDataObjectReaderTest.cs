using System.Linq;
using System.Reflection;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using NUnit.Framework;
using UniversalXml = Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	[TestedType(typeof(HVLVShipmentCollectionDataObjectReader))]
	sealed class HVLVShipmentCollectionDataObjectReaderTest : DataObjectCollectionReaderTest
	{
		public void TestShipmentCollectionDoesNotGetCreatedDuringDataTransferringAsItIsSlowAndUnnecessary()
		{
			var trip = ReadIntoTrip();
			var shipmentFieldInfo = typeof(Trip).GetField("shipments", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.GetField);
			AssertNull(@"trip.Shipments shouldn't get created, because it's an ActiveBusinessObjectCollection and system will maintain internal indexes when any of its child element is changed.
And this will severely slow down the data transfering process, and the collection is only used for databinding, which is not necessary for datatransfer", shipmentFieldInfo.GetValue(trip));
		}

		public override void TestReadIntoCollection()
		{
			var trip = ReadIntoTrip();
			AssertEquals("Should create 2 shipments", 2, trip.Shipments.Count);
			AssertContainsExactElementsInAnyOrder(new[] { "12345678", "87654321" }, trip.Shipments.Select(s => s.B0_MasterBillNumber));
		}

		public void TestReadIntoCollectionRemovesUnmatchedElements()
		{
			var trip = ReadIntoTrip();
			Factory.SaveForTesting();

			CombineAssertions(() =>
			{
				AssertEquals("Should create 2 shipments", 2, trip.Shipments.Count);
				AssertContainsExactElementsInAnyOrder(new[] { "12345678", "87654321" }, trip.Shipments.Select(s => s.B0_MasterBillNumber));
			});

			var dataObject = new UniversalXml.Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var subShipment = new UniversalXml.Shipment(DefaultDataObjectWriterStrategy.TestInstance);

			dataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalXml.Shipment>());
			dataObject.SubShipmentCollection.Add(subShipment);
			subShipment.SetSubShipmentCollection(() => new DataObjectList<UniversalXml.Shipment>());

			var consignmentSubShipment1 = new UniversalXml.Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			subShipment.SubShipmentCollection.Add(consignmentSubShipment1);
			consignmentSubShipment1.WayBillNumber = "12345678";

			var consignmentSubShipment2 = new UniversalXml.Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			subShipment.SubShipmentCollection.Add(consignmentSubShipment2);
			consignmentSubShipment2.WayBillNumber = "24680246";

			var shipment12345678 = trip.Shipments.Single(x => x.B0_MasterBillNumber == "12345678");

			using (trip.CreateTemporaryMasterBillToShipmentLookup())
			{
				var reader = new HVLVShipmentCollectionDataObjectReader(trip, dataObject, subShipment, new DummyLogger(), Factory, new[] { consignmentSubShipment1, consignmentSubShipment2 });
				reader.ReadIntoCollection();
			}

			CombineAssertions(() =>
			{
				AssertEquals("Should still have 2 shipments", 2, trip.Shipments.Count);
				AssertContainsExactElementsInAnyOrder("Unmatched shipment has been removed", new[] { "12345678", "24680246" }, trip.Shipments.Select(s => s.B0_MasterBillNumber));
				AssertEquals("Shipment with house bill 12345678 has been matched and is the same", shipment12345678.PK, trip.Shipments.Single(x => x.B0_MasterBillNumber == "12345678").PK);
			});
		}

		Trip ReadIntoTrip()
		{
			var trip = Factory.New<Trip>();
			var dataObject = new UniversalXml.Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			var subShipment = new UniversalXml.Shipment(DefaultDataObjectWriterStrategy.TestInstance);

			dataObject.SetSubShipmentCollection(() => new DataObjectList<UniversalXml.Shipment>());
			dataObject.SubShipmentCollection.Add(subShipment);
			subShipment.SetSubShipmentCollection(() => new DataObjectList<UniversalXml.Shipment>());

			var consignmentSubShipment1 = new UniversalXml.Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			subShipment.SubShipmentCollection.Add(consignmentSubShipment1);
			consignmentSubShipment1.WayBillNumber = "12345678";

			var consignmentSubShipment2 = new UniversalXml.Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			subShipment.SubShipmentCollection.Add(consignmentSubShipment2);
			consignmentSubShipment2.WayBillNumber = "87654321";

			using (trip.CreateTemporaryMasterBillToShipmentLookup())
			{
				var reader = new HVLVShipmentCollectionDataObjectReader(trip, dataObject, subShipment, new DummyLogger(), Factory, new[] { consignmentSubShipment1, consignmentSubShipment2 });
				reader.ReadIntoCollection();
			}
				
			return trip;
		}
	}
}
