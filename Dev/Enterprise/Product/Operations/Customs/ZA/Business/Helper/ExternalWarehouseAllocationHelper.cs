using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.Business.CusWHSOperatorTransactionNettOffProcedure;

namespace Enterprise.Customs.ZA.Business
{
	public class ExternalWarehouseAllocationHelper
	{
		public ExternalWarehouseAllocationHelper(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		public List<ExbondEntryLine> Allocate(IEnumerable<CusWHSOperatorTransactionLine> transactionLines)
		{
			var orderReceipts = transactionLines.Select(line => new TransactionLine(
				line.PK.ToGuid(),
				line.WOL_WOT_WHSOperatorTransactionOrder.ToGuid(),
				line.WOL_WOT_WHSOperatorTransactionReceipt.ToGuid(),
				line.WOL_Quantity,
				line.WOL_CustomsEntryLineNo));
			return Allocate(orderReceipts);
		}

		public List<ExbondEntryLine> Allocate(IEnumerable<TransactionLine> orderReceipts)
		{
			var usedInventory = new Dictionary<ZGuid, ZDecimal>();
			return orderReceipts.SelectMany(orderReceipt => Allocate(orderReceipt, usedInventory)).ToList();
		}

		protected List<ExbondEntryLine> Allocate(TransactionLine orderReceiptLine, Dictionary<ZGuid, ZDecimal> usedInventory)
		{
			var receipt = factory.Load<CusWHSOperatorTransaction>(orderReceiptLine.ReceiptPK);
			var order = factory.Load<CusWHSOperatorTransaction>(orderReceiptLine.OrderPK);
			var exbondLines = new List<ExbondEntryLine>();

			if (receipt.WOT_IsCustomsControlled && receipt.Product != null && !receipt.WOT_CustomsEntryNumber.IsEmpty)
			{
				var query = new ZDBOnlyQuery(typeof(IWhsInventoryView));
				query.AddToFilter(WhsInventoryViewSchema.WI_OH_Client, receipt.WOT_OH_ProductOwner);
				query.AddToFilter(WhsInventoryViewSchema.WI_WW_Whs, receipt.Batch.Warehouse.GetWhsWarehouse().PK);
				query.AddToFilter(WhsInventoryViewSchema.WI_InventoryStatus, InventoryStatus.Codes.Available);
				query.AddToFilter(WhsInventoryViewSchema.WI_OP, receipt.Product.PK);

				var whsInventories = factory.Load<IWhsInventoryView>(query);
				var whsDocketLines = factory.Load<IWhsDocketLine>(new ZQuery(WhsDocketLineSchema.PK, whsInventories.Select(x => x.WI_WE_InDocketLine)));

				var bondedAttributeQuery = new ZQuery();
				bondedAttributeQuery.AddToFilter(WhsBondedWarehouseAttributeSchema.WB_EntryKey, receipt.WOT_CustomsEntryNumber);
				bondedAttributeQuery.AddToFilter(WhsBondedWarehouseAttributeSchema.WB_ParentTableCode, WhsDocketLineSchema.Constants.Prefix);
				bondedAttributeQuery.AddToFilter(WhsBondedWarehouseAttributeSchema.WB_ParentID, whsInventories.Select(x => x.WI_WE_InDocketLine));

				var bondedAttributes = factory.Load<IWhsBondedWarehouseAttribute>(bondedAttributeQuery);

				var inventoryWrappers = new List<InventoryWrapper>();

				inventoryWrappers.AddRange(bondedAttributes
											.Select(attrib => new InventoryWrapper(attrib,
												whsInventories.First(x => x.WI_WE_InDocketLine == attrib.WB_ParentID),
												whsDocketLines.First(x => x.PK == attrib.WB_ParentID)))
											.OrderByDescending(iw => iw.MatchesEntryLineNo(orderReceiptLine.EntryLineNo))
											.ThenByDescending(iw => iw.HasAvailableStock(usedInventory))
											.ThenBy(iw => iw.Inventory.WI_ArrivalDate));

				var quantityNeeded = orderReceiptLine.Quantity;

				foreach (var inventoryWrapper in inventoryWrappers)
				{
					var remainingQuantity = GetRemainingQuantity(inventoryWrapper.Inventory, usedInventory);
					if (remainingQuantity > 0)
					{
						var exbondLine = new ExbondEntryLine();
						if (quantityNeeded > remainingQuantity)
						{
							exbondLine.Quantity = remainingQuantity;
							quantityNeeded -= remainingQuantity;
							UpdateUsedQuantity(inventoryWrapper.Inventory, usedInventory, remainingQuantity);
						}
						else if (quantityNeeded > 0)
						{
							exbondLine.Quantity = quantityNeeded;
							UpdateUsedQuantity(inventoryWrapper.Inventory, usedInventory, quantityNeeded);
							quantityNeeded = 0;
						}
						exbondLine.PopulateBondedGoodsProperties(orderReceiptLine.PK, receipt, order, inventoryWrapper.DocketLine, inventoryWrapper.WarehouseAttribute);
						exbondLines.Add(exbondLine);
					}
					if (quantityNeeded == 0)
					{
						break;
					}

					if (inventoryWrappers.IndexOf(inventoryWrapper) == inventoryWrappers.Count - 1 && quantityNeeded > 0)
					{
						var lastExbondLine = exbondLines.LastOrDefault();
						if (lastExbondLine == null)
						{
							lastExbondLine = new ExbondEntryLine();
							exbondLines.Add(lastExbondLine);
						}
						lastExbondLine.Quantity += quantityNeeded;
						lastExbondLine.PopulateBondedGoodsProperties(orderReceiptLine.PK, receipt, order, inventoryWrapper.DocketLine, inventoryWrapper.WarehouseAttribute);
					}
				}
			}

			if (!receipt.WOT_IsCustomsControlled)
			{
				var exbondLine = new ExbondEntryLine();
				exbondLine.PopulateLocalGoodsProperties(orderReceiptLine.PK, receipt, order, orderReceiptLine.Quantity);
				exbondLines.Add(exbondLine);
			}

			return exbondLines;
		}

		ZDecimal GetRemainingQuantity(IWhsInventoryView inventory, Dictionary<ZGuid, ZDecimal> usedInventory)
		{
			if (!usedInventory.TryGetValue(inventory.PK, out var used))
			{
				used = 0m;
			}
			return inventory.WI_TotalUnits - used;
		}

		void UpdateUsedQuantity(IWhsInventoryView inventory, Dictionary<ZGuid, ZDecimal> usedInventory, ZDecimal used)
		{
			if (!usedInventory.TryGetValue(inventory.PK, out var alreadyUsed))
			{
				alreadyUsed = 0m;
			}
			usedInventory[inventory.PK] = alreadyUsed + used;
		}

		readonly BusinessObjectFactory factory;
	}

