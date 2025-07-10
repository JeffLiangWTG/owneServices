using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	class WhsPickLineFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public WhsPickLineFetchStrategy(WhsPickLine pickLine)
			: base(pickLine)
		{
		}

		#region FetchForFactorySave

		protected override void FetchForFactorySaveCore()
		{
			base.FetchForFactorySaveCore();
			AddFetchHintsForInTransitTransferLineCreation();
		}

		// tested in WhsPick.TestPickingAllPickLines_DBHits & WhsPick.TestPickingAllPickLines_WorkOrder_DBHits
		void AddFetchHintsForInTransitTransferLineCreation()
		{
			var pickLine = PickLine;
			if (pickLine.IsPickedInMemory && pickLine.DocketLine is WhsPickableDocketLine transactionLine)
			{
				Factory.AddFetchHint(WhsBondedWarehouseAttributeSchema.WB_ParentID, pickLine.WZ_WE_InventoryLine);
				// the Product Params Fetch Hints are only necessary for Work Orders
				if (transactionLine is WhsWorkOrderLine)
				{
					Factory.AddFetchHint(WhsProductParamsByWhsAndClientSchema.W3_OP, transactionLine.WE_OP);

					// the Component Line needs to check its Parent for its Staging Area
					if (!transactionLine.WE_WE_ParentDocketLine.IsEmpty)
					{
						Factory.AddFetchHint(WhsProductParamsByWhsAndClientSchema.W3_OP, transactionLine.ParentLine.WE_OP);
					}
				}
			}
		}

		#endregion

		#region FetchForLoad

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();

			// Tested in WhsPickLine
			Factory.AddFetchHint(typeof(WhsInventoryView), WhsInventoryViewSchema.WI_WE_InDocketLine, PickLine.WZ_WE_InventoryLine);
		}

		#endregion

		#region PickLine

		WhsPickLine PickLine
		{
			get { return (WhsPickLine)BusinessObject; }
		}

		#endregion
	}
}
