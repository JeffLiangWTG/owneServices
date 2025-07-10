using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Module
{
	class LocationProvider : ILocationProvider
	{
		public ZQuery GetLocationQuery(LocationTypeEnum locationType, string codeOrDescription)
		{
			var query = LocationFilterBusinessObject.GetLocationTypeQuery(locationType, needsActiveQuery: true);
			var codeDescriptionQuery = new ZQuery();
			if (!string.IsNullOrEmpty(codeOrDescription))
			{
				codeDescriptionQuery.AddToFilter(LocationFilterBusinessObject.GetLocationCodeQuery(SQLComparisonOperator.Contains, codeOrDescription, locationType));
				codeDescriptionQuery.AddToFilter(LocationFilterBusinessObject.GetLocationDescQuery(SQLComparisonOperator.Contains, codeOrDescription, locationType), JoinCondition.Or);
			}
			query.AddToFilter(codeDescriptionQuery);
			return query;
		}
	}
}
