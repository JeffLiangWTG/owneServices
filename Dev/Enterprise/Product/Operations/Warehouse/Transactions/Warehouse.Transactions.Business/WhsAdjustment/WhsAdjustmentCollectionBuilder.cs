using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.CodeLists;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class WhsAdjustmentCollectionBuilder : WhsDocketCollectionBuilder<WhsAdjustment, LineData>
	{
		public WhsAdjustmentCollectionBuilder(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Overrides

		public override WhsDocketLine AddLine(LineData data, WhsDocketCollectionBuilderLineOptions options)
		{
			var line = base.AddLine(data, options);

			if (line != null)
			{
				line.WE_OriginalInventoryStatus = InventoryStatus.Codes.Available;
			}

			return line;
		}

		#endregion
	}

	public class WhsAdjustmentCollectionBuilderForBonded : WhsAdjustmentCollectionBuilder
	{
		public WhsAdjustmentCollectionBuilderForBonded(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override WhsAdjustment FindDocket(LineData data)
		{
			return Dockets.FindDocket(data.WhsPK, data.OrgPK);
		}

		// tested in WhsBondedTransactionProcessorInbound
		protected override void FinaliseDocket(WhsAdjustment docket)
		{
			using (docket.AttemptDodgyBondedFinalise())
			{
				base.FinaliseDocket(docket);
			}
		}
	}
}
