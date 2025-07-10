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
	public class WorkflowMilestonesFilterBusinessObject : WorkflowFilterBusinessObjectBase
	{
		public WorkflowMilestonesFilterBusinessObject()
		{
		}

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			var result = new ModuleFilterCollection();

			AddNumberFilters(result);
			AddTextFilters(result);
			AddDateFilters(result);
			AddFindboxFilters(result);

			return result;
		}

		protected override IEnumerable<Type> FilterStripsHelperTypesToExcludeFromAutomaticAddingOfFilters
		{
			get { return new[] { typeof(WorkflowFilterStripsHelper), typeof(IBMFilterStripsHelper) }; }
		}

		void AddNumberFilters(ModuleFilterCollection filters)
		{
			filters.AddNumberRangeFilter(
				description: "Sequence",
				filterColumn: ProcessTasksSchema.P9_Sequence
			).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|WorkflowMilestonesFilter|Sequence", "Sequence");
		}

		void AddTextFilters(ModuleFilterCollection filters)
		{
			filters.AddTextFilter(
				description: "Description",
				filterColumn: ProcessTasksSchema.P9_Description
			).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|WorkflowMilestonesFilter|Description", "Description");

			filters.AddTextFilter(
				description: "Event Code",
				queryDelegate: (comparisonOperator, value) => new ZQuery(ProcessTasksSchema.P9_SE_NKMilestoneEvent, comparisonOperator, value.SubstringSafe(0, ProcessTasksSchema.P9_SE_NKMilestoneEvent.MaxLength)),
				list: EventCodesList
			).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|WorkflowMilestonesFilter|EventCode", "Event Code");

			filters.AddTextFilter(
				description: "Exception Event Code",
				queryDelegate: (comparisonOperator, value) => new ZQuery(ProcessTasksSchema.P9_SE_NKExceptionEvent, comparisonOperator, value.SubstringSafe(0, ProcessTasksSchema.P9_SE_NKExceptionEvent.MaxLength)),
				list: ExceptionTypeCodesList
			).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|WorkflowMilestonesFilter|ExceptionEventCode", "Exception Event Code");

			filters.AddTextFilter(
				description: "Trigger Condition",
				queryDelegate: (comparisonOperator, value) => new ZQuery(ProcessTasksSchema.P9_TriggerCondition, comparisonOperator, value.SubstringSafe(0, ProcessTasksSchema.P9_TriggerCondition.MaxLength)),
				list: TriggerConditionsList
			).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|WorkflowMilestonesFilter|TriggerCondition", "Trigger Condition");

			filters.AddTextFilter(
				description: "Trigger Condition Value",
				queryDelegate: WorkflowFilterStripsHelper.GetTriggerConditionValueQuery
			).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|WorkflowMilestonesFilter|TriggerConditionValue", "Trigger Condition Value");
		}

		void AddDateFilters(ModuleFilterCollection filters)
		{
			filters.AddDateFilter(
				description: "Actual Start",
				dateFilterColumn: ProcessTasksSchema.P9_ActualDate
			).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|WorkflowMilestonesFilter|ActualStart", "Actual Start");

			filters.AddDateFilter(
				description: "Estimated",
				dateFilterColumn: ProcessTasksSchema.P9_ScheduledDateUtc,
				convertFromLocalToUTC: true
			).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|WorkflowMilestonesFilter|Estimated", "Estimated");

			filters.AddDateFilter(
				description: "Original Estimated",
				dateFilterColumn: ProcessTasksSchema.P9_OriginalScheduledDateUtc,
				convertFromLocalToUTC: true
			).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|WorkflowMilestonesFilter|OriginalEstimated", "Original Estimated");
		}

		void AddFindboxFilters(ModuleFilterCollection filters)
		{
			filters.AddNkFilter(
				description: "Staff",
				nkFilterColumn: ProcessTasksSchema.P9_GS_NKAssignedStaffMember,
				iD: ModuleIDs.GlbStaff,
				list: Staffs
			).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|WorkflowMilestonesFilter|Staff", "Staff");

			filters.AddGuidFilter(
				description: "Group",
				iD: ModuleIDs.GlbGroup,
				filterColumn: ProcessTasksSchema.P9_GG_AssignedGroup,
				list: Groups
			).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|WorkflowMilestonesFilter|Group", "Group");

			filters.AddGuidFilter(
				description: "Company",
				iD: ModuleIDs.GlbCompany,
				filterColumn: ProcessTasksSchema.P9_GC,
				list: Companies
			).MultilingualDescription = ResString.GetMultilingualString("MasterFiles|WorkflowMilestonesFilter|Company", "Company");

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
				result.AddToFilter(ProcessTasksSchema.P9_Type, SQLComparisonOperator.Equal, Core.Constants.Workflow.MilestoneType);

				return result;
			}
		}

		#region Lookups

		GlbGroupCollection Groups
		{
			get
			{
				return Factory.GetCachedValue("WorkflowMilestonesFilterBusinessObject|Groups", () =>
				{
					return new GlbGroupCollection(Factory);
				});
			}
		}

		GlbStaffCollection Staffs
		{
			get
			{
				return Factory.GetCachedValue("WorkflowMilestonesFilterBusinessObject|Staffs", () =>
				{
					return new GlbStaffCollection(Factory);
				});
			}
		}

		GlbCompanyCollection Companies
		{
			get
			{
				return Factory.GetCachedValue("WorkflowMilestonesFilterBusinessObject|Companies", () =>
				{
					return new GlbCompanyCollection(Factory);
				});
			}
		}

		ProcessTaskTemplateCollection Templates
		{
			get
			{
				return Factory.GetCachedValue("WorkflowMilestonesFilterBusinessObject|Templates", () =>
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

		public CodeDescriptionPairList ExceptionTypeCodesList
		{
			get
			{
				return Factory.GetCachedValue("WorkflowMilestonesFilterBusinessObject|ExceptionTypeCodesList", () =>
				{
					var exceptionTypeCodeList = new CodeDescriptionPairList();
					foreach (var eventType in Factory.Load<ProcessWorkflowExceptionType>(new ZQuery(ProcessWorkflowExceptionTypeSchema.WET_IsActive, true)))
					{
						exceptionTypeCodeList.Add(new CodeDescriptionPair(eventType.WET_Code.ToString(), eventType.WET_DescriptionMultilingual));
					}
					return exceptionTypeCodeList;
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
