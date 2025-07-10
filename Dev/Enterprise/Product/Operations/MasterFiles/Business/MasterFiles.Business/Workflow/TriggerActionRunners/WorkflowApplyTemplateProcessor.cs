using System.Globalization;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture;

namespace Enterprise.MasterFiles.Business
{
	public sealed class WorkflowApplyTemplateProcessor : IProcessor
	{
		readonly ProcessTaskNotification action;
		readonly IWorkflowProvider workflowProvider;

		public WorkflowApplyTemplateProcessor(ProcessTaskNotification action, IWorkflowProvider workflowProvider)
		{
			this.action = action;
			this.workflowProvider = workflowProvider;
		}

		public void Process(INotifications notifications, CancellationToken token
#if DEBUG
			= new CancellationToken()
#endif
		)
		{
			if (action != null && action.Parent != null)
			{
				var workflowProviderAsBusinessObject = workflowProvider as BusinessObject;

				if (workflowProvider == null || workflowProviderAsBusinessObject == null)
				{
					notifications.AddError(ApplyTemplateProcessorLogger.GetCouldNotLoadJobLog());
					return;
				}

				var template = action.WorkflowTemplate;
				if (template == null)
				{
					notifications.AddError(ApplyTemplateProcessorLogger.GetNullTemplateLog(workflowProviderAsBusinessObject));
					return;
				}

				if (!template.P0_IsActive)
				{
					notifications.AddWarning(ApplyTemplateProcessorLogger.GetInActiveTemplateLog(template.P0_Name));
					return;
				}

				var workflowProviderAsTemplate = workflowProvider as ProcessTaskTemplate;
				if (workflowProviderAsTemplate != null)
				{
					notifications.AddError(ApplyTemplateProcessorLogger.GetTriedToApplyTemplateToTemplateLog(template.P0_Name, workflowProviderAsTemplate.P0_Name));
					return;
				}

				var factory = workflowProviderAsBusinessObject.Factory;
				var parameters = TemplateApplicationParameters.ApplySpecificTemplates(new[] { template }, triggerApplicationType: TriggerApplicationTypeHelper.GetTriggerApplicationType(action.PQ_TriggerType), ignoreHasChanges: true);
				var result = new ProcessTask.Loader(factory).CreateTasksAndMilestonesFromTemplateIfRequired(workflowProvider, parameters);

				var (isWarning, log) = ApplyTemplateProcessorLogger.GetTemplateApplicationResultLog(JobNumberResolver.GetJobNumber(workflowProviderAsBusinessObject), template.P0_Name, result.ApplicationResult);

				if (isWarning)
				{
					notifications.AddWarning(log);
				}
				else
				{
					notifications.Add(new InfoNotification(log));
				}
			}
		}
	}

	public static class ApplyTemplateProcessorLogger
	{
		#region SuppressResourceStringsCheckRegion // Service task logs are not translated.

		public static string GetCouldNotLoadJobLog()
		{
			return "The job that the template should be applied to cannot be found.";
		}

		public static string GetNullTemplateLog(BusinessObject job)
		{
			return string.Format(CultureInfo.InvariantCulture, "Cannot find the specified template to apply to job {0}.", JobNumberResolver.GetJobNumber(job));
		}

		public static string GetInActiveTemplateLog(string templateName)
		{
			return string.Format(CultureInfo.InvariantCulture, "Did not apply the template [{0}] as it was not active.", templateName);
		}

		public static string GetTriedToApplyTemplateToTemplateLog(string templateJobName, string templateToApplyName)
		{
			return string.Format(CultureInfo.InvariantCulture, "Cannot apply a template to a template. Tried to apply template [{0}] to template [{1}].", templateJobName, templateToApplyName);
		}

		public static (bool IsWarning, string Log) GetTemplateApplicationResultLog(string jobNumber, string templateName, TemplateApplicationResult result)
		{
			string logForErrorReport;
			switch (result)
			{
				case TemplateApplicationResult.Success:
					return (false, string.Format(CultureInfo.InvariantCulture, "Partial template [{0}] applied to job [{1}].", templateName, jobNumber));
				case TemplateApplicationResult.NoNewMatchingTemplateItems:
					return (true, string.Format(CultureInfo.InvariantCulture, "Partial template [{0}] has already been applied to job [{1}].", templateName, jobNumber));
				case TemplateApplicationResult.JobDeleted:
				case TemplateApplicationResult.JobCancelled:
					return (true, string.Format(CultureInfo.InvariantCulture, "Partial template [{0}] cannot be applied to job [{1}] as it has been deleted or cancelled.", templateName, jobNumber));
				case TemplateApplicationResult.TemplateScopeEnforcerRestriction:
					// This is likley Template Company Rules are not met
					return (true, string.Format(CultureInfo.InvariantCulture, "Partial template [{0}] cannot be applied to job [{1}] as template conditions are not met.", templateName, jobNumber));
				case TemplateApplicationResult.NoMatchingTemplate:
					return (true, string.Format(CultureInfo.InvariantCulture, "Partial template [{0}] cannot be applied to job [{1}].", templateName, jobNumber));
				case TemplateApplicationResult.UnsupportedISometimesWorkflowProvider:
					return (true, string.Format(CultureInfo.InvariantCulture, "Partial template [{0}] cannot be applied to job [{1}] as it does not support template application.", templateName, jobNumber));

				// Everything below should not happen and will be error reported
				case TemplateApplicationResult.InvalidLogin:
					logForErrorReport = string.Format(CultureInfo.InvariantCulture, "Partial template [{0}] cannot be applied to job [{1}] due to invalid login.", templateName, jobNumber);
					break;
				case TemplateApplicationResult.InvalidWorkflowProvider:
					logForErrorReport = string.Format(CultureInfo.InvariantCulture, "Partial template [{0}] cannot be applied to job [{1}] due to invalid workflow provider.", templateName, jobNumber);
					break;
				case TemplateApplicationResult.TemplateApplicationSuspended:
					logForErrorReport = string.Format(CultureInfo.InvariantCulture, "Partial template [{0}] cannot be applied to job [{1}] as template application is suppended.", templateName, jobNumber);
					break;
				default:
					logForErrorReport = string.Format(CultureInfo.InvariantCulture, "Partial template [{0}] cannot be applied to job [{1}].", templateName, jobNumber);
					break;
			}

			ErrorReporter.ReportOnce($"Template application failed due to {result}", logForErrorReport);
			return (true, logForErrorReport);
		}

		#endregion
	}
}
