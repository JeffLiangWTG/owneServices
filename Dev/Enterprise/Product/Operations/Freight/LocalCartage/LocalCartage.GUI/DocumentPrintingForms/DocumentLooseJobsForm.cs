using System.Windows.Forms;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.LocalCartage.GUI
{
	[SuppressBindingMemberBashingTest]
	public partial class DocumentLooseJobsForm : ZChildForm
	{
		public DocumentLooseJobsForm(DocumentCartageLegOptions options)
			: base(options)
		{
		}

		public override string FormVerb
		{
			get { return string.Empty; }
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
