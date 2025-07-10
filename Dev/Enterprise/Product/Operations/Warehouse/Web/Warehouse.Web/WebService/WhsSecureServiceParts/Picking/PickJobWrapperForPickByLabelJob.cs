using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.PickByLabel;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	class PickJobWrapperForPickByLabelJob : PickJobWrapper<WhsPickByLabelJob>
	{
		public PickJobWrapperForPickByLabelJob(BusinessObjectFactory factory, Guid jobPk, WhsPickByLabelJob pickByLabelJob, WhsPick[] picks)
			: base(factory, jobPk, pickByLabelJob)
		{
			PickLines = new Lazy<PickByLabelPickLines>(() => GetPickByLabelPickLines(PickJobPk));
			Picks = new Lazy<WhsPick[]>(() => picks);
		}

		public PickJobWrapperForPickByLabelJob(BusinessObjectFactory factory, Guid jobPk, WhsPickByLabelJob pickByLabelJob)
			: base(factory, jobPk, pickByLabelJob)
		{
			PickLines = new Lazy<PickByLabelPickLines>(() => GetPickByLabelPickLines(PickJobPk));
			Picks = new Lazy<WhsPick[]>(() => GetPicks(() => GetPicksFromPickLines(PickLines.Value.AllPickLines)));
		}

		Lazy<WhsPick[]> Picks { get; }
		Lazy<PickByLabelPickLines> PickLines { get; }

		WhsPick[] GetPicks(Func<WhsPick[]> getPicks)
		{
			var picks = getPicks();
			Factory.AddFetchHint(WhsDocketSchema.Instance, new ZQuery(WhsDocketSchema.WD_WP, picks.Select(p => p.PK)));
			return picks;
		}

		protected override bool IsPackingStationSupportedForPickJobType() => true;

		public override PutawayTransferLinesAndPackagesToClose GetTransferLinesToPutawayAndPackagesToClose(Guid jobPk, PickJobType pickJobType, WhsLocation location, ZString rfUser, WebServiceResponse response)
		{
			var transferLines = Array.Empty<WhsTransferLine>();
			WhsPickByLabelHelper.PutawayPickedLabelsAndSplitJobIfNecessary(PickJob);
			PickJob.RunPreSaveValidation();
			if (PickJob.HasErrors)
			{
				response.LogBusinessValidationError(PickJob.NotificationsIncludingChildren.ToMessageListString());
			}
			else
			{
				if (CheckAndSetPackingStationOnPicks(Picks.Value, location, response))
				{
					transferLines = PutawayStockInDockDoorOrPackingStationHelper.GetInTransitTransferLinesFromPickLines(Factory, PickLines.Value.PickedPickLines, rfUser).ToArray();
					SiblingPickByLabelJobs.Union(new[] { PickJob }).ForEach(job => job.WTK_WL_PutawayLocation = location.PK);
				}
			}

			return new PutawayTransferLinesAndPackagesToClose(transferLines, Array.Empty<PkgPackage>());
		}

		PickByLabelPickLines GetPickByLabelPickLines(Guid jobPK)
		{
			var packages = PutawayStockInDockDoorOrPackingStationHelper.LoadPackagesForPickByLabelJob(Factory, jobPK);
			var pickedPickLines = new List<WhsPickLine>();
			var unpickedPickLines = new List<WhsPickLine>();
			var (pickLineLookup, packageDivotLookup) = PutawayStockInDockDoorOrPackingStationHelper.GetPickLineAndPackageDivotLookups(Factory, packages, PickJobType.PickByLabelJob);
			foreach (var package in packages)
			{
				var packagePickLinePKs = packageDivotLookup.TryGetValue(package.PK, out var divots) ? divots.Select(d => d.KI_ParentID) : Enumerable.Empty<ZGuid>();
				var pickLinesOnPackage = packagePickLinePKs.Select(pk => pickLineLookup[pk]).ToArray();
				if (pickLinesOnPackage.All(pl => pl.IsPickedFromPutawayLocation))
				{
					pickedPickLines.AddRange(pickLinesOnPackage);
				}
				else
				{
					unpickedPickLines.AddRange(pickLinesOnPackage);
				}
			}

			return new PickByLabelPickLines(pickedPickLines.ToArray(), unpickedPickLines.ToArray());
		}

		WhsPick[] GetPicksFromPickLines(IEnumerable<WhsPickLine> pickLines)
		{
			var orders = GetOrdersAndAddFetchHintForProcessTasks(pickLines);
			var pickQuery = new ZDBOnlyQuery(typeof(WhsPick));
			pickQuery.AddToFilter(new ZQuery(WhsPickSchema.PK, orders.Select(o => o.WD_WP)));
			return Factory.Load<WhsPick>(pickQuery);
		}

		WhsOrder[] GetOrdersAndAddFetchHintForProcessTasks(IEnumerable<WhsPickLine> pickLines)
		{
			var docketLineSubQuery = new ZDBOnlySubQuery(typeof(WhsDocketLine), WhsDocketLineSchema.WE_WD);
			docketLineSubQuery.AddToFilter(WhsDocketLineSchema.PK, pickLines.Select(pl => pl.WZ_WE_TransactionLine));
			var orderQuery = new ZDBOnlyQuery(typeof(WhsOrder));
			orderQuery.AddSubQuery(docketLineSubQuery, JoinCondition.And);
			var orders = Factory.Load<WhsOrder>(orderQuery);
			Factory.AddFetchHint(ProcessTasksSchema.Instance, new ZQuery(ProcessTasksSchema.P9_ParentID, orders.Select(o => o.PK)));
			return orders;
		}

		protected override WhsLocationInfo GetAssignedLocationForJobCore()
		{
			var assignedLocationInfo = base.GetAssignedLocationForJobCore();
			if (assignedLocationInfo == null)
			{
				if (PickJob.WTK_WL_PutawayLocation.IsValid)
				{
					assignedLocationInfo = GetAssignedLocationInfo(PickJob.WTK_WL_PutawayLocation);
				}
				else if (SiblingPickByLabelJobs.Length > 0)
				{
					var siblingJobWithPutawayLocation = SiblingPickByLabelJobs.FirstOrDefault(job => job.WTK_WL_PutawayLocation.IsValid);
					if (siblingJobWithPutawayLocation != null)
					{
						assignedLocationInfo = GetAssignedLocationInfo(siblingJobWithPutawayLocation.WTK_WL_PutawayLocation);
					}
				}
			}

			return assignedLocationInfo;

			WhsLocationInfo GetAssignedLocationInfo(ZGuid putawayLocationPK)
			{
				var assignedLocation = Factory.Load<WhsLocation>(putawayLocationPK);
				return new WhsLocationInfo(assignedLocation.PK.ToGuid(), assignedLocation.WLV_LocationString, assignedLocation.WLV_LocationString_UserFriendly, assignedLocation.WLV_LocationClass);
			}
		}

		protected override WhsPick[] GetPicksForJob() => Picks.Value;

		WhsPickByLabelJob[] SiblingPickByLabelJobs => siblingPickByLabelJobs ?? (siblingPickByLabelJobs = GetSiblingPickByLabelJobs());
		WhsPickByLabelJob[] siblingPickByLabelJobs;

		WhsPickByLabelJob[] GetSiblingPickByLabelJobs()
		{
			var siblingPickByLabelJobs = new HashSet<WhsPickByLabelJob>();
			var searchTimes = 0;
			var searchPickByLabelJobPKs = new[] { PickJob.PK };
			var searchPackageJobPKs = GetPackageJobPKs(searchPickByLabelJobPKs);
			while (searchPackageJobPKs.Length > 0)
			{
				var pickByLabelJobs = GetPickByLabelJobs(searchPackageJobPKs);
				searchPickByLabelJobPKs = AddPickByLabelJobToSiblingPackageJobsAndReturnPickByLabelJobsToSearch(pickByLabelJobs);
				searchPackageJobPKs = searchPickByLabelJobPKs.Length > 0
					? GetPackageJobPKs(searchPickByLabelJobPKs)
					: Array.Empty<ZGuid>();
				if (++searchTimes == 10)
				{
					ErrorReporter.ReportOnce("The depth of search all sibling pick by label jobs related to the job is more than 10.");
				}
			}

			return siblingPickByLabelJobs.ToArray();

			ZGuid[] AddPickByLabelJobToSiblingPackageJobsAndReturnPickByLabelJobsToSearch(WhsPickByLabelJob[] pickByLabelJobs)
			{
				var pickByLabelJobPKToSearch = new List<ZGuid>();
				foreach (var pickByLabelJob in pickByLabelJobs)
				{
					if (!pickByLabelJob.PK.Equals(PickJobPk) && siblingPickByLabelJobs.Add(pickByLabelJob))
					{
						pickByLabelJobPKToSearch.Add(pickByLabelJob.PK);
					}
				}

				return pickByLabelJobPKToSearch.ToArray();
			}
		}

		ZGuid[] GetPackageJobPKs(ZGuid[] pickByLabelJobPKs)
		{
			var getParentPackageJobPKsQuery = @"
SELECT
	DISTINCT KP_KJ_ParentPackageJob
FROM
	dbo.PkgPackage
	JOIN dbo.WhsPickByLabelLabel ON WTL_KP_Package = KP_PK
WHERE
	WTL_WTK_PickByLabelJob IN (SELECT Value FROM @PickByLabelJobPKs)
";

			var packageJobPKs = new DynamicBusinessObjectCollection(Factory);
			packageJobPKs.Load(getParentPackageJobPKsQuery, new[] { ZSqlParameter.New("@PickByLabelJobPKs", pickByLabelJobPKs, WhsPickByLabelLabelSchema.WTL_WTK_PickByLabelJob, isTableValued: true) });
			return packageJobPKs.Select(j => (ZGuid)j[PkgPackageSchema.Constants.KP_KJ_ParentPackageJob]).ToArray();
		}

		WhsPickByLabelJob[] GetPickByLabelJobs(ZGuid[] packageJobPKs)
		{
			var packageSubQuery = new ZDBOnlySubQuery(typeof(PkgPackage), PkgPackageSchema.PK);
			packageSubQuery.AddToFilter(PkgPackageSchema.KP_KJ_ParentPackageJob, packageJobPKs);

			var pickByLabelLabelSubQuery = new ZDBOnlySubQuery(typeof(WhsPickByLabelLabel), WhsPickByLabelLabelSchema.WTL_WTK_PickByLabelJob);
			pickByLabelLabelSubQuery.AddSubQuery(WhsPickByLabelLabelSchema.WTL_KP_Package, packageSubQuery, JoinCondition.And);

			var query = new ZDBOnlyQuery(typeof(WhsPickByLabelJob));
			query.AddSubQuery(pickByLabelLabelSubQuery, JoinCondition.And);

			return Factory.Load<WhsPickByLabelJob>(query);
		}

		class PickByLabelPickLines
		{
			public PickByLabelPickLines(WhsPickLine[] pickedPickLines, WhsPickLine[] unpickedPickLines)
			{
				Argument.NotNull(pickedPickLines, nameof(pickedPickLines));
				Argument.NotNull(unpickedPickLines, nameof(unpickedPickLines));

				PickedPickLines = pickedPickLines;
				UnpickedPickLines = unpickedPickLines;
			}

			public WhsPickLine[] PickedPickLines { get; }
			public WhsPickLine[] AllPickLines => PickedPickLines.Union(UnpickedPickLines).ToArray();
			WhsPickLine[] UnpickedPickLines { get; }
		}
	}
}
