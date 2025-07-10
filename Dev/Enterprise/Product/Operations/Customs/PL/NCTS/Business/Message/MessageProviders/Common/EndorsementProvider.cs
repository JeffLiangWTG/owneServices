using System;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.PL.NCTS.Business;

public class EndorsementProvider : IEndorsement
{
	public static EndorsementProvider NewOrNull(EnRouteIncident incident)
	{
		if (incident == null)
		{
			return null;
		}

		ZDateTime date;
		ZString authority;
		ZString place;
		ZString country;

		return !(date = incident.BN_EndorsementDate).IsEmpty
				&& !(authority = incident.BN_EndorsementAuthority).IsEmpty
				&& !(place = incident.BN_EndorsementPlace).IsEmpty
				&& !(country = incident.BN_EndorsementCountryCode).IsEmpty
			? new EndorsementProvider(date.ToDateTime(), authority, place, country)
			: null;
	}

	EndorsementProvider(DateTime date, string authority, string place, string country)
	{
		Date = date;
		Authority = authority;
		Place = place;
		Country = country;
	}

	public DateTime Date { get; }

	public string Authority { get; }

	public string Place { get; }

	public string Country { get; }
}
