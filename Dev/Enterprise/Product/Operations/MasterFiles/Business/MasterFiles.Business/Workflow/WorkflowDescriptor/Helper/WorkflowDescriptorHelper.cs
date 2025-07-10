using System;
using CargoWise.Application;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.MasterFiles.Business
{
	public static class WorkflowDescriptorHelper
	{
		public static IMessageProcessor GetWorkflowTriggerActionForStandardXmlActionType(Type adapterType, IWorkflowProvider workflowProvider, MessageProcessorCommunicationModesResult xmlModes, ProcessTaskNotification action)
		{
			object adapter = Activator.CreateInstance(adapterType);
			var delivererType = ObjectFactory.GetType("XmlMessageDeliver");
			var deliverer = Activator.CreateInstance(delivererType, xmlModes, workflowProvider, adapter, action, null);

			return deliverer as IMessageProcessor;
		}
	}
}
