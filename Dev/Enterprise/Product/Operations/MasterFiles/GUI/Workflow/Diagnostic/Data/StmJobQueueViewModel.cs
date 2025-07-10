using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.GUI.Workflow
{
	public class StmJobQueueViewModel : NonPersistentBusinessObject
	{
		public StmJobQueueViewModel(IQueuedLog stmJobQueue)
		{
			this.stmJobQueue = stmJobQueue;
		}

		#region Fields

		[ResourceStringData("StmJobQueueViewModel.SJ_EventTime", Caption = "Event Time", FullDescription = "The local time of the user or server who edited the record. It shows the date and time of the logged in branch.")]
		public ZDateTime SJ_EventTime => stmJobQueue.SJ_EventTime;
		public ZPropertyInfo SJ_EventTimeInfo => GetZPropertyInfo(nameof(SJ_EventTime));

		[ResourceStringData("StmJobQueueViewModel.SJ_EventTimeUtc", Caption = "Event Time (UTC)", FullDescription = "The UTC time of the user or server who edited the record. It shows the date and time of the logged in branch.")]
		public ZDateTime SJ_EventTimeUtc => stmJobQueue.SJ_EventTimeUtc;
		public ZPropertyInfo SJ_EventTimeUtcInfo => GetZPropertyInfo(nameof(SJ_EventTimeUtc));

		[ResourceStringData("StmJobQueueViewModel.EventLocalBranchTime", Caption = "Event Time (Local)", FullDescription = "The local time of the user or server who display the record. It shows the date and time of the logged in branch.")]
		public ZDateTime EventLocalBranchTime
		{
			get
			{
				ZDateTime result = (SJ_EventTimeUtc.IsValid) ? Env.Time.GetLocalTimeFromUtc(stmJobQueue.SJ_EventTimeUtc.ToDateTime()) : ZDateTime.Empty;
				return result;
			}
		}
		public ZPropertyInfo EventLocalBranchTimeInfo
		{
			get { return GetZPropertyInfo(nameof(EventLocalBranchTime)); }
		}

		[ResourceStringData("StmJobQueueViewModel.SJ_PostedTimeUtc", Caption = "Posted Time (UTC)", FullDescription = "The date and time that the event was created in the system. For system generated events this will be equal to the event time. For user generated events this may be different as the user may nominate a date and time that is not equal to the system date and time when adding an event.")]
		public ZDateTime SJ_PostedTimeUtc => stmJobQueue.SJ_PostedTimeUtc;
		public ZPropertyInfo SJ_PostedTimeUtcInfo => GetZPropertyInfo(nameof(SJ_PostedTimeUtc));

		[ResourceStringData("StmJobQueueViewModel.SJ_GS_NKUser", Caption = "User", FullDescription = "The code of the staff member who caused this log to be created")]
		public ZString SJ_GS_NKUser => stmJobQueue.SJ_GS_NKUser;
		public ZPropertyInfo SJ_GS_NKUserInfo => GetZPropertyInfo(nameof(SJ_GS_NKUser));

		[ResourceStringData("StmJobQueueViewModel.SJ_IsEstimate", Caption = "Is Estimate", FullDescription = "Specifies whether this event is an estimate, rather than an actual.")]
		public ZBool SJ_IsEstimate => stmJobQueue.SJ_IsEstimate;
		public ZPropertyInfo SJ_IsEstimateInfo => GetZPropertyInfo(nameof(SJ_IsEstimate));

		[ResourceStringData("StmJobQueueViewModel.SJ_Status", Caption = "Status")]
		public ZString SJ_Status => stmJobQueue.SJ_Status;
		public ZPropertyInfo SJ_StatusInfo => GetZPropertyInfo(nameof(SJ_Status));

		[ResourceStringData("StmJobQueueViewModel.SJ_IsDelayFired", Caption = "Is Delay Fired", FullDescription = "Specifies delay fired event i.e. deferred scheduled message.")]
		public ZBool SJ_IsDelayFired => stmJobQueue.SJ_IsDelayFired;
		public ZPropertyInfo SJ_IsDelayFiredInfo => GetZPropertyInfo(nameof(SJ_IsDelayFired));

		#endregion

		#region implementatin

		readonly IQueuedLog stmJobQueue;

		#endregion
	}
}
