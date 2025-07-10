using Enterprise.MasterFiles.Business.CountryCompliance.CountryComplianceInfoDisplay;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Accounting.CountryCompliance
{
	public partial class CountryComplianceInfoDisplayForm : ZForm
	{
		public CountryComplianceInfoDisplayForm(CountryComplianceInfoDisplay countryComplianceInfoDisplay)
			: base(countryComplianceInfoDisplay)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, null, closeButton);
		}

		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		public override string FormVerb => string.Empty;
	}
}
