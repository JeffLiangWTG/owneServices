namespace Enterprise.eTail.GUI
{
	partial class HVLVItemLinesUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo2 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			HVLVTariffColumnStyleInfo tariffColumnStyleInfo1 = new HVLVTariffColumnStyleInfo(true);
			HVLVTariffColumnStyleInfo tariffColumnStyleInfo2 = new HVLVTariffColumnStyleInfo(false);
			this.itemLinesGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.itemLinesGrid)).BeginInit();
			this.itemLinesGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.eTail.Business.HVLVItemLine);
			// 
			// itemLinesGrid
			// 
			this.itemLinesGrid.AllowNavigation = false;
			this.itemLinesGrid.CaptionVisible = false;
			this.BindingSource.SetBindingMember(this.itemLinesGrid, ".");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "HVS_RN_NKOriginCountryCode";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo1.BindToList = "Lookups.ClassificationList";
			zGuidFindBoxColumnStyleInfo1.ColumnName = "HVS_CC_Lookup";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "ShipmentOriginCountryCode";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo2.BindToList = "Lookups.Products";
			zCodeFindBoxColumnStyleInfo2.ColumnName = "HVS_ProductCode";
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "ShipmentDestinationCountryCode";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			tariffColumnStyleInfo1.ColumnName = "HVS_FormattedOriginTariff";
			tariffColumnStyleInfo1.PartialDescriptionMinLengthForSearch = 0;
			tariffColumnStyleInfo1.TariffType = "HSN";
			tariffColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			tariffColumnStyleInfo2.ColumnName = "HVS_FormattedDestinationTariff";
			tariffColumnStyleInfo2.PartialDescriptionMinLengthForSearch = 0;
			tariffColumnStyleInfo2.TariffType = "HSN";
			tariffColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.ColumnName = "HVS_ItemURL";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zMultiLineTextBoxColumnInfo1.ColumnName = "HVS_GoodsDescription";
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zMultiLineTextBoxColumnInfo2.ColumnName = "HVS_OriginGoodsDescription";
			zMultiLineTextBoxColumnInfo2.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "HVS_Quantity";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "HVS_NetWeight";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.ColumnName = "HVS_GrossWeight";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "HVS_WeightUnit";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "HVS_CustomsValue";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.ColumnName = "HVS_IntrinsicValue";
			zCalcEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.itemLinesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.itemLinesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.itemLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.itemLinesGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.itemLinesGrid.ColumnStyles.Add(tariffColumnStyleInfo1);
			this.itemLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.itemLinesGrid.ColumnStyles.Add(tariffColumnStyleInfo2);
			this.itemLinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.itemLinesGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.itemLinesGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo2);
			this.itemLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.itemLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.itemLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.itemLinesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.itemLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.itemLinesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.itemLinesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.itemLinesGrid.GridId = "48f50c5c-0ccd-45d2-8ae7-b95019c5b155";
			this.itemLinesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.itemLinesGrid.LayoutKey = "HVLVitemLinesGrid";
			this.itemLinesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.itemLinesGrid.Name = "itemLinesGrid";
			this.itemLinesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1042, 250, true);
			this.itemLinesGrid.TabIndex = 1;
			// 
			// HVLVItemLinesUserControl
			//
			this.CaptionRenderingEnabled = true;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.itemLinesGrid);
			this.Name = "HVLVItemLinesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1048, 269, true);
			((System.ComponentModel.ISupportInitialize)(this.itemLinesGrid)).EndInit();
			this.itemLinesGrid.ResumeLayout(false);
			this.itemLinesGrid.PerformLayout();
			this.ResumeLayout(false);

		}

		#endregion

		private ZArchitecture.ZGrid itemLinesGrid;
	}
}
