namespace Enterprise.Customs.NZ.GUI
{
	public partial class OCRManifestUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.interpretedGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.eM_InterpretedTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.notifyPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.NotifyPartyPortFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.NotifyPartyEmailTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NotifyPartyNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NotifyPartyAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.TopPanel.SuspendLayout();
			this.HistoryGroupBox.SuspendLayout();
			this.MessageTextGroupBox.SuspendLayout();
			this.HistoryPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessagesGrid)).BeginInit();
			this.MessagesGrid.SuspendLayout();
			this.MessageTextPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.interpretedGroupBox.SuspendLayout();
			this.notifyPanel.SuspendLayout();
			this.NotifyPartyPortFindBox.SuspendLayout();
			this.NotifyPartyAddressControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// TopPanel
			// 
			this.TopPanel.Controls.Add(this.notifyPanel);
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(948, 104, true);
			this.TopPanel.TabIndex = 0;
			this.TopPanel.Controls.SetChildIndex(this.CustomsEntryNumberTextBox, 0);
			this.TopPanel.Controls.SetChildIndex(this.E2_MessageStatusBoundTextBox, 0);
			this.TopPanel.Controls.SetChildIndex(this.CRNLabel, 0);
			this.TopPanel.Controls.SetChildIndex(this.StatusLabel, 0);
			this.TopPanel.Controls.SetChildIndex(this.notifyPanel, 0);
			// 
			// CRNLabel
			// 
			this.CRNLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 28, true);
			this.CRNLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(122, 23, true);
			this.CRNLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// StatusLabel
			// 
			this.StatusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.StatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 53, true);
			this.StatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 23, true);
			this.StatusLabel.TabIndex = 0;
			this.StatusLabel.Text = "";
			this.StatusLabel.Visible = false;
			// 
			// HistoryGroupBox
			// 
			this.HistoryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 508, true);
			// 
			// MessageTextGroupBox
			// 
			this.MessageTextGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.MessageTextGroupBox.Dock = System.Windows.Forms.DockStyle.None;
			this.MessageTextGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(345, 256, true);
			// 
			// HistoryPanel
			// 
			this.HistoryPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 104, true);
			this.HistoryPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 508, true);
			// 
			// MessagesGrid
			// 
			this.MessagesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(594, 489, true);
			// 
			// TheSplitter
			// 
			this.TheSplitter.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.TheSplitter.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.TheSplitter.Dock = System.Windows.Forms.DockStyle.Left;
			this.TheSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(600, 104, true);
			this.TheSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(3, 508, true);
			// 
			// MessageTextPanel
			// 
			this.MessageTextPanel.Controls.Add(this.interpretedGroupBox);
			this.MessageTextPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(603, 104, true);
			this.MessageTextPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(345, 508, true);
			this.MessageTextPanel.Controls.SetChildIndex(this.interpretedGroupBox, 0);
			this.MessageTextPanel.Controls.SetChildIndex(this.MessageTextGroupBox, 0);
			// 
			// MessageTextTextBox
			// 
			this.MessageTextTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.MessageTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(339, 237, true);
			// 
			// E2_MessageStatusBoundTextBox
			// 
			this.E2_MessageStatusBoundTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.E2_MessageStatusBoundTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.E2_MessageStatusBoundTextBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("e2d90324-9c17-4516-aa23-e3e9ed0b04e3", "Status");
			this.E2_MessageStatusBoundTextBox.ForeColor = System.Drawing.SystemColors.WindowText;
			this.E2_MessageStatusBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 6, true);
			// 
			// CustomsEntryNumberTextBox
			// 
			this.CustomsEntryNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(132, 30, true);
			this.CustomsEntryNumberTextBox.TabIndex = 1;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.NZ.Business.Declaration.OutwardReport.OutwardReportManifestStatus);
			// 
			// InterpretedGroupBox
			// 
			this.interpretedGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.interpretedGroupBox.Controls.Add(this.eM_InterpretedTextBox);
			this.interpretedGroupBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("6243665D-8095-4AD2-AEC6-CAC6BA166682", "Interpreted Message");
			this.interpretedGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 256, true);
			this.interpretedGroupBox.Name = "InterpretedGroupBox";
			this.interpretedGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(345, 252, true);
			this.interpretedGroupBox.TabIndex = 1;
			this.interpretedGroupBox.TabStop = false;
			// 
			// EM_InterpretedTextBox
			// 
			this.BindingSource.SetBindingMember(this.eM_InterpretedTextBox, "MessagesIncludingInterchangeRejections.EM_MessageInterpretation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Messaging.Business.EDIMessage)(((System.Collections.IList)(((Enterprise.Customs.NZ.Business.Declaration.OutwardReport.OutwardReportManifestStatus)(null)).MessagesIncludingInterchangeRejections)).SyncRoot)).EM_MessageInterpretation)));
			this.eM_InterpretedTextBox.CaptionResourceString = null;
			this.eM_InterpretedTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.eM_InterpretedTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.eM_InterpretedTextBox.Font = new System.Drawing.Font("Courier New", 8F);
			this.eM_InterpretedTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.eM_InterpretedTextBox.Multiline = true;
			this.eM_InterpretedTextBox.Name = "EM_InterpretedTextBox";
			this.eM_InterpretedTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.eM_InterpretedTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.eM_InterpretedTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(339, 233, true);
			this.eM_InterpretedTextBox.TabIndex = 0;
			// 
			// NotifyPanel
			// 
			this.notifyPanel.Controls.Add(this.NotifyPartyPortFindBox);
			this.notifyPanel.Controls.Add(this.NotifyPartyEmailTextBox);
			this.notifyPanel.Controls.Add(this.NotifyPartyNameTextBox);
			this.notifyPanel.Controls.Add(this.NotifyPartyAddressControl);
			this.notifyPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(428, 3, true);
			this.notifyPanel.Name = "NotifyPanel";
			this.notifyPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(517, 99, true);
			this.notifyPanel.TabIndex = 0;
			// 
			// NotifyPartyPortFindBox
			// 
			this.NotifyPartyPortFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NotifyPartyPortFindBox, "DeliveryNotificationParty.DeliveryNotificationPartyPort");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.Declaration.OutwardReport.OutwardReportManifestStatus)(null)).DeliveryNotificationParty.DeliveryNotificationPartyPort)));
			this.NotifyPartyPortFindBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("DDF6558A-2118-4562-A3FE-D9A95C46B766", "Delivery Notification Port");
			this.NotifyPartyPortFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 74, true);
			this.NotifyPartyPortFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.NotifyPartyPortFindBox.Name = "NotifyPartyPortFindBox";
			this.NotifyPartyPortFindBox.PreBoundMaxLength = 5;
			this.NotifyPartyPortFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(188, 20, true);
			this.NotifyPartyPortFindBox.TabIndex = 3;
			// 
			// NotifyPartyEmailTextBox
			// 
			this.BindingSource.SetBindingMember(this.NotifyPartyEmailTextBox, "DeliveryNotificationParty.DeliveryNotificationPartyEmail");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.Declaration.OutwardReport.OutwardReportManifestStatus)(null)).DeliveryNotificationParty.DeliveryNotificationPartyEmail)));
			this.NotifyPartyEmailTextBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("b623d834-e6b0-4c1a-8f7c-0a5b6674bbba", "Email", "A valid email address must be included in the associated communication details.");
			this.NotifyPartyEmailTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.NotifyPartyEmailTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 50, true);
			this.NotifyPartyEmailTextBox.Name = "NotifyPartyEmailTextBox";
			this.NotifyPartyEmailTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.NotifyPartyEmailTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(337, 20, true);
			this.NotifyPartyEmailTextBox.TabIndex = 2;
			// 
			// NotifyPartyNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.NotifyPartyNameTextBox, "DeliveryNotificationParty.DeliveryNotificationPartyName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.NZ.Business.Declaration.OutwardReport.OutwardReportManifestStatus)(null)).DeliveryNotificationParty.DeliveryNotificationPartyName)));
			this.NotifyPartyNameTextBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("8d33063d-3028-4469-bead-9298486d5f1c", "", "Notify Party", "Delivery Notification Party", "State the name of a Delivery notification party to be notified by TSW when the OCR is accepted.");
			this.NotifyPartyNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 27, true);
			this.NotifyPartyNameTextBox.Name = "NotifyPartyNameTextBox";
			this.NotifyPartyNameTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.NotifyPartyNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(337, 20, true);
			this.NotifyPartyNameTextBox.TabIndex = 1;
			this.NotifyPartyNameTextBox.Tag = "";
			// 
			// NotifyPartyAddressControl
			// 
			this.NotifyPartyAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NotifyPartyAddressControl, "DeliveryNotificationParty.E2_OA_DeliveryNotificationParty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.NZ.Business.Declaration.OutwardReport.OutwardReportManifestStatus)(null)).DeliveryNotificationParty.E2_OA_DeliveryNotificationParty)));
			this.NotifyPartyAddressControl.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("9032b8e2-2fe3-4374-be4c-09adfd3d192d", "Delivery Notification Organization", "Select an organization with an identification code issued by TSW for the party registered to receive delivery notifications by TSW when the OCR is accepted. No address or communication details need be transmitted for a registered Delivery notification party.");
			this.NotifyPartyAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 3, true);
			this.NotifyPartyAddressControl.Name = "NotifyPartyAddressControl";
			this.NotifyPartyAddressControl.PopupCaption = null;
			this.NotifyPartyAddressControl.ReadOnly = false;
			this.NotifyPartyAddressControl.ShowAddress = false;
			this.NotifyPartyAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.NotifyPartyAddressControl.TabIndex = 0;
			// 
			// OCRManifestUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "OCRManifestUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(948, 612, true);
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			this.HistoryGroupBox.ResumeLayout(false);
			this.HistoryGroupBox.PerformLayout();
			this.MessageTextGroupBox.ResumeLayout(false);
			this.MessageTextGroupBox.PerformLayout();
			this.HistoryPanel.ResumeLayout(false);
			this.HistoryPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessagesGrid)).EndInit();
			this.MessagesGrid.ResumeLayout(false);
			this.MessagesGrid.PerformLayout();
			this.MessageTextPanel.ResumeLayout(false);
			this.MessageTextPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.interpretedGroupBox.ResumeLayout(false);
			this.interpretedGroupBox.PerformLayout();
			this.notifyPanel.ResumeLayout(false);
			this.notifyPanel.PerformLayout();
			this.NotifyPartyPortFindBox.ResumeLayout(true);
			this.NotifyPartyPortFindBox.PerformLayout();
			this.NotifyPartyAddressControl.ResumeLayout(true);
			this.NotifyPartyAddressControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox NotifyPartyEmailTextBox;
		private ZArchitecture.ZTextBox NotifyPartyNameTextBox;
		private ZArchitecture.GUI.ZAddressControl NotifyPartyAddressControl;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox NotifyPartyPortFindBox;
	}
}
