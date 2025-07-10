using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration.TransportBooking;
using Enterprise.Integration.TransportConsignment;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportCommon.Business.Common;
using Enterprise.TransportCommon.Shared;

namespace Enterprise.TransportCommon.Business.Testing
{
	public abstract class DtbTransportCollectionTest<TBusinessObject, TCollection> : ActiveBusinessObjectCollectionTestCase<TCollection>
			where TBusinessObject : DtbTransport
			where TCollection : DtbTransportCollection<TBusinessObject>
	{
		#region TestCollectionIncludesOnlyCorrectTypesOfJobs

		public void TestCollectionIncludesOnlyCorrectTypesOfJobs()
		{
			var bookingConsolidation = (DtbBookingConsolidation)Factory.New<IDtbBookingConsolidation>();
			bookingConsolidation.KB_JobType = TransportConsolidationJobTypes.Codes.Booking;
			var booking = bookingConsolidation.Bookings.AddNew();

			var hvlvBookingConsolidation = (DtbBookingConsolidation)Factory.New<IDtbBookingConsolidation>();
			hvlvBookingConsolidation.KB_JobType = TransportConsolidationJobTypes.Codes.HighVolumeLowValue;
			var hvlvBooking = hvlvBookingConsolidation.Bookings.AddNew();

			var consignmentConsolidation = (DtbTransportConsolidation)Factory.New<IDtbConsignmentConsolidation>();
			var consignment = consignmentConsolidation.Bookings.AddNew();
			Factory.Save();

			var collection = base.GetCollectionToTest();
			var expected = new[] { booking, hvlvBooking, consignment }.Where(o => GetParentType_ForTesting() == ((AutoDtbBooking)o).KM_JobType);
			AssertContainsExactElementsInAnyOrder(expected, collection);
		}

		protected abstract string GetParentType_ForTesting();

		#endregion

		// interfaces

		#region IDtbTransportCollection

		public void TestIDtbTransportCollection()
		{
			var transport = GetTransportToAddToTheCollection();
			Factory.Save();

			var collection = GetCollectionToTest();
			if (((IBindingList)collection).AllowNew)
			{
				collection.Add(transport);
			}

			AssertContainsExactElementsInAnyOrder(new[] { transport }, collection);
			AssertContainsExactElementsInAnyOrder(new[] { transport }, ((IDtbTransportCollection)collection).Typed);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var transport = GetTransportToAddToTheCollection();
			Factory.Save(); // Consolidation + Transport must be in database
			return transport;
		}

		protected abstract TBusinessObject GetTransportToAddToTheCollection();

		#endregion
	}
}
