using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class OrdersBuyerCollection : ConsigneeCollection, IFilterModuleExtraNotificationProvider
	{
		public OrdersBuyerCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		#region IFilterModuleExtraNotificationProvider Member

		public INotification GetExtraNotification(BusinessObject businessObject)
		{
			var buyer = businessObject as OrgHeader;

			if (buyer != null && buyer.MiscServ != null && !buyer.MiscServ.OM_IMAllowOrders)
			{
				return new Notification(CargoWise.ComponentModel.NotificationType.Error, Res.GetString("c2bb399e-7da2-48ca-bc16-f05ae699ccbd", "This buyer is restricted from using Order Manager (Organization > Consignee > Disallow Order Manager flag is on)"));
			}

			return null;
		}

		#endregion

	}
}
