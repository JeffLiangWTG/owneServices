using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.NO.Business;

sealed public class CUSRESEDIMessageLookups : EDIMessageLookups
{
	public CUSRESEDIMessageLookups(CUSRESEDIMessage parent) : base(parent)
	{
	}

	public ICodeDescriptionPairList ErrorCodes => Factory.GetCachedValue("664C116C-3A5E-2DA7-25E1-6A064C09071D", GetErrorCodes);

	ICodeDescriptionPairList GetErrorCodes()
	{
		return RefCusCodeListTypes.GetCachedList(Factory,
			Core.Constants.CountryCodes.Norway,
			Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ErrorCode,
			ZDateTime.Now);
	}
}
