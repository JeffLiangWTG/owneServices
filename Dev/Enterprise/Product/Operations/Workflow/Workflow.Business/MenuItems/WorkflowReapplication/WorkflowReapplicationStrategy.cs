using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Workflow.Business
{
	public static class WorkflowReapplicationStrategy
	{
		public static void ReapplyWorkflowTemplates(IReapplyWorkflowTemplateConfiguration configuration, ReapplyWorkflowTemplateUserOptions options, IWorkflowProvider[] workflowProviders, IProgressReporter progressReporter)
		{
			if (Env.Security.WorkflowTaskTemplatesReapply.IsAllowed && workflowProviders.Any())
			{
				if (!options.HasAtLeastOnOptionSelected())
				{
					ErrorReporter.ReportOnce("One shan't call the workflow reapplication strategy unless one desires reapplication.");
				}

				using (configuration.DelayReapplyTemplatesToServiceTask ? ProcessTask.Loader.SuppressTemplateApplication() : null)
				{
					ReapplyWorkflowTemplatesCore(configuration, options, workflowProviders, progressReporter);
				}
			}
		}

		static void ReapplyWorkflowTemplatesCore(IReapplyWorkflowTemplateConfiguration configuration, ReapplyWorkflowTemplateUserOptions options, IWorkflowProvider[] workflowProviders, IProgressReporter progressReporter)
		{
			if (workflowProviders.Length == 0)
			{
				throw new InvalidOperationException("No workflow providers provided.");
			}

			for (int i = 0; i < workflowProviders.Length; i++)
			{
				var provider = workflowProviders[i];
				var factory = configuration.ProcessAndSaveInNewFactory
					? new BusinessObjectFactory { NameForDebugging = nameof(ReapplyWorkflowTemplatesCore) }
					: provider.WorkflowItems.Factory;

				using (WorkflowAfterOnSavingBOService.GetWorkflowItemsChangeLogService(factory).SuppressWorkflowChangeLog())
				{
					var reloadedBizo = factory.Load(provider.GetType(), provider.PK);
					var reloadedProvider = reloadedBizo as IWorkflowProvider;

					if (CanReapplyTemplates(reloadedProvider, options))
					{
						DeleteRequiredWorkflowItems(options, reloadedProvider, factory, workflowProviders.Length);

						ReapplyTemplates(configuration, options, reloadedProvider, factory);

						SaveIfRequired(configuration, factory, reloadedProvider, workflowProviders.Length);

						progressReporter.ReportOneItemProcessed();

						if (progressReporter.IsCancelled)
						{
							SaveIfRequired(configuration, factory, reloadedProvider, workflowProviders.Length);
							return;
						}
					}
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1113:DoNotShowMessageBoxFromBusinessLayer", Justification = "GUI always attached to menu items.")]
		static bool CanReapplyTemplates(IWorkflowProvider provider, ReapplyWorkflowTemplateUserOptions options)
		{
			if (provider is ISometimesWorkflowProvider sometimesWorkflowProvider && !sometimesWorkflowProvider.ShouldSupportWorkflowTemplateApplication)
			{
				return false;
			}

			if (IsReapplyingTemplatesWithExistingItems(options) && provider.Workflows.Cast<IProcessHeader>().Any(p => !p.IsInDatabase))
			{
				var list = new ReapplyWorkflowAndTasksOptionsList();
				Globals.Message.Show(ResString.GetMultilingualString("D68588E0-CA10-4096-ABC2-51C14D6BCF63",
				@"Existing Workflows must be saved before reapplying Workflows/Tasks with setting '{0} - {1}'", options.ReapplyWorkflowAndTasksOptions, list.GetDescriptionFromCode(options.ReapplyWorkflowAndTasksOptions)));
				return false;
			}

			return true;
		}

		static void SaveIfRequired(IReapplyWorkflowTemplateConfiguration configuration, BusinessObjectFactory factory, IWorkflowProvider workflowProvider, int providerCount)
		{
			if (configuration.ProcessAndSaveInNewFactory)
			{
				try
				{
					factory.Save();
				}
				catch (ZSaveException ex)
				{
					ReportMessage(ex.Message, (BusinessObject)workflowProvider, providerCount);
				}
			}
		}

		static void ReapplyTemplates(IReapplyWorkflowTemplateConfiguration configuration, ReapplyWorkflowTemplateUserOptions options, IWorkflowProvider workflowProvider, BusinessObjectFactory factory)
		{
			if (!configuration.DelayReapplyTemplatesToServiceTask)
			{
				var parameters = IsReapplyingTemplatesWithExistingItems(options)
					? TemplateApplicationParameters.ReapplyTemaplate()
					: TemplateApplicationParameters.Default;
				workflowProvider.ApplyWorkflowTemplates(parameters);
			}
			else
			{
				if (WorkflowDataRegistry.Instance.EnableWorkflowManualChangeEvent.Value)
				{
					RaiseReapplicationRequestedEvent(options, workflowProvider, factory);
				}
			}
		}

		static void DeleteRequiredWorkflowItems(ReapplyWorkflowTemplateUserOptions options, IWorkflowProvider workflowProvider, BusinessObjectFactory factory, int workflowProviderCount)
		{
			try
			{
				if (options.ReapplyWorkflowAndTasksOptions == ReapplyWorkflowAndTasksOptionsList.Codes.DeleteAllAndReapply)
				{
					DeleteWorkflows(workflowProvider, factory);
					DeleteTasks(workflowProvider);
				}
				else if (options.ReapplyWorkflowAndTasksOptions == ReapplyWorkflowAndTasksOptionsList.Codes.DeleteUnactionedAndReapply)
				{
					DeleteUnactionedTasksAndWorkflows(workflowProvider, factory);
				}

				if (options.ReapplyMilestonesOptions == ReapplyMilestonesOptionsList.Codes.DeleteAllAndReapply)
				{
					DeleteMilestones(workflowProvider);
				}

				if (options.ReapplyTriggersOptions == ReapplyTriggersOptionsList.Codes.DeleteAllAndReapply)
				{
					DeleteTriggers(workflowProvider);
				}
			}
			catch (CannotDeleteException ex)
			{
				ReportMessage(ex.Message, (BusinessObject)workflowProvider, workflowProviderCount);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1113:DoNotShowMessageBoxFromBusinessLayer", Justification = "GUI always attached to menu items.")]
		static void ReportMessage(string message, BusinessObject workflowProvider, int providerCount)
		{
			var jobNumber = JobNumberResolver.GetJobNumber(workflowProvider);

			if (providerCount > 1)
			{
				Globals.Message.Show(ResString.GetMultilingualString("CD8c8346-4350-4beb-b3d2-f46151fa5ac1",
				@"Part of {0} could not be deleted.

{1}

This job cannot have templates reapplied. Other jobs will be processed.", jobNumber, message));
			}
			else
			{
				Globals.Message.Show(ResString.GetMultilingualString("d13f8abb-c8c6-4afb-b435-4d1f4c853cb2",
				@"Part of {0} could not be deleted.

{1}

This job cannot have templates reapplied.", jobNumber, message));
			}
		}

		static void DeleteWorkflows(IWorkflowProvider job, BusinessObjectFactory factory)
		{
			var processJobHeader = ProcessJobHeaderProvider.GetForParent(job, factory);
			if (processJobHeader != null)
			{
				processJobHeader.ProcessHeaders.DeleteAll();
			}
		}

		static void DeleteTasks(IWorkflowProvider workflowProvider)
		{
			workflowProvider.WorkflowItems.Tasks.RemoveAndDeleteAll();
		}

		static void DeleteUnactionedTasksAndWorkflows(IWorkflowProvider job, BusinessObjectFactory factory)
		{
			foreach (ProcessTask task in job.WorkflowItems.Tasks.ToArray())
			{
				if (task.IsOpenOrAssigned)
				{
					job.WorkflowItems.Tasks.RemoveAndDelete(task);
				}
			}

			var processJobHeader = ProcessJobHeaderProvider.GetForParent(job, factory);
			if (processJobHeader != null)
			{
				foreach (IProcessHeader processHeader in processJobHeader.ProcessHeaders.ToArray())
				{
					if (!processHeader.Tasks.Any())
					{
						processJobHeader.ProcessHeaders.Delete(processHeader);
					}
				}
			}
		}

		static void DeleteMilestones(IWorkflowProvider workflowProvider)
		{
			workflowProvider.WorkflowItems.Milestones.RemoveAndDeleteAll();
		}

		static void DeleteTriggers(IWorkflowProvider workflowProvider)
		{
			workflowProvider.WorkflowItems.Triggers.RemoveAndDeleteAll();
		}

		static void RaiseReapplicationRequestedEvent(ReapplyWorkflowTemplateUserOptions options, IWorkflowProvider job, BusinessObjectFactory factory)
		{
			if (IsReapplyingTemplatesWithExistingItems(options))
			{
				throw new NotSupportedException("Keeping existing tasks is not currently supported when template application is deferred to Log Walker");
			}

			var @event = new EventValue(AutoEvents.ReapplyWorkflowTemplatesRequested, parameters: GetEventReferenceParameters(options), deferFiringWorkflow: true);
			var jobInCorrectFactory = factory.Load(((BusinessObject)job).TablePrefix, job.PK);

			jobInCorrectFactory.GetLogs().AddNew(@event);
		}

		static Dictionary<string, string> GetEventReferenceParameters(ReapplyWorkflowTemplateUserOptions options)
		{
			ZBool reapplyTasks = options.ReapplyWorkflowAndTasksOptions == ReapplyWorkflowAndTasksOptionsList.Codes.DeleteAllAndReapply;
			ZBool reapplyMilestones = options.ReapplyMilestonesOptions == ReapplyMilestonesOptionsList.Codes.DeleteAllAndReapply;
			ZBool reapplyTriggers = options.ReapplyTriggersOptions == ReapplyTriggersOptionsList.Codes.DeleteAllAndReapply;
			//must be ZBool as ZBool.ToString() is different from bool.ToString()

			return new Dictionary<string, string>
			{
				{ "TSK", reapplyTasks.ToString() },
				{ "MIL", reapplyMilestones.ToString() },
				{ "TRG", reapplyTriggers.ToString() },
				{ "RG", options.ReCalculateReleaseGroups.ToString() },
			};
		}

		static bool IsReapplyingTemplatesWithExistingItems(ReapplyWorkflowTemplateUserOptions options)
		{
			return options.ReapplyWorkflowAndTasksOptions == ReapplyWorkflowAndTasksOptionsList.Codes.KeepsExistingAndReapply
				|| options.ReapplyWorkflowAndTasksOptions == ReapplyWorkflowAndTasksOptionsList.Codes.DeleteUnactionedAndReapply;
		}
	}
}
