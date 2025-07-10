using Enterprise.Customs.NZ.Business.Declaration.OutwardReport;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Customs.NZ.GUI
{
	public partial class ConsolManifestUserControl
	{
		void InitializeComponent()
		{
			this.interpretedGroupBox = new ZArchitecture.GUI.ZGroupBox();
			this.eM_InterpretedTextBox = new ZArchitecture.ZTextBox();
			this.messagingModeDropEdit = new ZArchitecture.GUI.ZDropEdit();
			this.messagingModelbl = new CargoWise.Windows.UI.KLabel();
			this.TopPanel.SuspendLayout();
			this.HistoryGroupBox.SuspendLayout();
			this.MessageTextGroupBox.SuspendLayout();
			this.HistoryPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessagesGrid)).BeginInit();
			this.MessageTextPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.interpretedGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// TopPanel
			// 
			this.TopPanel.Controls.Add(this.messagingModelbl);
			this.TopPanel.Controls.Add(this.messagingModeDropEdit);
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(936, 40, true);
			this.TopPanel.Controls.SetChildIndex(this.CustomsEntryNumberTextBox, 0);
			this.TopPanel.Controls.SetChildIndex(this.E2_MessageStatusBoundTextBox, 0);
			this.TopPanel.Controls.SetChildIndex(this.CRNLabel, 0);
			this.TopPanel.Controls.SetChildIndex(this.StatusLabel, 0);
			this.TopPanel.Controls.SetChildIndex(this.messagingModeDropEdit, 0);
			this.TopPanel.Controls.SetChildIndex(this.messagingModelbl, 0);
			// 
			// CRNLabel
			// 
			this.CRNLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(116, 23, true);
			// 
			// StatusLabel
			// 
			this.StatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(600, 8, true);
			this.StatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(55, 23, true);
			// 
			// MessageTextGroupBox
			// 
			this.MessageTextGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right);
			this.MessageTextGroupBox.Dock = System.Windows.Forms.DockStyle.None;
			this.MessageTextGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(333, 256, true);
			// 
			// MessagesGrid
			// 
			this.MessagesGrid.TabIndex = 3;
			// 
			// MessageTextPanel
			// 
			this.MessageTextPanel.Controls.Add(this.interpretedGroupBox);
			this.MessageTextPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(333, 504, true);
			this.MessageTextPanel.Controls.SetChildIndex(this.interpretedGroupBox, 0);
			this.MessageTextPanel.Controls.SetChildIndex(this.MessageTextGroupBox, 0);
			// 
			// MessageTextTextBox
			// 
			this.MessageTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(327, 237, true);
			// 
			// E2_MessageStatusBoundTextBox
			// 
			this.E2_MessageStatusBoundTextBox.BackColor = System.Drawing.SystemColors.Control;
			this.E2_MessageStatusBoundTextBox.ForeColor = System.Drawing.SystemColors.WindowText;
			this.E2_MessageStatusBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(661, 9, true);
			this.E2_MessageStatusBoundTextBox.TabIndex = 2;
			// 
			// CustomsEntryNumberTextBox
			// 
			this.CustomsEntryNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 9, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(OutwardReportManifestStatus);
			// 
			// InterpretedGroupBox
			// 
			this.interpretedGroupBox.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right);
			this.interpretedGroupBox.CaptionResourceString = Enterprise.Customs.NZ.GUI.Res.GetData("B2984621-0648-4F5E-8B99-975BD7932239", "Interpreted Message");
			this.interpretedGroupBox.Controls.Add(this.eM_InterpretedTextBox);
			this.interpretedGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 256, true);
			this.interpretedGroupBox.Name = "InterpretedGroupBox";
			this.interpretedGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(333, 248, true);
			this.interpretedGroupBox.TabIndex = 1;
			this.interpretedGroupBox.TabStop = false;
			// 
			// EM_InterpretedTextBox
			// 
			this.BindingSource.SetBindingMember(this.eM_InterpretedTextBox, "MessagesIncludingInterchangeRejections.EM_MessageInterpretation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Messaging.Business.EDIMessage)(((System.Collections.IList)(((OutwardReportManifestStatus)(null)).MessagesIncludingInterchangeRejections)).SyncRoot)).EM_MessageInterpretation);
			this.eM_InterpretedTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.eM_InterpretedTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.eM_InterpretedTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.eM_InterpretedTextBox.Multiline = true;
			this.eM_InterpretedTextBox.Name = "EM_InterpretedTextBox";
			this.eM_InterpretedTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(327, 229, true);
			this.eM_InterpretedTextBox.TabIndex = 0;
			// 
			// MessagingModeDropEdit
			// 
			this.messagingModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.messagingModeDropEdit, "E2_MessagingMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((OutwardReportManifestStatus)(null)).E2_MessagingMode);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((OutwardReportManifestStatus)(null)).E2_MessagingModeDesc);
			this.messagingModeDropEdit.BindToForDescription = "E2_MessagingModeDesc";
			this.messagingModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(420, 10, true);
			this.messagingModeDropEdit.Name = "MessagingModeDropEdit";
			this.messagingModeDropEdit.PreBoundMaxLength = 3;
			this.messagingModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(107, 20, true);
			this.messagingModeDropEdit.TabIndex = 1;
			// 
			// MessagingModelbl
			// 
			this.messagingModelbl.AutoSize = true;
			this.messagingModelbl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(320, 13, true);
			this.messagingModelbl.Name = "MessagingModelbl";
			this.messagingModelbl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 13, true);
			this.messagingModelbl.TabIndex = 11;
			this.messagingModelbl.Text = "Messaging Mode: ";
			// 
			// ConsolManifestUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "ConsolManifestUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(936, 544, true);
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			this.HistoryGroupBox.ResumeLayout(false);
			this.MessageTextGroupBox.ResumeLayout(false);
			this.MessageTextGroupBox.PerformLayout();
			this.HistoryPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessagesGrid)).EndInit();
			this.MessageTextPanel.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.interpretedGroupBox.ResumeLayout(false);
			this.interpretedGroupBox.PerformLayout();
			this.ResumeLayout(false);
		}

	}
}
