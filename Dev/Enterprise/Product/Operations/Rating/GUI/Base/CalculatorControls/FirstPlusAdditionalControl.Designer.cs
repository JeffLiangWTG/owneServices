namespace Enterprise.Rating.GUI
{
	public partial class FirstPlusAdditionalControl
	{
		private ZArchitecture.ZCalcEdit AddlItemPriceCalcEdit;
		private ZArchitecture.ZCalcEdit FirstItemPriceCalcEdit;
		private System.ComponentModel.Container components = null;

		private void InitializeComponent()
		{
			this.AddlItemPriceCalcEdit = new ZArchitecture.ZCalcEdit();
			this.FirstItemPriceCalcEdit = new ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// AddlItemPriceCalcEdit
			// 
			this.AddlItemPriceCalcEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("FirstPlusAdditionalControl|b5bce4d5-3787-4877-ae1f-b1ccd521f8ee", "Addl. Item Price");
			this.AddlItemPriceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 32, true);
			this.AddlItemPriceCalcEdit.Name = "AddlItemPriceCalcEdit";
			this.AddlItemPriceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.AddlItemPriceCalcEdit.TabIndex = 3;
			this.AddlItemPriceCalcEdit.Text = "0.000";
			this.AddlItemPriceCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// FirstItemPriceCalcEdit
			// 
			this.FirstItemPriceCalcEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("FirstPlusAdditionalControl|db75dcf9-ab69-4d9f-ac0e-21364e9e499c", "First Item Price");
			this.FirstItemPriceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 8, true);
			this.FirstItemPriceCalcEdit.Name = "FirstItemPriceCalcEdit";
			this.FirstItemPriceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.FirstItemPriceCalcEdit.TabIndex = 1;
			this.FirstItemPriceCalcEdit.Text = "0.000";
			this.FirstItemPriceCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// FirstPlusAdditionalControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AddlItemPriceCalcEdit);
			this.Controls.Add(this.FirstItemPriceCalcEdit);
			this.Name = "FirstPlusAdditionalControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(216, 64, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
