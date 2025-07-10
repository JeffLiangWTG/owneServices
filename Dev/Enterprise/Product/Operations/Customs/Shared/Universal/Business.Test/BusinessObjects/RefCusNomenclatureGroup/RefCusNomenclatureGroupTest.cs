using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusNomenclatureGroup))]
	class RefCusNomenclatureGroupTest : EnterpriseBusinessObjectTestCase
	{
		public void TestITariffDataMemebers()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var nomenclatureGroup = helper.CreateNomenclatureGroup(Core.Constants.CountryCodes.SouthAfrica, "99999999", ZDateTime.BrettsBirthday, ZDateTime.Today, "Alpha Bravo", "99...99.99");
			ITariffData tariffData = nomenclatureGroup;
			AssertEquals("tariffData.IsNomenclatureGroup", true, tariffData.IsNomenclatureGroup);
			AssertEquals("tariffData.CompositeKey", "99...99.99", tariffData.CompositeKey);
			AssertEquals("tariffData.TariffCode", "99999999", tariffData.TariffCode);
			AssertEquals("tariffData.GetDescription", "Alpha Bravo", tariffData.GetDescription(Env.CurrentUser.Language));
		}

		public void TestGetFullDescriptionForCompositeKey()
		{
			var startDate = new ZDateTime(1900, 01, 01);
			var endDate = new ZDateTime(2050, 01, 01);
			var tariffType = "1P1";
			var cnTariffType = "CN";
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunDataGrouping = helper.CreateNewOrGetExistingDataGrouping("EUN", "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", parent: eunDataGrouping);
			helper.CreateNomenclatureGroup(Core.Constants.CountryCodes.SouthAfrica, "99999999", startDate, endDate, "Alpha Bravo", "99...99.99", nomenclatureGroupType: tariffType);
			helper.CreateNomenclatureGroup(Core.Constants.CountryCodes.SouthAfrica, "99999999", startDate, endDate, "Charlie Delta", "99...99", nomenclatureGroupType: tariffType);
			helper.CreateNomenclatureGroup(Core.Constants.CountryCodes.SouthAfrica, "99999999", startDate, endDate, "Echo Foxtrot", "99", nomenclatureGroupType: tariffType);
			helper.CreateNomenclatureGroup("EUN", "87", startDate, endDate, "VEHICLES OTHER THAN RAILWAY OR TRAMWAY ROLLING STOCK, AND PARTS AND ACCESSORIES THEREOF", compositeKey: "17.87", nomenclatureGroupType: cnTariffType);
			helper.CreateNomenclatureGroup("EUN", "8703", startDate, endDate, "Motor cars and other motor vehicles principally designed for the transport of persons (other than those of heading|8702), including station wagons and racing cars", compositeKey: "17.87..03", nomenclatureGroupType: cnTariffType);
			Factory.Save();
			AssertEquals("Echo Foxtrot Charlie Delta Alpha Bravo", RefCusNomenclatureGroup.GetFullDescriptionForCompositeKey(Factory, "99...99.99", Core.Constants.CountryCodes.SouthAfrica, tariffType, ZDateTime.Today, true, true));
			AssertEquals("Charlie Delta Alpha Bravo", RefCusNomenclatureGroup.GetFullDescriptionForCompositeKey(Factory, "99...99.99", Core.Constants.CountryCodes.SouthAfrica, tariffType, ZDateTime.Today, false, true));
			CombineAssertions("Testing Parent Data Grouping", () =>
			{
				var fullMessageError = "VEHICLES OTHER THAN RAILWAY OR TRAMWAY ROLLING STOCK, AND PARTS AND ACCESSORIES THEREOF Motor cars and other motor vehicles principally designed for the transport of persons (other than those of heading|8702), including station wagons and racing cars";
				AssertEquals("EUN is parent data grouping for DE", fullMessageError, RefCusNomenclatureGroup.GetFullDescriptionForCompositeKey(Factory, "17.87..03.90", Core.Constants.CountryCodes.Germany, cnTariffType, ZDateTime.Today, false, false));
				AssertEquals("EUN is NOT parent data grouping for ZA", "", RefCusNomenclatureGroup.GetFullDescriptionForCompositeKey(Factory, "17.87..03.90", Core.Constants.CountryCodes.SouthAfrica, cnTariffType, ZDateTime.Today, false, false));
			}

			);
		}

		[TestDate(2016, 08, 03)]
		public void TestGetDescriptionForCompositeKey()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var eunDataGrouping = helper.CreateNewOrGetExistingDataGrouping("EUN", "European Union");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany, "Germany", parent: eunDataGrouping);
			helper.CreateNomenclatureGroup(Core.Constants.CountryCodes.SouthAfrica, "99.99.99.99", new ZDateTime(1900, 01, 01), new ZDateTime(2050, 01, 01), "Test Description", "99.99.99.99.99", nomenclatureGroupType: "1P1");
			helper.CreateNomenclatureGroup(Core.Constants.CountryCodes.SouthAfrica, ZString.Empty, new ZDateTime(1900, 01, 01), new ZDateTime(2050, 01, 01), "Chapter Header", "99.99", nomenclatureGroupType: "1P1");
			helper.CreateNomenclatureGroup("EUN", "87", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, "VEHICLES OTHER THAN RAILWAY OR TRAMWAY ROLLING STOCK, AND PARTS AND ACCESSORIES THEREOF", compositeKey: "17.87", nomenclatureGroupType: "CN");
			Factory.Save();
			CombineAssertions("Variations of Parameters for GetDescriptionForCompositeKey", () =>
			{
				Assert("Invalid option", RefCusNomenclatureGroup.GetDescriptionForCompositeKey(Factory, "99.99.99.99.98", Core.Constants.CountryCodes.SouthAfrica, "1P1", ZDateTime.Today, true).IsEmpty);
				AssertEquals("Valid option 1.1", "Test Description", RefCusNomenclatureGroup.GetDescriptionForCompositeKey(Factory, "99.99.99.99.99", Core.Constants.CountryCodes.SouthAfrica, "1P1", ZDateTime.Today, true));
				AssertEquals("Valid option 1.2", "Test Description", RefCusNomenclatureGroup.GetDescriptionForCompositeKey(Factory, "99.99.99.99.99", Core.Constants.CountryCodes.SouthAfrica, "1P1", ZDateTime.Today, false));
				AssertEquals("Valid option 2.1", "Chapter Header", RefCusNomenclatureGroup.GetDescriptionForCompositeKey(Factory, "99.99", Core.Constants.CountryCodes.SouthAfrica, "1P1", ZDateTime.Today, true));
				Assert("Valid option 2.2", RefCusNomenclatureGroup.GetDescriptionForCompositeKey(Factory, "99.99", Core.Constants.CountryCodes.SouthAfrica, "1P1", ZDateTime.Today, false).IsEmpty);
			}

			);
			CombineAssertions("Testing Parent Data Grouping", () =>
			{
				AssertEquals("EUN is parent data grouping for DE", "VEHICLES OTHER THAN RAILWAY OR TRAMWAY ROLLING STOCK, AND PARTS AND ACCESSORIES THEREOF", RefCusNomenclatureGroup.GetDescriptionForCompositeKey(Factory, "17.87", Core.Constants.CountryCodes.Germany, "CN", ZDateTime.Today, false));
				AssertEquals("EUN is NOT parent data grouping for ZA", "", RefCusNomenclatureGroup.GetDescriptionForCompositeKey(Factory, "17.87", Core.Constants.CountryCodes.SouthAfrica, "CN", ZDateTime.Today, false));
			}

			);
		}

		public void TestGetDescriptionForCompositeKeyWithDifferentLanguage()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateOrGetLanguage("ENG", "English");
			helper.CreateOrGetLanguage("THA", "Thai");
			var tariffType = "1P1";
			var group1 = helper.CreateNomenclatureGroup(Core.Constants.CountryCodes.SouthAfrica, "10", ZDateTime.Today.AddDays(-1), ZDateTime.MaxSmallDateTime, "Description 01", "10", nomenclatureGroupType: tariffType);
			var group2 = helper.CreateNomenclatureGroup(Core.Constants.CountryCodes.SouthAfrica, "20", ZDateTime.Today.AddDays(-1), ZDateTime.MaxSmallDateTime, "Description 02", "20", nomenclatureGroupType: tariffType);
			Factory.Save();
			var language1 = Factory.New<RefCusNomenclatureLanguage>();
			language1.ZX8_ZZ5_NomenclatureGroup = group1.PK;
			language1.ZX8_ZX6_NKLanguage = "ENG";
			language1.ZX8_Description = "Test Decription 01";
			var language2 = Factory.New<RefCusNomenclatureLanguage>();
			language2.ZX8_ZZ5_NomenclatureGroup = group1.PK;
			language2.ZX8_ZX6_NKLanguage = "THA";
			language2.ZX8_Description = @"รายละเอียดการทดสอบ 01";
			var language3 = Factory.New<RefCusNomenclatureLanguage>();
			language3.ZX8_ZZ5_NomenclatureGroup = group2.PK;
			language3.ZX8_ZX6_NKLanguage = "ENG";
			language3.ZX8_Description = "Test Decription 03";
			var language4 = Factory.New<RefCusNomenclatureLanguage>();
			language4.ZX8_ZZ5_NomenclatureGroup = group2.PK;
			language4.ZX8_ZX6_NKLanguage = "THA";
			language4.ZX8_Description = @"รายละเอียดการทดสอบ 02";
			Factory.Save();
			var keys = new[] { group1.ZZ5_CompositeKey, group2.ZZ5_CompositeKey };
			var originalDescriptions = new ZString[] { "Description 01", "Description 02" };
			AssertCorrectDescriptionsWithLanguage("CS-CZ", keys, originalDescriptions);
			AssertCorrectDescriptionsWithLanguage("TH-TH", keys, originalDescriptions);
			AssertCorrectDescriptionsWithLanguage("EN", keys, originalDescriptions);
		}

		void AssertCorrectDescriptionsWithLanguage(string language, ZString[] keys, ZString[] expectedDescriptions)
		{
			var origianlLanguage = GlbStaff.CurrentUser.GS_WorkingLanguage;
			try
			{
				var factory = new BusinessObjectFactory();
				GlbStaff.CurrentUser[GlbStaffSchema.GS_WorkingLanguage] = language;
				foreach (var key in keys)
				{
					var description = RefCusNomenclatureGroup.GetDescriptionForCompositeKey(factory, key, Core.Constants.CountryCodes.SouthAfrica, "1P1", ZDateTime.Today, true);
					AssertCollectionContains(description, expectedDescriptions);
				}
			}
			finally
			{
				GlbStaff.CurrentUser[GlbStaffSchema.GS_WorkingLanguage] = origianlLanguage;
			}
		}

		public void TestZZ5_AlternateLanguageDescription()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateOrGetLanguage("ENG", "English");
			helper.CreateOrGetLanguage("THA", "Thai");
			var tariffType = "1P1";
			var group1 = helper.CreateNomenclatureGroup(Core.Constants.CountryCodes.SouthAfrica, "10", ZDateTime.Today.AddDays(-1), ZDateTime.MaxSmallDateTime, "Description 01", "10", nomenclatureGroupType: tariffType);
			var group2 = helper.CreateNomenclatureGroup(Core.Constants.CountryCodes.SouthAfrica, "20", ZDateTime.Today.AddDays(-1), ZDateTime.MaxSmallDateTime, "Description 02", "20", nomenclatureGroupType: tariffType);
			Factory.Save();
			var language1 = Factory.New<RefCusNomenclatureLanguage>();
			language1.ZX8_ZZ5_NomenclatureGroup = group1.PK;
			language1.ZX8_ZX6_NKLanguage = "ENG";
			language1.ZX8_Description = "Test Decription 01";
			var language2 = Factory.New<RefCusNomenclatureLanguage>();
			language2.ZX8_ZZ5_NomenclatureGroup = group1.PK;
			language2.ZX8_ZX6_NKLanguage = "THA";
			language2.ZX8_Description = @"รายละเอียดการทดสอบ 01";
			var language3 = Factory.New<RefCusNomenclatureLanguage>();
			language3.ZX8_ZZ5_NomenclatureGroup = group2.PK;
			language3.ZX8_ZX6_NKLanguage = "ENG";
			language3.ZX8_Description = "Test Decription 03";
			var language4 = Factory.New<RefCusNomenclatureLanguage>();
			language4.ZX8_ZZ5_NomenclatureGroup = group2.PK;
			language4.ZX8_ZX6_NKLanguage = "THA";
			language4.ZX8_Description = @"รายละเอียดการทดสอบ 02";
			Factory.Save();
			var values = new[] { group1.ZZ5_Value, group2.ZZ5_Value };
			var notAvailable = new ZString[] { "Not Available", "Not Available" };
			var thaiDescriptions = new[] { language2.ZX8_Description, language4.ZX8_Description };
			var englishDescriptions = new[] { language1.ZX8_Description, language3.ZX8_Description };
			AssertAlternateLanguageDescriptionWithLanguage("CS-CZ", values, notAvailable);
			AssertAlternateLanguageDescriptionWithLanguage("TH-TH", values, thaiDescriptions);
			AssertAlternateLanguageDescriptionWithLanguage("EN", values, englishDescriptions);
		}

		void AssertAlternateLanguageDescriptionWithLanguage(string language, ZString[] values, ZString[] expectedDescriptions)
		{
			var origianlLanguage = GlbStaff.CurrentUser.GS_WorkingLanguage;
			try
			{
				var factory = new BusinessObjectFactory();
				GlbStaff.CurrentUser[GlbStaffSchema.GS_WorkingLanguage] = language;
				foreach (var value in values)
				{
					var refCusNomenclatureGroup = GetRefCusNomenclatureGroup(factory, value, Core.Constants.CountryCodes.SouthAfrica, "1P1", ZDateTime.Today);
					var description = refCusNomenclatureGroup.ZZ5_AlternateLanguageDescription;
					AssertCollectionContains(description, expectedDescriptions);
				}
			}
			finally
			{
				GlbStaff.CurrentUser[GlbStaffSchema.GS_WorkingLanguage] = origianlLanguage;
			}
		}

		public void TestFetchHint()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateOrGetLanguage("ENG", "English");
			helper.CreateOrGetLanguage("CHS", "Chinese");
			var group1 = helper.CreateNomenclatureGroup(Core.Constants.CountryCodes.SouthAfrica, "10", ZDateTime.Today, ZDateTime.MaxSmallDateTime, "Description 01", "10", nomenclatureGroupType: "2X2");
			var group2 = helper.CreateNomenclatureGroup(Core.Constants.CountryCodes.SouthAfrica, "20", ZDateTime.Today, ZDateTime.MaxSmallDateTime, "Decription 02", "20", nomenclatureGroupType: "3E3");
			Factory.Save();
			var language1 = Factory.New<RefCusNomenclatureLanguage>();
			language1.ZX8_ZZ5_NomenclatureGroup = group1.PK;
			language1.ZX8_ZX6_NKLanguage = "ENG";
			language1.ZX8_Description = "Test Decription 01";
			var language2 = Factory.New<RefCusNomenclatureLanguage>();
			language2.ZX8_ZZ5_NomenclatureGroup = group2.PK;
			language2.ZX8_ZX6_NKLanguage = "ENG";
			language2.ZX8_Description = "Test Decription 02";
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			AssertCollectionNotContains(RefCusNomenclatureLanguageSchema.Constants.TableName, newFactory.GetAllFetchHintedTableNames());
			var query = new ZQuery(RefCusNomenclatureGroupSchema.PK, new[] { group1.PK, group2.PK });
			var groups = newFactory.Load<RefCusNomenclatureGroup>(query);
			AssertEquals(2, groups.Length);
			AssertCollectionContains(RefCusNomenclatureLanguageSchema.Constants.TableName, newFactory.GetAllFetchHintedTableNames());
			var descriptionsForLoad = groups.Select(c => c.ZZ5_Description).ToArray();
			//RefDatabase_RefCusNomenclatureGroup: 1
			//RefDatabase_RefCusNomenclatureLanguage: 1
			//Hits: 2 / 0
			AssertMaxDbHits("Should reduce to 2 by the fetch hint.", 2, newFactory);
		}

		RefCusNomenclatureGroup GetRefCusNomenclatureGroup(BusinessObjectFactory factory, ZString value, ZString dataGroupingCode, ZString type, ZDateTime date)
		{
			var query = new ZQuery(RefDataGrouping.GetQueryIncludeParentDataGrouping(factory, RefCusNomenclatureGroupSchema.ZZ5_ZZZ_NKDataGrouping, dataGroupingCode));
			query.AddToFilter(RefCusNomenclatureGroupSchema.ZZ5_Value, value);
			query.AddToFilter(RefCusNomenclatureGroupSchema.ZZ5_ZZ9_NKNomenclatureGroupType, type);
			query.AddToFilter(RefCusNomenclatureGroupSchema.ZZ5_StartDate, SQLComparisonOperator.LessThanOrEqualTo, date);
			query.AddToFilter(RefCusNomenclatureGroupSchema.ZZ5_EndDate, SQLComparisonOperator.GreaterThanOrEqualTo, date);
			return factory.LoadTop1<RefCusNomenclatureGroup>(query);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			return helper.CreateNomenclatureGroup(Core.Constants.CountryCodes.SouthAfrica, "9", ZDateTime.Today, ZDateTime.MaxSmallDateTime, "Test Description", "99");
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject();
		}
	}
}
