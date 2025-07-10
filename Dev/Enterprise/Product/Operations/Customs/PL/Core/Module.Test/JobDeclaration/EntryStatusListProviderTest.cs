using CargoWise.EntityFramework;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Module.Testing;

class EntryStatusListProviderTest : Customs.Module.Testing.EntryStatusListProviderTest
{
	public override void TestEntryStatusLists()
	{
		var codePair = new ReadOnlyBusinessObjectFactory().GetCachedValue<PLEntryStatusList>();
		RunStatusCodeListForVariousCountriesTester(Core.Constants.CountryCodes.Poland, codePair.GetAllCodes(), new[] { "WTO", "XXX", "CEO" });
	}
}
