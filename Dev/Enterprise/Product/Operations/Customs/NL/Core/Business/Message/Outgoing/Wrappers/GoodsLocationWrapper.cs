using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.Customs.NL.Business.Common;

namespace Enterprise.Customs.NL.Business;

public class GoodsLocationWrapper : IGoodsLocation
{
	GoodsLocationWrapper(CusGoodsLocation cusGoodsLocation)
	{
		this.cusGoodsLocation = Argument.NotNull(cusGoodsLocation, nameof(CusGoodsLocation));
	}
	readonly CusGoodsLocation cusGoodsLocation;

	public static GoodsLocationWrapper New(CusGoodsLocation cusGoodsLocation) =>
		cusGoodsLocation == null ? null : new GoodsLocationWrapper(cusGoodsLocation);

	public string TypeCode => cusGoodsLocation.CGL_Type;

	public IAddressLocation Address => AddressLocationWrapper.New(cusGoodsLocation);

	public string IdentificationTypeCode => NLConstants.DMSMessageValues.GoodsLocationIdentificationTypeCode;
}
