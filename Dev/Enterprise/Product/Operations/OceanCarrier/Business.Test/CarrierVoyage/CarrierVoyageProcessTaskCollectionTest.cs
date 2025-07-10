using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.OceanCarrier.Business.Testing
{
	[TestedType(typeof(CarrierVoyageProcessTaskCollection))]
	sealed class CarrierVoyageProcessTaskCollectionTest : ProcessTaskCollectionTest<CarrierVoyageProcessTaskCollection>
	{
		public void TestParent() => AssertType<CarrierVoyage>("Collection Parent Type", Collection.Parent);

		protected override CarrierVoyageProcessTaskCollection GetCollectionToTestCore() =>
			new CarrierVoyageProcessTaskCollection(CarrierVoyage);

		CarrierVoyage CarrierVoyage => carrierVoyage ?? (carrierVoyage = Factory.New<CarrierVoyage>());
		CarrierVoyage carrierVoyage;
	}
}
