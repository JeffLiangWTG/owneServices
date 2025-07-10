using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Business.Testing
{
	[TestedType(typeof(OrgSalesActualsInformation))]
	sealed class OrgSalesActualsInformationTest : NonPersistentBusinessObjectTestCase
	{
		[TestDate(2015, 5, 5)]
		public void TestProperties()
		{
			var helper = new AccountingPeriodTestHelper(Factory);
			helper.PostPeriodsForEntireYear(2015);

			var usdExRate = Factory.NewWithValidTestData<RefExchangeRate>();
			usdExRate.RE_RX_NKExCurrency = "USD";
			usdExRate.RE_StartDate = new ZDateTime(2014, 1, 1);
			usdExRate.RE_ExpiryDate = new ZDateTime(2016, 1, 1);
			usdExRate.RE_SellRate = 0.5m;
			usdExRate.RE_ExRateType = Enterprise.Core.Constants.ExchangeRateTypes.Code.SellRate;

			var au = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
			var nsw = Factory.LoadTop1<RefCountryStates>(new ZQuery(
				new ZQuery(RefCountryStatesSchema.RW_RN_NKCountryCode, au.RN_Code),
				new ZQuery(RefCountryStatesSchema.RW_Code, "NSW")));

			var vic = Factory.LoadTop1<RefCountryStates>(new ZQuery(
				new ZQuery(RefCountryStatesSchema.RW_RN_NKCountryCode, au.RN_Code),
				new ZQuery(RefCountryStatesSchema.RW_Code, "VIC")));

			var us = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "US");
			var gb = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "GB");

			var product = Factory.LoadFromNaturalKey<OrgSalesProduct>(OrgSalesProductSchema.MP_Code, "SHP");

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var salesHeaderCollection = (SalesHeaderCollection)org.ActualAndProspectiveSalesHeaderCollection;
			var entitySalesCollection = salesHeaderCollection.EntitySalesCollection;
			var salesHeader = salesHeaderCollection.AddNew(product);

			var actualSalesAuToUs = CreateActualSales(salesHeader, au, us, org, new ZDate(2014, 1, 1), GlbCompany.CurrentCompany.PK, "AUD", 10m);

			var actualSalesNswToVic = CreateActualSales(salesHeader, nsw, vic, org, new ZDate(2015, 2, 1), GlbCompany.CurrentCompany.PK, "USD", 100m);
			var additionalValueNswToVic = actualSalesNswToVic.TradeDetails[0].TradedPeriods[0].TradeValues.AddNew();
			additionalValueNswToVic.PAV_GC = Factory.NewWithValidTestData<GlbCompany>().PK;
			additionalValueNswToVic.PAV_Revenue = 100m;
			additionalValueNswToVic.PAV_RX_NKCurrency = "USD";

			var actualSalesVicToNsw = CreateActualSales(salesHeader, vic, nsw, org, new ZDate(2015, 4, 1), GlbCompany.CurrentCompany.PK, "AUD", 1000m);

			var prospectSalesAuToUs = CreateProspectSales(salesHeader, au, us);

			Factory.Save();

			var salesActualsInformationPopulater = new OrgSalesActualsInformationBulkPopulater(Factory, org.PK);
			salesActualsInformationPopulater.Execute(entitySalesCollection.Cast<EntitySalesWrapper>(), false);

			var prospectSalesAuToUsActualsInfo = prospectSalesAuToUs.ActualsInformation;
			AssertEquals(OrgSalesActualsStatusList.Codes.Lost, prospectSalesAuToUsActualsInfo.Status);
			AssertEquals(new ZDateTime(2014, 1, 1), prospectSalesAuToUsActualsInfo.ActualsLastTraded);
			AssertEquals(">12 Mths.", prospectSalesAuToUsActualsInfo.TradedPeriodString);
			AssertEquals("No traded within last 12 months", 0m, prospectSalesAuToUsActualsInfo.ActualsAnnualTotal);
			AssertEquals("No traded within last 12 months", 0m, prospectSalesAuToUsActualsInfo.ActualsMonthlyAverage);

			prospectSalesAuToUs.OW_LatestProspectDate = new ZDate(2012, 1, 1);
			AssertEquals("Should remain lost since latest prospect date was before the last traded date", OrgSalesActualsStatusList.Codes.Lost, prospectSalesAuToUsActualsInfo.Status);

			prospectSalesAuToUs.OW_LatestProspectDate = new ZDate(2015, 1, 1);
			AssertEquals("Should become prospective again if latest prospect date is after last traded date", OrgSalesActualsStatusList.Codes.Prospective, prospectSalesAuToUsActualsInfo.Status);

			var prospectSalesAuToGb = CreateProspectSales(salesHeader, au, gb);

			Factory.Save();
			salesActualsInformationPopulater = new OrgSalesActualsInformationBulkPopulater(Factory, org.PK);
			salesActualsInformationPopulater.Execute(entitySalesCollection.Cast<EntitySalesWrapper>(), false);
			var prospectSalesAuToGbActualsInfo = prospectSalesAuToGb.ActualsInformation;
			AssertEquals(OrgSalesActualsStatusList.Codes.Prospective, prospectSalesAuToGbActualsInfo.Status);
			AssertEquals("Still prospective", ZDateTime.Empty, prospectSalesAuToGbActualsInfo.ActualsLastTraded);
			AssertEquals("Still prospective", "", prospectSalesAuToGbActualsInfo.TradedPeriodString);
			AssertEquals("Still prospective", 0m, prospectSalesAuToGbActualsInfo.ActualsAnnualTotal);
			AssertEquals("Still prospective", 0m, prospectSalesAuToGbActualsInfo.ActualsMonthlyAverage);

			var actualsSalesAuToGbForDifferentCompany = CreateActualSales(salesHeader, au, gb, org, new ZDate(2014, 1, 1), Factory.NewWithValidTestData<GlbCompany>().PK, "AUD", 11m);

			Factory.Save();
			salesActualsInformationPopulater = new OrgSalesActualsInformationBulkPopulater(Factory, org.PK);
			salesActualsInformationPopulater.Execute(entitySalesCollection.Cast<EntitySalesWrapper>(), false);
			AssertEquals(OrgSalesActualsStatusList.Codes.Lost, prospectSalesAuToGbActualsInfo.Status);
			AssertEquals(new ZDate(2014, 1, 1), prospectSalesAuToGbActualsInfo.ActualsLastTraded);
			AssertEquals(">12 Mths.", prospectSalesAuToGbActualsInfo.TradedPeriodString);
			AssertEquals("No traded within last 12 months", 0m, prospectSalesAuToGbActualsInfo.ActualsAnnualTotal);
			AssertEquals("No traded within last 12 months", 0m, prospectSalesAuToGbActualsInfo.ActualsMonthlyAverage);

			var prospectSalesAuToVic = CreateProspectSales(salesHeader, au, vic);

			Factory.Save();
			salesActualsInformationPopulater = new OrgSalesActualsInformationBulkPopulater(Factory, org.PK);
			salesActualsInformationPopulater.Execute(entitySalesCollection.Cast<EntitySalesWrapper>(), false);
			var prospectSalesAuToVicActualsInfo = prospectSalesAuToVic.ActualsInformation;
			AssertEquals(OrgSalesActualsStatusList.Codes.Traded, prospectSalesAuToVicActualsInfo.Status);
			AssertEquals(new ZDateTime(2015, 2, 1), prospectSalesAuToVicActualsInfo.ActualsLastTraded);
			AssertEquals("3 Mths.", prospectSalesAuToVicActualsInfo.TradedPeriodString);
			AssertEquals(200m, prospectSalesAuToVicActualsInfo.ActualsAnnualTotal);
			AssertEquals(200m / 3, prospectSalesAuToVicActualsInfo.ActualsMonthlyAverage);

			var prospectSalesNswToVic = CreateProspectSales(salesHeader, nsw, vic);

			Factory.Save();
			salesActualsInformationPopulater = new OrgSalesActualsInformationBulkPopulater(Factory, org.PK);
			salesActualsInformationPopulater.Execute(entitySalesCollection.Cast<EntitySalesWrapper>(), false);
			var prospectSalesNswToVicActualsInfo = prospectSalesNswToVic.ActualsInformation;
			AssertEquals(OrgSalesActualsStatusList.Codes.Traded, prospectSalesNswToVicActualsInfo.Status);
			AssertEquals(new ZDateTime(2015, 2, 1), prospectSalesNswToVicActualsInfo.ActualsLastTraded);
			AssertEquals("3 Mths.", prospectSalesNswToVicActualsInfo.TradedPeriodString);
			AssertEquals(200m, prospectSalesNswToVicActualsInfo.ActualsAnnualTotal);
			AssertEquals(200m / 3, prospectSalesNswToVicActualsInfo.ActualsMonthlyAverage);

			var prospectSalesAuToAu = CreateProspectSales(salesHeader, au, au);

			Factory.Save();
			salesActualsInformationPopulater = new OrgSalesActualsInformationBulkPopulater(Factory, org.PK);
			salesActualsInformationPopulater.Execute(entitySalesCollection.Cast<EntitySalesWrapper>(), false);
			var prospectSalesAuToAuActualsInfo = prospectSalesAuToAu.ActualsInformation;
			AssertEquals(OrgSalesActualsStatusList.Codes.Traded, prospectSalesAuToAuActualsInfo.Status);
			AssertEquals(new ZDateTime(2015, 4, 1), prospectSalesAuToAuActualsInfo.ActualsLastTraded);
			AssertEquals("3 Mths.", prospectSalesAuToAuActualsInfo.TradedPeriodString);
			AssertEquals(1200m, prospectSalesAuToAuActualsInfo.ActualsAnnualTotal);
			AssertEquals(1200m / 3, prospectSalesAuToAuActualsInfo.ActualsMonthlyAverage);

			var prospectSalesVicToNsw = CreateProspectSales(salesHeader, vic, nsw);

			Factory.Save();
			salesActualsInformationPopulater = new OrgSalesActualsInformationBulkPopulater(Factory, org.PK);
			salesActualsInformationPopulater.Execute(entitySalesCollection.Cast<EntitySalesWrapper>(), false);
			var prospectSalesVicToNswActualsInfo = prospectSalesVicToNsw.ActualsInformation;
			AssertEquals(OrgSalesActualsStatusList.Codes.Traded, prospectSalesVicToNswActualsInfo.Status);
			AssertEquals(new ZDateTime(2015, 4, 1), prospectSalesVicToNswActualsInfo.ActualsLastTraded);
			AssertEquals("1 Mths.", prospectSalesVicToNswActualsInfo.TradedPeriodString);
			AssertEquals(1000m, prospectSalesVicToNswActualsInfo.ActualsAnnualTotal);
			AssertEquals(1000m, prospectSalesVicToNswActualsInfo.ActualsMonthlyAverage);
		}

		static OrgSales CreateActualSales(SalesHeader salesHeader, BusinessObject origin, BusinessObject destination, OrgHeader org, ZDate period, ZGuid companyPk, ZString currency, ZDecimal revenue)
		{
			var sales = salesHeader.TradedSalesCollectionProductView.AddNew();
			sales.OW_IsTraded = true;
			sales.OW_OriginID = origin.PK;
			sales.OW_OriginTableCode = origin.TablePrefix;
			sales.OW_DestinationID = destination.PK;
			sales.OW_DestinationTableCode = destination.TablePrefix;

			var detail = sales.TradeDetails.AddNew();
			var tradePeriod = detail.TradedPeriods.AddNew();
			tradePeriod.PAS_Period = period;
			tradePeriod.PAS_LastTraded = period;
			tradePeriod.PAS_OH_Client = org.PK;

			var tradeValue = tradePeriod.TradeValues.AddNew();
			tradeValue.PAV_GC = companyPk;
			tradeValue.PAV_RX_NKCurrency = currency;
			tradeValue.PAV_Revenue = revenue;

			return sales;
		}

		static EntitySalesWrapper CreateProspectSales(SalesHeader salesHeader, BusinessObject origin, BusinessObject destination)
		{
			var sales = salesHeader.EntitySalesCollectionProductView.AddNew();
			sales.OW_IsTraded = false;
			sales.OW_OriginID = origin.PK;
			sales.OW_OriginTableCode = origin.TablePrefix;
			sales.OW_DestinationID = destination.PK;
			sales.OW_DestinationTableCode = destination.TablePrefix;

			return sales;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var sales = Factory.New<EntitySalesWrapper>();
			var org = Factory.New<OrgHeader>();
			return new OrgSalesActualsInformation(sales, org);
		}
	}
}
