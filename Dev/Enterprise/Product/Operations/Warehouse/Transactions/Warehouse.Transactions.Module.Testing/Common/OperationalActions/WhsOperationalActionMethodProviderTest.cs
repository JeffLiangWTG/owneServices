using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Module;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Invoicing;
using Enterprise.Warehouse.Transactions.Module.Release.OperationalActions;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Module.Testing
{
	[TestedType(typeof(WhsOperationalActionMethodProvider))]
	public class WhsOperationalActionMethodProviderTest : OperationalActionMethodProviderTest
	{
		#region TestNewMethods

		public void TestNewMethods()
		{
			AssertNull("Expected Methods", Provider.NewMethods(new Supporter<BusinessObject>()));

			// Receive
			AssertContainsExactElementsInAnyOrder("Expected Methods for Receive",
				new Type[]
				{
					typeof(GenerateOrderFromReceiveActionMethod),
					typeof(CancelReceivesActionMethod),
					typeof(FinaliseReceivesActionMethod)
				},
				Array.ConvertAll(Provider.NewMethods(new Supporter<WhsReceive>()), m => m.GetType()));

			// Adjustment
			AssertContainsExactElementsInAnyOrder("Expected Methods for Adjustment",
				new[] { typeof(FinaliseAdjustmentsActionMethod) },
				Array.ConvertAll(Provider.NewMethods(new Supporter<WhsAdjustment>()), m => m.GetType()));

			// Inventory
			AssertContainsExactElementsInAnyOrder("Expected Methods for Inventory",
				new Type[] { typeof(GenerateOrderFromInventoryActionMethod), typeof(UpdateInventoryHeldCodeActionMethod) },
				Array.ConvertAll(Provider.NewMethods(new Supporter<WhsInventoryView>()), m => m.GetType()));

			// Order
			AssertContainsExactElementsInAnyOrder("Expected Methods for Orders",
				new Type[] {
					typeof(GeneratePickActionMethod),
					typeof(GenerateWorkOrdersActionMethod),
					typeof(ReleaseOrdersActionMethod),
					typeof(FinaliseOrdersActionMethod),
					typeof(OrderAutoPackAllLinesActionMethod),
					typeof(GenerateRMAByOrdersActionMethod) ,
					typeof(RemoveHoldAllPackagesActionMethod),
					typeof(CancelOrdersActionMethod),
					typeof(GenerateMultiOrderPickActionMethod),
					typeof(OrderSetCarrierAndCarrierServiceLevelActionMethod),
					typeof(OverrideWhsOrderFulfillmentRuleActionMethod),
					typeof(CreateLoadsFromOrdersActionMethod) },
				Array.ConvertAll(Provider.NewMethods(new Supporter<WhsOrder>()), m => m.GetType()));

			// Work Order
			AssertContainsExactElementsInAnyOrder("Expected Methods for Work Orders",
				new Type[] { typeof(GeneratePickActionMethod) },
				Array.ConvertAll(Provider.NewMethods(new Supporter<WhsWorkOrder>()), m => m.GetType()));

			// Stocktake
			AssertContainsExactElementsInAnyOrder("Expected Methods for Stocktake",
				new Type[] { typeof(StocktakeAssignAllLinesToUserActionMethod) },
				Array.ConvertAll(Provider.NewMethods(new Supporter<WhsStocktake>()), m => m.GetType()));

			// Release/Picks
			AssertContainsExactElementsInAnyOrder("Expected Methods for Release",
				new Type[] {
					typeof(PickAssignAllLinesToUserActionMethod),
					typeof(FinalisePicksActionMethod),
					typeof(PickAutoPackAllLinesActionMethod),
					typeof(ChangePriorityActionMethod),
					typeof(CancelPickActionMethod),
					typeof(PickUnAssignAllLinesActionMethod),
					typeof(AutoAllocateItemsActionMethod)
				},
				Array.ConvertAll(Provider.NewMethods(new ReleaseOperationalActionsSupporter()), m => m.GetType()));

			AssertContainsExactElementsInAnyOrder("Expected Methods for Pick",
				new Type[] {
					typeof(PickAssignAllLinesToUserActionMethod),
					typeof(FinalisePicksActionMethod),
					typeof(PickAutoPackAllLinesActionMethod),
					typeof(ChangePriorityActionMethod),
					typeof(CancelPickActionMethod),
					typeof(PickUnAssignAllLinesActionMethod),
					typeof(AutoAllocateItemsActionMethod)
				},
				Array.ConvertAll(Provider.NewMethods(new PickOperationalActionsSupporter()), m => m.GetType()));

			// Transfer
			AssertContainsExactElementsInAnyOrder("Expected Methods for Transfer",
				new Type[]
				{
					typeof(TransferAssignAllLinesToUserPickOnlyActionMethod),
					typeof(TransferUnAssignAllLinesPickOnlyActionMethod)
				},
				Array.ConvertAll(Provider.NewMethods(new Supporter<WhsTransfer>()), m => m.GetType()));

			// Pick Face View
			AssertContainsExactElementsInAnyOrder("Expected Methods for Pick Face View",
				new Type[]
				{
					typeof(AssignProductToPickFaceActionMethod),
					typeof(TransferInventoryOutAndUnassignProductFromPickFaceActionMethod),
					typeof(ForceReplenishmentToPickFaceActionMethod)
				},
				Array.ConvertAll(Provider.NewMethods(new Supporter<WhsPickFaceView>()), m => m.GetType()));

			// Periodic billing (WhsInvoice)
			AssertContainsExactElementsInAnyOrder("Expected Methods for Periodic billing (WhsInvoice)",
				new Type[] { typeof(ChangeOffBandProcessingStatusActionMethod) },
				Array.ConvertAll(Provider.NewMethods(new Supporter<WhsInvoice>()), m => m.GetType()));
		}

		#endregion

		#region Implementation

		protected override ActionMethodProviderID ID
		{
			get { return ActionMethodProviderIDs.Warehouse; }
		}

		class Supporter<T> : OperationalActionSupporter where T : BusinessObject
		{
			public override BusinessContext BusinessContext
			{
				get { throw new Exception("The method or operation is not implemented."); }
			}

			public override Type RootType
			{
				get { return typeof(T); }
			}
		}

		#endregion
	}
}
