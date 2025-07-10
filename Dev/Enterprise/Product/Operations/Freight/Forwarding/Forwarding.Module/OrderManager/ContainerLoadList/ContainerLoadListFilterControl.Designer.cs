
namespace Enterprise.Freight.Forwarding.Module
{
	partial class ContainerLoadListFilterControl
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
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zSupplierMultiControlColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zSupplierTextBoxColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo zControllingCustomerMultiControlColumnStyleInfo = new Enterprise.ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zControllingCustomerTextBoxColumnStyleInfo = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();

			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
            this.grid.SuspendLayout();
            this.AddStripButton.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.SuspendLayout();
			// 
			// grid
			// 
			zTextBoxColumnStyleInfo1.ColumnName = "CLH_LoadListId";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("ConatienrLoadListFilterControl|78641C29-43F3-424F-A798-44BF28FDFBA8", "Load List #");
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "Booking+JSB_BookingId";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("ConatienrLoadListFilterControl|C4CFAB80-362A-42F7-A0C0-D173D14A4CAE", "Supplier Booking #");
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "CLH_OH_LoadListParty";
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("ConatienrLoadListFilterControl|6CA2FD39-F505-44F9-A01C-6A087E4892FB", "Load List Party");
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.ColumnName = "CLH_Status";
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("ConatienrLoadListFilterControl|7C0CCA6B-28C5-4DA1-AF6A-D1CF98F50029", "Status");
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.ColumnName = "Booking+JSB_TransportMode";
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("ConatienrLoadListFilterControl|996C87F6-589A-43F4-9AD0-EB2531DAD2CF", "Transport Mode");
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.ColumnName = "Booking+JSB_RL_NKLoadPort";
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("ConatienrLoadListFilterControl|FB0BB62A-497D-4011-9D3A-F360C2527D6F", "Load Port(Booking)");
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.ColumnName = "Booking+JSB_RL_NKDischargePort";
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("ConatienrLoadListFilterControl|3EED70BC-2986-4FF1-8B97-6A07E097BDFD", "Discharge Port(Booking)");
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.ColumnName = "Booking+JSB_IncoTerm";
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("ConatienrLoadListFilterControl|1B750B32-27C2-448F-BF71-A581D11BD88C", "Inco Term");
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zSupplierMultiControlColumnStyleInfo.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("JobSupplierBookingFilterControl|f1803e84-15fa-4044-868a-514169723318", "Supplier");
			zSupplierMultiControlColumnStyleInfo.ColumnName = "Booking.SupplierNameOrPK";
			zSupplierMultiControlColumnStyleInfo.FieldTypeColumnName = "Booking.SupplierFieldType";
			zSupplierMultiControlColumnStyleInfo.IsVisible = false;
			zSupplierTextBoxColumnStyleInfo.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("JobSupplierBookingFilterControl|5389202b-f816-44c3-91a7-0936b6508e3a", "Supplier Address");
			zSupplierTextBoxColumnStyleInfo.ColumnName = "Booking.SupplierAddress.E2_Address1";
			zSupplierTextBoxColumnStyleInfo.IsVisible = false;
			zControllingCustomerMultiControlColumnStyleInfo.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("JobSupplierBookingFilterControl|b91cd95c-5e29-4e88-a2b7-266e629ca207", "Controlling Customer");
			zControllingCustomerMultiControlColumnStyleInfo.ColumnName = "Booking.ControllingCustomerNameOrPK";
			zControllingCustomerMultiControlColumnStyleInfo.FieldTypeColumnName = "Booking.ControllingCustomerFieldType";
			zControllingCustomerMultiControlColumnStyleInfo.IsVisible = false;
			zControllingCustomerTextBoxColumnStyleInfo.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("JobSupplierBookingFilterControl|ed6039db-15fd-4745-928b-4ad09a4de6cb", "Controlling Customer Address");
			zControllingCustomerTextBoxColumnStyleInfo.ColumnName = "Booking.ControllingCustomerAddress.E2_Address1";
			zControllingCustomerTextBoxColumnStyleInfo.IsVisible = false;
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.grid.ColumnStyles.Add(zSupplierMultiControlColumnStyleInfo);
			this.grid.ColumnStyles.Add(zSupplierTextBoxColumnStyleInfo);
			this.grid.ColumnStyles.Add(zControllingCustomerMultiControlColumnStyleInfo);
			this.grid.ColumnStyles.Add(zControllingCustomerTextBoxColumnStyleInfo);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.Orders.Business.ContainerLoadListCollection);
            // 
            // ContainerLoadListFilterControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Name = "ContainerLoadListFilterControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(758, 383, true);
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
