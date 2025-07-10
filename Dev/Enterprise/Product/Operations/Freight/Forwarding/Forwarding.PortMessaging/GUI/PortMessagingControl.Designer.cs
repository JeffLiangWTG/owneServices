namespace Enterprise.Freight.Forwarding.PortMessaging.GUI
{
	partial class PortMessagingControl
	{
		#region Component Designer generated code

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

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.shipmentDataDynamicCreationControl = new Enterprise.ZArchitecture.GUI.ZDynamicControlCreationUserControl();
			this.portMessagingTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.portOrderTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.consolDataControl = new Enterprise.Freight.Forwarding.PortMessaging.GUI.PortMessagingConsolDataControl();
			this.statusControl = new Enterprise.Forwarding.PortMessaging.GUI.PortMessagingStatusControl();
			this.messageLabel = new Enterprise.ZArchitecture.ZLabel();
			this.portMessagingEventsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.portMessagingEventsControl = new PortMessagingEventsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.portMessagingTabControl.SuspendLayout();
			this.portOrderTabPage.SuspendLayout();
			this.consolDataControl.SuspendLayout();
			this.statusControl.SuspendLayout();
			this.portMessagingEventsTabPage.SuspendLayout();
			this.portMessagingEventsControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingManager);
			// 
			// shipmentDataDynamicCreationControl
			// 
			this.shipmentDataDynamicCreationControl.AllowDrop = true;
			this.shipmentDataDynamicCreationControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.shipmentDataDynamicCreationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 366, true);
			this.shipmentDataDynamicCreationControl.Name = "shipmentDataDynamicCreationControl";
			this.shipmentDataDynamicCreationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(814, 232, true);
			this.shipmentDataDynamicCreationControl.TabIndex = 3;
			this.shipmentDataDynamicCreationControl.UserControlType = null;
			// 
			// portMessagingTabControl
			// 
			this.portMessagingTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.portMessagingTabControl.Controls.Add(this.portOrderTabPage);
			this.portMessagingTabControl.Controls.Add(this.portMessagingEventsTabPage);
			this.portMessagingTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.portMessagingTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.portMessagingTabControl.Name = "portMessagingTabControl";
			this.portMessagingTabControl.SelectedIndex = 0;
			this.portMessagingTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 620, true);
			this.portMessagingTabControl.TabIndex = 4;
			// 
			// portOrderTabPage
			// 
			this.portOrderTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.portOrderTabPage.Controls.Add(this.shipmentDataDynamicCreationControl);
			this.portOrderTabPage.Controls.Add(this.consolDataControl);
			this.portOrderTabPage.Controls.Add(this.statusControl);
			this.portOrderTabPage.Controls.Add(this.messageLabel);
			this.portOrderTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.portOrderTabPage.Name = "portOrderTabPage";
			this.portOrderTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.portOrderTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(814, 597, true);
			this.portOrderTabPage.TabIndex = 0;
			this.portOrderTabPage.Text = Enterprise.Freight.Forwarding.PortMessaging.GUI.Res.GetString("718ca2a6-1c6f-4865-b6cd-b0329a968054", "Port Order");
			// 
			// consolDataControl
			// 
			this.consolDataControl.AllowDrop = true;
			this.consolDataControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.consolDataControl, "Data");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingData)(((Enterprise.Freight.Forwarding.PortMessaging.Business.PortMessagingManager)(null)).Data)));
			this.consolDataControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 3, true);
			this.consolDataControl.Name = "consolDataControl";
			this.consolDataControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(814, 268, true);
			this.consolDataControl.TabIndex = 2;
			// 
			// statusControl
			// 
			this.statusControl.AllowDrop = true;
			this.statusControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.statusControl, ".");
			this.statusControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 276, true);
			this.statusControl.Name = "statusControl";
			this.statusControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(814, 86, true);
			this.statusControl.TabIndex = 3;
			// 
			// messageLabel
			// 
			this.messageLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.messageLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.messageLabel.Name = "messageLabel";
			this.messageLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(807, 590, true);
			this.messageLabel.TabIndex = 5;
			this.messageLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// portMessagingEventsTabPage
			// 
			this.portMessagingEventsTabPage.CaptionResourceString = Enterprise.Freight.Forwarding.PortMessaging.GUI.Res.GetData("b7dc07cc-a5cd-4431-8f51-956d39398c8a", "Events");
			this.portMessagingEventsTabPage.Controls.Add(this.portMessagingEventsControl);
			this.portMessagingEventsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.portMessagingEventsTabPage.Name = "portMessagingEventsTabPage";
			this.portMessagingEventsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.portMessagingEventsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(814, 597, true);
			this.portMessagingEventsTabPage.TabIndex = 2;
			this.portMessagingEventsTabPage.Text = Enterprise.Freight.Forwarding.PortMessaging.GUI.Res.GetString("8e9983d0-fde7-4b40-85e3-dd72a973521e", "Events");
			this.portMessagingEventsTabPage.UseVisualStyleBackColor = true;
			// 
			// portMessagingEventsControl
			// 
			this.portMessagingEventsControl.AllowDrop = true;
			this.portMessagingEventsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.portMessagingEventsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.portMessagingEventsControl.Name = "portMessagingEventsControl";
			this.portMessagingEventsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(807, 590, true);
			this.portMessagingEventsControl.TabIndex = 1;
			// 
			// PortMessagingControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.portMessagingTabControl);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 620, true);
			this.Name = "PortMessagingControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 620, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.portMessagingTabControl.ResumeLayout(false);
			this.portMessagingTabControl.PerformLayout();
			this.portOrderTabPage.ResumeLayout(false);
			this.portOrderTabPage.PerformLayout();
			this.consolDataControl.ResumeLayout(true);
			this.consolDataControl.PerformLayout();
			this.statusControl.ResumeLayout(true);
			this.statusControl.PerformLayout();
			this.portMessagingEventsTabPage.ResumeLayout(false);
			this.portMessagingEventsTabPage.PerformLayout();
			this.portMessagingEventsControl.ResumeLayout(true);
			this.portMessagingEventsControl.PerformLayout();

			this.ResumeLayout(false);
			this.PerformLayout();

		}
		private ZArchitecture.GUI.ZDynamicControlCreationUserControl shipmentDataDynamicCreationControl;
		private ZArchitecture.GUI.ZTabControl portMessagingTabControl;
		private ZArchitecture.GUI.ZTabPage portOrderTabPage;
		private PortMessagingConsolDataControl consolDataControl;
		private Enterprise.Forwarding.PortMessaging.GUI.PortMessagingStatusControl statusControl;
		private ZArchitecture.ZLabel messageLabel;
		private ZArchitecture.GUI.ZTabPage portMessagingEventsTabPage;
		private PortMessagingEventsUserControl portMessagingEventsControl;

		#endregion
	}
}
