using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	partial class BaseClassificationUserControl
	{
		protected Enterprise.ZArchitecture.GUI.ZGroupBox BaseClassificationGroupBox;
		protected Enterprise.ZArchitecture.ZTextBox LookupCodeTextBox;
		protected Enterprise.ZArchitecture.GUI.ZCheckBox CC_IsActiveCheckBox;
		protected Enterprise.ZArchitecture.ZTextBox DescriptionTextBox;
		protected ZDateEdit LastAuditDateEdit;
		protected ZCodeFindBox AuditStaffCodeFindBox;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
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
			this.BaseClassificationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AuditStaffCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.LastAuditDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.CC_IsActiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.DescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LookupCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BaseClassificationGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseCusClassification);
			// 
			// BaseClassificationGroupBox
			// 
			this.BaseClassificationGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseClassificationUserControl|085b1ac2-b95d-4ad0-a470-69754aa90d82", "Classification Lookup");
			this.BaseClassificationGroupBox.Controls.Add(this.AuditStaffCodeFindBox);
			this.BaseClassificationGroupBox.Controls.Add(this.LastAuditDateEdit);
			this.BaseClassificationGroupBox.Controls.Add(this.CC_IsActiveCheckBox);
			this.BaseClassificationGroupBox.Controls.Add(this.DescriptionTextBox);
			this.BaseClassificationGroupBox.Controls.Add(this.LookupCodeTextBox);
			this.BaseClassificationGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.BaseClassificationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.BaseClassificationGroupBox.Name = "BaseClassificationGroupBox";
			this.BaseClassificationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(535, 155, true);
			this.BaseClassificationGroupBox.TabIndex = 0;
			this.BaseClassificationGroupBox.TabStop = false;
			// 
			// AuditStaffCodeFindBox
			// 
			this.BindingSource.SetBindingMember(this.AuditStaffCodeFindBox, "CC_LastAuditedUser");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusClassification)(null)).CC_LastAuditedUser)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.BaseCusClassification)(null)).Lookups.Staffs)));
			this.AuditStaffCodeFindBox.BindToList = "Lookups+Staffs";
			this.AuditStaffCodeFindBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseClassificationUserControl|48d4f7db-0050-4f42-9539-6305a9c53f26", "Last Audited By");
			this.AuditStaffCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 122, true);
			this.AuditStaffCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbStaff;
			this.AuditStaffCodeFindBox.Name = "AuditStaffCodeFindBox";
			this.AuditStaffCodeFindBox.PreBoundMaxLength = 3;
			this.AuditStaffCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(149, 20, true);
			this.AuditStaffCodeFindBox.TabIndex = 3;
			// 
			// LastAuditDateEdit
			// 
			this.LastAuditDateEdit.AutoCompleteMonthThreshold = 1;
			this.LastAuditDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.LastAuditDateEdit, "CC_LastAuditedDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseCusClassification)(null)).CC_LastAuditedDate)));
			this.LastAuditDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(369, 122, true);
			this.LastAuditDateEdit.Name = "LastAuditDateEdit";
			this.LastAuditDateEdit.TabIndex = 4;
			// 
			// CC_IsActiveCheckBox
			// 
			this.CC_IsActiveCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.CC_IsActiveCheckBox, "CC_IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.Business.BaseCusClassification)(null)).CC_IsActive)));
			this.CC_IsActiveCheckBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("BaseClassificationUserControl|4cfdcd75-41fe-472c-8806-46212bad5ac5", "Active");
			this.CC_IsActiveCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CC_IsActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(462, 124, true);
			this.CC_IsActiveCheckBox.Name = "CC_IsActiveCheckBox";
			this.CC_IsActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 17, true);
			this.CC_IsActiveCheckBox.TabIndex = 5;
			// 
			// DescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.DescriptionTextBox, "CC_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusClassification)(null)).CC_Description)));
			this.DescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 80, true);
			this.DescriptionTextBox.Multiline = true;
			this.DescriptionTextBox.Name = "DescriptionTextBox";
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(386, 32, true);
			this.DescriptionTextBox.TabIndex = 2;
			// 
			// LookupCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.LookupCodeTextBox, "CC_LookupCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusClassification)(null)).CC_LookupCode)));
			this.LookupCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 24, true);
			this.LookupCodeTextBox.Name = "LookupCodeTextBox";
			this.LookupCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(386, 20, true);
			this.LookupCodeTextBox.TabIndex = 0;
			// 
			// BaseClassificationUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.BaseClassificationGroupBox);
			this.Name = "BaseClassificationUserControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(535, 264, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BaseClassificationGroupBox.ResumeLayout(false);
			this.BaseClassificationGroupBox.PerformLayout();
			this.ResumeLayout(false);
		}
		#endregion
	}
}
