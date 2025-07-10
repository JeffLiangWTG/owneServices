namespace Enterprise.Rating.GUI
{
	public partial class FlatControl
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
			this.PriceCalcEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("FlatControl|30cb4903-8647-432b-84e4-4465e4caaf53", "Base Price");
			this.PriceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 8, true);
			this.PriceCalcEdit.Name = "PriceCalcEdit";
			this.PriceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.PriceCalcEdit.TabIndex = 1;
			this.PriceCalcEdit.Text = "0.000";
			this.PriceCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// FlatControl
			// 
			this.Controls.Add(this.PriceCalcEdit);
			this.Name = "FlatControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 40, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
