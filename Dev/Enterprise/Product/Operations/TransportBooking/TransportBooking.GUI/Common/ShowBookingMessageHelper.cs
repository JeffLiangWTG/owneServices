using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.Shared;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportBookings.GUI
{
	public static class ShowBookingMessageHelper
	{
		public enum MessageOperation
		{
			Open,
			Create,
			Deliver
		}

		public static bool ShowErrorIfBookingFormOpen(DtbBookingConsolidation consolidation, MessageOperation operation, DtbBooking excludingBooking = null)
		{
			return ShowErrorIfBookingFormOpen(consolidation, null, operation, excludingBooking);
		}

		public static bool ShowErrorIfBookingFormOpen(DtbBookingConsolidation consolidation, ILogger logger, MessageOperation operation, DtbBooking excludingBooking = null)
		{
			var result = false;
			var openedBookingFormCounter = ObjectFactory.Get<IOpenedBookingFormCounter>();
			var count = openedBookingFormCounter.AlreadyOpenedBookingFormCount(consolidation, excludingBooking);
			result = (count > 0);
			if (result)
			{
				LogOrShowMessage(CannotOpenCreateDeliverMessage(operation, count), logger);
			}

			return result;
		}

		static void LogOrShowMessage(string message, ILogger logger)
		{
			if (logger != null)
			{
				logger.Error(message);
			}
			else
			{
				Globals.Message.ShowWarning(message);
			}
		}

		static string CannotOpenCreateDeliverMessage(MessageOperation operation, int openedFormsCount)
		{
			string message = "";

			switch (operation)
			{
				case MessageOperation.Create:
					if (openedFormsCount > 1)
					{
						message = Res.GetString("BE0A353C-144A-4F82-A1BB-E95085B56EF5",
@"Cannot create a new Transport Booking as the existing Transport Booking screens are open.

Close the Transport Booking screens and try again.");
					}
					else
					{
						message = Res.GetString("68C19F39-8CEE-4F68-8B9F-37B3F2E7CC61",
@"Cannot create a new Transport Booking as an existing Transport Booking screen is open.

Close the Transport Booking screen and try again.");
					}
					break;

				case MessageOperation.Deliver:
					if (openedFormsCount > 1)
					{
						message = Res.GetString("D49135D5-43D5-41D7-9327-B6CA32015A8C",
@"Cannot prepare new Cartage Advices as existing Transport Booking screens are open.

Close the Transport Booking screens and try again.");
					}
					else
					{
						message = Res.GetString("DF8CC816-366A-4BE6-92DF-594C6A254A41",
@"Cannot prepare new Cartage Advice as an existing Transport Booking screen is open.

Close the Transport Booking screen and try again.");
					}
					break;

				case MessageOperation.Open:
					if (openedFormsCount > 1)
					{
						message = ResString.GetMultilingualString("AAF5754B-30E0-4961-AC57-A507A636AE74",
@"More than one Transport Booking screen belonging to the same Consolidation is open.

Close the Transport Booking screens and try again.");
					}
					else
					{
						message = ResString.GetMultilingualString("1E8D40CE-42FD-4470-B4A3-8F17231F0114",
@"A Transport Booking screen belonging to the same consolidation is open.

Close the Transport Booking screen and try again.");
					}
					break;
			}

			return message;
		}

		public static ZString GetCannotDetachMessage(IEnumerable<DtbBooking> bookings)
		{
			var result = ZString.Empty;

			var bookingsWithApportionment = new List<DtbBooking>(bookings.Where(b => HasConsolApportionmentOnBooking(b)));
			if (bookingsWithApportionment.Any())
			{
				var bookingIDs = string.Join(", ", bookingsWithApportionment.OrderBy(b => b.KM_JobID).Select(b => b.HumanReadableName));
				var consolidationID = bookingsWithApportionment[0].ConsolidationMultiJob.HumanReadableName;

				result = Res.GetString("TransportBookingsModuleButtonGrid|DetachButton_Click", @"Cannot detach the {0} from the {1} as posted/unposted apportionments exist on the consolidation.
If the apportionment is not posted, you can detach the booking after you have deleted the apportionment.
If the apportionment is posted, you can detach the booking after you have reversed the invoice with the apportioned charges and deleted the apportionment.", bookingIDs, consolidationID);
			}

			return result;
		}

		public static ZString GetCannotDetachFromMasterBookingMessage(IEnumerable<DtbBooking> bookings, DtbBooking masterBooking)
		{
			var result = ZString.Empty;
			var checker = new DtbMasterBookingVersionChecker(masterBooking);
			var unsyncedBookingJobIDs = new List<string>();
			foreach (var booking in bookings)
			{
				if (booking.KM_KM_MasterBookingInfo.OriginalValue.Equals(masterBooking.PK) && !checker.IsSubInSyncWithMaster(booking))
				{
					unsyncedBookingJobIDs.Add(booking.KM_JobID);
				}
			}

			if (unsyncedBookingJobIDs.Any())
			{
				var unsyncedBookingJobIDsString = string.Join(", ", unsyncedBookingJobIDs.OrderBy(id => id));
				result = Res.GetString("853a1e47-b253-4691-a02e-26c5e6122e65", @"Cannot detach the transport booking(s) {0} from the {1} as they are not yet in sync.
Please reload the form and try again.", unsyncedBookingJobIDsString, masterBooking.HumanReadableName);
			}

			var factory = masterBooking.Factory;
			var hasCharges = false;
			foreach (var booking in bookings)
			{
				var bookingJob = booking.Job;

				if (masterBooking != null)
				{
					hasCharges = DoesBookingHaveMasterBookingApportionedChargesForCurrentJob(factory, masterBooking, bookingJob);
					if (!hasCharges && booking.IsInDatabase)
					{
						hasCharges = DoesBookingHaveMasterBookingApportionedChargesInDB(factory, masterBooking, booking);
					}
				}
			}

			if (hasCharges)
			{
				var bookingIDs = string.Join(", ", bookings.OrderBy(b => b.KM_JobID).Select(b => b.HumanReadableName));
				result = Res.GetString("9ff522ff-d21f-491b-9f16-4f60e484cd60", @"Cannot detach the {0} from the {1} as posted/unposted apportionments exist on the master booking.
If the apportionment is not posted, you can detach the booking after you have deleted the apportionment.
If the apportionment is posted, you can detach the booking after you have reversed the invoice with the apportioned charges and deleted the apportionment.", bookingIDs, masterBooking.HumanReadableName);
			}

			return result;
		}

		public static bool DoesBookingHaveMasterBookingApportionedChargesForCurrentJob(BusinessObjectFactory factory, DtbBooking masterBooking, JobHeader bookingJob)
		{
			var result = false;

			if (bookingJob != null)
			{
				var findChargesForJobQuery = new ZQuery(JobChargeSchema.JR_JH, bookingJob.PK);
				findChargesForJobQuery.AddToFilter(JobChargeSchema.JR_E6, SQLComparisonOperator.NotEqual, DBNull.Value);
				findChargesForJobQuery.AddToFilter(JobChargeSchema.JR_OSCostAmt, SQLComparisonOperator.NotEqual, 0m);
				findChargesForJobQuery.FetchOnlyFromLocalCache = true;

				var charges = factory.Load<JobCharge>(findChargesForJobQuery);

				if (charges.Any())
				{
					var consolCostPKs = charges.Select(x => x.JR_E6).Distinct();
					var consolCostQuery = new ZQuery(JobConsolCostSchema.E6_ParentID, masterBooking.PK);
					consolCostQuery.AddToFilter(JobConsolCostSchema.PK, consolCostPKs);

					result = factory.LoadTop1<IJobConsolCost>(consolCostQuery) != null;
				}
			}

			return result;
		}

		public static bool DoesBookingHaveMasterBookingApportionedChargesInDB(BusinessObjectFactory factory, DtbBooking masterBooking, DtbBooking booking)
		{
			var chargesInDBQuery = new ZDBOnlyQuery(typeof(JobHeader));
			var jobHeaderParentPK = ((IJobHeaderParent)booking).PK;
			chargesInDBQuery.AddToFilter(JobHeaderSchema.JH_ParentID, jobHeaderParentPK);

			var jobChargeSubQuery = new ZDBOnlySubQuery(typeof(JobCharge), JobChargeSchema.JR_JH);

			var consolCostSubQuery = new ZDBOnlySubQuery(ObjectFactory.GetType<IJobConsolCost>(), JobConsolCostSchema.PK);
			consolCostSubQuery.AddToFilter(JobConsolCostSchema.E6_ParentID, masterBooking.PK);
			consolCostSubQuery.AddToFilter(JobConsolCostSchema.E6_ParentTableCode, DtbBookingSchema.Constants.Prefix);
			jobChargeSubQuery.AddSubQuery(JobChargeSchema.JR_E6, consolCostSubQuery, JoinCondition.And);
			chargesInDBQuery.AddSubQuery(jobChargeSubQuery, JoinCondition.And);

			return factory.LoadTop1<JobHeader>(chargesInDBQuery) != null;
		}

		static bool HasConsolApportionmentOnBooking(DtbBooking booking)
		{
			Argument.NotNull(booking, "booking");

			var result = false;
			var factory = booking.Factory;
			var consolidationMultiJob = booking.ConsolidationMultiJob;
			var bookingJob = booking.Job;

			if (consolidationMultiJob != null)
			{
				result = DoesBookingHaveConsolApportionedChargesForCurrentJob(factory, consolidationMultiJob, bookingJob);

				if (!result && booking.IsInDatabase)
				{
					result = DoesBookingHaveConsolApportionedChargesInDB(factory, consolidationMultiJob, booking);
				}
			}

			return result;
		}

		public static bool DoesBookingHaveConsolApportionedChargesForCurrentJob(BusinessObjectFactory factory, DtbBookingConsolidation consolidationMultiJob, JobHeader bookingJob)
		{
			var result = false;

			if (bookingJob != null)
			{
				var findChargesForJobQuery = new ZQuery(JobChargeSchema.JR_JH, bookingJob.PK);
				findChargesForJobQuery.AddToFilter(JobChargeSchema.JR_E6, SQLComparisonOperator.NotEqual, DBNull.Value);
				findChargesForJobQuery.AddToFilter(JobChargeSchema.JR_OSCostAmt, SQLComparisonOperator.NotEqual, 0m);
				findChargesForJobQuery.FetchOnlyFromLocalCache = true;

				var charges = factory.Load<JobCharge>(findChargesForJobQuery);

				if (charges.Any())
				{
					var consolCostPKs = charges.Select(x => x.JR_E6).Distinct();
					var consolCostQuery = new ZQuery(JobConsolCostSchema.E6_ParentID, consolidationMultiJob.PK);
					consolCostQuery.AddToFilter(JobConsolCostSchema.PK, consolCostPKs);

					result = factory.LoadTop1<IJobConsolCost>(consolCostQuery) != null;
				}
			}

			return result;
		}

		public static bool DoesBookingHaveConsolApportionedChargesInDB(BusinessObjectFactory factory, DtbBookingConsolidation consolidationMultiJob, DtbBooking booking)
		{
			var chargesInDBQuery = new ZDBOnlyQuery(typeof(JobHeader));
			var jobHeaderParentPK = ((IJobHeaderParent)booking).PK;
			chargesInDBQuery.AddToFilter(JobHeaderSchema.JH_ParentID, jobHeaderParentPK);

			var jobChargeSubQuery = new ZDBOnlySubQuery(typeof(JobCharge), JobChargeSchema.JR_JH);

			var consolCostSubQuery = new ZDBOnlySubQuery(ObjectFactory.GetType<IJobConsolCost>(), JobConsolCostSchema.PK);
			consolCostSubQuery.AddToFilter(JobConsolCostSchema.E6_ParentID, consolidationMultiJob.PK);
			consolCostSubQuery.AddToFilter(JobConsolCostSchema.E6_ParentTableCode, DtbBookingConsolidationSchema.Constants.Prefix);
			jobChargeSubQuery.AddSubQuery(JobChargeSchema.JR_E6, consolCostSubQuery, JoinCondition.And);
			chargesInDBQuery.AddSubQuery(jobChargeSubQuery, JoinCondition.And);

			return factory.LoadTop1<JobHeader>(chargesInDBQuery) != null;
		}
	}
}
