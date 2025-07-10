using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using ShipmentDocumentDataStoreNames = Enterprise.Freight.Forwarding.Documents.ShipmentDocumentDataStoreNames;

namespace Enterprise.eTail.Documents.Business.Testing.US.Supporters
{
	public class HVLVConsignmentDocDataObjectProviderTest : TestCaseWithFactory
	{
		public void TestHVLVConsignmentDocDataObjectProviderTest()
		{
			var parameters = new DocDataObjectParameters(ShipmentDocumentNames.AdvancedCargoReport, ShipmentDocumentDataStoreNames.AdvancedCargoReportUS);
			var shipment = Factory.New<ForwardingShipment>();
			var docDataObjectProvider = new HVLVConsignmentDocDataObjectProvider(shipment);

			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			var dataObject = docDataObjectProvider.GetDocDataObject(consignment, DocumentVisualizer.Integration.DataContext.HVLVConsignment, parameters);

			AssertNotNull("Expected dataObject not to be null", dataObject);
		}

		public void TestWhenConsignmentHasNoShipment_ThenDataObjectIsNull()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var docDataObjectProvider = new HVLVConsignmentDocDataObjectProvider(shipment);
			var parameters = new DocDataObjectParameters(ShipmentDocumentNames.AdvancedCargoReport, ShipmentDocumentDataStoreNames.AdvancedCargoReportUS);

			var bookingHeader = Factory.New<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();

			var dataObject = docDataObjectProvider.GetDocDataObject(consignment, DocumentVisualizer.Integration.DataContext.HVLVConsignment, parameters);

			AssertEquals("Precondition: Expected consignment not to have a shipment", ZGuid.Empty, consignment.HVC_JS_ManifestedOnShipment);
			AssertNull("Expected dataObject to be null, as the consignment has no shipment", dataObject);
		}

		public void TestGetHVLVConsignmentsWithItemLoadedOnShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var otherShipment = Factory.New<ForwardingShipment>();

			var consignment = Factory.New<HVLVConsignment>();
			consignment.Items.AddNew().HVI_JS_LoadedOnShipment = shipment.PK;

			var consignmentLoadedOnOtherShipment = Factory.New<HVLVConsignment>();
			consignmentLoadedOnOtherShipment.Items.AddNew().HVI_JS_LoadedOnShipment = otherShipment.PK;

			var consignmentPartiallyLoadedOnShipment = Factory.New<HVLVConsignment>();
			consignmentPartiallyLoadedOnShipment.Items.AddNew().HVI_JS_LoadedOnShipment = shipment.PK;
			consignmentPartiallyLoadedOnShipment.Items.AddNew().HVI_JS_LoadedOnShipment = otherShipment.PK;

			var docDataObjectProvider = new HVLVConsignmentDocDataObjectProvider(shipment);
			var consignmentsWithItemLoadedOnShipment = docDataObjectProvider.GetHVLVConsignmentsWithItemLoadedOnShipment();
			AssertContainsExactElementsInAnyOrder("Expected only get consignments with item loaded on shipment", new[] { consignment.PK, consignmentPartiallyLoadedOnShipment.PK }, consignmentsWithItemLoadedOnShipment.Select(x => x.PK));
		}
	}
}
