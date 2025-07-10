using CargoWise.EntityFramework.Testing;

namespace Enterprise.Recruiter.Business.Testing
{
	sealed class GlbAccreditationGroupLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestMainAccreditationList()
		{
			var accreditation = Factory.NewWithValidTestData<GlbAccreditation>();
			accreditation.HAC_Code = "BBB";
			accreditation.HAC_CertificateCode = "BBC";
			var accreditationRefresher = Factory.NewWithValidTestData<GlbAccreditation>();
			accreditationRefresher.HAC_Code = "RBB";
			accreditationRefresher.HAC_CertificateCode = "BBC";
			accreditationRefresher.HAC_IsRefresher = true;
			accreditationRefresher.HAC_RefresherCertificateExpiryType = RefresherCertExpirationTypes.Codes.EndOfLastAttemptPlusValidityPeriodDefault;
			Factory.Save();

			var accreditationGroup = Factory.NewWithValidTestData<GlbAccreditationGroup>();
			var lookup = new GlbAccreditationGroupLookups(accreditationGroup);
			AssertEquals("Should contain main accreditation", true, lookup.MainAccreditationList.Contains(accreditation));
			AssertEquals("Should not contain refresher accreditation", false, lookup.MainAccreditationList.Contains(accreditationRefresher));
		}
	}
}
