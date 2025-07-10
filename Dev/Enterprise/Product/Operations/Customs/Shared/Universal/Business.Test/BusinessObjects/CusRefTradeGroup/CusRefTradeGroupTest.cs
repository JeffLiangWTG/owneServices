using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(CusRefTradeGroup))]
	public class CusRefTradeGroupTest : EnterpriseBusinessObjectTestCase
	{
		public void TestOnTradeGroupCountriesSelected()
		{
			var tradeGroup = Factory.New<CusRefTradeGroup>();
			var tradeGroupCountry = tradeGroup.TradeGroupCountries.AddNew();
			tradeGroupCountry.CRA_RN_NKTradeGroupCountryCode = "C1";
			tradeGroupCountry.CRA_Description = "C1 Desc";

			var country1 = Factory.New<RefCountry>();
			country1.RN_Code = "C2";
			country1.RN_Desc = "C2 Desc";
			var country2 = Factory.New<RefCountry>();
			country2.RN_Code = "C3";
			country2.RN_Desc = "C3 Desc";

			var countries = new BusinessObject[]
			{
				country1, country2,
			};

			tradeGroup.OnTradeGroupCountriesSelected(countries);

			AssertContainsExactElementsInAnyOrder(new[] { "C1", "C2", "C3" }, tradeGroup.TradeGroupCountries.Select(x => x.CRA_RN_NKTradeGroupCountryCode));
			AssertContainsExactElementsInAnyOrder(new[] { "C1 Desc", "C2 Desc", "C3 Desc" }, tradeGroup.TradeGroupCountries.Select(x => x.CRA_Description));
		}

		public void TestDefaultValue()
		{
			var tradeGroup = Factory.New<CusRefTradeGroup>();
			AssertEquals(GlbCompany.CurrentCompany.Country.Code, tradeGroup.CR9_RN_NKCountryCode);
			AssertEquals(ZDate.Today, tradeGroup.CR9_StartDate);
			AssertEquals((ZDate)ZDateTime.MaxSmallDateTime, tradeGroup.CR9_EndDate);
		}

		public void TestDelete()
		{
			var tradeGroup = Factory.New<CusRefTradeGroup>();
			var tradeGroupCountry = tradeGroup.TradeGroupCountries.AddNew();
			tradeGroup.Delete();
			Assert(tradeGroup.IsDeleted);
			Assert(tradeGroupCountry.IsDeleted);
		}

		public void TestHumanReadableName()
		{
			var tradeGroup = Factory.New<CusRefTradeGroup>();
			tradeGroup.CR9_TradeGroup = "WTO";
			AssertEquals("Trade Group WTO", tradeGroup.HumanReadableName);
		}

		public void TestCountryIsReadOnly()
		{
			AssertHasCustomAttribute<ReadOnlyAttribute>(typeof(CusRefTradeGroup), nameof(CusRefTradeGroup.CR9_RN_NKCountryCode), true, readOnlyAtt => readOnlyAtt.IsReadOnly);
		}

		public void TestDefaultValues()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var tradeGroup = Factory.New<CusRefTradeGroup>();
				AssertEquals(Core.Constants.CountryCodes.Australia, tradeGroup.CR9_RN_NKCountryCode);
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var result = Factory.NewWithValidTestData<CusRefTradeGroup>();
			return result;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}
	}
}
