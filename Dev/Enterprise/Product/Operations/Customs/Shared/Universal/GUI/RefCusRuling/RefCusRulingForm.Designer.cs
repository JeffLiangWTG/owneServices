using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.Universal.GUI
{
	public partial class RefCusRulingForm : ZTemplateForm
	{
		ZGroupBox BasicGroupBox;
		ZDropEdit ZZX_RulingTypeDropEdit;
		ZTextBox ZZX_RulingNumberTextBox;
		ZTextBox ZZX_DescriptionTextBox;
		ZAddressControl ZZX_OA_AppliesToAddressControl;
		ZDateEdit ZZX_EndDateDateEdit;
		ZDateEdit ZZX_StartDateDateEdit;
		ZCheckBox IsSystemCheckBox;
		ZGroupBox ConfigurationsGroupBox;
		ZGrid ConfigurationsGrid;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.BasicGroupBox = new ZGroupBox();
			this.ZZX_RulingTypeDropEdit = new ZDropEdit();
			this.ZZX_RulingNumberTextBox = new ZTextBox();
			this.ZZX_DescriptionTextBox = new ZTextBox();
			this.ZZX_OA_AppliesToAddressControl = new ZAddressControl();
			this.ZZX_EndDateDateEdit = new ZDateEdit();
			this.ZZX_StartDateDateEdit = new ZDateEdit();
			this.IsSystemCheckBox = new ZCheckBox();
			this.ConfigurationsGroupBox = new ZGroupBox();
			this.ConfigurationsGrid = new ZGrid();
			ZDropEditColumnStyleInfo zCategoryDropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zTypeDropEditColumnStyleInfo = new ZDropEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo = new ZMultiControlColumnStyleInfo();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ConfigurationsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ConfigurationsGrid)).BeginInit();
			this.ConfigurationsGrid.SuspendLayout();
			this.BasicGroupBox.SuspendLayout();
			this.ZZX_OA_AppliesToAddressControl.SuspendLayout();
			this.ZZX_EndDateDateEdit.SuspendLayout();
			this.ZZX_StartDateDateEdit.SuspendLayout();
			this.ZZX_DescriptionTextBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(845, 299, true);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.ConfigurationsGroupBox);
			this.MainTabPage.Controls.Add(this.BasicGroupBox);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(826, 272, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(826, 272, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(837, 272, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(845, 299, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(845, 24, true);
			// 
			// BasicGroupBox
			// 
			this.BasicGroupBox.CaptionResourceString = Res.GetData("44A98D0A-77BC-4A86-BFC7-54AEF6E4B26E", "Basic Information");
			this.BasicGroupBox.Controls.Add(this.ZZX_RulingTypeDropEdit);
			this.BasicGroupBox.Controls.Add(this.ZZX_RulingNumberTextBox);
			this.BasicGroupBox.Controls.Add(this.ZZX_DescriptionTextBox);
			this.BasicGroupBox.Controls.Add(this.ZZX_OA_AppliesToAddressControl);
			this.BasicGroupBox.Controls.Add(this.ZZX_StartDateDateEdit);
			this.BasicGroupBox.Controls.Add(this.ZZX_EndDateDateEdit);
			this.BasicGroupBox.Controls.Add(this.IsSystemCheckBox);
			this.BasicGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
			this.BasicGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BasicGroupBox.Name = "BasicGroupBox";
			this.BasicGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(410, 272, true);
			this.BasicGroupBox.TabIndex = 2;
			this.BasicGroupBox.TabStop = false;
			// 
			// ZZX_RulingNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.ZZX_RulingNumberTextBox, "ZZX_RulingNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ZZRefCusRulingCombined)(null)).ZZX_RulingNumber)));
			this.ZZX_RulingNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 23, true);
			this.ZZX_RulingNumberTextBox.Name = "ZZX_RulingNumberTextBox";
			this.ZZX_RulingNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(143, 20, true);
			this.ZZX_RulingNumberTextBox.TabIndex = 0;
			// 
			// ZZX_RulingTypeDropEdit
			// 
			this.ZZX_RulingTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ZZX_RulingTypeDropEdit, "ZZX_RulingType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((ZZRefCusRulingCombined)(null)).ZZX_RulingType)));
			this.ZZX_RulingTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 52, true);
			this.ZZX_RulingTypeDropEdit.Name = "ZZX_RulingTypeDropEdit";
			this.ZZX_RulingTypeDropEdit.PreBoundMaxLength = 5;
			this.ZZX_RulingTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(285, 20, true);
			this.ZZX_RulingTypeDropEdit.TabIndex = 1;
			// 
			// ZZX_DescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.ZZX_DescriptionTextBox, "ZZX_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ZZRefCusRulingCombined)(null)).ZZX_Description)));
			this.ZZX_DescriptionTextBox.CaptionResourceString = Res.GetData("ED9A967D-F0FE-4AA0-9AEF-4F5FBC81F1D5", "Description");
			this.ZZX_DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 81, true);
			this.ZZX_DescriptionTextBox.Multiline = true;
			this.ZZX_DescriptionTextBox.Name = "ZZX_DescriptionTextBox";
			this.ZZX_DescriptionTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.ZZX_DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(285, 49, true);
			this.ZZX_DescriptionTextBox.TabIndex = 2;
			// 
			// ZZX_OA_AppliesToAddressControl
			// 
			this.ZZX_OA_AppliesToAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ZZX_OA_AppliesToAddressControl, "ZZX_OA_AppliesTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((ZZRefCusRulingCombined)(null)).ZZX_OA_AppliesTo)));
			this.ZZX_OA_AppliesToAddressControl.BindToOrgList = "Lookups+AppliesToOrganizationList";
			this.ZZX_OA_AppliesToAddressControl.CaptionResourceString = Enterprise.Customs.Universal.GUI.Res.GetData("74EA8890-0AEB-466C-8E1A-BD438C891DAD", "Applies To");
			this.ZZX_OA_AppliesToAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 139, true);
			this.ZZX_OA_AppliesToAddressControl.Name = "ZZX_OA_AppliesToAddressControl";
			this.ZZX_OA_AppliesToAddressControl.PopupCaption = "Applies To";
			this.ZZX_OA_AppliesToAddressControl.ReadOnly = false;
			this.ZZX_OA_AppliesToAddressControl.ShowAddress = false;
			this.ZZX_OA_AppliesToAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(285, 20, true);
			this.ZZX_OA_AppliesToAddressControl.TabIndex = 3;
			// 
			// ZZD_StartDateDateEdit
			// 
			this.ZZX_StartDateDateEdit.AllowDrop = true;
			this.ZZX_StartDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.ZZX_StartDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ZZX_StartDateDateEdit, "ZZX_StartDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((ZZRefCusRulingCombined)(null)).ZZX_StartDate)));
			this.ZZX_StartDateDateEdit.CaptionResourceString = Res.GetData("96D10690-84F2-418A-90B5-60E155BBE387", "Start Date");
			this.ZZX_StartDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 168, true);
			this.ZZX_StartDateDateEdit.Name = "ZZX_StartDateDateEdit";
			this.ZZX_StartDateDateEdit.TabIndex = 4;
			// 
			// ZZX_EndDateDateEdit
			// 
			this.ZZX_EndDateDateEdit.AllowDrop = true;
			this.ZZX_EndDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.ZZX_EndDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ZZX_EndDateDateEdit, "ZZX_EndDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((ZZRefCusRulingCombined)(null)).ZZX_EndDate)));
			this.ZZX_EndDateDateEdit.CaptionResourceString = Res.GetData("21D5833F-A051-45C7-B8D2-5158A52739BF", "End Date");
			this.ZZX_EndDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 198, true);
			this.ZZX_EndDateDateEdit.Name = "ZZX_EndDateDateEdit";
			this.ZZX_EndDateDateEdit.TabIndex = 5;
			// 
			// IsSystemCheckBox
			// 
			this.IsSystemCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsSystemCheckBox, "IsSystem");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((ZZRefCusRulingCombined)(null)).IsSystem)));
			this.IsSystemCheckBox.CaptionResourceString = Res.GetData("CD53AD81-5D1C-480C-9555-4FD3469F9E70", "Is System");
			this.IsSystemCheckBox.CheckAlign = ZContentAlignment.Right;
			this.IsSystemCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsSystemCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 226, true);
			this.IsSystemCheckBox.Name = "IsSystemCheckBox";
			this.IsSystemCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.IsSystemCheckBox.TabIndex = 6;
			this.IsSystemCheckBox.UseVisualStyleBackColor = true;
			// 
			// ConfigurationsGroupBox
			// 
			this.ConfigurationsGroupBox.CaptionResourceString = Res.GetData("CF471C79-833A-407B-9BF7-7C25905AFA43", "Configurations");
			this.ConfigurationsGroupBox.Controls.Add(this.ConfigurationsGrid);
			this.ConfigurationsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConfigurationsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(410, 0, true);
			this.ConfigurationsGroupBox.Name = "ConfigurationsGroupBox";
			this.ConfigurationsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(416, 272, true);
			this.ConfigurationsGroupBox.TabIndex = 3;
			this.ConfigurationsGroupBox.TabStop = false;
			// 
			// ConfigurationsGrid
			// 
			this.ConfigurationsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.ConfigurationsGrid, "Configurations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((ZZRefCusRulingCombined)(null)).Configurations)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CusRulingConfigCombined)(((System.Collections.IList)(((ZZRefCusRulingCombined)(null)).Configurations)).SyncRoot)).ZZY_Category)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CusRulingConfigCombined)(((System.Collections.IList)(((ZZRefCusRulingCombined)(null)).Configurations)).SyncRoot)).ZZY_Type)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((CusRulingConfigCombined)(((System.Collections.IList)(((ZZRefCusRulingCombined)(null)).Configurations)).SyncRoot)).ZZY_Rate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CusRulingConfigCombined)(((System.Collections.IList)(((ZZRefCusRulingCombined)(null)).Configurations)).SyncRoot)).ZZY_Value)));
			this.ConfigurationsGrid.CaptionVisible = false;
			zCategoryDropEditColumnStyleInfo.CaptionResourceString = Enterprise.Customs.Universal.GUI.Res.GetData("BF020893-4B82-4889-BD62-5D58E303C1EA", "Category");
			zCategoryDropEditColumnStyleInfo.ColumnName = "ZZY_Category";
			zCategoryDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(93);
			zTypeDropEditColumnStyleInfo.CaptionResourceString = Enterprise.Customs.Universal.GUI.Res.GetData("81878944-384D-4633-85C8-4FFEBDB5A921", "Type");
			zTypeDropEditColumnStyleInfo.ColumnName = "ZZY_Type";
			zTypeDropEditColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(93);
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.Universal.GUI.Res.GetData("D073E03B-C342-4F09-92B8-BFA016FC9926", "Rate(%)");
			zCalcEditColumnStyleInfo1.ColumnName = "ZZY_Rate";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zMultiControlColumnStyleInfo.CaptionResourceString = Enterprise.Customs.Universal.GUI.Res.GetData("056C24F8-F378-44A6-B277-52EC14B7CEE6", "Value");
			zMultiControlColumnStyleInfo.ColumnName = "ZZY_Value";
			zMultiControlColumnStyleInfo.FieldTypeColumnName = "ValueFieldType";
			zMultiControlColumnStyleInfo.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zMultiControlColumnStyleInfo.BindToDecimalPlaces = "ZZY_ValueDecimalPlaces";
			this.ConfigurationsGrid.ColumnStyles.Add(zCategoryDropEditColumnStyleInfo);
			this.ConfigurationsGrid.ColumnStyles.Add(zTypeDropEditColumnStyleInfo);
			this.ConfigurationsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.ConfigurationsGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo);
			this.ConfigurationsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConfigurationsGrid.GridId = "9A4DBB59-ED04-4905-A737-B3DB8D34939B";
			this.ConfigurationsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ConfigurationsGrid.LayoutKey = "ConfigurationsGrid";
			this.ConfigurationsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ConfigurationsGrid.Name = "ConfigurationsGrid";
			this.ConfigurationsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(410, 184, true);
			this.ConfigurationsGrid.TabIndex = 0;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(ZZRefCusRulingCombined);
			// 
			// RefCusRulingForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Res.GetData("217E5DF5-BB74-4C89-96FA-3454DF384055", "Ruling");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(845, 355, true);
			this.DataSourceAssemblyName = "Enterprise.ZArchitecture.Business";
			this.DataSourceTypeName = "Enterprise.ZArchitecture.Business.Design.DataSourceTypeRequiredInstructionsType";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "RefCusRulingForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "RefCusRulingForm";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(false);
			this.NotesTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ConfigurationsGroupBox.ResumeLayout(false);
			this.ConfigurationsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ConfigurationsGrid)).EndInit();
			this.ConfigurationsGrid.ResumeLayout(false);
			this.ConfigurationsGrid.PerformLayout();
			this.BasicGroupBox.ResumeLayout(false);
			this.BasicGroupBox.PerformLayout();
			this.ZZX_RulingTypeDropEdit.ResumeLayout(true);
			this.ZZX_RulingTypeDropEdit.PerformLayout();
			this.ZZX_OA_AppliesToAddressControl.ResumeLayout(true);
			this.ZZX_OA_AppliesToAddressControl.PerformLayout();
			this.ZZX_StartDateDateEdit.ResumeLayout(true);
			this.ZZX_StartDateDateEdit.PerformLayout();
			this.ZZX_EndDateDateEdit.ResumeLayout(true);
			this.ZZX_EndDateDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
	}
}
