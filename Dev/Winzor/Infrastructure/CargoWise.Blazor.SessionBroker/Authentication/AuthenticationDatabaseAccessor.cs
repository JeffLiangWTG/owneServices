using System;
using System.Data;
using CargoWise.Blazor.SessionBroker.Helpers;
using Microsoft.Extensions.Logging;

namespace CargoWise.Blazor.SessionBroker.Authentication;

public interface IAuthenticationDatabaseAccessor
{
	bool TryPeekAccessToken(string clientToken);
}

public class AuthenticationDatabaseAccessor : IAuthenticationDatabaseAccessor
{
	readonly IDatabaseAccessor databaseAccessor;
	readonly ILogger logger;
	public AuthenticationDatabaseAccessor(IDatabaseAccessor databaseAccessor, ILogger logger)
	{
		this.databaseAccessor = databaseAccessor;
		this.logger = logger;
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Literal string is safe to use in this context.")]
	public bool TryPeekAccessToken(string token)
	{
		logger.LogInformation("SQL Run StoredProcedure: TryPeekAccessToken");
		return databaseAccessor.ExecuteDbCommand(command =>
		{
			command.CommandText = "TryPeekAccessToken";
			command.CommandType = CommandType.StoredProcedure;

			command.Parameters.AddWithValue("@Token", token);
			command.Parameters.AddWithValue("@Type", "BLC");
			command.Parameters.Add("@Scope", SqlDbType.VarChar, int.MaxValue).Direction = ParameterDirection.Output;
			command.Parameters.Add("@ParentId", SqlDbType.UniqueIdentifier).Direction = ParameterDirection.Output;
			command.Parameters.Add("@ParentTableCode", SqlDbType.VarChar, 3).Direction = ParameterDirection.Output;
			var resultParam = command.Parameters.Add("@TPATResult", SqlDbType.Bit);
			resultParam.Direction = ParameterDirection.Output;

			command.ExecuteNonQuery();
			return (bool)resultParam.Value;
		});
	}
}
