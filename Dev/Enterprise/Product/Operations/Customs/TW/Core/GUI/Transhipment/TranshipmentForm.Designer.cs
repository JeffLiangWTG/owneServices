using System;

namespace Enterprise.Customs.TW.GUI
{
	partial class TranshipmentForm
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

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			this.MessagesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MainTabControl.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.MessagesTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1161, 648, true);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MessagesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1153, 642, true);
			this.MainTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MainTabPage_InitializeTab));
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1153, 642, true);
			this.NotesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.NotesTabPage_InitializeTab));
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1153, 621, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1161, 648, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1161, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TW.Business.CusInBondHeader);
			// 
			// MessagesTabPage
			// 
			this.MessagesTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.MessagesTabPage.CaptionResourceString = Enterprise.Customs.TW.GUI.Res.GetData("TWInBondForm|B2EC7901-E696-4448-9B0C-28BDCE2433D5", "Messages");
			this.MessagesTabPage.CheckForNotifications = false;
			this.MessagesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.MessagesTabPage.Name = "MessagesTabPage";
			this.MessagesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.MessagesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1153, 642, true);
			this.MessagesTabPage.TabIndex = 5;
			this.MessagesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MessagesTabPage_InitializeTab));
			// 
			// TWInBondForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1161, 704, true);
			this.DataSourceType = typeof(Enterprise.Customs.TW.Business.CusInBondHeader);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1161, 725, true);
			this.Name = "TWInBondForm";
			this.ShouldSerializeTabPageMethods = true;
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}


		void NotesTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.NotesTabPage.SuspendLayout();
			this.NotesTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(true);

		}

		void MainTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.TWInBondHeaderDetailUserControl = new Enterprise.Customs.TW.GUI.TranshipmentHeaderDetailUserControl();
			this.MainTabPage.SuspendLayout();
			this.TWInBondHeaderDetailUserControl.SuspendLayout();
			this.MainTabPage.Controls.Add(this.TWInBondHeaderDetailUserControl);
			// 
			// TWInBondHeaderDetailUserControl
			// 
			this.TWInBondHeaderDetailUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TWInBondHeaderDetailUserControl, ".");
			this.TWInBondHeaderDetailUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TWInBondHeaderDetailUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TWInBondHeaderDetailUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(996, 526, true);
			this.TWInBondHeaderDetailUserControl.Name = "TWInBondHeaderDetailUserControl";
			this.TWInBondHeaderDetailUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1153, 642, true);
			this.TWInBondHeaderDetailUserControl.TabIndex = 0;
			this.MainTabPage.PerformLayout();
			this.TWInBondHeaderDetailUserControl.ResumeLayout(true);
			this.TWInBondHeaderDetailUserControl.PerformLayout();
			this.MainTabPage.ResumeLayout(true);

		}

		void MessagesTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.MessageDetailsUserControl = new Enterprise.Customs.TW.GUI.TranshipmentMessageDetailsUserControl();
			this.MessagesTabPage.SuspendLayout();
			this.MessageDetailsUserControl.SuspendLayout();
			this.MessagesTabPage.Controls.Add(this.MessageDetailsUserControl);
			// 
			// MessageDetailsUserControl
			// 
			this.MessageDetailsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MessageDetailsUserControl, ".");
			this.MessageDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessageDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.MessageDetailsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(996, 526, true);
			this.MessageDetailsUserControl.Name = "MessageDetailsUserControl";
			this.MessageDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1147, 636, true);
			this.MessageDetailsUserControl.TabIndex = 0;
			this.MessagesTabPage.PerformLayout();
			this.MessageDetailsUserControl.ResumeLayout(true);
			this.MessageDetailsUserControl.PerformLayout();
			this.MessagesTabPage.ResumeLayout(true);

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZTabPage MessagesTabPage;
		private TranshipmentMessageDetailsUserControl MessageDetailsUserControl;
		internal TranshipmentHeaderDetailUserControl TWInBondHeaderDetailUserControl;
	}
}
