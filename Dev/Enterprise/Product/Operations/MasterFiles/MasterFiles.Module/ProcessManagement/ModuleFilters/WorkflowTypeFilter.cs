using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Module
{
	class WorkflowTypeFilter : ModuleTextFilter
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Module filter 'name'")]
		public const string FilterDescription = "Workflow Type";

		public WorkflowTypeFilter(BusinessObjectFactory factory, Type typeOfObjectBeingFiltered, SchemaGuidColumn foreignKeyToParent)
			: base(FilterDescription, (comparisonOperator, workflowType) => GetWorkflowTypeQuery(comparisonOperator, workflowType, typeOfObjectBeingFiltered, foreignKeyToParent), ListDelegate(factory))
		{
			MultilingualDescription = ResString.GetMultilingualString("MasterFiles|ProcessTasksFilter|WorkflowType", "Workflow Type");
		}

		static ZQuery GetWorkflowTypeQuery(SQLComparisonOperator comparisonOperator, ZString workflowType, Type typeOfObjectBeingFiltered, SchemaGuidColumn foreignKeyToParent)
		{
			if (comparisonOperator.In(SQLComparisonOperator.Equal, SQLComparisonOperator.NotEqual))
			{
				if (workflowType == WorkflowDescriptors.StandAloneTaskWorkflowDescriptor)
				{
					return new ZQuery(foreignKeyToParent, comparisonOperator, null);
				}

				if (WorkflowDescriptors.Instance.TryGetValue(workflowType, out var descriptor))
				{
					var result = new ZDBOnlyQuery(typeOfObjectBeingFiltered);
					var isNotInQuery = comparisonOperator == SQLComparisonOperator.NotEqual;
					var jobSubQuery = ProcessTaskTypeDecider.GetInstance().GetParentJobSubQuery(descriptor, foreignKeyToParent, isNotInQuery);

					result.AddSubQuery(jobSubQuery, JoinCondition.And);

					return result;
				}
			}

			return ZQuery.NoResultQuery;
		}

		static GetList ListDelegate(BusinessObjectFactory factory) => new GetList(() => GetWorkflowTypeList(factory));

		public static CodeDescriptionPairList GetWorkflowTypeList(BusinessObjectFactory factory) => factory.GetCachedValue("IWorkflowDescriptorListWithStandaloneTaskType", () => (CodeDescriptionPairList)ObjectFactory.Get<IWorkflowDescriptorListWithStandaloneTaskType>());

		public override IReadOnlyList<string> AllowedComparisonOperators => new[] { ComparisonConstants.Exact, ComparisonConstants.NotEqual };
	}
}
