using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture;

namespace Enterprise.Rating.GUI
{
	public partial class ExcludeCompanyTariffsCalculatorUserControl
	{
		System.ComponentModel.Container components = null;

		void InitializeComponent()
		{
			this.descriptionLabel = new ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.RateLineItemsView);
			// 
			// descriptionLabel
			//
			this.descriptionLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.descriptionLabel.FontType = Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif;
			this.descriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 0, true);
			this.descriptionLabel.Name = "descriptionLabel";
			this.descriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(492, 69, true);
			this.descriptionLabel.TabIndex = 0;
			this.descriptionLabel.Text = Res.GetString("3e8975e9-6c9c-4e85-921c-431059c23a24", "A separate Job Charge will not be created when using the Exclude from Company Tariffs Calculator.  Instead the same Charge Code in Company Tariffs will be excluded from Autorating and will not come through to the Job.");
			// 
			// ExcludeCompanyTariffsCalculatorUserControl
			// 
			this.Controls.Add(this.descriptionLabel);
			this.Name = "ExcludeCompanyTariffsCalculatorUserControl";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
