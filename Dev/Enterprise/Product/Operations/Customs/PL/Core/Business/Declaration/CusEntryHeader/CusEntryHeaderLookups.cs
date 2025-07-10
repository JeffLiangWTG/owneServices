using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.PL.Business.Declaration;

public class CusEntryHeaderLookups : EU.Business.Declaration.CusEntryHeaderLookups
{
	public CusEntryHeaderLookups(EU.Business.Declaration.CusEntryHeader parent) : base(parent)
	{
	}

	protected new CusEntryHeader Parent => (CusEntryHeader)base.Parent;

	public override CodeDescriptionPairList CH_EntryStatusList => Factory.GetCachedValue<PLEntryStatusList>();
}
