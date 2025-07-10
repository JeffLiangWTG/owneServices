namespace Enterprise.Customs.TW.GUI
{
	partial class AdditionalDocumentForm
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
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.EDocsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SupportingDocumentsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.ValidationErrorsGroupBox.SuspendLayout();
			this.AdditionalWarningsGroupBox.SuspendLayout();
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
			this.ValidationErrorsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 237, true);
			this.ValidationErrorsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(763, 131, true);
			this.ValidationErrorsGroupBox.TabIndex = 5;
			// 
			// ValidationErrorsTextBox
			// 
			this.ValidationErrorsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(759, 114, true);
			// 
			// AdditionalWarningsGroupBox
			// 
			this.AdditionalWarningsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 372, true);
			this.AdditionalWarningsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(763, 130, true);
			this.AdditionalWarningsGroupBox.TabIndex = 6;
			// 
			// AdditionalWarningsTextBox
			// 
			this.AdditionalWarningsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(759, 113, true);
			// 
			// ContinueToSendCheckBox
			// 
			this.ContinueToSendCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(14, 514, true);
			// 
			// SendButton
			// 
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(589, 550, true);
			// 
			// CancelButton2
			// 
			this.CancelButton2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(685, 550, true);
			// 
			// messageSendingObjectsGroupBox
			//
			this.messageSendingObjectsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.messageSendingObjectsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(765, 63, true);
			// 
			// MessageSendingObjectsGrid
			// 
			this.MessageSendingObjectsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.MessageSendingObjectsGrid.Dock = System.Windows.Forms.DockStyle.None;
			this.MessageSendingObjectsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.MessageSendingObjectsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(759, 43, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 582, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(785, 23, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TW.Business.AdditionalDocumentMessageSendingObjectParent);
			// 
			// EDocsGroupBox
			// 
			this.EDocsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.EDocsGroupBox.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("B0AA5A7E-7713-44D7-9544-FCED674F2F1A", "eDocs to be sent");
			this.EDocsGroupBox.Controls.Add(this.SupportingDocumentsGrid);
			this.EDocsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 79, true);
			this.EDocsGroupBox.Name = "EDocsGroupBox";
			this.EDocsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(765, 145, true);
			this.EDocsGroupBox.TabIndex = 4;
			this.EDocsGroupBox.TabStop = false;
			// 
			// SupportingDocumentsGrid
			// 
			this.SupportingDocumentsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.SupportingDocumentsGrid, "SendingObjectsCollection.SupportingDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TW.Business.AdditionalDocumentMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.AdditionalDocumentMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).SupportingDocuments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.TW.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.AdditionalDocumentMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.AdditionalDocumentMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).SupportingDocuments)).SyncRoot)).EDoc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TW.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.AdditionalDocumentMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.AdditionalDocumentMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).SupportingDocuments)).SyncRoot)).StorageDocs)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.AdditionalDocumentMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.AdditionalDocumentMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).SupportingDocuments)).SyncRoot)).DocumentNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.AdditionalDocumentMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.AdditionalDocumentMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).SupportingDocuments)).SyncRoot)).Remarks)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TW.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.AdditionalDocumentMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.AdditionalDocumentMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).SupportingDocuments)).SyncRoot)).ControllingAgency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TW.Business.SupportingDocument)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.AdditionalDocumentMessageSendingObject)(((System.Collections.IList)(((Enterprise.Customs.TW.Business.AdditionalDocumentMessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).SupportingDocuments)).SyncRoot)).ControllingAgencyList)));
			this.SupportingDocumentsGrid.CaptionVisible = false;
			zGuidDropEditColumnStyleInfo1.BindToList = "StorageDocs";
			zGuidDropEditColumnStyleInfo1.ColumnName = "EDoc";
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo1.ColumnName = "DocumentNo";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.ColumnName = "Remarks";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo1.BindToList = "ControllingAgencyList";
			zDropEditColumnStyleInfo1.ColumnName = "ControllingAgency";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.SupportingDocumentsGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.SupportingDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.SupportingDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.SupportingDocumentsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.SupportingDocumentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SupportingDocumentsGrid.GridId = "52bafb3e-e070-4614-95ce-272d80314fce";
			this.SupportingDocumentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SupportingDocumentsGrid.LayoutKey = "MessageSendingObjectsGrid";
			this.SupportingDocumentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.SupportingDocumentsGrid.Name = "SupportingDocumentsGrid";
			this.SupportingDocumentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(761, 128, true);
			this.SupportingDocumentsGrid.TabIndex = 1;
			// 
			// AdditionalDocumentForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(785, 605, true);
			this.Controls.Add(this.EDocsGroupBox);
			this.DataSourceType = typeof(Enterprise.Customs.TW.Business.AdditionalDocumentMessageSendingObjectParent);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 610, true);
			this.Name = "AdditionalDocumentForm";
			this.Controls.SetChildIndex(this.ValidationErrorsGroupBox, 0);
			this.Controls.SetChildIndex(this.AdditionalWarningsGroupBox, 0);
			this.Controls.SetChildIndex(this.ContinueToSendCheckBox, 0);
			this.Controls.SetChildIndex(this.SendButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CancelButton2, 0);
			this.Controls.SetChildIndex(this.messageSendingObjectsGroupBox, 0);
			this.Controls.SetChildIndex(this.EDocsGroupBox, 0);
			this.ValidationErrorsGroupBox.ResumeLayout(false);
			this.ValidationErrorsGroupBox.PerformLayout();
			this.AdditionalWarningsGroupBox.ResumeLayout(false);
			this.AdditionalWarningsGroupBox.PerformLayout();
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
