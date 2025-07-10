using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	sealed class ClassificationTypeProviderTest : TestCaseWithFactory
	{
		public void TestGetProviderFor()
		{
			AssertGetProviderFor(Core.Constants.CountryCodes.Australia, ClassificationTypeList.Codes.HTI, ClassificationTypeList.Codes.HTE, "");
			AssertGetProviderFor(Core.Constants.CountryCodes.Canada, ClassificationTypeList.Codes.HTI, ClassificationTypeList.Codes.HTE, "", true, "SHB");
			AssertGetProviderFor(Core.Constants.CountryCodes.Chile, ClassificationTypeList.Codes.HTI, ClassificationTypeList.Codes.HTE, ClassificationTypeList.Codes.HTB);
			AssertGetProviderFor(Core.Constants.CountryCodes.Eritrea, ClassificationTypeList.Codes.HTI, ClassificationTypeList.Codes.HTE, ClassificationTypeList.Codes.HTB);
			AssertGetProviderFor(Core.Constants.CountryCodes.UnitedKingdom, "IMP", "EXP", "BTH");
			AssertGetProviderFor(Core.Constants.CountryCodes.Germany, "IMP", "EXP", "BTH");
			AssertGetProviderFor(Core.Constants.CountryCodes.SouthAfrica, ClassificationTypeList.Codes.HTI, ClassificationTypeList.Codes.HTE, ClassificationTypeList.Codes.HTB);
			AssertGetProviderFor(Core.Constants.CountryCodes.NewZealand, ClassificationTypeList.Codes.HTI, ClassificationTypeList.Codes.HTE, ClassificationTypeList.Codes.HTB);
			AssertGetProviderFor(Core.Constants.CountryCodes.UnitedStates, ClassificationTypeList.Codes.HTI, ClassificationTypeList.Codes.HTE, "", true, "SHB");

			AssertNoExceptionThrown("Should not throw exception for providing null country...", () =>
			{
				var provider = ClassificationTypeProvider.GetProviderFor(null);
				AssertType<ClassificationTypeProvider>(provider);
			});
		}

		void AssertGetProviderFor(string country, string hti, string hte, string htb, bool supportSHB = false, string shb = "")
		{
			CombineAssertions(country, () =>
			{
				var provider = ClassificationTypeProvider.GetProviderFor(country);
				AssertEquals("HTICode", hti, provider.HTICode);
				AssertEquals("HTECode", hte, provider.HTECode);
				AssertEquals("HTBCode", htb, provider.HTBCode);
				if (supportSHB)
				{
					AssertEquals("SHBCode", shb, provider.SHBCode);
				}
			});
		}
	}
}
