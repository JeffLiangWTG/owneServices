using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;

namespace Enterprise.Tracking.Business
{
	[HttpContextEnabledTest]
	sealed class OrderFilterBusinessObjectDateRangesFilterTest : TestCaseWithFactory
	{
		public void TestInvalidFilterTypeSelection()
		{
			FilterBO.JD_DateFilterType = "an invalid type";
			AssertEquals("Errors exist on Date Filter Type Filter", 1, FilterBO.JD_DateFilterTypeInfo.GetErrors().Count());
		}

		public void TestWithinDates()
		{
			AssertEquals(true, IsFound(Date1, Date3));
		}

		public void TestSameDates()
		{
			AssertEquals(true, IsFound(Date2, Date2));
		}

		public void TestTooEarly()
		{
			AssertEquals(false, IsFound(Date3, Date3));
		}

		public void TestTooLate()
		{
			AssertEquals(false, IsFound(Date1, Date1));
		}

		public void TestJustBeforeMidnightOnToDate()
		{
			NewOrder.JD_BookingConfDate = Date2.AddHours(23).AddMinutes(59).AddSeconds(59);
			AssertEquals(true, IsFound(Date1, Date2));
		}

		public void TestWithDatesButWithoutDateType()
		{
			FilterBO.JD_DateFilterType = FilterBusinessObject.QueryDeciderNoSelectionCode;
			AssertEquals("Should match despite date range, because no date type is specified", true, IsFound(Date1, Date1));
		}

		public void TestWithoutDatesButWithDateType()
		{
			AssertEquals("Should match despite date type, because no date range is specified", true, IsFound(ZDateTime.Empty, ZDateTime.Empty));
		}

		public void TestFilterByReqInStore()
		{
			Order order1 = Factory.New<Order>();
			order1.BuyerPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			ZDateTime date = ZDateTime.Now;
			Factory.Save();
			order1.JD_DeliveryRequiredBy = date;
			FilterBO.JD_DateFilterType = OrdersConstants.DateFilterTypes.ReqInStore;
			OrderCollection collection = new OrderCollection(Factory);
			collection.AdditionalFilter = FilterBO.Filter;
			Assert("Should find on empty filter date", collection.Contains(order1));

			FilterBO.JD_FromDate = date.AddDays(-1);
			FilterBO.JD_ToDate = date.AddDays(1);
			collection.AdditionalFilter = FilterBO.Filter;
			Assert("Should find Order", collection.Contains(order1));

			FilterBO.JD_FromDate = date.AddDays(-4);
			FilterBO.JD_ToDate = date.AddDays(-2);
			collection.AdditionalFilter = FilterBO.Filter;
			Assert("Should not find Order", !collection.Contains(order1));
		}

		public void TestFilterByReqExWorks()
		{
			Order order1 = Factory.New<Order>();
			order1.BuyerPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			ZDateTime date = ZDateTime.Now;
			Factory.Save();
			order1.JD_ExWorksRequiredBy = date;
			FilterBO.JD_DateFilterType = OrdersConstants.DateFilterTypes.ReqExWorks;
			OrderCollection collection = new OrderCollection(Factory);
			collection.AdditionalFilter = FilterBO.Filter;
			Assert("Should find on empty filter date", collection.Contains(order1));

			FilterBO.JD_FromDate = date.AddDays(-1);
			FilterBO.JD_ToDate = date.AddDays(1);
			collection.AdditionalFilter = FilterBO.Filter;
			Assert("Should find Order", collection.Contains(order1));

			FilterBO.JD_FromDate = date.AddDays(-4);
			FilterBO.JD_ToDate = date.AddDays(-2);
			collection.AdditionalFilter = FilterBO.Filter;
			Assert("Should not find Order", !collection.Contains(order1));
		}

		public void TestReadOnlyDatesWithDateType()
		{
			AssertReadOnlyDates(false);
		}

		public void TestReadOnlyDatesWithoutDateType()
		{
			FilterBO.JD_DateFilterType = FilterBusinessObject.QueryDeciderNoSelectionCode;
			AssertReadOnlyDates(true);
		}

		public void TestReadOnlyDatesAfterRetrievingFilterSettings()
		{
			FilterFactory.Save(FilterBO);
			FilterBO = (OrdersFilterBusinessObject_Old_ForWeb)(new WebFilterBusinessObjectFactory(new BusinessObjectFactory()).Load(typeof(OrdersFilterBusinessObject_Old_ForWeb)));
			AssertReadOnlyDates(false);
		}

