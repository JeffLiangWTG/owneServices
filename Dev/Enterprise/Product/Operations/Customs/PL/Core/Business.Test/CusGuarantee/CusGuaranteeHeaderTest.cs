using Enterprise.Customs.EU.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(CusGuaranteeHeader))]
class CusGuaranteeHeaderTest : CusGuaranteeHeaderAbstractTest
{
	public void TestMainAccessCode_MaxLength()
	{
		var guaranteeHeader = Factory.New<CusGuaranteeHeader>();
		AssertEquals(4, guaranteeHeader.MainAccessCodeInfo.MaxLength);
	}
}
