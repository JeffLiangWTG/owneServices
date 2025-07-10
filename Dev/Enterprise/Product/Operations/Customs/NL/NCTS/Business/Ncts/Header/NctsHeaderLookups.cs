using CargoWise.Common;
using Enterprise.Core;
using Enterprise.Customs.NL.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NL.NCTS.Business;

public class NctsHeaderLookups : EU.NCTS.Business.NctsHeaderLookups
{
	public NctsHeaderLookups(NctsHeader parent) : base(parent)
	{
	}

	public override CodeDescriptionPairList CommunicationLanguageList => Factory.GetCachedValue("NL.NctsHeaderLookups.CommunicationLanguageList", () =>
	{
		var result = new CodeDescriptionPairList();
		var languageList = LanguageHelper.GetDefaultLanguageForOLookUpEditType();
		result.AddPair(SharedConstants.Languages.English, languageList.GetValueSafe(SharedConstants.Languages.English));
		result.AddPair(Constants.CountryCodes.Netherlands, languageList.GetValueSafe(SharedConstants.Languages.Dutch));
		return result;
	});

	public CodeDescriptionPairList CalculationMethodList => Factory.GetCachedValue<CalculationMethodList>();
}

