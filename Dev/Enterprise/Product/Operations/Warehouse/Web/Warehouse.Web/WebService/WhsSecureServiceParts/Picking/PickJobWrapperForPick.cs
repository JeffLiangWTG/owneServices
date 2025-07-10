using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	class PickJobWrapperForPick : PickJobWrapper<WhsPick>
	{
		public PickJobWrapperForPick(BusinessObjectFactory factory, Guid jobPk, WhsPick pick)
			: base(factory, jobPk, pick)
		{
		}

		protected override bool IsPackingStationSupportedForPickJobType() => !IsPickAndPackEnabled(PickJob);

		protected override bool IsConsolidationLocationAllowedForPick(WhsPick[] picks)
		{
			return WebServiceHelper.IsPickAndPackEnabled(picks[0]) ? picks[0].Orders[0].WD_UseDirectedPackingConsolidation : base.IsConsolidationLocationAllowedForPick(picks);
		}

		public override PutawayTransferLinesAndPackagesToClose GetTransferLinesToPutawayAndPackagesToClose(Guid jobPk, PickJobType pickJobType, WhsLocation location, ZString rfUser, WebServiceResponse response)
		{
			var transferLines = Array.Empty<WhsTransferLine>();

			if (CheckAndSetPackingStationOnPicks(new[] { PickJob }, location, response))
			{
				var orders = PickJob.Orders.Cast<WhsOrder>().ToArray();
				Factory.AddFetchHint(typeof(PkgPackageJob), new ZQuery(PkgPackageJobSchema.KJ_ParentID, orders.Select(o => o.PK)));
				var pickLinesForTotes = new HashSet<WhsPickLine>(orders.SelectMany(o => o.PackageJob?.GetAllPackagesOnJob() ?? Array.Empty<PkgPackage>()).Where(p => p.IsToteOrParentIsTote()).SelectMany(p => p.GetPickLines()));
				var uomTypesToPutaway = new HashSet<string>(GetUOMTypesToPutaway(PickJob));

				var pickLines = PickJob.GetAllPickLines();
				var filteredPickLines = pickLines.Where(pl => uomTypesToPutaway.Contains(pl.AllocatedPackType?.F3_UOMType ?? string.Empty) && !pickLinesForTotes.Contains(pl));

				transferLines = PutawayStockInDockDoorOrPackingStationHelper.GetInTransitTransferLinesFromPickLines(Factory, filteredPickLines, rfUser).ToArray();
			}

			return new PutawayTransferLinesAndPackagesToClose(transferLines, Array.Empty<PkgPackage>());
		}

		static IEnumerable<string> GetUOMTypesToPutaway(WhsPick pick)
		{
			yield return string.Empty;

			// Exclude split cases because they should be handled by Trolley Picking
			if (!pick.WP_CartoniseSplitCases)
			{
				yield return UOMPackTypesList.Codes.SplitCase;
			}

			// Exclude cases because they should be handled by Pick By Label
			if (!pick.WP_PickCasesByLabel)
			{
				yield return UOMPackTypesList.Codes.Case;
			}

			// Exclude pallets because they should be handled by Pick By Label
			if (!pick.WP_PickPalletsByLabel)
			{
				yield return UOMPackTypesList.Codes.Pallet;
			}
		}

		static bool IsPickAndPackEnabled(WhsPick pick) => pick != null
			&& pick.Orders.Count == 1
			&& WebServiceHelper.GetPickPackParameter((WhsOrder)pick.Orders[0]) != null;

		protected override WhsPick[] GetPicksForJob() => new[] { PickJob };
	}
}
