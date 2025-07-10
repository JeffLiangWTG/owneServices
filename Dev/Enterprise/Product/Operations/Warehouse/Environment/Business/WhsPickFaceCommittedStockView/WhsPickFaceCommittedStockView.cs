using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Warehouse.Integration.Warehouse;

namespace Enterprise.Warehouse.Environment.Business
{
	public class WhsPickFaceCommittedStockView : AutoWhsPickFaceCommittedStockView, ISupportPickPriority
	{
		public WhsPickFaceCommittedStockView(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public override bool CanDelete => false;

		public override void Delete()
		{
			throw new NotSupportedException("You cannot delete this.");
		}

		ZInt ISupportPickPriority.PickPriority => WCP_PickPriority;
	}
}
