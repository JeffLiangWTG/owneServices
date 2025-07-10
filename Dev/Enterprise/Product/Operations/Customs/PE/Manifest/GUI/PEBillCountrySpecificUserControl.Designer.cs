namespace Enterprise.Customs.PE.Manifest.GUI
{
	partial class PEBillCountrySpecificUserControl
	{
		private void InitializeComponent()
		{
			this.BillIssueDateEdit = new ZArchitecture.GUI.ZDateEdit();
			this.CargoNatureDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CargoConditionDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BillIssueDateEdit.SuspendLayout();
			this.CargoNatureDropEdit.SuspendLayout();
			this.CargoConditionDropEdit.SuspendLayout();
			this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.PE.Manifest.Business.AsycudaBill);
			// 
			// BillIssueDateEdit
			// 
			this.BillIssueDateEdit.AllowDrop = true;
			this.BillIssueDateEdit.AutoCompleteMonthThreshold = 1;
			this.BillIssueDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.BillIssueDateEdit, "ABL_BillIssueDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.PE.Manifest.Business.AsycudaBill)(null)).ABL_BillIssueDate)));
			this.BillIssueDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			this.BillIssueDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 334, true);
			this.BillIssueDateEdit.Name = "BillIssueDateEdit";
			this.BillIssueDateEdit.TabIndex = 1;
			// 
			// CargoNatureDropEdit
			// 
			this.CargoNatureDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CargoNatureDropEdit, "CargoNature");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.PE.Manifest.Business.AsycudaBill)(null)).CargoNature)));
			this.CargoNatureDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 36, true);
			this.CargoNatureDropEdit.Name = "CargoNatureDropEdit";
			this.CargoNatureDropEdit.PreBoundMaxLength = 2;
			this.CargoNatureDropEdit.ShouldResizeByMaxLength = true;
			this.CargoNatureDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.CargoNatureDropEdit.TabIndex = 2;
			// 
			// CargoConditionDropEdit
			// 
			this.CargoConditionDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CargoConditionDropEdit, "CargoCondition");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.PE.Manifest.Business.AsycudaBill)(null)).CargoCondition)));
			this.CargoConditionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 36, true);
			this.CargoConditionDropEdit.Name = "CargoConditionDropEdit";
			this.CargoConditionDropEdit.PreBoundMaxLength = 2;
			this.CargoConditionDropEdit.ShouldResizeByMaxLength = true;
			this.CargoConditionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.CargoConditionDropEdit.TabIndex = 3;
			// 
			// PEBillCountrySpecificUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.BillIssueDateEdit);
			this.Controls.Add(this.CargoNatureDropEdit);
			this.Controls.Add(this.CargoConditionDropEdit);
            this.Name = "PEBillCountrySpecificUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(193, 68, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BillIssueDateEdit.ResumeLayout(true);
			this.BillIssueDateEdit.PerformLayout();
			this.CargoNatureDropEdit.ResumeLayout(true);
			this.CargoNatureDropEdit.PerformLayout();
            this.CargoConditionDropEdit.ResumeLayout(true);
            this.CargoConditionDropEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		internal Enterprise.ZArchitecture.GUI.ZDateEdit BillIssueDateEdit;
		internal ZArchitecture.GUI.ZDropEdit CargoNatureDropEdit;
		internal ZArchitecture.GUI.ZDropEdit CargoConditionDropEdit;
	}
}
