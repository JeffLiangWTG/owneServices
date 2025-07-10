using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture;
using CargoWise.Types;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.Modules;
using Enterprise.Core.Environment;

using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer;
using Enterprise.DataTransfer.GUI;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.GUI
{
	public partial class ZJobVoyageForm
	{
		protected override void InitialiseForm()
		{
			base.InitialiseForm();
			InitializeComponent();
		}

		new void InitializeComponent()
		{
			this.workflowTabPage = new JobVoyageWorkflowTabPage();
			this.messagingTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.messagingTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.RelatedJobsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.MainTabControl.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.messagingTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.messagingTabPage);
			this.MainTabControl.Controls.Add(this.RelatedJobsTabPage);
			this.MainTabControl.Controls.Add(this.workflowTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 467, true);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.messagingTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.workflowTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.RelatedJobsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
			// 
			// MainTabPage
			// 
			this.MainTabPage.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("ZJobVoyageForm|a5157a5a-3ad2-4eef-8825-91b5da6a7f30", "Schedule");
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 440, true);
			this.MainTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.MainTabPage_InitializeTab));
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(938, 440, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(946, 467, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(946, 24, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(931);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Business.JobVoyage);
			// 
			// ElectronicMessagingTabPage
			// 
			this.messagingTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.messagingTabPage.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("371f393c-fa84-4958-8599-6999d94e2ba4", "Electronic Messaging");
			this.messagingTabPage.Controls.Add(this.messagingTabControl);
			this.messagingTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.messagingTabPage.Name = "messagingTabPage";
			this.messagingTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.messagingTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(948, 450, true);
			this.messagingTabPage.TabIndex = 8;
			// MessagesTabControl
			// 
			this.messagingTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.messagingTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.messagingTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.messagingTabControl.Name = "messagingTabControl";
			this.messagingTabControl.SelectedIndex = 0;
			this.messagingTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(938, 440, true);
			this.messagingTabControl.TabIndex = 0;
			// 
			// workflowTabPage
			// 
			this.workflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.workflowTabPage.Name = "workflowTabPage";
			this.workflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(938, 440, true);
			this.workflowTabPage.TabIndex = 7;
			// 
			// RelatedJobsTabPage
			// 
			this.RelatedJobsTabPage.CaptionResourceString = Enterprise.Freight.GUI.Res.GetData("ZJobVoyageForm|88bfab0e-93df-4e1a-8b98-5e81f8a0ac8d", "Related Jobs");
			this.RelatedJobsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.RelatedJobsTabPage.Name = "RelatedJobsTabPage";
			this.RelatedJobsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.RelatedJobsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 428, true);
			this.RelatedJobsTabPage.TabIndex = 9;
			this.RelatedJobsTabPage.UseVisualStyleBackColor = true;
			this.RelatedJobsTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.RelatedJobsTabPage_InitializeTab));
			// 
			// ZJobVoyageForm
			// 
			this.AutoAddPreviousNextButtons = false;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1016, 523, true);
			this.DataSourceAssemblyName = "Enterprise.Freight";
			this.DataSourceType = typeof(Enterprise.Freight.Business.JobVoyage);
			this.DataSourceTypeName = "Enterprise.Freight.Business.JobVoyage";
			this.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(672, 632, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(940, 550, true);
			this.Name = "ZJobVoyageForm";
			this.ShouldSerializeTabPageMethods = true;
			this.Text = "Voyage Details";
			this.Controls.SetChildIndex(this.MainPanel, 0);
			this.MainTabControl.ResumeLayout(false);
			this.MainPanel.ResumeLayout(false);
			this.messagingTabPage.ResumeLayout();
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
			this.detailsControl = new Enterprise.Freight.GUI.VoyageDetailsControl();
			this.MainTabPage.SuspendLayout();
			this.MainTabPage.Controls.Add(this.detailsControl);
			// 
			// detailsControl
			// 
			this.detailsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.detailsControl, ".");
			this.detailsControl.CaptionResourceString = null;
			this.detailsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.detailsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0);
			this.detailsControl.Name = "detailsControl";
			this.detailsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 440);
			this.detailsControl.TabIndex = 0;
			this.MainTabPage.ResumeLayout(true);

		}

		private void RelatedJobsTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.relatedJobsControl = new Enterprise.Freight.GUI.VoyageRelatedJobsControl();
			this.RelatedJobsTabPage.SuspendLayout();
			this.relatedJobsControl.SuspendLayout();
			this.RelatedJobsTabPage.Controls.Add(this.relatedJobsControl);
			// 
			// relatedJobsControl
			// 
			this.relatedJobsControl.AllowDrop = true;
			this.relatedJobsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.relatedJobsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.relatedJobsControl.Name = "relatedJobsControl";
			this.relatedJobsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1002, 422, true);
			this.relatedJobsControl.TabIndex = 0;
			this.RelatedJobsTabPage.PerformLayout();
			this.relatedJobsControl.ResumeLayout(true);
			this.relatedJobsControl.PerformLayout();
			this.RelatedJobsTabPage.ResumeLayout(true);

		}

		private VoyageDetailsControl detailsControl;
		private VoyageRelatedJobsControl relatedJobsControl;
		private JobVoyageWorkflowTabPage workflowTabPage;
		private ZTabPage messagingTabPage;
		private ZTabPage RelatedJobsTabPage;
		protected ZTabControl messagingTabControl;
	}
}
