using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	[DependentBusinessObject(typeof(WhsWorkOrder), nameof(WhsWorkOrder.Lines))]
	public class WhsWorkOrderLine : WhsComponentOrderLine
	{
		public WhsWorkOrderLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		public new abstract class Schema : WhsPickableDocketLine.Schema
		{
			public const string WE_BOMParentLineNo = nameof(WE_BOMParentLineNo);
			public const string WE_Level = nameof(WE_Level);
		}

		#endregion

		#region Type Decider

		public override Type DocketType => typeof(WhsWorkOrder);

		#endregion

		#region Related Entities

		public WhsWorkOrder WorkOrder => (WhsWorkOrder)base.Docket;

		public new WhsWorkOrderLine ParentLine => (WhsWorkOrderLine)base.ParentLine;

		#endregion

		#region SetDefaultValues

		protected override string DocketLineType => CodeLists.DocketType.Codes.WorkOrder;

		#endregion

		#region Picking

		#region ReduceOverpickedStock

		/// <summary>
		/// The pick algorithm will pick as much as it can, but if one or more components are in shortfall, we may have overpicked *this* component.
		/// 
		/// For example, a Bike requires:
		///		1 frame
		///		2 wheels
		///		
		/// If we order 10 bikes, we need 10 frames + 20 wheels. If only 8 frames are in stock, 8 frames + 20 wheels will be picked.
		/// We need to return 4 wheels.
		/// 
		///		*** Tested in WhsPick.TestPickOrders_ReturnsOverpickedWorkOrderComponents() ***
		/// 
		/// </summary>
		internal void ReduceOverpickedStock()
		{
			var workOrder = Argument.NotNull(WorkOrder, "WorkOrder");
			if (workOrder.Pick == null)
			{
				throw new NotSupportedException("Cannot call ReturnUnusedComponents() on a WhsWorkOrderLine whose parent Order has no Pick.");
			}

			var pickLineQuantity = PickLineQuantity;
			if (workOrder.IsAssembly && pickLineQuantity > 0) // we are component used to build the product
			{
				var supplierPart = SupplierPart;
				var parentLine = ParentLine;
				var bomPart = parentLine.BillOfMaterials.FindByComponentPKandPackType(WE_OP, WE_F3_NKPackType);
				if (bomPart != null) // can be null if BOM definition changes before pick.
				{
					ZDecimal packsNeeded = bomPart.OE_ComponentQty * parentLine.QuantityAssembled;

					var packsPicked = supplierPart.UnitConverter.Convert(pickLineQuantity, supplierPart.OP_StockKeepingUnit, WE_F3_NKPackType);

					if (packsPicked > packsNeeded)
					{
						var quantityNeeded = supplierPart.UnitConverter.Convert(packsNeeded, WE_F3_NKPackType, supplierPart.OP_StockKeepingUnit);
						ReduceOverpickedStock(pickLineQuantity - quantityNeeded);
					}
				}
			}
		}

		protected override bool ReduceOverpickedStockAllowed => true;

		#endregion

		#endregion

		#region Validation

		public new WhsWorkOrderLineValidation Validation => (WhsWorkOrderLineValidation)base.Validation;

		protected override WhsPickableDocketLineValidation GetNewPickableDocketLineValidation() => new WhsWorkOrderLineValidation(this);

		#endregion

		#region Lookups

		public new WhsWorkOrderLineLookups Lookups => (WhsWorkOrderLineLookups)base.Lookups;

		protected override WhsDocketLineLookups GetNewLookups() => new WhsWorkOrderLineLookups(this);

		#endregion

		#region ReadOnly

		protected override bool StandardReadOnly => base.StandardReadOnly || ParentLine != null;

		public override bool ReadOnly
		{
			get { return base.ReadOnly || BOM.IsComponent; }
			set { base.ReadOnly = value; }
		}

		#endregion

		#region Properties

		#region WE_BOMParentLineNo

		public ZShort WE_BOMParentLineNo => ParentLine?.WE_LineNo ?? (ZShort)0;

		public virtual ZPropertyInfo WE_BOMParentLineNoInfo => GetZPropertyInfo(Schema.WE_BOMParentLineNo);

		protected bool WE_BOMParentLineNo_ReadOnly => true;

		#endregion

		#region WE_OP

		public override ZGuid WE_OP
		{
			get { return base.WE_OP; }
			set
			{
				bool valueChanged = base.WE_OP != value;

				if (!IsCopying && valueChanged && IsBOMProduct)
				{
					BOM.DeleteChildComponentLines();
				}

				base.WE_OP = value;
				if (!IsCopying && valueChanged)
				{
					BOM.CreateChildComponentLines();
				}
			}
		}

		#endregion

		#region WE_WD

		public override ZGuid WE_WD
		{
			get { return base.WE_WD; }
			set
			{
				var valueChanged = base.WE_WD != value;

				base.WE_WD = value;
				if (!IsCopying && valueChanged)
				{
					if (IsBOMProduct)
					{
						BOM.DeleteChildComponentLines();
					}

					ClearInVirtualWarehouse();
					if (WE_WD.IsValid)
					{
						if (BOM.ChildComponentLines.Count == 0) // Used after cloning when the line is added to the parent list (eg. the Grid)
						{
							BOM.CreateChildComponentLines();
						}
					}
				}
			}
		}

		#endregion

		#region InVirtualWarehouse

		ZBool? InVirtualWarehouse
		{
			get
			{
				if (inVirtualWhs == null)
				{
					inVirtualWhs = WorkOrder?.Warehouse?.WW_IsVirtualWarehouse;
				}
				return inVirtualWhs ?? false;
			}
		}
		ZBool? inVirtualWhs;

		public void ClearInVirtualWarehouse() => inVirtualWhs = null;

		#endregion

		#region WE_TransactionQuantity

		public override ZDecimal WE_TransactionQuantity
		{
			get { return base.WE_TransactionQuantity; }
			set
			{
				base.WE_TransactionQuantity = value;
				var workOrder = WorkOrder;
				if (workOrder != null && !workOrder.BOM.IsAutoCreatingWorkOrders)
				{
					BOM.UpdateChildComponentLineQtys();
					BOM.UpdateChildComponentLineShortfalls_ForUnpickedOrder();
				}
			}
		}

		#endregion

		#region WE_Level

		public ZDecimal WE_Level
		{
			get
			{
				ZDecimal result = 0m;

				var parent = ParentLine;
				while (parent != null)
				{
					parent = parent.ParentLine;
					result++;
				}

				return result;
			}
		}

		#endregion

		#region QuantityAssembled

		public ZDecimal QuantityAssembled
		{
			get { return BOM.IsTopLevelProduct ? GetQuantityAssembled(includeSecondaryProductsAndOverPickedComponents: false) : 0; }
		}

		// Tested in WhsWorkOrderValidationTest.cs
		public ZDecimal QuantityAssembledIncludingSecondaryProductsAndOverPickedComponents
		{
			get { return BOM.IsTopLevelProduct ? GetQuantityAssembled(includeSecondaryProductsAndOverPickedComponents: true) : 0; }
		}

		public bool HaveOverPickedComponent
		{
			get
			{
				if (BOM.IsTopLevelProduct)
				{
					var quantityAssembled = QuantityAssembled;
					foreach (var subPartWorkOrderLine in BOM.ChildComponentLines)
					{
						var bomPart = BillOfMaterials.FindByComponentPKandPackType(subPartWorkOrderLine.WE_OP, subPartWorkOrderLine.WE_F3_NKPackType);
						if (bomPart != null) // can be null if BOM definition changes.
						{
							var subPart = subPartWorkOrderLine.SupplierPart;
							var unitsPerPack = subPart.UnitConverter.Convert(bomPart.OE_ComponentQty, subPartWorkOrderLine.WE_F3_NKPackType, subPart.OP_StockKeepingUnit);

							var packsNeeded = unitsPerPack * quantityAssembled;
							var packsPicked = subPartWorkOrderLine.PickLines.GetQtyCommitted();

							if (packsPicked > packsNeeded)
							{
								return true;
							}
						}
					}
				}

				return false;
			}
		}

		/// <summary>
		/// Calculates the maximum valid Quantity Picked for a Work Order Line based on the Quantities Picked of its components.
		/// Enumerates the Pick Lines of the Work Order Line. The Pick Lines contain the quantities picked for the sub-components. 
		/// Each part's pick qty is totalled (eg 2 Red Rims + 2 Black Rims gives 4 Rims total), and then the unique part with the
		/// lowest qty picked represents the number of BOM items that can be built.
		/// 
		/// For example, if we have:
		/// 
		///  4x Bikes (ParentLine)
		///     1x Engine (Sub Part)
		///     2x Wheels (Sub Part)
		///     
		/// ..if we have picked 4 wheels and 3 engines, we can only assemble 2 Bikes because each bike requires 2 wheels.
		///   Thus, GetQuantityAssembled() will return 2.
		///   
		/// Including secondary products and over picked components will add to the result secondary products * the initial result, and the quantity of over picked components. 
		/// The amounts of secondary products are determined on the OrgSupplierPart's BOM.
		/// /// 
		/// </summary>
		/// <returns>The Quantity of BOM items that can be built.</returns>
		ZDecimal GetQuantityAssembled(bool includeSecondaryProductsAndOverPickedComponents)
		{
			var result = WE_TransactionQuantity;
			var supplierPart = SupplierPart;
			var overpickedQty = 0m;
			if (supplierPart != null && BillOfMaterials.Count > 0) // only check from top level lines
			{
				var subPartQtyPickedTotals = GetSubPartQtyPickedTotals();

				// grab the smallest picked qty
				foreach (var qtyPicked in subPartQtyPickedTotals)
				{
					var subPartQtyKey = qtyPicked.Key;
					var subPartQtyValue = qtyPicked.Value;

					var pickedQty = subPartQtyValue.PickedQty;
					var unitsPerPack = subPartQtyValue.UnitsPerPack;
					if (unitsPerPack == 0)
					{
						result = 0; // if one component's packtype is not convertible, can not assemble product therefore we do not need to continue. 
						break;
					}
					else
					{
						var subPartQtyPicked = Math.Floor(pickedQty / unitsPerPack);
						if (subPartQtyPicked < result)
						{
							result = subPartQtyPicked;
						}
					}
				}

				if (includeSecondaryProductsAndOverPickedComponents)
				{
					foreach (var qtyPicked in subPartQtyPickedTotals)
					{
						var subPartQtyValue = qtyPicked.Value;
						var overpickedCount = subPartQtyValue.PickedQty - (result * subPartQtyValue.UnitsPerPack);
						if (overpickedCount > 0)
						{
							overpickedQty += overpickedCount;
						}
					}

					if (result != 0 && WorkOrder.WD_IsInwardsProcessingJob)
					{
						// During a Inward processing job assembly can produce secondary products as a byproduct
						// Add these secondary products to the quantity assembled.

						var assembledPartQty = result;
						foreach (var secondaryPart in supplierPart.SecondaryParts)
						{
							result += secondaryPart.OSB_ProductQuantity * assembledPartQty;
						}
					}
				}
			}
			return result + overpickedQty;
		}

		Dictionary<string, (ZDecimal PickedQty, ZDecimal UnitsPerPack)> GetSubPartQtyPickedTotals()
		{
			var result = new Dictionary<string, (ZDecimal PickedQty, ZDecimal UnitsPerPack)>();

			foreach (var subPartWorkOrderLine in BOM.ChildComponentLines)
			{
				var subPartQtyPicked = subPartWorkOrderLine.PickLines.GetQtyCommitted();

				var bomPart = BillOfMaterials.FindByComponentPKandPackType(subPartWorkOrderLine.WE_OP, subPartWorkOrderLine.WE_F3_NKPackType);
				if (bomPart != null) // can be null if BOM definition changes before pick.
				{
					var subPart = subPartWorkOrderLine.SupplierPart;
					var unitsPerPack = subPart.UnitConverter.Convert(bomPart.OE_ComponentQty, subPartWorkOrderLine.WE_F3_NKPackType, subPart.OP_StockKeepingUnit);

					// Unique dictionary key using guid + packtype ( Handle duplicate child bom items )
					var dictKey = subPartWorkOrderLine.WE_OP.ToString() + subPartWorkOrderLine.WE_F3_NKPackType;
					if (result.TryGetValue(dictKey, out var dictValue))
					{
						result[dictKey] = (dictValue.PickedQty + subPartQtyPicked, unitsPerPack);
					}
					else
					{
						result.Add(dictKey, (subPartQtyPicked, unitsPerPack));
					}
				}
			}

			return result;
		}

		#endregion

		#region SumOfUnitsMet

		protected override ZDecimal SumOfUnitsMetCore
		{
			get
			{
				ZDecimal result;

				if (WorkOrder.IsAssembly && BOM.IsTopLevelProduct)
				{
					/// We will never pick the top level product as it is the BOM item being assembled. SumOfUnitsMet
					/// for the top level product is therefore based on how many products can be built using components
					/// (note we do not include components that require building -- they are for a separate WorkOrder).
					result = (PickableDocket.Pick != null) ? QuantityAssembled : 0;
				}
				else
				{
					result = base.SumOfUnitsMetCore;
				}

				return result;
			}
		}

		#endregion

		#region CanGenerateChildWorkOrder

		protected override bool CanGenerateChildWorkOrderCore => BOM.IsParentTopLevelProduct && IsBOMProduct;

		#endregion

		protected override bool CanOrderPalletID => false;

		#endregion

		#region Flags

		#region ShouldUpdateWeightAndVolumeOfDocketFromDocketLine

		protected override bool ShouldUpdateWeightAndVolumeOfDocketFromDocketLine(WhsDocket docket)
		{
			var workOrder = (WhsWorkOrder)docket;
			return workOrder.IsAssembly ? BOM.IsComponent && BOM.IsParentTopLevelProduct : BOM.IsTopLevelProduct;
		}

		#endregion

		#endregion

		#region BillOfMaterials

		// Tested in TestFinaliseDocket_CreatesReceive_ExcludeForVirtualWhs + bizO tests
		internal OrgPartBOMCollection BillOfMaterials
		{
			get
			{
				var result =
					InVirtualWarehouse ?? false
						? SupplierPart?.BillOfMaterialsForVirtualWarehouse
						: SupplierPart?.BillOfMaterials;

				return result;
			}
		}

		#endregion

		#region BOM Auto-Creation + Expand/Collapse

		public BOMHelper BOM
		{
			get { return bom ?? (bom = new BOMHelper(this)); }
		}

		public class BOMHelper
		{
			public BOMHelper(WhsWorkOrderLine line)
			{
				Line = line;
			}

			protected readonly WhsWorkOrderLine Line;

			#region Flags

			public bool IsComponent
			{
				get { return !Line.WE_WE_ParentDocketLine.IsEmpty; }
			}

			public bool IsTopLevelProduct
			{
				get { return Line.ParentLine == null && Line.IsBOMProduct; }
			}

			public bool IsParentTopLevelProduct
			{
				get { return Line.ParentLine?.BOM.IsTopLevelProduct ?? false; }
			}

			#endregion

			#region CreateChildComponentLines

			public void CreateChildComponentLines()
			{
				if (Line.IsBOMProduct)
				{
					// if there is no WorkOrder, then there is nothing to attach lines to. this should not happen in production code.
					var workOrder = Line.WorkOrder;
					Argument.NotNull(workOrder, "ParentLine.WorkOrder");

					// When a GUI (grid) is attached, entering the product and then pressing tab will result in
					// component lines being created in the grid. This changes the grid's current position and
					// because the existing line may not be committed, it dissapears. To prevent this, we commit
					// the parent line here.
					int index = workOrder.Lines.IndexOf(Line);
					if (index >= 0)
					{
						workOrder.Lines.EndNew(index);
					}

					var volumeUnit = workOrder.GetVolumeMeasure();
					var weightUnit = workOrder.GetWeightMeasure();
					var weight = 0m;
					var cubic = 0m;

					foreach (var bomPart in Line.BillOfMaterials)
					{
						var line = workOrder.AllLines.AddNew(); // add to AllLines because AllLines has no filter and will correctly calculate WE_LineNo's.

						line.WE_WE_ParentDocketLine = Line.PK;
						line.WE_F3_NKPackType = bomPart.OE_F3_NKPackType;
						line.WE_TransactionQuantity = GetBOMQtyForChildComponentLine(bomPart);
						line.WE_OP = bomPart.OE_OP_Component; // this will recurse and create children

						if (!Line.UpdatingTotalWeightAndVolumeSemaphore.IsSuspended && line.ShouldUpdateWeightAndVolumeOfDocketFromDocketLine(workOrder))
						{
							var lineWithProductAndQty = new LineWithProductAndQuantity(line.WE_OP, line.WE_TransactionQuantity);
							weight += UnitOfMeasureConverter.GetQuantityFromLine(workOrder, weightUnit, lineWithProductAndQty);
							cubic += UnitOfMeasureConverter.GetQuantityFromLine(workOrder, volumeUnit, lineWithProductAndQty);
						}
					}

					workOrder.WD_TotalWeight += weight;
					workOrder.WD_TotalCubic += cubic;
				}
			}

			#endregion

			#region UpdateChildComponentLineQtys

			internal void UpdateChildComponentLineQtys()
			{
				foreach (var line in ChildComponentLines)
				{
					var bomPart = Line.BillOfMaterials.FindByComponentPKandPackType(line.WE_OP, line.WE_F3_NKPackType);
					if (bomPart != null) // can be null if BOM definition changes before pick.
					{
						line.WE_TransactionQuantity = GetBOMQtyForChildComponentLine(bomPart);
					}
				}
			}

			internal ZDecimal GetBOMQtyForChildComponentLine(OrgPartBOM bomPart)
			{
				return BOMComponentQuantityHelper.GetComponentsQuantityToBuildKits(bomPart, Line.WE_TransactionQuantity);
			}

			#endregion

			#region DeleteChildComponentLines

			public void DeleteChildComponentLines()
			{
				foreach (var childComponentLine in ChildComponentLines.ToArray())
				{
					childComponentLine.WE_TransactionQuantity = 0; // need for recalculating weights					
					childComponentLine.WE_OP = ZGuid.Empty; // this will delete children of children					
					childComponentLine.Delete();
				}
			}

			#endregion

			#region Expand / Collapse Lines

			public void ToggleExpansion(bool expand)
			{
				if (Line.IsBOMProduct || Line.BOM.IsComponent)
				{
					isExpanded = expand;
					foreach (var childComponentLine in ChildComponentLines)
					{
						childComponentLine.BOM.ToggleExpansion(expand);
					}
				}
			}

			public IReadOnlyCollection<WhsWorkOrderLine> ChildComponentLines
			{
				get { return Line.Factory.Load<WhsWorkOrderLine>(new ZQuery(WhsDocketLineSchema.WE_WE_ParentDocketLine, Line.PK) { FetchOnlyFromLocalCache = !Line.IsInDatabase }); }
			}

			public ZBool IsExpanded
			{
				get { return isExpanded || !Line.BOM.IsComponent; } // if it's not a BOM component (ie. a real order line), it is always expanded
			}

			ZBool isExpanded;

			#endregion

			#region UpdateChildComponentLineShortfalls_ForUnpickedOrder

			public void UpdateChildComponentLineShortfalls_ForUnpickedOrder()
			{
				if (Line != null)
				{
					var docket = Line.PickableDocket;
					if (docket != null && docket.Pick == null && Line.SupplierPart != null)
					{
						Line.CalculateShortfallForUnpickedOrder();
						foreach (var childLine in ChildComponentLines)
						{
							childLine.CalculateShortfallForUnpickedOrder();
							childLine.BOM.UpdateChildComponentLineShortfalls_ForUnpickedOrder();
						}
					}
				}
			}

			#endregion
		}

		BOMHelper bom;

		#endregion

		#region Shortfalls

		#region AvailableReservedQty

		protected override ZDecimal AvailableReservedQtyCore => ZDecimal.Zero;

		#endregion

		#region Unpicked WorkOrder

		protected override decimal CalculateShortfallForUnpickedOrderCore()
		{
			var workOrder = WorkOrder;
			if (workOrder.Pick != null)
			{
				throw new NotSupportedException("Cannot call CalculateShortfallForUnpickedOrder() on a WhsWorkOrderLine whose parent Order has a Pick.");
			}

			decimal result;

			if (workOrder.IsAssembly)
			{
				if (BOM.IsTopLevelProduct) // never use existing stock for the WO line
				{
					var available = GetBOMProductBuildability(BillOfMaterials, workOrder);
					result = GetShortfallQtyConsideringLinesWithHigherPriorityForShortfall(WE_TransactionQuantity - available, WE_TransactionQuantity);
				}
				else if (!BOM.IsParentTopLevelProduct)
				{
					result = 0m; // we don't build components
				}
				else
				{ // one level below the top, these are the components we're using to build the BOM product...
					var parentLine = ParentLine;
					var parentPartsRequiredToBuild = parentLine.WE_TransactionQuantity;
					if (parentPartsRequiredToBuild > 0)
					{
						var partsInStock = GetProductAvailability(WE_OP, workOrder, null);
						var parentBomPart = parentLine.BillOfMaterials.FindByComponentPKandPackType(WE_OP, WE_F3_NKPackType);
						if (parentBomPart != null) // can be null if BOM definition changes before pick.
						{
							var shortfall = parentPartsRequiredToBuild * parentBomPart.OE_ComponentQty - partsInStock;
							result = parentLine.GetShortfallQtyConsideringLinesWithHigherPriorityForShortfall(shortfall, WE_TransactionQuantity, parentBomPart.OE_ComponentQty);
						}
						else
						{
							result = 0m;
						}
					}
					else
					{
						result = 0m;
					}
				}
			}
			else // disassembly
			{
				if (BOM.IsTopLevelProduct)
				{
					// check stock of top level
					result = GetShortfallQtyConsideringLinesWithHigherPriorityForShortfall(WE_TransactionQuantity - GetProductAvailability(workOrder, this), WE_TransactionQuantity);
				}
				else
				{
					// we don't ever pick component lines when disassembling
					result = 0m;
				}
			}

			return result;
		}

		ZDecimal GetProductAvailability(WhsWorkOrder workOrder, IPartAttributes line) => GetProductAvailability(WE_OP, workOrder, line);

		ZDecimal GetProductAvailability(ZGuid partPK, WhsWorkOrder workOrder, IPartAttributes line)
			=> workOrder.BOM.GetUnitsPickable(workOrder.WD_OH_Client, partPK, workOrder.WD_WW_Whs, line);

		ZDecimal GetBOMProductBuildability(OrgPartBOMCollection partBom, WhsWorkOrder workOrder)
		{
			ZDecimal result = 0m;
			if (partBom.Count > 0)
			{
				result = decimal.MaxValue;

				foreach (var orgPartBom in partBom.Where(x => x.OE_ComponentQty > 0))
				{
					var availableParts = GetProductAvailability(orgPartBom.OE_OP_Component, workOrder, null);
					var availablePartsForProduct = Math.Truncate(availableParts / orgPartBom.OE_ComponentQty);

					result = Math.Min(result, availablePartsForProduct); // get the smallest complete part count
				}
			}

			return result;
		}

		#endregion

		#region Picked WorkOrder

		/// <summary>
		/// Calculates both the Shortfall and the ItemsToBuild properties for a picked WorkOrder.
		/// </summary>
		protected override decimal CalculateShortfallForPickedOrder()
		{
			var workOrder = WorkOrder;
			if (workOrder.Pick == null)
			{
				throw new NotSupportedException("Cannot call CalculateShortfallForPickedOrder() on a WhsWorkOrderLine whose parent Order has no Pick.");
			}

			decimal result;

			if (workOrder.IsAssembly)
			{
				if (BOM.IsTopLevelProduct)
				{
					result = Math.Max(0m, WE_TransactionQuantity - QuantityAssembled);
				}
				else
				{
					result = Math.Max(0m, QuantityNotPicked);
				}
			}
			else
			{
				if (BOM.IsTopLevelProduct)
				{
					result = Math.Max(0m, QuantityNotPicked);
				}
				else
				{
					result = 0m; // we never pick components for disassembly
				}
			}

			return result;
		}

		#endregion

		#endregion

		#region Delete

		public override bool CanDelete
		{
			get { return base.CanDelete && (!WE_OP.IsValid || BOM.IsTopLevelProduct); } // user should not be able to delete components
		}

		public override void Delete()
		{
			BOM.DeleteChildComponentLines();
			base.Delete();
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get { return ResString.GetMultilingualString("ba7149cb-29f4-444d-a07c-493b983673c7", "Bill of Materials Component lines cannot be deleted."); }
		}

		#endregion

		#region BOA/BOD eDoc Printing Support

		public DocManagerInfo SupplierPartDocManagerInfo
		{
			get
			{
				if (supplierPartDocManagerInfo == null)
				{
					ZString docType = WorkOrder.WD_DocketSubType == WorkOrderType.Codes.Disassemble ? "BOD" : "BOA";
					supplierPartDocManagerInfo = new DocManagerInfo(SupplierPart, docType);
				}
				return supplierPartDocManagerInfo;
			}
		}
#if DEBUG
		internal
#endif
		DocManagerInfo supplierPartDocManagerInfo;

		internal void ResetSupplierPartDocManagerInfo()
		{
			supplierPartDocManagerInfo = null;
		}

		#endregion

		#region ISupportDataImporting Members

		protected override bool IsImportingDataCore
		{
			get
			{
				var topMostParent = ParentLine_TopMost;
				return (topMostParent != null) ? topMostParent.IsImportingData : base.IsImportingDataCore;
			}
			set { base.IsImportingDataCore = value; }
		}

		WhsWorkOrderLine ParentLine_TopMost
		{
			get
			{
				var parent = ParentLine;

				while (parent != null)
				{
					var nextParent = parent.ParentLine;
					if (nextParent == null)
					{
						return parent;
					}
					parent = nextParent;
				}

				return null;
			}
		}

		#endregion

		#region ProductDefinitionDoesNotMatchDocketLineProductDefinition

		internal bool ProductDefinitionDoesNotMatchDocketLineProductDefinition()
		{
			return BOM.ChildComponentLines.Any(c => !BillOfMaterials.Any(b => b.OE_OP_Component == c.WE_OP && b.OE_F3_NKPackType == c.WE_F3_NKPackType && c.WE_TransactionQuantity == BOM.GetBOMQtyForChildComponentLine(b)));
		}

		#endregion
	}
}
