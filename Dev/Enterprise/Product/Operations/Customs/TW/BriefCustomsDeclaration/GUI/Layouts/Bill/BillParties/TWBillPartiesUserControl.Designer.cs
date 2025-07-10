namespace Enterprise.Customs.TW.BriefCustomsDeclaration.GUI
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
			this.ShipperAddressUserControl = new Enterprise.Customs.TW.BriefCustomsDeclaration.GUI.ShipperAddressUserControl();
			this.ConsigneeAddressUserControl = new Enterprise.Customs.TW.BriefCustomsDeclaration.GUI.ConsigneeAddressUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ShipperAddressUserControl.SuspendLayout();
			this.ConsigneeAddressUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaBill);
			// 
			// ShipperAddressUserControl
			// 
			this.ShipperAddressUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipperAddressUserControl, ".");
			this.ShipperAddressUserControl.CaptionResourceString = Enterprise.Customs.TW.BriefCustomsDeclaration.GUI.Res.GetData("42274554-4ed4-4325-9b68-d32e06587d02", "Shipper");
			this.ShipperAddressUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.ShipperAddressUserControl.Name = "ShipperAddressUserControl";
			this.ShipperAddressUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 232, true);
			this.ShipperAddressUserControl.TabIndex = 1;
			// 
			// ConsigneeAddressUserControl
			// 
			this.ConsigneeAddressUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsigneeAddressUserControl, ".");
			this.ConsigneeAddressUserControl.CaptionResourceString = Enterprise.Customs.TW.BriefCustomsDeclaration.GUI.Res.GetData("6ab71bd0-3470-4fd8-8c09-d6fe43233fa9", "Consignee");
			this.ConsigneeAddressUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(408, 3, true);
			this.ConsigneeAddressUserControl.Name = "ConsigneeAddressUserControl";
			this.ConsigneeAddressUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(402, 232, true);
			this.ConsigneeAddressUserControl.TabIndex = 2;
			// 
			// TWBillPartiesUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.ConsigneeAddressUserControl);
			this.Controls.Add(this.ShipperAddressUserControl);
			this.Name = "TWBillPartiesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(822, 239, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ShipperAddressUserControl.ResumeLayout(true);
			this.ShipperAddressUserControl.PerformLayout();
			this.ConsigneeAddressUserControl.ResumeLayout(true);
			this.ConsigneeAddressUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private ShipperAddressUserControl ShipperAddressUserControl;
		private ConsigneeAddressUserControl ConsigneeAddressUserControl;
	}
}
