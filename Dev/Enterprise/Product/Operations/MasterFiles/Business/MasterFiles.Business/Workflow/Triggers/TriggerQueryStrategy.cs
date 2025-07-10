using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	class ProcessTaskTriggerQueryStrategy : TriggerQueryStrategy
	{
		internal ProcessTaskTriggerQueryStrategy()
			: this(ZString.Empty)
		{
		}

		internal ProcessTaskTriggerQueryStrategy(ZString lineTriggerType)
		{
			this.lineTriggerType = lineTriggerType;
		}

		readonly ZString lineTriggerType;

		protected override bool TryCreateQueryForTriggersDefinedDirectly(IStmALogParent logParent, ZString eventType, out ZQuery query)
		{
			if (ShouldRunQuery(logParent))
			{
				query = new ZQuery();
				query.AddToFilter(ProcessTasksSchema.P9_ParentID, logParent.LogsParentPK);

				if (!eventType.IsEmpty)
				{
					query.AddToFilter(ProcessTasksSchema.P9_SE_NKMilestoneEvent, eventType);
				}

				query.AddToFilter(ProcessTasksSchema.P9_Type, new[] { Constants.Workflow.MilestoneType, Constants.Workflow.WorkflowTriggerType });
				query.AddToFilter(ProcessTasksSchema.P9_LineTriggerType, lineTriggerType);

				return true;
			}
			else
			{
				query = null;
				return false;
			}
		}

		static bool ShouldRunQuery(IStmALogParent logParent)
		{
			if (logParent is IWorkflowProviderCore workflowProvider
				&& WorkflowDescriptors.Instance.TryGetValue(workflowProvider.WorkflowType, out WorkflowDescriptor descriptor)
				&& descriptor.SupportsEventTracking)
			{
				return true;
			}
			else
			{
				return Attribute.IsDefined(logParent.GetType(), typeof(CanHaveTriggersDespiteNotImplementingIWorkflowProviderAttribute)); // Only here because the fake news GovernmentInvoice hasn't been setup properly. Very Unfair!
			}
		}
	}

	class ProcessTemplateTriggerQueryStrategy : TriggerQueryStrategy
	{
		internal Dictionary<ZGuid, IWorkflowProvider> TemplatePKToMatchingWorkflowProviderMap { get; } = new Dictionary<ZGuid, IWorkflowProvider>();

		protected override bool TryCreateQueryForTriggersDefinedDirectly(IStmALogParent logParent, ZString eventType, out ZQuery query)
		{
			if (ObjectFactory.Get<IWorkflowRegistry>().AreUniversalTemplatesEnabled && logParent is IWorkflowProvider workflowProvider
				&& (!(logParent is ISometimesWorkflowProvider sometimesWorkflowProvider) || sometimesWorkflowProvider.ShouldSupportWorkflowTemplateApplication)
				&& WorkflowDescriptors.Instance.TryGetValue(workflowProvider.WorkflowType, out WorkflowDescriptor d) && d.SupportsUniversalTemplates)
			{
				var loader = new ProcessTaskTemplate.Loader(logParent.Factory);
				var allTemplates = loader.FindMatches(workflowProvider, includeOnlyUniversalTemplates: true);

				var iterator = new TemplateFallbackIterator(allTemplates, TemplateEntityType.Triggers, UniversalTriggerCreator.DisallowUniversalTriggersFallbackIfEmpty);
				var applicableTemplates = iterator.Where(x => x.IsApplicableToJob).Select(x => x.Template).ToArray();

				if (applicableTemplates.Length > 0)
				{
					foreach (var template in applicableTemplates)
					{
						TemplatePKToMatchingWorkflowProviderMap[template.PK] = workflowProvider;
					}

					query = new ZQuery(ProcessTemplateTriggerSchema.P9T_IsActive, true);
					query.AddToFilter(ProcessTemplateTriggerSchema.P9T_P0_Template, applicableTemplates.Select(t => t.PK).ToArray());

					if (!eventType.IsEmpty)
					{
						query.AddToFilter(ProcessTemplateTriggerSchema.P9T_SE_NKTriggerEvent, eventType);
					}

					return true;
				}
			}

			query = null;
			return false;
		}

		protected override bool FetchOnlyFromLocalCache(BusinessObject bizO, bool mustFetchFromDb) => false;
	}

	abstract class TriggerQueryStrategy
	{
		internal bool TryGetQueryForAllTriggersIncludingThoseOnParentObjects(IStmALogParent logParent, ZString eventType, out ZQuery query)
		{
			query = new ZQuery();
			var result = TryCreateQueryForTriggersDefinedDirectly(logParent, eventType, out ZQuery triggerQuery);
			if (result)
			{
				query.AddToFilter(triggerQuery);
			}

			var childFiringParentWorkflow = logParent as IWorkflowTriggerEventSource;
			var queryMustFetchFromDb = false;
			result |= TryAddQueryForParentWorkflowProviders(eventType, query, childFiringParentWorkflow, logParent.Factory, ref queryMustFetchFromDb);

			var bizOParent = logParent as BusinessObject;
			query.FetchOnlyFromLocalCache = FetchOnlyFromLocalCache(bizOParent, queryMustFetchFromDb);

			return result;
		}

		protected virtual bool FetchOnlyFromLocalCache(BusinessObject bizO, bool mustFetchFromDb) => bizO != null && !bizO.IsInDatabase && !mustFetchFromDb;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1075:DoNotUseLoopToAddOrConditionsToFilter", Justification = "Baseline")]
		bool TryAddQueryForParentWorkflowProviders(ZString logEvent, ZQuery mainQuery, IWorkflowTriggerEventSource childFiringParentWorkflow, BusinessObjectFactory factory, ref bool parentInDb)
		{
			var result = false;
			if (childFiringParentWorkflow != null && childFiringParentWorkflow.ParentWorkflowProviders != null)
			{
				foreach (var parentWorkFlowProvider in childFiringParentWorkflow.ParentWorkflowProviders)
				{
					var logsParent = parentWorkFlowProvider as IStmALogParent;
					if (parentWorkFlowProvider != null && logsParent != null && !logsParent.IsDeleted)
					{
						var parentWorkFlowProviderPK = parentWorkFlowProvider.PK;

						if (parentWorkFlowProviderPK.IsValid)
						{
							if (TryCreateQueryForTriggersDefinedDirectly(logsParent, logEvent, out ZQuery query))
							{
								mainQuery.AddToFilter(query, JoinCondition.Or);
								result = true;
							}

							if (factory != null)
							{
								factory.AddFetchHint(ProcessTasksSchema.P9_ParentID, parentWorkFlowProviderPK);
							}

							result |= TryAddQueryForParentWorkflowProviders(logEvent, mainQuery, parentWorkFlowProvider as IWorkflowTriggerEventSource, factory, ref parentInDb);
							parentInDb |= (logsParent as BusinessObject).IsInDatabase && logsParent.Identifier == logsParent.LogsParentPK;
						}
					}
				}
			}

			return result;
		}

		protected abstract bool TryCreateQueryForTriggersDefinedDirectly(IStmALogParent logParent, ZString eventType, out ZQuery query);
	}
}
