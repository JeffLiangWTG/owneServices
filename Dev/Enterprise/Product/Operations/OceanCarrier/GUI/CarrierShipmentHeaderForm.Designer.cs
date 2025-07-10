using System;

namespace Enterprise.OceanCarrier.GUI
{
	partial class CarrierShipmentHeaderForm
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
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
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
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.WorkflowTabPage);
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.OceanCarrier.Business.CarrierShipmentHeader);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.OceanCarrier.Business.CarrierShipmentHeader)(null)).CSH_CarrierShipmentReference)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.OceanCarrier.Business.CarrierShipmentHeader)(null)).CSH_HouseBill)));
			// 
			// WorkflowTabPage
			// 
			this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.WorkflowTabPage.Name = "WorkflowTabPage";
			this.WorkflowTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(978, 471, true);
			this.WorkflowTabPage.TabIndex = 1;
			//
			// CarrierShipmentHeaderForm
			//
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1204, 686, true);
			this.DataSourceType = typeof(Enterprise.OceanCarrier.Business.CarrierShipmentHeader);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1220, 725, true);
			this.Name = "CarrierShipmentHeaderForm";
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

		private void MainTabPage_InitializeTab(object sender, EventArgs e)
		{
			//
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			//
			this.shipmentReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.houseBillTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.glowLinkLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.MainTabPage.SuspendLayout();
			this.MainTabPage.Controls.Add(this.glowLinkLabel);
			this.MainTabPage.Controls.Add(this.houseBillTextBox);
			this.MainTabPage.Controls.Add(this.shipmentReferenceTextBox);
			//
			// WaybillTextBox
			//
			this.BindingSource.SetBindingMember(this.shipmentReferenceTextBox, "CSH_CarrierShipmentReference");
			this.shipmentReferenceTextBox.CaptionResourceString = Enterprise.OceanCarrier.GUI.Res.GetData("83C9435C-D4ED-4662-9CD5-D9E14A59F574", "Reference");
			this.shipmentReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 16, true);
			this.shipmentReferenceTextBox.Name = "CarrierShipmentHeaderReferenceTextBox";
			this.shipmentReferenceTextBox.ReadOnly = true;
			this.shipmentReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(185, 20, true);
			this.shipmentReferenceTextBox.TabIndex = 5;
			//
			// zTextBox1
			//
			this.BindingSource.SetBindingMember(this.houseBillTextBox, "CSH_HouseBill");
			this.houseBillTextBox.CaptionResourceString = Enterprise.OceanCarrier.GUI.Res.GetData("31D758BF-52F5-462D-BFE2-657607DDBC0D", "House Bill");
			this.houseBillTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 42, true);
			this.houseBillTextBox.Name = "HouseBillTextBox";
			this.houseBillTextBox.ReadOnly = true;
			this.houseBillTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(185, 20, true);
			this.houseBillTextBox.TabIndex = 6;
			//
			// glowLinkLabel
			//
			this.glowLinkLabel.AutoSize = true;
			this.glowLinkLabel.CaptionResourceString = Enterprise.OceanCarrier.GUI.Res.GetData("F9C5499D-E77E-4332-A800-0EFB095596FC", "Carrier Shipment Details can only be accessed via the Ocean Carrier Portal.");
			this.glowLinkLabel.IsFontBold = false;
			this.glowLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 70, true);
			this.glowLinkLabel.Name = "glowLinkLabel";
			this.glowLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(457, 13, true);
			this.glowLinkLabel.TabIndex = 8;
			this.glowLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.OnGlowLinkLabelLinkClicked);
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

		private ZArchitecture.ZTextBox shipmentReferenceTextBox;
		private ZArchitecture.ZTextBox houseBillTextBox;
		public ZArchitecture.GUI.ZLinkLabel glowLinkLabel;
		public Enterprise.MasterFiles.GUI.ZWorkflowTabPage WorkflowTabPage;
	}
}
