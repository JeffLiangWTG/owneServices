using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business
{
	public class RCCCertificateLookups : CertificateLookups
	{
		public RCCCertificateLookups(RCCCertificate parent)
			: base(parent)
		{
		}

		protected override ZString[] PermitTypes => new ZString[] { PermitTypeList.Codes.RCC, PermitTypeList.Codes.VALA };
	}
}
