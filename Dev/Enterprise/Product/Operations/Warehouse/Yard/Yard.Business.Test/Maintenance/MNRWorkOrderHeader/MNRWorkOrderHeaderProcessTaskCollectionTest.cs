using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(MNRWorkOrderHeaderProcessTaskCollection))]
	public class MNRWorkOrderHeaderProcessTaskCollectionTest : ProcessTaskCollectionTest<MNRWorkOrderHeaderProcessTaskCollection>
	{
		protected override MNRWorkOrderHeaderProcessTaskCollection GetCollectionToTestCore()
		{
			var advice = Factory.NewWithValidTestData<MNRWorkOrderHeader>();
			return new MNRWorkOrderHeaderProcessTaskCollection(advice);
		}
	}
}
