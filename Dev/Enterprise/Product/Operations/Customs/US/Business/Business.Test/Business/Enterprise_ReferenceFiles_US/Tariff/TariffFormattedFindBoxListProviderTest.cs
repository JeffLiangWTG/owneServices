namespace Enterprise.Customs.US.Business.Testing
{
	sealed class TariffFormattedFindBoxListProviderTest : DeclarationTestHelper
	{
		public void TestCanLookupFormattedTariff()
		{
			USCTariffCollection uSCTariffCollection = new USCTariffCollection(Factory);
			TariffFormattedFindBoxListProvider provider = new TariffFormattedFindBoxListProvider(uSCTariffCollection);

			USCTariff tariff = (USCTariff)provider.GetBusinessObjectFromCode(Tariff2710119000.UE_FormattedTariff);
			AssertEquals(tariff.UE_FormattedTariff, Tariff2710119000.UE_FormattedTariff);

			tariff = (USCTariff)provider.GetBusinessObjectFromCode(Tariff2710119000.UE_Tariff);
			AssertEquals(tariff.UE_Tariff, Tariff2710119000.UE_Tariff);
		}
	}
}
