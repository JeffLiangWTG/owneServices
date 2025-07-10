using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Universal.Testing
{
	internal class TariffViewLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTariffTypeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.Duty, "Duty");
			helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "1P1");
			var tariffType1P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.EastTimor, "1P1");
			Factory.Save();
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.EastTimor, tariffType1P1.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06));
			var list1 = tariff.Lookups.TariffTypeList;
			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06));
			var list2 = tariff2.Lookups.TariffTypeList;
			AssertNotEquals(list1, list2);
			AssertEquals(RefCusTariffTypeList.GetCachedList(Factory, Core.Constants.CountryCodes.SouthAfrica).Count, list2.Count);
			var tariff3 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType1P1.PK, "DUMMYTRF2", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06));
			var list3 = tariff3.Lookups.TariffTypeList;
			AssertSame(list2, list3);
			var tariff4 = Factory.New<TariffView>();
			tariff4.ZZ1_IsSystem = false;
			var list4 = tariff4.Lookups.TariffTypeList;
			AssertSame(tariff.Lookups.ManualTariffTypeList, list4);
			AssertEquals(1, tariff.Lookups.ManualTariffTypeList.Count);
			AssertEquals(true, tariff.Lookups.ManualTariffTypeList.ContainsCode(Constants.TariffTypes.HarmonizedSystem));
		}

		public void TestTaxOrFeeCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Eritrea, Constants.RateTypes.Duty, "Duty");
			helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "1P1");
			var tariffType1P1 = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "1P1");
			helper.CreateTaxOrFee("ER1", 0.1, Core.Constants.CountryCodes.Eritrea, 0, 10, Constants.RefCusTaxOrFeeTypes.VAT, new ZDateTime(2011, 2, 1), new ZDateTime(2011, 3, 1), "ERDESC1");
			helper.CreateTaxOrFee("ER1", 0.2, Core.Constants.CountryCodes.Eritrea, 0, 10, Constants.RefCusTaxOrFeeTypes.VAT, new ZDateTime(2011, 3, 2), new ZDateTime(2012, 12, 31), "ERDESC1");
			helper.CreateTaxOrFee("ER2", 0.3, Core.Constants.CountryCodes.Eritrea, 0, 10, Constants.RefCusTaxOrFeeTypes.VAT, new ZDateTime(2010, 12, 1), new ZDateTime(2011, 3, 1), "ERDESC2");
			helper.CreateTaxOrFee("ER2", 0.4, Core.Constants.CountryCodes.Eritrea, 0, 10, Constants.RefCusTaxOrFeeTypes.VAT, new ZDateTime(2011, 3, 2), new ZDateTime(2011, 12, 1), "ERDESC2");
			helper.CreateTaxOrFee("ER3", 0.5, Core.Constants.CountryCodes.Eritrea, 0, 10, Constants.RefCusTaxOrFeeTypes.VAT, new ZDateTime(2010, 12, 10), new ZDateTime(2012, 2, 1), "ERDESC3");
			Factory.Save();
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffType1P1.PK, "DUMMYTRF", new ZDateTime(2011, 1, 1), new ZDateTime(2011, 12, 31));
			CombineAssertions(() =>
			{
				tariff.ZZ1_IsSystem = false;
				var list = tariff.Lookups.TaxOrFeeCodeList;
				AssertEquals("Isn't system", "ER2, ER3", list.CodesAsString);
				AssertEquals("Cached", list, tariff.Lookups.TaxOrFeeCodeList);
				tariff.ZZ1_IsSystem = true;
				AssertEquals("Is system", ZString.Empty, tariff.Lookups.TaxOrFeeCodeList.CodesAsString);
				tariff.ZZ1_IsSystem = false;
				tariff.ZZ1_StartDate = new ZDateTime(2010, 12, 5);
				AssertEquals("Tariff's ZZ1_StartDate < TaxOrFeeER3's ZZF_StartDate", "ER2", tariff.Lookups.TaxOrFeeCodeList.CodesAsString);
				tariff.ZZ1_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.Albania;
				AssertEquals("Other dataGrouping", ZString.Empty, tariff.Lookups.TaxOrFeeCodeList.CodesAsString);
			}

			);
		}

		public void TestTariffVersionList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.EastTimor, "TP");
			Factory.Save();
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.EastTimor, tariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06));
			AssertType<CusRefTariffVersionCollection>("Type", tariff.Lookups.TariffVersionList);
		}

		[SelfManagedTariffCountries(Enterprise.Core.Constants.CountryCodes.Botswana, Enterprise.Core.Constants.CountryCodes.CoteDivoire)]
		public void TestCountryCodeList_IsNotSystem()
		{
			var tariff = Factory.New<TariffView>();
			var countryCodeList = tariff.Lookups.CountryCodeList;
			CombineAssertions(() =>
			{
				AssertType<RefCountryCollection>("Type", countryCodeList);
				var testList = countryCodeList.Select(c => c.Code.ToString());
				var expectedList = new List<string> { Enterprise.Core.Constants.CountryCodes.Botswana, Enterprise.Core.Constants.CountryCodes.CoteDivoire };
				AssertSequencesEqual("List", expectedList, testList);
				AssertSame("Cached", countryCodeList, tariff.Lookups.CountryCodeList);
			}

			);
		}

		public void TestCountryCodeList_IsSystem()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.EastTimor, "TP");
			Factory.Save();
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.EastTimor, tariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06));
			AssertType<RefCountryCollection>(tariff.Lookups.CountryCodeList);
		}
	}
}
