namespace Enterprise.Rating.GUI
{
	public partial class ItalianAirportTaxControl
	{
		private ZArchitecture.ZCalcEdit FlatRateCalcEdit;
		private ZArchitecture.ZCalcEdit BasicChargeCalcEdit;
		private ZArchitecture.ZCalcEdit AdditionalPackageCalcEdit;
		private ZArchitecture.ZCalcEdit FirstPackageCalcEdit;
		private System.ComponentModel.Container components = null;

		private void InitializeComponent()
		{
			this.FlatRateCalcEdit = new ZArchitecture.ZCalcEdit();
			this.BasicChargeCalcEdit = new ZArchitecture.ZCalcEdit();
			this.AdditionalPackageCalcEdit = new ZArchitecture.ZCalcEdit();
			this.FirstPackageCalcEdit = new ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// FlatRateCalcEdit
			// 
			this.FlatRateCalcEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("ItalianAirportTaxControl|e0aae0b2-4557-4078-b8bc-3669d48a04f6", "Rate per Kg");
			this.FlatRateCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 32, true);
			this.FlatRateCalcEdit.Name = "FlatRateCalcEdit";
			this.FlatRateCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.FlatRateCalcEdit.TabIndex = 3;
			this.FlatRateCalcEdit.Text = "0.000";
			this.FlatRateCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// BasicChargeCalcEdit
			// 
			this.BasicChargeCalcEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("ItalianAirportTaxControl|6979f3f3-edd6-477d-a905-769fcf33c0ba", "Basic Charge");
			this.BasicChargeCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 8, true);
			this.BasicChargeCalcEdit.Name = "BasicChargeCalcEdit";
			this.BasicChargeCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.BasicChargeCalcEdit.TabIndex = 1;
			this.BasicChargeCalcEdit.Text = "0.000";
			this.BasicChargeCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// AdditionalPackageCalcEdit
			// 
			this.AdditionalPackageCalcEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("ItalianAirportTaxControl|4c9377c4-c201-46fb-b204-aff693cfb509", "Additional Package");
			this.AdditionalPackageCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(304, 32, true);
			this.AdditionalPackageCalcEdit.Name = "AdditionalPackageCalcEdit";
			this.AdditionalPackageCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.AdditionalPackageCalcEdit.TabIndex = 7;
			this.AdditionalPackageCalcEdit.Text = "0.000";
			this.AdditionalPackageCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// FirstPackageCalcEdit
			// 
			this.FirstPackageCalcEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("ItalianAirportTaxControl|5a1eeee5-0452-49be-8b6a-8036175411a9", "First Package");
			this.FirstPackageCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(304, 8, true);
			this.FirstPackageCalcEdit.Name = "FirstPackageCalcEdit";
			this.FirstPackageCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.FirstPackageCalcEdit.TabIndex = 5;
			this.FirstPackageCalcEdit.Text = "0.000";
			this.FirstPackageCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ItalianAirportTaxControl
			// 
			this.Controls.Add(this.AdditionalPackageCalcEdit);
			this.Controls.Add(this.FirstPackageCalcEdit);
			this.Controls.Add(this.FlatRateCalcEdit);
			this.Controls.Add(this.BasicChargeCalcEdit);
			this.Name = "ItalianAirportTaxControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 64, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
