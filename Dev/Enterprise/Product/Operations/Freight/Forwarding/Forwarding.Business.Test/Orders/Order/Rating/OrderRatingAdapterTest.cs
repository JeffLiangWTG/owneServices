using System.Linq;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class OrderRatingAdapterTest : BaseFreightTest
	{
		public void TestInvoicingSupporter()
		{
			var order = Factory.NewWithValidTestData<Order>();
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			var adapter = new OrderRatingAdapter(order);

			AssertNull(adapter.InvoicingSupporter);

			order.JD_JS = shipment.PK;
			AssertEquals(shipment.InvoicingSupporter, adapter.InvoicingSupporter);
		}

		public void TestJobDatesProvider()
		{
			var order = Factory.NewWithValidTestData<Order>();
			var adapter = new OrderRatingAdapter(order);

			AssertType<OrderJobDatesProvider>(adapter.JobDatesProvider);
		}

		public void TestAdapterTypeAndId()
		{
			var order = Factory.NewWithValidTestData<Order>();
			var adapter = new OrderRatingAdapter(order);

			AssertEquals(AdapterType.Order, adapter.AdapterType);
			AssertEquals(order.JD_OrderNumberAndSplit, adapter.OperationalJobCode);
		}

		public void TestChargeCodeGroups()
		{
			var order = Factory.NewWithValidTestData<Order>();
			var adapter = new OrderRatingAdapter(order);

			Env.Registry.Rating.SetFreightRatedCodes("AAA");
			Env.Registry.Rating.SetBrokerageRatedCodes("BBB");

			AssertContainsExactElementsInAnyOrder(new[] { "AAA", "BBB" }, adapter.ChargeCodeGroups.Cast<string>());
		}

		public void TestConsumerType()
		{
			var order = Factory.NewWithValidTestData<Order>();
			var adapter = new OrderRatingAdapter(order);

			AssertEquals(JobInvoicingConsumerTypes.Shipment, adapter.ConsumerType);
		}

		public void TestMergeCharges()
		{
			var order = Factory.NewWithValidTestData<Order>();
			var adapter = new OrderRatingAdapter(order);

			AssertEquals(MergeChargeOptions.CrossAdapter, adapter.MergeCharges);
		}

		public void TestRateTypeToUse()
		{
			var order = Factory.NewWithValidTestData<Order>();
			var adapter = new OrderRatingAdapter(order);

			AssertEquals(RateType.Forwarding, adapter.RateTypeToUse);
		}

		public void TestOrigin()
		{
			var order = Factory.NewWithValidTestData<Order>().With(jD_RL_NKGoodsAvailableAt: "UAIEV", jD_RL_NKGoodsDeliveredTo: "AUSYD");

			AssertEquals("UAIEV", new OrderRatingAdapter(order).Origin.Code);
		}

		public void TestDestination()
		{
			var order = Factory.NewWithValidTestData<Order>().With(jD_RL_NKGoodsAvailableAt: "UAIEV", jD_RL_NKGoodsDeliveredTo: "AUSYD");

			AssertEquals("AUSYD", new OrderRatingAdapter(order).Destination.Code);
		}

		public void TestCarrier()
		{
			var someCarrier = Factory.NewWithValidTestData<OrgHeader>();
			var order = Factory.NewWithValidTestData<Order>().With(carrier: someCarrier);

			AssertEquals(someCarrier.PK, new OrderRatingAdapter(order).Carrier.PK);
		}

		public void TestDeliveryAddress()
		{
			var aaaAddress = Factory.New<OrgAddress>().With(oA_Code: "AAA", oA_Address1: "A");
			var bbbAddress = Factory.New<OrgAddress>().With(oA_Code: "BBB", oA_Address1: "B");
			var someOrg = Factory.NewWithValidTestData<OrgHeader>().With(address: aaaAddress).With(address: bbbAddress);

			var order = Factory.NewWithValidTestData<Order>().With(buyer: someOrg);

			order.With(jD_OA_NKDeliverAddress_NI: "AAA");
			AssertEquals("A", new OrderRatingAdapter(order).DeliveryAddress.E2_Address1);

			order.With(jD_OA_NKDeliverAddress_NI: "BBB");
			AssertEquals("B", new OrderRatingAdapter(order).DeliveryAddress.E2_Address1);

			order.With(jD_OA_NKDeliverAddress_NI: "CCC");
			AssertEquals(string.Empty, new OrderRatingAdapter(order).DeliveryAddress.E2_Address1);
		}

		public void TestConsignorDeliveryAddress()
		{
			var aaaAddress = Factory.New<OrgAddress>().With(oA_Code: "AAA", oA_Address1: "A");
			var bbbAddress = Factory.New<OrgAddress>().With(oA_Code: "BBB", oA_Address1: "B");
			var someOrg = Factory.NewWithValidTestData<OrgHeader>().With(address: aaaAddress).With(address: bbbAddress);

			var order = Factory.NewWithValidTestData<Order>().With(supplier: someOrg);

			order.With(jD_OA_NKPickupAddress_NI: "AAA");
			AssertEquals("A", new OrderRatingAdapter(order).PickupAddress.E2_Address1);

			order.With(jD_OA_NKPickupAddress_NI: "BBB");
			AssertEquals("B", new OrderRatingAdapter(order).PickupAddress.E2_Address1);

			order.With(jD_OA_NKPickupAddress_NI: "CCC");
			AssertEquals(string.Empty, new OrderRatingAdapter(order).PickupAddress.E2_Address1);
		}

		public void TestCreditors()
		{
			var someReceivingAgent = Factory.NewWithValidTestData<OrgHeader>();
			var someSendingAgent = Factory.NewWithValidTestData<OrgHeader>();
			var order = Factory.NewWithValidTestData<Order>().With(receivingAgent: someReceivingAgent, sendingAgent: someSendingAgent);

			var creditors = new OrderRatingAdapter(order).Creditors;
			AssertContainsExactElementsInAnyOrder(new[] { someReceivingAgent, someSendingAgent }, creditors.AllOrgs);
		}

		public void TestMeasures()
		{
			var order = Factory.NewWithValidTestData<Order>()
				.With(jD_ContainerMode: Constants.ContainerModes.FCL)
				.With(jD_ActualWeight: 2.0m, jD_UnitOfWeight: Constants.Weight.Kilograms)
				.With(jD_ActualVolume: 3.0m, jD_UnitOfVolume: Constants.Volume.CubicMetres)
				.With(jD_Packs: 5, jD_F3_NKPackType: Constants.PkgUnit.Box);

			order.PlannedContainers.AddNew().With(containerType: "20GP", j1_ContainerCount: 5);
			order.PlannedContainers.AddNew().With(containerType: "20GP", j1_ContainerCount: 2);
			order.PlannedContainers.AddNew().With(containerType: "40GP", j1_ContainerCount: 3);

			var rateableMeasures = (RateableMeasureSet)new OrderRatingAdapter(order).RateableMeasures;
			AssertEquals("Weight", 2m, rateableMeasures.GetActual(MeasureType.Weight));
			AssertEquals("Weight Unit", Constants.Weight.Kilograms, rateableMeasures.GetUnit(MeasureType.Weight));
			AssertEquals("Volume", 3m, rateableMeasures.GetActual(MeasureType.Volume));
			AssertEquals("Volume Unit", Constants.Volume.CubicMetres, rateableMeasures.GetUnit(MeasureType.Volume));
			AssertEquals("Packages", 5m, rateableMeasures.GetActual(MeasureType.Package));
			AssertEquals("Package Type", Constants.PkgUnit.Box, rateableMeasures.GetUnit(MeasureType.Package));
			AssertEquals("Containers count", 10m, rateableMeasures.GetActual(MeasureType.ContainerCount));
		}

		public void TestMonetaryValues()
		{
			var order = Factory.NewWithValidTestData<Order>().With(jD_RX_NKOrderCurrency: "UAH");
			order.OrderLines.AddNew().With(jO_LinePrice: 10m);
			order.OrderLines.AddNew().With(jO_LinePrice: 20m);

			var monetaryValues = new OrderRatingAdapter(order).MonetaryValues;
			AssertEquals("Number of values", 1, monetaryValues.Values.Count);

			var goodsValue = monetaryValues.Values[MoneyType.ValueType.GoodsValue];
			AssertEquals("Number of goods values", 1, goodsValue.Count);

			var goodValue = goodsValue.First();
			AssertEquals("Value", 30m, goodValue.Amount);
			AssertEquals("Currency", "UAH", goodValue.Currency.Code);
		}

		public void TestServiceLevel()
		{
			var order = Factory.NewWithValidTestData<Order>().With(jD_RS_NKServiceLevel_NI: "AAA");
			var level = new OrderRatingAdapter(order).ServiceLevel;

			AssertEquals("Infos count", 1, level.ServiceLevelData.Length);
			AssertEquals("Service Level", "AAA", level.ServiceLevelData[0].ServiceLevel);
			AssertEquals("Service Level Type", ServiceLevelType.Client, level.ServiceLevelData[0].ServiceLevelType);
		}
	}
}
