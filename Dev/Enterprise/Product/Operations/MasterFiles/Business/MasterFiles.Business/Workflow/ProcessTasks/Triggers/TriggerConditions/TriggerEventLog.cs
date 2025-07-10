using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business
{
	public class TriggerEventLog
	{
		public TriggerEventLog(IStmALog log, IBaseTrigger trigger, IBusiness parent)
			: this(log)
		{
			TriggerDataModel = new TriggerDataModel(trigger, parent);
		}

		public TriggerEventLog(IStmALog log, IMilestoneDateDefaultable trigger)
			: this(log)
		{
			TriggerDataModel = new TriggerDataModel(Lazy.Create(() => trigger.ActualDate.ToZDateTime()));
		}

		public TriggerEventLog(BusinessObjectFactory factory, IQueuedLog log, IBaseTrigger trigger, IBusiness parent)
			: this(log.SJ_Reference, StmALog.GetReferenceForBinding(log.SJ_Reference), log.SJ_IsEstimate)
		{
			EventCode = log.SJ_SE_NKEvent;
			LogParent = StmALog.GetMaster(log.SJ_ParentID, log.SJ_ParentTableCode, factory) as BusinessObject;
			EventDataModel = new LogEventDataModel(log);
			TriggerDataModel = new TriggerDataModel(trigger, parent);
		}

		public TriggerEventLog(IStmChangeLog log)
			: this(log.SY_Changes, log.SY_Changes, false)
		{
			var stmChangeLog = log as StmChangeLog;
			EventDataModel = new LogEventDataModel(log);
			LogParent = stmChangeLog?.Parent;
		}

		TriggerEventLog(IStmALog log)
			: this(log.SL_Reference, StmALog.GetReferenceForBinding(log.SL_Reference), log.SL_IsEstimate)
		{
			EventCode = log.SL_SE_NKEvent;
			LogParent = log.Master;
			EventDataModel = new LogEventDataModel(log);
		}

		TriggerEventLog(string reference, string referenceForBinding, bool isEstimate)
		{
			Reference = reference;
			ReferenceForBinding = referenceForBinding;
			IsEstimate = isEstimate;
		}

#if DEBUG
		public static TriggerEventLog CreateLog_ForTest(string reference, bool isEstimate)
		{
			return new TriggerEventLog(reference, reference, isEstimate);
		}
#endif

		public ZString EventCode { get; }
		public ZString Reference { get; }
		public ZString ReferenceForBinding { get; }
		public ZBool IsEstimate { get; }
		public BusinessObject LogParent { get; }
		public LogEventDataModel EventDataModel { get; }
		public TriggerDataModel TriggerDataModel { get; }
	}
}
