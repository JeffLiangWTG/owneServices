using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(CYDAdHocServiceOrderProcessTaskCollection))]
	public class CYDAdHocServiceOrderProcessTaskCollectionTest : ProcessTaskCollectionTest<CYDAdHocServiceOrderProcessTaskCollection>
	{
		protected override CYDAdHocServiceOrderProcessTaskCollection GetCollectionToTestCore()
		{
			var advice = Factory.NewWithValidTestData<CYDAdHocServiceOrder>();
			return new CYDAdHocServiceOrderProcessTaskCollection(advice);
		}
	}
}
