using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class ShipmentPiecesDetailForm : ZChildForm
	{
		public ShipmentPiecesDetailForm(CommonShipment shipment)
			: base(shipment)
		{
			InitializeComponent();
			this.Shipment = shipment;

#if DEBUG
			TypeDescriptor.AddAttributes(ShipmentWeightUnitLabel, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(TotalWeightUnitLabel, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(ShipmentVolumeUnitLabel, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(TotalVolumeUnitLabel, new SuppressFormsLocalizedTestAttribute());
#endif
			shipment.UpdatingShipmentVolumeFromPacks += new CancelEventHandler(Shipment_UpdatingShipmentVolumeFromPacks);
		}

		#region Form Setup

		public override string FormCaption
		{
			get { return Res.GetString("Forwarding|ShipmentPiecesDetailForm|FormCaptionPrefix", "Inner Package Details for Shipment ") + Shipment.JS_UniqueConsignRef; }
		}

		#endregion

		#region Saving

		void SaveButton_Click(object sender, System.EventArgs e)
		{
			Shipment.SyncMeasuresWithInnerPackLines();
		}

		void Shipment_UpdatingShipmentVolumeFromPacks(object sender, CancelEventArgs e)
		{
			var caption = Res.GetString("567a05fe-e3eb-4d84-ac82-57116b19c040", "Totals do not match");
			var message = Res.GetString("3d8dd885-5dd9-468a-915f-298cf42f550b", "Total Inner packs do not match the shipment total. Would you like to update the shipment to match the Inner packline totals?");
			e.Cancel = Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No;
		}

		#endregion

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Shipment != null)
				{
					Shipment.UpdatingShipmentVolumeFromPacks -= new CancelEventHandler(Shipment_UpdatingShipmentVolumeFromPacks);
				}
			}
			base.Dispose(disposing);
		}
	}
}
