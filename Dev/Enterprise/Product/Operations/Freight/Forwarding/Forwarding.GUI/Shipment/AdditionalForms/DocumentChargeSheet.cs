using System.Windows.Forms;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class DocumentChargeSheet : ZChildForm
	{
		public DocumentChargeSheet(DocumentShipment businessObject)
			: base(businessObject)
		{
			InitializeComponent();

			PrintButton.Click += (s, e) => DialogResult = DialogResult.Yes;
			CancelPrintButton.Click += (s, e) => DialogResult = DialogResult.No;
		}

		protected DocumentShipment DocumentShipment
		{
			get { return (DocumentShipment)BusinessEntity; }
		}
	}
}
