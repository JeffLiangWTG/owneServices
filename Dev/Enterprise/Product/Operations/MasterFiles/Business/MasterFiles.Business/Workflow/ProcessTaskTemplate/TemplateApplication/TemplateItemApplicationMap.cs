using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Common.Testing;

namespace Enterprise.MasterFiles.Business
{
	public class TemplateItemApplicationMap
	{
		public TemplateItemApplicationMap(ProcessTaskTemplate template, IEnumerable<TemplateItemApplication> applications)
		{
			Template = template;
			Applications = applications;
		}

		public IEnumerable<TemplateItemApplication> Applications { get; }
		public ProcessTaskTemplate Template { get; }
	}

	public sealed class TemplateItemApplication : IDisposable
	{
		readonly Func<IEnumerable<ProcessTaskNotification>> getNotificationsToCreate;

		public bool IsNeedingToBeLogged => ShouldCloneTask || NotificationsToCreate.Any();

		public bool ShouldCloneTask { get; }
		public RepeatApplicationParameters RepeatApplicationParams { get; }
		public ProcessTask TemplateTask { get; }
		public ProcessTaskTemplate Template { get; }

		ProcessTaskNotification[] notifications;

		public IEnumerable<ProcessTaskNotification> NotificationsToCreate => notifications ?? (notifications = getNotificationsToCreate().ToArray());

		public ProcessTask CreatedItem { get; internal set; }

		public TemplateItemApplication(ProcessTaskTemplate template, IEnumerable<ProcessTask> templatesToCreate, RepeatApplicationParameters repeatApplicationParams = null)
			: this(template, templatesToCreate.First(), () => templatesToCreate.SelectMany(t => t.ProcessTaskNotificationsWithoutMultipleIndexes), repeatApplicationParams: repeatApplicationParams)
		{
		}

		internal TemplateItemApplication(ProcessTaskTemplate template, ProcessTask existingTask, Func<IEnumerable<ProcessTaskNotification>> getNotificationsToCreate, bool shouldCloneTask = true, RepeatApplicationParameters repeatApplicationParams = null)
		{
			Template = template;
			this.TemplateTask = existingTask;
			this.getNotificationsToCreate = getNotificationsToCreate;
			ShouldCloneTask = shouldCloneTask;
			RepeatApplicationParams = repeatApplicationParams;
		}

		public void TrackDelete()
		{
			if (IsNeedingToBeLogged)
			{
				DisposableLeakListener.Instance.RegisterDisposable(this);
				TemplateTask.OnDelete += OnDelete;
			}
		}

		void OnDelete(object sender, EventArgs e)
		{
			ErrorReporter.ReportOnce("Should not be deleting a task in the middle of template application.");
		}

		public void Dispose()
		{
			if (IsNeedingToBeLogged)
			{
				TemplateTask.OnDelete -= OnDelete;
				DisposableLeakListener.Instance.UnRegisterDisposable(this);
			}
		}

		public class RepeatApplicationParameters
		{
			public RepeatApplicationParameters(int highestExistingSequenceNumber)
			{
				HighestExistingSequenceNumber = highestExistingSequenceNumber;
			}

			public int HighestExistingSequenceNumber { get; }
		}
	}
}
