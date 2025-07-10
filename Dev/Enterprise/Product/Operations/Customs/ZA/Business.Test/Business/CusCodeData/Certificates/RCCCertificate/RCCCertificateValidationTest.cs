using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class RCCCertificateValidationTest : CertificateValidationAbstractTest<RCCCertificate>
	{
		protected override ZString[] ExpectedPermitTypes => new ZString[] { PermitTypeList.Codes.RCC, PermitTypeList.Codes.VALA };

		protected override Customs.Business.CusCodeDataCollection<RCCCertificate> GetCertificateCollection(CusEntryInstruction instruction) => instruction.RCCCertificates;
	}
}
