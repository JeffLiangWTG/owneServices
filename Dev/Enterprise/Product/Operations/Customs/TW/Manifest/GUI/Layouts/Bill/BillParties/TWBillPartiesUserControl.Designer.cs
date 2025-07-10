
namespace Enterprise.Customs.TW.Manifest.GUI
{
	partial class TWBillPartiesUserControl
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
			this.ShipperAddressUserControl = new Enterprise.Customs.TW.Manifest.GUI.ShipperAddressUserControl();
			this.ConsigneeAddressUserControl = new Enterprise.Customs.TW.Manifest.GUI.ConsigneeAddressUserControl();
			this.NotifyPartyAddressUserControl = new Enterprise.Customs.TW.Manifest.GUI.NotifyPartyAddressUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ShipperAddressUserControl.SuspendLayout();
			this.ConsigneeAddressUserControl.SuspendLayout();
			this.NotifyPartyAddressUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TW.Manifest.Business.AsycudaBill);
			// 
			// ShipperAddressUserControl
			// 
			this.ShipperAddressUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipperAddressUserControl, ".");
			this.ShipperAddressUserControl.CaptionResourceString = Enterprise.Customs.TW.Manifest.GUI.Res.GetData("19398c44-9356-4bfe-966c-75962a7eaf0b", "Shipper");
			this.ShipperAddressUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.ShipperAddressUserControl.Name = "ShipperAddressUserControl";
			this.ShipperAddressUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 232, true);
			this.ShipperAddressUserControl.TabIndex = 1;
			// 
			// ConsigneeAddressUserControl
			// 
			this.ConsigneeAddressUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsigneeAddressUserControl, ".");
			this.ConsigneeAddressUserControl.CaptionResourceString = Enterprise.Customs.TW.Manifest.GUI.Res.GetData("f4cbe843-41b8-4798-97ae-78c370890b7e", "Consignee");
			this.ConsigneeAddressUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(408, 3, true);
			this.ConsigneeAddressUserControl.Name = "ConsigneeAddressUserControl";
			this.ConsigneeAddressUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(402, 232, true);
			this.ConsigneeAddressUserControl.TabIndex = 2;
			// 
			// NotifyPartyAddressUserControl
			// 
			this.NotifyPartyAddressUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NotifyPartyAddressUserControl, ".");
			this.NotifyPartyAddressUserControl.CaptionResourceString = Enterprise.Customs.TW.Manifest.GUI.Res.GetData("cbff1ec3-4855-4657-bc6f-4bae000cfbaf", "Notify Party");
			this.NotifyPartyAddressUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(816, 3, true);
			this.NotifyPartyAddressUserControl.Name = "NotifyPartyAddressUserControl";
			this.NotifyPartyAddressUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(402, 232, true);
			this.NotifyPartyAddressUserControl.TabIndex = 3;
			// 
			// TWBillPartiesUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.NotifyPartyAddressUserControl);
			this.Controls.Add(this.ConsigneeAddressUserControl);
			this.Controls.Add(this.ShipperAddressUserControl);
			this.Name = "TWBillPartiesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1226, 239, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ShipperAddressUserControl.ResumeLayout(true);
			this.ShipperAddressUserControl.PerformLayout();
			this.ConsigneeAddressUserControl.ResumeLayout(true);
			this.ConsigneeAddressUserControl.PerformLayout();
			this.NotifyPartyAddressUserControl.ResumeLayout(true);
			this.NotifyPartyAddressUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		internal ShipperAddressUserControl ShipperAddressUserControl;
		internal ConsigneeAddressUserControl ConsigneeAddressUserControl;
		internal NotifyPartyAddressUserControl NotifyPartyAddressUserControl;
	}
}
