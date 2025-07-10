using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(TariffView.Loader))]
	public class TariffViewLoaderTest : LoaderTestCase
	{
		protected override BusinessObject.Loader GetNewLoaderToTest() => new TariffView.Loader(Factory);

		public void TestLoadTariffByVersion()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var parentDatagrouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.EuropeanUnion, "EUN");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "EUN", parentDatagrouping);
			var xTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Germany, "XXX");
			var s1p1TariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Germany, "1P1");
			Factory.Save();

			var startDate = new ZDateTime(2010, 12, 10);
			var startDate2 = startDate.AddDays(1);
			var cusTariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Germany, xTariffType.PK, "XTYPE", startDate, new ZDateTime(2079, 06, 06), "dummy Description 0", isSystem: false, versionCode: "000");
			var cusTariff2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Germany, s1p1TariffType.PK, "DUMMYTRF", startDate, new ZDateTime(2079, 06, 06), "dummy Description 1", isSystem: false, versionCode: "001");
			var cusTariff3 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Germany, s1p1TariffType.PK, "DUMMYTRF", startDate2, new ZDateTime(2079, 06, 06), "dummy Description 2", isSystem: false, versionCode: "002");
			Factory.Save();

			var loader = new TariffView.Loader(Factory);
			CombineAssertions(() =>
			{
				AssertEquals("cusTariff1", cusTariff1.PK, loader.LoadTariffByVersion(Core.Constants.CountryCodes.Germany, "XXX", "XTYPE", "000", startDate).PK);
				AssertEquals("cusTariff2", cusTariff2.PK, loader.LoadTariffByVersion(Core.Constants.CountryCodes.Germany, "1P1", "DUMMYTRF", "001", startDate).PK);
				AssertEquals("cusTariff3: different version and start Date", cusTariff3.PK, loader.LoadTariffByVersion(Core.Constants.CountryCodes.Germany, "1P1", "DUMMYTRF", "002", startDate2).PK);

				AssertEquals("no matched start Date", null, loader.LoadTariffByVersion(Core.Constants.CountryCodes.Germany, "1P1", "DUMMYTRF", "002", startDate));
				AssertEquals("no matched tariff code", null, loader.LoadTariffByVersion(Core.Constants.CountryCodes.Germany, "1P1", "OTHER", "001", startDate));
				AssertEquals("no matched tariff type", null, loader.LoadTariffByVersion(Core.Constants.CountryCodes.Germany, "1P2", "OTHER", "001", startDate));
				AssertEquals("no matched data grouping", null, loader.LoadTariffByVersion(Core.Constants.CountryCodes.China, "1P1", "DUMMYTRF", "001", startDate));
				AssertEquals("no matched version", null, loader.LoadTariffByVersion("", "XXX", "XTYPE", "000", startDate));
				AssertEquals("no matched tariff type and tariff code", null, loader.LoadTariffByVersion(Core.Constants.CountryCodes.Germany, "", "XTYPE", "000", startDate));
				AssertEquals("no matched version and tariff code", null, loader.LoadTariffByVersion(Core.Constants.CountryCodes.Germany, "XXX", "", "000", startDate));
			});
		}

		[TestDate(2011, 01, 01)]
		public void TestExists()
		{
			var loader = new TariffView.Loader(Factory);
			var s1p1TariffType = Factory.LoadTop1<RefCusTariffType>(new ZQuery(RefCusTariffTypeSchema.ZZI_TariffType, "1P1"));
			var dummyTariff = Helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0");
			var manualTariff = Helper.CreateManualTariff(Core.Constants.CountryCodes.SouthAfrica, "1P1", "MANUALTF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "Manual Description 1");
			Factory.Save();
			Assert("Country Code not match", !loader.Exists(Core.Constants.CountryCodes.Canada, "1P1", "DUMMYTRF", ZDateTime.Today));
			Assert("Tariff Type not match", !loader.Exists(Core.Constants.CountryCodes.SouthAfrica, "EXP", "DUMMYTRF", ZDateTime.Today));
			Assert("Tariff Code not match", !loader.Exists(Core.Constants.CountryCodes.SouthAfrica, "1P1", "DUMMYTRF2", ZDateTime.Today));
			Assert("Effective Date not match", !loader.Exists(Core.Constants.CountryCodes.SouthAfrica, "1P1", "DUMMYTRF2", new ZDateTime(2010, 12, 09)));
			Assert("Invalid Date", !loader.Exists(Core.Constants.CountryCodes.SouthAfrica, "1P1", "DUMMYTRF", ZDateTime.Empty));
			Assert("Invalid Date", !loader.Exists(Core.Constants.CountryCodes.SouthAfrica, "1P1", "DUMMYTRF", ZDateTime.Invalid));
			Assert(loader.Exists(Core.Constants.CountryCodes.SouthAfrica, "1P1", "DUMMYTRF", ZDateTime.Today));
			Assert(loader.Exists(Core.Constants.CountryCodes.SouthAfrica, "1P1", "DUMMYTRF"));
			Assert("Work for manual tariffs.", loader.Exists(Core.Constants.CountryCodes.SouthAfrica, "1P1", "MANUALTF", ZDateTime.Today));
		}

		public void TestExists_ManualTariff()
		{
			var varsion1 = Helper.CreateTariffVersion("HS2021", "HS2021", new ZDate(2021, 1, 1));
			varsion1.CRT_RN_NKCountryCode = Core.Constants.CountryCodes.Congo;
			var varsion2 = Helper.CreateTariffVersion("HS2022", "HS2022", new ZDate(2022, 1, 1));
			varsion2.CRT_RN_NKCountryCode = Core.Constants.CountryCodes.Congo;
			Factory.Save();
			var loader = new TariffView.Loader(Factory);
			var tariff1 = Helper.CreateManualTariff(Core.Constants.CountryCodes.Congo, "HSN", "MANUALTF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "Manual Description 1", tariffVersion: "HS2021");
			var tariff2 = Helper.CreateManualTariff(Core.Constants.CountryCodes.Congo, "HSN", "MANUALTF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "Manual Description 2", tariffVersion: "HS2022");
			Factory.Save();

			CombineAssertions(() =>
			{
				AssertEquals("No tariffs", false, loader.Exists(Core.Constants.CountryCodes.Congo, "HSN", "MANUALTF", new ZDate(2020, 1, 1)));
				AssertEquals("tariff1", true, loader.Exists(Core.Constants.CountryCodes.Congo, "HSN", "MANUALTF", new ZDate(2021, 1, 1)));
				AssertEquals("tariff2", true, loader.Exists(Core.Constants.CountryCodes.Congo, "HSN", "MANUALTF", new ZDate(2022, 1, 1)));
			});
		}

		[TestDate(2021, 01, 01)]
		public void TestLoadMostRecentCachedTariff_InvalidValuationDate()
		{
			var s1p1TariffType = Factory.LoadTop1<RefCusTariffType>(new ZQuery(RefCusTariffTypeSchema.ZZI_TariffType, "1P1"));
			var tariff1 = Helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2021, 06, 06), "dummy Description 0");
			Helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "DUMMYTRF", new ZDateTime(2021, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0");
			Factory.Save();

			var loader = new TariffView.Loader(Factory);
			var tariffView = loader.LoadMostRecentCachedTariff(Core.Constants.CountryCodes.SouthAfrica, "1P1", "DUMMYTRF", ZDateTime.Invalid);
			AssertEquals("Use Today as Effective date to search tariff when valudation date is invalid", tariff1.PK, tariffView.PK);
		}

		[TestDate(2021, 01, 01)]
		public void TestLoadMostRecentCachedTariffWithoutTariffType_InvalidValuationDate()
		{
			var s1p1TariffType = Factory.LoadTop1<RefCusTariffType>(new ZQuery(RefCusTariffTypeSchema.ZZI_TariffType, "1P1"));
			var tariff1 = Helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2021, 06, 06), "dummy Description 0");
			Helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "DUMMYTRF", new ZDateTime(2021, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0");
			Factory.Save();

			var loader = new TariffView.Loader(Factory);
			var tariffView = loader.LoadMostRecentCachedTariff(Core.Constants.CountryCodes.SouthAfrica, "DUMMYTRF", ZDateTime.Invalid);
			AssertEquals("Use Today as Effective date to search tariff when valudation date is invalid", tariff1.PK, tariffView.PK);
		}

		[TestDate(2011, 01, 01)]
		public void TestLoadMostRecentCachedTariffWithType()
		{
			var loader = new TariffView.Loader(Factory);
			TariffView tariffView;
			CombineAssertions(() =>
			{
				var s1p1TariffType = Factory.LoadTop1<RefCusTariffType>(new ZQuery(RefCusTariffTypeSchema.ZZI_TariffType, "1P1"));
				var dummyTariff = Helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0");
				Factory.Save();
				var test1PK = dummyTariff.PK;
				tariffView = loader.LoadMostRecentCachedTariff(Core.Constants.CountryCodes.Cambodia, "1P1", "DUMMYTRF", ZDateTime.Empty);
				AssertNull("wrong country - null", tariffView);
				tariffView = loader.LoadMostRecentCachedTariff(Core.Constants.CountryCodes.SouthAfrica, "1P2", "DUMMYTRF", ZDateTime.Empty);
				AssertNull("wrong type - null", tariffView);
				tariffView = loader.LoadMostRecentCachedTariff(Core.Constants.CountryCodes.SouthAfrica, "1P1", "DUMMYTRFX", ZDateTime.Empty);
				AssertNull("wrong code - null", tariffView);
				tariffView = loader.LoadMostRecentCachedTariff(Core.Constants.CountryCodes.SouthAfrica, "1P1", "DUMMYTRF", new ZDateTime(1993, 01, 01));
				AssertNull("wrong date - not null", tariffView);
				tariffView = loader.LoadMostRecentCachedTariff(Core.Constants.CountryCodes.SouthAfrica, "1P1", "DUMMYTRF", ZDateTime.Today);
				AssertNotNull("test1 - today - not null", tariffView);
				AssertEquals("test1 - today - PK", test1PK, tariffView.PK);
				tariffView = loader.LoadMostRecentCachedTariff(Core.Constants.CountryCodes.SouthAfrica, "1P1", "DUMMYTRF", new ZDateTime(2011, 01, 02));
				AssertNotNull("test1 - fixdate - not null", tariffView);
				AssertEquals("test1 - fixdate - PK", test1PK, tariffView.PK);
				tariffView = loader.LoadMostRecentCachedTariff(Core.Constants.CountryCodes.SouthAfrica, "1P1", "DUMMYTRF", ZDateTime.Empty);
				AssertNotNull("test1 - emptydate - not null", tariffView);
				AssertEquals("test1 - emptydate - PK", test1PK, tariffView.PK);
				dummyTariff = Helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 29), new ZDateTime(2079, 06, 06), "dummy Description 0", 1);
				Factory.Save();
				loader = new TariffView.Loader(new BusinessObjectFactory());
				var test2PK = dummyTariff.PK;
				tariffView = loader.LoadMostRecentCachedTariff(Core.Constants.CountryCodes.SouthAfrica, "1P1", "DUMMYTRF", new ZDateTime(2010, 12, 15));
				AssertNotNull("test2 - fixdate - not null", tariffView);
				AssertEquals("test2 - fxidate - Description", test1PK, tariffView.PK);
				tariffView = loader.LoadMostRecentCachedTariff(Core.Constants.CountryCodes.SouthAfrica, "1P1", "DUMMYTRF", ZDateTime.Empty);
				AssertNotNull("test2 - emptydate - not null", tariffView);
				AssertEquals("test2 - emptydate - Description", test2PK, tariffView.PK);
			});
		}

		[TestDate(2011, 01, 01)]
		public void TestLoadMostRecentCachedTariffStartsWith()
		{
			var loader = new TariffView.Loader(Factory);
			TariffView tariffView;

			CombineAssertions(() =>
			{
				var s1p1TariffType = Factory.LoadTop1<RefCusTariffType>(new ZQuery(RefCusTariffTypeSchema.ZZI_TariffType, "1P1"));
				var dummyTariff = Helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0");
				Factory.Save();
				var test1PK = dummyTariff.PK;

				tariffView = loader.LoadMostRecentCachedTariff(Core.Constants.CountryCodes.SouthAfrica, "1P1", "DUM", new ZDateTime(2011, 01, 02), SQLComparisonOperator.StartsWith);
				AssertNotNull("test1 - fixdate - not null", tariffView);
				AssertEquals("test1 - fixdate - PK", test1PK, tariffView.PK);
			});
		}

		[TestDate(2011, 01, 01)]
		public void TestLoadMostRecentCachedTariffWithoutType()
		{
			var loader = new TariffView.Loader(Factory);
			TariffView tariffView;
			CombineAssertions(() =>
			{
				var s1p1TariffType = Factory.LoadTop1<RefCusTariffType>(new ZQuery(RefCusTariffTypeSchema.ZZI_TariffType, "1P1"));
				var dummyTariff = Helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0");
				Factory.Save();
				var test1PK = dummyTariff.PK;
				tariffView = loader.LoadMostRecentCachedTariff(Core.Constants.CountryCodes.Cambodia, "DUMMYTRF", ZDateTime.Empty);
				AssertNull("wrong country - null", tariffView);
				tariffView = loader.LoadMostRecentCachedTariff(Core.Constants.CountryCodes.SouthAfrica, "DUMMYTRFX", ZDateTime.Empty);
				AssertNull("wrong code - null", tariffView);
				tariffView = loader.LoadMostRecentCachedTariff(Core.Constants.CountryCodes.SouthAfrica, "DUMMYTRF", new ZDateTime(1993, 01, 01));
				AssertNull("wrong date - not null", tariffView);
				tariffView = loader.LoadMostRecentCachedTariff(Core.Constants.CountryCodes.SouthAfrica, "DUMMYTRF", ZDateTime.Today);
				AssertNotNull("test1 - today - not null", tariffView);
				AssertEquals("test1 - today - PK", test1PK, tariffView.PK);
				tariffView = loader.LoadMostRecentCachedTariff(Core.Constants.CountryCodes.SouthAfrica, "DUMMYTRF", new ZDateTime(2011, 01, 02));
				AssertNotNull("test1 - fixdate - not null", tariffView);
				AssertEquals("test1 - fixdate - PK", test1PK, tariffView.PK);
				tariffView = loader.LoadMostRecentCachedTariff(Core.Constants.CountryCodes.SouthAfrica, "DUMMYTRF", ZDateTime.Empty);
				AssertNotNull("test1 - emptydate - not null", tariffView);
				AssertEquals("test1 - emptydate - PK", test1PK, tariffView.PK);
				dummyTariff = Helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 29), new ZDateTime(2079, 06, 06), "dummy Description 0", 1);
				Factory.Save();
				loader = new TariffView.Loader(new BusinessObjectFactory());
				var test2PK = dummyTariff.PK;
				tariffView = loader.LoadMostRecentCachedTariff(Core.Constants.CountryCodes.SouthAfrica, "DUMMYTRF", new ZDateTime(2010, 12, 15));
				AssertNotNull("test2 - fixdate - not null", tariffView);
				AssertEquals("test2 - fxidate - Description", test1PK, tariffView.PK);
				tariffView = loader.LoadMostRecentCachedTariff(Core.Constants.CountryCodes.SouthAfrica, "DUMMYTRF", ZDateTime.Empty);
				AssertNotNull("test2 - emptydate - not null", tariffView);
				AssertEquals("test2 - emptydate - Description", test2PK, tariffView.PK);
			});
		}

		[TestDate(2011, 01, 01)]
		public void TestLoadLatestCachedTariffWithoutEffectiveDate()
		{
			var loader = new TariffView.Loader(Factory);
			TariffView tariffView;
			CombineAssertions(() =>
			{
				var s1p1TariffType = Factory.LoadTop1<RefCusTariffType>(new ZQuery(RefCusTariffTypeSchema.ZZI_TariffType, "1P1"));
				var futureTariff = Helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "DUMMYTRF1", new ZDateTime(2050, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0");
				var expiredTariff = Helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "DUMMYTRF2", new ZDateTime(2000, 12, 10), new ZDateTime(2010, 06, 06), "dummy Description 1");
				Factory.Save();
				tariffView = loader.LoadLatestCachedTariff(Core.Constants.CountryCodes.SouthAfrica, "1P1", "DUMMYTRF1");
				AssertNotNull("Future Tariff can be found", tariffView);
				AssertEquals("Future Tariff can be found", futureTariff.PK, tariffView.PK);
				tariffView = loader.LoadLatestCachedTariff(Core.Constants.CountryCodes.SouthAfrica, "1P1", "DUMMYTRF2");
				AssertNotNull("Expired Tariff can be found", tariffView);
				AssertEquals("Expired Tariff can be found", expiredTariff.PK, tariffView.PK);
			});
		}

		public void TestLoadMostRecentCachedTariffByRelatedTariff()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var s1p1TariffType = Factory.LoadTop1<RefCusTariffType>(new ZQuery(RefCusTariffTypeSchema.ZZI_TariffType, "1P1"));
			var s12ATariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "12A");
			var s12BTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "12B");
			Factory.Save();
			var startDate = ZDateTime.Today.AddDays(-2);
			var endDate = ZDateTime.Today.AddDays(2);
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "1", startDate, endDate);
			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "2", startDate, endDate);
			var tariff11 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s12ATariffType.PK, "11", startDate, endDate);
			var tariff12 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s12ATariffType.PK, "12", startDate, endDate);
			var tariff13 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s12BTariffType.PK, "13", startDate, endDate);
			var tariff21 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s12BTariffType.PK, "21", startDate, endDate);
			helper.CreateTariffRelationship(tariff11.PK, s1p1TariffType.PK, "1");
			helper.CreateTariffRelationship(tariff12.PK, s1p1TariffType.PK, "1");
			helper.CreateTariffRelationship(tariff13.PK, s1p1TariffType.PK, "1");
			helper.CreateTariffRelationship(tariff21.PK, s1p1TariffType.PK, "2");
			Factory.Save();
			var loader = new TariffView.Loader(new BusinessObjectFactory());
			var tariffView = loader.LoadMostRecentCachedTariff(Core.Constants.CountryCodes.SouthAfrica, "12A", "11", ZDateTime.Today);
			AssertEquals("12A/11 - parent not specified", tariffView.PK, tariff11.PK);
			tariffView = loader.LoadMostRecentCachedTariff(Core.Constants.CountryCodes.SouthAfrica, "12A", "11", ZDateTime.Today, "1");
			AssertEquals("12A/11 - parent 1P1/1", tariffView.PK, tariff11.PK);
			tariffView = loader.LoadMostRecentCachedTariff(Core.Constants.CountryCodes.SouthAfrica, "12A", "12", ZDateTime.Today, "1");
			AssertEquals("12A/12 - parent 1P1/1", tariffView.PK, tariff12.PK);
			tariffView = loader.LoadMostRecentCachedTariff(Core.Constants.CountryCodes.SouthAfrica, "12A", "13", ZDateTime.Today, "1");
			AssertNull("12A/13 - parent 1P1/1", tariffView);
			tariffView = loader.LoadMostRecentCachedTariff(Core.Constants.CountryCodes.SouthAfrica, "13", ZDateTime.Today, "1");
			AssertEquals("-/13 - parent 1P1/1", tariffView.PK, tariff13.PK);
			tariffView = loader.LoadMostRecentCachedTariff(Core.Constants.CountryCodes.SouthAfrica, "12B", "13", ZDateTime.Today, "1");
			AssertEquals("12B/12 - parent 1P1/1", tariffView.PK, tariff13.PK);
			tariffView = loader.LoadMostRecentCachedTariff(Core.Constants.CountryCodes.SouthAfrica, "12B", "21", ZDateTime.Today, "1");
			AssertNull("12B/21 - parent 1P1/1", tariffView);
			tariffView = loader.LoadMostRecentCachedTariff(Core.Constants.CountryCodes.SouthAfrica, "12B", "21", ZDateTime.Today, "2");
			AssertEquals("12B/21 - parent 1P1/2", tariffView.PK, tariff21.PK);
		}

		public void TestGetEffectiveChildTariffs()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var s1p1TariffType = Factory.LoadTop1<RefCusTariffType>(new ZQuery(RefCusTariffTypeSchema.ZZI_TariffType, "1P1"));
			var s12ATariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "12A");
			var s12BTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "12B");
			Factory.Save();
			var startDate = ZDateTime.Today.AddDays(-2);
			var endDate = ZDateTime.Today.AddDays(2);
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "1", startDate, endDate);
			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "2", startDate, endDate);
			var tariff11 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s12ATariffType.PK, "11", startDate, endDate);
			var tariff12 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s12ATariffType.PK, "12", startDate, endDate);
			var tariff13 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s12BTariffType.PK, "13", startDate, endDate);
			var tariff21 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s12BTariffType.PK, "21", startDate, endDate);
			helper.CreateTariffRelationship(tariff11.PK, s1p1TariffType.PK, "1");
			helper.CreateTariffRelationship(tariff12.PK, s1p1TariffType.PK, "1");
			helper.CreateTariffRelationship(tariff13.PK, s1p1TariffType.PK, "1");
			helper.CreateTariffRelationship(tariff21.PK, s1p1TariffType.PK, "2");
			Factory.Save();
			var loader = new TariffView.Loader(new BusinessObjectFactory());
			var childTariffs = loader.GetEffectiveChildTariffs(Core.Constants.CountryCodes.SouthAfrica, "1P1", "1", ZDateTime.Today);
			AssertContainsExactElementsInAnyOrder(new[] { tariff11.PK, tariff12.PK, tariff13.PK }, childTariffs.Select(x => x.PK));
			AssertContainsExactElementsInAnyOrder(new[] { tariff11.PK, tariff12.PK, tariff13.PK }, tariff1.GetEffectiveChildTariffs(ZDateTime.Today).Select(x => x.PK));
			childTariffs = loader.GetEffectiveChildTariffs(Core.Constants.CountryCodes.SouthAfrica, "1P1", "1", ZDateTime.Today, "12A");
			AssertContainsExactElementsInAnyOrder(new[] { tariff11.PK, tariff12.PK }, childTariffs.Select(x => x.PK));
			AssertContainsExactElementsInAnyOrder(new[] { tariff11.PK, tariff12.PK }, tariff1.GetEffectiveChildTariffs(ZDateTime.Today, "12A").Select(x => x.PK));
			childTariffs = loader.GetEffectiveChildTariffs(Core.Constants.CountryCodes.SouthAfrica, "1P1", "1", ZDateTime.Today, "12B");
			AssertContainsExactElementsInAnyOrder(new[] { tariff13.PK }, childTariffs.Select(x => x.PK));
			AssertContainsExactElementsInAnyOrder(new[] { tariff13.PK }, tariff1.GetEffectiveChildTariffs(ZDateTime.Today, "12B").Select(x => x.PK));
			childTariffs = loader.GetEffectiveChildTariffs(Core.Constants.CountryCodes.SouthAfrica, "1P1", "2", ZDateTime.Today, "12B");
			AssertContainsExactElementsInAnyOrder(new[] { tariff21.PK }, childTariffs.Select(x => x.PK));
			AssertContainsExactElementsInAnyOrder(new[] { tariff21.PK }, tariff2.GetEffectiveChildTariffs(ZDateTime.Today).Select(x => x.PK));
		}

		public void TestGetEffectiveChildTariffsDBHits()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var s1p1TariffType = Factory.LoadTop1<RefCusTariffType>(new ZQuery(RefCusTariffTypeSchema.ZZI_TariffType, "1P1"));
			var s12ATariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "12A");
			var s12BTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "12B");
			Factory.Save();
			var startDate = ZDateTime.Today.AddDays(-2);
			var endDate = ZDateTime.Today.AddDays(2);
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "1", startDate, endDate);
			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s1p1TariffType.PK, "2", startDate, endDate);
			var tariff11 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s12ATariffType.PK, "11", startDate, endDate);
			var tariff12 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s12ATariffType.PK, "12", startDate, endDate);
			var tariff13 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s12BTariffType.PK, "13", startDate, endDate);
			var tariff21 = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, s12BTariffType.PK, "21", startDate, endDate);
			helper.CreateTariffRelationship(tariff11.PK, s1p1TariffType.PK, "1");
			helper.CreateTariffRelationship(tariff12.PK, s1p1TariffType.PK, "1");
			helper.CreateTariffRelationship(tariff13.PK, s1p1TariffType.PK, "1");
			helper.CreateTariffRelationship(tariff21.PK, s1p1TariffType.PK, "2");
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loader = new TariffView.Loader(newFactory);
			newFactory.AddFetchHint(TariffRelationshipViewSchema.Instance, TariffView.Loader.GetTariffRelationshipViewQuery(ZGuid.Empty, Array.Empty<ZString>(), tariff1.ZZ1_TariffCode, s1p1TariffType.PK));
			newFactory.AddFetchHint(TariffRelationshipViewSchema.Instance, TariffView.Loader.GetTariffRelationshipViewQuery(s12ATariffType.PK, Array.Empty<ZString>(), tariff1.ZZ1_TariffCode, s1p1TariffType.PK));
			newFactory.AddFetchHint(TariffRelationshipViewSchema.Instance, TariffView.Loader.GetTariffRelationshipViewQuery(s12BTariffType.PK, Array.Empty<ZString>(), tariff1.ZZ1_TariffCode, s1p1TariffType.PK));
			newFactory.AddFetchHint(TariffRelationshipViewSchema.Instance, TariffView.Loader.GetTariffRelationshipViewQuery(s12BTariffType.PK, Array.Empty<ZString>(), tariff2.ZZ1_TariffCode, s1p1TariffType.PK));

			using (AssertDbHitsForAllFactories("Performance - Load And Save",
					   new Dictionary<string, int>
									   {
										   { RefCusTariffTypeSchema.Constants.TableName, 2 },
										   { RefDataGroupingSchema.Constants.TableName, 1 },
										   { TariffRelationshipViewSchema.Constants.TableName, 2 },
										   { TariffViewSchema.Constants.TableName, 2 },
										   { CusRefTariffVersionSchema.Constants.TableName, 1 }
									   }))
			{
				loader.GetEffectiveChildTariffs(Core.Constants.CountryCodes.SouthAfrica, "1P1", "1", ZDateTime.Today);
				loader.GetEffectiveChildTariffs(Core.Constants.CountryCodes.SouthAfrica, "1P1", "1", ZDateTime.Today, "12A");
				loader.GetEffectiveChildTariffs(Core.Constants.CountryCodes.SouthAfrica, "1P1", "1", ZDateTime.Today, "12B");
				loader.GetEffectiveChildTariffs(Core.Constants.CountryCodes.SouthAfrica, "1P1", "2", ZDateTime.Today, "12B");
			}
		}

		public void TestGetEffectiveTableType()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var impTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Spain, "IMP");
			var expTariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Spain, "EXP");
			Factory.Save();
			var startDate = ZDateTime.Today.AddDays(-2);
			var endDate = ZDateTime.Today.AddDays(2);
			var publishedDate = ZDate.Today.AddDays(-2);
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.Spain, impTariffType.PK, "1", startDate, endDate);
			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.Spain, expTariffType.PK, "2", startDate, endDate);
			var tariff11 = helper.CreateTariffNationalCode(Core.Constants.CountryCodes.Spain, tariff1.PK, "11", startDate, endDate, publishedDate);
			var tariff12 = helper.CreateTariffNationalCode(Core.Constants.CountryCodes.Spain, tariff1.PK, "12", startDate, endDate, publishedDate);
			Factory.Save();
			var tableTypesImp = TariffView.GetEffectiveTableType(Factory, Core.Constants.CountryCodes.Spain, "IMP", ZDate.Today);
			Assert("The tariffs used for type IMP are going to be national", tableTypesImp == "ZZW");
			var tableTypesExp = TariffView.GetEffectiveTableType(Factory, Core.Constants.CountryCodes.Spain, "EXP", ZDate.Today);
			Assert("The tariffs used for type EXP are going to be not national", tableTypesExp == "ZZ1");
		}

		[TestDate(2019, 2, 12)]
		public void TestGetEffectiveTariffFilter()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Constants.TariffTypes.Export);
			var filter = TariffView.Loader.GetEffectiveTariffFilter(Factory, Core.Constants.CountryCodes.UnitedStates, Constants.TariffTypes.Export, "123", ZDate.Today);
			AssertContains("ZZ1_ZZZ_NKDataGrouping = 'US' and ZZ1_StartDate <= #2019-02-12 00:00:00.000# and ZZ1_EndDate >= #2019-02-12 00:00:00.000# and ZZ1_CRT_NKTariffVersion = '' and ZZ1_TariffCode = '123' and ZZ1_ZZI_NKTariffType = 'EXP'", filter.LiteralTextADO);
		}

		[TestDate(2019, 2, 12)]
		public void TestGetEffectiveTariffFilter_TariffTypeHSN()
		{
			var filter = TariffView.Loader.GetEffectiveTariffFilter(Factory, Core.Constants.CountryCodes.UnitedStates, Constants.TariffTypes.HarmonizedSystem, "123", ZDate.Today);
			AssertContains("ZZ1_ZZZ_NKDataGrouping = 'US' and ZZ1_StartDate <= #2019-02-12 00:00:00.000# and ZZ1_EndDate >= #2019-02-12 00:00:00.000# and ZZ1_CRT_NKTariffVersion = '' and ZZ1_TariffCode = '123' and ZZ1_ZZI_NKTariffType = 'HSN'", filter.LiteralTextADO);
		}

		[TestDate(2019, 2, 12)]
		public void TestGetEffectiveTariffFilter_TariffVersion()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var version = helper.CreateTariffVersion("CG2022", "CG2022", ZDate.Today);
			version.CRT_RN_NKCountryCode = Core.Constants.CountryCodes.Congo;
			Factory.Save();

			var filter = TariffView.Loader.GetEffectiveTariffFilter(Factory, Core.Constants.CountryCodes.Congo, Constants.TariffTypes.HarmonizedSystem, "123", ZDate.Today);
			AssertContains("ZZ1_ZZZ_NKDataGrouping = 'CG' and ZZ1_StartDate <= #2019-02-12 00:00:00.000# and ZZ1_EndDate >= #2019-02-12 00:00:00.000# and ZZ1_CRT_NKTariffVersion = 'CG2022' and ZZ1_TariffCode = '123' and ZZ1_ZZI_NKTariffType = 'HSN'", filter.LiteralTextADO);
		}

		[TestDate(2019, 2, 12)]
		public void TestGetEffectiveTariffFilter_MultipleTariffCodes()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.UnitedStates, Constants.TariffTypes.Export);
			var filter = TariffView.Loader.GetEffectiveTariffFilter(Factory, Core.Constants.CountryCodes.UnitedStates, Constants.TariffTypes.Export, new ZString[] { "123", "456" }, ZDate.Today, SQLComparisonOperator.Equal);
			AssertContains("ZZ1_ZZZ_NKDataGrouping = 'US' and ZZ1_StartDate <= #2019-02-12 00:00:00.000# and ZZ1_EndDate >= #2019-02-12 00:00:00.000# and ZZ1_CRT_NKTariffVersion = '' and (ZZ1_TariffCode in ('123', '456')) and ZZ1_ZZI_NKTariffType = 'EXP'", filter.LiteralTextADO);
			filter = TariffView.Loader.GetEffectiveTariffFilter(Factory, Core.Constants.CountryCodes.UnitedStates, Constants.TariffTypes.Export, new ZString[] { "123", "456" }, ZDate.Today, SQLComparisonOperator.StartsWith);
			AssertContains("(ZZ1_ZZZ_NKDataGrouping = 'US' and ZZ1_StartDate <= #2019-02-12 00:00:00.000# and ZZ1_EndDate >= #2019-02-12 00:00:00.000# and ZZ1_CRT_NKTariffVersion = '' and (ZZ1_TariffCode like '123%' or ZZ1_TariffCode like '456%')) and ZZ1_ZZI_NKTariffType = 'EXP'", filter.LiteralTextADO);
		}

		public void TestGetTariffRelationshipViewQuery()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffTypePk = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "12A").PK;
			var relatedTariffTypePk = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "1P1").PK;
			CombineAssertions(() =>
			{
				var filter = TariffView.Loader.GetTariffRelationshipViewQuery(tariffTypePk, new ZString[] { "123", "456" }, "789", relatedTariffTypePk);
				AssertContains("ZZH_ZZI_TariffType = CONVERT('" + relatedTariffTypePk + "', 'System.Guid') and ('789' LIKE ZZH_TariffCode + '%') and ZZH_ZZI_RelatedTariffType = CONVERT('" + tariffTypePk + "', 'System.Guid') and (ZZH_RelatedTariffCode in ('123', '456'))", filter.LiteralTextADO);
				filter = TariffView.Loader.GetTariffRelationshipViewQuery(tariffTypePk, Array.Empty<ZString>(), "789", relatedTariffTypePk);
				AssertContains("ZZH_ZZI_TariffType = CONVERT('" + relatedTariffTypePk + "', 'System.Guid') and ('789' LIKE ZZH_TariffCode + '%') and ZZH_ZZI_RelatedTariffType = CONVERT('" + tariffTypePk + "', 'System.Guid')", filter.LiteralTextADO);
				filter = TariffView.Loader.GetTariffRelationshipViewQuery(ZGuid.Empty, new ZString[] { "123", "456" }, "789", relatedTariffTypePk);
				AssertContains("ZZH_ZZI_TariffType = CONVERT('" + relatedTariffTypePk + "', 'System.Guid') and ('789' LIKE ZZH_TariffCode + '%') and (ZZH_RelatedTariffCode in ('123', '456'))", filter.LiteralTextADO);
				filter = TariffView.Loader.GetTariffRelationshipViewQuery(ZGuid.Empty, Array.Empty<ZString>(), "789", relatedTariffTypePk);
				AssertContains("ZZH_ZZI_TariffType = CONVERT('" + relatedTariffTypePk + "', 'System.Guid') and ('789' LIKE ZZH_TariffCode + '%')", filter.LiteralTextADO);
			}

			);
		}

		[TestDate(2021, 5, 12)]
		public void TestGetUniqueFieldsFilter()
		{
			var tariff = Factory.New<TariffView>();
			var filter = TariffView.Loader.GetDuplicateManualTariffFilter(tariff, "HSN", "ZA", "CR1", "HS2021", ZDateTime.Today, ZDateTime.Today.AddDays(1));
			AssertContains($"ZZ1_ZZI_NKTariffType = 'HSN' and ZZ1_ZZZ_NKDataGrouping = 'ZA' and ZZ1_TariffCode = 'CR1' and ZZ1_CRT_NKTariffVersion = 'HS2021' and ZZ1_PK <> CONVERT('{tariff.PK}', 'System.Guid') and (ZZ1_StartDate = #2021-05-12 00:00:00.000# or ZZ1_EndDate = #2021-05-13 00:00:00.000#)", filter.LiteralTextADO);
		}

		[TestDate(2021, 5, 12)]
		public void TestGetDateOverlapTariffFilter()
		{
			var tariff = Factory.New<TariffView>();
			var filter = TariffView.Loader.GetDateOverlapManualTariffFilter(tariff, "HSN", "ZA", "CR1", "HS2021", ZDateTime.Today, ZDateTime.Today.AddDays(1));
			AssertContains($"ZZ1_ZZI_NKTariffType = 'HSN' and ZZ1_ZZZ_NKDataGrouping = 'ZA' and ZZ1_TariffCode = 'CR1' and ZZ1_CRT_NKTariffVersion = 'HS2021' and ZZ1_PK <> CONVERT('{tariff.PK}', 'System.Guid') and ((ZZ1_StartDate <= #2021-05-12 00:00:00.000# and ZZ1_EndDate >= #2021-05-12 00:00:00.000#) or (ZZ1_StartDate <= #2021-05-13 00:00:00.000# and ZZ1_EndDate >= #2021-05-13 00:00:00.000#))", filter.LiteralTextADO);
		}

		public void TestAdditionalAttributeInformationProvider()
		{
			var tariffType = helper.CreateNewOrGetExistingTariffType("TW", "1P1");
			Factory.Save();
			var tariff = helper.CreateTariff("TW", tariffType.PK, "DUMMYTRF", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 0");
			AssertNotNull(tariff.AdditionalAttributeInformationProvider);
		}

		public void TestSetupDefaultFilterDataIfNeeded()
		{
			var tariffType = Factory.LoadTop1<RefCusTariffType>(new ZQuery(RefCusTariffTypeSchema.ZZI_TariffType, "1P1"));
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.SouthAfrica, tariffType.PK, "1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			var filterDataMockUS = new Mock<ITariffViewFilterData>();
			filterDataMockUS.Setup(m => m.RatesApplyToCountry).Returns(Core.Constants.CountryCodes.UnitedStates);
			tariff.SetupDefaultFilterDataIfNeeded(filterDataMockUS.Object);
			AssertEquals("Wrapper.RatesApplyToCountry US", Core.Constants.CountryCodes.UnitedStates, tariff.Wrapper.RatesApplyToCountry);
			var filterDataMockIT = new Mock<ITariffViewFilterData>();
			filterDataMockIT.Setup(m => m.RatesApplyToCountry).Returns(Core.Constants.CountryCodes.Italy);
			tariff.SetupDefaultFilterDataIfNeeded(filterDataMockIT.Object);
			AssertEquals("Wrapper.RatesApplyToCountry still US", Core.Constants.CountryCodes.UnitedStates, tariff.Wrapper.RatesApplyToCountry);

			filterDataMockUS.VerifyAll();
			filterDataMockIT.Verify(m => m.RatesApplyToCountry, Times.Never());
		}

		public void TestCountryOrGroupingIsReadOnly()
		{
			AssertHasCustomAttribute<ReadOnlyAttribute>(typeof(TariffView), nameof(TariffView.ZZ1_ZZZ_NKDataGrouping), true, readOnlyAtt => readOnlyAtt.IsReadOnly);
		}

		public void TestDefaultValues()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var tariff = Factory.New<TariffView>();
				AssertEquals(Core.Constants.CountryCodes.Australia, tariff.ZZ1_ZZZ_NKDataGrouping);
				AssertEquals(new ZDateTime(2079, 06, 06), tariff.ZZ1_EndDate);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			Helper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.SouthAfrica, Constants.RateTypes.Duty, "Duty");
			Helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.SouthAfrica, "1P1");
			Helper.CreateOrGetLanguage("ENG", "English");
			Helper.CreateOrGetLanguage("CHS", "Chinese");
			Factory.Save();
		}

		UniversalReferenceTestDataHelper Helper => helper ?? (helper = new UniversalReferenceTestDataHelper(Factory));
		UniversalReferenceTestDataHelper helper;
	}
}
