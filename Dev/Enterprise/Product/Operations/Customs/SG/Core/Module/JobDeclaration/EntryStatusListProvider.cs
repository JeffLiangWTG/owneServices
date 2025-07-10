using CargoWise.EntityFramework;
using CargoWise.Integration;

namespace Enterprise.Customs.SG.V4.Module
{
	public class EntryStatusListProvider : Customs.Module.EntryStatusListProvider
	{
		public EntryStatusListProvider()
		{
		}

		protected override ICodeDescriptionPairList EntryStatusListCore(BusinessObjectFactory factory) => factory.GetCachedValue<Common.SG.CustomsEntryStatusList>();
	}
}
