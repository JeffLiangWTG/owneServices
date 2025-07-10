using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(TaxFrameworkAccTaxRate))]
	sealed class TaxFrameworkAccTaxRateTest : EnterpriseBusinessObjectTestCase
	{
		public void TestAT_Type()
		{
			var bizObj = GetNewBusinessObject() as TaxFrameworkAccTaxRate;
			Assert("Is read-only", bizObj.AT_TypeInfo.ReadOnly);
			AssertEquals("Default AT_Type is NOT", AccTaxRate.Types.NotReportable, bizObj.AT_Type);
		}
	}
}
