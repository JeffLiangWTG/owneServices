using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Yard.GUI
{
	partial class MNRWorkOrderForm : ZTemplateForm
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
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.SaveButtonUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zWorkflowTabPage.SuspendLayout();
			this.SuspendLayout();

			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(800, 450);
			this.CaptionRenderingEnabled = true;
			this.Text = Res.GetString("fa588805-460e-48e7-b757-6c4ffb56f3fd", "Work Order Form");
			// 
			// MainTabControl
			//
			this.MainTabControl.Controls.Add(this.zWorkflowTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1204, 603, true);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.zWorkflowTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			//
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1204, 686, true);
			this.MainTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MainTabPageInitializeTab));
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 21, true);
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 244, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 21, true);
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 244, true);
			//
			// MainPanel
			//
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1204, 630, true);
			// 
			// SaveButtonUserControl
			// 
			this.SaveButtonUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(387, 6, true);
			// 
			// MainStatusBar
			//
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1220, 725, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Warehouse.Yard.Business.MNRWorkOrderHeader);
			// 
			// zWorkflowTabPage
			// 
			this.zWorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 21, true);
			this.zWorkflowTabPage.Name = "zWorkflowTabPage";
			this.zWorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(679, 244, true);
			this.zWorkflowTabPage.TabIndex = 3;
			this.zWorkflowTabPage.UseVisualStyleBackColor = true;
			// 
			// MNRWorkOrderForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1204, 686, true);
			this.DataSourceType = typeof(Enterprise.Warehouse.Yard.Business.MNRWorkOrderHeader);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1220, 725, true);
			this.Name = "MNRWorkOrderHeader";
			this.ShouldSerializeTabPageMethods = false;
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainTabPage.ResumeLayout(false);
			this.MainTabPage.PerformLayout();
			this.NotesTabPage.ResumeLayout(false);
			this.NotesTabPage.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.SaveButtonUserControl.ResumeLayout(true);
			this.SaveButtonUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zWorkflowTabPage.ResumeLayout(false);
			this.zWorkflowTabPage.PerformLayout();
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
			this.GlowLinkLabel.CaptionResourceString = Enterprise.Warehouse.Yard.GUI.Res.GetData("f19eb64d-09d1-43f9-99be-e1bc8efe75e1", "Work order can only be accessed via the Container Yard GLOW Portal.");
			this.GlowLinkLabel.IsFontBold = false;
			this.GlowLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 70, true);
			this.GlowLinkLabel.Name = "GlowLinkLabel";
			this.GlowLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(460, 15, true);
			this.GlowLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.GlowLinkLabelClicked);
			// 
			// UnitNumberLabel
			//
			this.UnitNumberLabel = new ZLabel();
			this.UnitNumberLabel.AutoSize = true;
			this.UnitNumberLabel.CaptionResourceString = Enterprise.Warehouse.Yard.GUI.Res.GetData("f19eb66d-09d1-43f9-99be-e1bc8efe75e1", $"Unit Number: {this.workOrderHeader.YardUnitState?.YUS_UnitID}");
			this.UnitNumberLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 40, true);
			this.UnitNumberLabel.Name = "UnitNumberLabel";
			this.UnitNumberLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(40, 15, true);

			this.MainTabPage.Controls.Add(this.GlowLinkLabel);
			this.MainTabPage.Controls.Add(this.UnitNumberLabel);
			this.MainTabPage.ResumeLayout(true);
		}

		#endregion

		private ZWorkflowTabPage zWorkflowTabPage;
		private ZLabel UnitNumberLabel;
		public ZLinkLabel GlowLinkLabel;
	}
}
