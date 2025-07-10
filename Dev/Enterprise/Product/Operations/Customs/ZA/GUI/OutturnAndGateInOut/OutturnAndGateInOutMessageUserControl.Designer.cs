using Enterprise.Messaging.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ZA.GUI
{
    public partial class OutturnAndGateInOutMessageUserControl
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
		void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
            this.zGridMessage = new Enterprise.ZArchitecture.ZGrid();
            this.zTabControlMessage = new Enterprise.ZArchitecture.GUI.ZTabControl();
            this.zTabPageMessageDetail = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.MessageInterpretationWebBrowser = new Enterprise.Messaging.GUI.HtmlInterpretationBox();
            this.zTabPageMessageText = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.MessageTextTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.kSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.zGridMessage)).BeginInit();
            this.zGridMessage.SuspendLayout();
            this.zTabControlMessage.SuspendLayout();
            this.zTabPageMessageDetail.SuspendLayout();
            this.zTabPageMessageText.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.kSplitContainer)).BeginInit();
            this.kSplitContainer.Panel1.SuspendLayout();
            this.kSplitContainer.Panel2.SuspendLayout();
            this.kSplitContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ZA.Business.AsycudaManifestHeader);
            // 
            // zGridMessage
            // 
            this.zGridMessage.AllowNavigation = false;
            this.BindingSource.SetBindingMember(this.zGridMessage, "Messages");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Messages)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Messages)).SyncRoot)).EM_MessageNum)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Messages)).SyncRoot)).EM_MessageDateTime)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Messages)).SyncRoot)).EM_SystemCreateTimeUtc)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Messages)).SyncRoot)).EM_SystemCreateUser)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Messages)).SyncRoot)).EM_ReceiveTransmit)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Messages)).SyncRoot)).EM_MessageType)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Messages)).SyncRoot)).EM_MessageSubType)));
            this.zGridMessage.CaptionVisible = false;
            zTextBoxColumnStyleInfo1.ColumnName = "EM_MessageNum";
            zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zDateEditColumnStyleInfo1.ColumnName = "EM_MessageDateTime";
            zDateEditColumnStyleInfo1.DefaultCollectionIndex = 0;
            zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
            zDateEditColumnStyleInfo2.ColumnName = "EM_SystemCreateTimeUtc";
            zDateEditColumnStyleInfo2.DefaultCollectionIndex = 0;
            zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
            zTextBoxColumnStyleInfo2.ColumnName = "EM_SystemCreateUser";
            zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo3.ColumnName = "EM_ReceiveTransmit";
            zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo4.ColumnName = "EM_MessageType";
            zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zTextBoxColumnStyleInfo5.ColumnName = "EM_MessageSubType";
            zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
            zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.ColumnName = "EM_Status";
			zTextBoxColumnStyleInfo6.CaptionResourceString = Res.GetData("9FAEA565-9001-4F46-BD9D-06A131F66D11", "CUSRES Status");
			zTextBoxColumnStyleInfo6.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.ColumnName = "EM_InterchangeNumber";
			zTextBoxColumnStyleInfo7.CaptionResourceString = Res.GetData("FBF1964D-9EA9-471F-BED7-D123F85DE302", "Interchange No.");
			zTextBoxColumnStyleInfo7.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo3.ColumnName = "EM_DateTimeInterchangeSent";
			zDateEditColumnStyleInfo3.CaptionResourceString = Res.GetData("6BDB1425-CE15-44C6-9B79-4BB205D0B5DA", "Interchange Time");
			zDateEditColumnStyleInfo3.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.zGridMessage.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.zGridMessage.ColumnStyles.Add(zDateEditColumnStyleInfo1);
            this.zGridMessage.ColumnStyles.Add(zDateEditColumnStyleInfo2);
            this.zGridMessage.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
            this.zGridMessage.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
            this.zGridMessage.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
            this.zGridMessage.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
            this.zGridMessage.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
            this.zGridMessage.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
            this.zGridMessage.ColumnStyles.Add(zDateEditColumnStyleInfo3);
            this.zGridMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.zGridMessage.GridId = "eaff1b4b-d751-4d8b-b70d-be5664d597d5";
            this.zGridMessage.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.zGridMessage.LayoutKey = "zGridMessage";
            this.zGridMessage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.zGridMessage.Name = "zGridMessage";
            this.zGridMessage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(860, 98, true);
            this.zGridMessage.TabIndex = 0;
            // 
            // zTabControlMessage
            // 
            this.zTabControlMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.zTabControlMessage.Controls.Add(this.zTabPageMessageDetail);
            this.zTabControlMessage.Controls.Add(this.zTabPageMessageText);
            this.zTabControlMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.zTabControlMessage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.zTabControlMessage.Name = "zTabControlMessage";
            this.zTabControlMessage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(860, 316, true);
            this.zTabControlMessage.TabIndex = 1;
            // 
            // zTabPageMessageDetail
            // 
            this.zTabPageMessageDetail.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("edda8541-1499-4346-ab81-3c58549cac87", "Details");
            this.zTabPageMessageDetail.Controls.Add(this.MessageInterpretationWebBrowser);
            this.zTabPageMessageDetail.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            this.zTabPageMessageDetail.Name = "zTabPageMessageDetail";
            this.zTabPageMessageDetail.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.zTabPageMessageDetail.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(852, 289, true);
            this.zTabPageMessageDetail.TabIndex = 2;
            this.zTabPageMessageDetail.UseVisualStyleBackColor = true;
            // 
            // MessageInterpretationWebBrowser
            // 
            this.BindingSource.SetBindingMember(this.MessageInterpretationWebBrowser, "Messages.EM_MessageInterpretation");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Messages)).SyncRoot)).EM_MessageInterpretation)));
            this.MessageInterpretationWebBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MessageInterpretationWebBrowser.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            this.MessageInterpretationWebBrowser.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 8, 3, 3, true);
            this.MessageInterpretationWebBrowser.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(20, 20, true);
            this.MessageInterpretationWebBrowser.Name = "MessageInterpretationWebBrowser";
            this.MessageInterpretationWebBrowser.ScriptErrorsSuppressed = true;
            this.MessageInterpretationWebBrowser.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(846, 283, true);
            this.MessageInterpretationWebBrowser.TabIndex = 0;
            this.MessageInterpretationWebBrowser.UseFixedWidthForPlainText = true;
            // 
            // zTabPageMessageText
            // 
            this.zTabPageMessageText.CaptionResourceString = Enterprise.Customs.ZA.GUI.Res.GetData("75f4bda2-98ed-4fc5-a2f4-e4a1d3dab9bf", "Text");
            this.zTabPageMessageText.Controls.Add(this.MessageTextTextBox);
            this.zTabPageMessageText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            this.zTabPageMessageText.Name = "zTabPageMessageText";
            this.zTabPageMessageText.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.zTabPageMessageText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(852, 289, true);
            this.zTabPageMessageText.TabIndex = 1;
            this.zTabPageMessageText.UseVisualStyleBackColor = true;
            // 
            // MessageTextTextBox
            // 
            this.BindingSource.SetBindingMember(this.MessageTextTextBox, "Messages.EM_FormattedMessageText");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.ZA.Business.AsycudaManifestHeader)(null)).Messages)).SyncRoot)).EM_FormattedMessageText)));
            this.MessageTextTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.MessageTextTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MessageTextTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            this.MessageTextTextBox.Multiline = true;
            this.MessageTextTextBox.Name = "MessageTextTextBox";
            this.MessageTextTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.MessageTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(846, 283, true);
            this.MessageTextTextBox.TabIndex = 0;
            // 
            // kSplitContainer
            // 
            this.kSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.kSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.kSplitContainer.Name = "kSplitContainer";
            this.kSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // kSplitContainer.Panel1
            // 
            this.kSplitContainer.Panel1.AutoScroll = true;
            this.kSplitContainer.Panel1.Controls.Add(this.zGridMessage);
            // 
            // kSplitContainer.Panel2
            // 
            this.kSplitContainer.Panel2.Controls.Add(this.zTabControlMessage);
            this.kSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(860, 418, true);
            this.kSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(98);
            this.kSplitContainer.TabIndex = 2;
            // 
            // OutturnAndGateInOutMessageUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.kSplitContainer);
            this.Name = "OutturnAndGateInOutMessageUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(860, 418, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.zGridMessage)).EndInit();
            this.zGridMessage.ResumeLayout(false);
            this.zGridMessage.PerformLayout();
            this.zTabControlMessage.ResumeLayout(false);
            this.zTabControlMessage.PerformLayout();
            this.zTabPageMessageDetail.ResumeLayout(false);
            this.zTabPageMessageDetail.PerformLayout();
            this.zTabPageMessageText.ResumeLayout(false);
            this.zTabPageMessageText.PerformLayout();
            this.kSplitContainer.Panel1.ResumeLayout(false);
            this.kSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.kSplitContainer)).EndInit();
            this.kSplitContainer.ResumeLayout(false);
            this.kSplitContainer.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZTabControl zTabControlMessage;
		private ZTabPage zTabPageMessageText;
		private ZTabPage zTabPageMessageDetail;
		private ZArchitecture.ZTextBox MessageTextTextBox;
		private CargoWise.Windows.UI.KSplitContainer kSplitContainer;
		private ZArchitecture.ZGrid zGridMessage;
		private HtmlInterpretationBox MessageInterpretationWebBrowser;
	}
}
