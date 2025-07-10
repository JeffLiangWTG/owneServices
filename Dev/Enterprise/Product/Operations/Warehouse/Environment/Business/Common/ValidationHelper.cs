using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Environment.Business
{
	public static class ValidationHelper
	{
		public static bool CheckIfLocationsHaveCustomsStockOnHand(BusinessObjectFactory factory, params ZGuid[] locationPks)
			=> CheckIfLocationsHaveStockOnHand(factory, customsOnly: true, locationPks: locationPks);

		public static bool CheckIfLocationsHaveStockOnHand(BusinessObjectFactory factory, params ZGuid[] locationPks)
			=> CheckIfLocationsHaveStockOnHand(factory, customsOnly: false, locationPks: locationPks);

		static bool CheckIfLocationsHaveStockOnHand(BusinessObjectFactory factory, bool customsOnly, params ZGuid[] locationPks)
		{
			var result = false;
			if (locationPks.Length > 0)
			{
				var docketLineQuery = new ZDBOnlyQuery(typeof(IWhsDocketLine));
				docketLineQuery.AddToFilter(WhsDocketLineSchema.WE_StockOnHand, SQLComparisonOperator.GreaterThan, 0m);
				docketLineQuery.AddToFilter(WhsDocketLineSchema.WE_WL, locationPks);

				if (customsOnly)
				{
					var docketQuery = new ZDBOnlySubQuery(typeof(IWhsDocket), WhsDocketSchema.PK);
					docketQuery.AddToFilter(WhsDocketSchema.WD_DocketSubType, SQLComparisonOperator.Equal, "CUS");
					docketLineQuery.AddSubQuery(WhsDocketLineSchema.WE_WD, docketQuery, JoinCondition.And);
				}

				result = factory.LoadTop1<IWhsDocketLine>(docketLineQuery) != null;
			}

			return result;
		}

		public static bool CheckIfLocationsHaveTransitPackage(BusinessObjectFactory factory, params ZGuid[] locationPks)
		{
			var result = false;
			if (locationPks.Length > 0)
			{
				var packageStateQuery = new ZDBOnlyQuery(typeof(IWhsItemPackageState));
				packageStateQuery.AddToFilter(WhsItemPackageStateSchema.WPS_WL_LastLocation, locationPks);
				packageStateQuery.AddToFilter(WhsItemPackageStateSchema.WPS_IsHandlingUnit, false);
				packageStateQuery.AddToFilter(WhsItemPackageStateSchema.WPS_WDH_TransitDispatchHeader, null);

				result = factory.LoadTop1<IWhsItemPackageState>(packageStateQuery) != null;
			}

			return result;
		}
	}
}
