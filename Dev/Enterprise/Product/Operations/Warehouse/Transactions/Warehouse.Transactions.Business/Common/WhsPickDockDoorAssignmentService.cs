using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsPickDockDoorAssignmentService : IWhsPickDockDoorAssignmentService
	{
		public string AddPackageToHandlingUnit(ZGuid packagePK, ZGuid handlingUnitPackagePK)
		{
			var errorMessage = string.Empty;
			var factory = new BusinessObjectFactory();

			var package = factory.Load<PkgPackage>(packagePK);
			if (package == null)
			{
				errorMessage = Res.GetString("c000201e-90e7-4a8d-bd10-dd35291397a6", "Package could NOT be loaded using '{0}'.", packagePK);
			}
			else
			{
				var divot = factory.New<PkgPackageHandlingUnitDivot>();
				divot.KPD_KP_Package = packagePK;
				divot.KPD_KP_HandlingUnit = handlingUnitPackagePK;
				divot.KPD_PackedTime = ZDateTimeOffset.Now;
				divot.KPD_GS_NKPackedUser = GlbStaff.CurrentUser.GS_Code;

				package.KP_KP_TopHandlingUnitPackage = handlingUnitPackagePK;

				if (PackageNotOnLoad(packagePK, factory))
				{
					errorMessage = GeneratePickDockDoorAssignment(packagePK, DockDoorAssignmentLinkType.HandlingUnit, handlingUnitPackagePK, factory);
				}

				if (string.IsNullOrEmpty(errorMessage))
				{
					errorMessage = SaveWithExceptionHandling(factory);
				}
			}

			return errorMessage;
		}

		static string SaveWithExceptionHandling(BusinessObjectFactory factory)
		{
			var concurrencyErrorMessage = Res.GetString("9084ef07-fb48-4c62-ac45-08b3b979490c", "Another user has modified the job. Please restart the operation and try again.");
			return WhsWebAPIServiceHelper.SaveFactoryWithExceptionHandling(factory, (concurrencyException) => concurrencyErrorMessage);
		}

		static bool PackageNotOnLoad(ZGuid packagePK, BusinessObjectFactory factory)
		{
			var packageLoadPivots = factory.Load<WhsLoadPkgPackagePivot>(new ZQuery(WhsLoadPkgPackagePivotSchema.WLP_KP_Package, packagePK));
			return packageLoadPivots.Length == 0;
		}

		public string GeneratePickDockDoorAssignment(ZGuid packagePK, DockDoorAssignmentLinkType linkType, ZGuid parentJobPK, BusinessObjectFactory factory)
		{
			var errorMessage = string.Empty;
			var package = factory.Load<PkgPackage>(packagePK);
			if (package == null)
			{
				errorMessage = Res.GetString("f86f40e0-8d66-4536-be1e-ae0a64ada6d4", "Package could NOT be loaded using '{0}'.", packagePK);
			}
			else
			{
				var packagePick = GetPickFromPackage(package);
				if (packagePick != null)
				{
					var picksLinkedToPackagePick = GetLinkedPicks(factory, packagePK, linkType, parentJobPK);
					if (picksLinkedToPackagePick.Length > 1)
					{
						AddFetchHintsForGenerateDockDoorAssignment(picksLinkedToPackagePick, factory);
						var ddaPKs = picksLinkedToPackagePick.Select(p => p.WP_WDA_DockDoorAssignment);
						var ddaQuery = new ZQuery(WhsDockDoorAssignmentSchema.PK, ddaPKs);

						var allLinkedDDAs = factory.Load<WhsDockDoorAssignment>(ddaQuery);
						if (allLinkedDDAs.Any(d => d.WDA_WL_AssignedDockDoor != packagePick.DockDoorPK))
						{
							errorMessage = Res.GetString("937391bf-f066-4d9a-9646-0c2d9286b6a7", "Cannot add this package to this Job as it has a different Dock Door Location.");
						}
						else
						{
							var dockDoorAssignment =
								allLinkedDDAs.OrderByDescending(d => d.WDA_FirstPutawayToDockDoorUtc).ThenBy(d => d.WDA_SystemCreateTimeUtc).FirstOrDefault()
								?? CreateDockDoorAssignment(factory, packagePick.DockDoorPK);

							var ddasToDelete = allLinkedDDAs.Where(d => d.PK != dockDoorAssignment.PK);
							var picksToUpdate = picksLinkedToPackagePick.Where(p => p.WP_WDA_DockDoorAssignment.IsEmpty).ToList();
							var picksWithDockDoorAssignmentToUpdate = factory.Load<WhsPick>(new ZQuery(WhsPickSchema.WP_WDA_DockDoorAssignment, ddasToDelete.Select(f => f.PK)));
							picksToUpdate.AddRange(picksWithDockDoorAssignmentToUpdate);

							foreach (var pick in picksToUpdate)
							{
								pick.WP_WL_DockDoor = ZGuid.Empty;
								pick.WP_WDA_DockDoorAssignment = dockDoorAssignment.PK;
								UpdateBusinessObjectCriticalChangesVersionIDHelper<WhsPick>.UpdateBusinessObjectVersionAndSubscribeToFactory(factory, pick.PK);
							}

							foreach (var oldDockDoorAssignment in ddasToDelete)
							{
								oldDockDoorAssignment.Delete();
							}

							if (picksToUpdate.Count > 0)
							{
								UpdateBusinessObjectCriticalChangesVersionIDHelper<WhsDockDoorAssignment>.UpdateBusinessObjectVersionAndSubscribeToFactory(factory, dockDoorAssignment.PK);
							}
						}
					}
				}
				else
				{
					errorMessage = Res.GetString("8b7a96b0-1e76-4cdc-bd97-33101f656094", "Pick linked to Package from '{0}' could not be found.", packagePK);
				}
			}

			return errorMessage;
		}

		WhsPick GetPickFromPackage(PkgPackage package)
		{
			var order = package.PackageJob?.ParentJob as WhsOrder;
			return order?.Pick;
		}

		WhsPick[] GetLinkedPicks(BusinessObjectFactory factory, ZGuid packagePK, DockDoorAssignmentLinkType linkType, ZGuid parentJobPK)
		{
			var packageLSubQuery = parentJobPK.IsEmpty
				? GetLinkedPackagesDBSubQuery(packagePK, linkType)
				: GetLinkedPackagesLocalSubQuery(factory, linkType, parentJobPK);

			var packageLJobSubQuery = new ZDBOnlySubQuery(typeof(PkgPackageJob), PkgPackageJobSchema.KJ_ParentID);
			packageLJobSubQuery.AddToFilter(PkgPackageJobSchema.KJ_ParentTableCode, WhsDocketSchema.Constants.Prefix);
			packageLJobSubQuery.AddSubQuery(packageLSubQuery, JoinCondition.And);

			var docketLSubQuery = new ZDBOnlySubQuery(typeof(WhsOrder), WhsDocketSchema.WD_WP);
			docketLSubQuery.AddSubQuery(packageLJobSubQuery, JoinCondition.And);

			var pickQuery = new ZDBOnlyQuery(typeof(WhsPick));
			pickQuery.AddSubQuery(docketLSubQuery, JoinCondition.And);

			return factory.Load<WhsPick>(pickQuery);
		}

		static ZDBOnlySubQuery GetLinkedPackagesDBSubQuery(ZGuid packagePK, DockDoorAssignmentLinkType linkType)
		{
			ZDBOnlySubQuery linkedPackgesSubQuery;
			switch (linkType)
			{
				case DockDoorAssignmentLinkType.Trolley:
					var pkgSlotSubQuery = new ZDBOnlySubQuery(typeof(IWhsPickTrolleySlot), WhsPickTrolleySlotSchema.WTS_WTJ_TrolleyJob);
					pkgSlotSubQuery.AddToFilter(WhsPickTrolleySlotSchema.WTS_KP_Package, packagePK);

					linkedPackgesSubQuery = new ZDBOnlySubQuery(typeof(IWhsPickTrolleySlot), WhsPickTrolleySlotSchema.WTS_KP_Package);
					linkedPackgesSubQuery.AddSubQuery(WhsPickTrolleySlotSchema.WTS_WTJ_TrolleyJob, pkgSlotSubQuery, JoinCondition.And);
					break;

				case DockDoorAssignmentLinkType.PickByLabel:
					var pkgLabelSubQuery = new ZDBOnlySubQuery(typeof(IWhsPickByLabelLabel), WhsPickByLabelLabelSchema.WTL_WTK_PickByLabelJob);
					pkgLabelSubQuery.AddToFilter(WhsPickByLabelLabelSchema.WTL_KP_Package, packagePK);

					linkedPackgesSubQuery = new ZDBOnlySubQuery(typeof(IWhsPickByLabelLabel), WhsPickByLabelLabelSchema.WTL_KP_Package);
					linkedPackgesSubQuery.AddSubQuery(WhsPickByLabelLabelSchema.WTL_WTK_PickByLabelJob, pkgLabelSubQuery, JoinCondition.And);
					break;

				case DockDoorAssignmentLinkType.HandlingUnit:
					var huDivotSubQuery = new ZDBOnlySubQuery(typeof(PkgPackageHandlingUnitDivot), PkgPackageHandlingUnitDivotSchema.KPD_KP_HandlingUnit);
					huDivotSubQuery.AddToFilter(PkgPackageHandlingUnitDivotSchema.KPD_KP_Package, packagePK);

					linkedPackgesSubQuery = new ZDBOnlySubQuery(typeof(PkgPackageHandlingUnitDivot), PkgPackageHandlingUnitDivotSchema.KPD_KP_Package);
					linkedPackgesSubQuery.AddSubQuery(PkgPackageHandlingUnitDivotSchema.KPD_KP_HandlingUnit, huDivotSubQuery, JoinCondition.And);
					break;

				default:
					throw new InvalidOperationException($"Attempted to Generate a DockDoor Assignment with invalid link type '{linkType}'");
			}

			var packageLSubQuery = new ZDBOnlySubQuery(typeof(PkgPackage), PkgPackageSchema.KP_KJ_ParentPackageJob);
			packageLSubQuery.AddSubQuery(linkedPackgesSubQuery, JoinCondition.And);
			return packageLSubQuery;
		}

		ZDBOnlySubQuery GetLinkedPackagesLocalSubQuery(BusinessObjectFactory factory, DockDoorAssignmentLinkType linkType, ZGuid parentJobPK)
		{
			ZDBOnlySubQuery packageLSubQuery;
			if (linkType == DockDoorAssignmentLinkType.Trolley)
			{
				var slots = factory.Load<IWhsPickTrolleySlot>(new ZQuery(WhsPickTrolleySlotSchema.WTS_WTJ_TrolleyJob, parentJobPK));
				var packages = factory.Load<PkgPackage>(new ZQuery(PkgPackageSchema.PK, slots.Select(s => s.WTS_KP_Package)));
				packageLSubQuery = new ZDBOnlySubQuery(typeof(PkgPackageJob), PkgPackageJobSchema.PK);
				packageLSubQuery.AddToFilter(PkgPackageJobSchema.PK, packages.Select(p => p.KP_KJ_ParentPackageJob));
			}
			else if (linkType == DockDoorAssignmentLinkType.PickByLabel)
			{
				var labels = factory.Load<IWhsPickByLabelLabel>(new ZQuery(WhsPickByLabelLabelSchema.WTL_WTK_PickByLabelJob, parentJobPK));
				packageLSubQuery = new ZDBOnlySubQuery(typeof(PkgPackage), PkgPackageSchema.KP_KJ_ParentPackageJob);
				packageLSubQuery.AddToFilter(PkgPackageSchema.PK, labels.Select(l => l.WTL_KP_Package));
			}
			else if (linkType == DockDoorAssignmentLinkType.HandlingUnit)
			{
				var divots = factory.Load<PkgPackageHandlingUnitDivot>(new ZQuery(PkgPackageHandlingUnitDivotSchema.KPD_KP_HandlingUnit, parentJobPK));
				packageLSubQuery = new ZDBOnlySubQuery(typeof(PkgPackage), PkgPackageSchema.KP_KJ_ParentPackageJob);
				packageLSubQuery.AddToFilter(PkgPackageSchema.PK, divots.Select(l => l.KPD_KP_Package));
			}
			else
			{
				throw new InvalidOperationException($"Attempted to atomically Generate a DockDoor Assignment with invalid link type '{linkType}'");
			}

			return packageLSubQuery;
		}

		void AddFetchHintsForGenerateDockDoorAssignment(WhsPick[] picksLinkedToPackagePick, BusinessObjectFactory factory)
		{
			var orders = factory.Load<WhsOrder>(new ZQuery(WhsDocketSchema.WD_WP, picksLinkedToPackagePick.Select(p => p.PK)));
			foreach (var order in orders)
			{
				factory.AddFetchHint(ProcessTasksSchema.Instance, new ZQuery(ProcessTasksSchema.P9_ParentID, order.PK));
			}
		}

		WhsDockDoorAssignment CreateDockDoorAssignment(BusinessObjectFactory factory, ZGuid dockDoorLocationPK)
		{
			var assignment = factory.New<WhsDockDoorAssignment>();
			assignment.WDA_WL_AssignedDockDoor = dockDoorLocationPK;

			return assignment;
		}

		public string RemovePackageFromHandlingUnit(ZGuid packagePK)
		{
			var errorMessage = string.Empty;
			var factory = new BusinessObjectFactory();

			var package = factory.Load<PkgPackage>(packagePK);
			if (package == null)
			{
				errorMessage = Res.GetString("c000201e-90e7-4a8d-bd10-dd35291397a6", "Package could NOT be loaded using '{0}'.", packagePK);
			}
			else
			{
				var handlingUnitDivotQuery = new ZQuery(PkgPackageHandlingUnitDivotSchema.KPD_KP_Package, packagePK);
				handlingUnitDivotQuery.AddToFilter(PkgPackageHandlingUnitDivotSchema.KPD_UnpackedTime, null);
				var huDivot = factory.LoadTop1<PkgPackageHandlingUnitDivot>(handlingUnitDivotQuery);

				huDivot.KPD_UnpackedTime = ZDateTimeOffset.Now;
				huDivot.KPD_GS_NKUnpackedUser = GlbStaff.CurrentUser.GS_Code;

				package.KP_KP_TopHandlingUnitPackage = ZGuid.Empty;

				if (PackageNotOnLoad(packagePK, factory))
				{
					errorMessage = RemovePickDockDoorAssignment(packagePK, factory);
				}
				if (string.IsNullOrEmpty(errorMessage))
				{
					errorMessage = WhsWebAPIServiceHelper.SaveFactoryWithExceptionHandling(factory, (concurrencyException) => concurrencyException.Message);
				}
			}

			return errorMessage;
		}

		public string RemovePickDockDoorAssignment(ZGuid packagePK, BusinessObjectFactory factory)
		{
			var errorMessage = string.Empty;
			var package = factory.Load<PkgPackage>(packagePK);
			if (package == null)
			{
				errorMessage = Res.GetString("c000201e-90e7-4a8d-bd10-dd35291397a6", "Package could NOT be loaded using '{0}'.", packagePK);
			}
			else
			{
				var pkgPick = GetPickFromPackage(package);
				if (pkgPick != null && pkgPick.WP_WDA_DockDoorAssignment.IsValid)
				{
					var dockDoorAssignment = factory.Load<WhsDockDoorAssignment>(pkgPick.WP_WDA_DockDoorAssignment);
					if (dockDoorAssignment != null && dockDoorAssignment.WDA_FirstPutawayToDockDoorUtc.IsEmpty)
					{
						if (PickDoesNotHaveLinksToOtherPicks(pkgPick, packagePK))
						{
							pkgPick.WP_WL_DockDoor = dockDoorAssignment.WDA_WL_AssignedDockDoor;
							pkgPick.WP_WDA_DockDoorAssignment = ZGuid.Empty;
							UpdateBusinessObjectCriticalChangesVersionIDHelper<WhsPick>.UpdateBusinessObjectVersionAndSubscribeToFactory(factory, pkgPick.PK);

							var query = new ZQuery(WhsPickSchema.WP_WDA_DockDoorAssignment, dockDoorAssignment.PK);
							query.AddToFilter(WhsPickSchema.PK, SQLComparisonOperator.NotEqual, pkgPick.PK);
							if (!factory.Exists(typeof(WhsPick), query))
							{
								dockDoorAssignment.Delete();
							}
						}
					}
				}
			}

			return errorMessage;
		}

		bool PickDoesNotHaveLinksToOtherPicks(WhsPick packagePick, ZGuid packagePK)
		{
			var sql =
	@"
SELECT TOP 1 PkgPK
FROM
(
	SELECT
		WTS_KP_Package AS PkgPK
	FROM 
		dbo.WhsPickTrolleySlot
	
	UNION ALL
	
	SELECT
		KPD_KP_Package AS PkgPK
	FROM
		dbo.PkgPackageHandlingUnitDivot

	UNION ALL
	
	SELECT
		WTL_KP_Package AS PkgPK
	FROM
		dbo.WhsPickByLabelLabel
) AS PackageLink
JOIN
(
	SELECT KP_PK
	FROM
		dbo.WhsDocket
		JOIN dbo.PkgPackageJob ON KJ_ParentID = WD_PK
		JOIN dbo.PkgPackage ON KP_KJ_ParentPackageJob = KJ_PK
	WHERE 
		WD_WP = @PickPK
		AND KP_PK != @PackagePK
) PickLink ON PackageLink.PkgPK = PickLink.KP_PK
				";

			var otherLinkedPackages = new DynamicBusinessObjectCollection(packagePick.Factory);
			otherLinkedPackages.Load(sql, new ZSqlParameter[]
			{
				ZSqlParameter.New("@PickPK", packagePick.PK, WhsDocketSchema.WD_WP),
				ZSqlParameter.New("@PackagePK", packagePK, PkgPackageSchema.PK)
			});

			return otherLinkedPackages.Count == 0;
		}

		public string GetIsDockDoorOverrideAllowedForHandlingUnit(ZGuid huPackagePK)
			=> GetIsDockDoorOverrideAllowedForHandlingUnit(huPackagePK, null);

		public string GetIsDockDoorOverrideAllowedForHandlingUnit(ZGuid huPackagePK, BusinessObjectFactory inputFactory)
		{
			var factory = inputFactory ?? new BusinessObjectFactory();
			var picks = GetPicksFromHUPackage(huPackagePK, factory);
			var firstPick = picks.Length > 0 ? picks[0] : null;
			if (firstPick?.WP_WDA_DockDoorAssignment.IsValid ?? false)
			{
				picks = factory.Load<WhsPick>(new ZQuery(WhsPickSchema.WP_WDA_DockDoorAssignment, firstPick.WP_WDA_DockDoorAssignment));
			}

			return GetIsDockDoorOverrideAllowedForPicks(picks, factory);
		}

		static WhsPick[] GetPicksFromHUPackage(ZGuid huPackagePK, BusinessObjectFactory factory)
		{
			var huDivotSubQuery = new ZDBOnlySubQuery(typeof(PkgPackageHandlingUnitDivot), PkgPackageHandlingUnitDivotSchema.KPD_KP_Package);
			huDivotSubQuery.AddToFilter(PkgPackageHandlingUnitDivotSchema.KPD_KP_HandlingUnit, huPackagePK);

			var pickQuery = GetPickQuery(huDivotSubQuery);
			return factory.Load<WhsPick>(pickQuery);
		}

		public static ZDBOnlyQuery GetPickQuery(ZDBOnlySubQuery linkedPackgesSubQuery)
		{
			var packageLSubQuery = new ZDBOnlySubQuery(typeof(PkgPackage), PkgPackageSchema.KP_KJ_ParentPackageJob);
			packageLSubQuery.AddSubQuery(linkedPackgesSubQuery, JoinCondition.And);

			var packageLJobSubQuery = new ZDBOnlySubQuery(typeof(PkgPackageJob), PkgPackageJobSchema.KJ_ParentID);
			packageLJobSubQuery.AddToFilter(PkgPackageJobSchema.KJ_ParentTableCode, WhsDocketSchema.Constants.Prefix);
			packageLJobSubQuery.AddSubQuery(packageLSubQuery, JoinCondition.And);

			var docketLSubQuery = new ZDBOnlySubQuery(typeof(WhsOrder), WhsDocketSchema.WD_WP);
			docketLSubQuery.AddSubQuery(packageLJobSubQuery, JoinCondition.And);

			var pickQuery = new ZDBOnlyQuery(typeof(WhsPick));
			pickQuery.AddSubQuery(docketLSubQuery, JoinCondition.And);
			return pickQuery;
		}

		public string GetIsDockDoorOverrideAllowedForPicks(IEnumerable<IWhsPick> picks, BusinessObjectFactory inputFactory)
		{
			var result = string.Empty;
			var picksEvaluated = picks.ToArray();
			if (picksEvaluated.Length > 0)
			{
				var firstPick = picksEvaluated[0];
				var dockdoorAssignment = inputFactory.Load<WhsDockDoorAssignment>(firstPick.WP_WDA_DockDoorAssignment);

				if (dockdoorAssignment?.WDA_FirstPutawayToDockDoorUtc.IsValid ?? false)
				{
					result = CannotPutawayDockDoorError(dockdoorAssignment.AssignedDockDoor.WLV_LocationString_UserFriendly);
				}
				else if (HasInventoryPutawayToDockDoor(inputFactory, picksEvaluated))
				{
					var pick = inputFactory.Load<WhsPick>(firstPick.PK);
					result = CannotPutawayDockDoorError(pick.DockDoorLocation.WLV_LocationString_UserFriendly);
				}
				else
				{
					var disallowingClient = GetClientThatDisallowsOverrideFromConnectedOrderClients(inputFactory, picksEvaluated);
					if (!string.IsNullOrEmpty(disallowingClient))
					{
						result = Res.GetString("E526347F-C283-4FA3-8C8E-3BF1A15E4BEA", "Cannot override Dock Door Location as Client '{0}' does not allow overrides", disallowingClient);
					}
				}
			}

			return result;
		}

		static string CannotPutawayDockDoorError(ZString location)
			=> Res.GetString("53FCF585-CACC-49E4-86E1-2E9B681F4179", "Cannot override Dock Door Location as Stock is already putaway to '{0}'", location);

		bool HasInventoryPutawayToDockDoor(BusinessObjectFactory factory, IWhsPick[] picks)
		{
			var result = false;
			if (picks.FirstOrDefault(p => p.WP_WL_DockDoor.IsValid) == null)
			{
				var dockDoorAssignmentQuery = new ZQuery(WhsDockDoorAssignmentSchema.PK, picks.Select(p => p.WP_WDA_DockDoorAssignment));
				dockDoorAssignmentQuery.AddToFilter(WhsDockDoorAssignmentSchema.WDA_FirstPutawayToDockDoorUtc, SQLComparisonOperator.NotEqual, null);

				var putawaydockDoorAssignment = factory.LoadTop1<WhsDockDoorAssignment>(dockDoorAssignmentQuery);
				result = putawaydockDoorAssignment != null;
			}
			return result;
		}

		string GetClientThatDisallowsOverrideFromConnectedOrderClients(BusinessObjectFactory factory, IWhsPick[] picks)
		{
			var orderQuery = new ZDBOnlyQuery(typeof(WhsOrder));
			orderQuery.AddToFilter(WhsDocketSchema.WD_DocketType, DocketType.Codes.Order);

			var pickSubQuery = GetConnectedPicksQuery(picks);
			orderQuery.AddSubQuery(WhsDocketSchema.WD_WP, pickSubQuery, JoinCondition.And);
			var orders = factory.Load<WhsOrder>(orderQuery);

			var disallowingClientName = GetClientThatPreventsDDLOverride(picks[0].WP_WW_Whs, orders.Select(o => o.WD_OH_Client).Distinct().ToArray(), factory);
			return disallowingClientName;
		}

		static ZDBOnlySubQuery GetConnectedPicksQuery(IWhsPick[] picks)
		{
			var pickSubQuery = new ZDBOnlySubQuery(typeof(WhsPick), WhsPickSchema.PK);
			var firstPick = picks[0];
			if (firstPick.WP_WDA_DockDoorAssignment.IsValid)
			{
				pickSubQuery.AddToFilter(WhsPickSchema.WP_WDA_DockDoorAssignment, firstPick.WP_WDA_DockDoorAssignment);
			}
			else
			{
				pickSubQuery.AddToFilter(WhsPickSchema.PK, picks.Select(p => p.PK));
			}

			return pickSubQuery;
		}

		string GetClientThatPreventsDDLOverride(ZGuid whsPK, ZGuid[] clientPKs, BusinessObjectFactory factory)
		{
			var query = new ZQuery(WhsClientPickPackParamsByWhsSchema.WPP_WW_Warehouse, whsPK);
			query.AddToFilter(WhsClientPickPackParamsByWhsSchema.WPP_WSH_SalesChannel, null);
			var clientPickPackParams = factory.Load<WhsClientPickPackParamsByWhs>(query);

			var paramsByClient = clientPickPackParams.ToDictionary(c => c.WPP_OH_Client);
			foreach (var clientPK in clientPKs)
			{
				if (!paramsByClient.TryGetValue(clientPK, out var packParamsByWhs) || !packParamsByWhs.WPP_AllowPickDockDoorLocationOverride)
				{
					return factory.Load<OrgHeader>(clientPK).OH_FullNameTruncated;
				}
			}

			return string.Empty;
		}

		#region OverrideDockDoorLocationsOfPicks

		// Tested in usage of WhsPackingConsolidationServiceTest.cs, Whs.Web.PutawayPackagesInDockDoorLocationTest.cs Whs.Web.PutawayStockInDockDoorOrPackingStationTest.cs
		public void OverrideDockDoorLocationsOfPicks(IEnumerable<IWhsPick> picks, ZGuid dockDoorPK, BusinessObjectFactory inputFactory)
		{
			var pickPKs = picks.Select(p => p.PK).ToArray();
			AddFetchHintsForDockDoorOverride(pickPKs, inputFactory);
			foreach (var pickGroup in picks.GroupBy(p => p.WP_WDA_DockDoorAssignment))
			{
				if (pickGroup.Key.IsValid)
				{
					pickGroup.First().DockDoorPK = dockDoorPK;
				}
				else
				{
					pickGroup.ForEach(p => p.WP_WL_DockDoor = dockDoorPK);
				}
			}

			var pickByLabelJobs = GetPickByLabelJobs(pickPKs, inputFactory);
			foreach (var pickByLabelJob in pickByLabelJobs)
			{
				pickByLabelJob.WTK_WL_DockDoor = dockDoorPK;
			}
		}

		void AddFetchHintsForDockDoorOverride(ZGuid[] pickPKs, BusinessObjectFactory inputFactory)
		{
			foreach (var pickPK in pickPKs)
			{
				var pickableDocketQuery = new ZQuery();
				pickableDocketQuery.AddToFilter(WhsDocketSchema.WD_DocketType, new[] { DocketType.Codes.Order, DocketType.Codes.WorkOrder, DocketType.Codes.DynamicWorkOrder });
				pickableDocketQuery.AddToFilter(WhsDocketSchema.WD_WP, pickPK);
				inputFactory.AddFetchHint(WhsDocketSchema.Instance, pickableDocketQuery);

				var transferQuery = new ZQuery();
				transferQuery.AddToFilter(WhsDocketSchema.WD_WP_ParentPickForTransfer, pickPK);
				transferQuery.AddToFilter(WhsDocketSchema.WD_DocketType, DocketType.Codes.Transfer);
				inputFactory.AddFetchHint(WhsDocketSchema.Instance, transferQuery);
			}

			var orders = inputFactory.Load<WhsDocket>(new ZQuery(WhsDocketSchema.WD_WP, pickPKs));
			var orderLines = inputFactory.Load<WhsDocketLine>(new ZQuery(WhsDocketLineSchema.WE_WD, orders.Select(o => o.PK)));
			foreach (var line in orderLines)
			{
				inputFactory.AddFetchHint(WhsPickLineSchema.WZ_WE_TransactionLine, line.PK);
			}
		}

		IWhsPickByLabelJob[] GetPickByLabelJobs(ZGuid[] pickPKs, BusinessObjectFactory inputFactory)
		{
			var orderSubQuery = new ZDBOnlySubQuery(typeof(WhsDocket), WhsDocketSchema.PK);
			orderSubQuery.AddToFilter(WhsDocketSchema.WD_WP, pickPKs);

			var packageJobSubQuery = new ZDBOnlySubQuery(typeof(PkgPackageJob), PkgPackageJobSchema.PK);
			packageJobSubQuery.AddSubQuery(PkgPackageJobSchema.KJ_ParentID, orderSubQuery, JoinCondition.And);

			var packageSubQuery = new ZDBOnlySubQuery(typeof(PkgPackage), PkgPackageSchema.PK);
			packageSubQuery.AddSubQuery(PkgPackageSchema.KP_KJ_ParentPackageJob, packageJobSubQuery, JoinCondition.And);

			var pickByLabelLabelSubQuery = new ZDBOnlySubQuery(typeof(IWhsPickByLabelLabel), WhsPickByLabelLabelSchema.WTL_WTK_PickByLabelJob);
			pickByLabelLabelSubQuery.AddSubQuery(WhsPickByLabelLabelSchema.WTL_KP_Package, packageSubQuery, JoinCondition.And);

			var query = new ZDBOnlyQuery(typeof(IWhsPickByLabelJob));
			query.AddSubQuery(pickByLabelLabelSubQuery, JoinCondition.And);

			return inputFactory.Load<IWhsPickByLabelJob>(query);
		}

		#endregion

		#region TryCreateAndPutawayDockDoorAssignmentForPicks

		public string TryCreateAndPutawayDockDoorAssignmentForPicks(IEnumerable<IWhsPick> picks, BusinessObjectFactory factory)
		{
			var errorMessage = string.Empty;
			var firstPick = picks.FirstOrDefault() as WhsPick;
			if (firstPick != null)
			{
				if (!firstPick.IsFinalisedOrCancelled)
				{
					var dockdoorAssignment = firstPick.DockDoorAssignment;
					if (dockdoorAssignment == null)
					{
						dockdoorAssignment = CreateDockDoorAssignment(factory, firstPick.WP_WL_DockDoor);
						picks.ForEach(pick =>
						{
							pick.WP_WL_DockDoor = ZGuid.Empty;
							pick.WP_WDA_DockDoorAssignment = dockdoorAssignment.PK;
							UpdateBusinessObjectCriticalChangesVersionIDHelper<WhsPick>.UpdateBusinessObjectVersionAndSubscribeToFactory(factory, pick.PK);
						});
					}

					if (!dockdoorAssignment.WDA_FirstPutawayToDockDoorUtc.IsValid)
					{
						dockdoorAssignment.WDA_FirstPutawayToDockDoorUtc = ZDateTime.UtcNow;
						UpdateBusinessObjectCriticalChangesVersionIDHelper<WhsDockDoorAssignment>.UpdateBusinessObjectVersionAndSubscribeToFactory(factory, dockdoorAssignment.PK);
					}
				}
				else
				{
					errorMessage = Res.GetString("4BBE2D76-ED88-41FC-87EF-3D5EBD24D1DC", "Cannot complete dock door putaway for a finalized, canceled or already putaway Pick.");
				}
			}
			else
			{
				errorMessage = Res.GetString("D139D4ED-DAC6-45BD-8BFE-BF6E4B71A881", "No valid Pick to putaway to dock door found.");
			}

			return errorMessage;
		}

		#endregion
	}
}
