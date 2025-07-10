namespace Enterprise.Freight.DataTransfer.Testing
{
	sealed class ConsolValueObjectDataAdapterRegistrySettingsTestCheckChildObjects : ConsolValueObjectDataAdapterRegistrySettingsTest
	{
		protected override bool RoutingShouldUpdate(bool updateConsol, bool updateRouting)
		{
			return updateRouting;
		}

		protected override bool SailingShouldUpdate(bool updateConsol, bool updateRouting, bool updateSailing)
		{
			return updateRouting && updateSailing;
		}

		protected override bool ShipmentsShouldUpdate(bool updateConsol, bool updateShipments)
		{
			return updateShipments;
		}

		protected override bool ContainersShouldUpdate(bool updateConsol, bool updateContainers)
		{
			return updateContainers;
		}

		protected override bool AllwaysCheckForConsolsSailingShipmentsAndContainersValue
		{
			get { return true; }
		}
	}
}
