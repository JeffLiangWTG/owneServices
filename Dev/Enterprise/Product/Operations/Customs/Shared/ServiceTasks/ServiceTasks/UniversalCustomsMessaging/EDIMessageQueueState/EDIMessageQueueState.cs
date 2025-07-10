using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Scheduler.GraphEngine;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.ServiceTasks
{
	public class EDIMessageQueueState : IEquatable<EDIMessageQueueState>, IQueueState
	{
		public EDIMessageQueueState(Guid identifier, Guid messagePK, string parentMessageNumber, DateTime parentSystemCreateTimeUtc, string status, IEnumerable<string> keys, Guid chainID)
			: this(identifier, messagePK, parentSystemCreateTimeUtc, parentMessageNumber, status, keys)
		{
			ChainID = chainID;
			IsInDatabase = true;
		}

		public EDIMessageQueueState(EDIMessage message, IList<string> keys, ZString bizoMessageNumber)
			: this(Guid.NewGuid(), message.PK.ToGuid(), ConvertToDateTime(message.EM_SystemCreateTimeUtc), bizoMessageNumber, string.Empty, keys)
		{
		}

		static DateTime ConvertToDateTime(ZDateTime dateTime) => dateTime.IsValid ? dateTime.ToDateTime() : DateTime.MinValue;

		EDIMessageQueueState(Guid identifier, Guid messagePK, DateTime parentSystemCreateTimeUtc, string parentMessageNumber, string status, IEnumerable<string> keys)
		{
			Identifier = identifier;
			MessagePK = messagePK;
			ParentSystemCreateTimeUtc = parentSystemCreateTimeUtc;
			ParentMessageNumber = parentMessageNumber;
			Status = status;
			Keys = keys;
			OrderInfo = parentSystemCreateTimeUtc.Ticks.ToString().PadLeft(20, '0') + (parentMessageNumber ?? string.Empty);
		}

		public bool IsInDatabase { get; private set; }
		public Guid Identifier { get; }
		public Guid MessagePK { get; }
		public DateTime ParentSystemCreateTimeUtc { get; }
		public Guid ChainID { get; private set; }
		public bool ChainIdChanged { get; private set; }
		public string Status { get; private set; }
		public bool StatusChanged { get; private set; }
		public bool HasChanges => StatusChanged || ChainIdChanged;
		public IEnumerable<string> Keys { get; }
		public string ParentMessageNumber { get; }
		public string OrderInfo { get; }

		public void SetChainId(Guid id)
		{
			ChainID = id;
			ChainIdChanged = true;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Message string")]
		public void UpdateStatus(string newStatus)
		{
			if (newStatus != Status)
			{
				if (Status == QueueStatusCodes.Codes.Queued)
				{
					ErrorReporter.Instance.Report("Status should never change after a row is queued", FormattableString.Invariant($"EDIMessageQueueState.Status should never change after a row is queued: ParentID:{MessagePK}, ChainID:{ChainID}"), null);
				}
				Status = newStatus;
				StatusChanged = true;
			}
		}

		public void SetAsDatabaseSynced()
		{
			StatusChanged = false;
			ChainIdChanged = false;
			IsInDatabase = true;
		}

		public string GetFrontQueueDetails() => GetParentDetails();
		public string GetParentDetails() => parentDetails ??= GetParentDetailsCore();
		string parentDetails;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Message string")]
		string GetParentDetailsCore()
		{
			var result = new ZStringBuilder("Time:" + SqlFormatInfo.ToSqlDateTimeString(ParentSystemCreateTimeUtc));
			if (!string.IsNullOrEmpty(ParentMessageNumber))
			{
				result.Append("Number:" + ParentMessageNumber);
			}
			return result.ToStringWithDelimiterBetweenAppends("-");
		}

		#region IQueueState Members

		Guid IQueueState.ParentID => MessagePK;
		string IQueueState.TableCode => EDIMessageSchema.Constants.Prefix;
		#endregion

		#region IEquatable<EDIMessageQueueState>

		public bool Equals(EDIMessageQueueState other)
		{
			return other.MessagePK == MessagePK;
		}

		public override bool Equals(object obj)
		{
			return (obj as EDIMessageQueueState)?.Equals(this) ?? base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return MessagePK.GetHashCode();
		}

		#endregion
	}
}
