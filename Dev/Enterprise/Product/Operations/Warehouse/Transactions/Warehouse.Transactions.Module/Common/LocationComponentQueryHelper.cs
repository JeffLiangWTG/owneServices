using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Module
{
	static class LocationComponentQueryHelper
	{
		public static ZDBOnlyQuery GetLocationParserSubQueryForInventory(SchemaColumn locationComponent, SchemaColumn warehouseComponentAlpha, SchemaColumn warehouseComponentZeroBased, ZString value)
		{
			var isAlphanumerical = value.IsLettersOnlyOrEmpty;
			return WhsInventoryView.GetInventoryQueryByLocation(s =>
			{
				var alphanumericalSubQuery = GetLocationParserQueryCore(locationComponent, warehouseComponentAlpha, warehouseComponentZeroBased, GetParsedValue(value, isAlphanumerical), isAlphanumerical, false, true);
				var notZeroBasedSubQuery = GetLocationParserQueryCore(locationComponent, warehouseComponentAlpha, warehouseComponentZeroBased, GetParsedValue(value, isAlphanumerical), isAlphanumerical, false, false);
				var zeroBasedSubQuery = GetLocationParserQueryCore(locationComponent, warehouseComponentAlpha, warehouseComponentZeroBased, GetParsedValue(value, isAlphanumerical) + 1, isAlphanumerical, true, false);

				s.AddToFilter(alphanumericalSubQuery, JoinCondition.Or);
				s.AddToFilter(notZeroBasedSubQuery, JoinCondition.Or);
				s.AddToFilter(zeroBasedSubQuery, JoinCondition.Or);
			});
		}

		public static ZDBOnlyQuery GetLocationParserSubQueryForStocktake(SchemaColumn locationComponent, SchemaColumn warehouseComponentAlpha, SchemaColumn warehouseComponentZeroBased, ZString value)
		{
			var isAlphanumerical = value.IsLettersOnlyOrEmpty;
			var query = new ZDBOnlyQuery(typeof(WhsStocktakeLine));

			var alphanumericalSubQuery = GetLocationParserSubQueryCore(locationComponent, WhsStocktakeLineSchema.WU_WL, warehouseComponentAlpha, warehouseComponentZeroBased, GetParsedValue(value, isAlphanumerical), isAlphanumerical, false, true);
			var notZeroBasedSubQuery = GetLocationParserSubQueryCore(locationComponent, WhsStocktakeLineSchema.WU_WL, warehouseComponentAlpha, warehouseComponentZeroBased, GetParsedValue(value, isAlphanumerical), isAlphanumerical, false, false);
			var zeroBasedSubQuery = GetLocationParserSubQueryCore(locationComponent, WhsStocktakeLineSchema.WU_WL, warehouseComponentAlpha, warehouseComponentZeroBased, GetParsedValue(value, isAlphanumerical) + 1, isAlphanumerical, true, false);

			query.AddSubQuery(alphanumericalSubQuery, JoinCondition.Or);
			query.AddSubQuery(notZeroBasedSubQuery, JoinCondition.Or);
			query.AddSubQuery(zeroBasedSubQuery, JoinCondition.Or);

			return query;
		}

		static ZShort GetParsedValue(ZString value, ZBool isAlphanumerical)
		{
			var parser = new LocationParser();
			return parser.Parse(value, isAlphanumerical);
		}

		static ZDBOnlyQuery GetLocationParserQueryCore(SchemaColumn locationComponent, SchemaColumn warehouseComponentAlpha, SchemaColumn warehouseComponentZeroBased, ZShort value, ZBool isValueAlphanumerical,
		   ZBool isValueZeroBased, ZBool lookForAlphanumericalLocations)
		{
			var result = new ZDBOnlyQuery(typeof(WhsLocation));
			AddLocationParserSubQueryCore(result, locationComponent, warehouseComponentAlpha, warehouseComponentZeroBased, value, isValueAlphanumerical, isValueZeroBased, lookForAlphanumericalLocations);

			return result;
		}

		static ZDBOnlySubQuery GetLocationParserSubQueryCore(SchemaColumn locationComponent, SchemaColumn subQueryColumn, SchemaColumn warehouseComponentAlpha, SchemaColumn warehouseComponentZeroBased, ZShort value, ZBool isValueAlphanumerical,
			ZBool isValueZeroBased, ZBool lookForAlphanumericalLocations)
		{
			var result = new ZDBOnlySubQuery(typeof(WhsLocation), subQueryColumn);
			AddLocationParserSubQueryCore(result, locationComponent, warehouseComponentAlpha, warehouseComponentZeroBased, value, isValueAlphanumerical, isValueZeroBased, lookForAlphanumericalLocations);

			return result;
		}

		static void AddLocationParserSubQueryCore(ZDBOnlyQuery result, SchemaColumn locationComponent, SchemaColumn warehouseComponentAlpha, SchemaColumn warehouseComponentZeroBased, ZShort value, ZBool isValueAlphanumerical,
			ZBool isValueZeroBased, ZBool lookForAlphanumericalLocations)
		{
			result.AddToFilter(locationComponent, value);

			var rowSubQuery = new ZDBOnlySubQuery(typeof(WhsRow), WhsLocationViewSchema.WLV_WR);

			var warehouseSubQuery = new ZDBOnlySubQuery(typeof(WhsWarehouse), WhsRowSchema.WR_WW_Whs);
			warehouseSubQuery.AddToFilter(warehouseComponentAlpha, lookForAlphanumericalLocations);
			warehouseSubQuery.AddToFilter(warehouseComponentAlpha, isValueAlphanumerical);
			if (!lookForAlphanumericalLocations)
			{
				warehouseSubQuery.AddToFilter(warehouseComponentZeroBased, isValueZeroBased);
			}

			rowSubQuery.AddSubQuery(warehouseSubQuery, JoinCondition.And);
			result.AddSubQuery(rowSubQuery, JoinCondition.And);
		}
	}
}
