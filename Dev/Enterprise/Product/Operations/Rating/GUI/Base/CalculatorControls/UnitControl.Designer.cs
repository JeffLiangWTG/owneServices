namespace Enterprise.Rating.GUI
{
	public partial class UnitControl
	{
		private ZArchitecture.ZCalcEdit PriceCalcEdit;
		private System.ComponentModel.Container components = null;

		private void InitializeComponent()
		{
			this.PriceCalcEdit = new ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// PriceCalcEdit
			// 
			this.PriceCalcEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("UnitControl|9031c49d-b451-471b-b146-c487d38c9d1d", "Per Unit Price");
			this.PriceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 8, true);
			this.PriceCalcEdit.Name = "PriceCalcEdit";
			this.PriceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.PriceCalcEdit.TabIndex = 1;
			this.PriceCalcEdit.Text = "0.0000";
			this.PriceCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// UnitControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PriceCalcEdit);
			this.Name = "UnitControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 40, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
