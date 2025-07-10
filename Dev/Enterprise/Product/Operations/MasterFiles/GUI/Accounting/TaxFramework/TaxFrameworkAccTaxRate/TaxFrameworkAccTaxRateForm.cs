using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class TaxFrameworkAccTaxRateForm : ZForm
	{
		public TaxFrameworkAccTaxRateForm(TaxFrameworkAccTaxRateLoader loader)
			: base(loader)
		{
			InitializeComponent();
			ZFormPostingButtonsStrategy.SetupPosting(this, saveButtonsControl);
		}

		protected override bool AllowNew => false;
	}
}
