using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Enterprise.Customs.Common.US;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	public partial class AESMessageUserControl
	{
		void InitializeComponent()
		{
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new ZTextBoxColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZDateEditColumnStyleInfo();
			ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new ZDateEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo16 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo17 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZTextBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZTextBoxColumnStyleInfo();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZCheckBoxColumnStyleInfo();
			this.sEDDetailsPanel = new ZPanel();
			this.messagesPanel = new ZPanel();
			this.messagesGroupBox = new ZGroupBox();
			this.messagesGrid = new ZGrid();
			this.sEDSplitter = new CargoWise.Windows.UI.KSplitter();
			this.sEDsPanel = new ZPanel();
			this.sEDsGroupBox = new ZGroupBox();
			this.sEDsGrid = new ZGrid();
			this.theSplitter = new CargoWise.Windows.UI.KSplitter();
			this.messageTextPanel = new CargoWise.Windows.UI.KPanel();
			this.messageTextDataPanel = new ZPanel();
			this.aESTIRMessageTabControl = new ZTabControl();
			this.messageTextTabPage = new ZTabPage();
			this.aESTIRMessageTextTextBox = new ZTextBox();
			this.messageDetailsTabPage = new ZTabPage();
			this.aESTIRMessageDetailsTextBox = new ZTextBox();
			this.statusPanel = new ZPanel();
			this.messageStatusTextBox = new ZTextBox();
			this.messageStatusLabel = new ZLabel();
			this.sEDStatusTextBox = new ZTextBox();
			this.sEDStatusLabel = new ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.sEDDetailsPanel.SuspendLayout();
			this.messagesPanel.SuspendLayout();
			this.messagesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.messagesGrid)).BeginInit();
			this.sEDsPanel.SuspendLayout();
			this.sEDsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.sEDsGrid)).BeginInit();
			this.messageTextPanel.SuspendLayout();
			this.messageTextDataPanel.SuspendLayout();
			this.aESTIRMessageTabControl.SuspendLayout();
			this.messageTextTabPage.SuspendLayout();
			this.messageDetailsTabPage.SuspendLayout();
			this.statusPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.JobDeclaration);
			// 
			// SEDDetailsPanel
			// 
			this.sEDDetailsPanel.Controls.Add(this.messagesPanel);
			this.sEDDetailsPanel.Controls.Add(this.sEDSplitter);
			this.sEDDetailsPanel.Controls.Add(this.sEDsPanel);
			this.sEDDetailsPanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.sEDDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.sEDDetailsPanel.Name = "SEDDetailsPanel";
			this.sEDDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(656, 544, true);
			this.sEDDetailsPanel.TabIndex = 7;
			// 
			// MessagesPanel
			// 
			this.messagesPanel.Controls.Add(this.messagesGroupBox);
			this.messagesPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.messagesPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 206, true);
			this.messagesPanel.Name = "MessagesPanel";
			this.messagesPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(656, 338, true);
			this.messagesPanel.TabIndex = 18;
			// 
			// MessagesGroupBox
			// 
			this.messagesGroupBox.Controls.Add(this.messagesGrid);
			this.messagesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.messagesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.messagesGroupBox.Name = "MessagesGroupBox";
			this.messagesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(656, 338, true);
			this.messagesGroupBox.TabIndex = 8;
			this.messagesGroupBox.TabStop = false;
			this.messagesGroupBox.Text = "Messages";
			// 
			// MessagesGrid
			// 
			this.messagesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.messagesGrid, "EntryHeadersWithOptionalDeactivated.Messages");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.CusEntryHeader)(((System.Collections.IList)(((Business.JobDeclaration)(null)).EntryHeadersWithOptionalDeactivated)).SyncRoot)).Messages)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.EDIMessage)(((System.Collections.IList)(((Business.CusEntryHeader)(((System.Collections.IList)(((Business.JobDeclaration)(null)).EntryHeadersWithOptionalDeactivated)).SyncRoot)).Messages)).SyncRoot)).EM_MessageType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.EDIMessage)(((System.Collections.IList)(((Business.CusEntryHeader)(((System.Collections.IList)(((Business.JobDeclaration)(null)).EntryHeadersWithOptionalDeactivated)).SyncRoot)).Messages)).SyncRoot)).EM_MessageNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.EDIMessage)(((System.Collections.IList)(((Business.CusEntryHeader)(((System.Collections.IList)(((Business.JobDeclaration)(null)).EntryHeadersWithOptionalDeactivated)).SyncRoot)).Messages)).SyncRoot)).EM_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.EDIMessage)(((System.Collections.IList)(((Business.CusEntryHeader)(((System.Collections.IList)(((Business.JobDeclaration)(null)).EntryHeadersWithOptionalDeactivated)).SyncRoot)).Messages)).SyncRoot)).EM_SendOrReceiveHumanReadable)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Business.EDIMessage)(((System.Collections.IList)(((Business.CusEntryHeader)(((System.Collections.IList)(((Business.JobDeclaration)(null)).EntryHeadersWithOptionalDeactivated)).SyncRoot)).Messages)).SyncRoot)).EM_MessageDateTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Business.EDIMessage)(((System.Collections.IList)(((Business.CusEntryHeader)(((System.Collections.IList)(((Business.JobDeclaration)(null)).EntryHeadersWithOptionalDeactivated)).SyncRoot)).Messages)).SyncRoot)).EM_SystemCreateTimeUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.EDIMessage)(((System.Collections.IList)(((Business.CusEntryHeader)(((System.Collections.IList)(((Business.JobDeclaration)(null)).EntryHeadersWithOptionalDeactivated)).SyncRoot)).Messages)).SyncRoot)).EM_User)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.EDIMessage)(((System.Collections.IList)(((Business.CusEntryHeader)(((System.Collections.IList)(((Business.JobDeclaration)(null)).EntryHeadersWithOptionalDeactivated)).SyncRoot)).Messages)).SyncRoot)).EM_MessageSubType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.EDIMessage)(((System.Collections.IList)(((Business.CusEntryHeader)(((System.Collections.IList)(((Business.JobDeclaration)(null)).EntryHeadersWithOptionalDeactivated)).SyncRoot)).Messages)).SyncRoot)).EM_SendWithMessageErrorsFormatted)));
			this.messagesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo11.Caption = "Message Type";
			zTextBoxColumnStyleInfo11.ColumnName = "EM_MessageType";
			zTextBoxColumnStyleInfo12.Caption = "Message Number";
			zTextBoxColumnStyleInfo12.ColumnName = "EM_MessageNum";
			zTextBoxColumnStyleInfo13.Caption = "Status";
			zTextBoxColumnStyleInfo13.ColumnName = "EM_Status";
			zTextBoxColumnStyleInfo14.Caption = "Direction";
			zTextBoxColumnStyleInfo14.ColumnName = "EM_SendOrReceiveHumanReadable";
			zDateEditColumnStyleInfo1.Caption = "Message Time";
			zDateEditColumnStyleInfo1.ColumnName = "EM_MessageDateTime";
			zDateEditColumnStyleInfo2.Caption = "Create Time (UTC)";
			zDateEditColumnStyleInfo2.ColumnName = "EM_SystemCreateTimeUtc";
			zTextBoxColumnStyleInfo15.Caption = "Sender";
			zTextBoxColumnStyleInfo15.ColumnName = "EM_User";
			zTextBoxColumnStyleInfo15.ToolTip = "It is the person who created this message.";
			zTextBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo16.Caption = "Message Sub Type";
			zTextBoxColumnStyleInfo16.ColumnName = "EM_MessageSubType";
			zTextBoxColumnStyleInfo16.IsVisible = false;
			zTextBoxColumnStyleInfo17.Caption = "Sent With Errors";
			zTextBoxColumnStyleInfo17.ColumnName = "EM_SendWithMessageErrorsFormatted";
			this.messagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.messagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.messagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.messagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.messagesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.messagesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.messagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
			this.messagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo16);
			this.messagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo17);
			this.messagesGrid.CopySelectedRowsAllowed = true;
			this.messagesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.messagesGrid.GridId = "e48e1afe-41ce-4859-a45b-eefa3992ff50";
			this.messagesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.messagesGrid.LayoutKey = "MessagesGrid";
			this.messagesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.messagesGrid.Name = "MessagesGrid";
			this.messagesGrid.ReadOnly = true;
			this.messagesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(650, 319, true);
			this.messagesGrid.TabIndex = 2;
			// 
			// SEDSplitter
			// 
			this.sEDSplitter.Dock = System.Windows.Forms.DockStyle.Top;
			this.sEDSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 203, true);
			this.sEDSplitter.Name = "SEDSplitter";
			this.sEDSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(656, 3, true);
			this.sEDSplitter.TabIndex = 17;
			this.sEDSplitter.TabStop = false;
			// 
			// SEDsPanel
			// 
			this.sEDsPanel.Controls.Add(this.sEDsGroupBox);
			this.sEDsPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.sEDsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.sEDsPanel.Name = "SEDsPanel";
			this.sEDsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(656, 203, true);
			this.sEDsPanel.TabIndex = 15;
			// 
			// SEDsGroupBox
			// 
			this.sEDsGroupBox.Controls.Add(this.sEDsGrid);
			this.sEDsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.sEDsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.sEDsGroupBox.Name = "SEDsGroupBox";
			this.sEDsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(656, 203, true);
			this.sEDsGroupBox.TabIndex = 7;
			this.sEDsGroupBox.TabStop = false;
			this.sEDsGroupBox.Text = "Shipper\'s Export Declarations";
			// 
			// SEDsGrid
			// 
			this.sEDsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.sEDsGrid, "EntryHeadersWithOptionalDeactivated");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.JobDeclaration)(null)).EntryHeadersWithOptionalDeactivated)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.CusEntryHeader)(((System.Collections.IList)(((Business.JobDeclaration)(null)).EntryHeadersWithOptionalDeactivated)).SyncRoot)).CH_BGMReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.CusEntryHeader)(((System.Collections.IList)(((Business.JobDeclaration)(null)).EntryHeadersWithOptionalDeactivated)).SyncRoot)).EntryNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.CusEntryHeader)(((System.Collections.IList)(((Business.JobDeclaration)(null)).EntryHeadersWithOptionalDeactivated)).SyncRoot)).US_XTN)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.CusEntryHeader)(((System.Collections.IList)(((Business.JobDeclaration)(null)).EntryHeadersWithOptionalDeactivated)).SyncRoot)).EntryHeaderStatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.CusEntryHeader)(((System.Collections.IList)(((Business.JobDeclaration)(null)).EntryHeadersWithOptionalDeactivated)).SyncRoot)).MessageStatusDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Business.CusEntryHeader)(((System.Collections.IList)(((Business.JobDeclaration)(null)).EntryHeadersWithOptionalDeactivated)).SyncRoot)).US_IsDeactivated)));
			this.sEDsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "Shipment No.";
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "CH_BGMReference";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(72);
			zTextBoxColumnStyleInfo2.Caption = "ITN";
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "EntryNumber";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo3.Caption = "XTN";
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "US_XTN";
			zTextBoxColumnStyleInfo3.IsMandatory = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zTextBoxColumnStyleInfo4.Caption = "SED Status";
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zTextBoxColumnStyleInfo4.ColumnName = "EntryHeaderStatusDescription";
			zTextBoxColumnStyleInfo4.IsMandatory = true;
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(153);
			zTextBoxColumnStyleInfo5.Caption = "Message Status";
			zTextBoxColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zTextBoxColumnStyleInfo5.ColumnName = "MessageStatusDescription";
			zTextBoxColumnStyleInfo5.IsMandatory = true;
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(153);
			zCheckBoxColumnStyleInfo1.Caption = "Is Deactivated?";
			zCheckBoxColumnStyleInfo1.ColumnName = "US_IsDeactivated";
			zCheckBoxColumnStyleInfo1.IsReadOnly = true;
			zCheckBoxColumnStyleInfo1.IsVisible = false;
			zCheckBoxColumnStyleInfo1.IsMandatory = true;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(98);
			this.sEDsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.sEDsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.sEDsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.sEDsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.sEDsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.sEDsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.sEDsGrid.CopySelectedRowsAllowed = true;
			this.sEDsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.sEDsGrid.GridId = "15c8ef5b-73ee-45f0-a328-05efc5e1ad20";
			this.sEDsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.sEDsGrid.LayoutKey = "CusEntryHeaderGrid";
			this.sEDsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.sEDsGrid.Name = "SEDsGrid";
			this.sEDsGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.sEDsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(650, 184, true);
			this.sEDsGrid.TabIndex = 2;
			// 
			// TheSplitter
			// 
			this.theSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(656, 0, true);
			this.theSplitter.Name = "TheSplitter";
			this.theSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(3, 544, true);
			this.theSplitter.TabIndex = 8;
			this.theSplitter.TabStop = false;
			// 
			// MessageTextPanel
			// 
			this.messageTextPanel.Controls.Add(this.messageTextDataPanel);
			this.messageTextPanel.Controls.Add(this.statusPanel);
			this.messageTextPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.messageTextPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(659, 0, true);
			this.messageTextPanel.Name = "MessageTextPanel";
			this.messageTextPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 544, true);
			this.messageTextPanel.TabIndex = 9;
			// 
			// MessageTextDataPanel
			// 
			this.messageTextDataPanel.Controls.Add(this.aESTIRMessageTabControl);
			this.messageTextDataPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.messageTextDataPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 63, true);
			this.messageTextDataPanel.Name = "MessageTextDataPanel";
			this.messageTextDataPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 481, true);
			this.messageTextDataPanel.TabIndex = 1;
			// 
			// AESTIRMessageTabControl
			// 
			this.aESTIRMessageTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.aESTIRMessageTabControl.Controls.Add(this.messageTextTabPage);
			this.aESTIRMessageTabControl.Controls.Add(this.messageDetailsTabPage);
			this.aESTIRMessageTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.aESTIRMessageTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.aESTIRMessageTabControl.Name = "AESTIRMessageTabControl";
			this.aESTIRMessageTabControl.SelectedIndex = 0;
			this.aESTIRMessageTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 481, true);
			this.aESTIRMessageTabControl.TabIndex = 4;
			// 
			// MessageTextTabPage
			// 
			this.messageTextTabPage.Controls.Add(this.aESTIRMessageTextTextBox);
			this.messageTextTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.messageTextTabPage.Name = "MessageTextTabPage";
			this.messageTextTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.messageTextTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(285, 454, true);
			this.messageTextTabPage.TabIndex = 0;
			this.messageTextTabPage.Text = "Message Text";
			this.messageTextTabPage.UseVisualStyleBackColor = true;
			// 
			// AESTIRMessageTextTextBox
			// 
			this.BindingSource.SetBindingMember(this.aESTIRMessageTextTextBox, "EntryHeadersWithOptionalDeactivated.Messages.EM_FormattedMessageText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.EDIMessage)(((System.Collections.IList)(((Business.CusEntryHeader)(((System.Collections.IList)(((Business.JobDeclaration)(null)).EntryHeadersWithOptionalDeactivated)).SyncRoot)).Messages)).SyncRoot)).EM_FormattedMessageText)));
			this.aESTIRMessageTextTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.aESTIRMessageTextTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.aESTIRMessageTextTextBox.Multiline = true;
			this.aESTIRMessageTextTextBox.Name = "AESTIRMessageTextTextBox";
			this.aESTIRMessageTextTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.aESTIRMessageTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(279, 448, true);
			this.aESTIRMessageTextTextBox.TabIndex = 0;
			this.aESTIRMessageTextTextBox.WordWrap = false;
			// 
			// MessageDetailsTabPage
			// 
			this.messageDetailsTabPage.Controls.Add(this.aESTIRMessageDetailsTextBox);
			this.messageDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.messageDetailsTabPage.Name = "MessageDetailsTabPage";
			this.messageDetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.messageDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(285, 454, true);
			this.messageDetailsTabPage.TabIndex = 1;
			this.messageDetailsTabPage.Text = "Message Details";
			this.messageDetailsTabPage.UseVisualStyleBackColor = true;
			// 
			// AESTIRMessageDetailsTextBox
			// 
			this.BindingSource.SetBindingMember(this.aESTIRMessageDetailsTextBox, "EntryHeadersWithOptionalDeactivated.Messages.EM_MessageInterpretation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.EDIMessage)(((System.Collections.IList)(((Business.CusEntryHeader)(((System.Collections.IList)(((Business.JobDeclaration)(null)).EntryHeadersWithOptionalDeactivated)).SyncRoot)).Messages)).SyncRoot)).EM_MessageInterpretation)));
			this.aESTIRMessageDetailsTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.aESTIRMessageDetailsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.aESTIRMessageDetailsTextBox.Multiline = true;
			this.aESTIRMessageDetailsTextBox.Name = "AESTIRMessageDetailsTextBox";
			this.aESTIRMessageDetailsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.aESTIRMessageDetailsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(279, 448, true);
			this.aESTIRMessageDetailsTextBox.TabIndex = 0;
			this.aESTIRMessageDetailsTextBox.WordWrap = false;
			// 
			// StatusPanel
			// 
			this.statusPanel.Controls.Add(this.messageStatusTextBox);
			this.statusPanel.Controls.Add(this.messageStatusLabel);
			this.statusPanel.Controls.Add(this.sEDStatusTextBox);
			this.statusPanel.Controls.Add(this.sEDStatusLabel);
			this.statusPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.statusPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.statusPanel.Name = "StatusPanel";
			this.statusPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 63, true);
			this.statusPanel.TabIndex = 0;
			// 
			// MessageStatusTextBox
			// 
			this.messageStatusTextBox.AcceptsReturn = true;
			this.messageStatusTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.messageStatusTextBox, "EntryHeadersWithOptionalDeactivated.MessageStatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.CusEntryHeader)(((System.Collections.IList)(((Business.JobDeclaration)(null)).EntryHeadersWithOptionalDeactivated)).SyncRoot)).MessageStatusDescription)));
			this.messageStatusTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.messageStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 34, true);
			this.messageStatusTextBox.Name = "MessageStatusTextBox";
			this.messageStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(205, 20, true);
			this.messageStatusTextBox.TabIndex = 3;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.messageStatusTextBox, false);
			// 
			// MessageStatusLabel
			// 
			this.messageStatusLabel.AutoSize = true;
			this.messageStatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 36, true);
			this.messageStatusLabel.Name = "MessageStatusLabel";
			this.messageStatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(63, 13, true);
			this.messageStatusLabel.TabIndex = 2;
			this.messageStatusLabel.Text = "Msg Status:";
			// 
			// SEDStatusTextBox
			// 
			this.sEDStatusTextBox.AcceptsReturn = true;
			this.sEDStatusTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.sEDStatusTextBox, "EntryHeadersWithOptionalDeactivated.EntryHeaderStatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.CusEntryHeader)(((System.Collections.IList)(((Business.JobDeclaration)(null)).EntryHeadersWithOptionalDeactivated)).SyncRoot)).EntryHeaderStatusDescription)));
			this.sEDStatusTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.sEDStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 10, true);
			this.sEDStatusTextBox.Name = "SEDStatusTextBox";
			this.sEDStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(205, 20, true);
			this.sEDStatusTextBox.TabIndex = 1;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.sEDStatusTextBox, false);
			// 
			// SEDStatusLabel
			// 
			this.sEDStatusLabel.AutoSize = true;
			this.sEDStatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 12, true);
			this.sEDStatusLabel.Name = "SEDStatusLabel";
			this.sEDStatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(65, 13, true);
			this.sEDStatusLabel.TabIndex = 0;
			this.sEDStatusLabel.Text = "SED Status:";
			// 
			// AESMessageUserControl
			// 
			this.Controls.Add(this.messageTextPanel);
			this.Controls.Add(this.theSplitter);
			this.Controls.Add(this.sEDDetailsPanel);
			this.Name = "AESMessageUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(952, 544, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.sEDDetailsPanel.ResumeLayout(false);
			this.messagesPanel.ResumeLayout(false);
			this.messagesGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.messagesGrid)).EndInit();
			this.sEDsPanel.ResumeLayout(false);
			this.sEDsGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.sEDsGrid)).EndInit();
			this.messageTextPanel.ResumeLayout(false);
			this.messageTextDataPanel.ResumeLayout(false);
			this.aESTIRMessageTabControl.ResumeLayout(false);
			this.messageTextTabPage.ResumeLayout(false);
			this.messageTextTabPage.PerformLayout();
			this.messageDetailsTabPage.ResumeLayout(false);
			this.messageDetailsTabPage.PerformLayout();
			this.statusPanel.ResumeLayout(false);
			this.statusPanel.PerformLayout();
			this.ResumeLayout(false);
		}

		ZPanel sEDDetailsPanel;
		CargoWise.Windows.UI.KSplitter theSplitter;
		CargoWise.Windows.UI.KPanel messageTextPanel;
		ZPanel sEDsPanel;
		ZGroupBox sEDsGroupBox;
		ZGrid sEDsGrid;
		CargoWise.Windows.UI.KSplitter sEDSplitter;
		ZPanel messagesPanel;
		ZGroupBox messagesGroupBox;
		ZPanel statusPanel;
		ZTextBox sEDStatusTextBox;
		ZLabel sEDStatusLabel;
		ZTextBox messageStatusTextBox;
		ZLabel messageStatusLabel;
		ZPanel messageTextDataPanel;
		ZTabControl aESTIRMessageTabControl;
		ZTabPage messageTextTabPage;
		ZTabPage messageDetailsTabPage;
		ZTextBox aESTIRMessageTextTextBox;
		ZTextBox aESTIRMessageDetailsTextBox;
		ZGrid messagesGrid;
	}
}
