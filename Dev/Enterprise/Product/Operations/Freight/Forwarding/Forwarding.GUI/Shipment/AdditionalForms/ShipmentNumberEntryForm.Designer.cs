using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI
{
	public partial class ShipmentNumberEntryForm : ZChildForm
	{
		ZArchitecture.ZTextBox ShipmentNumber;
		ZButton Ok_Button;
		ZButton Cancel_Button;

		protected override void InitializeComponent()
		{
			this.ShipmentNumber = new ZArchitecture.ZTextBox();
			this.Ok_Button = new ZButton();
			this.Cancel_Button = new ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 89, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(218, 22, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 4;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(218);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(ShipmentNumberEntry);
			// 
			// ShipmentNumber
			// 
			this.BindingSource.SetBindingMember(this.ShipmentNumber, "ShipmentNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((ShipmentNumberEntry)(null)).ShipmentNumber)));
			this.ShipmentNumber.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentNumberEntryForm|89d0b0ee-928e-4fdb-9552-734c251f9e6e", "Shipment Number");
			this.ShipmentNumber.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 22, true);
			this.ShipmentNumber.Name = "ShipmentNumber";
			this.ShipmentNumber.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 20, true);
			this.ShipmentNumber.TabIndex = 1;
			// 
			// Ok_Button
			// 
			this.Ok_Button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.Ok_Button.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentNumberEntryForm|f2997073-4a13-4242-9f56-30310c420f50", "OK");
			this.Ok_Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(48, 60, true);
			this.Ok_Button.Name = "Ok_Button";
			this.Ok_Button.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 22, true);
			this.Ok_Button.TabIndex = 2;
			this.Ok_Button.Click += new System.EventHandler(this.OkButton_Click);
			// 
			// Cancel_Button
			// 
			this.Cancel_Button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.Cancel_Button.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentNumberEntryForm|01a82beb-8cb7-4273-9940-8abf2e3a54c6", "Cancel");
			this.Cancel_Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 60, true);
			this.Cancel_Button.Name = "Cancel_Button";
			this.Cancel_Button.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 22, true);
			this.Cancel_Button.TabIndex = 3;
			this.Cancel_Button.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// ShipmentNumberEntryForm
			// 

			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(218, 111, true);
			this.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("ShipmentNumberEntryForm|f19a70de-5f15-425a-b28f-f7d1b7433812", "Shipment Number");
			this.Controls.Add(this.Cancel_Button);
			this.Controls.Add(this.Ok_Button);
			this.Controls.Add(this.ShipmentNumber);
			this.DataSourceAssemblyName = "Enterprise.Freight.Forwarding.Business";
			this.DataSourceType = typeof(ShipmentNumberEntry);
			this.DataSourceTypeName = "Enterprise.Freight.Forwarding.Business.ShipmentNumberEntry";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimizeBox = false;
			this.Name = "ShipmentNumberEntryForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.ShipmentNumber, 0);
			this.Controls.SetChildIndex(this.Ok_Button, 0);
			this.Controls.SetChildIndex(this.Cancel_Button, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		System.ComponentModel.Container components = null;
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
