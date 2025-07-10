using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Freight.Business
{
	public class OrderItem : AutoJobOrderItem, Integration.IOrderItem
	{
		public OrderItem(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
	}
}
