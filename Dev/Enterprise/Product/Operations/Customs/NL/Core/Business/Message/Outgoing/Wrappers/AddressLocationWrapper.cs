using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;

namespace Enterprise.Customs.NL.Business;

public class AddressLocationWrapper : IAddressLocation
{
	AddressLocationWrapper(CusGoodsLocation cusGoodsLocation)
	{
		this.cusGoodsLocation = Argument.NotNull(cusGoodsLocation, nameof(CusGoodsLocation));
	}
	readonly CusGoodsLocation cusGoodsLocation;

	public static AddressLocationWrapper New(CusGoodsLocation cusGoodsLocation) =>
		cusGoodsLocation == null ? null : new AddressLocationWrapper(cusGoodsLocation);

	public string StreetNumberID => cusGoodsLocation.CGL_AdditionalIdentifier;
	public string CityName => null;
	public string CountryCode => cusGoodsLocation.Address.IsNull ? null : cusGoodsLocation.Address.E2_RN_NKCountryCode;
	public string Line => null;
	public string PostcodeId => cusGoodsLocation.Address.IsNull ? null : cusGoodsLocation.Address.E2_Postcode;
}
