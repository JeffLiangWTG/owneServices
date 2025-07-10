using CargoWise.Types;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class XsdOrderMilestoneDateHelperTest : BaseFreightTest
	{
		public void TestToEstimatedActualDates()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "AAA";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "BBB";

			var currentDate = ZDateTime.Now.ToSmallDateTimeFloor();
			var order = Factory.NewWithValidTestData<Order>();
			order.JD_OrderNumber = "1";
			order.SupplierPK = org2.PK;
			order.BuyerPK = org1.PK;

			Factory.Save();

			var milestoneTimes = new Xsd.MilestoneDates()
			{
				Actual = currentDate.AddDays(10),
				Estimated = currentDate.AddDays(1)
			};

			XsdOrderMilestoneDateHelper.ToEstimatedActualDates(milestoneTimes, order.JD_EstimateUserDate1Info, order.JD_ActualUserDate1Info);

			AssertEquals(currentDate.AddDays(1), order.JD_EstimateUserDate1);
			AssertEquals(currentDate.AddDays(10), order.JD_ActualUserDate1);

			milestoneTimes.Actual = new ZDateTime(2366, 03, 15);
			milestoneTimes.Estimated = new ZDateTime(2366, 06, 15);

			XsdOrderMilestoneDateHelper.ToEstimatedActualDates(milestoneTimes, order.JD_EstimateUserDate2Info, order.JD_ActualUserDate2Info);

			Assert(order.JD_EstimateUserDate2.IsEmpty);
			Assert(order.JD_ActualUserDate2.IsEmpty);

			milestoneTimes.Actual = ZDateTime.Empty;
			milestoneTimes.Estimated = ZDateTime.Empty;

			XsdOrderMilestoneDateHelper.ToEstimatedActualDates(milestoneTimes, order.JD_EstimateUserDate3Info, order.JD_ActualUserDate3Info);

			Assert(order.JD_EstimateUserDate3.IsEmpty);
			Assert(order.JD_ActualUserDate3.IsEmpty);
		}

		public void TestToEstimatedActualDates_UpdateMilestonesAndTriggersDates()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "AAA";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_Code = "BBB";

			var currentDate = ZDateTime.Now.ToSmallDateTimeFloor();
			var order = Factory.NewWithValidTestData<Order>();
			order.JD_OrderNumber = "1";
			order.SupplierPK = org2.PK;
			order.BuyerPK = org1.PK;

			Factory.Save();

			var milestoneTimes = new Xsd.MilestoneDates()
			{
				Actual = currentDate.AddDays(10),
				Estimated = currentDate.AddDays(1)
			};

			XsdOrderMilestoneDateHelper.ToEstimatedActualDates(milestoneTimes, order, Events.Arrival);

			AssertEquals(currentDate.AddDays(10), order.GetMilestoneActualDate(Events.Arrival).ToZDateTime());
			AssertEquals(currentDate.AddDays(1), order.GetMilestoneEstimatedDate(Events.Arrival).ToZDateTime());

			milestoneTimes.Actual = new ZDateTime(2366, 03, 15);
			milestoneTimes.Estimated = new ZDateTime(2366, 06, 15);

			XsdOrderMilestoneDateHelper.ToEstimatedActualDates(milestoneTimes, order, Events.Arrival);

			AssertEquals(currentDate.AddDays(10), order.GetMilestoneActualDate(Events.Arrival).ToZDateTime());
			AssertEquals(currentDate.AddDays(1), order.GetMilestoneEstimatedDate(Events.Arrival).ToZDateTime());

			milestoneTimes.Actual = currentDate.AddDays(16);
			milestoneTimes.Estimated = ZDateTime.Empty;

			XsdOrderMilestoneDateHelper.ToEstimatedActualDates(milestoneTimes, order, Events.Arrival);

			AssertEquals(currentDate.AddDays(16), order.GetMilestoneActualDate(Events.Arrival).ToZDateTime());
			AssertEquals(ZDateTime.Empty, order.GetMilestoneEstimatedDate(Events.Arrival).ToZDateTime());

			milestoneTimes.Actual = ZDateTime.Empty;
			milestoneTimes.Estimated = currentDate.AddDays(20);

			XsdOrderMilestoneDateHelper.ToEstimatedActualDates(milestoneTimes, order, Events.Arrival);

			AssertEquals(ZDateTime.Empty, order.GetMilestoneActualDate(Events.Arrival).ToZDateTime());
			AssertEquals(currentDate.AddDays(20), order.GetMilestoneEstimatedDate(Events.Arrival).ToZDateTime());
		}
	}
}
