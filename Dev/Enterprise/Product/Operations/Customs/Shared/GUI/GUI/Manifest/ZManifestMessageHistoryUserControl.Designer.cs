using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Customs.GUI
{
	public partial class ZManifestMessageHistoryUserControl
	{
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.TopPanel = new CargoWise.Windows.UI.KPanel();
			this.StatusLabel = new Enterprise.ZArchitecture.ZLabel();
			this.CRNLabel = new Enterprise.ZArchitecture.ZLabel();
			this.E2_MessageStatusBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CustomsEntryNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.HistoryPanel = new CargoWise.Windows.UI.KPanel();
			this.HistoryGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MessagesGrid = new Enterprise.Messaging.GUI.MessageZGrid();
			this.TheSplitter = new CargoWise.Windows.UI.KSplitter();
			this.MessageTextPanel = new CargoWise.Windows.UI.KPanel();
			this.MessageTextGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.MessageTextTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TopPanel.SuspendLayout();
			this.HistoryPanel.SuspendLayout();
			this.HistoryGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessagesGrid)).BeginInit();
			this.MessageTextPanel.SuspendLayout();
			this.MessageTextGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.CustomsManifestStatus);
			// 
			// TopPanel
			// 
			this.TopPanel.Controls.Add(this.StatusLabel);
			this.TopPanel.Controls.Add(this.CRNLabel);
			this.TopPanel.Controls.Add(this.E2_MessageStatusBoundTextBox);
			this.TopPanel.Controls.Add(this.CustomsEntryNumberTextBox);
			this.TopPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(952, 40, true);
			this.TopPanel.TabIndex = 1;
			// 
			// StatusLabel
			// 
			this.StatusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.StatusLabel.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("5F72BCA5-731A-44E1-B737-E3A06A7C7BF9", "Status:");
			this.StatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(624, 8, true);
			this.StatusLabel.Name = "StatusLabel";
			this.StatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 23, true);
			this.StatusLabel.TabIndex = 3;
			// 
			// CRNLabel
			// 
			this.BindingSource.SetBindingMember(this.CRNLabel, "E2_CustomsEntryNumberHumanReadableName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CustomsManifestStatus)(null)).E2_CustomsEntryNumberHumanReadableName)));
			this.CRNLabel.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("F0DBE50F-CEE2-4E78-B2C7-FC7022B88274", "CRN: ");
			this.CRNLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.CRNLabel.Name = "CRNLabel";
			this.CRNLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 23, true);
			this.CRNLabel.TabIndex = 2;
			// 
			// E2_MessageStatusBoundTextBox
			// 
			this.E2_MessageStatusBoundTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.E2_MessageStatusBoundTextBox, "E2_MessageStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CustomsManifestStatus)(null)).E2_MessageStatus)));
			this.E2_MessageStatusBoundTextBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("ZManifestMessageHistoryUserControl|e2d90324-9c17-4516-aa23-e3e9ed0b04e3", "Status");
			this.E2_MessageStatusBoundTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.E2_MessageStatusBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(672, 9, true);
			this.E2_MessageStatusBoundTextBox.Name = "E2_MessageStatusBoundTextBox";
			this.E2_MessageStatusBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(272, 20, true);
			this.E2_MessageStatusBoundTextBox.TabIndex = 1;
			this.E2_MessageStatusBoundTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
			this.E2_MessageStatusBoundTextBox.BackColorChanged += new System.EventHandler(this.E2_MessageStatusBoundTextBox4_UpdateColor);
			this.E2_MessageStatusBoundTextBox.TextChanged += new System.EventHandler(this.E2_MessageStatusBoundTextBox4_UpdateColor);
			// 
			// CustomsEntryNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.CustomsEntryNumberTextBox, "E2_CustomsEntryNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.CustomsManifestStatus)(null)).E2_CustomsEntryNumber)));
			this.CustomsEntryNumberTextBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("ZManifestMessageHistoryUserControl|dcb7a1fd-aa4a-4a7c-b5d8-e16aef4734c8", "CRN");
			this.CustomsEntryNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 9, true);
			this.CustomsEntryNumberTextBox.Name = "CustomsEntryNumberTextBox";
			this.CustomsEntryNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(112, 20, true);
			this.CustomsEntryNumberTextBox.TabIndex = 0;
			// 
			// HistoryPanel
			// 
			this.HistoryPanel.Controls.Add(this.HistoryGroupBox);
			this.HistoryPanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.HistoryPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 40, true);
			this.HistoryPanel.Name = "HistoryPanel";
			this.HistoryPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 504, true);
			this.HistoryPanel.TabIndex = 2;
			// 
			// HistoryGroupBox
			// 
			this.HistoryGroupBox.Controls.Add(this.MessagesGrid);
			this.HistoryGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("B09F2090-7408-4C03-A900-9A390F6D28E6", "History");
			this.HistoryGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HistoryGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HistoryGroupBox.Name = "HistoryGroupBox";
			this.HistoryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 504, true);
			this.HistoryGroupBox.TabIndex = 0;
			this.HistoryGroupBox.TabStop = false;
			// 
			// MessagesGrid
			// 
			this.MessagesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.MessagesGrid, "MessagesIncludingInterchangeRejections");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.Business.CustomsManifestStatus)(null)).MessagesIncludingInterchangeRejections)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.Business.CustomsManifestStatus)(null)).MessagesIncludingInterchangeRejections)).SyncRoot)).EM_MessageNum)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.Business.CustomsManifestStatus)(null)).MessagesIncludingInterchangeRejections)).SyncRoot)).EM_MessageSubType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.Business.CustomsManifestStatus)(null)).MessagesIncludingInterchangeRejections)).SyncRoot)).EM_MessageDateTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.Business.CustomsManifestStatus)(null)).MessagesIncludingInterchangeRejections)).SyncRoot)).EM_SystemCreateTimeUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.Business.CustomsManifestStatus)(null)).MessagesIncludingInterchangeRejections)).SyncRoot)).EM_InterchangeNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.Business.CustomsManifestStatus)(null)).MessagesIncludingInterchangeRejections)).SyncRoot)).EM_DateTimeInterchangeSent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.Business.CustomsManifestStatus)(null)).MessagesIncludingInterchangeRejections)).SyncRoot)).EM_User)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.Business.CustomsManifestStatus)(null)).MessagesIncludingInterchangeRejections)).SyncRoot)).EM_Status)));
			this.MessagesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.Caption = "Num";
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "EM_MessageNum";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.ToolTip = "Message Number";
			zTextBoxColumnStyleInfo2.Caption = "Sub Type";
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "EM_MessageSubType";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.ToolTip = "Message Sub Type";
			zDateEditColumnStyleInfo1.Caption = "Message Time";
			zDateEditColumnStyleInfo1.ColumnName = "EM_MessageDateTime";
			zDateEditColumnStyleInfo2.Caption = "Create Time (UTC)";
			zDateEditColumnStyleInfo2.ColumnName = "EM_SystemCreateTimeUtc";
			zTextBoxColumnStyleInfo3.Caption = "Interchange";
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("ZManifestMessageHistoryUserControl|b9662a93-748c-48bb-9ae3-cf61d2e9edd4", "Interchange");
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "EM_InterchangeNumber";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.ToolTip = "Interchange Number";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDateEditColumnStyleInfo3.Caption = "Interchange DT";
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("ZManifestMessageHistoryUserControl|498f1e07-29fe-4d56-ae0f-1379b8e04188", "Interchange DT");
			zDateEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDateEditColumnStyleInfo3.ColumnName = "EM_DateTimeInterchangeSent";
			zDateEditColumnStyleInfo3.IsReadOnly = true;
			zDateEditColumnStyleInfo3.ToolTip = "Date Time Interchange Sent";
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo4.Caption = "User";
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("ZManifestMessageHistoryUserControl|4b1ba54c-f961-4454-b0c2-5dd4e58b51b7", "User");
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo4.ColumnName = "EM_User";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.ToolTip = "Message Sent by User";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo5.Caption = "Status";
			zTextBoxColumnStyleInfo5.ColumnName = "EM_Status";
			zTextBoxColumnStyleInfo5.IsVisible = false;
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.MessagesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.MessagesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.MessagesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.MessagesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.MessagesGrid.CopySelectedRowsAllowed = true;
			this.MessagesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessagesGrid.GridId = "a29efb2a-21e7-474a-ac1d-a1db294eec6a";
			this.MessagesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MessagesGrid.LayoutKey = "zGrid1";
			this.MessagesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.MessagesGrid.Name = "MessagesGrid";
			this.MessagesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(594, 485, true);
			this.MessagesGrid.TabIndex = 2;
			// 
			// TheSplitter
			// 
			this.TheSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(600, 40, true);
			this.TheSplitter.Name = "TheSplitter";
			this.TheSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(3, 504, true);
			this.TheSplitter.TabIndex = 3;
			this.TheSplitter.TabStop = false;
			// 
			// MessageTextPanel
			// 
			this.MessageTextPanel.Controls.Add(this.MessageTextGroupBox);
			this.MessageTextPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageTextPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(603, 40, true);
			this.MessageTextPanel.Name = "MessageTextPanel";
			this.MessageTextPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(349, 504, true);
			this.MessageTextPanel.TabIndex = 4;
			// 
			// MessageTextGroupBox
			// 
			this.MessageTextGroupBox.Controls.Add(this.MessageTextTextBox);
			this.MessageTextGroupBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("B40C9EFB-2AC9-443F-B493-0E80A08421D6", "Message Text");
			this.MessageTextGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageTextGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessageTextGroupBox.Name = "MessageTextGroupBox";
			this.MessageTextGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(349, 504, true);
			this.MessageTextGroupBox.TabIndex = 0;
			this.MessageTextGroupBox.TabStop = false;
			// 
			// MessageTextTextBox
			// 
			this.BindingSource.SetBindingMember(this.MessageTextTextBox, "MessagesIncludingInterchangeRejections.EM_FormattedMessageText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.Business.CustomsManifestStatus)(null)).MessagesIncludingInterchangeRejections)).SyncRoot)).EM_FormattedMessageText)));
			this.MessageTextTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.MessageTextTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageTextTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.MessageTextTextBox.Multiline = true;
			this.MessageTextTextBox.Name = "MessageTextTextBox";
			this.MessageTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(343, 485, true);
			this.MessageTextTextBox.TabIndex = 0;
			// 
			// ZManifestMessageHistoryUserControl
			// 
			this.CaptionRenderingEnabled = false;
			this.Controls.Add(this.MessageTextPanel);
			this.Controls.Add(this.TheSplitter);
			this.Controls.Add(this.HistoryPanel);
			this.Controls.Add(this.TopPanel);
			this.Name = "ZManifestMessageHistoryUserControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(952, 544, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			this.HistoryPanel.ResumeLayout(false);
			this.HistoryGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessagesGrid)).EndInit();
			this.MessageTextPanel.ResumeLayout(false);
			this.MessageTextGroupBox.ResumeLayout(false);
			this.MessageTextGroupBox.PerformLayout();
			this.ResumeLayout(false);
		}
	}
}
