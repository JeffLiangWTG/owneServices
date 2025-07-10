using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.Module
{
	public partial class DocumentTrackingFilterControl : ZFilterStripControl
	{
		void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo zDateTimeOffsetEditColumnStyleInfo1 = new ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			zTextBoxColumnStyleInfo1.Caption = null;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("DocumentTrackingFilterControl|a11535fd-1931-46d7-a45c-3e299aae28f0", "Type");
			zTextBoxColumnStyleInfo1.ColumnName = "EQ_DocType";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo2.Caption = null;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("DocumentTrackingFilterControl|ffa01cbb-a436-4133-b19c-4b2b4c0960a0", "Doc Description");
			zTextBoxColumnStyleInfo2.ColumnName = "EQ_DocDescriptionMultilingual";
			zTextBoxColumnStyleInfo3.Caption = null;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("DocumentTrackingFilterControl|01a5931a-bc52-4665-b57f-cc03620ff87d", "Doc Number");
			zTextBoxColumnStyleInfo3.ColumnName = "EQ_DocNumber";
			zTextBoxColumnStyleInfo4.Caption = null;
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("DocumentTrackingFilterControl|d9269db5-1389-4b61-838a-8c12792918fd", "Owner");
			zTextBoxColumnStyleInfo4.ColumnName = "EQ_Calc_DocumentOwnerCode";
			zDateTimeOffsetEditColumnStyleInfo1.Caption = null;
			zDateTimeOffsetEditColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("DocumentTrackingFilterControl|1335f9d5-235d-4d15-8c55-c60e6653c4fb", "Rcvd. Date", "Received Date");
			zDateTimeOffsetEditColumnStyleInfo1.ColumnName = "EQ_DateReceived";
			zDateTimeOffsetEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.Caption = null;
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("DocumentTrackingFilterControl|0204b563-86c2-4b0f-82c6-6d220d0565f5", "Snt. To Broker", "Sent To Broker");
			zDateEditColumnStyleInfo2.ColumnName = "EQ_SntToCustomsBroker";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo3.Caption = null;
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("DocumentTrackingFilterControl|6c0ef6a0-5d53-484c-a30f-91de645e54e6", "Rcvd. From Broker", "Received From Broker");
			zDateEditColumnStyleInfo3.ColumnName = "EQ_RcvFromCustomsBroker";
			zDateEditColumnStyleInfo3.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo4.Caption = null;
			zDateEditColumnStyleInfo4.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("DocumentTrackingFilterControl|49eb85c4-e4b9-4580-a59a-de695d8a5d9f", "Shipper Return Date");
			zDateEditColumnStyleInfo4.ColumnName = "EQ_ReturnToShipper";
			zDateEditColumnStyleInfo4.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zTextBoxColumnStyleInfo5.Caption = null;
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("DocumentTrackingFilterControl|0fcd950b-3a9e-41d4-86c1-b5b57074d090", "Shipment ID");
			zTextBoxColumnStyleInfo5.ColumnName = "EQ_Calc_ParentUniqueConsignRef";
			zTextBoxColumnStyleInfo6.Caption = null;
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("DocumentTrackingFilterControl|7fcfda1a-a72a-4d36-9f15-43d39da8fe99", "Master Bill");
			zTextBoxColumnStyleInfo6.ColumnName = "EQ_Calc_ParentMasterBill";
			zTextBoxColumnStyleInfo7.Caption = null;
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("DocumentTrackingFilterControl|bf7890e9-9631-46b3-acd4-7412bed53144", "House Bill");
			zTextBoxColumnStyleInfo7.ColumnName = "EQ_Calc_ParentHouseBill";
			zTextBoxColumnStyleInfo8.Caption = null;
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Freight.Forwarding.Module.Res.GetData("DocumentTrackingFilterControl|994119a6-daf7-4b0c-962d-3acd21532e53", "Export Broker");
			zTextBoxColumnStyleInfo8.ColumnName = "EQ_Calc_ParentExportBrokerCode";
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zDateTimeOffsetEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 256, true);
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(744, 248, true);
			this.grid.TabIndex = 26;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(JobRequiredDocument);
			// 
			// DocumentTrackingFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "DocumentTrackingFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(744, 504, true);
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#region Dispose

		System.ComponentModel.Container components = null;
		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}

			base.Dispose(disposing);
		}

		#endregion
	}
}
