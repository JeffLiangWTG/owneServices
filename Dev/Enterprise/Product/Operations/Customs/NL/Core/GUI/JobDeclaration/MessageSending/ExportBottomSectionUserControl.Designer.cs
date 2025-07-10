namespace Enterprise.Customs.NL.GUI
{
	partial class ExportBottomSectionUserControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.AnnotationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ExitTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ExitDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ExportCustomsOfficeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ExitCustomsOfficeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.AlternativeEvidenceGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AlternativeEvidenceGrid = new Enterprise.ZArchitecture.ZGrid();
			this.AlternativeProofDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ExitTypeDropEdit.SuspendLayout();
			this.ExitDateEdit.SuspendLayout();
			this.ExitCustomsOfficeCodeFindBox.SuspendLayout();
			this.AlternativeEvidenceGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AlternativeEvidenceGrid)).BeginInit();
			this.AlternativeEvidenceGrid.SuspendLayout();
			this.AlternativeProofDetailsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NL.Business.JobDeclarationMessageSendingObjectParent);
			// 
			// AnnotationTextBox
			// 
			this.AnnotationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 18, true);
			this.AnnotationTextBox.Multiline = true;
			this.AnnotationTextBox.Name = "AnnotationTextBox";
			this.AnnotationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(273, 100, true);
			this.AnnotationTextBox.TabIndex = 0;
			// 
			// ExitTypeDropEdit
			// 
			this.ExitTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExitTypeDropEdit, "SendingObjectsCollection.ExitType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NL.Business.JobDeclarationMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.NL.Business.JobDeclarationMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).ExitType)));
			this.ExitTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(418, 21, true);
			this.ExitTypeDropEdit.Name = "ExitTypeDropEdit";
			this.ExitTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(343, 20, true);
			this.ExitTypeDropEdit.TabIndex = 1;
			// 
			// ExitDateEdit
			// 
			this.ExitDateEdit.AllowDrop = true;
			this.ExitDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.ExitDateEdit, "SendingObjectsCollection.ExitDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.NL.Business.JobDeclarationMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.NL.Business.JobDeclarationMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).ExitDate)));
			this.ExitDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(418, 46, true);
			this.ExitDateEdit.Name = "ExitDateEdit";
			this.ExitDateEdit.TabIndex = 2;
			// 
			// ExportCustomsOfficeTextBox
			// 
			this.BindingSource.SetBindingMember(this.ExportCustomsOfficeTextBox, "SendingObjectsCollection.ExportCustomsOffice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NL.Business.JobDeclarationMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.NL.Business.JobDeclarationMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).ExportCustomsOffice)));
			this.ExportCustomsOfficeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(418, 71, true);
			this.ExportCustomsOfficeTextBox.Name = "ExportCustomsOfficeTextBox";
			this.ExportCustomsOfficeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(343, 20, true);
			this.ExportCustomsOfficeTextBox.TabIndex = 3;
			// 
			// ExitCustomsOfficeCodeFindBox
			// 
			this.ExitCustomsOfficeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExitCustomsOfficeCodeFindBox, "SendingObjectsCollection.ExitCustomsOffice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NL.Business.JobDeclarationMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.NL.Business.JobDeclarationMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).ExitCustomsOffice)));
			this.ExitCustomsOfficeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(418, 96, true);
			this.ExitCustomsOfficeCodeFindBox.Name = "ExitCustomsOfficeCodeFindBox";
			this.ExitCustomsOfficeCodeFindBox.ParentType = null;
			this.ExitCustomsOfficeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(343, 20, true);
			this.ExitCustomsOfficeCodeFindBox.TabIndex = 4;
			// 
			// AlternativeEvidenceGroupBox
			// 
			this.AlternativeEvidenceGroupBox.CaptionResourceString = Enterprise.Customs.NL.GUI.Res.GetData("ddbe1ac1-b1ad-44df-bd63-6fdb2bf35463", "Alternative Evidence");
			this.AlternativeEvidenceGroupBox.Controls.Add(this.AlternativeEvidenceGrid);
			this.AlternativeEvidenceGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AlternativeEvidenceGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 120, true);
			this.AlternativeEvidenceGroupBox.Name = "AlternativeEvidenceGroupBox";
			this.AlternativeEvidenceGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(773, 239, true);
			this.AlternativeEvidenceGroupBox.TabIndex = 6;
			this.AlternativeEvidenceGroupBox.TabStop = false;
			// 
			// AlternativeEvidenceGrid
			// 
			this.AlternativeEvidenceGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AlternativeEvidenceGrid, "SendingObjectsCollection.AlternativeEvidences");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.NL.Business.JobDeclarationMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.NL.Business.JobDeclarationMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).AlternativeEvidences)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NL.Business.AlternativeEvidence)(((System.Collections.IList)(((Enterprise.Customs.NL.Business.JobDeclarationMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.NL.Business.JobDeclarationMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).AlternativeEvidences)).SyncRoot)).DocType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NL.Business.AlternativeEvidence)(((System.Collections.IList)(((Enterprise.Customs.NL.Business.JobDeclarationMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.NL.Business.JobDeclarationMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).AlternativeEvidences)).SyncRoot)).DocTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NL.Business.AlternativeEvidence)(((System.Collections.IList)(((Enterprise.Customs.NL.Business.JobDeclarationMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.NL.Business.JobDeclarationMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).AlternativeEvidences)).SyncRoot)).Reference)));
			this.AlternativeEvidenceGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "DocType";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "DocTypeDescription";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo2.ColumnName = "Reference";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.AlternativeEvidenceGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.AlternativeEvidenceGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.AlternativeEvidenceGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.AlternativeEvidenceGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AlternativeEvidenceGrid.GridId = "883610e1-07e2-4e07-afac-79b94552cacf";
			this.AlternativeEvidenceGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AlternativeEvidenceGrid.LayoutKey = "AlternativeEvidenceGrid";
			this.AlternativeEvidenceGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.AlternativeEvidenceGrid.MaximumRows = 9;
			this.AlternativeEvidenceGrid.Name = "AlternativeEvidenceGrid";
			this.AlternativeEvidenceGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(767, 220, true);
			this.AlternativeEvidenceGrid.TabIndex = 0;
			// 
			// AlternativeProofDetailsGroupBox
			// 
			this.AlternativeProofDetailsGroupBox.CaptionResourceString = Enterprise.Customs.NL.GUI.Res.GetData("9126c5d4-626c-4f7e-a693-b42184b678b8", "Alternative proof details");
			this.AlternativeProofDetailsGroupBox.Controls.Add(this.AnnotationTextBox);
			this.AlternativeProofDetailsGroupBox.Controls.Add(this.ExitTypeDropEdit);
			this.AlternativeProofDetailsGroupBox.Controls.Add(this.ExitDateEdit);
			this.AlternativeProofDetailsGroupBox.Controls.Add(this.ExportCustomsOfficeTextBox);
			this.AlternativeProofDetailsGroupBox.Controls.Add(this.ExitCustomsOfficeCodeFindBox);
			this.AlternativeProofDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.AlternativeProofDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AlternativeProofDetailsGroupBox.Name = "AlternativeProofDetailsGroupBox";
			this.AlternativeProofDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(773, 120, true);
			this.AlternativeProofDetailsGroupBox.TabIndex = 6;
			this.AlternativeProofDetailsGroupBox.TabStop = false;
			// 
			// ExportBottomSectionUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AlternativeEvidenceGroupBox);
			this.Controls.Add(this.AlternativeProofDetailsGroupBox);
			this.Name = "ExportBottomSectionUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(773, 359, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ExitTypeDropEdit.ResumeLayout(true);
			this.ExitTypeDropEdit.PerformLayout();
			this.ExitDateEdit.ResumeLayout(true);
			this.ExitDateEdit.PerformLayout();
			this.ExitCustomsOfficeCodeFindBox.ResumeLayout(true);
			this.ExitCustomsOfficeCodeFindBox.PerformLayout();
			this.AlternativeEvidenceGroupBox.ResumeLayout(false);
			this.AlternativeEvidenceGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AlternativeEvidenceGrid)).EndInit();
			this.AlternativeEvidenceGrid.ResumeLayout(false);
			this.AlternativeEvidenceGrid.PerformLayout();
			this.AlternativeProofDetailsGroupBox.ResumeLayout(false);
			this.AlternativeProofDetailsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		ZArchitecture.ZTextBox AnnotationTextBox;
		ZArchitecture.GUI.ZDropEdit ExitTypeDropEdit;
		ZArchitecture.GUI.ZDateEdit ExitDateEdit;
		ZArchitecture.ZTextBox ExportCustomsOfficeTextBox;
		ZArchitecture.GUI.ZCodeFindBox ExitCustomsOfficeCodeFindBox;
		ZArchitecture.GUI.ZGroupBox AlternativeEvidenceGroupBox;
		ZArchitecture.ZGrid AlternativeEvidenceGrid;
		ZArchitecture.GUI.ZGroupBox AlternativeProofDetailsGroupBox;
	}
}
