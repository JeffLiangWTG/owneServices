using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;

namespace Enterprise.Customs.PL.NCTS.Business;

public class LocationOfGoodsAddressProvider : IInternationalAddress
{
	public LocationOfGoodsAddressProvider(CusGoodsLocationAddress address)
	{
		this.address = Argument.NotNull(address, nameof(address));
	}
	readonly CusGoodsLocationAddress address;

	public string StreetAndNumber => address.E2_Address1AndE2_Address2;

	public string Postcode => address.E2_Postcode;

	public string City => address.E2_City;

	public string Country => address.E2_RN_NKCountryCode;

	public int StreetAndNumberMaxLength => 70;
}
