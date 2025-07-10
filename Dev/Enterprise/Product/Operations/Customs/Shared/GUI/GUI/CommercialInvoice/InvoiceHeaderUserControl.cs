using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class InvoiceHeaderUserControl : CommonInvoiceHeaderUserControl
	{
		public InvoiceHeaderUserControl()
		{
			InitializeComponent();
		}

		protected void ShowIncoTermDescriptionForm()
		{
			if (!Invoice.JZ_IncoTermInfo.HasErrors() && !Invoice.JZ_IncoTermInfo.HasMessageErrors())
			{
				Freight.GUI.IncoTermDescriptionForm form = new Freight.GUI.IncoTermDescriptionForm(Invoice.JZ_IncoTerm);
				ZFormModaliser.Show(form, ParentForm as ZForm);
			}
		}

		void IncoTermExplainButton_Click(object sender, System.EventArgs e)
		{
			ShowIncoTermDescriptionForm();
		}
	}
}
