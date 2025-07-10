namespace Enterprise.Rating.GUI
{
	public partial class MinimumControl
	{
		private ZArchitecture.ZCalcEdit PriceCalcEdit;
		private ZArchitecture.GUI.ZRadioButton zJobRadioButton;
		private ZArchitecture.GUI.ZRadioButton zChargeCodeRadioButton;
		private ZArchitecture.ZLabel zLabel2;
		private System.ComponentModel.Container components = null;

		private void InitializeComponent()
		{
			this.PriceCalcEdit = new ZArchitecture.ZCalcEdit();
			this.zJobRadioButton = new ZArchitecture.GUI.ZRadioButton();
			this.zChargeCodeRadioButton = new ZArchitecture.GUI.ZRadioButton();
			this.zLabel2 = new ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// PriceCalcEdit
			// 
			this.PriceCalcEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("MinimumControl|13a9da3a-3431-4778-b57e-053078e7e9cf", "Value");
			this.PriceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(62, 49, true);
			this.PriceCalcEdit.Name = "PriceCalcEdit";
			this.PriceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.PriceCalcEdit.TabIndex = 3;
			this.PriceCalcEdit.Text = "0.000";
			this.PriceCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zJobRadioButton
			// 
			this.zJobRadioButton.AutoCheck = false;
			this.zJobRadioButton.AutoSize = true;
			this.zJobRadioButton.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("MinimumControl|62b478b1-6866-4107-879c-adeae056b550", "per Job");
			this.zJobRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.zJobRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(62, 3, true);
			this.zJobRadioButton.Name = "zJobRadioButton";
			this.zJobRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 13, true);
			this.zJobRadioButton.TabIndex = 1;
			this.zJobRadioButton.TabStop = true;
			this.zJobRadioButton.UseVisualStyleBackColor = true;
			// 
			// zChargeCodeRadioButton
			// 
			this.zChargeCodeRadioButton.AutoCheck = false;
			this.zChargeCodeRadioButton.AutoSize = true;
			this.zChargeCodeRadioButton.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("MinimumControl|a76d0c20-1031-4359-9313-abfb338ba852", "per Charge Code");
			this.zChargeCodeRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.zChargeCodeRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(62, 26, true);
			this.zChargeCodeRadioButton.Name = "zChargeCodeRadioButton";
			this.zChargeCodeRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 13, true);
			this.zChargeCodeRadioButton.TabIndex = 2;
			this.zChargeCodeRadioButton.TabStop = true;
			this.zChargeCodeRadioButton.UseVisualStyleBackColor = true;
			// 
			// zLabel2
			// 
			this.zLabel2.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("MinimumControl|107cf087-0c2e-4a5c-9a4e-5e3406ed730d", "Minimum");
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 19, true);
			this.zLabel2.TabIndex = 4;
			// 
			// MinimumControl
			// 
			this.Controls.Add(this.zLabel2);
			this.Controls.Add(this.zChargeCodeRadioButton);
			this.Controls.Add(this.zJobRadioButton);
			this.Controls.Add(this.PriceCalcEdit);
			this.Name = "MinimumControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 82, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
