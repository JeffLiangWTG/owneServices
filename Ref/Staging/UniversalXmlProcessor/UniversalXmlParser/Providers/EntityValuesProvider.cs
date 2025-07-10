using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.RefDbRepo.Common.Argument;
using CargoWise.RefDbRepo.Common.Utils;
using CargoWise.RefDbRepo.UniversalXmlParser.Interfaces;
using Microsoft.Data.SqlClient;

namespace CargoWise.RefDbRepo.UniversalXmlParser.Providers
{
	public class EntityValuesProvider : IEntityValuesProvider, IDisposable
	{
		const string SqlGetValues = @"
SELECT obj.name as TableName, col.name as ColumnName, object_definition(default_object_id) as DefaultValue, col.is_nullable as IsNullable
FROM sys.objects obj INNER JOIN sys.columns col
ON obj.object_id = col.object_id
where obj.type = 'U'
";
		readonly IDbConnection connection;
		readonly Lazy<List<Tuple<string, string, object, bool>>> lazyGetValues;

		public EntityValuesProvider(string connectionString)
		{
			connection = new SqlConnection(connectionString);
			lazyGetValues = new Lazy<List<Tuple<string, string, object, bool>>>(QueryDatabaseForValues);
		}

		public void Dispose()
		{
			connection?.Dispose();
		}

		public object GetDefaultValue(string entityName, string propertyName)
		{
			Argument.NotNullOrEmpty(entityName, nameof(entityName));
			Argument.NotNullOrEmpty(propertyName, nameof(propertyName));

			var values = lazyGetValues.Value;
			var valueTuple = values.FirstOrDefault(x => (x.Item1 == entityName) && (x.Item2 == propertyName));

			return valueTuple?.Item3;
		}

		public bool IsNullable(string entityName, string propertyName)
		{
			Argument.NotNullOrEmpty(entityName, nameof(entityName));
			Argument.NotNullOrEmpty(propertyName, nameof(propertyName));

			var values = lazyGetValues.Value;
			var valueTuple = values.FirstOrDefault(x => (x.Item1 == entityName) && (x.Item2 == propertyName));

			return valueTuple != null && valueTuple.Item4;
		}

		List<Tuple<string, string, object, bool>> QueryDatabaseForValues()
		{
			Argument.NotNull(connection, nameof(connection));

			var values = new List<Tuple<string, string, object, bool>>();
			var valueDefinitions = new List<Tuple<string, string, string, string>>();
			using (var dataReader = connection.ExecuteReader(SqlGetValues))
			{
				if (dataReader != null)
				{
					while (dataReader.Read())
					{
						if (dataReader["TableName"] != null &&
							dataReader["ColumnName"] != null &&
							dataReader["DefaultValue"] != null &&
							dataReader["IsNullable"] != null)
						{
							var tableName = dataReader["TableName"].ToString();
							var columnName = dataReader["ColumnName"].ToString();
							var defaultValue = dataReader["DefaultValue"].ToString();
							var isNullable = dataReader["IsNullable"].ToString();

							valueDefinitions.Add(Tuple.Create(tableName, columnName, defaultValue, isNullable));
						}
					}
				}
			}

			foreach (var value in valueDefinitions)
			{
				if (value == null)
				{
					continue;
				}

				if (!string.IsNullOrEmpty(value.Item3))
				{
					using (var dataReader = connection.ExecuteReader($"SELECT {value.Item3}"))
					{
						if (dataReader != null)
						{
							if (!dataReader.Read())
							{
								continue;
							}

							var defaultValue = dataReader[0];
							values.Add(Tuple.Create(value.Item1, value.Item2, defaultValue, Convert.ToBoolean(value.Item4, CultureInfo.InvariantCulture)));
						}
					}
				}
				else
				{
					values.Add(Tuple.Create(value.Item1, value.Item2, default(object), Convert.ToBoolean(value.Item4, CultureInfo.InvariantCulture)));
				}

			}

			return values;
		}
	}
}
