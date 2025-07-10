using System.Windows.Forms;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class DocumentDeliveryAgentsForm : ZChildForm
	{
		public DocumentDeliveryAgentsForm(DocumentDeliveryAgents businessObject)
			: base(businessObject)
		{
			InitializeComponent();

			PrintButton.Click += (s, e) => DialogResult = DialogResult.Yes;
			CancelPrintButton.Click += (s, e) => DialogResult = DialogResult.No;
		}
	}
}
