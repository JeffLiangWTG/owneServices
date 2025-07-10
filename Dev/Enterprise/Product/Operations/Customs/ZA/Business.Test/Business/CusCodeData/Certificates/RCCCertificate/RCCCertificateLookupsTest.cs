using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.ZA.Business.Testing
{
	sealed class RCCCertificateLookupsTest : CertificateLookupsAbstractTest<RCCCertificate>
	{
		protected override ZString PermitType => CusCodeDataTypeList.Codes.RCC;

		protected override CusCodeDataCollection<RCCCertificate> GetCertificateCollection(CusEntryInstruction instruction) => instruction.RCCCertificates;

		protected override ZString[] ExpectedPermitTypes => new ZString[] { PermitTypeList.Codes.RCC, PermitTypeList.Codes.VALA };
	}
}
