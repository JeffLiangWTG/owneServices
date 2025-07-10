namespace Enterprise.MasterFiles.GUI
{
	public partial class TemporaryOrgRemoverErrorsForm
	{

		#region Windows Form Designer generated code

		private Enterprise.ZArchitecture.ZGrid OrgGrid;
		private Enterprise.ZArchitecture.ZLabel zLabel1;
		private Enterprise.ZArchitecture.ZLabel zLabel2;
		private Enterprise.ZArchitecture.ZLabel zLabel4;
		private Enterprise.ZArchitecture.GUI.ZButton CancelButtonX;

		protected override void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.OrgGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CancelButtonX = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel4 = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.OrgGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 440, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(608, 24, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(296);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(297);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.TemporaryOrgRemover.Remover);
			// 
			// OrgGrid
			// 
			this.OrgGrid.AllowNavigation = false;
			this.OrgGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.OrgGrid, "FailedOrganisations");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.TemporaryOrgRemover.Remover)(null)).FailedOrganisations)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.TemporaryOrgRemover.TemporaryOrg)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.TemporaryOrgRemover.Remover)(null)).FailedOrganisations)).SyncRoot)).Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.TemporaryOrgRemover.TemporaryOrg)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.TemporaryOrgRemover.Remover)(null)).FailedOrganisations)).SyncRoot)).FullName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.TemporaryOrgRemover.TemporaryOrg)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.TemporaryOrgRemover.Remover)(null)).FailedOrganisations)).SyncRoot)).ChildTableForFailedDelete)));
			this.OrgGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TemporaryOrgRemoverErrorsForm|5d54a6f0-4b6a-4269-ba49-f0f442d0e1f5", "Code");
			zTextBoxColumnStyleInfo1.ColumnName = "Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TemporaryOrgRemoverErrorsForm|d8787c16-6bd0-4737-9183-1fa29a7f4684", "Name");
			zTextBoxColumnStyleInfo2.ColumnName = "FullName";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TemporaryOrgRemoverErrorsForm|6bee695f-0a3d-4e06-9cb2-ecb305c40e0f", "Related Item");
			zTextBoxColumnStyleInfo3.ColumnName = "ChildTableForFailedDelete";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.OrgGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.OrgGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.OrgGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.OrgGrid.GridId = "6a9e118e-a7d0-4eb0-bf52-2cb7bdd7b4dd";
			this.OrgGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OrgGrid.LayoutKey = "zGrid1";
			this.OrgGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 119, true);
			this.OrgGrid.Name = "OrgGrid";
			this.OrgGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(584, 285, true);
			this.OrgGrid.TabIndex = 1;
			// 
			// CancelButtonX
			// 
			this.CancelButtonX.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelButtonX.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TemporaryOrgRemoverErrorsForm|081ff7d3-cacb-4f58-8c8a-265745d65092", "OK");
			this.CancelButtonX.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelButtonX.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(520, 410, true);
			this.CancelButtonX.Name = "CancelButtonX";
			this.CancelButtonX.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 24, true);
			this.CancelButtonX.TabIndex = 5;
			// 
			// zLabel1
			// 
			this.zLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.zLabel1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TemporaryOrgRemoverErrorsForm|fa0a0080-21ae-49d3-9c60-2ce7bc1d6c4f", "", "Some Temporary Organizations could not be deleted because they are still in use.");
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 4, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(588, 32, true);
			this.zLabel1.TabIndex = 6;
			// 
			// zLabel2
			// 
			this.zLabel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.zLabel2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TemporaryOrgRemoverErrorsForm|45855e82-361a-4b66-bb2e-8be804988581", "", "Listed below are the organizations that could not be deleted, and shown beside it is the the item that is still linked to the organization, preventing deletion. To delete these organizations, you will need to open the organization record and remove any of the Related Items still showing.");
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 38, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(584, 48, true);
			this.zLabel2.TabIndex = 7;
			// 
			// zLabel4
			// 
			this.zLabel4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.zLabel4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TemporaryOrgRemoverErrorsForm|98704409-fe1c-48a1-9d54-a63f6e848227", "", "NB: It is recommended that you export this list to Excel, by right-clicking the grid and selecting \"Export to Excel\".");
			this.zLabel4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 92, true);
			this.zLabel4.Name = "zLabel4";
			this.zLabel4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(584, 24, true);
			this.zLabel4.TabIndex = 9;
			// 
			// TemporaryOrgRemoverErrorsForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(608, 464, true);
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("TemporaryOrgRemoverErrorsForm|81b23804-8759-4abb-b058-de7a64f21ce8", "Could Not Delete Some Organizations");
			this.Controls.Add(this.zLabel4);
			this.Controls.Add(this.zLabel2);
			this.Controls.Add(this.zLabel1);
			this.Controls.Add(this.OrgGrid);
			this.Controls.Add(this.CancelButtonX);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.TemporaryOrgRemover.Remover);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 300, true);
			this.Name = "TemporaryOrgRemoverErrorsForm";
			this.Controls.SetChildIndex(this.CancelButtonX, 0);
			this.Controls.SetChildIndex(this.OrgGrid, 0);
			this.Controls.SetChildIndex(this.zLabel1, 0);
			this.Controls.SetChildIndex(this.zLabel2, 0);
			this.Controls.SetChildIndex(this.zLabel4, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.OrgGrid)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

	}
}
