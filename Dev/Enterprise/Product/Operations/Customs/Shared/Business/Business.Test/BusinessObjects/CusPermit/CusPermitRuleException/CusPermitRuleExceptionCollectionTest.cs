using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(CusPermitRuleExceptionCollection))]
	sealed class CusPermitRuleExceptionCollectionTest : ActiveBusinessObjectCollectionTestCase<CusPermitRuleExceptionCollection>
	{
		protected override CusPermitRuleExceptionCollection GetCollectionToTest()
		{
			return new CusPermitRuleExceptionCollection(Factory.New<BaseCusPermitRule>());
		}
	}
}
