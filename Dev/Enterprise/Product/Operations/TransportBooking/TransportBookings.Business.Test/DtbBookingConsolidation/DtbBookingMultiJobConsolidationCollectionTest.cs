using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportCommon.Shared;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Business.Test
{
	[TestedType(typeof(DtbBookingMultiJobConsolidationCollection))]
	public class DtbBookingMultiJobConsolidationCollectionTest : ActiveBusinessObjectCollectionTestCase<DtbBookingMultiJobConsolidationCollection>
	{
		public void TestAllowNew()
		{
			AssertEquals(false, ((IBindingList)new DtbBookingMultiJobConsolidationCollection(Factory)).AllowNew);
			AssertEquals(false, ((IBindingList)new DtbBookingMultiJobConsolidationCollection(Factory, new ZQuery())).AllowNew);
		}

		public void TestCollectionDoesNotIncludeBookingConsolidations()
		{
			var multiJobConsolidation = Helper.CreateConsolidationMultiJob();

			var bookingConsolidation = Helper.CreateConsolidation();
			bookingConsolidation.KB_JobType = TransportConsolidationJobTypes.Codes.Booking;
			Factory.Save();

			var collection = new DtbBookingMultiJobConsolidationCollection(Factory);
			AssertEquals(1, collection.Count);
			Assert(collection.Contains(multiJobConsolidation));
		}

		TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}

		TransportBookingTestHelper helper;

		protected override DtbBookingMultiJobConsolidationCollection GetCollectionToTest()
		{
			return new DtbBookingMultiJobConsolidationCollection(Factory);
		}
	}
}
