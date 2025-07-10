using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.GUI
{
	partial class DeclarationDetailsUserControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.ExitEUDateTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MessagingStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ReleaseDateTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AcceptanceDateTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PhaseStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.EntryStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DeclarationDetailsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MessagingStatusDropEdit.SuspendLayout();
			this.PhaseStatusDropEdit.SuspendLayout();
			this.EntryStatusDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.DeclarationDetailsGrid)).BeginInit();
			this.DeclarationDetailsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NL.Business.Declaration.JobDeclaration);
			// 
			// ExitEUDateTextBox
			// 
			this.ExitEUDateTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExitEUDateTextBox, "CustomsEntryHeaders.CH_ExitDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.NL.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.NL.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CH_ExitDate)));
			this.ExitEUDateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(404, 161, true);
			this.ExitEUDateTextBox.Name = "ExitEUDateTextBox";
			this.ExitEUDateTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(103, 20, true);
			this.ExitEUDateTextBox.TabIndex = 6;
			// 
			// MessagingStatusDropEdit
			// 
			this.MessagingStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MessagingStatusDropEdit, "CustomsEntryHeaders.CH_Status");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NL.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.NL.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CH_Status)));
			this.MessagingStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 161, true);
			this.MessagingStatusDropEdit.Name = "MessagingStatusDropEdit";
			this.MessagingStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(218, 20, true);
			this.MessagingStatusDropEdit.TabIndex = 3;
			// 
			// ReleaseDateTextBox
			// 
			this.ReleaseDateTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ReleaseDateTextBox, "CustomsEntryHeaders.CH_EntryReleaseDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.NL.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.NL.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CH_EntryReleaseDate)));
			this.ReleaseDateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(404, 141, true);
			this.ReleaseDateTextBox.Name = "ReleaseDateTextBox";
			this.ReleaseDateTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(103, 20, true);
			this.ReleaseDateTextBox.TabIndex = 5;
			// 
			// AcceptanceDateTextBox
			// 
			this.AcceptanceDateTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AcceptanceDateTextBox, "CustomsEntryHeaders.CusEntryNumberIssueDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.NL.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.NL.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CusEntryNumberIssueDate)));
			this.AcceptanceDateTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(404, 120, true);
			this.AcceptanceDateTextBox.Name = "AcceptanceDateTextBox";
			this.AcceptanceDateTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(103, 20, true);
			this.AcceptanceDateTextBox.TabIndex = 4;
			// 
			// PhaseStatusDropEdit
			// 
			this.PhaseStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PhaseStatusDropEdit, "CustomsEntryHeaders.CH_PhaseStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NL.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.NL.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CH_PhaseStatus)));
			this.PhaseStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 141, true);
			this.PhaseStatusDropEdit.Name = "PhaseStatusDropEdit";
			this.PhaseStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(218, 20, true);
			this.PhaseStatusDropEdit.TabIndex = 2;
			// 
			// EntryStatusDropEdit
			// 
			this.EntryStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EntryStatusDropEdit, "CustomsEntryHeaders.CH_EntryStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NL.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.NL.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CH_EntryStatus)));
			this.EntryStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 120, true);
			this.EntryStatusDropEdit.Name = "EntryStatusDropEdit";
			this.EntryStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(218, 20, true);
			this.EntryStatusDropEdit.TabIndex = 1;
			// 
			// DeclarationDetailsGrid
			// 
			this.DeclarationDetailsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.DeclarationDetailsGrid, "CustomsEntryHeaders");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.NL.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NL.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.NL.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).EntryNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NL.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.NL.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CH_BGMReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NL.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.NL.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).CH_EntryStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.NL.Business.Declaration.CusEntryHeader)(((System.Collections.IList)(((Enterprise.Customs.NL.Business.Declaration.JobDeclaration)(null)).CustomsEntryHeaders)).SyncRoot)).DMSFallbackIsActive)));
			this.DeclarationDetailsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "EntryNumber";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo3.ColumnName = "CH_BGMReference";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo4.ColumnName = "CH_EntryStatus";
			zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCheckBoxColumnStyleInfo1.ColumnName = "DMSFallbackIsActive";
			zCheckBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			this.DeclarationDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.DeclarationDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.DeclarationDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.DeclarationDetailsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.DeclarationDetailsGrid.GridId = "67BF074B-778B-4C6D-8931-CC31B704B657";
			this.DeclarationDetailsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DeclarationDetailsGrid.LayoutKey = "DeclarationDetailsGrid";
			this.DeclarationDetailsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 14, true);
			this.DeclarationDetailsGrid.Name = "DeclarationDetailsGrid";
			this.DeclarationDetailsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 100, true);
			this.DeclarationDetailsGrid.TabIndex = 0;
			// 
			// DeclarationDetailsUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ExitEUDateTextBox);
			this.Controls.Add(this.MessagingStatusDropEdit);
			this.Controls.Add(this.ReleaseDateTextBox);
			this.Controls.Add(this.AcceptanceDateTextBox);
			this.Controls.Add(this.PhaseStatusDropEdit);
			this.Controls.Add(this.EntryStatusDropEdit);
			this.Controls.Add(this.DeclarationDetailsGrid);
			this.Name = "DeclarationDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(518, 203, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MessagingStatusDropEdit.ResumeLayout(true);
			this.MessagingStatusDropEdit.PerformLayout();
			this.PhaseStatusDropEdit.ResumeLayout(true);
			this.PhaseStatusDropEdit.PerformLayout();
			this.EntryStatusDropEdit.ResumeLayout(true);
			this.EntryStatusDropEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.DeclarationDetailsGrid)).EndInit();
			this.DeclarationDetailsGrid.ResumeLayout(false);
			this.DeclarationDetailsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZGrid DeclarationDetailsGrid;
		internal ZDropEdit EntryStatusDropEdit;
		internal ZDropEdit PhaseStatusDropEdit;
		internal ZDropEdit MessagingStatusDropEdit;
		internal ZTextBox AcceptanceDateTextBox;
		internal ZTextBox ReleaseDateTextBox;
		internal ZTextBox ExitEUDateTextBox;
	}
}
