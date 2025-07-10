using System.Windows.Forms;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class DocumentSelectTransportForm : ZChildForm
	{
		public DocumentSelectTransportForm(DocumentShipment shipment)
			: base(shipment)
		{
			InitializeComponent();
		}

		protected DocumentShipment Shipment
		{
			get { return (DocumentShipment)BusinessEntity; }
		}

		#region Caption

		public override string FormVerb
		{
			get { return ""; }
		}

		#endregion

		#region Implementation

		void PrintSelectedButton_Click(object sender, System.EventArgs e)
		{
			if (TransportsGrid.SelectedRowCount == 0)
			{
				Globals.Message.Show(Res.GetString("02f8b528-9bd7-4035-b7a8-126a9abfc9d4", "No Transport selected"));
				return;
			}
			else
			{
				Shipment.SelectedTransport = (Transport)TransportsGrid.SelectedElements[0];
			}
			DialogResult = DialogResult.Yes;
			Close();
		}

		#endregion
	}
}
