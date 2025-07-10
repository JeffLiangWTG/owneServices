using System.ComponentModel;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Business
{
	public static class DefaultNumberOfDecimalsSupporterHelperForFreight
	{
		public static int GetDefaultNumberOfDecimalsMetaDataProperty<T>(T parent, PropertyDescriptor property)
			where T : BusinessObject, IDefaultNumberOfDecimalsSupporter
		{
			Argument.NotNull(parent, "DefaultNumberOfDecimalsHelperParent");

			if (!typeof(INumericZType).IsAssignableFrom(property.PropertyType))
			{
				return -1;
			}

			if (parent.IsDeleted)
			{
				return DefaultNumberOfDecimals.Schema.DefaultNumberOfDecimalsForWeightAndVolumeUnits;
			}

			var unitOfMeasure = parent.GetUnitOfMeasure(property);
			int result = FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.Value.GetNumberOfDecimals(parent.TransportMode, unitOfMeasure);

			if (result < 0)
			{
				result = (int)MetaData.GetMetaData(parent, property, MetaDataTypes.DecimalPlaces, true);

				if (result < 0)
				{
					result = GetNumberOfDecimalsFromDatabaseSchema(parent, property);
				}
			}

			return (result < 0) ? 0 : result;
		}

		public static int GetNumberOfDecimalsFromDatabaseSchema<T>(T parent, PropertyDescriptor property)
			where T : BusinessObject, IDefaultNumberOfDecimalsSupporter
		{
			Argument.NotNull(parent, "DefaultNumberOfDecimalsHelperParent");

			int result = -1;
			var propertyName = DefaultNumberOfDecimals.GetBoundPropertyName(property.Name);
			var columnSchema = ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumnSafe(propertyName, parent.TableName) as SchemaDecimalColumn;

			if (columnSchema != null)
			{
				result = columnSchema.Scale;
			}

			return result;
		}

		public static ZString GetRoundingMode<T>(T parent, PropertyDescriptor property)
			where T : BusinessObject, IDefaultNumberOfDecimalsSupporter
		{
			Argument.NotNull(parent, "DefaultNumberOfDecimalsHelperParent");

			if (!typeof(INumericZType).IsAssignableFrom(property.PropertyType))
			{
				return ZString.Empty;
			}

			var unitOfMeasure = parent.GetUnitOfMeasure(property);
			var result = FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.Value.GetRoundingMode(parent.TransportMode, unitOfMeasure);

			return result;
		}

		public static ZDecimal GetRoundedValue<T>(T parent, PropertyDescriptor property, ZDecimal value)
			where T : BusinessObject, IDefaultNumberOfDecimalsSupporter
		{
			var roundingMode = GetRoundingMode(parent, property);
			var decimals = GetDefaultNumberOfDecimalsMetaDataProperty(parent, property);

			var result = DefaultNumberOfDecimals.GetRoundedValue(value, roundingMode, decimals);

			return result;
		}

		public static bool IsRegistryDecimalValueOverridden<T>(T parent, PropertyDescriptor property)
			where T : BusinessObject, IDefaultNumberOfDecimalsSupporter
		{
			var unitOfMeasure = parent.GetUnitOfMeasure(property);
			return (FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.Value.GetDefaultValueForModeAndUnit(parent.TransportMode, unitOfMeasure) != null);
		}
	}
}
