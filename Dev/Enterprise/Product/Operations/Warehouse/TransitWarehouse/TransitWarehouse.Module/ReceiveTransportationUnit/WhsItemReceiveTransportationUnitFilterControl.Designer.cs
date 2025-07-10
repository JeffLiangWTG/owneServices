namespace Enterprise.Warehouse.Transit.Module
{
	partial class WhsItemReceiveTransportationUnitFilterControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo zDateTimeOffsetEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			this.grid.SuspendLayout();
			this.AddStripButton.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			this.BindingSource.SetBindingMember(this.grid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Warehouse.Transit.Business.WhsItemReceiveTransportationUnit)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transit.Business.WhsItemReceiveTransportationUnit)(null)).WRH_ReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transit.Business.WhsItemReceiveTransportationUnit)(null)).WRH_VehicleReference)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transit.Business.WhsItemReceiveTransportationUnit)(null)).ContainerTypeCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transit.Business.WhsItemReceiveTransportationUnit)(null)).TransportCompany.E2_CompanyName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transit.Business.WhsItemReceiveTransportationUnit)(null)).TransportCompany.OrganisationNameOrPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transit.Business.WhsItemDispatchTransportationUnit)(null)).BillingPartyCompanyName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transit.Business.WhsItemDispatchTransportationUnit)(null)).ClientRequestedBillToPartyDocAddress.OrganisationNameOrPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transit.Business.WhsItemDispatchTransportationUnit)(null)).JobHeader.JH_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTimeOffset)(((Enterprise.Warehouse.Transit.Business.WhsItemReceiveTransportationUnit)(null)).WRH_UnloadCompleteTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Core.MultilingualString)(((Enterprise.Warehouse.Transit.Business.WhsItemReceiveTransportationUnit)(null)).Warehouse.WW_WarehouseNameMultilingual)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transit.Business.WhsItemReceiveTransportationUnit)(null)).JobHeader.JH_HoldReason)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Warehouse.Transit.Business.WhsItemReceiveTransportationUnit)(null)).JobHeader.JH_ProfitLossReasonCode)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Warehouse.Transit.Business.WhsItemReceiveTransportationUnit)(null)).JobHeader.JH_TotalProfitRevenueMargin)));
            zTextBoxColumnStyleInfo1.ColumnName = "WRH_ReferenceNumber";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Transit.Module.Res.GetData("7d604195-0daa-4820-aa41-e5f4d4671f50", "RTU ID");
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "WRH_VehicleReference";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Warehouse.Transit.Module.Res.GetData("57327812-cb70-42e7-a296-96ac8e0d6275", "RTU Reference");
			zTextBoxColumnStyleInfo3.ColumnName = "Warehouse+WW_WarehouseNameMultilingual";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.ColumnName = "TransportCompany+E2_CompanyName";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Warehouse.Transit.Module.Res.GetData("8c6dcf43-e986-4fbe-8763-ff2de7bf017e", "Transport Company Name");
			zTextBoxColumnStyleInfo5.ColumnName = "ContainerTypeCode";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Warehouse.Transit.Module.Res.GetData("7ada53d7-db70-40c3-bc54-5f9e17d3651e", "Container Type");
			zTextBoxColumnStyleInfo6.ColumnName = "BillingPartyCompanyName";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Warehouse.Transit.Module.Res.GetData("9e992a55-4494-411d-9222-bf436bae0a0e", "Billing Party Company Name");
			zTextBoxColumnStyleInfo7.ColumnName = "JobHeader+JH_Status";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Warehouse.Transit.Module.Res.GetData("55043871-c9c3-472e-98df-2ce954ea3382", "Invoicing Status");
			zTextBoxColumnStyleInfo7.IsVisible = false;
			zDateTimeOffsetEditColumnStyleInfo1.ColumnName = "WRH_UnloadCompleteTime";
			zDateTimeOffsetEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zMultiControlColumnStyleInfo1.BindToDecimalPlaces = null;
			zMultiControlColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Transit.Module.Res.GetData("bc028995-fd6e-4ba0-a322-bb7a8f186050", "Transport Company");
			zMultiControlColumnStyleInfo1.ColumnName = "TransportCompany+OrganisationNameOrPK";
			zMultiControlColumnStyleInfo1.FieldTypeColumnName = "TransportCompany+OrganisationDataFieldType";
			zMultiControlColumnStyleInfo1.IsVisible = false;
			zMultiControlColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zMultiControlColumnStyleInfo2.BindToDecimalPlaces = null;
			zMultiControlColumnStyleInfo2.CaptionResourceString = Enterprise.Warehouse.Transit.Module.Res.GetData("e6f4e184-5f5f-44ea-a227-77225edcf8c7", "Bill To Party", "Client Requested Bill To Party");
			zMultiControlColumnStyleInfo2.ColumnName = "ClientRequestedBillToPartyDocAddress+OrganisationNameOrPK";
			zMultiControlColumnStyleInfo2.FieldTypeColumnName = "ClientRequestedBillToPartyDocAddress+OrganisationDataFieldType";
			zMultiControlColumnStyleInfo2.IsVisible = false;
			zMultiControlColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo8.ColumnName = "JobHeader+JH_HoldReason";
			zTextBoxColumnStyleInfo8.IsVisible = false;
			zTextBoxColumnStyleInfo9.ColumnName = "JobHeader+JH_ProfitLossReasonCode";
			zTextBoxColumnStyleInfo9.IsVisible = false;
            zCalcEditColumnStyleInfo1.ColumnName = "JobHeader+JH_TotalProfitRevenueMargin";
            zCalcEditColumnStyleInfo1.IsVisible = false;
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.grid.ColumnStyles.Add(zDateTimeOffsetEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zMultiControlColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zMultiControlColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
            this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Warehouse.Transit.Business.WhsItemReceiveTransportationUnit);
			// 
			// WhsItemReceiveTransportationUnitFilterControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Name = "WhsItemReceiveTransportationUnitFilterControl";
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			this.grid.ResumeLayout(false);
			this.grid.PerformLayout();
			this.AddStripButton.ResumeLayout(true);
			this.AddStripButton.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
	}
}
