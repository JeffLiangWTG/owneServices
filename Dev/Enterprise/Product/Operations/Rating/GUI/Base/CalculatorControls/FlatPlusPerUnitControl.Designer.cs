namespace Enterprise.Rating.GUI
{
	public partial class FlatPlusPerUnitControl
	{
		private ZArchitecture.ZCalcEdit PerUnitPriceCalcEdit;
		private ZArchitecture.ZCalcEdit FlatPriceCalcEdit;
		private System.ComponentModel.Container components = null;

		private void InitializeComponent()
		{
			this.PerUnitPriceCalcEdit = new ZArchitecture.ZCalcEdit();
			this.FlatPriceCalcEdit = new ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// PerUnitPriceCalcEdit
			// 
			this.PerUnitPriceCalcEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("FlatPlusPerUnitControl|bbb22be4-9a5d-473f-a9f4-d21f9c57f68c", "Per Unit Price");
			this.PerUnitPriceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 32, true);
			this.PerUnitPriceCalcEdit.Name = "PerUnitPriceCalcEdit";
			this.PerUnitPriceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.PerUnitPriceCalcEdit.TabIndex = 3;
			this.PerUnitPriceCalcEdit.Text = "0.000";
			this.PerUnitPriceCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// FlatPriceCalcEdit
			// 
			this.FlatPriceCalcEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("FlatPlusPerUnitControl|5cfa79d8-b7a6-422b-aa3e-7d6b97b4dea2", "Base Price");
			this.FlatPriceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 8, true);
			this.FlatPriceCalcEdit.Name = "FlatPriceCalcEdit";
			this.FlatPriceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.FlatPriceCalcEdit.TabIndex = 1;
			this.FlatPriceCalcEdit.Text = "0.000";
			this.FlatPriceCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// FlatPlusPerUnitControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PerUnitPriceCalcEdit);
			this.Controls.Add(this.FlatPriceCalcEdit);
			this.Name = "FlatPlusPerUnitControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 64, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
