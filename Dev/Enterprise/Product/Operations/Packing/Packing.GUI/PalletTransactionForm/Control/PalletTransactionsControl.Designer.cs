namespace Enterprise.Packing.GUI
{
	partial class PalletTransactionsControl
	{
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo2 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			this.PalletTransactionsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PalletTransactionsGrid)).BeginInit();
			this.PalletTransactionsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// PalletTransactionsGrid
			// 
			this.PalletTransactionsGrid.AllowNavigation = false;
			this.PalletTransactionsGrid.CaptionVisible = false;
			zGuidDropEditColumnStyleInfo1.ColumnName = "KTR_ParentID";
			zGuidDropEditColumnStyleInfo1.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zDropEditColumnStyleInfo1.ColumnName = "KTR_PalletType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo1.ColumnName = "PalletTypeDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo2.ColumnName = "KTR_EquipmentCode";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "KTR_Quantity";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo3.ColumnName = "KTR_PaperDocketID";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo2.ColumnName = "KTR_TransactionType";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo4.ColumnName = "TransactionTypeDescription";
			zTextBoxColumnStyleInfo4.IsVisible = false;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo3.ColumnName = "KTR_Status";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo5.ColumnName = "StatusDescription";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Packing.GUI.Res.GetData("f28e36ac-7d54-434c-bbb3-92877a5b5496", "Transfer From Account Number");
			zTextBoxColumnStyleInfo6.ColumnName = "KTR_TransferFromAccountNumber";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Packing.GUI.Res.GetData("d4defc4a-b05c-43ca-91d5-e6e4dddba959", "Transfer To Account Number");
			zTextBoxColumnStyleInfo7.ColumnName = "KTR_TransferToAccountNumber";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zOrganisationFindBoxColumnStyleInfo1.BindToList = "Lookups+Parties";
			zOrganisationFindBoxColumnStyleInfo1.Caption = "";
			zOrganisationFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Packing.GUI.Res.GetData("7c7b25a7-cdc4-47f1-be46-efee3ddc9b9b", "From");
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "TransferFrom+OrganisationPK";
			zOrganisationFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zOrganisationFindBoxColumnStyleInfo2.BindToList = "Lookups+Parties";
			zOrganisationFindBoxColumnStyleInfo2.Caption = "";
			zOrganisationFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Packing.GUI.Res.GetData("bf0a0a5b-dd0c-410b-9f42-ea3b17d58e2a", "To");
			zOrganisationFindBoxColumnStyleInfo2.ColumnName = "TransferTo+OrganisationPK";
			zOrganisationFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.PalletTransactionsGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.PalletTransactionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.PalletTransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.PalletTransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.PalletTransactionsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.PalletTransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.PalletTransactionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.PalletTransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.PalletTransactionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.PalletTransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.PalletTransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.PalletTransactionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.PalletTransactionsGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.PalletTransactionsGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo2);
			this.PalletTransactionsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PalletTransactionsGrid.GridId = "2d22df03-00d2-413f-aea7-c3fa76e853e9";
			this.PalletTransactionsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PalletTransactionsGrid.LayoutKey = "PalletTransactionsGrid";
			this.PalletTransactionsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PalletTransactionsGrid.Name = "PalletTransactionsGrid";
			this.PalletTransactionsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1335, 126, true);
			this.PalletTransactionsGrid.TabIndex = 4;
			// 
			// PalletTransactionsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PalletTransactionsGrid);
			this.Name = "PalletTransactionsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1335, 126, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PalletTransactionsGrid)).EndInit();
			this.PalletTransactionsGrid.ResumeLayout(false);
			this.PalletTransactionsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		System.ComponentModel.IContainer components = null;
		ZArchitecture.ZGrid PalletTransactionsGrid;
	}
}
