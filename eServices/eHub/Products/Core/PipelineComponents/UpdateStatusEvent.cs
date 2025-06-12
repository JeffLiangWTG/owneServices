using System;
using System.Data.SqlClient;
using CargoWise.eHub.DataAccess.Integration;
using Microsoft.BizTalk.Bam.EventObservation;

namespace CargoWise.eHub.Products.Core.PipelineComponents
{
	[Serializable]
	public class UpdateStatusEvent : IPersistQueryable
	{
		readonly string MessageTrackingID;
		readonly IOutboxAccessor Accessor;

		public UpdateStatusEvent(string messageTrackingID, IOutboxAccessor accessor)
		{
			MessageTrackingID = messageTrackingID;
			Accessor = accessor;
		}

		#region IPersistQueryable Members

		[NonSerialized]
		BAMEventsRecord parentRecord;

		public BAMEventsRecord ParentRecord
		{
			get { return parentRecord; }
			set { parentRecord = value; }
		}

		public void AddToBatch(SqlConnection connection, IBatch batch)
		{
		}

		public Type BatchType
		{
			get { return typeof(UpdateStatusEvent); }
		}

		public void PersistQueryable(SqlConnection connection, SqlTransaction transaction, int timeoutValue)
		{
			Accessor.UpdateInboxMessageDistributionStatus(MessageTrackingID);
		}
		#endregion
	}
}
