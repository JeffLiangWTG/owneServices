namespace Enterprise.MasterFiles.GUI
{
	public partial class RefCarrierConsortiumForm
	{
		#region Windows Form Designer generated code

		protected override void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.PostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.RG_CodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OrganisationModuleButtonGrid = new Enterprise.ZArchitecture.GUI.ZModuleButtonGrid();
			this.OrganisationsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.VesselsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.VesselsBoundButtonGrid = new Enterprise.ZArchitecture.GUI.ZModuleButtonGrid();
			this.OrgProxyFindBox = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.OrganisationModuleButtonGrid.InnerGrid)).BeginInit();
			this.OrganisationsGroupBox.SuspendLayout();
			this.VesselsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.VesselsBoundButtonGrid.InnerGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 361, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(664, 24, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(324);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(325);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefCarrierConsortium);
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(418, 335, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 23, true);
			this.PostingButtonsUserControl.TabIndex = 2;
			// 
			// RG_CodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.RG_CodeTextBox, "RG_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefCarrierConsortium)(null)).RG_Code)));
			this.RG_CodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(112, 7, true);
			this.RG_CodeTextBox.Name = "RG_CodeTextBox";
			this.RG_CodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(176, 20, true);
			this.RG_CodeTextBox.TabIndex = 0;
			// 
			// OrganisationModuleButtonGrid
			// 
			this.BindingSource.SetBindingMember(this.OrganisationModuleButtonGrid, "OrgHeaders");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefCarrierConsortium)(null)).OrgHeaders)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefCarrierConsortium)(null)).Lookups.ShippingLine_List)));
			this.OrganisationModuleButtonGrid.BindToFindBoxList = "Lookups+ShippingLine_List";
			zTextBoxColumnStyleInfo1.ColumnName = "OH_Code";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo2.ColumnName = "OH_FullName";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(205);
			zTextBoxColumnStyleInfo3.ColumnName = "OH_RL_NKClosestPort";
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			this.OrganisationModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.OrganisationModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.OrganisationModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.OrganisationModuleButtonGrid.GridId = "fffaa28e-b089-4a27-8be1-1bca5f64f94a";
			this.OrganisationModuleButtonGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			// 
			// 
			// 
			this.OrganisationModuleButtonGrid.InnerGrid.AllowNavigation = false;
			this.OrganisationModuleButtonGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.OrganisationModuleButtonGrid.InnerGrid.CaptionVisible = false;
			this.OrganisationModuleButtonGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OrganisationModuleButtonGrid.InnerGrid.IsWholeRowSelectedOnClick = true;
			this.OrganisationModuleButtonGrid.InnerGrid.LayoutKey = "Grid";
			this.OrganisationModuleButtonGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 2, true);
			this.OrganisationModuleButtonGrid.InnerGrid.Name = "Grid";
			this.OrganisationModuleButtonGrid.InnerGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.OrganisationModuleButtonGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(320, 231, true);
			this.OrganisationModuleButtonGrid.InnerGrid.TabIndex = 0;
			this.OrganisationModuleButtonGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.OrganisationModuleButtonGrid.Name = "OrganisationModuleButtonGrid";
			this.OrganisationModuleButtonGrid.NameOfAGridElement = Enterprise.MasterFiles.GUI.Res.GetData("43131589-48C7-4A7D-B31C-D4AD9074CA24", "Organization");
			this.OrganisationModuleButtonGrid.ReadOnly = false;
			this.OrganisationModuleButtonGrid.ShowNewButton = false;
			this.OrganisationModuleButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 269, true);
			this.OrganisationModuleButtonGrid.TabIndex = 0;
			// 
			// OrganisationsGroupBox
			// 
			this.OrganisationsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)));
			this.OrganisationsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCarrierConsortiumForm|e37f0dfe-6c3e-4046-ad0b-53cced182f31", "Organizations");
			this.OrganisationsGroupBox.Controls.Add(this.OrganisationModuleButtonGrid);
			this.OrganisationsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 37, true);
			this.OrganisationsGroupBox.Name = "OrganisationsGroupBox";
			this.OrganisationsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(336, 288, true);
			this.OrganisationsGroupBox.TabIndex = 4;
			this.OrganisationsGroupBox.TabStop = false;
			// 
			// VesselsGroupBox
			// 
			this.VesselsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.VesselsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCarrierConsortiumForm|1b5fcfe6-bd7b-486e-ab17-c587ab5f55f4", "Vessels");
			this.VesselsGroupBox.Controls.Add(this.VesselsBoundButtonGrid);
			this.VesselsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(345, 37, true);
			this.VesselsGroupBox.Name = "VesselsGroupBox";
			this.VesselsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 288, true);
			this.VesselsGroupBox.TabIndex = 5;
			this.VesselsGroupBox.TabStop = false;
			// 
			// VesselsBoundButtonGrid
			// 
			this.VesselsBoundButtonGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.VesselsBoundButtonGrid, "Vessels");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefCarrierConsortium)(null)).Vessels)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefCarrierConsortium)(null)).Lookups.AvailableVessel_List)));
			this.VesselsBoundButtonGrid.BindToFindBoxList = "Lookups+AvailableVessel_List";
			zGuidFindBoxColumnStyleInfo1.ColumnName = "RV_OH";
			zGuidFindBoxColumnStyleInfo1.IsVisible = false;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(220);
			zTextBoxColumnStyleInfo4.ColumnName = "RV_Code";
			zTextBoxColumnStyleInfo4.IsMandatory = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo5.ColumnName = "RV_LloydsNumber";
			zTextBoxColumnStyleInfo5.IsVisible = false;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
			zCheckBoxColumnStyleInfo1.ColumnName = "RV_IsActive";
			zCheckBoxColumnStyleInfo1.IsVisible = false;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo6.ColumnName = "RV_VesselType";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(85);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "RV_NetRegisterTon";
			zCalcEditColumnStyleInfo1.IsVisible = false;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			this.VesselsBoundButtonGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.VesselsBoundButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.VesselsBoundButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.VesselsBoundButtonGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.VesselsBoundButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.VesselsBoundButtonGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.VesselsBoundButtonGrid.GridId = "92752403-15c3-4aaf-a790-982e5c496375";
			// 
			// 
			// 
			this.VesselsBoundButtonGrid.InnerGrid.AllowNavigation = false;
			this.VesselsBoundButtonGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.VesselsBoundButtonGrid.InnerGrid.CaptionVisible = false;
			this.VesselsBoundButtonGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.VesselsBoundButtonGrid.InnerGrid.IsWholeRowSelectedOnClick = true;
			this.VesselsBoundButtonGrid.InnerGrid.LayoutKey = "Grid";
			this.VesselsBoundButtonGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 2, true);
			this.VesselsBoundButtonGrid.InnerGrid.Name = "Grid";
			this.VesselsBoundButtonGrid.InnerGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.VesselsBoundButtonGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(296, 232, true);
			this.VesselsBoundButtonGrid.InnerGrid.TabIndex = 0;
			this.VesselsBoundButtonGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 15, true);
			this.VesselsBoundButtonGrid.Name = "VesselsBoundButtonGrid";
			this.VesselsBoundButtonGrid.NameOfAGridElement = Enterprise.MasterFiles.GUI.Res.GetData("70B0FBF5-1DDC-4BA9-9F60-CA7C28C92124", "Vessel");
			this.VesselsBoundButtonGrid.ReadOnly = false;
			this.VesselsBoundButtonGrid.ShowNewButton = false;
			this.VesselsBoundButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(306, 270, true);
			this.VesselsBoundButtonGrid.TabIndex = 0;
			// 
			// OrgProxyFindBox
			// 
			this.BindingSource.SetBindingMember(this.OrgProxyFindBox, "RG_OH");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.RefCarrierConsortium)(null)).RG_OH)));
			this.OrgProxyFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(392, 7, true);
			this.OrgProxyFindBox.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
			this.OrgProxyFindBox.Name = "OrgProxyFindBox";
			this.OrgProxyFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(256, 20, true);
			this.OrgProxyFindBox.TabIndex = 1;
			// 
			// RefCarrierConsortiumForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(664, 385, true);
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCarrierConsortiumForm|fae29ad4-c57a-4c96-a39e-8cc0757d75df", "Consortium");
			this.Controls.Add(this.OrgProxyFindBox);
			this.Controls.Add(this.OrganisationsGroupBox);
			this.Controls.Add(this.PostingButtonsUserControl);
			this.Controls.Add(this.RG_CodeTextBox);
			this.Controls.Add(this.VesselsGroupBox);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefCarrierConsortium);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(603, 334, true);
			this.Name = "RefCarrierConsortiumForm";
			this.Controls.SetChildIndex(this.VesselsGroupBox, 0);
			this.Controls.SetChildIndex(this.RG_CodeTextBox, 0);
			this.Controls.SetChildIndex(this.PostingButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.OrganisationsGroupBox, 0);
			this.Controls.SetChildIndex(this.OrgProxyFindBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.OrganisationModuleButtonGrid.InnerGrid)).EndInit();
			this.OrganisationsGroupBox.ResumeLayout(false);
			this.VesselsGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.VesselsBoundButtonGrid.InnerGrid)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private Enterprise.Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		private Enterprise.ZArchitecture.GUI.ZModuleButtonGrid OrganisationModuleButtonGrid;
		private Enterprise.ZArchitecture.ZTextBox RG_CodeTextBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox OrganisationsGroupBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox VesselsGroupBox;
		private Enterprise.ZArchitecture.GUI.ZModuleButtonGrid VesselsBoundButtonGrid;
		private Enterprise.MasterFiles.GUI.ZOrganisationFindBox OrgProxyFindBox;
	}
}
