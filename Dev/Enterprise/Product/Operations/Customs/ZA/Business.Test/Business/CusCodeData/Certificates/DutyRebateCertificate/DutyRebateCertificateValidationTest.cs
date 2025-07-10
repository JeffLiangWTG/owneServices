using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class DutyRebateCertificateValidationTest : CertificateValidationAbstractTest<DutyRebateCertificate>
	{
		protected override ZString[] ExpectedPermitTypes => new ZString[] { PermitTypeList.Codes.PRC };

		protected override Customs.Business.CusCodeDataCollection<DutyRebateCertificate> GetCertificateCollection(CusEntryInstruction instruction) => instruction.DutyRebateCertificates;
	}
}
