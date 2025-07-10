using System;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsOutboundLocationView : AutoWhsOutboundLocationView
	{
		public WhsOutboundLocationView(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override bool CanDelete => false;

		public override void Delete()
		{
			throw new NotSupportedException("You cannot delete this.");
		}
	}
}
