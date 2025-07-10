using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.Module
{
	public partial class AdjustmentFilterControl
	{
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new ZGuidFindBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo zDateTimeOffsetEditColumnStyleInfo1 = new ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo4 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo5 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			zGuidFindBoxColumnStyleInfo1.Caption = null;
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Transactions.Module.Res.GetData("AdjustmentFilterControl|9c2488fb-72ad-4db4-aa3f-1c8d398148ab", "Client");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "WD_OH_Client";
			zGuidFindBoxColumnStyleInfo2.Caption = null;
			zGuidFindBoxColumnStyleInfo2.CaptionResourceString = null;
			zGuidFindBoxColumnStyleInfo2.ColumnName = "WD_WW_Whs";
			zTextBoxColumnStyleInfo1.Caption = null;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Transactions.Module.Res.GetData("AdjustmentFilterControl|c54653c0-976e-45ae-afaf-cbb06b06b70a", "Reference");
			zTextBoxColumnStyleInfo1.ColumnName = "WD_ExternalReference";
			zTextBoxColumnStyleInfo2.Caption = null;
			zTextBoxColumnStyleInfo2.CaptionResourceString = null;
			zTextBoxColumnStyleInfo2.ColumnName = "WD_DocketID";
			zDateEditColumnStyleInfo1.Caption = null;
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Transactions.Module.Res.GetData("AdjustmentFilterControl|e251b0fc-8039-47f2-a7f1-e610cfc0458f", "Date");
			zDateEditColumnStyleInfo1.ColumnName = "WD_BookingDate";
			zTextBoxColumnStyleInfo3.Caption = null;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Warehouse.Transactions.Module.Res.GetData("AdjustmentFilterControl|d6ea8730-3fe5-4a44-b200-919bb6495a00", "Status");
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "WD_DocketStatusDescription";
			zTextBoxColumnStyleInfo10.Caption = null;
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Warehouse.Transactions.Module.Res.GetData("AdjustmentFilterControl|655B6F9D-C914-4DAF-92C7-F775B9E5F53C", "Type");
			zTextBoxColumnStyleInfo10.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo10.ColumnName = "SubTypeDesc";
			zDateTimeOffsetEditColumnStyleInfo1.Caption = null;
			zDateTimeOffsetEditColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Transactions.Module.Res.GetData("AdjustmentFilterControl|cbcabaf8-6ca4-4255-bb69-7a91735a27ea", "Finalized Date");
			zDateTimeOffsetEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDateTimeOffsetEditColumnStyleInfo1.ColumnName = "WD_FinalisedDate";
			zTextBoxColumnStyleInfo4.Caption = null;
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Warehouse.Transactions.Module.Res.GetData("AdjustmentFilterControl|ca8e0acc-2e16-4177-9445-da7c1c352f7f", "Custom Attrib. 1");
			zTextBoxColumnStyleInfo4.ColumnName = "WD_CustomAttrib1";
			zTextBoxColumnStyleInfo4.IsVisible = false;
			zTextBoxColumnStyleInfo5.Caption = null;
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Warehouse.Transactions.Module.Res.GetData("AdjustmentFilterControl|93572e41-0a55-4beb-b702-91d37ddb1d50", "Custom Attrib. 2");
			zTextBoxColumnStyleInfo5.ColumnName = "WD_CustomAttrib2";
			zTextBoxColumnStyleInfo5.IsVisible = false;
			zTextBoxColumnStyleInfo6.Caption = null;
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Warehouse.Transactions.Module.Res.GetData("AdjustmentFilterControl|c1f7cf45-f6b9-46b9-98e0-ad164921a50a", "Custom Attrib. 3");
			zTextBoxColumnStyleInfo6.ColumnName = "WD_CustomAttrib3";
			zTextBoxColumnStyleInfo6.IsVisible = false;
			zTextBoxColumnStyleInfo7.Caption = null;
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Warehouse.Transactions.Module.Res.GetData("AdjustmentFilterControl|e6eb819a-f268-48be-ad7e-ab79767301a0", "Custom Attrib. 4");
			zTextBoxColumnStyleInfo7.ColumnName = "WD_CustomAttrib4";
			zTextBoxColumnStyleInfo7.IsVisible = false;
			zTextBoxColumnStyleInfo8.Caption = null;
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Warehouse.Transactions.Module.Res.GetData("AdjustmentFilterControl|c0e9046d-1e64-458d-a980-9b470bc3c361", "Custom Attrib. 5");
			zTextBoxColumnStyleInfo8.ColumnName = "WD_CustomAttrib5";
			zTextBoxColumnStyleInfo8.IsVisible = false;
			zDateEditColumnStyleInfo3.Caption = null;
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.Warehouse.Transactions.Module.Res.GetData("AdjustmentFilterControl|2ac0c462-76d8-48a7-a2ed-640a2bb50620", "Custom Date 1");
			zDateEditColumnStyleInfo3.ColumnName = "WD_CustomDate1";
			zDateEditColumnStyleInfo3.IsVisible = false;
			zDateEditColumnStyleInfo4.Caption = null;
			zDateEditColumnStyleInfo4.CaptionResourceString = Enterprise.Warehouse.Transactions.Module.Res.GetData("AdjustmentFilterControl|e098f580-7c77-4156-a82f-10d2439eca85", "Custom Date 2");
			zDateEditColumnStyleInfo4.ColumnName = "WD_CustomDate2";
			zDateEditColumnStyleInfo4.IsVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Transactions.Module.Res.GetData("AdjustmentFilterControl|f7d813ba-ed3a-43af-b767-50fc9cf5db9c", "Custom Decimal 1");
			zCalcEditColumnStyleInfo1.ColumnName = "WD_CustomDecimal1";
			zCalcEditColumnStyleInfo1.IsVisible = false;
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.Caption = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Warehouse.Transactions.Module.Res.GetData("AdjustmentFilterControl|3b3b7416-3335-43d1-a215-64836f127e10", "Custom Decimal 2");
			zCalcEditColumnStyleInfo2.ColumnName = "WD_CustomDecimal2";
			zCalcEditColumnStyleInfo2.IsVisible = false;
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.Caption = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Warehouse.Transactions.Module.Res.GetData("AdjustmentFilterControl|b2d42e37-f11c-429b-9ee4-2a3b1ff17844", "Custom Decimal 3");
			zCalcEditColumnStyleInfo3.ColumnName = "WD_CustomDecimal3";
			zCalcEditColumnStyleInfo3.IsVisible = false;
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.Caption = null;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.Warehouse.Transactions.Module.Res.GetData("AdjustmentFilterControl|b63677c4-486a-4fab-9cc6-41540ed42e45", "Custom Decimal 4");
			zCalcEditColumnStyleInfo4.ColumnName = "WD_CustomDecimal4";
			zCalcEditColumnStyleInfo4.IsVisible = false;
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.Caption = null;
			zCalcEditColumnStyleInfo5.CaptionResourceString = Enterprise.Warehouse.Transactions.Module.Res.GetData("AdjustmentFilterControl|54baa9be-9c94-41b7-bc64-85c722717471", "Custom Decimal 5");
			zCalcEditColumnStyleInfo5.ColumnName = "WD_CustomDecimal5";
			zCalcEditColumnStyleInfo5.IsVisible = false;
			zCheckBoxColumnStyleInfo1.Caption = null;
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Transactions.Module.Res.GetData("AdjustmentFilterControl|f44b3b7e-0ca0-4e07-8131-8eabbfd09573", "Custom Flag 1");
			zCheckBoxColumnStyleInfo1.ColumnName = "WD_CustomFlag1";
			zCheckBoxColumnStyleInfo1.IsVisible = false;
			zCheckBoxColumnStyleInfo2.Caption = null;
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Warehouse.Transactions.Module.Res.GetData("AdjustmentFilterControl|166e1e6c-aa17-46b4-a538-b3a5b361315e", "Custom Flag 2");
			zCheckBoxColumnStyleInfo2.ColumnName = "WD_CustomFlag2";
			zCheckBoxColumnStyleInfo2.IsVisible = false;
			zCheckBoxColumnStyleInfo3.Caption = null;
			zCheckBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Warehouse.Transactions.Module.Res.GetData("AdjustmentFilterControl|0673a01c-0ad2-4f43-80a4-abdad9c76e14", "Custom Flag 3");
			zCheckBoxColumnStyleInfo3.ColumnName = "WD_CustomFlag3";
			zCheckBoxColumnStyleInfo3.IsVisible = false;
			zCheckBoxColumnStyleInfo4.Caption = null;
			zCheckBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Warehouse.Transactions.Module.Res.GetData("AdjustmentFilterControl|156b578c-f9eb-4580-b7d2-ab9fe0cabd97", "Custom Flag 4");
			zCheckBoxColumnStyleInfo4.ColumnName = "WD_CustomFlag4";
			zCheckBoxColumnStyleInfo4.IsVisible = false;
			zCheckBoxColumnStyleInfo5.Caption = null;
			zCheckBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Warehouse.Transactions.Module.Res.GetData("AdjustmentFilterControl|c3ddbc70-8d23-439e-a496-e4f18b411288", "Custom Flag 5");
			zCheckBoxColumnStyleInfo5.ColumnName = "WD_CustomFlag5";
			zCheckBoxColumnStyleInfo5.IsVisible = false;
			zTextBoxColumnStyleInfo9.ColumnName = "ClientName";
			zTextBoxColumnStyleInfo9.IsVisible = false;
			this.grid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zDateTimeOffsetEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo5);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 173, true);
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(732, 371, true);
			this.grid.TabIndex = 99;
			// 
			// ToolStripRecordsFoundLabel
			// 
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ToolStripRecordsFoundLabel, false);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.WhsAdjustment);
			// 
			// AdjustmentFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "AdjustmentFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(732, 544, true);
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
