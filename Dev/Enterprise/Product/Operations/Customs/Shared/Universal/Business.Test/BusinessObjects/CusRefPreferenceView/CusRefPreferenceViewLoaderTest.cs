using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Universal.Internal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(CusRefPreferenceView.Loader))]
	public class CusRefPreferenceViewLoaderTest : LoaderTestCase
	{
		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new CusRefPreferenceView.Loader(Factory);
		}

		public void TestLoadByPreference()
		{
			var eunId = RefDataHelper.CreateNewOrGetExistingDataGrouping("EUN");
			RefDataHelper.CreateNewOrGetExistingDataGrouping("GB", parent: eunId);
			RefDataHelper.CreateNewOrGetExistingDataGrouping("AU");
			var cusPref1 = Factory.NewWithValidTestData<CusRefPreferenceView>();
			cusPref1.ZZS_Preference = "140";
			cusPref1.ZZS_Description = "Exemption for End-Use Resulting from the CCT";
			cusPref1.ZZS_ZZZ_NKDataGrouping = "GB";
			cusPref1.ZZS_DataSet = Core.Constants.Customs.Universal.DataSetTypes.OWNData;
			var cusPref2 = Factory.NewWithValidTestData<CusRefPreferenceView>();
			cusPref2.ZZS_Preference = "200";
			cusPref2.ZZS_Description = "GSP Rate Without Conditions Or Limits (Including Ceilings)";
			cusPref2.ZZS_ZZZ_NKDataGrouping = "GB";
			cusPref2.ZZS_DataSet = Core.Constants.Customs.Universal.DataSetTypes.OWNData;
			var cusPref3 = Factory.NewWithValidTestData<CusRefPreferenceView>();
			cusPref3.ZZS_Preference = "200";
			cusPref3.ZZS_Description = "200 Description";
			cusPref3.ZZS_ZZZ_NKDataGrouping = "AU";
			cusPref3.ZZS_DataSet = Core.Constants.Customs.Universal.DataSetTypes.OWNData;
			var zzPref1 = Factory.NewWithValidTestData<RefCusPreference>();
			zzPref1.ZZS_Preference = "140";
			zzPref1.ZZS_Description = "Exemption for End-Use Resulting from the CCT";
			zzPref1.ZZS_ZZZ_NKDataGrouping = "EUN";
			Factory.Save();
			AssertEquals(cusPref1.PK, CusRefPreferenceView.Loader.LoadByPreference(Factory, "GB", "140", Core.Constants.Customs.Universal.DataSetTypes.OWNData).PK);
			AssertEquals(cusPref2.PK, CusRefPreferenceView.Loader.LoadByPreference(Factory, "GB", "200", Core.Constants.Customs.Universal.DataSetTypes.OWNData).PK);
			AssertEquals(cusPref3.PK, CusRefPreferenceView.Loader.LoadByPreference(Factory, "AU", "200", Core.Constants.Customs.Universal.DataSetTypes.OWNData).PK);
			AssertEquals(zzPref1.PK, CusRefPreferenceView.Loader.LoadByPreference(Factory, "GB", "140", Core.Constants.Customs.Universal.DataSetTypes.WTGData).PK);
		}

		public void TestGetList()
		{
			// Populate database with code pairs
			// Verify all codes are returned in a CodeDescriptionPairList
			var eunId = RefDataHelper.CreateNewOrGetExistingDataGrouping("EUN");
			var wcoId = RefDataHelper.CreateNewOrGetExistingDataGrouping("WCO");
			RefDataHelper.CreateNewOrGetExistingDataGrouping("GB", parent: eunId);
			RefDataHelper.CreateNewOrGetExistingDataGrouping("IT", parent: eunId);
			RefDataHelper.CreateNewOrGetExistingDataGrouping("JP", parent: wcoId);
			var cusPref1 = Factory.NewWithValidTestData<RefCusPreference>();
			cusPref1.ZZS_Preference = "140";
			cusPref1.ZZS_Description = "Exemption for End-Use Resulting from the CCT";
			cusPref1.ZZS_ZZZ_NKDataGrouping = "EUN";
			var cusPref2 = Factory.NewWithValidTestData<RefCusPreference>();
			cusPref2.ZZS_Preference = "200";
			cusPref2.ZZS_Description = "GSP Rate Without Conditions Or Limits (Including Ceilings)";
			cusPref2.ZZS_ZZZ_NKDataGrouping = "EUN";
			var cusPref3 = Factory.NewWithValidTestData<RefCusPreference>();
			cusPref3.ZZS_Preference = "123";
			cusPref3.ZZS_Description = "123Description";
			cusPref3.ZZS_ZZZ_NKDataGrouping = "WCO";
			Factory.Save();
			var testList = CusRefPreferenceView.Loader.GetList(Factory, "GB");
			AssertEquals(2, testList.Count);
			AssertEquals(true, testList.ContainsCode("140"));
			AssertEquals(true, testList.ContainsCode("200"));
			testList = CusRefPreferenceView.Loader.GetList(Factory, "IT");
			AssertEquals(2, testList.Count);
			AssertEquals(true, testList.ContainsCode("140"));
			AssertEquals(true, testList.ContainsCode("200"));
			testList = CusRefPreferenceView.Loader.GetList(Factory, "JP");
			AssertEquals(1, testList.Count);
			AssertEquals(true, testList.ContainsCode("123"));
		}

		public void TestGetListWithDifferentLanguage()
		{
			var eunDataGrouping = RefDataHelper.CreateNewOrGetExistingDataGrouping("EUN");
			RefDataHelper.CreateNewOrGetExistingDataGrouping("GB", parent: eunDataGrouping);
			var wcoDataGrouping = RefDataHelper.CreateNewOrGetExistingDataGrouping("WCO");
			RefDataHelper.CreateNewOrGetExistingDataGrouping("TH", parent: wcoDataGrouping);
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateOrGetLanguage("ENG", "English");
			helper.CreateOrGetLanguage("THA", "Thai");
			helper.CreateOrGetLanguage("CZE", "Czech");
			Factory.Save();

			var pref1 = Factory.New<RefCusPreference>();
			pref1.ZZS_Preference = "100";
			pref1.ZZS_Description = "Original Description 001";
			pref1.ZZS_ZZZ_NKDataGrouping = "EUN";
			var language1 = Factory.New<RefCusPreferenceLanguage>();
			language1.ZX9_ZZS_Preference = pref1.PK;
			language1.ZX9_ZX6_NKLanguage = "ENG";
			language1.ZX9_Description = "Test Description 001";
			var language2 = Factory.New<RefCusPreferenceLanguage>();
			language2.ZX9_ZZS_Preference = pref1.PK;
			language2.ZX9_ZX6_NKLanguage = "THA";
			language2.ZX9_Description = @"รายละเอียดการทดสอบ001";
			var pref2 = Factory.New<RefCusPreference>();
			pref2.ZZS_Preference = "200";
			pref2.ZZS_Description = "Original Description 002";
			pref2.ZZS_ZZZ_NKDataGrouping = "EUN";
			var language3 = Factory.New<RefCusPreferenceLanguage>();
			language3.ZX9_ZZS_Preference = pref2.PK;
			language3.ZX9_ZX6_NKLanguage = "ENG";
			language3.ZX9_Description = "Test Description 002";
			var language4 = Factory.New<RefCusPreferenceLanguage>();
			language4.ZX9_ZZS_Preference = pref2.PK;
			language4.ZX9_ZX6_NKLanguage = "THA";
			language4.ZX9_Description = @"รายละเอียดการทดสอบ002";
			Factory.Save();

			var originalDescriptions = new string[] { pref1.ZZS_Description, pref2.ZZS_Description };
			var thaiDescriptions = new string[] { language2.ZX9_Description, language4.ZX9_Description };
			var englishDescriptions = new string[] { language1.ZX9_Description, language3.ZX9_Description };
			var emptyDescriptions = Array.Empty<string>();
			AssertCorrectDescriptionsWithLanguage("GB", "CS-CZ", originalDescriptions);
			AssertCorrectDescriptionsWithLanguage("GB", "TH-TH", thaiDescriptions);
			AssertCorrectDescriptionsWithLanguage("GB", "EN", englishDescriptions);
			AssertCorrectDescriptionsWithLanguage("TH", "EN", emptyDescriptions);
			AssertCorrectDescriptionsWithLanguage("TH", "TH-TH", emptyDescriptions);
			AssertCorrectDescriptionsWithLanguage("TH", "CS-CZ", emptyDescriptions);
		}

		void AssertCorrectDescriptionsWithLanguage(string countryCode, string language, string[] expectedDescriptions)
		{
			var origianlLanguage = GlbStaff.CurrentUser.GS_WorkingLanguage;
			try
			{
				var factory = new BusinessObjectFactory();
				GlbStaff.CurrentUser[GlbStaffSchema.GS_WorkingLanguage] = language;
				var list = CusRefPreferenceView.Loader.GetList(factory, countryCode);
				var expectedDataCount = expectedDescriptions.Length;
				AssertEquals(expectedDataCount, list.Count);
				if (expectedDataCount > 0)
				{
					var descriptions = list.ToArray().Select(c => c.Description);
					AssertContainsExactElementsInAnyOrder(expectedDescriptions, descriptions);
				}
			}
			finally
			{
				GlbStaff.CurrentUser[GlbStaffSchema.GS_WorkingLanguage] = origianlLanguage;
			}
		}

		UniversalReferenceTestDataHelper RefDataHelper => refDataHelper ?? (refDataHelper = new UniversalReferenceTestDataHelper(Factory));
		UniversalReferenceTestDataHelper refDataHelper;
	}
}
