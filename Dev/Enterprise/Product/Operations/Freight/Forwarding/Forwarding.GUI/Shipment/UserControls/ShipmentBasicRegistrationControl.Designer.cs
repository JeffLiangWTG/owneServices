using System;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class ShipmentBasicRegistrationControl : ZUserControl
	{
		CargoWise.Windows.UI.KSplitContainer splitContainer1;
		ZPanel MainLeftPanel;
		ModeAndPartyControl ModeAndParty;
		CargoWise.Windows.UI.KSplitContainer splitContainer2;
		CargoWise.Windows.UI.KSplitContainer splitContainer4;
		ZPanel MiddleTopPanel;
		ZPanel MiddleBottomPanel;
		CargoWise.Windows.UI.KSplitContainer splitContainer3;
		ZPanel RightTopPanel;
		ZPanel RightBottomPanel;
		internal ZDynamicControlCreationUserControl Details;
		ZDynamicControlCreationUserControl OrderLinks;
		ZDynamicControlCreationUserControl CustomFields;

		void InitializeComponent()
		{
			this.splitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			this.MainLeftPanel = new ZPanel();
			this.ModeAndParty = new ModeAndPartyControl();
			this.splitContainer2 = new CargoWise.Windows.UI.KSplitContainer();
			this.splitContainer4 = new CargoWise.Windows.UI.KSplitContainer();
			this.MiddleTopPanel = new ZPanel();
			this.Details = new ZDynamicControlCreationUserControl();
			this.MiddleBottomPanel = new ZPanel();
			this.splitContainer3 = new CargoWise.Windows.UI.KSplitContainer();
			this.RightTopPanel = new ZPanel();
			this.RightBottomPanel = new ZPanel();
			this.OrderLinks = new ZDynamicControlCreationUserControl();
			this.CustomFields = new ZDynamicControlCreationUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.MainLeftPanel.SuspendLayout();
			this.splitContainer2.Panel1.SuspendLayout();
			this.splitContainer2.Panel2.SuspendLayout();
			this.splitContainer2.SuspendLayout();
			this.splitContainer4.Panel1.SuspendLayout();
			this.splitContainer4.Panel2.SuspendLayout();
			this.splitContainer4.SuspendLayout();
			this.MiddleTopPanel.SuspendLayout();
			this.splitContainer3.Panel1.SuspendLayout();
			this.splitContainer3.Panel2.SuspendLayout();
			this.splitContainer3.SuspendLayout();
			this.RightTopPanel.SuspendLayout();
			this.RightBottomPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(ForwardingShipment);
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainer1.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 579, true);
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.MainLeftPanel);
			this.splitContainer1.Panel1MinSize = 250;
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
			this.splitContainer1.Panel2MinSize = 700;
			this.splitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 579, true);
			this.splitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			this.splitContainer1.TabIndex = 39;
			// 
			// MainLeftPanel
			// 
			this.MainLeftPanel.Controls.Add(this.ModeAndParty);
			this.MainLeftPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.VisibilityConfigurationProvider.SetIsVisibilityConfigured(this.MainLeftPanel, true);
			this.MainLeftPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainLeftPanel.Name = "MainLeftPanel";
			this.MainLeftPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 579, true);
			this.MainLeftPanel.TabIndex = 1;
			// 
			// ModeAndParty
			// 
			this.BindingSource.SetBindingMember(this.ModeAndParty, ".");
			this.ModeAndParty.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ModeAndParty.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ModeAndParty.Name = "ModeAndParty";
			this.ModeAndParty.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 579, true);
			this.ModeAndParty.TabIndex = 0;
			// 
			// splitContainer2
			// 
			this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainer2.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(747, 579, true);
			this.splitContainer2.Name = "splitContainer2";
			// 
			// splitContainer2.Panel1
			// 
			this.splitContainer2.Panel1.Controls.Add(this.splitContainer4);
			this.splitContainer2.Panel1MinSize = 250;
			// 
			// splitContainer2.Panel2
			// 
			this.splitContainer2.Panel2.Controls.Add(this.splitContainer3);
			this.splitContainer2.Panel2MinSize = 250;
			this.splitContainer2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(754, 579, true);
			this.splitContainer2.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(366);
			this.splitContainer2.TabIndex = 0;
			// 
			// splitContainer4
			// 
			this.splitContainer4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainer4.Name = "splitContainer4";
			this.splitContainer4.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer4.Panel1
			// 
			this.splitContainer4.Panel1.Controls.Add(this.MiddleTopPanel);
			this.splitContainer4.Panel1MinSize = 300;
			// 
			// splitContainer4.Panel2
			// 
			this.splitContainer4.Panel2.Controls.Add(this.MiddleBottomPanel);
			this.splitContainer4.Panel2MinSize = 0;
			this.splitContainer4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(366, 579, true);
			this.splitContainer4.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(547);
			this.splitContainer4.TabIndex = 1;
			// 
			// MiddleTopPanel
			// 
			this.MiddleTopPanel.Controls.Add(this.Details);
			this.MiddleTopPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.VisibilityConfigurationProvider.SetIsVisibilityConfigured(this.MiddleTopPanel, true);
			this.MiddleTopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MiddleTopPanel.Name = "MiddleTopPanel";
			this.MiddleTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(366, 547, true);
			this.MiddleTopPanel.TabIndex = 0;
			// 
			// Details
			// 
			this.Details.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Details.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.Details.Name = "Details";
			this.Details.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(366, 547, true);
			this.Details.TabIndex = 4;
			this.Details.UserControlType = typeof(DetailsEntryControl);
			// 
			// MiddleBottomPanel
			// 
			this.MiddleBottomPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MiddleBottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.VisibilityConfigurationProvider.SetIsVisibilityConfigured(this.MiddleBottomPanel, true);
			this.MiddleBottomPanel.Name = "MiddleBottomPanel";
			this.MiddleBottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(366, 28, true);
			this.MiddleBottomPanel.TabIndex = 0;
			// 
			// splitContainer3
			// 
			this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainer3.Name = "splitContainer3";
			this.splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer3.Panel1
			// 
			this.splitContainer3.Panel1.AutoScroll = true;
			this.splitContainer3.Panel1.Controls.Add(this.RightTopPanel);
			this.splitContainer3.Panel1.ForeColor = System.Drawing.SystemColors.ControlText;
			// 
			// splitContainer3.Panel2
			// 
			this.splitContainer3.Panel2.Controls.Add(this.RightBottomPanel);
			this.splitContainer3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 579, true);
			this.splitContainer3.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(241);
			this.splitContainer3.TabIndex = 2;
			// 
			// RightTopPanel
			// 
			this.RightTopPanel.Controls.Add(this.CustomFields);
			this.RightTopPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RightTopPanel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.VisibilityConfigurationProvider.SetIsVisibilityConfigured(this.RightTopPanel, true);
			this.RightTopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RightTopPanel.Name = "RightTopPanel";
			this.RightTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 241, true);
			this.RightTopPanel.TabIndex = 2;
			// 
			// RightBottomPanel
			// 
			this.RightBottomPanel.Controls.Add(this.OrderLinks);
			this.RightBottomPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.VisibilityConfigurationProvider.SetIsVisibilityConfigured(this.RightBottomPanel, true);
			this.RightBottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RightBottomPanel.Name = "RightBottomPanel";
			this.RightBottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 334, true);
			this.RightBottomPanel.TabIndex = 4;
			// 
			// OrderLinks
			// 
			this.OrderLinks.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OrderLinks.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OrderLinks.Name = "OrderLinks";
			this.OrderLinks.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 334, true);
			this.OrderLinks.TabIndex = 4;
			this.OrderLinks.UserControlType = typeof(GenericOrderManagementControl);
			// 
			// CustomFields
			// 
			this.CustomFields.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CustomFields.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CustomFields.Name = "CustomFields";
			this.CustomFields.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(384, 241, true);
			this.CustomFields.TabIndex = 3;
			this.CustomFields.UserControlType = typeof(CustomFieldsControl);
			// 
			// ShipmentBasicRegistrationControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.splitContainer1);
			this.Name = "ShipmentBasicRegistrationControl";
			this.ShouldSerializeTabPageMethods = true;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 579, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.ResumeLayout(false);
			this.MainLeftPanel.ResumeLayout(false);
			this.splitContainer2.Panel1.ResumeLayout(false);
			this.splitContainer2.Panel2.ResumeLayout(false);
			this.splitContainer2.ResumeLayout(false);
			this.splitContainer4.Panel1.ResumeLayout(false);
			this.splitContainer4.Panel2.ResumeLayout(false);
			this.splitContainer4.ResumeLayout(false);
			this.MiddleTopPanel.ResumeLayout(false);
			this.splitContainer3.Panel1.ResumeLayout(false);
			this.splitContainer3.Panel2.ResumeLayout(false);
			this.splitContainer3.ResumeLayout(false);
			this.RightTopPanel.ResumeLayout(false);
			this.RightBottomPanel.ResumeLayout(false);
			this.ResumeLayout(false);
		}
	}
}
