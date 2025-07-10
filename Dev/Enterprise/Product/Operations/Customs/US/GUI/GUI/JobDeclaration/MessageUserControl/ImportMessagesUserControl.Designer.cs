using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CargoWise.Windows.UI;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using ErrorsRecord = Enterprise.Customs.US.Business.ErrorsRecord;

namespace Enterprise.Customs.US.GUI
{
	public partial class ImportMessagesUserControl
	{

		KSplitter inBondHeadersHistorySplitter;
		ZTabPage messageDetailsTabPage;
		ZGroupBox relatedRecordsGroupBox;
		internal ZGrid InBondHeadersGrid;
		ZTabPage statusErrorsTabPage;
		public ZLabel StatusesErrorsLabel;
		internal MessagesStatusErrorsUserControl messagesStatusErrorsUserControl;
		Enterprise.Messaging.GUI.HtmlInterpretationBox messageInterpretationBox;
		ZTextBox messageDetailsTextBox;
		ZPanel messagesStatusErrorsPanel;

		void InitializeComponent()
		{
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new ZTextBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZDateEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new ZDateEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			this.inBondHeadersHistorySplitter = new KSplitter();
			this.messageDetailsTabPage = new ZTabPage();
			this.messageInterpretationBox = new Enterprise.Messaging.GUI.HtmlInterpretationBox();
			this.messageDetailsTextBox = new ZTextBox();
			this.relatedRecordsGroupBox = new ZGroupBox();
			this.InBondHeadersGrid = new ZGrid();
			this.statusErrorsTabPage = new ZTabPage();
			this.messagesStatusErrorsUserControl = new MessagesStatusErrorsUserControl();
			this.messagesStatusErrorsPanel = new ZPanel();
			this.StatusesErrorsLabel = new ZLabel();
			this.MainPanel.SuspendLayout();
			this.MessagesTabControl.SuspendLayout();
			this.MessageTextTabPage.SuspendLayout();
			this.HistoryGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessagesGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.messageDetailsTabPage.SuspendLayout();
			this.relatedRecordsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.InBondHeadersGrid)).BeginInit();
			this.statusErrorsTabPage.SuspendLayout();
			this.messagesStatusErrorsPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainPanel
			// 
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 186, true);
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(976, 460, true);
			// 
			// MessagesTabControl
			// 
			this.MessagesTabControl.Controls.Add(this.messageDetailsTabPage);
			this.MessagesTabControl.Controls.Add(this.statusErrorsTabPage);
			this.MessagesTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(414, 460, true);
			this.MessagesTabControl.TabIndex = 0;
			this.MessagesTabControl.Controls.SetChildIndex(this.statusErrorsTabPage, 0);
			this.MessagesTabControl.Controls.SetChildIndex(this.messageDetailsTabPage, 0);
			this.MessagesTabControl.Controls.SetChildIndex(this.MessageTextTabPage, 0);
			// 
			// MessageTextTabPage
			// 
			this.MessageTextTabPage.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("MessageUserControl|600f7644-c825-4e2c-81a9-1a309ac35008", "Message Text");
			// 
			// MessageTextTextBox
			// 
			this.BindingSource.SetBindingMember(this.MessageTextTextBox, "InBondRelatedRecords.MessagesToShow.EM_MessageTextDetail");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.MessageTextTextBox, false);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((EDIMessage)(((System.Collections.IList)(((MessageActionRelatedRecordWrapper)(((System.Collections.IList)(((JobDeclaration)(null)).InBondRelatedRecords)).SyncRoot)).MessagesToShow)).SyncRoot)).EM_MessageTextDetail)));
			// 
			// HistoryGroupBox
			// 
			this.HistoryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(559, 460, true);
			this.HistoryGroupBox.TabIndex = 1;
			this.HistoryGroupBox.Text = "Message History";
			// 
			// MessagesGrid
			// 
			this.BindingSource.SetBindingMember(this.MessagesGrid, "InBondRelatedRecords.MessagesToShow");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((MessageActionRelatedRecordWrapper)(((System.Collections.IList)(((JobDeclaration)(null)).InBondRelatedRecords)).SyncRoot)).MessagesToShow)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((EDIMessage)(((System.Collections.IList)(((MessageActionRelatedRecordWrapper)(((System.Collections.IList)(((JobDeclaration)(null)).InBondRelatedRecords)).SyncRoot)).MessagesToShow)).SyncRoot)).EM_MessageNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((EDIMessage)(((System.Collections.IList)(((MessageActionRelatedRecordWrapper)(((System.Collections.IList)(((JobDeclaration)(null)).InBondRelatedRecords)).SyncRoot)).MessagesToShow)).SyncRoot)).EM_MessageSubType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((EDIMessage)(((System.Collections.IList)(((MessageActionRelatedRecordWrapper)(((System.Collections.IList)(((JobDeclaration)(null)).InBondRelatedRecords)).SyncRoot)).MessagesToShow)).SyncRoot)).EM_MessageDateTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((EDIMessage)(((System.Collections.IList)(((MessageActionRelatedRecordWrapper)(((System.Collections.IList)(((JobDeclaration)(null)).InBondRelatedRecords)).SyncRoot)).MessagesToShow)).SyncRoot)).EM_SystemCreateTimeUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((EDIMessage)(((System.Collections.IList)(((MessageActionRelatedRecordWrapper)(((System.Collections.IList)(((JobDeclaration)(null)).InBondRelatedRecords)).SyncRoot)).MessagesToShow)).SyncRoot)).EM_InterchangeNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((EDIMessage)(((System.Collections.IList)(((MessageActionRelatedRecordWrapper)(((System.Collections.IList)(((JobDeclaration)(null)).InBondRelatedRecords)).SyncRoot)).MessagesToShow)).SyncRoot)).EM_DateTimeInterchangeSent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((EDIMessage)(((System.Collections.IList)(((MessageActionRelatedRecordWrapper)(((System.Collections.IList)(((JobDeclaration)(null)).InBondRelatedRecords)).SyncRoot)).MessagesToShow)).SyncRoot)).EM_User)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((EDIMessage)(((System.Collections.IList)(((MessageActionRelatedRecordWrapper)(((System.Collections.IList)(((JobDeclaration)(null)).InBondRelatedRecords)).SyncRoot)).MessagesToShow)).SyncRoot)).MessageTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((EDIMessage)(((System.Collections.IList)(((MessageActionRelatedRecordWrapper)(((System.Collections.IList)(((JobDeclaration)(null)).InBondRelatedRecords)).SyncRoot)).MessagesToShow)).SyncRoot)).EM_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((EDIMessage)(((System.Collections.IList)(((MessageActionRelatedRecordWrapper)(((System.Collections.IList)(((JobDeclaration)(null)).InBondRelatedRecords)).SyncRoot)).MessagesToShow)).SyncRoot)).EM_SendWithMessageErrorsFormatted)));
			zTextBoxColumnStyleInfo1.Caption = "Message Type Desc";
			zTextBoxColumnStyleInfo1.ColumnName = "MessageTypeDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo2.Caption = "Status";
			zTextBoxColumnStyleInfo2.ColumnName = "EM_Status";
			zTextBoxColumnStyleInfo3.Caption = "Sent With Errors";
			zTextBoxColumnStyleInfo3.ColumnName = "EM_SendWithMessageErrorsFormatted";
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.MessagesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(553, 441, true);
			this.MessagesGrid.TabIndex = 0;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(JobDeclaration);
			// 
			// InBondHeadersHistorySplitter
			// 
			this.inBondHeadersHistorySplitter.Dock = System.Windows.Forms.DockStyle.Top;
			this.inBondHeadersHistorySplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 183, true);
			this.inBondHeadersHistorySplitter.Name = "InBondHeadersHistorySplitter";
			this.inBondHeadersHistorySplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(976, 3, true);
			this.inBondHeadersHistorySplitter.TabIndex = 2;
			this.inBondHeadersHistorySplitter.TabStop = false;
			// 
			// MessageDetailsTabPage
			// 
			this.messageDetailsTabPage.Controls.Add(this.messageDetailsTextBox);
			this.messageDetailsTabPage.Controls.Add(this.messageInterpretationBox);
			this.messageDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.messageDetailsTabPage.Name = "MessageDetailsTabPage";
			this.messageDetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.messageDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(382, 517, true);
			this.messageDetailsTabPage.TabIndex = 0;
			this.messageDetailsTabPage.Text = "Message Details";
			// 
			// MessageDetailsTextBox
			// 
			this.BindingSource.SetBindingMember(this.messageInterpretationBox, "InBondRelatedRecords.MessagesToShow.EM_MessageInterpretation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((EDIMessage)(((System.Collections.IList)(((MessageActionRelatedRecordWrapper)(((System.Collections.IList)(((JobDeclaration)(null)).InBondRelatedRecords)).SyncRoot)).MessagesToShow)).SyncRoot)).EM_MessageInterpretation)));
			this.messageInterpretationBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.messageInterpretationBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.messageInterpretationBox.Name = "MessageInterpretationBox";
			this.messageInterpretationBox.Visible = false;
			this.messageInterpretationBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(376, 511, true);
			this.messageInterpretationBox.TabIndex = 1;
			// 
			// MessageDetailsTextBox
			// 
			this.BindingSource.SetBindingMember(this.messageDetailsTextBox, "InBondRelatedRecords.MessagesToShow.EM_MessageInterpretation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((EDIMessage)(((System.Collections.IList)(((MessageActionRelatedRecordWrapper)(((System.Collections.IList)(((JobDeclaration)(null)).InBondRelatedRecords)).SyncRoot)).MessagesToShow)).SyncRoot)).EM_MessageInterpretation)));
			this.messageDetailsTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.messageDetailsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.messageDetailsTextBox.Multiline = true;
			this.messageDetailsTextBox.Name = "MessageDetailsTextBox";
			this.messageDetailsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.messageDetailsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(376, 511, true);
			this.messageDetailsTextBox.TabIndex = 0;
			this.messageDetailsTextBox.WordWrap = false;
			// 
			// RelatedRecordsGroupBox
			// 
			this.relatedRecordsGroupBox.Controls.Add(this.InBondHeadersGrid);
			this.relatedRecordsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.relatedRecordsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.relatedRecordsGroupBox.Name = "RelatedRecordsGroupBox";
			this.relatedRecordsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(976, 183, true);
			this.relatedRecordsGroupBox.TabIndex = 0;
			this.relatedRecordsGroupBox.TabStop = false;
			this.relatedRecordsGroupBox.Text = "Messages";
			// 
			// InBondHeadersGrid
			// 
			this.InBondHeadersGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.InBondHeadersGrid, "InBondRelatedRecords");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((JobDeclaration)(null)).InBondRelatedRecords)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((MessageActionRelatedRecordWrapper)(((System.Collections.IList)(((JobDeclaration)(null)).InBondRelatedRecords)).SyncRoot)).RecordTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((MessageActionRelatedRecordWrapper)(((System.Collections.IList)(((JobDeclaration)(null)).InBondRelatedRecords)).SyncRoot)).HumanFriendlyReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((MessageActionRelatedRecordWrapper)(((System.Collections.IList)(((JobDeclaration)(null)).InBondRelatedRecords)).SyncRoot)).Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((MessageActionRelatedRecordWrapper)(((System.Collections.IList)(((JobDeclaration)(null)).InBondRelatedRecords)).SyncRoot)).StatusDesc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((MessageActionRelatedRecordWrapper)(((System.Collections.IList)(((JobDeclaration)(null)).InBondRelatedRecords)).SyncRoot)).EntryStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((MessageActionRelatedRecordWrapper)(((System.Collections.IList)(((JobDeclaration)(null)).InBondRelatedRecords)).SyncRoot)).EntryStatusDesc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((MessageActionRelatedRecordWrapper)(((System.Collections.IList)(((JobDeclaration)(null)).InBondRelatedRecords)).SyncRoot)).ReleaseDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((MessageActionRelatedRecordWrapper)(((System.Collections.IList)(((JobDeclaration)(null)).InBondRelatedRecords)).SyncRoot)).TIBExpiryDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((MessageActionRelatedRecordWrapper)(((System.Collections.IList)(((JobDeclaration)(null)).InBondRelatedRecords)).SyncRoot)).TIBNumOfExtensions)));
			this.InBondHeadersGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo4.Caption = "Type";
			zTextBoxColumnStyleInfo4.ColumnName = "RecordTypeDescription";
			zTextBoxColumnStyleInfo4.IsMandatory = true;
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo5.Caption = "Reference";
			zTextBoxColumnStyleInfo5.ColumnName = "HumanFriendlyReference";
			zTextBoxColumnStyleInfo6.Caption = "Msg. Sts";
			zTextBoxColumnStyleInfo6.ColumnName = "Status";
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo7.Caption = "Message Status Description";
			zTextBoxColumnStyleInfo7.ColumnName = "StatusDesc";
			zTextBoxColumnStyleInfo7.IsVisible = false;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170);
			zTextBoxColumnStyleInfo8.Caption = "Customs Sts";
			zTextBoxColumnStyleInfo8.ColumnName = "EntryStatus";
			zTextBoxColumnStyleInfo8.IsReadOnly = true;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(75);
			zTextBoxColumnStyleInfo9.Caption = "Customs Status Description";
			zTextBoxColumnStyleInfo9.ColumnName = "EntryStatusDesc";
			zTextBoxColumnStyleInfo9.IsVisible = false;
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170);
			zDateEditColumnStyleInfo1.Caption = "Release Date";
			zDateEditColumnStyleInfo1.ColumnName = "ReleaseDate";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo2.Caption = "TIB Expiry Date";
			zDateEditColumnStyleInfo2.ColumnName = "TIBExpiryDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.GroupName = Enterprise.Customs.US.GUI.Res.GetData("ImportMessagesUserControl|7783450e-13df-450b-86f7-c6b27639aab4", "TIB");
			zDateEditColumnStyleInfo2.IsVisible = false;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = "TIB No Of Extns";
			zCalcEditColumnStyleInfo1.ColumnName = "TIBNumOfExtensions";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.GroupName = Enterprise.Customs.US.GUI.Res.GetData("ImportMessagesUserControl|7783450e-13df-450b-86f7-c6b27639aab4", "TIB");
			zCalcEditColumnStyleInfo1.IsVisible = false;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(91);
			this.InBondHeadersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.InBondHeadersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.InBondHeadersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.InBondHeadersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.InBondHeadersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.InBondHeadersGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.InBondHeadersGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.InBondHeadersGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.InBondHeadersGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.InBondHeadersGrid.GridId = "22275e02-8b50-407a-a899-d229c0cadac6";
			this.InBondHeadersGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InBondHeadersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.InBondHeadersGrid.LayoutKey = "InBondHeadersGrid";
			this.InBondHeadersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.InBondHeadersGrid.Name = "InBondHeadersGrid";
			this.InBondHeadersGrid.ReadOnly = true;
			this.InBondHeadersGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.InBondHeadersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(970, 164, true);
			this.InBondHeadersGrid.TabIndex = 0;
			// 
			// messagesStatusErrorsPanel
			// 
			this.messagesStatusErrorsPanel.Controls.Add(this.messagesStatusErrorsUserControl);
			this.messagesStatusErrorsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.messagesStatusErrorsPanel.Name = "messagesStatusErrorsPanel";
			this.messagesStatusErrorsPanel.TabIndex = 91;
			this.messagesStatusErrorsPanel.AutoScroll = true;
			this.messagesStatusErrorsPanel.AutoScrollMinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 330, true);
			// 
			// StatusErrorsTabPage
			// 
			this.statusErrorsTabPage.CaptionResourceString = Enterprise.Customs.US.GUI.Res.GetData("ImportMessagesUserControl|0cfa3694-fbbc-48ff-b60f-9b2d3bd75260", "Status/Errors", "Status/Errors", "");
			this.statusErrorsTabPage.Controls.Add(this.messagesStatusErrorsPanel);
			this.statusErrorsTabPage.Controls.Add(this.StatusesErrorsLabel);
			this.statusErrorsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.statusErrorsTabPage.Name = "StatusErrorsTabPage";
			this.statusErrorsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(406, 433, true);
			this.statusErrorsTabPage.TabIndex = 1;
			// 
			// messagesStatusErrorsUserControl
			// 
			this.BindingSource.SetBindingMember(this.messagesStatusErrorsUserControl, "InBondRelatedRecords.MessagesToShow.StatusesAndErrors");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.StatusErrorsDataViewCollection)(((EDIMessage)(((System.Collections.IList)(((MessageActionRelatedRecordWrapper)(((System.Collections.IList)(((JobDeclaration)(null)).InBondRelatedRecords)).SyncRoot)).MessagesToShow)).SyncRoot)).StatusesAndErrors)));
			this.messagesStatusErrorsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.messagesStatusErrorsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.messagesStatusErrorsUserControl.Name = "messagesStatusErrorsUserControl";
			this.messagesStatusErrorsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(406, 433, true);
			this.messagesStatusErrorsUserControl.TabIndex = 18;
			// 
			// StatusesErrorsLabel
			// 
			this.BindingSource.SetBindingMember(this.StatusesErrorsLabel, "InBondRelatedRecords.MessagesToShow.StatusesErrorsExist");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((EDIMessage)(((System.Collections.IList)(((MessageActionRelatedRecordWrapper)(((System.Collections.IList)(((JobDeclaration)(null)).InBondRelatedRecords)).SyncRoot)).MessagesToShow)).SyncRoot)).StatusesErrorsExist)));
			this.StatusesErrorsLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.StatusesErrorsLabel.ForeColor = System.Drawing.Color.Black;
			this.StatusesErrorsLabel.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.StatusesErrorsLabel, false);
			this.StatusesErrorsLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.StatusesErrorsLabel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 0, true);
			this.StatusesErrorsLabel.Name = "StatusesErrorsLabel";
			this.StatusesErrorsLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(406, 433, true);
			this.StatusesErrorsLabel.TabIndex = 17;
			this.StatusesErrorsLabel.VisibleChanged += new EventHandler(this.StatusesErrorsLabel_VisibleChanged);
			// 
			// ImportMessagesUserControl
			// 
			this.Controls.Add(this.inBondHeadersHistorySplitter);
			this.Controls.Add(this.relatedRecordsGroupBox);
			this.Name = "ImportMessagesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(976, 646, true);
			this.Controls.SetChildIndex(this.relatedRecordsGroupBox, 0);
			this.Controls.SetChildIndex(this.inBondHeadersHistorySplitter, 0);
			this.Controls.SetChildIndex(this.MainPanel, 0);
			this.MainPanel.ResumeLayout(false);
			this.MessagesTabControl.ResumeLayout(false);
			this.MessageTextTabPage.ResumeLayout(false);
			this.MessageTextTabPage.PerformLayout();
			this.HistoryGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessagesGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.messageDetailsTabPage.ResumeLayout(false);
			this.messageDetailsTabPage.PerformLayout();
			this.relatedRecordsGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.InBondHeadersGrid)).EndInit();
			this.statusErrorsTabPage.ResumeLayout(false);
			this.messagesStatusErrorsPanel.ResumeLayout(false);
			this.messagesStatusErrorsPanel.PerformLayout();
			this.ResumeLayout(false);
		}
	}
}
