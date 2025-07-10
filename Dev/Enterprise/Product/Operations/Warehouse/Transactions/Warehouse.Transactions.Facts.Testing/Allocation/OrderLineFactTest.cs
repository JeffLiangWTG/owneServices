using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Warehouse.Integration;
using Moq;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Business.ProductWarehouseAllocation;

namespace Enterprise.Warehouse.Transactions.Facts.Testing
{
	class OrderLineFactTest : TestCaseWithFactory
	{
		public void TestConstructor_NullArguments_Throws()
		{
			var orderLine = new Mock<IWhsPickableDocketLine>();
			orderLine.Setup(ol => ol.PK).Returns(ZGuid.NewZGuid());

			var order = new Mock<IWhsPickableDocket>();
			order.Setup(ol => ol.WD_RequiredDate).Returns(ZDateTimeOffset.Today);

			var pick = Mock.Of<IWhsPick>();
			var organisation = Mock.Of<IOrganisationFact>();
			var product = Mock.Of<IAllocationProductFact>();
			AssertExceptionThrown<ArgumentNullException>(() => new OrderLineFact(null, order.Object, organisation, product, null, ZGuid.NewZGuid(), null, null));
			AssertExceptionThrown<ArgumentNullException>(() => new OrderLineFact(orderLine.Object, null, organisation, product, null, ZGuid.NewZGuid(), null, null));
			AssertExceptionThrown<ArgumentNullException>(() => new OrderLineFact(orderLine.Object, order.Object, null, product, null, ZGuid.NewZGuid(), null, null));
			AssertExceptionThrown<ArgumentNullException>(() => new OrderLineFact(orderLine.Object, order.Object, organisation, null, null, ZGuid.NewZGuid(), null, null));

			AssertNoExceptionThrown(() => new OrderLineFact(orderLine.Object, order.Object, organisation, product, null, ZGuid.NewZGuid(), null, null));
		}

		public void TestProperties()
		{
			var pk = ZGuid.NewZGuid();
			var orderedInvPK = ZGuid.NewZGuid();

			var orderLine = new Mock<IWhsPickableDocketLine>();
			orderLine.Setup(ol => ol.PK).Returns(pk);
			orderLine.Setup(ol => ol.QuantityNotMet).Returns(5m);
			orderLine.Setup(ol => ol.WE_PickGroup).Returns(2);

			var requiredDate = ZDateTimeOffset.Today;

			var order = new Mock<IWhsPickableDocket>();
			order.Setup(o => o.WD_DocketSubType).Returns("KEK");
			order.Setup(o => o.WD_ExternalReference).Returns("W1");
			order.Setup(o => o.WD_CustomerReference).Returns("C1");
			order.Setup(o => o.WD_RequiredDate).Returns(requiredDate);
			order.Setup(o => o.WD_RS_NKServiceLevel).Returns("VIP");
			order.Setup(o => o.WD_PL_NKCarrierServiceLevel).Returns("QIK");
			order.Setup(o => o.TransportZoneName).Returns("CITY");
			order.Setup(o => o.WD_PickPriority).Returns(3);
			order.Setup(o => o.SalesChannelCode).Returns("RFT");

			var clientFact = Mock.Of<IOrganisationFact>();
			var productFact = Mock.Of<IAllocationProductFact>();
			var consigneeFact = Mock.Of<IOrganisationFact>();

			var orderLineFact = new OrderLineFact(orderLine.Object, order.Object, clientFact, productFact, consigneeFact, orderedInvPK, null, null);

			// Loop twice to make sure we store these values rather than wrapping the bizo values (bad for performance)
			for (var i = 0; i < 2; i++)
			{
				CombineAssertions(() =>
				{
					AssertEquals(nameof(OrderLineFact.PK), pk, orderLineFact.PK);
					AssertEquals(nameof(OrderLineFact.OrderedInventoryPK), orderedInvPK, orderLineFact.OrderedInventoryPK);

					AssertEquals(nameof(OrderLineFact.Client), clientFact, orderLineFact.Client.Fact);
					AssertEquals(nameof(OrderLineFact.Product), productFact, orderLineFact.Product.Fact);
					AssertEquals(nameof(OrderLineFact.Consignee), consigneeFact, orderLineFact.Consignee.Fact);
					AssertNull(nameof(OrderLineFact.TransportCompany), orderLineFact.TransportCompany.Fact);
					AssertNull(nameof(OrderLineFact.DistributionCentre), orderLineFact.DistributionCentre.Fact);

					AssertEquals(nameof(OrderLineFact.Quantity), 5m, orderLineFact.Quantity);
					AssertEquals(nameof(OrderLineFact.PickPriority), 3, orderLineFact.PickPriority);
					AssertEquals(nameof(OrderLineFact.OrderType), "KEK", orderLineFact.OrderType);
					AssertEquals(nameof(OrderLineFact.OrderNumber), "W1", orderLineFact.OrderNumber);
					AssertEquals(nameof(OrderLineFact.CustomerReference), "C1", orderLineFact.CustomerReference);
					AssertEquals(nameof(OrderLineFact.RequiredDate), requiredDate.ToZDateTime(), orderLineFact.RequiredDate);
					AssertEquals(nameof(OrderLineFact.ServiceLevel), "VIP", orderLineFact.ServiceLevel);
					AssertEquals(nameof(OrderLineFact.CarrierServiceLevel), "QIK", orderLineFact.CarrierServiceLevel);
					AssertEquals(nameof(OrderLineFact.TransportZone), "CITY", orderLineFact.TransportZone);
					AssertEquals(nameof(OrderLineFact.SalesChannelCode), "RFT", orderLineFact.SalesChannelCode);
					AssertEquals(nameof(OrderLineFact.PickGroupsCode), "2", orderLineFact.PickGroupsCode);
				});
			}

			AssertNull(nameof(OrderLineFact.ExpiryDateFilter), orderLineFact.ExpiryDateFilter);
			AssertNull(nameof(OrderLineFact.ExpiryDateFilterKey), orderLineFact.ExpiryDateFilterKey);

			orderLine.Verify(ol => ol.PK, Times.Once);
			orderLine.Verify(ol => ol.QuantityNotMet, Times.Once);
			orderLine.Verify(ol => ol.WE_PickGroup, Times.Once);
			order.Verify(o => o.WD_DocketSubType, Times.Once);
			order.Verify(o => o.WD_ExternalReference, Times.Once);
			order.Verify(o => o.WD_CustomerReference, Times.Once);
			order.Verify(o => o.WD_RequiredDate, Times.Once);
			order.Verify(o => o.WD_RS_NKServiceLevel, Times.Once);
			order.Verify(o => o.WD_PL_NKCarrierServiceLevel, Times.Once);
			order.Verify(o => o.TransportZoneName, Times.Once);
			order.Verify(o => o.SalesChannelCode, Times.Once);
			order.Verify(o => o.WD_PickPriority, Times.Once);
		}

