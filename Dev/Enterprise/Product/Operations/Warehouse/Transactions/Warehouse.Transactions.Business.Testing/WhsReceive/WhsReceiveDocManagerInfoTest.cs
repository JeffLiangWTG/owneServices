using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsReceiveDocManagerInfo))]
	class WhsReceiveDocManagerInfoTest : DocManagerInfoTestCase
	{
		#region TestGetRelatedObjects

		public void TestGetRelatedObjects()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var forwardingOrder = (BusinessObject)Factory.New<Enterprise.Integration.Forwarding.IOrder>();
			var pivot = Factory.New<WhsDocketJobPivot>();
			pivot.WV_ParentId = forwardingOrder.PK;
			pivot.WV_ParentTableCode = forwardingOrder.TablePrefix;
			pivot.WV_DocketType = receive.WD_DocketType;
			pivot.WV_WD_Docket = receive.PK;

			var receiveDocManagerInfo = new WhsReceiveDocManagerInfo(receive);
			AssertContainsExactElementsInAnyOrder(new BusinessObject[] { forwardingOrder }, receiveDocManagerInfo.RelatedObjects);
		}

		public void TestGetRelatedObjects_Shipment()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1);
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType<Enterprise.Integration.Forwarding.IForwardingShipment>());
			var pivot = Factory.New<WhsDocketJobPivot>();
			pivot.WV_ParentId = shipment.PK;
			pivot.WV_ParentTableCode = shipment.TablePrefix;
			pivot.WV_DocketType = receive.WD_DocketType;
			pivot.WV_WD_Docket = receive.PK;

			var receiveDocManagerInfo = new WhsReceiveDocManagerInfo(receive);
			AssertContainsExactElementsInAnyOrder(new BusinessObject[] { shipment }, receiveDocManagerInfo.RelatedObjects);
		}

		#endregion

		#region Implementation

		public override BusinessObject GetEmptyParentBusinessObject()
		{
			return Factory.New<WhsReceive>();
		}

		public override BusinessObject GetPopulatedParentBusinessObject()
		{
			var receive = Factory.New<WhsReceive>();
			var forwardingOrder = (BusinessObject)Factory.New<Enterprise.Integration.Forwarding.IOrder>();
			var pivot = Factory.New<WhsDocketJobPivot>();
			pivot.WV_ParentId = forwardingOrder.PK;
			pivot.WV_ParentTableCode = forwardingOrder.TablePrefix;
			pivot.WV_DocketType = receive.WD_DocketType;
			pivot.WV_WD_Docket = receive.PK;

			return receive;
		}

		WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}

		WhsTestHelperFunctions helper;

		#endregion
	}
}
