namespace Enterprise.Customs.PL.GUI
{
	partial class RetrospectiveQuotaRequestSendingForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.AdditionalDataTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.EDocsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MessageSendingEDocsUserControl = new Enterprise.Customs.PL.GUI.MessageSendingEDocsUserControl();
			this.EntryLinesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MessageSendingEntryLinesGridUserControl = new Enterprise.Customs.PL.GUI.MessageSendingEntryLinesGridUserControl();
			this.AdditionalDataGroupBox.SuspendLayout();
			this.messageSendingObjectsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageSendingObjectsGrid)).BeginInit();
			this.MessageSendingObjectsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AdditionalDataTabControl.SuspendLayout();
			this.EDocsTabPage.SuspendLayout();
			this.MessageSendingEDocsUserControl.SuspendLayout();
			this.EntryLinesTabPage.SuspendLayout();
			this.MessageSendingEntryLinesGridUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// AdditionalDataGroupBox
			// 
			this.AdditionalDataGroupBox.Controls.Add(this.AdditionalDataTabControl);
			// 
			// SendButton
			// 
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(647, 827, true);
			// 
			// CancelButton2
			// 
			this.CancelButton2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(744, 827, true);
			// 
			// messageSendingObjectsGroupBox
			// 
			this.messageSendingObjectsGroupBox.CaptionResourceString = Enterprise.Customs.PL.GUI.Res.GetData("2EDA115D-00A5-4ED8-9064-E158BBB16A3C", "Messages to be sent - Retrospective Quota Request");
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 853, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(868, 23, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.PL.Business.RetrospectiveQuotaRequestMessageSendingObjectParent);
			// 
			// AdditionalDataTabControl
			// 
			this.AdditionalDataTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.AdditionalDataTabControl.Controls.Add(this.EDocsTabPage);
			this.AdditionalDataTabControl.Controls.Add(this.EntryLinesTabPage);
			this.AdditionalDataTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalDataTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.AdditionalDataTabControl.Name = "AdditionalDataTabControl";
			this.AdditionalDataTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(821, 209, true);
			this.AdditionalDataTabControl.TabIndex = 0;
			// 
			// EDocsTabPage
			// 
			this.EDocsTabPage.CaptionResourceString = Enterprise.Customs.PL.GUI.Res.GetData("A65DE8D7-B44A-4878-AD80-308BBE16B718", "eDocs to be sent");
			this.EDocsTabPage.Controls.Add(this.MessageSendingEDocsUserControl);
			this.EDocsTabPage.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EDocsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.EDocsTabPage.Name = "EDocsTabPage";
			this.EDocsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(814, 186, true);
			this.EDocsTabPage.TabIndex = 0;
			// 
			// MessageSendingEDocsUserControl
			// 
			this.MessageSendingEDocsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MessageSendingEDocsUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.PL.Business.BaseMessageSendingObjectParent)(((Enterprise.Customs.PL.Business.RetrospectiveQuotaRequestMessageSendingObjectParent)(null)))));
			this.MessageSendingEDocsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageSendingEDocsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessageSendingEDocsUserControl.Name = "MessageSendingEDocsUserControl";
			this.MessageSendingEDocsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(814, 186, true);
			this.MessageSendingEDocsUserControl.TabIndex = 0;
			// 
			// EntryLinesTabPage
			// 
			this.EntryLinesTabPage.CaptionResourceString = Enterprise.Customs.PL.GUI.Res.GetData("D8E967CB-F242-42FE-B606-A8234D3BAB28", "ZCX05 Entry Lines");
			this.EntryLinesTabPage.Controls.Add(this.MessageSendingEntryLinesGridUserControl);
			this.EntryLinesTabPage.Dock = System.Windows.Forms.DockStyle.Fill;
			this.EntryLinesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.EntryLinesTabPage.Name = "EntryLinesTabPage";
			this.EntryLinesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(814, 186, true);
			this.EntryLinesTabPage.TabIndex = 0;
			// 
			// MessageSendingEntryLinesGridUserControl
			// 
			this.MessageSendingEntryLinesGridUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MessageSendingEntryLinesGridUserControl, ".");
			this.MessageSendingEntryLinesGridUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageSendingEntryLinesGridUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessageSendingEntryLinesGridUserControl.Name = "MessageSendingEntryLinesGridUserControl";
			this.MessageSendingEntryLinesGridUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(814, 186, true);
			this.MessageSendingEntryLinesGridUserControl.TabIndex = 0;
			// 
			// RetrospectiveQuotaRequestSendingForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(868, 876, true);
			this.DataSourceType = typeof(Enterprise.Customs.PL.Business.RetrospectiveQuotaRequestMessageSendingObjectParent);
			this.Name = "RetrospectiveQuotaRequestSendingForm";
			this.ShouldSerializeTabPageMethods = false;
			this.AdditionalDataGroupBox.ResumeLayout(false);
			this.AdditionalDataGroupBox.PerformLayout();
			this.messageSendingObjectsGroupBox.ResumeLayout(false);
			this.messageSendingObjectsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageSendingObjectsGrid)).EndInit();
			this.MessageSendingObjectsGrid.ResumeLayout(false);
			this.MessageSendingObjectsGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AdditionalDataTabControl.ResumeLayout(false);
			this.AdditionalDataTabControl.PerformLayout();
			this.EDocsTabPage.ResumeLayout(false);
			this.EDocsTabPage.PerformLayout();
			this.MessageSendingEDocsUserControl.ResumeLayout(true);
			this.MessageSendingEDocsUserControl.PerformLayout();
			this.EntryLinesTabPage.ResumeLayout(false);
			this.EntryLinesTabPage.PerformLayout();
			this.MessageSendingEntryLinesGridUserControl.ResumeLayout(true);
			this.MessageSendingEntryLinesGridUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZTabControl AdditionalDataTabControl;
		internal ZArchitecture.GUI.ZTabPage EntryLinesTabPage;
		private ZArchitecture.GUI.ZTabPage EDocsTabPage;
		private MessageSendingEDocsUserControl MessageSendingEDocsUserControl;
		private MessageSendingEntryLinesGridUserControl MessageSendingEntryLinesGridUserControl;
	}
}
