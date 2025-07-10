using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public sealed class LocationForIncidentsProvider : INCTSLocation
{
	readonly EnRouteIncident incident;
	readonly CusGoodsLocation location;

	public LocationForIncidentsProvider(EnRouteIncident incident)
	{
		this.incident = Argument.NotNull(incident, nameof(incident));
		location = incident.GoodsLocation as CusGoodsLocation;
	}

	public string QualifierOfIdentification => location.CGL_Qualifier;

	public string GNSSLatitute => location.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.GnssCoordinates ? location.Address.E2_Latitude.ToString() : null;

	public string GNSSLongitude => location.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.GnssCoordinates ? location.Address.E2_Longitude.ToString() : null;

	public INCTSAddress Address => location.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.Address ? address ??= new AddressProvider(location.Address) : null;
	INCTSAddress address;

	public string UnLocode => location.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.UnLocode ? location.Unlocode : null;

	public string Country => incident.BN_EventCountryCode;

	public string Location => null;
}
