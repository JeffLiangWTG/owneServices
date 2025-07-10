using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.QuotedBookings.Business
{
	sealed class QuotedBookingConcurrencyCheck : ConcurrencyChecker
	{
		QuotedBookingConcurrencyCheck(BusinessObjectFactory factory) : base(factory)
		{
			registrationDateTime = ZDateTime.UtcNow;
		}

		readonly ZDateTime registrationDateTime;
		HashSet<ZGuid> bookingsToCheck;
		Dictionary<ZGuid, string> quotedBookingsCreationStacks;

		public static bool Register(BusinessObjectFactory factory, ZGuid bookingPK, string quotedBookingCreationStack)
		{
			if (!bookingPK.IsValid)
			{
				return false;
			}

			var concurrencyCheck = GetRegisteredQuotedBookingConcurrencyCheck(factory);

			if (concurrencyCheck == null)
			{
				concurrencyCheck = new QuotedBookingConcurrencyCheck(factory);
				factory.SaveInTransactionActions.Add(concurrencyCheck);
			}
			else if (concurrencyCheck.bookingsToCheck.Contains(bookingPK))
			{
				return false;
			}

			if (concurrencyCheck.bookingsToCheck == null)
			{
				concurrencyCheck.bookingsToCheck = new HashSet<ZGuid>();
			}

			if (concurrencyCheck.quotedBookingsCreationStacks == null)
			{
				concurrencyCheck.quotedBookingsCreationStacks = new Dictionary<ZGuid, string>();
			}

			concurrencyCheck.bookingsToCheck.Add(bookingPK);
			concurrencyCheck.quotedBookingsCreationStacks[bookingPK] = quotedBookingCreationStack;

			return true;
		}

		public static bool IsRegisteredForBooking(BusinessObjectFactory factory, ZGuid bookingPK) =>
			bookingPK.IsValid
			&& GetRegisteredQuotedBookingConcurrencyCheck(factory) is QuotedBookingConcurrencyCheck concurrencyCheck
			&& concurrencyCheck.bookingsToCheck.Contains(bookingPK);

		static QuotedBookingConcurrencyCheck GetRegisteredQuotedBookingConcurrencyCheck(BusinessObjectFactory factory)
		{
			return factory?
				.SaveInTransactionActions
				.OfType<QuotedBookingConcurrencyCheck>()
				.FirstOrDefault();
		}

		protected override void SaveInTransactionCore() => RunConcurrencyCheck();

		#region Implementation

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "missing literal string")]
		void RunConcurrencyCheck()
		{
			var query = new ZQuery
			{
				FetchOnlyFromLocalCache = true
			};

			query.AddToFilter(JobShipmentSchema.PK, bookingsToCheck);

			bool ShouldRunConcurrencyCheck(ForwardingShipment booking)
			{
				return booking != null
					&& booking.IsInDatabase
					&& !booking.IsDeleted
					&& booking.JS_IsBooking
					&& !booking.JS_IsForwardRegistered;
			}

			var bookingPKsToCheck = Factory.Load<ForwardingShipment>(query)
				.Where(ShouldRunConcurrencyCheck)
				.Select(booking => booking.PK)
				.ToArray();

			if (bookingPKsToCheck.Length == 0)
			{
				return;
			}

			var convertedBookings = GetConvertedShipmentPKs(bookingPKsToCheck);

			if (convertedBookings.Length > 0)
			{
				var message = Res.GetString("e729b8bf-9838-4333-9737-28440917554f", "Another user has converted the booking into a shipment.");
				var heading = Res.GetString("ffb9f4f3-38f3-4178-9c54-a3ae455f9e25", "Booking concurrency error");

				var exc = new ZConcurrencyCheckFailureException(message, heading, shouldReprocess: true);
				exc.Data["RegistrationDateTime(UTC)"] = registrationDateTime.ToISO8601String();
				exc.Data["ExceptionDateTime(UTC)"] = ZDateTime.UtcNow.ToISO8601String();

				foreach (var convertedBookingPK in convertedBookings)
				{
					exc.Data[$"QuotedBookingCreationCallStack [PK:{convertedBookingPK}]"] = quotedBookingsCreationStacks.TryGetValue(convertedBookingPK, out var stack)
						? stack
						: "missing";
				}

				throw exc;
			}
		}

		ZGuid[] GetConvertedShipmentPKs(IReadOnlyCollection<ZGuid> bookingPKs)
		{
			const string sql = @"SELECT
JS_PK
FROM dbo.JobShipment
WHERE JS_PK IN (SELECT Value FROM @bookingPKs)
	AND JS_IsBooking = 1
	AND JS_IsForwardRegistered = 1";

			var sqlParams = new ZSqlParameterCollection
			{
				ZSqlParameter.New("@bookingPKs", bookingPKs, JobShipmentSchema.PK, true)
			};

			var collection = new DynamicBusinessObjectCollection(Factory);
			collection.Load(sql, sqlParams);

			return collection
				.Select(bizObj => (ZGuid)bizObj[JobShipmentSchema.Constants.PK])
				.ToArray();
		}

		#endregion
	}
}