	public class InventoryWrapper
	{
		public InventoryWrapper(IWhsBondedWarehouseAttribute warehouseAttribute, IWhsInventoryView inventoryView, IWhsDocketLine docketLine)
		{
			Inventory = inventoryView;
			DocketLine = docketLine;
			WarehouseAttribute = warehouseAttribute;
		}

		public IWhsInventoryView Inventory { get; private set; }
		public IWhsDocketLine DocketLine { get; private set; }
		public IWhsBondedWarehouseAttribute WarehouseAttribute { get; private set; }

		public bool HasAvailableStock(Dictionary<ZGuid, ZDecimal> usedInventory)
		{
			if (!usedInventory.TryGetValue(Inventory.PK, out var used))
			{
				used = 0m;
			}
			return (Inventory.WI_TotalUnits - used) > 0;
		}

		public bool MatchesEntryLineNo(short entryLineNo) => WarehouseAttribute.WB_EntryLineNo == entryLineNo;
	}

	public class ExbondEntryLine
	{
		public bool IsCustomsControlled;
		public ZGuid Owner;
		public ZString OwnerReference;
		public ZString Currency;
		public ZDate InvoiceDate;
		public ZString CountryOfOrigin;
		public ZDecimal Quantity;
		public ZString ProductCode;
		public ZDecimal Price;
		public ZDecimal PriceExbond;
		public ZGuid? Warehouse;
		public ZString? TariffCode;
		public ZString? Preference;
		public ZString? ROOCert;
		public ZString? MRN;
		public ZShort? MRNLine;
		public ZDecimal? CustomsQuantity;
		public ZDecimal? AdditionalQty1;
		public ZDecimal? AdditionalQty2;
		public ZDecimal? CountableQty;
		public ZString? CountableUom;
		public ZString? PreviousProcedure;
		public ZString DataImportMatchingKey;

