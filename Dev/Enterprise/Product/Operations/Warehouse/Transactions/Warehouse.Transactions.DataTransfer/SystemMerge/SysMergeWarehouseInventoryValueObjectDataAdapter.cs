using System;
using System.Xml.Schema;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.DataTransfer
{
	public class SysMergeWarehouseInventoryValueObjectDataAdapter : ValueObjectDataAdapter<WhsDocket, SystemMergeWarehouseReceive>
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard Coded Reference.")]
		internal const string SystemMergeReference = "System Merge";

		#region ExportToValueObjectCore

		protected override void ExportToValueObjectCore(WhsDocket docket, SystemMergeWarehouseReceive xsdReceive, IValueObjectExportContext context)
		{
			if (docket != null && xsdReceive != null)
			{
				ExportReceiveDetails(docket, xsdReceive);
				ExportDocketReferences(docket, xsdReceive);
				ExportReceiveLines(docket, xsdReceive);
			}
		}

		#region ExportReceiveDetails

		void ExportReceiveDetails(WhsDocket docket, SystemMergeWarehouseReceive xsdReceive)
		{
			xsdReceive.PK = docket.PK.ToString();
			xsdReceive.ClientPK = docket.WD_OH_Client.ToString();
			xsdReceive.WarehousePK = docket.WD_WW_Whs.ToString();
			xsdReceive.StagingAreaPK = docket.WD_WL_CrossDock.ToString();
			xsdReceive.ExWhsJobGuid = docket.WD_ExWhsJobGuid.ToString();

			xsdReceive.ExternalReference = docket.WD_ExternalReference;
			xsdReceive.DocketID = docket.WD_DocketID;

			xsdReceive.DocketType = DocketType.Codes.Receive;
			if (docket.WD_DocketType == DocketType.Codes.Receive)
			{
				xsdReceive.DocketSubType = docket.WD_DocketSubType;
			}
			else if (docket.Warehouse.WW_IsVirtualWarehouse)
			{
				xsdReceive.DocketSubType = ReceiveType.Codes.Customs;
			}
			else
			{
				xsdReceive.DocketSubType = ReceiveType.Codes.Receipt;
			}
			xsdReceive.TotalWeightUnit = docket.WD_TotalWeightUnit;
			xsdReceive.TotalCubicUnit = docket.WD_TotalCubicUnit;
			xsdReceive.DocketStatus = docket.WD_DocketStatus;
			xsdReceive.PickOption = docket.WD_PickOption;
			xsdReceive.CustomerReference = docket.WD_CustomerReference;
			xsdReceive.TransportReference = docket.WD_TransportReference;
			xsdReceive.DropMode = docket.WD_DropMode;
			xsdReceive.CODPayMethod = docket.WD_CODPayMethod;
			xsdReceive.INCO = docket.WD_INCO;
			xsdReceive.F3_NKTotalPackType = docket.WD_F3_NKTotalPackType;
			xsdReceive.PL_NKCarrierServiceLevel = docket.WD_PL_NKCarrierServiceLevel;
			xsdReceive.RS_NKServiceLevel = docket.WD_RS_NKServiceLevel;
			xsdReceive.CustomAttrib1 = docket.WD_CustomAttrib1;
			xsdReceive.CustomAttrib2 = docket.WD_CustomAttrib2;
			xsdReceive.CustomAttrib3 = docket.WD_CustomAttrib3;
			xsdReceive.CustomAttrib4 = docket.WD_CustomAttrib4;
			xsdReceive.CustomAttrib5 = docket.WD_CustomAttrib5;
			xsdReceive.TotalUnits = docket.WD_TotalUnits;
			xsdReceive.UnitsSent = docket.WD_UnitsSent;
			xsdReceive.CubicSent = docket.WD_CubicSent;
			xsdReceive.LocalCartInsuranceCost = docket.WD_LocalCartInsuranceCost;
			xsdReceive.ShipperCODAmount = docket.WD_ShipperCODAmount;
			xsdReceive.WeightSent = docket.WD_WeightSent;
			xsdReceive.WeightSentUserEntered = docket.WD_WeightSentUserEntered;
			xsdReceive.CustomDecimal1 = docket.WD_CustomDecimal1;
			xsdReceive.CustomDecimal2 = docket.WD_CustomDecimal2;
			xsdReceive.CustomDecimal3 = docket.WD_CustomDecimal3;
			xsdReceive.CustomDecimal4 = docket.WD_CustomDecimal4;
			xsdReceive.CustomDecimal5 = docket.WD_CustomDecimal5;
			xsdReceive.PackagesSent = docket.WD_PackagesSent;
			xsdReceive.TotalPallets = docket.WD_TotalPallets;
			xsdReceive.PalletsSent = docket.WD_PalletsSent;
			xsdReceive.ExternalReferenceSplit = docket.WD_ExternalReferenceSplit;
			xsdReceive.BookingDateTimeOffset = docket.WD_BookingDate;
			xsdReceive.ArrivalDateTimeOffset = docket.WD_ArrivalDate;
			xsdReceive.FinalisedDateTimeOffset = docket.WD_FinalisedDate;
			xsdReceive.RequiredDateTimeOffset = docket.WD_RequiredDate;
			xsdReceive.ETADateTimeOffset = docket.WD_ETA;
			xsdReceive.ETDDateTimeOffset = docket.WD_ETD;
			xsdReceive.CustomDate1 = docket.WD_CustomDate1;
			xsdReceive.CustomDate2 = docket.WD_CustomDate2;

			if (docket.WD_WeightVolSetFromImport)
			{ xsdReceive.WeightVolSetFromImport = xsdReceive.WeightVolSetFromImportSpecified = docket.WD_WeightVolSetFromImport; }
			if (docket.WD_AddPalletWeightToOrder)
			{ xsdReceive.AddPalletWeightToOrder = xsdReceive.AddPalletWeightToOrderSpecified = docket.WD_AddPalletWeightToOrder; }
			if (docket.WD_AutoFinaliseBOMIntoInventory)
			{ xsdReceive.AutoFinaliseBOMIntoInventory = xsdReceive.AutoFinaliseBOMIntoInventorySpecified = docket.WD_AutoFinaliseBOMIntoInventory; }
			if (docket.WD_CustomFlag1)
			{ xsdReceive.CustomFlag1 = xsdReceive.CustomFlag1Specified = docket.WD_CustomFlag1; }
			if (docket.WD_CustomFlag2)
			{ xsdReceive.CustomFlag2 = xsdReceive.CustomFlag2Specified = docket.WD_CustomFlag2; }
			if (docket.WD_CustomFlag3)
			{ xsdReceive.CustomFlag3 = xsdReceive.CustomFlag3Specified = docket.WD_CustomFlag3; }
			if (docket.WD_CustomFlag4)
			{ xsdReceive.CustomFlag4 = xsdReceive.CustomFlag4Specified = docket.WD_CustomFlag4; }
			if (docket.WD_CustomFlag5)
			{ xsdReceive.CustomFlag5 = xsdReceive.CustomFlag5Specified = docket.WD_CustomFlag5; }
		}

		#endregion

		#region ExportDocketReferences

		void ExportDocketReferences(WhsDocket docket, SystemMergeWarehouseReceive xsdReceive)
		{
			foreach (WhsDocketReference reference in docket.References)
			{
				var xsdReference = xsdReceive.DocketReferences.AddNew();
				xsdReference.PK = reference.PK.ToString();
				xsdReference.RefType = reference.WX_RefType;
				xsdReference.Reference = reference.WX_Reference;
			}
		}

		#endregion

		#region ExportReceiveLines

		/// <summary>
		/// Docket is Receive, Adjustment or Transfer.
		/// </summary>
		void ExportReceiveLines(WhsDocket docket, SystemMergeWarehouseReceive xsdReceive)
		{
			var isBondedWarehouse = docket.Warehouse.WW_IsVirtualWarehouse;

			var allDocketLines = docket.Factory.Load<WhsDocketLine>(new ZQuery(WhsDocketLineSchema.WE_WD, docket.PK)); // will include status changed and matching docket lines.
			foreach (var docketLine in allDocketLines)
			{
				var availableQty = GetAvailableQty(docketLine, isBondedWarehouse);
				if (availableQty > 0)
				{
					var xsdReceiveLine = xsdReceive.ReceiveLines.AddNew();
					xsdReceiveLine.PK = docketLine.PK.ToString();
					xsdReceiveLine.ProductPK = docketLine.WE_OP.ToString();
					xsdReceiveLine.LocationPK = docketLine.WE_WL.ToString();
					xsdReceiveLine.PalletID = docketLine.WE_PalletID;
					xsdReceiveLine.PartAttrib1 = docketLine.WE_PartAttrib1;
					xsdReceiveLine.PartAttrib2 = docketLine.WE_PartAttrib2;
					xsdReceiveLine.PartAttrib3 = docketLine.WE_PartAttrib3;
					xsdReceiveLine.SerialNumber = docketLine.WE_SerialNumber;
					xsdReceiveLine.BondedEntryKey = docketLine.WE_BondedEntryKey;
					xsdReceiveLine.LineComment = docketLine.WE_LineComment;
					xsdReceiveLine.CustomAttrib1 = docketLine.WE_CustomAttrib1;
					xsdReceiveLine.CustomAttrib2 = docketLine.WE_CustomAttrib2;
					xsdReceiveLine.CustomAttrib3 = docketLine.WE_CustomAttrib3;
					xsdReceiveLine.DocketLineStatus = docketLine.WE_DocketLineStatus;
					xsdReceiveLine.RX_NKUnitPriceCurrency = docketLine.WE_RX_NKUnitPriceCurrency;
					xsdReceiveLine.F3_NKPackType = docketLine.WE_F3_NKPackType;
					xsdReceiveLine.ReceiveCrossDockOrderNo = docketLine.WE_ReceiveCrossDockOrderNo;
					xsdReceiveLine.CustomAttrib4 = docketLine.WE_CustomAttrib4;
					xsdReceiveLine.CustomAttrib5 = docketLine.WE_CustomAttrib5;
					xsdReceiveLine.CustomAttrib6 = docketLine.WE_CustomAttrib6;
					xsdReceiveLine.CustomTextBlob1 = docketLine.WE_CustomTextBlob1;

					xsdReceiveLine.ExpiryDate = docketLine.WE_ExpiryDate;
					xsdReceiveLine.PackingDate = docketLine.WE_PackingDate;
					xsdReceiveLine.AdjustmentArrivalDateTimeOffset = docketLine.WE_AdjustmentArrivalDate;
					xsdReceiveLine.FinalisedDateTimeOffset = docketLine.WE_FinalisedDate;
					xsdReceiveLine.RequiredByDateTimeOffset = docketLine.WE_RequiredByDate;
					xsdReceiveLine.CustomDate1 = docketLine.WE_CustomDate1;
					xsdReceiveLine.CustomDate2 = docketLine.WE_CustomDate2;
					xsdReceiveLine.CustomDate3 = docketLine.WE_CustomDate3;
					xsdReceiveLine.CustomDate4 = docketLine.WE_CustomDate4;
					xsdReceiveLine.CustomDate5 = docketLine.WE_CustomDate5;

					xsdReceiveLine.Units = availableQty;
					xsdReceiveLine.ClientOrderedUnits = docketLine.WE_ClientOrderedUnits;
					xsdReceiveLine.RecommendedUnitPrice = docketLine.WE_RecommendedUnitPrice;
					xsdReceiveLine.UnitDiscountPercent = docketLine.WE_UnitDiscountPercent;
					xsdReceiveLine.UnitDiscountAmount = docketLine.WE_UnitDiscountAmount;
					xsdReceiveLine.UnitPriceAfterDiscount = docketLine.WE_UnitPriceAfterDiscount;
					xsdReceiveLine.ExtendedLinePrice = docketLine.WE_ExtendedLinePrice;
					xsdReceiveLine.CustomDecimal1 = docketLine.WE_CustomDecimal1;
					xsdReceiveLine.CustomDecimal2 = docketLine.WE_CustomDecimal2;
					xsdReceiveLine.CustomDecimal3 = docketLine.WE_CustomDecimal3;
					xsdReceiveLine.CustomDecimal4 = docketLine.WE_CustomDecimal4;
					xsdReceiveLine.CustomDecimal5 = docketLine.WE_CustomDecimal5;

					xsdReceiveLine.LineNo = docketLine.WE_LineNo;
					xsdReceiveLine.SubLineNo = docketLine.WE_SubLineNo;

					if (docketLine.WE_CustomFlag1)
					{ xsdReceiveLine.CustomFlag1 = xsdReceiveLine.CustomFlag1Specified = true; }
					if (docketLine.WE_CustomFlag2)
					{ xsdReceiveLine.CustomFlag2 = xsdReceiveLine.CustomFlag2Specified = true; }
					if (docketLine.WE_CustomFlag3)
					{ xsdReceiveLine.CustomFlag3 = xsdReceiveLine.CustomFlag3Specified = true; }
					if (docketLine.WE_CustomFlag4)
					{ xsdReceiveLine.CustomFlag4 = xsdReceiveLine.CustomFlag4Specified = true; }
					if (docketLine.WE_CustomFlag5)
					{ xsdReceiveLine.CustomFlag5 = xsdReceiveLine.CustomFlag5Specified = true; }

					ExportBondedWarehouseAttribute(docketLine, xsdReceiveLine, availableQty);
					ExportInventory(docketLine, xsdReceiveLine, availableQty, isBondedWarehouse);
				}
			}
		}

		#region GetAvailableQty

		ZDecimal GetAvailableQty(WhsDocketLine docketLine, bool isBondedWarehouse)
		{
			ZDecimal result = 0m;

			foreach (WhsInventoryView inventory in docketLine.Inventory)
			{
				result += GetAvailableQty(inventory, isBondedWarehouse);
			}

			return result;
		}

		#endregion

		#region ExportBondedWarehouseAttribute

		void ExportBondedWarehouseAttribute(WhsDocketLine docketLine, SystemMergeReceiveLine xsdReceiveLine, ZDecimal availableQty)
		{
			var customsData = docketLine.CustomsData;
			if (customsData != null)
			{
				var xsdCustomsData = xsdReceiveLine.CustomsData;

				xsdCustomsData.PK = customsData.PK.ToString();
				xsdCustomsData.WB_InwardsEntry = customsData.WB_WB_InwardsEntry.ToString();
				xsdCustomsData.ParentID = customsData.WB_ParentID.ToString();

				xsdCustomsData.DeclarationReference = customsData.WB_DeclarationReference;
				xsdCustomsData.EntryKey = customsData.WB_EntryKey;
				xsdCustomsData.CustomsUnitOfQty = customsData.WB_CustomsUnitOfQty;
				xsdCustomsData.BondedWhsUnitOfQty = customsData.WB_BondedWhsUnitOfQty;
				xsdCustomsData.RN_NKCountryOfOrigin = customsData.WB_RN_NKCountryOfOrigin;
				xsdCustomsData.AddInfo = customsData.WB_AddInfo;
				xsdCustomsData.ParentTableCode = customsData.WB_ParentTableCode;
				xsdCustomsData.RX_NKTILVCurrency = customsData.WB_RX_NKTILVCurrency;

				xsdCustomsData.EntryLineNo = customsData.WB_EntryLineNo;

				xsdCustomsData.CustomsQty = GetCustomsValueForReducedStock(customsData.WB_CustomsQty, docketLine.WE_TransactionQuantity, availableQty);
				xsdCustomsData.BondedWhsQty = GetCustomsValueForReducedStock(customsData.WB_BondedWhsQty, docketLine.WE_TransactionQuantity, availableQty);
				xsdCustomsData.ValueForDuty = GetCustomsValueForReducedStock(customsData.WB_ValueForDuty, docketLine.WE_TransactionQuantity, availableQty);
				xsdCustomsData.TILV = GetCustomsValueForReducedStock(customsData.WB_TILV, docketLine.WE_TransactionQuantity, availableQty);

				xsdCustomsData.EntryDate = customsData.WB_EntryDate;

				if (customsData.WB_IsActive)
				{ xsdCustomsData.IsActive = xsdCustomsData.IsActiveSpecified = true; }

				xsdCustomsData.CustomsSecondQuantity = customsData.WB_CustomsSecondQuantity;
				xsdCustomsData.CustomsSecondUnitQty = customsData.WB_CustomsSecondUnitQty;
				xsdCustomsData.Tariff = customsData.WB_Tariff;
				xsdCustomsData.PrimaryPreference = customsData.WB_PrimaryPreference;

				xsdCustomsData.CustomsThirdQuantity = customsData.WB_CustomsThirdQuantity;
				xsdCustomsData.CustomsThirdUnitQty = customsData.WB_CustomsThirdUnitQty;
				xsdCustomsData.ManufacturerAddress = customsData.WB_OA_ManufacturerAddress.ToString();

				xsdCustomsData.ZoneStatus = customsData.WB_ZoneStatus;
				if (customsData.WB_IsFromAnotherFTZWhs)
				{ xsdCustomsData.IsFromOtherFTZWarehouse = xsdCustomsData.IsFromOtherFTZWarehouseSpecified = true; }
				xsdCustomsData.OutwardType = customsData.WB_OutwardType;
			}
		}

		ZDecimal GetCustomsValueForReducedStock(ZDecimal valueToSplit, ZDecimal originalStockQty, ZDecimal currentStockQty)
		{
			var result = valueToSplit;
			if (originalStockQty != currentStockQty && originalStockQty > 0)
			{
				result = valueToSplit * currentStockQty / originalStockQty;
			}
			return result;
		}

		#endregion

		#region ExportInventory

		void ExportInventory(WhsDocketLine docketLine, SystemMergeReceiveLine xsdReceiveLine, ZDecimal availableUnitsFromReceive, bool isBondedWarehouse)
		{
			foreach (WhsInventoryView inventory in docketLine.Inventory)
			{
				var availableUnits = GetAvailableQty(inventory, isBondedWarehouse);
				if (availableUnits > 0)
				{
					var xsdInventory = xsdReceiveLine.Inventories.AddNew();
					xsdInventory.PK = inventory.PK.ToString();

					xsdInventory.InventoryStatus = inventory.WI_InventoryStatus;
					xsdInventory.InDocketLineType = DocketType.Codes.Receive;
					xsdInventory.F3_NKPackType = inventory.WI_F3_NKPackType;
					xsdInventory.PalletID = inventory.WI_PalletID;

					xsdInventory.TotalUnits = availableUnits;
					xsdInventory.CommittedUnits = 0m;
					xsdInventory.InDocketLineUnits = availableUnitsFromReceive; // in case of split qty (eg. 1 line of 25 units with 2 inv -- 20 avail + 5 dmg'd).

					xsdInventory.ArrivalDateTimeOffset = inventory.WI_ArrivalDate;

					if (inventory.WI_IsOriginalReceiptLine)
					{ xsdInventory.IsOriginalReceiptLine = xsdInventory.IsOriginalReceiptLineSpecified = true; }
				}
			}
		}

		ZDecimal GetAvailableQty(WhsInventoryView inventory, bool isBondedWarehouse)
		{
			return isBondedWarehouse ? (ZDecimal)(inventory.InDocketLine.WE_TransactionQuantity - inventory.CommittedQuantityIncludingUnfinalisedReceipt) : inventory.WI_TotalUnits;
		}

		#endregion

		#endregion

		#endregion

		#region ImportFromValueObjectCore

		#region ImportFromValueObjectCore

		protected override void ImportFromValueObjectCore(WhsDocket docket, SystemMergeWarehouseReceive xsdReceive, IValueObjectImportContext context)
		{
			var errorContext = Res.GetString("c4185515-870c-4d69-97cd-9f59fae4d87d", "Receive [({0}) - {1}]", xsdReceive.PK, xsdReceive.ExternalReference);

			var receive = (WhsReceive)docket;
			ImportReceiveDetails(receive, xsdReceive, context, errorContext);
			ImportDocketReferences(receive, xsdReceive, context);
			ImportReceiveLines(receive, xsdReceive, context, errorContext);

			if (xsdReceive.DocketStatus.EqualsIgnoringCase(DocketStatus.Codes.Finalised))
			{
				context.SetPropertyInfoValueIfValueNotEmpty(receive.WD_DocketStatusInfo, xsdReceive.DocketStatus);
			}
		}

		#region ImportReceiveDetails

		void ImportReceiveDetails(WhsReceive receive, SystemMergeWarehouseReceive xsdReceive, IValueObjectImportContext context, string errorContext)
		{
			ValidateAndImportClient(receive, xsdReceive, context, errorContext);
			ValidateAndImportWarehouse(receive, xsdReceive, context, errorContext);

			receive.WD_WL_CrossDock = new ZGuid(xsdReceive.StagingAreaPK);
			receive.WD_ExWhsJobGuid = new ZGuid(xsdReceive.ExWhsJobGuid);

			var newExternalReference = SystemMergeReference + " " + xsdReceive.DocketID;
			context.SetPropertyInfoValueIfValueNotEmpty(receive.WD_ExternalReferenceInfo, newExternalReference);

			context.SetPropertyInfoValueIfValueNotEmpty(receive.WD_DocketTypeInfo, xsdReceive.DocketType);
			context.SetPropertyInfoValueIfValueNotEmpty(receive.WD_DocketSubTypeInfo, xsdReceive.DocketSubType);
			context.SetPropertyInfoValueIfValueNotEmpty(receive.WD_TotalWeightUnitInfo, xsdReceive.TotalWeightUnit);
			context.SetPropertyInfoValueIfValueNotEmpty(receive.WD_TotalCubicUnitInfo, xsdReceive.TotalCubicUnit);
			if (!xsdReceive.DocketStatus.EqualsIgnoringCase(DocketStatus.Codes.Finalised))
			{
				context.SetPropertyInfoValueIfValueNotEmpty(receive.WD_DocketStatusInfo, xsdReceive.DocketStatus);
			}
			context.SetPropertyInfoValueIfValueNotEmpty(receive.WD_PickOptionInfo, xsdReceive.PickOption);
			context.SetPropertyInfoValueIfValueNotEmpty(receive.WD_CustomerReferenceInfo, xsdReceive.CustomerReference);
			context.SetPropertyInfoValueIfValueNotEmpty(receive.WD_TransportReferenceInfo, xsdReceive.TransportReference);
			context.SetPropertyInfoValueIfValueNotEmpty(receive.WD_DropModeInfo, xsdReceive.DropMode);
			context.SetPropertyInfoValueIfValueNotEmpty(receive.WD_CODPayMethodInfo, xsdReceive.CODPayMethod);
			context.SetPropertyInfoValueIfValueNotEmpty(receive.WD_INCOInfo, xsdReceive.INCO);
			context.SetPropertyInfoValueIfValueNotEmpty(receive.WD_F3_NKTotalPackTypeInfo, xsdReceive.F3_NKTotalPackType);
			context.SetPropertyInfoValueIfValueNotEmpty(receive.WD_PL_NKCarrierServiceLevelInfo, xsdReceive.PL_NKCarrierServiceLevel);
			context.SetPropertyInfoValueIfValueNotEmpty(receive.WD_RS_NKServiceLevelInfo, xsdReceive.RS_NKServiceLevel);
			context.SetPropertyInfoValueIfValueNotEmpty(receive.WD_CustomAttrib1Info, xsdReceive.CustomAttrib1);
			context.SetPropertyInfoValueIfValueNotEmpty(receive.WD_CustomAttrib2Info, xsdReceive.CustomAttrib2);
			context.SetPropertyInfoValueIfValueNotEmpty(receive.WD_CustomAttrib3Info, xsdReceive.CustomAttrib3);
			context.SetPropertyInfoValueIfValueNotEmpty(receive.WD_CustomAttrib4Info, xsdReceive.CustomAttrib4);
			context.SetPropertyInfoValueIfValueNotEmpty(receive.WD_CustomAttrib5Info, xsdReceive.CustomAttrib5);
			context.SetPropertyInfoValueIfValueNotEmpty(receive.WD_TotalUnitsInfo, xsdReceive.TotalUnits.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(receive.WD_UnitsSentInfo, xsdReceive.UnitsSent.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(receive.WD_CubicSentInfo, xsdReceive.CubicSent.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(receive.WD_LocalCartInsuranceCostInfo, xsdReceive.LocalCartInsuranceCost.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(receive.WD_ShipperCODAmountInfo, xsdReceive.ShipperCODAmount.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(receive.WD_WeightSentInfo, xsdReceive.WeightSent.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(receive.WD_WeightSentUserEnteredInfo, xsdReceive.WeightSentUserEntered.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(receive.WD_CustomDecimal1Info, xsdReceive.CustomDecimal1.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(receive.WD_CustomDecimal2Info, xsdReceive.CustomDecimal2.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(receive.WD_CustomDecimal3Info, xsdReceive.CustomDecimal3.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(receive.WD_CustomDecimal4Info, xsdReceive.CustomDecimal4.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(receive.WD_CustomDecimal5Info, xsdReceive.CustomDecimal5.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(receive.WD_PackagesSentInfo, xsdReceive.PackagesSent.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(receive.WD_TotalPalletsInfo, xsdReceive.TotalPallets.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(receive.WD_PalletsSentInfo, xsdReceive.PalletsSent.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(receive.WD_ExternalReferenceSplitInfo, xsdReceive.ExternalReferenceSplit.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(receive.WD_WeightVolSetFromImportInfo, xsdReceive.WeightVolSetFromImport.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(receive.WD_AddPalletWeightToOrderInfo, xsdReceive.AddPalletWeightToOrder.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(receive.WD_AutoFinaliseBOMIntoInventoryInfo, xsdReceive.AutoFinaliseBOMIntoInventory.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(receive.WD_CustomFlag1Info, xsdReceive.CustomFlag1.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(receive.WD_CustomFlag2Info, xsdReceive.CustomFlag2.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(receive.WD_CustomFlag3Info, xsdReceive.CustomFlag3.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(receive.WD_CustomFlag4Info, xsdReceive.CustomFlag4.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(receive.WD_CustomFlag5Info, xsdReceive.CustomFlag5.ToString());
			context.SetPropertyInfoValueIfValueNotEmpty(receive.WD_BookingDateInfo, xsdReceive.BookingDateTimeOffset);
			context.SetPropertyInfoValueIfValueNotEmpty(receive.WD_ArrivalDateInfo, xsdReceive.ArrivalDateTimeOffset);
			context.SetPropertyInfoValueIfValueNotEmpty(receive.WD_FinalisedDateInfo, xsdReceive.FinalisedDateTimeOffset);
			context.SetPropertyInfoValueIfValueNotEmpty(receive.WD_RequiredDateInfo, xsdReceive.RequiredDateTimeOffset);
			context.SetPropertyInfoValueIfValueNotEmpty(receive.WD_ETAInfo, xsdReceive.ETADateTimeOffset);
			context.SetPropertyInfoValueIfValueNotEmpty(receive.WD_ETDInfo, xsdReceive.ETDDateTimeOffset);
			context.SetPropertyInfoValueIfValueNotEmpty(receive.WD_CustomDate1Info, xsdReceive.CustomDate1);
			context.SetPropertyInfoValueIfValueNotEmpty(receive.WD_CustomDate2Info, xsdReceive.CustomDate2);
		}

		#region ValidateAndImportClient

		void ValidateAndImportClient(WhsReceive receive, SystemMergeWarehouseReceive xsdReceive, IValueObjectImportContext context, string errorContext)
		{
			var clientPK = new ZGuid(xsdReceive.ClientPK);
			var client = context.Factory.Load<OrgHeader>(clientPK);
			if (client != null)
			{
				receive.WD_OH_Client = clientPK;
			}
			else
			{
				var errorMessage = Res.GetString("f91fabad-e044-45ce-b2ec-221845e725a0", "{0}\r\nCould not find Organization with PK = ({1}).\r\nPlease import it first and then retry the import operation.", errorContext, clientPK) + "\r\n";
				receive.Delete(); // delete Receive to not allow DataTransfer architecture to save it to DB.
				throw new InvalidOperationException(errorMessage);
			}
		}

		#endregion

		#region ValidateAndImportWarehouse

		void ValidateAndImportWarehouse(WhsReceive receive, SystemMergeWarehouseReceive xsdReceive, IValueObjectImportContext context, string errorContext)
		{
			var whsPK = new ZGuid(xsdReceive.WarehousePK);
			var whs = context.Factory.Load<WhsWarehouse>(whsPK);
			if (whs != null)
			{
				receive.WD_WW_Whs = whsPK;
			}
			else
			{
				var errorMessage = Res.GetString("45af243b-7d06-458b-8b7e-dd6aa62115fb", "{0}\r\nCould not find Warehouse with PK = ({1}).\r\nPlease import it first and then retry the import operation.", errorContext, whsPK) + "\r\n";
				receive.Delete(); // delete Receive to not allow DataTransfer architecture to save it to DB.
				throw new InvalidOperationException(errorMessage);
			}
		}

		#endregion

		#endregion

		#region ImportDocketReferences

		void ImportDocketReferences(WhsReceive receive, SystemMergeWarehouseReceive xsdReceive, IValueObjectImportContext context)
		{
			foreach (SystemMergeDocketReference xsdReference in xsdReceive.DocketReferences)
			{
				var reference = context.Factory.NewWithPrimaryKey<WhsDocketReference>(new Guid(xsdReference.PK));
				reference.WX_WD = receive.PK;

				context.SetPropertyInfoValueIfValueNotEmpty(reference.WX_RefTypeInfo, xsdReference.RefType);
				context.SetPropertyInfoValueIfValueNotEmpty(reference.WX_ReferenceInfo, xsdReference.Reference);
			}

			AddDocketReference(receive, WarehouseAdditionalReferenceTypes.Codes.PreMergeJobRefCode, xsdReceive.ExternalReference);
			AddDocketReference(receive, WarehouseAdditionalReferenceTypes.Codes.PreMergeJobNoCode, xsdReceive.DocketID);
		}

		void AddDocketReference(WhsReceive receive, ZString refType, ZString reference)
		{
			var docketReference = receive.References.AddNew();
			docketReference.WX_RefType = refType;

			if (reference.Length > docketReference.WX_ReferenceInfo.MaxLength)
			{
				docketReference.WX_Reference = reference.Substring(0, docketReference.WX_ReferenceInfo.MaxLength);
				receive.Notes.AddNew(true, Res.GetString("17817e35-a6e4-4cc1-99f5-7b3181ffd052", "System Merge Reference import"),
					Res.GetString("2d37436d-3759-4809-a2ca-b94d01642c2e", "During System Merge, part of an old external reference was cut. Original reference '{0}' was shortened to '{1}'.", reference, docketReference.WX_Reference));
			}
			else
			{
				docketReference.WX_Reference = reference;
			}
		}

		#endregion

		#region ImportReceiveLines

		void ImportReceiveLines(WhsReceive receive, SystemMergeWarehouseReceive xsdReceive, IValueObjectImportContext context, string errorContext)
		{
			foreach (SystemMergeReceiveLine xsdReceiveLine in xsdReceive.ReceiveLines)
			{
				var receiveLine = context.Factory.NewWithPrimaryKey<WhsReceiveLine>(new Guid(xsdReceiveLine.PK));
				var xsdInventory = xsdReceiveLine.Inventories[0];
				var inventory = context.Factory.NewWithPrimaryKey<WhsInventoryView>(new Guid(xsdInventory.PK));
				inventory.WI_InDocketLineType = xsdInventory.InDocketLineType;
				inventory.WI_WE_InDocketLine = receiveLine.PK;
				inventory.WI_WE_OriginalInDocketLineForRating = receiveLine.PK;
				inventory.WI_IsOriginalReceiptLine = receiveLine.WE_IsOriginalInventory;

				receiveLine.WE_WD = receive.PK;

				context.SetPropertyInfoValueIfValueNotEmpty(receiveLine.WE_DocketLineTypeInfo, DocketType.Codes.Receive);

				ValidateAndImportProduct(receive, receiveLine, xsdReceiveLine, context, errorContext);
				ValidateAndImportLocation(receive, receiveLine, xsdReceiveLine, context, errorContext);

				context.SetPropertyInfoValueIfValueNotEmpty(receiveLine.WE_PartAttrib1Info, xsdReceiveLine.PartAttrib1);
				context.SetPropertyInfoValueIfValueNotEmpty(receiveLine.WE_PartAttrib2Info, xsdReceiveLine.PartAttrib2);
				context.SetPropertyInfoValueIfValueNotEmpty(receiveLine.WE_PartAttrib3Info, xsdReceiveLine.PartAttrib3);
				context.SetPropertyInfoValueIfValueNotEmpty(receiveLine.WE_SerialNumberInfo, xsdReceiveLine.SerialNumber);
				context.SetPropertyInfoValueIfValueNotEmpty(receiveLine.WE_BondedEntryKeyInfo, xsdReceiveLine.BondedEntryKey);
				context.SetPropertyInfoValueIfValueNotEmpty(receiveLine.WE_LineCommentInfo, xsdReceiveLine.LineComment);
				context.SetPropertyInfoValueIfValueNotEmpty(receiveLine.WE_CustomAttrib1Info, xsdReceiveLine.CustomAttrib1);
				context.SetPropertyInfoValueIfValueNotEmpty(receiveLine.WE_CustomAttrib2Info, xsdReceiveLine.CustomAttrib2);
				context.SetPropertyInfoValueIfValueNotEmpty(receiveLine.WE_CustomAttrib3Info, xsdReceiveLine.CustomAttrib3);
				context.SetPropertyInfoValueIfValueNotEmpty(receiveLine.WE_RX_NKUnitPriceCurrencyInfo, xsdReceiveLine.RX_NKUnitPriceCurrency);
				context.SetPropertyInfoValueIfValueNotEmpty(receiveLine.WE_F3_NKPackTypeInfo, xsdReceiveLine.F3_NKPackType);
				context.SetPropertyInfoValueIfValueNotEmpty(receiveLine.WE_PalletIDInfo, xsdReceiveLine.PalletID);
				context.SetPropertyInfoValueIfValueNotEmpty(receiveLine.WE_ReceiveCrossDockOrderNoInfo, xsdReceiveLine.ReceiveCrossDockOrderNo);
				context.SetPropertyInfoValueIfValueNotEmpty(receiveLine.WE_CustomAttrib4Info, xsdReceiveLine.CustomAttrib4);
				context.SetPropertyInfoValueIfValueNotEmpty(receiveLine.WE_CustomAttrib5Info, xsdReceiveLine.CustomAttrib5);
				context.SetPropertyInfoValueIfValueNotEmpty(receiveLine.WE_CustomAttrib6Info, xsdReceiveLine.CustomAttrib6);
				context.SetPropertyInfoValueIfValueNotEmpty(receiveLine.WE_CustomTextBlob1Info, xsdReceiveLine.CustomTextBlob1);
				context.SetPropertyInfoValueIfValueNotEmpty(receiveLine.WE_ExpiryDateInfo, xsdReceiveLine.ExpiryDate);
				context.SetPropertyInfoValueIfValueNotEmpty(receiveLine.WE_PackingDateInfo, xsdReceiveLine.PackingDate);
				context.SetPropertyInfoValueIfValueNotEmpty(receiveLine.WE_AdjustmentArrivalDateInfo, xsdReceiveLine.AdjustmentArrivalDateTimeOffset);
				context.SetPropertyInfoValueIfValueNotEmpty(receiveLine.WE_RequiredByDateInfo, xsdReceiveLine.RequiredByDateTimeOffset);
				context.SetPropertyInfoValueIfValueNotEmpty(receiveLine.WE_CustomDate1Info, xsdReceiveLine.CustomDate1);
				context.SetPropertyInfoValueIfValueNotEmpty(receiveLine.WE_CustomDate2Info, xsdReceiveLine.CustomDate2);
				context.SetPropertyInfoValueIfValueNotEmpty(receiveLine.WE_CustomDate3Info, xsdReceiveLine.CustomDate3);
				context.SetPropertyInfoValueIfValueNotEmpty(receiveLine.WE_CustomDate4Info, xsdReceiveLine.CustomDate4);
				context.SetPropertyInfoValueIfValueNotEmpty(receiveLine.WE_CustomDate5Info, xsdReceiveLine.CustomDate5);
				context.SetPropertyInfoValueIfValueNotEmpty(receiveLine.WE_ClientOrderedUnitsInfo, xsdReceiveLine.ClientOrderedUnits.ToString());
				context.SetPropertyInfoValueIfValueNotEmpty(receiveLine.WE_TransactionQuantityInfo, xsdReceiveLine.Units.ToString());
				context.SetPropertyInfoValueIfValueNotEmpty(receiveLine.WE_RecommendedUnitPriceInfo, xsdReceiveLine.RecommendedUnitPrice.ToString());
				context.SetPropertyInfoValueIfValueNotEmpty(receiveLine.WE_UnitDiscountPercentInfo, xsdReceiveLine.UnitDiscountPercent.ToString());
				context.SetPropertyInfoValueIfValueNotEmpty(receiveLine.WE_UnitDiscountAmountInfo, xsdReceiveLine.UnitDiscountAmount.ToString());
				context.SetPropertyInfoValueIfValueNotEmpty(receiveLine.WE_UnitPriceAfterDiscountInfo, xsdReceiveLine.UnitPriceAfterDiscount.ToString());
				context.SetPropertyInfoValueIfValueNotEmpty(receiveLine.WE_ExtendedLinePriceInfo, xsdReceiveLine.ExtendedLinePrice.ToString());
				context.SetPropertyInfoValueIfValueNotEmpty(receiveLine.WE_CustomDecimal1Info, xsdReceiveLine.CustomDecimal1.ToString());
				context.SetPropertyInfoValueIfValueNotEmpty(receiveLine.WE_CustomDecimal2Info, xsdReceiveLine.CustomDecimal2.ToString());
				context.SetPropertyInfoValueIfValueNotEmpty(receiveLine.WE_CustomDecimal3Info, xsdReceiveLine.CustomDecimal3.ToString());
				context.SetPropertyInfoValueIfValueNotEmpty(receiveLine.WE_CustomDecimal4Info, xsdReceiveLine.CustomDecimal4.ToString());
				context.SetPropertyInfoValueIfValueNotEmpty(receiveLine.WE_CustomDecimal5Info, xsdReceiveLine.CustomDecimal5.ToString());
				context.SetPropertyInfoValueIfValueNotEmpty(receiveLine.WE_LineNoInfo, xsdReceiveLine.LineNo.ToString());
				context.SetPropertyInfoValueIfValueNotEmpty(receiveLine.WE_SubLineNoInfo, xsdReceiveLine.SubLineNo.ToString());
				context.SetPropertyInfoValueIfValueNotEmpty(receiveLine.WE_CustomFlag1Info, xsdReceiveLine.CustomFlag1.ToString());
				context.SetPropertyInfoValueIfValueNotEmpty(receiveLine.WE_CustomFlag2Info, xsdReceiveLine.CustomFlag2.ToString());
				context.SetPropertyInfoValueIfValueNotEmpty(receiveLine.WE_CustomFlag3Info, xsdReceiveLine.CustomFlag3.ToString());
				context.SetPropertyInfoValueIfValueNotEmpty(receiveLine.WE_CustomFlag4Info, xsdReceiveLine.CustomFlag4.ToString());
				context.SetPropertyInfoValueIfValueNotEmpty(receiveLine.WE_CustomFlag5Info, xsdReceiveLine.CustomFlag5.ToString());
				context.SetPropertyInfoValueIfValueNotEmpty(receiveLine.WE_IsOriginalInventoryInfo, xsdInventory.IsOriginalReceiptLine.ToString());
				context.SetPropertyInfoValueIfValueNotEmpty(receiveLine.WE_OriginalInventoryStatusInfo, xsdInventory.InventoryStatus);
				context.SetPropertyInfoValueIfValueNotEmpty(receiveLine.WE_DocketLineStatusInfo, xsdReceiveLine.DocketLineStatus);
				context.SetPropertyInfoValueIfValueNotEmpty(receiveLine.WE_FinalisedDateInfo, xsdReceiveLine.FinalisedDateTimeOffset);

				ImportBondedWarehouseAttribute(xsdReceiveLine.CustomsData, context);
			}
		}

		#region ValidateAndImportProduct

		void ValidateAndImportProduct(WhsReceive receive, WhsReceiveLine receiveLine, SystemMergeReceiveLine xsdReceiveLine, IValueObjectImportContext context, string errorContext)
		{
			var partPK = new ZGuid(xsdReceiveLine.ProductPK);
			var part = context.Factory.Load<OrgSupplierPart>(partPK);
			if (part != null)
			{
				receiveLine.WE_OP = partPK;
			}
			else
			{
				var errorMessage = Res.GetString("174b6e03-a592-4641-ac62-24fe1ddc7b7e", "{0}\r\nCould not find Product with PK = ({1}).\r\nPlease import it first and then retry the import operation.", errorContext, partPK) + "\r\n";
				receive.Delete(); // delete Receive to not allow DataTransfer architecture to save it to DB.
				throw new ArgumentException(errorMessage);
			}
		}

		#endregion

		#region ValidateAndImportLocation

		void ValidateAndImportLocation(WhsReceive receive, WhsReceiveLine receiveLine, SystemMergeReceiveLine xsdReceiveLine, IValueObjectImportContext context, string errorContext)
		{
			var locationPK = new ZGuid(xsdReceiveLine.LocationPK);
			var location = context.Factory.Load<WhsLocation>(locationPK);
			if (location != null)
			{
				receiveLine.WE_WL = locationPK;
			}
			else
			{
				var errorMessage = Res.GetString("8f761547-53ce-4e04-9ae6-55b5e56010a4", "{0}\r\nCould not find Location with PK = ({1}).\r\nPlease import it first and then retry the import operation.", errorContext, locationPK) + "\r\n";
				receive.Delete(); // delete Receive to not allow DataTransfer architecture to save it to DB.
				throw new ArgumentException(errorMessage);
			}
		}

		#endregion

		#region ImportBondedWarehouseAttribute

		void ImportBondedWarehouseAttribute(SystemMergeBondedWarehouseAttribute xsdCustomsData, IValueObjectImportContext context)
		{
			var customsDataPK = new Guid(xsdCustomsData.PK);
			if (customsDataPK != Guid.Empty)
			{
				var customsData = context.Factory.NewWithPrimaryKey<WhsBondedWarehouseAttribute>(customsDataPK);
				customsData.WB_WB_InwardsEntry = new ZGuid(xsdCustomsData.WB_InwardsEntry);
				customsData.WB_ParentID = new ZGuid(xsdCustomsData.ParentID);

				customsData.WB_DeclarationReference = xsdCustomsData.DeclarationReference;
				customsData.WB_EntryKey = xsdCustomsData.EntryKey;
				customsData.WB_CustomsUnitOfQty = xsdCustomsData.CustomsUnitOfQty;
				customsData.WB_BondedWhsUnitOfQty = xsdCustomsData.BondedWhsUnitOfQty;
				customsData.WB_RN_NKCountryOfOrigin = xsdCustomsData.RN_NKCountryOfOrigin;
				customsData.WB_AddInfo = xsdCustomsData.AddInfo;
				customsData.WB_ParentTableCode = xsdCustomsData.ParentTableCode;
				customsData.WB_RX_NKTILVCurrency = xsdCustomsData.RX_NKTILVCurrency;

				customsData.WB_IsActive = xsdCustomsData.IsActive;

				customsData.WB_EntryLineNo = xsdCustomsData.EntryLineNo;

				customsData.WB_CustomsQty = xsdCustomsData.CustomsQty;
				customsData.WB_BondedWhsQty = xsdCustomsData.BondedWhsQty;
				customsData.WB_ValueForDuty = xsdCustomsData.ValueForDuty;
				customsData.WB_TILV = xsdCustomsData.TILV;

				customsData.WB_CustomsSecondQuantity = xsdCustomsData.CustomsSecondQuantity;
				customsData.WB_CustomsSecondUnitQty = xsdCustomsData.CustomsSecondUnitQty;
				customsData.WB_Tariff = xsdCustomsData.Tariff;
				customsData.WB_PrimaryPreference = xsdCustomsData.PrimaryPreference;

				customsData.WB_CustomsThirdQuantity = xsdCustomsData.CustomsThirdQuantity;
				customsData.WB_CustomsThirdUnitQty = xsdCustomsData.CustomsThirdUnitQty;
				customsData.WB_OA_ManufacturerAddress = new ZGuid(xsdCustomsData.ManufacturerAddress);

				customsData.WB_EntryDate = xsdCustomsData.EntryDate;
			}
		}

		#endregion

		#endregion

		#endregion

		#region FindBusinessObject

		protected override WhsDocket FindBusinessObject(SystemMergeWarehouseReceive xsdReceive, IValueObjectImportContext context)
		{
			return context.Factory.Load<WhsDocket>(new ZGuid(xsdReceive.PK));
		}

		#endregion

		#region NewBusinessObject

		protected override WhsDocket NewBusinessObject(SystemMergeWarehouseReceive xsdReceive, IValueObjectImportContext context)
		{
			return context.Factory.NewWithPrimaryKey<WhsReceive>(new Guid(xsdReceive.PK));
		}

		#endregion

		#region ConfirmUpdateOfExistingBusinessObject

		protected override bool ConfirmUpdateOfExistingBusinessObject(WhsDocket docket, INotifications notifications)
		{
			return false;
		}

		#endregion

		#region OnUserDeclinedImport

		protected override void OnUserDeclinedImport(WhsDocket docket, SystemMergeWarehouseReceive xsdReceive, IValueObjectImportContext context)
		{
			var message = Res.GetString("715477a3-ec0a-4b76-9fa5-1bd5f3d5ddc5", "Import of receive [({0}) - {1} - {2}] skipped. Reason: Receive already exists.", docket.PK, docket.WD_ExternalReference, docket.WD_DocketID) + "\r\n";
			context.Notify(new InfoNotification(message));
		}

		#endregion

		#region NotifyBizObjCreatedOrUpdated

		protected override void NotifyBizObjCreatedOrUpdated(INotifications notifications, BusinessObject receive)
		{
			// Don't notify here. It will be notified if save succeeds.
		}

		#endregion

		#endregion

		#region Overrides

		public override XmlSchema CollectionSchema
		{
			get { return null; }
		}

		public override string RootCollectionElementName
		{
			get { return "SystemMergeWarehouseReceives"; }
		}

		public override string RootElementName
		{
			get { return "SystemMergeWarehouseReceive"; }
		}

		public override XmlSchema Schema
		{
			get { return null; }
		}

		#endregion
	}
}
