using System;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Yard.GUI
{
	partial class MNRSurveyForm : ZTemplateForm
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
		protected override void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();

			this.SuspendLayout();
			this.MainTabControl.SuspendLayout();
			this.MainPanel.SuspendLayout();

			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = new System.Drawing.Size(800, 450);
			this.CaptionRenderingEnabled = true;
			this.Text = Res.GetString("36253183-3e6b-4f27-84d1-9a6cb1d29937", "Survey Form");

			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1204, 630, true);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);

			// 
			// MainTabPage
			// 
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 19, true);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1029, 636, true);
			this.MainTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MainTabPageInitializeTab));

			//
			// MainPanel
			//
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1204, 630, true);

			this.BindingSource.DataSourceType = typeof(Enterprise.Warehouse.Yard.Business.MNRSurvey);
			this.DataSourceType = typeof(Enterprise.Warehouse.Yard.Business.MNRSurvey);
			this.Name = "MNRSurvey";

			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
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
			this.GlowLinkLabel.CaptionResourceString = Enterprise.Warehouse.Yard.GUI.Res.GetData("d2150857-5e76-433b-b001-118ee00a5565", "Survey can only be accessed via the Container Yard GLOW Portal.");
			this.GlowLinkLabel.IsFontBold = false;
			this.GlowLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 70, true);
			this.GlowLinkLabel.Name = "GlowLinkLabel";
			this.GlowLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(460, 15, true);
			this.GlowLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.GlowLinkLabelClicked);

			this.MainTabPage.Controls.Add(this.GlowLinkLabel);
			this.MainTabPage.ResumeLayout(true);
		}

		#endregion

		public ZLinkLabel GlowLinkLabel;
	}
}
