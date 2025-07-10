using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsOrderLineValidation : WhsPickableDocketLineValidation
	{
		public WhsOrderLineValidation(WhsOrderLine parent)
			: base(parent)
		{
		}

		#region Parent

		protected new WhsOrderLine Parent
		{
			get { return (WhsOrderLine)base.Parent; }
		}

		#endregion

		// Calculated

		#region ValidatePickGroup

		protected override void CheckPickGroupForBinding()
		{
			base.CheckPickGroupForBinding();
			CheckIfValidPickGroup();
		}

		void CheckIfValidPickGroup()
		{
			if (!Parent.WE_PickGroup.IsEmpty && !Parent.Lookups.PickGroups.ContainsCode(Parent.WE_PickGroup))
			{
				var errorMessage = ListValidation.GetNotificationMessage(Parent.WE_PickGroupInfo).ToString();
				Parent.PickGroupForBindingInfo.AddError(errorMessage);
			}
		}

		#endregion

		#region CheckWE_PalletID

		protected override void CheckWE_PalletID()
		{
			base.CheckWE_PalletID();
			if (!Parent.WE_PalletID.IsEmpty && Parent.WE_OP.IsValid)
			{
				var product = WhsProduct.GetWhsProduct(Factory, Parent.WE_OP);
				var docket = Parent.Docket;
				var productHasPickFace = product.PickFaces.HasPickFace(docket.WD_OH_Client, Parent.WE_OP, docket.WD_WW_Whs);
				if (productHasPickFace)
				{
					Parent.WE_PalletIDInfo.AddError(Res.GetString("D67C7677-E146-4E48-B8EF-7905597E77FE", "Product {0} has Fixed Pick Face configured. No Pallet ID entry allowed.", Parent.SupplierPart.OP_PartNum));
				}
				else
				{
					var productParams = docket.GetProductParams(product);
					if (productParams != null && !productParams.W3_WA_DynamicPickFaceArea.IsEmpty)
					{
						Parent.WE_PalletIDInfo.AddError(Res.GetString("78BC086B-0E0A-4B82-A367-1444849AC268", "Product {0} has Dynamic Pick Face Area configured. No Pallet ID entry allowed.", Parent.SupplierPart.OP_PartNum));
					}
				}
			}
		}

		#endregion

		// Persistent

		#region CheckWE_RX_NKUnitPriceCurrency

		protected override void CheckWE_RX_NKUnitPriceCurrency()
		{
			base.CheckWE_RX_NKUnitPriceCurrency();
			if (!Parent.WE_RX_NKUnitPriceCurrency.IsEmpty && Parent.UnitPriceCurrency == null)
			{
				Parent.WE_RX_NKUnitPriceCurrencyInfo.AddError(Res.GetString("843a5572-2735-4648-8142-6995c2155f72", "Please enter a valid currency."));
			}
		}

		#endregion

		#region CheckWE_ShortfallQuantityCached

		protected override void CheckWE_ShortfallQuantityCached()
		{
			base.CheckWE_ShortfallQuantityCached();

			if (!Parent.IsDeleted && Parent.Product != null && Parent.WE_ShortfallQuantityCachedInfo.HasWarning(ShortfallWarning))
			{
				Parent.Docket.Validation.ValidateWD_WhsOrderFulfillmentRule();
			}
		}

		protected override string ShortfallWarning
		{
			get
			{
				var docket = Parent.PickableDocket;
				return
					Res.GetString("82f37bbe-a88e-47be-abe1-437b88f8eb87", "Shortfall: Only") + " " +
					(Parent.WE_TransactionQuantity - Parent.WE_ShortfallQuantityCached).ToString(Parent.Product.QtyToStringFormat, Culture.Current) + " "
					+ Res.GetString("71866214-a62d-4562-a4ab-fdc6f4957034", "unit(s)") + " " +
					((docket == null || docket.Pick == null) ? Res.GetString("6ba58698-6e41-4107-b60b-44cec1287055", "currently available") : Res.GetString("0e7137e5-7b59-4dd8-8893-741621e3943a", "have been selected for release"));
			}
		}

		#endregion

		#region CheckWE_TransactionQuantity

		protected override void CheckWE_TransactionQuantity()
		{
			base.CheckWE_TransactionQuantity();

			var order = Parent.Order;
			CheckWE_TransactionQuantityDoesNotGoBelowReservedAmount();
			CheckWE_OrderLineIsNotForBOMProduct(order);

			if (!Parent.WE_TransactionQuantityInfo.HasErrors() && Parent.IsInDatabase && Parent.WE_TransactionQuantityInfo.HasChanges)
			{
				var part = Parent.SupplierPart;
				CheckWE_TransactionQuantityDoesNotGoBelowPackedQty();
				CheckWE_TransactionQuantityDoesNotAllowTooLargeTotalLineWeight(order, part);
				CheckWE_TransactionQuantityDoesNotAllowTooLargeVolume(order, part);
			}
		}

		void CheckWE_TransactionQuantityDoesNotGoBelowReservedAmount()
		{
			if (!Parent.WE_TransactionQuantityInfo.HasErrors() && Parent.IsInDatabase)
			{
				var reservedQuantity = Parent.WE_CrossDockQuantity;
				if (Parent.WE_TransactionQuantity < reservedQuantity)
				{
					Parent.WE_TransactionQuantityInfo.AddError(Res.GetString("0c206523-7a37-4643-917e-6d9e74cf50b0", "This Order Line has {0} units of Cross Docked Inventory, you cannot set the Quantity less than this.", reservedQuantity));
				}
			}
		}

		void CheckWE_OrderLineIsNotForBOMProduct(WhsOrder order)
		{
			if (Parent.IsBOMProductPickedOnSalesOrder && (order?.WD_WP.IsValid ?? false))
			{
				if (Parent.WE_TransactionQuantityInfo.HasChanges)
				{
					Parent.WE_TransactionQuantityInfo.AddError(Res.GetString("0c377fb4-cf6d-4acf-81b0-54659735b4e6", "This is a BOM product that is setup so that the components are picked on the sales order. The quantity ordered cannot be changed once the pick is created."));
				}
			}
		}

		void CheckWE_TransactionQuantityDoesNotGoBelowPackedQty()
		{
			var packedQty = Parent.PickLines.Sum(pl => pl.GetPackedQty(Factory));
			if (Parent.WE_TransactionQuantity < packedQty)
			{
				Parent.WE_TransactionQuantityInfo.AddError(Res.GetString("0c106523-2a55-4641-917b-1d9e74cf50b0", "This Order Line has {0} units packed, you cannot set the Quantity less than this.", packedQty));
			}
		}

		void CheckWE_TransactionQuantityDoesNotAllowTooLargeTotalLineWeight(WhsOrder order, OrgSupplierPart part)
		{
			if (part != null && order != null)
			{
				var weightMeasure = order.GetWeightMeasure();
				ZDecimal changeInUnits = (Parent.WE_TransactionQuantity - (ZDecimal)Parent.WE_TransactionQuantityInfo.OriginalValue);
				ZDecimal changeInWeight = UnitOfMeasureConverter.GetQuantityFromLine(Parent, weightMeasure, new LineWithProductAndQuantity(Parent.WE_OP, changeInUnits));
				ZDecimal newTotalWeight = changeInWeight + (ZDecimal)order.WD_TotalWeightInfo.OriginalValue;

				if ((changeInWeight > 0) && !newTotalWeight.IsWithinSqlPrecisionAndScale(9, 3))
				{
					Parent.WE_TransactionQuantityInfo.AddError(Res.GetString("FEDD3681-C43F-4616-84A6-33E6A9A682AC", "The number {0} will make the Warehouse Order {1}'s Total Line Weight too large, the current value is {2}, and the maximum value is {3}.", Parent.WE_TransactionQuantity.ToString("n3", Culture.Current), order.WD_ExternalReference, newTotalWeight.ToString("n3", Culture.Current), "999,999.999"));
				}
			}
		}

		void CheckWE_TransactionQuantityDoesNotAllowTooLargeVolume(WhsOrder order, OrgSupplierPart part)
		{
			if (part != null && order != null)
			{
				var volumeMeasure = order.GetVolumeMeasure();
				ZDecimal changeInUnits = (Parent.WE_TransactionQuantity - (ZDecimal)Parent.WE_TransactionQuantityInfo.OriginalValue);
				ZDecimal changeInVolume = UnitOfMeasureConverter.GetQuantityFromLine(Parent, volumeMeasure, new LineWithProductAndQuantity(Parent.WE_OP, changeInUnits));
				ZDecimal newTotalVolume = changeInVolume + (ZDecimal)order.WD_TotalCubicInfo.OriginalValue;

				if ((changeInVolume > 0) && !newTotalVolume.IsWithinSqlPrecisionAndScale(9, 3))
				{
					Parent.WE_TransactionQuantityInfo.AddError(Res.GetString("45869E33-3DAD-4E18-8163-FCF8DB97FD63", "The number {0} will make the Warehouse Order {1}'s Total Line Volume too large, the current value is {2}, and the maximum value is {3}.", Parent.WE_TransactionQuantity.ToString("n3", Culture.Current), order.WD_ExternalReference, newTotalVolume.ToString("n3", Culture.Current), "999,999.999"));
				}
			}
		}

		#endregion

		#region CheckWE_OP

		protected override void CheckWE_OP()
		{
			base.CheckWE_OP();
			var part = Parent.SupplierPart;
			if (part != null && !Parent.ReadOnly)
			{
				CheckProductHasPalletDefinition(part);
				CheckCanResell(part);
				CheckWE_OPShouldNotBeChangedIfThisOrderLineHasReservedInventory();
			}
		}

		void CheckProductHasPalletDefinition(OrgSupplierPart part)
		{
			var docket = Parent.Docket;
			if ((docket == null || !docket.IsFinalising) && part.OP_StockKeepingUnitPerPallet == 0m)
			{
				Parent.WE_OPInfo.AddWarning(ProductHasNoPalletDefinitionError);
			}
		}

		void CheckCanResell(OrgSupplierPart part)
		{
			if (!part.OP_CanResell)
			{
				Parent.WE_OPInfo.AddError(OrgSupplierPartCollection.ProductIsNotForResaleErrorMessage);
			}
		}

		void CheckWE_OPShouldNotBeChangedIfThisOrderLineHasReservedInventory()
		{
			if (!Parent.WE_OPInfo.HasErrors() && Parent.IsInDatabase && !Parent.WE_OPInfo.OriginalValue.Equals(Parent.WE_OP) && Parent.ReservedPickLines.Count > 0)
			{
				Parent.WE_OPInfo.AddError(Res.GetString("3cacf022-7236-435c-b1bc-ef09a033d39e", "There is Inventory Cross Docked to this Order Line, you cannot change the Product."));
			}
		}

		public static string ProductHasNoPalletDefinitionError
		{
			get { return Res.GetString("f13727f2-2448-41d0-95db-e295f931c7de", "This product does not have a pallet unit defined. This means the pick system cannot manage palletized goods as efficiently as it might."); }
		}

		#endregion

		#region CheckWE_WHC_NKOrderedHeldCode

		protected override void CheckWE_WHC_NKOrderedHeldCode()
		{
			if (Parent.Docket != null && !Parent.Docket.WD_UseDirectedPackingConsolidation)
			{
				CheckInvalidHeldCode();
				CheckOrderContainsMixedInventory(Parent.WE_WHC_NKOrderedHeldCode.IsEmpty);
				CheckPickDoesNotContainMixedInventory(Parent.Order);
			}
			else
			{
				MandatoryValidation.CheckNotEntered(Parent.WE_WHC_NKOrderedHeldCodeInfo);
			}
		}

		void CheckInvalidHeldCode()
		{
			if (!Parent.WE_WHC_NKOrderedHeldCode.IsEmpty)
			{
				if (Parent.WE_WHC_NKOrderedHeldCode == InventoryHoldCodes.Codes.ShortPicked || Parent.WE_WHC_NKOrderedHeldCode == InventoryHoldCodes.Codes.LostInCycleCount)
				{
					Parent.WE_WHC_NKOrderedHeldCodeInfo.AddError(Res.GetString("c2ef9f61-715b-426b-977d-f17a005b8faa", "{0} hold code may not be selected.", Parent.WE_WHC_NKOrderedHeldCode));
				}
				else
				{
					ListValidation.ErrorIfInvalidCode(Parent.WE_WHC_NKOrderedHeldCodeInfo);
				}
			}
		}

		void CheckOrderContainsMixedInventory(bool isHeldCodeEmpty)
		{
			var validationCache = Parent.Order.PreSaveValidationCache;
			if (validationCache != null && ((!isHeldCodeEmpty && validationCache.LinesContainAvailableInventory()) || (isHeldCodeEmpty && validationCache.LinesContainHeldInventory())))
			{
				Parent.WE_WHC_NKOrderedHeldCodeInfo.AddError(Res.GetString("6b226d5b-2830-4e90-8c9c-90ab291d3703", "All lines must be for either held or available inventory."));
			}
		}

		void CheckPickDoesNotContainMixedInventory(WhsOrder updatedOrder)
		{
			if (updatedOrder.Pick != null && updatedOrder.Pick.PickContainsMixedInventory(updatedOrder))
			{
				Parent.WE_WHC_NKOrderedHeldCodeInfo.AddError(Res.GetString("e974dc89-c419-4691-8618-a4a329198acd", "{0}", PickErrorTypes.PickContainsMixedInventoryOrders.Message));
			}
		}

		#endregion

		#region Attributes

		#region IsJulianBatchNumberFormatValidationRequired

		protected override bool IsJulianBatchNumberFormatValidationRequired(ZPropertyInfo partAttributeInfo, int attributeNumber)
		{
			return !partAttributeInfo.Value.IsEmpty;
		}

		#endregion

		#region IsReleaseCapturedValidationRequired

		protected override bool IsReleaseCapturedValidationRequired => true;

		#endregion

		#endregion

		#region ValidStatuses

		protected override IEnumerable<ZString> ValidStatuses => validStatuses.Value;

		readonly Lazy<IEnumerable<ZString>> validStatuses = new Lazy<IEnumerable<ZString>>(() => new ZString[] { string.Empty, DocketLineStatus.Codes.Finalised, DocketLineStatus.Codes.Departed, DocketLineStatus.Codes.Cancelled });

		#endregion

		#region ValidateAll

		public override void ValidateAll()
		{
			base.ValidateAll();
			CheckIsNewLineOnLoadingOrLoadedOrDepartedOrder();
		}

		void CheckIsNewLineOnLoadingOrLoadedOrDepartedOrder()
		{
			if (!Parent.IsInDatabase)
			{
				var order = (WhsOrder)Parent.Docket;
				if (order != null && !CanOrderHaveNewLinesAfterFinalization(order) && order.IsLoadingOrLoadedOrDeparted)
				{
					Parent.AddRowError(Res.GetString("b224c4d1-ac87-44d9-8d0a-f31aa0f93ca2", "Cannot add Line to an Order which is Loading, Loaded or Departed."));
				}
			}
		}

		bool CanOrderHaveNewLinesAfterFinalization(WhsOrder order)
		{
			return order.IsCustomsTransaction && order.Warehouse.WW_IsVirtualWarehouse && (order.Pick?.IsFinalised ?? false);
		}

		#endregion
	}
}
