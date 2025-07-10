using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using GlowIndexQueryService.Business;

namespace Enterprise.MasterFiles.Module
{
	public class WorkflowFilterStripsHelper : FilterStripsHelper, IWorkflowFilterStripsHelper
	{
		protected WorkflowFilterStripsHelper()
		{
			ShouldAddMilestoneFilters = true;
			ShouldAddMiscFilters = true;
			shouldAddWorkflowCustomFieldsFilters = true;
			ShouldAddExceptionsInMiscFilters = true;
			templateCodeForCustomFieldsFilters = TemplateCode;
		}

		protected WorkflowFilterStripsHelper(Type businessObjectType, ZString templateCode, BusinessObjectFactory factory, bool shouldFilterByCompanyForAnyOpenTask)
			: base(businessObjectType, factory)
		{
			TemplateCode = templateCode;
			this.shouldFilterByCompanyForAnyOpenTask = shouldFilterByCompanyForAnyOpenTask;
			ShouldAddMilestoneFilters = true;
			ShouldAddMiscFilters = true;
			shouldAddWorkflowCustomFieldsFilters = true;
			ShouldAddExceptionsInMiscFilters = true;
			templateCodeForCustomFieldsFilters = TemplateCode;
		}

		public WorkflowFilterStripsHelper(Type businessObjectType, ZString templateCode, BusinessObjectFactory factory)
			: this(businessObjectType, templateCode, factory, true)
		{
		}

		protected WorkflowFilterStripsHelper(Type businessObjectType)
			: this(businessObjectType, "", null)
		{
		}

		public bool ShouldAddMilestoneFilters { get; set; }
		public bool ShouldAddRelatedMilestoneFilters { get; set; }
		public bool ShouldAddMiscFilters { get; set; }
		public bool ShouldAddExceptionsInMiscFilters { get; set; }

		protected ZString TemplateCode { get; private set; }

		protected List<ZDBOnlySubQuery> RelatedParentJoiningQueries
		{
			get { return relatedMilestonesParentJoiningQueries ?? (relatedMilestonesParentJoiningQueries = new List<ZDBOnlySubQuery>()); }
		}
		List<ZDBOnlySubQuery> relatedMilestonesParentJoiningQueries;

		protected readonly FilterCategory TasksCategory = GetTasksCategory();
		readonly FilterCategory milestonesCategory = FilterCategories.GetOrCreateFilterCategory(ResString.GetMultilingualString("ModuleFilter|Common|WorkflowFilterCategory|Milestones", "Workflow Milestones"));
		readonly FilterCategory milestonesRelatedCategory = FilterCategories.GetOrCreateFilterCategory(ResString.GetMultilingualString("ModuleFilter|Common|WorkflowFilterCategory|MilestonesRelated", "Workflow Milestones (Related)"));
		readonly bool shouldFilterByCompanyForAnyOpenTask;

		public static FilterCategory GetTasksCategory() => FilterCategories.GetOrCreateFilterCategory(ResString.GetMultilingualString("ModuleFilter|Common|WorkflowFilterCategory|Tasks", "Workflow Tasks"));

		public override void Initialise(Type businessObjectType, BusinessObjectFactory factory)
		{
			base.Initialise(businessObjectType, factory);

			if (IsApplicableToBizOTypeIsAssignableFrom() && string.IsNullOrEmpty(TemplateCode))
			{
				SetTemplateCodeFromBusinessObjectType();
			}
		}

		void SetTemplateCodeFromBusinessObjectType()
		{
			CheckApplicableToBizOTypeIsAssignableFrom();
			TemplateCode = GetTemplateCodeForBusinessObjectType(BusinessObjectType);
		}

