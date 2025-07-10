namespace Enterprise.Freight.Forwarding.GUI
{
	partial class OrderManualShipmentNumberEntryForm
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		protected new void InitializeComponent()
		{
			this.ShipmentNumber = new Enterprise.ZArchitecture.ZTextBox();
			this.Ok_Button = new Enterprise.ZArchitecture.GUI.ZButton();
			this.Cancel_Button = new Enterprise.ZArchitecture.GUI.ZButton();
			this.label1 = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 98, true);
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
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.Business.ShipmentNumberEntry);
			// 
			// ShipmentNumber
			// 
			this.BindingSource.SetBindingMember(this.ShipmentNumber, "ShipmentNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.Forwarding.Business.ShipmentNumberEntry)(null)).ShipmentNumber)));
			this.ShipmentNumber.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrderManualShipmentNumberEntryForm|599559d0-9ff0-4c2c-aaa2-1e343dc95c59", "Shipment Number");
			this.ShipmentNumber.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 42, true);
			this.ShipmentNumber.Name = "ShipmentNumber";
			this.ShipmentNumber.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 20, true);
			this.ShipmentNumber.TabIndex = 1;
			// 
			// Ok_Button
			// 
			this.Ok_Button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.Ok_Button.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrderManualShipmentNumberEntryForm|0caf40a3-ba60-488f-bc48-da5fbe219a0a", "OK");
			this.Ok_Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 70, true);
			this.Ok_Button.Name = "Ok_Button";
			this.Ok_Button.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 22, true);
			this.Ok_Button.TabIndex = 2;
			this.Ok_Button.Click += new System.EventHandler(this.OkButton_Click);
			// 
			// Cancel_Button
			// 
			this.Cancel_Button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.Cancel_Button.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrderManualShipmentNumberEntryForm|25ce4c80-f6be-46c4-8e7b-8210f46638de", "Cancel");
			this.Cancel_Button.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(124, 69, true);
			this.Cancel_Button.Name = "Cancel_Button";
			this.Cancel_Button.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 22, true);
			this.Cancel_Button.TabIndex = 3;
			this.Cancel_Button.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
			this.label1.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrderManualShipmentNumberEntryForm|6d24e8fc-f99b-46e8-bc6e-f9d9e0d01ea1", "Please enter Shipment number or leave blank for the system to Auto-Generate.");
			this.label1.ForeColor = System.Drawing.Color.Green;
			this.label1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 9, true);
			this.label1.Name = "label1";
			this.label1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(206, 27, true);
			this.label1.TabIndex = 4;
			this.label1.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// OrderManualShipmentNumberEntryForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(218, 120, true);
			this.CaptionResourceString = Enterprise.Freight.Forwarding.GUI.Res.GetData("OrderManualShipmentNumberEntryForm|26396569-56dc-416d-911d-e5368476cba6", "Shipment Number");
			this.Controls.Add(this.Cancel_Button);
			this.Controls.Add(this.Ok_Button);
			this.Controls.Add(this.ShipmentNumber);
			this.Controls.Add(this.label1);
			this.DataSourceAssemblyName = "Enterprise.Freight.Forwarding.Business";
			this.DataSourceType = typeof(Enterprise.Freight.Forwarding.Business.ShipmentNumberEntry);
			this.DataSourceTypeName = "Enterprise.Freight.Forwarding.Business.ShipmentNumberEntry";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimizeBox = false;
			this.Name = "OrderManualShipmentNumberEntryForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.label1, 0);
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

		#endregion

		Enterprise.ZArchitecture.ZTextBox ShipmentNumber;
		Enterprise.ZArchitecture.GUI.ZButton Ok_Button;
		Enterprise.ZArchitecture.GUI.ZButton Cancel_Button;
		Enterprise.ZArchitecture.ZLabel label1;
	   }
}
