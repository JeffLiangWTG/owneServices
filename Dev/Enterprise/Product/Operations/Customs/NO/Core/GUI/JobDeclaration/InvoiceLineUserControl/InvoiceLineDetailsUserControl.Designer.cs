namespace Enterprise.Customs.NO.GUI
{
	partial class InvoiceLineDetailsUserControl
	{
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ProcedureCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CustomsRateOverrideUserControl = new Enterprise.Customs.NO.GUI.CustomsRateOverrideUserControl();
			this.RtRateOverrideCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.GoodsMarksLongTextControl = new Enterprise.Customs.GUI.LongTextControl();
			this.ReducedCustomsFlagDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MergeOverrideTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PackageTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SupplementaryCode1DropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SupplementaryCode2DropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AdditionalSupplementaryCodesUserControl = new Enterprise.Customs.NO.GUI.AdditionalSupplementaryCodesUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ProcedureCodeDropEdit.SuspendLayout();
			this.CustomsRateOverrideUserControl.SuspendLayout();
			this.RtRateOverrideCalcEdit.SuspendLayout();
			this.GoodsMarksLongTextControl.SuspendLayout();
			this.ReducedCustomsFlagDropEdit.SuspendLayout();
			this.PackageTypeDropEdit.SuspendLayout();
			this.SupplementaryCode1DropEdit.SuspendLayout();
			this.SupplementaryCode2DropEdit.SuspendLayout();
			this.AdditionalSupplementaryCodesUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NO.Business.JobComInvoiceLine);
			// 
			// ProcedureCodeDropEdit
			// 
			this.ProcedureCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ProcedureCodeDropEdit, "JI_Procedure");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NO.Business.JobComInvoiceLine)(null)).JI_Procedure)));
			this.ProcedureCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(91, 14, true);
			this.ProcedureCodeDropEdit.Name = "ProcedureCodeDropEdit";
			this.ProcedureCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 18, true);
			this.ProcedureCodeDropEdit.TabIndex = 0;
			// 
			// CustomsRateOverrideUserControl
			// 
			this.CustomsRateOverrideUserControl.AllowDrop = true;
			this.CustomsRateOverrideUserControl.AutoSize = true;
			this.BindingSource.SetBindingMember(this.CustomsRateOverrideUserControl, ".");
			this.CustomsRateOverrideUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(91, 44, true);
			this.CustomsRateOverrideUserControl.Name = "CustomsRateOverrideUserControl";
			this.CustomsRateOverrideUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 23, true);
			this.CustomsRateOverrideUserControl.TabIndex = 1;
			// 
			// GoodsMarksLongTextControl
			// 
			this.BindingSource.SetBindingMember(this.GoodsMarksLongTextControl, "JI_GoodsMarks");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NO.Business.JobComInvoiceLine)(null)).JI_GoodsMarks)));
			this.GoodsMarksLongTextControl.AllowDrop = true;
			this.GoodsMarksLongTextControl.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.GoodsMarksLongTextControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(91, 72, true);
			this.GoodsMarksLongTextControl.Name = "GoodsMarksLongTextControl";
			this.GoodsMarksLongTextControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.GoodsMarksLongTextControl.TabIndex = 2;
			// 
			// ReducedCustomsFlagDropEdit
			// 
			this.ReducedCustomsFlagDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReducedCustomsFlagDropEdit, "JI_ReducedCustomsFlag");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NO.Business.JobComInvoiceLine)(null)).JI_ReducedCustomsFlag)));
			this.ReducedCustomsFlagDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(91, 107, true);
			this.ReducedCustomsFlagDropEdit.Name = "ReducedCustomsFlagDropEdit";
			this.ReducedCustomsFlagDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 18, true);
			this.ReducedCustomsFlagDropEdit.TabIndex = 3;
			// 
			// MergeOverrideTextBox
			// 
			this.MergeOverrideTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MergeOverrideTextBox, "JI_MergeOverride");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Business.JobComInvoiceLine)(null)).JI_MergeOverride)));
			this.MergeOverrideTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(91, 138, true);
			this.MergeOverrideTextBox.Name = "MergeOverrideTextBox";
			this.MergeOverrideTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 18, true);
			this.MergeOverrideTextBox.TabIndex = 4;
			// 
			// PackageTypeDropEdit
			// 
			this.PackageTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PackageTypeDropEdit, "JI_PackageType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NO.Business.JobComInvoiceLine)(null)).JI_PackageType)));
			this.PackageTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 169, true);
			this.PackageTypeDropEdit.Name = "PackageTypeDropEdit";
			this.PackageTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 18, true);
			this.PackageTypeDropEdit.TabIndex = 5;
			// 
			// SupplementaryCode1DropEdit
			// 
			this.SupplementaryCode1DropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SupplementaryCode1DropEdit, "JI_SupplementaryCode1");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NO.Business.JobComInvoiceLine)(null)).JI_SupplementaryCode1)));
			this.SupplementaryCode1DropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(91, 199, true);
			this.SupplementaryCode1DropEdit.Name = "SupplementaryCode1DropEdit";
			this.SupplementaryCode1DropEdit.PreBoundMaxLength = 3;
			this.SupplementaryCode1DropEdit.ShouldResizeByMaxLength = false;
			this.SupplementaryCode1DropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 18, true);
			this.SupplementaryCode1DropEdit.TabIndex = 2;
			// 
			// SupplementaryCode2DropEdit
			// 
			this.SupplementaryCode2DropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SupplementaryCode2DropEdit, "JI_SupplementaryCode2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NO.Business.JobComInvoiceLine)(null)).JI_SupplementaryCode2)));
			this.SupplementaryCode2DropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(91, 231, true);
			this.SupplementaryCode2DropEdit.Name = "SupplementaryCode2DropEdit";
			this.SupplementaryCode2DropEdit.PreBoundMaxLength = 3;
			this.SupplementaryCode2DropEdit.ShouldResizeByMaxLength = false;
			this.SupplementaryCode2DropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 18, true);
			this.SupplementaryCode2DropEdit.TabIndex = 2;
			// 
			// AdditionalSupplementaryCodesUserControl
			// 
			this.AdditionalSupplementaryCodesUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AdditionalSupplementaryCodesUserControl, ".");
			this.AdditionalSupplementaryCodesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 263, true);
			this.AdditionalSupplementaryCodesUserControl.Name = "AdditionalSupplementaryCodesUserControl";
			this.AdditionalSupplementaryCodesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(236, 22, true);
			this.AdditionalSupplementaryCodesUserControl.TabIndex = 4;
			// 
			// RtRateOverrideCalcEdit
			//
			this.BindingSource.SetBindingMember(this.RtRateOverrideCalcEdit, "JI_RTOValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.NO.Business.JobComInvoiceLine)(null)).JI_RTOValue)));
			this.RtRateOverrideCalcEdit.AllowDrop = true;
			this.RtRateOverrideCalcEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.RtRateOverrideCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(91, 285, true);
			this.RtRateOverrideCalcEdit.Name = "RtRateOverrideCalcEdit";
			this.RtRateOverrideCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 20, true);
			// 
			// InvoiceLineDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ProcedureCodeDropEdit);
			this.Controls.Add(this.CustomsRateOverrideUserControl);
			this.Controls.Add(this.RtRateOverrideCalcEdit);
			this.Controls.Add(this.GoodsMarksLongTextControl);
			this.Controls.Add(this.ReducedCustomsFlagDropEdit);
			this.Controls.Add(this.MergeOverrideTextBox);
			this.Controls.Add(this.PackageTypeDropEdit);
			this.Controls.Add(this.SupplementaryCode1DropEdit);
			this.Controls.Add(this.SupplementaryCode2DropEdit);
			this.Controls.Add(this.AdditionalSupplementaryCodesUserControl);
			this.Name = "InvoiceLineDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 520, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ProcedureCodeDropEdit.ResumeLayout(true);
			this.ProcedureCodeDropEdit.PerformLayout();
			this.CustomsRateOverrideUserControl.ResumeLayout(true);
			this.CustomsRateOverrideUserControl.PerformLayout();
			this.RtRateOverrideCalcEdit.ResumeLayout(true);
			this.RtRateOverrideCalcEdit.PerformLayout();
			this.GoodsMarksLongTextControl.ResumeLayout(true);
			this.GoodsMarksLongTextControl.PerformLayout();
			this.ReducedCustomsFlagDropEdit.ResumeLayout(true);
			this.ReducedCustomsFlagDropEdit.PerformLayout();
			this.PackageTypeDropEdit.ResumeLayout(true);
			this.PackageTypeDropEdit.PerformLayout();
			this.SupplementaryCode1DropEdit.ResumeLayout(true);
			this.SupplementaryCode1DropEdit.PerformLayout();
			this.SupplementaryCode2DropEdit.ResumeLayout(true);
			this.SupplementaryCode2DropEdit.PerformLayout();
			this.AdditionalSupplementaryCodesUserControl.ResumeLayout(true);
			this.AdditionalSupplementaryCodesUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal Enterprise.ZArchitecture.GUI.ZDropEdit ProcedureCodeDropEdit;
		internal CustomsRateOverrideUserControl CustomsRateOverrideUserControl;
		internal Enterprise.ZArchitecture.ZCalcEdit RtRateOverrideCalcEdit;
		internal Enterprise.Customs.GUI.LongTextControl GoodsMarksLongTextControl;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit ReducedCustomsFlagDropEdit;
		internal Enterprise.ZArchitecture.ZTextBox MergeOverrideTextBox;
		internal ZArchitecture.GUI.ZDropEdit PackageTypeDropEdit;
		internal ZArchitecture.GUI.ZDropEdit SupplementaryCode1DropEdit;
		internal ZArchitecture.GUI.ZDropEdit SupplementaryCode2DropEdit;
		internal AdditionalSupplementaryCodesUserControl AdditionalSupplementaryCodesUserControl;
	}
}
