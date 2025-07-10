using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.QuotedBookings.Business
{
	class QuotedBookingLoggingStrategy : IBusinessObjectStrategy
	{
		public void BeforeSuccessfulDelete(BusinessObject businessObject) { }

		public DeleteDetails DeleteDetails(BusinessObject businessObject) => null;

		public void OnDelete(BusinessObject businessObject) { }

		public void OnSaving(BusinessObject businessObject) { }

		public void OnSaved(BusinessObject businessObject, bool saveSucceeded) { }

		public void OnFactorySaving(BusinessObject businessObject)
		{
			if (businessObject is QuotedBooking quotedBooking
				&& !quotedBooking.IsForwardRegistered
				&& quotedBooking.GetFromBooking && quotedBooking.Booking.IsInDatabase
				&& (quotedBooking.Booking.HasChanges ||
					((ICustomFieldProvider)quotedBooking).GetCustomBusinessObject().HasChanges))
			{
				var query = GetEDTLogsQuery(quotedBooking);
				var edtLogs = quotedBooking.Factory.Load<StmALog>(query);
				if (!edtLogs.Any(l => !l.IsInDatabase))
				{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					quotedBooking.Booking.Logs.AddNew(Events.EditedARecord);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}
			}
		}

		public void OnFactorySaved(BusinessObject businessObject, bool saveSucceeded) { }

		public void OnSaveRollback(BusinessObject businessObject) { }

		public void FetchForLoad(BusinessObject businessObject) { }

		ZQuery GetEDTLogsQuery(QuotedBooking quotedBooking)
		{
			var result = new ZQuery { FetchOnlyFromLocalCache = true };
			result.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.EditedARecordCode);
			result.AddToFilter(StmALogSchema.SL_Table, quotedBooking.Booking.PKSchemaColumn.TableName);
			result.AddToFilter(StmALogSchema.SL_Parent, quotedBooking.Booking.PK);
			return result;
		}

		public void OnSavingInObjectsWithLateChanges(BusinessObject businessObject) { }
	}
}
