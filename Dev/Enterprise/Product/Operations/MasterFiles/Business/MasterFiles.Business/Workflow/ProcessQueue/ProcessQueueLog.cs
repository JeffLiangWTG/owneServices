using System.Data;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business
{
	/// <summary>
	/// I'm leaving a note so you don't have to spend ages trying to work this out like me.
	/// This class exists for a UPE specific plugin, and has no other purpose.
	/// This plugin is under UPE Express > Cargo Reporting (And appears in the Process Queue tab).
	/// There is an oddness/probable bug involving this class in that constructed ProcessQueueLog will not fire workflow (But will appear as a CES log in the events tab when it is loaded).
	/// I am not doing anything about this.
	/// </summary>
	public class ProcessQueueLog : BaseStmALog
	{
		#region Schema

		public new class Schema : BaseStmALog.Schema
		{
			public const string Queue = "Queue";
			public const string Status = "Status";
			public const string SubStatus = "SubStatus";
			public const string Reason = "Reason";
			public const string AssignedTo = "AssignedTo";
		}

		#endregion

		#region Constants

		abstract class Constants
		{
			public const int FieldCount = 5;

			public abstract class Position
			{
				public const int Queue = 0;
				public const int Status = 1;
				public const int SubStatus = 2;
				public const int Reason = 3;
				public const int AssignedTo = 4;
			}
		}

		#endregion

		#region Static

		public static string GetEncodedLogReference(ProcessQueueType.Enum queueType, ZString queueName)
		{
			return GetEncodedLogReference(queueType, GetLogReferenceAsCsvLine(queueName));
		}

		public static string GetEncodedLogReference(ProcessQueueType.Enum queueType, ZString queueName, ZString status)
		{
			return GetEncodedLogReference(queueType, GetLogReferenceAsCsvLine(queueName, status));
		}

		public static string GetEncodedLogReference(ProcessQueueType.Enum queueType, ZString queueName, ZString status, ZString subStatus)
		{
			return GetEncodedLogReference(queueType, GetLogReferenceAsCsvLine(queueName, status, subStatus));
		}

		public static string GetEncodedLogReference(ProcessQueueType.Enum queueType, ZString queueName, ZString status, ZString subStatus, ZString reason)
		{
			return GetEncodedLogReference(queueType, GetLogReferenceAsCsvLine(queueName, status, subStatus, reason));
		}

		public static string GetEncodedLogReference(ProcessQueueType.Enum queueType, ZString queueName, ZString status, ZString subStatus, ZString reason, ZString assignedTo)
		{
			return GetEncodedLogReference(queueType, GetLogReferenceAsCsvLine(queueName, status, subStatus, reason, assignedTo));
		}

		static string GetEncodedLogReference(ProcessQueueType.Enum queueType, OCsvLine logReferenceCsvLine)
		{
			ZString queueTypeAsString = (queueType == ProcessQueueType.Enum.Customs) ? ProcessQueueType.Customs : ProcessQueueType.Commercial;
			return GetEncodedLogReference(queueTypeAsString, logReferenceCsvLine);
		}

		static string GetEncodedLogReference(ZString queueTypeAsString, OCsvLine logReferenceCsvLine)
		{
			return queueTypeAsString + logReferenceCsvLine.ToString();
		}

		static OCsvLine GetLogReferenceAsCsvLine(params string[] fieldValues)
		{
			OCsvLine result = new OCsvLine(fieldValues);
			for (int i = 0; i < fieldValues.Length; i++)
			{
				result.FieldValues[i] = fieldValues[i];
			}
			return result;
		}

		#endregion

		public ProcessQueueLog(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public bool IsCustomsQueueLog
		{
			get { return QueueTypeString == ProcessQueueType.Customs; }
		}

		public bool IsCommercialQueueLog
		{
			get { return QueueTypeString == ProcessQueueType.Commercial; }
		}

		public override bool ReadOnly
		{
			get { return true; }
		}

		public new ProcessQueue Master
		{
			get { return (ProcessQueue)base.Master; }
			set { base.Master = value; }
		}

		public void SetQueueDetails(ProcessQueueType.Enum queueType, ZString queueName, ZString status, ZString subStatus, ZString reason, ZString assignedTo)
		{
			ZString queueTypeString = (queueType == ProcessQueueType.Enum.Customs) ? ProcessQueueType.Customs : ProcessQueueType.Commercial;
			SL_Reference = queueTypeString;
			SetQueueDetails(queueName, status, subStatus, reason, assignedTo);
		}

		public virtual void SetQueueDetails(ZString queueName, ZString status, ZString subStatus, ZString reason, ZString assignedTo)
		{
			QueueDetailsLine = GetLogReferenceAsCsvLine(queueName, status, subStatus, reason, assignedTo);
		}

		public override void OnSaving()
		{
			base.OnSaving();
			if (!IsInDatabase && ValidQueueType)
			{
				if (QueueDetailsLine == null)
				{
					SetQueueDetailsFromMaster();
				}
				SL_Reference = new ZString(GetEncodedLogReference(QueueTypeString, QueueDetailsLine)).SubstringSafe(0, SL_ReferenceInfo.MaxLength);
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			SL_SE_NKEvent = Events.QueueChanged.Code;
		}

		#region QueueDetails

		protected void SetQueueDetailsFromMaster()
		{
			if (QueueTypeString == ProcessQueueType.Customs)
			{
				SetQueueDetails(Master.P4_CustomsQueue, Master.P4_CustomsStatus, Master.P4_CustomsSubStatus, Master.P4_CustomsReason, Master.P4_GS_NKCustomsTaskAssignedTo);
			}
			else if (QueueTypeString == ProcessQueueType.Commercial)
			{
				SetQueueDetails(Master.P4_QueueName, Master.P4_Status, Master.P4_SubStatus, Master.P4_Reason, Master.P4_GS_NKTaskAssignedTo);
			}
		}

		bool ValidQueueType
		{
			get { return QueueTypeString == ProcessQueueType.Commercial || QueueTypeString == ProcessQueueType.Customs; }
		}

		protected ZString QueueTypeString
		{
			get { return SL_Reference.Left(3); }
		}

		ZString QueueDetails
		{
			get { return SL_Reference.SubstringSafe(3); }
		}

		#endregion

		#region New Properties

		#region Queue

		public ZString Queue
		{
			get { return (ValidQueueDetailsLine) ? QueueDetailsLine.FieldValues[Constants.Position.Queue] : ""; }
		}

		public ZPropertyInfo QueueInfo
		{
			get { return GetZPropertyInfo(nameof(Queue)); }
		}

		#endregion

		#region Status

		public ZString Status
		{
			get { return (ValidQueueDetailsLine) ? QueueDetailsLine.FieldValues[Constants.Position.Status] : ""; }
		}

		public ZPropertyInfo StatusInfo
		{
			get { return GetZPropertyInfo(nameof(Status)); }
		}

		#endregion

		#region SubStatus
		[List("Lookups.SubStatusList")]
		public ZString SubStatus
		{
			get { return (ValidQueueDetailsLine) ? QueueDetailsLine.FieldValues[Constants.Position.SubStatus] : ""; }
		}

		public ZPropertyInfo SubStatusInfo
		{
			get { return GetZPropertyInfo(nameof(SubStatus)); }
		}

		#endregion

		#region Reason

		public ZString Reason
		{
			get { return (ValidQueueDetailsLine) ? QueueDetailsLine.FieldValues[Constants.Position.Reason] : ""; }
		}

		public ZPropertyInfo ReasonInfo
		{
			get { return GetZPropertyInfo(nameof(Reason)); }
		}

		#endregion

		#region AssignedTo
		[List("Lookups.TaskAssignedToList")]
		public ZString AssignedTo
		{
			get { return (ValidQueueDetailsLine) ? QueueDetailsLine.FieldValues[Constants.Position.AssignedTo] : ""; }
		}

		public ZPropertyInfo AssignedToInfo
		{
			get { return GetZPropertyInfo(nameof(AssignedTo)); }
		}

		#endregion

		bool ValidQueueDetailsLine
		{
			get { return QueueDetailsLine != null && QueueDetailsLine.FieldValues.Length == Constants.FieldCount; }
		}

		OCsvLine QueueDetailsLine
		{
			get
			{
				if (fQueueDetailsLine == null && ValidQueueType && !QueueDetails.IsEmpty)
				{
					fQueueDetailsLine = new OCsvLine(QueueDetails);
				}
				return fQueueDetailsLine;
			}
			set { fQueueDetailsLine = value; }
		}

		OCsvLine fQueueDetailsLine;

		#endregion
	}
}
