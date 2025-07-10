using System.Data;
using System.Text;
using CargoWise.Data.Utils;
using Enterprise.DbUpgrader.Resource.Version;

namespace CargoWise.Data.SqlProxyServer.Core.Db;

public static class DbUtils
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1116:UseEnterpriseCoreDataWrapperClasses", Justification = "SQLOverHttp")]
	public static (VersionLabel SchemaVersion, VersionLabel ScriptVersion, VersionLabel TransformationVersion) GetSchemaVersions(SqlConnection sqlConnection)
	{
		var keyValueDictionary = ReadBulkRegistry();
		var schemaVersion = GetVersionLabel(keyValueDictionary, DatabaseMajorSchemaVersionName, DatabaseMinorSchemaVersionName);
		var scriptVersion = GetVersionLabel(keyValueDictionary, DatabaseMajorScriptVersionName, DatabaseMinorScriptVersionName);
		var transformationVersion = GetVersionLabel(keyValueDictionary, DatabaseMajorTransformationVersionName, DatabaseMinorTransformationVersionName);
		return (schemaVersion, scriptVersion, transformationVersion);

		static VersionLabel GetVersionLabel(Dictionary<string, int> dictionary, string majorName, string minorName)
			=> new VersionLabel(SafeGet(dictionary, majorName), SafeGet(dictionary, minorName));

		static int SafeGet(Dictionary<string, int> dictionary, string key) => dictionary.TryGetValue(key, out var value) ? value : 0;

		Dictionary<string, int> ReadBulkRegistry()
		{
			var names = new[]
			{
				DatabaseMajorSchemaVersionName,
				DatabaseMinorSchemaVersionName,
				DatabaseMajorScriptVersionName,
				DatabaseMinorScriptVersionName,
				DatabaseMajorTransformationVersionName,
				DatabaseMinorTransformationVersionName,
				LockTimeoutName
			};

			var sqlQuery = @"
	SELECT SD_Name, SD_BinaryValue
	FROM dbo.StmData
	WHERE SD_Name in (" + string.Join(", ", names.Select((_, i) => $"@CW{i}")) + @")
		AND SD_Owner is null
		AND SD_DepartmentGuid is null
		AND SD_BinaryValue is not null;";

			var dictionary = new Dictionary<string, int>();
			using var command = sqlConnection.CreateCommand();
			command.CommandTimeout = 0;
			command.CommandType = CommandType.Text;
			command.CommandText = sqlQuery;
			
			for (var i = 0; i < names.Length; i++)
			{
				var sqlParameter = new SqlParameter($"@CW{i}", SqlDbType.VarChar)
				{
					Direction = ParameterDirection.Input,
					Value = names[i]
				};

				command.Parameters.Add(sqlParameter);
			}

			using var reader = command.ExecuteReader();
			while (reader.Read())
			{
				var key = reader.GetString(0);
				var value = Encoding.Unicode.GetString(reader.ReadAllBytes(1));
				if (key == DbRegistry.SuspendAuditTriggersName)
				{
					dictionary[key] = string.CompareOrdinal(bool.TrueString, value) == 0 ? 1 : 0;
				}
				else
				{
					dictionary[key] = Convert.ToInt32(value);
				}
			}

			return dictionary;
		}
	}

	internal const string DatabaseMajorSchemaVersionName = "DATABASE_SCHEMA_VERSION";
	internal const string DatabaseMinorSchemaVersionName = "DATABASE_MINOR_SCHEMA_VERSION";
	internal const string DatabaseMajorScriptVersionName = "DatabaseMajorCoreScriptVersion";
	internal const string DatabaseMinorScriptVersionName = "DatabaseMinorCoreScriptVersion";
	internal const string DatabaseMajorTransformationVersionName = "DatabaseMajorTransformationVersion";
	internal const string DatabaseMinorTransformationVersionName = "DatabaseMinorTransformationVersion";
	internal const string LockTimeoutName = "LockTimeout";
}
