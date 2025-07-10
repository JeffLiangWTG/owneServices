using System;
using System.Collections.Generic;
using Enterprise.Freight.Integration.OnlineSailingSchedulesWebApi;

namespace Enterprise.Freight.Integration
{
	public interface IOnlineSailingSchedulesWebApi
	{
		IImportGSSResponse ImportGSSFromNaturalKey(IImportGSSPayload gssImport);
		IImportManyGSSResponse ImportManyGSS(IImportManyGSSPayload importManyPayload);
	}

	public interface IImportGSSPayload
	{
		IGSSNaturalKey GSSNaturalKey { get; }
	}

	public interface IGSSNaturalKey
	{
		string CarrierSCAC { get; }
		IEnumerable<IConnectionNaturalKey> Legs { get; }
	}

	public interface IConnectionNaturalKey
	{
		string Origin { get; }
		string Destination { get; }
		string VoyageCode { get; }
		string VesselName { get; }
		string CarrierSCAC { get; }
		DateTime Departure { get; }
		DateTime Arrival { get; }
		string LegType { get; }
	}

	public interface IImportGSSResponse
	{
		ImportGSSResponseCode Code { get; }
		string Message { get; }
		string ErrorMessage { get; }
		IEnumerable<string> ErrorNotifications { get; }
		IEnumerable<ISailingModel> Sailings { get; }
	}

	public interface ISailingModel
	{
		Guid PK { get; }
		string SailingID { get; }
		string VesselName { get; }
		bool IsNew { get; }
		bool? MatchesConnectionQuery { get; }
		DateTime Arrival { get; }
		DateTime Departure { get; }
	}

	public enum ImportGSSResponseCode
	{
		Success,
		LoadRoutesValidation,
		OnlineSchedulesValidation,
		ParameterError,
	}
}
