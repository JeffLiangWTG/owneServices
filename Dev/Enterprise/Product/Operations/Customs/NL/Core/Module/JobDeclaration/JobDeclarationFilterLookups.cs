using CargoWise.Types;
using Enterprise.Customs.NL.Business;
using Enterprise.Customs.Universal;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NL.Module;

public class JobDeclarationFilterLookups : Customs.Module.JobDeclarationFilterLookups
{
	public JobDeclarationFilterLookups(JobDeclarationFilterBusinessObject filterBizObj)
	: base(filterBizObj)
	{
	}

	public override CodeDescriptionPairList EntryStatusList() => RefCusCodeListTypes.GetCachedList(Factory, Core.Constants.CountryCodes.Netherlands, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsStatus, ZDateTime.Now);

	public CodeDescriptionPairList EntryPhaseStatusList() => Factory.GetCachedValue<CustomsEntryPhaseStatusList>();

	public override CodeDescriptionPairList MessageStatusList() => Factory.GetCachedValue<CustomsEntryMessageStatusList>();
}
