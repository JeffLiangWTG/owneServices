using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.NL.Module.Testing
{
	sealed class EntryStatusListProviderTest : Customs.Module.Testing.EntryStatusListProviderTest
	{
		public override void TestEntryStatusLists()
		{
			var countryCode = Core.Constants.CountryCodes.Netherlands;
			var cusCodeHelper = new UniversalReferenceTestDataHelper(Factory);

			cusCodeHelper.CreateNewOrGetExistingCusCodeType(RefCusCodeListTypes.Codes.CustomsStatus, "Entry Status List Type");
			cusCodeHelper.CreateNewOrGetExistingCusCodeList(countryCode, RefCusCodeListTypes.Codes.CustomsStatus, "X", "x", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			cusCodeHelper.CreateNewOrGetExistingCusCodeList(countryCode, RefCusCodeListTypes.Codes.CustomsStatus, "B3", "y", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			cusCodeHelper.CreateNewOrGetExistingCusCodeList(countryCode, RefCusCodeListTypes.Codes.CustomsStatus, "Z", "Z", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();

			var provider = new EntryStatusListProvider();
			var entryStatusList = (CodeDescriptionPairList)provider.EntryStatusList(Factory, Core.Constants.CountryCodes.Netherlands, ZString.Empty);
			AssertEquals("B3, X, Z", entryStatusList.CodesAsString);
		}
	}
}
