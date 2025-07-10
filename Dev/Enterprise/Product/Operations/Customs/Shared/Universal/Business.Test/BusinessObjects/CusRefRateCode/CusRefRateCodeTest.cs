using System.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(CusRefRateCode))]
	class CusRefRateCodeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCheckDuplication()
		{
			var existingObj1 = Factory.NewWithValidTestData<CusRefRateCode>();
			existingObj1.CR7_RateCode = "CD1";
			existingObj1.CR7_RateType = "OTH";
			existingObj1.CR7_RN_NKCountryCode = "ER";
			Factory.Save();
			var testItem = Factory.NewWithValidTestData<CusRefRateCode>();
			testItem.CR7_RateCode = "CD1";
			testItem.CR7_RN_NKCountryCode = "ER";
			testItem.RunPreSaveValidation();
			AssertHasRowError(testItem, "The combination of Rate Code and Country/Region should be unique.");
			existingObj1.CR7_RateType = "OTH";
			testItem.RunPreSaveValidation();
			AssertHasRowError(testItem, "The combination of Rate Code and Country/Region should be unique.");
			testItem.CR7_RateCode = "CD2";
			testItem.RunPreSaveValidation();
			AssertNoRowErrors(testItem);
			testItem.CR7_RateCode = "CD1";
			testItem.CR7_RN_NKCountryCode = "CN";
			testItem.RunPreSaveValidation();
			AssertNoRowErrors(testItem);
		}

		public void TestCountryIsReadOnly()
		{
			AssertHasCustomAttribute<ReadOnlyAttribute>(typeof(CusRefRateCode), nameof(CusRefRateCode.CR7_RN_NKCountryCode), true, readOnlyAtt => readOnlyAtt.IsReadOnly);
		}

		public void TestDefaultValues()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var rateCode = Factory.New<CusRefRateCode>();
				AssertEquals(Core.Constants.CountryCodes.Australia, rateCode.CR7_RN_NKCountryCode);
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var result = Factory.NewWithValidTestData<CusRefRateCode>();
			result.CR7_RateType = "OTH";
			return result;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		protected override void SetUp()
		{
			base.SetUp();
			universalHelper = new UniversalReferenceTestDataHelper(Factory);
			universalHelper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Eritrea);
			universalHelper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.China);
			Factory.Save();
		}

		UniversalReferenceTestDataHelper universalHelper;
	}
}
