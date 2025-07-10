using System;
using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public sealed class EndorsementProvider : INCTSEndorsement
{
	readonly EnRouteIncident incident;

	public EndorsementProvider(EnRouteIncident incident)
	{
		this.incident = Argument.NotNull(incident, nameof(incident));
	}

	public DateTime Date => incident.BN_EndorsementDate.IsValid ? incident.BN_EndorsementDate.ToDateTime() : ZDateTime.Now.ToDateTime();

	public string Authority => incident.BN_EndorsementAuthority;

	public string Place => incident.BN_EndorsementPlace;

	public string Country => incident.BN_EndorsementCountryCode;
}
