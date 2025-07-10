using System;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.GUI
{
	partial class CampaignTrackingControl
	{
		void InitializeComponent()
		{
			this.trackingActivityTabControl = new ZTabControl();
			this.salesRelationTab = new ZTabPage();
			this.linkTrackingTab = new ZTabPage();
			this.linkActivityUserControl = new CampaignTrackingLinkActivityUserControl();
			this.BottomPanel = new ZPanel();
			this.CreateTaskButton = new ZButton();
			this.RefreshButton = new ZButton();
			this.ResendButton = new ZButton();
			this.LastCommunicationButton = new ZButton();
			this.DeliveryDetailsButton = new ZButton();
			this.UnsubscribeButton = new ZButton();
			this.UpdateContactsButton = new ZButton();
			this.TopPanel = new ZPanel();
			this.ToolStrip = new ZToolStrip();
			this.mainSplitContainer = new KSplitContainer();
			this.topLeftPanel = new ZPanel();
			this.bottomSplitContainer = new KSplitContainer();
			this.salesRelationControl = new CampaignTrackingSalesRelationControl();
			this.bottomLeftSplitContainer = new KSplitContainer();
			this.campaignItemContactEDocsGroupBox = new ZGroupBox();
			this.CampaignItemContactEDocsControl = new EdocsSwappableControl();
			this.campaignItemContactCrossReferencesGroupBox = new ZGroupBox();
			this.CampaignItemContactCrossReferencesControl = new CampaignItemContactCrossReferencesControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.trackingActivityTabControl.SuspendLayout();
			this.salesRelationTab.SuspendLayout();
			this.linkTrackingTab.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.TopPanel.SuspendLayout();
			this.ToolStrip.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).BeginInit();
			this.mainSplitContainer.Panel1.SuspendLayout();
			this.mainSplitContainer.Panel2.SuspendLayout();
			this.mainSplitContainer.SuspendLayout();
			this.topLeftPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.bottomSplitContainer)).BeginInit();
			this.bottomSplitContainer.Panel1.SuspendLayout();
			this.bottomSplitContainer.Panel2.SuspendLayout();
			this.linkActivityUserControl.SuspendLayout();
			this.bottomSplitContainer.SuspendLayout();
			this.salesRelationControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.bottomLeftSplitContainer)).BeginInit();
			this.bottomLeftSplitContainer.Panel1.SuspendLayout();
			this.bottomLeftSplitContainer.Panel2.SuspendLayout();
			this.bottomLeftSplitContainer.SuspendLayout();
			this.campaignItemContactEDocsGroupBox.SuspendLayout();
			this.CampaignItemContactEDocsControl.SuspendLayout();
			this.campaignItemContactCrossReferencesGroupBox.SuspendLayout();
			this.CampaignItemContactCrossReferencesControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(GlbCompanyCampaign);
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.CreateTaskButton);
			this.BottomPanel.Controls.Add(this.RefreshButton);
			this.BottomPanel.Controls.Add(this.ResendButton);
			this.BottomPanel.Controls.Add(this.LastCommunicationButton);
			this.BottomPanel.Controls.Add(this.DeliveryDetailsButton);
			this.BottomPanel.Controls.Add(this.UnsubscribeButton);
			this.BottomPanel.Controls.Add(this.UpdateContactsButton);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 319, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1096, 34, true);
			this.BottomPanel.TabIndex = 1;
			// 
			// CreateTaskButton
			// 
			this.CreateTaskButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("CampaignTrackingControl|780bc1d8-c033-476f-bbe0-e99217751e63", "Create Task");
			this.CreateTaskButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 5, true);
			this.CreateTaskButton.Name = "CreateTaskButton";
			this.CreateTaskButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 23, true);
			this.CreateTaskButton.TabIndex = 2;
			this.CreateTaskButton.Click += new EventHandler(this.CreateTaskButton_Click);
			// 
			// RefreshButton
			// 
			this.RefreshButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("CampaignTrackingControl|080eeb9a-e63e-4ac7-8fec-826a4d89c65a", "Refresh");
			this.RefreshButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 5, true);
			this.RefreshButton.Name = "RefreshButton";
			this.RefreshButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 23, true);
			this.RefreshButton.TabIndex = 2;
			this.RefreshButton.Click += new EventHandler(this.RefreshButton_Click);
			// 
			// ResendButton
			// 
			this.ResendButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.ResendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1000, 5, true);
			this.ResendButton.Name = "ResendButton";
			this.ResendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 23, true);
			this.ResendButton.TabIndex = 6;
			this.ResendButton.Click += new EventHandler(this.ResendButton_Click);
			// 
			// LastCommunicationButton
			// 
			this.LastCommunicationButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.LastCommunicationButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("CampaignTrackingControl|77a86b63-ddbe-4bf1-a342-49ef12dbf9f5", "New Communication");
			this.LastCommunicationButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(874, 5, true);
			this.LastCommunicationButton.Name = "LastCommunicationButton";
			this.LastCommunicationButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 23, true);
			this.LastCommunicationButton.TabIndex = 3;
			this.LastCommunicationButton.Click += new EventHandler(this.NewCommunicationButton_Click);
			// 
			// DeliveryDetailsButton
			// 
			this.DeliveryDetailsButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.DeliveryDetailsButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("CampaignTrackingControl|d9a3fe22-624f-496d-857b-43eae1621bbc", "Delivery Details");
			this.DeliveryDetailsButton.Enabled = false;
			this.DeliveryDetailsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(746, 5, true);
			this.DeliveryDetailsButton.Name = "DeliveryDetailsButton";
			this.DeliveryDetailsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 23, true);
			this.DeliveryDetailsButton.TabIndex = 3;
			this.DeliveryDetailsButton.Click += new EventHandler(this.DeliveryDetailsButton_Click);
			// 
			// UpdateContactsButton
			// 
			this.UpdateContactsButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | AnchorStyles.Right);
			this.UpdateContactsButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("CampaignTrackingControl|9e7ea809-6d9e-4fce-9997-d1ab71d14e6c", "Update NDR Contact(s)");
			this.UpdateContactsButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(593, 5, true);
			this.UpdateContactsButton.Name = "UpdateContactsButton";
			this.UpdateContactsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 23, true);
			this.UpdateContactsButton.TabIndex = 4;
			this.UpdateContactsButton.Click += new EventHandler(this.UpdateContactsButton_Click);
			this.UpdateContactsButton.Enabled = false;
			// 
			// UnsubscribeButton
			// 
			this.UnsubscribeButton.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.UnsubscribeButton.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("CampaignTrackingControl|3d83c3e3-70e5-4e87-9a5b-096d8d6cc03f", "Subscription Preference");
			this.UnsubscribeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(439, 5, true);
			this.UnsubscribeButton.Name = "UpdateContactsButton";
			this.UnsubscribeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 23, true);
			this.UnsubscribeButton.TabIndex = 4;
			this.UnsubscribeButton.Click += new EventHandler(this.UnsubscribeButton_Click);
			this.UnsubscribeButton.Enabled = false;
			// 
			// TopPanel
			// 
			this.TopPanel.Controls.Add(this.ToolStrip);
			this.TopPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TopPanel.Name = "TopPanel";
			this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1096, 319, true);
			this.TopPanel.TabIndex = 0;
			// 
			// mainSplitContainer
			// 
			this.mainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.mainSplitContainer.Name = "mainSplitContainer";
			this.mainSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// mainSplitContainer.Panel1
			// 
			this.mainSplitContainer.Panel1.Controls.Add(this.topLeftPanel);
			this.mainSplitContainer.Panel1MinSize = 200;
			// 
			// mainSplitContainer.Panel2
			// 
			this.mainSplitContainer.Panel2.Controls.Add(this.bottomSplitContainer);
			this.mainSplitContainer.Panel2MinSize = 200;
			this.mainSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1100, 561, true);
			this.mainSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(357);
			this.mainSplitContainer.TabIndex = 2;
			// 
			// topLeftPanel
			// 
			this.topLeftPanel.Anchor = (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right);
			this.topLeftPanel.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.topLeftPanel.Controls.Add(this.TopPanel);
			this.topLeftPanel.Controls.Add(this.BottomPanel);
			this.topLeftPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 5, true);
			this.topLeftPanel.Name = "topLeftPanel";
			this.topLeftPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1100, 352, true);
			this.topLeftPanel.TabIndex = 0;
			// 
			// bottomSplitContainer
			// 
			this.bottomSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.bottomSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.bottomSplitContainer.IsSplitterFixed = true;
			this.bottomSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.bottomSplitContainer.Name = "bottomSplitContainer";
			// 
			// bottomSplitContainer.Panel1
			// 
			this.bottomSplitContainer.Panel1.Controls.Add(this.bottomLeftSplitContainer);
			// 
			// bottomSplitContainer.Panel2
			// 
			this.bottomSplitContainer.Panel2.Controls.Add(this.campaignItemContactCrossReferencesGroupBox);
			this.bottomSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1100, 200, true);
			this.bottomSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(800);
			this.bottomSplitContainer.TabIndex = 1;
			//
			// trackingActivityTabControl
			// 
			this.trackingActivityTabControl.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left);
			this.trackingActivityTabControl.Controls.Add(this.salesRelationTab);
			this.trackingActivityTabControl.Controls.Add(this.linkTrackingTab);
			this.trackingActivityTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.trackingActivityTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.trackingActivityTabControl.Name = "trackingActivityTabControl";
			this.trackingActivityTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1100, 200, true);
			this.trackingActivityTabControl.TabIndex = 1;
			this.trackingActivityTabControl.SelectedIndex = 0;
			this.trackingActivityTabControl.TabStop = false;
			// 
			// salesRelationTab
			// 
			this.salesRelationTab.Controls.Add(this.salesRelationControl);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.salesRelationTab, false);
			this.salesRelationTab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.salesRelationTab.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.salesRelationTab.Name = "salesRelationTab";
			this.salesRelationTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1088, 177, true);
			this.salesRelationTab.TabIndex = 0;
			this.salesRelationTab.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("71812939-a0ba-47cd-8002-1631a035a0b7", "Sales Relations");
			this.salesRelationTab.UseVisualStyleBackColor = true;
			// 
			// linkTrackingTab
			// 
			this.linkTrackingTab.Controls.Add(this.linkActivityUserControl);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.linkTrackingTab, false);
			this.linkTrackingTab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.linkTrackingTab.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.linkTrackingTab.Name = "linkTrackingTab";
			this.linkTrackingTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1088, 177, true);
			this.linkTrackingTab.TabIndex = 0;
			this.linkTrackingTab.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("836c2214-dd10-4a9f-b4e2-056b5eb4d3c0", "Link Activity");
			this.linkTrackingTab.UseVisualStyleBackColor = true;
			// 
			// linkActivityUserControl
			// 
			this.linkActivityUserControl.AllowDrop = true;
			this.linkActivityUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.linkActivityUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 13, true);
			this.linkActivityUserControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 0, 3, 0, true);
			this.linkActivityUserControl.Name = "linkActivityUserControl";
			this.linkActivityUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1094, 186, true);
			this.linkActivityUserControl.TabIndex = 0;
			// 
			// salesRelationControl
			// 
			this.salesRelationControl.AllowDrop = true;
			this.salesRelationControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.salesRelationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 13, true);
			this.salesRelationControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 0, 3, 0, true);
			this.salesRelationControl.Name = "salesRelationControl";
			this.salesRelationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1094, 186, true);
			this.salesRelationControl.TabIndex = 0;
			// 
			// bottomLeftSplitContainer
			// 
			this.bottomLeftSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.bottomLeftSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.bottomLeftSplitContainer.Name = "bottomLeftSplitContainer";
			// 
			// bottomLeftSplitContainer.Panel1
			// 
			this.bottomLeftSplitContainer.Panel1.Controls.Add(this.trackingActivityTabControl);
			// 
			// bottomLeftSplitContainer.Panel2
			// 
			this.bottomLeftSplitContainer.Panel2.Controls.Add(this.campaignItemContactEDocsGroupBox);
			this.bottomLeftSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 201, true);
			this.bottomLeftSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(582);
			this.bottomLeftSplitContainer.TabIndex = 1;
			// 
			// campaignItemContactEDocsGroupBox
			// 
			this.campaignItemContactEDocsGroupBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("1E1BEDF3-7A6E-49B2-AA93-E4D3BCFCFDC7", "Contact eDocs");
			this.campaignItemContactEDocsGroupBox.Controls.Add(this.CampaignItemContactEDocsControl);
			this.campaignItemContactEDocsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.campaignItemContactEDocsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.campaignItemContactEDocsGroupBox.Name = "campaignItemContactEDocsGroupBox";
			this.campaignItemContactEDocsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(215, 201, true);
			this.campaignItemContactEDocsGroupBox.TabIndex = 1;
			this.campaignItemContactEDocsGroupBox.TabStop = false;
			// 
			// CampaignItemContactEDocsControl
			// 
			this.CampaignItemContactEDocsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CampaignItemContactEDocsControl, ".");
			this.CampaignItemContactEDocsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CampaignItemContactEDocsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.CampaignItemContactEDocsControl.Name = "CampaignItemContactEDocsControl";
			this.CampaignItemContactEDocsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(210, 184, true);
			this.CampaignItemContactEDocsControl.TabIndex = 0;
			// 
			// campaignItemContactCrossReferencesGroupBox
			// 
			this.campaignItemContactCrossReferencesGroupBox.CaptionResourceString = Enterprise.MarketingManager.GUI.Res.GetData("625fb60e-d24a-4bf4-9206-d4f6f9998408", "Cross Referenced Contacts for Organization");
			this.campaignItemContactCrossReferencesGroupBox.Controls.Add(this.CampaignItemContactCrossReferencesControl);
			this.campaignItemContactCrossReferencesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.campaignItemContactCrossReferencesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.campaignItemContactCrossReferencesGroupBox.Name = "campaignItemContactCrossReferencesGroupBox";
			this.campaignItemContactCrossReferencesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(296, 200, true);
			this.campaignItemContactCrossReferencesGroupBox.TabIndex = 0;
			this.campaignItemContactCrossReferencesGroupBox.TabStop = false;
			// 
			// CampaignItemContactCrossReferencesControl
			// 
			this.CampaignItemContactCrossReferencesControl.AllowDrop = true;
			this.CampaignItemContactCrossReferencesControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CampaignItemContactCrossReferencesControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.CampaignItemContactCrossReferencesControl.Name = "CampaignItemContactCrossReferencesControl";
			this.CampaignItemContactCrossReferencesControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 181, true);
			this.CampaignItemContactCrossReferencesControl.TabIndex = 0;
			// 
			// CampaignTrackingControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.mainSplitContainer);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(950, 400, true);
			this.Name = "CampaignTrackingControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1100, 561, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.TopPanel.ResumeLayout(false);
			this.TopPanel.PerformLayout();
			this.ToolStrip.ResumeLayout(false);
			this.ToolStrip.PerformLayout();
			this.mainSplitContainer.Panel1.ResumeLayout(false);
			this.mainSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).EndInit();
			this.mainSplitContainer.ResumeLayout(false);
			this.mainSplitContainer.PerformLayout();
			this.topLeftPanel.ResumeLayout(false);
			this.topLeftPanel.PerformLayout();
			this.bottomSplitContainer.Panel1.ResumeLayout(false);
			this.bottomSplitContainer.Panel2.ResumeLayout(false);
			this.trackingActivityTabControl.ResumeLayout();
			this.salesRelationTab.ResumeLayout();
			this.linkTrackingTab.ResumeLayout();
			((System.ComponentModel.ISupportInitialize)(this.bottomSplitContainer)).EndInit();
			this.bottomSplitContainer.ResumeLayout(false);
			this.bottomSplitContainer.PerformLayout();
			this.salesRelationControl.ResumeLayout(true);
			this.salesRelationControl.PerformLayout();
			this.linkActivityUserControl.ResumeLayout();
			this.linkActivityUserControl.PerformLayout();
			this.bottomLeftSplitContainer.Panel1.ResumeLayout(false);
			this.bottomLeftSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.bottomLeftSplitContainer)).EndInit();
			this.bottomLeftSplitContainer.ResumeLayout(false);
			this.bottomLeftSplitContainer.PerformLayout();
			this.campaignItemContactEDocsGroupBox.ResumeLayout(false);
			this.campaignItemContactEDocsGroupBox.PerformLayout();
			this.CampaignItemContactEDocsControl.ResumeLayout(true);
			this.CampaignItemContactEDocsControl.PerformLayout();
			this.campaignItemContactCrossReferencesGroupBox.ResumeLayout(false);
			this.campaignItemContactCrossReferencesGroupBox.PerformLayout();
			this.CampaignItemContactCrossReferencesControl.ResumeLayout(true);
			this.CampaignItemContactCrossReferencesControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		ZTabControl trackingActivityTabControl;
		ZTabPage salesRelationTab;
		internal ZTabPage linkTrackingTab;
		CampaignTrackingLinkActivityUserControl linkActivityUserControl;
		ZButton CreateTaskButton;
		ZButton RefreshButton;
		ZPanel BottomPanel;
		ZPanel TopPanel;
		ZButton LastCommunicationButton;
		internal ZButton DeliveryDetailsButton;
		ZButton UpdateContactsButton;
		internal ZButton ResendButton;
		ZButton UnsubscribeButton;
		System.ComponentModel.IContainer components = null;
		KSplitContainer mainSplitContainer;
		CampaignTrackingSalesRelationControl salesRelationControl;
		ZPanel topLeftPanel;
		KSplitContainer bottomSplitContainer;
		KSplitContainer bottomLeftSplitContainer;
		internal CampaignItemContactCrossReferencesControl CampaignItemContactCrossReferencesControl;
		internal EdocsSwappableControl CampaignItemContactEDocsControl;
		ZGroupBox campaignItemContactCrossReferencesGroupBox;
		ZGroupBox campaignItemContactEDocsGroupBox;
		ZToolStrip ToolStrip;
	}
}
