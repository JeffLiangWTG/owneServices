using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.NL.Business;

public class ReleaseDeclarationGoodsLocationWrapper : NonPersistentBusinessObject, IDeclarationGoodsLocation
{
	readonly CusGoodsLocation goodsLocation;

	public ReleaseDeclarationGoodsLocationWrapper(CusGoodsLocation goodsLocation) : base((goodsLocation as BusinessObject)?.Factory ?? new BusinessObjectFactory())
	{
		this.goodsLocation = Argument.NotNull(goodsLocation, nameof(goodsLocation));
	}

	public ZString TypeCode => goodsLocation.CGL_Qualifier;

	public IAddressDocumentInformation Address => new AddressInformationDocumentWrapper(goodsLocation);

	public ZString IdentificationTypeCode => goodsLocation.CGL_Type;
}