		[SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
		[SuppressMessage("CargoWiseOne", "CW1024:BadConcurrentCollectionAccess")]

		static WorkflowFilterStripsHelper()
		{
			//Initialize some common and expensive workflow types.
			templateCodeCache = new MRUCache<(string, string), string>(20);
			lock (templateCodeCache)
			{
				templateCodeCache.SetValue(("", "ForwardingShipment"), "SHP");
				templateCodeCache.SetValue(("", "ForwardingModuleShipment"), "SHP");
				templateCodeCache.SetValue(("", "ForwardingConsol"), "CON");
				templateCodeCache.SetValue(("", "ForwardingModuleConsol"), "CON");
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1021", Justification = "We are always locking it when accessing it.")]
		static MRUCache<(string, string), string> templateCodeCache;

		[SuppressMessage("CargoWiseOne", "CW1024:BadConcurrentCollectionAccess")]
		public static ZString GetTemplateCodeForBusinessObjectType(Type businessObjectType)
		{
			if (businessObjectType == null)
			{
				return ZString.Empty;
			}

			IWorkflowProvider provider = null;

			//prevent possible NRE? I don't know if this is even remotely possible though
			if (templateCodeCache == null)
			{
				Thread.Sleep(1000);
				while (templateCodeCache == null)
				{ templateCodeCache = new MRUCache<(string, string), string>(20); Thread.Sleep(1000); }
			}

			lock (templateCodeCache)
			{
				if (templateCodeCache.TryGetValue((GlbCompany.CurrentCompany?.Country?.Code ?? "", businessObjectType.Name), out var result))
				{
					return result;
				}
				if (templateCodeCache.TryGetValue(("", businessObjectType.Name), out result))
				{
					return result;
				}
			}

			try
			{
				var readOnlyFactory = new ReadOnlyBusinessObjectFactory() { NameForDebugging = "WorkflowFilterStripsHelper ReadOnlyFactory", RefreshEnabled = false };
				readOnlyFactory.SuspendValidation();
				provider = readOnlyFactory.New(businessObjectType) as IWorkflowProvider;
			}
			catch (NoConcreteTypeException)
			{
			}
			catch (NotSupportedException ex)
			{
				if (!ex.Message.StartsWith((NoResString)"No default concrete type for", StringComparison.OrdinalIgnoreCase))
				{
					throw;
				}
			}
			catch (ApplicationException)
			{
			}
			catch (ZException ex)
			{
				if (!ex.Message.Contains((NoResString)"is not a valid BusinessObject type", StringComparison.OrdinalIgnoreCase) && !ex.Message.Contains((NoResString)"does not have a Schema.TableName"))
				{
					throw;
				}
			}

			lock (templateCodeCache)
			{
				var result = provider?.WorkflowType ?? ZString.Empty;
				templateCodeCache.SetValue((GlbCompany.CurrentCompany?.Country?.Code ?? "", businessObjectType.Name),
					result);
				return result;
			}
		}

		public override bool IsApplicableToBizOTypeIsAssignableFrom()
		{
			return typeof(IWorkflowProvider).IsAssignableFrom(BusinessObjectType);
		}

		public override bool CanAddFilters()
		{
			return base.CanAddFilters() && (!string.IsNullOrEmpty(TemplateCode));
		}

		protected override void AddFilterStrips(ModuleFilterCollection filters)
		{
			if (CanAddFilters())
			{
				if (ShouldAddMilestoneFilters)
				{
					AddMilestonesFilters(filters);
				}

				if (ShouldAddRelatedMilestoneFilters)
				{
					AddRelatedMilestoneFilters(filters, RelatedParentJoiningQueries.ToArray());
				}

				if (shouldAddWorkflowCustomFieldsFilters)
				{
					filters.AddWorkflowCustomFieldsFilters(Factory, templateCodeForCustomFieldsFilters, BusinessObjectType);
				}

				if (ShouldAddMiscFilters)
				{
					AddMiscFilters(filters);
				}
			}
		}

		public virtual bool IsAlternativeTaskSearchOnly { get => false; }

		protected SchemaGuidColumn AlternativeTaskParentColumn { get; set; }

		protected ZString AlternativeTaskParentTableCode { get; set; }

		protected virtual SchemaGuidColumn GetPrimaryKeyColumn() => BusinessObjectFactory.GetTableSchemaFromType(BusinessObjectType).PK;

		void AddMiscFilters(ModuleFilterCollection filters)
		{
			var primaryKeyColumn = GetPrimaryKeyColumn();
			var tasksFilter = new TasksModuleFilter("Tasks", primaryKeyColumn, ProcessTasksSchema.P9_ParentID, new ProcessTaskCollection(Factory), BusinessObjectType);
			tasksFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|WorkflowFilterStripsHelper|Tasks", "Tasks");
			tasksFilter.AlternativeParentColumn = AlternativeTaskParentColumn;
			tasksFilter.AlternativeParentTableCode = AlternativeTaskParentTableCode;
			tasksFilter.IsAlternativeSearchOnly = IsAlternativeTaskSearchOnly;

			var milestonesFilter = new MilestonesModuleFilter("Milestones", primaryKeyColumn, Factory, BusinessObjectType);
			milestonesFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|WorkflowFilterStripsHelper|Milestones", "Milestones");
			milestonesFilter.AlternativeParentColumn = AlternativeTaskParentColumn;
			milestonesFilter.AlternativeParentTableCode = AlternativeTaskParentTableCode;
			milestonesFilter.IsAlternativeSearchOnly = IsAlternativeTaskSearchOnly;

			var triggersFilter = new TriggersModuleFilter("Triggers", primaryKeyColumn, Factory, BusinessObjectType);
			triggersFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|WorkflowFilterStripsHelper|Triggers", "Triggers");
			triggersFilter.AlternativeParentColumn = AlternativeTaskParentColumn;
			triggersFilter.AlternativeParentTableCode = AlternativeTaskParentTableCode;
			triggersFilter.IsAlternativeSearchOnly = IsAlternativeTaskSearchOnly;

			AddTaskCategoryFilter(tasksFilter, filters);

			if (ShouldAddExceptionsInMiscFilters)
			{
				var exceptionsFilter = new ExceptionsModuleFilter("Exceptions", primaryKeyColumn, Factory, BusinessObjectType);
				exceptionsFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|WorkflowFilterStripsHelper|Exceptions", "Exceptions");
				exceptionsFilter.AlternativeParentColumn = AlternativeTaskParentColumn;
				exceptionsFilter.AlternativeParentTableCode = AlternativeTaskParentTableCode;
				exceptionsFilter.IsAlternativeSearchOnly = IsAlternativeTaskSearchOnly;

				AddTaskCategoryFilter(exceptionsFilter, filters);
			}

			AddTaskCategoryFilter(milestonesFilter, filters);
			AddTaskCategoryFilter(triggersFilter, filters);
		}

		void AddTaskCategoryFilter(ModuleFilter filter, ModuleFilterCollection filters)
		{
			filter.Category = TasksCategory;
			filter.IsPublishedOnWeb = false;
			filters.AddFilter(filter);
		}

		public void SetShouldAddWorkflowCustomFieldsFilters(bool shouldAdd, string customTemplateCode = default(string))
		{
			shouldAddWorkflowCustomFieldsFilters = shouldAdd;
			templateCodeForCustomFieldsFilters = string.IsNullOrEmpty(customTemplateCode) ? TemplateCode.ToString() : customTemplateCode;
		}

		bool shouldAddWorkflowCustomFieldsFilters;
		string templateCodeForCustomFieldsFilters;

		#region Milestone Filters

		protected void AddMilestonesFilters(ModuleFilterCollection filters)
		{
			WorkflowModuleFilter milestoneDateFilter = GetNewFilter(WorkflowFiltersDescription.MilestoneDateName, WorkflowModuleFilterTypes.MilestoneDate);
			milestoneDateFilter.Category = milestonesCategory;
			milestoneDateFilter.MultilingualDescription = WorkflowFiltersDescription.MilestoneDateDesc;
			filters.AddCustomFilter(milestoneDateFilter);

			WorkflowModuleTextFilter milestoneCompletedFilter = GetNewTextFilter(WorkflowFiltersDescription.MilestoneCompletedName, GetMilestoneCompletedFilter, MilestoneCompletedList);
			milestoneCompletedFilter.Category = milestonesCategory;
			milestoneCompletedFilter.Property = MilestoneCompletedCodes.Completed;
			milestoneCompletedFilter.MultilingualDescription = WorkflowFiltersDescription.MilestoneCompletedDesc;
			filters.AddCustomFilter(milestoneCompletedFilter);

			WorkflowModuleFilter milestoneNextFilter = GetNewFilter(WorkflowFiltersDescription.NextMilestoneName, WorkflowModuleFilterTypes.MilestoneNext);
			milestoneNextFilter.Category = milestonesCategory;
			milestoneNextFilter.MultilingualDescription = WorkflowFiltersDescription.NextMilestoneDesc;
			filters.AddCustomFilter(milestoneNextFilter);

			WorkflowModuleFilter milestoneLastCompletedFilter = GetNewFilter(WorkflowFiltersDescription.LastCompletedMilestoneName, WorkflowModuleFilterTypes.MilestoneLastCompleted);
			milestoneLastCompletedFilter.Category = milestonesCategory;
			milestoneLastCompletedFilter.MultilingualDescription = WorkflowFiltersDescription.LastCompletedMilestoneDesc;
			filters.AddCustomFilter(milestoneLastCompletedFilter);

			ModuleNkFilter anyOpenTaskAssignedToFilter = filters.AddNkFilter(WorkflowFiltersDescription.AnyOpenTaskAssignedToName, GetAnyOpenTaskAssignedToFilter, ModuleIDs.GlbStaff, GetStaffList(Factory));
			anyOpenTaskAssignedToFilter.Category = TasksCategory;
			anyOpenTaskAssignedToFilter.IsPublishedOnWeb = false;
			anyOpenTaskAssignedToFilter.MultilingualDescription = WorkflowFiltersDescription.AnyOpenTaskAssignedToDesc;

			ModuleNkFilter nextTaskAssignedToFilter = filters.AddNkFilter(WorkflowFiltersDescription.NextTaskAssignedToName, GetNextTaskAssignedToFilter, ModuleIDs.GlbStaff, GetStaffList(Factory));
			nextTaskAssignedToFilter.Category = TasksCategory;
			nextTaskAssignedToFilter.IsPublishedOnWeb = false;
			nextTaskAssignedToFilter.MultilingualDescription = WorkflowFiltersDescription.NextTaskAssignedToDesc;
		}

		#endregion

		#region Related Job Filters

		protected void AddRelatedMilestoneFilters(ModuleFilterCollection filters, params ZDBOnlySubQuery[] relatedParentJoiningQueries)
		{
			var milestoneRelatedDateFilter = GetNewFilter((NoResString)"Milestone Date (Related)", WorkflowModuleFilterTypes.MilestoneDate);
			milestoneRelatedDateFilter.Category = milestonesRelatedCategory;
			milestoneRelatedDateFilter.RelatedParentSubQueries = relatedParentJoiningQueries;
			milestoneRelatedDateFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|WorkflowFilterStipsHelper|MilestoneDateRelated", "Milestone Date (Related)");
			filters.AddCustomFilter(milestoneRelatedDateFilter);

			var milestoneRelatedCompletedFilter = GetNewTextFilter((NoResString)"Milestone Completed (Related)", GetMilestoneCompletedFilter, MilestoneCompletedList);
			milestoneRelatedCompletedFilter.Category = milestonesRelatedCategory;
			milestoneRelatedCompletedFilter.Property = MilestoneCompletedCodes.Completed;
			milestoneRelatedCompletedFilter.RelatedParentSubQueries = relatedParentJoiningQueries;
			milestoneRelatedCompletedFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|WorkflowFilterStipsHelper|MilestoneCompletedRelated", "Milestone Completed (Related)");
			filters.AddCustomFilter(milestoneRelatedCompletedFilter);

			var milestoneRelatedNextFilter = GetNewFilter((NoResString)"Next Milestone (Related)", WorkflowModuleFilterTypes.MilestoneNext);
			milestoneRelatedNextFilter.Category = milestonesRelatedCategory;
			milestoneRelatedNextFilter.RelatedParentSubQueries = relatedParentJoiningQueries;
			milestoneRelatedNextFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|WorkflowFilterStipsHelper|NextMilestoneRelated", "Next Milestone (Related)");
			filters.AddCustomFilter(milestoneRelatedNextFilter);

			var milestoneRelatedLastCompletedFilter = GetNewFilter((NoResString)"Last Completed Milestone (Related)", WorkflowModuleFilterTypes.MilestoneLastCompleted);
			milestoneRelatedLastCompletedFilter.Category = milestonesRelatedCategory;
			milestoneRelatedLastCompletedFilter.RelatedParentSubQueries = relatedParentJoiningQueries;
			milestoneRelatedLastCompletedFilter.MultilingualDescription = ResString.GetMultilingualString("MasterFiles|WorkflowFilterStipsHelper|LastCompletedMilestoneRelated", "Last Completed Milestone (Related)");
			filters.AddCustomFilter(milestoneRelatedLastCompletedFilter);
		}

		public void AddRelatedParentJoiningQuery(ZDBOnlySubQuery query)
		{
			RelatedParentJoiningQueries.Add(query);
		}

		#endregion

		#region Implementation

		protected virtual WorkflowModuleFilter GetNewFilter(string description, WorkflowModuleFilterTypes filterType)
		{
			return new WorkflowModuleFilter(description, BusinessObjectType, filterType, TemplateCode);
		}

		protected virtual WorkflowModuleTextFilter GetNewTextFilter(string description, GetTextQuery queryDelegate, IList list)
		{
			return new WorkflowModuleTextFilter(description, queryDelegate, list, BusinessObjectType, TemplateCode);
		}

		ZQuery GetMilestoneCompletedFilter(ZString value)
		{
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(ProcessTask), ProcessTasksSchema.P9_ParentID);
			subQuery.AddToFilter(ProcessTasksSchema.P9_Type, Core.Constants.Workflow.MilestoneType);

			if (Globals.IsWeb)
			{
				subQuery.AddToFilter(ProcessTasksSchema.P9_IsPublished, true);
			}

			if (value == MilestoneCompletedCodes.Completed)
			{
				subQuery.AddToFilter(ProcessTasksSchema.P9_ActualDate, SQLComparisonOperator.NotEqual, ZDateTime.Empty);
			}
			else if (value == MilestoneCompletedCodes.NotCompleted)
			{
				subQuery.AddToFilter(ProcessTasksSchema.P9_ActualDate, SQLComparisonOperator.Equal, ZDateTime.Empty);
			}

			return subQuery;
		}

		ZQuery GetAnyOpenTaskAssignedToFilter(ZString value)
		{
			ZDBOnlySubQuery subQuery = GetAnyOpenTaskAssignedToFilter_ProcessTaskSubQuery(value);
			ZDBOnlyQuery query = new ZDBOnlyQuery(BusinessObjectType);

			if (IsAlternativeTaskSearchOnly)
			{
				if (AlternativeTaskParentColumn != null)
				{
					if (TryGetColumnSuffix(AlternativeTaskParentColumn, out var tableCode))
					{
						subQuery.AddToFilter(GetParentTableCodeQuery(tableCode, ProcessTasksSchema.P9_ParentTableCode));
					}
					query.AddSubQuery(AlternativeTaskParentColumn, subQuery, JoinCondition.Or);
				}
			}
			else
			{
				if (AlternativeTaskParentColumn == null)
				{
					if (typeof(IWorkflowProvider).IsAssignableFrom(BusinessObjectType))
					{
						subQuery.AddToFilter(GetParentTableCodeQuery(BusinessObjectFactory.GetTableCodeFromType(BusinessObjectType), ProcessTasksSchema.P9_ParentTableCode));
					}
					query.AddSubQuery(GetPrimaryKeyColumn(), subQuery, JoinCondition.Or);
				}
				else
				{
					var parentPKColumn = AlternativeTaskParentColumn.TableSchema.PK;
					ZDBOnlySubQuery mainSubQuery = new ZDBOnlySubQuery(BusinessObjectType, parentPKColumn);
					var subQuery1 = (ZDBOnlySubQuery)subQuery.DeepClone();
					if (typeof(IWorkflowProvider).IsAssignableFrom(BusinessObjectType))
					{
						subQuery1.AddToFilter(GetParentTableCodeQuery(BusinessObjectFactory.GetTableCodeFromType(BusinessObjectType), ProcessTasksSchema.P9_ParentTableCode));
					}

					mainSubQuery.AddSubQuery(subQuery1, JoinCondition.And);

					ZDBOnlySubQuery altSubQuery = new ZDBOnlySubQuery(BusinessObjectType, parentPKColumn);
					var subQuery2 = subQuery;
					if (TryGetColumnSuffix(AlternativeTaskParentColumn, out var tableCode))
					{
						subQuery2.AddToFilter(GetParentTableCodeQuery(tableCode, ProcessTasksSchema.P9_ParentTableCode));
					}
					altSubQuery.AddSubQuery(AlternativeTaskParentColumn, subQuery2, JoinCondition.And);

					mainSubQuery.AddAsUnionQuery(altSubQuery, true);

					query.AddSubQuery(mainSubQuery, JoinCondition.Or);
				}
			}

			return query;
		}

		ZDBOnlySubQuery GetAnyOpenTaskAssignedToFilter_ProcessTaskSubQuery(ZString value)
		{
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(ProcessTask), ProcessTasksSchema.P9_ParentID);
			var assignedStatuses = new[]
			{
				ProcessTaskStatusCodeList.Codes.Assigned,
				ProcessTaskStatusCodeList.Codes.Working,
				ProcessTaskStatusCodeList.Codes.Suspended
			};
			subQuery.AddToFilter(ProcessTasksSchema.P9_Status, assignedStatuses);
			subQuery.AddToFilter(JoinCondition.And, ProcessTasksSchema.P9_GS_NKAssignedStaffMember, SQLComparisonOperator.Equal, value);
			subQuery.AddToFilter(ProcessTasksSchema.P9_Type, SQLComparisonOperator.NotEqual, Enterprise.Core.Constants.Workflow.ExceptionType);
			subQuery.AddToFilter(ProcessTasksSchema.P9_Type, SQLComparisonOperator.NotEqual, Enterprise.Core.Constants.Workflow.MilestoneType);
			subQuery.AddToFilter(ProcessTasksSchema.P9_Type, SQLComparisonOperator.NotEqual, Enterprise.Core.Constants.Workflow.WorkflowTriggerType);
			if (shouldFilterByCompanyForAnyOpenTask)
			{
				subQuery.AddToFilter(ProcessTasksSchema.P9_GC, GlbCompany.CurrentCompany.PK);
			}
			return subQuery;
		}

		ZQuery GetNextTaskAssignedToFilter(ZString value)
		{
			ZDBOnlySubQuery subQuery = GetNextTaskAssignedToFilter_ProcessTaskSubQuery(value);

			ZDBOnlyQuery query = new ZDBOnlyQuery(BusinessObjectType);

			if (IsAlternativeTaskSearchOnly)
			{
				if (AlternativeTaskParentColumn != null)
				{
					if (TryGetColumnSuffix(AlternativeTaskParentColumn, out var tableCode))
					{
						subQuery.AddToFilter(GetParentTableCodeQuery( tableCode, ProcessTasksSchema.P9_ParentTableCode));
					}
					query.AddSubQuery(AlternativeTaskParentColumn, subQuery, JoinCondition.And);
				}
			}
			else
			{
				if (typeof(IWorkflowProvider).IsAssignableFrom(BusinessObjectType))
				{
					subQuery.AddToFilter(GetParentTableCodeQuery(BusinessObjectFactory.GetTableCodeFromType(BusinessObjectType), ProcessTasksSchema.P9_ParentTableCode));
				}
				query.AddSubQuery(subQuery, JoinCondition.And);
			}

			if (BusinessObjectType == ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>())
			{
				AddNextTaskAssignedToFilter_AdditionalQueryForCustomsDeclaration(query, value);
			}

			return query;
		}

		public static ZQuery GetParentTableCodeQuery(string prefix, SchemaStringColumn column)
		{
			if (prefix != ProcessTasksSchema.Constants.Prefix)
			{
				var query = new ZQuery(column, prefix);
				if (prefix == JobDeclarationSchema.Constants.Prefix)
				{
					//because we compare against JE_ComputedParent, we also need to include JS.
					query.AddToFilter(JoinCondition.Or, column, JobShipmentSchema.Constants.Prefix);
				}
				//repeat for country specific cases...
				if (prefix == JPAFRHeaderSchema.Constants.Prefix)
				{
					query.AddToFilter(JoinCondition.Or, column, JobConsolSchema.Constants.Prefix);
				}
				if (prefix == CusInBondHeaderSchema.Constants.Prefix)
				{
					query.AddToFilter(JoinCondition.Or, column, JobShipmentSchema.Constants.Prefix);
				}
				return query;
			}
			return new ZQuery();
		}

		static bool TryGetColumnSuffix(SchemaGuidColumn column, out string tableCode)
		{
			tableCode = null;
			var columnName = column.Name;
			var split = columnName.Split('_');
			if (split.Length == 2 && split[1].Length >= 2 && split[1].Length <= 3)
			{
				tableCode = split[1];
				return true;
			}
			return false;
		}

		void AddNextTaskAssignedToFilter_AdditionalQueryForCustomsDeclaration(ZDBOnlyQuery query, ZString value)
		{
			string filterString = JobDeclarationSchema.Constants.PK + " IN (SELECT " + JobDeclarationSchema.Constants.PK
				+ " FROM " + JobDeclarationSchema.Constants.SqlSchemaName + "." + JobDeclarationSchema.Constants.TableName + " WHERE " + JobDeclarationSchema.Constants.JE_JS
				+ " IN (SELECT " + JobShipmentSchema.Constants.PK + " FROM " + JobShipmentSchema.Constants.SqlSchemaName + "." + JobShipmentSchema.Constants.TableName
				+ " WHERE " + JobShipmentSchema.Constants.PK + " IN (SELECT pt1." + ProcessTasksSchema.Constants.P9_ParentID + " FROM ( SELECT MIN(" + ProcessTasksSchema.Constants.P9_Sequence
				+ ") minP9Sequence, " + ProcessTasksSchema.Constants.P9_ParentID + " FROM " + ProcessTasksSchema.Constants.SqlSchemaName + "." + ProcessTasksSchema.Constants.TableName
				+ " WHERE " + ProcessTasksSchema.Constants.P9_Status + " <> '" + ProcessTaskStatusCodeList.Codes.Closed + "' AND "
				+ ProcessTasksSchema.Constants.P9_Status + " <> '" + ProcessTaskStatusCodeList.Codes.Cancelled + "' GROUP BY "
				+ ProcessTasksSchema.Constants.P9_ParentID + ") pt1 INNER JOIN (SELECT * FROM "
				+ ProcessTasksSchema.Constants.SqlSchemaName + "." + ProcessTasksSchema.Constants.TableName + " ) pt2 ON pt1." + ProcessTasksSchema.Constants.P9_ParentID + " = pt2."
				+ ProcessTasksSchema.Constants.P9_ParentID + " AND pt1.minP9Sequence = pt2." + ProcessTasksSchema.Constants.P9_Sequence + " WHERE "
				+ ProcessTasksSchema.Constants.P9_GS_NKAssignedStaffMember + " = '" + value + "') AND " + JobShipmentSchema.Constants.PK
				+ " IN (SELECT " + JobDeclarationSchema.Constants.JE_JS + " FROM " + JobDeclarationSchema.Constants.SqlSchemaName + "." + JobDeclarationSchema.Constants.TableName + " )))";

			query.AddFilterAndZSQLParameterCollection(filterString, new ZSqlParameterCollection(), JoinCondition.Or);
		}

		ZDBOnlySubQuery GetNextTaskAssignedToFilter_ProcessTaskSubQuery(ZString value)
		{
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(ProcessTask), ProcessTasksSchema.P9_ParentID);

			const string sqlTemplate =
				"{0} IN (SELECT pt1.{0}" + // P9_ParentID
						 " FROM {1} pt1" + // ProcessTasks
						" WHERE pt1.{2} = '{3}'" + // P9_GS_NKAssignedStaffMember = value
							" AND pt1.{4} <> '{5}'" + // P9_Status <> 'CLS'
							" AND pt1.{4} <> '{6}'" + // P9_Status <> 'CAN'
							" AND NOT EXISTS(SELECT null" +
											 " FROM {1} pt2" + // ProcessTasks
											" WHERE pt2.{0} = pt1.{0}" + // P9_ParentID
											" AND pt2.{4} <> '{5}'" + // P9_Status <> 'CLS'
											" AND pt2.{4} <> '{6}'" + // P9_Status <> 'CAN'
											" AND pt2.{7} < pt1.{7}" + // P9_Sequence
											" and " +
											"(" +
												" pt1.{8} IN ('{9}', '{10}') AND pt2.{8} = pt1.{8} " + //if type is MIL or EXC, should look inside MIL or EXC collection
												" or pt1.{8} not in ('{9}','{10}','{11}') and " + // if not MIL or EXC or TRG, should look for any other types,
												" pt2.{8} not in ('{9}','{10}','{11}') " + // because they may be customized via registry
											")" +
										")" +
						 ")";

			string sql = string.Format(sqlTemplate,
				ProcessTasksSchema.Constants.P9_ParentID, ProcessTasksSchema.Constants.TableName,
				ProcessTasksSchema.Constants.P9_GS_NKAssignedStaffMember, value,
				ProcessTasksSchema.Constants.P9_Status, ProcessTaskStatusCodeList.Codes.Closed, ProcessTaskStatusCodeList.Codes.Cancelled,
				ProcessTasksSchema.Constants.P9_Sequence, ProcessTasksSchema.Constants.P9_Type,
				Core.Constants.Workflow.MilestoneType, Core.Constants.Workflow.ExceptionType, Core.Constants.Workflow.WorkflowTriggerType);

			subQuery.AddFilterAndZSQLParameterCollection(sql, new ZSqlParameterCollection(), JoinCondition.And);

			return subQuery;
		}

