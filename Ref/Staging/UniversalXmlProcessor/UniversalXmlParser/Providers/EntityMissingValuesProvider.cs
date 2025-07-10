using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.TypeProvider;
using CargoWise.RefDbRepo.UniversalXmlParser.Interfaces;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using SequentialGuid;

namespace CargoWise.RefDbRepo.UniversalXmlParser.Providers
{
	public class EntityMissingValuesProvider : IEntityMissingValuesProvider
	{
		readonly IEntityTypeHelper entityTypeHelper;
		readonly IEntityValuesProvider valuesProvider;
		public EntityMissingValuesProvider(IEntityTypeHelper entityTypeHelper, IEntityValuesProvider entityValuesProvider)
		{
			Argument.NotNull(entityTypeHelper, nameof(entityTypeHelper));
			Argument.NotNull(entityValuesProvider, nameof(entityValuesProvider));

			this.entityTypeHelper = entityTypeHelper;
			valuesProvider = entityValuesProvider;
		}

		public void SetPrimaryKey(object instance)
		{
			Argument.NotNull(instance, nameof(instance));
			var entityType = instance.GetType();
			var pk = entityTypeHelper.GetPrimaryKeyProperty(entityType);
			var sequentialGuidInstance = SequentialSqlGuidGenerator.Instance;
			pk.SetValue(instance, sequentialGuidInstance.NewGuid());
		}

		public void FillOutMissingValues(object instance)
		{
			Argument.NotNull(instance, nameof(instance));
			var entityType = instance.GetType();
			var properties = entityTypeHelper.GetProperties(entityType);

			foreach (var propertyInfo in properties)
			{
				var propertyType = propertyInfo.PropertyType;
				var existingValue = propertyInfo.GetValue(instance, null);
				if (existingValue != null)
				{
					if (propertyType.IsDateTime())
					{
						if ((DateTime)existingValue != DateTime.MinValue)
						{
							continue;
						}
					}
					else
					{
						continue;
					}
				}

				var isNullable = false;
				if (propertyType.IsNullable())
				{
					propertyType = Nullable.GetUnderlyingType(propertyInfo.PropertyType);
					isNullable = true;
				}

				var defaultValue = valuesProvider.GetDefaultValue(entityType.Name, propertyInfo.Name);
				if (defaultValue == null)
				{
					if (propertyType.IsStringType())
					{
						if (!valuesProvider.IsNullable(entityType.Name, propertyInfo.Name))
						{
							defaultValue = string.Empty;
						}
					}
					else if (propertyType.IsIntegerType())
					{
						defaultValue = 0;
					}
					else if (propertyType.IsNumericType())
					{
						defaultValue = 0.0f;
					}
					else if (propertyType.IsDateTime() && !isNullable)
					{
						defaultValue = DateTime.Now;
					}
					else if (propertyType == typeof(Geometry))
					{
						defaultValue = new WKTReader(new NtsGeometryServices(precisionModel: PrecisionModel.Floating.Value, srid: 4326)).Read("POINT(EMPTY POINT)");
					}
				}

				if (defaultValue != null)
				{
					SetMissingValue(instance, propertyInfo, defaultValue, isNullable);
				}
			}
		}

		static void SetMissingValue(object instance, PropertyInfo propertyInfo, object value, bool isNullable)
		{
			Argument.NotNull(propertyInfo, nameof(propertyInfo));

			if (isNullable)
			{
				var genericArgs = propertyInfo.PropertyType.GetGenericArguments();

				var targetValue = TypeExtension.ChangeType(value.ToString(), genericArgs.First());
				propertyInfo.SetValue(instance, targetValue, null);
			}
			else if (DateTime.TryParse(value.ToString(),out var dateTime))
			{
				propertyInfo.SetValue(instance, TypeExtension.ChangeType(dateTime.ToString(CultureInfo.InvariantCulture), propertyInfo.PropertyType), null);
			}
			else
			{
				propertyInfo.SetValue(instance, TypeExtension.ChangeType(value.ToString(), propertyInfo.PropertyType), null);
			}
		}
	}
}
