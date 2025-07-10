using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.EntityFramework;
using Enterprise.Customs.PL.Business.Declaration;
using CusGoodsLocation = Enterprise.Customs.EU.Business.CusGoodsLocation;

namespace Enterprise.Customs.PL.Business;

public class AESLocationOfGoodsProvider : ILocationOfGoods
{
	readonly CusGoodsLocation goodsLocation;

	public AESLocationOfGoodsProvider(CusGoodsLocation cusGoodsLocation)
	{
		goodsLocation = Argument.NotNull(cusGoodsLocation, nameof(cusGoodsLocation));
	}

	public string TypeOfLocation => goodsLocation.CGL_Type;

	public string QualifierOfIdentification => goodsLocation.CGL_Qualifier;

	public string AuthorisationNumber => CachedValueHelper.GetValue(ref authorisationNumber,
		() => QualifierOfIdentification == QualifierOfTheIdentificationList.Codes.Y ? goodsLocation.Address.AuthorisationNumber.ToString() : null);
	CachedValue<string> authorisationNumber;

	public string AdditionalIdentifier => CachedValueHelper.GetValue(ref additionalIdentifier, () =>
		(QualifierOfIdentification == QualifierOfTheIdentificationList.Codes.Y
		|| QualifierOfIdentification == QualifierOfTheIdentificationList.Codes.X)
		&& !goodsLocation.AdditionalIdentifier.IsEmpty
			? goodsLocation.AdditionalIdentifier.ToString()
			: null);
	CachedValue<string> additionalIdentifier;

	public string UNLocode => CachedValueHelper.GetValue(ref uNLocode,
		() => QualifierOfIdentification == QualifierOfTheIdentificationList.Codes.U ? goodsLocation.Unlocode.ToString() : null);
	CachedValue<string> uNLocode;

	public ICustomsOffice CustomsOffice => CachedValueHelper.GetValue(ref customsOffice,
		() => QualifierOfIdentification == QualifierOfTheIdentificationList.Codes.V
			? new AESCustomsOfficeProvider(goodsLocation.CGL_CustomsOffice)
			: null);
	CachedValue<ICustomsOffice> customsOffice;

	public IGNSS GNSS => CachedValueHelper.GetValue(ref gNSS,
		() => QualifierOfIdentification == QualifierOfTheIdentificationList.Codes.W
			? new AESGNSSProvider(goodsLocation.Address.E2_GeoLocation)
			: null);
	CachedValue<IGNSS> gNSS;

	public IEconomicOperator EconomicOperator => CachedValueHelper.GetValue(ref economicOperator,
		() => QualifierOfIdentification == QualifierOfTheIdentificationList.Codes.X
			? new AESEconomicOperatorProvider(goodsLocation.Address)
			: null);
	CachedValue<IEconomicOperator> economicOperator;

	public IAddress Address => CachedValueHelper.GetValue(ref address,
		() => QualifierOfIdentification == QualifierOfTheIdentificationList.Codes.Z
			? new AddressProvider(goodsLocation.Address)
			: null);
	CachedValue<IAddress> address;

	public IContactPerson ContactPerson => CachedValueHelper.GetValue(ref contactPerson,
		() => QualifierOfIdentification != QualifierOfTheIdentificationList.Codes.V
			? ContactPersonJobDocAddressProvider.NewOrNull(goodsLocation.Address)
			: null);
	CachedValue<IContactPerson> contactPerson;

	public IPostcodeAddress PostcodeAddress => null;
}
