using Enterprise.Customs.EU.Business.Declaration.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

[TestedType(typeof(CusFiscalReference))]
sealed class CusFiscalReferenceTest : CusFiscalReferenceAbstractTest<CusFiscalReference>
{
	public void TestCFR_ReferenceCaption()
	{
		AssertEquals("CFR_Reference Caption", "Tax Number (TIN)", cusFiscalReference.CFR_ReferenceInfo.HumanReadableName);
	}

	protected override void SetUp()
	{
		base.SetUp();
		cusFiscalReference = Factory.NewWithValidTestData<CusFiscalReference>();
	}
	CusFiscalReference cusFiscalReference;
}
