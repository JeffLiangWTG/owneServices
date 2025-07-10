using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Customs.TW.Business;

namespace Enterprise.Customs.TW.Module
{
	public class EntryStatusListProvider : Customs.Module.EntryStatusListProvider
	{
		public EntryStatusListProvider()
		{
		}

		protected override ICodeDescriptionPairList EntryStatusListCore(BusinessObjectFactory factory) => factory.GetCachedValue<EntryStatusCodeList>();
	}
}
