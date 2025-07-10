using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;
using static Enterprise.MasterFiles.Business.WorkflowDescriptor;

namespace Enterprise.MasterFiles.Business
{
	static class LineTriggerWorkflowDescriptorHelper
	{
		public static bool ParentSupportsWorkflowTriggerActionUniversalShipmentXML(IBusiness parent)
		{
			if (!(parent is IWorkflowProvider workflowProvider))
			{
				return false;
			}

			var workflowType = workflowProvider is ProcessTaskTemplate template ? template.P0_ProcessType : workflowProvider.WorkflowType;

			return WorkflowDescriptors.Instance.TryGetValueSafe(workflowType)?.SupportsWorkflowTriggerActionUniversalShipmentXML ?? false;
		}

		public static IProcessor GetWorkflowTriggerActionForXUE_or_XUS(this WorkflowTriggerActionSource source, IQueuedLog queuedLog)
		{
			/* Design Trick
				* We use the parent job's workflow descriptor instead of this because
				* we use the parent job's code when specifying which EDI Communication modes are chosen.
				* 
				* Due to this, clients must specify Communications Modes as if the trigger was firing against the parent of the task 
				* rather than the Task itself.
				* This behaviour is distinct from default Trigger's supporting XUE and XUS.
			*/
			var action = source.Action;
			var processTask = source.Job as ProcessTask;
			var trigger = source.Trigger;
			var parentJobWorkflowDescriptor = processTask.WorkflowDescriptorCore;
			var eventInfo = new EventInfoProvider(queuedLog, trigger, processTask) { DoNotProvideEventReference = true };

			IProcessor processor = null;

			if (action.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML)
			{
				if (parentJobWorkflowDescriptor.SupportsWorkflowTriggerActionUniversalEventXML)
				{
					processor = new LineTriggerUniversalEventTriggerActionBuilder(parentJobWorkflowDescriptor, new ActionWrapper(action, processTask, source.EventProvider, queuedLog), eventInfo).GetUniversalWorkflowProcessor();
				}
				else
				{
					processor = new LogAction((NoResString)"Universal Event is not supported by this job type.");
				}
			}

			if (action.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML)
			{
				if (parentJobWorkflowDescriptor.SupportsWorkflowTriggerActionUniversalShipmentXML)
				{
					processor = new UniversalShipmentTriggerActionBuilder(parentJobWorkflowDescriptor, new ActionWrapper(action, processTask.Parent as BusinessObject, source.EventProvider, queuedLog), eventInfo).GetUniversalWorkflowProcessor();
				}
				else
				{
					processor = new LogAction((NoResString)"Universal Shipment is not supported by this job type.");
				}
			}

			return processor;
		}

		public static BusinessObject[] GetUDFMacroDataContext(ITriggerConditions workflowItem, BusinessObject parent)
		{
			var dataContext = new[] { workflowItem as BusinessObject };
			if (parent is ProcessTask processTask)
			{
				var enterpriseItemType = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(processTask.P9_ParentTableCode);
				var processTaskParent = processTask.Factory.Load(enterpriseItemType, processTask.P9_ParentID.ToGuid());
				dataContext = dataContext.Append(processTaskParent).ToArray();
			}
			return dataContext;
		}

		public static string[] GetEmailAddresses(BusinessObject parent, Func<ProcessTask, IEnumerable<GlbStaff>> taskStaffGetter)
		{
			var parentTask = parent as ProcessTask;

			if (parentTask == null)
			{
				return Array.Empty<string>();
			}

			var staffRecipients = taskStaffGetter(parentTask);

			if (staffRecipients != null)
			{
				return staffRecipients.Select(staff => staff?.GS_EmailAddress.ToString()).Where(s => !string.IsNullOrEmpty(s)).ToArray();
			}

			return Array.Empty<string>();
		}
	}
}
