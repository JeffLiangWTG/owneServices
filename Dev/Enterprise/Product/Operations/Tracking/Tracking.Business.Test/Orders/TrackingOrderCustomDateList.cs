/*using System.Collections.Generic;

namespace Enterprise.Tracking.Business
{
	public class TrackingOrderCustomDateList : List<TrackingOrderCustomDate>
	{
		public TrackingOrderCustomDate this[string LabelName]
		{
			get
			{
				foreach (TrackingOrderCustomDate TrackingOrderCustomDate in this)
				{
					if (TrackingOrderCustomDate.LabelName == LabelName)
						return TrackingOrderCustomDate;
				}

				return null;
			}
		}
	}
}

#region Test
#if DEBUG

namespace Enterprise.Tracking.Business.Testing
{
	using NUnit.Framework;
	using Enterprise.Freight.Forwarding.Orders.Business;

	public class TrackingOrderCustomDateListTest : TestCase
	{
		public void TestCustomIndexer()
		{
			TrackingOrderCustomDateList customDateList = new TrackingOrderCustomDateList();
			TrackingOrderCustomDate customDate = new TrackingOrderCustomDate();
			customDate.ActualDateField = Order.Schema.JD_ActualUserDate1;
			customDate.EstimatedDateField = Order.Schema.JD_EstimateUserDate1;
			customDate.LabelName = "OrderHeader.UserTrackDate1";
			customDate.Caption = "Picking Date";

			customDateList.Add(customDate);

			AssertEquals("The indexer must return the TrackingOrderCustomDate.", customDate, customDateList[customDate.LabelName]);
		}
	}
}

#endif
#endregion*/
