namespace Enterprise.MasterFiles.GUI
{
	public partial class ConsigneeUserControl
	{

		#region Component Designer generated code

		protected internal Enterprise.ZArchitecture.GUI.ZTemplateTabControl ConsigneeTabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage DetailsTabPage;
		protected internal Enterprise.ZArchitecture.GUI.ZTabPage RelationshipsTabPage;
		protected internal Enterprise.MasterFiles.GUI.ConsigneeRelationshipsUserControl RelationshipsControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage LandedCostingPrefsTabPage;
		private Enterprise.MasterFiles.GUI.LandedCostingPrefsPageControl landedCostingPrefsPageControl1;
		private Enterprise.ZArchitecture.GUI.ZTabPage ProductPrefsTabPage;
		private Enterprise.MasterFiles.GUI.ConsigneeDetailsUserControl DetailsControl;
		private ConsigneeProdPrefsUserControl productPreferencesUserControl1;
		private System.ComponentModel.IContainer components;

		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.ConsigneeTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.DetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.DetailsControl = new Enterprise.MasterFiles.GUI.ConsigneeDetailsUserControl();
			this.RelationshipsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.RelationshipsControl = new Enterprise.MasterFiles.GUI.ConsigneeRelationshipsUserControl();
			this.LandedCostingPrefsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.landedCostingPrefsPageControl1 = new Enterprise.MasterFiles.GUI.LandedCostingPrefsPageControl();
			this.ProductPrefsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.productPreferencesUserControl1 = new Enterprise.MasterFiles.GUI.ConsigneeProdPrefsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ConsigneeTabControl.SuspendLayout();
			this.DetailsTabPage.SuspendLayout();
			this.RelationshipsTabPage.SuspendLayout();
			this.LandedCostingPrefsTabPage.SuspendLayout();
			this.ProductPrefsTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// SecurityPanel
			// 
			this.SecurityPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(855, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			// 
			// ConsigneeTabControl
			// 
			this.ConsigneeTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.ConsigneeTabControl.Controls.Add(this.DetailsTabPage);
			this.ConsigneeTabControl.Controls.Add(this.RelationshipsTabPage);
			this.ConsigneeTabControl.Controls.Add(this.LandedCostingPrefsTabPage);
			this.ConsigneeTabControl.Controls.Add(this.ProductPrefsTabPage);
			this.ConsigneeTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConsigneeTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 24, true);
			this.ConsigneeTabControl.Name = "ConsigneeTabControl";
			this.ConsigneeTabControl.SelectedIndex = 0;
			this.ConsigneeTabControl.ShowToolTips = true;
			this.ConsigneeTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(855, 531, true);
			this.ConsigneeTabControl.TabIndex = 0;
			// 
			// DetailsTabPage
			// 
			this.DetailsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConsigneeUserControl|ce51e12b-69ea-4729-9790-ec4eaee7fd80", "Details");
			this.DetailsTabPage.Controls.Add(this.DetailsControl);
			this.DetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DetailsTabPage.Name = "DetailsTabPage";
			this.DetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(847, 504, true);
			this.DetailsTabPage.TabIndex = 0;
			// 
			// DetailsControl
			// 
			this.BindingSource.SetBindingMember(this.DetailsControl, ".");
			this.DetailsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailsControl.Name = "DetailsControl";
			this.DetailsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(847, 504, true);
			this.DetailsControl.TabIndex = 0;
			// 
			// RelationshipsTabPage
			// 
			this.RelationshipsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConsigneeUserControl|6515f3d8-0315-4be2-8e8e-eb66aa3b5d04", "Consignor / Supplier / Shipper Relationships - Defaults");
			this.RelationshipsTabPage.Controls.Add(this.RelationshipsControl);
			this.RelationshipsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.RelationshipsTabPage.Name = "RelationshipsTabPage";
			this.RelationshipsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(847, 504, true);
			this.RelationshipsTabPage.TabIndex = 1;
			// 
			// RelationshipsControl
			// 
			this.BindingSource.SetBindingMember(this.RelationshipsControl, "SupplierLinks");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.OrgSupplierLinkCollection)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).SupplierLinks)));
			this.RelationshipsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RelationshipsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RelationshipsControl.Name = "RelationshipsControl";
			this.RelationshipsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(847, 504, true);
			this.RelationshipsControl.TabIndex = 0;
			// 
			// LandedCostingPrefsTabPage
			// 
			this.LandedCostingPrefsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConsigneeUserControl|f5b27a51-8804-4a5e-8d25-b5570f2720db", "Landed Costing Preferences");
			this.LandedCostingPrefsTabPage.Controls.Add(this.landedCostingPrefsPageControl1);
			this.LandedCostingPrefsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.LandedCostingPrefsTabPage.Name = "LandedCostingPrefsTabPage";
			this.LandedCostingPrefsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(847, 504, true);
			this.LandedCostingPrefsTabPage.TabIndex = 2;
			// 
			// landedCostingPrefsPageControl1
			// 
			this.BindingSource.SetBindingMember(this.landedCostingPrefsPageControl1, ".");
			this.landedCostingPrefsPageControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.landedCostingPrefsPageControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.landedCostingPrefsPageControl1.Name = "landedCostingPrefsPageControl1";
			this.landedCostingPrefsPageControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(847, 504, true);
			this.landedCostingPrefsPageControl1.TabIndex = 0;
			// 
			// ProductPrefsTabPage
			// 
			this.ProductPrefsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConsigneeUserControl|780ec55f-c1a4-4f09-9054-918b1a4084aa", "Product Preferences");
			this.ProductPrefsTabPage.Controls.Add(this.productPreferencesUserControl1);
			this.ProductPrefsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ProductPrefsTabPage.Name = "ProductPrefsTabPage";
			this.ProductPrefsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(847, 504, true);
			this.ProductPrefsTabPage.TabIndex = 3;
			// 
			// productPreferencesUserControl1
			// 
			this.BindingSource.SetBindingMember(this.productPreferencesUserControl1, ".");
			this.productPreferencesUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.productPreferencesUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.productPreferencesUserControl1.Name = "productPreferencesUserControl1";
			this.productPreferencesUserControl1.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.productPreferencesUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(847, 504, true);
			this.productPreferencesUserControl1.TabIndex = 0;
			// 
			// ConsigneeUserControl
			// 
			this.Controls.Add(this.ConsigneeTabControl);
			this.IsModifyConsignee = true;
			this.IsModifyConsigneeDetails = true;
			this.IsModifyConsigneeLandedCosting = true;
			this.IsModifyConsigneeRelationships = true;
			this.Name = "ConsigneeUserControl";
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(855, 555, true);
			this.Controls.SetChildIndex(this.SecurityPanel, 0);
			this.Controls.SetChildIndex(this.ConsigneeTabControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ConsigneeTabControl.ResumeLayout(false);
			this.DetailsTabPage.ResumeLayout(false);
			this.RelationshipsTabPage.ResumeLayout(false);
			this.LandedCostingPrefsTabPage.ResumeLayout(false);
			this.ProductPrefsTabPage.ResumeLayout(false);
			this.ResumeLayout(false);
		}
		#endregion

	}
}
