using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI.AWB
{
	public partial class AWBViewEditForm : ZChildForm
	{
		internal HAWBUserControl hawbUserControl1;

		protected override void InitializeComponent()
		{
			this.hawbUserControl1 = new HAWBUserControl();
			((ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 624, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1005, 24, true);
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
			this.BindingSource.DataSourceType = typeof(Freight.Business.DocumentShipment);
			// 
			// hawbUserControl1
			// 
			this.hawbUserControl1.AutoScroll = true;
			this.hawbUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.hawbUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.hawbUserControl1.Name = "hawbUserControl1";
			this.hawbUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1005, 648, true);
			this.hawbUserControl1.TabIndex = 1;
			// 
			// AWBViewEditForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1005, 648, true);
			this.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("AWBViewEditForm|b5bf2085-239b-412b-a15c-2338dcb982d1", "Air Waybill");
			this.Controls.Add(this.hawbUserControl1);
			this.DataSourceAssemblyName = "Enterprise.Freight";
			this.DataSourceType = typeof(Freight.Business.DocumentShipment);
			this.DataSourceTypeName = "Enterprise.Freight.Business.DocumentShipment";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1021, 684, true);
			this.Name = "AWBViewEditForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Controls.SetChildIndex(this.hawbUserControl1, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		IContainer components = null;
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
