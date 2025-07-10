using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Universal.Testing
{
	internal class TariffRelationshipViewLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTariffTypeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.Duty, "Duty");
			helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.AntiDumping, "AntiDumping");
			var tariffType1P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "1P1");
			Factory.Save();
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06));
			var relatedTariff = helper.CreateTariffRelationship(tariff.PK, tariff.ZZ1_ZZI_TariffType, tariff.ZZ1_TariffCode);
			var list1 = relatedTariff.Lookups.TariffTypeList;
			AssertEquals(list1, RefCusTariffTypeList.GetCachedList(Factory, Core.Constants.CountryCodes.SouthAfrica));
		}
	}
}
