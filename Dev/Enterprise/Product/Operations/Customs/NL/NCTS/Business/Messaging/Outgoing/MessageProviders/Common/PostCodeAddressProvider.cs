using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class PostCodeAddressProvider : IPostCodeAddress
{
	public PostCodeAddressProvider(EU.NCTS.Business.CusGoodsLocation location)
	{
		this.location = Argument.NotNull(location, nameof(location)) as CusGoodsLocation;
		address = location.Address;
	}
	readonly CusGoodsLocationAddress address;
	readonly CusGoodsLocation location;

	public string HouseNumber => location.CGL_AdditionalIdentifier;

	public string Postcode => address.E2_Postcode;

	public string Country => address.E2_RN_NKCountryCode;
}
