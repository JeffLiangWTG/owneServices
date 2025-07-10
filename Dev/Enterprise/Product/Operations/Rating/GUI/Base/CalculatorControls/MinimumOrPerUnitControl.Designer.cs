namespace Enterprise.Rating.GUI
{
	public partial class MinimumOrPerUnitControl
	{
		private ZArchitecture.ZCalcEdit PerUnitPriceCalcEdit;
		private ZArchitecture.ZCalcEdit MinPriceCalcEdit;
		private System.ComponentModel.Container components = null;

		private void InitializeComponent()
		{
			this.PerUnitPriceCalcEdit = new ZArchitecture.ZCalcEdit();
			this.MinPriceCalcEdit = new ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// PerUnitPriceCalcEdit
			// 
			this.PerUnitPriceCalcEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("MinimumOrPerUnitControl|edb2f302-e9e0-457c-bcd8-892bdd72f6a9", "Per Unit Price");
			this.PerUnitPriceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 32, true);
			this.PerUnitPriceCalcEdit.Name = "PerUnitPriceCalcEdit";
			this.PerUnitPriceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.PerUnitPriceCalcEdit.TabIndex = 3;
			this.PerUnitPriceCalcEdit.Text = "0.000";
			this.PerUnitPriceCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// MinPriceCalcEdit
			// 
			this.MinPriceCalcEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("MinimumOrPerUnitControl|3872eb90-583b-4043-bfbc-4da0cda69fc3", "Minimum");
			this.MinPriceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 8, true);
			this.MinPriceCalcEdit.Name = "MinPriceCalcEdit";
			this.MinPriceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.MinPriceCalcEdit.TabIndex = 1;
			this.MinPriceCalcEdit.Text = "0.000";
			this.MinPriceCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// MinimumOrPerUnitControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PerUnitPriceCalcEdit);
			this.Controls.Add(this.MinPriceCalcEdit);
			this.Name = "MinimumOrPerUnitControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 64, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
