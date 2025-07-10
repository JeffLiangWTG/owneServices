namespace Enterprise.Customs.US.eManifest.GUI
{
	partial class ShipmentPartiesUserControl
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
		private void InitializeComponent()
		{
			this.ShipperAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.ConsigneeControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.PartiesUserControl = new Enterprise.Customs.US.eManifest.GUI.PartiesUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.eManifest.Business.Shipment);
			// 
			// ShipperAddressControl
			// 
			this.ShipperAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipperAddressControl, "Shipper");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.US.eManifest.Business.Shipment)(null)).Shipper)));
			this.ShipperAddressControl.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("7136cd8b-dd5e-4f55-803f-477d334ecf2b", "Shipper");
			this.ShipperAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ShipperAddressControl.Name = "ShipperAddressControl";
			this.ShipperAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 182, true);
			this.ShipperAddressControl.TabIndex = 0;
			// 
			// ConsigneeControl
			// 
			this.ConsigneeControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsigneeControl, "Consignee");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.Customs.US.eManifest.Business.Shipment)(null)).Consignee)));
			this.ConsigneeControl.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("7ed34d0f-4217-4bae-becb-8692ac71af38", "Consignee");
			this.ConsigneeControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(252, 0, true);
			this.ConsigneeControl.Name = "ConsigneeControl";
			this.ConsigneeControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 182, true);
			this.ConsigneeControl.TabIndex = 1;
			// 
			// PartiesUserControl
			// 
			this.PartiesUserControl.AllowDrop = true;
			this.PartiesUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.PartiesUserControl, ".");
			this.PartiesUserControl.CaptionResourceString = null;
			this.PartiesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(504, 0, true);
			this.PartiesUserControl.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(255, 2000, true);
			this.PartiesUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(255, 300, true);
			this.PartiesUserControl.Name = "PartiesUserControl";
			this.PartiesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(255, 300, true);
			this.PartiesUserControl.TabIndex = 2;
			// 
			// ShipmentPartiesUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.PartiesUserControl);
			this.Controls.Add(this.ConsigneeControl);
			this.Controls.Add(this.ShipperAddressControl);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(760, 300, true);
			this.Name = "ShipmentPartiesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(760, 300, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private MasterFiles.GUI.ZDocAddressControl ShipperAddressControl;
		private MasterFiles.GUI.ZDocAddressControl ConsigneeControl;
		private PartiesUserControl PartiesUserControl;
	}
}
