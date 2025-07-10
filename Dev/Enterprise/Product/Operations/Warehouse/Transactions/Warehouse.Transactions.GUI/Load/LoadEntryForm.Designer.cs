using System;
using Enterprise.MasterFiles.GUI;

namespace Enterprise.Warehouse.Transactions.GUI
{
	partial class LoadEntryForm
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
			this.WorkflowTabPage = new ZWorkflowTabPage();
			this.MainTabControl.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.WorkflowTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1204, 630, true);
			this.MainTabControl.TabIndex = 0;
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.WorkflowTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1196, 603, true);
			this.MainTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MainTabPage_InitializeTab));
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1196, 603, true);
			this.NotesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.NotesTabPage_InitializeTab));
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1196, 603, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1196, 603, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1204, 630, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1204, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Warehouse.Transactions.Business.WhsLoad);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transactions.Business.WhsLoad)(null)).WLO_JobID)));
			// 
			// LoadEntryForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1204, 686, true);
			this.DataSourceType = typeof(Enterprise.Warehouse.Transactions.Business.WhsLoad);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1220, 725, true);
			this.Name = "LoadEntryForm";
			this.ShouldSerializeTabPageMethods = true;
			this.Text = "Load Planning";
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

		private void MainTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.JobIdTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.WarehouseLoadGlowLinkLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.MainTabPage.SuspendLayout();
			this.JobIdTextBox.SuspendLayout();
			this.MainTabPage.Controls.Add(this.JobIdTextBox);
			this.MainTabPage.Controls.Add(this.WarehouseLoadGlowLinkLabel);
			// 
			// JobId
			// 
			this.BindingSource.SetBindingMember(this.JobIdTextBox, "WLO_JobID");
			this.JobIdTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 16, true);
			this.JobIdTextBox.Name = "JobIdTextBox";
			this.JobIdTextBox.ReadOnly = true;
			this.JobIdTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(204, 20, true);
			this.JobIdTextBox.TabIndex = 0;
			this.MainTabPage.PerformLayout();
			this.JobIdTextBox.ResumeLayout(true);
			this.JobIdTextBox.PerformLayout();
			this.MainTabPage.ResumeLayout(true);
			// 
			// WarehouseLoadGlowLinkLabel
			// 
			this.WarehouseLoadGlowLinkLabel.AutoSize = true;
			this.WarehouseLoadGlowLinkLabel.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("f6a8ec91-a2b0-4045-af5b-1bd28efd360e", "Load Planning Details can only be accessed via the Product Warehouse Desktop Portal.");
			this.WarehouseLoadGlowLinkLabel.IsFontBold = false;
			this.WarehouseLoadGlowLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 50, true);
			this.WarehouseLoadGlowLinkLabel.Name = "WarehouseLoadGlowLinkLabel";
			this.WarehouseLoadGlowLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(457, 13, true);
			this.WarehouseLoadGlowLinkLabel.TabIndex = 1;
			this.WarehouseLoadGlowLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.WarehouseLoadGlowLinkLabel_LinkClicked);
			this.MainTabPage.PerformLayout();
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

		private ZArchitecture.ZTextBox JobIdTextBox;
		private ZArchitecture.GUI.ZLinkLabel WarehouseLoadGlowLinkLabel;
		private ZWorkflowTabPage WorkflowTabPage;
	}
}
