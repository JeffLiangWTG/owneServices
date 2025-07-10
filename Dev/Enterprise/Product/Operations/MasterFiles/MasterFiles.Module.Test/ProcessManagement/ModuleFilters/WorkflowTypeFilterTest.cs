using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ComparisonConstants = Enterprise.ZArchitecture.Business.ModuleFilterWithListAndComparisonOperators<CargoWise.Types.IZType>.ComparisonConstants;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(WorkflowTypeFilter))]
	sealed class WorkflowTypeFilterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestStandaloneTaskWorkflowType_WithInvalidComparisonOperator_ShouldNotThrowException()
		{
			var bizo = new ProcessTaskFilterBusinessObject();
			var filter = bizo.AddTextFilterStrip(WorkflowTypeFilter.FilterDescription, WorkflowDescriptors.StandAloneTaskWorkflowDescriptor);

			filter.ComparisonOperator = "ShabooBapBee";
			AssertEquals("Invalid comparison operator selection should not be possible.", filter.ComparisonOperator, filter.GetComparisonOperatorDefault());
			Assert("Invalid comparison operator selection should cause no-result-query.", !bizo.Filter.IsNoResultQuery);

			filter.ComparisonOperator = ComparisonConstants.Exact;
			AssertContains("P9_ParentID is NULL", bizo.Filter.LiteralTextSqlFormatted);

			filter.ComparisonOperator = ComparisonConstants.NotEqual;
			AssertContains("P9_ParentID is not NULL", bizo.Filter.LiteralTextSqlFormatted);
		}

		public void TestFilterModuleSelection_ShouldNotIncludeTaskLineTriggers()
		{
			var bizo = new ProcessTaskFilterBusinessObject();
			var filter = bizo.AddTextFilterStrip(WorkflowTypeFilter.FilterDescription, WorkflowDescriptors.StandAloneTaskWorkflowDescriptor);
			var list = (ICodeDescriptionPairList)filter.List;

			AssertEquals(true, list.ContainsCode(WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode));
			AssertEquals(false, list.ContainsCode(ProcessTasksLookups.TaskLineTriggerCode));
		}

		public static void EnsureAllWorkflowTypesProduceDistinctQueries(FilterStripBusinessObject filterBizo)
		{
			var workflowTypeFilter = (ModuleTextFilter)filterBizo[WorkflowTypeFilter.FilterDescription];
			var filterQueries = new Dictionary<string, List<WorkflowDescriptor>>();

			foreach (var descriptor in WorkflowDescriptors.Instance.Values
				.Where(d => d.Code != ProcessTasksLookups.TaskLineTriggerCode && d.Code != ProcessTasksLookups.ExceptionLineTriggerCode)
				.OrderBy(d => d.GetType().FullName))
			{
				workflowTypeFilter.Property = descriptor.Code;

				var query = workflowTypeFilter.Query.LiteralTextADO;
				filterQueries.GetOrAdd(query, () => new List<WorkflowDescriptor>()).Add(descriptor);
			}

			var failureMessage = new StringBuilder();

			foreach (var kvp in filterQueries.Where(kvp => kvp.Value.Count > 1))
			{
				failureMessage.Append("<br />This query:<br />");
				failureMessage.Append(kvp.Key);
				failureMessage.Append("<br />Is shared by:<ul>");

				foreach (var descriptor in kvp.Value)
				{
					failureMessage.Append("<li>");
					failureMessage.Append(descriptor.GetType());
					failureMessage.Append(", workflow type: ");
					failureMessage.Append(descriptor.Code);
					failureMessage.Append("</li>");
				}

				failureMessage.Append("</ul>");
			}

			if (failureMessage.Length > 0)
			{
				var message = string.Format(CultureInfo.InvariantCulture, "There are workflow types which cannot produce SQL queries that differentiate rows from those of other workflow types. To resolve this, implement the {0} interface for the ProcessTask types relevant to all affected descriptors shown below, adding predicates to the query in {1} This should be registered in EnterpriseApplicationConfiguration in the ProcessTaskTypes collection.<br />",
					nameof(IProcessTaskLoadStrategy), nameof(IProcessTaskLoadStrategy.AddAdditionalParentFilters));

				HtmlFail(message + failureMessage.ToString());
			}
			else
			{
				Assert(true);
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new WorkflowTypeFilter(Factory, typeof(ProcessTask), ProcessTasksSchema.P9_ParentID);
		}
	}
}
