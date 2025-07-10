namespace Enterprise.Freight.Agency.GUI
{
	public partial class OuterVoyageAllocationControl
	{
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.principalTabControl = new Enterprise.Freight.Agency.GUI.BindingTabControl();
			this.principalsBoundModuleButtonGrid = new Enterprise.ZArchitecture.GUI.ZModuleButtonGrid();
			this.allocationByPrincipalHelpMessageLabel = new Enterprise.ZArchitecture.ZLabel();
			mainPrincipalTab = new Enterprise.ZArchitecture.GUI.ZTabPage();
			allocationByPrincipalGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			principalDetailsPanel = new CargoWise.Windows.UI.KPanel();
			byPrincipalCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			generalDetailsPanel = new CargoWise.Windows.UI.KPanel();
			allocationMethodDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			currentCountryField = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.principalTabControl.SuspendLayout();
			mainPrincipalTab.SuspendLayout();
			allocationByPrincipalGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.principalsBoundModuleButtonGrid.InnerGrid)).BeginInit();
			principalDetailsPanel.SuspendLayout();
			generalDetailsPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Agency.Business.AgencyCountry);
			// 
			// principalTabControl
			// 
			this.principalTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.principalTabControl.Controls.Add(mainPrincipalTab);
			this.principalTabControl.DataCollection = null;
			this.principalTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.principalTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 0, true);
			this.principalTabControl.Name = "principalTabControl";
			this.principalTabControl.SelectedIndex = 0;
			this.principalTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(698, 421, true);
			this.principalTabControl.TabIndex = 0;
			// 
			// mainPrincipalTab
			// 
			mainPrincipalTab.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("OuterVoyageAllocationControl|f4a73896-5919-44cb-ab7d-2708fb72b6e2", "Allocation Method");
			mainPrincipalTab.Controls.Add(allocationByPrincipalGroupBox);
			mainPrincipalTab.Controls.Add(generalDetailsPanel);
			mainPrincipalTab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			mainPrincipalTab.Name = "mainPrincipalTab";
			mainPrincipalTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(690, 394, true);
			mainPrincipalTab.TabIndex = 0;
			// 
			// allocationByPrincipalGroupBox
			// 
			allocationByPrincipalGroupBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("OuterVoyageAllocationControl|f9ddeb84-2c28-4836-a232-5866fde7c8a7", "Allocation by Principal");
			allocationByPrincipalGroupBox.Controls.Add(this.principalsBoundModuleButtonGrid);
			allocationByPrincipalGroupBox.Controls.Add(principalDetailsPanel);
			allocationByPrincipalGroupBox.Controls.Add(this.allocationByPrincipalHelpMessageLabel);
			allocationByPrincipalGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			allocationByPrincipalGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 64, true);
			allocationByPrincipalGroupBox.Name = "allocationByPrincipalGroupBox";
			allocationByPrincipalGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(690, 330, true);
			allocationByPrincipalGroupBox.TabIndex = 2;
			allocationByPrincipalGroupBox.TabStop = false;
			// 
			// principalsBoundModuleButtonGrid
			// 
			this.BindingSource.SetBindingMember(this.principalsBoundModuleButtonGrid, "Principals");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.AgencyCountry)(null)).Principals)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Freight.Agency.Business.AgencyCountry)(null)).Lookups.Principals)));
			this.principalsBoundModuleButtonGrid.BindToFindBoxList = "Lookups.Principals";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("ZModuleButtonGrid|ac7aa14d-a23c-4293-8a00-dd43d754f574", "Principal Code");
			zTextBoxColumnStyleInfo1.ColumnName = "OH_Code";
			zTextBoxColumnStyleInfo2.ColumnName = "OH_FullName";
			this.principalsBoundModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.principalsBoundModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.principalsBoundModuleButtonGrid.GridId = "363b036a-c923-4b17-9c32-37d6f9989acb";
			this.principalsBoundModuleButtonGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			// 
			// 
			// 
			this.principalsBoundModuleButtonGrid.InnerGrid.AllowNavigation = false;
			this.principalsBoundModuleButtonGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.principalsBoundModuleButtonGrid.InnerGrid.CaptionVisible = false;
			this.principalsBoundModuleButtonGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.principalsBoundModuleButtonGrid.InnerGrid.IsWholeRowSelectedOnClick = true;
			this.principalsBoundModuleButtonGrid.InnerGrid.LayoutKey = "Grid";
			this.principalsBoundModuleButtonGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 5, true);
			this.principalsBoundModuleButtonGrid.InnerGrid.Name = "Grid";
			this.principalsBoundModuleButtonGrid.InnerGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.Remove;
			this.principalsBoundModuleButtonGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(674, 193, true);
			this.principalsBoundModuleButtonGrid.InnerGrid.TabIndex = 0;
			this.principalsBoundModuleButtonGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 96, true);
			this.principalsBoundModuleButtonGrid.Name = "principalsBoundModuleButtonGrid";
			this.principalsBoundModuleButtonGrid.NameOfAGridElement = Enterprise.Freight.Agency.GUI.Res.GetData("28BBFD64-ED04-4992-8D64-A081AD8F5CA8", "Principal");
			this.principalsBoundModuleButtonGrid.ReadOnly = false;
			this.principalsBoundModuleButtonGrid.ShowEditButton = false;
			this.principalsBoundModuleButtonGrid.ShowNewButton = false;
			this.principalsBoundModuleButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(684, 231, true);
			this.principalsBoundModuleButtonGrid.TabIndex = 4;
			// 
			// principalDetailsPanel
			// 
			principalDetailsPanel.Controls.Add(byPrincipalCheckBox);
			principalDetailsPanel.Dock = System.Windows.Forms.DockStyle.Top;
			principalDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 66, true);
			principalDetailsPanel.Name = "principalDetailsPanel";
			principalDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(684, 30, true);
			principalDetailsPanel.TabIndex = 6;
			// 
			// byPrincipalCheckBox
			// 
			byPrincipalCheckBox.AutoSize = true;
			byPrincipalCheckBox.BackColor = System.Drawing.SystemColors.Control;
			this.BindingSource.SetBindingMember(byPrincipalCheckBox, "J0_AllocationsByPrincipal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Freight.Agency.Business.AgencyCountry)(null)).J0_AllocationsByPrincipal)));
			byPrincipalCheckBox.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("OuterVoyageAllocationControl|e683c457-ae64-4677-a5e7-2d1a32feee4d", "By Principal");
			byPrincipalCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			byPrincipalCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			byPrincipalCheckBox.Name = "byPrincipalCheckBox";
			byPrincipalCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(81, 17, true);
			byPrincipalCheckBox.TabIndex = 3;
			byPrincipalCheckBox.UseVisualStyleBackColor = false;
			// 
			// allocationByPrincipalHelpMessageLabel
			// 
			this.allocationByPrincipalHelpMessageLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.allocationByPrincipalHelpMessageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.allocationByPrincipalHelpMessageLabel.Name = "allocationByPrincipalHelpMessageLabel";
			this.allocationByPrincipalHelpMessageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(684, 50, true);
			this.allocationByPrincipalHelpMessageLabel.TabIndex = 5;
			// 
			// generalDetailsPanel
			// 
			generalDetailsPanel.Controls.Add(allocationMethodDropEdit);
			generalDetailsPanel.Controls.Add(currentCountryField);
			generalDetailsPanel.Dock = System.Windows.Forms.DockStyle.Top;
			generalDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			generalDetailsPanel.Name = "generalDetailsPanel";
			generalDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(690, 64, true);
			generalDetailsPanel.TabIndex = 0;
			// 
			// allocationMethodDropEdit
			// 
			this.BindingSource.SetBindingMember(allocationMethodDropEdit, "J0_AllocationMethod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.Agency.Business.AgencyCountry)(null)).J0_AllocationMethod)));
			allocationMethodDropEdit.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("OuterVoyageAllocationControl|8027fe67-c6b6-45a5-b7d0-71d1188393b1", "Method", "Allocation Method", "The allocation method defines how the allocations are entered.\r\nSAI -- The allocations are entered and checked at the sailing level.\r\nORI -- The allocations are entered and checked at the origin level.\r\nCOU -- The allocations are entered and checked at the country level.");
			allocationMethodDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 32, true);
			allocationMethodDropEdit.Name = "allocationMethodDropEdit";
			allocationMethodDropEdit.PreBoundMaxLength = 3;
			allocationMethodDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(169, 20, true);
			allocationMethodDropEdit.TabIndex = 1;
			// 
			// currentCountryField
			// 
			currentCountryField.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(currentCountryField, "CountryName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Agency.Business.AgencyCountry)(null)).CountryName)));
			currentCountryField.CaptionResourceString = Enterprise.Freight.Agency.GUI.Res.GetData("OuterVoyageAllocationControl|f25fea85-ba38-46c1-87b1-bb8abaf0abd5", "Country/Region", "The name of the current country/region.");
			currentCountryField.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 8, true);
			currentCountryField.Name = "currentCountryField";
			currentCountryField.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 20, true);
			currentCountryField.TabIndex = 0;
			// 
			// OuterVoyageAllocationControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.principalTabControl);
			this.Name = "OuterVoyageAllocationControl";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 0, 3, 3, true);
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(704, 424, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.principalTabControl.ResumeLayout(false);
			mainPrincipalTab.ResumeLayout(false);
			allocationByPrincipalGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.principalsBoundModuleButtonGrid.InnerGrid)).EndInit();
			principalDetailsPanel.ResumeLayout(false);
			principalDetailsPanel.PerformLayout();
			generalDetailsPanel.ResumeLayout(false);
			generalDetailsPanel.PerformLayout();
			this.ResumeLayout(false);

		}

		BindingTab genericTab;
		BindingTabControl principalTabControl;
		Enterprise.ZArchitecture.GUI.ZModuleButtonGrid principalsBoundModuleButtonGrid;
		Enterprise.ZArchitecture.ZLabel allocationByPrincipalHelpMessageLabel;
		Enterprise.ZArchitecture.GUI.ZTabPage mainPrincipalTab;
		Enterprise.ZArchitecture.GUI.ZGroupBox allocationByPrincipalGroupBox;
		CargoWise.Windows.UI.KPanel principalDetailsPanel;
		Enterprise.ZArchitecture.GUI.ZCheckBox byPrincipalCheckBox;
		CargoWise.Windows.UI.KPanel generalDetailsPanel;
		Enterprise.ZArchitecture.GUI.ZDropEdit allocationMethodDropEdit;
		Enterprise.ZArchitecture.ZTextBox currentCountryField;
	}
}
