using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Freight.Module
{
	public static class VoyageVesselModuleFilterHelper
	{
		public static ZQuery GetBasicVoyageVesselQuery(SQLComparisonOperator @operator, ZString voyage, ZString vessel, ZBool includeArchived, SchemaColumn voyageColumn, SchemaColumn vesselColumn)
		{
			var query = new ZQuery();

			if (!voyage.IsEmpty || @operator == SpecialComparisonOperator.IsBlank || @operator == SpecialComparisonOperator.IsNotBlank)
			{
				query.AddToFilter_PossiblyCommaSeparated(voyageColumn, @operator, voyage);
			}

			if (!vessel.IsEmpty || @operator == SpecialComparisonOperator.IsBlank || @operator == SpecialComparisonOperator.IsNotBlank)
			{
				query.AddToFilter_PossiblyCommaSeparated(vesselColumn, @operator, vessel);
			}

			query.IgnoreActiveFilter = includeArchived;

			return query;
		}

		public static void ApplyBasicVoyageVesselQuery(ZQuery query, SQLComparisonOperator @operator, ZString voyage, ZString vessel, ZBool includeArchived, SchemaColumn voyageColumn, SchemaColumn vesselColumn)
		{
			if (!voyage.IsEmpty || @operator == SpecialComparisonOperator.IsBlank || @operator == SpecialComparisonOperator.IsNotBlank)
			{
				query.AddToFilter_PossiblyCommaSeparated(voyageColumn, @operator, voyage);
			}

			if (!vessel.IsEmpty || @operator == SpecialComparisonOperator.IsBlank || @operator == SpecialComparisonOperator.IsNotBlank)
			{
				query.AddToFilter_PossiblyCommaSeparated(vesselColumn, @operator, vessel);
			}

			query.IgnoreActiveFilter = includeArchived;
		}
	}
}
