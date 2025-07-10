using System;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.eTail.Business
{
	public class HVLVShipmentItemsCountView : AutoHVLVShipmentItemsCountView
	{
		public HVLVShipmentItemsCountView(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override bool CanDelete => false;

		public override void Delete() => throw new NotSupportedException();
	}
}