		public static ZQuery GetParentTableCodeQuery(Type bizObjType)
		{
			return new ZQuery(ProcessTasksSchema.P9_ParentTableCode, SQLComparisonOperator.Equal, BusinessObjectFactory.GetTableCodeFromType(bizObjType));
		}

		internal static ZQuery GetSourceTemplateQuery(SQLComparisonOperator comparisonOperator, object value)
		{
			var query = new ZDBOnlyQuery(typeof(ProcessTask));

			if (comparisonOperator == SpecialComparisonOperator.IsBlank)
			{
				query.AddToFilter(ProcessTasksSchema.P9_ParentTemplateID, null);
			}
			else if (comparisonOperator == SpecialComparisonOperator.IsNotBlank)
			{
				query.AddToFilter(ProcessTasksSchema.P9_ParentTemplateID, SQLComparisonOperator.NotEqual, null);
			}
			else
			{
				var taskTemplateQuery = new ZDBOnlySubQuery(typeof(ProcessTask), ProcessTasksSchema.PK);
				taskTemplateQuery.AddToFilter(ProcessTasksSchema.P9_ParentID, comparisonOperator, value);

				if (comparisonOperator == SQLComparisonOperator.NotEqual)
				{
					var taskWithBlankTemplateQuery = new ZDBOnlySubQuery(typeof(ProcessTask), ProcessTasksSchema.PK);
					taskWithBlankTemplateQuery.AddToFilter(ProcessTasksSchema.P9_ParentTemplateID, SQLComparisonOperator.Equal, null);

					var taskTemplateWrapperQuery = new ZDBOnlySubQuery(typeof(ProcessTask), ProcessTasksSchema.PK);
					taskTemplateWrapperQuery.AddSubQuery(ProcessTasksSchema.P9_ParentTemplateID, taskTemplateQuery, JoinCondition.And);
					taskTemplateWrapperQuery.AddAsUnionQuery(taskWithBlankTemplateQuery, addAsUnionAll: true);

					query.AddSubQuery(taskTemplateWrapperQuery, JoinCondition.And);
				}
				else
				{
					query.AddSubQuery(ProcessTasksSchema.P9_ParentTemplateID, taskTemplateQuery, JoinCondition.And);
				}
			}

			return query;
		}

