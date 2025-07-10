namespace Enterprise.MasterFiles.GUI
{
	partial class ZDocAdditionalAddressInfoControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.AdditionalAddressInfoDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AdditionalInfoLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AdditionalAddressInfoDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// AdditionalAddressInfoDropEdit
			// 
			this.AdditionalAddressInfoDropEdit.AllowDrop = true;
			this.AdditionalAddressInfoDropEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("b010c50f-6f14-45ef-99e7-4be17422aedf", "Additional Address Info");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AdditionalAddressInfoDropEdit, false);
			this.AdditionalAddressInfoDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 7, true);
			this.AdditionalAddressInfoDropEdit.Name = "AdditionalAddressInfoDropEdit";
			this.AdditionalAddressInfoDropEdit.PreBoundMaxLength = 34;
			this.AdditionalAddressInfoDropEdit.ShowDescriptionBox = false;
			this.AdditionalAddressInfoDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.AdditionalAddressInfoDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 22, true);
			this.AdditionalAddressInfoDropEdit.TabIndex = 1;
			// 
			// AdditionalInfoLabel
			// 
			this.AdditionalInfoLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.AdditionalInfoLabel.AutoSize = true;
			this.AdditionalInfoLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 36, true);
			this.AdditionalInfoLabel.Name = "AdditionalInfoLabel";
			this.AdditionalInfoLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(97, 13, true);
			this.AdditionalInfoLabel.TabIndex = 5;
			this.AdditionalInfoLabel.UseMnemonic = false;
			// 
			// ZDocAdditionalAddressInfoControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AdditionalInfoLabel);
			this.Controls.Add(this.AdditionalAddressInfoDropEdit);
			this.Name = "ZDocAdditionalAddressInfoControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(235, 102, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AdditionalAddressInfoDropEdit.ResumeLayout(true);
			this.AdditionalAddressInfoDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZDropEdit AdditionalAddressInfoDropEdit;
		internal ZArchitecture.ZLabel AdditionalInfoLabel;
	}
}
