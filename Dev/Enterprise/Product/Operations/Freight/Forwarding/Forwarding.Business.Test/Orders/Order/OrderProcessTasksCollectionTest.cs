using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(OrderProcessTasksCollection))]
	sealed class OrderProcessTasksCollectionTest : ProcessTaskCollectionTest<OrderProcessTasksCollection>
	{
		public void TestSupportsContactAndAddress()
		{
			AssertEquals("Orders Task DOES NOT support contacts and addresses", false, Order.WorkflowItems.SupportsContactAndAddress);
		}

		#region OriginCountry / DestinationCountry

		public void TestOriginCountry()
		{
			Order.JD_RL_NKGoodsAvailableAt = "MYPKG";
			AssertEquals("MY", Collection.OriginCountry);
		}

		public void TestDestinationCountry()
		{
			Order.JD_RL_NKGoodsDeliveredTo = "MYPKG";
			AssertEquals("MY", Collection.DestinationCountry);
		}

		#endregion

		#region IsConditionMet

		public void TestIsCondition1Met_ForOriginDifferentFromFirstLoad()
		{
			Order.JD_RL_NKPortOfLoading = "AUSYD";
			Order.JD_RL_NKGoodsAvailableAt = "AUSYD";
			AssertEquals(false, Collection.IsCondition1Met(OrderWorkflowCondition1CodeList.Codes.OriginDifferentFromFirstLoad));

			Order.JD_RL_NKGoodsAvailableAt = "AUMEL";
			AssertEquals(true, Collection.IsCondition1Met(OrderWorkflowCondition1CodeList.Codes.OriginDifferentFromFirstLoad));
		}

		public void TestIsCondition1Met_ForDestinationDifferentFromFinalDischarge()
		{
			Order.JD_RL_NKPortOfDischarge = "AUSYD";
			Order.JD_RL_NKGoodsDeliveredTo = "AUSYD";
			AssertEquals(false, Collection.IsCondition1Met(OrderWorkflowCondition1CodeList.Codes.DestinationDifferentFromFinalDischarge));

			Order.JD_RL_NKGoodsDeliveredTo = "AUMEL";
			AssertEquals(true, Collection.IsCondition1Met(OrderWorkflowCondition1CodeList.Codes.DestinationDifferentFromFinalDischarge));
		}

		public void TestIsCondition2Met_ForLCL()
		{
			Order.JD_ContainerMode = Core.Constants.ContainerModes.LCL;
			AssertEquals(true, Collection.IsCondition2Met(Core.Constants.ContainerModes.LCL, ""));
			Order.JD_ContainerMode = Core.Constants.ContainerModes.BuyersConsol;
			AssertEquals(true, Collection.IsCondition2Met(Core.Constants.ContainerModes.LCL, ""));
			Order.JD_ContainerMode = Core.Constants.ContainerModes.FCL;
			AssertEquals(false, Collection.IsCondition2Met(Core.Constants.ContainerModes.LCL, ""));
		}

		public void TestIsCondition2Met_ForFCL()
		{
			Order.JD_ContainerMode = Core.Constants.ContainerModes.FCL;
			AssertEquals(true, Collection.IsCondition2Met(Core.Constants.ContainerModes.FCL, ""));
			Order.JD_ContainerMode = Core.Constants.ContainerModes.FreightAllKind;
			AssertEquals(false, Collection.IsCondition2Met(Core.Constants.ContainerModes.FCL, ""));
		}

		public void TestIsCondition2Met_ForTransportMode()
		{
			Order.JD_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals(true, Collection.IsCondition2Met(Core.Constants.TransportModes.Air, ""));
			Order.JD_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals(false, Collection.IsCondition2Met(Core.Constants.TransportModes.Air, ""));

			Order.JD_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals(false, Collection.IsCondition2Met(Core.Constants.TransportModes.Sea, ""));
			Order.JD_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals(true, Collection.IsCondition2Met(Core.Constants.TransportModes.Sea, ""));
		}

		#endregion

		#region Implementation

		protected override OrderProcessTasksCollection GetCollectionToTestCore()
		{
			return new OrderProcessTasksCollection(Order);
		}

		Order Order
		{
			get
			{
				if (order == null)
				{
					order = Factory.NewWithValidTestData<Order>();
				}
				return order;
			}
		}
		Order order;

		#endregion
	}
}
