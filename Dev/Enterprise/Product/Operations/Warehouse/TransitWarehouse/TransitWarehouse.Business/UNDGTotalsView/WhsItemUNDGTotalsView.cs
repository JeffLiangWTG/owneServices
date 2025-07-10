using System;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transit.Business
{
	[GlowInterfaceReference("IWhsItemUNDGTotalsView")]
	public class WhsItemUNDGTotalsView : AutoWhsItemUNDGTotalsView, ICanDelete
	{
		public WhsItemUNDGTotalsView(BusinessObjectFactory factory, DataRow row)
			   : base(factory, row)
		{
		}
		public override bool CanDelete => false;

		public override void Delete()
		{
			throw new NotSupportedException("You cannot delete this.");
		}
	}
}
