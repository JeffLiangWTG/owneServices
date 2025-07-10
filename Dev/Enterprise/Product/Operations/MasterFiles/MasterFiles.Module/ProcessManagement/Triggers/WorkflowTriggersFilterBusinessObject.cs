using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Module
{
	public class WorkflowTriggersFilterBusinessObject : WorkflowFilterBusinessObjectBase
	{
		public WorkflowTriggersFilterBusinessObject()
		{
		}

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();

			AddTextFilters(result);
			AddDateFilters(result);
			AddFindboxFilters(result);

			return result;
		}

		protected override IEnumerable<Type> FilterStripsHelperTypesToExcludeFromAutomaticAddingOfFilters
		{
			get { return new[] { typeof(WorkflowFilterStripsHelper), typeof(IBMFilterStripsHelper) }; }
		}

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter(
				description: "Description",
				filterColumn: ProcessTasksSchema.P9_Description
			).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|WorkflowTriggersFilter|Description", "Description");

			filters.AddTextFilter(
				description: "Event Code",
				queryDelegate: (comparisonOperator, value) => new ZQuery(ProcessTasksSchema.P9_SE_NKMilestoneEvent, comparisonOperator, value.SubstringSafe(0, ProcessTasksSchema.P9_SE_NKMilestoneEvent.MaxLength)),
				list: EventCodesList
			).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|WorkflowTriggersFilter|EventCode", "Event Code");

			filters.AddTextFilter(
				description: "Trigger Field",
				filterColumn: ProcessTasksSchema.P9_TriggerField
			).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|WorkflowTriggersFilter|TriggerField", "Trigger Field");

			filters.AddTextFilter(
				description: "Trigger Condition",
				queryDelegate: (comparisonOperator, value) => new ZQuery(ProcessTasksSchema.P9_TriggerCondition, comparisonOperator, value.SubstringSafe(0, ProcessTasksSchema.P9_TriggerCondition.MaxLength)),
				list: TriggerConditionsList
			).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|WorkflowTriggersFilter|TriggerCondition", "Trigger Condition");

			filters.AddTextFilter(
				description: "Trigger Condition Value",
				queryDelegate: WorkflowFilterStripsHelper.GetTriggerConditionValueQuery
			).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|WorkflowTriggersFilter|TriggerConditionValue", "Trigger Condition Value");

			filters.AddTextFilter(
				description: "Line Trigger Type",
				filterColumn: ProcessTasksSchema.P9_LineTriggerType
			).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|WorkflowTriggersFilter|LineTriggerType", "Line Trigger Type");
		}

		void AddDateFilters(ModuleFilterCollection filters)
		{
			filters.AddDateFilter(
				description: "Event Date",
				dateFilterColumn: ProcessTasksSchema.P9_ActualDate
			).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|WorkflowTriggersFilter|EventDate", "Event Date");
		}

		void AddFindboxFilters(ModuleFilterCollection filters)
		{
			filters.AddGuidFilter(
				description: "Source Template",
				iD: ModuleIDs.ProcessTemplates,
				queryDelegate: WorkflowFilterStripsHelper.GetSourceTemplateQuery,
				list: Templates
			).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|WorkflowMilestonesFilter|SourceTemplate", "Source Template");

			AddCompletionTriggerActionFilter(filters);
		}

		#endregion

		public override ZQuery Filter
		{
			get
			{
				ZQuery result = base.Filter;
				result.AddToFilter(ProcessTasksSchema.P9_Type, SQLComparisonOperator.Equal, Core.Constants.Workflow.WorkflowTriggerType);
				return result;
			}
		}

		#region Lookups

		ProcessTaskTemplateCollection Templates
		{
			get
			{
				return Factory.GetCachedValue("WorkflowMilestonesFilterBusinessObject|Staffs", () =>
				{
					return new ProcessTaskTemplateCollection(Factory);
				});
			}
		}

		CodeDescriptionPairList customizableEventList;
		CodeDescriptionPairList CustomizableEventList
		{
			get
			{
				if (customizableEventList == null)
				{
					customizableEventList = new CodeDescriptionPairList();
					var query = new ZQuery(StmEventSchema.SE_IsCustomizable, true);
					query.AddToFilter(StmEventSchema.SE_IsActive, true);
					var customizableEvents = Factory.Load<StmEvent>(query);
					foreach (var item in customizableEvents)
					{
						customizableEventList.AddPair(item.SE_Code, item.SE_DescMultilingual);
					}
				}
				return customizableEventList;
			}
		}

		CodeDescriptionPairList EventCodesList
		{
			get
			{
				return Factory.GetCachedValue("WorkflowMilestonesFilterBusinessObject|EventCodesList", () =>
				{
					var eventCodes = new CodeDescriptionPairList();
					foreach (Event ev in AutoEvents.All)
					{
						var description = ev.Description;
						if (CustomizableEventList.ContainsCode(ev.Code))
						{
							description = CustomizableEventList[ev.Code, System.StringComparison.OrdinalIgnoreCase].Description;
						}
						eventCodes.Add(new CodeDescriptionPair(ev.Code, description));
					}
					return eventCodes;
				});
			}
		}

		CodeDescriptionPairList TriggerConditionsList
		{
			get
			{
				return Factory.GetCachedValue("WorkflowMilestonesFilterBusinessObject|TriggerConditionsList", () =>
				{
					var triggerConditions = new CodeDescriptionPairList();
					foreach (CodeDescriptionPair pair in new EventReferenceConditionList())
					{
						triggerConditions.Add(new CodeDescriptionPair(pair.Code, pair.Description));
					}
					return triggerConditions;
				});
			}
		}

		#endregion
	}
}
