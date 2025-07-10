using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Warehouse.Environment.Business
{
	public class TransitWarehouseLocationCollection : LocationCollection, IFilterModuleExtraNotificationProvider
	{
		public TransitWarehouseLocationCollection(BusinessObjectFactory factory)
		: base(factory)
		{
		}

		public INotification GetExtraNotification(BusinessObject businessObject)
		{
			var zone = businessObject as RefZoneHeader;
			if (zone != null)
			{
				bool isTWHType = zone.FZ_ZoneType.EqualsIgnoringCase(RefZoneHeaderLookups.ZoneTypeCodes.TransitWarehouse);
				if (!isTWHType)
				{
					return new Notification(CargoWise.EntityFramework.NotificationType.Error, Res.GetString("42243fdf-e789-4e72-8d23-c01da138644c", "This Zone cannot be chosen here as it is not for Transit Warehouse."));
				}
			}

			return null;
		}
	}
}
