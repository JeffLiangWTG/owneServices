using System.Collections.Generic;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Module;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Invoicing;
using Enterprise.Warehouse.Transactions.Module.Release.OperationalActions;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class WhsOperationalActionMethodProvider : OperationalActionMethodProvider
	{
		public WhsOperationalActionMethodProvider()
		{
		}

		public override OperationalActionMethod[] NewMethods(OperationalActionSupporter actionSupporter)
		{
			var result = new List<OperationalActionMethod>();

			if (typeof(WhsInventoryView).IsAssignableFrom(actionSupporter.RootType))
			{
				result.Add(new GenerateOrderFromInventoryActionMethod());
				result.Add(new UpdateInventoryHeldCodeActionMethod());
			}
			else if (typeof(WhsReceive).IsAssignableFrom(actionSupporter.RootType))
			{
				result.Add(new GenerateOrderFromReceiveActionMethod());
				result.Add(new CancelReceivesActionMethod());
				result.Add(new FinaliseReceivesActionMethod());
			}
			else if (typeof(WhsAdjustment).IsAssignableFrom(actionSupporter.RootType))
			{
				result.Add(new FinaliseAdjustmentsActionMethod());
			}
			else if (typeof(WhsPickableDocket).IsAssignableFrom(actionSupporter.RootType))
			{
				result.Add(new GeneratePickActionMethod());

				if (typeof(WhsOrder).IsAssignableFrom(actionSupporter.RootType))
				{
					result.Add(new GenerateMultiOrderPickActionMethod());
					result.Add(new GenerateWorkOrdersActionMethod());
					result.Add(new ReleaseOrdersActionMethod());
					result.Add(new FinaliseOrdersActionMethod());
					result.Add(new OrderAutoPackAllLinesActionMethod());
					result.Add(new GenerateRMAByOrdersActionMethod());
					result.Add(new RemoveHoldAllPackagesActionMethod());
					result.Add(new CancelOrdersActionMethod());
					result.Add(new OrderSetCarrierAndCarrierServiceLevelActionMethod());
					result.Add(new OverrideWhsOrderFulfillmentRuleActionMethod());
					result.Add(new CreateLoadsFromOrdersActionMethod());
				}
			}
			else if (typeof(WhsStocktake).IsAssignableFrom(actionSupporter.RootType))
			{
				result.Add(new StocktakeAssignAllLinesToUserActionMethod());
			}
			else if (typeof(WhsPick).IsAssignableFrom(actionSupporter.RootType) && actionSupporter is ReleaseOperationalActionsSupporter)
			{
				result.Add(new PickAssignAllLinesToUserActionMethod(true));
				result.Add(new FinalisePicksActionMethod());
				result.Add(new ChangePriorityActionMethod());
				result.Add(new PickAutoPackAllLinesActionMethod());
				result.Add(new CancelPickActionMethod());
				result.Add(new PickUnAssignAllLinesActionMethod(true));
				result.Add(new AutoAllocateItemsActionMethod());
			}
			else if (typeof(WhsPick).IsAssignableFrom(actionSupporter.RootType) && actionSupporter is PickOperationalActionsSupporter)
			{
				result.Add(new PickAssignAllLinesToUserActionMethod(false));
				result.Add(new FinalisePicksActionMethod());
				result.Add(new ChangePriorityActionMethod());
				result.Add(new PickAutoPackAllLinesActionMethod());
				result.Add(new CancelPickActionMethod());
				result.Add(new PickUnAssignAllLinesActionMethod(false));
				result.Add(new AutoAllocateItemsActionMethod());
			}
			else if (typeof(WhsTransfer).IsAssignableFrom(actionSupporter.RootType))
			{
				result.Add(new TransferAssignAllLinesToUserPickOnlyActionMethod());
				result.Add(new TransferUnAssignAllLinesPickOnlyActionMethod());
			}
			else if (typeof(WhsPickFaceView).IsAssignableFrom(actionSupporter.RootType))
			{
				result.Add(new AssignProductToPickFaceActionMethod());
				result.Add(new TransferInventoryOutAndUnassignProductFromPickFaceActionMethod());
				result.Add(new ForceReplenishmentToPickFaceActionMethod());
			}
			else if (typeof(WhsInvoice).IsAssignableFrom(actionSupporter.RootType))
			{
				result.Add(new ChangeOffBandProcessingStatusActionMethod());
			}

			return (result.Count > 0) ? result.ToArray() : null;
		}
	}
}
