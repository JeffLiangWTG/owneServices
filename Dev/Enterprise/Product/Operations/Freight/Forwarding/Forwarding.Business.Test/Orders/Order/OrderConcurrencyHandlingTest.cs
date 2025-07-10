using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[CargoWise.Data.Testing.UseSnapshotProtection]
	sealed class OrderConcurrencyHandlingTest : TestCase
	{
		#region CreateAndLinkPreAdviceToOrder when pre advice has already been created by another user

		public void TestCreateAndLinkPreAdviceToOrder_PreAdviceCreated_TargetShipment()
		{
			using (var mock = Res.UseMockData())
			{
				mock.Put("0500a28b-f631-44a1-882f-ee11add5d7fb", new ResourceStringData("0500a28b-f631-44a1-882f-ee11add5d7fb", "货运"));

				var notifier = new JobShipmentPreplanningTest.TestNotificationSubscriber();

				var preAdvice = AssertCreateAndLinkPreAdviceToOrder_PreAdviceCreated(notifier, Order.TargetObjectForCreate.Shipment);

				AssertEquals("message was shown to the user",
	string.Format(@"This order is attached to the {0} pre-advice. The created 货运 will be for that pre-advice and not just this one order.

Do you want to continue?", preAdvice.EF_PreshipID),
				notifier.LastMessage);
			}
		}

		public void TestCreateAndLinkPreAdviceToOrder_PreAdviceCreated_TargetDeclaration()
		{
			using (var mock = Res.UseMockData())
			{
				mock.Put("5ea4b6fe-86f2-4073-9df1-2860d35097f2", new ResourceStringData("5ea4b6fe-86f2-4073-9df1-2860d35097f2", "报关单"));
				var notifier = new JobShipmentPreplanningTest.TestNotificationSubscriber();

				var preAdvice = AssertCreateAndLinkPreAdviceToOrder_PreAdviceCreated(notifier, Order.TargetObjectForCreate.Declaration);

				AssertEquals("message was shown to the user",
					string.Format(
						@"This order is attached to the {0} pre-advice. The created 报关单 will be for that pre-advice and not just this one order.

Do you want to continue?", preAdvice.EF_PreshipID),
					notifier.LastMessage);
			}
		}

		public void TestCreateAndLinkPreAdviceToOrder_PreAdviceCreated_TargetPreAdvice()
		{
			var notifier = new JobShipmentPreplanningTest.TestNotificationSubscriber();

			AssertCreateAndLinkPreAdviceToOrder_PreAdviceCreated(notifier, Order.TargetObjectForCreate.PreAdvice);
			AssertEquals("message was shown to the user", "This order is already part of a Shipment Pre Advice.", notifier.LastMessage);
		}

		JobShipmentPreplanning AssertCreateAndLinkPreAdviceToOrder_PreAdviceCreated(INotifications notifier, Order.TargetObjectForCreate targetObjectForCreate)
		{
			var order = Factory.New<Order>();

			var someRandomOrg = Factory.New<OrgHeader>();
			someRandomOrg.OH_Code = "XXX";
			someRandomOrg.MainAddress.Address1 = "Random Address 1";

			order.JD_MasterWaybill = "MASTER";
			order.BuyerPK = someRandomOrg.PK;
			order.JD_OH_Carrier = someRandomOrg.PK;
			order.JD_OH_ReceivingAgent = someRandomOrg.PK;
			order.JD_OH_SendingAgent = someRandomOrg.PK;
			order.JD_RL_NKPortOfLoading = "AUSYD";
			order.JD_RL_NKPortOfDischarge = "USLAX";
			order.JD_TransportMode = Core.Constants.TransportModes.Sea;

			Factory.Save();

			var otherFactory = CreateNewIsolatedFactory();

			var orderOnOtherFactory = otherFactory.Load<Order>(order.PK);
			var preAdviceOnOtherFactory = orderOnOtherFactory.CreateAndLinkPreAdviceToOrder(notifier, targetObjectForCreate);

			AssertNoExceptionThrown("should handle concurrency error", () => order.CreateAndLinkPreAdviceToOrder(notifier, targetObjectForCreate));

			AssertEquals("order has no pending changes", false, order.HasChanges);
			AssertNotNull("order has pre advice created", order.PreAdvice);
			AssertEquals("order's pre advice is the same as created by another user", preAdviceOnOtherFactory.PK, order.PreAdvice.PK);

			return order.PreAdvice;
		}

		#endregion

		#region CreateAndLinkPreAdviceToOrder when order has changed by another user

		public void TestCreateAndLinkPreAdviceToOrder_OrderChanged_TargetShipment()
		{
			var notifier = new JobShipmentPreplanningTest.TestNotificationSubscriber();

			AssertCreateAndLinkPreAdviceToOrder_OrderChanged(notifier, Order.TargetObjectForCreate.Shipment);
			AssertNull("no message was shown to the user", notifier.LastMessage);
		}

		public void TestCreateAndLinkPreAdviceToOrder_OrderChanged_TargetDeclaration()
		{
			var notifier = new JobShipmentPreplanningTest.TestNotificationSubscriber();

			AssertCreateAndLinkPreAdviceToOrder_OrderChanged(notifier, Order.TargetObjectForCreate.Declaration);
			AssertNull("no message was shown to the user", notifier.LastMessage);
		}

		public void TestCreateAndLinkPreAdviceToOrder_OrderChanged_TargetPreAdvice()
		{
			var notifier = new JobShipmentPreplanningTest.TestNotificationSubscriber();

			AssertCreateAndLinkPreAdviceToOrder_OrderChanged(notifier, Order.TargetObjectForCreate.PreAdvice);
			AssertNull("no message was shown to the user", notifier.LastMessage);
		}

		void AssertCreateAndLinkPreAdviceToOrder_OrderChanged(INotifications notifier, Order.TargetObjectForCreate targetObjectForCreate)
		{
			var order = Factory.New<Order>();

			var org1 = Factory.New<OrgHeader>();
			org1.OH_Code = "ZZZ";
			org1.MainAddress.OA_Address1 = "ZZZ Address 1";

			var org2 = Factory.New<OrgHeader>();
			org2.OH_Code = "XXX";
			org2.MainAddress.OA_Address1 = "XXX Address 1";

			order.JD_MasterWaybill = "MASTER";
			order.BuyerPK = org1.PK;
			order.JD_OH_Carrier = org1.PK;
			order.JD_OH_ReceivingAgent = org1.PK;
			order.JD_OH_SendingAgent = org1.PK;
			order.JD_RL_NKPortOfLoading = "AUSYD";
			order.JD_RL_NKPortOfDischarge = "USLAX";
			order.JD_TransportMode = Core.Constants.TransportModes.Sea;

			Factory.Save();

			var otherFactory = CreateNewIsolatedFactory();

			var orderOnOtherFactory = otherFactory.Load<Order>(order.PK);
			orderOnOtherFactory.BuyerPK = org2.PK;
			otherFactory.Save();

			AssertNoExceptionThrown("should handle concurrency error", () => order.CreateAndLinkPreAdviceToOrder(notifier, targetObjectForCreate));

			AssertEquals("order has no pending changes", false, order.HasChanges);
			AssertNotNull("order has pre advice created", order.PreAdvice);
		}

		#endregion

		public void TestReturnValueOnConcurrencyError()
		{
			var order = Factory.New<Order>();

			var someRandomOrg = Factory.New<OrgHeader>();
			someRandomOrg.OH_Code = "XXX";
			someRandomOrg.MainAddress.Address1 = "Random Address 1";

			order.JD_MasterWaybill = "MASTER";
			order.BuyerPK = someRandomOrg.PK;
			order.JD_OH_Carrier = someRandomOrg.PK;
			order.JD_OH_ReceivingAgent = someRandomOrg.PK;
			order.JD_OH_SendingAgent = someRandomOrg.PK;
			order.JD_RL_NKPortOfLoading = "AUSYD";
			order.JD_RL_NKPortOfDischarge = "USLAX";
			order.JD_TransportMode = Core.Constants.TransportModes.Sea;

			Factory.Save();

			var otherFactory = CreateNewIsolatedFactory();

			var notifier = new JobShipmentPreplanningTest.TestNotificationSubscriber();

			var orderOnOtherFactory = otherFactory.Load<Order>(order.PK);
			var preAdviceOnOtherFactory = orderOnOtherFactory.CreateAndLinkPreAdviceToOrder(notifier, Order.TargetObjectForCreate.PreAdvice);

			var preAdvice = order.CreateAndLinkPreAdviceToOrder(notifier, Order.TargetObjectForCreate.PreAdvice);

			AssertNull("should return null from HandleExistingPreAdvice", preAdvice);
			AssertNotNull("order has pre advice created", order.PreAdvice);
			AssertEquals("order's pre advice is the same as created by another user", preAdviceOnOtherFactory.PK, order.PreAdvice.PK);
		}

		#region Implementation

		BusinessObjectFactory Factory
		{
			get { return factory ?? (factory = CreateNewIsolatedFactory()); }
		}

		BusinessObjectFactory factory;

		BusinessObjectFactory CreateNewIsolatedFactory()
		{
			return new BusinessObjectFactory
			{
				RefreshEnabled = false
			};
		}

		#endregion
	}
}
