using System.Windows.Forms;

using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.GUI
{
	public partial class DocumentServicesForm : ZChildForm
	{
		public DocumentServicesForm(DocumentServices businessObject)
			: base(businessObject)
		{
			InitializeComponent();

			PrintButton.Click += (s, e) => DialogResult = DialogResult.Yes;
			CancelPrintButton.Click += (s, e) => DialogResult = DialogResult.No;
		}
	}
}
