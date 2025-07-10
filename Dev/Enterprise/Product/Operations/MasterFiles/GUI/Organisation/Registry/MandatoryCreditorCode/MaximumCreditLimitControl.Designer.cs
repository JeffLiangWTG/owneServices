namespace Enterprise.MasterFiles.GUI
{
	partial class MaximumCreditLimitControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.MaximumCreditLimitRegistryGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ActionGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ActionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ApplyToAllOrganizationsButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.MaximumCreditLimitRegistryGrid)).BeginInit();
			this.MaximumCreditLimitRegistryGrid.SuspendLayout();
			this.ActionGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.MaximumCreditLimitCollection);
			// 
			// MaximumCreditLimitRegistryGrid
			// 
			this.MaximumCreditLimitRegistryGrid.AllowNavigation = false;
			this.MaximumCreditLimitRegistryGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.MaximumCreditLimitRegistryGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.MaximumCreditLimitItem)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.MaximumCreditLimitItem)(null)).CreditLimit)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.MaximumCreditLimitItem)(null)).CurrencyPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.MaximumCreditLimitItem)(null)).ComprehensiveReportEnabled)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.MaximumCreditLimitItem)(null)).FailureRiskEnabled)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.MaximumCreditLimitItem)(null)).LatePaymentRiskEnabled)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.MaximumCreditLimitItem)(null)).CommercialBureauEnquiryEnabled)));
			this.MaximumCreditLimitRegistryGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("E7C7458E-7D53-4098-A13A-FE8FF806EEC7", "Credit Limit");
			zTextBoxColumnStyleInfo1.ColumnName = "CreditLimit";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("8C254C32-1339-4743-A4F5-611B1F6200CD", "Currency");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "CurrencyPK";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("28FE6D7F-B597-4FD8-B6C7-256FFA7CDAB6", "Comprehensive Report");
			zCheckBoxColumnStyleInfo1.ColumnName = "ComprehensiveReportEnabled";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("FAC10688-89BE-4B44-A29A-0064E2D94277", "Failure Risk");
			zCheckBoxColumnStyleInfo2.ColumnName = "FailureRiskEnabled";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("6A3357E2-E646-4A35-AB4A-757F5BB7DAD4", "Late Payment Risk");
			zCheckBoxColumnStyleInfo3.ColumnName = "LatePaymentRiskEnabled";
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(104);
			zCheckBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AED90D06-793D-43D1-99C8-14BC9D16AD06", "Commercial Bureau Enquiry");
			zCheckBoxColumnStyleInfo4.ColumnName = "CommercialBureauEnquiryEnabled";
			zCheckBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(144);
			this.MaximumCreditLimitRegistryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.MaximumCreditLimitRegistryGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.MaximumCreditLimitRegistryGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.MaximumCreditLimitRegistryGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.MaximumCreditLimitRegistryGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.MaximumCreditLimitRegistryGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);
			this.MaximumCreditLimitRegistryGrid.GridId = "6A3C7173-41E7-4ACD-AF7E-28EEB4DC7D0E";
			this.MaximumCreditLimitRegistryGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MaximumCreditLimitRegistryGrid.LayoutKey = "MaximumCreditLimitRegistryGrid";
			this.MaximumCreditLimitRegistryGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MaximumCreditLimitRegistryGrid.Name = "MaximumCreditLimitRegistryGrid";
			this.MaximumCreditLimitRegistryGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(594, 300, true);
			this.MaximumCreditLimitRegistryGrid.TabIndex = 0;
			// 
			// ActionGroupBox
			// 
			this.ActionGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("41CE236D-3616-40F8-A4E9-76CDB62B09B2", "Action");
			this.ActionGroupBox.Controls.Add(this.ActionLabel);
			this.ActionGroupBox.Controls.Add(this.ApplyToAllOrganizationsButton);
			this.ActionGroupBox.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ActionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 310, true);
			this.ActionGroupBox.Name = "ActionGroupBox";
			this.ActionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(594, 190, true);
			this.ActionGroupBox.TabIndex = 1;
			this.ActionGroupBox.TabStop = false;
			// 
			// ActionLabel
			// 
			this.ActionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ActionLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ActionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 15, true);
			this.ActionLabel.Name = "ActionLabel";
			this.ActionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(585, 138, true);
			this.ActionLabel.TabIndex = 0;
			this.ActionLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// ApplyToAllOrganizationsButton
			// 
			this.ApplyToAllOrganizationsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ApplyToAllOrganizationsButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("2DFCB22B-8897-4973-B4BB-DD483635A225", "Apply to all Organizations");
			this.ApplyToAllOrganizationsButton.IsCaptionOverridden = false;
			this.ApplyToAllOrganizationsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(411, 162, true);
			this.ApplyToAllOrganizationsButton.Name = "ApplyToAllOrganizationsButton";
			this.ApplyToAllOrganizationsButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ApplyToAllOrganizationsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(176, 23, true);
			this.ApplyToAllOrganizationsButton.TabIndex = 1;
			this.ApplyToAllOrganizationsButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.ApplyToAllOrganizationsButton.ToolTipCaption = null;
			this.ApplyToAllOrganizationsButton.Click += new System.EventHandler(this.ApplyToAllOrganizationsButton_Click);
			// 
			// MaximumCreditLimitControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MaximumCreditLimitRegistryGrid);
			this.Controls.Add(this.ActionGroupBox);
			this.Name = "MaximumCreditLimitControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(594, 500, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.MaximumCreditLimitRegistryGrid)).EndInit();
			this.MaximumCreditLimitRegistryGrid.ResumeLayout(false);
			this.MaximumCreditLimitRegistryGrid.PerformLayout();
			this.ActionGroupBox.ResumeLayout(false);
			this.ActionGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal Enterprise.ZArchitecture.ZGrid MaximumCreditLimitRegistryGrid;
		private ZArchitecture.GUI.ZGroupBox ActionGroupBox;
		private ZArchitecture.ZLabel ActionLabel;
		private ZArchitecture.GUI.ZButton ApplyToAllOrganizationsButton;

		#endregion
	}
}
