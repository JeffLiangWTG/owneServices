using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces;

namespace Enterprise.Customs.PL.NCTS.Business;

public class GNSSProvider(CusGoodsLocationAddress address) : IGNSS
{
	readonly CusGoodsLocationAddress address = Argument.NotNull(address, nameof(address));

	public string Latitude => address.E2_Latitude.ToString();

	public string Longitude => address.E2_Longitude.ToString();
}
