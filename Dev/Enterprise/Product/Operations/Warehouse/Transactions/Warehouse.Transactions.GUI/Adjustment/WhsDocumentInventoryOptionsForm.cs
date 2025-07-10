using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public partial class WhsDocumentInventoryOptionsForm : ZChildForm
	{
		public WhsDocumentInventoryOptionsForm(WhsDocumentInventoryOptions options)
			: base(options)
		{
			InitializeComponent();
		}

		public WhsDocumentInventoryOptionsForm()
			: base()
		{
			InitializeComponent();
		}

		#region Button Clicks

		void PrintButton_Click(object sender, System.EventArgs e)
		{
			if (Options.HasErrors())
			{
				ShowErrorsDialog();
			}
			else
			{
				DialogResult = DialogResult.Yes;
			}
		}

		void CancelPrintButton_Click(object sender, System.EventArgs e)
		{
			DialogResult = DialogResult.No;
		}

		#endregion

		#region Overrides

		public override string FormHeading => Res.GetString("WhsDocumentInventoryOptionsForm|FormHeading", "Labels Printing");

		#endregion

		#region Options

		WhsDocumentInventoryOptions Options => (WhsDocumentInventoryOptions)BusinessEntity;

		#endregion
	}
}
