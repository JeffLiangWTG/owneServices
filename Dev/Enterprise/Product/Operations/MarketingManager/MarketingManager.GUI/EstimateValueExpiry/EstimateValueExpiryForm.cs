using CargoWise.ComponentModel;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	public partial class EstimateValueExpiryForm : ZChildForm
	{
		public EstimateValueExpiryForm(EstimateValueExpiryAction expiryAction)
			: base(expiryAction)
		{
			InitializeComponent();
		}

		public override string FormVerb => string.Empty;

		void SetExpiryButton_Click(object sender, System.EventArgs e)
		{
			BusinessEntity.RunPreSaveValidation();
			if (!BusinessEntity.HasErrors())
			{
				((EstimateValueExpiryAction)BusinessEntity).Apply();
				Close();
			}
		}
	}
}
