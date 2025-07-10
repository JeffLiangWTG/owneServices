using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Recruiter.Business.Testing
{
	public sealed class GlbAccreditationGroupTestHelper
	{
		public GlbAccreditationGroupTestHelper(BusinessObjectFactory factory)
		{
			Factory = factory;
		}

		public BusinessObjectFactory Factory;
		public GlbAccreditation Accreditation1;
		public GlbAccreditation Accreditation1Refresher;
		public GlbAccreditation Accreditation2;
		public GlbAccreditation Accreditation3;
		public GlbAccreditationGroup AccreditationGroup1;
		public GlbAccreditationGroup AccreditationGroup2;

		public void SetupAccreditations()
		{
			var certCodes = RecruiterDataRegistry.Instance.CertificateTypesExtra.Value;
			certCodes.Add("C01", (NoResString)"C01");
			RecruiterDataRegistry.Instance.CertificateTypesExtra.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, certCodes);

			Accreditation1 = Factory.NewWithValidTestData<GlbAccreditation>();
			Accreditation1Refresher = Factory.NewWithValidTestData<GlbAccreditation>();
			Accreditation2 = Factory.NewWithValidTestData<GlbAccreditation>();
			Accreditation3 = Factory.NewWithValidTestData<GlbAccreditation>();

			Accreditation1.HAC_CertificateCode = "C01";
			Accreditation1Refresher.HAC_CertificateCode = "C01";
			Accreditation1Refresher.HAC_IsRefresher = true;
			Accreditation1Refresher.HAC_RefresherCertificateExpiryType = RefresherCertExpirationTypes.Codes.EndOfLastAttemptPlusValidityPeriodDefault;

			AccreditationGroup1 = Factory.NewWithValidTestData<GlbAccreditationGroup>();
			AccreditationGroup2 = Factory.NewWithValidTestData<GlbAccreditationGroup>();

			AccreditationGroup1.Accreditations.Add(Accreditation1);
			AccreditationGroup2.Accreditations.Add(Accreditation2);
			AccreditationGroup1.Accreditations.Add(Accreditation3);
			AccreditationGroup2.Accreditations.Add(Accreditation3);
		}
	}
}
