namespace Enterprise.MasterFiles.GUI
{
	partial class OrganizationInfoControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.companyNameLabel = new Enterprise.ZArchitecture.ZLabel();
			this.companyDunsLabel = new Enterprise.ZArchitecture.ZLabel();
			this.companyCityStatePostLabel = new Enterprise.ZArchitecture.ZLabel();
			this.cityStatePostInfoRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.companyInfoContentPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.companyAbnIDLabel = new Enterprise.ZArchitecture.ZLabel();
			this.companyAcnIDLabel = new Enterprise.ZArchitecture.ZLabel();
			this.companyDunsIDLabel = new Enterprise.ZArchitecture.ZLabel();
			this.companyScorePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.companyScoreLabel = new Enterprise.ZArchitecture.ZLabel();
			this.companyABNLabel = new Enterprise.ZArchitecture.ZLabel();
			this.companyACNLabel = new Enterprise.ZArchitecture.ZLabel();
			this.companyAddressLabel = new Enterprise.ZArchitecture.ZLabel();
			this.companyInfoPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.companyInfoHighlightPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.companyScoreToolTip = new CargoWise.Windows.UI.KToolTip(this.components);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.companyInfoContentPanel.SuspendLayout();
			this.companyScorePanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.GUI.CompanyLookupItemModel);
			// 
			// companyNameLabel
			// 
			this.companyNameLabel.AutoSize = true;
			this.companyNameLabel.BackColor = System.Drawing.Color.White;
			this.BindingSource.SetBindingMember(this.companyNameLabel, "CompanyItemName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.MasterFiles.GUI.CompanyLookupItemModel)(null)).CompanyItemName)));
			this.companyNameLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.companyNameLabel.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.companyNameLabel, false);
			this.companyNameLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(44, 7, true);
			this.companyNameLabel.Name = "companyNameLabel";
			this.companyNameLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.companyNameLabel.TabIndex = 4;
			this.companyNameLabel.UseMnemonic = false;
			// 
			// companyDunsLabel
			// 
			this.companyDunsLabel.AutoSize = true;
			this.companyDunsLabel.BackColor = System.Drawing.Color.White;
			this.companyDunsLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("11225684-00E3-4CE6-9985-55E8D0C0B177", "DUNS");
			this.companyDunsLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.companyDunsLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(134)))), ((int)(((byte)(180)))));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.companyDunsLabel, false);
			this.companyDunsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(426, 8, true);
			this.companyDunsLabel.Name = "companyDunsLabel";
			this.companyDunsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(37, 13, true);
			this.companyDunsLabel.TabIndex = 9;
			this.companyDunsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.companyDunsLabel.UseMnemonic = false;
			// 
			// companyCityStatePostLabel
			// 
			this.companyCityStatePostLabel.AutoSize = true;
			this.companyCityStatePostLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.companyCityStatePostLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(135)))), ((int)(((byte)(135)))), ((int)(((byte)(135)))));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.companyCityStatePostLabel, false);
			this.companyCityStatePostLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(44, 44, true);
			this.companyCityStatePostLabel.Name = "companyCityStatePostLabel";
			this.companyCityStatePostLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.companyCityStatePostLabel.TabIndex = 6;
			this.companyCityStatePostLabel.UseMnemonic = false;
			// 
			// cityStatePostInfoRadioButton
			// 
			this.cityStatePostInfoRadioButton.AutoCheck = false;
			this.cityStatePostInfoRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.cityStatePostInfoRadioButton, "Selected");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.MasterFiles.GUI.CompanyLookupItemModel)(null)).Selected)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.cityStatePostInfoRadioButton, false);
			this.cityStatePostInfoRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 22, true);
			this.cityStatePostInfoRadioButton.Name = "cityStatePostInfoRadioButton";
			this.cityStatePostInfoRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(13, 13, true);
			this.cityStatePostInfoRadioButton.TabIndex = 3;
			this.cityStatePostInfoRadioButton.TabStop = true;
			this.cityStatePostInfoRadioButton.UseVisualStyleBackColor = true;
			this.cityStatePostInfoRadioButton.Click += new System.EventHandler(this.CityStatePostInfoRadioButton_Click);
			// 
			// companyInfoContentPanel
			// 
			this.companyInfoContentPanel.AutoSize = true;
			this.companyInfoContentPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.companyInfoContentPanel.BackColor = System.Drawing.Color.White;
			this.companyInfoContentPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.companyInfoContentPanel.Controls.Add(this.companyAbnIDLabel);
			this.companyInfoContentPanel.Controls.Add(this.companyAcnIDLabel);
			this.companyInfoContentPanel.Controls.Add(this.companyDunsIDLabel);
			this.companyInfoContentPanel.Controls.Add(this.companyScorePanel);
			this.companyInfoContentPanel.Controls.Add(this.companyABNLabel);
			this.companyInfoContentPanel.Controls.Add(this.companyACNLabel);
			this.companyInfoContentPanel.Controls.Add(this.companyAddressLabel);
			this.companyInfoContentPanel.Controls.Add(this.companyDunsLabel);
			this.companyInfoContentPanel.Controls.Add(this.companyInfoPanel);
			this.companyInfoContentPanel.Controls.Add(this.cityStatePostInfoRadioButton);
			this.companyInfoContentPanel.Controls.Add(this.companyNameLabel);
			this.companyInfoContentPanel.Controls.Add(this.companyCityStatePostLabel);
			this.companyInfoContentPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.companyInfoContentPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.companyInfoContentPanel.Name = "companyInfoContentPanel";
			this.companyInfoContentPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(626, 77, true);
			this.companyInfoContentPanel.TabIndex = 1;
			this.companyInfoContentPanel.Click += new System.EventHandler(this.CompanyInfoContentPanel_Click);
			this.companyInfoContentPanel.MouseEnter += new System.EventHandler(this.CompanyInfoContentPanel_MouseEnter);
			this.companyInfoContentPanel.MouseLeave += new System.EventHandler(this.CompanyInfoContentPanel_MouseLeave);
			// 
			// companyAbnIDLabel
			// 
			this.companyAbnIDLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.companyAbnIDLabel, "ABN");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.MasterFiles.GUI.CompanyLookupItemModel)(null)).ABN)));
			this.companyAbnIDLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.companyAbnIDLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(135)))), ((int)(((byte)(135)))), ((int)(((byte)(135)))));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.companyAbnIDLabel, false);
			this.companyAbnIDLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(503, 40, true);
			this.companyAbnIDLabel.Name = "companyAbnIDLabel";
			this.companyAbnIDLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.companyAbnIDLabel.TabIndex = 14;
			this.companyAbnIDLabel.UseMnemonic = false;
			// 
			// companyAcnIDLabel
			// 
			this.companyAcnIDLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.companyAcnIDLabel, "ACN");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.MasterFiles.GUI.CompanyLookupItemModel)(null)).ACN)));
			this.companyAcnIDLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.companyAcnIDLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(135)))), ((int)(((byte)(135)))), ((int)(((byte)(135)))));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.companyAcnIDLabel, false);
			this.companyAcnIDLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(503, 22, true);
			this.companyAcnIDLabel.Name = "companyAcnIDLabel";
			this.companyAcnIDLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.companyAcnIDLabel.TabIndex = 13;
			this.companyAcnIDLabel.UseMnemonic = false;
			// 
			// companyDunsIDLabel
			// 
			this.companyDunsIDLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.companyDunsIDLabel, "DUNS");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.MasterFiles.GUI.CompanyLookupItemModel)(null)).DUNS)));
			this.companyDunsIDLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.companyDunsIDLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(135)))), ((int)(((byte)(135)))), ((int)(((byte)(135)))));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.companyDunsIDLabel, false);
			this.companyDunsIDLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(503, 8, true);
			this.companyDunsIDLabel.Name = "companyDunsIDLabel";
			this.companyDunsIDLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.companyDunsIDLabel.TabIndex = 12;
			this.companyDunsIDLabel.UseMnemonic = false;
			// 
			// companyScorePanel
			// 
			this.companyScorePanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(241)))), ((int)(((byte)(242)))), ((int)(((byte)(247)))));
			this.companyScorePanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.companyScorePanel.Controls.Add(this.companyScoreLabel);
			this.companyScorePanel.Enabled = false;
			this.companyScorePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(241, 35, true);
			this.companyScorePanel.Name = "companyScorePanel";
			this.companyScorePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(132, 26, true);
			this.companyScorePanel.TabIndex = 7;
			this.companyScorePanel.Visible = false;
			// 
			// companyScoreLabel
			// 
			this.companyScoreLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.companyScoreLabel, "CompanyItemScore");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.MasterFiles.GUI.CompanyLookupItemModel)(null)).CompanyItemScore)));
			this.companyScoreLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.companyScoreLabel, false);
			this.companyScoreLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 5, true);
			this.companyScoreLabel.Name = "companyScoreLabel";
			this.companyScoreLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.companyScoreLabel.TabIndex = 8;
			this.companyScoreLabel.UseMnemonic = false;
			// 
			// companyABNLabel
			// 
			this.companyABNLabel.AutoSize = true;
			this.companyABNLabel.BackColor = System.Drawing.Color.White;
			this.companyABNLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("606F6510-4215-4CB1-A1DF-F0B45B98CBF9", "ABN");
			this.companyABNLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.companyABNLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(134)))), ((int)(((byte)(180)))));
			this.companyABNLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(426, 40, true);
			this.companyABNLabel.Name = "companyABNLabel";
			this.companyABNLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(29, 13, true);
			this.companyABNLabel.TabIndex = 11;
			this.companyABNLabel.UseMnemonic = false;
			// 
			// companyACNLabel
			// 
			this.companyACNLabel.AutoSize = true;
			this.companyACNLabel.BackColor = System.Drawing.Color.White;
			this.companyACNLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("003E5280-EF0A-4F61-9E32-09BA79F3B0F8", "ACN");
			this.companyACNLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.companyACNLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(88)))), ((int)(((byte)(134)))), ((int)(((byte)(180)))));
			this.companyACNLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(426, 24, true);
			this.companyACNLabel.Name = "companyACNLabel";
			this.companyACNLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(29, 13, true);
			this.companyACNLabel.TabIndex = 10;
			this.companyACNLabel.UseMnemonic = false;
			// 
			// companyAddressLabel
			// 
			this.companyAddressLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.companyAddressLabel, "CompanyItemAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.MasterFiles.GUI.CompanyLookupItemModel)(null)).CompanyItemAddress)));
			this.companyAddressLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.companyAddressLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(135)))), ((int)(((byte)(135)))), ((int)(((byte)(135)))));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.companyAddressLabel, false);
			this.companyAddressLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(44, 26, true);
			this.companyAddressLabel.Name = "companyAddressLabel";
			this.companyAddressLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.companyAddressLabel.TabIndex = 5;
			this.companyAddressLabel.UseMnemonic = false;
			// 
			// companyInfoPanel
			// 
			this.companyInfoPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
			this.companyInfoPanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.companyInfoPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.companyInfoPanel.Name = "companyInfoPanel";
			this.companyInfoPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(10, 76, true);
			this.companyInfoPanel.TabIndex = 2;
			// 
			// companyInfoHighlightPanel
			// 
			this.companyInfoHighlightPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.companyInfoHighlightPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(225)))), ((int)(((byte)(228)))));
			this.companyInfoHighlightPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 1, true);
			this.companyInfoHighlightPanel.Name = "companyInfoHighlightPanel";
			this.companyInfoHighlightPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 0, true);
			this.companyInfoHighlightPanel.TabIndex = 0;
			// 
			// OrganizationInfoControl
			// 
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.companyInfoContentPanel);
			this.Controls.Add(this.companyInfoHighlightPanel);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.Name = "OrganizationInfoControl";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(630, 81, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.companyInfoContentPanel.ResumeLayout(false);
			this.companyInfoContentPanel.PerformLayout();
			this.companyScorePanel.ResumeLayout(false);
			this.companyScorePanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private ZArchitecture.ZLabel companyNameLabel;
		private ZArchitecture.ZLabel companyDunsLabel;
		private ZArchitecture.ZLabel companyCityStatePostLabel;
		private Enterprise.ZArchitecture.GUI.ZRadioButton cityStatePostInfoRadioButton;
		private ZArchitecture.GUI.ZPanel companyInfoContentPanel;
		private ZArchitecture.GUI.ZPanel companyInfoPanel;
		private ZArchitecture.GUI.ZPanel companyInfoHighlightPanel;
		private ZArchitecture.GUI.ZPanel companyScorePanel;
		private ZArchitecture.ZLabel companyScoreLabel;
		private CargoWise.Windows.UI.KToolTip companyScoreToolTip;
		private ZArchitecture.ZLabel companyAddressLabel;
		private ZArchitecture.ZLabel companyABNLabel;
		private ZArchitecture.ZLabel companyACNLabel;
		private ZArchitecture.ZLabel companyAbnIDLabel;
		private ZArchitecture.ZLabel companyAcnIDLabel;
		private ZArchitecture.ZLabel companyDunsIDLabel;
	}
}
