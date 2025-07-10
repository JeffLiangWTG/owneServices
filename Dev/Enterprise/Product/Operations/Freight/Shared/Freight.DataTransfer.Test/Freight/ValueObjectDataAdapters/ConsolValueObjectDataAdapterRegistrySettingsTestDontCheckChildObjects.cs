namespace Enterprise.Freight.DataTransfer.Testing
{
	sealed class ConsolValueObjectDataAdapterRegistrySettingsTestDontCheckChildObjects : ConsolValueObjectDataAdapterRegistrySettingsTest
	{
		protected override bool AllwaysCheckForConsolsSailingShipmentsAndContainersValue
		{
			get { return false; }
		}

		protected override bool ContainersShouldUpdate(bool updateConsol, bool updateContainers)
		{
			return updateConsol && updateContainers;
		}

		protected override bool RoutingShouldUpdate(bool updateConsol, bool updateRouting)
		{
			return updateConsol && updateRouting;
		}

		protected override bool SailingShouldUpdate(bool updateConsol, bool updateRouting, bool updateSailing)
		{
			return updateConsol && updateRouting && updateSailing;
		}

		protected override bool ShipmentsShouldUpdate(bool updateConsol, bool updateShipments)
		{
			return updateConsol && updateShipments;
		}
	}
}
