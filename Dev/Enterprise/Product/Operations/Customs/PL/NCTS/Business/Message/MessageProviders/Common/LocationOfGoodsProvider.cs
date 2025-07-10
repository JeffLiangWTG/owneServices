using CargoWise.Common;
using CargoWise.Customs.PL.MessageContracts.Interfaces;
using CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using ILocationOfGoods = CargoWise.Customs.PL.MessageContracts.Interfaces.NCTS.ILocationOfGoods;

namespace Enterprise.Customs.PL.NCTS.Business;

public class LocationOfGoodsProvider : ILocationOfGoods
{
	public LocationOfGoodsProvider(CusGoodsLocation cusGoodsLocation)
	{
		this.cusGoodsLocation = Argument.NotNull(cusGoodsLocation, nameof(cusGoodsLocation));
	}
	readonly CusGoodsLocation cusGoodsLocation;

	public string TypeOfLocation => cusGoodsLocation.CGL_Type;

	public string QualifierOfIdentification => cusGoodsLocation.CGL_Qualifier;

	public string AuthorisationNumber => CachedValueHelper.GetValue(ref authorisationNumber, () => cusGoodsLocation.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.AuthorizationNumber
		? cusGoodsLocation.Address.AuthorisationNumber
		: null);
	CachedValue<string> authorisationNumber;

	public string AdditionalIdentifier => CachedValueHelper.GetValue(ref additionalIdentifier, () => cusGoodsLocation.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.EoriNumber
																									|| cusGoodsLocation.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.AuthorizationNumber
		? cusGoodsLocation.CGL_AdditionalIdentifier
		: null);
	CachedValue<string> additionalIdentifier;

	public string UNLocode => CachedValueHelper.GetValue(ref unlocode, () => cusGoodsLocation.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.UnLocode
		? cusGoodsLocation.CGL_AdditionalIdentifier
		: null);
	CachedValue<string> unlocode;

	public string CustomsOffice => CachedValueHelper.GetValue(ref customsOffice, () => cusGoodsLocation.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier
		? cusGoodsLocation.CGL_CustomsOffice
		: null);
	CachedValue<string> customsOffice;

	public IGNSS GNSS => CachedValueHelper.GetValue(ref gnss, () => cusGoodsLocation.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.GnssCoordinates
		? new GNSSProvider(cusGoodsLocation.Address)
		: null);
	CachedValue<IGNSS> gnss;

	public string EconomicOperator => CachedValueHelper.GetValue(ref economicOperator, () => cusGoodsLocation.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.EoriNumber
																							&& cusGoodsLocation.Address != null
		? cusGoodsLocation.Address.E2_GovRegNum
		: null);
	CachedValue<string> economicOperator;

	public IInternationalAddress Address => CachedValueHelper.GetValue(ref address, () => cusGoodsLocation.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.Address
		? new LocationOfGoodsAddressProvider(cusGoodsLocation.Address)
		: null);
	CachedValue<IInternationalAddress> address;

	public IPostcodeAddress PostcodeAddress => CachedValueHelper.GetValue(ref postcodeAddress, () => cusGoodsLocation.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.PostcodeAddress
		? new PostcodeAddressProvider(cusGoodsLocation.Address, cusGoodsLocation.CGL_AdditionalIdentifier)
		: null);
	CachedValue<IPostcodeAddress> postcodeAddress;

	public IContactPerson ContactPerson => CachedValueHelper.GetValue(ref contactPerson, () => CanContainsContactPerson(cusGoodsLocation)
		? LocationOfGoodsContactPersonProvider.NewOrNull(cusGoodsLocation.Address)
		: null);
	CachedValue<IContactPerson> contactPerson;

	internal static bool CanContainsContactPerson(CusGoodsLocation goodsLocation) => goodsLocation.CGL_Qualifier != CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier;
}
