using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Transactions.PickByLabel;
using Enterprise.Warehouse.Transactions.TrolleyPicking;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService
{
	public static class PutawayStockInDockDoorOrPackingStationHelper
	{
		public static PkgPackage[] LoadPackagesForPickByLabelJob(BusinessObjectFactory factory, Guid jobPK)
		{
			var pickByLabelLabelQuery = new ZDBOnlySubQuery(typeof(WhsPickByLabelLabel), PkgPackageSchema.PK, WhsPickByLabelLabelSchema.WTL_KP_Package);
			pickByLabelLabelQuery.AddToFilter(WhsPickByLabelLabelSchema.WTL_WTK_PickByLabelJob, jobPK);

			var pickByLabelPackagesQuery = new ZDBOnlyQuery(typeof(PkgPackage));
			pickByLabelPackagesQuery.AddToFilter(PkgPackageSchema.KP_KP_ParentPackage, null);
			pickByLabelPackagesQuery.AddToFilter(PkgPackageSchema.KP_KPH_PackageHeader, SQLComparisonOperator.NotEqual, null);

			pickByLabelPackagesQuery.AddSubQuery(pickByLabelLabelQuery, JoinCondition.And);
			AddPackageJobFetchHints(factory, pickByLabelPackagesQuery);

			return factory.Load<PkgPackage>(pickByLabelPackagesQuery);
		}

		public static void AddPackageJobFetchHints(BusinessObjectFactory factory, ZQuery pkgPackageQuery)
		{
			var packageRows = ((IBusinessObjectFactoryInternals)factory).RowFactory.Load(PkgPackageSchema.Constants.TableName, pkgPackageQuery); // avoid triggering type decider.
			var packageJobPKs = packageRows.Select(row => (Guid)row[nameof(PkgPackageSchema.KP_KJ_ParentPackageJob)]);
			factory.AddFetchHint(PkgPackageJobSchema.Instance, new ZQuery(PkgPackageJobSchema.PK, packageJobPKs));
		}

		public static (Dictionary<ZGuid, WhsPickLine>, IEnumerable<PkgPackageItemDivot>) GetPickLineLookup(BusinessObjectFactory factory, PkgPackage[] packages, PickJobType pickJobType)
		{
			var allPackageDivots = packages.Where(p => !p.KP_KPH_PackageHeader.IsEmpty).SelectMany(p => p.PackedItemDivots);
			var allPickLinePKs = allPackageDivots.Select(d => d.KI_ParentID);
			var pickLineLookup = factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.PK, allPickLinePKs)).ToDictionary(pl => pl.PK);
			AddFetchHintsForInTransitPickLines(factory, pickLineLookup.Values, pickJobType);
			return (pickLineLookup, allPackageDivots);
		}

		public static (Dictionary<ZGuid, WhsPickLine>, Dictionary<ZGuid, PkgPackageItemDivot[]>) GetPickLineAndPackageDivotLookups(BusinessObjectFactory factory, PkgPackage[] packages, PickJobType pickJobType)
		{
			var (pickLineLookup, allPackageDivots) = GetPickLineLookup(factory, packages, pickJobType);
			var packageDivotLookup = allPackageDivots.GroupBy(d => d.KI_KP_Package).ToDictionary(group => group.Key, group => group.ToArray());
			return (pickLineLookup, packageDivotLookup);
		}

		static void AddFetchHintsForInTransitPickLines(BusinessObjectFactory factory, IEnumerable<WhsPickLine> allPickLines, PickJobType pickJobType)
		{
			// Orders + Consignees for Pick Jobs are already loaded in memory, so not required
			if (pickJobType != PickJobType.Pick)
			{
				var orderLines = factory.Load<WhsOrderLine>(new ZQuery(WhsDocketLineSchema.PK, allPickLines.Select(pl => pl.WZ_WE_TransactionLine)));

				var orderPKs = orderLines.Select(ol => ol.WE_WD).ToArray();
				factory.AddFetchHint(typeof(WhsOrder), new ZQuery(WhsDocketSchema.PK, orderPKs));
				factory.AddFetchHint(JobDocAddressSchema.Instance, new ZQuery(JobDocAddressSchema.E2_ParentID, orderPKs));
			}

			// Add fetch hints for WhsTransferLine + WhsPickLines
			var allTransferLinePKs = allPickLines.Where(pl => !pl.WZ_WE_OriginalPickedInventoryLine.IsEmpty).Select(pl => pl.WZ_WE_InventoryLine).ToArray();
			factory.AddFetchHint(WhsDocketLineSchema.Instance, new ZQuery(WhsDocketLineSchema.PK, allTransferLinePKs));
			factory.AddFetchHint(WhsPickLineSchema.Instance, new ZQuery(WhsPickLineSchema.WZ_WE_TransactionLine, allTransferLinePKs));

			// Required for updating WE_WL
			factory.AddFetchHint(WhsDocketLineSchema.Instance, new ZQuery(WhsDocketLineSchema.WE_WE_MatchingLine, allTransferLinePKs));
			factory.AddFetchHint(WhsDocketLineSchema.Instance, new ZQuery(WhsDocketLineSchema.WE_WE_ParentDocketLine, allTransferLinePKs));

			// Add Fetch Hints for Transfer Dockets
			var transferLineRows = ((IBusinessObjectFactoryInternals)factory).RowFactory.Load(WhsDocketLineSchema.Constants.TableName, new ZQuery(WhsDocketLineSchema.PK, allTransferLinePKs));
			var transferPKs = transferLineRows.Select(row => (Guid)row[nameof(WhsDocketLineSchema.WE_WD)]);
			factory.AddFetchHint(typeof(WhsTransfer), new ZQuery(WhsDocketSchema.PK, transferPKs));
		}

		public static PkgPackage[] LoadOuterPackagesFromPackageJobs(BusinessObjectFactory factory, IEnumerable<ZGuid> packageJobPKs)
		{
			var allOutersQuery = new ZQuery();
			allOutersQuery.AddToFilter(PkgPackageSchema.KP_KJ_ParentPackageJob, packageJobPKs);
			allOutersQuery.AddToFilter(PkgPackageSchema.KP_KP_ParentPackage, null);

			return factory.Load<PkgPackage>(allOutersQuery);
		}

		public static IEnumerable<WhsTransferLine> GetInTransitTransferLinesFromPickLines(BusinessObjectFactory factory, IEnumerable<WhsPickLine> pickLines, ZString rfUser)
		{
			var inventoryLineQuery = new ZQuery(WhsDocketLineSchema.PK, pickLines.Where(pl => !pl.WZ_WE_OriginalPickedInventoryLine.IsEmpty).Select(pl => pl.WZ_WE_InventoryLine));
			var inventoryLines = factory.Load<WhsTransferLine>(inventoryLineQuery).Where(tl => tl.WE_GS_NKPutawayBy.EqualsIgnoringCase(rfUser) && tl.WE_CurrentInventoryStatus == InventoryStatus.Codes.InTransit);
			return inventoryLines;
		}

		public static PkgPackage[] LoadPackagesForTrolleyJob(BusinessObjectFactory factory, Guid jobPK)
		{
			var slotsSubQuery = new ZDBOnlySubQuery(typeof(WhsPickTrolleySlot), PkgPackageSchema.PK, WhsPickTrolleySlotSchema.WTS_KP_Package);
			slotsSubQuery.AddToFilter(WhsPickTrolleySlotSchema.WTS_WTJ_TrolleyJob, jobPK);

			var trolleyPackagesQuery = new ZDBOnlyQuery(typeof(PkgPackage));
			trolleyPackagesQuery.AddToFilter(PkgPackageSchema.KP_KP_ParentPackage, null);
			trolleyPackagesQuery.AddToFilter(PkgPackageSchema.KP_KPH_PackageHeader, SQLComparisonOperator.NotEqual, null);

			trolleyPackagesQuery.AddSubQuery(slotsSubQuery, JoinCondition.And);

			AddPackageJobFetchHints(factory, trolleyPackagesQuery);

			return factory.Load<PkgPackage>(trolleyPackagesQuery);
		}

		public static PickByBOMComponentLineInfo LoadComponentPickLinesFromKitPickLines(BusinessObjectFactory factory, ZGuid[] kitPickLinePKs, bool isUnPickedComponentOnly)
		{
			var rawSql = @$"
SELECT
	ParentPickLine.WZ_PK AS ParentPickLinePK,
	ParentOrderLine.WE_PK AS ParentOrderLinePK,
	ComponentPickLine.WZ_PK AS ComponentPickLinePK,
	ComponentOrderLine.WE_OP AS ComponentProductPK
FROM
	dbo.WhsPickLine ParentPickLine
	JOIN dbo.WhsDocketLine ParentOrderLine ON ParentOrderLine.WE_PK = ParentPickLine.WZ_WE_TransactionLine
	JOIN dbo.WhsDocketLine ComponentOrderLine ON ComponentOrderLine.WE_WE_ParentDocketLine = ParentOrderLine.WE_PK
	JOIN dbo.WhsPickLine ComponentPickLine ON ComponentPickLine.WZ_WE_TransactionLine = ComponentOrderLine.WE_PK
WHERE
	ParentPickLine.WZ_PK IN (SELECT VALUE FROM @KitPickLinePKs)
	{(isUnPickedComponentOnly ? "AND ComponentPickLine.WZ_PickedDateTime IS NULL AND ComponentPickLine.WZ_WE_OriginalPickedInventoryLine IS NULL" : "")}
";

			var pickLinePKs = new DynamicBusinessObjectCollection(factory);
			var tvp = ZSqlParameter.New("@KitPickLinePKs", kitPickLinePKs, WhsPickLineSchema.PK, isTableValued: true);
			pickLinePKs.Load(rawSql, new[] { tvp });

			var componentPickLinePKs = new List<ZGuid>();
			var relationPKs = new List<PickByBOMComponentKitRelation>();
			foreach (var pickLinePKRelation in pickLinePKs)
			{
				var parentPickLinePK = (ZGuid)pickLinePKRelation["ParentPickLinePK"];
				var parentOrderLinePK = (ZGuid)pickLinePKRelation["ParentOrderLinePK"];
				var componentPickLinePK = (ZGuid)pickLinePKRelation["ComponentPickLinePK"];
				var componentProductPK = (ZGuid)pickLinePKRelation["ComponentProductPK"];

				componentPickLinePKs.Add(componentPickLinePK);

				var relationPK = new PickByBOMComponentKitRelation(parentPickLinePK, parentOrderLinePK, componentPickLinePK, componentProductPK);
				relationPKs.Add(relationPK);
			}

			var componentPickLines = factory.Load<WhsPickLine>(new ZQuery(WhsPickLineSchema.PK, componentPickLinePKs));

			return new PickByBOMComponentLineInfo(componentPickLines, relationPKs);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Used for Generating SQL")]
		public static IReadOnlyCollection<ZGuid> BuildAndRunQueryForPicksWithLooseInventory(BusinessObjectFactory factory, ZGuid[] pickPKs, bool topOneOnly)
		{
			var selectSQLClause = topOneOnly
				? "TOP 1 WD_WP"
				: "WD_WP";

			var sql = $@"
SELECT {selectSQLClause} 
FROM
    dbo.WhsDocket
    JOIN dbo.WhsDocketLine OrderLines on WE_WD = WD_PK 
    JOIN dbo.WhsPickLine PickLines on WZ_WE_TransactionLine = WE_PK
    LEFT JOIN dbo.PkgPackageItemDivot ON KI_ParentID = WZ_PK AND KI_ParentTableCode = 'WZ'
WHERE
    WD_WP IN (SELECT VALUE FROM @PickPKs) AND KI_ParentID IS NULL
GROUP BY
	WD_WP
";
			var hasLooseInventory = new DynamicBusinessObjectCollection(factory);
			var sqlParams = new ZSqlParameterCollection();
			sqlParams.Add(ZSqlParameter.New("@PickPKs", pickPKs, WhsDocketSchema.WD_WP, isTableValued: true));
			hasLooseInventory.Load(sql, sqlParams);

			return hasLooseInventory.Select(pl => (ZGuid)pl[WhsDocketSchema.WD_WP]).ToArray();
		}
	}
}
