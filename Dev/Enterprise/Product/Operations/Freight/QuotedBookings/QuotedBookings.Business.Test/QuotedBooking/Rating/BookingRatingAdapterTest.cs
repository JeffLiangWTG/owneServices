using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;

namespace Enterprise.Freight.QuotedBookings.Business.Test
{
	public class BookingRatingAdapterTest : TestCaseWithFactory
	{
		public void TestParentJob()
		{
			var quote = QuotedBooking.CreateNewQuote(Factory, QuotedBooking.QuoteState.ApprovedAndAccepted);
			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			var glbDepartment = Factory.New<GlbDepartment>();
			glbDepartment.GE_Code = "CCC";
			jobHeader.JH_GE = glbDepartment.PK;
			jobHeader.JH_ParentID = quote.PK;
			var shipment = Factory.New<CommonShipment>();
			var quotedBooking = QuotedBooking.New(quote.PK, shipment.PK, Factory);
			var adapter = new BookingRatingAdapterForTest(quotedBooking);
			AssertEquals("CCC", adapter.ParentJobForTest.Department.GE_Code);
		}

		public void TestRateableMeasures_Containers()
		{
			var quote = Factory.NewWithValidTestData<Quote>();
			quote.TH_OneTimeQuote = true;

			var booking = Factory.NewWithValidTestData<ForwardingShipment>();
			booking.JS_UnitOfWeight = "T";
			booking.JS_UnitOfVolume = "CC";

			var quotedBooking = QuotedBooking.New(quote.PK, booking.PK, Factory);
			quotedBooking.Mode = "ULD";

			#region 20GP AUTO

			var cnt20GPAuto = quotedBooking.QuotedBookingContainers.AddNew();
			cnt20GPAuto.JC_ContainerCount = 2;
			cnt20GPAuto.JC_RC = Helper.Containers["20GP"].PK;
			cnt20GPAuto.JC_RH_NKContainerCommodityCode = "AUTO";

			var cnt20GPAuto_2 = quotedBooking.QuotedBookingContainers.AddNew();
			cnt20GPAuto_2.JC_ContainerCount = 8;
			cnt20GPAuto_2.JC_RC = Helper.Containers["20GP"].PK;
			cnt20GPAuto_2.JC_RH_NKContainerCommodityCode = "AUTO";

			var packline1 = quotedBooking.Booking.OuterPackLines.AddNew();
			packline1.JL_F3_NKPackType = "PLT";
			packline1.JL_PackageCount = 10;
			packline1.JL_ActualWeight = 1.0;
			packline1.JL_ActualWeightUQ = "T";
			packline1.JL_ActualVolume = 5;
			packline1.JL_ActualVolumeUQ = "M3";
			packline1.JL_RC_ContainerType = Helper.Containers["20GP"].PK;
			packline1.JL_RH_NKCommodityCode = "AUTO";

			var packline2 = quotedBooking.Booking.OuterPackLines.AddNew();
			packline2.JL_F3_NKPackType = "BOX";
			packline2.JL_PackageCount = 5;
			packline2.JL_ActualWeight = 100000;
			packline2.JL_ActualWeightUQ = "G";
			packline2.JL_ActualVolume = 2;
			packline2.JL_ActualVolumeUQ = "M3";
			packline2.JL_RC_ContainerType = Helper.Containers["20GP"].PK;
			packline2.JL_RH_NKCommodityCode = "AUTO";

			#endregion

			#region 20GP BOAT

			var cnt20GPBoat = quotedBooking.QuotedBookingContainers.AddNew();
			cnt20GPBoat.JC_ContainerCount = 4;
			cnt20GPBoat.JC_RC = Helper.Containers["20GP"].PK;
			cnt20GPBoat.JC_RH_NKContainerCommodityCode = "BOAT";

			var packline3 = quotedBooking.Booking.OuterPackLines.AddNew();
			packline3.JL_F3_NKPackType = "PLT";
			packline3.JL_PackageCount = 3;
			packline3.JL_ActualWeight = 300;
			packline3.JL_ActualWeightUQ = "KG";
			packline3.JL_ActualVolume = 2000000;
			packline3.JL_ActualVolumeUQ = "CC";
			packline3.JL_RC_ContainerType = Helper.Containers["20GP"].PK;
			packline3.JL_RH_NKCommodityCode = "BOAT";

			#endregion

			#region 40GP AUTO

			var cnt40GPAuto = quotedBooking.QuotedBookingContainers.AddNew();
			cnt40GPAuto.JC_ContainerCount = 6;
			cnt40GPAuto.JC_RC = Helper.Containers["40GP"].PK;
			cnt40GPAuto.JC_RH_NKContainerCommodityCode = "AUTO";

			var packline4 = quotedBooking.Booking.OuterPackLines.AddNew();
			packline4.JL_F3_NKPackType = "PLT";
			packline4.JL_PackageCount = 3;
			packline4.JL_ActualWeight = 500;
			packline4.JL_ActualWeightUQ = "KG";
			packline4.JL_ActualVolume = 6;
			packline4.JL_ActualVolumeUQ = "M3";
			packline4.JL_RC_ContainerType = Helper.Containers["40GP"].PK;
			packline4.JL_RH_NKCommodityCode = "AUTO";

			#endregion

			#region 20FR AUTO

			var cnt20FRAuto = quotedBooking.QuotedBookingContainers.AddNew();
			cnt20FRAuto.JC_ContainerCount = 7;
			cnt20FRAuto.JC_RC = Helper.Containers["20FR"].PK;
			cnt20FRAuto.JC_RH_NKContainerCommodityCode = "AUTO";

			#endregion

			#region Unrelated Lines which don't match any container

			var packline5 = quotedBooking.Booking.OuterPackLines.AddNew();
			packline5.JL_F3_NKPackType = "PLT";
			packline5.JL_PackageCount = 3;
			packline5.JL_ActualWeight = 1500;
			packline5.JL_ActualWeightUQ = "KG";
			packline5.JL_ActualVolume = 2;
			packline5.JL_ActualVolumeUQ = "M3";
			packline5.JL_RC_ContainerType = Helper.Containers["20GP"].PK;
			packline5.JL_RH_NKCommodityCode = "GEN";

			#endregion

			var quotedBookingAdapter = new QuotedBookingRatingAdapter(quotedBooking);
			var measures = (RateableMeasureSet)quotedBookingAdapter.RateableMeasures;

			var containers = measures.GetPartList(MeasureType.ContainerCount).OfType<IRateableContainer>();

			RatingTestUtils.AssertArraysAreEquivalent(new[]
			{
				new
				{
					ContainerTypePk = Helper.Containers["20GP"].PK.ToGuid(),
					CommodityCode = "AUTO",
					ContainerCount = 10,
					ContainerWeightInKG = 1100m,
					ContainerVolumeInM3 = 7m
				},
				new
				{
					ContainerTypePk = Helper.Containers["20GP"].PK.ToGuid(),
					CommodityCode = "BOAT",
					ContainerCount = 4,
					ContainerWeightInKG = 300m,
					ContainerVolumeInM3 = 2m
				},
				new
				{
					ContainerTypePk = Helper.Containers["40GP"].PK.ToGuid(),
					CommodityCode = "AUTO",
					ContainerCount = 6,
					ContainerWeightInKG = 500m,
					ContainerVolumeInM3 = 6m
				},
				new
				{
					ContainerTypePk = Helper.Containers["20FR"].PK.ToGuid(),
					CommodityCode = "AUTO",
					ContainerCount = 7,
					ContainerWeightInKG = 0m,
					ContainerVolumeInM3 = 0m
				}
			}, containers.ToArray());
		}

		protected TestHelper Helper => helper ?? (helper = new TestHelper(Factory));
		TestHelper helper;
	}
}
