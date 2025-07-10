using CargoWise.EntityFramework;

namespace Enterprise.Customs.SG.V4.Module.Testing
{
	sealed class EntryStatusListProviderTest : Customs.Module.Testing.EntryStatusListProviderTest
	{
		public override void TestEntryStatusLists()
		{
			var codePair = (new ReadOnlyBusinessObjectFactory()).GetCachedValue<Common.SG.CustomsEntryStatusList>();
			RunStatusCodeListForVariousCountriesTester(Core.Constants.CountryCodes.Singapore, codePair.GetAllCodes(), new string[] { "SUB", "RT1", "CEO" });
		}
	}
}
