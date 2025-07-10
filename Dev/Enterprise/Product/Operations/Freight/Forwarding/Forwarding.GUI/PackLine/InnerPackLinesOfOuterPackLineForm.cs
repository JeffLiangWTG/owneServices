using System;
using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	partial class InnerPackLinesOfOuterPackLineForm : ZChildForm
	{
		public InnerPackLinesOfOuterPackLineForm(ForwardingPackLine parentOuterPackLine)
			: base(parentOuterPackLine)
		{
			InitializeComponent();
			this.parentOuterPackLine = parentOuterPackLine;
			InnerPackLinesGrid.SetOuterPackLinesColumnReadOnly();
			parentOuterPackLine.Shipment.UpdatingShipmentVolumeFromPacks += ParentShipment_OnTotalsChange;
		}

		readonly ForwardingPackLine parentOuterPackLine;

		#region Overrides

		public override string FormCaption
		{
			get { return Res.GetString("InnerPackLinesOfOuterPackLineForm|FormCaption", "Inner Package Details for ") + parentOuterPackLine.JL_PackLineId; }
		}

		#endregion

		void OKButton_Click(object sender, EventArgs e)
		{
			parentOuterPackLine.Shipment?.SyncMeasuresWithInnerPackLines();
		}

		void ParentShipment_OnTotalsChange(object sender, CancelEventArgs e)
		{
			var caption = Res.GetString("97585664-5cdc-739e-49e5-2f42e022c32b", "Totals do not match");
			var message = Res.GetString("4826a2d7-c59c-fbb1-4562-8f441f5b5040", "Total Inner packs do not match the shipment total. Would you like to update the shipment to match the Inner packline totals?");
			e.Cancel = Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (parentOuterPackLine.Shipment != null)
				{
					parentOuterPackLine.Shipment.UpdatingShipmentVolumeFromPacks -= ParentShipment_OnTotalsChange;
				}
			}
			base.Dispose(disposing);
		}
	}
}
