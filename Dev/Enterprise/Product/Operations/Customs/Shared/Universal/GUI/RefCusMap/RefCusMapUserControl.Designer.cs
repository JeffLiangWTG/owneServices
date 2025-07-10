using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.Universal.GUI
{
	public partial class RefCusMapUserControl
	{
		void InitializeComponent()
		{
			this.ZZM_IsSystemCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ZZM_ZZZ_NKDataGroupingCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ZZM_CustomsValue = new Enterprise.ZArchitecture.ZTextBox();
			this.ZZM_ZZP_NKMapType = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ZZM_CW1orCommercialValue = new Enterprise.ZArchitecture.ZTextBox();
			this.EndDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.StartDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ZZM_ZZZ_NKDataGroupingCodeFindBox.SuspendLayout();
			this.ZZM_ZZP_NKMapType.SuspendLayout();
			this.EndDateEdit.SuspendLayout();
			this.StartDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Universal.ZZRefCusMapCombined);
			// 
			// ZZM_IsSystemCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ZZM_IsSystemCheckBox, "ZZM_IsSystem");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.Universal.ZZRefCusMapCombined)(null)).ZZM_IsSystem)));
			this.ZZM_IsSystemCheckBox.CaptionResourceString = Enterprise.Customs.Universal.GUI.Res.GetData("ed77ab24-09a7-41f7-a9f9-0fd8191d9901", "Is System");
			this.ZZM_IsSystemCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.ZZM_IsSystemCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ZZM_IsSystemCheckBox.ForeColor = System.Drawing.SystemColors.GrayText;
			this.ZZM_IsSystemCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 175, true);
			this.ZZM_IsSystemCheckBox.Name = "ZZM_IsSystemCheckBox";
			this.ZZM_IsSystemCheckBox.ReadOnly = true;
			this.ZZM_IsSystemCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 19, true);
			this.ZZM_IsSystemCheckBox.TabIndex = 6;
			this.ZZM_IsSystemCheckBox.UseVisualStyleBackColor = true;
			// 
			// ZZM_ZZZ_NKDataGroupingCodeFindBox
			// 
			this.ZZM_ZZZ_NKDataGroupingCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ZZM_ZZZ_NKDataGroupingCodeFindBox, "ZZM_ZZZ_NKDataGrouping");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Universal.ZZRefCusMapCombined)(null)).ZZM_ZZZ_NKDataGrouping)));
			this.ZZM_ZZZ_NKDataGroupingCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 97, true);
			this.ZZM_ZZZ_NKDataGroupingCodeFindBox.Name = "ZZM_ZZZ_NKDataGroupingCodeFindBox";
			this.ZZM_ZZZ_NKDataGroupingCodeFindBox.PreBoundMaxLength = 2;
			this.ZZM_ZZZ_NKDataGroupingCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(285, 17, true);
			this.ZZM_ZZZ_NKDataGroupingCodeFindBox.TabIndex = 3;
			// 
			// ZZM_CustomsValue
			// 
			this.BindingSource.SetBindingMember(this.ZZM_CustomsValue, "ZZM_CustomsValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Universal.ZZRefCusMapCombined)(null)).ZZM_CustomsValue)));
			this.ZZM_CustomsValue.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 71, true);
			this.ZZM_CustomsValue.Name = "ZZM_CustomsValue";
			this.ZZM_CustomsValue.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.ZZM_CustomsValue.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(285, 17, true);
			this.ZZM_CustomsValue.TabIndex = 2;
			// 
			// ZZM_ZZP_NKMapType
			// 
			this.ZZM_ZZP_NKMapType.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ZZM_ZZP_NKMapType, "ZZM_ZZP_NKMapType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Universal.ZZRefCusMapCombined)(null)).ZZM_ZZP_NKMapType)));
			this.ZZM_ZZP_NKMapType.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 19, true);
			this.ZZM_ZZP_NKMapType.Name = "ZZM_ZZP_NKMapType";
			this.ZZM_ZZP_NKMapType.PreBoundMaxLength = 5;
			this.ZZM_ZZP_NKMapType.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(285, 17, true);
			this.ZZM_ZZP_NKMapType.TabIndex = 0;
			// 
			// ZZM_CW1orCommercialValue
			// 
			this.BindingSource.SetBindingMember(this.ZZM_CW1orCommercialValue, "ZZM_CW1orCommercialValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Universal.ZZRefCusMapCombined)(null)).ZZM_CW1orCommercialValue)));
			this.ZZM_CW1orCommercialValue.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 45, true);
			this.ZZM_CW1orCommercialValue.Name = "ZZM_CW1orCommercialValue";
			this.ZZM_CW1orCommercialValue.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(285, 17, true);
			this.ZZM_CW1orCommercialValue.TabIndex = 1;
			// 
			// EndDateEdit
			// 
			this.EndDateEdit.AllowDrop = true;
			this.EndDateEdit.AutoCompleteMonthThreshold = 1;
			this.EndDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.EndDateEdit, "ZZM_EndDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Universal.ZZRefCusMapCombined)(null)).ZZM_EndDate)));
			this.EndDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 149, true);
			this.EndDateEdit.Name = "EndDateEdit";
			this.EndDateEdit.TabIndex = 5;
			// 
			// StartDateEdit
			// 
			this.StartDateEdit.AllowDrop = true;
			this.StartDateEdit.AutoCompleteMonthThreshold = 1;
			this.StartDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.StartDateEdit, "ZZM_StartDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Universal.ZZRefCusMapCombined)(null)).ZZM_StartDate)));
			this.StartDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(148, 124, true);
			this.StartDateEdit.Name = "StartDateEdit";
			this.StartDateEdit.TabIndex = 4;
			// 
			// RefCusMapUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.StartDateEdit);
			this.Controls.Add(this.EndDateEdit);
			this.Controls.Add(this.ZZM_IsSystemCheckBox);
			this.Controls.Add(this.ZZM_ZZZ_NKDataGroupingCodeFindBox);
			this.Controls.Add(this.ZZM_CustomsValue);
			this.Controls.Add(this.ZZM_ZZP_NKMapType);
			this.Controls.Add(this.ZZM_CW1orCommercialValue);
			this.Name = "RefCusMapUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(510, 218, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ZZM_ZZZ_NKDataGroupingCodeFindBox.ResumeLayout(true);
			this.ZZM_ZZZ_NKDataGroupingCodeFindBox.PerformLayout();
			this.ZZM_ZZP_NKMapType.ResumeLayout(true);
			this.ZZM_ZZP_NKMapType.PerformLayout();
			this.EndDateEdit.ResumeLayout(true);
			this.EndDateEdit.PerformLayout();
			this.StartDateEdit.ResumeLayout(true);
			this.StartDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		ZCheckBox ZZM_IsSystemCheckBox;
		ZCodeFindBox ZZM_ZZZ_NKDataGroupingCodeFindBox;
		ZDropEdit ZZM_ZZP_NKMapType;
		ZArchitecture.ZTextBox ZZM_CustomsValue;
		ZDateEdit EndDateEdit;
		ZDateEdit StartDateEdit;
		ZArchitecture.ZTextBox ZZM_CW1orCommercialValue;
	}
}