		internal static ZQuery GetTriggerConditionValueQuery(SQLComparisonOperator comparisonOperator, ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(ProcessTask));

			if (comparisonOperator == SpecialComparisonOperator.IsBlank)
			{
				query.AddToFilter(ProcessTasksSchema.P9_Notes, null);
			}
			else if (comparisonOperator == SpecialComparisonOperator.IsNotBlank)
			{
				query.AddToFilter(ProcessTasksSchema.P9_Notes, SQLComparisonOperator.NotEqual, null);
			}
			else
			{
				var eventReferenceSqlComparisonOperator = ModuleTextFilter.GetSqlComparisonOperator(comparisonOperator.ToString(), SQLComparisonOperator.StartsWith);

				var subQuery = new ZDBOnlySubQuery(typeof(ProcessTask), ProcessTasksSchema.PK);
				subQuery.AddToFilter(ProcessTasksSchema.P9_Notes, eventReferenceSqlComparisonOperator, value);
				query.AddSubQuery(subQuery, JoinCondition.And);
			}

			return query;
		}

		#endregion

		#region Lookups

		#region Milestone List

		CodeDescriptionPairList MilestoneCompletedList
		{
			get
			{
				if (MCStatusList == null)
				{
					MCStatusList = new CodeDescriptionPairList();
					MCStatusList.AddPair(MilestoneCompletedCodes.All, Res.GetString("1575af7d-f996-4ab4-b12a-0b78cbaf25ef", "Where the Milestone exists on the job."));
					MCStatusList.AddPair(MilestoneCompletedCodes.Completed, Res.GetString("14db3abd-fb9e-4a17-8bf3-2d1e3be03ca2", "Where the Milestone is Completed."));
					MCStatusList.AddPair(MilestoneCompletedCodes.NotCompleted, Res.GetString("115e9773-6559-4b11-a55b-73b783193a68", "Where the Milestone is Not Completed."));
				}
				return MCStatusList;
			}
		}

