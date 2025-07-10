using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business
{
	public class ApplyWorkflowTemplateResult
	{
		ApplyWorkflowTemplateResult(List<TemplateItemApplicationMap> tasks, IList<StmALog> logs)
			: this(hasTemplateApplied: tasks.Any(), TemplateApplicationResult.Success)
		{
			items = tasks.AsReadOnly();
			this.logs = logs;
		}

		ApplyWorkflowTemplateResult(bool hasTemplateApplied, TemplateApplicationResult applicationResult)
		{
			HasTemplateApplied = hasTemplateApplied;
			ApplicationResult = applicationResult;
		}

		readonly IList<TemplateItemApplicationMap> items;
		readonly IList<StmALog> logs;
		public bool HasTemplateApplied { get; }
		public TemplateApplicationResult ApplicationResult { get; }
		public IEnumerable<TemplateItemApplicationMap> CreatedItems => items ?? Enumerable.Empty<TemplateItemApplicationMap>();
		public IEnumerable<StmALog> CreatedLogs => logs ?? Enumerable.Empty<StmALog>();

		public class Builder
		{
			public Builder(IWorkflowProvider logProvider)
			{
				this.workflowProvider = Argument.NotNull(logProvider, nameof(logProvider));
			}

			readonly IWorkflowProvider workflowProvider;
			readonly List<CreateItemsFromTemplateResult> results = new List<CreateItemsFromTemplateResult>();

			public Builder Add(CreateItemsFromTemplateResult result)
			{
				results.Add(result);
				return this;
			}

			public Builder Add(IEnumerable<CreateItemsFromTemplateResult> result)
			{
				results.AddRange(result);
				return this;
			}

			public int ResultsCount => results.Count;

			public ApplyWorkflowTemplateResult Build()
			{
				var applications = results.SelectMany(a => a.CreatedItems)
					.Where(t => t.IsNeedingToBeLogged)
					.GroupBy(t => t.Template)
					.Select(g => new TemplateItemApplicationMap(g.Key, g))
					.ToList();

				foreach (var createdTask in results.SelectMany(s => s.CreatedItems))
				{
					if (createdTask.ShouldCloneTask && createdTask.CreatedItem.IsMilestoneOrWorkflowTrigger)
					{
						WorkflowDefaultDateProvider.DefaultDatesFromMilestoneEvent(createdTask.CreatedItem, createdTask.CreatedItem.GetJob(), false, false);

						if (WorkflowDataRegistry.Instance.PreventMilestoneFutureActualStart.Value && !workflowProvider.WorkflowItems.Parent.IsBound)
						{
							createdTask.CreatedItem.RaiseMilestoneExceptionForFutureActualDate();
						}
					}
				}

				if (applications.Any())
				{
					WorkflowAfterOnSavingBOService.TryHookupRaceConditionHandlerService(workflowProvider.WorkflowItems.Factory, TemplateApplicationRaceHandlingConfig.GetConfig());
					return new ApplyWorkflowTemplateResult(applications, new WorkflowTemplateApplicationLogBuilder(workflowProvider, applications).Build());
				}
				else
				{
					return Empty(TemplateApplicationResult.NoNewMatchingTemplateItems);
				}
			}
		}

		public static ApplyWorkflowTemplateResult Empty(TemplateApplicationResult applicationResult) => new ApplyWorkflowTemplateResult(false, applicationResult);
	}

	public enum TemplateApplicationResult
	{
		Success,
		NoMatchingTemplate,
		NoNewMatchingTemplateItems,
		InvalidWorkflowProvider,
		InvalidLogin,
		TemplateApplicationSuspended,
		UnsupportedISometimesWorkflowProvider,
		JobDeleted,
		JobCancelled,
		TemplateScopeEnforcerRestriction,
		NoChangeMadeToBusinessObject
	}
}
