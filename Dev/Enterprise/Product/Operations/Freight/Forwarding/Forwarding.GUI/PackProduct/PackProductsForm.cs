using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class PackProductsForm : ZChildForm
	{
		public PackProductsForm(ForwardingPackLine packLine)
			: base(packLine)
		{
			InitializeComponent();
		}

		void CloseButton_Click(object sender, System.EventArgs e)
		{
			bool errors = false;

			foreach (BusinessObject bizo in this.ProductsGrid.List)
			{
				bizo.RunPreSaveValidation();
				if (bizo.HasErrors())
				{
					errors = true;
				}
			}

			if (errors)
			{
				ShowErrorsDialog();
			}
			else
			{
				Close();
			}
		}

		public override string FormVerb
		{
			get { return ""; }
		}
	}
}
