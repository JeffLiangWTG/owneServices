using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(TariffAdditionalCodeView))]
	class TariffAdditionalCodeViewTest : EnterpriseBusinessObjectTestCase
	{
		public void TestZY2_CategoryDescription()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "T1T");
			Factory.Save();
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffType.PK, "DUMMYTRF01", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06));
			var tariffAdditionalCode = helper.CreateNewOrGetExistingTariffAdditionalCodeView(tariff, "CT1", "T1T");
			AssertEquals("ZY2_CategoryDescription", "CT1 DESC", tariffAdditionalCode.ZY2_CategoryDescription);
		}

		public void TestITariffDataGroupingRelatedBusinessObjectMembers()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "T1T");
			Factory.Save();
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffType.PK, "DUMMYTRF01", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06));
			ITariffDataGroupingRelatedBusinessObject tariffAdditionalCode = helper.CreateNewOrGetExistingTariffAdditionalCodeView(tariff, "CT1", "T1T");
			AssertEquals("DataGrouping", Core.Constants.CountryCodes.Eritrea, tariffAdditionalCode.DataGrouping);
		}

		public void TestITranslatableZZBusinessObjectMembers()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "T1T");
			Factory.Save();
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffType.PK, "DUMMYTRF01", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06));
			ITranslatableZZBusinessObject tariffAdditionalCode = helper.CreateNewOrGetExistingTariffAdditionalCodeView(tariff, "CT1", "T1T");
			AssertEquals(RefCusTariffAdditionalCodeLanguageSchema.Instance, tariffAdditionalCode.LanguageTableSchema);
			AssertEquals(typeof(RefCusTariffAdditionalCodeLanguage), tariffAdditionalCode.LanguageTableType);
		}

		public void TestDataGrouping()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dataGrouping1 = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Eritrea, "ER TEST");
			var dataGrouping2 = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Denmark, "ER TEST");
			Factory.Save();
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "T1T");
			Factory.Save();
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffType.PK, "DUMMYTRF01", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06));
			var tariffAdditionalCode = helper.CreateNewOrGetExistingTariffAdditionalCodeView(tariff, "CT1", "T1T");
			AssertSame(dataGrouping1, tariffAdditionalCode.DataGrouping);
			tariffAdditionalCode.ZY2_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.Denmark;
			AssertSame(dataGrouping2, tariffAdditionalCode.DataGrouping);
		}

		public void TestCusTariff()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "T1T");
			Factory.Save();
			var cusTariff1 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Eritrea, tariffType.PK, "DUMMYTRF1", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 1");
			var cusTariff2 = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Eritrea, tariffType.PK, "DUMMYTRF2", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), "dummy Description 2");
			var tariffAdditionalCode = helper.CreateNewOrGetExistingTariffAdditionalCodeView(cusTariff1, "CT1", "T1T");
			AssertSame(cusTariff1, tariffAdditionalCode.CusTariff);
			tariffAdditionalCode.ZY2_ZZ1_ParentTariffOrNationalCode = cusTariff2.PK;
			AssertSame(cusTariff2, tariffAdditionalCode.CusTariff);
		}

		public void TestCategory()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Eritrea);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.DemocraticRepublicOfCongo);
			Factory.Save();
			var tariffType = UniversalReferenceTestDataHelper.CreateTariffType(Factory, Core.Constants.CountryCodes.Eritrea, "T1T", ensureDataGroupingExists: false);
			var category1 = UniversalReferenceTestDataHelper.CreateCusTariffAdditionalCodeCategory(Factory, Core.Constants.CountryCodes.Eritrea, "CT1");
			var category2 = UniversalReferenceTestDataHelper.CreateCusTariffAdditionalCodeCategory(Factory, Core.Constants.CountryCodes.Eritrea, "CT2");
			var category3 = UniversalReferenceTestDataHelper.CreateCusTariffAdditionalCodeCategory(Factory, Core.Constants.CountryCodes.DemocraticRepublicOfCongo, "CT2");
			Factory.Save();
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffType.PK, "DUMMYTRF01", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06), ensureDataGroupingExists: false);
			Factory.Save();
			var tariffAdditionalCode = helper.CreateTariffAdditionalCodeView(tariff, "CT1", "T1T", ensureDataGroupingExists: false, ensureCategoryExists: false);
			AssertSame(category1, tariffAdditionalCode.Category);
			tariffAdditionalCode.ZY2_ZY3_NKCategory = "CT2";
			AssertSame(category2, tariffAdditionalCode.Category);
			tariffAdditionalCode.ZY2_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.DemocraticRepublicOfCongo;
			AssertSame(category3, tariffAdditionalCode.Category);
		}

		public void TestFetchHint()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "T1T");
			helper.CreateOrGetLanguage("ENG", "English");
			Factory.Save();
			var tariff1 = helper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffType.PK, "DUMMYTRF01", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06));
			var tariff2 = helper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffType.PK, "DUMMYTRF02", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06));
			var tariffAdditionalCode1 = helper.CreateNewOrGetExistingTariffAdditionalCodeView(tariff1, "CT1", "T1T");
			var tariffAdditionalCode2 = helper.CreateNewOrGetExistingTariffAdditionalCodeView(tariff2, "CT1", "T1T");
			Factory.Save();
			var language1 = Factory.New<RefCusTariffAdditionalCodeLanguage>();
			language1.ZY4_ZY2_TariffAdditionalCode = tariffAdditionalCode1.PK;
			language1.ZY4_Description = "Test Description 001";
			language1.ZY4_ZX6_NKLanguage = "ENG";
			var language2 = Factory.New<RefCusTariffAdditionalCodeLanguage>();
			language2.ZY4_ZY2_TariffAdditionalCode = tariffAdditionalCode2.PK;
			language2.ZY4_Description = "Test Description 002";
			language2.ZY4_ZX6_NKLanguage = "ENG";
			Factory.Save();
			var newFactory = NewFactory();
			AssertCollectionNotContains(RefCusTariffAdditionalCodeLanguageSchema.Constants.TableName, newFactory.GetAllFetchHintedTableNames());
			var query = new ZQuery(TariffAdditionalCodeViewSchema.ZY2_AdditionalCode, "T1T");
			var tariffAdditionalCodeViews = newFactory.Load<TariffAdditionalCodeView>(query);
			AssertEquals(2, tariffAdditionalCodeViews.Length);
			AssertCollectionContains(RefCusTariffAdditionalCodeLanguageSchema.Constants.TableName, newFactory.GetAllFetchHintedTableNames());
			var descriptionsForLoad = tariffAdditionalCodeViews.Select(c => c.ZY2_Description).ToArray();
			//RefDatabase_RefCusTariffAdditionalCodeLanguage: 1
			//RefDatabase_TariffAdditionalCodeView: 1
			//Hits: 2 / 0
			AssertMaxDbHits("Should reduce to 2 by the fetch hint.", 2, newFactory);
		}

		public void TestTranslationDescription()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "T1T");
			helper.CreateOrGetLanguage("ENG", "English");
			helper.CreateOrGetLanguage("CHS", "Chinese");
			Factory.Save();
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffType.PK, "DUMMYTRF01", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06));
			var tariffAdditionalCode = helper.CreateNewOrGetExistingTariffAdditionalCodeView(tariff, "CT1", "T1T");
			var language1 = Factory.New<RefCusTariffAdditionalCodeLanguage>();
			language1.ZY4_ZY2_TariffAdditionalCode = tariffAdditionalCode.PK;
			language1.ZY4_Description = "Test Description";
			language1.ZY4_ZX6_NKLanguage = "ENG";
			var language2 = Factory.New<RefCusTariffAdditionalCodeLanguage>();
			language2.ZY4_ZY2_TariffAdditionalCode = tariffAdditionalCode.PK;
			language2.ZY4_Description = @"测试描述";
			language2.ZY4_ZX6_NKLanguage = "CHS";
			Factory.Save();
			AssertTranslationDescription(tariffAdditionalCode.PK, "ENG", language1.ZY4_Description);
			AssertTranslationDescription(tariffAdditionalCode.PK, "CHS", language2.ZY4_Description);
		}

		public override void TestCallsBaseSetDefaultValues()
		{
			Assert("View don't need defaulting", true);
		}

		void AssertTranslationDescription(ZGuid pk, string language, string expectedDescription)
		{
			var orginalLanguage = GlbStaff.CurrentUser[GlbStaffSchema.GS_WorkingLanguage];
			try
			{
				GlbStaff.CurrentUser[GlbStaffSchema.GS_WorkingLanguage] = language;
				var newFactory = NewFactory();
				var tariffAdditionalCodeView = (CargoWise.Integration.ICodeDescription)newFactory.Load<TariffAdditionalCodeView>(pk);
				AssertEquals(expectedDescription, tariffAdditionalCodeView.Description);
			}
			finally
			{
				GlbStaff.CurrentUser[GlbStaffSchema.GS_WorkingLanguage] = orginalLanguage;
			}
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var helper = new UniversalReferenceTestDataHelper(factory);
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "T1T");
			factory.Save();
			var tariff = helper.LoadOrCreateNewTariff(Core.Constants.CountryCodes.Eritrea, tariffType.PK, "DUMMYTRF01", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06));
			return helper.CreateNewOrGetExistingTariffAdditionalCodeView(tariff, "CT1", "T1T");
		}

		public void TestApplicabilities()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var dataGrouping1 = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Eritrea, "ER TEST");
			Factory.Save();
			var tariffType = helper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Eritrea, "T1T");
			Factory.Save();
			var tariff = helper.CreateTariff(Core.Constants.CountryCodes.Eritrea, tariffType.PK, "DUMMYTRF01", new ZDateTime(2010, 12, 10), new ZDateTime(2079, 06, 06));
			var tariffAdditionalCode = helper.CreateNewOrGetExistingTariffAdditionalCodeView(tariff, "CT1", "T1T");
			AssertType<FilteredCusRefApplicabilityViewCollection>(tariffAdditionalCode.FilteredApplicabilities);
			AssertType<CusRefApplicabilityViewCollection>(tariffAdditionalCode.Applicabilities);
		}
	}
}
