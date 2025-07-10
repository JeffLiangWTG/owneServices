namespace Enterprise.Customs.PL.GUI
{
	partial class MessageSendingEDocsUserControl
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
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			this.EDocsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SupportingDocumentsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.UserInputGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RefMrnNumberLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CustomsOfficeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.PurposeOfSendingDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ProcedureDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RefNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MrnNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CommentsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RefMRNNumberSeparatorLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.EDocsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SupportingDocumentsGrid)).BeginInit();
			this.SupportingDocumentsGrid.SuspendLayout();
			this.UserInputGroupBox.SuspendLayout();
			this.CustomsOfficeFindBox.SuspendLayout();
			this.PurposeOfSendingDropEdit.SuspendLayout();
			this.ProcedureDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.PL.Business.BaseMessageSendingObjectParent);
			// 
			// EDocsGroupBox
			// 
			this.EDocsGroupBox.CaptionResourceString = Enterprise.Customs.PL.GUI.Res.GetData("PLMessageSendingEDocsUserControl|EDocsGroupBox", "eDocs");
			this.EDocsGroupBox.Controls.Add(this.SupportingDocumentsGrid);
			this.EDocsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.EDocsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.EDocsGroupBox.Name = "EDocsGroupBox";
			this.EDocsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(841, 105, true);
			this.EDocsGroupBox.TabIndex = 2;
			this.EDocsGroupBox.TabStop = false;
			// 
			// SupportingDocumentsGrid
			// 
			this.SupportingDocumentsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.SupportingDocumentsGrid, "EDocs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.PL.Business.BaseMessageSendingObjectParent)(null)).EDocs)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.PL.Business.JobDeclarationMessageSendingEDocs)(((System.Collections.IList)(((Enterprise.Customs.PL.Business.BaseMessageSendingObjectParent)(null)).EDocs)).SyncRoot)).EDoc)));
			this.SupportingDocumentsGrid.CaptionVisible = false;
			zGuidDropEditColumnStyleInfo1.ColumnName = "EDoc";
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.SupportingDocumentsGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.SupportingDocumentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SupportingDocumentsGrid.GridId = "5F2ABBEC-D1C5-413F-91CA-4E9C5AEDFB85";
			this.SupportingDocumentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SupportingDocumentsGrid.LayoutKey = "SupportingDocumentsGrid";
			this.SupportingDocumentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 14, true);
			this.SupportingDocumentsGrid.Name = "SupportingDocumentsGrid";
			this.SupportingDocumentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(838, 90, true);
			this.SupportingDocumentsGrid.TabIndex = 1;
			// 
			// UserInputGroupBox
			// 
			this.UserInputGroupBox.Controls.Add(this.RefMrnNumberLabel);
			this.UserInputGroupBox.Controls.Add(this.CustomsOfficeFindBox);
			this.UserInputGroupBox.Controls.Add(this.PurposeOfSendingDropEdit);
			this.UserInputGroupBox.Controls.Add(this.ProcedureDropEdit);
			this.UserInputGroupBox.Controls.Add(this.RefNumberTextBox);
			this.UserInputGroupBox.Controls.Add(this.MrnNumberTextBox);
			this.UserInputGroupBox.Controls.Add(this.CommentsTextBox);
			this.UserInputGroupBox.Controls.Add(this.RefMRNNumberSeparatorLabel);
			this.UserInputGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.UserInputGroupBox, false);
			this.UserInputGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 105, true);
			this.UserInputGroupBox.Name = "UserInputGroupBox";
			this.UserInputGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(841, 81, true);
			this.UserInputGroupBox.TabIndex = 3;
			this.UserInputGroupBox.TabStop = false;
			// 
			// RefMrnNumberLabel
			// 
			this.RefMrnNumberLabel.CaptionResourceString = Enterprise.Customs.PL.GUI.Res.GetData("46cb5857-11ac-42a5-9854-0c6b6b6488fa", "Ref/MRN No.");
			this.RefMrnNumberLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.RefMrnNumberLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 34, true);
			this.RefMrnNumberLabel.Name = "RefMrnNumberLabel";
			this.RefMrnNumberLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(91, 20, true);
			this.RefMrnNumberLabel.TabIndex = 2;
			this.RefMrnNumberLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// CustomsOfficeFindBox
			// 
			this.CustomsOfficeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsOfficeFindBox, "CustomsOffice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.PL.Business.BaseMessageSendingObjectParent)(null)).CustomsOffice)));
			this.CustomsOfficeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 11, true);
			this.CustomsOfficeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			this.CustomsOfficeFindBox.Name = "CustomsOfficeFindBox";
			this.CustomsOfficeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			this.CustomsOfficeFindBox.ParentType = null;
			this.CustomsOfficeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(379, 15, true);
			this.CustomsOfficeFindBox.TabIndex = 0;
			// 
			// PurposeOfSendingDropEdit
			// 
			this.PurposeOfSendingDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PurposeOfSendingDropEdit, "PurposeOfSending");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.PL.Business.BaseMessageSendingObjectParent)(null)).PurposeOfSending)));
			this.PurposeOfSendingDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(613, 11, true);
			this.PurposeOfSendingDropEdit.Name = "PurposeOfSendingDropEdit";
			this.PurposeOfSendingDropEdit.PreBoundMaxLength = 29;
			this.PurposeOfSendingDropEdit.ShouldResizeByMaxLength = false;
			this.PurposeOfSendingDropEdit.ShowDescriptionBox = false;
			this.PurposeOfSendingDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(456, 15, true);
			this.PurposeOfSendingDropEdit.TabIndex = 1;
			// 
			// ProcedureDropEdit
			// 
			this.ProcedureDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ProcedureDropEdit, "Procedure");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.PL.Business.BaseMessageSendingObjectParent)(null)).Procedure)));
			this.ProcedureDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(613, 34, true);
			this.ProcedureDropEdit.Name = "ProcedureDropEdit";
			this.ProcedureDropEdit.PreBoundMaxLength = 29;
			this.ProcedureDropEdit.ShouldResizeByMaxLength = false;
			this.ProcedureDropEdit.ShowDescriptionBox = false;
			this.ProcedureDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(456, 15, true);
			this.ProcedureDropEdit.TabIndex = 6;
			// 
			// RefNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.RefNumberTextBox, "RefNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.PL.Business.BaseMessageSendingObjectParent)(null)).RefNumber)));
			this.RefNumberTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.RefNumberTextBox, false);
			this.RefNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 34, true);
			this.RefNumberTextBox.Name = "RefNumberTextBox";
			this.RefNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(182, 15, true);
			this.RefNumberTextBox.TabIndex = 3;
			// 
			// MrnNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.MrnNumberTextBox, "MrnNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.PL.Business.BaseMessageSendingObjectParent)(null)).MrnNumber)));
			this.MrnNumberTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.MrnNumberTextBox, false);
			this.MrnNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(299, 34, true);
			this.MrnNumberTextBox.Name = "MrnNumberTextBox";
			this.MrnNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(182, 15, true);
			this.MrnNumberTextBox.TabIndex = 5;
			// 
			// CommentsTextBox
			// 
			this.BindingSource.SetBindingMember(this.CommentsTextBox, "Comments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.PL.Business.BaseMessageSendingObjectParent)(null)).Comments)));
			this.CommentsTextBox.CaptionResourceString = null;
			this.CommentsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(102, 56, true);
			this.CommentsTextBox.Name = "CommentsTextBox";
			this.CommentsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(379, 15, true);
			this.CommentsTextBox.TabIndex = 7;
			// 
			// RefMRNNumberSeparatorLabel
			// 
			this.RefMRNNumberSeparatorLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.RefMRNNumberSeparatorLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(287, 34, true);
			this.RefMRNNumberSeparatorLabel.Name = "RefMRNNumberSeparatorLabel";
			this.RefMRNNumberSeparatorLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(13, 18, true);
			this.RefMRNNumberSeparatorLabel.TabIndex = 4;
			this.RefMRNNumberSeparatorLabel.Text = "/";
			// 
			// MessageSendingEDocsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.UserInputGroupBox);
			this.Controls.Add(this.EDocsGroupBox);
			this.Name = "MessageSendingEDocsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(841, 206, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.EDocsGroupBox.ResumeLayout(false);
			this.EDocsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SupportingDocumentsGrid)).EndInit();
			this.SupportingDocumentsGrid.ResumeLayout(false);
			this.SupportingDocumentsGrid.PerformLayout();
			this.UserInputGroupBox.ResumeLayout(false);
			this.UserInputGroupBox.PerformLayout();
			this.CustomsOfficeFindBox.ResumeLayout(true);
			this.CustomsOfficeFindBox.PerformLayout();
			this.PurposeOfSendingDropEdit.ResumeLayout(true);
			this.PurposeOfSendingDropEdit.PerformLayout();
			this.ProcedureDropEdit.ResumeLayout(true);
			this.ProcedureDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZGroupBox EDocsGroupBox;
		internal ZArchitecture.ZGrid SupportingDocumentsGrid;
		internal ZArchitecture.GUI.ZGroupBox UserInputGroupBox;
		internal ZArchitecture.ZLabel RefMrnNumberLabel;
		internal ZArchitecture.GUI.ZCodeFindBox CustomsOfficeFindBox;
		internal ZArchitecture.GUI.ZDropEdit PurposeOfSendingDropEdit;
		internal ZArchitecture.GUI.ZDropEdit ProcedureDropEdit;
		internal ZArchitecture.ZTextBox RefNumberTextBox;
		internal ZArchitecture.ZTextBox MrnNumberTextBox;
		internal ZArchitecture.ZTextBox CommentsTextBox;
		internal ZArchitecture.ZLabel RefMRNNumberSeparatorLabel;
	}
}
