using System;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.SafeDataClient;
using CargoWise.RefDbRepo.Common.SafeDataClient.CargoWise.RefDbRepo.Service.Schema_0_9_New;
using CargoWise.RefDbRepo.Common.TypeProvider;
using CargoWise.RefDbRepo.DataProcessingExplanation;
using Microsoft.OData.Edm;

namespace CargoWise.RefDbRepo.Staging.UniversalXMLStagingDataProcessor;

public static class ObjectExtensions
{
	public static Guid GetPKValue(this object obj)
	{
		Argument.NotNull(obj, nameof(obj));
		var pkProperty = obj.GetType().GetPKPropertyInfo();
		var result = pkProperty.GetValue(obj);
		return (Guid)result;
	}

	public static object GetValue(this object obj, string propertyName)
	{
		Argument.NotNull(obj, nameof(obj));
		Argument.NotNullOrEmpty(propertyName, nameof(propertyName));

		return obj.GetEntityType().GetProperty(propertyName)?.GetValue(obj);
	}

	public static object[] GetValues(this object obj, string[] propertyNames)
	{
		Argument.NotNull(obj, nameof(obj));
		Argument.NotNull(propertyNames, nameof(propertyNames));
		return propertyNames.Select(x => obj.GetValue(x)).ToArray();
	}

	public static Type GetEntityType(this object obj)
	{
		Argument.NotNull(obj, nameof(obj));

		var result = obj.GetType();
		return result;
	}

	public static object ConvertToSafeValue(this object value, Type safeValueType)
	{
		if (value != null && (value.GetType() == typeof(DateTime) || value.GetType() == typeof(DateTime?)) &&
			(safeValueType == typeof(DateTimeOffset) || safeValueType == typeof(DateTimeOffset?)))
		{
			var datetime = (DateTime?)value;
			return datetime.Value.ToUTCDateTimeOffset();
		}
		if (value != null && (value.GetType() == typeof(DateTime) || value.GetType() == typeof(DateTime?)) &&
			(safeValueType == typeof(Date) || safeValueType == typeof(Date?)))
		{
			var datetime = ((DateTime?)value).Value;
			return new Date(datetime.Year, datetime.Month, datetime.Day);
		}
		if (value != null && safeValueType == typeof(SerializedGeometry))
		{
			return new SerializedGeometry { Geography = new GeographyWellKnownValue { CoordinateSystemId = 4326, WellKnownText = value.ToString() } };
		}
		return value;
	}

	public static T ConvertToSafeValue<T>(this object value)
	{
		var result = ConvertToSafeValue(value, typeof(T));
		if (result != null)
		{
			return (T)result;
		}
		if (default(T) == null)
		{
			return default;
		}
		throw new RefDataProcessingException($"Unable to convert value <{value}> to type <{typeof(T)}>.", ErrorCodes.UnableToConvertToSafeValue, [typeof(T)]);
	}
}
