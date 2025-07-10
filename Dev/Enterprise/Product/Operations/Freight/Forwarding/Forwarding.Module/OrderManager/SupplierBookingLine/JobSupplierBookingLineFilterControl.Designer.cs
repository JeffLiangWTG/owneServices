namespace Enterprise.Freight.Forwarding.Module
{
	partial class JobSupplierBookingLineFilterControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// FilteredGrid
			//
			zTextBoxColumnStyleInfo1.ColumnName = "SupplierBooking.JSB_BookingId";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("JobSupplierBookingLineFilterControl|3149350f-929f-4c4e-b952-672a8f1cc2eb", "Booking #");
			zTextBoxColumnStyleInfo2.ColumnName = "OrderLine.Order.JD_OrderNumber";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("JobSupplierBookingLineFilterControl|dc089879-4485-4e9b-ab20-beb144bdde9b", "Order No");
			zTextBoxColumnStyleInfo3.ColumnName = "OrderLine.JO_LineNo";
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("JobSupplierBookingLineFilterControl|bcc653c1-f0f2-4649-aa21-3c391ac56c34", "Order Line No");
			zTextBoxColumnStyleInfo4.ColumnName = "OrderLine.JO_Partno";
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("JobSupplierBookingLineFilterControl|bb014ed5-8f59-4454-acc8-1940ad204d39", "Part No");
			zTextBoxColumnStyleInfo5.ColumnName = "OrderLine.JO_LineReference";
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("JobSupplierBookingLineFilterControl|8c2d5156-06a6-41b4-bba1-678b2be5b07d", "Line Reference ");
			zTextBoxColumnStyleInfo6.ColumnName = "SupplierBooking.JSB_GoodsDescription";
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("JobSupplierBookingLineFilterControl|99e7084b-7259-46f5-8b88-aa6dec73896b", "Booking Goods Description");
			zTextBoxColumnStyleInfo8.ColumnName = "OrderLine.JO_LineSplitNumber";
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("JobSupplierBookingLineFilterControl|6da7d8a9-95a1-4c14-a663-4fb2b29eb6e0", "Order Line Split No");
			zTextBoxColumnStyleInfo9.ColumnName = "OrderLine.JO_SubLineNo";
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("JobSupplierBookingLineFilterControl|6da7d8a9-95a1-4c14-a663-4fb2b29eb6e0", "Order Line Sub Line No");
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.Forwarding.Orders.Business.JobSupplierBookingLineCollection);
			// 
			// JobSupplierBookingLineFilterControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Name = "JobSupplierBookingLineFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(758, 383, true);
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
	}
}
