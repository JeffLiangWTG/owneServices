using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.GUI
{
	partial class AdditionalSupplementaryCodesUserControl
	{
		void InitializeComponent()
		{
			this.AdditionalSupplementsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AdditionalSupplementaryCodesEditButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NO.Business.JobComInvoiceLine);
			// 
			// AdditionalSupplementsTextBox
			// 
			this.BindingSource.SetBindingMember(this.AdditionalSupplementsTextBox, "JI_AdditionalSupplements");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Business.JobComInvoiceLine)(null)).JI_AdditionalSupplements)));
			this.AdditionalSupplementsTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AdditionalSupplementsTextBox, false);
			this.AdditionalSupplementsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AdditionalSupplementsTextBox.Name = "AdditionalSupplementsTextBox";
			this.AdditionalSupplementsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
			this.AdditionalSupplementsTextBox.TabIndex = 0;
			this.AdditionalSupplementsTextBox.TabStop = false;
			// 
			// AdditionalSupplementaryCodesEditButton
			// 
			this.AdditionalSupplementaryCodesEditButton.IsCaptionOverridden = false;
			this.AdditionalSupplementaryCodesEditButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(181, 0, true);
			this.AdditionalSupplementaryCodesEditButton.Name = "AdditionalSupplementaryCodesEditButton";
			this.AdditionalSupplementaryCodesEditButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.AdditionalSupplementaryCodesEditButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 20, true);
			this.AdditionalSupplementaryCodesEditButton.TabIndex = 1;
			this.AdditionalSupplementaryCodesEditButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.AdditionalSupplementaryCodesEditButton.ToolTipCaption = null;
			this.AdditionalSupplementaryCodesEditButton.Click += new System.EventHandler(this.AdditionalSupplementaryCodesEditButton_Click);
			// 
			// AdditionalSupplementaryCodesUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AdditionalSupplementsTextBox);
			this.Controls.Add(this.AdditionalSupplementaryCodesEditButton);
			this.Name = "AdditionalSupplementaryCodesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		ZArchitecture.ZTextBox AdditionalSupplementsTextBox;
		ZButton AdditionalSupplementaryCodesEditButton;
	}
}
