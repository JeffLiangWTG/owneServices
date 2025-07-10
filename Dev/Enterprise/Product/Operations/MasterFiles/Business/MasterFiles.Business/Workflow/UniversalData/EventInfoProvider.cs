using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.UniversalData
{
	public class EventInfoProvider : IEventInfo
	{
		public EventInfoProvider(IQueuedLog queuedLog, IBaseTrigger trigger, BusinessObject parent)
		{
			Argument.NotNull(queuedLog, "queuedLog");
			var factory = queuedLog.Factory;

			var eventData = new WorkflowTriggerEventData(queuedLog);
			branchProvider = Lazy.Create(() => !eventData.TriggeringBranchCode.IsEmpty ? factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, eventData.TriggeringBranchCode) : null);
			departmentProvider = Lazy.Create(() => !eventData.TriggeringDepartmentCode.IsEmpty ? factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, eventData.TriggeringDepartmentCode) : null);
			staffProvider = Lazy.Create(() => factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, queuedLog.SJ_GS_NKUser));

			@eventProvider = Lazy.Create(() =>
			{
				TriggeringLogFinder.TryFindLog(eventData, trigger, parent, out var triggeringLog, findCancelled: true, wteFallbackLog: queuedLog);
				return triggeringLog;
			});
		}

		public ZString EventReference => FoundEvent && !DoNotProvideEventReference ? Event.SL_Reference : ZString.Empty;

		public StmALog Event => eventProvider.Value;
		public IBranch EventBranch => branchProvider.Value;
		public IDepartment EventDepartment => departmentProvider.Value;
		public IUser EventUser => staffProvider.Value;

		readonly Lazy<StmALog> eventProvider;
		readonly Lazy<GlbBranch> branchProvider;
		readonly Lazy<GlbDepartment> departmentProvider;
		readonly Lazy<GlbStaff> staffProvider;
		internal bool FoundEvent => Event != null;
		internal bool DoNotProvideEventReference { get; set; }
	}
}
