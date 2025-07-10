namespace Enterprise.Customs.TR.GUI
{
	partial class ShipmentTypeUserControl
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
            this.EntrySubStyleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.EntryDateForDutyDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.BankCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.DutyPaymentTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.InspectionClerkTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.GoodsAtCustomsAreaCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.OverTimePaymentCompletedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.EntrySubStyleDropEdit.SuspendLayout();
            this.EntryDateForDutyDateEdit.SuspendLayout();
            this.BankCodeFindBox.SuspendLayout();
            this.DutyPaymentTypeDropEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TR.Business.Declaration.JobDeclaration);
            // 
            // EntrySubStyleDropEdit
            // 
            this.EntrySubStyleDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.EntrySubStyleDropEdit, "JE_EntrySubStyle");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).JE_EntrySubStyle)));
            this.EntrySubStyleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 12, true);
            this.EntrySubStyleDropEdit.Name = "EntrySubStyleDropEdit";
            this.EntrySubStyleDropEdit.PreBoundMaxLength = 2;
            this.EntrySubStyleDropEdit.ShouldResizeByMaxLength = true;
            this.EntrySubStyleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 20, true);
            this.EntrySubStyleDropEdit.TabIndex = 6;
            // 
            // EntryDateForDutyDateEdit
            // 
            this.EntryDateForDutyDateEdit.AllowDrop = true;
            this.EntryDateForDutyDateEdit.AutoCompleteMonthThreshold = 1;
            this.EntryDateForDutyDateEdit.AutoCompleteYear = true;
            this.BindingSource.SetBindingMember(this.EntryDateForDutyDateEdit, "JE_EntryDateForDuty");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).JE_EntryDateForDuty)));
            this.EntryDateForDutyDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 38, true);
            this.EntryDateForDutyDateEdit.Name = "EntryDateForDutyDateEdit";
            this.EntryDateForDutyDateEdit.TabIndex = 7;
            // 
            // BankCodeFindBox
            // 
            this.BankCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.BankCodeFindBox, "ZG_BankCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).ZG_BankCode)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).AddInfoLookups.BankCodeList)));
            this.BankCodeFindBox.BindToList = "AddInfoLookups.BankCodeList";
            this.BankCodeFindBox.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("C27EB97B-1DEE-426C-84C0-C2C3DD6072DC", "[28] Bank Code");
            this.BankCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 60, true);
            this.BankCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
            this.BankCodeFindBox.Name = "BankCodeFindBox";
            this.BankCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.BankCodeFindBox.ParentType = null;
            this.BankCodeFindBox.ShowDescriptionBox = false;
            this.BankCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
            this.BankCodeFindBox.TabIndex = 8;
            // 
            // DutyPaymentTypeDropEdit
            // 
            this.DutyPaymentTypeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.DutyPaymentTypeDropEdit, "JE_PaymentMethod");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).JE_PaymentMethod)));
            this.DutyPaymentTypeDropEdit.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("59c6e00c-64a7-4290-bfa1-3aaf312ef290", "Pay.Type", "[47] Duty Pay.Type", "");
            this.DutyPaymentTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 126, true);
            this.DutyPaymentTypeDropEdit.Name = "DutyPaymentTypeDropEdit";
            this.DutyPaymentTypeDropEdit.PreBoundMaxLength = 1;
            this.DutyPaymentTypeDropEdit.ShouldResizeByMaxLength = true;
            this.DutyPaymentTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 20, true);
            this.DutyPaymentTypeDropEdit.TabIndex = 10;
            // 
            // InspectionClerkTextBox
            // 
            this.InspectionClerkTextBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.InspectionClerkTextBox, "InspectionClerk");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).InspectionClerk)));
            this.InspectionClerkTextBox.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("000CBD33-F98D-47A6-A31A-6D27199BD879", "Inspection Clerk");
            this.InspectionClerkTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(100, 153, true);
            this.InspectionClerkTextBox.Name = "InspectionClerkTextBox";
            this.InspectionClerkTextBox.ShouldEscapeAllSpecialCharacters = false;
            this.InspectionClerkTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(145, 20, true);
            this.InspectionClerkTextBox.TabIndex = 20;
            // 
            // GoodsAtCustomsAreaCheckBox
            // 
            this.BindingSource.SetBindingMember(this.GoodsAtCustomsAreaCheckBox, "GoodsAtCustomsArea");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).GoodsAtCustomsArea)));
            this.GoodsAtCustomsAreaCheckBox.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("1c685839-6ba3-422e-a835-291f126720cf", "Customs Area", "Goods are at Customs Area", "");
            this.GoodsAtCustomsAreaCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
            this.GoodsAtCustomsAreaCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.GoodsAtCustomsAreaCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 174, true);
            this.GoodsAtCustomsAreaCheckBox.Name = "GoodsAtCustomsAreaCheckBox";
            this.GoodsAtCustomsAreaCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 17, true);
            this.GoodsAtCustomsAreaCheckBox.TabIndex = 21;
            this.GoodsAtCustomsAreaCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.GoodsAtCustomsAreaCheckBox.UseVisualStyleBackColor = true;
            // 
            // OverTimePaymentCompletedCheckBox
            // 
            this.BindingSource.SetBindingMember(this.OverTimePaymentCompletedCheckBox, "OverTimePaymentCompleted");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).OverTimePaymentCompleted)));
            this.OverTimePaymentCompletedCheckBox.CaptionResourceString = Enterprise.Customs.TR.GUI.Res.GetData("41934894-1666-4832-b15b-6e652cfbe24f", "OTP Completed", "Overtime Time Procedure Completed", "");
            this.OverTimePaymentCompletedCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
            this.OverTimePaymentCompletedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.OverTimePaymentCompletedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 197, true);
            this.OverTimePaymentCompletedCheckBox.Name = "OverTimePaymentCompletedCheckBox";
            this.OverTimePaymentCompletedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 19, true);
            this.OverTimePaymentCompletedCheckBox.TabIndex = 22;
            this.OverTimePaymentCompletedCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.OverTimePaymentCompletedCheckBox.UseVisualStyleBackColor = true;
            // 
            // ShipmentTypeUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.EntrySubStyleDropEdit);
            this.Controls.Add(this.EntryDateForDutyDateEdit);
            this.Controls.Add(this.BankCodeFindBox);
            this.Controls.Add(this.DutyPaymentTypeDropEdit);
            this.Controls.Add(this.InspectionClerkTextBox);
            this.Controls.Add(this.GoodsAtCustomsAreaCheckBox);
            this.Controls.Add(this.OverTimePaymentCompletedCheckBox);
            this.Name = "ShipmentTypeUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 227, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.EntrySubStyleDropEdit.ResumeLayout(true);
            this.EntrySubStyleDropEdit.PerformLayout();
            this.EntryDateForDutyDateEdit.ResumeLayout(true);
            this.EntryDateForDutyDateEdit.PerformLayout();
            this.BankCodeFindBox.ResumeLayout(true);
            this.BankCodeFindBox.PerformLayout();
            this.DutyPaymentTypeDropEdit.ResumeLayout(true);
            this.DutyPaymentTypeDropEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZDropEdit EntrySubStyleDropEdit;
		internal ZArchitecture.GUI.ZDateEdit EntryDateForDutyDateEdit;
		internal ZArchitecture.GUI.ZCodeFindBox BankCodeFindBox;
		internal ZArchitecture.GUI.ZDropEdit DutyPaymentTypeDropEdit;
		internal Enterprise.ZArchitecture.ZTextBox InspectionClerkTextBox;
		internal ZArchitecture.GUI.ZCheckBox GoodsAtCustomsAreaCheckBox;
		internal ZArchitecture.GUI.ZCheckBox OverTimePaymentCompletedCheckBox;
	}
}
