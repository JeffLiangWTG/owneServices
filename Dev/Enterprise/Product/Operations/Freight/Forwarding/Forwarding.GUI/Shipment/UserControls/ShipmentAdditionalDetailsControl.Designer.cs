using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class ShipmentAdditionalDetailsControl : ZUserControl
	{
		CargoWise.Windows.UI.KSplitContainer splitContainer1;
		CargoWise.Windows.UI.KSplitContainer splitContainer3;
		ZPanel LeftTopPanel;
		ZPanel LeftBottomPanel;
		CargoWise.Windows.UI.KSplitContainer splitContainer4;
		CargoWise.Windows.UI.KSplitContainer splitContainer5;
		ZPanel RightTopPanel;
		ZPanel MiddleTopPanel;
		ZPanel MiddleBottomPanel;
		ZPanel RightBottomPanel;
		ZDynamicControlCreationUserControl Services;
		ZDynamicControlCreationUserControl WeightVolume;
		ZDynamicControlCreationUserControl ReferenceNumbers;
		ZDynamicControlCreationUserControl Consols;
		ZDynamicControlCreationUserControl FreightRatesAndGateways;
		ZDynamicControlCreationUserControl NotifyParty;
		CargoWise.Windows.UI.KSplitContainer splitContainer2;

		void InitializeComponent()
		{
			this.splitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			this.splitContainer3 = new CargoWise.Windows.UI.KSplitContainer();
			this.LeftTopPanel = new ZPanel();
			this.Consols = new ZDynamicControlCreationUserControl();
			this.LeftBottomPanel = new ZPanel();
			this.FreightRatesAndGateways = new ZDynamicControlCreationUserControl();
			this.splitContainer2 = new CargoWise.Windows.UI.KSplitContainer();
			this.splitContainer4 = new CargoWise.Windows.UI.KSplitContainer();
			this.MiddleTopPanel = new ZPanel();
			this.WeightVolume = new ZDynamicControlCreationUserControl();
			this.MiddleBottomPanel = new ZPanel();
			this.Services = new ZDynamicControlCreationUserControl();
			this.splitContainer5 = new CargoWise.Windows.UI.KSplitContainer();
			this.RightTopPanel = new ZPanel();
			this.NotifyParty = new ZDynamicControlCreationUserControl();
			this.RightBottomPanel = new ZPanel();
			this.ReferenceNumbers = new ZDynamicControlCreationUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
			this.splitContainer3.Panel1.SuspendLayout();
			this.splitContainer3.Panel2.SuspendLayout();
			this.splitContainer3.SuspendLayout();
			this.LeftTopPanel.SuspendLayout();
			this.Consols.SuspendLayout();
			this.LeftBottomPanel.SuspendLayout();
			this.FreightRatesAndGateways.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
			this.splitContainer2.Panel1.SuspendLayout();
			this.splitContainer2.Panel2.SuspendLayout();
			this.splitContainer2.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).BeginInit();
			this.splitContainer4.Panel1.SuspendLayout();
			this.splitContainer4.Panel2.SuspendLayout();
			this.splitContainer4.SuspendLayout();
			this.MiddleTopPanel.SuspendLayout();
			this.WeightVolume.SuspendLayout();
			this.MiddleBottomPanel.SuspendLayout();
			this.Services.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer5)).BeginInit();
			this.splitContainer5.Panel1.SuspendLayout();
			this.splitContainer5.Panel2.SuspendLayout();
			this.splitContainer5.SuspendLayout();
			this.RightTopPanel.SuspendLayout();
			this.NotifyParty.SuspendLayout();
			this.RightBottomPanel.SuspendLayout();
			this.ReferenceNumbers.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.ForwardingShipment);
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainer1.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(998, 584, true);
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.splitContainer3);
			this.splitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1244, 584, true);
			this.splitContainer1.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
			this.splitContainer1.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(500);
			this.splitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(452);
			this.splitContainer1.TabIndex = 31;
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
			this.splitContainer3.Panel1.Controls.Add(this.LeftTopPanel);
			// 
			// splitContainer3.Panel2
			// 
			this.splitContainer3.Panel2.Controls.Add(this.LeftBottomPanel);
			this.splitContainer3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(452, 584, true);
			this.splitContainer3.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(301);
			this.splitContainer3.TabIndex = 32;
			// 
			// LeftTopPanel
			// 
			this.LeftTopPanel.Controls.Add(this.Consols);
			this.LeftTopPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.VisibilityConfigurationProvider.SetIsVisibilityConfigured(this.LeftTopPanel, true);
			this.LeftTopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LeftTopPanel.Name = "LeftTopPanel";
			this.LeftTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(452, 301, true);
			this.LeftTopPanel.TabIndex = 33;
			// 
			// Consols
			// 
			this.Consols.AllowDrop = true;
			this.Consols.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Consols.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.Consols.Name = "Consols";
			this.Consols.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(452, 301, true);
			this.Consols.TabIndex = 5;
			this.Consols.UserControlType = typeof(ConsolDetailsControl);
			// 
			// LeftBottomPanel
			// 
			this.LeftBottomPanel.Controls.Add(this.FreightRatesAndGateways);
			this.LeftBottomPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.VisibilityConfigurationProvider.SetIsVisibilityConfigured(this.LeftBottomPanel, true);
			this.LeftBottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LeftBottomPanel.Name = "LeftBottomPanel";
			this.LeftBottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(452, 279, true);
			this.LeftBottomPanel.TabIndex = 34;
			// 
			// FreightRatesAndGateways
			// 
			this.FreightRatesAndGateways.AllowDrop = true;
			this.FreightRatesAndGateways.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FreightRatesAndGateways.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FreightRatesAndGateways.Name = "FreightRatesAndGateways";
			this.FreightRatesAndGateways.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(452, 279, true);
			this.FreightRatesAndGateways.TabIndex = 1;
			this.FreightRatesAndGateways.UserControlType = typeof(FreightRatesControl);
			// 
			// splitContainer2
			// 
			this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainer2.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(614, 584, true);
			this.splitContainer2.Name = "splitContainer2";
			// 
			// splitContainer2.Panel1
			// 
			this.splitContainer2.Panel1.Controls.Add(this.splitContainer4);
			this.splitContainer2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(788, 584, true);
			this.splitContainer2.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			// 
			// splitContainer2.Panel2
			// 
			this.splitContainer2.Panel2.Controls.Add(this.splitContainer5);
			this.splitContainer2.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			this.splitContainer2.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(514);
			this.splitContainer2.TabIndex = 0;
			// 
			// splitContainer4
			// 
			this.splitContainer4.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainer4.Name = "MiddleSplitContainer";
			this.splitContainer4.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer4.Panel1
			// 
			this.splitContainer4.Panel1.Controls.Add(this.MiddleTopPanel);
			// 
			// splitContainer4.Panel2
			// 
			this.splitContainer4.Panel2.Controls.Add(this.MiddleBottomPanel);
			this.splitContainer4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(514, 584, true);
			this.splitContainer4.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(301);
			this.splitContainer4.TabIndex = 0;
			// 
			// MiddleTopPanel
			// 
			this.MiddleTopPanel.Controls.Add(this.WeightVolume);
			this.MiddleTopPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.VisibilityConfigurationProvider.SetIsVisibilityConfigured(this.MiddleTopPanel, true);
			this.MiddleTopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MiddleTopPanel.Name = "MiddleTopPanel";
			this.MiddleTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(514, 301, true);
			this.MiddleTopPanel.TabIndex = 40;
			// 
			// WeightVolume
			// 
			this.WeightVolume.AllowDrop = true;
			this.WeightVolume.Dock = System.Windows.Forms.DockStyle.Fill;
			this.WeightVolume.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.WeightVolume.Name = "WeightVolume";
			this.WeightVolume.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(514, 156, true);
			this.WeightVolume.TabIndex = 1;
			this.WeightVolume.UserControlType = typeof(WeightVolChargeableControl);
			// 
			// MiddleBottomPanel
			// 
			this.MiddleBottomPanel.Controls.Add(this.Services);
			this.MiddleBottomPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.VisibilityConfigurationProvider.SetIsVisibilityConfigured(this.MiddleBottomPanel, true);
			this.MiddleBottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MiddleBottomPanel.Name = "MiddleBottomPanel";
			this.MiddleBottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(514, 279, true);
			this.MiddleBottomPanel.TabIndex = 41;
			// 
			// Services
			// 
			this.Services.AllowDrop = true;
			this.Services.Dock = System.Windows.Forms.DockStyle.Fill;
			this.Services.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.Services.Name = "Services";
			this.Services.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(514, 424, true);
			this.Services.TabIndex = 0;
			this.Services.UserControlType = typeof(MasterFiles.GUI.ServicesControl);
			// 
			// splitContainer5
			// 
			this.splitContainer5.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainer5.Name = "splitContainer5";
			this.splitContainer5.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer5.Panel1
			// 
			this.splitContainer5.Panel1.Controls.Add(this.RightTopPanel);
			// 
			// splitContainer5.Panel2
			// 
			this.splitContainer5.Panel2.Controls.Add(this.RightBottomPanel);
			this.splitContainer5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 584, true);
			this.splitContainer5.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(297);
			this.splitContainer5.TabIndex = 0;
			// 
			// RightTopPanel
			// 
			this.RightTopPanel.Controls.Add(this.NotifyParty);
			this.RightTopPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.VisibilityConfigurationProvider.SetIsVisibilityConfigured(this.RightTopPanel, true);
			this.RightTopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RightTopPanel.Name = "RightTopPanel";
			this.RightTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 297, true);
			this.RightTopPanel.TabIndex = 39;
			// 
			// NotifyParty
			// 
			this.NotifyParty.AllowDrop = true;
			this.NotifyParty.Dock = System.Windows.Forms.DockStyle.Fill;
			this.NotifyParty.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.NotifyParty.Name = "NotifyParty";
			this.NotifyParty.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 297, true);
			this.NotifyParty.TabIndex = 1;
			this.NotifyParty.UserControlType = typeof(NotifyPartyControl);
			// 
			// RightBottomPanel
			// 
			this.RightBottomPanel.Controls.Add(this.ReferenceNumbers);
			this.RightBottomPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.VisibilityConfigurationProvider.SetIsVisibilityConfigured(this.RightBottomPanel, true);
			this.RightBottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 4, true);
			this.RightBottomPanel.Name = "RightBottomPanel";
			this.RightBottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 279, true);
			this.RightBottomPanel.TabIndex = 40;
			// 
			// ReferenceNumbers
			// 
			this.ReferenceNumbers.AllowDrop = true;
			this.ReferenceNumbers.BindingMember = "Numbers";
			this.ReferenceNumbers.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ReferenceNumbers.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ReferenceNumbers.Name = "ReferenceNumbers";
			this.ReferenceNumbers.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 279, true);
			this.ReferenceNumbers.TabIndex = 3;
			this.ReferenceNumbers.UserControlType = typeof(MasterFiles.GUI.NumbersControl);
			// 
			// ShipmentAdditionalDetailsControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.splitContainer1);
			this.VisibilityConfigurationProvider.SetIsVisibilityConfigured(this, true);
			this.Name = "ShipmentAdditionalDetailsControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1244, 584, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.splitContainer1.PerformLayout();
			this.splitContainer3.Panel1.ResumeLayout(false);
			this.splitContainer3.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
			this.splitContainer3.ResumeLayout(false);
			this.splitContainer3.PerformLayout();
			this.LeftTopPanel.ResumeLayout(false);
			this.LeftTopPanel.PerformLayout();
			this.Consols.ResumeLayout(true);
			this.Consols.PerformLayout();
			this.LeftBottomPanel.ResumeLayout(false);
			this.LeftBottomPanel.PerformLayout();
			this.FreightRatesAndGateways.ResumeLayout(true);
			this.FreightRatesAndGateways.PerformLayout();
			this.splitContainer2.Panel1.ResumeLayout(false);
			this.splitContainer2.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
			this.splitContainer2.ResumeLayout(false);
			this.splitContainer2.PerformLayout();
			this.splitContainer4.Panel1.ResumeLayout(false);
			this.splitContainer4.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).EndInit();
			this.splitContainer4.ResumeLayout(false);
			this.splitContainer4.PerformLayout();
			this.MiddleTopPanel.ResumeLayout(false);
			this.MiddleTopPanel.PerformLayout();
			this.WeightVolume.ResumeLayout(true);
			this.WeightVolume.PerformLayout();
			this.MiddleBottomPanel.ResumeLayout(false);
			this.MiddleBottomPanel.PerformLayout();
			this.Services.ResumeLayout(true);
			this.Services.PerformLayout();
			this.splitContainer5.Panel1.ResumeLayout(false);
			this.splitContainer5.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer5)).EndInit();
			this.splitContainer5.ResumeLayout(false);
			this.splitContainer5.PerformLayout();
			this.RightTopPanel.ResumeLayout(false);
			this.RightTopPanel.PerformLayout();
			this.NotifyParty.ResumeLayout(true);
			this.NotifyParty.PerformLayout();
			this.RightBottomPanel.ResumeLayout(false);
			this.RightBottomPanel.PerformLayout();
			this.ReferenceNumbers.ResumeLayout(true);
			this.ReferenceNumbers.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
