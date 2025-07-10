using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business
{
	public class DutyRebateCertificateValidation : CertificateValidation
	{
		public DutyRebateCertificateValidation(DutyRebateCertificate parent)
			: base(parent)
		{
		}

		protected override ZString[] ExpectedPermitTypes => new ZString[] { PermitTypeList.Codes.PRC };
	}
}
