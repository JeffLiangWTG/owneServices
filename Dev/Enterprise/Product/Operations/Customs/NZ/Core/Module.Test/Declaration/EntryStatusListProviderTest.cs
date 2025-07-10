using CargoWise.EntityFramework;
using Enterprise.Customs.NZ.Business;

namespace Enterprise.Customs.NZ.Module.Testing
{
	sealed class EntryStatusListProviderTest : Customs.Module.Testing.EntryStatusListProviderTest
	{
		public override void TestEntryStatusLists()
		{
			var codePair = (new ReadOnlyBusinessObjectFactory()).GetCachedValue<AmalgamatedStatusList>();
			RunStatusCodeListForVariousCountriesTester(Core.Constants.CountryCodes.NewZealand, codePair.GetAllCodes(), new string[] { "WTO", "XXX", "CEO" });
			var codePair2 = (new ReadOnlyBusinessObjectFactory()).GetCachedValue<LowValueConsignmentStatusList>();
			RunStatusCodeListForVariousCountriesTester(Core.Constants.CountryCodes.NewZealand, codePair2.GetAllCodes(), new string[] { "WTO", "XXX", "CEO" });
		}
	}
}
