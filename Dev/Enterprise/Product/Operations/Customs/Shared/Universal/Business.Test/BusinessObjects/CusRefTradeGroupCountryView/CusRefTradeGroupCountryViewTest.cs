using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(CusRefTradeGroupCountryView))]
	class CusRefTradeGroupCountryViewTest : EnterpriseBusinessObjectTestCase
	{
		public void TestEmptyZDateTimeInLoader()
		{
			var loader = new CusRefTradeGroupView.Loader(Factory);
			AssertEquals(0, loader.Load("ASD", ZDateTime.Empty, "AU").Length);
		}

		public void TestZZB_DataSet()
		{
			var tradeGroupCountry = Factory.New<CusRefTradeGroupCountryView>();
			AssertEquals("O", tradeGroupCountry.ZZB_DataSet);
			Assert(tradeGroupCountry.ZZB_DataSetInfo.ReadOnly);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<CusRefTradeGroupView>().TradeGroupCountries.AddNew();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var tradeGroup = helper.CreateTradeGroup(Core.Constants.CountryCodes.SouthAfrica, "SADC", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			var tradeGroupCountry = helper.AddCountry(tradeGroup, Core.Constants.CountryCodes.SouthAfrica, ZDateTime.BrettsBirthday.Date);
			return tradeGroupCountry;
		}

		//ZZRef RefCusTradeGroupCountry is not supported to delete by CusRefTradeGroupCountryView, so suspend the delete testing in TestSaveAndDeleteBusinessObject().
		protected override bool CanPersistedObjectBeDeleted => false;
	}
}
