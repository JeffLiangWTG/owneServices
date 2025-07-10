using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;

namespace Enterprise.Freight.Business.Testing
{
	sealed class CommonShipmentDocumentSupporterQueryProviderTest : TestCaseWithFactory
	{
		public void TestConfirmBOLPrinting()
		{
			ICommonShipmentDocumentSupporterQueryProvider queryProvider = new CommonShipmentDocumentSupporterQueryProvider();
			CommonShipment shipment = Factory.New<CommonShipment>();

			Env.Security.AllowPrintingOfAWBHBLIfNoExportDeclarationFiled.IsAllowed = false;
			AssertEquals(false, queryProvider.ConfirmBOLPrinting(shipment));

			Env.Security.AllowPrintingOfAWBHBLIfNoExportDeclarationFiled.IsAllowed = true;
			AssertEquals(true, queryProvider.ConfirmBOLPrinting(shipment));
		}

		public void TestGetConfirmsToPrint()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.OuterPackLines.AddNew();

			CommonPickupDeliveryConfirm pickupConfirm1 = shipment.PickupConfirms.AddNew();
			CommonPickupDeliveryConfirm pickupConfirm2 = shipment.PickupConfirms.AddNew();
			CommonPickupDeliveryConfirm pickupConfirm3 = shipment.PickupConfirms.AddNew();

			CommonPickupDeliveryConfirm deliveryConfirm1 = shipment.DeliveryConfirms.AddNew();
			CommonPickupDeliveryConfirm deliveryConfirm2 = shipment.DeliveryConfirms.AddNew();
			CommonPickupDeliveryConfirm deliveryConfirm3 = shipment.DeliveryConfirms.AddNew();

			AssertEquals("Prerequisite", 3, shipment.PickupConfirms.Count);
			AssertEquals("Prerequisite", 3, shipment.DeliveryConfirms.Count);

			ICommonShipmentDocumentSupporterQueryProvider queryProvider = new CommonShipmentDocumentSupporterQueryProvider();

			DocumentPickupDeliveryConfirmOptions options = new DocumentPickupDeliveryConfirmOptions(new DocumentPickupDeliveryConfirmCollection(shipment.PickupConfirms));
			DocumentPickupDeliveryConfirm[] confirmsToPrint = queryProvider.GetConfirmsToPrint(shipment, options);

			AssertContainsExactElementsInAnyOrder(new[] { pickupConfirm1, pickupConfirm2, pickupConfirm3 }, confirmsToPrint.Select((doc) => doc.Confirm));

			options = new DocumentPickupDeliveryConfirmOptions(new DocumentPickupDeliveryConfirmCollection(shipment.DeliveryConfirms));
			confirmsToPrint = queryProvider.GetConfirmsToPrint(shipment, options);

			AssertContainsExactElementsInAnyOrder(new[] { deliveryConfirm1, deliveryConfirm2, deliveryConfirm3 }, confirmsToPrint.Select((doc) => doc.Confirm));
		}

		public void TestGetContainersToPrint()
		{
			CommonConsol consol = Factory.New<CommonConsol>();
			CommonContainer container1 = consol.Containers.AddNew();
			CommonContainer container2 = consol.Containers.AddNew();
			CommonContainer container3 = consol.Containers.AddNew();

			ContainerToSelectFromForPrintingCollection containersToSelectFrom = new ContainerToSelectFromForPrintingCollection(Factory);
			ContainerToSelectFromForPrinting containerToSelect1 = new ContainerToSelectFromForPrinting(container1);
			ContainerToSelectFromForPrinting containerToSelect2 = new ContainerToSelectFromForPrinting(container2);
			ContainerToSelectFromForPrinting containerToSelect3 = new ContainerToSelectFromForPrinting(container3);

			containersToSelectFrom.Add(containerToSelect1);
			containersToSelectFrom.Add(containerToSelect2);
			containersToSelectFrom.Add(containerToSelect3);

			ICommonShipmentDocumentSupporterQueryProvider queryProvider = new CommonShipmentDocumentSupporterQueryProvider();
			ContainersToPrintOptions containersToPrintOptions = queryProvider.GetContainersToPrint(containersToSelectFrom, true);

			AssertNotNull(containersToPrintOptions);
			AssertContainsExactElementsInAnyOrder(new[] { containerToSelect1.Container, containerToSelect2.Container, containerToSelect3.Container }, containersToPrintOptions.ContainersToPrint);
			AssertEquals(true, containersToPrintOptions.IncludeUnContainerised);
		}

		public void TestGetDocumentOptions()
		{
			ICommonShipmentDocumentSupporterQueryProvider queryProvider = new CommonShipmentDocumentSupporterQueryProvider();

			AssertEquals(null, queryProvider.GetDocumentOptions(null));

			DocumentShipment documentShipment = new DocumentShipment(Factory.New<CommonShipment>(), Core.Constants.DataContext.Shipment);

			AssertEquals(documentShipment, queryProvider.GetDocumentOptions(documentShipment));
		}
	}
}
