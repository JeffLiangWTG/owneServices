using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.Universal.Testing
{
	internal class CusRefApplicabilityViewLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTradeGroupList()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var helper = new UniversalReferenceTestDataHelper(Factory);
				var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "1P1");
				Factory.Save();
				var rateType = helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Eritrea, "ADD");
				var cusTariff = helper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06));
				Factory.Save();
				var rateCode = helper.LoadOrCreateNewCusRateCode(Factory, "RATE0", rateType.PK, isSystem: true);
				Factory.Save();
				var cusRate = helper.CreateRate(cusTariff, rateCode.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "DUMMYFORMULA", dataGrouping: Core.Constants.CountryCodes.Eritrea, isSystem: true);
				Factory.Save();
				var refBo = helper.CreateCusApplicability(cusRate, null, new ZDateTime(1900, 1, 1), new ZDateTime(2079, 6, 6), "ADD", "1");
				helper.CreateTradeGroup(Core.Constants.CountryCodes.Australia, "G001", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), isSystem: false);
				helper.CreateTradeGroup(Core.Constants.CountryCodes.Eritrea, "G002", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), isSystem: false);
				Factory.Save();
				var cusRefApplicabilityView = Factory.Load<CusRefApplicabilityView>(refBo.PK);
				var list = cusRefApplicabilityView.Lookups.TradeGroupList;
				CombineAssertions(() =>
				{
					AssertType<CusRefTradeGroupCollection>("Type", list);
					AssertEquals("Count", 1, list.Count);
					AssertEquals("Default Value", Core.Constants.CountryCodes.Eritrea, list.FilterBusinessObjectDefaults[CusRefTradeGroupCollection.FilterConstants.CountryCode + ":Property"].Value.ToString());
				}

				);
			}
		}
	}
}
