using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class ComInvoiceReconciliationForm : ZChildForm
	{
		public ComInvoiceReconciliationForm(ComInvoiceReconciliator invoiceReconciliator) : base(invoiceReconciliator)
		{
			Reconciliator = invoiceReconciliator;
		}

		public readonly ComInvoiceReconciliator Reconciliator;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		protected void ImportInvoicesButton_Click(object sender, System.EventArgs e)
		{
			string message = Res.GetString("548f0999-e01c-4ef8-a004-3e2b577471ff", "Not all invoices are correctly balanced.\r\nInvoice amounts on those invoices will be set to the sum of lines.\r\nDo you still want to continue?");
			if (!Reconciliator.ComInvHeaders.HasWarnings() || Globals.Message.Show(message, Res.GetString("38ef3229-5728-44a4-a3ef-627313f26c55", "Unbalanced Invoice"), MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.No) == DialogResult.Yes)
			{
				Reconciliator.ImportInvoices();
				Close();
			}
		}

		void CancelImportButton_Click(object sender, System.EventArgs e)
		{
			Close();
		}
	}
}
