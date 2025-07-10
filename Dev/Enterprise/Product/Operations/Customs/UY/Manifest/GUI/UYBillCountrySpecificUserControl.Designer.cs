namespace Enterprise.Customs.UY.Manifest.GUI
{
	partial class UYBillCountrySpecificUserControl
	{
		private void InitializeComponent()
		{
			this.TransshipmentCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TransshipmentCheckBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.UY.Manifest.Business.AsycudaBill);
			// 
			// TransshipmentCheckBox
			//
			this.BindingSource.SetBindingMember(this.TransshipmentCheckBox, "ABL_Transshipment");
			this.TransshipmentCheckBox.AutoSize = true;
			this.TransshipmentCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Left;
			this.TransshipmentCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.TransshipmentCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(68, 68, true);
			this.TransshipmentCheckBox.Name = "TransshipmentCheckBox";
			this.TransshipmentCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.TransshipmentCheckBox.TabIndex = 8;
			this.TransshipmentCheckBox.UseVisualStyleBackColor = true;
			// 
			// UYBillCountrySpecificUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.TransshipmentCheckBox);
			this.Name = "UYBillCountrySpecificUserControl";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TransshipmentCheckBox.ResumeLayout(true);
			this.TransshipmentCheckBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		internal ZArchitecture.GUI.ZCheckBox TransshipmentCheckBox;
	}
}
