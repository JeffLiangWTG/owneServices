using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(CusRefRateCodeView))]
	public class CusRefRateCodeViewTest : EnterpriseBusinessObjectTestCase
	{
		public void TestZY1_DataSet()
		{
			var rateCode = Factory.New<CusRefRateCodeView>();
			AssertEquals(Core.Constants.Customs.Universal.DataSetTypes.OWNData, rateCode.ZY1_DataSet);
			Assert(rateCode.ZY1_DataSetInfo.ReadOnly);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return CusRefRateCodeViewTestHelper.CreateAndSaveCusRateCodeForCountryIfNotExists(factory, Core.Constants.CountryCodes.SouthAfrica, "ADD", "ADD", false);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForDeleteTest(Factory);

		public void TestDescriptionNotTranslated() => CombineAssertions(() =>
		{
			var testHelper = new UniversalReferenceTestDataHelper(Factory);
			testHelper.CreateOrGetLanguage("FR", "French");
			testHelper.CreateNewOrGetExistingDataGrouping(GlbCompany.CurrentCompany.Country.Code);
			var rateType = testHelper.CreateNewOrGetExistingRateType(GlbCompany.CurrentCompany.Country.Code, "T1");
			var rateCode = testHelper.CreateCusRateCode(Factory, "R1", rateType.PK, description: "R1 Default");
			var rateCodeLanguage = testHelper.LoadOrCreateNewCusRateCodeLanguage(Factory, rateCode, "FR");
			rateCodeLanguage.ZXC_Description = "R1 French";
			Factory.Save();

			GlbStaff.CurrentUser.GS_WorkingLanguage = "FR";
			rateCode = new BusinessObjectFactory().Load<CusRefRateCodeView>(rateCode.PK);
			AssertEquals("ZY1_Description", "R1 French", rateCode.ZY1_Description);
			AssertEquals("DescriptionNotTranslated", "R1 Default", rateCode.DescriptionNotTranslated);
		});
	}
}
