using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.TrolleyPicking;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	class PickJobWrapperForTrolleyJob : PickJobWrapper<WhsPickTrolleyJob>
	{
		public PickJobWrapperForTrolleyJob(BusinessObjectFactory factory, Guid jobPk, WhsPickTrolleyJob trolleyJob, WhsPick[] picks)
			: base(factory, jobPk, trolleyJob)
		{
			PackagesLazy = new Lazy<PkgPackage[]>(() => GetPackages());
		}

		public PickJobWrapperForTrolleyJob(BusinessObjectFactory factory, Guid jobPk, WhsPickTrolleyJob trolleyJob)
			: base(factory, jobPk, trolleyJob)
		{
			PackagesLazy = new Lazy<PkgPackage[]>(() => GetPackages());
		}

		Lazy<PkgPackage[]> PackagesLazy { get; }
		PkgPackage[] Packages => PackagesLazy.Value;

		PkgPackage[] GetPackages() => PutawayStockInDockDoorOrPackingStationHelper.LoadPackagesForTrolleyJob(Factory, PickJobPk);

		protected override bool IsClosingPackagesWhenPutToDockDoorCore => IsTrolleyCartonPicking();

		protected override bool IsPackingStationSupportedForPickJobType() => !IsTrolleyCartonPicking()
			|| (GetAssignedLocationForJob()?.LocationClass.Equals(LocationClasses.Codes.PST) ?? false);

		public override PutawayTransferLinesAndPackagesToClose GetTransferLinesToPutawayAndPackagesToClose(Guid jobPk, PickJobType pickJobType, WhsLocation location, ZString rfUser, WebServiceResponse response)
		{
			var transferLines = new List<WhsTransferLine>();
			var packagesToClose = new List<PkgPackage>();
			if (Packages != null && Packages.Length > 0)
			{
				var (picks, trolleys) = GetAllPicksAndTrolleysRelatedToTrolley(PickJob);
				Factory.AddFetchHint(WhsDocketSchema.Instance, new ZQuery(WhsDocketSchema.WD_WP, picks.Select(p => p.PK)));
				if (PickJob.PickingType != TrolleyPickingType.Tote || CheckAndSetPackingStationOnPicks(picks, location, response, trolleys))
				{
					var kitPickLinesInPackages = new List<WhsPickLine>();
					var (pickLineLookup, packageDivotLookup) = PutawayStockInDockDoorOrPackingStationHelper.GetPickLineAndPackageDivotLookups(Factory, Packages, PickJobType.TrolleyJob);
					WhsPickByBOMHelper.AddFetchHintsForIsPickByBOMKitPickLine(Factory, pickLineLookup.Values);

					foreach (var package in Packages)
					{
						var packagePickLinePKs = packageDivotLookup.TryGetValue(package.PK, out var divots) ? divots.Select(d => d.KI_ParentID) : Enumerable.Empty<ZGuid>();
						var packagePickLines = packagePickLinePKs.Select(pk => pickLineLookup[pk]);
						kitPickLinesInPackages.AddRange(packagePickLines.Where(pl => pl.IsPickByBOMKitPickLine()));
						var transferLinesToPutaway = PutawayStockInDockDoorOrPackingStationHelper.GetInTransitTransferLinesFromPickLines(Factory, packagePickLines, rfUser).ToArray();

						if (transferLinesToPutaway.Length > 0)
						{
							transferLines.AddRange(transferLinesToPutaway);
							if (!package.GetIsTote())
							{
								packagesToClose.Add(package);
							}
						}
					}

					if (kitPickLinesInPackages.Count > 0)
					{
						var componentPickLines = PutawayStockInDockDoorOrPackingStationHelper.LoadComponentPickLinesFromKitPickLines(Factory, kitPickLinesInPackages.Select(pl => pl.PK).ToArray(), isUnPickedComponentOnly: false).ComponentPickLines;
						if (componentPickLines.Length > 0)
						{
							var componentTransferLinesToPutaway = PutawayStockInDockDoorOrPackingStationHelper.GetInTransitTransferLinesFromPickLines(Factory, componentPickLines, rfUser).ToArray();
							transferLines.AddRange(componentTransferLinesToPutaway);
						}
					}
				}
			}

			return new PutawayTransferLinesAndPackagesToClose(transferLines.ToArray(), packagesToClose.ToArray());
		}

		bool IsTrolleyCartonPicking()
		{
			var referencePackage = Packages?.FirstOrDefault();
			return referencePackage != null && !referencePackage.GetIsTote();
		}

		(IEnumerable<WhsPick>, IEnumerable<WhsPickTrolleyJob>) GetAllPicksAndTrolleysRelatedToTrolley(WhsPickTrolleyJob trolley)
		{
			var picks = new List<WhsPick>();
			var allTrolleys = new List<WhsPickTrolleyJob>() { trolley };
			IReadOnlyCollection<WhsPickTrolleyJob> searchTrolleys = allTrolleys;
			var searchTimes = 0;
			while (searchTrolleys.Count > 0)
			{
				var currentPicks = GetPicksByTrolleyJob(searchTrolleys, picks);
				picks.AddRange(currentPicks);
				searchTrolleys = GetTrolleyJobsByPick(currentPicks, allTrolleys.Select(t => t.PK));
				allTrolleys.AddRange(searchTrolleys);
				if (++searchTimes == 10)
				{
					ErrorReporter.ReportOnce("The depth of search all Picks related to the Trolley is more than 10.");
				}
			}

			return (picks, allTrolleys);
		}

		WhsPick[] GetPicksByTrolleyJob(IEnumerable<WhsPickTrolleyJob> trolleys, IEnumerable<WhsPick> excludePicks)
		{
			var slotSubQuery = new ZDBOnlySubQuery(typeof(WhsPickTrolleySlot), WhsPickTrolleySlotSchema.WTS_KP_Package);
			slotSubQuery.AddToFilter(WhsPickTrolleySlotSchema.WTS_WTJ_TrolleyJob, trolleys.Select(t => t.PK));

			var packageSubQuery = new ZDBOnlySubQuery(typeof(PkgPackage), PkgPackageSchema.KP_KJ_ParentPackageJob);
			packageSubQuery.AddSubQuery(slotSubQuery, JoinCondition.And);

			var packageJobSubQuery = new ZDBOnlySubQuery(typeof(PkgPackageJob), PkgPackageJobSchema.KJ_ParentID);
			packageJobSubQuery.AddToFilter(PkgPackageJobSchema.KJ_ParentTableCode, WhsDocketSchema.Constants.Prefix);
			packageJobSubQuery.AddSubQuery(packageSubQuery, JoinCondition.And);

			var docketSubQuery = new ZDBOnlySubQuery(typeof(WhsOrder), WhsDocketSchema.WD_WP);
			docketSubQuery.AddSubQuery(packageJobSubQuery, JoinCondition.And);

			var query = new ZDBOnlyQuery(typeof(WhsPick));
			query.AddToFilter(WhsPickSchema.PK, SQLComparisonOperator.NotEqual, excludePicks.Select(x => x.PK));
			query.AddSubQuery(docketSubQuery, JoinCondition.And);

			return Factory.Load<WhsPick>(query);
		}

		protected override WhsPick[] GetPicksForJob() => GetPicksByTrolleyJob(new[] { PickJob }, Enumerable.Empty<WhsPick>());

		protected override bool AnyPickOnToteTrolleyJob(WhsPick[] picks) => !IsTrolleyCartonPicking();
	}
}
