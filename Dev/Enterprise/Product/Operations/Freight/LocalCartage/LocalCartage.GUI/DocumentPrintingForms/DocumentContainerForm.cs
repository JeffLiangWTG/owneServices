using System.Windows.Forms;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.LocalCartage.GUI
{
	public partial class DocumentContainerForm : ZChildForm
	{
		public DocumentContainerForm(DocumentContainerOptions options)
			: base(options)
		{
		}

		void PrintButton_Click(object sender, System.EventArgs e)
		{
			DialogResult = DialogResult.Yes;
		}

		void CancelPrintButton_Click(object sender, System.EventArgs e)
		{
			DialogResult = DialogResult.No;
		}
	}
}
