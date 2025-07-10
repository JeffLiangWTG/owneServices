using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(CusRefPreference))]
	public class CusRefPreferenceTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCR8_RN_NKCountryCode()
		{
			var preference = Factory.New<CusRefPreference>();
			AssertEquals("Country code should be defaulted to logged in country.", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, preference.CR8_RN_NKCountryCode);
			var info = preference.CR8_RN_NKCountryCodeInfo;
			AssertEquals(2, info.MaxLength);
			AssertEquals("Country code should be read only.", true, info.ReadOnly);
		}

		public void TestCR8_Description()
		{
			var info = ((CusRefPreference)GetNewBusinessObject()).CR8_DescriptionInfo;
			AssertEquals(500, info.MaxLength);
		}

		public void TestCR8_Preference()
		{
			var preference = Factory.New<CusRefPreference>();
			AssertEquals("Preference should be defaulted to empty.", ZString.Empty, preference.CR8_Preference);
			var info = ((CusRefPreference)GetNewBusinessObject()).CR8_PreferenceInfo;
			AssertEquals(8, info.MaxLength);
		}

		public void TestCR8_Preference_CaptionShouldBePreference()
		{
			AssertEquals("Preference", DataBoundResourceStrings.GetDataForProperty(Factory.New<CusRefPreference>().CR8_PreferenceInfo).Caption);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var preference = Factory.New<CusRefPreference>();
			preference.CR8_RN_NKCountryCode = "ZA";
			preference.CR8_Preference = "Pre";
			preference.CR8_Description = "Pre Description";
			return preference;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}
	}
}
