using Enterprise.Customs.Business;
using Enterprise.Freight.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class ShipmentDetailsIncoTermsUserControl : ZUserControl
	{
		public ShipmentDetailsIncoTermsUserControl()
		{
			InitializeComponent();
		}

		void IncoTermExplainButton_Click(object sender, System.EventArgs e)
		{
			ZFormModaliser.Show(new IncoTermDescriptionForm(JobDeclaration.JE_ShipmentIncoTerm), ParentForm as ZForm);
		}

		BaseJobDeclaration JobDeclaration => (BaseJobDeclaration)BindingSource.DataSource;
	}
}
