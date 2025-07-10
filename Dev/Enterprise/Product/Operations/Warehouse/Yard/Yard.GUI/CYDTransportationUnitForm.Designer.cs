using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Yard.GUI
{
	partial class CYDTransportationUnitForm : ZTemplateForm
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
		new void InitializeComponent()
		{
            this.zWorkflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
            this.MainTabControl.SuspendLayout();
            this.MainPanel.SuspendLayout();
            this.SaveButtonUserControl.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // MainTabControl
            // 
            this.MainTabControl.Controls.Add(this.zWorkflowTabPage);
            this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1036, 659, true);
            this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
            this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
            this.MainTabControl.Controls.SetChildIndex(this.zWorkflowTabPage, 0);
            this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
            // 
            // MainTabPage
            // 
            this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
            this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1029, 636, true);
            this.MainTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MainTabPageInitializeTab));
            // 
            // NotesTabPage
            // 
            this.NotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
            this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1029, 636, true);
            this.NotesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.NotesTabPage_InitializeTab));
            // 
            // LogsTabPage
            // 
            this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
            this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1028, 636, true);
            // 
            // MainPanel
            // 
            this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1036, 659, true);
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1036, 24, true);
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Warehouse.Yard.Business.CYDTransportationUnit);
            // 
            // zWorkflowTabPage
            // 
            this.zWorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
            this.zWorkflowTabPage.Name = "zWorkflowTabPage";
            this.zWorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(839, 550, true);
            this.zWorkflowTabPage.TabIndex = 3;
            this.zWorkflowTabPage.UseVisualStyleBackColor = true;
            // 
            // CYDTransportationUnitForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1036, 715, true);
            this.DataSourceType = typeof(Enterprise.Warehouse.Yard.Business.CYDTransportationUnit);
            this.Name = "CYDTransportationUnitForm";
            this.ShouldSerializeTabPageMethods = true;
            this.Text = "Transportation Unit Form";
            this.MainTabControl.ResumeLayout(false);
            this.MainTabControl.PerformLayout();
            this.MainPanel.ResumeLayout(false);
            this.MainPanel.PerformLayout();
            this.SaveButtonUserControl.ResumeLayout(true);
            this.SaveButtonUserControl.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		private void MainTabPageInitializeTab(object sender, EventArgs e)
		{
			this.MainTabPage.SuspendLayout();

			// 
			// GlowLinkLabel
			//
			this.GlowLinkLabel = new ZArchitecture.GUI.ZLinkLabel();
			this.GlowLinkLabel.AutoSize = true;
			this.GlowLinkLabel.CaptionResourceString = Enterprise.Warehouse.Yard.GUI.Res.GetData("96811742-c62e-4155-aefb-3a1223b39c0e", "Transportation units can only be accessed via the Container Yard Desktop Portal.");
			this.GlowLinkLabel.IsFontBold = false;
			this.GlowLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 70, true);
			this.GlowLinkLabel.Name = "GlowLinkLabel";
			this.GlowLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(460, 15, true);
			this.GlowLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.GlowLinkLabelClicked);

			this.MainTabPage.Controls.Add(this.GlowLinkLabel);
			this.MainTabPage.ResumeLayout(true);
		}

		private void NotesTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.NotesTabPage.SuspendLayout();
			this.NotesTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(true);

		}

		#endregion

		private Enterprise.MasterFiles.GUI.ZWorkflowTabPage zWorkflowTabPage;
		public ZLinkLabel GlowLinkLabel;
	}
}
