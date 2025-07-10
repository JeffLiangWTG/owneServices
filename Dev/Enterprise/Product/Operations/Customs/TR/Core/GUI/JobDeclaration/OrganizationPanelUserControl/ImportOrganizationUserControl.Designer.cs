namespace Enterprise.Customs.TR.GUI
{
	partial class ImportOrganizationUserControl
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
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo zAddressDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.Internal.ZAddressDropEditColumnStyleInfo();
			this.FromWarehouseAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.ToWarehouseAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.TradersGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.TradersGrid = new Enterprise.ZArchitecture.ZGrid();
			this.SellerAddressControl.SuspendLayout();
			this.ManufacturerAddressControl.SuspendLayout();
			this.RepresentativeAddressControl.SuspendLayout();
			this.DeclarantOfficeAddressControl.SuspendLayout();
			this.DefermentPartyDocAddressControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.FromWarehouseAddressControl.SuspendLayout();
			this.ToWarehouseAddressControl.SuspendLayout();
			this.TradersGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TradersGrid)).BeginInit();
			this.TradersGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// SellerAddressControl
			// 
			this.SellerAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 3, true);
			// 
			// ManufacturerAddressControl
			// 
			this.ManufacturerAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 25, true);
			// 
			// RepresentativeAddressControl
			// 
			this.RepresentativeAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 69, true);
			// 
			// DeclarantOfficeAddressControl
			// 
			this.DeclarantOfficeAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 91, true);
			// 
			// DefermentPartyDocAddressControl
			// 
			this.DefermentPartyDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 47, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.TR.Business.Declaration.JobDeclaration);
			// 
			// FromWarehouseAddressControl
			// 
			this.FromWarehouseAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FromWarehouseAddressControl, "JE_EntryFromWarehouse");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).JE_EntryFromWarehouse)));
			this.FromWarehouseAddressControl.BindToOrgList = "Lookups+LocationOfGoodsList";
			this.FromWarehouseAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 135, true);
			this.FromWarehouseAddressControl.Name = "FromWarehouseAddressControl";
			this.FromWarehouseAddressControl.PopupCaption = "";
			this.FromWarehouseAddressControl.ReadOnly = false;
			this.FromWarehouseAddressControl.ShowAddress = false;
			this.FromWarehouseAddressControl.ShowOrganisationName = true;
			this.FromWarehouseAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 20, true);
			this.FromWarehouseAddressControl.TabIndex = 6;
			// 
			// ToWarehouseAddressControl
			// 
			this.ToWarehouseAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ToWarehouseAddressControl, "JE_EntryToWarehouse");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).JE_EntryToWarehouse)));
			this.ToWarehouseAddressControl.BindToOrgList = "Lookups+LocationOfGoodsList";
			this.ToWarehouseAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 113, true);
			this.ToWarehouseAddressControl.Name = "ToWarehouseAddressControl";
			this.ToWarehouseAddressControl.PopupCaption = "";
			this.ToWarehouseAddressControl.ReadOnly = false;
			this.ToWarehouseAddressControl.ShowAddress = false;
			this.ToWarehouseAddressControl.ShowOrganisationName = true;
			this.ToWarehouseAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 20, true);
			this.ToWarehouseAddressControl.TabIndex = 5;
			// 
			// TradersGroupBox
			// 
			this.TradersGroupBox.Controls.Add(this.TradersGrid);
			this.TradersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 165, true);
			this.TradersGroupBox.Name = "TradersGroupBox";
			this.TradersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(455, 127, true);
			this.TradersGroupBox.TabIndex = 10;
			this.TradersGroupBox.TabStop = false;
			this.TradersGroupBox.Text = Enterprise.Customs.TR.GUI.Res.GetString("C0562118-84B4-4B93-BD8D-7E6AA0ABBE91", "Other Seller/Buyer");
			// 
			// TradersGrid
			// 
			this.TradersGrid.AllowNavigation = false;
			this.TradersGrid.AllowSorting = false;
			this.BindingSource.SetBindingMember(this.TradersGrid, "Traders");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).Traders)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.TR.Business.Declaration.Trader)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).Traders)).SyncRoot)).E2_AddressType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.TR.Business.Declaration.Trader)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).Traders)).SyncRoot)).OrganisationPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.TR.Business.Declaration.Trader)(((System.Collections.IList)(((Enterprise.Customs.TR.Business.Declaration.JobDeclaration)(null)).Traders)).SyncRoot)).E2_OA_Address)));
			this.TradersGrid.CaptionText = Enterprise.Customs.TR.GUI.Res.GetString("3CA6E580-5C1C-434A-B049-7A27FD8A763E", "Other Seller/Buyer");
			this.TradersGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "E2_AddressType";
			zDropEditColumnStyleInfo1.IsCustomColumn = false;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "OrganisationPK";
			zGuidFindBoxColumnStyleInfo1.IsCustomColumn = false;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zAddressDropEditColumnStyleInfo1.ColumnName = "E2_OA_Address";
			zAddressDropEditColumnStyleInfo1.IsCustomColumn = false;
			zAddressDropEditColumnStyleInfo1.IsReadOnly = true;
			zAddressDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			this.TradersGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.TradersGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.TradersGrid.ColumnStyles.Add(zAddressDropEditColumnStyleInfo1);
			this.TradersGrid.GridId = "69f97afa-c479-45ee-8a4f-930bc139d976";
			this.TradersGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TradersGrid.LayoutKey = "TradersGrid";
			this.TradersGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.TradersGrid.Name = "TradersGrid";
			this.TradersGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(449, 100, true);
			this.TradersGrid.TabIndex = 8;
			// 
			// ImportOrganizationUserControl
			// 
			this.Controls.Add(this.TradersGroupBox);
			this.Controls.Add(this.ToWarehouseAddressControl);
			this.Controls.Add(this.FromWarehouseAddressControl);
			this.Name = "ImportOrganizationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(477, 304, true);
			this.Controls.SetChildIndex(this.DefermentPartyDocAddressControl, 0);
			this.Controls.SetChildIndex(this.ManufacturerAddressControl, 0);
			this.Controls.SetChildIndex(this.SellerAddressControl, 0);
			this.Controls.SetChildIndex(this.RepresentativeAddressControl, 0);
			this.Controls.SetChildIndex(this.DeclarantOfficeAddressControl, 0);
			this.Controls.SetChildIndex(this.FromWarehouseAddressControl, 0);
			this.Controls.SetChildIndex(this.ToWarehouseAddressControl, 0);
			this.Controls.SetChildIndex(this.TradersGroupBox, 0);
			this.SellerAddressControl.ResumeLayout(true);
			this.SellerAddressControl.PerformLayout();
			this.ManufacturerAddressControl.ResumeLayout(true);
			this.ManufacturerAddressControl.PerformLayout();
			this.RepresentativeAddressControl.ResumeLayout(true);
			this.RepresentativeAddressControl.PerformLayout();
			this.DeclarantOfficeAddressControl.ResumeLayout(true);
			this.DeclarantOfficeAddressControl.PerformLayout();
			this.DefermentPartyDocAddressControl.ResumeLayout(true);
			this.DefermentPartyDocAddressControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.FromWarehouseAddressControl.ResumeLayout(true);
			this.FromWarehouseAddressControl.PerformLayout();
			this.ToWarehouseAddressControl.ResumeLayout(true);
			this.ToWarehouseAddressControl.PerformLayout();
			this.TradersGroupBox.ResumeLayout(false);
			this.TradersGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.TradersGrid)).EndInit();
			this.TradersGrid.ResumeLayout(false);
			this.TradersGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private ZArchitecture.GUI.ZAddressControl FromWarehouseAddressControl;
		private ZArchitecture.GUI.ZAddressControl ToWarehouseAddressControl;
		private ZArchitecture.GUI.ZGroupBox TradersGroupBox;
		private ZArchitecture.ZGrid TradersGrid;
	}
}
