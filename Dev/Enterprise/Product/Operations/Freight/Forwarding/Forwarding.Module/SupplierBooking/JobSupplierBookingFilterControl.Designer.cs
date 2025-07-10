
namespace Enterprise.Freight.Forwarding.Module
{
	partial class JobSupplierBookingFilterControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zSupplierMultiControlColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zSupplierTextBoxColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zControllingCustomerMultiControlColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zControllingCustomerTextBoxColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// FilteredGrid
			//
			zTextBoxColumnStyleInfo1.ColumnName = "JSB_BookingId";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("JobSupplierBookingFilterControl|60801f56-af44-41f8-a2c7-b6c2ffef0b0b", "Booking #");
			zTextBoxColumnStyleInfo2.ColumnName = "JSB_Status";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("JobSupplierBookingFilterControl|afe2d750-1863-43d3-862b-9a0d60bc272b", "Booking Status");
			zTextBoxColumnStyleInfo3.ColumnName = "JSB_LoadMode";
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("JobSupplierBookingFilterControl|6a059ae1-1af1-4be0-b443-7ba91345b575", "Load Mode");
			zTextBoxColumnStyleInfo4.ColumnName = "JSB_TransportMode";
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("JobSupplierBookingFilterControl|1fa255e9-f083-494a-bdfd-0281a7b4137a", "Transport Mode");
			zTextBoxColumnStyleInfo5.ColumnName = "JSB_RL_NKLoadPort";
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("JobSupplierBookingFilterControl|5ff25203-7eef-4549-9370-165b238ede0d", "Load Port");
			zTextBoxColumnStyleInfo6.ColumnName = "JSB_RL_NKDischargePort";
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("JobSupplierBookingFilterControl|25218173-90a9-4a41-b2d8-f2d530f1294b", "Discharge Port");
			zTextBoxColumnStyleInfo8.ColumnName = "JSB_IncoTerm";
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("JobSupplierBookingFilterControl|6e3fc052-a3ea-4e59-9898-3c9505d632c3", "Incoterm");
			zTextBoxColumnStyleInfo8.IsVisible = false;
			zGuidFindBoxColumnStyleInfo1.ColumnName = "JSB_OH_BookingParty";
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("JobSupplierBookingFilterControl|2eb16014-d42a-4f5e-8278-869278d04963", "Booking Party");
			zDateEditColumnStyleInfo1.ColumnName = "JSB_BookedOnDate";
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("JobSupplierBookingFilterControl|00287570-7446-4505-97dc-0ff66901a179", "Booking Date");
			zDateEditColumnStyleInfo2.ColumnName = "JSB_CargoAvailableDate";
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("JobSupplierBookingFilterControl|4CBEEFB4-05FF-4C82-A2C9-CD047763A9F2", "Cargo Available Date");
			zDateEditColumnStyleInfo2.IsVisible = false;
			zSupplierMultiControlColumnStyleInfo.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("JobSupplierBookingFilterControl|f1803e84-15fa-4044-868a-514169723318", "Supplier");
			zSupplierMultiControlColumnStyleInfo.ColumnName = "SupplierNameOrPK";
			zSupplierMultiControlColumnStyleInfo.FieldTypeColumnName = "SupplierFieldType";
			zSupplierMultiControlColumnStyleInfo.IsVisible = false;
			zSupplierTextBoxColumnStyleInfo.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("JobSupplierBookingFilterControl|5389202b-f816-44c3-91a7-0936b6508e3a", "Supplier Address");
			zSupplierTextBoxColumnStyleInfo.ColumnName = "SupplierAddress.E2_Address1";
			zSupplierTextBoxColumnStyleInfo.IsVisible = false;
			zControllingCustomerMultiControlColumnStyleInfo.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("JobSupplierBookingFilterControl|b91cd95c-5e29-4e88-a2b7-266e629ca207", "Controlling Customer");
			zControllingCustomerMultiControlColumnStyleInfo.ColumnName = "ControllingCustomerNameOrPK";
			zControllingCustomerMultiControlColumnStyleInfo.FieldTypeColumnName = "ControllingCustomerFieldType";
			zControllingCustomerTextBoxColumnStyleInfo.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("JobSupplierBookingFilterControl|ed6039db-15fd-4745-928b-4ad09a4de6cb", "Controlling Customer Address");
			zControllingCustomerTextBoxColumnStyleInfo.ColumnName = "ControllingCustomerAddress.E2_Address1";
			zControllingCustomerTextBoxColumnStyleInfo.IsVisible = false;
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.FilteredGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zSupplierMultiControlColumnStyleInfo);
			this.grid.ColumnStyles.Add(zSupplierTextBoxColumnStyleInfo);
			this.grid.ColumnStyles.Add(zControllingCustomerMultiControlColumnStyleInfo);
			this.grid.ColumnStyles.Add(zControllingCustomerTextBoxColumnStyleInfo);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.Orders.Business.JobSupplierBookingCollection);
			// 
			// JobSupplierBookingFilterControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Name = "JobSupplierBookingFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(758, 383, true);
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
	}
}
