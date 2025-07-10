using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsPackageCollectionFetchStrategy : BusinessObjectCollectionFetchStrategy
	{
		public WhsPackageCollectionFetchStrategy(IBusinessObjectCollection collection)
			: base(collection)
		{
		}

		protected override void FetchForViewCore(BusinessObject[] businessObjects, TableColumn[] columns)
		{
			base.FetchForViewCore(businessObjects, columns);
			var factory = Collection.Factory;

			if (columns.Any(c => c.ColumnName == PkgPackage.Schema.PackageStatus))
			{
				foreach (var package in businessObjects)
				{
					var pivotQuery = new ZQuery(WhsLoadPkgPackagePivotSchema.WLP_KP_Package, package.PK);
					pivotQuery.AddToFilter(WhsLoadPkgPackagePivotSchema.WLP_UnloadedTime, SQLComparisonOperator.Equal, null);
					factory.AddFetchHint(WhsLoadPkgPackagePivotSchema.Instance, pivotQuery);

					var subQueryPPP = new ZDBOnlySubQuery(typeof(WhsLoadPkgPackagePivot), WhsLoadPkgPackagePivotSchema.WLP_WLO_Load);
					subQueryPPP.AddToFilter(pivotQuery);

					var queryLoad = new ZDBOnlyQuery(typeof(WhsLoad));
					queryLoad.AddSubQuery(subQueryPPP, JoinCondition.And);
					factory.AddFetchHint(WhsLoadSchema.Instance, queryLoad);
				}
			}

			var hasPickAreaColumn = columns.Any(c => c.ColumnName == WhsLocation.Schema.PickArea);
			var hasPickerColumn = columns.Any(c => c.ColumnName == WhsPick.Schema.Picker);

			if (hasPickAreaColumn || hasPickerColumn)
			{
				var pickLineDivotSubQuery = GetPickLineDivotQuery(businessObjects.Select(bo => bo.PK).ToArray());

				var pickLineQuery = new ZDBOnlyQuery(typeof(WhsPickLine));
				pickLineQuery.AddToFilter(WhsPickLineSchema.WZ_WE_OriginalPickedInventoryLine, SQLComparisonOperator.NotEqual, null);
				pickLineQuery.AddSubQuery(pickLineDivotSubQuery, JoinCondition.And);

				var inTransitPickLines = factory.Load<WhsPickLine>(pickLineQuery);

				if (hasPickAreaColumn)
				{
					var docketLineSubQuery = new ZDBOnlySubQuery(typeof(WhsDocketLine), WhsDocketLineSchema.WE_WL);
					docketLineSubQuery.AddSubQuery(GetPickLineQuery(pickLineDivotSubQuery), JoinCondition.Or);
					docketLineSubQuery.AddSubQuery(GetOriginalPickedInventoryLineQuery(pickLineDivotSubQuery), JoinCondition.Or);

					var locationForAreaSubQuery = new ZDBOnlySubQuery(typeof(WhsLocation), WhsLocationViewSchema.WLV_WA_PickingArea);
					locationForAreaSubQuery.AddSubQuery(docketLineSubQuery, JoinCondition.And);

					var areaQuery = new ZDBOnlyQuery(typeof(WhsArea));
					areaQuery.AddSubQuery(WhsAreaSchema.PK, locationForAreaSubQuery, JoinCondition.And);
					factory.AddFetchHint(WhsAreaSchema.Instance, areaQuery);

					if (!inTransitPickLines.IsNullOrEmpty())
					{
						var originalPickedInventoryLinePks = inTransitPickLines.Select(pl => pl.WZ_WE_OriginalPickedInventoryLine).Distinct();
						factory.AddFetchHint(WhsDocketLineSchema.Instance, new ZQuery(WhsDocketLineSchema.PK, originalPickedInventoryLinePks));

						var docketLineQuery = new ZDBOnlyQuery(typeof(WhsDocketLine));
						docketLineQuery.AddToFilter(WhsDocketLineSchema.PK, originalPickedInventoryLinePks);
						var locationPks = factory.Load<WhsDocketLine>(docketLineQuery).Select(dl => dl.WE_WL).Distinct();

						if (!locationPks.IsNullOrEmpty())
						{
							factory.AddFetchHint(WhsLocationViewSchema.Instance, new ZQuery(WhsLocationViewSchema.PK, locationPks));
						}
					}
				}

				if (hasPickerColumn && !inTransitPickLines.IsNullOrEmpty())
				{
					var inventoryLinePks = inTransitPickLines.Select(pl => pl.WZ_WE_InventoryLine).Distinct();
					factory.AddFetchHint(WhsPickLineSchema.Instance, new ZQuery(WhsPickLineSchema.WZ_WE_TransactionLine, inventoryLinePks));
				}
			}
		}

		ZDBOnlySubQuery GetPickLineQuery(ZDBOnlySubQuery pickLineDivotQuery)
		{
			var pickLineSubQuery = new ZDBOnlySubQuery(typeof(WhsPickLine), WhsPickLineSchema.WZ_WE_InventoryLine);
			pickLineSubQuery.AddToFilter(WhsPickLineSchema.WZ_WE_OriginalPickedInventoryLine, null);
			pickLineSubQuery.AddSubQuery(pickLineDivotQuery, JoinCondition.And);

			return pickLineSubQuery;
		}

		ZDBOnlySubQuery GetOriginalPickedInventoryLineQuery(ZDBOnlySubQuery pickLineDivotQuery)
		{
			var originalPickedInventoryLineQuery = new ZDBOnlySubQuery(typeof(WhsPickLine), WhsPickLineSchema.WZ_WE_OriginalPickedInventoryLine);
			originalPickedInventoryLineQuery.AddToFilter(WhsPickLineSchema.WZ_WE_OriginalPickedInventoryLine, SQLComparisonOperator.NotEqual, null);
			originalPickedInventoryLineQuery.AddSubQuery(pickLineDivotQuery, JoinCondition.And);

			return originalPickedInventoryLineQuery;
		}

		ZDBOnlySubQuery GetPickLineDivotQuery(ZGuid[] packagePKs)
		{
			var packageItemDivotSubQuery = new ZDBOnlySubQuery(typeof(PkgPackageItemDivot), PkgPackageItemDivotSchema.KI_ParentID) { AllowTableValuedParameters = true };
			packageItemDivotSubQuery.AddToFilter(PkgPackageItemDivotSchema.KI_ParentTableCode, WhsPickLineSchema.Constants.Prefix);
			packageItemDivotSubQuery.AddToFilter(PkgPackageItemDivotSchema.KI_KP_Package, packagePKs);

			return packageItemDivotSubQuery;
		}
	}
}
