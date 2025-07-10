using CargoWise.EntityFramework;
using Enterprise.Customs.TW.Business;

namespace Enterprise.Customs.TW.Module.Testing
{
	public sealed class EntryStatusListProviderTest : Customs.Module.Testing.EntryStatusListProviderTest
	{
		public override void TestEntryStatusLists()
		{
			var codePair = (new ReadOnlyBusinessObjectFactory()).GetCachedValue<EntryStatusCodeList>();
			RunStatusCodeListForVariousCountriesTester(Core.Constants.CountryCodes.Taiwan, codePair.GetAllCodes(), new string[] { "WTO", "XXX", "CEO" });
		}
	}
}
