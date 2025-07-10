namespace Enterprise.Customs.US.eManifest.GUI
{
	partial class ShipmentUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.ShipmentTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.ShipmentDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ShipmentDetailsUserControl = new Enterprise.Customs.US.eManifest.GUI.ShipmentDetailsUserControl();
			this.PartiesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.PartiesUserControl = new Enterprise.Customs.US.eManifest.GUI.ShipmentPartiesUserControl();
			this.CommoditiesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.CommoditiesUserControl = new Enterprise.Customs.US.eManifest.GUI.CommoditiesUserControl();
			this.InBondTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ShipmentTabControl.SuspendLayout();
			this.ShipmentDetailsTabPage.SuspendLayout();
			this.ShipmentDetailsUserControl.SuspendLayout();
			this.PartiesTabPage.SuspendLayout();
			this.PartiesUserControl.SuspendLayout();
			this.CommoditiesTabPage.SuspendLayout();
			this.CommoditiesUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.US.eManifest.Business.Shipment);
			// 
			// ShipmentTabControl
			// 
			this.ShipmentTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.ShipmentTabControl.Controls.Add(this.ShipmentDetailsTabPage);
			this.ShipmentTabControl.Controls.Add(this.PartiesTabPage);
			this.ShipmentTabControl.Controls.Add(this.CommoditiesTabPage);
			this.ShipmentTabControl.Controls.Add(this.InBondTabPage);
			this.ShipmentTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ShipmentTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ShipmentTabControl.Name = "ShipmentTabControl";
			this.ShipmentTabControl.SelectedIndex = 0;
			this.ShipmentTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(850, 350, true);
			this.ShipmentTabControl.TabIndex = 0;
			// 
			// ShipmentDetailsTabPage
			// 
			this.ShipmentDetailsTabPage.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("ShipmentUserControl|575e77dc-b046-4d6c-83f0-3884918cc7c2", "Shipment Details");
			this.ShipmentDetailsTabPage.Controls.Add(this.ShipmentDetailsUserControl);
			this.ShipmentDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.ShipmentDetailsTabPage.Name = "ShipmentDetailsTabPage";
			this.ShipmentDetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ShipmentDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(843, 326, true);
			this.ShipmentDetailsTabPage.TabIndex = 0;
			// 
			// ShipmentDetailsUserControl
			// 
			this.ShipmentDetailsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ShipmentDetailsUserControl, ".");
			this.ShipmentDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ShipmentDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ShipmentDetailsUserControl.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 2000, true);
			this.ShipmentDetailsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(760, 253, true);
			this.ShipmentDetailsUserControl.Name = "ShipmentDetailsUserControl";
			this.ShipmentDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(760, 253, true);
			this.ShipmentDetailsUserControl.TabIndex = 0;
			// 
			// PartiesTabPage
			// 
			this.PartiesTabPage.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("ShipmentUserControl|3f8e4461-984a-4786-9755-9a1b7828d923", "Parties");
			this.PartiesTabPage.Controls.Add(this.PartiesUserControl);
			this.PartiesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.PartiesTabPage.Name = "PartiesTabPage";
			this.PartiesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.PartiesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(843, 326, true);
			this.PartiesTabPage.TabIndex = 3;
			// 
			// PartiesUserControl
			// 
			this.PartiesUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PartiesUserControl, ".");
			this.PartiesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PartiesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.PartiesUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(760, 300, true);
			this.PartiesUserControl.Name = "PartiesUserControl";
			this.PartiesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(760, 300, true);
			this.PartiesUserControl.TabIndex = 0;
			// 
			// CommoditiesTabPage
			// 
			this.CommoditiesTabPage.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("ShipmentUserControl|820a38b8-b11e-4836-83d0-f16dc9584abb", "Commodities");
			this.CommoditiesTabPage.Controls.Add(this.CommoditiesUserControl);
			this.CommoditiesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.CommoditiesTabPage.Name = "CommoditiesTabPage";
			this.CommoditiesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.CommoditiesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(843, 326, true);
			this.CommoditiesTabPage.TabIndex = 2;
			// 
			// CommoditiesUserControl
			// 
			this.CommoditiesUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CommoditiesUserControl, ".");
			this.CommoditiesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CommoditiesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.CommoditiesUserControl.Name = "CommoditiesUserControl";
			this.CommoditiesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(837, 320, true);
			this.CommoditiesUserControl.TabIndex = 0;
			// 
			// InBondTabPage
			// 
			this.InBondTabPage.CaptionResourceString = Enterprise.Customs.US.eManifest.GUI.Res.GetData("ShipmentUserControl|7595cfe4-919d-425e-9d95-96e756e292a0", "In-Bond");
			this.InBondTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.InBondTabPage.Name = "InBondTabPage";
			this.InBondTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.InBondTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(843, 326, true);
			this.InBondTabPage.TabIndex = 1;
			// 
			// ShipmentUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.ShipmentTabControl);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(850, 350, true);
			this.Name = "ShipmentUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(850, 350, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ShipmentTabControl.ResumeLayout(false);
			this.ShipmentTabControl.PerformLayout();
			this.ShipmentDetailsTabPage.ResumeLayout(false);
			this.ShipmentDetailsTabPage.PerformLayout();
			this.ShipmentDetailsUserControl.ResumeLayout(true);
			this.ShipmentDetailsUserControl.PerformLayout();
			this.PartiesTabPage.ResumeLayout(false);
			this.PartiesTabPage.PerformLayout();
			this.PartiesUserControl.ResumeLayout(true);
			this.PartiesUserControl.PerformLayout();
			this.CommoditiesTabPage.ResumeLayout(false);
			this.CommoditiesTabPage.PerformLayout();
			this.CommoditiesUserControl.ResumeLayout(true);
			this.CommoditiesUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZTabControl ShipmentTabControl;
		private ZArchitecture.GUI.ZTabPage ShipmentDetailsTabPage;
		private ZArchitecture.GUI.ZTabPage InBondTabPage;
		private ZArchitecture.GUI.ZTabPage CommoditiesTabPage;
		private ZArchitecture.GUI.ZTabPage PartiesTabPage;
		private ShipmentDetailsUserControl ShipmentDetailsUserControl;
		private ShipmentPartiesUserControl PartiesUserControl;
		private CommoditiesUserControl CommoditiesUserControl;
		private InBondUserControl InBondUserControl;
	}
}
