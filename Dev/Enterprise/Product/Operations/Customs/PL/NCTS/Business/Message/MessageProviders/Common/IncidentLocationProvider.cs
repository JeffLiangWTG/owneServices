using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.ZArchitecture.Schema;
using CusGoodsLocationQualifierList = Enterprise.Customs.Business.CusGoodsLocationQualifierList;

namespace Enterprise.Customs.PL.NCTS.Business;

public class IncidentLocationProvider : ILocation
{
	readonly EnRouteIncident incident;
	readonly CusGoodsLocation location;

	public IncidentLocationProvider(EnRouteIncident incident)
	{
		this.incident = Argument.NotNull(incident, nameof(incident));
		location = (CusGoodsLocation)Argument.NotNull(incident.GoodsLocation, $"{nameof(incident)}.{nameof(incident.GoodsLocation)}");
	}

	public string QualifierOfIdentification => location.CGL_Qualifier;

	public string UNLocode => CachedValueHelper.GetValue(ref unlocode, ()
		=> IsCusInBondEvent && QualifierOfIdentification == CusGoodsLocationQualifierList.Codes.UnLocode ? (string)location.Unlocode : null);
	CachedValue<string> unlocode;

	public string Country => incident.BN_EventCountryCode;

	public IGNSS GNSS => CachedValueHelper.GetValue(ref gnss, ()
		=> IsCusInBondEvent && QualifierOfIdentification == CusGoodsLocationQualifierList.Codes.GnssCoordinates ? new GNSSProvider(location.Address) : null);
	CachedValue<IGNSS> gnss;

	public ILocalAddress Address => CachedValueHelper.GetValue(ref address, ()
		=> IsCusInBondEvent && QualifierOfIdentification == CusGoodsLocationQualifierList.Codes.Address ? new LocationOfGoodsAddressProvider(location.Address) : null);
	CachedValue<ILocalAddress> address;

	bool IsCusInBondEvent => location.CGL_ParentTableCode == CusInBondEventSchema.Constants.Prefix && location.CGL_ParentID == incident.PK;
}
