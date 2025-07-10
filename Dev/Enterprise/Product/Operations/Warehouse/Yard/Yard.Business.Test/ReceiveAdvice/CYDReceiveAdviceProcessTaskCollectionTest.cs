using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(CYDReceiveAdviceProcessTaskCollection))]
	public class CYDReceiveAdviceProcessTaskCollectionTest : ProcessTaskCollectionTest<CYDReceiveAdviceProcessTaskCollection>
	{
		protected override CYDReceiveAdviceProcessTaskCollection GetCollectionToTestCore()
		{
			var advice = Factory.NewWithValidTestData<CYDReceiveAdvice>();
			return new CYDReceiveAdviceProcessTaskCollection(advice);
		}
	}
}
