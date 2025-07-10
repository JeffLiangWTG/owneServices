using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

[assembly: InternalsVisibleTo("CargoWise.RefDbRepo.Service.SchemaManagement.Test")]
namespace CargoWise.RefDbRepo.Common.Utils
{
	static class EntityExtension
	{
		public static DataTable ToDataTable<T>(this IEnumerable<T> entities)
		{
			var table = new DataTable(typeof(T).Name);
			var byteWriterGeography = new SqlServerBytesWriter() { IsGeography = true };
			var flattenProperties = typeof(T).GetProperties().Where(
				x => !(x.PropertyType.IsGenericType && typeof(ICollection<>).IsAssignableFrom(x.PropertyType.GetGenericTypeDefinition()))
				&& !x.GetMethod.IsVirtual
				&& !x.CustomAttributes.Any(a => a.AttributeType == typeof(NotMappedAttribute)));

			foreach (var prop in flattenProperties)
			{
				if (prop.PropertyType == typeof(Geometry))
				{
					table.Columns.Add(prop.Name, typeof(object));
				}
				else
				{
					table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
				}
			}

			foreach (T item in entities)
			{
				DataRow row = table.NewRow();
				foreach (var prop in flattenProperties)
				{
					if (prop.PropertyType == typeof(Geometry))
					{
						var propValue = prop.GetValue(item, null);
						if (propValue != null)
						{
							row[prop.Name] = byteWriterGeography.Write((Geometry)propValue);
						}
						else
						{
							row[prop.Name] = DBNull.Value;
						}
					}
					else
					{
						row[prop.Name] = prop.GetValue(item, null) ?? DBNull.Value;
					}
				}
				table.Rows.Add(row);
			}

			return table;
		}
	}
}
