using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(UNDGCountryReference))]
	sealed class UNDGCountryReferenceTest : EnterpriseBusinessObjectTestCase
	{
		public void TestCanDetach_Security()
		{
			var countryReference = Factory.New<UNDGCountryReference>();
			countryReference.DCR_RN_NKCountry = Core.Constants.CountryCodes.Australia;
			countryReference.DCR_Type = Core.Constants.UNDGCountryReference.Type.PSA;
			CombineAssertions(() =>
			{
				Env.Security.UNDGSubstanceCountryReferenceAttachDetach.IsAllowed = false;
				AssertEquals("Security Not Allowed", false, countryReference.CanDetach);
				Env.Security.UNDGSubstanceCountryReferenceAttachDetach.IsAllowed = true;
				AssertEquals("Security Allowed", true, countryReference.CanDetach);
			});
		}

		public void TestCanDetach_SystemCountryReference()
		{
			Env.Security.UNDGSubstanceCountryReferenceAttachDetach.IsAllowed = true;
			var countryReference = Factory.New<UNDGCountryReference>();
			countryReference.DCR_RN_NKCountry = Core.Constants.CountryCodes.Singapore;
			countryReference.DCR_Type = Core.Constants.UNDGCountryReference.Type.PSA;
			CombineAssertions(() =>
			{
				AssertEquals("Is System Country Reference", false, countryReference.CanDetach);
				countryReference.DCR_RN_NKCountry = Core.Constants.CountryCodes.Australia;
				AssertEquals("Country not Singapore", true, countryReference.CanDetach);
				countryReference.DCR_RN_NKCountry = Core.Constants.CountryCodes.Singapore;
				countryReference.DCR_Type = Core.Constants.UNDGCountryReference.Type.ICPE;
				AssertEquals("Type not PSA", true, countryReference.CanDetach);
			});
		}
	}
}
