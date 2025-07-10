using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Confirmations.GUI.Testing
{
	[TestedType(typeof(DocumentPickupDeliveryConfirmForm))]
	sealed class DocumentPickupDeliveryConfirmFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.OuterPackLines.AddNew();
			shipment.DeliveryConfirms.AddNew();
			CommonPickupDeliveryConfirmCollection confirms = new CommonPickupDeliveryConfirmCollection(Factory, shipment, Constants.PickupDeliveryConfirmTypes.DestinationDelivery);
			DocumentPickupDeliveryConfirmCollection documentConfirms = new DocumentPickupDeliveryConfirmCollection(confirms);
			return new DocumentPickupDeliveryConfirmForm(documentConfirms);
		}

		public void TestTransportCompanyValidation()
		{
			CommonShipment shipment = Factory.New<CommonShipment>();

			PackLine line = shipment.OuterPackLines.AddNew();

			CommonPickupDeliveryConfirm confirmWithNoTransport = shipment.PickupConfirms.AddNew();
			CommonPickupDeliveryConfirm confirmWithSameTransport = shipment.PickupConfirms.AddNew();
			CommonPickupDeliveryConfirm confirmWithDiffTransport = shipment.PickupConfirms.AddNew();

			OrgHeader shipmentTransportCo = Factory.New<OrgHeader>();
			OrgHeader confirmTransportCo = Factory.New<OrgHeader>();

			shipment.DocsAndCartage.PickupCartageCoPK = shipmentTransportCo.PK;

			confirmWithSameTransport.TransportCoPK = shipmentTransportCo.PK;
			confirmWithDiffTransport.TransportCoPK = confirmTransportCo.PK;
			confirmWithDiffTransport.EU_OA_TransportProvider = confirmTransportCo.MainAddress.PK;

			DocumentPickupDeliveryConfirmCollection pickupDeliveryConfirms = new DocumentPickupDeliveryConfirmCollection(shipment.PickupConfirms);

			using (DocumentPickupDeliveryConfirmForm form = new DocumentPickupDeliveryConfirmForm(pickupDeliveryConfirms, shipment))
			{
				AssertEquals(false, pickupDeliveryConfirms[0].PrintConfirmInfo.HasWarnings());
				AssertEquals(false, pickupDeliveryConfirms[1].PrintConfirmInfo.HasWarnings());
				AssertEquals(true, pickupDeliveryConfirms[2].PrintConfirmInfo.HasWarnings());
			}
		}

		public void TestDialogResult()
		{
			using (DocumentPickupDeliveryConfirmForm form = (DocumentPickupDeliveryConfirmForm)GetFormToBash())
			{
				form.Show();

				ZButton printButton = form.Controls.Find("PrintButton", true)[0] as ZButton;
				printButton.PerformClick();

				AssertEquals(form.DialogResult, DialogResult.Yes);
			}

			using (DocumentPickupDeliveryConfirmForm form = (DocumentPickupDeliveryConfirmForm)GetFormToBash())
			{
				form.Show();

				ZButton cancelPrintButton = form.Controls.Find("CancelPrintButton", true)[0] as ZButton;
				cancelPrintButton.PerformClick();

				AssertEquals(form.DialogResult, DialogResult.No);
			}
		}
	}
}
