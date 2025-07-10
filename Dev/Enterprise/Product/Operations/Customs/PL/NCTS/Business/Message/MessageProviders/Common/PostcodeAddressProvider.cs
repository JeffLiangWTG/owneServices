using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Types;

namespace Enterprise.Customs.PL.NCTS.Business;

public class PostcodeAddressProvider : IPostcodeAddress
{
	public PostcodeAddressProvider(CusGoodsLocationAddress address, ZString houseNumber)
	{
		this.address = Argument.NotNull(address, nameof(address));
		HouseNumber = houseNumber;
	}
	readonly CusGoodsLocationAddress address;

	public string HouseNumber { get; }

	public string Postcode => address.E2_Postcode;

	public string Country => address.E2_RN_NKCountryCode;
}
