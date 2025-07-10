using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business
{
	public class RCCCertificateValidation : CertificateValidation
	{
		public RCCCertificateValidation(RCCCertificate parent)
			: base(parent)
		{
		}

		protected override ZString[] ExpectedPermitTypes => new ZString[] { PermitTypeList.Codes.RCC, PermitTypeList.Codes.VALA };
	}
}
