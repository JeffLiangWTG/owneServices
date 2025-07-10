using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(USCTariffFilterStripBusinessObject))]
	sealed class USCTariffFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestFilters()
		{
			var filter = new USCTariffFilterStripBusinessObject();
			AssertNotNull(filter[USCTariff.FilterSchema.Date]);
			AssertNotNull(filter[USCTariff.FilterSchema.Description]);
			AssertNotNull(filter[USCTariff.FilterSchema.SPI]);
			AssertNotNull(filter[USCTariff.FilterSchema.PermitLicenseCode]);
			AssertNotNull(filter[USCTariff.FilterSchema.Tariff]);
		}

		public void TestGetTariffQuery()
		{
			var tariff1 = Factory.New<USCTariff>();
			tariff1.UE_Tariff = TariffForTest1;
			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_Tariff = TariffForTest2;
			var tariff3 = Factory.New<USCTariff>();
			tariff3.UE_Tariff = TariffForTest3;
			var filterBizObj = new USCTariffFilterStripBusinessObject();
			var tariffNumberFilter = (ModuleTextFilter)filterBizObj[USCTariff.FilterSchema.Tariff];
			tariffNumberFilter.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			tariffNumberFilter.IsActive = true;
			AssertEquals(12, tariffNumberFilter.MaxLength);
			tariffNumberFilter.Property = "010";
			Assert("tariff1 should match filter", tariff1.MatchesFilter(filterBizObj.Filter));
			Assert("tariff2 should match filter", tariff2.MatchesFilter(filterBizObj.Filter));
			Assert("tariff3 should match filter", tariff3.MatchesFilter(filterBizObj.Filter));
			tariffNumberFilter.Property = "0101.0";
			Assert("tariff1 should match filter", tariff1.MatchesFilter(filterBizObj.Filter));
			Assert("tariff2 should match filter", tariff2.MatchesFilter(filterBizObj.Filter));
			Assert("tariff3 should not match filter", !tariff3.MatchesFilter(filterBizObj.Filter));
			tariffNumberFilter.Property = "0101.01";
			Assert("tariff1 should match filter", tariff1.MatchesFilter(filterBizObj.Filter));
			Assert("tariff2 should not match filter", !tariff2.MatchesFilter(filterBizObj.Filter));
			Assert("tariff3 should not match filter", !tariff3.MatchesFilter(filterBizObj.Filter));
			tariffNumberFilter.Property = "0101.01.1";
			Assert("tariff1 should not match filter", !tariff1.MatchesFilter(filterBizObj.Filter));
			Assert("tariff2 should not match filter", !tariff2.MatchesFilter(filterBizObj.Filter));
			Assert("tariff3 should not match filter", !tariff3.MatchesFilter(filterBizObj.Filter));
			tariffNumberFilter.Property = TariffForTest3;
			Assert("tariff1 should not match filter", !tariff1.MatchesFilter(filterBizObj.Filter));
			Assert("tariff2 should not match filter", !tariff2.MatchesFilter(filterBizObj.Filter));
			Assert("tariff3 should match filter", tariff3.MatchesFilter(filterBizObj.Filter));
		}

		public void TestGetPGACodeQuery()
		{
			var tariff1 = Factory.New<USCTariff>();
			tariff1.UE_PGACodes = "FD1";
			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_PGACodes = "AM1";
			var tariff3 = Factory.New<USCTariff>();
			tariff3.UE_PGACodes = "";
			var filterBizObj = new USCTariffFilterStripBusinessObject();
			var filter = (ModuleTextFilter)filterBizObj[USCTariff.FilterSchema.PGACode];
			filter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filter.IsActive = true;
			filter.Property = "FD";
			Assert("tariff1 should match filter", tariff1.MatchesFilter(filterBizObj.Filter));
			Assert("tariff2 should not match filter", !tariff2.MatchesFilter(filterBizObj.Filter));
			Assert("tariff3 should not match filter", !tariff3.MatchesFilter(filterBizObj.Filter));
		}

		public void TestGetSPIQuery()
		{
			var tariff1 = Factory.New<USCTariff>();
			tariff1.UE_SPICode = "S Y J N E Y";
			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_SPICode = "OPERA";
			var tariff3 = Factory.New<USCTariff>();
			tariff3.UE_SPICode = "H O U S E";
			var tariff4 = Factory.New<USCTariff>();
			tariff4.UE_SPICode = "AGAINEJOY";
			var filterBizObj = new USCTariffFilterStripBusinessObject();
			var spiFilter = (ModuleTextFilter)filterBizObj[USCTariff.FilterSchema.SPI];
			spiFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			spiFilter.IsActive = true;
			CombineAssertions(() =>
			{
				spiFilter.Property = "N";
				Assert("1.tariff1 should match with N filter", tariff1.MatchesFilter(filterBizObj.Filter));
				Assert("1.tariff2 should not match filter", !tariff2.MatchesFilter(filterBizObj.Filter));
				Assert("1.tariff3 should not match filter", !tariff3.MatchesFilter(filterBizObj.Filter));
				Assert("1.tariff3 should not match filter", !tariff4.MatchesFilter(filterBizObj.Filter));
				spiFilter.Property = "JO";
				Assert("2.tariff1 should not match filter", !tariff1.MatchesFilter(filterBizObj.Filter));
				Assert("2.tariff2 should not match filter", !tariff2.MatchesFilter(filterBizObj.Filter));
				Assert("2.tariff3 should not match filter", !tariff3.MatchesFilter(filterBizObj.Filter));
				Assert("2.tariff4 should match with JO filter", tariff4.MatchesFilter(filterBizObj.Filter));
				spiFilter.Property = "J";
				Assert("3.tariff1 should match with J filter", tariff1.MatchesFilter(filterBizObj.Filter));
				Assert("3.tariff2 should not match filter", !tariff2.MatchesFilter(filterBizObj.Filter));
				Assert("3.tariff3 should not match filter", !tariff3.MatchesFilter(filterBizObj.Filter));
				Assert("3.tariff4 should not match filter", !tariff4.MatchesFilter(filterBizObj.Filter));
				spiFilter.Property = "NE";
				Assert("4.tariff1 should not match filter", !tariff1.MatchesFilter(filterBizObj.Filter));
				Assert("4.tariff2 should not match filter", !tariff2.MatchesFilter(filterBizObj.Filter));
				Assert("4.tariff3 should not match filter", !tariff3.MatchesFilter(filterBizObj.Filter));
				Assert("4.tariff4 should match with NE filter", tariff4.MatchesFilter(filterBizObj.Filter));
			});
		}

		public void TestGetPermitLicenseCode()
		{
			var tariff1 = Factory.New<USCTariff>();
			tariff1.UE_PermitLicenseIndicator = "04";
			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_PermitLicenseIndicator = "05";
			var tariff3 = Factory.New<USCTariff>();
			tariff3.UE_PermitLicenseIndicator = "06";
			var filterBizObj = new USCTariffFilterStripBusinessObject();
			var permitFilter = (ModuleTextFilter)filterBizObj[USCTariff.FilterSchema.PermitLicenseCode];
			permitFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			permitFilter.IsActive = true;
			permitFilter.Property = "05";
			Assert("tariff1 should not match filter", !tariff1.MatchesFilter(filterBizObj.Filter));
			Assert("tariff2 should match filter", tariff2.MatchesFilter(filterBizObj.Filter));
			Assert("tariff3 should not match filter", !tariff3.MatchesFilter(filterBizObj.Filter));
		}

		public void TestGetFlagsCode()
		{
			var tariff1 = Factory.New<USCTariff>();
			tariff1.UE_QuotaIndicator = true;
			tariff1.UE_AntiDumping = false;
			tariff1.UE_CountervailingDutyFlag = false;
			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_QuotaIndicator = true;
			tariff2.UE_AntiDumping = true;
			tariff2.UE_CountervailingDutyFlag = true;
			var tariff3 = Factory.New<USCTariff>();
			tariff3.UE_QuotaIndicator = false;
			tariff3.UE_AntiDumping = false;
			tariff3.UE_CountervailingDutyFlag = true;
			var filterBizObj = new USCTariffFilterStripBusinessObject();
			var antiDumpingFilter = (ModuleFlagsFilter)filterBizObj[USCTariffFilterStripBusinessObject.Constants.AntiDumpingFlag];
			antiDumpingFilter.IsActive = true;
			antiDumpingFilter.Property0 = true;
			Assert("tariff1 should not match filter", !tariff1.MatchesFilter(filterBizObj.Filter));
			Assert("tariff2 should match filter", tariff2.MatchesFilter(filterBizObj.Filter));
			Assert("tariff3 should not match filter", !tariff3.MatchesFilter(filterBizObj.Filter));
			antiDumpingFilter.IsActive = false;
			antiDumpingFilter.Property0 = false;
			var quotaIndicatorFilter = (ModuleFlagsFilter)filterBizObj[USCTariffFilterStripBusinessObject.Constants.QuotaIndicator];
			quotaIndicatorFilter.IsActive = true;
			quotaIndicatorFilter.Property0 = true;
			Assert("tariff1 should match filter", tariff1.MatchesFilter(filterBizObj.Filter));
			Assert("tariff2 should match filter", tariff2.MatchesFilter(filterBizObj.Filter));
			Assert("tariff3 should not match filter", !tariff3.MatchesFilter(filterBizObj.Filter));
			quotaIndicatorFilter.IsActive = false;
			quotaIndicatorFilter.Property0 = false;
			var countervailingDutyFlagFilter = (ModuleFlagsFilter)filterBizObj[USCTariffFilterStripBusinessObject.Constants.CountervailingDutyFlag];
			countervailingDutyFlagFilter.IsActive = true;
			countervailingDutyFlagFilter.Property0 = true;
			Assert("tariff1 should not match filter", !tariff1.MatchesFilter(filterBizObj.Filter));
			Assert("tariff2 should not match filter", tariff2.MatchesFilter(filterBizObj.Filter));
			Assert("tariff3 should match filter", tariff3.MatchesFilter(filterBizObj.Filter));
			countervailingDutyFlagFilter.IsActive = false;
			countervailingDutyFlagFilter.Property0 = false;
		}

		public void TestGetDateQuery()
		{
			var tariff1 = Factory.New<USCTariff>();
			tariff1.UE_DateFrom = dateForTest1;
			tariff1.UE_DateTo = dateForTest4;
			var tariff2 = Factory.New<USCTariff>();
			tariff2.UE_DateFrom = dateForTest2;
			tariff2.UE_DateTo = dateForTest3;
			var tariff3 = Factory.New<USCTariff>();
			tariff3.UE_DateFrom = dateForTest1;
			tariff3.UE_DateTo = dateForTest3;
			var filterBizObj = new USCTariffFilterStripBusinessObject();
			var dateFilter = (ModuleDateFilter)filterBizObj[USCTariff.FilterSchema.Date];
			dateFilter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
			dateFilter.IsActive = true;
			dateFilter.Property1 = ZDate.Empty; //date from
			dateFilter.Property2 = filterDateForTest1; //date to
			Assert("tariff1 should match filter", tariff1.MatchesFilter(filterBizObj.Filter));
			Assert("tariff2 should not match filter", !tariff2.MatchesFilter(filterBizObj.Filter));
			Assert("tariff3 should match filter", tariff3.MatchesFilter(filterBizObj.Filter));
			dateFilter.Property1 = filterDateForTest1;
			dateFilter.Property2 = filterDateForTest2;
			Assert("tariff1 should match filter", tariff1.MatchesFilter(filterBizObj.Filter));
			Assert("tariff2 should not match filter", !tariff2.MatchesFilter(filterBizObj.Filter));
			Assert("tariff3 should not match filter", !tariff3.MatchesFilter(filterBizObj.Filter));
			dateFilter.Property1 = filterDateForTest2;
			dateFilter.Property2 = filterDateForTest3;
			Assert("tariff1 should not match filter", !tariff1.MatchesFilter(filterBizObj.Filter));
			Assert("tariff2 should not match filter", !tariff2.MatchesFilter(filterBizObj.Filter));
			Assert("tariff3 should not match filter", !tariff3.MatchesFilter(filterBizObj.Filter));
			dateFilter.Property1 = dateForTest2;
			dateFilter.Property2 = ZDate.Empty;
			Assert("tariff1 should match filter", tariff1.MatchesFilter(filterBizObj.Filter));
			Assert("tariff2 should match filter", tariff2.MatchesFilter(filterBizObj.Filter));
			Assert("tariff3 should match filter", tariff3.MatchesFilter(filterBizObj.Filter));
			dateFilter.PropertySearch = ModuleDateFilter.HasNoDateEntered;
			AssertEquals(true, filterBizObj.Filter.IsNoResultQuery);
			dateFilter.PropertySearch = ModuleDateFilter.HasDateEntered;
			AssertEquals(false, filterBizObj.Filter.IsNoResultQuery);
			AssertEquals(true, filterBizObj.Filter.IsEmpty);
		}

		public void TestSpecProgramIndicatorList()
		{
			var filterBizObj = new USCTariffFilterStripBusinessObject();
			var list = filterBizObj.SpecProgramIndicatorList;
			AssertEquals(39, list.Count);
			Assert(list.ContainsCode(SpecialProgramList.Codes.CA));
			Assert(list.ContainsCode(SpecialProgramList.Codes.MX));
			Assert(list.ContainsCode(SpecialProgramList.Codes.S));
			Assert(list.ContainsCode(SpecialProgramList.Codes.SPlus));
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new USCTariffFilterStripBusinessObject();

		const string TariffForTest1 = "0101010000";
		const string TariffForTest2 = "0101020000";
		const string TariffForTest3 = "0102010000";
		readonly ZDateTime dateForTest1 = new ZDate(1995, 01, 01);
		readonly ZDateTime dateForTest2 = new ZDate(2000, 01, 01);
		readonly ZDateTime dateForTest3 = new ZDate(2005, 01, 01);
		readonly ZDateTime dateForTest4 = new ZDate(2010, 01, 01);
		readonly ZDateTime filterDateForTest1 = new ZDate(1998, 01, 01);
		readonly ZDateTime filterDateForTest2 = new ZDate(2008, 01, 01);
		readonly ZDateTime filterDateForTest3 = new ZDate(2012, 01, 01);
	}
}
