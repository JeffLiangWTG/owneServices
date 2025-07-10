using Enterprise.Customs.NL.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NL.Module;

public class EntryHeaderFilterLookups : EU.Module.EntryHeaderFilterLookups
{
	public EntryHeaderFilterLookups(EntryHeaderFilterBusinessObject filterBizObj)
	: base(filterBizObj)
	{
	}

	protected override CodeDescriptionPairList GetMainMessageStatusList() => Factory.GetCachedValue<CustomsEntryMessageStatusList>();
}
