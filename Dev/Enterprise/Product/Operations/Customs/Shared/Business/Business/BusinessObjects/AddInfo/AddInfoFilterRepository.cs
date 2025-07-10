using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public static class AddInfoFilterRepository
	{
		public static ZQuery GetAddInfoQuery(SQLComparisonOperator comparisonOperator, ZString value, SchemaStringColumn addInfoSchemaColumn, string addInfoProperty)
		{
			var result = new ZQuery();

			if (!value.IsEmpty)
			{
				if (comparisonOperator == SQLComparisonOperator.StartsWith)
				{
					result.AddToFilter(JoinCondition.And, addInfoSchemaColumn, SQLComparisonOperator.StartsWith, addInfoProperty + "=" + value);
					result.AddToFilter(JoinCondition.Or, addInfoSchemaColumn, SQLComparisonOperator.Contains, "*" + addInfoProperty + "=" + value);
				}
				else
				{
					result.AddToFilter(JoinCondition.And, addInfoSchemaColumn, SQLComparisonOperator.Equal, addInfoProperty + "=" + value);
					result.AddToFilter(JoinCondition.Or, addInfoSchemaColumn, SQLComparisonOperator.StartsWith, addInfoProperty + "=" + value + "*");
					result.AddToFilter(JoinCondition.Or, addInfoSchemaColumn, SQLComparisonOperator.Contains, "*" + addInfoProperty + "=" + value + "*");
					result.AddToFilter(JoinCondition.Or, addInfoSchemaColumn, SQLComparisonOperator.EndsWith, "*" + addInfoProperty + "=" + value);
				}
			}

			return result;
		}
	}
}
