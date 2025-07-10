using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public abstract class WorkflowFilterBusinessObjectBase : FilterStripBusinessObject
	{
		public WorkflowFilterBusinessObjectBase()
		{
		}

		public override ZQuery Filter
		{
			get
			{
				ZQuery result = base.Filter;

				if (QueryObjectType == typeof(ProcessTask) && ParentType != null)
				{
					if (typeof(IWorkflowProvider).IsAssignableFrom(ParentType))
					{
						var prefix = GetColumnPrefixFromType(ParentType);
						result.AddToFilter(WorkflowFilterStripsHelper.GetParentTableCodeQuery(prefix, ProcessTasksSchema.P9_ParentTableCode));
					}
				}

				return result;
			}
		}

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			throw new NotImplementedException();
		}

		protected void AddCompletionTriggerActionFilter(ModuleFilterCollection filters)
		{
			var completionTriggerActionsFilter = new ModuleGuidForeignCollectionFilter(
				description: "Completion Trigger Actions",
				moduleId: ModuleIDs.CompletionTriggerAction,
				primaryKeyColumn: ProcessTasksSchema.PK,
				foreignKeyColumn: ProcessTaskNotificationSchema.PQ_P9,
				listDelegate: () => new ProcessTaskNotificationCollection(Factory),
				parentBusinessObjectType: typeof(ProcessTask))
			{
				MultilingualDescription = ResString.GetMultilingualString("MasterFiles|WorkflowMilestonesFilter|CompletionTriggerActions", "Completion Trigger Actions")
			};
			filters.AddCustomFilter(completionTriggerActionsFilter);
		}
	}
}
