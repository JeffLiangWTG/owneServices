using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration.TransitWarehouse;

namespace Enterprise.Warehouse.Transit.Business
{
	public class WhsItemConsignmentOrderReference : AutoWhsItemConsignmentOrderReference, IWhsItemConsignmentOrderReference
	{
		public WhsItemConsignmentOrderReference(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
		{
		}

		public ZString ConsignmentOrderNumber => WOR_OrderReference;
	}
}
