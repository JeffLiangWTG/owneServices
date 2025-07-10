using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NO.GUI
{
	partial class EntryInstructionDetailsBasicUserControl
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

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.GoodsNumberUserControl = new Enterprise.Customs.NO.GUI.GoodsNumberAndPositionUserControl();
			this.PackageCountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.RelatedDeclarationSeparatorUserControl = new Enterprise.ZArchitecture.GUI.SeparatorUserControl();
			this.CustomsReplyMessageTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ReasonTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CaseCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OriginalDeclarationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SelectedDeclTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RequestProcessingDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.GoodsNumberUserControl.SuspendLayout();
			this.RelatedDeclarationSeparatorUserControl.SuspendLayout();
			this.CaseCodeDropEdit.SuspendLayout();
			this.OriginalDeclarationDropEdit.SuspendLayout();
			this.SelectedDeclTypeDropEdit.SuspendLayout();
			this.RequestProcessingDateDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NO.Business.CusEntryInstruction);
			// 
			// GoodsNumberUserControl
			// 
			this.GoodsNumberUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GoodsNumberUserControl, ".");
			this.GoodsNumberUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 321, true);
			this.GoodsNumberUserControl.Name = "GoodsNumberUserControl";
			this.GoodsNumberUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.GoodsNumberUserControl.TabIndex = 0;
			// 
			// PackageCountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PackageCountCalcEdit, "CEI_PackageCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.NO.Business.CusEntryInstruction)(null)).CEI_PackageCount)));
			this.PackageCountCalcEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PackageCountCalcEdit.DecimalPlaces = 2;
			this.PackageCountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 299, true);
			this.PackageCountCalcEdit.Name = "PackageCountCalcEdit";
			this.PackageCountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.PackageCountCalcEdit.TabIndex = 0;
			this.PackageCountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			this.PackageCountCalcEdit.TrackDisposedAccess = true;
			// 
			// RelatedDeclarationSeparatorUserControl
			// 
			this.RelatedDeclarationSeparatorUserControl.AllowDrop = true;
			this.RelatedDeclarationSeparatorUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 1, true);
			this.RelatedDeclarationSeparatorUserControl.Name = "RelatedDeclarationSeparatorUserControl";
			this.RelatedDeclarationSeparatorUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(298, 20, true);
			this.RelatedDeclarationSeparatorUserControl.TabIndex = 0;
			// 
			// CustomsReplyMessageTextBox
			// 
			this.BindingSource.SetBindingMember(this.CustomsReplyMessageTextBox, "EntryHeader.CH_ReCalcReplyMessage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Business.CusEntryHeader)(null)).CH_ReCalcReplyMessage)));
			this.CustomsReplyMessageTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 203, true);
			this.CustomsReplyMessageTextBox.Multiline = true;
			this.CustomsReplyMessageTextBox.Name = "CustomsReplyMessageTextBox";
			this.CustomsReplyMessageTextBox.ReadOnly = true;
			this.CustomsReplyMessageTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.CustomsReplyMessageTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 73, true);
			this.CustomsReplyMessageTextBox.TabIndex = 5;
			// 
			// ReasonTextBox
			// 
			this.BindingSource.SetBindingMember(this.ReasonTextBox, "EntryHeader.CH_ReCalcReason");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NO.Business.CusEntryHeader)(null)).CH_ReCalcReason)));
			this.ReasonTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 124, true);
			this.ReasonTextBox.Multiline = true;
			this.ReasonTextBox.Name = "ReasonTextBox";
			this.ReasonTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.ReasonTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 73, true);
			this.ReasonTextBox.TabIndex = 4;
			// 
			// CaseCodeDropEdit
			// 
			this.CaseCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CaseCodeDropEdit, "EntryHeader.CH_ReCalcCaseCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NO.Business.CusEntryHeader)(null)).CH_ReCalcCaseCode)));
			this.CaseCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 98, true);
			this.CaseCodeDropEdit.Name = "CaseCodeDropEdit";
			this.CaseCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 18, true);
			this.CaseCodeDropEdit.TabIndex = 3;
			// 
			// OriginalDeclarationDropEdit
			// 
			this.OriginalDeclarationDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OriginalDeclarationDropEdit, "EntryHeader.CH_ReCalcOrigDecl");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NO.Business.CusEntryHeader)(null)).CH_ReCalcOrigDecl)));
			this.OriginalDeclarationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 71, true);
			this.OriginalDeclarationDropEdit.Name = "OriginalDeclarationDropEdit";
			this.OriginalDeclarationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 18, true);
			this.OriginalDeclarationDropEdit.TabIndex = 2;
			// 
			// SelectedDeclTypeDropEdit
			// 
			this.SelectedDeclTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SelectedDeclTypeDropEdit, "EntryHeader.CH_ReCalcDeclType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NO.Business.CusEntryHeader)(null)).CH_ReCalcDeclType)));
			this.SelectedDeclTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 45, true);
			this.SelectedDeclTypeDropEdit.Name = "SelectedDeclTypeDropEdit";
			this.SelectedDeclTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 18, true);
			this.SelectedDeclTypeDropEdit.TabIndex = 1;
			// 
			// RequestProcessingDateDateEdit
			// 
			this.RequestProcessingDateDateEdit.AllowDrop = true;
			this.RequestProcessingDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.RequestProcessingDateDateEdit, "CEI_DateForDuty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NO.Business.CusEntryInstruction)(null)).CEI_DateForDuty)));
			this.RequestProcessingDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 345, true);
			this.RequestProcessingDateDateEdit.Name = "RequestProcessingDateDateEdit";
			this.RequestProcessingDateDateEdit.TabIndex = 9;
			// 
			// EntryInstructionDetailsBasicUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PackageCountCalcEdit);
			this.Controls.Add(this.GoodsNumberUserControl);
			this.Controls.Add(this.RelatedDeclarationSeparatorUserControl);
			this.Controls.Add(this.CustomsReplyMessageTextBox);
			this.Controls.Add(this.ReasonTextBox);
			this.Controls.Add(this.CaseCodeDropEdit);
			this.Controls.Add(this.OriginalDeclarationDropEdit);
			this.Controls.Add(this.SelectedDeclTypeDropEdit);
			this.Controls.Add(this.RequestProcessingDateDateEdit);
			this.Name = "EntryInstructionDetailsBasicUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(462, 378, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.GoodsNumberUserControl.ResumeLayout(true);
			this.GoodsNumberUserControl.PerformLayout();
			this.RelatedDeclarationSeparatorUserControl.ResumeLayout(true);
			this.RelatedDeclarationSeparatorUserControl.PerformLayout();
			this.CaseCodeDropEdit.ResumeLayout(true);
			this.CaseCodeDropEdit.PerformLayout();
			this.OriginalDeclarationDropEdit.ResumeLayout(true);
			this.OriginalDeclarationDropEdit.PerformLayout();
			this.SelectedDeclTypeDropEdit.ResumeLayout(true);
			this.SelectedDeclTypeDropEdit.PerformLayout();
			this.RequestProcessingDateDateEdit.ResumeLayout(true);
			this.RequestProcessingDateDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal GoodsNumberAndPositionUserControl GoodsNumberUserControl;
		internal ZCalcEdit PackageCountCalcEdit;
		internal SeparatorUserControl RelatedDeclarationSeparatorUserControl;
		internal ZTextBox CustomsReplyMessageTextBox;
		internal ZTextBox ReasonTextBox;
		internal ZDropEdit CaseCodeDropEdit;
		internal ZDropEdit OriginalDeclarationDropEdit;
		internal ZDropEdit SelectedDeclTypeDropEdit;
		internal ZDateEdit RequestProcessingDateDateEdit;
	}
}
