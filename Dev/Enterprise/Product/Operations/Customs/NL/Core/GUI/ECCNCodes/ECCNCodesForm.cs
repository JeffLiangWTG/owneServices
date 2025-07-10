using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.Customs.NL.Business;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.GUI
{
	public partial class ECCNCodesForm : ZChildForm
	{
		public ECCNCodesForm(JobComInvoiceLine invoiceLine) : base(invoiceLine)
		{
			this.invoiceLine = invoiceLine;
		}

		public override string FormVerb => "";

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			oldItems = invoiceLine.ECCNCodes.AsString;
		}

		string oldItems;

		protected override void OnClosing(CancelEventArgs e)
		{
			zButtonClose.Focus();

			if (DialogResult == DialogResult.Cancel)
			{
				invoiceLine.ECCNCodes.AsString = oldItems;
				invoiceLine.ECCNCodesAsStringInfo.RefreshBinding();
			}
			else
			{
				invoiceLine.ECCNCodes.RunPreSaveValidation();
				if (invoiceLine.ECCNCodes.Cast<ECCNCode>().Any(code => code.NotificationsIncludingChildren.GetErrors().Any()))
				{
					Globals.Message.ShowError(Res.GetString("215EC597-D91E-41A1-8EFC-AC260D3D92B7", "The form has errors. Please fix them before continuing."));
					e.Cancel = true;
				}
			}

			base.OnClosing(e);
		}

		void ZButtonOK_Click(object sender, EventArgs e)
		{
			Close();
		}

		void ZButtonClose_Click(object sender, EventArgs e)
		{
			Close();
		}

		readonly JobComInvoiceLine invoiceLine;
	}
}
