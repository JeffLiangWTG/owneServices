using System;
using System.Data.SqlClient;

namespace CargoWise.eHub.DataAccess.Integration
{
	public interface IExceptionsAccessor
	{
		void SubmitError(Guid errorPK, string source, string errorType, string description, Guid inboxPK, Guid outboxPK);
		void SubmitError(Guid errorPK, string source, string errorType, string description);
		void SubmitErrorAndUpdateStatus(Guid errorPK, string source, string errorType, string description, Guid inboxPK, Guid outboxPK, Guid inboxMessageTrackingID, Guid outboxMessageTrackingID, SqlConnection connection);
		void SubmitErrorAndUpdateStatus(Guid errorPK, string source, string errorType, string description, Guid inboxPK, Guid outboxPK, Guid inboxMessageTrackingID, Guid outboxMessageTrackingID, SqlConnection connection, bool alerted);
	}
}
