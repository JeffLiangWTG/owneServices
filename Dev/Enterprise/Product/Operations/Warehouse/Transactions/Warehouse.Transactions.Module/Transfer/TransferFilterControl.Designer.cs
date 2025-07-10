using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.Module
{
	public partial class TransferFilterControl
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
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo4 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo5 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			zGuidFindBoxColumnStyleInfo1.Caption = null;
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Transactions.Module.Res.GetData("TransferFilterControl|4aa0b070-f62b-465f-b679-bd39763e18bd", "Client");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "WD_OH_Client";
			zGuidFindBoxColumnStyleInfo2.Caption = null;
			zGuidFindBoxColumnStyleInfo2.CaptionResourceString = null;
			zGuidFindBoxColumnStyleInfo2.ColumnName = "WD_WW_Whs";
			zTextBoxColumnStyleInfo1.Caption = null;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Transactions.Module.Res.GetData("TransferFilterControl|9387738a-df21-4b56-968b-1ad5c9e4557d", "Reference");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zTextBoxColumnStyleInfo1.ColumnName = "WD_ExternalReference";
			zTextBoxColumnStyleInfo2.Caption = null;
			zTextBoxColumnStyleInfo2.CaptionResourceString = null;
			zTextBoxColumnStyleInfo2.ColumnName = "WD_DocketID";
			zDateEditColumnStyleInfo1.Caption = null;
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Transactions.Module.Res.GetData("TransferFilterControl|d81091c1-bb88-4426-a099-6a52ce403d95", "Date");
			zDateEditColumnStyleInfo1.ColumnName = "WD_BookingDate";
			zTextBoxColumnStyleInfo3.Caption = null;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Warehouse.Transactions.Module.Res.GetData("TransferFilterControl|b3998196-2dc5-4e82-a421-ca2f00c24fb8", "Status");
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "WD_DocketStatusDescription";
			zDateTimeOffsetEditColumnStyleInfo1.Caption = null;
			zDateTimeOffsetEditColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Transactions.Module.Res.GetData("TransferFilterControl|421e0545-fd13-46b4-953a-9e07f2ac0727", "Finalized Date");
			zDateTimeOffsetEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDateTimeOffsetEditColumnStyleInfo1.ColumnName = "WD_FinalisedDate";
			zTextBoxColumnStyleInfo4.Caption = null;
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Warehouse.Transactions.Module.Res.GetData("TransferFilterControl|16503a3e-dbe9-4762-a129-2a2005bf77cb", "Type");
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo4.ColumnName = "SubTypeDesc";
			zTextBoxColumnStyleInfo4.ToolTip = "Transfer SubType";
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = null;
			zCalcEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zCalcEditColumnStyleInfo1.ColumnName = "WD_ExternalReferenceSplit";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zTextBoxColumnStyleInfo5.Caption = null;
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Warehouse.Transactions.Module.Res.GetData("TransferFilterControl|dec09611-f85e-40a5-ad72-976a453d37e4", "Custom Attrib. 1");
			zTextBoxColumnStyleInfo5.ColumnName = "WD_CustomAttrib1";
			zTextBoxColumnStyleInfo5.IsVisible = false;
			zTextBoxColumnStyleInfo6.Caption = null;
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Warehouse.Transactions.Module.Res.GetData("TransferFilterControl|59cd88a5-db48-438b-9fcc-8348ffd63b23", "Custom Attrib. 2");
			zTextBoxColumnStyleInfo6.ColumnName = "WD_CustomAttrib2";
			zTextBoxColumnStyleInfo6.IsVisible = false;
			zTextBoxColumnStyleInfo7.Caption = null;
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Warehouse.Transactions.Module.Res.GetData("TransferFilterControl|da779b41-b79c-4e07-8dcf-f068b1762c60", "Custom Attrib. 3");
			zTextBoxColumnStyleInfo7.ColumnName = "WD_CustomAttrib3";
			zTextBoxColumnStyleInfo7.IsVisible = false;
			zTextBoxColumnStyleInfo8.Caption = null;
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Warehouse.Transactions.Module.Res.GetData("TransferFilterControl|c1b7dd85-525d-49c5-a39e-384ce68c1601", "Custom Attrib. 4");
			zTextBoxColumnStyleInfo8.ColumnName = "WD_CustomAttrib4";
			zTextBoxColumnStyleInfo8.IsVisible = false;
			zTextBoxColumnStyleInfo9.Caption = null;
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Warehouse.Transactions.Module.Res.GetData("TransferFilterControl|cb5bcc24-71ef-4b93-96b8-0f50beab43b8", "Custom Attrib. 5");
			zTextBoxColumnStyleInfo9.ColumnName = "WD_CustomAttrib5";
			zTextBoxColumnStyleInfo9.IsVisible = false;
			zDateEditColumnStyleInfo3.Caption = null;
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.Warehouse.Transactions.Module.Res.GetData("TransferFilterControl|a70c7c4c-dbc2-469c-ad8e-af70addcfc82", "Custom Date 1");
			zDateEditColumnStyleInfo3.ColumnName = "WD_CustomDate1";
			zDateEditColumnStyleInfo3.IsVisible = false;
			zDateEditColumnStyleInfo4.Caption = null;
			zDateEditColumnStyleInfo4.CaptionResourceString = Enterprise.Warehouse.Transactions.Module.Res.GetData("TransferFilterControl|56e8491c-2234-432e-847a-fc9f8c6c6833", "Custom Date 2");
			zDateEditColumnStyleInfo4.ColumnName = "WD_CustomDate2";
			zDateEditColumnStyleInfo4.IsVisible = false;
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.Caption = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Warehouse.Transactions.Module.Res.GetData("TransferFilterControl|3a623284-7051-4699-9f6c-f80e88a5525e", "Custom Decimal 1");
			zCalcEditColumnStyleInfo2.ColumnName = "WD_CustomDecimal1";
			zCalcEditColumnStyleInfo2.IsVisible = false;
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.Caption = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Warehouse.Transactions.Module.Res.GetData("TransferFilterControl|73e468d3-c117-41dd-9908-9c1274864180", "Custom Decimal 2");
			zCalcEditColumnStyleInfo3.ColumnName = "WD_CustomDecimal2";
			zCalcEditColumnStyleInfo3.IsVisible = false;
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.Caption = null;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.Warehouse.Transactions.Module.Res.GetData("TransferFilterControl|9e45be5f-1001-40ba-a998-4f3d851ab2eb", "Custom Decimal 3");
			zCalcEditColumnStyleInfo4.ColumnName = "WD_CustomDecimal3";
			zCalcEditColumnStyleInfo4.IsVisible = false;
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.Caption = null;
			zCalcEditColumnStyleInfo5.CaptionResourceString = Enterprise.Warehouse.Transactions.Module.Res.GetData("TransferFilterControl|9233128a-bd96-4893-aafe-af496cc769de", "Custom Decimal 4");
			zCalcEditColumnStyleInfo5.ColumnName = "WD_CustomDecimal4";
			zCalcEditColumnStyleInfo5.IsVisible = false;
			zCheckBoxColumnStyleInfo1.Caption = null;
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Transactions.Module.Res.GetData("TransferFilterControl|152912f7-b444-4aad-b79e-58c5b2efc5ee", "Custom Flag 1");
			zCheckBoxColumnStyleInfo1.ColumnName = "WD_CustomFlag1";
			zCheckBoxColumnStyleInfo1.IsVisible = false;
			zCheckBoxColumnStyleInfo2.Caption = null;
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Warehouse.Transactions.Module.Res.GetData("TransferFilterControl|8093c51e-7835-45f4-beed-01b984f78a5f", "Custom Flag 2");
			zCheckBoxColumnStyleInfo2.ColumnName = "WD_CustomFlag2";
			zCheckBoxColumnStyleInfo2.IsVisible = false;
			zCheckBoxColumnStyleInfo3.Caption = null;
			zCheckBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Warehouse.Transactions.Module.Res.GetData("TransferFilterControl|c379a326-dd6d-411e-b7e1-39a1da00fb08", "Custom Flag 3");
			zCheckBoxColumnStyleInfo3.ColumnName = "WD_CustomFlag3";
			zCheckBoxColumnStyleInfo3.IsVisible = false;
			zCheckBoxColumnStyleInfo4.Caption = null;
			zCheckBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Warehouse.Transactions.Module.Res.GetData("TransferFilterControl|51a02bad-9aa7-4fe7-b0fc-88f52256dd33", "Custom Flag 4");
			zCheckBoxColumnStyleInfo4.ColumnName = "WD_CustomFlag4";
			zCheckBoxColumnStyleInfo4.IsVisible = false;
			zCheckBoxColumnStyleInfo5.Caption = null;
			zCheckBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Warehouse.Transactions.Module.Res.GetData("TransferFilterControl|8b460aef-438e-4c64-84b2-96a0bc4dea06", "Custom Flag 5");
			zCheckBoxColumnStyleInfo5.ColumnName = "WD_CustomFlag5";
			zCheckBoxColumnStyleInfo5.IsVisible = false;
			zTextBoxColumnStyleInfo10.ColumnName = "ClientName";
			zTextBoxColumnStyleInfo10.IsVisible = false;
			zTextBoxColumnStyleInfo11.ColumnName = "WD_TaskPlanningStatus";
			zTextBoxColumnStyleInfo11.IsVisible = false;
			this.grid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zDateTimeOffsetEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo5);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 173, true);
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(732, 259, true);
			this.grid.TabIndex = 99;
			// 
			// ToolStripRecordsFoundLabel
			// 
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ToolStripRecordsFoundLabel, false);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.WhsTransfer);
			// 
			// TransferFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "TransferFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(732, 432, true);
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
