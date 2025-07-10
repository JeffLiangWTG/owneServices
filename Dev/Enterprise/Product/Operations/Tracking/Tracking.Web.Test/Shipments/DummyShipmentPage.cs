namespace Enterprise.Tracking.Web.Testing
{
	sealed class DummyShipmentPage : Shipment
	{
		protected override ZArchitecture.Web.GUI.ZGlobal GetNewTestGlobal()
		{
			return new TestGlobal();
		}
	}
}
