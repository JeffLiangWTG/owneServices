using CargoWise.Common;
using CargoWise.Customs.NL.MessageContracts.Interfaces;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class LocationOfGoodsProvider : ILocationOfGoods
{
	readonly NctsHeader header;
	readonly EU.NCTS.Business.CusGoodsLocation cusGoodsLocation;

	public LocationOfGoodsProvider(NctsHeader nctsHeader)
	{
		header = Argument.NotNull(nctsHeader, nameof(nctsHeader));
		cusGoodsLocation = header.IsArrivalMovement ? header.ArrivalMovementHeader.GoodsLocation : header.MovementHeader.GoodsLocation;
	}

	public string TypeOfLocation => cusGoodsLocation.CGL_Type;

	public string AuthorisationNumber => null;

	public string AdditionalIdentifier => null;

	public string CustomsOfficeReferenceNumber => QualifierOfIdentification == CusGoodsLocationQualifierList.Codes.CustomsOfficeIdentifier ? cusGoodsLocation.CGL_AdditionalIdentifier : null;

	public string EconomicOperatorIdentificationNumber => null;

	public IContact ContactPerson => QualifierOfIdentification == CusGoodsLocationQualifierList.Codes.UnLocode ? (contactPerson ??= ContactProvider.NewOrNull(cusGoodsLocation.Address)) : null;
	IContact contactPerson;

	public IPostCodeAddress PostCodeAddress => QualifierOfIdentification == CusGoodsLocationQualifierList.Codes.PostcodeAddress && cusGoodsLocation != null ? (postCodeAddress ??= new PostCodeAddressProvider(cusGoodsLocation)) : null;
	IPostCodeAddress postCodeAddress;

	public string QualifierOfIdentification => cusGoodsLocation.CGL_Qualifier;

	public string UnLocode => QualifierOfIdentification == CusGoodsLocationQualifierList.Codes.UnLocode ? cusGoodsLocation.CGL_AdditionalIdentifier : null;

	public string GNSSLatitute => null;

	public string GNSSLongitude => null;

	public INCTSAddress Address => null;
}
