using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Integration.Customs.TW;

namespace Enterprise.Customs.TW.Manifest.Business
{
	public class TWGlbCompanyWrapper : GlbCompanyWrapper, ITWGlbCompanyWrapper
	{
		public TWGlbCompanyWrapper(GlbCompany company)
			: base(company)
		{
		}

		public override bool IsValidWrapper => true;

		[ChildEditable]
		public GlbCompanyCredential ForwarderCertificate
		{
			get
			{
				if (forwarderCertificate == null)
				{
					forwarderCertificate = GetGlbExternalPasswordOrCreateNew<GlbCompanyCredential>(PasswordTypesList.Codes.TVF);
					RegisterEditableChildObject(forwarderCertificate);
				}

				return forwarderCertificate;
			}
		}
		GlbCompanyCredential forwarderCertificate;

		IGlbExternalPassword ITWGlbCompanyWrapper.ForwarderCertificate => ForwarderCertificate;

		[ChildEditable]
		public GlbCompanyLicensingCredential LicensingCertificate
		{
			get
			{
				if (licensingCertificate == null)
				{
					licensingCertificate = GetGlbExternalPasswordOrCreateNew<GlbCompanyLicensingCredential>(PasswordTypesList.Codes.NXM);
					RegisterEditableChildObject(licensingCertificate);
				}

				return licensingCertificate;
			}
		}
		GlbCompanyLicensingCredential licensingCertificate;

		IGlbExternalPassword ITWGlbCompanyWrapper.LicensingCertificate => LicensingCertificate;
	}
}
