using CargoWise.EntityFramework;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsReceiveCollectionBuilder : WhsDocketCollectionBuilder<WhsReceive, LineData>
	{
		public WhsReceiveCollectionBuilder(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public override WhsDocketLine AddLine(LineData data, WhsDocketCollectionBuilderLineOptions options)
		{
			var docketLine = base.AddLine(data, options);
			if (docketLine != null)
			{
				docketLine.WE_ClientOrderedUnits += data.Quantity;
			}

			return docketLine;
		}

		protected override WhsReceive CreateNewDocket()
		{
			var receive = base.CreateNewDocket();
			receive.IsAutoCreatingReceive = true;
			return receive;
		}
	}

	public class WhsReceiveCollectionBuilderForBonded : WhsReceiveCollectionBuilder
	{
		public WhsReceiveCollectionBuilderForBonded(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override WhsReceive FindDocket(LineData data)
		{
			return Dockets.FindDocket(data.WhsPK, data.OrgPK);
		}
	}
}
