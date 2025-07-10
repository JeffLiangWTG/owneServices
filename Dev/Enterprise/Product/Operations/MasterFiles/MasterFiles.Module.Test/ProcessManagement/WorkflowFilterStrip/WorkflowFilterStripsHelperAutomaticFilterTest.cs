using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.MasterFiles.Module.Testing
{
	sealed class WorkflowFilterStripsHelperAutomaticFilterTest : AutomaticFilterTest
	{
		public void AssertMilestoneDate(FilterStripBusinessObject filterBusinessObject, Type businessObjectType, Func<BusinessObjectFactory, Type, BusinessObject> getNewBusinessObject)
		{
			var filter = (WorkflowModuleFilter)filterBusinessObject["Milestone Date"];

			if (filter != null)
			{
				var bizo1 = (IWorkflowProvider)getNewBusinessObject(Factory, businessObjectType);
				var milestone1 = bizo1.WorkflowItems.Milestones.AddNew();
				milestone1.P9_Type = Core.Constants.Workflow.MilestoneType;
				milestone1.SetMilestoneActualDateForTest(new ZDateTime(2000, 1, 2));

				var bizo2 = (IWorkflowProvider)getNewBusinessObject(Factory, businessObjectType);
				var milestone2 = bizo2.WorkflowItems.Milestones.AddNew();
				milestone2.P9_Type = Core.Constants.Workflow.MilestoneType;
				milestone2.SetMilestoneActualDateForTest(new ZDateTime(2000, 1, 4));

				Factory.Save();

				filter.PropertySearch = ModuleDateFilter.SpecifiedDateRange;
				filter.Property1 = new ZDateTime(2000, 1, 1);
				filter.Property2 = new ZDateTime(2000, 1, 3);
				filter.IsActive = true;

				var query = filterBusinessObject.Filter;
				var result = Factory.Load(businessObjectType, query);

				AssertCollectionContains("Milestone Date filter should have returned the object with the milestone within the correct date range, and yet...", bizo1, result);
				AssertCollectionNotContains("Milestone Date filter should not have returned the object with the milestone outside the correct date range, and yet...", bizo2, result);
			}
		}

		public void AssertTaskStatusFilter(FilterStripBusinessObject filterBusinessObject, Type businessObjectType, Func<BusinessObjectFactory, Type, BusinessObject> getNewBusinessObject)
		{
			var filter = (ModuleTextFilter)filterBusinessObject["Task Status"];

			if (filter != null)
			{
				var openParent = (IWorkflowProvider)getNewBusinessObject(Factory, businessObjectType);
				openParent.WorkflowItems.AddNew().P9_Status = ProcessTaskStatusCodeList.Codes.Open;

				var workingParent = (IWorkflowProvider)getNewBusinessObject(Factory, businessObjectType);
				workingParent.WorkflowItems.AddNew().P9_Status = ProcessTaskStatusCodeList.Codes.Working;

				var closedParent = (IWorkflowProvider)getNewBusinessObject(Factory, businessObjectType);
				closedParent.WorkflowItems.AddNew().P9_Status = ProcessTaskStatusCodeList.Codes.Closed;

				Factory.Save();

				filter.Property = ProcessTaskStatusCodeList.Codes.Open;
				filter.IsActive = true;
				var query = filterBusinessObject.Filter;
				var result = Factory.Load(businessObjectType, query);
				AssertEquals(1, result.Length);
				AssertCollectionContains(openParent, result);

				filter.Property = ProcessTaskStatusCodeList.Codes.Closed;
				query = filterBusinessObject.Filter;
				result = Factory.Load(businessObjectType, query);
				AssertEquals(1, result.Length);
				AssertCollectionContains(closedParent, result);

				filter.Property = "NCM";
				query = filterBusinessObject.Filter;
				result = Factory.Load(businessObjectType, query);
				AssertEquals(2, result.Length);
				AssertCollectionContains(openParent, result);
				AssertCollectionContains(workingParent, result);

				filter.Property = ZString.Empty;
				query = filterBusinessObject.Filter;
				result = Factory.Load(businessObjectType, query);

				AssertEquals(3, result.Length);
			}
		}

		public void AssertTasksFilter(FilterStripBusinessObject filterBusinessObject, Type businessObjectType, Func<BusinessObjectFactory, Type, BusinessObject> getNewBusinessObject)
		{
			var filter = (TasksModuleFilter)filterBusinessObject["Tasks"];

			if (filter != null)
			{
				var jobWithOpenTasks = (IWorkflowProvider)getNewBusinessObject(Factory, businessObjectType);
				var taskTemplate = jobWithOpenTasks.WorkflowItems.Tasks.AddNew();

				var openTask = (ProcessTask)Factory.NewWithValidTestData(taskTemplate.GetType());
				openTask.P9_Status = "ASN";
				openTask.P9_ParentTableCode = "Z0";
				openTask.P9_Type = taskTemplate.P9_Type;
				openTask.P9_Description = "AssertTasksFilter_Open task";
				openTask.P9_ParentID = jobWithOpenTasks.PK;
				openTask.P9_ParentTableCode = BusinessObjectFactory.GetTableCodeFromType(jobWithOpenTasks.GetType());

				var jobWithClosedTasks = (IWorkflowProvider)getNewBusinessObject(Factory, businessObjectType);
				var closedTask = (ProcessTask)Factory.NewWithValidTestData(taskTemplate.GetType());
				closedTask.P9_Status = "CLS";
				closedTask.P9_ParentTableCode = "Z0";
				closedTask.P9_Type = taskTemplate.P9_Type;
				closedTask.P9_Description = "AssertTasksFilter_Closed task";
				closedTask.P9_ParentID = jobWithClosedTasks.PK;
				closedTask.P9_ParentTableCode = openTask.P9_ParentTableCode;

				taskTemplate.Delete();
				Factory.Save();

				AssertNotEquals("P0", closedTask.P9_ParentTableCode);

				filter.SelectedFilters.AddTextFilterStrip("Status", "CLS");
				filter.SelectedFilters.AddTextFilterStrip("Description", "AssertTasksFilter");

				var subFilterResult = Factory.Load<ProcessTask>(filter.SelectedFilters.Filter);
				AssertEquals("Sub filter: " + filter.SelectedFilters.Filter.LiteralTextSqlFormatted, 1, subFilterResult.Length);
				AssertEquals("Sub filter: " + filter.SelectedFilters.Filter.LiteralTextSqlFormatted, closedTask.P9_Description, subFilterResult.Single().P9_Description);

				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;

				var pkQuery = new ZQuery();
				var parentTableSchema = BusinessObjectFactory.GetTableSchemaFromType(businessObjectType);
				pkQuery.AddToFilter(JoinCondition.And, parentTableSchema.PK, SQLComparisonOperator.Equal, jobWithOpenTasks.PK);
				pkQuery.AddToFilter(JoinCondition.Or, parentTableSchema.PK, SQLComparisonOperator.Equal, jobWithClosedTasks.PK);
				var query = new ZQuery();
				query.AddToFilter(filter.Query);
				query.AddToFilter(pkQuery);

				var result = Factory.Load(businessObjectType, query);
				AssertEquals("Any match: " + query.LiteralTextSqlFormatted, 1, result.Length);
				AssertEquals("Any match: " + query.LiteralTextSqlFormatted, jobWithClosedTasks.PK, result.Single().PK);

				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NoneMatch;
				query = new ZQuery();
				query.AddToFilter(filter.Query);
				query.AddToFilter(pkQuery);

				result = Factory.Load(businessObjectType, query);
				AssertEquals("None match: " + query.LiteralTextSqlFormatted, 1, result.Length);
				AssertEquals("None match: " + query.LiteralTextSqlFormatted, jobWithOpenTasks.PK, result.Single().PK);

				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch;
				query = new ZQuery();
				query.AddToFilter(filter.Query);
				query.AddToFilter(pkQuery);

				result = Factory.Load(businessObjectType, query);
				AssertEquals("All match: " + query.LiteralTextSqlFormatted, 1, result.Length);
				AssertEquals("All match: " + query.LiteralTextSqlFormatted, jobWithClosedTasks.PK, result.Single().PK);
			}
		}

		public void AssertExceptionsFilter(FilterStripBusinessObject filterBusinessObject, Type businessObjectType, Func<BusinessObjectFactory, Type, BusinessObject> getNewBusinessObject)
		{
			var filter = (ExceptionsModuleFilter)filterBusinessObject["Exceptions"];

			if (filter != null)
			{
				var jobWithOpenExceptions = (IWorkflowProvider)getNewBusinessObject(Factory, businessObjectType);
				var openException = jobWithOpenExceptions.WorkflowItems.Exceptions.AddNew();
				openException.P9_Description = "AssertExceptionsFilter_Open Exception";

				var jobWithClosedExceptions = (IWorkflowProvider)getNewBusinessObject(Factory, businessObjectType);
				var closedException = jobWithClosedExceptions.WorkflowItems.Exceptions.AddNew();
				closedException.P9_GS_NKAssignedStaffMember = GlbStaff.CurrentUser.GS_Code;
				closedException.IsExceptionActioned = true;
				closedException.P9_Description = "AssertExceptionsFilter_Closed Exception";

				Factory.Save();

				((ModuleTextFilter)filter.SelectedFilters.ActiveModuleFilters[0]).Property = ExceptionStatusCodeList.Codes.Actioned;
				filter.SelectedFilters.AddTextFilterStrip("Description", "AssertExceptionsFilter");

				var subFilterResult = Factory.Load<ProcessTask>(filter.SelectedFilters.Filter);
				AssertEquals("Sub filter: " + filter.SelectedFilters.Filter.LiteralTextSqlFormatted, 1, subFilterResult.Length);
				AssertEquals("Sub filter: " + filter.SelectedFilters.Filter.LiteralTextSqlFormatted, closedException.P9_Description, subFilterResult.Single().P9_Description);

				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;

				var pkQuery = new ZQuery();
				var parentTableSchema = BusinessObjectFactory.GetTableSchemaFromType(businessObjectType);
				pkQuery.AddToFilter(JoinCondition.And, parentTableSchema.PK, SQLComparisonOperator.Equal, jobWithOpenExceptions.PK);
				pkQuery.AddToFilter(JoinCondition.Or, parentTableSchema.PK, SQLComparisonOperator.Equal, jobWithClosedExceptions.PK);
				var query = new ZQuery();
				query.AddToFilter(filter.Query);
				query.AddToFilter(pkQuery);

				var result = Factory.Load(businessObjectType, query);
				AssertEquals("Any match: " + query.LiteralTextSqlFormatted, 1, result.Length);
				AssertEquals("Any match: " + query.LiteralTextSqlFormatted, jobWithClosedExceptions.PK, result.Single().PK);

				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NoneMatch;
				query = new ZQuery();
				query.AddToFilter(filter.Query);
				query.AddToFilter(pkQuery);

				result = Factory.Load(businessObjectType, query);
				AssertEquals("None match: " + query.LiteralTextSqlFormatted, 1, result.Length);
				AssertEquals("None match: " + query.LiteralTextSqlFormatted, jobWithOpenExceptions.PK, result.Single().PK);

				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch;
				query = new ZQuery();
				query.AddToFilter(filter.Query);
				query.AddToFilter(pkQuery);

				result = Factory.Load(businessObjectType, query);
				AssertEquals("All match: " + query.LiteralTextSqlFormatted, 1, result.Length);
				AssertEquals("All match: " + query.LiteralTextSqlFormatted, jobWithClosedExceptions.PK, result.Single().PK);
			}
		}

		public void AssertMilestonesFilter(FilterStripBusinessObject filterBusinessObject, Type businessObjectType, Func<BusinessObjectFactory, Type, BusinessObject> getNewBusinessObject)
		{
			var filter = (MilestonesModuleFilter)filterBusinessObject["Milestones"];

			if (filter != null)
			{
				var jobWithAuthorisedMilestone = (IWorkflowProvider)getNewBusinessObject(Factory, businessObjectType);
				var authorisedMilestone = jobWithAuthorisedMilestone.WorkflowItems.Milestones.AddNew();
				authorisedMilestone.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;
				authorisedMilestone.P9_Description = "AssertMilestonesFilter Authorised-Milestone";

				var jobWithArrivalMilestone = (IWorkflowProvider)getNewBusinessObject(Factory, businessObjectType);
				var arrivalMilestone = jobWithArrivalMilestone.WorkflowItems.Milestones.AddNew();
				arrivalMilestone.TriggerConditions.TriggerEventCode = Events.ArrivalCode;
				arrivalMilestone.P9_Description = "AssertMilestonesFilter Arrival-Milestone";

				Factory.Save();

				filter.SelectedFilters.AddTextFilterStrip("Event Code", Events.ArrivalCode);
				filter.SelectedFilters.AddTextFilterStrip("Description", "AssertMilestonesFilter");

				var subFilterResult = Factory.Load<ProcessTask>(filter.SelectedFilters.Filter);

				CombineAssertions("WHEN load with filter description='AssertMilestonesFilter' and Arrival, SHOULD find 1 match i.e. 'Arrival-Milestone'", () =>
				{
					AssertEquals("Length", 1, subFilterResult.Length);
					AssertEquals("Description", arrivalMilestone.P9_Description, subFilterResult.Single().P9_Description);
				});

				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;

				var pkQuery = new ZQuery();
				var parentTableSchema = BusinessObjectFactory.GetTableSchemaFromType(businessObjectType);
				pkQuery.AddToFilter(JoinCondition.And, parentTableSchema.PK, SQLComparisonOperator.Equal, jobWithAuthorisedMilestone.PK);
				pkQuery.AddToFilter(JoinCondition.Or, parentTableSchema.PK, SQLComparisonOperator.Equal, jobWithArrivalMilestone.PK);
				var query = new ZQuery();
				query.AddToFilter(filter.Query);
				query.AddToFilter(pkQuery);

				var result = Factory.Load(businessObjectType, query);
				AssertEquals("Any match count: " + query.LiteralTextSqlFormatted, 1, result.Length);
				AssertEquals("Any match PK: " + query.LiteralTextSqlFormatted, jobWithArrivalMilestone.PK, result.Single().PK);

				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NoneMatch;
				query = new ZQuery();
				query.AddToFilter(filter.Query);
				query.AddToFilter(pkQuery);

				result = Factory.Load(businessObjectType, query);
				AssertEquals("None match count: " + query.LiteralTextSqlFormatted, 1, result.Length);
				AssertEquals("None match PK: " + query.LiteralTextSqlFormatted, jobWithAuthorisedMilestone.PK, result.Single().PK);

				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch;
				query = new ZQuery();
				query.AddToFilter(filter.Query);
				query.AddToFilter(pkQuery);

				result = Factory.Load(businessObjectType, query);
				AssertEquals("All match count: " + query.LiteralTextSqlFormatted, 1, result.Length);
				AssertEquals("All match PK: " + query.LiteralTextSqlFormatted, jobWithArrivalMilestone.PK, result.Single().PK);
			}
		}

		public void AssertTriggersFilter(FilterStripBusinessObject filterBusinessObject, Type businessObjectType, Func<BusinessObjectFactory, Type, BusinessObject> getNewBusinessObject)
		{
			var filter = (TriggersModuleFilter)filterBusinessObject["Triggers"];

			if (filter != null)
			{
				var jobWithAuthorisedTrigger = (IWorkflowProvider)getNewBusinessObject(Factory, businessObjectType);
				var authorisedTrigger = jobWithAuthorisedTrigger.WorkflowItems.Triggers.AddNew();
				authorisedTrigger.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;
				authorisedTrigger.P9_Description = "AssertTriggersFilter Authorised-Trigger";

				var jobWithArrivalTrigger = (IWorkflowProvider)getNewBusinessObject(Factory, businessObjectType);
				var arrivalTrigger = jobWithArrivalTrigger.WorkflowItems.Triggers.AddNew();
				arrivalTrigger.TriggerConditions.TriggerEventCode = Events.ArrivalCode;
				arrivalTrigger.P9_Description = "AssertTriggersFilter Arrival-Trigger";

				Factory.Save();

				filter.SelectedFilters.AddTextFilterStrip("Event Code", Events.ArrivalCode);
				filter.SelectedFilters.AddTextFilterStrip("Description", "AssertTriggersFilter");

				var subFilterResult = Factory.Load<ProcessTask>(filter.SelectedFilters.Filter);

				CombineAssertions("WHEN load with filter description='AssertTriggersFilter' and Arrival, SHOULD find 1 match i.e. 'Arrival-Trigger'", () =>
				{
					AssertEquals("Length", 1, subFilterResult.Length);
					AssertEquals("Description", arrivalTrigger.P9_Description, subFilterResult.Single().P9_Description);
				});

				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;

				var pkQuery = new ZQuery();
				var parentTableSchema = BusinessObjectFactory.GetTableSchemaFromType(businessObjectType);
				pkQuery.AddToFilter(JoinCondition.And, parentTableSchema.PK, SQLComparisonOperator.Equal, jobWithAuthorisedTrigger.PK);
				pkQuery.AddToFilter(JoinCondition.Or, parentTableSchema.PK, SQLComparisonOperator.Equal, jobWithArrivalTrigger.PK);
				var query = new ZQuery();
				query.AddToFilter(filter.Query);
				query.AddToFilter(pkQuery);

				var result = Factory.Load(businessObjectType, query);
				AssertEquals("Any match count: " + query.LiteralTextSqlFormatted, 1, result.Length);
				AssertEquals("Any match PK: " + query.LiteralTextSqlFormatted, jobWithArrivalTrigger.PK, result.Single().PK);

				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.NoneMatch;
				query = new ZQuery();
				query.AddToFilter(filter.Query);
				query.AddToFilter(pkQuery);

				result = Factory.Load(businessObjectType, query);
				AssertEquals("None match count: " + query.LiteralTextSqlFormatted, 1, result.Length);
				AssertEquals("None match PK: " + query.LiteralTextSqlFormatted, jobWithAuthorisedTrigger.PK, result.Single().PK);

				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AllMatch;
				query = new ZQuery();
				query.AddToFilter(filter.Query);
				query.AddToFilter(pkQuery);

				result = Factory.Load(businessObjectType, query);
				AssertEquals("All match count: " + query.LiteralTextSqlFormatted, 1, result.Length);
				AssertEquals("All match PK: " + query.LiteralTextSqlFormatted, jobWithArrivalTrigger.PK, result.Single().PK);
			}
		}

		public void AssertCustomFields(FilterStripBusinessObject filterBusinessObject, Type businessObjectType, Func<BusinessObjectFactory, Type, BusinessObject> getNewBusinessObject)
		{
			if (filterBusinessObject.ParentModule.SupportsWorkflow)
			{
				var parentTableCode = BusinessObjectFactory.GetTableCodeFromType(businessObjectType);

				var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
				template.P0_ProcessType = parentTableCode;
				template.P0_IsActive = true;

				var def = Factory.New<GenCustomColumnDefinition>();
				def.XC_ParentTableCode = parentTableCode;
				def.XC_Name = "Pinot Noir";
				def.XC_Type = "STR";
				def.XC_ParentID = template.PK;

				Factory.Save();

				var helper = new WorkflowFilterStripsHelper(businessObjectType, parentTableCode, Factory);
				helper.ClearCache();

				var filters = new ModuleFilterCollection();
				helper.AddFilterStrips(filters);
				AssertNotNull("WorkflowFilterStripsHelper was created for a bizO that supports workflow, so should not be null. This test will fail if a module that supports workflow chooses to opt out of custom filters.", filters["Pinot Noir"]);
			}
		}

		public override void SetUpForHelperFiltersWorkTests(Type businessObjectType)
		{
		}
	}
}
