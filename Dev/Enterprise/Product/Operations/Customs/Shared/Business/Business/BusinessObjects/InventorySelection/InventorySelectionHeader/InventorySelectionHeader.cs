using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public abstract class InventorySelectionHeader : AutoInventorySelectionHeader
	{
		protected InventorySelectionHeader(IWarehouseIntegrationSupporter parent)
			: base(new BusinessObjectFactory())
		{
			this.parent = parent;
			IsGroupByCartonSupported = true;
		}

		protected readonly IWarehouseIntegrationSupporter parent;

		public void UpdateSelectionLinesDetails(IEnumerable<IWhsInventoryView> inventories)
		{
			var inventoryWrappers = GetInventoryWrappersAndLoadRelated(inventories);
			UpdateUnusedSelectionLines();
			PackingGroupSelectionLineMap.Clear();
			using (SelectionLines.SuspendListChanged())
			{
				SelectionLines.RemoveAll();
				switch (groupByType)
				{
					case InventorySelectionGroupBy.Carton:
						UpdateSelectionLinesDetailsGroupByCarton(inventoryWrappers);
						break;
					case InventorySelectionGroupBy.Product:
						UpdateSelectionLinesDetailsGroupByProduct(inventoryWrappers);
						break;
					default:
						UpdateSelectionLinesDetailsGroupByInventory(inventoryWrappers);
						break;
				}
			}
		}

		public void ImportInventories()
		{
			ImportInventoriesResult = ZString.Empty;
			if (SelectedLines.Count > 0)
			{
				try
				{
					ImportSetup();
					ImportInventoriesCore();
					UpdateParentData();
				}
				finally
				{
					ImportTearDown();
				}
			}
		}

		public ZString ImportInventoriesResult;

		protected virtual void ImportInventoriesCore()
		{
			foreach (var selectedLine in SelectedLines.Cast<WhsInventoryWrapper>())
			{
				CreateProductLineFromWarehouseData(selectedLine);
			}
		}

		public override ZBool IsGroupByInventory
		{
			get { return groupByType == InventorySelectionGroupBy.Inventory; }
			set { SetGroupByType(IsGroupByInventoryInfo, InventorySelectionGroupBy.Inventory, IsGroupByInventory, value); }
		}

		public override ZBool IsGroupByProduct
		{
			get { return groupByType == InventorySelectionGroupBy.Product; }
			set { SetGroupByType(IsGroupByProductInfo, InventorySelectionGroupBy.Product, IsGroupByProduct, value); }
		}

		public override ZBool IsGroupByCarton
		{
			get { return groupByType == InventorySelectionGroupBy.Carton; }
			set { SetGroupByType(IsGroupByCartonInfo, InventorySelectionGroupBy.Carton, IsGroupByCarton, value); }
		}

		public void ClearDrawQuantities()
		{
			foreach (InventorySelectionLine line in SelectionLines)
			{
				line.US_ProductQtyToDraw = ZDecimal.Zero;
				line.US_CartonQtytoDraw = ZInt.Zero;
			}
		}

		public virtual void FillOutDrawQuantities()
		{
			foreach (InventorySelectionLine line in SelectionLines)
			{
				line.US_ProductQtyToDraw = line.US_ProductQtyOnHand;
			}
		}

		[ChildEditable]
		public IInventorySelectionLineCollection<InventorySelectionLine> SelectionLines
		{
			get
			{
				if (selectionLines == null)
				{
					selectionLines = GetNewInventorySelectionLineCollection();
					RegisterEditableChildObject(selectionLines);
				}
				return selectionLines;
			}
		}
		IInventorySelectionLineCollection<InventorySelectionLine> selectionLines;

		protected virtual IInventorySelectionLineCollection<InventorySelectionLine> GetNewInventorySelectionLineCollection() => new InventorySelectionLineCollection<InventorySelectionLine>(this);

		public WhsInventoryWrapperCollection SelectedLines
		{
			get
			{
				if (selectedLines == null)
				{
					selectedLines = new WhsInventoryWrapperCollection(this);
				}
				return selectedLines;
			}
		}
		WhsInventoryWrapperCollection selectedLines;

		public FilterBusinessObjectDefaults GetFilterDefaults()
		{
			return GetFilterDefaultsCore();
		}

		public event EventHandler OnGroupByChanged;

		public bool IsGroupByCartonSupported
		{
			get;
			protected set;
		}

		public virtual bool IsGroupByProductSupported => true;

		public virtual bool IsAutoSelectionSupported => false;

		public virtual bool AutoFillOutDrawQuantities => false;

		public bool AllowWithdrawalOfMultipleEntryDetails
		{
			get { return AllowWithdrawalOfMultipleEntryDetailsCore; }
		}

		#region Implementation

		WhsInventoryWrapper GetWrapperAndAddToInventoriesPerCarton(IWhsInventoryView inventory)
		{
			var result = GetWrapperAndClearQuantityDraw(inventory);
			AddToInventoriesPerCarton(result);
			return result;
		}

		WhsInventoryWrapper GetWrapperAndClearQuantityDraw(IWhsInventoryView inventory)
		{
			Dictionary<IWhsInventoryView, WhsInventoryWrapper> packingGroupDictionary;
			var packingGroupKey = GetPackingGroupKey(inventory);
			if (!FullInventoryWrapperDictionary.TryGetValue(packingGroupKey, out packingGroupDictionary))
			{
				packingGroupDictionary = new Dictionary<IWhsInventoryView, WhsInventoryWrapper>();
				FullInventoryWrapperDictionary.Add(packingGroupKey, packingGroupDictionary);
			}

			WhsInventoryWrapper result;
			if (!packingGroupDictionary.TryGetValue(inventory, out result))
			{
				result = new WhsInventoryWrapper(inventory, this);
				packingGroupDictionary.Add(inventory, result);
			}
			result.QuantityToDraw = ZDecimal.Zero;
			return result;
		}

		void AddToInventoriesPerCarton(WhsInventoryWrapper inventoryWrapper)
		{
			var packingGroupKey = inventoryWrapper.PackingGroupKey;
			Dictionary<ZString, SortedList<string, WhsInventoryWrapper>> inventoriesPerProduct;
			if (!InventoriesPerCarton.TryGetValue(packingGroupKey, out inventoriesPerProduct))
			{
				inventoriesPerProduct = new Dictionary<ZString, SortedList<string, WhsInventoryWrapper>>();
				InventoriesPerCarton.Add(packingGroupKey, inventoriesPerProduct);
			}
			AddToInventoriesPerProduct(inventoryWrapper, packingGroupKey, inventoriesPerProduct);
		}

		void AddToInventoriesPerProduct(WhsInventoryWrapper inventoryWrapper, ZString packingGroupKey, Dictionary<ZString, SortedList<string, WhsInventoryWrapper>> inventoriesPerProduct)
		{
			var productGroupingKey = packingGroupKey.IsEmpty ? ZString.Empty : inventoryWrapper.ProductGroupKey;
			if (!inventoriesPerProduct.TryGetValue(productGroupingKey, out var orderedInventories))
			{
				orderedInventories = new SortedList<string, WhsInventoryWrapper>();
				inventoriesPerProduct.Add(productGroupingKey, orderedInventories);
			}

			if (!orderedInventories.ContainsValue(inventoryWrapper))
			{
				orderedInventories.Add(inventoryWrapper.LineNoKey, inventoryWrapper);
			}
		}

		IEnumerable<WhsInventoryWrapper> GetListFromInventoriesPerCartonFor(ZString packingGroupKey)
		{
			IEnumerable<WhsInventoryWrapper> result = null;
			Dictionary<ZString, SortedList<string, WhsInventoryWrapper>> inventoriesPerProduct;
			if (InventoriesPerCarton.TryGetValue(packingGroupKey, out inventoriesPerProduct))
			{
				result = GetFirstProductFromProductGroup(inventoriesPerProduct);//Take out all other products packed together
			}
			return result ?? Enumerable.Empty<WhsInventoryWrapper>();
		}

		IEnumerable<WhsInventoryWrapper> GetFirstProductFromProductGroup(Dictionary<ZString, SortedList<string, WhsInventoryWrapper>> inventoriesPerProduct)
		{
			foreach (var inventories in inventoriesPerProduct.Values)
			{
				var result = inventories.Values.FirstOrDefault();
				if (result != null)
				{
					yield return result;
				}
			}
		}

		internal static ZString GetPackingGroupKey(IWhsDocketLine receiveLine)
		{
			var result = ZString.Empty;
			if (receiveLine != null)
			{
				result = receiveLine != null && !receiveLine.WE_PackageGroupId.IsEmpty ? receiveLine.WE_WD.ToString() + receiveLine.WE_PackageGroupId : string.Empty;
			}
			return result;
		}

		internal ZDecimal GetOriginalBondedQty(ZString packingGroupKey, ZString productGroupKey)
		{
			return GetValueTotal(packingGroupKey, productGroupKey, (x) => x.OriginalBondedQty);
		}

		internal ZDecimal GetQuantityOnHand(ZString packingGroupKey, ZString productGroupKey)
		{
			return GetValueTotal(packingGroupKey, productGroupKey, (x) => x.QuantityOnHand);
		}

		ZDecimal GetValueTotal(ZString packingGroupKey, ZString productGroupKey, Func<WhsInventoryWrapper, decimal> getValue)
		{
			var result = ZDecimal.Zero;
			if (InventoriesPerCarton.TryGetValue(packingGroupKey, out var inventoriesPerProduct) && inventoriesPerProduct.TryGetValue(productGroupKey, out var sortedInventories))
			{
				result = sortedInventories.Values.Sum(getValue);
			}
			return result;
		}

		ZString GetPackingGroupKey(IWhsInventoryView inventory)
		{
			var result = ZString.Empty;
			if (inventory != null)
			{
				result = GetPackingGroupKey(((BusinessObject)inventory).Factory.Load<IWhsDocketLine>(inventory.WI_WE_InDocketLine));
			}
			return result;
		}

		protected virtual void ImportSetup()
		{
		}

		protected virtual void ImportTearDown()
		{
		}

		protected virtual bool AllowWithdrawalOfMultipleEntryDetailsCore
		{
			get { return true; }
		}

		IEnumerable<WhsInventoryWrapper> GetInventoryWrappersAndLoadRelated(IEnumerable<IWhsInventoryView> inventories)
		{
			var result = new List<WhsInventoryWrapper>();
			InventoriesPerCarton.Clear();
			if (inventories != null)
			{
				var availableInventories = inventories.Where(x => x.WI_AvailableToPickQuantity > 0);
				foreach (var inventory in GetInventoriesInLocalFactory(availableInventories))
				{
					if (IsInventoryValid(inventory))
					{
						result.Add(GetWrapperAndAddToInventoriesPerCarton(inventory));
					}
				}
				LoadRelatedInventoryToInventoriesPerCarton();
			}
			return result;
		}

		protected virtual bool IsInventoryValid(IWhsInventoryView inventory) => true;

		IEnumerable<IWhsInventoryView> GetInventoriesInLocalFactory(IEnumerable<IWhsInventoryView> inventories)
		{
			return Factory.Load<IWhsInventoryView>(new ZQuery(WhsInventoryViewSchema.PK, inventories.Select(x => x.PK)));
		}

		void LoadRelatedInventoryToInventoriesPerCarton()
		{
			foreach (var dataPair in InventoriesPerCarton)
			{
				var packingGroupID = dataPair.Key;
				if (!packingGroupID.IsEmpty)
				{
					LoadRelatedInventoryToList(packingGroupID, dataPair.Value);
				}
			}
		}

		void LoadRelatedInventoryToList(ZString packingGroupKey, Dictionary<ZString, SortedList<string, WhsInventoryWrapper>> inventoriesPerProduct)
		{
			var inventoryWrapper = inventoriesPerProduct.Values.FirstOrDefault()?.Values?.FirstOrDefault();
			if (inventoryWrapper != null)
			{
				var receiveLine = inventoryWrapper.ReceiveLine;
				if (receiveLine != null)
				{
					var receiveQuery = new ZQuery(WhsDocketLineSchema.WE_WD, receiveLine.WE_WD);
					receiveQuery.AddToFilter(WhsDocketLineSchema.WE_PackageGroupId, receiveLine.WE_PackageGroupId);
					receiveQuery.AddToFilter(WhsDocketLineSchema.PK, SQLComparisonOperator.NotEqual, GetReceiveLinePKs(inventoriesPerProduct.Values.Select(x => x.Values).SelectMany(x => x)));
					var factory = inventoryWrapper.Factory;
					var relatedReceiveLines = factory.Load<IWhsDocketLine>(receiveQuery);
					if (relatedReceiveLines.Length > 0)
					{
						foreach (var relatedInventory in factory.Load<IWhsInventoryView>(new ZQuery(WhsInventoryViewSchema.WI_WE_InDocketLine, relatedReceiveLines.Select(x => x.PK))))
						{
							var relatedInventoryWrapper = GetWrapperAndClearQuantityDraw(relatedInventory);
							AddToInventoriesPerProduct(relatedInventoryWrapper, packingGroupKey, inventoriesPerProduct);
						}
					}
				}
			}
		}

		ZGuid[] GetReceiveLinePKs(IEnumerable<WhsInventoryWrapper> list)
		{
			return list.Select(x => x.ReceiveLine == null ? ZGuid.Empty : x.ReceiveLine.PK).Where(x => !x.IsEmpty).ToArray();
		}

		Dictionary<ZString, Dictionary<ZString, SortedList<string, WhsInventoryWrapper>>> InventoriesPerCarton
		{
			get { return inventoriesPerCartonDictionary ?? (inventoriesPerCartonDictionary = new Dictionary<ZString, Dictionary<ZString, SortedList<string, WhsInventoryWrapper>>>()); }
		}
		Dictionary<ZString, Dictionary<ZString, SortedList<string, WhsInventoryWrapper>>> inventoriesPerCartonDictionary;

		Dictionary<ZString, Dictionary<IWhsInventoryView, WhsInventoryWrapper>> FullInventoryWrapperDictionary
		{
			get { return fullInventoryWrapperDictionary ?? (fullInventoryWrapperDictionary = new Dictionary<ZString, Dictionary<IWhsInventoryView, WhsInventoryWrapper>>()); }
		}
		Dictionary<ZString, Dictionary<IWhsInventoryView, WhsInventoryWrapper>> fullInventoryWrapperDictionary;

		IWarehouseProductLine CreateProductLineFromWarehouseData(WhsInventoryWrapper inventoryWrapper)
		{
			IWarehouseProductLine result = null;
			if (inventoryWrapper != null && CanCreateProductLine)
			{
				var whsReceiveLine = inventoryWrapper.ReceiveLine;
				if (whsReceiveLine != null)
				{
					var invoiceQuantity = inventoryWrapper.QuantityToDraw;
					var ratio = invoiceQuantity / (whsReceiveLine.WE_TransactionQuantity.IsEmpty ? 1m : (decimal)whsReceiveLine.WE_TransactionQuantity);
					var attributeQuery = new ZQuery(WhsBondedWarehouseAttributeSchema.WB_ParentID, whsReceiveLine.PK);
					attributeQuery.AddToFilter(WhsBondedWarehouseAttributeSchema.WB_ParentTableCode, WhsDocketLineSchema.Constants.Prefix);
					IWhsBondedWarehouseAttribute whsBondedWarehouseAttribute = Factory.LoadTop1<IWhsBondedWarehouseAttribute>(attributeQuery);
					result = CreateProductLineFromWarehouseDataCore(inventoryWrapper, whsReceiveLine, whsBondedWarehouseAttribute, invoiceQuantity, ratio);
				}
			}
			return result;
		}

		protected virtual bool CanCreateProductLine
		{
			get { return true; }
		}

		protected abstract void UpdateParentData();

		protected abstract IWarehouseProductLine CreateProductLineFromWarehouseDataCore(WhsInventoryWrapper inventoryWrapper, IWhsDocketLine whsReceiveLine, IWhsBondedWarehouseAttribute whsBondedWarehouseAttribute, ZDecimal invoiceQuantity, ZDecimal ratio);

		protected ZDecimal Round(ZDecimal value, int numberOfDecimalPlaces)
		{
			return value.Round(numberOfDecimalPlaces);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			groupByType = InventorySelectionGroupBy.Inventory;
		}

		void SetGroupByType(ZPropertyInfo info, InventorySelectionGroupBy type, ZBool oldValue, ZBool newValue)
		{
			if (oldValue != newValue)
			{
				var oldGroupByType = groupByType;
				groupByType = newValue ? type : oldValue ? InventorySelectionGroupBy.None : oldGroupByType;
				if (newValue && OnGroupByChanged != null)
				{
					OnGroupByChanged(this, EventArgs.Empty);
				}
				Factory.InvalidateCachedProperties();
				info.RefreshBinding(oldValue);
			}
		}
		InventorySelectionGroupBy groupByType;

		InventorySelectionLine UpdateUnusedOrNewSelectionLine(params WhsInventoryWrapper[] inventoryWrappers)
		{
			var selectionLine = GetUnusedSelectionLine();
			if (selectionLine == null)
			{
				selectionLine = SelectionLines.AddNew();
			}
			else
			{
				SelectionLines.Add(selectionLine);
			}
			selectionLine.UpdateSelectionLinesDetails(inventoryWrappers);
			return selectionLine;
		}

		void UpdateSelectionLinesDetailsGroupByInventory(IEnumerable<WhsInventoryWrapper> inventoryWrappers)
		{
			foreach (var inventoryWrappersGroupByEntryKey in inventoryWrappers.GroupBy(x => x.WarehouseKey + x.BondedEntryKeyForGrouping + x.ProductGroupKey + x.PackingGroupKey))
			{
				var selectionLine = UpdateUnusedOrNewSelectionLine(inventoryWrappersGroupByEntryKey.ToArray());
				var inventoryWrapper = inventoryWrappersGroupByEntryKey.FirstOrDefault();
				var packingGroupKey = inventoryWrapper == null ? ZString.Empty : inventoryWrapper.PackingGroupKey;
				if (!packingGroupKey.IsEmpty)
				{
					List<InventorySelectionLine> list;
					if (!PackingGroupSelectionLineMap.TryGetValue(packingGroupKey, out list))
					{
						list = new List<InventorySelectionLine>();
						PackingGroupSelectionLineMap.Add(packingGroupKey, list);
					}
					if (!list.Contains(selectionLine))
					{
						list.Add(selectionLine);
					}
				}
			}
		}

		Dictionary<ZString, List<InventorySelectionLine>> PackingGroupSelectionLineMap
		{
			get { return packingGroupSelectionLineMap ?? (packingGroupSelectionLineMap = new Dictionary<ZString, List<InventorySelectionLine>>()); }
		}
		Dictionary<ZString, List<InventorySelectionLine>> packingGroupSelectionLineMap;

		void UpdateSelectionLinesDetailsGroupByProduct(IEnumerable<WhsInventoryWrapper> inventoryWrappers)
		{
			foreach (var inventoriesGroupByProduct in inventoryWrappers.GroupBy(x => x.WarehouseKey + x.ProductKey + x.Inventory.WI_F3_NKPackType))
			{
				UpdateUnusedOrNewSelectionLine(inventoriesGroupByProduct.ToArray());
			}
		}

		void UpdateSelectionLinesDetailsGroupByCarton(IEnumerable<WhsInventoryWrapper> inventoryWrappers)
		{
			foreach (var inventoryWrappersGroupByCarton in inventoryWrappers.GroupBy(x => x.PackingGroupKey))
			{
				if (inventoryWrappersGroupByCarton.Key.IsEmpty)
				{
					foreach (var inventoriesGroupByWarehouse in inventoryWrappersGroupByCarton.GroupBy(x => x.WarehouseKey))
					{
						foreach (var inventoryWrapper in inventoriesGroupByWarehouse)
						{
							UpdateUnusedOrNewSelectionLine(inventoryWrapper);
						}
					}
				}
				else
				{
					foreach (var inventoryWrappersGroupByWarehouse in inventoryWrappersGroupByCarton.GroupBy(x => x.WarehouseKey))
					{
						UpdateUnusedOrNewSelectionLine(inventoryWrappersGroupByCarton.ToArray());
					}
				}
			}
		}

		InventorySelectionLine GetUnusedSelectionLine()
		{
			InventorySelectionLine result = null;
			if (UnusedSelectionLines.Count > 0)
			{
				result = UnusedSelectionLines[0];
				UnusedSelectionLines.Remove(result);
			}
			return result;
		}

		void UpdateUnusedSelectionLines()
		{
			// Keep newest line on the top
			var existingUnusedLines = UnusedSelectionLines.ToArray();
			UnusedSelectionLines.Clear();
			foreach (var line in SelectionLines.OfType<InventorySelectionLine>())
			{
				line.ClearInventoryDetail();
				UnusedSelectionLines.Add(line);
			}
			UnusedSelectionLines.AddRange(existingUnusedLines);
		}

		List<InventorySelectionLine> UnusedSelectionLines
		{
			get { return unusedSelectionLines ?? (unusedSelectionLines = new List<InventorySelectionLine>()); }
		}
		List<InventorySelectionLine> unusedSelectionLines;

		protected virtual FilterBusinessObjectDefaults GetFilterDefaultsCore()
		{
			var result = new FilterBusinessObjectDefaults();
			if (parent != null && parent.ClientPK.IsValid)
			{
				result.Add(new FilterBusinessObjectDefault("Client", "Property", parent.ClientPK, false));
			}
			var warehouse = WhsWarehouse;
			if (warehouse != null)
			{
				result.Add(new FilterBusinessObjectDefault("Warehouse", "Property", warehouse.PK, false));
			}
			return result;
		}

		protected IWhsWarehouse WhsWarehouse
		{
			get
			{
				IWhsWarehouse result = null;
				var warehouseAddress = GetWarehouseAddress();
				if (warehouseAddress != null)
				{
					result = parent?.Factory.LoadTop1<IWhsWarehouse>(new ZQuery(WhsWarehouseSchema.WW_OA_WarehouseAddress, warehouseAddress.PK));
				}
				return result;
			}
		}

		protected virtual OrgAddress GetWarehouseAddress()
		{
			return parent?.WarehouseAddress;
		}

		#endregion

		internal protected virtual string GetCannotWithdrawProductHavingDifferentEntryNumber()
		{
			return InventorySelectionLine.CannotWithdrawProductHavingDifferentEntryNumber;
		}

		bool isUpdateRelatedProductQtyInProgress;
		internal void UpdateRelatedProductQty(InventorySelectionLine selectionLineToIgnore, ZInt cartonQty)
		{
			if (!isUpdateRelatedProductQtyInProgress && PackingGroupSelectionLineMap.Count > 0)
			{
				try
				{
					isUpdateRelatedProductQtyInProgress = true;
					var inventoryWrapper = selectionLineToIgnore.InventoryWrappers.FirstOrDefault();
					if (inventoryWrapper != null)
					{
						var packingGroupKey = inventoryWrapper.PackingGroupKey;
						if (!packingGroupKey.IsEmpty)
						{
							List<InventorySelectionLine> list;
							if (PackingGroupSelectionLineMap.TryGetValue(packingGroupKey, out list))
							{
								foreach (var otherSelection in list.Where(x => x != selectionLineToIgnore))
								{
									otherSelection.US_CartonQtytoDraw = cartonQty;
								}
							}
						}
					}
				}
				finally
				{
					isUpdateRelatedProductQtyInProgress = false;
				}
			}
		}

		internal void UpdateInventoriesPerCartonQuantityMatching(ZString packingGroupKey, ZInt cartonQtytoDraw)
		{
			var list = GetListFromInventoriesPerCartonFor(packingGroupKey);
			if (list != null)
			{
				foreach (var inventoryWrapper in list)
				{
					inventoryWrapper.QuantityToDraw = inventoryWrapper.PerPackageQty * cartonQtytoDraw;
				}
			}
		}

		internal void UpdateSelectedInventory(WhsInventoryWrapper inventoryWrapper)
		{
			if (inventoryWrapper.QuantityToDraw.IsEmpty)
			{
				SelectedLines.Remove(inventoryWrapper);
			}
			else
			{
				SelectedLines.Add(inventoryWrapper);
			}
		}

		protected Dictionary<ZString, ZString> PopulateAddInfoData(IWhsBondedWarehouseAttribute whsBondedWarehouseAttribute)
		{
			Dictionary<ZString, ZString> addInfos = null;
			if (whsBondedWarehouseAttribute != null)
			{
				addInfos = AddInfoParser.CreateDictionaryWithAddInfoString(whsBondedWarehouseAttribute.WB_AddInfo);
			}
			return addInfos ?? new Dictionary<ZString, ZString>();
		}
	}
}
