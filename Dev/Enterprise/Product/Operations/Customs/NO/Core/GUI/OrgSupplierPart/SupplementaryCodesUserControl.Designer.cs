namespace Enterprise.Customs.NO.GUI;

partial class SupplementaryCodesUserControl
{
	private void InitializeComponent()
	{
            this.Supplement1TextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.SupplementLabel1 = new Enterprise.ZArchitecture.ZLabel();
            this.Supplement2TextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.SupplementLabel2 = new Enterprise.ZArchitecture.ZLabel();
            this.CI_AdditionalSupplementsTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.AdditionalSupplementaryCodesEditButton = new Enterprise.ZArchitecture.GUI.ZButton();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NO.Business.CusClassPartPivot);
            // 
            // Supplement1TextBox
            // 
            this.BindingSource.SetBindingMember(this.Supplement1TextBox, "CI_Supplement1");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Business.CusClassPartPivot)(null)).CI_Supplement1)));
            this.Supplement1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 2, true);
            this.Supplement1TextBox.Name = "Supplement1TextBox";
            this.Supplement1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
            this.Supplement1TextBox.TabIndex = 0;
            // 
            // SupplementLabel1
            // 
            this.SupplementLabel1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.SupplementLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 2, true);
            this.SupplementLabel1.Name = "SupplementLabel1";
            this.SupplementLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(10, 20, true);
            this.SupplementLabel1.TabIndex = 1;
            this.SupplementLabel1.Text = "/";
            this.SupplementLabel1.UseMnemonic = false;
            // 
            // Supplement2TextBox
            // 
            this.BindingSource.SetBindingMember(this.Supplement2TextBox, "CI_Supplement2");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Business.CusClassPartPivot)(null)).CI_Supplement2)));
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.Supplement2TextBox, false);
            this.Supplement2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 2, true);
            this.Supplement2TextBox.Name = "Supplement2TextBox";
            this.Supplement2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
            this.Supplement2TextBox.TabIndex = 2;
            // 
            // SupplementLabel2
            // 
            this.SupplementLabel2.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.SupplementLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(195, 2, true);
            this.SupplementLabel2.Name = "SupplementLabel2";
            this.SupplementLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(10, 20, true);
            this.SupplementLabel2.TabIndex = 3;
            this.SupplementLabel2.Text = "/";
            this.SupplementLabel2.UseMnemonic = false;
            // 
            // CI_AdditionalSupplementsTextBox
            // 
            this.BindingSource.SetBindingMember(this.CI_AdditionalSupplementsTextBox, "CI_AdditionalSupplements");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Business.CusClassPartPivot)(null)).CI_AdditionalSupplements)));
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.CI_AdditionalSupplementsTextBox, false);
            this.CI_AdditionalSupplementsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(205, 2, true);
            this.CI_AdditionalSupplementsTextBox.Name = "CI_AdditionalSupplementsTextBox";
            this.CI_AdditionalSupplementsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
            this.CI_AdditionalSupplementsTextBox.TabIndex = 4;
            this.CI_AdditionalSupplementsTextBox.TabStop = false;
            // 
            // AdditionalSupplementaryCodesEditButton
            // 
            this.AdditionalSupplementaryCodesEditButton.CaptionResourceString = Enterprise.Customs.NO.GUI.Res.GetData("895025BD-F190-4E5B-8EAA-E44C086A04F3", "More...");
            this.AdditionalSupplementaryCodesEditButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(305, 0, true);
            this.AdditionalSupplementaryCodesEditButton.Name = "AdditionalSupplementaryCodesEditButton";
            this.AdditionalSupplementaryCodesEditButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
            this.AdditionalSupplementaryCodesEditButton.TabIndex = 5;
            this.AdditionalSupplementaryCodesEditButton.ToolTipCaption = null;
            this.AdditionalSupplementaryCodesEditButton.Click += new System.EventHandler(this.AdditionalSupplementaryCodesEditButton_Click);
            // 
            // SupplementaryCodesUserControl
            // 
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.Supplement1TextBox);
            this.Controls.Add(this.SupplementLabel1);
            this.Controls.Add(this.Supplement2TextBox);
            this.Controls.Add(this.SupplementLabel2);
            this.Controls.Add(this.CI_AdditionalSupplementsTextBox);
            this.Controls.Add(this.AdditionalSupplementaryCodesEditButton);
            this.Name = "SupplementaryCodesUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(374, 20, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

	}

	ZArchitecture.ZTextBox Supplement1TextBox;
	ZArchitecture.ZLabel SupplementLabel1;
	ZArchitecture.ZTextBox Supplement2TextBox;
	ZArchitecture.ZLabel SupplementLabel2;
	ZArchitecture.ZTextBox CI_AdditionalSupplementsTextBox;
	ZArchitecture.GUI.ZButton AdditionalSupplementaryCodesEditButton;
}
