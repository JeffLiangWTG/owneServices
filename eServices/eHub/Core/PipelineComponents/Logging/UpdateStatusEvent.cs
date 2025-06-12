using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Threading;
using CargoWise.eHub.Core.Logging;
using CargoWise.eHub.DataAccess.Integration;
using Common.Logging;
using Microsoft.BizTalk.Bam.EventObservation;

namespace CargoWise.eHub.Core.PipelineComponents
{
	[Serializable]
	public class UpdateStatusEvent : IPersistQueryable
	{
		readonly string MessageTrackingID;
		readonly string SenderID;
		readonly string RecipientID;
		readonly IOutboxAccessor Accessor;
		[NonSerialized]
		ILog logger;
		[NonSerialized]
		BAMEventsRecord parentRecord;

		public UpdateStatusEvent(
			string messageTrackingID, 
			string senderID, 
			string recipientID, 
			IOutboxAccessor accessor)
		{
			this.MessageTrackingID = messageTrackingID;
			this.SenderID = senderID;
			this.RecipientID = recipientID;
			this.Accessor = accessor;
		}

		#region IPersistQueryable Members

		public ILog Logger
		{
			get { return logger ?? (logger = LoggerHelpers.GetHostLogger("UpdateStatusEvent")); }
			set { logger = value; }
		}

		public BAMEventsRecord ParentRecord
		{
			get { return parentRecord; }
			set { parentRecord = value; }
		}

		public TimeSpan WaitQuick
		{
			get { return SafeGetTimeSpanConfig("UpdateStatusEventWaitQuick", TimeSpan.FromSeconds(5)); }
		}

		public TimeSpan MaxQuick
		{
			get { return SafeGetTimeSpanConfig("UpdateStatusEventMaxQuick", TimeSpan.FromMinutes(1)); }
		}

		public TimeSpan WaitLong
		{
			get { return SafeGetTimeSpanConfig("UpdateStatusEventWaitLong", TimeSpan.FromMinutes(1)); }
		}

		public TimeSpan MaxLong
		{
			get { return SafeGetTimeSpanConfig("UpdateStatusEventMaxLong", TimeSpan.FromDays(1)); }
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
			PersistQueryableInternal(connection, transaction, timeoutValue);
		}

		private TimeSpan SafeGetTimeSpanConfig(string configString, TimeSpan defaultValue)
		{
			var config = ConfigurationManager.AppSettings[configString];
			if (string.IsNullOrEmpty(config))
			{
				Logger.WarnFormat("Expected <appSettings><add key=\"{0}\" value=\"{1}\"/></appSettings> but not found any in Biztalk's config. Using default value ({1} seconds) instead.", configString, defaultValue.TotalSeconds);
				return defaultValue;
			}
			double result;
			if (!double.TryParse(config, out result))
			{
				Logger.WarnFormat("Expected \"{0}\" config's value is a number but it is \"{1}\". Using default value ({2} seconds) instead.", configString, config, defaultValue.TotalSeconds);
				return defaultValue;
			}
			return TimeSpan.FromSeconds(result);
		}

		internal void PersistQueryableInternal(SqlConnection connection, SqlTransaction transaction, int timeoutValue)
		{
			int retriesQuick = 0;
			int retriesLong = 0;

			Logger.DebugFormat("Starting update for [MessageTrackingID: {0}, SenderID: {1}, RecipientID: {2}]", MessageTrackingID, SenderID, RecipientID);

			var shouldRun = !string.IsNullOrEmpty(MessageTrackingID) && !string.IsNullOrEmpty(SenderID) && !string.IsNullOrEmpty(RecipientID);

			if (!shouldRun)
			{
				Logger.WarnFormat("Unexpected message going through UpdateMessageDistributionStatus which has one of following properties which is empty or null: [MessageTrackingID: {0}, SenderID: {1}, RecipientID: {2}", MessageTrackingID, SenderID, RecipientID);
			}

			while (shouldRun)
			{
				try
				{
					Accessor.UpdateMessageDistributionStatus(MessageTrackingID, SenderID, RecipientID);
					Logger.DebugFormat("Completed update for [MessageTrackingID: {0}, SenderID: {1}, RecipientID: {2}]", MessageTrackingID, SenderID, RecipientID);
					break;
				}
				catch (Exception ex)
				{
					var waitQuick = WaitQuick;
					var waitLong = WaitLong;
					if (waitQuick.Ticks * retriesQuick < MaxQuick.Ticks)
					{
						Logger.DebugFormat("Error. Will be retried. [MessageTrackingID: {0}, SenderID: {1}, RecipientID: {2}]", ex, MessageTrackingID, SenderID, RecipientID);

						retriesQuick++;
						Thread.Sleep(waitQuick);
					}
					else if (waitLong.Ticks * retriesLong < MaxLong.Ticks)
					{
						Logger.WarnFormat("UpdateMessageDistributionStatus error. Will be retried. [MessageTrackingID: {0}, SenderID: {1}, RecipientID: {2}]", ex, MessageTrackingID, SenderID, RecipientID);

						retriesLong++;
						Thread.Sleep(waitLong);
					}
					else
					{
						Logger.ErrorFormat("UpdateMessageDistributionStatus permanently failed after multiple retries. [MessageTrackingID: {0}, SenderID: {1}, RecipientID: {2}]", ex, MessageTrackingID, SenderID, RecipientID);
						break;
					}
				}
			}
		}
		#endregion
	}
}
