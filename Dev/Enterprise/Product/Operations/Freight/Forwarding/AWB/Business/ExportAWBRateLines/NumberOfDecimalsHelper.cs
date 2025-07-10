using System;
using System.ComponentModel;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Forwarding.AWB.Business
{
	static class NumberOfDecimalsHelper
	{
		public static int GetNumberOfDecimals(BusinessObject host, PropertyDescriptor property, Func<PropertyDescriptor, int> numberOfDecimalsGetter)
		{
			if (!typeof(INumericZType).IsAssignableFrom(property.PropertyType))
			{
				return -1;
			}

			int result = numberOfDecimalsGetter(property);
			if (result < 0)
			{
				result = (int)MetaData.GetMetaData(host, property, MetaDataTypes.DecimalPlaces, true);

				if (result < 0)
				{
					result = GetNumberOfDecimalsFromDatabaseSchema(host, property);
				}
			}
			return (result < 0) ? 0 : result;
		}

		static int GetNumberOfDecimalsFromDatabaseSchema(BusinessObject host, PropertyDescriptor property)
		{
			int result = -1;
			var propertyName = DefaultNumberOfDecimals.GetBoundPropertyName(property.Name);
			var columnSchema = ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumnSafe(propertyName, host.TableName) as SchemaDecimalColumn;

			if (columnSchema != null)
			{
				result = columnSchema.Scale;
			}

			return result;
		}
	}
}
