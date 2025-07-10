namespace Enterprise.MasterFiles.GUI
{
	public partial class ConsignorUserControl
	{

		#region Component Designer generated code

		protected Enterprise.ZArchitecture.GUI.ZTabPage DetailsTabPage;
		protected Enterprise.ZArchitecture.GUI.ZTabPage RelationshipsTabPage;
		protected Enterprise.ZArchitecture.GUI.ZTemplateTabControl ConsignorTabControl;
		protected Enterprise.MasterFiles.GUI.ConsignorDetailsUserControl DetailsControl;
		protected Enterprise.MasterFiles.GUI.ConsignorRelationshipsUserControl consignorRelationshipsControl1;
		private System.ComponentModel.IContainer components;

		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.ConsignorTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.DetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.DetailsControl = new Enterprise.MasterFiles.GUI.ConsignorDetailsUserControl();
			this.RelationshipsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.consignorRelationshipsControl1 = new Enterprise.MasterFiles.GUI.ConsignorRelationshipsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ConsignorTabControl.SuspendLayout();
			this.DetailsTabPage.SuspendLayout();
			this.RelationshipsTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// SecurityPanel
			// 
			this.SecurityPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 5, true);
			this.SecurityPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(870, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgHeader);
			// 
			// ConsignorTabControl
			// 
			this.ConsignorTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.ConsignorTabControl.Controls.Add(this.DetailsTabPage);
			this.ConsignorTabControl.Controls.Add(this.RelationshipsTabPage);
			this.ConsignorTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConsignorTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 29, true);
			this.ConsignorTabControl.Name = "ConsignorTabControl";
			this.ConsignorTabControl.SelectedIndex = 0;
			this.ConsignorTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(870, 518, true);
			this.ConsignorTabControl.TabIndex = 0;
			// 
			// DetailsTabPage
			// 
			this.DetailsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConsignorUserControl|2c71b284-17b8-4e27-a626-96b7986c34f5", "Details");
			this.DetailsTabPage.Controls.Add(this.DetailsControl);
			this.DetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DetailsTabPage.Name = "DetailsTabPage";
			this.DetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(862, 491, true);
			this.DetailsTabPage.TabIndex = 0;
			// 
			// DetailsControl
			// 
			this.BindingSource.SetBindingMember(this.DetailsControl, ".");
			this.DetailsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailsControl.Name = "DetailsControl";
			this.DetailsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(862, 491, true);
			this.DetailsControl.TabIndex = 0;
			// 
			// RelationshipsTabPage
			// 
			this.RelationshipsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ConsignorUserControl|e7b627cc-ac91-4199-b7ab-4421ef11955c", "Consignee / Buyer / Importer Defaults");
			this.RelationshipsTabPage.Controls.Add(this.consignorRelationshipsControl1);
			this.RelationshipsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.RelationshipsTabPage.Name = "RelationshipsTabPage";
			this.RelationshipsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(862, 491, true);
			this.RelationshipsTabPage.TabIndex = 1;
			// 
			// consignorRelationshipsControl1
			// 
			this.BindingSource.SetBindingMember(this.consignorRelationshipsControl1, "BuyerLinks");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.OrgBuyerLinkCollection)(((Enterprise.MasterFiles.Business.OrgHeader)(null)).BuyerLinks)));
			this.consignorRelationshipsControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.consignorRelationshipsControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.consignorRelationshipsControl1.Name = "consignorRelationshipsControl1";
			this.consignorRelationshipsControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(862, 491, true);
			this.consignorRelationshipsControl1.TabIndex = 0;
			// 
			// ConsignorUserControl
			// 
			this.Controls.Add(this.ConsignorTabControl);
			this.IsModifyConsignor = true;
			this.IsModifyConsignorDetails = true;
			this.IsModifyConsignorExporterScheme = true;
			this.IsModifyConsignorRelationships = true;
			this.Name = "ConsignorUserControl";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
			this.ShouldSerializeTabPageMethods = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(880, 552, true);
			this.Controls.SetChildIndex(this.SecurityPanel, 0);
			this.Controls.SetChildIndex(this.ConsignorTabControl, 0);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ConsignorTabControl.ResumeLayout(false);
			this.DetailsTabPage.ResumeLayout(false);
			this.RelationshipsTabPage.ResumeLayout(false);
			this.ResumeLayout(false);
		}
		#endregion

	}
}
