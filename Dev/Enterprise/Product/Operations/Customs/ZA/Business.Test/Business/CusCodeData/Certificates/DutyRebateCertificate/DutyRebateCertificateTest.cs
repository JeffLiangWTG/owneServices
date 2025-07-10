using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(DutyRebateCertificate))]
	sealed class DutyRebateCertificateTest : CertificateCusCodeDataAbstractTest<DutyRebateCertificate>
	{
		public void TestPermitType()
		{
			var dutyRebateCertificate = GetNewBusinessObject() as DutyRebateCertificate;
			AssertEquals(PermitTypeList.Codes.PRC, dutyRebateCertificate.PermitType);
		}

		protected override ZString PermitTypeTypeForTesting => PermitTypeList.Codes.PRC;
	}
}
