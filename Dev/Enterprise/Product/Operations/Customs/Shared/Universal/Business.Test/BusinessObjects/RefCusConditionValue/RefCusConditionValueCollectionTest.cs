using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCusConditionValueCollection))]
	public class RefCusConditionValueCollectionTest : ActiveBusinessObjectCollectionTestCase<RefCusConditionValueCollection>
	{
		protected override RefCusConditionValueCollection GetCollectionToTest()
		{
			return Factory.New<RefCusCondition>().ConditionValues;
		}
	}
}
