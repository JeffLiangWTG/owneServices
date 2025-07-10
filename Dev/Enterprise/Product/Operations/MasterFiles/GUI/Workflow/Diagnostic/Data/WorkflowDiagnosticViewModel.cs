using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.GUI.Workflow
{
	public class WorkflowDiagnosticViewModel : NonPersistentBusinessObject
	{
		public WorkflowDiagnosticViewModel(ProcessTask task)
		{
			this.task = task;
		}

		#region Properties

		public ProcessTask Task => task;

		#endregion

		#region Related Business Objects

		#region WTE Logs

		public WTELogViewModelCollection WTELogs
		{
			get
			{
				if (wteLogs == null)
				{
					wteLogs = new WTELogViewModelCollection(task);
					wteLogs.Load();
				}

				return wteLogs;
			}
		}

		WTELogViewModelCollection wteLogs;

		#endregion

		#region Scheduled Delayed Events

		public ScheduledDelayedEventViewModelCollection ScheduledDelayedEvents
		{
			get
			{
				if (scheduledDelayedEvents == null)
				{
					scheduledDelayedEvents = new ScheduledDelayedEventViewModelCollection(task);
					scheduledDelayedEvents.Load();
				}

				return scheduledDelayedEvents;
			}
		}

		ScheduledDelayedEventViewModelCollection scheduledDelayedEvents;

		#endregion

		#endregion

		#region implementatin

		readonly ProcessTask task;

		#endregion
	}
}
