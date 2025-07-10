using System;

namespace Enterprise.TransportConsignment.GUI
{
	partial class DtbConsignmentForm
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
			this.WorkflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
			this.MainTabControl.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.WorkflowTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainTabControl
			//
			this.MainTabControl.Controls.Add(this.WorkflowTabPage);
			this.MainTabControl.Controls.SetChildIndex(this.WorkflowTabPage, 0);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1205, 631, true);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 589, true);
			this.MainTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MainTabPage_InitializeTab));
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 589, true);
			this.NotesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.NotesTabPage_InitializeTab));
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 609, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1205, 631, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1205, 24, true);
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 553, true);
			this.WorkflowTabPage.TabIndex = 3;
			this.WorkflowTabPage.UseVisualStyleBackColor = true;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.TransportConsignment.Business.DtbConsignment);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportConsignment.Business.DtbConsignment)(null)).LTC_JobID)));
			// 
			// DtbConsignmentForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1205, 687, true);
			this.DataSourceType = typeof(Enterprise.TransportConsignment.Business.DtbConsignment);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1220, 725, true);
			this.Name = "DtbConsignmentForm";
			this.ShouldSerializeTabPageMethods = true;
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.WorkflowTabPage.ResumeLayout(false);
			this.WorkflowTabPage.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private void MainTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.ConsignmentIdTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.glowLinkLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.MainTabPage.SuspendLayout();
			this.MainTabPage.Controls.Add(this.glowLinkLabel);
			this.MainTabPage.Controls.Add(this.ConsignmentIdTextBox);
			// 
			// ConsignmentIdTextBox
			// 
			this.BindingSource.SetBindingMember(this.ConsignmentIdTextBox, "LTC_JobID");
			this.ConsignmentIdTextBox.CaptionResourceString = Enterprise.TransportConsignment.GUI.Res.GetData("05088947-e35e-4773-848c-47a75f170fdf", "Consignment ID");
			this.ConsignmentIdTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(107, 16, true);
			this.ConsignmentIdTextBox.Name = "ConsignmentIdTextBox";
			this.ConsignmentIdTextBox.ReadOnly = true;
			this.ConsignmentIdTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(204, 17, true);
			this.ConsignmentIdTextBox.TabIndex = 5;
			// 
			// glowLinkLabel
			// 
			this.glowLinkLabel.AutoSize = true;
			this.glowLinkLabel.CaptionResourceString = Enterprise.TransportConsignment.GUI.Res.GetData("61ddbb3f-232e-4775-998d-525c1d9cdf25", "Consignment Details can only be accessed via the Land Transport Desktop Portal.");
			this.glowLinkLabel.IsFontBold = false;
			this.glowLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 43, true);
			this.glowLinkLabel.Name = "glowLinkLabel";
			this.glowLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.glowLinkLabel.TabIndex = 8;
			this.glowLinkLabel.TabStop = false;
			this.glowLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.GlowLinkLabel_LinkClicked);
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

		private ZArchitecture.ZTextBox ConsignmentIdTextBox;
		public ZArchitecture.GUI.ZLinkLabel glowLinkLabel;
		private Enterprise.MasterFiles.GUI.ZWorkflowTabPage WorkflowTabPage;
	}
}
