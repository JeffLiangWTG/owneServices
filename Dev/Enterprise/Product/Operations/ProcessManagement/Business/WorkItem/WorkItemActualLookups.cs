using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ProcessManagement.Business
{
	/// <summary>
	/// Lookups actually for the WorkItem class, since WorkItemLookups is already used as the lookups for the base WorkItemCommon class.
	/// </summary>
	public class WorkItemActualLookups : WorkItemLookups, ISelectionCriteriaLookup
	{
		public WorkItemActualLookups()
			: base(new BusinessObjectFactory())
		{
		}

		public WorkItemActualLookups(WorkItem parent)
			: base(parent)
		{
		}

		public WorkItemActualLookups(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public WorkItemActualLookups(IWorkItemLookupsParent parent, BusinessObjectFactory factory)
			: base(factory)
		{
			nonBusinessObjectParent = parent;
		}

		public WorkItem ParentWorkItem => (WorkItem)base.Parent;
		public new IWorkItemLookupsParent Parent => nonBusinessObjectParent ?? (IWorkItemLookupsParent)base.Parent;
		readonly IWorkItemLookupsParent nonBusinessObjectParent;

		#region WorkItemType

		public CodeDescriptionPairList ActiveTypes
		{
			get { return GetActiveTypes(Factory); }
		}

		public static CodeDescriptionPairList GetActiveTypes(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("WorkItemActualLookups.ActiveTypes",
				delegate
				{
					return CreateWorkItemTypeList(true);
				});
		}

		public CodeDescriptionPairList AllTypes
		{
			get { return GetAllTypes(Factory); }
		}

		public static CodeDescriptionPairList GetAllTypes(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("WorkItemActualLookups.AllTypes",
				delegate
				{
					return CreateWorkItemTypeList(false);
				});
		}

		public static CodeDescriptionPairList CreateWorkItemTypeList(bool activeOnly) => new WorkItemTypeTreeProvider().GetWorkItemTypes(activeOnly);

		#endregion

		#region Work Item Area

		public CodeDescriptionPairList ActiveAreas
		{
			get
			{
				ZString parentType = Parent != null ? Parent.WKI_WorkItemType : ZString.Empty;
				return GetAreas(parentType, true);
			}
		}

		public CodeDescriptionPairList AllAreas
		{
			get
			{
				ZString parentType = Parent != null ? Parent.WKI_WorkItemType : ZString.Empty;
				return GetAreas(parentType, false);
			}
		}

		public CodeDescriptionPairList GetAreas(ZString parentType, bool activeOnly)
		{
			return GetAreas(Factory, parentType, activeOnly);
		}

		public static CodeDescriptionPairList GetAreas(BusinessObjectFactory factory, ZString parentType, bool activeOnly)
		{
			return factory.GetCachedValue(
				"WorkItemActualLookups.Areas:" + parentType + ":" + (activeOnly ? 'A' : 'X'),
				delegate
				{
					return CreateAreas(parentType, activeOnly);
				});
		}

		public static CodeDescriptionPairList CreateAreas(ZString parentType, bool activeOnly) => new WorkItemTypeTreeProvider().GetWorkItemAreas(parentType, activeOnly);

		#endregion

		#region Activity Type

		public CodeDescriptionPairList ActiveActivityTypes
		{
			get
			{
				return GetActivityTypeList(
					Parent != null ? Parent.WKI_WorkItemType : ZString.Empty,
					Parent != null ? Parent.WKI_WorkItemArea : ZString.Empty,
					true);
			}
		}

		public CodeDescriptionPairList AllActivityTypes
		{
			get
			{
				return GetActivityTypeList(
					Parent != null ? Parent.WKI_WorkItemType : ZString.Empty,
					Parent != null ? Parent.WKI_WorkItemArea : ZString.Empty,
					false);
			}
		}

		public CodeDescriptionPairList GetActivityTypeList(ZString parentType, ZString parentArea, bool activeOnly)
		{
			return GetActivityTypeList(Factory, parentType, parentArea, activeOnly);
		}

		public static CodeDescriptionPairList GetActivityTypeList(BusinessObjectFactory factory, ZString parentType, ZString parentArea, bool activeOnly)
		{
			return factory.GetCachedValue(
				"WorkItemActualLookups.ActivityTypeList:" + parentType + ':' + parentArea + ":" + (activeOnly ? 'A' : 'X'),
				delegate
				{
					return CreateActivityTypeList(parentType, parentArea, activeOnly);
				});
		}

		public static CodeDescriptionPairList CreateActivityTypeList(ZString parentType, ZString parentArea, bool activeOnly)
		{
			return new WorkItemTypeTreeProvider().GetActivityTypes(parentType, parentArea, activeOnly);
		}

		#endregion

		#region Activity Subtype

		public CodeDescriptionPairList ActiveActivitySubtypes
		{
			get
			{
				return GetActivitySubtypeList(
					Parent != null ? Parent.WKI_WorkItemType : ZString.Empty,
					Parent != null ? Parent.WKI_WorkItemArea : ZString.Empty,
					Parent != null ? Parent.WKI_ActivityType : ZString.Empty,
					true);
			}
		}

		public CodeDescriptionPairList AllActivitySubtypes
		{
			get
			{
				return GetActivitySubtypeList(
					Parent != null ? Parent.WKI_WorkItemType : ZString.Empty,
					Parent != null ? Parent.WKI_WorkItemArea : ZString.Empty,
					Parent != null ? Parent.WKI_ActivityType : ZString.Empty,
					false);
			}
		}

		public CodeDescriptionPairList GetActivitySubtypeList(ZString parentType, ZString parentArea, ZString activityType, bool activeOnly)
		{
			return GetActivitySubtypeList(Factory, parentType, parentArea, activityType, activeOnly);
		}

		public static CodeDescriptionPairList GetActivitySubtypeList(BusinessObjectFactory factory, ZString parentType, ZString parentArea, ZString activityType, bool activeOnly)
		{
			return factory.GetCachedValue(
				"WorkItemActualLookups.ActivitySubtypeList:" + parentType + ':' + parentArea + ':' + activityType + ':' + (activeOnly ? 'A' : 'X'),
				delegate
				{
					return CreateActivitySubtypeList(parentType, parentArea, activityType, activeOnly);
				});
		}

		public static CodeDescriptionPairList CreateActivitySubtypeList(ZString parentType, ZString parentArea, ZString activityType, bool activeOnly)
		{
			return new WorkItemTypeTreeProvider().GetActivitySubtypes(parentType, parentArea, activityType, activeOnly);
		}

		#endregion

		#region PriorityList

		public CodeDescriptionPairList ActivePriorities
		{
			get
			{
				return GetPriorities(true,
					Parent != null ? Parent.WKI_WorkItemType : ZString.Empty,
					Parent != null ? Parent.WKI_WorkItemArea : ZString.Empty,
					Parent != null ? Parent.WKI_ActivityType : ZString.Empty,
					Parent != null ? Parent.WKI_ActivitySubtype : ZString.Empty);
			}
		}

		public CodeDescriptionPairList AllPriorities
		{
			get
			{
				return GetPriorities(false,
					Parent != null ? Parent.WKI_WorkItemType : ZString.Empty,
					Parent != null ? Parent.WKI_WorkItemArea : ZString.Empty,
					Parent != null ? Parent.WKI_ActivityType : ZString.Empty,
					Parent != null ? Parent.WKI_ActivitySubtype : ZString.Empty);
			}
		}

		public CodeDescriptionPairList GetPriorities(bool activeOnly, ZString parentType, ZString parentArea, ZString activityType, ZString activitySubType)
		{
			return GetPriorities(Factory, activeOnly, parentType, parentArea, activityType, activitySubType);
		}

		public static CodeDescriptionPairList GetPriorities(BusinessObjectFactory factory, bool activeOnly, ZString parentType, ZString parentArea, ZString activityType, ZString activitySubType)
		{
			return factory.GetCachedValue(
				"WorkItemActualLookups.Priorities:" + parentType + ':' + parentArea + ':' + activityType + ':' + activitySubType + ':' + (activeOnly ? 'A' : 'X'),
				delegate
				{
					return CreatePriorities(activeOnly, parentType, parentArea, activityType, activitySubType);
				});
		}

		static public CodeDescriptionPairList CreatePriorities(bool activeOnly, ZString parentType, ZString parentArea, ZString activityType, ZString activitySubType)
		{
			return new WorkItemTypeTreeProvider().GetPriorities(parentType, parentArea, activityType, activitySubType, activeOnly);
		}

		#endregion

		#region ISelectionCriteriaLookup

		ZString ISelectionCriteriaLookup.GetCriterionLabel(int index)
		{
			switch (index)
			{
				case 1:
					return ProcessManagementRegistry.Instance.WorkItemTypeLabel.Value;
				case 2:
					return ProcessManagementRegistry.Instance.WorkItemAreaLabel.Value;
				case 3:
					return ProcessManagementRegistry.Instance.WorkItemActivityTypeLabel.Value;
				case 4:
					return ProcessManagementRegistry.Instance.WorkItemActivitySubTypeLabel.Value;
				case 5:
					return ProcessManagementRegistry.Instance.WorkItemPriorityLabel.Value;
				default:
					return ZString.Empty;
			}
		}

		ICodeDescriptionPairList ISelectionCriteriaLookup.GetCriterionList(string criterion1, string criterion2, string criterion3, string criterion4)
		{
			if (criterion1 == null)
			{
				return ActiveTypes;
			}
			if (criterion2 == null)
			{
				return GetAreas(criterion1, true);
			}
			if (criterion3 == null)
			{
				return GetActivityTypeList(criterion1, criterion2, true);
			}
			if (criterion4 == null)
			{
				return GetActivitySubtypeList(criterion1, criterion2, criterion3, true);
			}
			return GetPriorities(true, criterion1, criterion2, criterion3, criterion4);
		}

		#endregion

		#region Status

		public CodeDescriptionPairList StatusList
		{
			get { return Factory.GetCachedValue<ProcessTaskStatusCodeList>(); }
		}

		#endregion

		#region Other Work Items

		public WorkItemCollection OtherWorkItems
		{
			get
			{
				if (otherWorkItems == null)
				{
					ZQuery query = new ZQuery(WorkItemSchema.PK, SQLComparisonOperator.NotEqual, base.Parent.PK);
					otherWorkItems = new WorkItemCollection(Factory, query);
				}
				return otherWorkItems;
			}
		}

		WorkItemCollection otherWorkItems;

		#endregion

		#region Defect Tasks

		public override ProcessTaskCollection DefectCausedByTasks
		{
			get
			{
				var defectCausedByWorkItem = ParentWorkItem.DefectCausedByWorkItem;
				using (ProcessTaskCollection.CanCreateTaskCollection())
				{
					WorkItemProcessTaskCollection collection;

					if (defectCausedByWorkItem != null)
					{
						collection = new WorkItemProcessTaskCollection(defectCausedByWorkItem);

						var query = new ZQuery(ProcessTasksSchema.P9_Status, ProcessTaskStatusCodeList.Codes.Closed);
						var taskTypes = ProcessManagementRegistry.Instance.DefectIntroducedTaskTypes.Value;
						if (taskTypes != null && taskTypes.Length > 0)
						{
							query.AddToFilter(ProcessTasksSchema.P9_Type, taskTypes);
						}
						if (!ParentWorkItem.WKI_P9_DefectCausedByTask.IsEmpty)
						{
							query.AddToFilter(JoinCondition.Or, ProcessTasksSchema.PK, ParentWorkItem.WKI_P9_DefectCausedByTask);
						}

						collection.Load(query);
						collection.Sort(ProcessTasksSchema.Constants.P9_Sequence, System.ComponentModel.ListSortDirection.Ascending);
					}
					else
					{
						collection = new WorkItemProcessTaskCollection(Factory);
					}
					return collection;
				}
			}
		}

		public override ProcessTaskCollection DefectFirstMissedInTasks
		{
			get
			{
				var defectCausedByWorkItem = ParentWorkItem.DefectCausedByWorkItem;
				using (ProcessTaskCollection.CanCreateTaskCollection())
				{
					WorkItemProcessTaskCollection collection = null;

					if (defectCausedByWorkItem != null)
					{
						collection = new WorkItemProcessTaskCollection(defectCausedByWorkItem);

						var categorisedTaskTypes = WorkflowDataRegistry.Instance.TaskTypes.Value;
						var workItemCategory = categorisedTaskTypes.Cast<CategorisedWorkflowTaskTypes>()
							.SingleOrDefault(c => c.Code == WorkflowDescriptors.WorkItemWorkflowDescriptorCode);

						if (workItemCategory != null)
						{
							var containmentBarrierTaskTypes = workItemCategory.TaskTypes
								.OfType<WorkflowTaskType>()
								.Where(task => task.ContainmentBarrierIterationType != ContainmentBarrierIterationTypeList.Codes.NCB)
								.Select(task => task.Code);

							var query = new ZQuery(ProcessTasksSchema.P9_Status, ProcessTaskStatusCodeList.Codes.Closed);
							query.AddToFilter(ProcessTasksSchema.P9_Type, containmentBarrierTaskTypes);

							collection.Load(query);
							collection.Sort(ProcessTasksSchema.Constants.P9_Sequence, System.ComponentModel.ListSortDirection.Ascending);
						}
					}

					return collection ?? new WorkItemProcessTaskCollection(Factory);
				}
			}
		}

		#endregion
	}
}
