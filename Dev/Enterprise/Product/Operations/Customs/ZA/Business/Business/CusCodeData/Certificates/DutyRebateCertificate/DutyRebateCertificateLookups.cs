using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business
{
	public class DutyRebateCertificateLookups : CertificateLookups
	{
		public DutyRebateCertificateLookups(DutyRebateCertificate parent)
			: base(parent)
		{
		}

		protected override ZString[] PermitTypes => new ZString[] { PermitTypeList.Codes.PRC };
	}
}
