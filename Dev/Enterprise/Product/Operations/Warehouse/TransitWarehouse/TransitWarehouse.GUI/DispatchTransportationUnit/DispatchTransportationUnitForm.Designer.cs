using System;

namespace Enterprise.Warehouse.Transit.GUI
{
	partial class DispatchTransportationUnitForm
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
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.zWorkflowTabPage);
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1204, 630, true);
			this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.zWorkflowTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
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
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Warehouse.Transit.Business.WhsItemDispatchTransportationUnit);
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transit.Business.WhsItemDispatchTransportationUnit)(null)).WDH_ReferenceNumber)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transit.Business.WhsItemDispatchTransportationUnit)(null)).WDH_VehicleReference)));
			// 
			// zTabPage1
			// 
			this.zWorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zWorkflowTabPage.Name = "zWorkflowTabPage";
			this.zWorkflowTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.zWorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1196, 603, true);
			this.zWorkflowTabPage.TabIndex = 3;
			this.zWorkflowTabPage.UseVisualStyleBackColor = true;
			// 
			// ReceiveConsignmentForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1204, 686, true);
			this.DataSourceType = typeof(Enterprise.Warehouse.Transit.Business.WhsItemDispatchTransportationUnit);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1220, 725, true);
			this.Name = "DTUForm";
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
			this.WaybillTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			this.glowLinkLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.MainTabPage.SuspendLayout();
			this.MainTabPage.Controls.Add(this.glowLinkLabel);
			this.MainTabPage.Controls.Add(this.zTextBox1);
			this.MainTabPage.Controls.Add(this.WaybillTextBox);
			// 
			// WaybillTextBox
			// 
			this.BindingSource.SetBindingMember(this.WaybillTextBox, "WDH_ReferenceNumber");
			this.WaybillTextBox.CaptionResourceString = Enterprise.Warehouse.Transit.GUI.Res.GetData("ead93964-3a8b-4c71-876d-b6e570a9153b", "DTU ID");
			this.WaybillTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 16, true);
			this.WaybillTextBox.Name = "WaybillTextBox";
			this.WaybillTextBox.ReadOnly = true;
			this.WaybillTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(185, 20, true);
			this.WaybillTextBox.TabIndex = 5;
			// 
			// zTextBox1
			// 
			this.BindingSource.SetBindingMember(this.zTextBox1, "WDH_VehicleReference");
			this.zTextBox1.CaptionResourceString = Enterprise.Warehouse.Transit.GUI.Res.GetData("dcb833cb-451d-4003-806d-c0c938f559c0", "DTU Reference");
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 42, true);
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.ReadOnly = true;
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(185, 20, true);
			this.zTextBox1.TabIndex = 6;
			// 
			// glowLinkLabel
			// 
			this.glowLinkLabel.AutoSize = true;
			this.glowLinkLabel.CaptionResourceString = Enterprise.Warehouse.Transit.GUI.Res.GetData("33d375c1-bec1-4e55-90e1-28f2774fccb8", "Dispatch Transportation Unit Details can only be accessed via the Transit Warehouse Desktop Portal.");
			this.glowLinkLabel.IsFontBold = false;
			this.glowLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 70, true);
			this.glowLinkLabel.Name = "glowLinkLabel";
			this.glowLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(457, 13, true);
			this.glowLinkLabel.TabIndex = 8;
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

		private ZArchitecture.ZTextBox WaybillTextBox;
		private ZArchitecture.ZTextBox zTextBox1;
		private Enterprise.MasterFiles.GUI.ZWorkflowTabPage zWorkflowTabPage;
		public ZArchitecture.GUI.ZLinkLabel glowLinkLabel;
	}
}
