using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NL.Business;

public static class NLRefCusCodeListTypes
{
	public static CodeDescriptionPairList GetCustomsStatusList(BusinessObjectFactory factory)
	{
		return RefCusCodeListTypes.GetCachedList(factory, Core.Constants.CountryCodes.Netherlands, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, ZDateTime.Today);
	}
}
