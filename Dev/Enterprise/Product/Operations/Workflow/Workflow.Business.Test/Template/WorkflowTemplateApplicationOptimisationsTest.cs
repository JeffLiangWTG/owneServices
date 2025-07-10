using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Workflow.Business.Test
{
	public class WorkflowTemplateApplicationOptimisationsTest : TestCaseWithFactory
	{
		public void TestTemplateApplicationDoesNotHappenDuringUniversalShipmentExport()
		{
			var consol = Factory.New<Forwarding.IForwardingConsol>() as IWorkflowProvider;

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var communicationMode = orgHeader.EDICommunicationsModes.AddNew();
			communicationMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
			communicationMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
			communicationMode.EK_Destination = "DDP_DummyDestination";
			communicationMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
			communicationMode.EK_Module = WorkflowDescriptors.JobConsolWorkflowDescriptorCode;

			var trigger = consol.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Test2";
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00.Code;
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
			action.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.SpecialCodes.Other;
			action.PQ_TriggerParty = nameof(RecipientRoleType.CNE);
			action.PQ_OH_Recipient = orgHeader.PK;

			consol.Logs.AddNew(AutoEvents.CustomisableEvent00);

			Factory.Save();

			var template = Factory.New<ProcessTaskTemplate>();
			template.P0_Name = "TEST";
			template.P0_ProcessType = WorkflowDescriptors.JobConsolWorkflowDescriptorCode;
			template.GlobalTemplate = true;

			var task = template.WorkflowItems.Tasks.AddNew();
			task.P9_Description = "Test1";
			task.TemplateConditions.TemplateCondition2 = ProcessTasksLookups.UserDefinedCondition;
			task.TemplateConditions.TemplateCondition2Value = "\"1\" == \"1\"";

			Factory.Save();

			void AssertTaskCountOnConsol(string assertionMessage, int taskCount)
			{
				var reloadFactory = new BusinessObjectFactory();
				var workflowProvider = reloadFactory.Load<Forwarding.IForwardingConsol>(consol.PK) as IWorkflowProvider;
				AssertEquals(assertionMessage, taskCount, workflowProvider.WorkflowItems.Tasks.Count);
			}

			AssertTaskCountOnConsol("Pre-condition: Template Task not applied yet", 0);

			using (WorkflowDataRegistry.Instance.ApplyTemplatesWhenEventsAreAddedToAJob.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
			{
				MasterFilesTestHelper.RunLogWalker();
			}

			AssertTaskCountOnConsol("The only edit a Universal Shipment Export should cause is a DataExport event, this should not cause template application unless explicitly turned on.", 0);

			consol.Logs.AddNew(AutoEvents.CustomisableEvent00);
			Factory.Save();

			using (WorkflowDataRegistry.Instance.ApplyTemplatesWhenEventsAreAddedToAJob.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://"))
			{
				MasterFilesTestHelper.RunLogWalker();
			}

			AssertTaskCountOnConsol("The only edit a Universal Shipment Export should cause is a DataExport event, this should not cause template application unless explicitly turned on.", 1);
		}
	}
}
