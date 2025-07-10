using System.Windows.Forms;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.ZArchitecture.GUI;
namespace Enterprise.Freight.Forwarding.GUI.AWB
{
	public partial class PrintMAWBBarcodeLabelsActionMethodForm : ZChildForm
	{
		public PrintMAWBBarcodeLabelsActionMethodForm(BulkMAWBLabelActions businessEntity)
			: base(businessEntity)
		{
			InitializeComponent();
		}

		BulkMAWBLabelActions MAWBLabelActions
		{
			get { return (BulkMAWBLabelActions)BusinessEntity; }
		}

		void OKButton_Click(object sender, System.EventArgs e)
		{
			MAWBLabelActions.ValidateAll();
			if (MAWBLabelActions.HasErrors)
			{
				ShowErrorsDialog();
			}
			else
			{
				DialogResult = DialogResult.OK;
			}
		}
	}
}
