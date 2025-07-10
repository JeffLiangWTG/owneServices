namespace Enterprise.Customs.TW.BriefCustomsDeclaration.GUI
{
	partial class SendToCustomsMessageSendingForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.EDocsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SupportingDocumentsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ValidationErrorsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).BeginInit();
			this.SplitContainer.Panel1.SuspendLayout();
			this.SplitContainer.Panel2.SuspendLayout();
			this.SplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.WarningSplitContainer)).BeginInit();
			this.WarningSplitContainer.Panel1.SuspendLayout();
			this.WarningSplitContainer.SuspendLayout();
			this.messageSendingObjectsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageSendingObjectsGrid)).BeginInit();
			this.MessageSendingObjectsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.EDocsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SupportingDocumentsGrid)).BeginInit();
			this.SupportingDocumentsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// ValidationErrorsGroupBox
			// 
			this.ValidationErrorsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(786, 140, true);
			// 
			// SendWithValidationErrorsCheckBox
			// 
			this.SendWithValidationErrorsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 505, true);
			// 
			// SendWithAdditionalWarningCheckBox
			// 
			this.SendWithAdditionalWarningCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 525, true);
			// 
			// PreviewMessageCheckBox
			// 
			this.PreviewMessageCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 545, true);
			// 
			// SplitContainer
			// 
			// 
			// SplitContainer.Panel1
			// 
			this.SplitContainer.Panel1.Controls.Add(this.EDocsGroupBox);
			this.SplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(786, 499, true);
			this.SplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(178);
			// 
			// WarningSplitContainer
			// 
			this.WarningSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(786, 317, true);
			this.WarningSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(140);
			// 
			// SendButton
			// 
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(590, 551, true);
			// 
			// CancelButton2
			// 
			this.CancelButton2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(686, 551, true);
			// 
			// messageSendingObjectsGroupBox
			// 
			this.messageSendingObjectsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.messageSendingObjectsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(788, 80, true);
			// 
			// MessageSendingObjectsGrid
			// 
			this.MessageSendingObjectsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(785, 65, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 582, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(788, 23, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TW.BriefCustomsDeclaration.Business.MessageSendingObjectParent);
			// 
			// EDocsGroupBox
			// 
			this.EDocsGroupBox.CaptionResourceString = Enterprise.Customs.TW.BriefCustomsDeclaration.GUI.Res.GetData("83751E18-4E28-4D06-9789-FBFD0366D81B", "eDocs to be sent");
			this.EDocsGroupBox.Controls.Add(this.SupportingDocumentsGrid);
			this.EDocsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 83, true);
			this.EDocsGroupBox.Name = "EDocsGroupBox";
			this.EDocsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(784, 94, true);
			this.EDocsGroupBox.TabIndex = 5;
			this.EDocsGroupBox.TabStop = false;
			// 
			// SupportingDocumentsGrid
			// 
			this.SupportingDocumentsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.SupportingDocumentsGrid, "SendingObjectsCollection.SupportingDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.MessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.MessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).SupportingDocuments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.MessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.MessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).SupportingDocuments)).SyncRoot)).BillNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.MessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.MessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).SupportingDocuments)).SyncRoot)).BillNumberList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.MessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.MessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).SupportingDocuments)).SyncRoot)).EDoc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.MessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.MessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).SupportingDocuments)).SyncRoot)).StorageDocs)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.MessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.MessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).SupportingDocuments)).SyncRoot)).DocumentNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.MessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.MessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).SupportingDocuments)).SyncRoot)).Remarks)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.MessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.MessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).SupportingDocuments)).SyncRoot)).ControllingAgency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.MessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.MessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).SupportingDocuments)).SyncRoot)).ControllingAgencyList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.MessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.TW.BriefCustomsDeclaration.Business.MessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).SupportingDocuments)).SyncRoot)).Type)));
			this.SupportingDocumentsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.BindToList = "BillNumberList";
			zDropEditColumnStyleInfo1.ColumnName = "BillNumber";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zGuidDropEditColumnStyleInfo1.BindToList = "StorageDocs";
			zGuidDropEditColumnStyleInfo1.ColumnName = "EDoc";
			zGuidDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "DocumentNo";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "Remarks";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo2.BindToList = "ControllingAgencyList";
			zDropEditColumnStyleInfo2.ColumnName = "ControllingAgency";
			zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo3.ColumnName = "Type";
			zDropEditColumnStyleInfo3.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.SupportingDocumentsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.SupportingDocumentsGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.SupportingDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.SupportingDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.SupportingDocumentsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.SupportingDocumentsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.SupportingDocumentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SupportingDocumentsGrid.GridId = "52bafb3e-e070-4614-95ce-272d80314fce";
			this.SupportingDocumentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SupportingDocumentsGrid.LayoutKey = "MessageSendingObjectsGrid";
			this.SupportingDocumentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 14, true);
			this.SupportingDocumentsGrid.Name = "SupportingDocumentsGrid";
			this.SupportingDocumentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(782, 79, true);
			this.SupportingDocumentsGrid.TabIndex = 2;
			// 
			// SendToCustomsMessageSendingForm1
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(788, 605, true);
			this.DataSourceType = typeof(Enterprise.Customs.TW.BriefCustomsDeclaration.Business.MessageSendingObjectParent);
			this.Name = "SendToCustomsMessageSendingForm1";
			this.ValidationErrorsGroupBox.ResumeLayout(false);
			this.ValidationErrorsGroupBox.PerformLayout();
			this.SplitContainer.Panel1.ResumeLayout(false);
			this.SplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SplitContainer)).EndInit();
			this.SplitContainer.ResumeLayout(false);
			this.SplitContainer.PerformLayout();
			this.WarningSplitContainer.Panel1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.WarningSplitContainer)).EndInit();
			this.WarningSplitContainer.ResumeLayout(false);
			this.WarningSplitContainer.PerformLayout();
			this.messageSendingObjectsGroupBox.ResumeLayout(false);
			this.messageSendingObjectsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageSendingObjectsGrid)).EndInit();
			this.MessageSendingObjectsGrid.ResumeLayout(false);
			this.MessageSendingObjectsGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.EDocsGroupBox.ResumeLayout(false);
			this.EDocsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SupportingDocumentsGrid)).EndInit();
			this.SupportingDocumentsGrid.ResumeLayout(false);
			this.SupportingDocumentsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected ZArchitecture.GUI.ZGroupBox EDocsGroupBox;
		protected ZArchitecture.ZGrid SupportingDocumentsGrid;
	}
}
