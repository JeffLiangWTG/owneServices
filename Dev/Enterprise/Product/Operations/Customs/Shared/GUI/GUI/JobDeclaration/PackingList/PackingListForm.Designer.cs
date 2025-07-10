using System;

namespace Enterprise.Customs.GUI
{
	partial class PackingListForm
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
		protected new void InitializeComponent()
		{
			this.CustomTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MainTabControl.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.CustomTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1352, 630, true);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.CustomTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1344, 603, true);
			this.MainTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MainTabPage_InitializeTab));
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1344, 603, true);
			this.NotesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.NotesTabPage_InitializeTab));
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1344, 603, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1352, 630, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1352, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.CusPackingList);
			// 
			// CustomTabPage
			// 
			this.CustomTabPage.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("61149fe1-d9ce-4259-be79-9a6388156a51", "Custom");
			this.CustomTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CustomTabPage.Name = "CustomTabPage";
			this.CustomTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1344, 603, true);
			this.CustomTabPage.TabIndex = 3;
			this.CustomTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.CustomTabPage_InitializeTab));
			// 
			// PackingListForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1352, 686, true);
			this.DataSourceType = typeof(Enterprise.Customs.Business.CusPackingList);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1224, 725, true);
			this.Name = "PackingListForm";
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

		void CustomTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.PackingListCustomUserControl = new PackingListCustomUserControl();
			this.CustomTabPage.SuspendLayout();
			this.PackingListCustomUserControl.SuspendLayout();
			this.CustomTabPage.Controls.Add(this.PackingListCustomUserControl);
			// 
			// PackingListCustomUserControl
			// 
			this.PackingListCustomUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PackingListCustomUserControl, ".");
			this.PackingListCustomUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackingListCustomUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PackingListCustomUserControl.Name = "PackingListCustomUserControl";
			this.PackingListCustomUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1344, 603, true);
			this.PackingListCustomUserControl.TabIndex = 0;
			this.CustomTabPage.PerformLayout();
			this.PackingListCustomUserControl.ResumeLayout(true);
			this.PackingListCustomUserControl.PerformLayout();
			this.CustomTabPage.ResumeLayout(true);
		}

		void MainTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.PackingListDetailsDynamicUserControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.MainTabPage.SuspendLayout();
			this.MainTabPage.Controls.Add(this.PackingListDetailsDynamicUserControl);
			// 
			// PackingListDetailsDynamicUserControl
			// 
			this.PackingListDetailsDynamicUserControl.AllowDrop = true;
			this.PackingListDetailsDynamicUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackingListDetailsDynamicUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PackingListDetailsDynamicUserControl.Name = "PackingListDetailsDynamicUserControl";
			this.PackingListDetailsDynamicUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1344, 603, true);
			this.PackingListDetailsDynamicUserControl.TabIndex = 0;
			this.MainTabPage.PerformLayout();
			this.MainTabPage.ResumeLayout(true);

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
		internal ZArchitecture.GUI.ZTabPage CustomTabPage;
		internal PackingListCustomUserControl PackingListCustomUserControl;

		#endregion

		internal ZArchitecture.GUI.ZDynamicControlCreationUserControl PackingListDetailsDynamicUserControl;
	}
}
