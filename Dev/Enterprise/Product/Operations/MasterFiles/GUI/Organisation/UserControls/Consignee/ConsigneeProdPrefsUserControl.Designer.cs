using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.GUI
{
	partial class ConsigneeProdPrefsUserControl
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
			this.PartAttributeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PartAttributeType3GuidFindBox = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PartAttributeName3TextBox = new Enterprise.ZArchitecture.ZTranslatableTextControl();
			this.PartAttributeType2GuidFindBox = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PartAttributeName2TextBox = new Enterprise.ZArchitecture.ZTranslatableTextControl();
			this.PartAttributeType1GuidFindBox = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PartAttributeName1TextBox = new Enterprise.ZArchitecture.ZTranslatableTextControl();
			this.ScanPackingGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.IsAutoPackAllowedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.AutoPrintLabelOnPackageCloseCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ScanQtyModeCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.MinimumShelfLifeCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PickingGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PartAttributeGroupBox.SuspendLayout();
			this.ScanPackingGroupBox.SuspendLayout();
			this.PickingGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			// 
			// PartAttributeGroupBox
			// 
			this.PartAttributeGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConsigneeProdPrefsUserControl|39ae053b-e9ee-420f-9db3-75c87b040efc", "Part Attribute Rules");
			this.PartAttributeGroupBox.Controls.Add(this.PartAttributeType3GuidFindBox);
			this.PartAttributeGroupBox.Controls.Add(this.PartAttributeName3TextBox);
			this.PartAttributeGroupBox.Controls.Add(this.PartAttributeType2GuidFindBox);
			this.PartAttributeGroupBox.Controls.Add(this.PartAttributeName2TextBox);
			this.PartAttributeGroupBox.Controls.Add(this.PartAttributeType1GuidFindBox);
			this.PartAttributeGroupBox.Controls.Add(this.PartAttributeName1TextBox);
			this.PartAttributeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.PartAttributeGroupBox.Name = "PartAttributeGroupBox";
			this.PartAttributeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(663, 112, true);
			this.PartAttributeGroupBox.TabIndex = 1;
			this.PartAttributeGroupBox.TabStop = false;
			// 
			// PartAttributeType3GuidFindBox
			// 
			this.PartAttributeType3GuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PartAttributeType3GuidFindBox, "MiscServ+OM_IMPartAttrib3Type");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_IMPartAttrib3Type)));
			this.PartAttributeType3GuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(415, 76, true);
			this.PartAttributeType3GuidFindBox.Name = "PartAttributeType3GuidFindBox";
			this.PartAttributeType3GuidFindBox.PreBoundMaxLength = 3;
			this.PartAttributeType3GuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(229, 20, true);
			this.PartAttributeType3GuidFindBox.TabIndex = 11;
			// 
			// PartAttributeName3TextBox
			// 
			this.BindingSource.SetBindingMember(this.PartAttributeName3TextBox, "MiscServ+OM_IMPartAttrib3Name");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_IMPartAttrib3Name)));
			this.PartAttributeName3TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PartAttributeName3TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 76, true);
			this.PartAttributeName3TextBox.Name = "PartAttributeName3TextBox";
			this.PartAttributeName3TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(209, 20, true);
			this.PartAttributeName3TextBox.TabIndex = 9;
			// 
			// PartAttributeType2GuidFindBox
			// 
			this.PartAttributeType2GuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PartAttributeType2GuidFindBox, "MiscServ+OM_IMPartAttrib2Type");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_IMPartAttrib2Type)));
			this.PartAttributeType2GuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(415, 50, true);
			this.PartAttributeType2GuidFindBox.Name = "PartAttributeType2GuidFindBox";
			this.PartAttributeType2GuidFindBox.PreBoundMaxLength = 3;
			this.PartAttributeType2GuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(229, 20, true);
			this.PartAttributeType2GuidFindBox.TabIndex = 7;
			// 
			// PartAttributeName2TextBox
			// 
			this.BindingSource.SetBindingMember(this.PartAttributeName2TextBox, "MiscServ+OM_IMPartAttrib2Name");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_IMPartAttrib2Name)));
			this.PartAttributeName2TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PartAttributeName2TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 50, true);
			this.PartAttributeName2TextBox.Name = "PartAttributeName2TextBox";
			this.PartAttributeName2TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(209, 20, true);
			this.PartAttributeName2TextBox.TabIndex = 5;
			// 
			// PartAttributeType1GuidFindBox
			// 
			this.PartAttributeType1GuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PartAttributeType1GuidFindBox, "MiscServ+OM_IMPartAttrib1Type");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_IMPartAttrib1Type)));
			this.PartAttributeType1GuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(415, 24, true);
			this.PartAttributeType1GuidFindBox.Name = "PartAttributeType1GuidFindBox";
			this.PartAttributeType1GuidFindBox.PreBoundMaxLength = 3;
			this.PartAttributeType1GuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(229, 20, true);
			this.PartAttributeType1GuidFindBox.TabIndex = 3;
			// 
			// PartAttributeName1TextBox
			// 
			this.BindingSource.SetBindingMember(this.PartAttributeName1TextBox, "MiscServ+OM_IMPartAttrib1Name");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_IMPartAttrib1Name)));
			this.PartAttributeName1TextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PartAttributeName1TextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 24, true);
			this.PartAttributeName1TextBox.Name = "PartAttributeName1TextBox";
			this.PartAttributeName1TextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(209, 20, true);
			this.PartAttributeName1TextBox.TabIndex = 1;
			// 
			// ScanPackingGroupBox
			// 
			this.ScanPackingGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("b0fe3070-01ec-4d17-86fb-9537e898f3e2", "Scan Packing");
			this.ScanPackingGroupBox.Controls.Add(this.IsAutoPackAllowedCheckBox);
			this.ScanPackingGroupBox.Controls.Add(this.AutoPrintLabelOnPackageCloseCheckBox);
			this.ScanPackingGroupBox.Controls.Add(this.ScanQtyModeCheckBox);
			this.ScanPackingGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 126, true);
			this.ScanPackingGroupBox.Name = "ScanPackingGroupBox";
			this.ScanPackingGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(227, 101, true);
			this.ScanPackingGroupBox.TabIndex = 2;
			this.ScanPackingGroupBox.TabStop = false;
			// 
			// IsAutoPackAllowedCheckBox
			// 
			this.IsAutoPackAllowedCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsAutoPackAllowedCheckBox, "MiscServ.OM_IsAutoPackAllowed");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_IsAutoPackAllowed)));
			this.IsAutoPackAllowedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsAutoPackAllowedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(19, 47, true);
			this.IsAutoPackAllowedCheckBox.Name = "IsAutoPackAllowedCheckBox";
			this.IsAutoPackAllowedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 17, true);
			this.IsAutoPackAllowedCheckBox.TabIndex = 1;
			this.IsAutoPackAllowedCheckBox.UseVisualStyleBackColor = true;
			// 
			// AutoPrintLabelOnPackageCloseCheckBox
			// 
			this.AutoPrintLabelOnPackageCloseCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.AutoPrintLabelOnPackageCloseCheckBox, "MiscServ.OM_IsLabelPrintedOnClosePackage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_IsLabelPrintedOnClosePackage)));
			this.AutoPrintLabelOnPackageCloseCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("d4e50e38-f3d0-432e-8c5f-7f7d08e2bdbb", "Auto-Print Label on Close Package");
			this.AutoPrintLabelOnPackageCloseCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AutoPrintLabelOnPackageCloseCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(19, 70, true);
			this.AutoPrintLabelOnPackageCloseCheckBox.Name = "AutoPrintLabelOnPackageCloseCheckBox";
			this.AutoPrintLabelOnPackageCloseCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(191, 17, true);
			this.AutoPrintLabelOnPackageCloseCheckBox.TabIndex = 2;
			this.AutoPrintLabelOnPackageCloseCheckBox.UseVisualStyleBackColor = true;
			// 
			// ScanQtyModeCheckBox
			// 
			this.ScanQtyModeCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ScanQtyModeCheckBox, "MiscServ.OM_IsScanPackQtyAllowed");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_IsScanPackQtyAllowed)));
			this.ScanQtyModeCheckBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("80271e0b-0f9f-43b1-b32c-a598db720a5c", "Enable Scan Qty Mode");
			this.ScanQtyModeCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ScanQtyModeCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(19, 24, true);
			this.ScanQtyModeCheckBox.Name = "ScanQtyModeCheckBox";
			this.ScanQtyModeCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 17, true);
			this.ScanQtyModeCheckBox.TabIndex = 0;
			this.ScanQtyModeCheckBox.UseVisualStyleBackColor = true;
			// 
			// MinimumShelfLifeCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.MinimumShelfLifeCalcEdit, "MiscServ.OM_MinimumShelfLifeAccepted");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).MiscServ.OM_MinimumShelfLifeAccepted)));
			this.MinimumShelfLifeCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(193, 44, true);
			this.MinimumShelfLifeCalcEdit.DecimalPlaces = 2;
			this.MinimumShelfLifeCalcEdit.Name = "MinimumShelfLifeCalcEdit";
			this.MinimumShelfLifeCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.MinimumShelfLifeCalcEdit.TabIndex = 3;
			this.MinimumShelfLifeCalcEdit.Text = "0";
			this.MinimumShelfLifeCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PickingGroupBox
			// 
			this.PickingGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("a23cb01a-6f25-4044-b65e-a816fd803170", "Picking");
			this.PickingGroupBox.Controls.Add(this.MinimumShelfLifeCalcEdit);
			this.PickingGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(241, 126, true);
			this.PickingGroupBox.Name = "PickingGroupBox";
			this.PickingGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(430, 101, true);
			this.PickingGroupBox.TabIndex = 4;
			this.PickingGroupBox.TabStop = false;
			// 
			// ConsigneeProdPrefsUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PickingGroupBox);
			this.Controls.Add(this.ScanPackingGroupBox);
			this.Controls.Add(this.PartAttributeGroupBox);
			this.Name = "ConsigneeProdPrefsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(566, 236, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PartAttributeGroupBox.ResumeLayout(false);
			this.PartAttributeGroupBox.PerformLayout();
			this.ScanPackingGroupBox.ResumeLayout(false);
			this.ScanPackingGroupBox.PerformLayout();
			this.PickingGroupBox.ResumeLayout(false);
			this.PickingGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGroupBox PartAttributeGroupBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit PartAttributeType3GuidFindBox;
		private Enterprise.ZArchitecture.ZTranslatableTextControl PartAttributeName3TextBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit PartAttributeType2GuidFindBox;
		private Enterprise.ZArchitecture.ZTranslatableTextControl PartAttributeName2TextBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit PartAttributeType1GuidFindBox;
		private Enterprise.ZArchitecture.ZTranslatableTextControl PartAttributeName1TextBox;
		private ZArchitecture.GUI.ZGroupBox ScanPackingGroupBox;
		private ZArchitecture.GUI.ZCheckBox ScanQtyModeCheckBox;
		private ZArchitecture.GUI.ZCheckBox AutoPrintLabelOnPackageCloseCheckBox;
		private ZArchitecture.ZCalcEdit MinimumShelfLifeCalcEdit;
		private ZArchitecture.GUI.ZCheckBox IsAutoPackAllowedCheckBox;
		private ZArchitecture.GUI.ZGroupBox PickingGroupBox;

	}
}
