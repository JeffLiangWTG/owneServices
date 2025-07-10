using System;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsLoadOrder : AutoWhsLoadOrder
	{
		public WhsLoadOrder(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public override bool CanDelete => false;

		public override void Delete()
		{
			throw new NotSupportedException("You cannot delete this.");
		}

		public WhsLoad Load => Factory.Load<WhsLoad>(WOV_WLO_Load);
	}
}
