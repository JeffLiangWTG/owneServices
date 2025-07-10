using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public partial class TransferDocketLinesGridUserControl
	{
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo zDateTimeOffsetEditColumnStyleInfo1 = new ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new ZGuidFindBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new ZCodeFindBoxColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo3 = new ZCodeFindBoxColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo4 = new ZCodeFindBoxColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new ZCodeFindBoxColumnStyleInfo();
			ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo zDateTimeOffsetEditColumnStyleInfo2 = new ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new ZDropEditColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.LinesGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// LinesGrid
			// 
			zDateTimeOffsetEditColumnStyleInfo1.Caption = null;
			zDateTimeOffsetEditColumnStyleInfo1.CaptionResourceString = null;
			zDateTimeOffsetEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDateTimeOffsetEditColumnStyleInfo1.ColumnName = "ArrivalDateForBinding";
			zDateTimeOffsetEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo1.Caption = null;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("TransferDocketLinesGridUserControl|FE2D7DA3-5A95-458D-9308-636D7D1E7BF1", "Pallet ID (Source)");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "WE_TransferFromPalletId";
			zCodeFindBoxColumnStyleInfo3.AutoCompleteDisabled = true;
			zCodeFindBoxColumnStyleInfo3.Caption = null;
			zCodeFindBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("TransferDocketLinesGridUserControl|66fef21f-9292-4569-ba7f-d619e15d7a59", "Location (Source)");
			zCodeFindBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCodeFindBoxColumnStyleInfo3.ColumnName = "TransferFromLocationString";
			zCodeFindBoxColumnStyleInfo1.Caption = null;
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("TransferDocketLinesGridUserControl|7d5850be-460a-4f21-8c38-12875d916ed5", "Picked By");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "GS_NKPickedBy";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateTimeOffsetEditColumnStyleInfo2.Caption = null;
			zDateTimeOffsetEditColumnStyleInfo2.ColumnName = "PickedTime";
			zDateTimeOffsetEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zGuidFindBoxColumnStyleInfo1.Caption = null;
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("TransferDocketLinesGridUserControl|0E0A5A66-676D-406B-81D5-3CCE9A48D61B", "Whs. (Source)");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "TransferFromWarehousePK";
			zGuidFindBoxColumnStyleInfo1.ToolTip = "Source warehouse for an inter warehouse destination transfer";
			zGuidFindBoxColumnStyleInfo2.Caption = null;
			zGuidFindBoxColumnStyleInfo2.ColumnName = "DestinationWarehousePK";
			zGuidFindBoxColumnStyleInfo2.ToolTip = "Destination warehouse for an inter warehouse source transfer";
			zTextBoxColumnStyleInfo3.Caption = null;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("TransferDocketLinesGridUserControl|99CC2B66-6E7C-4279-8F0B-49D3EC01CEF2", "Pallet ID (Dest.)");
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "WE_PalletID";
			zCodeFindBoxColumnStyleInfo4.AutoCompleteDisabled = true;
			zCodeFindBoxColumnStyleInfo4.Caption = null;
			zCodeFindBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("TransferDocketLinesGridUserControl|45875fa0-b62a-46e2-aed5-0f8fa0ed73cd", "Location (Dest.)");
			zCodeFindBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCodeFindBoxColumnStyleInfo4.ColumnName = "LocationString";
			zCodeFindBoxColumnStyleInfo2.Caption = null;
			zCodeFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("TransferDocketLinesGridUserControl|5efe7fc4-f29a-4a42-a3cf-5821fd519f70", "Putaway By");
			zCodeFindBoxColumnStyleInfo2.ColumnName = "WE_GS_NKPutawayBy";
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo3.Caption = null;
			zDateEditColumnStyleInfo3.ColumnName = "LocalisedPutawayTime";
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo5.Caption = null;
			zTextBoxColumnStyleInfo5.CaptionResourceString = null;
			zTextBoxColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo5.ColumnName = "WE_LineComment";
			zTextBoxColumnStyleInfo5.ToolTip = "Enter a comment specific to this line.";
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = "SupplierPart+OP_CountDecimalPlaces";
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("TransferDocketLinesGridUserControl|41a10b39-d943-4546-a61d-f3cbbec42bf4", "Committed Qty");
			zCalcEditColumnStyleInfo1.ColumnName = "QtyCommittedIncludingMatchingLines";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo6.Caption = null;
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("TransferDocketLinesGridUserControl|78de0325-9337-4596-b296-c60f9cccb8de", "Status");
			zTextBoxColumnStyleInfo6.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo6.ColumnName = "WE_DocketLineStatus";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zDropEditColumnStyleInfo1.Caption = null;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("TransferDocketLinesGridUserControl|316709aa-3f59-48ee-848e-0b391f3f26e6", "Inventory Status");
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "WE_OriginalInventoryStatus";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zDropEditColumnStyleInfo2.Caption = null;
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "WE_WHC_NKOriginalInventoryHeldCode";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			this.LinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.LinesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.LinesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.LinesGrid.ColumnStyles.Add(zDateTimeOffsetEditColumnStyleInfo1);
			this.LinesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.LinesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo3);
			this.LinesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.LinesGrid.ColumnStyles.Add(zDateTimeOffsetEditColumnStyleInfo2);
			this.LinesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.LinesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo4);
			this.LinesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.LinesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(WhsTransfer);
			// 
			// TransferDocketLinesGridUserControl
			// 
			this.Name = "TransferDocketLinesGridUserControl";
			((System.ComponentModel.ISupportInitialize)(this.LinesGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
