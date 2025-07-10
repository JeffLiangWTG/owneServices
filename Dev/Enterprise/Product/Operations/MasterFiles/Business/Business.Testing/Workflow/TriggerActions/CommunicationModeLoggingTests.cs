using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Integration.Forwarding;
using static Enterprise.MasterFiles.Business.Testing.MasterFilesTestHelper;
using FileCodes = Enterprise.MasterFiles.Business.EDICommunicationsModeFileFormatList.Codes;
using TypeCodes = Enterprise.MasterFiles.Business.WorkflowTriggerActionTypeConstants.Codes;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CommunicationModeLoggingTests : TemplateApplicationTestCase
	{
		void InvoiceUniversalSetup(OrgProxyEnv orgp) => AddCommunicationMode(orgp.Org, FileCodes.All, WorkflowDescriptors.ARInvoiceCode, EDICommunicationsModeCommunicationsTransportList.Codes.EHubService);

		void ShipmentUniversalSetup(OrgProxyEnv orgp) => AddCommunicationMode(orgp.Org, FileCodes.All, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode, EDICommunicationsModeCommunicationsTransportList.Codes.EHubService);

		void NTFSetup(OrgProxyEnv orgp) => AddCommunicationMode(orgp.Org, FileCodes.NotificationEmail, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode, transport: EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText);

		void NoMatchingOrgSetup(OrgProxyEnv orgp) => orgp.Branch.Company.GC_OH_OrgProxy = ZGuid.Empty;

		string ActionStartingLog(ProcessTask trigger, ProcessTaskNotification action)
			=> $"Action [Type={action.PQ_TriggerType},Recipient={action.PQ_Calc_TriggerParty},Purpose={action.PQ_MessagePurpose}] starting for trigger [{trigger.TriggerConditions.TriggerEventCode}-{trigger.P9_Description}]";

		string ActionFailedLog(ProcessTask trigger, ProcessTaskNotification action)
			=> $"Action [Type={action.PQ_TriggerType},Recipient={action.PQ_Calc_TriggerParty},Purpose={action.PQ_MessagePurpose}] failed for trigger [{trigger.TriggerConditions.TriggerEventCode}-{trigger.P9_Description}]";

		string SuccessLog(OrgProxyEnv orgp, ProcessTask trigger, ProcessTaskNotification action)
			=> $"{ActionStartingLog(trigger, action)} with Organization [{orgp.Org.OH_Code}] for Company [{orgp.Branch.Company.GC_Code}]";

		string EmailLog(OrgProxyEnv orgp, ProcessTask trigger, ProcessTaskNotification action)
			=> $"{ActionStartingLog(trigger, action)} with Address taken from Trigger Party";

		string NoCommunicationModesLog(OrgProxyEnv orgp, ProcessTask trigger, ProcessTaskNotification action)
			=> $"{ActionFailedLog(trigger, action)} because Organization [{orgp.Org.OH_Code}] for Company [{orgp.Branch.Company.GC_Code}] has no matching Communication Modes.";

		string NoMatchingOrgLog(OrgProxyEnv orgp, ProcessTask trigger, ProcessTaskNotification action)
			=> $"{ActionFailedLog(trigger, action)} because Company [{orgp.Branch.Company.GC_Code}] has no Organization Proxy";

		public void TestNTF_Success()
			=> AssertLogForCommunicationModes<IForwardingShipment>(TypeCodes.NotificationEmail, SuccessLog, NTFSetup);

		public void TestNTF_NoCommunicationModes()
			=> AssertLogForCommunicationModes<IForwardingShipment>(TypeCodes.NotificationEmail, NoCommunicationModesLog);

		public void TestNTF_EML()
			=> AssertLogForCommunicationModes<IForwardingShipment>(TypeCodes.NotificationEmail, EmailLog, NTFSetup, MessageRecipientPartyTypeList.Codes.Email);

		public void TestNTF_NoMatchingOrg()
			=> AssertLogForCommunicationModes<IForwardingShipment>(TypeCodes.NotificationEmail, NoMatchingOrgLog, NoMatchingOrgSetup);

		public void TestXUS_Success()
			=> AssertLogForCommunicationModes<IForwardingShipment>(TypeCodes.SendUniversalShipmentXML, SuccessLog, ShipmentUniversalSetup);

		public void TestXUS_NoCommunicationModes()
			=> AssertLogForCommunicationModes<IForwardingShipment>(TypeCodes.SendUniversalShipmentXML, NoCommunicationModesLog);

		public void TestXUS_NoMatchingOrg()
			=> AssertLogForCommunicationModes<IForwardingShipment>(TypeCodes.SendUniversalShipmentXML, NoMatchingOrgLog, NoMatchingOrgSetup);

		public void TestXUE_Success()
			=> AssertLogForCommunicationModes<IForwardingShipment>(TypeCodes.SendUniversalEventXML, SuccessLog, ShipmentUniversalSetup);

		public void TestXUE_NoCommunicationModes()
			=> AssertLogForCommunicationModes<IForwardingShipment>(TypeCodes.SendUniversalEventXML, NoCommunicationModesLog);

		public void TestXUE_NoMatchingOrg()
			=> AssertLogForCommunicationModes<IForwardingShipment>(TypeCodes.SendUniversalEventXML, NoMatchingOrgLog, NoMatchingOrgSetup);

		public void TestXUT_Success()
			=> AssertLogForCommunicationModes<IARInvoice>(TypeCodes.SendUniversalTransactionXML, SuccessLog, InvoiceUniversalSetup);

		public void TestXUT_NoCommunicationModes()
			=> AssertLogForCommunicationModes<IARInvoice>(TypeCodes.SendUniversalTransactionXML, NoCommunicationModesLog);

		public void TestXUT_NoMatchingOrg()
			=> AssertLogForCommunicationModes<IARInvoice>(TypeCodes.SendUniversalTransactionXML, NoMatchingOrgLog, NoMatchingOrgSetup);

		public void TestNative_Success()
			=> AssertLogForCommunicationModes<IForwardingShipment>(TypeCodes.SendNativeXML, SuccessLog, ShipmentUniversalSetup);

		public void TestNative_NoCommunicationModes()
			=> AssertLogForCommunicationModes<IForwardingShipment>(TypeCodes.SendNativeXML, NoCommunicationModesLog);

		public void TestNative_NoMatchingOrg()
			=> AssertLogForCommunicationModes<IForwardingShipment>(TypeCodes.SendNativeXML, NoMatchingOrgLog, NoMatchingOrgSetup);

		public void TestEmailTransports_NotSupportedForUniversalXml()
		{
			void CommunicationMode_ALL_EMT(OrgProxyEnv orgp) => AddCommunicationMode(orgp.Org, FileCodes.All, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode, EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText);
			AssertLogForCommunicationModes<IForwardingShipment>(TypeCodes.SendUniversalShipmentXML, NoCommunicationModesLog, CommunicationMode_ALL_EMT);

			void CommunicationMode_ALL_EMA(OrgProxyEnv orgp) => AddCommunicationMode(orgp.Org, FileCodes.All, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode, EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsAttachment);
			AssertLogForCommunicationModes<IForwardingShipment>(TypeCodes.SendUniversalShipmentXML, NoCommunicationModesLog, CommunicationMode_ALL_EMA);

			void CommunicationMode_ALL_EDP(OrgProxyEnv orgp) => AddCommunicationMode(orgp.Org, FileCodes.All, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode, EDICommunicationsModeCommunicationsTransportList.Codes.EHubService);
			AssertLogForCommunicationModes<IForwardingShipment>(TypeCodes.SendUniversalShipmentXML, SuccessLog, CommunicationMode_ALL_EDP);

			void CommunicationMode_ALL_HUB(OrgProxyEnv orgp) => AddCommunicationMode(orgp.Org, FileCodes.All, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode, EDICommunicationsModeCommunicationsTransportList.Codes.EHubService);
			AssertLogForCommunicationModes<IForwardingShipment>(TypeCodes.SendUniversalShipmentXML, SuccessLog, CommunicationMode_ALL_HUB);
		}

		void AssertLogForCommunicationModes<BizoT>(string triggerType, Func<OrgProxyEnv, ProcessTask, ProcessTaskNotification, string> expectedLog, Action<OrgProxyEnv> orgSetup = null, string recipientPartyType = MessageRecipientPartyTypeList.Codes.OrgProxy) where BizoT : class
		{
			using (var orgp = SetupOrgProxy(Factory))
			{
				orgSetup?.Invoke(orgp);

				var bizo = (IWorkflowProvider)Factory.New<BizoT>();
				((BusinessObject)bizo).FillWithValidTestData();

				var trigger = MakeTrigger(bizo);
				var action = MakeNotification(trigger);
				action.PQ_TriggerType = triggerType;
				action.PQ_Calc_TriggerParty = recipientPartyType;

				bizo.Logs.AddNew(Events.CustomisableEvent00);
				Factory.Save();

				var logs = ObjectFactory.Get<IWorkflowServiceTaskTestHelper>().RunLogWalker();

				AssertContains(expectedLog(orgp, trigger, action), logs);
			}
		}

		#region Regression Tests

		class WorkflowDescriptorForFallbackEmailTest : DummyWorkflowDescriptor
		{
			protected internal override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageTriggerParties, BusinessObject bizObj, ZString partyType)
			{
				messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(((IBusiness)bizObj).Factory.New<OrgHeader>(), "aaa@bbb.ccc"));
			}

			public IEDICommunicationsMode[] GetMessageRecipientEdiCommunicationsModesExposed(ProcessTaskNotification action, BusinessObject parent, ZString fileFormat)
			{
				return GetMessageRecipientEdiCommunicationsModes(new WorkflowTriggerActionSource(parent, action.Parent, action, new ExampleLog(action.Parent), null), fileFormat).CommunicationModes.ToArray();
			}

			public IEDICommunicationsMode[] GetMessageRecipientEdiCommunicationsModesExposed(ProcessTaskNotification action, BusinessObject parent, ZString fileFormat, MessageRecipientPartyCollection collection)
			{
				return GetMessageRecipientEdiCommunicationsModes(new WorkflowTriggerActionSource(parent, action.Parent, action, new ExampleLog(action.Parent), null), fileFormat, collection).CommunicationModes.ToArray();
			}
		}

		public void TestGetMessageRecipientEdiCommunicationsModesFallbackToEmail_ALLDoesNotBreakThis()
		{
			using (var orgp = SetupOrgProxy(Factory))
			using (var orgp2 = SetupOrgProxy(Factory))
			{
				AddCommunicationMode(orgp.Org, FileCodes.All);

				var dummy = Factory.New<DummyWithWorkflow>();
				var trigger = MakeTrigger(dummy);
				var action = MakeNotification(trigger);
				action.PQ_TriggerType = TypeCodes.SendNativeXML;
				action.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;

				var workFlowDescriptor = new WorkflowDescriptorForFallbackEmailTest();

				WorkflowDataRegistry.Instance.EDICommunicationModeFallbackToOrganizationEmail.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				var collection = new MessageRecipientPartyCollection();
				collection.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(orgp.Org, ZString.Empty));
				collection.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(orgp2.Org, ZString.Empty));
				var communicationModes = workFlowDescriptor.GetMessageRecipientEdiCommunicationsModesExposed(action, dummy, "XYZ", collection);

				AssertEquals(2, communicationModes.Length);
			}
		}

		public void TestGetMessageRecipientEdiCommunicationsModesFallbackToEmail_MultipleCommunicationModes()
		{
			using (var orgp = SetupOrgProxy(Factory))
			{
				AddCommunicationMode(orgp.Org, FileCodes.All);
				AddCommunicationMode(orgp.Org, FileCodes.All);

				var dummy = Factory.New<DummyWithWorkflow>();
				var trigger = MakeTrigger(dummy);
				var action = MakeNotification(trigger);
				action.PQ_TriggerType = TypeCodes.SendNativeXML;
				action.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;

				var workFlowDescriptor = new WorkflowDescriptorForFallbackEmailTest();
				WorkflowDataRegistry.Instance.EDICommunicationModeFallbackToOrganizationEmail.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				var communicationModes = workFlowDescriptor.GetMessageRecipientEdiCommunicationsModesExposed(action, dummy, "XYZ");

				AssertEquals(0, communicationModes.Length);

				WorkflowDataRegistry.Instance.EDICommunicationModeFallbackToOrganizationEmail.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				communicationModes = workFlowDescriptor.GetMessageRecipientEdiCommunicationsModesExposed(action, dummy, "XYZ");

				AssertEquals(1, communicationModes.Length);
				AssertEquals("turtle@fish.com", communicationModes[0].EK_Destination);
			}
		}

		#endregion
	}
}
