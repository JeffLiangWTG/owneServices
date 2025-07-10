using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.OceanCarrier.Business.Testing
{
	[TestedType(typeof(CarrierVoyagePortCallProcessTaskCollection))]
	sealed class CarrierVoyagePortCallProcessTaskCollectionTest : ProcessTaskCollectionTest<CarrierVoyagePortCallProcessTaskCollection>
	{
		public void TestParent() => AssertType<CarrierVoyagePortCall>("Collection Parent Type", Collection.Parent);

		protected override CarrierVoyagePortCallProcessTaskCollection GetCollectionToTestCore() =>
			new CarrierVoyagePortCallProcessTaskCollection(CarrierVoyagePortCall);

		CarrierVoyagePortCall CarrierVoyagePortCall => carrierVoyagePortCall ?? (carrierVoyagePortCall = Factory.New<CarrierVoyagePortCall>());
		CarrierVoyagePortCall carrierVoyagePortCall;
	}
}
