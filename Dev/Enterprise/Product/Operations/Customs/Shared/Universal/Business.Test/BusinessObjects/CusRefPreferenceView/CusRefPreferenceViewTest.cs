using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Universal.Internal;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(CusRefPreferenceView))]
	public class CusRefPreferenceViewTest : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var testItem = Factory.New<CusRefPreferenceView>();
			AssertEquals(Core.Constants.Customs.Universal.DataSetTypes.OWNData, testItem.ZZS_DataSet);
		}

		public void TestReadonlyAttributes()
		{
			var testItem = Factory.New<CusRefPreferenceView>();
			AssertEquals(true, testItem.ZZS_DataSetInfo.ReadOnly);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var view = helper.CreatePreferenceView("PR", "Test Preference", "CN", false);
			factory.Save();
			var result = factory.Load<CusRefPreferenceView>(view.PK);
			return result;
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var view = helper.CreatePreferenceView("PR", "Test Preference", "CN", false);
			Factory.Save();
			var result = Factory.Load<CusRefPreferenceView>(view.PK);
			return result;
		}

		public void TestFetchHint()
		{
			var cusPref1 = Factory.New<RefCusPreference>();
			cusPref1.ZZS_Preference = "140";
			cusPref1.ZZS_Description = "TEST A";
			cusPref1.ZZS_ZZZ_NKDataGrouping = "DE";
			var language1 = Factory.New<RefCusPreferenceLanguage>();
			language1.ZX9_Description = "TEST LANGUAGE 001";
			language1.ZX9_ZX6_NKLanguage = "CHS";
			language1.ZX9_ZZS_Preference = cusPref1.PK;
			var cusPref2 = Factory.New<RefCusPreference>();
			cusPref2.ZZS_Preference = "200";
			cusPref2.ZZS_Description = "TEST B";
			cusPref2.ZZS_ZZZ_NKDataGrouping = "DE";
			var language2 = Factory.New<RefCusPreferenceLanguage>();
			language2.ZX9_Description = "TEST LANGUAGE 002";
			language2.ZX9_ZX6_NKLanguage = "CHS";
			language2.ZX9_ZZS_Preference = cusPref2.PK;
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			AssertCollectionNotContains(RefCusPreferenceLanguageSchema.Constants.TableName, newFactory.GetAllFetchHintedTableNames());
			var query = new ZQuery(RefCusPreferenceSchema.PK, new[] { cusPref1.PK, cusPref2.PK });
			var preferences = newFactory.Load<CusRefPreferenceView>(query);
			AssertEquals(2, preferences.Length);
			AssertCollectionContains(RefCusPreferenceLanguageSchema.Constants.TableName, newFactory.GetAllFetchHintedTableNames());
			var descriptionsForLoad = preferences.Select(c => c.ZZS_Description).ToArray();
			//RefDatabase_RefCusPreference: 1
			//RefDatabase_RefCusPreferenceLanguage: 1
			//Hits: 2 / 0
			AssertMaxDbHits("Should reduce to 2 by the fetch hint.", 2, newFactory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping("DE");
			helper.CreateOrGetLanguage("ENG", "English");
			helper.CreateOrGetLanguage("CHS", "Chinese");
			Factory.Save();
		}
	}
}
