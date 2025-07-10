namespace Enterprise.MasterFiles.GUI
{
	public partial class RefPostCodeForm
	{
		#region Windows Form Designer generated code

		new void InitializeComponent()
		{
			this.MainTabControl.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(687, 345, true);
			this.MainTabControl.TabIndex = 0;
			// 
			// MainTabPage
			// 
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 318, true);
			this.MainTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MainTabPage_InitializeTab));
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 318, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(687, 345, true);
			// 
			// PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel
			// 
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.TabIndex = 0;
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(687, 24, true);
			this.MainStatusBar.TabIndex = 0;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefPostCode);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefPostCode)(null)).RK_CityTownPostCode)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefPostCode)(null)).RK_RN_NKCountry)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.RefPostCode)(null)).RK_Lattitude)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.RefPostCode)(null)).RK_Longitude)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefPostCode)(null)).CityTowns)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefPostCode)(null)).Lookups.CityTowns)));
			// 
			// 
			// 
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefPostCode)(null)).RK_IsActive)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefPostCode)(null)).RK_IsSystem)));
			// 
			// RefPostCodeForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("47924d32-e296-4691-8183-3fc88cbb9aa4", "Postcode");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(687, 401, true);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefPostCode);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(575, 272, true);
			this.Name = "RefPostCodeForm";
			this.ShouldSerializeTabPageMethods = true;
			this.MainTabControl.ResumeLayout(false);
			this.MainPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		void MainTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.RK_CityTownPostCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RK_RN_NKCountryFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.RK_LattitudeCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.RK_LongitudeCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.zModuleButtonGrid1 = new Enterprise.ZArchitecture.GUI.ZModuleButtonGrid();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RK_IsActiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RK_IsSystemCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.MainTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.zModuleButtonGrid1.InnerGrid)).BeginInit();
			this.zGroupBox1.SuspendLayout();
			this.MainTabPage.Controls.Add(this.RK_IsSystemCheckBox);
			this.MainTabPage.Controls.Add(this.RK_IsActiveCheckBox);
			this.MainTabPage.Controls.Add(this.zGroupBox1);
			this.MainTabPage.Controls.Add(this.RK_LongitudeCalcEdit);
			this.MainTabPage.Controls.Add(this.RK_LattitudeCalcEdit);
			this.MainTabPage.Controls.Add(this.RK_RN_NKCountryFindBox);
			this.MainTabPage.Controls.Add(this.RK_CityTownPostCodeTextBox);
			// 
			// RK_CityTownPostCodeTextBox
			// 
			this.RK_CityTownPostCodeTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.RK_CityTownPostCodeTextBox, "RK_CityTownPostCode");
			this.RK_CityTownPostCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 15, true);
			this.RK_CityTownPostCodeTextBox.Name = "RK_CityTownPostCodeTextBox";
			this.RK_CityTownPostCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 20, true);
			this.RK_CityTownPostCodeTextBox.TabIndex = 0;
			// 
			// RK_RN_NKCountryFindBox
			// 
			this.RK_RN_NKCountryFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RK_RN_NKCountryFindBox, "RK_RN_NKCountry");
			this.RK_RN_NKCountryFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 93, true);
			this.RK_RN_NKCountryFindBox.Name = "RK_RN_NKCountryFindBox";
			this.RK_RN_NKCountryFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(221, 20, true);
			this.RK_RN_NKCountryFindBox.TabIndex = 3;
			// 
			// RK_LattitudeCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.RK_LattitudeCalcEdit, "RK_Lattitude");
			this.RK_LattitudeCalcEdit.DecimalPlaces = 6;
			this.RK_LattitudeCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 41, true);
			this.RK_LattitudeCalcEdit.Name = "RK_LattitudeCalcEdit";
			this.RK_LattitudeCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.RK_LattitudeCalcEdit.TabIndex = 1;
			this.RK_LattitudeCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// RK_LongitudeCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.RK_LongitudeCalcEdit, "RK_Longitude");
			this.RK_LongitudeCalcEdit.DecimalPlaces = 6;
			this.RK_LongitudeCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 67, true);
			this.RK_LongitudeCalcEdit.Name = "RK_LongitudeCalcEdit";
			this.RK_LongitudeCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.RK_LongitudeCalcEdit.TabIndex = 2;
			this.RK_LongitudeCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// zModuleButtonGrid1
			// 
			this.zModuleButtonGrid1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zModuleButtonGrid1, "CityTowns");
			this.zModuleButtonGrid1.BindToFindBoxList = "Lookups.CityTowns";
			zTextBoxColumnStyleInfo5.ColumnName = "R9_InternationalName";
			zTextBoxColumnStyleInfo5.IsSortable = false;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zTextBoxColumnStyleInfo6.ColumnName = "R9_LocalLanguageName";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zTextBoxColumnStyleInfo7.ColumnName = "R9_RN_NKCountry";
			zTextBoxColumnStyleInfo8.ColumnName = "R9_RW_NKState";
			zCheckBoxColumnStyleInfo3.ColumnName = "R9_IsActive";
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCheckBoxColumnStyleInfo4.ColumnName = "R9_IsSystem";
			zCheckBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			this.zModuleButtonGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.zModuleButtonGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.zModuleButtonGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.zModuleButtonGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.zModuleButtonGrid1.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.zModuleButtonGrid1.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);
			this.zModuleButtonGrid1.GridId = "a3528a4b-0908-4def-9222-6c0a3abba273";
			this.zModuleButtonGrid1.InnerGrid.AllowNavigation = false;
			this.zModuleButtonGrid1.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.zModuleButtonGrid1.InnerGrid.CaptionVisible = false;
			this.zModuleButtonGrid1.InnerGrid.CopySelectedRowsAllowed = true;
			this.zModuleButtonGrid1.InnerGrid.GridId = null;
			this.zModuleButtonGrid1.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zModuleButtonGrid1.InnerGrid.LayoutKey = "Grid";
			this.zModuleButtonGrid1.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.zModuleButtonGrid1.InnerGrid.Name = "Grid";
			this.zModuleButtonGrid1.InnerGrid.ReadOnly = true;
			this.zModuleButtonGrid1.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(645, 130, true);
			this.zModuleButtonGrid1.InnerGrid.TabIndex = 0;
			this.zModuleButtonGrid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.zModuleButtonGrid1.Name = "zModuleButtonGrid1";
			this.zModuleButtonGrid1.NameOfAGridElement = Enterprise.MasterFiles.GUI.Res.GetData("354c4224-9f18-4ede-a36d-5c36fc27df21", "City Town");
			this.zModuleButtonGrid1.ReadOnly = true;
			this.zModuleButtonGrid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(651, 171, true);
			this.zModuleButtonGrid1.TabIndex = 4;
			this.zModuleButtonGrid1.Detaching += new Enterprise.ZArchitecture.GUI.ModuleButtonGridOperationCancelEventHandler(this.zModuleButtonGrid1_Detaching);
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("6cab9ce4-4234-4256-81c9-5e2056397e2f", "Related cities/towns");
			this.zGroupBox1.Controls.Add(this.zModuleButtonGrid1);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 119, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(663, 196, true);
			this.zGroupBox1.TabIndex = 5;
			this.zGroupBox1.TabStop = false;
			// 
			// RK_IsActiveCheckBox
			// 
			this.RK_IsActiveCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.RK_IsActiveCheckBox, "RK_IsActive");
			this.RK_IsActiveCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefPostCodeForm|d4e8fb04-f0b8-4085-9133-b86d20116d0d", "Is Active");
			this.RK_IsActiveCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RK_IsActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(404, 15, true);
			this.RK_IsActiveCheckBox.Name = "RK_IsActiveCheckBox";
			this.RK_IsActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(68, 17, true);
			this.RK_IsActiveCheckBox.TabIndex = 6;
			this.RK_IsActiveCheckBox.UseVisualStyleBackColor = true;
			// 
			// RK_IsSystemCheckBox
			// 
			this.RK_IsSystemCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.RK_IsSystemCheckBox, "RK_IsSystem");
			this.RK_IsSystemCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCityTownForm|483d2916-d345-44ba-8b59-160a03d6d9fb", "Is System");
			this.RK_IsSystemCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RK_IsSystemCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(404, 38, true);
			this.RK_IsSystemCheckBox.Name = "RK_IsSystemCheckBox";
			this.RK_IsSystemCheckBox.ReadOnly = true;
			this.RK_IsSystemCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 17, true);
			this.RK_IsSystemCheckBox.TabIndex = 7;
			this.RK_IsSystemCheckBox.UseVisualStyleBackColor = true;
			this.MainTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.zModuleButtonGrid1.InnerGrid)).EndInit();
			this.zGroupBox1.ResumeLayout(false);
			this.MainTabPage.ResumeLayout(true);
		}

		#endregion

		private Enterprise.ZArchitecture.ZTextBox RK_CityTownPostCodeTextBox;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox RK_RN_NKCountryFindBox;
		private Enterprise.ZArchitecture.ZCalcEdit RK_LattitudeCalcEdit;
		private Enterprise.ZArchitecture.ZCalcEdit RK_LongitudeCalcEdit;
		private Enterprise.ZArchitecture.GUI.ZModuleButtonGrid zModuleButtonGrid1;
		private Enterprise.ZArchitecture.GUI.ZGroupBox zGroupBox1;
		private Enterprise.ZArchitecture.GUI.ZCheckBox RK_IsSystemCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox RK_IsActiveCheckBox;
	}
}
