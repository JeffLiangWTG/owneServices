using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Enterprise.MasterData.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterData.GUI
{
	public partial class GenShapeGeographyForm
	{
		System.ComponentModel.IContainer components = null;
		ZPanel zPanel1;
		ZArchitecture.ZTextBox SHG_NameTextBox;
		ZArchitecture.ZTextBox SHG_DescriptionTextBox;
		ZCheckBox SHG_IsSystemCheckBox;
		ZDropEdit SHG_ParentTableCodeDropEdit;
		ZGuidFindBox SHG_ParentIDGuidFindBox;
		ZArchitecture.ZTextBox SHG_ShapeInformationTextBox;
		ZButton SHG_ShapeImportButton;
		ZDropEdit SHG_TypeDropEdit;
		ZCheckBox SHG_IsActiveCheckBox;

		new void InitializeComponent()
		{
			this.zPanel1 = new ZPanel();
			this.SHG_TypeDropEdit = new ZDropEdit();
			this.SHG_ShapeImportButton = new ZButton();
			this.SHG_ShapeInformationTextBox = new ZArchitecture.ZTextBox();
			this.SHG_ParentIDGuidFindBox = new ZGuidFindBox();
			this.SHG_ParentTableCodeDropEdit = new ZDropEdit();
			this.SHG_IsSystemCheckBox = new ZCheckBox();
			this.SHG_IsActiveCheckBox = new ZCheckBox();
			this.SHG_DescriptionTextBox = new ZArchitecture.ZTextBox();
			this.SHG_NameTextBox = new ZArchitecture.ZTextBox();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zPanel1.SuspendLayout();
			this.SHG_TypeDropEdit.SuspendLayout();
			this.SHG_ParentIDGuidFindBox.SuspendLayout();
			this.SHG_ParentTableCodeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(747, 296, true);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.zPanel1);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(739, 269, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(739, 269, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(739, 269, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(747, 296, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(747, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(GenShapeGeography);
			// 
			// zPanel1
			// 
			this.zPanel1.Controls.Add(this.SHG_TypeDropEdit);
			this.zPanel1.Controls.Add(this.SHG_ShapeImportButton);
			this.zPanel1.Controls.Add(this.SHG_ShapeInformationTextBox);
			this.zPanel1.Controls.Add(this.SHG_ParentIDGuidFindBox);
			this.zPanel1.Controls.Add(this.SHG_ParentTableCodeDropEdit);
			this.zPanel1.Controls.Add(this.SHG_IsSystemCheckBox);
			this.zPanel1.Controls.Add(this.SHG_IsActiveCheckBox);
			this.zPanel1.Controls.Add(this.SHG_DescriptionTextBox);
			this.zPanel1.Controls.Add(this.SHG_NameTextBox);
			this.zPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(739, 269, true);
			this.zPanel1.TabIndex = 0;
			// 
			// SHG_TypeDropEdit
			// 
			this.SHG_TypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SHG_TypeDropEdit, "SHG_Type");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((GenShapeGeography)(null)).SHG_Type)));
			this.SHG_TypeDropEdit.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("a2ba4e0e-0ada-41de-aabc-722f117594fa", "Geography Type");
			this.SHG_TypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 47, true);
			this.SHG_TypeDropEdit.Name = "SHG_TypeDropEdit";
			this.SHG_TypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 20, true);
			this.SHG_TypeDropEdit.TabIndex = 2;
			// 
			// SHG_ShapeImportButton
			// 
			this.SHG_ShapeImportButton.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("4f581830-118c-443b-9085-2e25e058cfd4", "Import");
			this.SHG_ShapeImportButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(337, 136, true);
			this.SHG_ShapeImportButton.Name = "SHG_ShapeImportButton";
			this.SHG_ShapeImportButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SHG_ShapeImportButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 21, true);
			this.SHG_ShapeImportButton.TabIndex = 7;
			this.SHG_ShapeImportButton.ToolTipCaption = null;
			this.SHG_ShapeImportButton.UseVisualStyleBackColor = true;
			this.SHG_ShapeImportButton.Click += new System.EventHandler(this.SHG_ShapeImportButton_Click);
			// 
			// SHG_ShapeInformationTextBox
			// 
			this.BindingSource.SetBindingMember(this.SHG_ShapeInformationTextBox, "ShapeInformation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((GenShapeGeography)(null)).ShapeInformation)));
			this.SHG_ShapeInformationTextBox.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("5c9b492c-6d11-4f74-ac33-d39a48e33b37", "Shape Data");
			this.SHG_ShapeInformationTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SHG_ShapeInformationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 137, true);
			this.SHG_ShapeInformationTextBox.Name = "SHG_ShapeInformationTextBox";
			this.SHG_ShapeInformationTextBox.ReadOnly = true;
			this.SHG_ShapeInformationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 20, true);
			this.SHG_ShapeInformationTextBox.TabIndex = 6;
			// 
			// SHG_ParentIDGuidFindBox
			// 
			this.SHG_ParentIDGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SHG_ParentIDGuidFindBox, "SHG_ParentID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((GenShapeGeography)(null)).SHG_ParentID)));
			this.SHG_ParentIDGuidFindBox.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("ee1e7736-28c0-43cb-beea-00cb552a9b63", "Related Record");
			this.SHG_ParentIDGuidFindBox.IsPrimaryKeyFromCodeRequired = false;
			this.SHG_ParentIDGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 107, true);
			this.SHG_ParentIDGuidFindBox.Name = "SHG_ParentIDGuidFindBox";
			this.SHG_ParentIDGuidFindBox.ShouldResize = true;
			this.SHG_ParentIDGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 20, true);
			this.SHG_ParentIDGuidFindBox.TabIndex = 5;
			// 
			// SHG_ParentTableCodeDropEdit
			// 
			this.SHG_ParentTableCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SHG_ParentTableCodeDropEdit, "SHG_ParentTableCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((GenShapeGeography)(null)).SHG_ParentTableCode)));
			this.SHG_ParentTableCodeDropEdit.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("534919b8-2253-4cb9-91b9-1fff7ca9f0b9", "Related Record Type");
			this.SHG_ParentTableCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 77, true);
			this.SHG_ParentTableCodeDropEdit.Name = "SHG_ParentTableCodeDropEdit";
			this.SHG_ParentTableCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 20, true);
			this.SHG_ParentTableCodeDropEdit.TabIndex = 4;
			// 
			// SHG_IsSystemCheckBox
			// 
			this.BindingSource.SetBindingMember(this.SHG_IsSystemCheckBox, "SHG_IsSystem");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((GenShapeGeography)(null)).SHG_IsSystem)));
			this.SHG_IsSystemCheckBox.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("GenShapeGeographyForm|367d862b-b695-4d86-b8f7-65ac17c465c5", "Is System");
			this.SHG_IsSystemCheckBox.Enabled = false;
			this.SHG_IsSystemCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SHG_IsSystemCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(571, 43, true);
			this.SHG_IsSystemCheckBox.Name = "SHG_IsSystemCheckBox";
			this.SHG_IsSystemCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(81, 24, true);
			this.SHG_IsSystemCheckBox.TabIndex = 3;
			this.SHG_IsSystemCheckBox.UseVisualStyleBackColor = true;
			// 
			// SHG_IsActiveCheckBox
			// 
			this.BindingSource.SetBindingMember(this.SHG_IsActiveCheckBox, "SHG_IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((GenShapeGeography)(null)).SHG_IsActive)));
			this.SHG_IsActiveCheckBox.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("GenShapeGeographyForm|88c3f1f1-b94f-427a-a79f-f5f5a1f0cc8d", "Is Active");
			this.SHG_IsActiveCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SHG_IsActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(571, 17, true);
			this.SHG_IsActiveCheckBox.Name = "SHG_IsActiveCheckBox";
			this.SHG_IsActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(81, 20, true);
			this.SHG_IsActiveCheckBox.TabIndex = 1;
			this.SHG_IsActiveCheckBox.UseVisualStyleBackColor = true;
			// 
			// SHG_DescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.SHG_DescriptionTextBox, "SHG_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((GenShapeGeography)(null)).SHG_Description)));
			this.SHG_DescriptionTextBox.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("GenShapeGeographyForm|1cae7f59-ec46-4942-8b41-4854232d3028", "Description");
			this.SHG_DescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SHG_DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 167, true);
			this.SHG_DescriptionTextBox.Multiline = true;
			this.SHG_DescriptionTextBox.Name = "SHG_DescriptionTextBox";
			this.SHG_DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(423, 98, true);
			this.SHG_DescriptionTextBox.TabIndex = 8;
			// 
			// SHG_NameTextBox
			// 
			this.BindingSource.SetBindingMember(this.SHG_NameTextBox, "SHG_Name");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((GenShapeGeography)(null)).SHG_Name)));
			this.SHG_NameTextBox.CaptionResourceString = Enterprise.MasterData.GUI.Res.GetData("GenShapeGeographyForm|8cbfb408-66f0-4435-a036-fbf2e4e8a4b0", "Name");
			this.SHG_NameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SHG_NameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 17, true);
			this.SHG_NameTextBox.Name = "SHG_NameTextBox";
			this.SHG_NameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(423, 20, true);
			this.SHG_NameTextBox.TabIndex = 0;
			// 
			// GenShapeGeographyForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(747, 352, true);
			this.DataSourceType = typeof(GenShapeGeography);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "GenShapeGeographyForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "";
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
			this.zPanel1.ResumeLayout(false);
			this.zPanel1.PerformLayout();
			this.SHG_TypeDropEdit.ResumeLayout(true);
			this.SHG_TypeDropEdit.PerformLayout();
			this.SHG_ParentIDGuidFindBox.ResumeLayout(true);
			this.SHG_ParentIDGuidFindBox.PerformLayout();
			this.SHG_ParentTableCodeDropEdit.ResumeLayout(true);
			this.SHG_ParentTableCodeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
