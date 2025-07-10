using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class DutyRebateCertificateLookupsTest : CertificateLookupsAbstractTest<DutyRebateCertificate>
	{
		protected override ZString PermitType => PermitTypeList.Codes.PRC;

		protected override CusCodeDataCollection<DutyRebateCertificate> GetCertificateCollection(CusEntryInstruction instruction) => instruction.DutyRebateCertificates;

		protected override ZString[] ExpectedPermitTypes => new ZString[] { PermitType };
	}
}
