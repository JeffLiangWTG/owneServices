using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	class ProcessTaskFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		internal ProcessTaskFetchStrategy(ProcessTask processTask)
			: base(processTask)
		{
		}

		ProcessTask Task => (ProcessTask)BusinessObject;

		protected override void FetchForLoadCore()
		{
			base.FetchForLoadCore();

			var task = Task;
			if (task.IsTemplate)
			{
				task.AddCompletionTriggerActionsFetchHint();
			}
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);
			var processTask = Task;
			processTask.AddCompletionTriggerActionsFetchHint();

			if (columns.Any(col => col.ColumnName == ProcessTasksSchema.Constants.P9_ParentTemplateID))
			{
				Factory.AddFetchHint(ProcessTasksSchema.Constants.TableName, processTask.P9_ParentTemplateID);
			}

			processTask.AddProcessWorkflowExceptionFetchHint();
		}

		protected override void FetchForValidateCore()
		{
			base.FetchForValidateCore();
		}

		protected override void FetchForLoadChildEditableObjectsCore()
		{
			base.FetchForLoadChildEditableObjectsCore();
			var task = Task;
			task.AddProcessTaskExtraResourceFetchHint();
			task.AddSkillsPivotsFetchHint();
			task.AddCompletionTriggerActionsFetchHint();

			if (task.ShouldAddProcessWorkflowExceptionFetchHintsForLoadChildEditableObjects)
			{
				task.AddProcessWorkflowExceptionFetchHint();
			}
		}

		protected override void FetchForFactorySaveCore()
		{
			base.FetchForFactorySaveCore();

			ObjectFactory.Get<TaskStatusChangedEventParametersStrategy>("BMTaskStatusChangedEventParametersStrategy").AddFetchHints(BusinessObject as ProcessTask, Factory);
		}

		protected override void FetchForDeleteCore()
		{
			base.FetchForDeleteCore();
			var task = Task;
			var factory = task.Factory;
			factory.AddFetchHint(ProcessHeaderSchema.PK, task.P9_FH_ProcessHeader);
			factory.AddFetchHint(ProcessJobTriggerLinkSchema.Instance, new ZQuery(ProcessJobTriggerLinkSchema.P9L_ParentId, task.PK));
			factory.AddFetchHint(TagLinkSchema.Instance, task.CreateTagLinksQuery());
			factory.AddFetchHint(ProcessTaskIterationLinkSchema.Instance, task.IterationLinks.CompleteFilter);
			factory.AddFetchHint(ProcessTaskIterationLinkPivotSchema.P9P_P9_Task, task.PK);
			task.AddCompletionTriggerActionsFetchHint();
		}
	}
}
