using System.Windows.Forms;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Confirmations.GUI
{
	public partial class DocumentPickupDeliveryConfirmForm : ZChildForm
	{
		public DocumentPickupDeliveryConfirmForm(DocumentPickupDeliveryConfirmCollection pickupDeliveryConfirms)
			: base(pickupDeliveryConfirms)
		{
			this.PickupDeliveryConfirms = pickupDeliveryConfirms;

			InitializeComponent();

			PrintButton.AllowOverlap(MainStatusBar);
			CancelPrintButton.AllowOverlap(MainStatusBar);
		}

		public DocumentPickupDeliveryConfirmForm(DocumentPickupDeliveryConfirmCollection pickupDeliveryConfirms, CommonShipment parentShipment)
			: this(pickupDeliveryConfirms)
		{
			ShipmentTransportCoLabel.Visible = true;

			if (pickupDeliveryConfirms != null && pickupDeliveryConfirms.Count > 0 && pickupDeliveryConfirms[0].Confirm != null)
			{
				bool pickup = pickupDeliveryConfirms[0].Confirm.IsOrigin;
				OrgHeader shipmentTransportCo = pickup ? parentShipment.DocsAndCartage.PickupCartageCo : parentShipment.DocsAndCartage.DeliveryCartageCo;
				ShipmentTransportCoLabel.Text = Res.GetString("0cb656c8-ccc3-41d4-aac9-3e9b9b8bd127", "Document Recipient (Shipment Local Transport Provider) : {0}", shipmentTransportCo == null ? (NoResString)"***None Selected***" : shipmentTransportCo.OH_FullName.ToString());

				bool shipmentHasTransportCo = shipmentTransportCo != null;

				foreach (DocumentPickupDeliveryConfirm docConfirm in pickupDeliveryConfirms)
				{
					bool confirmtHasTransportCo = docConfirm.Confirm.TransportCo != null;

					if ((!shipmentHasTransportCo && confirmtHasTransportCo)
						||
						(shipmentHasTransportCo && confirmtHasTransportCo && docConfirm.Confirm.TransportCo.PK != shipmentTransportCo.PK))
					{
						docConfirm.PrintConfirmInfo.AddWarning(Res.GetString("e106333a-b258-41fd-b5db-0bf0ff74f741", @"Local Transport Provider selected on the Confirmation is different to Local Transport Provider selected on the Shipment. The recipient for these documents will be the Local Transport Provider on the Shipment. Use the Consolidated Transport Booking Module if using multiple Local Transport Providers for the one Shipment."));
					}
				}
			}
		}

		readonly DocumentPickupDeliveryConfirmCollection PickupDeliveryConfirms;

		#region Events

		void PrintButton_Click(object sender, System.EventArgs e)
		{
			int included = 0;
			foreach (DocumentPickupDeliveryConfirm confirm in PickupDeliveryConfirms)
			{
				if (confirm.PrintConfirm)
				{
					included++;
				}
			}

			if (included > 0)
			{
				DialogResult = DialogResult.Yes;
			}
			else
			{
				Globals.Message.ShowInformation(Res.GetString("a6cc12b3-6b9c-4785-b5b9-c29f75bf534d", "Please include one or more to print."), Res.GetString("02aec0df-f345-4c4c-9efc-d4ea6d17373b", "None selected"));
			}
		}

		void CancelPrintButton_Click(object sender, System.EventArgs e)
		{
			DialogResult = DialogResult.No;
		}

		#endregion
	}
}
