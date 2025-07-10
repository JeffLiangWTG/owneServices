using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.Customs.NZ.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NZ.Module
{
	public class EntryStatusListProvider : Customs.Module.EntryStatusListProvider
	{
		public EntryStatusListProvider()
		{
		}

		protected override ICodeDescriptionPairList EntryStatusListCore(BusinessObjectFactory factory)
		{
			var result = new CodeDescriptionPairList();
			result.AddRange(factory.GetCachedValue<AmalgamatedStatusList>());
			var amalgamatedStatusList = factory.GetCachedValue<LowValueConsignmentStatusList>();
			foreach (CodeDescriptionPair pair in amalgamatedStatusList)
			{
				result.AddPairIfNotExist(pair.Code, pair.Description);
			}
			return result;
		}
	}
}
