using System;
using System.Collections.Generic;

namespace Enterprise.Freight.Integration.OnlineSailingSchedulesWebApi
{
	/// <summary>
	/// Import all matching connections from the GSS api
	/// </summary>
	public interface IImportManyGSSPayload
	{
		IEnumerable<IConnectionQuery> ConnectionQueries { get; }
	}

	public interface IConnectionQuery
	{
		DateTime StartDate { get; }
		DateTime ExpiryDate { get; }
		string LoadPort { get; }
		string DischargePort { get; }
		bool AllowRelatedUNLOCOs { get; }
		IGSSTradeLane TradeLane { get; }
		bool DirectRoutesOnly { get; }
	}

	public interface IGSSTradeLane
	{
		string Code { get; }
		string Name { get; }
	}

	public interface IImportManyGSSResponse
	{
		bool Success { get; }
		List<List<List<ISailingModel>>> Sailings { get; }
		IImportManyGSSError Error { get; }
	}

	public interface IImportManyGSSError
	{
		ImportGSSResponseCode Code { get; }
		string ErrorMessage { get; }
		IConnectionQuery ConnectionQuery { get; }
		IEnumerable<string> ErrorNotifications { get; }
	}
}
