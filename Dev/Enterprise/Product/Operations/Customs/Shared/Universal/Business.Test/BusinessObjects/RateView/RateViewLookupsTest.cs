using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Universal.Testing
{
	internal class RateViewLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestPreferenceCodeList()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var rateView = CreateRateView();
				var list = rateView.Lookups.PreferenceCodeList;
				CombineAssertions(() =>
				{
					AssertType<CusRefPreferenceCollection>("Type", list);
					AssertEquals("Count", 1, list.Count);
					AssertEquals("Default Value", Core.Constants.CountryCodes.Eritrea, list.FilterBusinessObjectDefaults[CusRefPreferenceCollection.FilterConstants.CountryCode + ":Property"].Value.ToString());
				}

				);
			}
		}

		public void TestRateCodeList()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var rateView = CreateRateView();
				var list = rateView.Lookups.RateCodeList;
				CombineAssertions(() =>
				{
					AssertType<CusRefRateCodeCollection>("Type", list);
					AssertEquals("Count", 1, list.Count);
					AssertEquals("Default Value", Core.Constants.CountryCodes.Eritrea, list.FilterBusinessObjectDefaults[CusRefRateCodeCollection.FilterConstants.CountryCode + ":Property"].Value.ToString());
				}

				);
			}
		}

		RateView CreateRateView()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Eritrea);
			var tariffType = helper.CreateTariffType(Core.Constants.CountryCodes.Eritrea, "1P1", ensureDataGroupingExists: false);
			helper.CreatePreferenceView("P001", "P001 DESC", Core.Constants.CountryCodes.Australia, isSystem: false);
			helper.CreatePreferenceView("P002", "P002 DESC", Core.Constants.CountryCodes.Eritrea, isSystem: false);
			CreateCusRefRateCodeView("R001", Core.Constants.CountryCodes.Australia);
			CreateCusRefRateCodeView("R002", Core.Constants.CountryCodes.Eritrea);
			Factory.Save();
			var tariffView = helper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffType.PK, "DUMMYTRF", new ZDateTime(2021, 01, 01), new ZDateTime(2022, 01, 01), "dummy Description 0");
			var rateView = tariffView.FilteredRates.AddNew();
			return rateView;
		}

		CusRefRateCodeView CreateCusRefRateCodeView(ZString code, ZString dataGrouping)
		{
			var rateCode = Factory.New<CusRefRateCodeView>();
			rateCode.ZY1_RateCode = code;
			rateCode.ZY1_Description = code + " DESC";
			rateCode.ZY1_RateType = "OTH";
			rateCode.ZY1_ZZZ_NKDataGrouping = dataGrouping;
			return rateCode;
		}
	}
}
