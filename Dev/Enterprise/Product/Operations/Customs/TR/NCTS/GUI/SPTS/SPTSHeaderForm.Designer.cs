namespace Enterprise.Customs.TR.NCTS.GUI
{
	partial class SPTSHeaderForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.MessagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.sptsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.sptsHeaderUserControl = new Enterprise.Customs.TR.NCTS.GUI.SPTSHeaderUserControl();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MessagesTabPage.SuspendLayout();
			this.sptsPanel.SuspendLayout();
			this.sptsHeaderUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			//
			this.MainTabControl.Controls.Add(this.MessagesTabPage);
			this.MainTabControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1236, 0, true);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1274, 632, true);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MessagesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.CaptionResourceString = Enterprise.Customs.TR.NCTS.GUI.Res.GetData("SPTSForm|70F750B0-6525-4FBD-A05B-0211309F6E42", "Main");
			this.MainTabPage.Controls.Add(this.sptsPanel);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1266, 605, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1266, 605, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1266, 605, true);
			// 
			// MainPanel
			// 
			this.MainPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1236, 0, true);
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1274, 632, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1274, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TR.NCTS.Business.SPTSHeader);
			// 
			// MessagesTabPage
			//
			this.MessagesTabPage.CaptionResourceString = Enterprise.Customs.TR.NCTS.GUI.Res.GetData("2CC96DD3-F824-41AD-802E-9CE1F0F99BCD", "Messages");
			this.MessagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessagesTabPage.Name = "MessagesTabPage";
			this.MessagesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MessagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1153, 633, true);
			this.MessagesTabPage.TabIndex = 4;
			this.MessagesTabPage.Text = Enterprise.Customs.TR.NCTS.GUI.Res.GetString("58C26C3E-3882-4FD4-99C5-40EAD273D68A", "Messages");
			this.MessagesTabPage.UseVisualStyleBackColor = true;
			this.MessagesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MessagesTabPage_InitializeTab));
			// 
			// trsptsPanel
			// 
			this.sptsPanel.Controls.Add(this.sptsHeaderUserControl);
			this.sptsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.sptsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.sptsPanel.Name = "trsptsPanel";
			this.sptsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1266, 605, true);
			this.sptsPanel.TabIndex = 1;
			// 
			// trsptsHeaderUserControl
			// 
			this.sptsHeaderUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.sptsHeaderUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.TR.NCTS.Business.SPTSHeader)(((Enterprise.Customs.TR.NCTS.Business.SPTSHeader)(null)))));
			this.sptsHeaderUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.sptsHeaderUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.sptsHeaderUserControl.Name = "trsptsHeaderUserControl";
			this.sptsHeaderUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1266, 605, true);
			this.sptsHeaderUserControl.TabIndex = 0;
			// 
			// SPTSHeaderForm
			//
			this.CaptionRenderingEnabled = true;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1274, 688, true);
			this.DataSourceType = typeof(Enterprise.Customs.TR.NCTS.Business.SPTSHeader);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1280, 725, true);
			this.Name = "SPTSHeaderForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = Enterprise.Customs.TR.NCTS.GUI.Res.GetString("0E31478D-CB38-4682-AD2A-A8738DCD837B", "SPTS");
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(false);
			this.NotesTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MessagesTabPage.ResumeLayout(false);
			this.MessagesTabPage.PerformLayout();
			this.sptsPanel.ResumeLayout(false);
			this.sptsPanel.PerformLayout();
			this.sptsHeaderUserControl.ResumeLayout(true);
			this.sptsHeaderUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected ZArchitecture.GUI.ZTabPage MessagesTabPage;
		SPTSMessagesTabUserControl MessagesUserControl;
		private ZArchitecture.GUI.ZPanel sptsPanel;
		private SPTSHeaderUserControl sptsHeaderUserControl;
	}
}
