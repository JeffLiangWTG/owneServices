namespace Enterprise.Freight.Agency.Business.Testing
{
	internal class ContainerStockManagerTest : BaseAgencyTest
	{
		#region Implementation
		protected AgencyShipment Shipment
		{
			get
			{
				return shipment ?? (shipment = Factory.New<AgencyShipment>());
			}
		}

		AgencyShipment shipment;
		protected AgencyShipmentContainer Container
		{
			get
			{
				return container ?? (container = Shipment.RealContainers.AddNew());
			}
		}

		AgencyShipmentContainer container;
		#endregion
	}
}