		public void TestTransportCompany()
		{
			var pk = ZGuid.NewZGuid();
			var orderedInvPK = ZGuid.NewZGuid();

			var orderLine = new Mock<IWhsPickableDocketLine>();
			orderLine.Setup(ol => ol.PK).Returns(pk);

			var order = new Mock<IWhsPickableDocket>();
			order.Setup(o => o.WD_RequiredDate).Returns(ZDateTimeOffset.Today);

			var clientFact = Mock.Of<IOrganisationFact>();
			var productFact = Mock.Of<IAllocationProductFact>();
			var consigneeFact = Mock.Of<IOrganisationFact>();

			var transportCompanyFact = Mock.Of<IOrganisationFact>();

			var orderLineFact = new OrderLineFact(
				orderLine.Object,
				order.Object,
				clientFact,
				productFact,
				consigneeFact,
				orderedInvPK,
				transportCompany: transportCompanyFact,
				distributionCentre: null);

			AssertEquals(nameof(OrderLineFact.TransportCompany), transportCompanyFact, orderLineFact.TransportCompany.Fact);
		}

		public void TestDistributionCentre()
		{
			var pk = ZGuid.NewZGuid();
			var orderedInvPK = ZGuid.NewZGuid();

			var orderLine = new Mock<IWhsPickableDocketLine>();
			orderLine.Setup(ol => ol.PK).Returns(pk);

			var order = new Mock<IWhsPickableDocket>();
			order.Setup(o => o.WD_RequiredDate).Returns(ZDateTimeOffset.Today);

			var clientFact = Mock.Of<IOrganisationFact>();
			var productFact = Mock.Of<IAllocationProductFact>();
			var consigneeFact = Mock.Of<IOrganisationFact>();

			var distributionCentreFact = Mock.Of<IOrganisationFact>();

			var orderLineFact = new OrderLineFact(
				orderLine.Object,
				order.Object,
				clientFact,
				productFact,
				consigneeFact,
				orderedInvPK,
				transportCompany: null,
				distributionCentre: distributionCentreFact);

			AssertEquals(nameof(OrderLineFact.DistributionCentre), distributionCentreFact, orderLineFact.DistributionCentre.Fact);
		}

		public void TestIsCustomsTransaction() => TestIsCustomsTransaction(isCustomsTransaction: true);
		public void TestIsCustomsTransaction_False() => TestIsCustomsTransaction(isCustomsTransaction: false);

