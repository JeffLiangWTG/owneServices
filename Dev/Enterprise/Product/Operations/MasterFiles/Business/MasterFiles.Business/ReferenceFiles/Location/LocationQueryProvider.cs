using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class LocationQueryProvider : ZArchitecture.Business.IQueryProvider
	{
		public LocationQueryProvider(BusinessObjectFactory factory, SchemaColumn propertySchema, Type typeToQuery)
		{
			this.PropertySchema = propertySchema;
			this.TypeToQuery = typeToQuery;
			this.Factory = factory;
		}

		public ZQuery GetQuery(SQLComparisonOperator @operator, object value)
		{
			ZQuery result = new ZQuery();
			if (!string.IsNullOrEmpty(PropertySchema.Name))
			{
				if (value is string || value is ZString)
				{
					ZQuery locationFilter = LocationHelper.GetLocationFilter(Factory, value.ToString(), PropertySchema, TypeToQuery);
					result.AddToFilter(locationFilter, JoinCondition.Or);
				}
				else if (value is ZString[])
				{
					ZQuery locationFilter = LocationHelper.GetLocationFilter(Factory, PropertySchema, false, TypeToQuery, (ZString[])value);
					result.AddToFilter(locationFilter, JoinCondition.Or);
				}
			}
			return result;
		}

		readonly SchemaColumn PropertySchema;
		readonly Type TypeToQuery;
		readonly BusinessObjectFactory Factory;
	}
}
