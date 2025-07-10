namespace Enterprise.Warehouse.Transactions.GUI
{
	public partial class AdjustmentDocketLinesGridUserControl
	{
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo zDateTimeOffsetEditColumnStyleInfo1 = new ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AdjustmentDocketLinesGridUserControl));
			ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.GUI.ZDropEditColumnStyleInfo inventoryStatusColumnStyleInfo = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZArchitecture.GUI.ZDropEditColumnStyleInfo inventoryHeldCodeColumnStyleInfo = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.LinesGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// LinesGrid
			// 
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = "SupplierPart+OP_CountDecimalPlaces";
			zCalcEditColumnStyleInfo1.ColumnName = "CommittedQuantity";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateTimeOffsetEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDateTimeOffsetEditColumnStyleInfo1.ColumnName = "WE_AdjustmentArrivalDate";
			zDateTimeOffsetEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateTimeOffsetEditColumnStyleInfo1.ToolTip = resources.GetString("zDateTimeOffsetEditColumnStyleInfo1.ToolTip");
			zCodeFindBoxColumnStyleInfo1.AutoCompleteDisabled = true;
			zCodeFindBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "LocationString";
			zCodeFindBoxColumnStyleInfo1.IsVisible = true;
			zCodeFindBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "WE_LineComment";
			zTextBoxColumnStyleInfo2.ToolTip = "Enter a comment specific to this line.";
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "WE_PalletID";
			zDropEditColumnStyleInfo1.Caption = null;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("c497d997-8b93-4fca-b9ed-433759d92eb1", "Adjustment Reason");
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "WE_ReasonCode";
			zDropEditColumnStyleInfo1.GroupName = Enterprise.Warehouse.Transactions.GUI.Res.GetData("AdjustmentDocketLinesGridUserControl|ac0292b7-a580-4944-b6de-f0e1b0da2ef4", "Adjustment Reason");
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("baa8fbba-2cfc-4824-abda-c5aca11a4081", "Desc.", "Reason Desc.", "Reason Description", "");
			zTextBoxColumnStyleInfo4.ColumnName = "WE_ReasonDescription";
			zTextBoxColumnStyleInfo4.GroupName = Enterprise.Warehouse.Transactions.GUI.Res.GetData("AdjustmentDocketLinesGridUserControl|ac0292b7-a580-4944-b6de-f0e1b0da2ef4", "Adjustment Reason");
			inventoryStatusColumnStyleInfo.ColumnName = "WE_OriginalInventoryStatus";
			inventoryStatusColumnStyleInfo.IsMandatory = true;
			inventoryStatusColumnStyleInfo.ToolTip = "Enter or select the inventory status.";
			inventoryStatusColumnStyleInfo.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("d31dba12-e4cf-4f4f-bf4a-ed5367f4f7d1", "Inventory Status");
			inventoryHeldCodeColumnStyleInfo.ColumnName = "WE_WHC_NKOriginalInventoryHeldCode";
			inventoryHeldCodeColumnStyleInfo.IsMandatory = true;
			inventoryHeldCodeColumnStyleInfo.ToolTip = "Enter or select the Inventory Hold Code.";

			this.LinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.LinesGrid.ColumnStyles.Add(zDateTimeOffsetEditColumnStyleInfo1);
			this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.LinesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.LinesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.LinesGrid.ColumnStyles.Add(inventoryStatusColumnStyleInfo);
			this.LinesGrid.ColumnStyles.Add(inventoryHeldCodeColumnStyleInfo);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.WhsAdjustment);
			// 
			// AdjustmentDocketLinesGridUserControl
			// 
			this.Name = "AdjustmentDocketLinesGridUserControl";
			((System.ComponentModel.ISupportInitialize)(this.LinesGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