		CodeDescriptionPairList MCStatusList;

		[SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		static class MilestoneCompletedCodes
		{
			public const string All = "All";
			public const string Completed = "Completed";
			public const string NotCompleted = "Not Completed";
		}

		#endregion

		#region Staff List

		protected GlbStaffCollection GetStaffList(BusinessObjectFactory factory)
		{
			return new GlbStaffCollection(factory);
		}

		#endregion

		#endregion

		#region IndexSearch

		protected override void AddFilterStripsForIndexSearch(ModuleFilterCollection filters, SearchField[] defaultHiddenIndexSearchFields)
		{
			if (CanAddFilters() && ShouldAddMilestoneFilters)
			{
				AddMilestonesFiltersForIndexSearch(filters, defaultHiddenIndexSearchFields);
			}
		}

		void AddMilestonesFiltersForIndexSearch(ModuleFilterCollection filters, SearchField[] searchFields)
		{
			var searchField = searchFields.FirstOrDefault(sf => sf.FieldName.ToUpperInvariant() == "MILESTONECOMPLETED");
			if (searchField != null)
			{
				var milestoneCompletedFilter = new IndexSearchModuleTextFilter(WorkflowFiltersDescription.MilestoneCompletedName, searchField, GetMilestoneCompletedIndexFilter, MilestoneCompletedList, milestonesCategory);
				milestoneCompletedFilter.Property = MilestoneCompletedCodes.Completed;
				milestoneCompletedFilter.MultilingualDescription = WorkflowFiltersDescription.MilestoneCompletedDesc;
				filters.AddCustomFilter(milestoneCompletedFilter);
			}

			searchField = searchFields.FirstOrDefault(sf => sf.FieldName.ToUpperInvariant() == "LASTCOMPLETEDMILESTONEACTUALTIME");
			if (searchField != null)
			{
				var milestoneLastCompletedFilter = new IndexSearchModuleDateFilter(searchField, milestonesCategory);
				((IModuleFilterForStrategyInternal)milestoneLastCompletedFilter).Description = WorkflowFiltersDescription.LastCompletedMilestoneName;
				milestoneLastCompletedFilter.MultilingualDescription = WorkflowFiltersDescription.LastCompletedMilestoneDesc;
				filters.AddCustomFilter(milestoneLastCompletedFilter);
			}

			searchField = searchFields.FirstOrDefault(sf => sf.FieldName.ToUpperInvariant() == "NEXTMILESTONEESTIMATEDTIME");
			if(searchField != null)
			{
				var milestoneNextFilter = new IndexSearchModuleDateFilter(searchField, milestonesCategory);
				((IModuleFilterForStrategyInternal)milestoneNextFilter).Description = WorkflowFiltersDescription.NextMilestoneName;
				milestoneNextFilter.MultilingualDescription = WorkflowFiltersDescription.NextMilestoneDesc;
				filters.AddCustomFilter(milestoneNextFilter);
			}
		}

		IGlowQuery GetMilestoneCompletedIndexFilter(SearchField field, ZString value)
		{
			var term = new Term(field.FieldName, null);
			if (value == MilestoneCompletedCodes.Completed)
			{
				return new EqualQuery(new Term(field.FieldName, $"true"), useQuotes: false);
			}
			else if (value == MilestoneCompletedCodes.NotCompleted)
			{
				return new EqualQuery(new Term(field.FieldName, $"false"), useQuotes: false);
			}
			return new EmptyQuery();
		}

		#endregion

		#region TestFiltersWorkForAllModules
#if DEBUG
		public override string GetAutomaticFilterTestCaseName_ForObjectFactory() => "WorkflowFilterStripsHelperAutomaticFilterTest";
#endif
		#endregion

		public void ClearCache() => WorkflowCustomFieldsFilter.ClearCache();

		static class WorkflowFiltersDescription
		{
			public static readonly string MilestoneDateName = (NoResString)"Milestone Date";
			public static readonly ResourceString MilestoneDateDesc = ResString.GetMultilingualString("MasterFiles|WorkflowFilterStipsHelper|MilestoneDate", "Milestone Date");
			public static readonly string MilestoneCompletedName = (NoResString)"Milestone Completed";
			public static readonly ResourceString MilestoneCompletedDesc = ResString.GetMultilingualString("MasterFiles|WorkflowFilterStipsHelper|MilesoneCompleted", "Milestone Completed");
			public static readonly string NextMilestoneName = (NoResString)"Next Milestone";
			public static readonly ResourceString NextMilestoneDesc = ResString.GetMultilingualString("MasterFiles|WorkflowFilterStipsHelper|NextMilesone", "Next Milestone");
			public static readonly string LastCompletedMilestoneName = (NoResString)"Last Completed Milestone";
			public static readonly ResourceString LastCompletedMilestoneDesc = ResString.GetMultilingualString("MasterFiles|WorkflowFilterStipsHelper|LastCompletedMilestone", "Last Completed Milestone");
			public static readonly string AnyOpenTaskAssignedToName = (NoResString)"Any Open Task Assigned To";
			public static readonly ResourceString AnyOpenTaskAssignedToDesc = ResString.GetMultilingualString("MasterFiles|WorkflowFilterStipsHelper|AnyOpenTaskAssignedTo", "Any Open Task Assigned To");
			public static readonly string NextTaskAssignedToName = (NoResString)"Next Task Assigned To";
			public static readonly ResourceString NextTaskAssignedToDesc = ResString.GetMultilingualString("MasterFiles|WorkflowFilterStipsHelper|NextTaskAssignedTo", "Next Task Assigned To");
		}
	}
}
