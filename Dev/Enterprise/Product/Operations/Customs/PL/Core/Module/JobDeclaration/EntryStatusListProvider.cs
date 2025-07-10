using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Module;

internal class EntryStatusListProvider : Customs.Module.EntryStatusListProvider
{
	public EntryStatusListProvider()
	{
	}

	protected override ICodeDescriptionPairList EntryStatusListCore(BusinessObjectFactory factory) => factory.GetCachedValue<PLEntryStatusList>();
}
