using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.GUI.Testing
{
	sealed class ShipmentDocumentSupporterGuiQueryProviderTest : SharedGuiQueryProviderTest
	{
		public void TestRegister()
		{
			ICommonShipmentDocumentSupporterQueryProvider provider = Factory.GetValue<ICommonShipmentDocumentSupporterQueryProvider>();
			AssertNull("prerequisite", provider);

			ShipmentDocumentSupporterGuiQueryProvider.Register(Factory);

			provider = Factory.GetValue<ICommonShipmentDocumentSupporterQueryProvider>();

			AssertNotNull(provider);
			Assert(provider is ShipmentDocumentSupporterGuiQueryProvider);
		}

		public void TestGetConfirmsToPrint()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.OuterPackLines.AddNew();

			ICommonShipmentDocumentSupporterQueryProvider queryProvider = new ShipmentDocumentSupporterGuiQueryProvider();

			DocumentPickupDeliveryConfirmOptions options = new DocumentPickupDeliveryConfirmOptions(new DocumentPickupDeliveryConfirmCollection(shipment.PickupConfirms));
			DocumentPickupDeliveryConfirm[] confirmsToPrint = queryProvider.GetConfirmsToPrint(shipment, options);

			AssertNull(confirmsToPrint);
			AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasInformation);
			AssertContains("There are no deliveries to print", UnitTestUserNotification.Instance.LastMessage.Text);

			CommonPickupDeliveryConfirm pickupConfirm1 = shipment.PickupConfirms.AddNew();
			options = new DocumentPickupDeliveryConfirmOptions(new DocumentPickupDeliveryConfirmCollection(shipment.PickupConfirms));
			confirmsToPrint = queryProvider.GetConfirmsToPrint(shipment, options);

			AssertNull("no dialog shown for one confirm", ZFormModaliser.LastFormShownDialogForTest);
			AssertContainsExactElementsInAnyOrder(new[] { pickupConfirm1 }, confirmsToPrint.Select((doc) => doc.Confirm));

			CommonPickupDeliveryConfirm pickupConfirm2 = shipment.PickupConfirms.AddNew();
			CommonPickupDeliveryConfirm pickupConfirm3 = shipment.PickupConfirms.AddNew();

			AssertEquals("Prerequisite", 3, shipment.PickupConfirms.Count);

			options = new DocumentPickupDeliveryConfirmOptions(new DocumentPickupDeliveryConfirmCollection(shipment.PickupConfirms));
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
			confirmsToPrint = queryProvider.GetConfirmsToPrint(shipment, options);

			AssertContainsExactElementsInAnyOrder(new[] { pickupConfirm1, pickupConfirm2, pickupConfirm3 }, confirmsToPrint.Select((doc) => doc.Confirm));
			AssertNotNull("dialog shown", ZFormModaliser.LastFormShownDialogForTest);

			CommonPickupDeliveryConfirm deliveryConfirm1 = shipment.DeliveryConfirms.AddNew();
			CommonPickupDeliveryConfirm deliveryConfirm2 = shipment.DeliveryConfirms.AddNew();
			CommonPickupDeliveryConfirm deliveryConfirm3 = shipment.DeliveryConfirms.AddNew();

			AssertEquals("Prerequisite", 3, shipment.DeliveryConfirms.Count);

			options = new DocumentPickupDeliveryConfirmOptions(new DocumentPickupDeliveryConfirmCollection(shipment.DeliveryConfirms));
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
			confirmsToPrint = queryProvider.GetConfirmsToPrint(shipment, options);

			AssertNull(confirmsToPrint);
		}

		public void TestDocumentOptions()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			DocumentShipment documentShipment = new DocumentShipment(shipment, Core.Constants.DataContext.FreightLabels);

			ICommonShipmentDocumentSupporterQueryProvider queryProvider = new ShipmentDocumentSupporterGuiQueryProvider();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
			AssertEquals(null, queryProvider.GetDocumentOptions(documentShipment));
			AssertEquals(typeof(DocumentShipmentForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
			AssertEquals(documentShipment, queryProvider.GetDocumentOptions(documentShipment));
			AssertEquals(typeof(DocumentShipmentForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
		}

		public void TestDocumentOptionsForGenericFreightJobBySelectedPackagesVariations()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			DocumentShipment documentShipment = new DocumentShipment(shipment, Core.Constants.DataContext.GenericFreightJobBySelectedPackages);

			ICommonShipmentDocumentSupporterQueryProvider queryProvider = new ShipmentDocumentSupporterGuiQueryProvider();

			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Yes;
			AssertEquals(documentShipment, queryProvider.GetDocumentOptions(documentShipment));
			AssertEquals(typeof(DocumentShipmentWithUniqueIDForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

			documentShipment = new DocumentShipment(shipment, Core.Constants.DataContext.GenericFreightJobBySelectedPkgs1Doc);
			AssertEquals(documentShipment, queryProvider.GetDocumentOptions(documentShipment));
			AssertEquals(typeof(DocumentShipmentWithUniqueIDForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
		}
	}
}
