using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.CFS.Business.Testing
{
	public class GatePassShipmentValidationTest : TestCaseWithFactory
	{
		#region Imlementation

		protected GatePassShipment Shipment;

		protected override void SetUp()
		{
			base.SetUp();
			Shipment = Factory.New<GatePassShipment>();
		}

		#endregion
	}
}
