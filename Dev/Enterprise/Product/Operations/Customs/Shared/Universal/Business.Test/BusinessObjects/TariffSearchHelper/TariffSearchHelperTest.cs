using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(TariffSearchHelper))]
	public class TariffSearchHelperTest : NonPersistentBusinessObjectTestCase
	{
		class TestTariffFormatterRemovePeriods : ITariffFormatter
		{
			public ZString DisplayFormat(ZString unformattedTariff)
			{
				return unformattedTariff.Replace(".", "");
			}

			public ZString Format(ZString unformattedTariff)
			{
				return unformattedTariff.Replace(".", "");
			}

			public ZString UniversalFormat(ZString unformattedTariff)
			{
				return Format(unformattedTariff);
			}
		}

		class TestTariffFormatterReplacePeriodsWithAs : ITariffFormatter
		{
			public ZString DisplayFormat(ZString unformattedTariff)
			{
				return unformattedTariff.Replace(".", "A");
			}

			public ZString Format(ZString unformattedTariff)
			{
				return unformattedTariff.Replace(".", "A");
			}

			public ZString UniversalFormat(ZString unformattedTariff)
			{
				return Format(unformattedTariff);
			}
		}

		public void TestSetChapterHeadingTariff()
		{
			var helper = new TariffSearchHelper(Core.Constants.CountryCodes.Eritrea, "", null, null);
			helper.TariffFormatter = new TestTariffFormatterRemovePeriods();
			helper.ChapterHeadingTariff = "0706.10.00.0";
			AssertEquals("Tariff formatter should remove periods (.)", helper.ChapterHeadingTariff, "070610000");
			helper.TariffFormatter = new TestTariffFormatterReplacePeriodsWithAs();
			helper.ChapterHeadingTariff = "0706.10.00.0";
			AssertEquals("Tariff formatter should remove periods (.) and replace it with A's.", helper.ChapterHeadingTariff, "0706A10A00A0");
			helper.TariffFormatter = null;
			helper.ChapterHeadingTariff = "0706.10.00.0";
			AssertEquals("If no tariffView formatter is provided, then no formatting should apply.", helper.ChapterHeadingTariff, "0706.10.00.0");
		}

		public void TestHasNomenclatureGroup()
		{
			var kddDataGrouping = dataHelper.CreateNewOrGetExistingDataGrouping("KDD", "KDD DESC");
			var wk1DataGrouping = dataHelper.CreateNewOrGetExistingDataGrouping("WK1", "WK1 DESC");
			Factory.Save();
			var wk1TariffType = dataHelper.CreateNewOrGetExistingTariffType("WK1", "TS3");
			wk1TariffType.ZZI_ZZ9_NKNomenclatureGroupType = "KDD";
			Factory.Save();
			AssertEquals("HasNomenclatureGroup", false, new TariffSearchHelper(Core.Constants.CountryCodes.Eritrea, "TST", null, null).HasNomenclatureGroup);
			var tariffType = dataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "TST");
			Factory.Save();
			AssertEquals("HasNomenclatureGroup", false, new TariffSearchHelper(Core.Constants.CountryCodes.Eritrea, "TST", null, null).HasNomenclatureGroup);
			tariffType.ZZI_ZZ9_NKNomenclatureGroupType = "KDD";
			Factory.Save();
			AssertEquals("HasNomenclatureGroup", true, new TariffSearchHelper(Core.Constants.CountryCodes.Eritrea, "TST", null, null).HasNomenclatureGroup);
			AssertEquals("HasNomenclatureGroup", false, new TariffSearchHelper(Core.Constants.CountryCodes.Eritrea, "TS3", null, null).HasNomenclatureGroup);
			var erDataGrouping = dataHelper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Eritrea, "ER DESC");
			erDataGrouping.ZZZ_ZZZ_Grouping = wk1DataGrouping.PK;
			Factory.Save();
			AssertEquals("HasNomenclatureGroup", true, new TariffSearchHelper(Core.Constants.CountryCodes.Eritrea, "TS3", null, null).HasNomenclatureGroup);
		}

		public void TestLoadRefCusNomenclatureGroup()
		{
			dataHelper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Eritrea).ZZZ_ZZZ_Grouping = dataHelper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN).PK;
			var tariffType = dataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "TST", nomenclatureGroupType: "NGT");
			var group1 = dataHelper.CreateNomenclatureGroup(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, "10", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "DESC GROUP 1", "A", "NGT");
			var group2 = dataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Eritrea, "20", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "DESC GROUP 2", "A.B", "XXX");
			var group3 = dataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Eritrea, "30", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "DESC GROUP 3", "C.D", "NGT");
			var group4 = dataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Eritrea, "40", ZDateTime.BrettsBirthday.AddDays(1), ZDateTime.Today.AddYears(1), "DESC GROUP 4", "A.B", "NGT");
			var group5 = dataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Eritrea, "50", ZDateTime.BrettsBirthday.AddDays(1), ZDateTime.Today.AddYears(1), "DESC GROUP 5", "A", "NGT");
			var group6 = dataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Eritrea, "60", ZDateTime.Today.AddMonths(1), ZDateTime.Today.AddYears(1), "DESC GROUP 6", "A", "NGT");
			var group7 = dataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Eritrea, "70", ZDateTime.BrettsBirthday.AddDays(2), ZDateTime.Today.AddDays(-1), "DESC GROUP 7", "A", "NGT");
			var group8 = dataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Ethiopia, "80", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "DESC GROUP 8", "A", "NGT");
			Factory.Save();
			var helper = new TariffSearchHelper(Core.Constants.CountryCodes.Eritrea, "", null, null);
			group1 = helper.Factory.Load<RefCusNomenclatureGroup>(group1.PK);
			group2 = helper.Factory.Load<RefCusNomenclatureGroup>(group2.PK);
			group3 = helper.Factory.Load<RefCusNomenclatureGroup>(group3.PK);
			group4 = helper.Factory.Load<RefCusNomenclatureGroup>(group4.PK);
			group5 = helper.Factory.Load<RefCusNomenclatureGroup>(group5.PK);
			group6 = helper.Factory.Load<RefCusNomenclatureGroup>(group6.PK);
			group7 = helper.Factory.Load<RefCusNomenclatureGroup>(group7.PK);
			group8 = helper.Factory.Load<RefCusNomenclatureGroup>(group8.PK);
			ZQuery dateQuery = new ZQuery();
			dateQuery.AddToFilter(RefCusNomenclatureGroupSchema.ZZ5_StartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, ZDateTime.Today);
			dateQuery.AddToFilter(RefCusNomenclatureGroupSchema.ZZ5_EndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, ZDateTime.Today);
			var groups = helper.LoadRefCusNomenclatureGroup(dateQuery.DeepClone());
			AssertEquals("length", 5, groups.Length);
			AssertContainsExactElementsInAnyOrder(new[] { group1, group2, group3, group4, group5 }, groups);
			groups = helper.LoadRefCusNomenclatureGroup(dateQuery.DeepClone().AddToFilter(RefCusNomenclatureGroupSchema.ZZ5_CompositeKey, SQLComparisonOperator.StartsWith, "A"));
			AssertEquals("length", 4, groups.Length);
			AssertContainsExactElementsInAnyOrder(new[] { group1, group2, group4, group5 }, groups);
			helper = new TariffSearchHelper(Core.Constants.CountryCodes.Eritrea, "TST", dateQuery.DeepClone(), null);
			group1 = helper.Factory.Load<RefCusNomenclatureGroup>(group1.PK);
			group2 = helper.Factory.Load<RefCusNomenclatureGroup>(group2.PK);
			group3 = helper.Factory.Load<RefCusNomenclatureGroup>(group3.PK);
			group4 = helper.Factory.Load<RefCusNomenclatureGroup>(group4.PK);
			group5 = helper.Factory.Load<RefCusNomenclatureGroup>(group5.PK);
			group6 = helper.Factory.Load<RefCusNomenclatureGroup>(group6.PK);
			group7 = helper.Factory.Load<RefCusNomenclatureGroup>(group7.PK);
			group8 = helper.Factory.Load<RefCusNomenclatureGroup>(group8.PK);
			groups = helper.LoadRefCusNomenclatureGroup(new ZQuery());
			AssertEquals("length", 4, groups.Length);
			AssertContainsExactElementsInAnyOrder(new[] { group1, group3, group4, group5 }, groups);
			groups = helper.LoadRefCusNomenclatureGroup(dateQuery.DeepClone().AddToFilter(RefCusNomenclatureGroupSchema.ZZ5_CompositeKey, SQLComparisonOperator.StartsWith, "A"));
			AssertEquals("length", 3, groups.Length);
			AssertContainsExactElementsInAnyOrder(new[] { group1, group4, group5 }, groups);
			ZQuery brettBirthdayQuery = new ZQuery();
			brettBirthdayQuery.AddToFilter(RefCusNomenclatureGroupSchema.ZZ5_StartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, ZDateTime.BrettsBirthday.AddDays(2));
			brettBirthdayQuery.AddToFilter(RefCusNomenclatureGroupSchema.ZZ5_EndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, ZDateTime.BrettsBirthday.AddDays(2));
			helper = new TariffSearchHelper(Core.Constants.CountryCodes.Eritrea, "TST", brettBirthdayQuery, null);
			group1 = helper.Factory.Load<RefCusNomenclatureGroup>(group1.PK);
			group2 = helper.Factory.Load<RefCusNomenclatureGroup>(group2.PK);
			group3 = helper.Factory.Load<RefCusNomenclatureGroup>(group3.PK);
			group4 = helper.Factory.Load<RefCusNomenclatureGroup>(group4.PK);
			group5 = helper.Factory.Load<RefCusNomenclatureGroup>(group5.PK);
			group6 = helper.Factory.Load<RefCusNomenclatureGroup>(group6.PK);
			group7 = helper.Factory.Load<RefCusNomenclatureGroup>(group7.PK);
			group8 = helper.Factory.Load<RefCusNomenclatureGroup>(group8.PK);
			groups = helper.LoadRefCusNomenclatureGroup(new ZQuery(RefCusNomenclatureGroupSchema.ZZ5_CompositeKey, SQLComparisonOperator.StartsWith, "A"));
			AssertEquals("length", 3, groups.Length);
			AssertContainsExactElementsInAnyOrder(new[] { group1, group4, group5 }, groups);
		}

		public void TestLoadRefCusTariff()
		{
			var tariffTypeTST = dataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "TST");
			var tariffTypeT1T = dataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "T1T");
			Factory.Save();
			var tariff1 = dataHelper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffTypeTST.PK, "10", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "DESC TARIFF 1", compositeKey: "A");
			var tariff2 = dataHelper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffTypeTST.PK, "20", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "DESC TARIFF 2", compositeKey: "A.B");
			var tariff3 = dataHelper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffTypeTST.PK, "30", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "DESC TARIFF 3", compositeKey: "C.D");
			var tariff4 = dataHelper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffTypeTST.PK, "40", ZDateTime.BrettsBirthday.AddDays(1), ZDateTime.Today.AddYears(1), "DESC TARIFF 4", compositeKey: "A.B");
			var tariff5 = dataHelper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffTypeT1T.PK, "50", ZDateTime.BrettsBirthday.AddDays(1), ZDateTime.Today.AddYears(1), "DESC TARIFF 5", compositeKey: "A");
			var tariff6 = dataHelper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffTypeTST.PK, "60", ZDateTime.Today.AddMonths(1), ZDateTime.Today.AddYears(1), "DESC TARIFF 6", compositeKey: "A");
			var tariff7 = dataHelper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffTypeTST.PK, "70", ZDateTime.BrettsBirthday.AddDays(2), ZDateTime.Today.AddDays(-1), "DESC TARIFF 7", compositeKey: "A");
			var tariff8 = dataHelper.CreateTariff(Core.Constants.CountryCodes.Ethiopia, tariffTypeTST.PK, "80", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "DESC TARIFF 8", compositeKey: "A");
			Factory.Save();
			var helper = new TariffSearchHelper(Core.Constants.CountryCodes.Eritrea, "", null, null);
			tariff1 = helper.Factory.Load<TariffView>(tariff1.PK);
			tariff2 = helper.Factory.Load<TariffView>(tariff2.PK);
			tariff3 = helper.Factory.Load<TariffView>(tariff3.PK);
			tariff4 = helper.Factory.Load<TariffView>(tariff4.PK);
			tariff5 = helper.Factory.Load<TariffView>(tariff5.PK);
			tariff6 = helper.Factory.Load<TariffView>(tariff6.PK);
			tariff7 = helper.Factory.Load<TariffView>(tariff7.PK);
			tariff8 = helper.Factory.Load<TariffView>(tariff8.PK);
			ZQuery dateQuery = new ZQuery();
			dateQuery.AddToFilter(TariffViewSchema.ZZ1_StartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, ZDateTime.Today);
			dateQuery.AddToFilter(TariffViewSchema.ZZ1_EndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, ZDateTime.Today);
			var tariffs = helper.LoadRefCusTariff(dateQuery.DeepClone());
			AssertEquals("length", 5, tariffs.Length);
			AssertContainsExactElementsInAnyOrder(new[] { tariff1, tariff2, tariff3, tariff4, tariff5 }, tariffs);
			tariffs = helper.LoadRefCusTariff(dateQuery.DeepClone().AddToFilter(TariffViewSchema.ZZ1_CompositeKeyOnZZ5, SQLComparisonOperator.StartsWith, "A"));
			AssertEquals("length", 4, tariffs.Length);
			AssertContainsExactElementsInAnyOrder(new[] { tariff1, tariff2, tariff4, tariff5 }, tariffs);
			helper = new TariffSearchHelper(Core.Constants.CountryCodes.Eritrea, "TST", null, dateQuery.DeepClone());
			tariff1 = helper.Factory.Load<TariffView>(tariff1.PK);
			tariff2 = helper.Factory.Load<TariffView>(tariff2.PK);
			tariff3 = helper.Factory.Load<TariffView>(tariff3.PK);
			tariff4 = helper.Factory.Load<TariffView>(tariff4.PK);
			tariff5 = helper.Factory.Load<TariffView>(tariff5.PK);
			tariff6 = helper.Factory.Load<TariffView>(tariff6.PK);
			tariff7 = helper.Factory.Load<TariffView>(tariff7.PK);
			tariff8 = helper.Factory.Load<TariffView>(tariff8.PK);
			tariffs = helper.LoadRefCusTariff(new ZQuery());
			AssertEquals("length", 4, tariffs.Length);
			AssertContainsExactElementsInAnyOrder(new[] { tariff1, tariff2, tariff3, tariff4 }, tariffs);
			tariffs = helper.LoadRefCusTariff(new ZQuery(TariffViewSchema.ZZ1_CompositeKeyOnZZ5, SQLComparisonOperator.StartsWith, "A"));
			AssertEquals("length", 3, tariffs.Length);
			AssertContainsExactElementsInAnyOrder(new[] { tariff1, tariff2, tariff4 }, tariffs);
			ZQuery brettBirthdayQuery = new ZQuery();
			brettBirthdayQuery.AddToFilter(TariffViewSchema.ZZ1_StartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, ZDateTime.BrettsBirthday.AddDays(2));
			brettBirthdayQuery.AddToFilter(TariffViewSchema.ZZ1_EndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, ZDateTime.BrettsBirthday.AddDays(2));
			helper = new TariffSearchHelper(Core.Constants.CountryCodes.Eritrea, "TST", null, brettBirthdayQuery.DeepClone());
			tariff1 = helper.Factory.Load<TariffView>(tariff1.PK);
			tariff2 = helper.Factory.Load<TariffView>(tariff2.PK);
			tariff3 = helper.Factory.Load<TariffView>(tariff3.PK);
			tariff4 = helper.Factory.Load<TariffView>(tariff4.PK);
			tariff5 = helper.Factory.Load<TariffView>(tariff5.PK);
			tariff6 = helper.Factory.Load<TariffView>(tariff6.PK);
			tariff7 = helper.Factory.Load<TariffView>(tariff7.PK);
			tariff8 = helper.Factory.Load<TariffView>(tariff8.PK);
			tariffs = helper.LoadRefCusTariff(new ZQuery(TariffViewSchema.ZZ1_CompositeKeyOnZZ5, SQLComparisonOperator.StartsWith, "A"));
			AssertEquals("length", 4, tariffs.Length);
			AssertContainsExactElementsInAnyOrder(new[] { tariff1, tariff2, tariff4, tariff7 }, tariffs);
		}

		public void TestSearch()
		{
			dataHelper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Eritrea).ZZZ_ZZZ_Grouping = dataHelper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN).PK;
			var groupType1 = "GR1";
			var groupType2 = "GR2";
			var group1 = dataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Eritrea, "10", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "DESC BOB GROUP 1", "A", groupType1);
			var group2 = dataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Eritrea, "20", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "DESC JAY GROUP 2", "A.B", groupType1);
			var group3 = dataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Eritrea, "30", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "DESC BOB GROUP 3", "C.D", groupType1);
			var group4 = dataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Eritrea, "40", ZDateTime.BrettsBirthday.AddDays(1), ZDateTime.Today.AddYears(1), "DESC JOE GROUP 4", "A.E", groupType1);
			var group5 = dataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Eritrea, "50", ZDateTime.BrettsBirthday.AddDays(1), ZDateTime.Today.AddYears(1), "DESC GROUP 5", "A", groupType2);
			var group6 = dataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Eritrea, "60", ZDateTime.Today.AddMonths(1), ZDateTime.Today.AddYears(1), "DESC GROUP 6", "A", groupType1);
			var group7 = dataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Eritrea, "70", ZDateTime.BrettsBirthday.AddDays(2), ZDateTime.Today.AddDays(-1), "DESC GROUP 7", "A", groupType1);
			var group8 = dataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Ethiopia, "80", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "DESC GROUP 8", "A", groupType1);
			Factory.Save();
			var tariffTypeTST = dataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "TST", nomenclatureGroupType: groupType1);
			var tariffTypeT1T = dataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "T1T", nomenclatureGroupType: groupType2);
			Factory.Save();
			var tariff1 = dataHelper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffTypeTST.PK, "11", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "DESC TARIFF 1", compositeKey: "A.11");
			var tariff2 = dataHelper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffTypeTST.PK, "21", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "DESC TARIFF 2", compositeKey: "A.B.21");
			var tariff3 = dataHelper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffTypeTST.PK, "31", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "DESC JAY TARIFF 3", compositeKey: "G.D.31");
			var tariff4 = dataHelper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffTypeTST.PK, "41", ZDateTime.BrettsBirthday.AddDays(1), ZDateTime.Today.AddYears(1), "DESC TARIFF 4", compositeKey: "A.B.41");
			var tariff5 = dataHelper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffTypeT1T.PK, "51", ZDateTime.BrettsBirthday.AddDays(1), ZDateTime.Today.AddYears(1), "DESC TARIFF 5", compositeKey: "A.51");
			var tariff6 = dataHelper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffTypeTST.PK, "61", ZDateTime.Today.AddMonths(1), ZDateTime.Today.AddYears(1), "DESC TARIFF 6", compositeKey: "A.61");
			var tariff7 = dataHelper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffTypeTST.PK, "71", ZDateTime.BrettsBirthday.AddDays(2), ZDateTime.Today.AddDays(-1), "DESC TARIFF 7", compositeKey: "A.71");
			var tariff8 = dataHelper.CreateTariff(Core.Constants.CountryCodes.Ethiopia, tariffTypeTST.PK, "81", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "DESC TARIFF 8", compositeKey: "A.81");
			var tariff9 = dataHelper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffTypeTST.PK, "91", ZDateTime.BrettsBirthday.AddDays(3), ZDateTime.Today.AddDays(-1), "DESC TARIFF 9", compositeKey: "");
			var tariff10 = dataHelper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffTypeTST.PK, "92", ZDateTime.BrettsBirthday.AddDays(4), ZDateTime.Today.AddYears(1), "DESC TARIFF 10", compositeKey: "");
			var tariff11 = dataHelper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffTypeTST.PK, "93", ZDateTime.BrettsBirthday.AddDays(5), ZDateTime.Today.AddYears(1), "DESC TARIFF 11", compositeKey: "C.D.93");
			Factory.Save();
			var dateQuery = new ZQuery();
			dateQuery.AddToFilter(TariffViewSchema.ZZ1_StartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, ZDateTime.Today);
			dateQuery.AddToFilter(TariffViewSchema.ZZ1_EndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, ZDateTime.Today);
			var helper = new TariffSearchHelper(Core.Constants.CountryCodes.Eritrea, "TST", null, null);
			var bizObjs = helper.Search(new ZQuery(TariffViewSchema.ZZ1_Description, SQLComparisonOperator.StartsWith, " "), dateQuery, new ZQuery(TariffViewSchema.ZZ1_Description, SQLComparisonOperator.StartsWith, " "), null).ToArray();
			AssertEquals(0, bizObjs.Length);
			bizObjs = helper.Search(new ZQuery(TariffViewSchema.ZZ1_Description, SQLComparisonOperator.Contains, "BOB").AddToFilter(dateQuery), dateQuery, new ZQuery(TariffViewSchema.ZZ1_Description, SQLComparisonOperator.Contains, "BOB"), null).ToArray();
			AssertEquals(2, bizObjs.Length);
			AssertEquals("10", bizObjs[0].TariffCode);
			AssertEquals("OTHER", bizObjs[1].TariffCode);
			var relatedDataCollection = bizObjs[1].RelatedDataCollection.ToArray();
			AssertEquals("OTHER.RelatedDataCollection", 1, relatedDataCollection.Length);
			AssertEquals("OTHER.RelatedDataCollection[0].TariffCode", "30", relatedDataCollection[0].TariffCode);
			bizObjs = helper.Search(new ZQuery(TariffViewSchema.ZZ1_TariffCode, SQLComparisonOperator.StartsWith, "31"), dateQuery, null, new ZQuery(TariffViewSchema.ZZ1_TariffCode, SQLComparisonOperator.StartsWith, "31")).ToArray();
			AssertEquals(1, bizObjs.Length);
			AssertEquals("OTHER", bizObjs[0].TariffCode);
			relatedDataCollection = bizObjs[0].RelatedDataCollection.ToArray();
			AssertEquals("OTHER.RelatedDataCollection", 1, relatedDataCollection.Length);
			AssertEquals("OTHER.RelatedDataCollection[0].TariffCode", "31", relatedDataCollection[0].TariffCode);
			bizObjs = helper.Search(new ZQuery(TariffViewSchema.ZZ1_Description, SQLComparisonOperator.Contains, "JAY"), dateQuery, new ZQuery(TariffViewSchema.ZZ1_Description, SQLComparisonOperator.Contains, "JAY"), null).ToArray();
			AssertEquals(2, bizObjs.Length);
			AssertEquals("10", bizObjs[0].TariffCode);
			AssertEquals("OTHER", bizObjs[1].TariffCode);
			bizObjs = helper.Search(new ZQuery(TariffViewSchema.ZZ1_TariffCode, SQLComparisonOperator.StartsWith, "2"), dateQuery, null, new ZQuery(TariffViewSchema.ZZ1_TariffCode, SQLComparisonOperator.StartsWith, "2")).ToArray();
			AssertEquals(1, bizObjs.Length);
			AssertEquals("10", bizObjs[0].TariffCode);
			bizObjs = helper.Search(new ZQuery(TariffViewSchema.ZZ1_TariffCode, SQLComparisonOperator.StartsWith, "9"), dateQuery, null, new ZQuery(TariffViewSchema.ZZ1_TariffCode, SQLComparisonOperator.StartsWith, "9")).ToArray();
			AssertEquals(1, bizObjs.Length);
			AssertEquals("OTHER", bizObjs[0].TariffCode);
			helper = new TariffSearchHelper(Core.Constants.CountryCodes.Eritrea, "", null, null);
			helper.TariffFormatter = new TestTariffFormatterRemovePeriods();
			helper.ChapterHeadingTariff = "3.1..";
			bizObjs = helper.Search(new ZQuery(TariffViewSchema.ZZ1_TariffCode, SQLComparisonOperator.StartsWith, helper.ChapterHeadingTariff), dateQuery, null, new ZQuery(TariffViewSchema.ZZ1_TariffCode, SQLComparisonOperator.StartsWith, helper.ChapterHeadingTariff)).ToArray();
			AssertEquals(1, bizObjs.Length);
			AssertEquals("OTHER", bizObjs[0].TariffCode);
			relatedDataCollection = bizObjs[0].RelatedDataCollection.ToArray();
			AssertEquals("OTHER.RelatedDataCollection", 1, relatedDataCollection.Length);
			AssertEquals("OTHER.RelatedDataCollection[0].TariffCode", "31", relatedDataCollection[0].TariffCode);
		}

		public void TestSearch_Hierarchy()
		{
			var tariffTypeTST = dataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "TST");
			var group1 = dataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Eritrea, "11", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "DESC GROUP 1", "A");
			var group2 = dataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Eritrea, "11.20", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "DESC GROUP 2", "A.B");
			var group3 = dataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Eritrea, "11.20..10", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "DESC GROUP 3", "A.B..C");
			var group4 = dataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Eritrea, "11.20..11", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "DESC GROUP 4", "A.B..D");
			var group5 = dataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Eritrea, "11.20..11.01", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "DESC GROUP 5", "A.B..D.E");
			var group6 = dataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Eritrea, "11.20..11.02", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "DESC GROUP 6", "A.B..D.F");
			Factory.Save();
			var tariff1 = dataHelper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffTypeTST.PK, "201001", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "DESC TARIFF 1", compositeKey: "A.B..C");
			var tariff2 = dataHelper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffTypeTST.PK, "201101", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "DESC TARIFF 2", compositeKey: "A.B..D.E");
			var tariff3 = dataHelper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffTypeTST.PK, "201102", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "DESC TARIFF 3", compositeKey: "A.B..D.F");
			Factory.Save();

			var helper = new TariffSearchHelper(Core.Constants.CountryCodes.Eritrea, "TST", null, null);
			var dateQuery = new ZQuery();
			dateQuery.AddToFilter(TariffViewSchema.ZZ1_StartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, ZDateTime.Today);
			dateQuery.AddToFilter(TariffViewSchema.ZZ1_EndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, ZDateTime.Today);
			var bizObjs = helper.Search(new ZQuery(TariffViewSchema.ZZ1_TariffCode, SQLComparisonOperator.StartsWith, "20"), dateQuery, null, null).ToArray();
			AssertHierararchy(
				"Filter StartsWith 20",
				"11",
				"-11.20",
				"--201001",
				"--11.20..11",
				"---201101",
				"---201102"
			);

			bizObjs = helper.Search(new ZQuery(TariffViewSchema.ZZ1_TariffCode, SQLComparisonOperator.StartsWith, "2010"), dateQuery, null, null).ToArray();
			AssertHierararchy(
				"Filter StartsWith 2010",
				"11",
				"-11.20",
				"--201001"
			);

			bizObjs = helper.Search(new ZQuery(TariffViewSchema.ZZ1_TariffCode, SQLComparisonOperator.StartsWith, "2011"), dateQuery, null, null).ToArray();
			AssertHierararchy(
				"Filter StartsWith 2011",
				"11",
				"-11.20",
				"--11.20..11",
				"---201101",
				"---201102"
			);

			void AssertHierararchy(string message, params string[] expectedResult)
			{
				var actualResult = new List<string>();
				FlattenTariffDataObjects(bizObjs, actualResult, 0);

				AssertSequencesEqual(message, expectedResult, actualResult);
			}

			static void FlattenTariffDataObjects(IEnumerable<TariffDataObject> tariffDataObjects, List<string> tariffs, int level)
			{
				foreach (var tariffDataObject in tariffDataObjects)
				{
					tariffs.Add(string.Empty.PadLeft(level, '-') + tariffDataObject.TariffCode.ToString());
					FlattenTariffDataObjects(tariffDataObject.RelatedDataCollection, tariffs, level + 1);
				}
			}
		}

		public void TestSearch_NomenclatureCode()
		{
			dataHelper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Eritrea).ZZZ_ZZZ_Grouping = dataHelper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN).PK;
			var groupType1 = "GR1";
			var group1 = dataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Eritrea, "10", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "DESC BOB GROUP 1", "A", groupType1);
			Factory.Save();

			var tariffTypeTST = dataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "TST", nomenclatureGroupType: groupType1);
			Factory.Save();

			var tariff1 = dataHelper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffTypeTST.PK, "101", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "DESC TARIFF 1", compositeKey: "A.11");
			var tariff2 = dataHelper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffTypeTST.PK, "211", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "DESC TARIFF 2", compositeKey: "A.22");
			var tariff3 = dataHelper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffTypeTST.PK, "311", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "DESC JAY TARIFF 3", compositeKey: "A.33");
			Factory.Save();

			var dateQuery = new ZQuery();
			dateQuery.AddToFilter(TariffViewSchema.ZZ1_StartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, ZDateTime.Today);
			dateQuery.AddToFilter(TariffViewSchema.ZZ1_EndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, ZDateTime.Today);

			var helper = new TariffSearchHelper(Core.Constants.CountryCodes.Eritrea, "TST", null, null, false, true);
			var bizObjs = helper.Search(new ZQuery(TariffViewSchema.ZZ1_TariffCode, SQLComparisonOperator.StartsWith, "10"), dateQuery, null, new ZQuery(TariffViewSchema.ZZ1_TariffCode, SQLComparisonOperator.StartsWith, "10")).ToArray();

			AssertEquals(1, bizObjs.Length);
			AssertEquals(group1.ZZ5_Value, bizObjs[0].TariffCode);

			var expectedTariffCodes = new[] { tariff1.ZZ1_TariffCode, tariff2.ZZ1_TariffCode, tariff3.ZZ1_TariffCode };
			var actualTariffCodes = bizObjs[0].RelatedDataCollection.Select(x => x.TariffCode);

			AssertContainsExactElementsInAnyOrder(expectedTariffCodes, actualTariffCodes);
		}

		public void TestSearch_HandleNomenclatureGroup_Description()
		{
			var dataGroup = "Z1!";
			dataHelper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Eritrea).ZZZ_ZZZ_Grouping = dataHelper.CreateNewOrGetExistingDataGrouping(dataGroup).PK;
			var groupType1 = "GR1";
			var group1 = dataHelper.CreateNomenclatureGroup(dataGroup, "01", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "LIVE ANIMALS; ANIMAL PRODUCTS", "01", groupType1);
			var group2 = dataHelper.CreateNomenclatureGroup(dataGroup, "01", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "LIVE ANIMALS", "01.02", groupType1);
			var group3 = dataHelper.CreateNomenclatureGroup(dataGroup, "0101", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "Live horses, asses, mules and hinnies", "01.02..03", groupType1);
			var group4 = dataHelper.CreateNomenclatureGroup(dataGroup, "", ZDateTime.BrettsBirthday.AddDays(1), ZDateTime.Today.AddYears(1), "Horses", "01.02..03.2", groupType1);
			var group5 = dataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Ethiopia, "0101A", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "Invalid data", "01.02..03", groupType1);
			Factory.Save();
			var tariffTypeTST = dataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "TST", nomenclatureGroupType: groupType1);
			Factory.Save();
			var tariff1 = dataHelper.CreateTariff(dataGroup, tariffTypeTST.PK, "0101210000", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "Pure-bred breeding animals", compositeKey: "01.02..03.2.1");
			var tariff2 = dataHelper.CreateTariff(dataGroup, tariffTypeTST.PK, "0101291000", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "For slaughter", compositeKey: "01.02..03.2.9.10");
			var tariff3 = dataHelper.CreateTariff(dataGroup, tariffTypeTST.PK, "0101299000", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "Other", compositeKey: "01.02..03.2.9.20");
			Factory.Save();
			var dateQuery = new ZQuery(TariffViewSchema.ZZ1_StartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, ZDateTime.Today);
			dateQuery.AddToFilter(TariffViewSchema.ZZ1_EndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, ZDateTime.Today);
			var helper = new TariffSearchHelper(Core.Constants.CountryCodes.Eritrea, "TST", null, null);
			var descriptionQuery = new ZQuery(TariffViewSchema.ZZ1_Description, SQLComparisonOperator.Contains, "hinnies");
			var bizObjs = helper.Search(new ZQuery(descriptionQuery), dateQuery, descriptionQuery, null).ToArray();
			AssertMultilineASCIIEquals("Data", @"01 LIVE ANIMALS; ANIMAL PRODUCTS - 1
	01 LIVE ANIMALS - 1
		0101 Live horses, asses, mules and hinnies - 1
			 Horses - 3
				0101210000 Pure-bred breeding animals - 0
				0101291000 For slaughter - 0
				0101299000 Other - 0
", GetDataDetails(bizObjs, 0));
		}

		public void TestSearch_HandleNomenclatureGroup()
		{
			var dataGroup = "Z1!";
			dataHelper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Eritrea).ZZZ_ZZZ_Grouping = dataHelper.CreateNewOrGetExistingDataGrouping(dataGroup).PK;
			var groupType1 = "GR1";
			var groupType2 = "GR2";
			var group1 = dataHelper.CreateNomenclatureGroup(dataGroup, "01", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "LIVE ANIMALS; ANIMAL PRODUCTS", "01", groupType1);
			var group2 = dataHelper.CreateNomenclatureGroup(dataGroup, "01", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "LIVE ANIMALS", "01.01", groupType1);
			var group3 = dataHelper.CreateNomenclatureGroup(dataGroup, "0101", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "Live horses, asses, mules and hinnies", "01.01..01", groupType1);
			var group4 = dataHelper.CreateNomenclatureGroup(dataGroup, "", ZDateTime.BrettsBirthday.AddDays(1), ZDateTime.Today.AddYears(1), "Horses", "01.01..01.2", groupType1);
			var group5 = dataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Ethiopia, "0101A", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "Invalid data", "01.01..01", groupType1);
			var group6 = dataHelper.CreateNomenclatureGroup(dataGroup, "0101C", ZDateTime.BrettsBirthday.AddDays(-1), ZDateTime.Today.AddYears(1), "Invalid data", "01.01..01", groupType2);
			Factory.Save();
			var tariffTypeTST = dataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "TST", nomenclatureGroupType: groupType1);
			var tariffTypeTSTET = dataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Ethiopia, "TST", nomenclatureGroupType: groupType2);
			Factory.Save();
			var tariff1 = dataHelper.CreateTariff(dataGroup, tariffTypeTST.PK, "0101210000", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "Pure-bred breeding animals", compositeKey: "01.01..01.2.1");
			var tariff2 = dataHelper.CreateTariff(dataGroup, tariffTypeTST.PK, "0101291000", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "For slaughter", compositeKey: "01.01..01.2.9.10");
			var tariff3 = dataHelper.CreateTariff(dataGroup, tariffTypeTST.PK, "0101299000", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "Other", compositeKey: "01.01..01.2.9.20");
			Factory.Save();
			var dateQuery = new ZQuery(TariffViewSchema.ZZ1_StartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, ZDateTime.Today);
			dateQuery.AddToFilter(TariffViewSchema.ZZ1_EndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, ZDateTime.Today);
			var helper = new TariffSearchHelper(Core.Constants.CountryCodes.Eritrea, "TST", null, null);
			var bizObjs = helper.Search(new ZQuery(TariffViewSchema.ZZ1_TariffCode, SQLComparisonOperator.StartsWith, "010121"), dateQuery, null, null).ToArray();
			AssertMultilineASCIIEquals("Data", @"01 LIVE ANIMALS; ANIMAL PRODUCTS - 1
	01 LIVE ANIMALS - 1
		0101 Live horses, asses, mules and hinnies - 1
			 Horses - 1
				0101210000 Pure-bred breeding animals - 0", GetDataDetails(bizObjs, 0));
		}

		string GetDataDetails(TariffDataObject[] bizObjs, int level)
		{
			var result = new ZStringBuilder();
			foreach (var bizObj in bizObjs)
			{
				var relatedDatas = bizObj.RelatedDataCollection.ToArray();
				result.AppendLine($"{"".PadRight(level, '\t')}{bizObj.TariffCodeAndDescription} - {relatedDatas.Length}");
				result.Append(GetDataDetails(relatedDatas, level + 1));
			}

			return result.ToString();
		}

		public void TestGenerateCompositeKey()
		{
			var helper = new TariffSearchHelper(Core.Constants.CountryCodes.Eritrea, "", null, null);
			var compositeKeyComponents = new ZString[] { "10", "20", "30", "40", "50", "60", "70", "80" };
			AssertEquals("10", helper.GenerateCompositeKey(compositeKeyComponents, 1));
			AssertEquals("10.20", helper.GenerateCompositeKey(compositeKeyComponents, 2));
			AssertEquals("10.20.30", helper.GenerateCompositeKey(compositeKeyComponents, 3));
			AssertEquals("10.20.30.40", helper.GenerateCompositeKey(compositeKeyComponents, 4));
			AssertEquals("10.20.30.40.50", helper.GenerateCompositeKey(compositeKeyComponents, 5));
			AssertEquals("10.20.30.40.50.60", helper.GenerateCompositeKey(compositeKeyComponents, 6));
			AssertEquals("10.20.30.40.50.60.70", helper.GenerateCompositeKey(compositeKeyComponents, 7));
			AssertEquals("10.20.30.40.50.60.70.80", helper.GenerateCompositeKey(compositeKeyComponents, 8));
			AssertEquals("10.20.30.40.50.60.70.80", helper.GenerateCompositeKey(compositeKeyComponents, 9));
		}

		public void TestGetRefCusNomenclatureGroupMainFilter()
		{
			dataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "TST", nomenclatureGroupType: "NGT");
			var group1 = dataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Eritrea, "10", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "DESC GROUP 1", "A", "NGT");
			var group2 = dataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Eritrea, "20", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "DESC GROUP 2", "A.B", "XXX");
			var group3 = dataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Eritrea, "30", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "DESC GROUP 3", "C.D", "NGT");
			var group4 = dataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Eritrea, "40", ZDateTime.BrettsBirthday.AddDays(1), ZDateTime.Today.AddYears(1), "DESC GROUP 4", "A.B", "NGT");
			var group5 = dataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Eritrea, "50", ZDateTime.BrettsBirthday.AddDays(1), ZDateTime.Today.AddYears(1), "DESC GROUP 5", "A", "NGT");
			var group6 = dataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Eritrea, "60", ZDateTime.Today.AddMonths(1), ZDateTime.Today.AddYears(1), "DESC GROUP 6", "A", "NGT");
			var group7 = dataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Eritrea, "70", ZDateTime.BrettsBirthday.AddDays(2), ZDateTime.Today.AddDays(-1), "DESC GROUP 7", "A", "NGT");
			var group8 = dataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Ethiopia, "80", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "DESC GROUP 8", "A", "NGT");
			Factory.Save();
			ZQuery dateQuery = new ZQuery();
			dateQuery.AddToFilter(RefCusNomenclatureGroupSchema.ZZ5_StartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, ZDateTime.Today);
			dateQuery.AddToFilter(RefCusNomenclatureGroupSchema.ZZ5_EndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, ZDateTime.Today);
			var helper = new TariffSearchHelper(Core.Constants.CountryCodes.Eritrea, "", dateQuery, null);
			var groups = Factory.Load<RefCusNomenclatureGroup>(helper.GetRefCusNomenclatureGroupMainFilter());
			AssertEquals("length", 5, groups.Length);
			AssertContainsExactElementsInAnyOrder(new[] { group1, group2, group3, group4, group5 }, groups);
			helper = new TariffSearchHelper(Core.Constants.CountryCodes.Eritrea, "TST", dateQuery, null);
			groups = Factory.Load<RefCusNomenclatureGroup>(helper.GetRefCusNomenclatureGroupMainFilter());
			AssertEquals("length", 4, groups.Length);
			AssertContainsExactElementsInAnyOrder(new[] { group1, group3, group4, group5 }, groups);
			var count = Factory.GetTableHitCount(RefCusNomenclatureGroup.Schema.TableName);
			groups = Factory.Load<RefCusNomenclatureGroup>(helper.GetRefCusNomenclatureGroupMainFilter());
			AssertEquals("length", 4, groups.Length);
			AssertContainsExactElementsInAnyOrder(new[] { group1, group3, group4, group5 }, groups);
			AssertEquals("Should not hit the db againt", count, Factory.GetTableHitCount(RefCusNomenclatureGroup.Schema.TableName));
		}

		public void TestGetRefCusTariffMainFilter()
		{
			var tariffTypeTST = dataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "TST");
			var tariffTypeT1T = dataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "T1T");
			Factory.Save();
			var tariff1 = dataHelper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffTypeTST.PK, "10", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "DESC TARIFF 1", compositeKey: "A");
			var tariff2 = dataHelper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffTypeTST.PK, "20", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "DESC TARIFF 2", compositeKey: "A.B");
			var tariff3 = dataHelper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffTypeTST.PK, "30", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "DESC TARIFF 3", compositeKey: "C.D");
			var tariff4 = dataHelper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffTypeTST.PK, "40", ZDateTime.BrettsBirthday.AddDays(1), ZDateTime.Today.AddYears(1), "DESC TARIFF 4", compositeKey: "A.B");
			var tariff5 = dataHelper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffTypeT1T.PK, "50", ZDateTime.BrettsBirthday.AddDays(1), ZDateTime.Today.AddYears(1), "DESC TARIFF 5", compositeKey: "A");
			var tariff6 = dataHelper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffTypeTST.PK, "60", ZDateTime.Today.AddMonths(1), ZDateTime.Today.AddYears(1), "DESC TARIFF 6", compositeKey: "A");
			var tariff7 = dataHelper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffTypeTST.PK, "70", ZDateTime.BrettsBirthday.AddDays(2), ZDateTime.Today.AddDays(-1), "DESC TARIFF 7", compositeKey: "A");
			var tariff8 = dataHelper.CreateTariff(Core.Constants.CountryCodes.Ethiopia, tariffTypeTST.PK, "80", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "DESC TARIFF 8", compositeKey: "A");
			Factory.Save();
			ZQuery dateQuery = new ZQuery();
			dateQuery.AddToFilter(TariffViewSchema.ZZ1_StartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, ZDateTime.Today);
			dateQuery.AddToFilter(TariffViewSchema.ZZ1_EndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, ZDateTime.Today);
			var helper = new TariffSearchHelper(Core.Constants.CountryCodes.Eritrea, "", null, dateQuery.DeepClone());
			var tariffs = Factory.Load<TariffView>(helper.GetRefCusTariffMainFilter());
			AssertEquals("length", 5, tariffs.Length);
			AssertContainsExactElementsInAnyOrder(new[] { tariff1, tariff2, tariff3, tariff4, tariff5 }, tariffs);
			helper = new TariffSearchHelper(Core.Constants.CountryCodes.Eritrea, "TST", null, dateQuery.DeepClone());
			tariffs = Factory.Load<TariffView>(helper.GetRefCusTariffMainFilter());
			AssertEquals("length", 4, tariffs.Length);
			AssertContainsExactElementsInAnyOrder(new[] { tariff1, tariff2, tariff3, tariff4 }, tariffs);
		}

		public void TestGetRefCusTariffMainFilterNationalCode()
		{
			var tariffTypeTST = dataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "TST");
			var tariffTypeT1T = dataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "T1T");
			Factory.Save();
			var tariff1 = dataHelper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffTypeTST.PK, "10", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "DESC TARIFF 1", compositeKey: "A");
			var tariff2 = dataHelper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffTypeT1T.PK, "20", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "DESC TARIFF 2", compositeKey: "A.B");
			var tariff11 = dataHelper.CreateTariffNationalCode(Core.Constants.CountryCodes.Eritrea, tariff1.PK, "101", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), ZDate.Today.AddMonths(1));
			var tariff12 = dataHelper.CreateTariffNationalCode(Core.Constants.CountryCodes.Eritrea, tariff1.PK, "102", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), ZDate.BrettsBirthday.AddDays(2));
			Factory.Save();
			var helper = new TariffSearchHelper(Core.Constants.CountryCodes.Eritrea, "TST", null, null);
			var tariffs = Factory.Load<TariffView>(helper.GetRefCusTariffMainFilter());
			AssertEquals("length", 2, tariffs.Length);
			AssertContainsExactElementsInAnyOrder(new[] { tariff11, tariff12 }, tariffs);
			helper = new TariffSearchHelper(Core.Constants.CountryCodes.Eritrea, "T1T", null, null);
			tariffs = Factory.Load<TariffView>(helper.GetRefCusTariffMainFilter());
			AssertEquals("length", 1, tariffs.Length);
			AssertContainsExactElementsInAnyOrder(new[] { tariff2 }, tariffs);
		}

		public void TestGetOrCreateTariffDataObject()
		{
			var tariffTypeTST = dataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "TST");
			Factory.Save();
			var group1 = dataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Eritrea, "10", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "DESC BOB GROUP 1", "A");
			var tariff1 = dataHelper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffTypeTST.PK, "10", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "DESC TARIFF 1", compositeKey: "A");
			Factory.Save();
			var helper = new TariffSearchHelper(Core.Constants.CountryCodes.Eritrea, "", null, null);
			var data1 = helper.GetOrCreateTariffDataObject(group1);
			var data2 = helper.GetOrCreateTariffDataObject(group1);
			AssertEquals(true, object.ReferenceEquals(data1, data2));
			var data3 = helper.GetOrCreateTariffDataObject(tariff1);
			AssertEquals(false, object.ReferenceEquals(data1, data3));
		}

		[StressTest]
		public void TestLoadingLargeDataDBHits()
		{
			var groupType1 = "NGT";
			var tariffTypeTST = dataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "TST", nomenclatureGroupType: groupType1);
			Factory.Save();
			var group1 = dataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Eritrea, "A", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "DESC BOB GROUP A", "A", groupType1);
			for (int i = 1; i < 10; i++)
			{
				var iValue = "A.B" + i;
				var iDate = ZDateTime.BrettsBirthday.AddDays(i);
				var group2 = dataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Eritrea, iValue, iDate, ZDateTime.Today.AddYears(1), $"DESC{i} BOB GROUP {iValue}", iValue, groupType1);
				for (int j = 1; j < 4; j++)
				{
					var jValue = iValue + ".C" + j;
					var jDate = iDate.AddHours(j);
					var group3 = dataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Eritrea, jValue, iDate, ZDateTime.Today.AddYears(1), $"DESC{j} BOB{i} GROUP {jValue}", jValue, groupType1);
					for (int k = 1; k < 10; k++)
					{
						var kValue = jValue + "." + k;
						var kDate = jDate.AddMinutes(k);
						var tariff1 = dataHelper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffTypeTST.PK, kValue, kDate, ZDateTime.Today.AddYears(1), $"DESC{k} BOB{i} TARIFF {kValue}", compositeKey: kValue);
					}
				}

				for (int k = 1; k < 10; k++)
				{
					var kValue = $"A.{i}.{k}";
					var kDate = iDate.AddMonths(k);
					var tariff1 = dataHelper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffTypeTST.PK, kValue, kDate, ZDateTime.Today.AddYears(1), $"DESC{k} BOB TARIFF {kValue}", compositeKey: kValue);
				}
			}

			Factory.Save();
			CombineAssertions(() =>
			{
				ObjectFactory.Get<Integration.Customs.Shared.ICustomsDataRegistry>().ShowHeaderTariffData.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
				AssertDbHits("1", ZString.Empty, "DESC", 1, 3, 2, 3, 2, @"1|1
2|90
3|27
4|243");
				AssertDbHits("2", ".3", "DESC", 1, 3, 2, 3, 2, @"1|1
2|18
3|27
4|27");
				AssertDbHits("3", ZString.Empty, "DESC3 BOB9", 1, 3, 1, 3, 1, @"1|1
2|1
3|3
4|3");
				AssertDbHits("4", ".3", "DESC3 BOB9", 1, 3, 2, 3, 2, @"1|1
2|1
3|3
4|3");
				AssertDbHits("5", ZString.Empty, "TARIFF", 1, 2, 1, 2, 1, @"1|1
2|90
3|27
4|243");
				AssertDbHits("6", ".3", "TARIFF", 1, 2, 1, 2, 1, @"1|1
2|18
3|27
4|27");
				AssertDbHits("7", ZString.Empty, "DESC3", 1, 3, 2, 3, 2, @"1|1
2|18
3|27
4|51");
				AssertDbHits("8", ".3", "DESC3", 1, 3, 2, 3, 2, @"1|1
2|18
3|27
4|27");
				ObjectFactory.Get<Integration.Customs.Shared.ICustomsDataRegistry>().ShowHeaderTariffData.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
				AssertDbHits("9", ZString.Empty, "DESC", 10, 3, 2, 3, 2, @"1|1
2|3
3|27");
				AssertDbHits("10", ".3", "DESC", 10, 3, 2, 3, 2, @"1|1
2|3
3|3");
				AssertDbHits("11", ZString.Empty, "DESC3 BOB9", 1, 3, 1, 3, 1, @"1|1
2|3
3|3");
				AssertDbHits("12", ".3", "DESC3 BOB9", 1, 3, 2, 3, 2, @"1|1
2|3
3|3");
				AssertDbHits("13", ZString.Empty, "TARIFF", 10, 2, 1, 2, 1, @"1|1
2|3
3|27");
				AssertDbHits("14", ".3", "TARIFF", 10, 2, 1, 2, 1, @"1|1
2|3
3|3");
				AssertDbHits("15", ZString.Empty, "DESC3", 10, 3, 2, 3, 2, @"1|1
2|3
3|3");
				AssertDbHits("16", ".3", "DESC3", 10, 3, 2, 3, 2, @"1|1
2|3
3|3");
			}

			);
		}

		TariffSearchHelper AssertDbHits(ZString messagePrefix, ZString tariffCode, ZString partialDesc, int resultCount, int nomenclatureGroupBefore, int tariffBefore, int nomenclatureGroupAfter, int tariffAfter, ZString expectedMessageData)
		{
			ZQuery dateQuery = new ZQuery();
			dateQuery.AddToFilter(RefCusNomenclatureGroupSchema.ZZ5_StartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, ZDateTime.Today);
			dateQuery.AddToFilter(RefCusNomenclatureGroupSchema.ZZ5_EndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, ZDateTime.Today);
			var helper = new TariffSearchHelper(Core.Constants.CountryCodes.Eritrea, "", dateQuery.DeepClone(), null);
			var factory = helper.Factory;
			var query = new ZQuery();
			ZQuery tariffQuery = null;
			if (!tariffCode.IsEmpty)
			{
				tariffQuery = new ZQuery(TariffViewSchema.ZZ1_TariffCode, SQLComparisonOperator.EndsWith, tariffCode);
				query.AddToFilter(tariffQuery);
			}

			ZQuery descQuery = null;
			if (!partialDesc.IsEmpty)
			{
				descQuery = new ZQuery(TariffViewSchema.ZZ1_Description, SQLComparisonOperator.Contains, partialDesc);
				query.AddToFilter(descQuery);
			}

			var result = helper.Search(query, dateQuery, descQuery, tariffQuery);
			AssertEquals(messagePrefix + " result.Length", resultCount, result.Length);
			AssertEquals(messagePrefix + " RefCusNomenclatureGroup hit count before", nomenclatureGroupBefore, factory.GetTableHitCount(RefCusNomenclatureGroupSchema.Constants.TableName));
			AssertEquals(messagePrefix + " TariffView hit count before", tariffBefore, factory.GetTableHitCount(TariffViewSchema.Constants.TableName));
			var dataLevel = new Dictionary<int, int>();
			LoadAll(result[0], dataLevel, 0);
			AssertXMLEquals(messagePrefix, expectedMessageData, new ZStringBuilder(dataLevel.Select(x => x.Key + "|" + x.Value)).ToStringWithNewLineBetweenAppends());
			AssertEquals(messagePrefix + " RefCusNomenclatureGroup hit count after", nomenclatureGroupAfter, factory.GetTableHitCount(RefCusNomenclatureGroupSchema.Constants.TableName));
			AssertEquals(messagePrefix + " TariffView hit count after", tariffAfter, factory.GetTableHitCount(TariffViewSchema.Constants.TableName));
			return helper;
		}

		public void TestTariffMatched()
		{
			var tariffTypeTST = dataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "TST");
			Factory.Save();
			var group1 = dataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Eritrea, "10", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "DESC BOB GROUP 1", compositeKey: "A");
			var tariff1 = dataHelper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffTypeTST.PK, "1010", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "DESC TARIFF 1", compositeKey: "A.B");
			Factory.Save();
			ZQuery dateQuery = new ZQuery();
			dateQuery.AddToFilter(TariffViewSchema.ZZ1_StartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, ZDateTime.Today);
			dateQuery.AddToFilter(TariffViewSchema.ZZ1_EndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, ZDateTime.Today);
			var helper = new TariffSearchHelper(Core.Constants.CountryCodes.Eritrea, "", null, dateQuery.DeepClone());
			AssertNull("TariffMatched", helper.TariffMatched);
			var result = helper.Search(new ZQuery(TariffViewSchema.ZZ1_Description, SQLComparisonOperator.StartsWith, "DESC"), dateQuery, new ZQuery(TariffViewSchema.ZZ1_Description, SQLComparisonOperator.StartsWith, "DESC"), null).ToArray();
			AssertEquals(1, result.Length);
			AssertNull("TariffMatched", helper.TariffMatched);
			AssertEquals("result[0].TariffCode", "10", result[0].TariffCode);
			result = helper.Search(new ZQuery(TariffViewSchema.ZZ1_TariffCode, SQLComparisonOperator.StartsWith, "10"), dateQuery, null, new ZQuery(TariffViewSchema.ZZ1_TariffCode, SQLComparisonOperator.StartsWith, "10")).ToArray();
			AssertEquals(1, result.Length);
			AssertNull("TariffMatched", helper.TariffMatched);
			AssertEquals("result[0].TariffCode", "10", result[0].TariffCode);
			helper.ChapterHeadingTariff = "1010";
			result = helper.Search(new ZQuery(TariffViewSchema.ZZ1_TariffCode, SQLComparisonOperator.StartsWith, "1010"), dateQuery, null, new ZQuery(TariffViewSchema.ZZ1_TariffCode, SQLComparisonOperator.StartsWith, "1010")).ToArray();
			AssertEquals(1, result.Length);
			AssertEquals("result[0].TariffCode", "10", result[0].TariffCode);
			AssertNotNull("TariffMatched", helper.TariffMatched);
		}

		public void TestMinimumDescriptionLength()
		{
			var helper = new TariffSearchHelper(Core.Constants.CountryCodes.NewZealand, "", null, null);
			AssertEquals("Default PartialDescriptionMinLength is 3 :", 3, helper.PartialDescriptionMinLength);
			helper.Language = "ZHT";
			helper.PartialDescriptionMinLength = helper.GetPartialDescriptionMinLength(3);
			AssertEquals("PartialDescriptionMinLength is 1 when Language is ZHT :", 1, helper.PartialDescriptionMinLength);
			helper.Language = "ZHS";
			helper.PartialDescriptionMinLength = helper.GetPartialDescriptionMinLength(3);
			AssertEquals("PartialDescriptionMinLength is 1 when Language is ZHS :", 1, helper.PartialDescriptionMinLength);
		}

		public void TestPreferedLanguage()
		{
			dataHelper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Taiwan);
			var group1 = dataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Taiwan, "10", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "VEGETABLE PRODUCTS", "10", Core.Constants.CountryCodes.Taiwan);
			Factory.Save();
			dataHelper.CreateOrGetLanguage("ZHT", "ChineseTraditional");
			dataHelper.CreateNomenclatureGroupLanguage(group1.PK, "ZHT", "植物產品");
			Factory.Save();
			var tariffTypeHSN = dataHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "HSN", nomenclatureGroupType: Core.Constants.CountryCodes.Taiwan);
			Factory.Save();
			var tariff1 = dataHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, tariffTypeHSN.PK, "1011", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "HEALTHY VEGETABLE", compositeKey: "10.11");
			Factory.Save();
			dataHelper.LoadOrCreateNewCusRefTariffLanguageView(Factory, tariff1.PK, "ZHT", "健康植物");
			Factory.Save();
			var dateQuery = new ZQuery();
			dateQuery.AddToFilter(TariffViewSchema.ZZ1_StartDate, SQLComparisonOperator.LessThanOrEqualToDatePartOnly, ZDateTime.Today);
			dateQuery.AddToFilter(TariffViewSchema.ZZ1_EndDate, SQLComparisonOperator.GreaterThanOrEqualToDatePartOnly, ZDateTime.Today);
			var groupQuery = new ZQuery(TariffViewSchema.ZZ1_TariffCode, SQLComparisonOperator.StartsWith, "10");
			TariffDataObject[] bizObjs;
			GlbStaff.CurrentUser.GS_WorkingLanguage = Core.Constants.Languages.English;
			var helper = new TariffSearchHelper(Core.Constants.CountryCodes.Taiwan, "HSN", null, null);
			AssertContainsExactElementsInAnyOrder(new CodeDescriptionPairList { new CodeDescriptionPair("DEF", "Default"), new CodeDescriptionPair("ZHT", "Chinese - Traditional") }, helper.Languages);
			helper.Language = "DEF";
			AssertEquals("", helper.LanguageCode);
			bizObjs = helper.Search(groupQuery, dateQuery, null, null).ToArray();
			AssertEquals("VEGETABLE PRODUCTS", bizObjs[0].FullDescription);
			bizObjs = bizObjs[0].RelatedDataCollection.ToArray();
			AssertEquals("HEALTHY VEGETABLE", bizObjs[0].FullDescription);
			GlbStaff.CurrentUser.GS_WorkingLanguage = Core.Constants.Languages.ChineseTraditional;
			helper = new TariffSearchHelper(Core.Constants.CountryCodes.Taiwan, "HSN", null, null);
			AssertEquals("ZHT", helper.LanguageCode);
			bizObjs = helper.Search(groupQuery, dateQuery, null, null).ToArray();
			AssertEquals("植物產品", bizObjs[0].FullDescription);
			bizObjs = bizObjs[0].RelatedDataCollection.ToArray();
			AssertEquals("健康植物", bizObjs[0].FullDescription);
			GlbStaff.CurrentUser.GS_WorkingLanguage = Core.Constants.Languages.English;
			helper = new TariffSearchHelper(Core.Constants.CountryCodes.Taiwan, "HSN", null, null);
			helper.Language = "ZHT";
			AssertEquals("ZHT", helper.LanguageCode);
			bizObjs = helper.Search(groupQuery, dateQuery, null, null).ToArray();
			AssertEquals("植物產品", bizObjs[0].FullDescription);
			bizObjs = bizObjs[0].RelatedDataCollection.ToArray();
			AssertEquals("健康植物", bizObjs[0].FullDescription);
			helper.Language = "ZHS";
			AssertEquals("ZHS", helper.LanguageCode);
			bizObjs = helper.Search(groupQuery, dateQuery, null, null).ToArray();
			AssertEquals("VEGETABLE PRODUCTS", bizObjs[0].FullDescription);
			bizObjs = bizObjs[0].RelatedDataCollection.ToArray();
			AssertEquals("HEALTHY VEGETABLE", bizObjs[0].FullDescription);
			helper = new TariffSearchHelper(Core.Constants.CountryCodes.Taiwan, "HSN", null, null);
			AssertEquals("DEF", helper.Language);
		}

		public void TestLoadPreferedLanguage()
		{
			dataHelper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Taiwan);
			var group1 = dataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Taiwan, "10", ZDateTime.BrettsBirthday, ZDateTime.Today.AddYears(1), "VEGETABLE PRODUCTS", "10", Core.Constants.CountryCodes.Taiwan);
			Factory.Save();
			dataHelper.CreateOrGetLanguage("ZHT", "ChineseTraditional");
			dataHelper.CreateNomenclatureGroupLanguage(group1.PK, "ZHT", "植物產品");
			Factory.Save();

			var manager = new TariffPreferredLanguageManager();
			manager.Language = "ZH-TW";
			var helper = new TariffSearchHelper(Core.Constants.CountryCodes.Taiwan, "HSN", null, null);
			AssertEquals("ZHT", helper.Language);
			helper.Language = "ZHT";
			helper = new TariffSearchHelper(Core.Constants.CountryCodes.Taiwan, "HSN", null, null);
			AssertEquals("ZHT", helper.Language);
			manager.Language = "";
			helper = new TariffSearchHelper(Core.Constants.CountryCodes.Taiwan, "HSN", null, null);
			AssertEquals("DEF", helper.Language);
			helper.Language = "ZHS";
			helper = new TariffSearchHelper(Core.Constants.CountryCodes.Taiwan, "HSN", null, null);
			AssertEquals("DEF", helper.Language);
		}

		public void TestPreferredLanguageCode()
		{
			GlbStaff.CurrentUser.GS_WorkingLanguage = Enterprise.Core.Constants.Languages.English;
			var helper = new TariffSearchHelper(Core.Constants.CountryCodes.Taiwan, "HSN", null, null);
			AssertEquals("DEF", helper.Language);
			AssertEquals("", helper.LanguageCode);
			helper.Language = "ZHT";
			AssertEquals("ZHT", helper.Language);
			AssertEquals("ZHT", helper.LanguageCode);
			helper.Language = "ZHS";
			AssertEquals("ZHS", helper.Language);
			AssertEquals("ZHS", helper.LanguageCode);
			helper.Language = "DEF";
			AssertEquals("DEF", helper.Language);
			AssertEquals("", helper.LanguageCode);
			helper.Language = "DE";
			AssertEquals("DE", helper.Language);
			AssertEquals("DE", helper.LanguageCode);
			GlbStaff.CurrentUser.GS_WorkingLanguage = Enterprise.Core.Constants.Languages.German;
			helper = new TariffSearchHelper(Core.Constants.CountryCodes.Taiwan, "HSN", null, null);
			helper.Language = "DEF";
			AssertEquals("DEF", helper.Language);
			AssertEquals("", helper.LanguageCode);
			helper.Language = "DE";
			AssertEquals("DE", helper.Language);
			AssertEquals("DE", helper.LanguageCode);
			helper.Language = "DEF";
			AssertEquals("DEF", helper.Language);
			AssertEquals("", helper.LanguageCode);
		}

		public void TestPreferredLanguageCodeForAGermanCompany()
		{
			dataHelper.CreateOrGetLanguage("FR", "French");
			var nc = dataHelper.CreateNomenclatureGroup(Core.Constants.CountryCodes.Germany, "123", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			nc.ZZ5_CompositeKey = "1.2.3";
			dataHelper.CreateNomenclatureGroupLanguage(nc.PK, "FR", "Chaussure");
			Factory.Save();

			GlbStaff.CurrentUser.GS_WorkingLanguage = Core.Constants.Languages.English;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var helper = new TariffSearchHelper(Core.Constants.CountryCodes.Germany, "HSN", null, null);
				AssertEquals("DE", helper.Language);
				AssertEquals("DE", helper.LanguageCode);
				helper.Language = "FR";
				var helper1 = new TariffSearchHelper(Core.Constants.CountryCodes.Germany, "HSN", null, null);
				AssertEquals("FR", helper1.LanguageCode);
			}
		}

		public void TestLanguagesWithoutDuplicatesIgnoringCountryCodes()
		{
			const string DataGroupingCode = Core.Constants.CountryCodes.Eritrea;
			const string TariffType = "TST";
			dataHelper.CreateOrGetLanguage("FR", "French");
			dataHelper.CreateOrGetLanguage("EN", "English");
			var nc = dataHelper.CreateNomenclatureGroup(DataGroupingCode, "123", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			nc.ZZ5_CompositeKey = "1.2.3";
			dataHelper.CreateNomenclatureGroupLanguage(nc.PK, "FR", "Chaussure");
			dataHelper.CreateNomenclatureGroupLanguage(nc.PK, "EN", "Shoes");
			Factory.Save();
			var helper = new TariffSearchHelper(DataGroupingCode, TariffType, null, null);
			AssertEquals("DEF, EN, FR", helper.Languages.CodesAsString);
		}

		static void LoadAll(TariffDataObject tariffDataObject, Dictionary<int, int> dataLevel, int level)
		{
			int count;
			level++;
			if (dataLevel.TryGetValue(level, out count))
			{
				dataLevel[level] = ++count;
			}
			else
			{
				dataLevel.Add(level, 1);
			}

			foreach (var relatedData in tariffDataObject.RelatedDataCollection)
			{
				LoadAll(relatedData, dataLevel, level);
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new TariffSearchHelper(Core.Constants.CountryCodes.Eritrea, "", null, null);
		}

		UniversalReferenceTestDataHelper dataHelper;
		protected override void SetUp()
		{
			base.SetUp();
			dataHelper = new UniversalReferenceTestDataHelper(Factory);
		}
	}
}
