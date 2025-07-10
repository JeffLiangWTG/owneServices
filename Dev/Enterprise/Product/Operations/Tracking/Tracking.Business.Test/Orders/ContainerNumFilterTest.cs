using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.Tracking.Business
{
	sealed class ContainerNumFilterTest : TestCaseWithFactory
	{
		public void TestOrderFilterFromJobOrderLineDeliverContainer()
		{
			ZQuery query = new ZQuery();
			OrdersFilterBusinessObject_Old_ForWeb filterBO = (OrdersFilterBusinessObject_Old_ForWeb)(new WebFilterBusinessObjectFactory(new BusinessObjectFactory()).Load(typeof(OrdersFilterBusinessObject_Old_ForWeb)));
			filterBO.AddContainerNumFilter(query, SQLComparisonOperator.Equal, new ZString("1"));
			OrderCollection collection = new OrderCollection(Factory, query);
			AssertEquals("Should conatin order 1", true, collection.Contains(Order1));
			AssertEquals("Should not conatin order 2", false, collection.Contains(Order2));
			AssertEquals("Should not conatin order 3", false, collection.Contains(Order3));
		}

		public void TestOrderFilterFromJobOrderContainer()
		{
			ZQuery query = new ZQuery();
			OrdersFilterBusinessObject_Old_ForWeb filterBO = (OrdersFilterBusinessObject_Old_ForWeb)(new WebFilterBusinessObjectFactory(new BusinessObjectFactory()).Load(typeof(OrdersFilterBusinessObject_Old_ForWeb)));
			filterBO.AddContainerNumFilter(query, SQLComparisonOperator.Equal, new ZString("2"));
			OrderCollection collection = new OrderCollection(Factory, query);
			AssertEquals("Should not conatin order 1", false, collection.Contains(Order1));
			AssertEquals("Should conatin order 2", true, collection.Contains(Order2));
			AssertEquals("Should not conatin order 3", false, collection.Contains(Order3));
		}

		public void TestOrderFilterFromJobContainer()
		{
			ZQuery query = new ZQuery();
			OrdersFilterBusinessObject_Old_ForWeb filterBO = (OrdersFilterBusinessObject_Old_ForWeb)(new WebFilterBusinessObjectFactory(new BusinessObjectFactory()).Load(typeof(OrdersFilterBusinessObject_Old_ForWeb)));
			filterBO.AddContainerNumFilter(query, SQLComparisonOperator.Equal, new ZString("3"));
			OrderCollection collection = new OrderCollection(Factory, query);
			AssertEquals("Should not conatin order 1", false, collection.Contains(Order1));
			AssertEquals("Should not conatin order 2", false, collection.Contains(Order2));
			AssertEquals("Should conatin order 3", true, collection.Contains(Order3));
		}

		public void TestOrderFilterFromJobContainer_MaxLength()
		{
			var overlengthContainerNumber = new ZString('1', JobContainerSchema.JC_ContainerNum.MaxLength + JobOrderContainerSchema.J1_ContainerNumber.MaxLength + JobOrderLineDeliverContainerSchema.J5_ContainerNum.MaxLength);

			var query = new ZQuery();
			var filterBO = new WebFilterBusinessObjectFactory(Factory).Load<OrdersFilterBusinessObject_Old_ForWeb>();
			filterBO.AddContainerNumFilter(query, SQLComparisonOperator.Contains, overlengthContainerNumber);
			var collection = new OrderCollection(Factory, query);
			AssertEquals(0, collection.Count);

			var expectedSubFilter1 = $"JC_ContainerNum = '{overlengthContainerNumber.Substring(0, JobContainerSchema.JC_ContainerNum.MaxLength)}'";
			var expectedSubFilter2 = $"J1_ContainerNumber = '{overlengthContainerNumber.Substring(0, JobOrderContainerSchema.J1_ContainerNumber.MaxLength)}'";
			var expectedSubFilter3 = $"J5_ContainerNum = '{overlengthContainerNumber.Substring(0, JobOrderLineDeliverContainerSchema.J5_ContainerNum.MaxLength)}'";

			AssertContains(expectedSubFilter1, query.LiteralTextADO, true);
			AssertContains(expectedSubFilter2, query.LiteralTextADO, true);
			AssertContains(expectedSubFilter3, query.LiteralTextADO, true);
		}

		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();

			Order1 = Factory.New<Order>();
			Order2 = Factory.New<Order>();
			Order3 = Factory.New<Order>();

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "AAA";

			Order1.BuyerPK = orgHeader.PK;
			Order2.BuyerPK = orgHeader.PK;
			Order3.BuyerPK = orgHeader.PK;
			Order1.JD_OrderNumber = new ZString("1");
			Order2.JD_OrderNumber = new ZString("2");
			Order3.JD_OrderNumber = new ZString("3");

			var orderLine = Factory.New<OrderLine>();
			orderLine.JO_JD = Order1.PK;
			var orderLineDelivery = Factory.New<OrderLineDelivery>();
			orderLineDelivery.J4_JO = orderLine.PK;
			var orderLineDeliverContainer = Factory.New<OrderLineDeliverContainer>();
			orderLineDeliverContainer.J5_J4 = orderLineDelivery.PK;
			orderLineDeliverContainer.J5_ContainerNum = "1";

			var orderContainer = Factory.New<OrderContainer>();
			orderContainer.J1_ParentID = Order2.PK;
			orderContainer.J1_ParentTableCode = JobOrderHeaderSchema.Constants.Prefix;
			orderContainer.J1_ContainerNumber = "2";

			var shipment = CommonShipment.New(Factory);
			Order3.JD_JS = shipment.PK;
			var packLine = shipment.OuterPackLines.AddNew();

			var jobContainerPackPivot = Factory.New<JobContainerPackPivot>();
			var container = Factory.New<CommonContainer>();
			jobContainerPackPivot.J6_JL = packLine.PK;
			jobContainerPackPivot.J6_JC = container.PK;
			container.JC_ContainerNum = "3";

			Factory.Save();
		}

		Order Order1;
		Order Order2;
		Order Order3;

		#endregion

	}
}
