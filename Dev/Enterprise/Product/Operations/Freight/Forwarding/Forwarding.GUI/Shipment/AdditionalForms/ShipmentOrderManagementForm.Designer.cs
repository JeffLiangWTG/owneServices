using System;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class ShipmentOrderManagementForm : ZChildForm
	{
		ZPanel zPanel1;
		ZButton OKButton;
		GenericOrderManagementControl orderManagementControl1;

		protected new void InitializeComponent()
		{
			this.zPanel1 = new ZPanel();
			this.orderManagementControl1 = new GenericOrderManagementControl();
			this.OKButton = new ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zPanel1.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 364, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(407, 0, true);
			this.MainStatusBar.Visible = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(133);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(133);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(ForwardingShipment);
			// 
			// zPanel1
			// 
			this.zPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.zPanel1.Controls.Add(this.orderManagementControl1);
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(407, 333, true);
			this.zPanel1.TabIndex = 2;
			// 
			// orderManagementControl1
			// 
			this.BindingSource.SetBindingMember(this.orderManagementControl1, ".");
			this.orderManagementControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.orderManagementControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.orderManagementControl1.Name = "orderManagementControl1";
			this.orderManagementControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(407, 333, true);
			this.orderManagementControl1.TabIndex = 2;
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentOrderManagementForm|d11aed2f-4be8-4a30-99ec-9f5ae5ccf4ab", "Close");
			this.OKButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(324, 336, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 23, true);
			this.OKButton.TabIndex = 3;
			this.OKButton.UseVisualStyleBackColor = true;
			this.OKButton.Click += new EventHandler(this.OKButton_Click);
			// 
			// ShipmentOrderManagementForm
			// 
			this.AcceptButton = this.OKButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.OKButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(407, 364, true);
			this.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentOrderManagementForm|7663f436-ec58-44a4-a374-1c8dafc966da", "Shipment Order Management");
			this.Controls.Add(this.zPanel1);
			this.Controls.Add(this.OKButton);
			this.DataSourceAssemblyName = "Enterprise.Freight";
			this.DataSourceType = typeof(ForwardingShipment);
			this.DataSourceTypeName = "Enterprise.Freight.Business.DocumentShipment";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "ShipmentOrderManagementForm";
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.zPanel1, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zPanel1.ResumeLayout(false);
			this.ResumeLayout(false);
		}

		System.ComponentModel.IContainer components = null;
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}
	}
}
