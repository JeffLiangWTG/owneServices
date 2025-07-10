using System;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.EUReferenceData.OfficeCodes.Business
{
	public interface IOfficeCode
	{
		string ReferenceNumber { get; }
		string CountryCode { get; }
		string UsualName { get; }
		DateTime StartDate { get; }
		bool StartDateSuccessfullyParsed { get; }
		DateTime EndDate { get; }
		bool EndDateSuccessfullyParsed { get; }
		string City { get; }
		string StreetAndNumber { get; }
		string PostalCode { get; }
		string EMailAddress { get; }
		IEnumerable<IOfficeCodeRole> Roles { get; }
	}

	public interface IOfficeCodeRole
	{
		string Name { get; }
		IEnumerable<string> TransportModes { get; }
	}
}
