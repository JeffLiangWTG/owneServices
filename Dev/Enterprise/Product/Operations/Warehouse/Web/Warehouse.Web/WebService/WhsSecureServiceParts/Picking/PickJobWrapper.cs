using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.TrolleyPicking;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	// These classes represent the base Wrappers for the various Pick job Types, and allow caching of properties
	// And access to common logic these jobs will share
	abstract class PickJobWrapper
	{
		protected PickJobWrapper(BusinessObjectFactory factory, Guid jobPk)
		{
			Factory = Argument.NotNull(factory, nameof(factory));
			PickJobPk = Argument.NotNull(jobPk, nameof(jobPk));
		}
		protected BusinessObjectFactory Factory { get; }
		public Guid PickJobPk { get; }
		public bool IsPickJobLoaded { get; protected set; }
		public bool IsClosingPackagesWhenPutToDockDoor => IsClosingPackagesWhenPutToDockDoorCore;
		protected virtual bool IsClosingPackagesWhenPutToDockDoorCore => false;

		public bool CheckIfPackingStationIsAllowed() => isPackingStationAllowed ?? (isPackingStationAllowed = CheckIfPackingStationIsAllowedCore()).Value;
		bool? isPackingStationAllowed;
		protected abstract bool CheckIfPackingStationIsAllowedCore();

		public WhsLocationInfo GetAssignedLocationForJob() => GetAssignedLocationForJobCore();
		protected abstract WhsLocationInfo GetAssignedLocationForJobCore();
		public abstract PutawayTransferLinesAndPackagesToClose GetTransferLinesToPutawayAndPackagesToClose(Guid jobPk, PickJobType pickJobType, WhsLocation location, ZString rfUser, WebServiceResponse response);

		public WhsLocationInfo GetCurrentDockDoorLocationForJob() => GetCurrentDockDoorLocationForJobCore();
		protected abstract WhsLocationInfo GetCurrentDockDoorLocationForJobCore();

		public string GetAllowPickDockDoorLocationOverride() => GetAllowPickDockDoorLocationOverrideCore();
		protected abstract string GetAllowPickDockDoorLocationOverrideCore();

		public void OverrideDockDoorLocation(ZGuid locationPK) => OverrideDockDoorLocationCore(locationPK);
		protected abstract void OverrideDockDoorLocationCore(ZGuid locationPK);

		public string SetDockDoorAssignmentPutawayTime() => SetDockDoorAssignmentPutawayTimeCore();
		protected abstract string SetDockDoorAssignmentPutawayTimeCore();
	}

	abstract class PickJobWrapper<TPickJob> : PickJobWrapper where TPickJob : BusinessObject
	{
		protected PickJobWrapper(BusinessObjectFactory factory, Guid pickJobPK, TPickJob pickJob)
			: base(factory, pickJobPK)
		{
			PickJob = pickJob;
			IsPickJobLoaded = PickJob != null;
		}

		public TPickJob PickJob { get; }

		static Lazy<IWhsPickDockDoorAssignmentService> DockDoorAssignmentService { get; } = new Lazy<IWhsPickDockDoorAssignmentService>(ObjectFactory.Get<IWhsPickDockDoorAssignmentService>);

		protected bool CheckAndSetPackingStationOnPicks(IEnumerable<WhsPick> picks, WhsLocation location, WebServiceResponse response, IEnumerable<WhsPickTrolleyJob> trolleys = null)
		{
			if (location.IsPackingStationLocation)
			{
				foreach (var pick in picks)
				{
					if (pick.WP_WL_PackingStation.IsValid && pick.WP_WL_PackingStation != location.PK)
					{
						var existingPackingStation = Factory.Load<WhsLocation>(pick.WP_WL_PackingStation);
						response.LogBusinessValidationError(Res.GetString("5aa9bd3b-247f-4088-827d-580dfd4ed864", "All picked inventory must go to Packing Station {0}.", existingPackingStation?.WLV_LocationString));
						return false;
					}
					else
					{
						pick.WP_WL_PackingStation = location.PK;
					}
				}
				if (trolleys != null)
				{
					trolleys.ForEach(t => UpdateBusinessObjectCriticalChangesVersionIDHelper<WhsPickTrolleyJob>.UpdateBusinessObjectVersionAndSubscribeToFactory(Factory, t.PK));
				}
			}
			return true;
		}

		protected override WhsLocationInfo GetAssignedLocationForJobCore() => GetAssignedLocationForJobFromPicks(GetPicksForJob());

		WhsLocationInfo GetAssignedLocationForJobFromPicks(WhsPick[] picks)
		{
			WhsLocationInfo assignedLocation = null;

			var pickWithInventoryInPackingStation = picks.FirstOrDefault(p => p.WP_WL_PackingStation.IsValid);
			if (pickWithInventoryInPackingStation != null)
			{
				var packingStation = pickWithInventoryInPackingStation.PackingStationLocation;
				assignedLocation = new WhsLocationInfo(packingStation.PK.ToGuid(), packingStation.WLV_LocationString, packingStation.WLV_LocationString_UserFriendly, packingStation.WLV_LocationClass);
			}
			else if (picks.Length > 0)
			{
				if (HasInventoryPutawayToDockDoor(picks))
				{
					var dockdoorLocation = picks[0].DockDoorLocation;
					assignedLocation = new WhsLocationInfo(dockdoorLocation.PK.ToGuid(), dockdoorLocation.WLV_LocationString, dockdoorLocation.WLV_LocationString_UserFriendly, dockdoorLocation.WLV_LocationClass);
				}
				else if (WhsPickJobInfo.WarehouseHasPackingStationLocation(Factory, picks[0].WP_WW_Whs.ToGuid())
					&& AnyPickOnToteTrolleyJob(picks))
				{
					assignedLocation = new WhsLocationInfo { LocationClass = LocationClasses.Codes.PST };
				}
			}

			return assignedLocation;
		}

		protected override WhsLocationInfo GetCurrentDockDoorLocationForJobCore()
		{
			var jobDockDoorLocation = GetPicksForJob()[0].DockDoorLocation;
			return new WhsLocationInfo(jobDockDoorLocation.PK.ToGuid(), jobDockDoorLocation.WLV_LocationString, jobDockDoorLocation.WLV_LocationString_UserFriendly, jobDockDoorLocation.WLV_LocationClass);
		}

		protected virtual bool AnyPickOnToteTrolleyJob(WhsPick[] picks)
			=> GetTrolleyJobsByPick(picks, Enumerable.Empty<ZGuid>()).Any(trolleyJob => trolleyJob.PickingType.Equals(TrolleyPickingType.Tote));

		protected WhsPickTrolleyJob[] GetTrolleyJobsByPick(IEnumerable<WhsPick> picks, IEnumerable<ZGuid> excludeTrolleyPKs)
		{
			var docketSubQuery = new ZDBOnlySubQuery(typeof(WhsOrder), PkgPackageJobSchema.KJ_ParentID);
			docketSubQuery.AddToFilter(WhsDocketSchema.WD_WP, picks.Select(x => x.PK));

			var packageJobSubQuery = new ZDBOnlySubQuery(typeof(PkgPackageJob), PkgPackageSchema.KP_KJ_ParentPackageJob);
			packageJobSubQuery.AddSubQuery(docketSubQuery, JoinCondition.And);

			var packageSubQuery = new ZDBOnlySubQuery(typeof(PkgPackage), WhsPickTrolleySlotSchema.WTS_KP_Package);
			packageSubQuery.AddSubQuery(packageJobSubQuery, JoinCondition.And);

			var slotSubQuery = new ZDBOnlySubQuery(typeof(WhsPickTrolleySlot), WhsPickTrolleySlotSchema.WTS_WTJ_TrolleyJob);
			slotSubQuery.AddSubQuery(packageSubQuery, JoinCondition.And);

			var query = new ZDBOnlyQuery(typeof(WhsPickTrolleyJob));
			query.AddToFilter(WhsPickTrolleyJobSchema.PK, SQLComparisonOperator.NotEqual, excludeTrolleyPKs);
			query.AddSubQuery(slotSubQuery, JoinCondition.And);

			return Factory.Load<WhsPickTrolleyJob>(query);
		}

		bool HasInventoryPutawayToDockDoor(WhsPick[] picks)
		{
			var sql = @"
	SELECT TOP 1 NULL AS RESULT
	FROM
		dbo.WhsDocketLine TransferLine
		JOIN dbo.WhsDocket WhsTransfer ON WE_WD = WD_PK
	WHERE
		WhsTransfer.WD_WP_ParentPickForTransfer IN (SELECT VALUE FROM @PickPKs)
		AND TransferLine.WE_PutawayTime IS NOT NULL
		AND TransferLine.WE_WL = @DockDoorPK
";

			var hasInventoryPutawayToDDL = new DynamicBusinessObjectCollection(Factory);
			var sqlParams = new ZSqlParameterCollection
				{
					ZSqlParameter.New("@PickPKs", picks.Select(p => p.PK).ToArray(), WhsDocketSchema.WD_WP_ParentPickForTransfer, isTableValued: true),
					ZSqlParameter.New("@DockDoorPK", picks[0].DockDoorPK, WhsDocketLineSchema.WE_WL)
				};
			hasInventoryPutawayToDDL.Load(sql, sqlParams);

			return hasInventoryPutawayToDDL.Count > 0;
		}

		public bool CheckIfConsolidationLocationIsAllowedForJob()
			=> CheckIfConsolidationLocationIsAllowedForJobCore(GetPicksForJob());

		protected bool CheckIfConsolidationLocationIsAllowedForJobCore(WhsPick[] picks)
			=> IsConsolidationLocationAllowedForPick(picks) && GetAssignedLocationForJob() is null;

		protected virtual bool IsConsolidationLocationAllowedForPick(WhsPick[] picks) => PicksHaveNoLooseInventory(picks);

		bool PicksHaveNoLooseInventory(WhsPick[] picks)
		{
			var picksHaveNoLooseInventory = false;
			var picksWithLooseInventoryResults = PutawayStockInDockDoorOrPackingStationHelper.BuildAndRunQueryForPicksWithLooseInventory(Factory, picks.Select(p => p.PK).ToArray(), topOneOnly: false);
			var picksWithLooseInventoryLookup = picksWithLooseInventoryResults.ToHashSet();
			foreach (var pick in picks)
			{
				if (!picksWithLooseInventoryLookup.Contains(pick.PK) && pick.Orders.Cast<WhsDocket>().Any(o => o.WD_UseDirectedPackingConsolidation))
				{
					picksHaveNoLooseInventory = true;
					break;
				}
			}

			return picksHaveNoLooseInventory;
		}

		protected sealed override bool CheckIfPackingStationIsAllowedCore()
		{
			var result = false;

			var referencePick = GetPicksForJob().FirstOrDefault();
			if (referencePick != null)
			{
				result = referencePick.WP_WL_PackingStation.IsValid
					|| WhsPickJobInfo.WarehouseHasPackingStationLocation(Factory, referencePick.WP_WW_Whs);
			}

			return result && IsPackingStationSupportedForPickJobType();
		}
		protected abstract bool IsPackingStationSupportedForPickJobType();

		protected abstract WhsPick[] GetPicksForJob();

		#region GetAllowPickDockDoorLocationOverride

		// Tested in consumers
		protected override string GetAllowPickDockDoorLocationOverrideCore()
			=> DockDoorAssignmentService.Value.GetIsDockDoorOverrideAllowedForPicks(GetPicksForJob(), Factory);

		#endregion

		#region OverrideDockDoorLocationCore

		protected override void OverrideDockDoorLocationCore(ZGuid locationPK)
		{
			var picks = GetPicksForJob();
			DockDoorAssignmentService.Value.OverrideDockDoorLocationsOfPicks(picks, locationPK, Factory);
		}

		#endregion

		#region SetDockDoorAssignmentPutawayTimeCore

		protected override string SetDockDoorAssignmentPutawayTimeCore()
		{
			var picks = GetPicksForJob();
			return DockDoorAssignmentService.Value.TryCreateAndPutawayDockDoorAssignmentForPicks(picks, Factory);
		}

		#endregion
	}
}