		public void TestContainerModeBasedOnTransportModes()
		{
			OrdersFilterBusinessObject_Old_ForWeb filterObject = (OrdersFilterBusinessObject_Old_ForWeb)FilterFactory.New(typeof(OrdersFilterBusinessObject_Old_ForWeb));
			filterObject.JD_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("The CodeAsString of FilterObject for Air transport mode must be", "LSE, ULD, CON, OTH", filterObject.JD_ContainerMode_List.CodesAsString);

			filterObject.JD_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("The CodeAsString of FilterObject for Sea transport mode must be", "FCL, LCL, BLK, LQD, BBK, ROR, OTH", filterObject.JD_ContainerMode_List.CodesAsString);

			filterObject.JD_TransportMode = Core.Constants.TransportModes.AirSea;
			AssertEquals("The CodeAsString of FilterObject for AirSea transport mode must be", "LSE, ULD, LCL, OTH", filterObject.JD_ContainerMode_List.CodesAsString);

			filterObject.JD_TransportMode = Core.Constants.TransportModes.SeaAir;
			AssertEquals("The CodeAsString of FilterObject for SeaAir transport mode must be", "LCL, LSE, ULD, OTH", filterObject.JD_ContainerMode_List.CodesAsString);

			filterObject.JD_TransportMode = Core.Constants.TransportModes.Road;
			AssertEquals("The CodeAsString of FilterObject for Road transport mode must be", "FCL, FTL, LCL, LTL, OTH", filterObject.JD_ContainerMode_List.CodesAsString);

			filterObject.JD_TransportMode = Core.Constants.TransportModes.Rail;
			AssertEquals("The CodeAsString of FilterObject for Rail transport mode must be", "FCL, LCL, BLK, LQD, BBK, OTH", filterObject.JD_ContainerMode_List.CodesAsString);

			filterObject.JD_TransportMode = Core.Constants.TransportModes.Courier;
			AssertEquals("The CodeAsString of FilterObject for Courier transport mode must be", "OBC, UNA", filterObject.JD_ContainerMode_List.CodesAsString);

			filterObject.JD_TransportMode = "";
			AssertEquals("The CodeAsString of FilterObject if any transport mode is not selected must be", "LSE, ULD, CON, OBC, UNA, MAI, FCL, FTL, LCL, LTL, BLK, LQD, BBK, OTH",
				filterObject.JD_ContainerMode_List.CodesAsString);
		}

		#region Implementation

		Order NewOrder;
		WebFilterBusinessObjectFactory FilterFactory;
		OrdersFilterBusinessObject_Old_ForWeb FilterBO;
		readonly ZDateTime Date1 = new ZDateTime(2004, 1, 1);
		readonly ZDateTime Date2 = new ZDateTime(2004, 1, 2);
		readonly ZDateTime Date3 = new ZDateTime(2004, 1, 3);

		protected override void SetUp()
		{
			base.SetUp();

			NewOrder = Factory.New<Order>();
			NewOrder.JD_OrderNumber = "myorder";
			NewOrder.BuyerPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			NewOrder.SupplierPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			NewOrder.JD_BookingConfDate = Date2;

			FilterFactory = new WebFilterBusinessObjectFactory(new BusinessObjectFactory());
			FilterBO = (OrdersFilterBusinessObject_Old_ForWeb)FilterFactory.New(typeof(OrdersFilterBusinessObject_Old_ForWeb));
			FilterBO.JD_DateFilterType = OrdersConstants.DateFilterTypes.ConfirmedDate;
		}

		bool IsFound(ZDateTime from, ZDateTime to)
		{
			FilterBO.JD_FromDate = from;
			FilterBO.JD_ToDate = to;

			Factory.Save();
			OrderCollection collection = new OrderCollection(Factory, FilterBO.Filter);
			return collection.Contains(NewOrder);
		}

		void AssertReadOnlyDates(bool expected)
		{
			AssertEquals("From Date ReadOnly", expected, FilterBO.JD_FromDateInfo.ReadOnly);
			AssertEquals("To Date ReadOnly", expected, FilterBO.JD_ToDateInfo.ReadOnly);
		}

		#endregion
	}
}
