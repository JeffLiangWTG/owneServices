namespace Enterprise.MasterFiles.GUI
{
	public partial class WarehouseUserControl
	{

		#region Component Designer generated code

		internal Enterprise.ZArchitecture.GUI.ZTemplateTabControl WarehouseTabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage DetailsTabPage;
		private Enterprise.ZArchitecture.GUI.ZTabPage ProductPrefsTabPage;
		internal Enterprise.ZArchitecture.GUI.ZTabPage InvoicingTabPage;
		private Enterprise.ZArchitecture.GUI.ZTabPage EdiTabPage;
		private WhsDetailsUserControl whsDetailsUserControl1;
		private WhsProductPrefsUserControl whsProductPrefsUserControl1;
		private WhsInvoicingUserControl whsInvoicingUserControl1;
		private WhsEDIUserControl whsEDIUserControl1;
		private Enterprise.ZArchitecture.GUI.ZTabPage ShippingTabPage;
		private Enterprise.ZArchitecture.GUI.ZPanel ShippingPanel;
		private WhsShippingUserControl whsShippingUserControl1;
		private System.ComponentModel.IContainer components;

		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.WarehouseTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.DetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ProductPrefsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.InvoicingTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.EdiTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ShippingTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.SecurityPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.WarehouseTabControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// SecurityPanel
			// 
			this.SecurityPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(736, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			// 
			// WarehouseTabControl
			// 
			this.WarehouseTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.WarehouseTabControl.Controls.Add(this.DetailsTabPage);
			this.WarehouseTabControl.Controls.Add(this.ProductPrefsTabPage);
			this.WarehouseTabControl.Controls.Add(this.InvoicingTabPage);
			this.WarehouseTabControl.Controls.Add(this.EdiTabPage);
			this.WarehouseTabControl.Controls.Add(this.ShippingTabPage);
			this.WarehouseTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.WarehouseTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 24, true);
			this.WarehouseTabControl.Name = "WarehouseTabControl";
			this.WarehouseTabControl.SelectedIndex = 0;
			this.WarehouseTabControl.ShowToolTips = true;
			this.WarehouseTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(736, 427, true);
			this.WarehouseTabControl.TabIndex = 0;
			// 
			// DetailsTabPage
			// 
			this.DetailsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("WarehouseUserControl|ea902b3f-a461-421f-a9f7-5aa80587da9f", "Details");
			this.DetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DetailsTabPage.Name = "DetailsTabPage";
			this.DetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(728, 400, true);
			this.DetailsTabPage.TabIndex = 0;
			this.DetailsTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.DetailsTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.OrgHeader)(((Enterprise.MasterFiles.Business.OrgHeader)(null)))));
			// 
			// ProductPrefsTabPage
			// 
			this.ProductPrefsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("WarehouseUserControl|83dae1be-421f-4b74-a7d8-43325faef075", "Product Preferences");
			this.ProductPrefsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ProductPrefsTabPage.Name = "ProductPrefsTabPage";
			this.ProductPrefsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(728, 400, true);
			this.ProductPrefsTabPage.TabIndex = 1;
			this.ProductPrefsTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.ProductPrefsTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.OrgHeader)(((Enterprise.MasterFiles.Business.OrgHeader)(null)))));
			// 
			// InvoicingTabPage
			// 
			this.InvoicingTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("WarehouseUserControl|799abd3c-6169-47c2-a8d0-1ee127974d4d", "Invoicing");
			this.InvoicingTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.InvoicingTabPage.Name = "InvoicingTabPage";
			this.InvoicingTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(728, 400, true);
			this.InvoicingTabPage.TabIndex = 3;
			this.InvoicingTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.InvoicingTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.OrgHeader)(((Enterprise.MasterFiles.Business.OrgHeader)(null)))));
			// 
			// EdiTabPage
			// 
			this.EdiTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("WarehouseUserControl|8e2f5185-3544-4b2b-adba-b375ed3ded6d", "EDI");
			this.EdiTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.EdiTabPage.Name = "EdiTabPage";
			this.EdiTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(728, 400, true);
			this.EdiTabPage.TabIndex = 4;
			this.EdiTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.EdiTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.OrgHeader)(((Enterprise.MasterFiles.Business.OrgHeader)(null)))));
			// 
			// ShippingTabPage
			// 
			this.ShippingTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("WarehouseUserControl|b34eea0f-93de-4be2-9367-cf14fbaa471e", "Shipping");
			this.ShippingTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ShippingTabPage.Name = "ShippingTabPage";
			this.ShippingTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ShippingTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(728, 400, true);
			this.ShippingTabPage.TabIndex = 5;
			this.ShippingTabPage.UseVisualStyleBackColor = true;
			this.ShippingTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.ShippingTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.OrgHeader)(((Enterprise.MasterFiles.Business.OrgHeader)(null)))));
			// 
			// WarehouseUserControl
			// 
			this.Controls.Add(this.WarehouseTabControl);
			this.IsModifyWarehouse = true;
			this.Name = "WarehouseUserControl";
			this.ShouldSerializeTabPageMethods = true;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(736, 451, true);
			this.Controls.SetChildIndex(this.SecurityPanel, 0);
			this.Controls.SetChildIndex(this.WarehouseTabControl, 0);
			this.SecurityPanel.ResumeLayout(false);
			this.SecurityPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.WarehouseTabControl.ResumeLayout(false);
			this.WarehouseTabControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		private void DetailsTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.whsDetailsUserControl1 = new Enterprise.MasterFiles.GUI.WhsDetailsUserControl();
			this.DetailsTabPage.SuspendLayout();
			this.whsDetailsUserControl1.SuspendLayout();
			this.DetailsTabPage.Controls.Add(this.whsDetailsUserControl1);
			// 
			// whsDetailsUserControl1
			// 
			this.whsDetailsUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.whsDetailsUserControl1, ".");
			this.whsDetailsUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.whsDetailsUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.whsDetailsUserControl1.Name = "whsDetailsUserControl1";
			this.whsDetailsUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 73, true);
			this.whsDetailsUserControl1.TabIndex = 0;
			this.DetailsTabPage.PerformLayout();
			this.whsDetailsUserControl1.ResumeLayout(true);
			this.whsDetailsUserControl1.PerformLayout();
			this.DetailsTabPage.ResumeLayout(true);
		}

		private void ProductPrefsTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.whsProductPrefsUserControl1 = new Enterprise.MasterFiles.GUI.WhsProductPrefsUserControl();
			this.ProductPrefsTabPage.SuspendLayout();
			this.whsProductPrefsUserControl1.SuspendLayout();
			this.ProductPrefsTabPage.Controls.Add(this.whsProductPrefsUserControl1);
			// 
			// whsProductPrefsUserControl1
			// 
			this.whsProductPrefsUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.whsProductPrefsUserControl1, ".");
			this.whsProductPrefsUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.whsProductPrefsUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.whsProductPrefsUserControl1.Name = "whsProductPrefsUserControl1";
			this.whsProductPrefsUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 73, true);
			this.whsProductPrefsUserControl1.TabIndex = 0;
			this.ProductPrefsTabPage.PerformLayout();
			this.whsProductPrefsUserControl1.ResumeLayout(true);
			this.whsProductPrefsUserControl1.PerformLayout();
			this.ProductPrefsTabPage.ResumeLayout(true);
		}

		private void InvoicingTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.whsInvoicingUserControl1 = new Enterprise.MasterFiles.GUI.WhsInvoicingUserControl();
			this.InvoicingTabPage.SuspendLayout();
			this.whsInvoicingUserControl1.SuspendLayout();
			this.InvoicingTabPage.Controls.Add(this.whsInvoicingUserControl1);
			// 
			// whsInvoicingUserControl1
			// 
			this.whsInvoicingUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.whsInvoicingUserControl1, ".");
			this.whsInvoicingUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.whsInvoicingUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.whsInvoicingUserControl1.Name = "whsInvoicingUserControl1";
			this.whsInvoicingUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(192, 73, true);
			this.whsInvoicingUserControl1.TabIndex = 0;
			this.InvoicingTabPage.PerformLayout();
			this.whsInvoicingUserControl1.ResumeLayout(true);
			this.whsInvoicingUserControl1.PerformLayout();
			this.InvoicingTabPage.ResumeLayout(true);
		}

		private void EdiTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.whsEDIUserControl1 = new Enterprise.MasterFiles.GUI.WhsEDIUserControl();
			this.EdiTabPage.SuspendLayout();
			this.whsEDIUserControl1.SuspendLayout();
			this.EdiTabPage.Controls.Add(this.whsEDIUserControl1);
			// 
			// whsEDIUserControl1
			// 
			this.whsEDIUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.whsEDIUserControl1, ".");
			this.whsEDIUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.whsEDIUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.whsEDIUserControl1.Name = "whsEDIUserControl1";
			this.whsEDIUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(728, 400, true);
			this.whsEDIUserControl1.TabIndex = 0;
			this.EdiTabPage.PerformLayout();
			this.whsEDIUserControl1.ResumeLayout(true);
			this.whsEDIUserControl1.PerformLayout();
			this.EdiTabPage.ResumeLayout(true);
		}

		#endregion

		void ShippingTabPage_InitializeTab(object sender, System.EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.ShippingPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.whsShippingUserControl1 = new Enterprise.MasterFiles.GUI.WhsShippingUserControl();
			this.ShippingTabPage.SuspendLayout();
			this.ShippingPanel.SuspendLayout();
			this.whsShippingUserControl1.SuspendLayout();
			this.ShippingTabPage.Controls.Add(this.ShippingPanel);
			// 
			// ShippingPanel
			// 
			this.ShippingPanel.Controls.Add(this.whsShippingUserControl1);
			this.ShippingPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ShippingPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ShippingPanel.Name = "ShippingPanel";
			this.ShippingPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(722, 394, true);
			this.ShippingPanel.TabIndex = 1;
			// 
			// whsShippingUserControl1
			// 
			this.whsShippingUserControl1.AllowDrop = true;
			this.whsShippingUserControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.whsShippingUserControl1, ".");
			this.whsShippingUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.whsShippingUserControl1.Name = "whsShippingUserControl1";
			this.whsShippingUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(716, 388, true);
			this.whsShippingUserControl1.TabIndex = 0;
			this.ShippingTabPage.PerformLayout();
			this.ShippingPanel.ResumeLayout(false);
			this.ShippingPanel.PerformLayout();
			this.whsShippingUserControl1.ResumeLayout(true);
			this.whsShippingUserControl1.PerformLayout();
			this.ShippingTabPage.ResumeLayout(true);
		}
	}
}
