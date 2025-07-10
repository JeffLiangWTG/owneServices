using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportCommon.Business.Testing
{
	internal class DtbTransportProcessTaskLoadStrategyTest : TestCaseWithFactory
	{
		#region TestGetTypeForLoad_InvalidID

		public void TestGetTypeForLoad_InvalidID()
		{
			var strategy = new DtbTransportProcessTaskLoadStrategy();
			var expectedType = ObjectFactory.GetType<IDtbBookingProcessTask>();
			AssertEquals(expectedType, strategy.GetTypeForLoad(DtbBookingSchema.Constants.Prefix, ZGuid.Empty, Factory));
			AssertEquals(expectedType, strategy.GetTypeForLoad(DtbBookingSchema.Constants.Prefix, ZGuid.Invalid, Factory));
		}

		#endregion

		#region TestGetTypeForLoad

		public void TestGetTypeForLoad()
		{
			var strategy = new DtbTransportProcessTaskLoadStrategy();
			var bookingConsolidation = (DtbBookingConsolidation)Factory.New<IDtbBookingConsolidation>();
			var booking = Factory.New<DtbBooking>();
			bookingConsolidation.Bookings.Add(booking);

			var hvlvBookingConsolidation = Factory.New<DtbBookingConsolidation>();
			hvlvBookingConsolidation.KB_JobType = TransportConsolidationJobTypes.Codes.HighVolumeLowValue;
			var hvlvBooking = Factory.New<DtbBooking>();
			hvlvBookingConsolidation.Bookings.Add(hvlvBooking);
			Factory.Save();

			AssertEquals(ObjectFactory.GetType<IDtbBookingProcessTask>(), strategy.GetTypeForLoad(booking.TablePrefix, booking.PK, Factory));
			AssertEquals(ObjectFactory.GetType<IDtbBookingProcessTask>(), strategy.GetTypeForLoad(hvlvBooking.TablePrefix, hvlvBooking.PK, Factory));
		}

		#endregion

		#region TestAddAdditionalParentFilters

		public void TestAddAdditionalParentFilters()
		{
			var bookingConsolidation = Factory.New<DtbBookingConsolidation>();
			var booking = Factory.New<DtbBooking>();
			bookingConsolidation.Bookings.Add(booking);
			var bookingTask = booking.WorkflowItems.AddNew();

			var hvlvBookingConsolidation = Factory.New<DtbBookingConsolidation>();
			hvlvBookingConsolidation.KB_JobType = TransportConsolidationJobTypes.Codes.HighVolumeLowValue;
			var hvlvBooking = Factory.New<DtbBooking>();
			hvlvBookingConsolidation.Bookings.Add(hvlvBooking);
			var hvlvBookingTask = hvlvBooking.WorkflowItems.AddNew();
			Factory.Save();

			var tasks1 = Factory.Load<ProcessTask>(GetNewQueryForTestAddAdditionalParentFilters(WorkflowDescriptors.DtbBookingWorkflowDescriptorCode));
			AssertEquals(2, tasks1.Length);
			AssertContainsExactElementsInAnyOrder(new[] { bookingTask, hvlvBookingTask }, tasks1);

			var tasks2 = Factory.Load<ProcessTask>(GetNewQueryForTestAddAdditionalParentFilters(WorkflowDescriptors.DtbBookingConsignmentWorkflowDescriptorCode));
			AssertEquals(0, tasks2.Length);
		}

		#endregion

		#region GetNewQueryForTestAddAdditionalParentFilters

		ZDBOnlyQuery GetNewQueryForTestAddAdditionalParentFilters(string workflowTypeCode)
		{
			var descriptor = WorkflowDescriptors.Instance.TryGetValueSafe(workflowTypeCode);
			var query = new ZDBOnlyQuery(typeof(ProcessTask));
			var subQuery = new ZDBOnlySubQuery(typeof(DtbTransport), ProcessTasksSchema.P9_ParentID);
			var strategy = new DtbTransportProcessTaskLoadStrategy();

			strategy.AddAdditionalParentFilters(descriptor, subQuery);
			query.AddSubQuery(subQuery, JoinCondition.And);

			return query;
		}

		#endregion
	}
}
