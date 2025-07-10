using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Warehouse.Transactions.CodeLists;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsOrderCollectionForAttaching))]
	public class WhsOrderCollectionForAttachingTest : WhsBusinessObjectCollectionTestCase
	{
		#region TestAdditionalFilter

		public void TestAdditionalAndRelationshipFilter()
		{
			var shipment = (BusinessObject)Factory.New<Forwarding.IForwardingShipment>();

			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(true);
			Factory.Save();

			var orderWithoutPick = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			orderWithoutPick.WD_ExternalReference = "1";

			var orderWithUnfinalisedPick = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			orderWithUnfinalisedPick.WD_ExternalReference = "2";
			var unFinalisedPick = Helper.CreatePickNew(true, false, orderWithUnfinalisedPick);

			var orderWithFinalisedPick = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			orderWithFinalisedPick.WD_ExternalReference = "3";
			var finalisedPick1 = Helper.CreatePickNew(true, true, orderWithFinalisedPick);

			var finalisedOrderAlreadyAttachedToAnotherShipment = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			finalisedOrderAlreadyAttachedToAnotherShipment.WD_ExternalReference = "4";
			var finalisedPick2 = Helper.CreatePickNew(true, true, finalisedOrderAlreadyAttachedToAnotherShipment);
			var pivot = Factory.New<WhsDocketJobPivot>();
			pivot.WV_DocketType = DocketType.Codes.Order;
			pivot.WV_ParentId = shipment.PK;
			pivot.WV_ParentTableCode = shipment.TablePrefix;
			pivot.WV_WD_Docket = finalisedOrderAlreadyAttachedToAnotherShipment.PK;

			Helper.CreateWhsWorkOrder(data.Org1, data.Whs1);
			Helper.CreateWhsReceive(data.Org1, data.Whs1);
			Helper.CreateWhsTransfer(data.Org1, data.Whs1);
			Helper.CreateWhsAdjustment(data.Org1, data.Whs1);
			Factory.Save();

			var orders = new WhsOrderCollectionForAttaching(Factory);
			orders.Load();

			AssertContainsExactElementsInAnyOrder(new[] { orderWithoutPick, orderWithUnfinalisedPick, orderWithFinalisedPick }, orders);
			AssertEquals("Cannot attach an Order already attached to another Shipment.", (orders.GetAllNotificationsWhenAdditionalFilterNotMet(finalisedOrderAlreadyAttachedToAnotherShipment)));
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new WhsOrderCollectionForAttaching(Factory);
		}

		#endregion
	}
}
