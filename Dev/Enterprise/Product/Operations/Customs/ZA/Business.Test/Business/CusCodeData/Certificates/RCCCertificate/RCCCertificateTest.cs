using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(RCCCertificate))]
	sealed class RCCCertificateTest : CertificateCusCodeDataAbstractTest<RCCCertificate>
	{
		public void TestPermitType()
		{
			var rccCertificate = GetNewBusinessObject() as RCCCertificate;
			AssertEquals(PermitTypeList.Codes.RCC, rccCertificate.PermitType);
		}

		protected override ZString PermitTypeTypeForTesting => PermitTypeList.Codes.RCC;
	}
}
