
namespace Enterprise.Customs.US.GUI
{
	partial class StatementMessagesUserControl
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
			this.MessageDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MessageDetailsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MainPanel.SuspendLayout();
			this.MessagesTabControl.SuspendLayout();
			this.MessageTextTabPage.SuspendLayout();
			this.HistoryGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessagesGrid)).BeginInit();
			this.MessageDetailsTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(846, 497, true);
			// 
			// MessagesTabControl
			// 
			this.MessagesTabControl.Controls.Add(this.MessageDetailsTabPage);
			this.MessagesTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(284, 497, true);
			this.MessagesTabControl.Controls.SetChildIndex(this.MessageDetailsTabPage, 0);
			this.MessagesTabControl.Controls.SetChildIndex(this.MessageTextTabPage, 0);
			// 
			// MessageTextTabPage
			// 
			this.MessageTextTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(276, 470, true);
			// 
			// MessageTextTextBox
			// 
			this.MessageTextTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 464, true);
			// 
			// HistoryGroupBox
			// 
			this.HistoryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(559, 497, true);
			// 
			// MessagesGrid
			// 
			this.MessagesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(553, 478, true);
			// Compile time check for the above grid columns. If any of these lines fail, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.US.Business.EDIMessage)(((object)(((Enterprise.Customs.US.Business.CusStatementHeader)(null)).Messages)))).EM_MessageNumInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.EDIMessage)(((object)(((Enterprise.Customs.US.Business.CusStatementHeader)(null)).Messages)))).EM_MessageNum)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.US.Business.EDIMessage)(((object)(((Enterprise.Customs.US.Business.CusStatementHeader)(null)).Messages)))).EM_MessageSubTypeInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.EDIMessage)(((object)(((Enterprise.Customs.US.Business.CusStatementHeader)(null)).Messages)))).EM_MessageSubType)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.US.Business.EDIMessage)(((object)(((Enterprise.Customs.US.Business.CusStatementHeader)(null)).Messages)))).EM_SystemCreateTimeUtc)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.US.Business.EDIMessage)(((object)(((Enterprise.Customs.US.Business.CusStatementHeader)(null)).Messages)))).EM_SystemCreateTimeUtcInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.US.Business.EDIMessage)(((object)(((Enterprise.Customs.US.Business.CusStatementHeader)(null)).Messages)))).EM_InterchangeNumberInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.EDIMessage)(((object)(((Enterprise.Customs.US.Business.CusStatementHeader)(null)).Messages)))).EM_InterchangeNumber)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.US.Business.EDIMessage)(((object)(((Enterprise.Customs.US.Business.CusStatementHeader)(null)).Messages)))).EM_DateTimeInterchangeSent)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.US.Business.EDIMessage)(((object)(((Enterprise.Customs.US.Business.CusStatementHeader)(null)).Messages)))).EM_DateTimeInterchangeSentInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.US.Business.EDIMessage)(((object)(((Enterprise.Customs.US.Business.CusStatementHeader)(null)).Messages)))).EM_UserInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.EDIMessage)(((object)(((Enterprise.Customs.US.Business.CusStatementHeader)(null)).Messages)))).EM_User)));
			// 
			// MessageDetailsTabPage
			// 
			this.MessageDetailsTabPage.Controls.Add(this.MessageDetailsTextBox);
			this.MessageDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessageDetailsTabPage.Name = "MessageDetailsTabPage";
			this.MessageDetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MessageDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(276, 470, true);
			this.MessageDetailsTabPage.TabIndex = 1;
			this.MessageDetailsTabPage.Text = "Message Details";
			// 
			// MessageDetailsTextBox
			// 
			this.MessageDetailsTextBox.BindTo = "Messages.EM_MessageInterpretation";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Customs.US.Business.EDIMessage)(((object)(((Enterprise.Customs.US.Business.CusStatementHeader)(null)).Messages)))).EM_MessageInterpretationInfo)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZString)(((Enterprise.Customs.US.Business.EDIMessage)(((object)(((Enterprise.Customs.US.Business.CusStatementHeader)(null)).Messages)))).EM_MessageInterpretation)));
			this.MessageDetailsTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageDetailsTextBox.Font = new System.Drawing.Font("Courier New", 8F);
			this.MessageDetailsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MessageDetailsTextBox.Multiline = true;
			this.MessageDetailsTextBox.Name = "MessageDetailsTextBox";
			this.MessageDetailsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.MessageDetailsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 464, true);
			this.MessageDetailsTextBox.TabIndex = 2;
			this.MessageDetailsTextBox.WordWrap = false;
			// 
			// StatementMessagesUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.DataSourceAssemblyName = "Enterprise.Customs.US.Business";
			this.DataSourceTypeName = "Enterprise.Customs.US.Business.CusStatementHeader";
			this.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(507, 0, true);
			this.Name = "StatementMessagesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(846, 497, true);
			this.MainPanel.ResumeLayout(false);
			this.MessagesTabControl.ResumeLayout(false);
			this.MessageTextTabPage.ResumeLayout(false);
			this.MessageTextTabPage.PerformLayout();
			this.HistoryGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessagesGrid)).EndInit();
			this.MessageDetailsTabPage.ResumeLayout(false);
			this.MessageDetailsTabPage.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZTabPage MessageDetailsTabPage;
		private Enterprise.ZArchitecture.ZTextBox MessageDetailsTextBox;
	}
}
