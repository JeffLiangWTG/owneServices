using System;
using System.Data.SqlClient;
using CargoWise.eHub.DataAccess.Integration;

namespace CargoWise.eHub.DataAccess.Sql
{
	[Serializable]
	class ExceptionsAccessor : IExceptionsAccessor
	{
		public ExceptionsAccessor() : this(new eServices.eHubDataAccess.Sql.ExceptionsAccessor()) { }

		public ExceptionsAccessor(eServices.eHubDataAccess.Integration.IExceptionsAccessor exceptionsAccessor)
		{
			this.exceptionsAccessor = exceptionsAccessor;
		}

		private readonly eServices.eHubDataAccess.Integration.IExceptionsAccessor exceptionsAccessor;

		public void SubmitError(Guid errorPK, string source, string errorType, string description)
			=> exceptionsAccessor.SubmitError(errorPK, source, errorType, description);

		public void SubmitError(Guid errorPK, string source, string errorType, string description, Guid inboxPK, Guid outboxPK)
			=> exceptionsAccessor.SubmitError(errorPK, source, errorType, description, inboxPK, outboxPK);

		public void SubmitErrorAndUpdateStatus(Guid errorPK, string source, string errorType, string description, Guid inboxPK, Guid outboxPK, Guid inboxMessageTrackingID, Guid outboxMessageTrackingID, SqlConnection connection)
			=> exceptionsAccessor.SubmitErrorAndUpdateStatus(errorPK, source, errorType, description, inboxPK, outboxPK, inboxMessageTrackingID, outboxMessageTrackingID, connection);

		public void SubmitErrorAndUpdateStatus(Guid errorPK, string source, string errorType, string description, Guid inboxPK, Guid outboxPK, Guid inboxMessageTrackingID, Guid outboxMessageTrackingID, SqlConnection connection, bool alerted)
			=> exceptionsAccessor.SubmitErrorAndUpdateStatus(errorPK, source, errorType, description, inboxPK, outboxPK, inboxMessageTrackingID, outboxMessageTrackingID, connection, alerted);
	}
}
