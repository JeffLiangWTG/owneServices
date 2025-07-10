using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusTradeGroup))]
	sealed class RefCusTradeGroupTest : EnterpriseBusinessObjectTestCase
	{
		[TestDate(2019, 2, 18)]
		public void TestGetApplicableTradeGroupCountries()
		{
			var tradeGroup = RefDataHelper.CreateTradeGroup(Core.Constants.CountryCodes.SouthAfrica, "SADC", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			var tradeGroupCountry1 = tradeGroup.TradeGroupCountries.AddNew();
			tradeGroupCountry1.ZZB_RN_NKTradeGroupCountryCode = Core.Constants.CountryCodes.SouthAfrica;
			tradeGroupCountry1.ZZB_StartDate = new ZDate(2019, 2, 10);
			tradeGroupCountry1.ZZB_EndDate = new ZDate(2019, 2, 17);
			var tradeGroupCountry2 = tradeGroup.TradeGroupCountries.AddNew();
			tradeGroupCountry2.ZZB_RN_NKTradeGroupCountryCode = Core.Constants.CountryCodes.SouthAfrica;
			tradeGroupCountry2.ZZB_StartDate = new ZDate(2019, 2, 10);
			tradeGroupCountry2.ZZB_EndDate = new ZDate(2019, 2, 25);
			var tradeGroupCountry3 = tradeGroup.TradeGroupCountries.AddNew();
			tradeGroupCountry3.ZZB_RN_NKTradeGroupCountryCode = Core.Constants.CountryCodes.SouthAfrica;
			tradeGroupCountry3.ZZB_StartDate = new ZDate(2019, 2, 26);
			tradeGroupCountry3.ZZB_EndDate = new ZDate(2079, 2, 25);
			AssertContainsExactElementsInAnyOrder(new[] { tradeGroupCountry2 }, tradeGroup.GetApplicableTradeGroupCountries(ZDateTime.Now));
		}

		public void TestHumanReadableName()
		{
			var tradeGroup = Factory.NewWithValidTestData<RefCusTradeGroup>();
			tradeGroup.ZZA_Description = "Description";
			AssertEquals("Customs Trade Group - Description", tradeGroup.HumanReadableName);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			new UniversalReferenceTestDataHelper(factory).CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.SouthAfrica);
			var bo = (RefCusTradeGroup)base.GetNewBusinessObjectForDeleteTest(factory);
			bo.ZZA_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.SouthAfrica;
			return bo;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			RefDataHelper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.SouthAfrica);
			var bo = (RefCusTradeGroup)base.GetBusinessObjectForFetchForLoad();
			bo.ZZA_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.SouthAfrica;
			return bo;
		}

		UniversalReferenceTestDataHelper RefDataHelper => refDataHelper ?? (refDataHelper = new UniversalReferenceTestDataHelper(Factory));
		UniversalReferenceTestDataHelper refDataHelper;
	}
}
