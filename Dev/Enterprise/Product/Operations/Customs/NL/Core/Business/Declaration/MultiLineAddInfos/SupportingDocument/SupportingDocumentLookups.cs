using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NL.Business.Declaration;

public class SupportingDocumentLookups : EU.Business.Declaration.MultiLineAddInfos.SupportingDocumentLookups
{
	public SupportingDocumentLookups(SupportingDocument parent) : base(parent)
	{
	}

	public override CodeDescriptionPairList UnitOfQuantityList => CachedCustomsUQList;

	public override CodeDescriptionPairList UnitOfQuantity2List => CachedCustomsUQList;

	CodeDescriptionPairList CachedCustomsUQList => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Netherlands, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsUQ, ZDateTime.Today);
}
