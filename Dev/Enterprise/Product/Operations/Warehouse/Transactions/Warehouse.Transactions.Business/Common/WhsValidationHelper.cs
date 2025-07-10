using System;
using System.Collections.Generic;
using System.Linq;
#if NETFRAMEWORK
using CargoWise.Common;
#endif
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.ResourceStrings.Grammar;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.US;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public static class WhsValidationHelper
	{
		#region Messages

		public static ZString ValueHasToBeTrimmed => Res.GetString("AEFDA0BC-A2EB-4FF7-A873-5AA1E189B45E", "This value cannot begin or end with white-spaces.");
		public static ZString UnfulfilledCrossDockedUnitsError => Res.GetString("e7122dc9-a342-4126-b943-92f7be06fe5e", "This Inventory has Cross Docked Units, you must set Expected and/or Transaction Quantity equal to or greater than the Reserved Quantity.");

		public static ZString GetDuplicatePalletIdMessage(WhsLocation location) => GetDuplicatePalletIdMessage(location.WLV_LocationString);
		public static ZString GetDuplicatePalletIdMessage(ZString locationString) => Res.GetString("412caa02-c9fa-4e25-aa83-e3d9ba237d10",
										"Another location ({0}) was already used for the same Pallet ID. Please select another location or Pallet ID.", locationString);

		#endregion

		#region CheckIfTrimIsNeeded

		public static void CheckIfTrimIsNeeded(ZPropertyInfo propertyInfo)
		{
			var propertyValue = (ZString)propertyInfo.Value;
			if (!propertyValue.Equals(propertyValue.Trim()))
			{
				propertyInfo.AddError(ValueHasToBeTrimmed);
			}
		}

		#endregion

		#region GetNotEnoughStockMessage

		public static string GetNotEnoughStockMessage(ILineWithCommittedPickLines transactionLine, ZDecimal quantityInStock)
		{
			var docket = transactionLine.ParentDocket;
			var client = docket.Client;
			var product = transactionLine.Product;

			var notEnoughStockMessageBuilder = new ZStringBuilder();
			var availableStock = GetAvailableStockMessage(transactionLine, product, quantityInStock);
			notEnoughStockMessageBuilder.Append(availableStock);
			notEnoughStockMessageBuilder.Append(Res.GetString("831bbb78-4f95-4e14-b4b1-42e2d88a417a", "Check the location is correct, and check the arrival date, or try leaving the arrival date empty on the {0} line.", transactionLine.Noun));

			if (product.IsAnyAttributeUsed(client) || IsAnyCustomAttributeUsed(docket, client))
			{
				notEnoughStockMessageBuilder.Append(Res.GetString("9d348852-8e08-4e10-a2ca-a44fcda674ce", "Check that all the attributes exactly match the attributes on the inventory you are trying to {0}.", transactionLine.Verb));
				notEnoughStockMessageBuilder.Append(Res.GetString("530addc9-29d7-4717-bbdb-d44ad527c829", "If you are trying to {0} stock with attributes, you must enter the attribute exactly. Blank non mandatory attributes will only match to inventory with blank attributes.", transactionLine.Verb));
			}

			notEnoughStockMessageBuilder.Append(Res.GetString("6fcad0b7-e990-4e31-8938-5aebc3ffbe35", "Check that all the Pallet IDs exactly match the Pallet IDs on the inventory you are trying to {0}.", transactionLine.Verb));
			notEnoughStockMessageBuilder.Append(Res.GetString("a7594ca6-9f07-4c3a-9b27-0bcd89958d03", "If you are trying to {0} stock with Pallet IDs you must enter the Pallet ID exactly. Blank Pallet IDs will only match to inventory with blank Pallet IDs.", transactionLine.Verb));
			notEnoughStockMessageBuilder.Append(Res.GetString("D5FA408C-013C-4F50-AA4C-95710923AA58", "If you are trying to {0} stock from a bonded location, you must enter the correct Customs Entry Number and Entry Line Number.", transactionLine.Verb));

			if (IsUSBondedTransaction(transactionLine))
			{
				notEnoughStockMessageBuilder.Append(Res.GetString("6f4e7a2a-3cdf-4eee-ab3a-df90098cbbac",
					"If you are trying to {0} stock with a Package Group ID you must enter it exactly. Blank Package Group IDs will only match to inventory with blank Package Group IDs.", transactionLine.Verb));
			}

			return notEnoughStockMessageBuilder.ToStringWithNewLineBetweenAppends();
		}

		static ZString GetAvailableStockMessage(ILineWithCommittedPickLines transactionLine, WhsProduct product, ZDecimal quantityInStock)
		{
			var result = new ZStringBuilder();

			var transactionQty = transactionLine.CommittedStrategy.TotalTransactionQty;
			result.Append(Res.GetString("9527c7a3-61ce-450b-bc43-9e2daa64cfa1", "Attempted to {0} {1}, but", transactionLine.Verb, product.FormattedQtyAndUnit(transactionQty)));

			if (quantityInStock > 0)
			{
				result.Append(Res.GetString("28e7eae1-b5d2-4cb7-98c6-e7a5f594cb2d", "only"));
				result.Append(product.FormattedQtyAndUnit(quantityInStock));
			}
			else
			{
				var supplierPart = product.Parent;
				result.Append(Res.GetString("b9cd2256-e651-4aba-9bd0-868f5377370e", "no"));

				ZString unit = supplierPart.Lookups.OP_ProductUQ_List.GetDescriptionFromCode(supplierPart.OP_StockKeepingUnit);
				result.Append(Grammar.Instance.Pluralize(unit));
			}

			result.Append(Res.GetString("edb2b10e-e3b4-4791-bc68-ddd0524a0137", "are available for {0} out of this location.", transactionLine.Noun));

			return result.ToStringWithDelimiterBetweenAppends(" ");
		}

		static bool IsAnyCustomAttributeUsed(WhsDocket docket, OrgHeader client)
		{
			var customFields = new WhsDocketLine.CustomLabelsProvider(docket).GetCustomFields(client, client.Factory);
			foreach (OrgCustomLabels label in client.CustomLabels)
			{
				if (customFields.Cast<CustomLabelInfo>().Any(l => l.PropertyName == label.OT_FieldName))
				{
					return true;
				}
			}
			return false;
		}

		static bool IsUSBondedTransaction(ILineWithCommittedPickLines transactionLine)
		{
			var docket = transactionLine.ParentDocket;
			return docket != null && docket.IsUSBonded;
		}

		#endregion

		#region GetOrgSupplierPartErrorMessage

		// Tested by consuming validation classes (Inventory, docket, stocktake, VAS Orders)
		public static void CheckProductIsValid(OrgSupplierPart product, ZGuid clientPK, ZPropertyInfo productInfo, bool showInactiveErrorAsWarning = false)
		{
			if (product != null)
			{
				CheckProductRelationships(product, clientPK, productInfo);
				CheckForInvalidProduct(product, productInfo, showInactiveErrorAsWarning);
			}
		}

		static void CheckProductRelationships(OrgSupplierPart part, ZGuid clientPK, ZPropertyInfo productInfo)
		{
			if (part.RelatedOrganisations != null)
			{
				if (part.RelatedOrganisations.FindByOrganisationPKAndRelationship(clientPK, OrgPartRelation.RelationshipTypes.Owner) == null)
				{
					productInfo.AddError(ProductRelationshipsErrorMessage);
				}
			}
		}

		internal static string ProductRelationshipsErrorMessage => Res.GetString("F83D292D-76D6-4637-B2CE-89ECF7F11A89", @"This product is not related to the client. Products must have an Owner Relationship for the client");

		static void CheckForInvalidProduct(OrgSupplierPart part, ZPropertyInfo productInfo, bool showInactiveErrorAsWarning)
		{
			if (part.OP_PartNum == ProductType.Codes.Invalid)
			{
				productInfo.AddError(OrgSupplierPartCollection.InvalidProductErrorMessage);
			}
			else if (!part.OP_IsActive)
			{
				if (!showInactiveErrorAsWarning)
				{
					productInfo.AddError(OrgSupplierPartCollection.ProductIsNotActiveErrorMessage);
				}
				else
				{
					productInfo.AddWarning(Res.GetString("eecb9799-4a54-43b0-ae28-009fbb9578a5", "This Product is Inactive"));
				}
			}
		}

		#endregion

		#region CheckProductHasWeightDefinition

		// tested in WhsReceiveLineValidation and WhsInventoryViewValidation
		public static void CheckProductHasWeightDefinition(OrgSupplierPart part, ZPropertyInfo productInfo)
		{
			if (part.OP_Weight == 0)
			{
				productInfo.AddWarning(ProductHasNoWeightDefinitionError);
			}
		}

		public static ZString ProductHasNoWeightDefinitionError
		{
			get { return Res.GetString("b063286f-9b29-4a28-a9f4-8ea66bfce8fe", "This product does not have a weight defined. This means the putaway cannot check if the location weight is exceeded."); }
		}

		#endregion

		#region CheckProductHasCubicDefinition

		// tested in WhsReceiveLineValidation and WhsInventoryViewValidation
		public static void CheckProductHasCubicDefinition(OrgSupplierPart part, ZPropertyInfo productInfo)
		{
			if (part.OP_Cubic == 0)
			{
				productInfo.AddWarning(ProductHasNoVolumeDefinitionError);
			}
		}

		public static ZString ProductHasNoVolumeDefinitionError
		{
			get { return Res.GetString("ab64e506-bcbc-4fb3-912b-844efd1a04b7", "This product does not have a volume defined. This means the putaway cannot check if the location volume is exceeded."); }
		}

		#endregion

		#region CheckProductHasPalletDefinition

		// tested in WhsReceiveLineValidation and WhsInventoryViewValidation
		public static void CheckProductHasPalletDefinition(OrgSupplierPart part, ZPropertyInfo productInfo)
		{
			if (part.OP_StockKeepingUnitPerPallet == 0)
			{
				productInfo.AddWarning(ProductHasNoPalletDefinitionError);
			}
		}

		public static ZString ProductHasNoPalletDefinitionError
		{
			get { return Res.GetString("0b284f74-6419-4ae5-af0f-2315c809a421", "This product does not have a pallet unit defined. This may cause inefficiencies with putting away palletized goods."); }
		}

		#endregion

		#region CheckProductShouldNotBeChangedIfInventoryIsReserved

		// tested in WhsReceiveLineValidation and WhsInventoryViewValidation
		public static void CheckProductShouldNotBeChangedIfInventoryIsReserved(bool isInDatabase, ZPropertyInfo productInfo, IReservableInventory reservableInventory)
		{
			if (!productInfo.HasErrors() && isInDatabase && reservableInventory != null && reservableInventory.ReservedPickLines.Count > 0)
			{
				productInfo.AddError(CannotChangeCrossDocketInventoryProduct);
			}
		}

		public static ZString CannotChangeCrossDocketInventoryProduct
		{
			get { return Res.GetString("0af61b5f-107f-4f75-9664-6b828584e20b", "This Inventory is Cross Docked, you cannot change the Product."); }
		}

		#endregion

		#region CheckForTempProduct

		// tested in WhsReceiveLineValidation and WhsInventoryViewValidation
		public static void CheckForTempProduct(ISupportTemporaryProduct bizO, ZPropertyInfo productInfo)
		{
			if (bizO != null && bizO.IsTemporaryProduct)
			{
				productInfo.AddWarning(NoPartFoundWithThisCode);
			}
		}

		public static ZString NoPartFoundWithThisCode
		{
			get { return Res.GetString("17ef1c10-8469-41fa-950f-46a27cf6b4d4", "No Product Found with this code. Either select a valid Product (F4) or this will be treated as a new Product."); }
		}

		#endregion

		#region CheckForProductWarningMessage

		// tested in WhsReceiveLineValidation and WhsInventoryViewValidation
		public static void CheckForProductWarningMessage(IExternalValidationMessages bizO, ZPropertyInfo productInfo)
		{
			if (bizO != null && !bizO.ValidationProductWarningMessage.IsEmpty)
			{
				productInfo.AddWarning(bizO.ValidationProductWarningMessage);
			}
		}

		#endregion

		#region CheckCommodityCode

		public static void CheckCommodityCode(WhsDocket docket, ZPropertyInfo propertyInfo)
		{
			var bizO = propertyInfo.BizObj;
			var supportTemporaryProductBizO = bizO as ISupportTemporaryProduct;
			if (supportTemporaryProductBizO != null && supportTemporaryProductBizO.IsTemporaryProduct)
			{
				if (!supportTemporaryProductBizO.CommodityCode.IsEmpty)
				{
					ListValidation.ErrorIfInvalidCode(propertyInfo);
				}
				CheckJobHasRecordWithSameProductCodeButDifferentPropertyValue(docket, propertyInfo);
			}
		}

		#endregion

		#region CheckStockKeepingUnits

		public static void CheckStockKeepingUnits(WhsDocket docket, ZPropertyInfo propertyInfo)
		{
			var bizO = propertyInfo.BizObj;
			var supportTemporaryProductBizO = bizO as ISupportTemporaryProduct;
			if (supportTemporaryProductBizO != null && supportTemporaryProductBizO.IsTemporaryProduct)
			{
				if (supportTemporaryProductBizO.ProductUQ.IsEmpty)
				{
					propertyInfo.AddWarning(WhsDocketLineValidation.ThisFieldIsRequiredToAutoCreateAProductWarning);
				}
				ListValidation.ErrorIfInvalidCode(propertyInfo);
				CheckJobHasRecordWithSameProductCodeButDifferentPropertyValue(docket, propertyInfo);
			}
		}

		#endregion

		#region CheckSplitQuantity

		public static void CheckSplitQuantity(ZPropertyInfo propertyInfo, ZDecimal docketLineUnits)
		{
			var splitQuantity = (ZDecimal)propertyInfo.Value;
			if (splitQuantity < 0m)
			{
				propertyInfo.AddError(Res.GetString("dad6d542-232e-4ce2-931b-97f5af17f0fb", "Split quantity cannot be negative."));
			}
			else if (splitQuantity > docketLineUnits)
			{
				propertyInfo.AddError(Res.GetString("019ee095-efd1-40b0-82ad-2dfd634d8238", "Split quantity cannot be greater than the Line Quantity."));
			}
		}

		#endregion

		#region CheckWE_PalletID

		public static void CheckPalletID(ZPropertyInfo propertyInfo, ZGuid locationPK, WhsDocket docket, WhsDocketLine docketLine, ZString palletMessage)
		{
			CheckIfTrimIsNeeded(propertyInfo);

			if (!palletMessage.IsEmpty)
			{
				propertyInfo.AddWarning(palletMessage);
			}

			var palletID = (ZString)propertyInfo.Value;
			if (!palletID.IsEmpty)
			{
				if (docket != null)
				{
					// Pallet Id cannot be in many location
					if (locationPK.IsValid)
					{
						// transfer is finalising each line separatedly. So we cannot run this check until all lines are finalised or it is going to fail every time we have > 1 line for same pallet.
						var transferLine = docketLine as WhsTransferLine;
						var isTransferLineAndStillFinalising = (transferLine != null && transferLine.IsFinalising);

						bool isNotFinalisedOrFinalisedButNotSaved = (!docket.IsFinalised || !docket.IsInDatabase);

						if (!isTransferLineAndStillFinalising && isNotFinalisedOrFinalisedButNotSaved)// inventory might have been transferred after finalisation, in which case don't run this check.
						{
							var warehouse = docket.Warehouse;
							if (warehouse != null)
							{
								var locations = PalletIDLocationValidationCacheManager.GetUniqueLocationsWithStockForPalletID(docketLine.Factory, warehouse.PK, palletID);
								var otherLocationWithID = locations.FirstOrDefault(l => !l.PK.Equals(locationPK));
								if (otherLocationWithID != null)
								{
									propertyInfo.AddError(GetDuplicatePalletIdMessage(otherLocationWithID));
								}
							}
						}
					}
				}
			}
		}

		// Tested in PalletIDLocationValidationCacheManagerTest used there and in fetch hints in other files.
		public static ZQuery GetLocationsWithStockIncludingNotYetFinalisedQuery(ZString palletID)
		{
			var query = new ZQuery(WhsDocketLineSchema.WE_StockOnHand, SQLComparisonOperator.GreaterThan, 0m);
			query.AddToFilter(WhsDocketLineSchema.WE_PalletID, SQLComparisonOperator.NotEqual, ZString.Empty);
			query.AddToFilter(WhsDocketLineSchema.WE_PalletID, palletID);
			query.AddToFilter(WhsDocketLineSchema.WE_WL, SQLComparisonOperator.NotEqual, null);
			return query;
		}

		#endregion

		#region CheckDocketLineUnits

		public static void CheckInDocketLineUnits(ZPropertyInfo propertyInfo)
		{
			var docketLineUnits = (ZDecimal)propertyInfo.Value;
			if (docketLineUnits < 0)
			{
				propertyInfo.AddError(Res.GetString("d9e8dc27-9758-4347-a9bc-fca3a7e1135b", "Quantity must be greater than or equal to zero. Otherwise delete this row"));
			}
		}

		#endregion

		#region CheckCrossDockedUnits

		public static void CheckCrossDockedUnits(ZPropertyInfo propertyInfo, ZDecimal otherPossibleCrossDockQuantitySource, ZDecimal reservedQuantity, bool isInDatabase)
		{
			if (!propertyInfo.HasErrors() && isInDatabase)
			{
				var value = (ZDecimal)propertyInfo.Value;
				if (value >= otherPossibleCrossDockQuantitySource && value < reservedQuantity)
				{
					propertyInfo.AddError(UnfulfilledCrossDockedUnitsError);
				}
			}
		}

		#endregion

		#region CheckQtyForSerialNumber

		public static void CheckQtyForSerialNumber(PartAttributeValidation partAttributeValidation, ZPropertyInfo propertyInfo, ZPropertyInfo serialNumberInfo, WhsReceive receive, WhsProduct product)
		{
			// Tested in WhsReceiveLineValidationTest TestCheckWE_TransactionQuantitySerialNumberQtyCheck
			if (!propertyInfo.HasErrors()
				&& (receive.IsFinalising || ((IBusinessObjectInternals)propertyInfo.BizObj).IsInPreSaveValidation))
			{
				partAttributeValidation.CheckQtyForSerialNumber(product, receive.Client, propertyInfo, serialNumberInfo);
			}
		}

		#endregion

		#region CheckLocation

		#region GetLocationCapacityInfo

		internal static LocationWithCapacity GetLocationCapacityInfo(WhsLocation location, ZGuid? docketPKToExclude)
		{
			LocationWithCapacity result = null;
			var rawSql = @"
SELECT WL_PK, AvailableWeight, AvailableVolume, AvailableUnits
FROM dbo.WhsLocation 
	CROSS APPLY(SELECT AvailableWeight, AvailableVolume, AvailableUnits FROM dbo.WhsLocationCapacity(WL_PK, @DocketToExclude)) as Capacity
WHERE WL_PK = @LocationPK
";
			var sqlResults = new DynamicBusinessObjectCollection(location.Factory);
			sqlResults.Load(rawSql, new[]
			{
				ZSqlParameter.New("@LocationPK", location.PK, WhsAreaSchema.PK),
				ZSqlParameter.New("@DocketToExclude", docketPKToExclude, WhsDocketSchema.WD_OH_Forwarder), // I have to use nullable GUID as a schema column to make some exiting dodge tests to pass when PK is Empty
			});
			var sqlResult = sqlResults.FirstOrDefault();
			if (sqlResult == null)
			{
				result = new LocationWithCapacity(location, 0, 0, 0); // should happen only in tests where location is not saved to DB prior this check
			}
			else
			{
				result = ReadFromDynamicBizo(location, sqlResult);
			}
			return result;
		}

		internal static List<LocationWithCapacity> GetLocationCapacityInfoForNormalLocations(WhsArea area)
		{
			var rawSql = @"
SELECT WL_PK, AvailableWeight, AvailableVolume, AvailableUnits
FROM dbo.WhsLocation 
	CROSS APPLY(SELECT AvailableWeight, AvailableVolume, AvailableUnits FROM dbo.WhsLocationCapacity(WL_PK, NULL)) as Capacity
WHERE WL_WA_PickingArea = @AreaPK
	AND WL_LocationStatus = @NormalLocationStatus	
";
			var result = new List<LocationWithCapacity>();
			var sqlResults = new DynamicBusinessObjectCollection(area.Factory);
			sqlResults.Load(rawSql, new[]
			{
				ZSqlParameter.New("@AreaPK", area.PK, WhsAreaSchema.PK),
				ZSqlParameter.New("@NormalLocationStatus", LocationStatus.Codes.Normal, WhsLocationSchema.WL_LocationStatus)
			});

			foreach (DynamicBusinessObject sqlResult in sqlResults)
			{
				var location = area.PickLocations.Cast<WhsLocation>().Single(loc => loc.PK == (ZGuid)sqlResult[WhsLocationSchema.PK]);
				result.Add(ReadFromDynamicBizo(location, sqlResult));
			}

			return result;
		}

		static LocationWithCapacity ReadFromDynamicBizo(WhsLocation location, DynamicBusinessObject sqlResult)
		{
			var availableWeight = (ZDecimal)sqlResult["AvailableWeight"];
			var availableVolume = (ZDecimal)sqlResult["AvailableVolume"];
			var availableUnits = (ZDecimal)sqlResult["AvailableUnits"];
			return new LocationWithCapacity(location, availableWeight, availableVolume, availableUnits);
		}

		#endregion

		#region CheckLocationIsRequiredWhenFinalising

		public static void CheckLocationIsRequiredWhenFinalising(WhsReceive receive, ZDecimal receiveQty, ZPropertyInfo info)
		{
			var locationPK = (ZGuid)info.Value;

			if (!info.HasErrors() && receive.IsFinalising && receiveQty > 0 && (locationPK.IsEmpty || !locationPK.IsValid))
			{
				info.AddError(Res.GetString("629e9f6e-5ed5-45e0-80a3-4290ecfcc460", "Please enter a valid location."));
			}
		}

		#endregion

		#region CheckLocationIsInCorrectWarehouse

		public static void CheckLocationIsInCorrectWarehouse(WhsReceive receive, ZPropertyInfo info, WhsLocation location)
		{
			if (!info.HasErrors() && location != null)
			{
				var warehouse = receive.Warehouse;
				if (warehouse != null)
				{
					var row = location.Row;
					if (row != null && row.WR_WW_Whs != warehouse.PK)
					{
						info.AddError(Res.GetString("bb55203e-34ca-4150-a845-6a0421096bac", "Location does not belong to the selected Warehouse."));
					}
				}
			}
		}

		#endregion

		#region CheckLocationIsNotVoid

		public static void CheckLocationIsNotVoid(ZPropertyInfo info, WhsLocation location)
		{
			if (!info.HasErrors() && location != null && location.WLV_LocationStatus == LocationStatus.Codes.Void)
			{
				info.AddError(Res.GetString("5d88c06a-7891-477c-93ee-e71e69dda45c", "Please enter a valid location, the Location you entered is Void."));
			}
		}

		#endregion

		#region CheckLocationIsNotInAnInwardProcessingArea

		public static void CheckLocationIsNotInAnInwardProcessingArea(ZPropertyInfo info, WhsLocation location)
		{
			if (!info.HasErrors() && (location?.IsInInwardProcessingArea ?? false))
			{
				info.AddError(Res.GetString("19c1f611-93df-45a4-920f-3e27cc4e40f2", "Please enter a valid location, the Location you have entered is in an Inward Processing area."));
			}
		}

		#endregion

		#region CheckLocationForReceiptEntry

		public static void CheckLocationForReceiptEntry(ZPropertyInfo info, WhsLocation location, WhsWarehouse whs, WhsReceiveLine receiveLine)
		{
			var inventory = receiveLine.Inventory[0];
			if (!info.HasErrors() && location != null && whs != null && inventory != null && !WhsEnvironment.IsRF)
			{
				if (receiveLine.IsPutaway)
				{
					CheckReceiveLineLocationStockOnHand(info, location, receiveLine);
					CheckStagingAreaIfCrossDocked(info, location, inventory);
				}
				CheckForPendingOrders(info, inventory);
			}
		}

		#region CheckLocationStockOnHand

		static void CheckReceiveLineLocationStockOnHand(ZPropertyInfo info, WhsLocation location, WhsReceiveLine currentDocketLine)
		{
			var needToAddSOHWarning = WarehouseDataRegistry.Instance.SOHLocationWarning.Value && !GetIsFixedLocation(location);
			if (needToAddSOHWarning)
			{
				var inventoryInLocationInfos = GetInventoryInLocationInfos_IncludingPending(location, currentDocketLine.WE_WD);
				CheckLocationStockOnHand(info, location, currentDocketLine, inventoryInLocationInfos);
			}
		}

		static IEnumerable<LocationStockOnHandInfo> GetInventoryInLocationInfos_IncludingPending(WhsLocation location, ZGuid docketPK)
		{
			var inventoryIncludingPending_InMemory = GetInventoryInLocationInfos_IncludingPending_InMemory(location);
			return inventoryIncludingPending_InMemory.Union(GetInventoryInLocationInfos_IncludingPending_InDB(location, docketPK));
		}

		static IEnumerable<LocationStockOnHandInfo> GetInventoryInLocationInfos_IncludingPending_InMemory(WhsLocation location)
		{
			var finalisedQuery = new ZQuery(WhsDocketLineSchema.WE_DocketLineStatus, DocketLineStatus.Codes.Finalised);
			finalisedQuery.AddToFilter(WhsDocketLineSchema.WE_StockOnHand, SQLComparisonOperator.GreaterThan, 0m);

			var pendingQuery = new ZQuery(WhsDocketLineSchema.WE_DocketLineStatus, SQLComparisonOperator.NotEqual, DocketLineStatus.Codes.Finalised);
			pendingQuery.AddToFilter(WhsDocketLineSchema.WE_TransactionQuantity, SQLComparisonOperator.GreaterThan, 0m);

			var finalisedAndPendingQuery = new ZQuery();
			finalisedAndPendingQuery.AddToFilter(finalisedQuery, JoinCondition.Or);
			finalisedAndPendingQuery.AddToFilter(pendingQuery, JoinCondition.Or);

			var inventoryIncludingPendingQuery = new ZQuery(WhsDocketLineSchema.WE_WL, location.PK);
			inventoryIncludingPendingQuery.AddToFilter(finalisedAndPendingQuery, JoinCondition.And);
			inventoryIncludingPendingQuery.FetchOnlyFromLocalCache = true;

			return GetLocationStockOnHandInfos(location.Factory.Load<WhsDocketLine>(inventoryIncludingPendingQuery));
		}

		static IEnumerable<LocationStockOnHandInfo> GetLocationStockOnHandInfos(IEnumerable<WhsDocketLine> docketLines)
		{
			var locationStockOnHandInfos = new List<LocationStockOnHandInfo>();
			var docketsWithClient = new Dictionary<ZGuid, OrgHeader>();
			var clients = new Dictionary<ZGuid, OrgHeader>();
			var products = new Dictionary<ZGuid, OrgSupplierPart>();
			var groupedDocketLines = docketLines.GroupBy(line => new { line.WE_WD, line.WE_PalletID, line.WE_OP });
			foreach (var groupedDocketLine in groupedDocketLines)
			{
				var referenceDocketLine = groupedDocketLine.First();
				var client = GetClient(referenceDocketLine);
				var product = GetProduct(referenceDocketLine);
				if (product != null)
				{
					locationStockOnHandInfos.Add(new LocationStockOnHandInfo(
						referenceDocketLine.WE_WD,
						referenceDocketLine.WE_PalletID,
						client.OH_Code,
						client.PK,
						product.OP_PartNum,
						product.PK));
				}
			}

			return locationStockOnHandInfos;

			OrgHeader GetClient(WhsDocketLine docketLineReference)
			{
				var docketPK = docketLineReference.WE_WD;
				if (!docketsWithClient.TryGetValue(docketPK, out var docketClient))
				{
					var docket = docketLineReference.Docket;
					if (!clients.TryGetValue(docket.WD_OH_Client, out docketClient))
					{
						docketClient = docket.Client;
						clients.Add(docket.WD_OH_Client, docketClient);
					}

					docketsWithClient.Add(docketPK, docketClient);
				}

				return docketClient;
			}

			OrgSupplierPart GetProduct(WhsDocketLine docketLineReference)
			{
				var productPK = docketLineReference.WE_OP;
				if (!products.TryGetValue(productPK, out var product))
				{
					product = docketLineReference.SupplierPart;
					products.Add(productPK, product);
				}

				return product;
			}
		}

		static IEnumerable<LocationStockOnHandInfo> GetInventoryInLocationInfos_IncludingPending_InDB(WhsLocation location, ZGuid docketPK)
		{
			return LocationStockOnHandInfoCacheManager.GetLocationStockOnHandCache(location.Factory, location.PK, docketPK);
		}

		public static void CheckLocationStockOnHand(ZPropertyInfo info, WhsLocation location, WhsDocketLine currentDocketLine, IEnumerable<LocationStockOnHandInfo> finalisedAndPendingLines = null)
		{
			if (!info.HasErrors() && WarehouseDataRegistry.Instance.SOHLocationWarning.Value && !GetIsFixedLocation(location))
			{
				finalisedAndPendingLines = finalisedAndPendingLines ?? GetInventoryInLocationInfos_IncludingPending(location, currentDocketLine.WE_WD);
				var finalisedAndPendingLines_ExcludingCurrentDocketAndSourcePallet = RemoveLinesMatchingDocketOrPalletID(finalisedAndPendingLines, currentDocketLine);

				var list = new HashSet<ZString>();
				foreach (var line in finalisedAndPendingLines_ExcludingCurrentDocketAndSourcePallet
					.DistinctBy(l => new { l.ProductPK, l.DocketPK })
					.DistinctBy(l => new { l.ProductPK, l.ClientPK })
					)
				{
					var clientProduct = Res.GetString("EC5177DB-270F-436C-9A11-0121E329DCDA", "Client: {0}, Product: {1}", line.ClientCode, line.ProductCode);
					list.Add(clientProduct);
				}

				if (list.Count > 0)
				{
					var error = new ZStringBuilder(Res.GetString("41c48ec2-5214-4731-878b-4e741551d7cd", "Stock On Hand exists."));
					Array.ForEach(list.OrderBy(s => s).ToArray(), s => error.Append(s));
					info.AddWarning(error.ToStringWithNewLineBetweenAppends());
				}
			}
		}

		static IEnumerable<LocationStockOnHandInfo> RemoveLinesMatchingDocketOrPalletID(IEnumerable<LocationStockOnHandInfo> lines, WhsDocketLine docketLine)
		{
			// remove lines that match docket - don't want warnings showing because of lines on the same Docket being received/transferred into the same location
			var result = lines.Where(l => l.DocketPK != docketLine.WE_WD);

			// remove lines that match pallet id - don't want warnings showing because of lines on the pallet that is being transferred
			if (!docketLine.WE_PalletID.IsEmpty)
			{
				result = result.Where(l => l.PalletId != docketLine.WE_PalletID);
			}

			return result;
		}

		#endregion

		#region CheckStagingAreaIfCrossDocked

		static void CheckStagingAreaIfCrossDocked(ZPropertyInfo info, WhsLocation location, WhsInventoryView inventory)
		{
			if (!info.HasErrors() && inventory.WI_CrossDockQuantity > 0)
			{
				var locationIsOrderCrossDockLocation = false;
				var atleastOneOrderHasCrossDockLocation = false;

				foreach (var pickLine in inventory.ReservedPickLines)
				{
					var pickableDocketLine = (WhsPickableDocketLine)pickLine.DocketLine;
					var crossDockLocation = pickableDocketLine?.PickableDocket.CrossDockLocation;
					if (crossDockLocation != null)
					{
						atleastOneOrderHasCrossDockLocation = true;
						if (location.PK == crossDockLocation.PK)
						{
							locationIsOrderCrossDockLocation = true;
							break;
						}
					}
				}

				if (!locationIsOrderCrossDockLocation)
				{
					if (atleastOneOrderHasCrossDockLocation)
					{
						info.AddWarning(Res.GetString("d11bf4b8-aaa4-4f28-ae2e-c758b6303247", "This receipt line is cross docked, but you have selected a location that is not the customer order's cross dock location"));
					}
					else
					{
						info.AddWarning(Res.GetString("164b871d-c471-468f-93e4-a0946212712b", "This receipt line is cross docked (no cross dock location has been selected on the customer order"));
					}
				}
			}
		}

		#endregion

		#region CheckForPendingOrders

		static void CheckForPendingOrders(ZPropertyInfo info, WhsInventoryView inventory)
		{
			if (!info.HasErrors())
			{
				var docketOriginal = inventory.DocketOriginal;
				if (IsParentValidForPendingOrderCheck(docketOriginal, inventory))
				{
					var pendingOrderExtRefs = PendingOrdersCacheManager.GetPendingOrders(docketOriginal.Factory, docketOriginal.WD_WW_Whs, docketOriginal.WD_OH_Client, inventory.WI_OP);
					if (!string.IsNullOrEmpty(pendingOrderExtRefs))
					{
						info.AddWarning(
							Res.GetString("e3edf5a1-8a72-4394-a224-d23a8f2d9fbe", "This product is required by the following pending order(s): {0}",
							pendingOrderExtRefs));
					}
				}
			}
		}

		static bool IsParentValidForPendingOrderCheck(WhsDocket docketOriginal, WhsInventoryView inventory)
		{
			return (docketOriginal != null &&
					docketOriginal.WD_WW_Whs.IsValid &&
					docketOriginal.WD_OH_Client.IsValid &&
					inventory.WI_OP.IsValid &&
					inventory.ReservedPickLines.Count == 0);
		}

		#endregion

		#endregion

		#region CheckLocationsIsTSAKnown

		public static void CheckLocationIsTSAKnown(WhsDocketLine docketLine, ZPropertyInfo info)
		{
			var docket = docketLine.Docket;
			var location = docketLine.Location;
			if (docket != null && location != null)
			{
				var warehouse = docket.Warehouse;
				if (warehouse != null && warehouse.IsApprovedKnown)
				{
					if (TSAInfo.IsOrgHeaderTSAKnown(docket.Client))
					{
						if (!location.IsApprovedKnownLocation)
						{
							info.AddError(LocationIsNotKnownByTSAErrorMessage);
						}
					}
					else
					{
						if (location.IsApprovedKnownLocation)
						{
							info.AddError(LocationIsKnownByTSAErrorMessage);
						}
					}
				}
			}
		}

		static ZString LocationIsNotKnownByTSAErrorMessage
		{
			get { return Res.GetString("f79fba75-ff3c-429f-b35b-404018559d52", "This location is not known by TSA."); }
		}

		static ZString LocationIsKnownByTSAErrorMessage
		{
			get { return Res.GetString("d8079dec-f20e-4f55-bd48-8afebda83dd1", "This location is known by TSA."); }
		}

		#endregion

		#endregion

		#region CheckConsigneeNameOrPK

		public static void CheckConsigneeNameOrPK(WhsReceiveLine parent, ZWrappedPropertyInfo propertyInfo)
		{
			var address = parent.ConsigneeDocAddress;

			using (new SemaphoreManager(parent.IsAdditionalValidationRunning))
			{
				address.Validation.ValidateAll();
			}

			// proxy across errors because ConsigneeNameOrPK is basically a duplicate of ConsigneeDocAddress, but need to show all errors in one field rather that in JobDocAddress control.
			if (address.HasErrors())
			{
				propertyInfo.InnerInfo.AddRange(address.GetErrors().ToArray());
			}
		}

		#endregion

		#region CheckJobHasRecordWithSameProductCodeButDifferentPropertyValue

		public static void CheckJobHasRecordWithSameProductCodeButDifferentPropertyValue(WhsDocket docket, ZPropertyInfo info)
		{
			var receive = docket as WhsReceive;
			if (receive != null)
			{
				var bizO = info.BizObj;
				if (bizO != null)
				{
					foreach (var inventory in ((ISupportTemporaryProduct)bizO).GetSiblingsWithTheSameProduct())
					{
						if (inventory.IsTemporaryProduct && inventory[info.Name].ToString() != bizO[info.Name].ToString())
						{
							info.AddError(WhsDocketLineValidation.ThisFieldShouldBeTheSameForAllAutoCreatedProductsWithSameProductCode);
						}
					}
				}
			}
		}

		#endregion

		#region CheckProductAssignedToCorrectFixedOrDynamicLocation

		internal static void CheckProductAssignedToCorrectFixedOrDynamicLocation(WhsLocation location, OrgHeader client, OrgSupplierPart supplierPart, ZPropertyInfo propertyInfo)
		{
			CheckIsFixLocationAndProductAssignedToTheLocation(location, client, supplierPart, propertyInfo);
			CheckIsDynamicLocationAndProductAssignedToDynamicArea(location, client, supplierPart, propertyInfo);
		}

		#endregion

		#region CheckIsDynamicLocationAndProductAssignedToDynamicArea

		static void CheckIsDynamicLocationAndProductAssignedToDynamicArea(WhsLocation location, OrgHeader client, OrgSupplierPart supplierPart, ZPropertyInfo propertyInfo)
		{
			var product = WhsProduct.GetWhsProduct(supplierPart);
			if (location != null && product != null)
			{
				var productParams = product.ParamsByWhsAndClient.SingleOrDefault(p => p.W3_OH == client.PK && p.W3_WW == location.Warehouse.PK);
				if (location.LocationType.WLT_LocationClass == LocationClasses.Codes.DPF)
				{
					if (!IsValidDynamicProduct(productParams))
					{
						propertyInfo.AddError(Res.GetString("1a2a9b4a-eab9-4f55-9881-431d8b683043", "Cannot put product {0} in dynamic location {1}, as it is not a dynamic product.", supplierPart.OP_PartNum, location.ToLocationString()));
					}
					else if (location.PickingArea.PK != productParams.W3_WA_DynamicPickFaceArea)
					{
						propertyInfo.AddError(Res.GetString("8492155c-4fbd-4168-b2ea-42f0763ebb77", "Cannot put dynamic product {0} in dynamic location {1}, as the location is not within the product's designated dynamic area ({2}).", supplierPart.OP_PartNum, location.ToLocationString(), productParams.DynamicPickFaceArea.WA_NameMultilingual));
					}
				}
			}
		}

		static bool IsValidDynamicProduct(WhsProductParamsByWhsAndClient productParams)
		{
			return productParams != null && !productParams.W3_WA_DynamicPickFaceArea.IsEmpty;
		}

		#endregion

		#region CheckIsFixLocationAndProductAssignedToTheLocation

		static void CheckIsFixLocationAndProductAssignedToTheLocation(WhsLocation location, OrgHeader client, OrgSupplierPart supplierPart, ZPropertyInfo propertyInfo)
		{
			var error = GetErrorFixLocationAndProductAssignedToTheLocation(location, client, supplierPart);
			if (!string.IsNullOrEmpty(error))
			{
				propertyInfo.AddError(error);
			}
		}

		public static string GetErrorFixLocationAndProductAssignedToTheLocation(WhsLocation location, OrgHeader client, OrgSupplierPart supplierPart)
		{
			string result = string.Empty;
			if (GetIsFixedLocation(location) && !IsLocationFixedForSupplierPart(location, client, supplierPart))
			{
				result = Res.GetString("863750ca-de34-4540-97f0-3ed04af141e8", "This location is a fixed pick face location and '{0}' is not assigned to this location.", supplierPart.OP_PartNum);
			}
			return result;
		}

		public static bool GetIsFixedLocation(WhsLocation location)
		{
			var value = location.LocationType;
			return value != null && value.WLT_LocationClass == LocationClasses.Codes.FIX;
		}

		#endregion

		#region CheckInwardProcessingJobOnlySetOnVirtualWarehouses

		public static void CheckInwardProcessingJobOnlySetOnVirtualWarehouses(WhsDocket docket, ZPropertyInfo info)
		{
			// Tested in WhsPickableDocketValidationTest.TestCheckWD_IsInwardsProcessingJob and WhsReceiveValidationTest.TestCheckWD_IsInwardsProcessingJob
			if (docket.WD_IsInwardsProcessingJob && !(docket.Warehouse?.WW_IsVirtualWarehouse ?? false))
			{
				info.AddError(Res.GetString("48ea9b6a-63d0-457d-bed9-6086e2d90438", "Inward Processing Jobs can only be created in Virtual Warehouses."));
			}
		}

		#endregion

		#region CurrentProductsInPickFaceLocation

		static bool IsLocationFixedForSupplierPart(WhsLocation location, OrgHeader client, OrgSupplierPart part)
		{
			var filter = new ZQuery();
			filter.AddToFilter(WhsPickFaceSchema.WF_WL, location.PK);
			filter.AddToFilter(WhsPickFaceSchema.WF_OH_Client, client.PK);
			filter.AddToFilter(WhsPickFaceSchema.WF_OP, part.PK);
			return location.Factory.LoadTop1<WhsPickFace>(filter) != null;
		}

		#endregion

		#region GetAvailableAndPendingTotalInventoryQuantity

		public static decimal GetAvailableAndPendingTotalInventoryQuantity(WhsLocation destLocation, WhsDocket docket, WhsInventoryView[] inventoryList, bool includeDocketLines = true)
		{
			return GetTotalInventoryQuantity(inventoryList) + GetTotalTransactionQuantity(destLocation.PK, docket, includeDocketLines);
		}

		#endregion

		#region GetTotalTransactionQuantity

		static ZDecimal GetTotalTransactionQuantity(ZGuid parentLocationPK, WhsDocket docket, bool includeDocketLines)
		{
			var totalDocketLinesQuantity = 0m;
			if (includeDocketLines)
			{
				foreach (var docketLine in docket.Lines)
				{
					var location = docketLine.Location;

					if (location != null)
					{
						if (location.PK == parentLocationPK)
						{
							totalDocketLinesQuantity += docketLine.WE_TransactionQuantity;
						}
					}
				}
			}

			totalDocketLinesQuantity += GetPendingUnitsIntoLocaion(docket, parentLocationPK);

			return totalDocketLinesQuantity;
		}

		#endregion

		#region GetTotalInventoryStorage

		internal static ZDecimal GetTotalInventoryStorage(WhsInventoryView[] inventoryList, ZString parentLocationMaxUnit, SchemaDecimalColumn productWgtOrVolColumn, SchemaStringColumn productWgtOrVolUnitColumn)
		{
			ZDecimal totalInventoryWeightOrVolume = 0m;

			foreach (var inventory in inventoryList)
			{
				var product = inventory.SupplierPart;
				if (product != null)
				{
					var productWgtOrVol = (ZDecimal)product[productWgtOrVolColumn];
					if (productWgtOrVol > 0)
					{
						var productWgtOrVolUnit = (ZString)product[productWgtOrVolUnitColumn];
						var inventoryQty = inventory.Docket.IsFinalised ? inventory.WI_TotalUnits : inventory.WI_InDocketLineUnits;
						var conversionFactorToMatchLocationUnit = product.UnitConverter.ConversionFactor(productWgtOrVolUnit, parentLocationMaxUnit);

						totalInventoryWeightOrVolume += inventoryQty * productWgtOrVol * conversionFactorToMatchLocationUnit;
					}
				}
			}

			return totalInventoryWeightOrVolume;
		}

		#endregion

		#region GetPendingUnitsIntoLocaion

		internal static ZDecimal GetPendingUnitsIntoLocaion(WhsDocket docketToExclude, ZGuid destLocationPK)
		{
			var result = 0m;
			if (docketToExclude != null)
			{
				var otherDocketLines = GetPendingDocketLinesAtLocation(docketToExclude, destLocationPK);
				if (otherDocketLines.Length > 0)
				{
					result = otherDocketLines.Sum(dl => dl.WE_TransactionQuantity);
				}
			}
			return result;
		}

		#endregion

		#region GetPendingDocketLinesAtLocation

		internal static WhsDocketLine[] GetPendingDocketLinesAtLocation(WhsDocket docketToExclude, ZGuid destLocationPK)
		{
			CargoWise.Common.Argument.NotNull(docketToExclude, "docketToExclude");
			var docketSubQuery = new ZDBOnlySubQuery(typeof(WhsDocket), WhsDocketLineSchema.WE_WD);
			docketSubQuery.AddToFilter(WhsDocketSchema.WD_WW_Whs, docketToExclude.WD_WW_Whs);

			var docketLineQuery = new ZDBOnlyQuery(typeof(WhsDocketLine));
			docketLineQuery.AddToFilter(WhsDocketLineSchema.WE_WD, SQLComparisonOperator.NotEqual, docketToExclude.PK);
			docketLineQuery.AddToFilter(WhsDocketLineSchema.WE_WL, SQLComparisonOperator.Equal, destLocationPK);
			docketLineQuery.AddToFilter(WhsDocketLineSchema.WE_DocketLineStatus, SQLComparisonOperator.NotEqual, DocketLineStatus.Codes.Finalised);
			docketLineQuery.AddToFilter(WhsDocketLineSchema.WE_TransactionQuantity, SQLComparisonOperator.GreaterThan, 0m);
			docketLineQuery.AddSubQuery(docketSubQuery, JoinCondition.And);

			return docketToExclude.Factory.Load<WhsDocketLine>(docketLineQuery);
		}

		#endregion

		#region GetTotalInventoryQuantity

		internal static ZDecimal GetTotalInventoryQuantity(WhsInventoryView[] inventoryList)
		{
			ZDecimal totalInventoryQuantity = 0m;

			foreach (var inventory in inventoryList.Where(i => i.IsDocketLineFinalised))
			{
				totalInventoryQuantity += inventory.WI_TotalUnits;
			}

			return totalInventoryQuantity;
		}

		#endregion

		#region CheckLocationIsNotDockDoorLocation

		public static void CheckLocationIsNotDockDoorLocation(ZPropertyInfo info, WhsDocketLine line, string errorMessage)
		{
			if (!info.HasErrors() && line.Docket != null && (line.Location?.IsDockDoorLocation ?? false))
			{
				info.AddError(errorMessage);
			}
		}

		#endregion

		#region CheckLocationIsNotPackingStationLocation

		public static void CheckLocationIsNotPackingStationLocation(ZPropertyInfo info, WhsLocation location, string errorMessage)
		{
			if (!info.HasErrors() && (location?.IsPackingStationLocation ?? false))
			{
				info.AddError(errorMessage);
			}
		}

		#endregion

		#region CheckLocationIsNotPackingConsolidationLocation

		public static void CheckLocationIsNotPackingConsolidationLocation(ZPropertyInfo info, WhsLocation location, string errorMessage)
		{
			if (!info.HasErrors() && (location?.IsPackingConsolidationLocation ?? false))
			{
				info.AddError(errorMessage);
			}
		}

		#endregion

		#region CheckProductWithoutPalletConversionInLocationUsingPalletSpaces

		public static void CheckProductWithoutPalletConversionInLocationUsingPalletSpaces(ZPropertyInfo info, WhsLocation location, ZDecimal stockKeepingUnitPerPallet)
		{
			if (location != null
				&& location.WLV_PalletFloorSpaces != 0
				&& location.WLV_PalletStackHeight != 0
				&& stockKeepingUnitPerPallet == 0)
			{
				info.AddWarning(Res.GetString("5061012E-4973-4755-9AA6-AAB441C0367D", "Products without pallet conversions cannot be put in locations using pallet spaces."));
			}
		}

		#endregion

		#region CheckInventoryWithoutPalletID

		public static void CheckInventoryWithoutPalletIDInLocationUsingPalletSpaces(ZPropertyInfo info, WhsLocation location, ZString palletID, bool addNotificationAsError = true)
		{
			if (location != null
				&& location.WLV_PalletFloorSpaces != 0
				&& location.WLV_PalletStackHeight != 0
				&& palletID.IsEmpty)
			{
				var notification = Res.GetString("EA029CF2-4652-455E-ADD5-1A725DEC1AE6", "Inventory without Pallet ID cannot be put in locations using pallet spaces.");
				if (addNotificationAsError)
				{
					info.AddError(notification);
				}
				else
				{
					info.AddWarning(notification);
				}
			}
		}

		#endregion

		#region Check Procedure Names

		public const string WhsCheckTotalUnits = "WhsCheckTotalUnits_V3";
		public const string WhsCheckPickLinesAreNotOverCommitting = "WhsCheckPickLinesAreNotOverCommitting_V3";
		public const string WhsCheckAllVariancesForSingleCycleCountLocationHasSameStatus = "WhsCheckAllVariancesForSingleCycleCountLocationHasSameStatus";
		public const string WhsCheckIfLocationHasOpenVariance = "WhsCheckIfLocationHasOpenVariance";
		public const string WhsCheckStockOnHandIsBalanced = "WhsCheckStockOnHandIsBalanced";
		public const string WhsCheckStockOnHandIsBalanced_ForInsert = "WhsCheckStockOnHandIsBalanced_ForInsert";
		public const string WhsCheckOrderDockDoorLocationIsMatchWithPick = "WhsCheckOrderDockDoorLocationIsMatchWithPick";
		public const string WhsCheckPreventFinalizeIfLinesAreUnfinalized = "WhsCheckPreventFinalizeIfLinesAreUnfinalized";
		public const string WhsCheckDocketStatusDepartedOnPickFinalisation = "WhsCheckDocketStatusDepartedOnPickFinalisation";
		public const string WhsCheckTransactionAndPickQtyIsCorrect = "WhsCheckTransactionAndPickQtyIsCorrect_V3";
		public const string WhsCheckTransactionAndPickQtyIsCorrect_ForInsert = "WhsCheckTransactionAndPickQtyIsCorrect_ForInsert";
		public const string WhsCheckLinesDoNotExceedLocationCapacity = "WhsCheckLinesDoNotExceedLocationCapacity";
		public const string WhsCheckDocketStatusAndDateWithLines = "WhsCheckDocketStatusAndDateWithLines";
		public const string WhsCheckFinalisedDocketLineWithUnPickedPicklines = "WhsCheckFinalisedDocketLineWithUnPickedPicklines";
		public const string WhsCheckFinalisedPickWithUnPickedPicklines = "WhsCheckFinalisedPickWithUnPickedPicklines";
		public const string WhsCheckPickPackingStationCorrect = "WhsCheckPickPackingStationCorrect";
		public const string WhsCheckInterWhsTransfersAreInSync = "WhsCheckInterWhsTransfersAreInSync";
		public const string WhsCheckFinalisedJobsWithUnPickedPicklines = "WhsCheckFinalisedJobsWithUnPickedPicklines";
		public const string WhsCheckSerialNumberMatchWithDocketLineQuantity = "WhsCheckSerialNumberMatchWithDocketLineQuantity";
		public const string WhsCheckSerialNumberQuantityMatchWithAsnLine = "WhsCheckSerialNumberQuantityMatchWithAsnLine";
		public const string WhsCheckSerialNumberMatchWithPickLineUnits = "WhsCheckSerialNumberMatchWithPickLineUnits";
		public const string WhsCheckTransferSerialNumberMatchWithnventory = "WhsCheckTransferSerialNumberMatchWithnventory";
		public const string WhsCheckForUnreferencedWhsDockDoorAssignment = "WhsCheckForUnreferencedWhsDockDoorAssignment";

		#endregion

		#region Trigger Names

		public const string TG_WhsOrder_EnsureDDLIsEnteredOnPick = "TG_WhsOrder_EnsureDDLIsEnteredOnPick";
		public const string TG_WhsDocketLine_PreventChangeCriticalFieldsOnPickedOrder = "TG_WhsDocketLine_PreventChangeCriticalFieldsOnPickedOrder";
		public const string TG_WhsDocketLine_PreventFinalisationOfReceiveWithShortageOfStock = "TG_WhsDocketLine_PreventFinalisationOfReceiveWithShortageOfStock";
		public const string TG_WhsDocketLine_StockOnHandIsBalanced = "TG_WhsDocketLine_StockOnHandIsBalanced";
		public const string TG_WhsDocketLine_StockOnHandIsBalanced_Insert = "TG_WhsDocketLine_StockOnHandIsBalanced_Insert";
		public const string TG_WhsDocketLine_StockOnHandIsBalanced_InsertForParent = "TG_WhsDocketLine_StockOnHandIsBalanced_InsertForParent";
		public const string TG_WhsPick_EnsureDocketStatusDepartedOnPickFinalisation = "TG_WhsPick_EnsureDocketStatusDepartedOnPickFinalisation";
		public const string TG_WhsPickLine_StockOnHandIsBalanced = "TG_WhsPickLine_StockOnHandIsBalanced";
		public const string TG_WhsPutawayJob_PreventFinalizeIfLinesAreUnfinalized = "TG_WhsPutawayJob_PreventFinalizeIfLinesAreUnfinalized";
		public const string TG_WhsPickLine_TransactionAndPickedQtyIsCorrect = "TG_WhsPickLine_TransactionAndPickedQtyIsCorrect";
		public const string TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect = "TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect";
		public const string TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert = "TG_WhsDocketLine_TransactionAndPickedQtyIsCorrect_Insert";
		public const string TG_PreventOverfillLocationWithUnitsCapacity = "TG_PreventOverfillLocationWithUnitsCapacity";
		public const string TG_PreventMismatchOnDocketStatusAndDateWithLines = "TG_PreventMismatchOnDocketStatusAndDateWithLines";
		public const string TG_PreventUnPickedPickLinesOnFinalisedDocketLines = "TG_PreventUnPickedPickLinesOnFinalisedDocketLines";
		public const string TG_PreventOverReduceOfStockViaInventoryLine = "TG_PreventOverReduceOfStockViaInventoryLine";
		public const string TG_WhsPick_PackingStationIsValid = "TG_WhsPick_PackingStationIsValid";
		public const string TG_PreventUnPickedPickLinesOnFinalisedPicks = "TG_PreventUnPickedPickLinesOnFinalisedPicks";
		public const string TG_WhsDocketLine_EnsureInterWhsChildLinesAreSynchronised = "TG_WhsDocketLine_EnsureInterWhsChildLinesAreSynchronised";
		public const string TG_PreventUnPickedPickLinesOnFinalisedJobs = "TG_PreventUnPickedPickLinesOnFinalisedJobs";
		public const string TG_PreventOverCommitOfStockViaPickLine = "TG_PreventOverCommitOfStockViaPickLine";
		public const string TG_WhsDocketLine_EnsureSerialNumberQuantityMatchWithLine = "TG_WhsDocketLine_EnsureSerialNumberQuantityMatchWithLine";
		public const string TG_WhsAsnLine_EnsureSerialNumberQuantityMatchWithLine = "TG_WhsAsnLine_EnsureSerialNumberQuantityMatchWithLine";
		public const string TG_WhsPickLine_EnsureSerialNumberMatchWithPickLineUnits = "TG_WhsPickLine_EnsureSerialNumberMatchWithPickLineUnits";
		public const string TG_WhsPickLine_EnsureTransferSerialNumberMatchWithInventory = "TG_WhsPickLine_EnsureTransferSerialNumberMatchWithInventory";
		public const string TG_WhsPick_EnsureDockDoorAssignmentsAreReferenced_ByAPick = "TG_WhsPick_EnsureDockDoorAssignmentsAreReferenced_ByAPick";
		public const string TG_WhsDockDoorAssignment_EnsureDockDoorAssignmentsAreReferenced_ByAPick = "TG_WhsDockDoorAssignment_EnsureDockDoorAssignmentsAreReferenced_ByAPick";

		#endregion
	}
}
