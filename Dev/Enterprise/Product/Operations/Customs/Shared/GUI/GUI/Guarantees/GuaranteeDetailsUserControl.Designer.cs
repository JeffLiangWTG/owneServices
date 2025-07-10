namespace Enterprise.Customs.GUI
{
	public partial class GuaranteeDetailsUserControl
	{
		void InitializeComponent()
		{
			this.GuaranteeTypeZDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.EndDateZDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.GuaranteeSubTypeZDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.GuaranteeHolderZGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.CreationCountryZCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.StartDateZDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.GuaranteeNumberZTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.QtyValIndicatorZDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CurrencyZCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.GuaranteeTypeZDropEdit.SuspendLayout();
			this.EndDateZDateEdit.SuspendLayout();
			this.GuaranteeSubTypeZDropEdit.SuspendLayout();
			this.GuaranteeHolderZGuidFindBox.SuspendLayout();
			this.CreationCountryZCodeFindBox.SuspendLayout();
			this.StartDateZDateEdit.SuspendLayout();
			this.QtyValIndicatorZDropEdit.SuspendLayout();
			this.CurrencyZCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.BaseCusGuaranteeHeader);
			// 
			// GuaranteeTypeZDropEdit
			// 
			this.GuaranteeTypeZDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GuaranteeTypeZDropEdit, "CPH_Type");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseCusGuaranteeHeader)(null)).CPH_Type)));
			this.GuaranteeTypeZDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 3, true);
			this.GuaranteeTypeZDropEdit.Name = "GuaranteeTypeZDropEdit";
			this.GuaranteeTypeZDropEdit.PreBoundMaxLength = 3;
			this.GuaranteeTypeZDropEdit.ShouldResizeByMaxLength = true;
			this.GuaranteeTypeZDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			this.GuaranteeTypeZDropEdit.TabIndex = 0;
			// 
			// EndDateZDateEdit
			// 
			this.EndDateZDateEdit.AllowDrop = true;
			this.EndDateZDateEdit.AutoCompleteMonthThreshold = 1;
			this.EndDateZDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.EndDateZDateEdit, "CPH_EndDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseCusGuaranteeHeader)(null)).CPH_EndDate)));
			this.EndDateZDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(295, 81, true);
			this.EndDateZDateEdit.Name = "EndDateZDateEdit";
			this.EndDateZDateEdit.TabIndex = 8;
			// 
			// GuaranteeSubTypeZDropEdit
			// 
			this.GuaranteeSubTypeZDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GuaranteeSubTypeZDropEdit, "CPH_SubType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseCusGuaranteeHeader)(null)).CPH_SubType)));
			this.GuaranteeSubTypeZDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(532, 3, true);
			this.GuaranteeSubTypeZDropEdit.Name = "GuaranteeSubTypeZDropEdit";
			this.GuaranteeSubTypeZDropEdit.PreBoundMaxLength = 3;
			this.GuaranteeSubTypeZDropEdit.ShouldResizeByMaxLength = true;
			this.GuaranteeSubTypeZDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			this.GuaranteeSubTypeZDropEdit.TabIndex = 1;
			// 
			// GuaranteeHolderZGuidFindBox
			// 
			this.GuaranteeHolderZGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GuaranteeHolderZGuidFindBox, "CPH_OH_PermitHolder");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.Business.BaseCusGuaranteeHeader)(null)).CPH_OH_PermitHolder)));
			this.GuaranteeHolderZGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 29, true);
			this.GuaranteeHolderZGuidFindBox.Name = "GuaranteeHolderZGuidFindBox";
			this.GuaranteeHolderZGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.GuaranteeHolderZGuidFindBox.ParentType = null;
			this.GuaranteeHolderZGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			this.GuaranteeHolderZGuidFindBox.TabIndex = 2;
			// 
			// CreationCountryFindBox
			// 
			this.CreationCountryZCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CreationCountryZCodeFindBox, "CPH_RN_NKCountryCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusGuaranteeHeader)(null)).CPH_RN_NKCountryCode)));
			this.CreationCountryZCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(532, 29, true);
			this.CreationCountryZCodeFindBox.Name = "CreationCountryZCodeFindBox";
			this.CreationCountryZCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CreationCountryZCodeFindBox.ParentType = null;
			this.CreationCountryZCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			this.CreationCountryZCodeFindBox.TabIndex = 3;
			// 
			// StartDateZDateEdit
			// 
			this.StartDateZDateEdit.AllowDrop = true;
			this.StartDateZDateEdit.AutoCompleteMonthThreshold = 1;
			this.StartDateZDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.StartDateZDateEdit, "CPH_StartDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseCusGuaranteeHeader)(null)).CPH_StartDate)));
			this.StartDateZDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 81, true);
			this.StartDateZDateEdit.Name = "StartDateZDateEdit";
			this.StartDateZDateEdit.TabIndex = 7;
			// 
			// GuaranteeNumberZTextBox
			// 
			this.GuaranteeNumberZTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GuaranteeNumberZTextBox, "CPH_Number");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusGuaranteeHeader)(null)).CPH_Number)));
			this.GuaranteeNumberZTextBox.CaptionResourceString = null;
			this.GuaranteeNumberZTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 55, true);
			this.GuaranteeNumberZTextBox.Name = "GuaranteeNumberZTextBox";
			this.GuaranteeNumberZTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			this.GuaranteeNumberZTextBox.TabIndex = 4;
			// 
			// QtyValIndicatorZDropEdit
			// 
			this.QtyValIndicatorZDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.QtyValIndicatorZDropEdit, "CPH_QtyValIndicator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.BaseCusGuaranteeHeader)(null)).CPH_QtyValIndicator)));
			this.QtyValIndicatorZDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(532, 55, true);
			this.QtyValIndicatorZDropEdit.Name = "QtyValIndicatorZDropEdit";
			this.QtyValIndicatorZDropEdit.PreBoundMaxLength = 3;
			this.QtyValIndicatorZDropEdit.ShouldResizeByMaxLength = true;
			this.QtyValIndicatorZDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.QtyValIndicatorZDropEdit.TabIndex = 6;
			this.QtyValIndicatorZDropEdit.Visible = false;
			// 
			// CurrencyZCodeFindBox
			// 
			this.CurrencyZCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CurrencyZCodeFindBox, "CPH_UnitOfMeasure");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.BaseCusGuaranteeHeader)(null)).CPH_UnitOfMeasure)));
			this.CurrencyZCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(532, 55, true);
			this.CurrencyZCodeFindBox.Name = "CurrencyZCodeFindBox";
			this.CurrencyZCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CurrencyZCodeFindBox.ParentType = null;
			this.CurrencyZCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 20, true);
			this.CurrencyZCodeFindBox.TabIndex = 5;
			// 
			// GuaranteeDetailsUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CurrencyZCodeFindBox);
			this.Controls.Add(this.QtyValIndicatorZDropEdit);
			this.Controls.Add(this.GuaranteeTypeZDropEdit);
			this.Controls.Add(this.EndDateZDateEdit);
			this.Controls.Add(this.GuaranteeSubTypeZDropEdit);
			this.Controls.Add(this.GuaranteeHolderZGuidFindBox);
			this.Controls.Add(this.CreationCountryZCodeFindBox);
			this.Controls.Add(this.StartDateZDateEdit);
			this.Controls.Add(this.GuaranteeNumberZTextBox);
			this.Name = "GuaranteeDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(792, 109, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.GuaranteeTypeZDropEdit.ResumeLayout(true);
			this.GuaranteeTypeZDropEdit.PerformLayout();
			this.EndDateZDateEdit.ResumeLayout(true);
			this.EndDateZDateEdit.PerformLayout();
			this.GuaranteeSubTypeZDropEdit.ResumeLayout(true);
			this.GuaranteeSubTypeZDropEdit.PerformLayout();
			this.GuaranteeHolderZGuidFindBox.ResumeLayout(true);
			this.GuaranteeHolderZGuidFindBox.PerformLayout();
			this.CreationCountryZCodeFindBox.ResumeLayout(true);
			this.CreationCountryZCodeFindBox.PerformLayout();
			this.StartDateZDateEdit.ResumeLayout(true);
			this.StartDateZDateEdit.PerformLayout();
			this.QtyValIndicatorZDropEdit.ResumeLayout(true);
			this.QtyValIndicatorZDropEdit.PerformLayout();
			this.CurrencyZCodeFindBox.ResumeLayout(true);
			this.CurrencyZCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		protected ZArchitecture.GUI.ZDropEdit GuaranteeTypeZDropEdit;
		private ZArchitecture.GUI.ZDateEdit EndDateZDateEdit;
		protected ZArchitecture.GUI.ZDropEdit GuaranteeSubTypeZDropEdit;
		private ZArchitecture.GUI.ZGuidFindBox GuaranteeHolderZGuidFindBox;
		private ZArchitecture.GUI.ZCodeFindBox CreationCountryZCodeFindBox;
		protected ZArchitecture.GUI.ZDateEdit StartDateZDateEdit;
		protected ZArchitecture.ZTextBox GuaranteeNumberZTextBox;
		protected ZArchitecture.GUI.ZDropEdit QtyValIndicatorZDropEdit;
		protected ZArchitecture.GUI.ZCodeFindBox CurrencyZCodeFindBox;
	}
}
