using Enterprise.Freight.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class NonLayoutCustomsSupplierHeaderUserControl : BaseCustomsSupplierHeaderUserControl
	{
		public NonLayoutCustomsSupplierHeaderUserControl()
		{
			InitializeComponent();
		}

		public ZTextBox InvoiceNumberBoundTextBox
		{
			get { return JZ_InvoiceNumberBoundTextBox; }
		}

		public ZDropEdit IncoTermBoundDropDownEdit
		{
			get { return JZ_IncoTermBoundDropDownEdit; }
		}

		protected void IncoTermExplainButton_Click(object sender, System.EventArgs e)
		{
			IncoTermDescriptionForm form = new IncoTermDescriptionForm(JZ_IncoTermBoundDropDownEdit.Text);
			ZFormModaliser.Show(form, ParentForm as ZForm);
		}
	}
}
