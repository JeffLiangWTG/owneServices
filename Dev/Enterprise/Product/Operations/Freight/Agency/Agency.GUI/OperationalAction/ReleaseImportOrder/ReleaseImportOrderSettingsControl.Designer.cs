namespace Enterprise.Freight.Agency.GUI
{
	partial class ReleaseImportOrderSettingsControl
	{
		private void InitializeComponent()
		{
			abortOnValidationCheckBox = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Agency.Business.ReleaseImportOrderSettings);
			// 
			// abortOnValidationCheckBox
			// 
			abortOnValidationCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(abortOnValidationCheckBox, "ErrorBehaviour");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.ReleaseImportOrderSettings)(null)).ErrorBehaviour)));
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(abortOnValidationCheckBox, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			abortOnValidationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 24, true);
			abortOnValidationCheckBox.Name = "abortOnValidationCheckBox";
			abortOnValidationCheckBox.PreBoundMaxLength = 3;
			abortOnValidationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 20, true);
			abortOnValidationCheckBox.TabIndex = 0;
			// 
			// EIDOSettingsControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(abortOnValidationCheckBox);
			this.Name = "ReleaseImportOrderSettingsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(153, 150, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		Enterprise.ZArchitecture.GUI.ZDropEdit abortOnValidationCheckBox;
	}
}
