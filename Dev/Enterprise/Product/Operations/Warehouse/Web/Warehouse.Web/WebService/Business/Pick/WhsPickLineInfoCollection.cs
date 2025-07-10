using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService.Business
{
#if DEBUG
	[Serializable]
#endif
	public class WhsPickLineInfoCollection : DataObjectInfoCollection<WhsPickLineInfo>
	{
		#region Constructors

		public WhsPickLineInfoCollection()
		{
		}

		public WhsPickLineInfoCollection(IEnumerable<WhsPickLine> pickLines, WhsPickJobInfo parent)
		{
			var dockets = pickLines.Select(l => l.InventoryLine).Select(il => il.Docket);
			if (dockets.Any())
			{
				JobDocAddressFetchHintsHelper.AddFetchHints(dockets.Select(d => d.PK), pickLines.First().Factory);
			}

			foreach (var pickLineGroup in pickLines.GroupBy(pl => new PickLineGroupingInfo(pl)))
			{
				var pickLinePKs = pickLineGroup.Select(pl => pl.PK).ToArray();
				var pickLineInfo = new WhsPickLineInfo(pickLineGroup.Key, pickLinePKs, pickLineGroup.Sum(pl => pl.WZ_Units), parent);
				Add(pickLineInfo);
			}
		}

		public WhsPickLineInfoCollection(SortedDictionary<WhsPickLine, PickLineAdditionalInfo> pickLines, WhsPickJobInfo parent)
		{
			AddFetchHints(pickLines.Keys.ToArray());

			foreach (var pickLineGroup in pickLines.GroupBy(pl => new PickLineGroupingInfo(pl.Key, pl.Value.PackageID, pl.Value.SlotNumber)))
			{
				var pickLinePKs = pickLineGroup.Select(pl => pl.Key.PK).ToArray();
				var pickLineInfo = new WhsPickLineInfo(pickLineGroup.Key, pickLinePKs, pickLineGroup.Sum(pl => pl.Key.WZ_Units), parent, pickLineGroup.Key.PackageID, pickLineGroup.Key.SlotNumber);
				Add(pickLineInfo);
			}
		}

		void AddFetchHints(WhsPickLine[] pickLines)
		{
			if (pickLines.Length > 0)
			{
				var factory = pickLines[0].Factory;

				var docketPKs = pickLines.Select(l => l.InventoryLine.WE_WD).Distinct().ToArray();
				factory.AddFetchHint(WhsDocketSchema.Instance, new ZQuery(WhsDocketSchema.PK, docketPKs));

				JobDocAddressFetchHintsHelper.AddFetchHints(docketPKs, factory);

				foreach (var (locationPK, partPK) in pickLines.Select(pl => (pl.InventoryLine.Location.PK, pl.Inventory.SupplierPart.PK)).Distinct())
				{
					var productsInLocationQuery = new ZQuery();
					productsInLocationQuery.AddToFilter(WhsInventoryViewSchema.WI_WL, locationPK);
					productsInLocationQuery.AddToFilter(WhsInventoryViewSchema.WI_OP, SQLComparisonOperator.NotEqual, partPK);
					productsInLocationQuery.AddToFilter(WhsInventoryViewSchema.WI_TotalUnits, SQLComparisonOperator.GreaterThan, 0m);
					factory.AddFetchHint(WhsInventoryViewSchema.Instance, productsInLocationQuery);
				}
			}
		}

		#endregion
	}

	#region PickLineAdditionalInfo class

	public class PickLineAdditionalInfo
	{
		public PickLineAdditionalInfo(ZString packageID, ZShort slotNumber)
		{
			PackageID = packageID;
			SlotNumber = slotNumber;
		}

		public readonly ZString PackageID;
		public readonly ZShort SlotNumber;
	}

	#endregion
}
