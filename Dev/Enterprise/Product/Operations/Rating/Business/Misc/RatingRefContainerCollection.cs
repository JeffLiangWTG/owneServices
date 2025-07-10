using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business
{
	public class RatingRefContainerCollection : RefContainerCollection, IFilterModuleExtraNotificationProvider
	{
		public RatingRefContainerCollection(BusinessObjectFactory factory)
			: base(factory) { }

		public RatingRefContainerCollection(BusinessObjectFactory factory, ZQuery additionalFilter)
			: base(factory, additionalFilter) { }

		public RatingRefContainerCollection(BusinessObjectFactory factory, ICollectionRelationship relationship)
			: base(factory, relationship) { }

		public RatingRefContainerCollection(BusinessObjectFactory factory, string transportMode)
			: base(factory, transportMode) { }

		public INotification GetExtraNotification(BusinessObject businessObject)
		{
			if (businessObject is RefContainer container)
			{
				var errorMessage = new StringBuilder();
				if (!container.RC_IsActive)
				{
					errorMessage.AppendLine(Res.GetString("161494F8-6EEF-4FA1-95C2-122C4BDCCDEC", "This container is not active - it may not be used."));
				}

				if (TransportMode.EqualsIgnoringCase(RefContainerLookups.ShippingModes.Air) && !container.IsAirContainer)
				{
					errorMessage.AppendLine(Res.GetString("CFB85AC3-ED76-4604-AEAA-62781C71AF8B", "This is an Air Freight entry - please choose an Air Freight ULD Container."));
				}
				else if (TransportMode.EqualsIgnoringCase(RefContainerLookups.ShippingModes.Sea) && !container.IsSeaContainer)
				{
					errorMessage.AppendLine(Res.GetString("7ED7AF93-47BB-4C79-B3C8-3C0F48BD7220", "This is a Sea Freight entry - please choose a Sea Freight FCL Container."));
				}
				else if (TransportMode.EqualsIgnoringCase(RefContainerLookups.ShippingModes.Road) && !(container.IsRoadTruckContainer || container.IsSeaContainer))
				{
					errorMessage.AppendLine(Res.GetString("A970C70B-CEEF-4484-8DC1-0CA37FDE8401", "This is a Road Freight entry - please choose a Road or Sea Freight Container."));
				}

				if (errorMessage.Length > 0)
				{
					return new Notification(CargoWise.EntityFramework.NotificationType.Error, errorMessage.ToString());
				}
			}

			return null;
		}
	}
}

