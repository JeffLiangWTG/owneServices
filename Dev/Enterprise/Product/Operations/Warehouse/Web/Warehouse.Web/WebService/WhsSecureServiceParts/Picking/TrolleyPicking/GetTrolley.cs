using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.TrolleyPicking;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	public partial class WhsSecureService
	{
		#region GetTrolley

		[WebMethod(Description = "Gets the Trolley for building or picking")]
		[SoapHeader("SecurityHeader", Direction = SoapHeaderDirection.In)]
		public WhsPickTrolleyWebServiceResponse GetTrolley(string trolleyNumber, TrolleyJobStatus trolleyStatus, bool shouldOverrideOtherUsers, TrolleyPickingType trolleyType)
		{
			return HandleWebServiceRequest<WhsPickTrolleyWebServiceResponse>(r => LoadOrCreateTrolleyJob(r, trolleyNumber, trolleyStatus, shouldOverrideOtherUsers, trolleyType));
		}

		void LoadOrCreateTrolleyJob(WhsPickTrolleyWebServiceResponse response, string trolleyNumber, TrolleyJobStatus trolleyStatus, bool shouldOverrideOtherUsers, TrolleyPickingType trolleyType)
		{
			// Load the trolley
			var trolleyEquipment = LoadTrolley(response, trolleyNumber);
			if (response.Error != ErrorTypes.None || trolleyEquipment == null)
			{
				return;
			}

			// Load the pick trolley job
			var pickTrolleyJob = HandleTrolleyPickingOptionForTrolleyJob(response, trolleyNumber, trolleyStatus, trolleyEquipment);
			if (response.NoError() && pickTrolleyJob != null)
			{
				// Call the appropriate build / pick trolley method based on existing status
				switch (trolleyStatus)
				{
					case TrolleyJobStatus.Building:
						BuildTrolley(response, pickTrolleyJob, trolleyNumber, trolleyType);
						break;
					case TrolleyJobStatus.Picking:
						PickTrolley(response, pickTrolleyJob, trolleyNumber, trolleyType, shouldOverrideOtherUsers);
						break;
					default:
						response.Error = ErrorTypes.BusinessValidationError;
						response.ErrorMessage = response.ErrorMessage = Res.GetString("3f132937-07e0-40cc-af11-980e15ff6459", "Un-handled trolley job status: {0}", trolleyStatus);
						break;
				}

				var concurrencyErrorMessage = Res.GetString("2b061cd6-83cd-46db-95d2-b572454a3a6d", "Another user has modified the trolley job. Please restart the operation and try again.");
				WebServiceHelper.SaveFactoryWithExceptionHandling(Factory, response, (concurrencyException) => concurrencyErrorMessage);
			}
		}

		// Given a trolley number, try to load that trolley (equipment) from the DB
		RefEquipment LoadTrolley(WhsPickTrolleyWebServiceResponse response, string trolleyNumber)
		{
			RefEquipment refEquipment = null;

			if (string.IsNullOrEmpty(trolleyNumber))
			{
				response.Error = ErrorTypes.BusinessValidationError;
				response.ErrorMessage = Res.GetString("02204bcb-727b-4dba-868a-27cdfd48de4b", "Please provide a Trolley Number.");
			}
			else
			{
				refEquipment = Factory.LoadTop1<RefEquipment>(new ZQuery(RefEquipmentSchema.RQ_Registration, trolleyNumber));
				if (refEquipment == null)
				{
					response.Error = ErrorTypes.BusinessValidationError;
					response.ErrorMessage = Res.GetString("8f62c7ec-09a1-4e39-95f5-7991b332f676", "Trolley '{0}' cannot be found.", trolleyNumber);
				}
			}

			return refEquipment;
		}

		void BuildTrolley(WhsPickTrolleyWebServiceResponse response, WhsPickTrolleyJob pickTrolleyJob, string trolleyNumber, TrolleyPickingType trolleyType)
		{
			AddFetchHintsForPickTrolley(pickTrolleyJob);

			// The trolley type in the DB will be None if we have an empty trolley.
			// We also check the type we get from the DB is the type we asked for.
			if (pickTrolleyJob.PickingType == TrolleyPickingType.None || pickTrolleyJob.PickingType == trolleyType)
			{
				var warehouse = WebServiceHelper.GetWarehouse(Factory, SecurityHeader.WarehouseCode);
				if (VerifyTrolleyWarehouse(response, trolleyNumber, pickTrolleyJob, warehouse))
				{
					response.Job = CreateTrolleyJobInfo(warehouse, pickTrolleyJob, trolleyNumber, trolleyType);
				}
			}
			else
			{
				response.Error = ErrorTypes.BusinessValidationError;
				response.ErrorMessage = Res.GetString("5b6054c6-7400-4f96-9b48-da2bfd48965c", "Trolley should be of type '{0}'.", trolleyType.ToString());
			}
		}

		void PickTrolley(WhsPickTrolleyWebServiceResponse response, WhsPickTrolleyJob pickTrolleyJob, string trolleyNumber, TrolleyPickingType trolleyType, bool shouldOverrideOtherUsers)
		{
			AddFetchHintsForPickTrolley(pickTrolleyJob);

			// When picking the RF gun doesn't know what type the trolley is so it asks for None (any trolley).
			// We also check the type we get from the DB is the type we asked for.
			if (pickTrolleyJob.PickingType == trolleyType || trolleyType == TrolleyPickingType.None)
			{
				var warehouse = WebServiceHelper.GetWarehouse(Factory, SecurityHeader.WarehouseCode);
				if (VerifyTrolleyWarehouse(response, trolleyNumber, pickTrolleyJob, warehouse))
				{
					response.Job = CreateTrolleyJobInfo(warehouse, pickTrolleyJob, trolleyNumber, trolleyType);
					PopulatePickSpecificInformation(response, pickTrolleyJob, shouldOverrideOtherUsers);
				}
			}
			else
			{
				response.Error = ErrorTypes.BusinessValidationError;
				response.ErrorMessage = Res.GetString("a023c69d-8d5e-49bd-ab89-ec2b4a8f29c4", "Couldn't find existing trolley with pick type '{0}'.", trolleyType.ToString());
			}
		}

		void AddFetchHintsForPickTrolley(WhsPickTrolleyJob pickTrolleyJob)
		{
			foreach (var slot in pickTrolleyJob.Slots)
			{
				pickTrolleyJob.Factory.AddFetchHint(PkgPackageSchema.PK, slot.WTS_KP_Package);

				pickTrolleyJob.Factory.AddFetchHint(GenAddOnColumnSchema.Instance,
					new ZQuery(GenAddOnColumnSchema.XA_ParentID, slot.WTS_KP_Package));

				var query = new ZDBOnlyQuery(typeof(PkgPackageJob));
				var subQuery = new ZDBOnlySubQuery(typeof(PkgPackage), PkgPackageSchema.KP_KJ_ParentPackageJob);
				subQuery.AddToFilter(PkgPackageSchema.PK, slot.WTS_KP_Package);
				query.AddSubQuery(subQuery, JoinCondition.And);
				pickTrolleyJob.Factory.AddFetchHint(PkgPackageJobSchema.Instance, query);
			}
		}

		TrolleyJobInfo CreateTrolleyJobInfo(WhsWarehouse warehouse, WhsPickTrolleyJob pickTrolleyJob, string trolleyNumber, TrolleyPickingType trolleyType)
		{
			var trolleyJobInfo = new TrolleyJobInfo(warehouse, pickTrolleyJob)
			{
				Reference = trolleyNumber
			};

			// if we ask for none, we should return the pick type of what we got from the DB
			if (trolleyJobInfo.TrolleyPickType == TrolleyPickingType.None)
			{
				trolleyJobInfo.TrolleyPickType = trolleyType;
			}

			return trolleyJobInfo;
		}

		#region HandleTrolleyPickingOptionForTrolleyJob

		WhsPickTrolleyJob HandleTrolleyPickingOptionForTrolleyJob(WebServiceResponse response, string trolleyNumber, TrolleyJobStatus trolleyStatus, RefEquipment refEquipment)
		{
			var query = new ZQuery(WhsPickTrolleyJobSchema.WTJ_RQ_Equipment, refEquipment.PK);
			query.AddToFilter(WhsPickTrolleyJobSchema.WTJ_Status, SQLComparisonOperator.NotEqual, PickTrolleyStatus.Codes.Finalised);
			var pickTrolleyJob = Factory.LoadTop1<WhsPickTrolleyJob>(query);

			if (trolleyStatus == TrolleyJobStatus.Building)
			{
				if (pickTrolleyJob != null && pickTrolleyJob.WTJ_Status == PickTrolleyStatus.Codes.Picking)
				{
					var rfUser = WebServiceHelper.GetStaff(Factory, SecurityHeader.UserName);
					Argument.NotNull(rfUser, nameof(rfUser));

					var allPickLines = Factory.Load<WhsPickLine>(GetAllRelatedPickLinesQuery(pickTrolleyJob));
					var anyLinesAvailableForPickingOrPutaway = allPickLines.Any(pl => !pl.IsPickedFromPutawayLocation || IsPickLineAvailableForPutaway(pl, rfUser));
					if (anyLinesAvailableForPickingOrPutaway)
					{
						response.Error = ErrorTypes.BusinessValidationError;
						response.ErrorMessage = Res.GetString("9ac6d65b-ac35-470b-b70c-a1adf18c8813", "Trolley '{0}' is not available for building. You need to finish Picking this Trolley first, or investigate the status of the trolley by looking up the Trolley # in CW1.", trolleyNumber);
					}
					else // Edge case, the job should be finalised
					{
						ChangeTrolleyJobStatus(response, pickTrolleyJob.PK.ToGuid(), TrolleyJobStatus.Finalised);

						if (string.IsNullOrEmpty(response.ErrorMessage))
						{
							pickTrolleyJob = null;
						}
					}
				}

				if (pickTrolleyJob == null)
				{
					pickTrolleyJob = Factory.New<WhsPickTrolleyJob>();
					pickTrolleyJob.WTJ_RQ_Equipment = refEquipment.PK;
					pickTrolleyJob.WTJ_Status = PickTrolleyStatus.Codes.Building;
				}
			}
			else if (trolleyStatus == TrolleyJobStatus.Picking)
			{
				if (pickTrolleyJob == null || pickTrolleyJob.WTJ_Status != PickTrolleyStatus.Codes.Picking)
				{
					response.Error = ErrorTypes.BusinessValidationError;
					response.ErrorMessage = Res.GetString("aaf3a5b2-3616-406a-9146-e14871b93c59", "Trolley '{0}' is not available for picking. You need to finish Building that Trolley first.", trolleyNumber);
				}
			}
			else
			{
				throw new NotSupportedException("TrolleyJobStatus " + trolleyStatus + " is not supported.");
			}

			return pickTrolleyJob;
		}

		#endregion

		#region VerifyTrolleyWarehouse

		bool VerifyTrolleyWarehouse(WebServiceResponse response, string trolleyNumber, WhsPickTrolleyJob pickTrolleyJob, WhsWarehouse warehouse)
		{
			var orderPKsForTrolleyJob = pickTrolleyJob.Slots.Select(sl => sl.Package.PackageJob.KJ_ParentID).Distinct();
			if (orderPKsForTrolleyJob.Any())
			{
				var ordersQuery = new ZQuery(WhsDocketSchema.PK, orderPKsForTrolleyJob);
				ordersQuery.AddToFilter(WhsDocketSchema.WD_WW_Whs, SQLComparisonOperator.NotEqual, warehouse.PK);
				if (Factory.ExistsInDatabase(WhsDocketSchema.Constants.TableName, ordersQuery)) // if there are orders for other warehouses
				{
					response.Error = ErrorTypes.BusinessValidationError;
					response.ErrorMessage = Res.GetString("2d2e527e-9420-4f2f-95fa-d5be00529da7", "Trolley '{0}' is being used by another warehouse.", trolleyNumber);
				}
			}

			return response.Error == ErrorTypes.None;
		}

		#endregion

		#region PopulatePickSpecificInformation

		void PopulatePickSpecificInformation(WhsPickTrolleyWebServiceResponse response, WhsPickTrolleyJob trolleyJob, bool shouldOverrideOtherUsers)
		{
			var factory = trolleyJob.Factory;
			var trolleyJobInfo = response.Job;
			var rfUser = WebServiceHelper.GetStaff(Factory, SecurityHeader.UserName);

			if (rfUser != null)
			{
				var allPickLines = factory.Load<WhsPickLine>(GetAllRelatedPickLinesQuery(trolleyJob));
				var notPickedPickLines = allPickLines.Where(l => !l.IsPickedFromPutawayLocation).ToArray();

				WebServiceHelper.CheckPickability(response, rfUser, notPickedPickLines, shouldOverrideOtherUsers);
				if (string.IsNullOrEmpty(response.ErrorMessage))
				{
					SortedDictionary<WhsPickLine, PickLineAdditionalInfo> pickLinesWithPackageIDAndSlots = null;
					if (notPickedPickLines.Length > 0)
					{
						pickLinesWithPackageIDAndSlots = GetPickLinesWithPackageIDAndSlots(factory, notPickedPickLines);
						if (pickLinesWithPackageIDAndSlots.Count == 0)
						{
							var componentPickLines = allPickLines
								.Select(pl => ((WhsOrderLine)pl.DocketLine))
								.Where(dl => dl.IsBOMProductPickedOnSalesOrder)
								.SelectMany(dl => dl.ChildComponentLines.SelectMany(cl => cl.PickLines));

							allPickLines = allPickLines.Concat(componentPickLines).ToArray();
						}
					}

					if (notPickedPickLines.Length == 0 || pickLinesWithPackageIDAndSlots.Count == 0)
					{
						var isReadyToPutaway = allPickLines.Any(pl => IsPickLineAvailableForPutaway(pl, rfUser));
						if (isReadyToPutaway)
						{
							trolleyJobInfo.IsPutawayOnly = true;
						}
						else // Exceptional case when all items from all packages had been picked/putaway in some other way, not through RF gun.
						{
							response.Job = null;
							ChangeTrolleyJobStatus(response, trolleyJob.PK.ToGuid(), TrolleyJobStatus.Finalised);
							if (string.IsNullOrEmpty(response.ErrorMessage))
							{
								response.Error = ErrorTypes.BusinessValidationError;
								response.ErrorMessage = Res.GetString("cf987a7d-1e7b-444f-a66b-02900f43f69a", "All packages for this trolley are already picked. You need to build a new trolley.");
							}
						}
					}
					else
					{
						trolleyJobInfo.SetLines(new WhsPickLineInfoCollection(pickLinesWithPackageIDAndSlots, trolleyJobInfo)); // will also populate products and product attributes

						var relatedOrders = GetAllRelatedOrders(factory, allPickLines);
						trolleyJobInfo.Orders = new WhsDocketInfoCollection(relatedOrders, shouldCreateDocketLines: false);

						TrolleyJobInfo.PopulateUnitConversionsPerProduct(pickLinesWithPackageIDAndSlots.Keys, trolleyJobInfo);
						foreach (var pickLine in notPickedPickLines)
						{
							pickLine.WZ_GS_NKAssignedTo = rfUser.GS_Code;
							pickLine.WZ_IsPicking = true;
						}
					}
				}
			}
		}

		static bool IsPickLineAvailableForPutaway(WhsPickLine pickLine, GlbStaff rfUser)
		{
			var result = false;

			if (!pickLine.WZ_WE_OriginalPickedInventoryLine.IsEmpty)
			{
				var dockDoorTransferLine = pickLine.InventoryLine;
				result = dockDoorTransferLine.WE_GS_NKPutawayBy == rfUser.GS_Code && !dockDoorTransferLine.IsFinalised;
			}

			return result;
		}

		#region GetAllRelatedPickLinesQuery

		ZDBOnlyQuery GetAllRelatedPickLinesQuery(WhsPickTrolleyJob trolleyJob)
		{
			var trolleySlotSubQuery = new ZDBOnlySubQuery(typeof(WhsPickTrolleySlot), WhsPickTrolleySlotSchema.WTS_KP_Package);
			trolleySlotSubQuery.AddToFilter(WhsPickTrolleySlotSchema.WTS_WTJ_TrolleyJob, trolleyJob.PK);

			var pkgPackageSubQuery = new ZDBOnlySubQuery(typeof(PkgPackage), PkgPackageItemDivotSchema.KI_KP_Package);
			pkgPackageSubQuery.AddSubQuery(trolleySlotSubQuery, JoinCondition.And);

			var pkgPackageItemDivotSubQuery = new ZDBOnlySubQuery(typeof(PkgPackageItemDivot), PkgPackageItemDivotSchema.KI_ParentID);
			pkgPackageItemDivotSubQuery.AddSubQuery(pkgPackageSubQuery, JoinCondition.And);

			var result = new ZDBOnlyQuery(typeof(WhsPickLine));
			result.AddSubQuery(pkgPackageItemDivotSubQuery, JoinCondition.And);

			return result;
		}

		#endregion

		#region GetPickLinesWithPackageIDAndSlots

		SortedDictionary<WhsPickLine, PickLineAdditionalInfo> GetPickLinesWithPackageIDAndSlots(BusinessObjectFactory factory, IEnumerable<WhsPickLine> notPickedPickLines)
		{
			var result = new SortedDictionary<WhsPickLine, PickLineAdditionalInfo>(new SortPickLinesForTrolleyPicking());
			if (notPickedPickLines.Any())
			{
				var rawSql = @"
SELECT
	WZ_PK,
	OrderLine.WE_OP,
	KPH_PackageID,
	WTS_SlotNumber,
	CAST(CASE WHEN Docket.WD_WP_ParentPickForReceive IS NOT NULL THEN 1 ELSE 0 END AS BIT) AS IsAssembledKitLine
FROM
	dbo.WhsPickLine
	JOIN dbo.WhsDocketLine OrderLine ON OrderLine.WE_PK = WZ_WE_TransactionLine
	JOIN dbo.PkgPackageItemDivot ON KI_ParentID = WZ_PK
	JOIN dbo.PkgPackage ON KP_PK = KI_KP_Package
	JOIN dbo.PkgPackageHeader ON KPH_PK = KP_KPH_PackageHeader
	JOIN dbo.WhsPickTrolleySlot ON WTS_KP_Package = KP_PK
	JOIN dbo.WhsDocketLine InventoryLine ON InventoryLine.WE_PK = WZ_WE_InventoryLine
	JOIN dbo.WhsDocket Docket ON Docket.WD_PK = InventoryLine.WE_WD
WHERE
	WZ_PK IN (SELECT VALUE FROM @PickLinePKs)
";

				var pickLinesWithAdditionalInformation = new DynamicBusinessObjectCollection(factory);
				var tvp = ZSqlParameter.New("@PickLinePKs", notPickedPickLines.Select(l => l.PK).ToArray(), WhsPickLineSchema.PK, isTableValued: true);
				pickLinesWithAdditionalInformation.Load(rawSql, new[] { tvp });

				var (componentLineMap, bomParts) = LoadComponentPickLines(pickLinesWithAdditionalInformation);

				factory.AddFetchHint(WhsLocationSchema.Instance, new ZQuery(WhsLocationSchema.PK, notPickedPickLines.Select(l => l.InventoryLineForAvailableInventory.WE_WL)));

				foreach (var line in pickLinesWithAdditionalInformation)
				{
					var pickLinePK = (ZGuid)line[WhsPickLineSchema.Constants.PK];
					var packageID = (ZString)line[PkgPackageHeaderSchema.Constants.KPH_PackageID];
					var slotNumber = (ZShort)line[WhsPickTrolleySlotSchema.Constants.WTS_SlotNumber];
					var notPickedPickLine = notPickedPickLines.Single(l => l.PK == pickLinePK);
					if ((ZBool)line["IsAssembledKitLine"])
					{
						foreach (var componentPickLine in GetComponentPickLinesToAdd(notPickedPickLine, bomParts, componentLineMap))
						{
							result.Add(componentPickLine, new PickLineAdditionalInfo(packageID, slotNumber));
						}
					}
					else
					{
						result.Add(notPickedPickLine, new PickLineAdditionalInfo(packageID, slotNumber));
					}
				}
			}

			return result;
		}

		(Dictionary<(ZGuid, ZGuid), HashSet<WhsPickLine>> componentLineMap, Dictionary<BOMPartCacheKey, OrgPartBOM> bomParts) LoadComponentPickLines(DynamicBusinessObjectCollection pickLinesWithAdditionalInformation)
		{
			var assembledKitLinePKs = pickLinesWithAdditionalInformation.Where(i => (ZBool)i["IsAssembledKitLine"])
					.Select(i => (ZGuid)i[WhsPickLineSchema.Constants.PK]).ToHashSet();

			Dictionary<(ZGuid, ZGuid), HashSet<WhsPickLine>> componentLineMap = null;
			Dictionary<BOMPartCacheKey, OrgPartBOM> bomParts = null;
			if (assembledKitLinePKs.Count > 0)
			{
				(componentLineMap, var kitOrderLines) = LoadComponentPickLinesCore(assembledKitLinePKs);
				bomParts = WhsPickByBOMHelper.CacheBOMParts(kitOrderLines);
			}

			return (componentLineMap, bomParts);
		}

		(Dictionary<(ZGuid, ZGuid), HashSet<WhsPickLine>> ComponentLineMap, WhsOrderLine[] KitOrderLines) LoadComponentPickLinesCore(IEnumerable<ZGuid> assembledKitLinePKs)
		{
			var (componentPickLines, relationPKs) = PutawayStockInDockDoorOrPackingStationHelper.LoadComponentPickLinesFromKitPickLines(Factory, assembledKitLinePKs.ToArray(), isUnPickedComponentOnly: true);
			var componentPickLineMap = componentPickLines.ToDictionary(l => l.PK);

			var pickLineMap = new Dictionary<(ZGuid, ZGuid), HashSet<WhsPickLine>>();
			var componentPickLinesByKitOrderLine = new Dictionary<(ZGuid, ZGuid), HashSet<WhsPickLine>>();
			var kitOrderLinePKs = new List<ZGuid>();

			foreach (var (parentPickLinePK, parentOrderLinePK, componentPickLinePK, componentProductPK) in relationPKs)
			{
				kitOrderLinePKs.Add(parentOrderLinePK);

				if (!componentPickLinesByKitOrderLine.TryGetValue((parentOrderLinePK, componentProductPK), out var sharedComponentPickLines))
				{
					sharedComponentPickLines = new HashSet<WhsPickLine>();
					componentPickLinesByKitOrderLine[(parentOrderLinePK, componentProductPK)] = sharedComponentPickLines;
				}
				sharedComponentPickLines.Add(componentPickLineMap[componentPickLinePK]);

				// Share the Component Line HashSet among Kit Pick Lines because it will be altered while processing, we want to read the newest values after splitting/removing.
				if (!pickLineMap.ContainsKey((parentPickLinePK, componentProductPK)))
				{
					pickLineMap[(parentPickLinePK, componentProductPK)] = componentPickLinesByKitOrderLine[(parentOrderLinePK, componentProductPK)];
				}
			}

			var kitOrderLines = kitOrderLinePKs.Count > 0 ? Factory.Load<WhsOrderLine>(new ZQuery(WhsDocketLineSchema.PK, kitOrderLinePKs)) : Array.Empty<WhsOrderLine>();
			return (pickLineMap, kitOrderLines);
		}

		WhsOrderLine[] GetOrderLinesFromPickLine(IEnumerable<ZGuid> kitPickLinePKs)
		{
			var pickLineSubQuery = new ZDBOnlySubQuery(typeof(WhsPickLine), WhsPickLineSchema.WZ_WE_TransactionLine);
			pickLineSubQuery.AddToFilter(WhsPickLineSchema.PK, kitPickLinePKs);

			var orderLineQuery = new ZDBOnlyQuery(typeof(WhsOrderLine));
			orderLineQuery.AddSubQuery(pickLineSubQuery, JoinCondition.And);

			return Factory.Load<WhsOrderLine>(orderLineQuery);
		}

		IEnumerable<WhsPickLine> GetComponentPickLinesToAdd(
			WhsPickLine kitPickLine,
			Dictionary<BOMPartCacheKey, OrgPartBOM> bomParts,
			Dictionary<(ZGuid, ZGuid), HashSet<WhsPickLine>> componentLineMap)
		{
			var result = new List<WhsPickLine>();
			var componentOrderLines = ((WhsOrderLine)kitPickLine.DocketLine).ChildComponentLines;

			foreach (var componentOrderLine in componentOrderLines)
			{
				if (bomParts.TryGetValue(new BOMPartCacheKey(componentOrderLine.WE_OP, componentOrderLine.WE_F3_NKPackType), out var bomPart)
					&& componentLineMap.TryGetValue((kitPickLine.PK, componentOrderLine.WE_OP), out var componentPickLines))
				{
					var componentQtyNeededForThisKitLine = BOMComponentQuantityHelper.GetComponentsQuantityToBuildKits(bomPart, kitPickLine.WZ_Units);
					while (componentQtyNeededForThisKitLine > 0 && componentPickLines.Count > 0)
					{
						var componentPickLine = componentPickLines.First();
						if (componentPickLine.WZ_Units > componentQtyNeededForThisKitLine)
						{
							var newComponentPickLine = componentPickLine.Split(componentPickLine.WZ_Units - componentQtyNeededForThisKitLine);
							componentPickLines.Add(newComponentPickLine);
							componentQtyNeededForThisKitLine = 0m;
						}
						else
						{
							componentQtyNeededForThisKitLine -= componentPickLine.WZ_Units;
						}
						componentPickLines.Remove(componentPickLine);
						result.Add(componentPickLine);
					}
				}
			}

			return result;
		}

		#endregion

		#region GetAllRelatedOrders

		IEnumerable<WhsPickableDocket> GetAllRelatedOrders(BusinessObjectFactory factory, WhsPickLine[] allPickLines)
		{
			var pickLineSubQuery = new ZDBOnlySubQuery(typeof(WhsPickLine), WhsPickLineSchema.WZ_WE_TransactionLine);
			pickLineSubQuery.AddToFilter(WhsPickLineSchema.PK, allPickLines.Select(l => l.PK));

			var orderLineSubQuery = new ZDBOnlySubQuery(typeof(WhsDocketLine), WhsDocketLineSchema.WE_WD);
			orderLineSubQuery.AddSubQuery(pickLineSubQuery, JoinCondition.And);

			var orderQuery = new ZDBOnlyQuery(typeof(WhsDocket));
			orderQuery.AddSubQuery(orderLineSubQuery, JoinCondition.And);

			return factory.Load<WhsPickableDocket>(orderQuery);
		}

		#endregion

		#endregion

		#endregion
	}
}
