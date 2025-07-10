using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.GUI
{
	public partial class ECCNCodesUserControl
	{
		private void InitializeComponent()
		{
			this.ECCNCodesAsStringTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ECCNCodesEditButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NL.Business.Declaration.JobComInvoiceLine);
			// 
			// ECCNCodesAsStringTextBox
			// 
			this.BindingSource.SetBindingMember(this.ECCNCodesAsStringTextBox, "ECCNCodesAsString");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NL.Business.Declaration.JobComInvoiceLine)(null)).ECCNCodesAsString)));
			this.ECCNCodesAsStringTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ECCNCodesAsStringTextBox, false);
			this.ECCNCodesAsStringTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ECCNCodesAsStringTextBox.Name = "ECCNCodesAsStringTextBox";
			this.ECCNCodesAsStringTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(180, 20, true);
			this.ECCNCodesAsStringTextBox.TabIndex = 0;
			this.ECCNCodesAsStringTextBox.TabStop = false;
			// 
			// ECCNCodesEditButton
			// 
			this.ECCNCodesEditButton.CaptionResourceString = Enterprise.Customs.NL.GUI.Res.GetData("06CEECE6-EBB7-4006-A32A-F3B31D4DB5AF", "More..");
			this.ECCNCodesEditButton.IsCaptionOverridden = false;
			this.ECCNCodesEditButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 0, true);
			this.ECCNCodesEditButton.Name = "ECCNCodesEditButton";
			this.ECCNCodesEditButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ECCNCodesEditButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 20, true);
			this.ECCNCodesEditButton.TabIndex = 1;
			this.ECCNCodesEditButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.ECCNCodesEditButton.ToolTipCaption = null;
			this.ECCNCodesEditButton.Click += new System.EventHandler(this.ECCNCodesEditButton_Click);
			// 
			// ECCNCodesUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ECCNCodesAsStringTextBox);
			this.Controls.Add(this.ECCNCodesEditButton);
			this.Name = "ECCNCodesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(228, 20, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private ZArchitecture.ZTextBox ECCNCodesAsStringTextBox;
		private ZButton ECCNCodesEditButton;
	}
}
