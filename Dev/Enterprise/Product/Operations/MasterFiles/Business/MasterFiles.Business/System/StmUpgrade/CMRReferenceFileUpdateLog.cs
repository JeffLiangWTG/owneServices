using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	/// <summary>
	/// Summary description for CMRReferenceFileUpgradeLog.
	/// </summary>
	public class CMRReferenceFileUpdateLog : NonPersistentBusinessObject, IObsoleteValidation, IStmALogParent
	{
		public CMRReferenceFileUpdateLog(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Fixed PK

		// {8A0D6208-258D-4f56-9EFA-CD121A7E8733}
		public static readonly Guid LogReferencePK = new Guid(0x8a0d6208, 0x258d, 0x4f56, 0x9e, 0xfa, 0xcd, 0x12, 0x1a, 0x7e, 0x87, 0x33);

		protected override ZGuid GetPK()
		{
			return new ZGuid(LogReferencePK);
		}

		#endregion

		#region Constants

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Log Reference Values should be in English only")]
		public const string Reference = "CMR Reference Files Update";
		public const string ReferenceTableName = "CMRReferenceFileUpdate";
		public const int MaxLogRecordsToLoad = 20;

		#endregion

		#region Logging Update Result

		public void LogUpdateSuccess()
		{
			AddLogRecord(true);
		}

		public void LogUpdateSuccess(ZDateTimeOffset dataLastModified)
		{
			AddLogRecord(true, dataLastModified);
		}

		public void LogUpdateFailure()
		{
			AddLogRecord(false);
		}

#if DEBUG
		public void LogUpdateSuccessForTesting(ZDateTime logTime)
		{
			//This way, CheckLastSuccessLog will return this log added by this method
			var existingLogs = (StmALog[])Factory.Load(typeof(StmALog), SuccessfulUpdatesFilter);
			foreach (StmALog log in existingLogs)
			{
				log.SL_EventTime = logTime.AddDays(-1);
			}

			AddLogRecord(true, logTime.ToOffset());
		}
#endif
		#endregion

		#region LoadLastSuccessfulUpdate

		public void LoadLastSuccessfulUpdate()
		{
			StmALog log = (StmALog)Factory.LoadTop1(typeof(StmALog), SuccessfulUpdatesFilter);
			if (log != null)
			{
				fSuccessfulUpdateFileTimeStamp = log.SL_EventTimeOffset;
				fSuccessfulUpdatePostedTime = log.SL_PostedTimeUtc;
			}
		}

		#endregion

		#region SuccessfulUpdateFileTimeStamp

		public ZDateTimeOffset SuccessfulUpdateFileTimeStamp
		{
			get { return fSuccessfulUpdateFileTimeStamp; }
		}

		ZDateTimeOffset fSuccessfulUpdateFileTimeStamp;

		#endregion

		#region SuccessfulUpdatePostedTime

		public ZDateTime SuccessfulUpdatePostedTime
		{
			get { return fSuccessfulUpdatePostedTime; }
		}

		ZDateTime fSuccessfulUpdatePostedTime;

		#endregion

		#region Overrides

		public BusinessObject[] BusinessObjectsWithRelatedEvents
		{
			get { return new BusinessObject[] { this }; }
		}

		public override string TableName
		{
			get { return ReferenceTableName; }
		}

		#endregion

		#region IStmALogParent

		public Logs Logs
		{
			get
			{
				if (logs == null)
				{
					logs = new Logs(this);
					logs.AutoCreatedLogDefaultSL_Reference = "CMR Reference Files Upgrade";

					// Pre-loading depended StmALog records into Factory cache, because it could be accessible in cache only
					StmALogCollection upgradeLogs = new StmALogCollection(Factory, LogFilter);
					upgradeLogs.Load();

					return logs;
				}
				return logs;
			}
		}
		Logs logs;

		ZGuid IStmALogParent.LogsParentPK
		{
			get { return PK; }
		}

		string IStmALogParent.LogsParentTableName
		{
			get { return TableName; }
		}

		public BusinessObjectFactory LogsFactory
		{
			get { return Factory; }
		}

		BusinessObject[] IStmALogParent.BusinessObjectsWithRelatedEvents
		{
			get { return Array.Empty<BusinessObject>(); }
		}

		void IStmALogParent.ProcessLog(IStmALog log)
		{
		}

		bool IStmALogParent.DeferFiringWorkflow
		{
			get { return false; }
		}

		#endregion

		#region Implementation

		#region Filters

		protected ZQuery LogFilter
		{
			get
			{
				ZQuery result = new ZQuery(StmALogSchema.SL_Parent, LogReferencePK);
				result.OrderBy = new ZString(StmALogSchema.SL_EventTime.Name + " desc," + StmALogSchema.SL_PostedTimeUtc.Name + " desc");
				result.MaximumRows = MaxLogRecordsToLoad;
				return result;
			}
		}

		protected ZQuery SuccessfulUpdatesFilter
		{
			get
			{
				ZQuery result = LogFilter;
				result.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.UpgradeSucceeded.Code);
				return result;
			}
		}

		#endregion

		#region AddLogRecord

		protected StmALog AddLogRecord(bool success, ZDateTimeOffset dataLastModified)
		{
			var log = Logs.AddNew(success ? Events.UpgradeSucceeded : Events.UpgradeFailed, Reference, dataLastModified);
			Factory.Save();
			return log;
		}

		protected StmALog AddLogRecord(bool success)
		{
			return AddLogRecord(success, ZDateTimeOffset.Empty);
		}

		#endregion

		#endregion
	}
}
