using CargoWise.EntityFramework.Testing;
using Enterprise.OceanCarrier.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.OceanCarrier.Module.Testing
{
	sealed class CarrierShipmentHeaderFilterControlTest : TestCaseWithFactory
	{
		public void TestPerformSearchShouldShouldNotThrowAnException()
		{
			var collection = new CarrierShipmentHeaderCollection(Factory);
			var filter = new CarrierShipmentHeaderFilterStripBusinessObject();

			using (var filterControl = new CarrierShipmentHeaderFilterControl(collection, filter))
			using (var form = new ZForm())
			{
				form.Controls.Add(filterControl);
				form.Show();

				AssertNoExceptionThrown(() => filterControl.FirePerformSearch());
			}
		}
	}
}
