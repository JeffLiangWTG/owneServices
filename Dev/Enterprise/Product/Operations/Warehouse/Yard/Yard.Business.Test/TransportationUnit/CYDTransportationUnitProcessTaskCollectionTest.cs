using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	[TestedType(typeof(CYDTransportationUnitProcessTaskCollection))]
	public class CYDTransportationUnitProcessTaskCollectionTest : ProcessTaskCollectionTest<CYDTransportationUnitProcessTaskCollection>
	{
		protected override CYDTransportationUnitProcessTaskCollection GetCollectionToTestCore()
		{
			var transportationUnit = Factory.NewWithValidTestData<CYDTransportationUnit>();
			return new CYDTransportationUnitProcessTaskCollection(transportationUnit);
		}
	}
}
