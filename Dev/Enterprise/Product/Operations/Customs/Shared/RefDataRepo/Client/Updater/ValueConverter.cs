using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.RefDbRepo.Client.Common;
using Microsoft.SqlServer.Types;

namespace CargoWise.RefDataRepo.Ent.Client
{
	public class ValueConverter : IValueConverter
	{
		List<string> GeographyColumns
		{
			get
			{
				return geographyColumns ?? (geographyColumns = new List<string>()
				{
					nameof(IRefUNLOCO.RL_GeoLocation),
					nameof(IRefFacility.RFT_GeoLocation)
				});
			}
		}

		List<string> geographyColumns;

		public object GetValue(Type type, object value)
		{
			if (type == typeof(SqlGeography) && !string.IsNullOrEmpty(value?.ToString()))
			{
				return SqlGeography.Parse(value.ToString());
			}

			return value;
		}

		public Type GetPropertyType(PropertyInfo property)
		{
			var result = property.PropertyType;
			if (result.IsGenericType && result.GetGenericTypeDefinition() == typeof(Nullable<>))
			{
				result = result.GetGenericArguments().FirstOrDefault();
			}
			if (GeographyColumns.Contains(property.Name))
			{
				result = typeof(SqlGeography);
			}
			return result;
		}
	}
}