		internal void PopulateLocalGoodsProperties(Guid transactionLinePK, CusWHSOperatorTransaction receipt, CusWHSOperatorTransaction order, ZDecimal quantity)
		{
			Owner = receipt.WOT_OH_ProductOwner;
			OwnerReference = order.WOT_OwnerReference;
			Currency = order.WOT_RX_NKCurrency;
			InvoiceDate = order.WOT_TransactionDate;
			ProductCode = order.Product.OP_PartNum;
			IsCustomsControlled = false;
			CountryOfOrigin = receipt.WOT_RN_NKOrigin;
			Quantity = quantity;
			Price = CalculateByRatio(order.WOT_TotalValue, GetQuantityRatio(order.WOT_Quantity), WhsBondedWarehouseAttributeSchema.WB_ValueForDuty);
			DataImportMatchingKey = transactionLinePK.ToString();
		}

		internal void PopulateBondedGoodsProperties(Guid transactionLinePK, CusWHSOperatorTransaction receipt, CusWHSOperatorTransaction order, IWhsDocketLine docketLine,
			IWhsBondedWarehouseAttribute warehouseAttribute)
		{
			Owner = receipt.WOT_OH_ProductOwner;
			OwnerReference = order.WOT_OwnerReference;
			Currency = order.WOT_RX_NKCurrency;
			InvoiceDate = order.WOT_TransactionDate;
			ProductCode = order.Product.OP_PartNum;
			IsCustomsControlled = true;
			Warehouse = receipt.Batch.WOB_OA_Warehouse;
			DataImportMatchingKey = transactionLinePK.ToString();

			if (warehouseAttribute != null)
			{
				var ratio = GetQuantityRatio(docketLine?.WE_TransactionQuantity);
				var addInfos = AddInfoParser.CreateDictionaryWithAddInfoString(warehouseAttribute.WB_AddInfo);
				addInfos.TryGetValue(BondedWarehousingHelper.Constants.OriginalProcedureCode, out var previousProcedure);
				if (previousProcedure.IsEmpty && !warehouseAttribute.WB_InwardProcedure.IsEmpty)
				{
					previousProcedure = warehouseAttribute.WB_InwardProcedure.Left(2);
				}

				CountryOfOrigin = warehouseAttribute.WB_RN_NKCountryOfOrigin;
				Price = CalculateByRatio(order.WOT_TotalValue, GetQuantityRatio(order.WOT_Quantity), WhsBondedWarehouseAttributeSchema.WB_ValueForDuty);
				PriceExbond = CalculateByRatio(warehouseAttribute.WB_ValueForDuty, ratio, WhsBondedWarehouseAttributeSchema.WB_ValueForDuty);
				TariffCode = warehouseAttribute.WB_Tariff;
				Preference = warehouseAttribute.WB_PrimaryPreference;
				ROOCert = addInfos.GetValueSafe(BondedWarehousingHelper.Constants.ROOCert);
				MRN = warehouseAttribute.WB_EntryKey;
				MRNLine = warehouseAttribute.WB_EntryLineNo;
				CustomsQuantity = CalculateByRatio(warehouseAttribute.WB_CustomsQty, ratio, WhsBondedWarehouseAttributeSchema.WB_CustomsQty);
				AdditionalQty1 = CalculateByRatio(warehouseAttribute.WB_CustomsSecondQuantity, ratio, WhsBondedWarehouseAttributeSchema.WB_CustomsSecondQuantity);
				AdditionalQty2 = CalculateByRatio(warehouseAttribute.WB_CustomsThirdQuantity, ratio, WhsBondedWarehouseAttributeSchema.WB_CustomsThirdQuantity);
				CountableQty = Quantity;

				if (!warehouseAttribute.WB_BondedWhsUnitOfQty.IsEmpty)
				{
					CountableUom = warehouseAttribute.WB_BondedWhsUnitOfQty;
				}
				else
				{
					CountableUom = docketLine.WE_F3_NKPackType;
				}
				PreviousProcedure = previousProcedure;
			}
		}

		ZDecimal GetQuantityRatio(ZDecimal? baseQuantity) => baseQuantity.HasValue && baseQuantity.Value != 0 ? Quantity / baseQuantity.Value : 1m;

		ZDecimal CalculateByRatio(ZDecimal @value, ZDecimal ratio, SchemaDecimalColumn info)
		{
			ZDecimal result = value * ratio;
			return result.Round(info.Scale);
		}
	}
}
