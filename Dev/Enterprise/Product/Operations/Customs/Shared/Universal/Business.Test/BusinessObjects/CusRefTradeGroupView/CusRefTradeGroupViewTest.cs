using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(CusRefTradeGroupView))]
	class CusRefTradeGroupViewTest : EnterpriseBusinessObjectTestCase
	{
		public void TestHumanReadableName()
		{
			var tradeGroup = Factory.NewWithValidTestData<CusRefTradeGroupView>();
			AssertEquals("Customs Trade Group - Description", tradeGroup.HumanReadableName);
		}

		public void TestZZA_DataSet()
		{
			var tradeGroup = Factory.New<CusRefTradeGroupView>();
			AssertEquals("O", tradeGroup.ZZA_DataSet);
			Assert(tradeGroup.ZZA_DataSetInfo.ReadOnly);
		}

		[TestDate(2019, 2, 18)]
		public void TestGetApplicableTradeGroupCountries()
		{
			var tradeGroup = RefDataHelper.CreateTradeGroup(Core.Constants.CountryCodes.SouthAfrica, "SADC", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			var tradeGroupCountry1 = RefDataHelper.AddCountry(tradeGroup, Core.Constants.CountryCodes.SouthAfrica, new ZDate(2019, 2, 10), new ZDate(2019, 2, 17));
			var tradeGroupCountry2 = RefDataHelper.AddCountry(tradeGroup, Core.Constants.CountryCodes.SouthAfrica, new ZDate(2019, 2, 12), new ZDate(2019, 2, 25));
			var tradeGroupCountry3 = RefDataHelper.AddCountry(tradeGroup, Core.Constants.CountryCodes.SouthAfrica, new ZDate(2019, 2, 26), new ZDate(2019, 2, 28));
			AssertContainsExactElementsInAnyOrder(new[] { tradeGroupCountry2 }, tradeGroup.GetApplicableTradeGroupCountries(ZDateTime.Now));
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetBusinessObjectForFetchForLoad();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			RefDataHelper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.SouthAfrica);
			var tradeGroup = RefDataHelper.CreateTradeGroup(Core.Constants.CountryCodes.SouthAfrica, "SADC", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			var tradeGroupCountry = RefDataHelper.AddCountry(tradeGroup, Core.Constants.CountryCodes.SouthAfrica);
			return tradeGroupCountry;
		}

		//ZZRef RefCusTradeGroup is not supported to delete by CusRefTradeGroupView, so suspend the delete testing in TestSaveAndDeleteBusinessObject().
		protected override bool CanPersistedObjectBeDeleted => false;
		UniversalReferenceTestDataHelper RefDataHelper => refDataHelper ?? (refDataHelper = new UniversalReferenceTestDataHelper(Factory));
		UniversalReferenceTestDataHelper refDataHelper;
	}
}
