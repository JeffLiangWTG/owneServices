using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsPackingConsolidationService : IWhsPackingConsolidationService
	{
		#region CreateDockDoorTransferAndPick

		public string CreateDockDoorTransfer(ZGuid handlingUnitPackagePK) => CreateDockDoorTransferCore(handlingUnitPackagePK);

		string CreateDockDoorTransferCore(ZGuid handlingUnitPackagePK)
		{
			var factory = new BusinessObjectFactory();
			var errorMessage = string.Empty;

			var handlingUnit = factory.Load<PkgPackage>(handlingUnitPackagePK);
			if (handlingUnit?.KP_IsClosed ?? true)
			{
				errorMessage = Res.GetString("578df9e9-d4ce-4948-8679-53888aa16255", "Handling Unit not found or is already Closed.");
			}
			else if (CheckIsLoadHandlingUnitOrIsLoaded(factory, handlingUnitPackagePK))
			{
				errorMessage = HandlingUnitIsNotConsolidationOrIsLoaded;
			}
			else if (!CheckCurrentLocationOfHandlingUnit(factory, handlingUnitPackagePK, LocationClasses.Codes.CON))
			{
				errorMessage = HandlingUnitNotFound;
			}
			else
			{
				var pickLines = GetPickLines(factory, handlingUnitPackagePK);
				if (pickLines.Length > 0)
				{
					if (pickLines.Any(pl => pl.WZ_WE_OriginalPickedInventoryLine.IsEmpty || pl.WZ_PickedDateTime.IsValid))
					{
						errorMessage = Res.GetString("ed166e0e-ba1b-46f0-b8a2-10db84c51919", "Some Pick Lines on this Handling Unit have been picked already or is not picking from a consolidation location.");
					}
					else
					{
						AddFetchHints(factory, pickLines);

						var warehouse = pickLines[0].Pick.Warehouse;
						var now = warehouse.GetWarehouseBranchDateTimeOffset(ZDateTime.UtcNow);

						foreach (var pickLine in pickLines)
						{
							pickLine.WZ_PickedDateTime = now;
						}

						CloseHandlingUnit(handlingUnit);

						var concurrencyErrorMessage = Res.GetString("82af0062-7aa7-4d70-85e0-75a1989d373e", "Another user has changed the Handling Unit. Please restart the operation and try again.");
						errorMessage = WhsWebAPIServiceHelper.SaveFactoryWithExceptionHandling(factory, (concurrencyException) => concurrencyErrorMessage);
					}
				}
				else
				{
					errorMessage = Res.GetString("28a51de8-312c-419e-97bd-c45aea3ab863", "No Pick Lines found.");
				}
			}

			return errorMessage;
		}

		bool CheckIsLoadHandlingUnitOrIsLoaded(BusinessObjectFactory factory, ZGuid handlingUnitPackagePK)
		{
			var pivotQuery = new ZQuery(WhsLoadPkgPackagePivotSchema.WLP_KP_Package, handlingUnitPackagePK);
			return factory.LoadTop1<WhsLoadPkgPackagePivot>(pivotQuery) != null;
		}

		bool CheckCurrentLocationOfHandlingUnit(BusinessObjectFactory factory, ZGuid handlingUnitPackagePK, string expectedLocationType)
		{
			var query = new ZQuery(WhsPackageLocationViewSchema.WPK_KP_Package, handlingUnitPackagePK);
			query.AddToFilter(WhsPackageLocationViewSchema.WPK_LocationClass, expectedLocationType);
			query.AddToFilter(WhsPackageLocationViewSchema.WPK_IsHandlingUnit, true);
			return factory.LoadTop1<WhsPackageLocationView>(query) != null;
		}

		WhsPickLine[] GetPickLines(BusinessObjectFactory factory, ZGuid handlingUnitPackagePK)
		{
			var packageDivotSubQuery = new ZDBOnlySubQuery(typeof(PkgPackageHandlingUnitDivot), PkgPackageHandlingUnitDivotSchema.KPD_KP_Package);
			packageDivotSubQuery.AddToFilter(PkgPackageHandlingUnitDivotSchema.KPD_KP_HandlingUnit, handlingUnitPackagePK);
			packageDivotSubQuery.AddToFilter(PkgPackageHandlingUnitDivotSchema.KPD_UnpackedTime, null);

			var packageSubQuery = new ZDBOnlySubQuery(typeof(PkgPackage), PkgPackageSchema.PK);
			packageSubQuery.AddSubQuery(packageDivotSubQuery, JoinCondition.And);

			var packedItemSubQuery = new ZDBOnlySubQuery(typeof(PkgPackageItemDivot), PkgPackageItemDivotSchema.KI_ParentID);
			packedItemSubQuery.AddSubQuery(PkgPackageItemDivotSchema.KI_KP_Package, packageSubQuery, JoinCondition.And);

			var pickLineQuery = new ZDBOnlyQuery(typeof(WhsPickLine));
			pickLineQuery.AddSubQuery(packedItemSubQuery, JoinCondition.And);

			return factory.Load<WhsPickLine>(pickLineQuery);
		}

		void AddFetchHints(BusinessObjectFactory factory, WhsPickLine[] pickLines)
		{
			var transferLinePKs = new HashSet<ZGuid>();
			var orderLinePKs = new HashSet<ZGuid>();
			foreach (var pickLine in pickLines)
			{
				transferLinePKs.Add(pickLine.WZ_WE_InventoryLine);
				orderLinePKs.Add(pickLine.WZ_WE_TransactionLine);
			}

			foreach (var transferLinePK in transferLinePKs)
			{
				var matchLineQuery = new ZQuery(WhsDocketLineSchema.WE_WE_MatchingLine, transferLinePK);
				matchLineQuery.AddToFilter(WhsDocketLineSchema.WE_IsOriginalInventory, true);
				factory.AddFetchHint(WhsDocketLineSchema.Instance, matchLineQuery);
			}
			foreach (var orderLinePK in orderLinePKs)
			{
				factory.AddFetchHint(WhsDocketLineSchema.PK, orderLinePK);
				factory.AddFetchHint(WhsPickLineSchema.Instance, new ZQuery(WhsPickLineSchema.WZ_WE_TransactionLine, orderLinePK));
			}

			WhsPickByBOMHelper.AddFetchHintsForIsPickByBOMKitPickLine(factory, pickLines);

			var dockets = GetDockets(factory, orderLinePKs.AsEnumerable().Union(transferLinePKs).ToArray());

			foreach (var docket in dockets)
			{
				if (docket is WhsOrder)
				{
					factory.AddFetchHint(StmALogSchema.SL_Parent, docket.WD_WP);
					factory.AddFetchHint(WhsDocketSchema.WD_WP_ParentPickForTransfer, docket.WD_WP);
					factory.AddFetchHint(WhsDocketLineSchema.WE_WD, docket.PK);
					factory.AddFetchHint(WhsPickSchema.PK, docket.WD_WP);
					AddFindDocketFromPickFetchHint(factory, docket.WD_WP);
				}
				else if (docket is WhsTransfer)
				{
					var transferLineQuery = new ZQuery(WhsDocketLineSchema.WE_WD, docket.PK);
					var query = new ZQuery(WhsDocketLineSchema.WE_IsOriginalInventory, true);
					query.AddToFilter(WhsDocketLineSchema.WE_WE_MatchingLine, null);
					transferLineQuery.AddToFilter(query);
					factory.AddFetchHint(WhsDocketLineSchema.Instance, transferLineQuery);

					factory.AddFetchHint(WhsVASOrderSchema.Instance, new ZQuery(WhsVASOrderSchema.WVO_WD_TransferIntoServiceArea, docket.PK));
					factory.AddFetchHint(WhsVASOrderSchema.Instance, new ZQuery(WhsVASOrderSchema.WVO_WD_TransferOutOfServiceArea, docket.PK));
				}
			}
		}

		static void AddFindDocketFromPickFetchHint(BusinessObjectFactory factory, ZGuid pickPK)
		{
			var query = new ZQuery();
			query.AddToFilter(WhsDocketSchema.WD_DocketType, new[] { DocketType.Codes.Order, DocketType.Codes.WorkOrder, DocketType.Codes.DynamicWorkOrder });
			query.AddToFilter(WhsDocketSchema.WD_WP, pickPK);
			factory.AddFetchHint(WhsDocketSchema.Instance, query);
		}

		WhsDocket[] GetDockets(BusinessObjectFactory factory, ZGuid[] docketLinePKs)
		{
			var docketLineSubQuery = new ZDBOnlySubQuery(typeof(WhsDocketLine), WhsDocketLineSchema.WE_WD);
			docketLineSubQuery.AddToFilter(WhsDocketLineSchema.PK, docketLinePKs);

			var docketQuery = new ZDBOnlyQuery(typeof(WhsDocket));
			docketQuery.AddSubQuery(docketLineSubQuery, JoinCondition.And);

			return factory.Load<WhsDocket>(docketQuery);
		}

		void CloseHandlingUnit(PkgPackage handlingUnit)
		{
			handlingUnit.KP_ClosedTimeUtc = ZDateTime.UtcNow;
			handlingUnit.KP_IsClosed = true;
			handlingUnit.KP_GS_NKClosedBy = GlbStaff.CurrentUser.GS_Code;
		}

		#endregion

		#region PutawayStockInDockDoor

		public string PutawayStockInDockDoor(ZGuid handlingUnitPackagePK, ZGuid dockDoorLocationPK, ZGuid warehousePK)
			=> PutawayStockInDockDoorCore(handlingUnitPackagePK, dockDoorLocationPK, warehousePK);

		string PutawayStockInDockDoorCore(ZGuid handlingUnitPackagePK, ZGuid dockDoorLocationPK, ZGuid warehousePK)
		{
			var factory = new BusinessObjectFactory();
			var errorMessage = string.Empty;

			var warehouse = factory.Load<WhsWarehouse>(warehousePK);
			if (warehouse == null)
			{
				errorMessage = WarehouseNotFound;
			}
			else
			{
				var dockDoorLocation = factory.Load<WhsLocation>(dockDoorLocationPK);
				if (dockDoorLocation == null)
				{
					errorMessage = Res.GetString("d87db5b0-445f-4c56-92e3-b2343de231af", "Location not found.");
				}
				else if (!dockDoorLocation.IsDockDoorLocation)
				{
					errorMessage = Res.GetString("562e194c-6c5e-48c6-b4d9-d258b58caccd", "Location {0} is not an Outbound Dock Door Location.", dockDoorLocation.WLV_LocationString_UserFriendly);
				}
				else if (CheckIsLoadHandlingUnitOrIsLoaded(factory, handlingUnitPackagePK))
				{
					errorMessage = HandlingUnitIsNotConsolidationOrIsLoaded;
				}
				else if (!CheckCurrentLocationOfHandlingUnit(factory, handlingUnitPackagePK, LocationClasses.Codes.DDL))
				{
					errorMessage = HandlingUnitNotFound;
				}
				else
				{
					var locationPKOnLoad = GetLocationPKOnLoad(factory, handlingUnitPackagePK);
					if (locationPKOnLoad != ZGuid.Empty && locationPKOnLoad != dockDoorLocation.PK)
					{
						errorMessage = Res.GetString("fcdedd0d-2ef8-4394-a99a-1b85416b2b46", "You need to Putaway to the Planned Load configured Dock Door Location.");
					}
					else
					{
						var transitLinesToPutaway = GetTransferLinesToPutaway(factory, handlingUnitPackagePK).ToArray();
						var inTransitLinesCount = transitLinesToPutaway.Count(l => l.WE_GS_NKPutawayBy == GlbStaff.CurrentUser.GS_Code && l.WE_CurrentInventoryStatus == InventoryStatus.Codes.InTransit);
						if (transitLinesToPutaway.Length == 0 || transitLinesToPutaway.Length != inTransitLinesCount)
						{
							errorMessage = Res.GetString("9b061f40-d562-4def-8b57-60a09fe8c16f", "Wrong status of Handling Unit. Either is In-Transit by another staff, or Put has already been completed.");
						}
						else
						{
							AddFetchHints(factory, transitLinesToPutaway);

							WhsPick[] picks = null;
							var dockDoorAssignmentService = ObjectFactory.New<IWhsPickDockDoorAssignmentService>();
							if (transitLinesToPutaway.Any(l => l.WE_WL != dockDoorLocationPK))
							{
								errorMessage = dockDoorAssignmentService.GetIsDockDoorOverrideAllowedForHandlingUnit(handlingUnitPackagePK, factory);
								if (string.IsNullOrEmpty(errorMessage))
								{
									picks = GetPicksFromHandlingUnit(factory, handlingUnitPackagePK);
									dockDoorAssignmentService.OverrideDockDoorLocationsOfPicks(picks, dockDoorLocationPK, factory);
								}
							}

							if (string.IsNullOrEmpty(errorMessage))
							{
								picks ??= GetPicksFromHandlingUnit(factory, handlingUnitPackagePK);
								AddFetchHintsForDockDoorAssignmentPutaway(factory, picks);
								errorMessage = dockDoorAssignmentService.TryCreateAndPutawayDockDoorAssignmentForPicks(picks, factory);
							}

							if (string.IsNullOrEmpty(errorMessage))
							{
								errorMessage = PutawayTransferLines(dockDoorLocation, transitLinesToPutaway);
							}

							if (string.IsNullOrEmpty(errorMessage))
							{
								var concurrencyErrorMessage = Res.GetString("fcf5b1d2-a9e4-4de1-aa7e-a2dca1f78b23", "Another user has changed the putaway job while you have been working on it. Please restart the operation and try again.");
								errorMessage = WhsWebAPIServiceHelper.SaveFactoryWithExceptionHandling(factory, (concurrencyException) => concurrencyErrorMessage);
							}
						}
					}
				}
			}

			return errorMessage;
		}

		ZGuid GetLocationPKOnLoad(BusinessObjectFactory factory, ZGuid handlingUnitPackagePK)
		{
			var handlingUnitPackages = new DynamicBusinessObjectCollection(factory);
			var sql = "SELECT WHP_WL_LoadDockDoorLocation FROM dbo.WhsHandlingUnitPackage WHERE KP_PK = @HandlingUnitPackagePK";
			var sqlParams = new ZSqlParameterCollection();
			sqlParams.Add(ZSqlParameter.New("@HandlingUnitPackagePK", handlingUnitPackagePK, PkgPackageHandlingUnitDivotSchema.KPD_KP_Package));
			handlingUnitPackages.Load(sql, sqlParams);
			return handlingUnitPackages.Count > 0 ? (ZGuid)handlingUnitPackages[0]["WHP_WL_LoadDockDoorLocation"] : ZGuid.Empty;
		}

		WhsTransferLine[] GetTransferLinesToPutaway(BusinessObjectFactory factory, ZGuid handlingUnitPackagePK)
		{
			var packageDivotSubQuery = new ZDBOnlySubQuery(typeof(PkgPackageHandlingUnitDivot), PkgPackageHandlingUnitDivotSchema.KPD_KP_Package);
			packageDivotSubQuery.AddToFilter(PkgPackageHandlingUnitDivotSchema.KPD_KP_HandlingUnit, handlingUnitPackagePK);
			packageDivotSubQuery.AddToFilter(PkgPackageHandlingUnitDivotSchema.KPD_UnpackedTime, null);

			var packageSubQuery = new ZDBOnlySubQuery(typeof(PkgPackage), PkgPackageSchema.PK);
			packageSubQuery.AddSubQuery(packageDivotSubQuery, JoinCondition.And);

			var packedItemSubQuery = new ZDBOnlySubQuery(typeof(PkgPackageItemDivot), PkgPackageItemDivotSchema.KI_ParentID);
			packedItemSubQuery.AddSubQuery(PkgPackageItemDivotSchema.KI_KP_Package, packageSubQuery, JoinCondition.And);

			var pickLineSubQuery = new ZDBOnlySubQuery(typeof(WhsPickLine), WhsPickLineSchema.WZ_WE_InventoryLine);
			pickLineSubQuery.AddSubQuery(packedItemSubQuery, JoinCondition.And);

			var transferLineQuery = new ZDBOnlyQuery(typeof(WhsTransferLine));
			transferLineQuery.AddSubQuery(pickLineSubQuery, JoinCondition.And);

			var transferLineSubQuery = new ZDBOnlySubQuery(typeof(WhsTransferLine), WhsDocketLineSchema.WE_WD);
			transferLineSubQuery.AddSubQuery(pickLineSubQuery, JoinCondition.And);

			var transferFetchHintQuery = new ZDBOnlyQuery(typeof(WhsTransfer));
			transferFetchHintQuery.AddSubQuery(transferLineSubQuery, JoinCondition.And);
			factory.AddFetchHint(WhsDocketSchema.Instance, transferFetchHintQuery);

			return factory.Load<WhsTransferLine>(transferLineQuery);
		}

		void AddFetchHints(BusinessObjectFactory factory, WhsTransferLine[] transitLinesToPutaway)
		{
			var originalReceiveLinePKs = new HashSet<ZGuid>();
			foreach (var transferLine in transitLinesToPutaway)
			{
				var matchLineQuery = new ZQuery(WhsDocketLineSchema.WE_WE_MatchingLine, transferLine.PK);
				matchLineQuery.AddToFilter(WhsDocketLineSchema.WE_IsOriginalInventory, true);
				factory.AddFetchHint(WhsDocketLineSchema.Instance, matchLineQuery);

				var childQuery = new ZQuery(WhsDocketLineSchema.WE_WE_ParentDocketLine, transferLine.PK);
				childQuery.AddToFilter(WhsDocketLineSchema.WE_IsOriginalInventory, true);
				factory.AddFetchHint(WhsDocketLineSchema.Instance, childQuery);

				factory.AddFetchHint(WhsInventoryViewSchema.WI_WE_InDocketLine, transferLine.PK);
				factory.AddFetchHint(WhsPickLineSchema.WZ_WE_TransactionLine, transferLine.PK);
				factory.AddFetchHint(WhsDocketLineSchema.Constants.TableName, transferLine.WE_WE_OriginalDocketLineForRating);
				originalReceiveLinePKs.Add(transferLine.WE_WE_OriginalDocketLineForRating);
			}

			AddFetchHintsForDockets(factory, originalReceiveLinePKs);
		}

		#region GetPicksFromHandlingUnit

		static WhsPick[] GetPicksFromHandlingUnit(BusinessObjectFactory factory, ZGuid handlingUnitPackagePK)
		{
			var huDivotSubQuery = new ZDBOnlySubQuery(typeof(PkgPackageHandlingUnitDivot), PkgPackageHandlingUnitDivotSchema.KPD_KP_Package);
			huDivotSubQuery.AddToFilter(PkgPackageHandlingUnitDivotSchema.KPD_KP_HandlingUnit, handlingUnitPackagePK);
			var pickQuery = WhsPickDockDoorAssignmentService.GetPickQuery(huDivotSubQuery);

			var picks = factory.Load<WhsPick>(pickQuery);
			var pickPKs = picks.Select(p => p.PK).ToArray();
			return picks;
		}

		#endregion

		static void AddFetchHintsForDockDoorAssignmentPutaway(BusinessObjectFactory factory, WhsPick[] picks)
		{
			foreach (var pick in picks)
			{
				AddFindDocketFromPickFetchHint(factory, pick.PK);
			}

			var orders = picks.SelectMany(p => p.Orders).ToArray();
			foreach (var order in orders)
			{
				factory.AddFetchHint(ProcessTasksSchema.P9_ParentID, order.PK);
			}
		}

		public static string PutawayTransferLines(WhsLocation location, IEnumerable<WhsTransferLine> inTransitLines)
			=> PutawayTransferLinesCore(location, inTransitLines, null);

		public static string PutawayTransferLines(WhsLocation location, IEnumerable<WhsTransferLine> inTransitLines, HashSet<WhsTransferLine> transferLinesToIgnoreWhenSettingLocation)
			=> PutawayTransferLinesCore(location, inTransitLines, transferLinesToIgnoreWhenSettingLocation);

		static string PutawayTransferLinesCore(WhsLocation location, IEnumerable<WhsTransferLine> inTransitLines, HashSet<WhsTransferLine> transferLinesToIgnoreWhenSettingLocation)
		{
			var errorMessage = string.Empty;
			var linesByDocket = new Dictionary<ZGuid, List<WhsTransferLine>>();
			foreach (var line in inTransitLines)
			{
				if (transferLinesToIgnoreWhenSettingLocation == null || !transferLinesToIgnoreWhenSettingLocation.Contains(line))
				{
					line.WE_WL = location.PK;
				}

				if (!linesByDocket.TryGetValue(line.WE_WD, out var linesForTransfer))
				{
					linesByDocket[line.WE_WD] = linesForTransfer = new List<WhsTransferLine>();
				}

				linesForTransfer.Add(line);
			}

			AddFetchHintsForDockets(location.Factory, inTransitLines.Select(t => t.WE_WE_OriginalDocketLineForRating));
			foreach (var transfersLines in linesByDocket.Values)
			{
				var transfer = transfersLines[0].Docket;
				transfer.ValidateAndFinaliseDocketLines(transfersLines, confirmFinalise: false);

				if (transfer.HasErrors)
				{
					errorMessage = Res.GetString("c92dcc56-5968-40ff-831b-e6e2101d2e84", "Failed to Putaway inventory: {0}.", transfer.GetErrors().ToUniqueMessageListString());
					break;
				}
			}

			return errorMessage;
		}

		static void AddFetchHintsForDockets(BusinessObjectFactory factory, IEnumerable<ZGuid> receiveLinePKs)
		{
			if (receiveLinePKs.Any())
			{
				var receiveQuery = new ZDBOnlyQuery(typeof(WhsDocket));
				var receiveLineQuery = new ZDBOnlySubQuery(typeof(WhsDocketLine), WhsDocketLineSchema.WE_WD);
				receiveLineQuery.AddToFilter(WhsDocketLineSchema.PK, receiveLinePKs);
				receiveQuery.AddSubQuery(receiveLineQuery, JoinCondition.And);
				factory.AddFetchHint(WhsDocketSchema.Instance, receiveQuery);
			}
		}

		#endregion

		#region GenerateHandlingUnit

		public string GenerateHandlingUnit(ZGuid handlingUnitPK, ZGuid warehousePK, ZGuid? loadPK)
		{
			var errorMessage = string.Empty;
			var factory = new BusinessObjectFactory();
			var warehouse = factory.Load<WhsWarehouse>(warehousePK);

			if (warehouse == null)
			{
				errorMessage = WarehouseNotFound;
			}
			else
			{
				if (factory.Load<PkgHandlingUnit>(handlingUnitPK) != null)
				{
					errorMessage = Res.GetString("e00df751-7ca3-46cb-8245-7d5d52291d13", "Handling Unit already exists.");
				}
				else
				{
					CreateNewPkgHandlingUnit(factory, handlingUnitPK, warehouse);
					var packageJob = CreateNewPkgPackageJob(factory, handlingUnitPK);
					var package = CreateNewPkgPackage(factory, packageJob.PK);

					if (loadPK.HasValue && loadPK.Value.IsValid)
					{
						if (factory.Load<WhsLoad>((ZGuid)loadPK) != null)
						{
							CreateNewWhsLoadPkgPackagePivot(factory, package.PK, (ZGuid)loadPK);
						}
						else
						{
							errorMessage = Res.GetString("7eeb555d-98b2-4694-a906-1aaa9f1a10b2", "Load not found.");
						}
					}

					if (string.IsNullOrEmpty(errorMessage))
					{
						var concurrencyErrorMessage = Res.GetString("eaa7b1f8-42d1-4a18-868e-998b9c19bae0", "Another user may have modified the Handling Unit. Please try again.");
						errorMessage = WhsWebAPIServiceHelper.SaveFactoryWithExceptionHandling(factory, (concurrencyException) => concurrencyErrorMessage);
					}
				}
			}
			return errorMessage;
		}

		void CreateNewPkgHandlingUnit(BusinessObjectFactory factory, ZGuid handlingUnitPK, WhsWarehouse warehouse)
		{
			var handlingUnit = factory.NewWithPrimaryKey<PkgHandlingUnit>(handlingUnitPK.ToGuid());
			handlingUnit.KPU_JobContext = "3PL";
			handlingUnit.KPU_GB_Branch = warehouse.WW_GB_RelatedCompanyBranch;
		}

		PkgPackageJob CreateNewPkgPackageJob(BusinessObjectFactory factory, ZGuid handlingUnitPK)
		{
			var packageJob = factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = handlingUnitPK;
			packageJob.KJ_ParentTableCode = "KPU";
			return packageJob;
		}

		PkgPackage CreateNewPkgPackage(BusinessObjectFactory factory, ZGuid packageJobPK)
		{
			var package = factory.New<PkgPackage>();
			package.KP_F3_NKPackType = "PKG";
			package.KP_KJ_ParentPackageJob = packageJobPK;
			((ISupportPackageIDGeneration)package).ShouldGenerateIDOnSaving = true;
			return package;
		}

		void CreateNewWhsLoadPkgPackagePivot(BusinessObjectFactory factory, ZGuid handlingUnitPackagePK, ZGuid loadPK)
		{
			var loadPivot = factory.New<WhsLoadPkgPackagePivot>();
			loadPivot.WLP_KP_Package = handlingUnitPackagePK;
			loadPivot.WLP_WLO_Load = loadPK;
		}

		#endregion

		string WarehouseNotFound => Res.GetString("b5ec453f-c8c8-4d27-85c3-865736fc26a4", "Warehouse not found.");
		string HandlingUnitNotFound => Res.GetString("1ce69020-3c6d-4d84-937c-fe169bf18523", "Handling Unit not found or is in the wrong Location.");
		string HandlingUnitIsNotConsolidationOrIsLoaded => Res.GetString("5811b793-f567-4c5b-aed6-c3b42a248cd0", "Handling Unit is not a Consolidation Handling Unit or is already loaded.");
	}
}
