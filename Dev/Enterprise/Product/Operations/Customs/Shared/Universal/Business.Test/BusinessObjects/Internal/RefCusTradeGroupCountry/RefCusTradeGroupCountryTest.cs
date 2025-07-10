using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusTradeGroupCountry))]
	class RefCusTradeGroupCountryTest : EnterpriseBusinessObjectTestCase
	{
		RefCusTradeGroup CusTradeGroup
		{
			get
			{
				return cusTradeGroup ?? (cusTradeGroup = Factory.NewWithValidTestData<RefCusTradeGroup>());
			}
		}

		RefCusTradeGroup cusTradeGroup;
		protected override BusinessObject GetNewBusinessObject()
		{
			return CusTradeGroup.TradeGroupCountries.AddNew();
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var tradeGroup = new UniversalReferenceTestDataHelper(factory).CreateTradeGroup(Core.Constants.CountryCodes.SouthAfrica, "SADC", ZDateTime.BrettsBirthday, ZDateTime.MaxSmallDateTime);
			var tradeGroupCountry = factory.New<RefCusTradeGroupCountry>();
			tradeGroupCountry.ZZB_ZZA_TradeGroup = tradeGroup.PK;
			tradeGroupCountry.ZZB_RN_NKTradeGroupCountryCode = Core.Constants.CountryCodes.SouthAfrica;
			tradeGroupCountry.ZZB_StartDate = ZDateTime.BrettsBirthday.Date;
			tradeGroupCountry.ZZB_EndDate = ZDateTime.MaxSmallDateTime.Date;
			return tradeGroupCountry;
		}
	}
}
