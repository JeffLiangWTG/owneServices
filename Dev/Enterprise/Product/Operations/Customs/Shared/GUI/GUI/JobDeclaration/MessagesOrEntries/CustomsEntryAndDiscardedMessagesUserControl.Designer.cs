using System.ComponentModel;

namespace Enterprise.Customs.GUI
{
	partial class CustomsEntryAndDiscardedMessagesUserControl
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		protected Enterprise.ZArchitecture.GUI.ZTemplateTabControl MessagesTabControl;
		protected Enterprise.ZArchitecture.GUI.ZTabPage MessagesTabPage;
		protected Enterprise.ZArchitecture.GUI.ZTabPage DiscardedTabPage;
		protected Enterprise.Customs.GUI.DiscardedMessageUserControl discardedMessageUserContor;
		private IContainer components;

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.MessagesTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.MessagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.DiscardedTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.discardedMessageUserContor = new Enterprise.Customs.GUI.DiscardedMessageUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MessagesTabControl.SuspendLayout();
			this.DiscardedTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// MessagesTabControl
			// 
			this.MessagesTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.MessagesTabControl.Controls.Add(this.MessagesTabPage);
			this.MessagesTabControl.Controls.Add(this.DiscardedTabPage);
			this.MessagesTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessagesTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MessagesTabControl.Name = "MessagesTabControl";
			this.MessagesTabControl.SelectedIndex = 0;
			this.MessagesTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(840, 584, true);
			this.MessagesTabControl.TabIndex = 0;
			// 
			// MessagesTabPage
			// 
			this.MessagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessagesTabPage.Name = "MessagesTabPage";
			this.MessagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(832, 557, true);
			this.MessagesTabPage.TabIndex = 0;
			this.MessagesTabPage.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("21AEF37C-6095-4493-9700-B74AD00569BE", "Messages");
			// 
			// DiscardedTabPage
			// 
			this.DiscardedTabPage.Controls.Add(this.discardedMessageUserContor);
			this.DiscardedTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DiscardedTabPage.Name = "DiscardedTabPage";
			this.DiscardedTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(832, 557, true);
			this.DiscardedTabPage.TabIndex = 1;
			this.DiscardedTabPage.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("71F78379-D3F9-40BA-8469-6D2452E1BDF8", "Discarded Messages");
			// 
			// discardedMessageUserContor
			// 
			this.discardedMessageUserContor.Dock = System.Windows.Forms.DockStyle.Fill;
			this.discardedMessageUserContor.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.discardedMessageUserContor.Name = "discardedMessageUserContor";
			this.discardedMessageUserContor.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(832, 557, true);
			this.discardedMessageUserContor.TabIndex = 0;
			// 
			// CustomsEntryAndDiscardedMessagesUserControl
			//
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MessagesTabControl);
			this.Name = "CustomsEntryAndDiscardedMessagesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(840, 584, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MessagesTabControl.ResumeLayout(false);
			this.DiscardedTabPage.ResumeLayout(false);
			this.ResumeLayout(false);
		}
		#endregion
	}
}
