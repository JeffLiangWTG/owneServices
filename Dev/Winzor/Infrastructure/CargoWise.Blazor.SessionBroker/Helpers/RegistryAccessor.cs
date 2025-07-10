using System;
using System.Data;
using System.Text;
using CargoWise.Blazor.SessionBroker.Authentication;
using Enterprise.ZArchitecture.Environment;

namespace CargoWise.Blazor.SessionBroker.Helpers;

public interface IRegistryAccessor
{
	byte[] GetBinaryValue(string registryName);
	bool? GetBoolValue(string registryName);
	string GetStringValue(string registryName);
	string GetDBSchemaVersion();
}
public class RegistryAccessor : IRegistryAccessor
{
	public const string DBSchemaVersion = "DATABASE_SCHEMA_VERSION";
	const string ProcedureNameGetValueNOD = "DataRegGetValueNOD";

	readonly IDatabaseAccessor databaseAccessor;

	public RegistryAccessor(IDatabaseAccessor databaseAccessor)
	{
		this.databaseAccessor = databaseAccessor;
	}

	public string GetDBSchemaVersion()
	{
		var data = GetBinaryValue(DBSchemaVersion);
		return data is not null ? Encoding.Unicode.GetString(data) : string.Empty;
	}

	public byte[] GetBinaryValue(string registryName)
	{
		var data = databaseAccessor.ExecuteScalar<object>(
			commandText: ProcedureNameGetValueNOD,
			commandType: CommandType.StoredProcedure,
			parameters: ("@Name", registryName));

		return data != DBNull.Value ? data as byte[] : null;
	}

	public bool? GetBoolValue(string registryName)
	{
		var bytes = GetBinaryValue(registryName);
		if (bytes == null)
		{
			return null;
		}

		return new BooleanRegistryDataType().Deserialise(bytes);
	}

	public string GetStringValue(string registryName)
	{
		var bytes = GetBinaryValue(registryName);
		if (bytes == null)
		{
			return null;
		}
		return new StringRegistryDataType().Deserialise(bytes);
	}
}