		void TestIsCustomsTransaction(bool isCustomsTransaction)
		{
			var pk = ZGuid.NewZGuid();
			var orderedInvPK = ZGuid.NewZGuid();

			var orderLine = new Mock<IWhsPickableDocketLine>();
			orderLine.Setup(ol => ol.PK).Returns(pk);

			var order = new Mock<IWhsPickableDocket>();
			order.Setup(o => o.WD_RequiredDate).Returns(ZDateTimeOffset.Today);
			order.Setup(o => o.IsCustomsTransaction).Returns(isCustomsTransaction);

			var clientFact = Mock.Of<IOrganisationFact>();
			var productFact = Mock.Of<IAllocationProductFact>();
			var consigneeFact = Mock.Of<IOrganisationFact>();

			var distributionCentreFact = Mock.Of<IOrganisationFact>();

			var orderLineFact = new OrderLineFact(
				orderLine.Object,
				order.Object,
				clientFact,
				productFact,
				consigneeFact,
				orderedInvPK,
				transportCompany: null,
				distributionCentre: distributionCentreFact);

			AssertEquals(nameof(OrderLineFact.IsCustomsTransaction), isCustomsTransaction, orderLineFact.IsCustomsTransaction);
		}

		public void TestIsWorkOrder_Order() => TestIsWorkOrder(orderType: "ORD", expectedIsWorkOrder: false);
		public void TestIsWorkOrder_Order_CaseInsensitive() => TestIsWorkOrder(orderType: "oRd", expectedIsWorkOrder: false);
		public void TestIsWorkOrder_WorkOrder() => TestIsWorkOrder(orderType: "WOR", expectedIsWorkOrder: true);
		public void TestIsWorkOrder_WorkOrder_CaseInsensitive() => TestIsWorkOrder(orderType: "wOr", expectedIsWorkOrder: true);
		public void TestIsWorkOrder_DynamicWorkOrder() => TestIsWorkOrder(orderType: "DWO", expectedIsWorkOrder: true);
		public void TestIsWorkOrder_DynamicWorkOrder_CaseInsensitive() => TestIsWorkOrder(orderType: "dWo", expectedIsWorkOrder: true);

		void TestIsWorkOrder(string orderType, bool expectedIsWorkOrder)
		{
			var pk = ZGuid.NewZGuid();
			var orderedInvPK = ZGuid.NewZGuid();

			var orderLine = new Mock<IWhsPickableDocketLine>();
			orderLine.Setup(ol => ol.PK).Returns(pk);

			var order = new Mock<IWhsPickableDocket>();
			order.Setup(o => o.WD_RequiredDate).Returns(ZDateTimeOffset.Today);
			order.Setup(o => o.WD_DocketType).Returns(orderType);

			var clientFact = Mock.Of<IOrganisationFact>();
			var productFact = Mock.Of<IAllocationProductFact>();
			var consigneeFact = Mock.Of<IOrganisationFact>();

			var distributionCentreFact = Mock.Of<IOrganisationFact>();

			var orderLineFact = new OrderLineFact(
				orderLine.Object,
				order.Object,
				clientFact,
				productFact,
				consigneeFact,
				orderedInvPK,
				transportCompany: null,
				distributionCentre: distributionCentreFact);

			AssertEquals(nameof(OrderLineFact.IsWorkOrder), expectedIsWorkOrder, orderLineFact.IsWorkOrder);
		}

		public void TestIsHeldInventoryOrder_AvailableInventoryOrder() => TestIsHeldInventoryOrder_Core(orderedHeldCode: "", expectedIsHeldInventoryOrder: false);
		public void TestIsHeldInventoryOrder_HeldInventoryOrder() => TestIsHeldInventoryOrder_Core(orderedHeldCode: "HEL", expectedIsHeldInventoryOrder: true);
		public void TestIsHeldInventoryOrder_HeldInventoryOrder_Damaged() => TestIsHeldInventoryOrder_Core(orderedHeldCode: "DAM", expectedIsHeldInventoryOrder: true);

		void TestIsHeldInventoryOrder_Core(string orderedHeldCode, bool expectedIsHeldInventoryOrder)
		{
			var pk = ZGuid.NewZGuid();
			var orderedInvPK = ZGuid.NewZGuid();

			var orderLine = new Mock<IWhsPickableDocketLine>();
			orderLine.Setup(ol => ol.PK).Returns(pk);
			orderLine.Setup(ol => ol.WE_WHC_NKOrderedHeldCode).Returns(orderedHeldCode);

			var order = new Mock<IWhsPickableDocket>();
			order.Setup(o => o.WD_RequiredDate).Returns(ZDateTimeOffset.Today);

			var clientFact = Mock.Of<IOrganisationFact>();
			var productFact = Mock.Of<IAllocationProductFact>();
			var consigneeFact = Mock.Of<IOrganisationFact>();

			var distributionCentreFact = Mock.Of<IOrganisationFact>();

			var orderLineFact = new OrderLineFact(
				orderLine.Object,
				order.Object,
				clientFact,
				productFact,
				consigneeFact,
				orderedInvPK,
				transportCompany: null,
				distributionCentre: distributionCentreFact);

			AssertEquals(nameof(OrderLineFact.IsHeldInventoryOrder), expectedIsHeldInventoryOrder, orderLineFact.IsHeldInventoryOrder);
		}
	}
}
