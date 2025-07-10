using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class DocumentTrackingBulkUpdateForm : ZForm
	{
		ZDateEdit SentToBrokerDateEdit;
		ZGroupBox SelectedDocumentsGroupBox;
		ZArchitecture.ZGrid SelectedDocumentsGrid;
		ZGroupBox DetailsToUpdateGroupBox;
		ZDateEdit ReceivedFromBrokerDateEdit;
		ZDateEdit ReturnedToShipperDateEdit;
		ZButton AttachButton;
		ZButton DetachButton;
		new ZButton CancelButton;
		ZButton UpdateButton;
		ZDateTimeOffsetEdit DateReceivedDateTimeOffsetEdit;

		protected new void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo zDateTimeOffsetEditColumnStyleInfo1 = new ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			this.SentToBrokerDateEdit = new ZDateEdit();
			this.SelectedDocumentsGroupBox = new ZGroupBox();
			this.AttachButton = new ZButton();
			this.SelectedDocumentsGrid = new ZArchitecture.ZGrid();
			this.DetachButton = new ZButton();
			this.DetailsToUpdateGroupBox = new ZGroupBox();
			this.DateReceivedDateTimeOffsetEdit = new ZDateTimeOffsetEdit();
			this.ReturnedToShipperDateEdit = new ZDateEdit();
			this.ReceivedFromBrokerDateEdit = new ZDateEdit();
			this.CancelButton = new ZButton();
			this.UpdateButton = new ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SelectedDocumentsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SelectedDocumentsGrid)).BeginInit();
			this.DetailsToUpdateGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 263, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(536, 22, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(260);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(261);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(DocumentTrackingBulkUpdateBusinessObject);
			// 
			// SentToBrokerDateEdit
			// 
			this.SentToBrokerDateEdit.AutoCompleteMonthThreshold = 1;
			this.SentToBrokerDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.SentToBrokerDateEdit, "EQ_SntToCustomsBroker");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((DocumentTrackingBulkUpdateBusinessObject)(null)).EQ_SntToCustomsBroker)));
			this.SentToBrokerDateEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentTrackingBulkUpdateForm|6d2bcc33-28d2-4349-8552-d815cd8e9de5", "Sent to Broker");
			this.SentToBrokerDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.SentToBrokerDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 37, true);
			this.SentToBrokerDateEdit.Name = "SentToBrokerDateEdit";
			this.SentToBrokerDateEdit.TabIndex = 3;
			// 
			// SelectedDocumentsGroupBox
			// 
			this.SelectedDocumentsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.SelectedDocumentsGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentTrackingBulkUpdateForm|57f427f2-17a6-4444-960f-415e4dfa8367", "Select Documents to Update");
			this.SelectedDocumentsGroupBox.Controls.Add(this.AttachButton);
			this.SelectedDocumentsGroupBox.Controls.Add(this.SelectedDocumentsGrid);
			this.SelectedDocumentsGroupBox.Controls.Add(this.DetachButton);
			this.SelectedDocumentsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 7, true);
			this.SelectedDocumentsGroupBox.Name = "SelectedDocumentsGroupBox";
			this.SelectedDocumentsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(256, 218, true);
			this.SelectedDocumentsGroupBox.TabIndex = 0;
			this.SelectedDocumentsGroupBox.TabStop = false;
			// 
			// AttachButton
			// 
			this.AttachButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.AttachButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentTrackingBulkUpdateForm|d6c2103e-6bec-4ae0-8781-2d1ab714db64", "Attach");
			this.AttachButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 187, true);
			this.AttachButton.Name = "AttachButton";
			this.AttachButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 22, true);
			this.AttachButton.TabIndex = 1;
			this.AttachButton.Click += new EventHandler(this.AttachButton_Click);
			// 
			// SelectedDocumentsGrid
			// 
			this.SelectedDocumentsGrid.AllowNavigation = false;
			this.SelectedDocumentsGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.SelectedDocumentsGrid, "SelectedDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((DocumentTrackingBulkUpdateBusinessObject)(null)).SelectedDocuments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((RequiredDocToBulkUpdate)(((System.Collections.IList)(((DocumentTrackingBulkUpdateBusinessObject)(null)).SelectedDocuments)).SyncRoot)).DocumentType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((RequiredDocToBulkUpdate)(((System.Collections.IList)(((DocumentTrackingBulkUpdateBusinessObject)(null)).SelectedDocuments)).SyncRoot)).DocumentNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((RequiredDocToBulkUpdate)(((System.Collections.IList)(((DocumentTrackingBulkUpdateBusinessObject)(null)).SelectedDocuments)).SyncRoot)).DocumentParentID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTimeOffset)(((RequiredDocToBulkUpdate)(((System.Collections.IList)(((DocumentTrackingBulkUpdateBusinessObject)(null)).SelectedDocuments)).SyncRoot)).DateReceived)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((RequiredDocToBulkUpdate)(((System.Collections.IList)(((DocumentTrackingBulkUpdateBusinessObject)(null)).SelectedDocuments)).SyncRoot)).DateSentToBroker)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((RequiredDocToBulkUpdate)(((System.Collections.IList)(((DocumentTrackingBulkUpdateBusinessObject)(null)).SelectedDocuments)).SyncRoot)).DateReceivedFromBroker)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((RequiredDocToBulkUpdate)(((System.Collections.IList)(((DocumentTrackingBulkUpdateBusinessObject)(null)).SelectedDocuments)).SyncRoot)).DateReturnedToShipper)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((RequiredDocToBulkUpdate)(((System.Collections.IList)(((DocumentTrackingBulkUpdateBusinessObject)(null)).SelectedDocuments)).SyncRoot)).DocumentOwner)));
			this.SelectedDocumentsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentTrackingBulkUpdateForm|29c40048-1cb5-4a87-b490-100831b1bf2e", "Doc Type");
			zTextBoxColumnStyleInfo1.ColumnName = "DocumentType";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentTrackingBulkUpdateForm|a9a64430-f3c0-4a82-a6ac-2e25b367f396", "Doc Number");
			zTextBoxColumnStyleInfo2.ColumnName = "DocumentNumber";
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentTrackingBulkUpdateForm|1e7a04a4-9611-47c4-8990-97a521e71f1e", "Shipment ID");
			zTextBoxColumnStyleInfo3.ColumnName = "DocumentParentID";
			zDateTimeOffsetEditColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentTrackingBulkUpdateForm|5103c5d6-6f26-489e-8b13-35f8eabc2b3f", "Received Date");
			zDateTimeOffsetEditColumnStyleInfo1.ColumnName = "DateReceived";
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentTrackingBulkUpdateForm|7831f81c-1774-45d1-87a1-dbdd1a2d21e3", "Sent To Broker Date");
			zDateEditColumnStyleInfo2.ColumnName = "DateSentToBroker";
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentTrackingBulkUpdateForm|66e096d6-38dc-4ad2-8665-1a5d251c6a3d", "Rcd. From Broker Date", "Received From Broker Date");
			zDateEditColumnStyleInfo3.ColumnName = "DateReceivedFromBroker";
			zDateEditColumnStyleInfo4.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentTrackingBulkUpdateForm|8ee2f364-f5ab-4088-993d-5ecae6ea9d22", "Return to Shipper Date");
			zDateEditColumnStyleInfo4.ColumnName = "DateReturnedToShipper";
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentTrackingBulkUpdateForm|e31fb86f-b4c8-495d-bfc4-50d67d917005", "Doc Owner");
			zTextBoxColumnStyleInfo4.ColumnName = "DocumentOwner";
			this.SelectedDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.SelectedDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.SelectedDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.SelectedDocumentsGrid.ColumnStyles.Add(zDateTimeOffsetEditColumnStyleInfo1);
			this.SelectedDocumentsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.SelectedDocumentsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.SelectedDocumentsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.SelectedDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.SelectedDocumentsGrid.GridId = "fc28043a-4826-4d8f-b230-25bbddd2e9e2";
			this.SelectedDocumentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SelectedDocumentsGrid.LayoutKey = "SelectedDocumentsGrid";
			this.SelectedDocumentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 15, true);
			this.SelectedDocumentsGrid.Name = "SelectedDocumentsGrid";
			this.SelectedDocumentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 165, true);
			this.SelectedDocumentsGrid.TabIndex = 0;
			// 
			// DetachButton
			// 
			this.DetachButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.DetachButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentTrackingBulkUpdateForm|d81a6491-939f-414a-b036-6807ccd74e66", "Detach");
			this.DetachButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 187, true);
			this.DetachButton.Name = "DetachButton";
			this.DetachButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 22, true);
			this.DetachButton.TabIndex = 2;
			this.DetachButton.Click += new EventHandler(this.DetachButton_Click);
			// 
			// DetailsToUpdateGroupBox
			// 
			this.DetailsToUpdateGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.DetailsToUpdateGroupBox.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentTrackingBulkUpdateForm|4bb91eca-1af1-4a81-ace9-df99321faaf8", "Details to Update");
			this.DetailsToUpdateGroupBox.Controls.Add(this.DateReceivedDateTimeOffsetEdit);
			this.DetailsToUpdateGroupBox.Controls.Add(this.ReturnedToShipperDateEdit);
			this.DetailsToUpdateGroupBox.Controls.Add(this.ReceivedFromBrokerDateEdit);
			this.DetailsToUpdateGroupBox.Controls.Add(this.SentToBrokerDateEdit);
			this.DetailsToUpdateGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(264, 7, true);
			this.DetailsToUpdateGroupBox.Name = "DetailsToUpdateGroupBox";
			this.DetailsToUpdateGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 218, true);
			this.DetailsToUpdateGroupBox.TabIndex = 1;
			this.DetailsToUpdateGroupBox.TabStop = false;
			// 
			// DateReceivedDateEdit
			// 
			this.DateReceivedDateTimeOffsetEdit.AutoCompleteMonthThreshold = 1;
			this.DateReceivedDateTimeOffsetEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DateReceivedDateTimeOffsetEdit, "EQ_DateReceived");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((DocumentTrackingBulkUpdateBusinessObject)(null)).EQ_DateReceived)));
			this.DateReceivedDateTimeOffsetEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentTrackingBulkUpdateForm|03899ca7-8c28-4b1c-8a25-90cbbbfc30f6", "Received from Shipper");
			this.DateReceivedDateTimeOffsetEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.DateReceivedDateTimeOffsetEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 15, true);
			this.DateReceivedDateTimeOffsetEdit.Name = "DateReceivedDateEdit";
			this.DateReceivedDateTimeOffsetEdit.TabIndex = 1;
			// 
			// ReturnedToShipperDateEdit
			// 
			this.ReturnedToShipperDateEdit.AutoCompleteMonthThreshold = 1;
			this.ReturnedToShipperDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ReturnedToShipperDateEdit, "EQ_ReturnToShipper");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((DocumentTrackingBulkUpdateBusinessObject)(null)).EQ_ReturnToShipper)));
			this.ReturnedToShipperDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ReturnedToShipperDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 82, true);
			this.ReturnedToShipperDateEdit.Name = "ReturnedToShipperDateEdit";
			this.ReturnedToShipperDateEdit.TabIndex = 7;
			// 
			// ReceivedFromBrokerDateEdit
			// 
			this.ReceivedFromBrokerDateEdit.AutoCompleteMonthThreshold = 1;
			this.ReceivedFromBrokerDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ReceivedFromBrokerDateEdit, "EQ_RcvFromCustomsBroker");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((DocumentTrackingBulkUpdateBusinessObject)(null)).EQ_RcvFromCustomsBroker)));
			this.ReceivedFromBrokerDateEdit.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentTrackingBulkUpdateForm|8b341c01-84eb-49ff-ba3e-b015fe1aa65c", "Received from Broker");
			this.ReceivedFromBrokerDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ReceivedFromBrokerDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 59, true);
			this.ReceivedFromBrokerDateEdit.Name = "ReceivedFromBrokerDateEdit";
			this.ReceivedFromBrokerDateEdit.TabIndex = 5;
			// 
			// CancelButton
			// 
			this.CancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentTrackingBulkUpdateForm|157c3bf7-2ec0-410b-87a2-33631c08a0da", "Cancel");
			this.CancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(440, 232, true);
			this.CancelButton.Name = "CancelButton";
			this.CancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.CancelButton.TabIndex = 3;
			// 
			// UpdateButton
			// 
			this.UpdateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.UpdateButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentTrackingBulkUpdateForm|5db362d9-1816-4168-9b4c-a9b677478a74", "Update");
			this.UpdateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(360, 232, true);
			this.UpdateButton.Name = "UpdateButton";
			this.UpdateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.UpdateButton.TabIndex = 2;
			// 
			// DocumentTrackingBulkUpdateForm
			// 

			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(536, 285, true);
			this.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("DocumentTrackingBulkUpdateForm|7ceab69d-6c45-4580-a5ba-129164bde4b3", "Document Bulk Update");
			this.Controls.Add(this.DetailsToUpdateGroupBox);
			this.Controls.Add(this.SelectedDocumentsGroupBox);
			this.Controls.Add(this.CancelButton);
			this.Controls.Add(this.UpdateButton);
			this.DataSourceAssemblyName = "Enterprise.MasterFiles.Business";
			this.DataSourceType = typeof(DocumentTrackingBulkUpdateBusinessObject);
			this.DataSourceTypeName = "Enterprise.MasterFiles.Business.DocumentTrackingBulkUpdateBusinessObject";
			this.Name = "DocumentTrackingBulkUpdateForm";
			this.Controls.SetChildIndex(this.UpdateButton, 0);
			this.Controls.SetChildIndex(this.CancelButton, 0);
			this.Controls.SetChildIndex(this.SelectedDocumentsGroupBox, 0);
			this.Controls.SetChildIndex(this.DetailsToUpdateGroupBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SelectedDocumentsGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.SelectedDocumentsGrid)).EndInit();
			this.DetailsToUpdateGroupBox.ResumeLayout(false);
			this.ResumeLayout(false);
		}

		System.ComponentModel.IContainer components = null;
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
