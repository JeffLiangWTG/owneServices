namespace Enterprise.Freight.Agency.GUI
{
	partial class ReleaseImportOrderControl
	{
		void InitializeComponent()
		{
			this.descriptionLabel = new Enterprise.ZArchitecture.ZLabel();
			groupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			allowSendWhilePendingCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			groupBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Agency.Business.EIDOBaseApplicator);
			// 
			// descriptionLabel
			// 
			this.descriptionLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.descriptionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 18, true);
			this.descriptionLabel.Name = "descriptionLabel";
			this.descriptionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(294, 38, true);
			this.descriptionLabel.TabIndex = 0;
			// 
			// groupBox1
			// 
			groupBox1.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("ReleaseImportOrderControl|5c1b2fb2-c751-4893-aefd-c30c0fd4f186", "Messages Pending Response");
			groupBox1.Controls.Add(allowSendWhilePendingCheckBox);
			groupBox1.Controls.Add(this.descriptionLabel);
			groupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			groupBox1.Name = "groupBox1";
			groupBox1.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			groupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(304, 88, true);
			groupBox1.TabIndex = 0;
			groupBox1.TabStop = false;
			// 
			// allowSendWhilePendingCheckBox
			// 
			allowSendWhilePendingCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(allowSendWhilePendingCheckBox, "AllowSendWhileResponsePending");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Agency.Business.ReleaseImportOrderActionMethodApplicator)(null)).AllowSendWhileResponsePending)));
			allowSendWhilePendingCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			allowSendWhilePendingCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(24, 64, true);
			allowSendWhilePendingCheckBox.Name = "allowSendWhilePendingCheckBox";
			allowSendWhilePendingCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			allowSendWhilePendingCheckBox.TabIndex = 1;
			allowSendWhilePendingCheckBox.UseVisualStyleBackColor = true;
			// 
			// EIDORunControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(groupBox1);
			this.Name = "EIDORunControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(322, 105, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			groupBox1.ResumeLayout(false);
			groupBox1.PerformLayout();
			this.ResumeLayout(false);

		}

		Enterprise.ZArchitecture.ZLabel descriptionLabel;
		Enterprise.ZArchitecture.GUI.ZGroupBox groupBox1;
		Enterprise.ZArchitecture.GUI.ZCheckBox allowSendWhilePendingCheckBox;
	}
}
