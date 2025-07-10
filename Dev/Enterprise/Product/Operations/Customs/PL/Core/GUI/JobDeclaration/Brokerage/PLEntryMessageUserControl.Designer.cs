namespace Enterprise.Customs.PL.GUI
{
	public partial class PLEntryMessageUserControl
	{
		void InitializeComponent()
		{
			this.AttachmentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.AttachmentMessageUserControl = new AttachmentMessageUserControl();
			this.MessagesTabControl.SuspendLayout();
			this.DiscardedTabPage.SuspendLayout();
			this.discardedMessageUserContor.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MessagesTabControl
			// 
			this.MessagesTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(805, 567, true);
			this.MessagesTabControl.Controls.Add(this.AttachmentsTabPage);
			// 
			// MessagesTabPage
			// 
			this.MessagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(797, 540, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.PL.Business.Declaration.JobDeclaration);
			// 
			// AttachmentsTabPage
			// 
			this.AttachmentsTabPage.Controls.Add(this.AttachmentMessageUserControl);
			this.AttachmentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AttachmentsTabPage.Name = "AttachmentsTabPage";
			this.AttachmentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(832, 557, true);
			this.AttachmentsTabPage.TabIndex = 1;
			this.AttachmentsTabPage.CaptionResourceString = Enterprise.Customs.PL.GUI.Res.GetData("F47446B5-E508-4273-BF68-EF2846605146", "Attachment Messages");
			// 
			// AttachmentMessageUserControl
			// 
			this.AttachmentMessageUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AttachmentMessageUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AttachmentMessageUserControl.Name = "AttachmentMessageUserControl";
			this.AttachmentMessageUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(832, 557, true);
			this.AttachmentMessageUserControl.TabIndex = 0;
			// 
			// PLEntryMessageUserControl
			// 
			this.Name = "PLEntryMessageUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(805, 567, true);
			this.MessagesTabControl.ResumeLayout(false);
			this.MessagesTabControl.PerformLayout();
			this.DiscardedTabPage.ResumeLayout(false);
			this.DiscardedTabPage.PerformLayout();
			this.discardedMessageUserContor.ResumeLayout(true);
			this.discardedMessageUserContor.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		Enterprise.ZArchitecture.GUI.ZTabPage AttachmentsTabPage;
		AttachmentMessageUserControl AttachmentMessageUserControl;
	}
}
