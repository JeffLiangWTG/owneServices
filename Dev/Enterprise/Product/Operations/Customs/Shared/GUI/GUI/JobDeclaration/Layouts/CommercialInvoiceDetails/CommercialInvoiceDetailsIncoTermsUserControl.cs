using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.Freight.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public partial class CommercialInvoiceDetailsIncoTermsUserControl : ZUserControl, IExtendedControl, IResourceStringBindingMember
	{
		public CommercialInvoiceDetailsIncoTermsUserControl()
		{
			InitializeComponent();
			extensions = new DefaultControlExtensionCollection(this);
		}

		protected void IncoTermExplainButton_Click(object sender, System.EventArgs e)
		{
			ZFormModaliser.Show(new IncoTermDescriptionForm(IncoTermBoundDropEdit.Text), ParentForm as ZForm);
		}

		#region IExtendedControl Members

		Control IExtendedControl.Host => this;

		IControlExtensionCollection IExtendedControl.Extensions => extensions;

		#endregion

		#region IResourceStringBindingMember Members

		string IResourceStringBindingMember.ResourceStringBindingMember => nameof(BaseJobComInvoiceHeader.JZ_IncoTerm);

		#endregion

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				extensions.Dispose();
			}

			base.Dispose(disposing);
		}

		readonly DefaultControlExtensionCollection extensions;
	}
}
