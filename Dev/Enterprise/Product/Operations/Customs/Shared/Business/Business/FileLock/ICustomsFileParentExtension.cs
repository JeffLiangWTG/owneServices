using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business
{
	public static class ICustomsFileParentExtension
	{
		public static void AddLockEvent(this ICustomsFileParent parent, ZString reference)
		{
			AddEditableEventOnParent(parent, AutoEvents.LockForEdit, reference);
		}

		public static void AddUnlockEvent(this ICustomsFileParent parent, ZString reference)
		{
			AddEditableEventOnParent(parent, AutoEvents.UnlockForEdit, reference);
		}

		static void AddEditableEventOnParent(ICustomsFileParent parent, Event eventType, ZString reference)
		{
			if (parent != null && eventType != null)
			{
				var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, new[] { AutoEvents.UnlockForEditCode, AutoEvents.LockForEditCode });
				query.AddToFilter(StmALogSchema.SL_IsCancelled, false);

				var logsForCancel = parent.Logs.Find(query);
				logsForCancel.ForEach(c => c.Cancel());

				parent.Logs.CreateRecreateOrUpdateEventLog(eventType, EstimateActual.Actual, ZDateTimeOffset.Now, reference);
			}
		}
	}
}
