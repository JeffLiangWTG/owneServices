namespace Enterprise.MasterFiles.GUI
{
	public partial class RefCityTownForm
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
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(731, 386, true);
			this.MainTabControl.TabIndex = 0;
			// 
			// MainTabPage
			// 
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(723, 359, true);
			this.MainTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MainTabPage_InitializeTab));
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(723, 338, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(731, 386, true);
			// 
			// PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel
			// 
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.TabIndex = 0;
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(731, 24, true);
			this.MainStatusBar.TabIndex = 0;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefCityTown);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefCityTown)(null)).R9_InternationalName)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefCityTown)(null)).R9_LocalLanguageName)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefCityTown)(null)).R9_RN_NKCountry)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.RefCityTown)(null)).R9_RW_NKState)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefCityTown)(null)).PostCodes)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.RefCityTown)(null)).Lookups.PostCodes)));
			// 
			// 
			// 
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefCityTown)(null)).R9_IsActive)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.RefCityTown)(null)).R9_IsSystem)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.RefCityTown)(null)).R9_R3_TimeZone)));
			// 
			// RefCityTownForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("fda33c18-3d2f-436e-91cd-9b3c711c7515", "City/Town");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(731, 442, true);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefCityTown);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(575, 272, true);
			this.Name = "RefCityTownForm";
			this.ShouldSerializeTabPageMethods = true;
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		void MainTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.R9_InternationalNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.R9_LocalLanguageNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.R9_RN_NKCountryCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.R9_RW_NKStateCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.zModuleButtonGrid1 = new Enterprise.ZArchitecture.GUI.ZModuleButtonGrid();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.R9_IsActiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.R9_IsSystemCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.TimeZoneGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.MainTabPage.SuspendLayout();
			this.R9_RN_NKCountryCodeFindBox.SuspendLayout();
			this.R9_RW_NKStateCodeFindBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.zModuleButtonGrid1.InnerGrid)).BeginInit();
			this.zModuleButtonGrid1.SuspendLayout();
			this.zGroupBox1.SuspendLayout();
			this.TimeZoneGuidFindBox.SuspendLayout();
			this.MainTabPage.Controls.Add(this.TimeZoneGuidFindBox);
			this.MainTabPage.Controls.Add(this.R9_IsSystemCheckBox);
			this.MainTabPage.Controls.Add(this.R9_IsActiveCheckBox);
			this.MainTabPage.Controls.Add(this.zGroupBox1);
			this.MainTabPage.Controls.Add(this.R9_RW_NKStateCodeFindBox);
			this.MainTabPage.Controls.Add(this.R9_RN_NKCountryCodeFindBox);
			this.MainTabPage.Controls.Add(this.R9_LocalLanguageNameTextBox);
			this.MainTabPage.Controls.Add(this.R9_InternationalNameTextBox);
			// 
			// R9_InternationalNameTextBox
			// 
			this.R9_InternationalNameTextBox.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.R9_InternationalNameTextBox, "R9_InternationalName");
			this.R9_InternationalNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.R9_InternationalNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 15, true);
			this.R9_InternationalNameTextBox.Name = "R9_InternationalNameTextBox";
			this.R9_InternationalNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(288, 20, true);
			this.R9_InternationalNameTextBox.TabIndex = 0;
			// 
			// R9_LocalLanguageNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.R9_LocalLanguageNameTextBox, "R9_LocalLanguageName");
			this.R9_LocalLanguageNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.R9_LocalLanguageNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 42, true);
			this.R9_LocalLanguageNameTextBox.Name = "R9_LocalLanguageNameTextBox";
			this.R9_LocalLanguageNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(288, 20, true);
			this.R9_LocalLanguageNameTextBox.TabIndex = 1;
			// 
			// R9_RN_NKCountryCodeFindBox
			// 
			this.R9_RN_NKCountryCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.R9_RN_NKCountryCodeFindBox, "R9_RN_NKCountry");
			this.R9_RN_NKCountryCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 69, true);
			this.R9_RN_NKCountryCodeFindBox.Name = "R9_RN_NKCountryCodeFindBox";
			this.R9_RN_NKCountryCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(288, 20, true);
			this.R9_RN_NKCountryCodeFindBox.TabIndex = 2;
			// 
			// R9_RW_NKStateCodeFindBox
			// 
			this.R9_RW_NKStateCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.R9_RW_NKStateCodeFindBox, "R9_RW_NKState");
			this.R9_RW_NKStateCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 96, true);
			this.R9_RW_NKStateCodeFindBox.Name = "R9_RW_NKStateCodeFindBox";
			this.R9_RW_NKStateCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(288, 20, true);
			this.R9_RW_NKStateCodeFindBox.TabIndex = 3;
			// 
			// zModuleButtonGrid1
			// 
			this.zModuleButtonGrid1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zModuleButtonGrid1, "PostCodes");
			this.zModuleButtonGrid1.BindToFindBoxList = "Lookups.PostCodes";
			zTextBoxColumnStyleInfo1.ColumnName = "RK_CityTownPostCode";
			zTextBoxColumnStyleInfo1.IsSortable = false;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "RK_Lattitude";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "RK_Longitude";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "RK_RN_NKCountry";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.ColumnName = "RK_IsActive";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCheckBoxColumnStyleInfo2.ColumnName = "RK_IsSystem";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			this.zModuleButtonGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.zModuleButtonGrid1.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.zModuleButtonGrid1.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.zModuleButtonGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.zModuleButtonGrid1.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.zModuleButtonGrid1.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.zModuleButtonGrid1.GridId = "edbce14a-88c7-4329-bf8e-f85bcb8181fd";
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
			this.zModuleButtonGrid1.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(689, 139, true);
			this.zModuleButtonGrid1.InnerGrid.TabIndex = 0;
			this.zModuleButtonGrid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.zModuleButtonGrid1.Name = "zModuleButtonGrid1";
			this.zModuleButtonGrid1.NameOfAGridElement = Enterprise.MasterFiles.GUI.Res.GetData("464c4224-8h98-4ede-a36d-5c36fc27df34", "Post Code");
			this.zModuleButtonGrid1.ReadOnly = true;
			this.zModuleButtonGrid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(695, 177, true);
			this.zModuleButtonGrid1.TabIndex = 4;
			this.zModuleButtonGrid1.Detaching += new Enterprise.ZArchitecture.GUI.ModuleButtonGridOperationCancelEventHandler(this.zModuleButtonGrid1_Detaching);
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("dd0c2b7d-6068-4bcf-8f6f-7169d9fc902b", "Related Postcodes");
			this.zGroupBox1.Controls.Add(this.zModuleButtonGrid1);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 148, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 202, true);
			this.zGroupBox1.TabIndex = 5;
			this.zGroupBox1.TabStop = false;
			// 
			// R9_IsActiveCheckBox
			// 
			this.R9_IsActiveCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.R9_IsActiveCheckBox, "R9_IsActive");
			this.R9_IsActiveCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCityTownForm|981bf110-ff02-4448-9727-a6902f1886a8", "Is Active");
			this.R9_IsActiveCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.R9_IsActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(500, 15, true);
			this.R9_IsActiveCheckBox.Name = "R9_IsActiveCheckBox";
			this.R9_IsActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(68, 17, true);
			this.R9_IsActiveCheckBox.TabIndex = 7;
			this.R9_IsActiveCheckBox.UseVisualStyleBackColor = true;
			// 
			// R9_IsSystemCheckBox
			// 
			this.R9_IsSystemCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.R9_IsSystemCheckBox, "R9_IsSystem");
			this.R9_IsSystemCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("RefCityTownForm|8d1fe293-6628-4729-96ca-c663977de7ac", "Is System");
			this.R9_IsSystemCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.R9_IsSystemCheckBox.ForeColor = System.Drawing.SystemColors.GrayText;
			this.R9_IsSystemCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(500, 38, true);
			this.R9_IsSystemCheckBox.Name = "R9_IsSystemCheckBox";
			this.R9_IsSystemCheckBox.ReadOnly = true;
			this.R9_IsSystemCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 17, true);
			this.R9_IsSystemCheckBox.TabIndex = 8;
			this.R9_IsSystemCheckBox.UseVisualStyleBackColor = true;
			// 
			// TimeZoneGuidFindBox
			// 
			this.TimeZoneGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TimeZoneGuidFindBox, "R9_R3_TimeZone");
			this.TimeZoneGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 122, true);
			this.TimeZoneGuidFindBox.Name = "TimeZoneGuidFindBox";
			this.TimeZoneGuidFindBox.PopupCaption = null;
			this.TimeZoneGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(288, 20, true);
			this.TimeZoneGuidFindBox.TabIndex = 9;
			this.MainTabPage.PerformLayout();
			this.R9_RN_NKCountryCodeFindBox.ResumeLayout(true);
			this.R9_RN_NKCountryCodeFindBox.PerformLayout();
			this.R9_RW_NKStateCodeFindBox.ResumeLayout(true);
			this.R9_RW_NKStateCodeFindBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.zModuleButtonGrid1.InnerGrid)).EndInit();
			this.zModuleButtonGrid1.ResumeLayout(true);
			this.zModuleButtonGrid1.PerformLayout();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.TimeZoneGuidFindBox.ResumeLayout(true);
			this.TimeZoneGuidFindBox.PerformLayout();
			this.MainTabPage.ResumeLayout(true);
		}

		#endregion

		private Enterprise.ZArchitecture.ZTextBox R9_InternationalNameTextBox;
		private Enterprise.ZArchitecture.ZTextBox R9_LocalLanguageNameTextBox;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox R9_RW_NKStateCodeFindBox;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox R9_RN_NKCountryCodeFindBox;
		private Enterprise.ZArchitecture.GUI.ZModuleButtonGrid zModuleButtonGrid1;
		private Enterprise.ZArchitecture.GUI.ZGroupBox zGroupBox1;
		private Enterprise.ZArchitecture.GUI.ZCheckBox R9_IsSystemCheckBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox R9_IsActiveCheckBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox TimeZoneGuidFindBox;
	}
}
