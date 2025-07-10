using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	public static class EDICommunicationRulesValidation
	{
		delegate IMessageProcessor GetMessageProcessor();

		static object ContextGroupingKey(ProcessTask task)
		{
			switch (task.TriggerConditions.TriggerContextCode)
			{
				case TriggerUserContextList.Codes.Specified:
					return new
					{
						task.TriggerConditions.TriggerCompany,
						task.TriggerConditions.TriggerBranch,
						task.TriggerConditions.TriggerDepartment,
					};
				case TriggerUserContextList.Codes.Event: //No context switch happens for validation as we can't predict the triggering event context
					return new
					{
						task.TriggerConditions.TriggerContextCode,
					};
				case TriggerUserContextList.Codes.Default:
				default:
					return new
					{
						task.TriggerConditions.TriggerCompany,
					};
			}
		}

		static bool IsValidationRequired(IBaseTrigger trigger, ProcessTaskNotification action, out GetMessageProcessor getProcessor, out BusinessObject parent)
		{
			var descriptor = action.WorkflowDescriptor;
			parent = null;
			getProcessor = null;

			if (descriptor == null)
			{
				return false;
			}

			if (trigger is ILineTriggerSupport line && !line.LineTriggerType.IsEmpty)
			{
				// Communication modes aren't known because the line isn't known.
				return false;
			}

			parent = trigger?.GetJob();
			if (parent == null || typeof(ProcessTaskTemplate).IsAssignableFrom(parent.GetType()))
			{
				return false;
			}

			var log = new ExampleLog(action);
			var source = new WorkflowTriggerActionSource(parent, trigger, action, log, null);
			getProcessor = () => descriptor.GetWorkflowTriggerAction(source, log) as IMessageProcessor;
			return getProcessor() is IMessageProcessor;
		}

		/// <summary>
		/// The correct user context must be set before calling this method
		/// </summary>
		static bool HasInvalidCommunicationModes(GetMessageProcessor getProcessor, out string validationWarning)
		{
			var destinations = getProcessor().GetDestinations();
			validationWarning = destinations.ConfigurationLogging;
			return destinations.Destinations.Count == 0 && !string.IsNullOrEmpty(validationWarning);
		}

		public static void ValidateEDICommunicationRules(BusinessObjectCollection<ProcessTask> tasks)
		{
			var taskGroups = tasks.Where(t => t.IsMilestoneOrWorkflowTrigger).GroupBy(t => ContextGroupingKey(t));

			foreach (var group in taskGroups)
			{
				IDisposable userContext = null;
				try
				{
					foreach (var task in group)
					{
						foreach (var action in task.ProcessTaskNotifications)
						{
							if (IsValidationRequired(task, action, out GetMessageProcessor getProcessor, out BusinessObject parent))
							{
								userContext = userContext ?? ObjectFactory.Get<IWorkflowTriggerUserContextProvider>().SetTemporaryUserContext(task, parent);

								using (HasInvalidCommunicationModes(getProcessor, out string validationWarning) ? AdditionalCommunicationModeValidation(action.PQ_Calc_TriggerPartyInfo, validationWarning) : null)
								{
									action.Validation.ValidatePQ_Calc_TriggerParty();
								}
							}
						}
					}
				}
				finally
				{
					userContext?.Dispose();
				}
			}
		}

		static IDisposable AdditionalCommunicationModeValidation(ZPropertyInfo info, string validationWarning)
		{
			var addValidationWarning = new RunValidationInvoker(() => info.AddWarning(validationWarning));
			info.AdditionalValidation += addValidationWarning;
			return new DisposableAction(
				() =>
				{
					info.AdditionalValidation -= addValidationWarning;
				});
		}
	}
}
