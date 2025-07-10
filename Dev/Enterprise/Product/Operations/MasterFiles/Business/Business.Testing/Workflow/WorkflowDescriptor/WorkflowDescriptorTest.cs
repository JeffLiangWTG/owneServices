using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business.MessageDelivery;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Customs.EUExitControl;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(DummyEnterpriseBusinessObjectWorkflowDescriptor))]
	sealed class WorkflowDescriptorTest : WorkflowDescriptorTestCase<DummyEnterpriseBusinessObjectWorkflowDescriptor>
	{
		public void TestGetAction_DbHits()
		{
			var descriptor = WorkflowDescriptor;
			var dummy = Factory.New<DummyWithWorkflow>();
			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Emu";
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			var notification = trigger.ProcessTaskNotifications.AddNew();

			var log = new ExampleLog(notification);
			var source = new WorkflowTriggerActionSource(dummy, trigger, notification, log, null);
			var hits = new Dictionary<string, int>();
			hits[StmEventSchema.Constants.TableName] = 1;
			hits[GlbCompanySchema.Constants.TableName] = 1;
			foreach (CodeDescriptionPair pair in new WorkflowTriggerActionTypeConstants())
			{
				notification.PQ_TriggerType = pair.Code;
				using (AssertDbHitsWithUsefulQueryInformation(hits, Factory))
				{
					try
					{
						descriptor.GetWorkflowTriggerAction(source, log);
					}
					catch (Exception)
					{
						// Eh.
					}
				}
			}
		}

		public void TestGetSecurityCheckPoint()
		{
			var parentBO = Factory.New<DummyWithWorkflow>();
			var trigger = parentBO.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;

			AssertEquals(true, WorkflowDescriptor.WorkflowTriggerActionTypeSecurityCheckPoints.Contains(new KeyValuePair<ZString, SecurityCheckpoint>(WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML, Env.Security.None)));
		}

		public void TestWorkFlowTriggerActionForUniversalShipmentXMLSendsInternallyOrExternallyAppropriately()
		{
			TestWorkflowTriggerActionForUniversalXMLSendsInternallyOrExternallyAppropriately(
				EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment, WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML);
		}

		public void TestWorkFlowTriggerActionForUniversalEventXMLSendsInternallyOrExternallyAppropriately()
		{
			TestWorkflowTriggerActionForUniversalXMLSendsInternallyOrExternallyAppropriately(
				EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent, WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML);
		}

		public void TestWorkFlowTriggerActionForUniversalEventCollectionXMLSendsInternallyOrExternallyAppropriately()
		{
			TestWorkflowTriggerActionForUniversalXMLSendsInternallyOrExternallyAppropriately(
				EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent, WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventCollectionXML);
		}

		public void TestWorkFlowTriggerActionForUniversalTransactionXMLSendsInternallyOrExternallyAppropriately()
		{
			TestWorkflowTriggerActionForUniversalXMLSendsInternallyOrExternallyAppropriately(
				EDICommunicationsModeFileFormatList.Codes.XmlUniversalTransaction, WorkflowTriggerActionTypeConstants.Codes.SendUniversalTransactionXML, false);
		}

		public void TestWorkFlowTriggerActionForUniversalScheduleXMLSendsInternallyOrExternallyAppropriately()
		{
			TestWorkflowTriggerActionForUniversalXMLSendsInternallyOrExternallyAppropriately(
				EDICommunicationsModeFileFormatList.Codes.XmlUniversalSchedule, WorkflowTriggerActionTypeConstants.Codes.SendUniversalScheduleXML);
		}

		void TestWorkflowTriggerActionForUniversalXMLSendsInternallyOrExternallyAppropriately(ZString fileFormat, ZString actionType)
		{
			TestWorkflowTriggerActionForUniversalXMLSendsInternallyOrExternallyAppropriately(fileFormat, actionType, true);
		}

		void TestWorkflowTriggerActionForUniversalXMLSendsInternallyOrExternallyAppropriately(ZString fileFormat, ZString actionType, bool shouldSendInternally)
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Name = "When all the world";
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "Is a hopless jumble";
			company.GC_OH_OrgProxy = org.PK;
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_BranchName = "And the raindrops";
			branch.GB_GC = company.PK;

			Factory.Save();
			using (Env.SetTemporaryUserContext(Env.Instance.CurrentUserPK, branch.PK.ToGuid(), Env.Instance.CurrentDepartmentPK))
			{
				AssertEquals(0, GlbCompany.CurrentCompany.OrgProxy.EDICommunicationsModes.Count);
				var communicationMode = GlbCompany.CurrentCompany.OrgProxy.EDICommunicationsModes.AddNew();
				communicationMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
				communicationMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
				communicationMode.EK_Destination = "DummyDestination";
				communicationMode.EK_FileFormat = fileFormat;
				communicationMode.EK_Module = "DUM";

				var parentBO = Factory.New<DummyWithWorkflow>();
				var processTask = parentBO.WorkflowItems.Triggers.AddNew();
				processTask.TriggerConditions.TriggerEventCode = Events.CargoCheckin.Code;
				var internalSendAction = processTask.ProcessTaskNotifications.AddNew();
				internalSendAction.PQ_TriggerType = actionType;
				internalSendAction.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Forwarder;
				var logBO = parentBO.Logs.AddNew(Events.CargoCheckin);

				var workFlowDescriptor = new DummyEnterpriseBusinessObjectWorkflowDescriptor();
				var triggerLogBO = processTask.Logs.Find(aLog => aLog.SL_SE_NKEvent == Events.WorkflowTriggerEventCode).First();
				Factory.Save();

				var internalSendProcessor = workFlowDescriptor.GetWorkflowTriggerAction(internalSendAction, new QueuedLogForTesting(triggerLogBO, processTask));
				AssertNotNull("workFlowDescriptor.GetWorkflowTriggerAction() as IUniversalXmlWorkflowProcessor", internalSendProcessor as IUniversalXmlWorkflowProcessor);

				var communicationModesFieldInfo = internalSendProcessor.GetType().GetProperty("CommunicationModes", BindingFlags.Instance | BindingFlags.NonPublic);
				var communicationModesForInternal = (IList<IEDICommunicationsMode>)communicationModesFieldInfo.GetValue(internalSendProcessor);

				if (shouldSendInternally)
				{
					AssertNotNull("Should have internal communication mode", communicationModesForInternal.Single(m => m.EK_CommunicationsTransport == EDICommunicationsModeCommunicationsTransportList.Codes.UniversalDataBuss));
				}
				else
				{
					AssertNull("Should have external communication mode", communicationModesForInternal.SingleOrDefault(m => m.EK_CommunicationsTransport == EDICommunicationsModeCommunicationsTransportList.Codes.UniversalDataBuss));
				}

				var externalSendAction = processTask.ProcessTaskNotifications.AddNew();
				externalSendAction.PQ_TriggerType = actionType;
				externalSendAction.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;

				var externalSendProcessor = workFlowDescriptor.GetWorkflowTriggerAction(externalSendAction, new QueuedLogForTesting(triggerLogBO, processTask));
				AssertNotNull("workFlowDescriptor.GetWorkflowTriggerAction() as IUniversalXmlWorkflowProcessor", externalSendProcessor as IUniversalXmlWorkflowProcessor);

				var communicationModes = (IList<IEDICommunicationsMode>)communicationModesFieldInfo.GetValue(externalSendProcessor);

				AssertContainsExactElementsInAnyOrder(new IEDICommunicationsMode[] { communicationMode }, communicationModes);
			}
		}

		public void TestWorkflowTriggerActionForUniversalXMLUsingAlternateParty()
		{
			var communicationMode = GlbCompany.CurrentCompany.OrgProxy.EDICommunicationsModes.Cast<EDICommunicationsMode>().FirstOrDefault(x =>
				x.EK_CommsDirection == EDICommunicationsModeCommsDirectionList.Codes.Transmit &&
				x.EK_CommunicationsTransport == EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface &&
				x.EK_Destination == "DummyDestination" &&
				x.EK_FileFormat == EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment &&
				x.EK_Module == "DUM");
			if (communicationMode == null)
			{
				communicationMode = GlbCompany.CurrentCompany.OrgProxy.EDICommunicationsModes.AddNew();
				communicationMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
				communicationMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
				communicationMode.EK_Destination = "DummyDestination";
				communicationMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalShipment;
				communicationMode.EK_Module = "DUM";
			}

			var parentBO = Factory.New<DummyWithWorkflow>();
			var processTask = parentBO.WorkflowItems.Triggers.AddNew();
			processTask.TriggerConditions.TriggerEventCode = Events.CargoCheckinCode;
			var taskNotification = processTask.ProcessTaskNotifications.AddNew();
			taskNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
			taskNotification.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.SpecialCodes.Other;
			taskNotification.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Forwarder;
			taskNotification.PQ_OH_Recipient = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			var logBO = parentBO.Logs.AddNew(Events.CargoCheckin);

			var workFlowDescriptor = new DummyEnterpriseBusinessObjectWorkflowDescriptor();
			var triggerLogBO = processTask.Logs.Find(aLog => aLog.SL_SE_NKEvent == Events.WorkflowTriggerEventCode).First();

			Factory.Save();

			var internalSendProcessor = workFlowDescriptor.GetWorkflowTriggerAction(taskNotification, new QueuedLogForTesting(triggerLogBO, processTask));
			AssertNotNull("workFlowDescriptor.GetWorkflowTriggerAction() as IUniversalXmlWorkflowProcessor", internalSendProcessor as IUniversalXmlWorkflowProcessor);

			var communicationModesFieldInfo = internalSendProcessor.GetType().GetProperty("CommunicationModes", BindingFlags.Instance | BindingFlags.NonPublic);
			var communicationModesForInternal = (IList<IEDICommunicationsMode>)communicationModesFieldInfo.GetValue(internalSendProcessor);

			AssertNotNull("Should have internal communication mode", communicationModesForInternal.Single(m => m.EK_CommunicationsTransport == EDICommunicationsModeCommunicationsTransportList.Codes.UniversalDataBuss));

			var externalSendAction = processTask.ProcessTaskNotifications.AddNew();
			externalSendAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
			externalSendAction.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;

			var externalSendProcessor = workFlowDescriptor.GetWorkflowTriggerAction(externalSendAction, new QueuedLogForTesting(triggerLogBO, processTask));
			AssertNotNull("workFlowDescriptor.GetWorkflowTriggerAction() as IUniversalXmlWorkflowProcessor", externalSendProcessor as IUniversalXmlWorkflowProcessor);

			var communicationModes = (IList<IEDICommunicationsMode>)communicationModesFieldInfo.GetValue(externalSendProcessor);

			AssertContainsExactElementsInAnyOrder(new IEDICommunicationsMode[] { communicationMode }, communicationModes);
		}

		public void TestGetWorkFlowTriggerActionForUniversalShipmentXML()
		{
			var parentBO = Factory.New<DummyWithWorkflow>();
			var processTask = parentBO.WorkflowItems.Triggers.AddNew();
			var action = processTask.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;

			var workFlowDescriptor = new DummyEnterpriseBusinessObjectWorkflowDescriptor();

			var result = workFlowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(Factory) { SJ_SE_NKEvent = Events.WorkflowTriggerEventCode });
			AssertNotNull("workFlowDescriptor.GetWorkflowTriggerAction() as IUniversalShipmentXmlWorkflowProcessor", result as IUniversalXmlWorkflowProcessor);
		}

		public void TestMessageTargetAllRecipientParties()
		{
			var parentBO = Factory.New<DummyWithWorkflow>();
			var processTask = parentBO.WorkflowItems.Triggers.AddNew();
			var action = processTask.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;

			var workFlowDescriptor = new DummyEnterpriseBusinessObjectWorkflowDescriptor();

			WorkflowTriggerNotification processor = (WorkflowTriggerNotification)workFlowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(Factory));
			AssertEquals("Should get all 3 recipients from 2 recipient organizations", 3, processor.Modes.CommunicationModes.Count);
			AssertEquals("Should have contact from recipient organization1", "contact@Recipient1.com", processor.Modes.CommunicationModes[0].EK_Destination);
			AssertEquals("Should have contact from recipient organization2", "contact@Recipient2.com", processor.Modes.CommunicationModes[1].EK_Destination);
			AssertEquals("Should have contact2 from recipient organization2", "contact2@Recipient2.com", processor.Modes.CommunicationModes[2].EK_Destination);
		}

		#region TestGetMessageRecipientEdiCommunicationsModesFallbackToEmail

		public void TestGetMessageRecipientEdiCommunicationsModesFallbackToEmail()
		{
			var parentBO = Factory.New<DummyWithWorkflow>();
			var processTask = parentBO.WorkflowItems.Triggers.AddNew();
			var action = processTask.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;

			var workFlowDescriptor = new WorkflowDescriptorForFallbackEmailTest();
			WorkflowDataRegistry.Instance.EDICommunicationModeFallbackToOrganizationEmail.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var communicationModes = workFlowDescriptor.GetMessageRecipientEdiCommunicationsModesExposed(action, parentBO, null, "XYZ");

			AssertEquals(0, communicationModes.Length);

			WorkflowDataRegistry.Instance.EDICommunicationModeFallbackToOrganizationEmail.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			communicationModes = workFlowDescriptor.GetMessageRecipientEdiCommunicationsModesExposed(action, parentBO, null, "XYZ");

			AssertEquals(1, communicationModes.Length);
			AssertEquals("aaa@bbb.ccc", communicationModes[0].EK_Destination);
			AssertEquals(EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText, communicationModes[0].EK_CommunicationsTransport);
			AssertEquals(workFlowDescriptor.Description + " - " + processTask.DescriptionWithReference, communicationModes[0].EK_ServerAddressSubject);
			AssertEquals("XYZ", communicationModes[0].EK_FileFormat);

			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationBodyEmail;
			WorkflowDataRegistry.Instance.EDICommunicationModeFallbackToOrganizationEmail.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			communicationModes = workFlowDescriptor.GetMessageRecipientEdiCommunicationsModesExposed(action, parentBO, null, "XYZ");

			AssertEquals(1, communicationModes.Length);
			AssertEquals("aaa@bbb.ccc", communicationModes[0].EK_Destination);
			AssertEquals(EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText, communicationModes[0].EK_CommunicationsTransport);
			AssertEquals(workFlowDescriptor.Description + " - " + processTask.DescriptionWithReference, communicationModes[0].EK_ServerAddressSubject);
			AssertEquals("XYZ", communicationModes[0].EK_FileFormat);
		}

		class WorkflowDescriptorForFallbackEmailTest : DummyWorkflowDescriptor
		{
			protected internal override void AddToMessageRecipientPartyList(MessageRecipientPartyCollection messageTriggerParties, BusinessObject bizObj, ZString partyType)
			{
				messageTriggerParties.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(((IBusiness)bizObj).Factory.New<OrgHeader>(), "aaa@bbb.ccc"));
			}

			public IEDICommunicationsMode[] GetMessageRecipientEdiCommunicationsModesExposed(ProcessTaskNotification action, BusinessObject parent, WorkflowTriggerEventData @event, ZString fileFormat)
			{
				return GetMessageRecipientEdiCommunicationsModes(new WorkflowTriggerActionSource(parent, action.Parent, action, @event, null), fileFormat).CommunicationModes.ToArray();
			}
		}

		#endregion

		public void TestGetWorkFlowTriggerActionForUniversalEventXMLWithEDoc()
		{
			var parentBO = Factory.New<DummyWithWorkflow>();
			var trigger = parentBO.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.DocumentAllocated.Code;
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXMLWithEDoc;
			var logBO = parentBO.Logs.AddNew(Events.DocumentAllocated);

			Factory.Save();

			var triggerLogBO = trigger.Logs.Find(aLog => aLog.SL_SE_NKEvent == Events.WorkflowTriggerEventCode).First();
			var workFlowDescriptor = new DummyEnterpriseBusinessObjectWorkflowDescriptor();

			var result = workFlowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(triggerLogBO, trigger));
			AssertNotNull("workFlowDescriptor.GetWorkflowTriggerAction() as IUniversalEventXmlWorkflowProcessor", result as IUniversalXmlWorkflowProcessor);
		}

		public void TestGetWorkFlowTriggerActionForUniversalEventXML()
		{
			var parentBO = Factory.New<DummyWithWorkflow>();
			var trigger = parentBO.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.CargoCheckinCode;
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML;
			var logBO = parentBO.Logs.AddNew(Events.CargoCheckin);

			Factory.Save();

			var triggerLogBO = trigger.Logs.Find(aLog => aLog.SL_SE_NKEvent == Events.WorkflowTriggerEventCode).First();
			var workFlowDescriptor = new DummyEnterpriseBusinessObjectWorkflowDescriptor();

			var result = workFlowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(triggerLogBO, trigger));
			AssertNotNull("workFlowDescriptor.GetWorkflowTriggerAction() as IUniversalEventXmlWorkflowProcessor", result as IUniversalXmlWorkflowProcessor);
		}

		public void TestGetWorkFlowTriggerActionForUniversalEventCollectionXML()
		{
			var parentBO = Factory.New<DummyWithWorkflow>();
			var trigger = parentBO.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.CargoCheckinCode;
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventCollectionXML;
			var logBO = parentBO.Logs.AddNew(Events.CargoCheckin);

			Factory.Save();

			var triggerLogBO = trigger.Logs.Find(aLog => aLog.SL_SE_NKEvent == Events.WorkflowTriggerEventCode).First();
			var workFlowDescriptor = new DummyEnterpriseBusinessObjectWorkflowDescriptor();

			var result = workFlowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(triggerLogBO, trigger));
			AssertNotNull("workFlowDescriptor.GetWorkflowTriggerAction() as IUniversalEventXmlWorkflowProcessor", result as IUniversalXmlWorkflowProcessor);
		}

		public void TestGetWorkflowTriggerActionForSendNativeXML()
		{
			var parentBO = Factory.New<DummyWithWorkflow>();
			var trigger = parentBO.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendNativeXML;
			var logBO = parentBO.Logs.AddNew(Events.Authorised);

			Factory.Save();
			var triggerLogBO = trigger.Logs.Find(aLog => aLog.SL_SE_NKEvent == Events.WorkflowTriggerEventCode).First();
			var workFlowDescriptor = new DummyEnterpriseBusinessObjectWorkflowDescriptor();

			var result = workFlowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(triggerLogBO, trigger));
			AssertNotNull("workFlowDescriptor.GetWorkflowTriggerAction() result", result);
			AssertEquals("workFlowDescriptor.GetWorkflowTriggerAction() result Type", "Enterprise.MasterFiles.DataTransfer.Native.NativeXmlWorkflowProcessor", result.GetType().FullName);
		}

		public void TestSetXmlFileNameForXmlActionTypes()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			ProcessTaskNotification action = dummy.WorkflowItems.Triggers.AddNew().ProcessTaskNotifications.AddNew();
			var workFlowDescriptor = new DummyEnterpriseBusinessObjectWorkflowDescriptor();

			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendNativeXML;
			var actionSource = new WorkflowTriggerActionSource(dummy, action.Parent, action, new ExampleLog(action.Parent), null);
			var communicationsModes = workFlowDescriptor.GetMessageRecipientEdiCommunicationsModesForEmailParty(actionSource).CommunicationModes;

			AssertEquals(1, communicationsModes.Count);
			AssertEquals(EDIMessageDelivery.ReplacementConstants.JobNumber + ".xml", communicationsModes[0].EK_Filename);

			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXMLSimplified;
			communicationsModes = workFlowDescriptor.GetMessageRecipientEdiCommunicationsModesForEmailParty(actionSource).CommunicationModes;

			AssertEquals(1, communicationsModes.Count);
			AssertEquals(EDIMessageDelivery.ReplacementConstants.JobNumber + ".xml", communicationsModes[0].EK_Filename);

			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.AddDocumentToEDocs;
			communicationsModes = workFlowDescriptor.GetMessageRecipientEdiCommunicationsModesForEmailParty(actionSource).CommunicationModes;

			AssertEquals(1, communicationsModes.Count);
			AssertEquals("", communicationsModes[0].EK_Filename);
		}

		public void TestLoader()
		{
			var loader = new WorkflowDescriptor.Loader();
			var list = ObjectFactory.Get<IWorkflowDescriptorList>();
			Assert("Providers exist in the list", list.Count > 10);

			foreach (CodeDescriptionPair providerCodePair in list)
			{
				AssertNotNull(loader.GetWorkflowDescriptor(providerCodePair.Code));
			}

			AssertNull("Should return null for wrong workflow type code", loader.GetWorkflowDescriptor("@#$"));
		}

		public void TestIsForCountryShouldNotUseCurrentCompany()
		{
			var countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var template = Factory.New<ProcessTaskTemplate>();
			template.GlobalTemplate = true;
			var templateTask = template.WorkflowItems.Tasks.AddNew();
			var task = Factory.New<ProcessTask>();
			task.P9_GC = ZGuid.Empty;

			CombineAssertions(() =>
			{
				Assert($"!IsForCountry({countryCode}) when GlobalTemplate = true", !templateTask.IsForCountry(template, countryCode));
				Assert($"!IsForCountry({countryCode}) when P9_GC is Empty", !task.IsForCountry(template, countryCode));
			});
		}

		public void TestIsForCountry()
		{
			AssertIsForCountry(ConsolAirCargoWorkflowDescriptorExtensions.IsForCountry);
		}

		public void TestIsForCountryOrTemplateDischargePortCountry()
		{
			AssertIsForCountry(ConsolAirCargoWorkflowDescriptorExtensions.IsForCountryOrTemplateDischargePortCountry);
			var countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var template = Factory.New<ProcessTaskTemplate>();
			template.GlobalTemplate = true;
			var templateTask = template.WorkflowItems.Tasks.AddNew();

			Assert($"ProcessTask !IsForCountry({countryCode}) by P9_GC", !templateTask.IsForCountryOrTemplateDischargePortCountry(template, countryCode));
			template.P0_DischargePortCountry = countryCode;
			Assert($"ProcessTask IsForCountry({countryCode}) by P0_DischargePortCountry", templateTask.IsForCountryOrTemplateDischargePortCountry(template, countryCode));
		}

		void AssertIsForCountry(Func<IWorkflowItem, BusinessObject, ZString, bool> isForMethod)
		{
			var countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			var template = Factory.New<ProcessTaskTemplate>();
			var templateTask = template.WorkflowItems.Tasks.AddNew();
			var task = Factory.New<ProcessTask>();

			CombineAssertions(() =>
			{
				Assert($"ProcessTask IsForCountry({countryCode}) by P9_GC", isForMethod(task, null, countryCode));

				var company = Factory.New<GlbCompany>();
				company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Eritrea;
				task.P9_GC = company.PK;

				Assert($"ProcessTask !IsForCountry({countryCode}) by P9_GC", !isForMethod(task, null, countryCode));
				Assert($"ProcessTask IsForCountry({Core.Constants.CountryCodes.Eritrea}) by P9_GC", isForMethod(task, null, Core.Constants.CountryCodes.Eritrea));

				Assert($"TemplateProcessTask IsForCountry({countryCode}) by P0_GC", isForMethod(templateTask, template, countryCode));

				var branch = Factory.New<GlbBranch>();
				branch.GB_GC = company.PK;
				template.P0_GB = branch.PK;
				Assert($"TemplateProcessTask !IsForCountry({countryCode}) by P0_GB", !isForMethod(templateTask, template, countryCode));
				Assert($"TemplateProcessTask IsForCountry({Core.Constants.CountryCodes.Eritrea}) by P0_GB", isForMethod(templateTask, template, Core.Constants.CountryCodes.Eritrea));

				template.P0_GB = ZGuid.Empty;
				Assert($"TemplateProcessTask IsForCountry({countryCode}) by P0_GC", isForMethod(templateTask, template, countryCode));
				template.GlobalTemplate = true;
				Assert($"TemplateProcessTask !IsForCountry({countryCode}) when GlobalTemplate = true", !isForMethod(templateTask, template, countryCode));
			});
		}

		public void TestGetCompanyCountryCodeFromWorkflowItemOrTemplate_FromProcessTask()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Latvia;
			var processTask = Factory.New<ProcessTask>();
			processTask.P9_GC = company.PK;
			AssertEquals(Core.Constants.CountryCodes.Latvia, WorkflowDescriptor.GetCompanyCountryCodeFromWorkflowItemOrTemplate(processTask, null));
		}

		public void TestGetCompanyCountryCodeFromWorkflowItemOrTemplate_FromProcessTaskTemplate()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Latvia;
			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = company.PK;

			var processTask = Factory.New<ProcessTask>();
			processTask.P9_GC = ZGuid.Empty;
			var template = Factory.New<ProcessTaskTemplate>();
			CombineAssertions(() =>
			{
				AssertEquals("by P0_GC", GlbCompany.CurrentCompany.GC_RN_NKCountryCode, WorkflowDescriptor.GetCompanyCountryCodeFromWorkflowItemOrTemplate(processTask, template));

				template.P0_GB = branch.PK;
				AssertEquals("by P0_GB", Core.Constants.CountryCodes.Latvia, WorkflowDescriptor.GetCompanyCountryCodeFromWorkflowItemOrTemplate(processTask, template));
			});
		}

		public void TestGetCompanyCountryCodeFromWorkflowItemOrTemplate_Empty()
		{
			var processTask = Factory.New<ProcessTask>();
			processTask.P9_GC = ZGuid.Empty;
			var template = Factory.New<ProcessTaskTemplate>();
			template.P0_GB = ZGuid.Empty;
			template.P0_GC = ZGuid.Empty;
			AssertEquals(ZString.Empty, WorkflowDescriptor.GetCompanyCountryCodeFromWorkflowItemOrTemplate(processTask, template));
		}

		public void TestWorkFlowTriggerActionForSendUniversalManifestEventXML()
		{
			var parentBO = Factory.New<DummyWithWorkflow>();
			var trigger = parentBO.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.CargoCheckinCode;
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalManifestEventXML;

			var workFlowDescriptor = new DummyEnterpriseBusinessObjectWorkflowDescriptor();

			var result = workFlowDescriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(trigger.Logs.AddNew(Events.WorkflowTriggerEvent), trigger));
			AssertType<WorkflowDescriptor.LogAction>(result);
		}

		public void TestGetWorkflowTriggerActionForTransactionAllocationAndPost()
		{
			var workflow = Factory.New<DummyWithWorkflow>();
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			workflow.Logs.AddNew(Events.AddedARecordToTheSystem);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Factory.Save();

			var trigger = workflow.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.AddedARecordToTheSystemCode;

			var notificationATP = trigger.ProcessTaskNotifications.AddNew();
			notificationATP.PQ_TriggerType = "ATP";
			Factory.Save();

			var log = trigger.Logs.GetAllLogs().OfType<StmALog>().Single(x => x.SL_SE_NKEvent == Events.WorkflowTriggerEventCode);
			var result = WorkflowDescriptor.GetWorkflowTriggerAction(notificationATP, new QueuedLogForTesting(log, trigger));

			AssertNotNull(result);
			AssertEquals("TriggerActionWithOptionalFactorySaveProcessor", result.GetType().Name);
		}

		public void TestGetWorkflowTriggerActionForImportAPInvoicesFromOtherCompanies()
		{
			var workflow = Factory.New<DummyWithWorkflow>();
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			workflow.Logs.AddNew(Events.AddedARecordToTheSystem);
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
			Factory.Save();

			var trigger = workflow.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.AddedARecordToTheSystemCode;

			var notificationATP = trigger.ProcessTaskNotifications.AddNew();
			notificationATP.PQ_TriggerType = "ISI";
			Factory.Save();

			var log = trigger.Logs.GetAllLogs().OfType<StmALog>().Single(x => x.SL_SE_NKEvent == Events.WorkflowTriggerEventCode);
			var result = WorkflowDescriptor.GetWorkflowTriggerAction(notificationATP, new QueuedLogForTesting(log, trigger));

			AssertNotNull(result);
			AssertEquals("TriggerActionWithOptionalFactorySaveProcessor", result.GetType().Name);
		}

		public void TestSupportsAutomatedExitReportTransferMessageSending_RelatedCountrySupportsAutoSendExitReportTransferMessage()
		{
			var companyDE = Factory.New<GlbCompany>();
			companyDE.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Germany; //Has countrySpecific CusExitReport-implementation and supports SupportAutoSendExitReportTransferMessage
			var companyES = Factory.New<GlbCompany>();
			companyES.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Spain;  //Has countrySpecific CusExitReport-implementation
			var companyLV = Factory.New<GlbCompany>();
			companyLV.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Latvia; //No countrySpecific CusExitReport-implementation

			var dummyExitControlMessagesSupporter = Factory.New<DummyExitControlMessagesSupporterForTest>();
			var processTask = Factory.New<ProcessTask>();
			processTask.P9_Type = Enterprise.Core.Constants.Workflow.WorkflowTriggerType;
			var workFlowDescriptor = new DummyEnterpriseBusinessObjectWorkflowDescriptor();

			CombineAssertions(() =>
			{
				processTask.P9_GC = companyDE.PK;
				AssertEquals("IsTrigger: true; DE-RelatedCountrySupportsAutoSendExitReportTransferMessage: true", expected: true, workFlowDescriptor.SupportsAutomatedExitReportTransferMessageSending(dummyExitControlMessagesSupporter, processTask));

				processTask.P9_GC = companyES.PK;
				AssertEquals("IsTrigger: true; ES-RelatedCountrySupportsAutoSendExitReportTransferMessage: false", expected: false, workFlowDescriptor.SupportsAutomatedExitReportTransferMessageSending(dummyExitControlMessagesSupporter, processTask));

				processTask.P9_GC = companyLV.PK;
				AssertEquals("IsTrigger: true; LV-RelatedCountrySupportsAutoSendExitReportTransferMessage: false", expected: false, workFlowDescriptor.SupportsAutomatedExitReportTransferMessageSending(dummyExitControlMessagesSupporter, processTask));
			});
		}

		public void TestSupportsAutomatedExitReportTransferMessageSending_IsTrigger()
		{
			var companyDE = Factory.New<GlbCompany>();
			companyDE.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Germany; //Has countrySpecific CusExitReport-implementation and supports SupportAutoSendExitReportTransferMessage

			var dummyExitControlMessagesSupporter = Factory.New<DummyExitControlMessagesSupporterForTest>();
			var processTask = Factory.New<ProcessTask>();
			processTask.P9_GC = companyDE.PK;
			var workFlowDescriptor = new DummyEnterpriseBusinessObjectWorkflowDescriptor();

			CombineAssertions(() =>
			{
				processTask.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;
				AssertEquals("IsTrigger: true; DE-RelatedCountrySupportsAutoSendExitReportTransferMessage: true", expected: true, workFlowDescriptor.SupportsAutomatedExitReportTransferMessageSending(dummyExitControlMessagesSupporter, processTask));

				processTask.P9_Type = Core.Constants.Workflow.MilestoneType;
				AssertEquals("IsTrigger: false; DE-RelatedCountrySupportsAutoSendExitReportTransferMessage: true", expected: false, workFlowDescriptor.SupportsAutomatedExitReportTransferMessageSending(dummyExitControlMessagesSupporter, processTask));
			});
		}

		public void TestSupportsAutomatedExitReportTransferMessageSending_ProcessTaskTemplate()
		{
			var companyDE = Factory.New<GlbCompany>();
			companyDE.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Germany; //Has countrySpecific CusExitReport-implementation and supports SupportAutoSendExitReportTransferMessage

			var template = Factory.New<ProcessTaskTemplate>();
			var processTask = Factory.New<ProcessTask>();
			processTask.P9_GC = companyDE.PK;
			processTask.P9_Type = Core.Constants.Workflow.WorkflowTriggerType;
			var workFlowDescriptor = new DummyEnterpriseBusinessObjectWorkflowDescriptor();

			var supportedWorkflowTypes = new[] { WorkflowDescriptors.CusExitHeaderWorkflowDescriptorCode, WorkflowDescriptors.JobDeclarationWorkflowDescriptorCode, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode };
			var allWorkflowTypes = template.Lookups.WorkflowTypeList.GetAllCodes();

			supportedWorkflowTypes.ForEach(x => AssertCollectionContains($"Prereq: {x} is part of allWorkflowTypes", x, allWorkflowTypes));
			CombineAssertions(() =>
			{
				foreach (var workflowType in allWorkflowTypes)
				{
					template.P0_ProcessType = workflowType;
					var isSupported = supportedWorkflowTypes.Contains(workflowType);
					AssertEquals($"P0_ProcessType: {workflowType}; WorkflowItemType: TRG", expected: isSupported, workFlowDescriptor.SupportsAutomatedExitReportTransferMessageSending(template, processTask));
				}
			});
		}

		sealed class DummyExitControlMessagesSupporterForTest : DummyBusinessObject, ISupportAutoSendExitReportTransferMessage
		{
			public DummyExitControlMessagesSupporterForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public Guid RegistryBranchPK => ZGuid.BrettsGuid.ToGuid();

			public IProcessor CreateStmProcessQueueProcessor(BusinessObject parent, ZString triggerActionCode) => null;

			public IProcessor GetSendExitReportTransferMessageProcessor(BusinessObject parent) => null;
		}

		#region Test Event Context

		public void TestIsRelatedEntityInContext_HandleInvalidOperationException()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			AssertNoExceptionThrown(() => DummyWorkflowDescriptor.Instance.IsRelatedEntityInContext(dummy, dummy, "XX YY ZZ"));
			AssertEquals(false, DummyWorkflowDescriptor.Instance.IsRelatedEntityInContext(dummy, dummy, "XX YY ZZ"));
		}

		public void TestGetContextStringFromPath()
		{
			var contextPair1 = new WorkflowEventContextPair(new CodeDescriptionPair("C1", null), new CodeDescriptionPair("T1", null));
			var contextPair2 = new WorkflowEventContextPair(new CodeDescriptionPair("C1", null), new CodeDescriptionPair("T2", null));
			var contextPair3 = new WorkflowEventContextPair(new CodeDescriptionPair("C2", null), new CodeDescriptionPair("T1", null));
			var contextPair4 = new WorkflowEventContextPair(new CodeDescriptionPair("", null), new CodeDescriptionPair("T1", null));
			var contextPair5 = new WorkflowEventContextPair(new CodeDescriptionPair("C3", null), new CodeDescriptionPair("T3", null));

			var contextString = DummyWorkflowDescriptor.Instance.GetContextStringFromPath(new[] { contextPair1, contextPair4, contextPair3, contextPair5, contextPair2, contextPair1 });

			AssertEquals("C1 T1,T1,C2 T1,C3 T3,C1 T2,C1 T1", contextString);
		}

		public void TestGetContextPathFromString()
		{
			var contextPair1 = new WorkflowEventContextPair(new CodeDescriptionPair("C1", null), new CodeDescriptionPair("T1", null));
			var contextPair2 = new WorkflowEventContextPair(new CodeDescriptionPair("C1", null), new CodeDescriptionPair("T2", null));
			var contextPair3 = new WorkflowEventContextPair(new CodeDescriptionPair("C2", null), new CodeDescriptionPair("T1", null));
			var contextPair4 = new WorkflowEventContextPair(new CodeDescriptionPair("", null), new CodeDescriptionPair("T1", null));
			var contextPair5 = new WorkflowEventContextPair(new CodeDescriptionPair("C3", null), new CodeDescriptionPair("T3", null));

			DummyWorkflowDescriptor.Instance.FollowingContextStepsForTest =
				new List<WorkflowEventContextPair>
				{
						contextPair1,
						contextPair2,
						contextPair3,
						contextPair4,
						contextPair5,
				};

			var contextPath = DummyWorkflowDescriptor.Instance.GetContextPathFromString("C1 T1,T1,C2 T1,C3 T3,C1 T2,C1 T1,XX YY", false).ToArray();

			AssertEquals(7, contextPath.Length);
			AssertEquals(contextPair1, contextPath[0]);
			AssertEquals(contextPair4, contextPath[1]);
			AssertEquals(contextPair3, contextPath[2]);
			AssertEquals(contextPair5, contextPath[3]);
			AssertEquals(contextPair2, contextPath[4]);
			AssertEquals(contextPair1, contextPath[5]);
			AssertEquals("XX", contextPath[6].MasterClassifier.Code);
			AssertEquals("YY", contextPath[6].MasterType.Code);
		}

		public void TestGetContextPathFromString_ExceptionOnWrongPart()
		{
			DummyWorkflowDescriptor.Instance.FollowingContextStepsForTest =
				new List<WorkflowEventContextPair>
				{
						new WorkflowEventContextPair(new CodeDescriptionPair("C1", null), new CodeDescriptionPair("T1", null)),
				};

			AssertNoExceptionThrown(() => DummyWorkflowDescriptor.Instance.GetContextPathFromString("C1 T1,C1 T1,C1 T1"));
			AssertNoExceptionThrown(() => DummyWorkflowDescriptor.Instance.GetContextPathFromString(" C1 T1, C1 T1 ,C1 T1 "));
			AssertNoExceptionThrown(() => DummyWorkflowDescriptor.Instance.GetContextPathFromString("C1 T1,XX YY,C1 T1", false));
			AssertExceptionThrown(typeof(InvalidOperationException), "Incorrect step 'XX YY' in context path 'C1 T1,XX YY,C1 T1'.", () => DummyWorkflowDescriptor.Instance.GetContextPathFromString("C1 T1,XX YY,C1 T1"));
			AssertExceptionThrown(typeof(InvalidOperationException), "Wrong number of parts in 'AA BB CC' in context path 'C1 T1,AA BB CC,C1 T1'.", () => DummyWorkflowDescriptor.Instance.GetContextPathFromString("C1 T1,AA BB CC,C1 T1"));
		}

		#endregion

		#region Test Methods not required for Base Testing.

		public override void TestID()
		{
			Assert("Base Testing Only.", true);
		}

		public override void TestDescription()
		{
			Assert("Base Testing Only.", true);
		}

		public override void TestSubTypes()
		{
			Assert("Base Testing Only.", true);
		}

		public override void TestRequiresPorts()
		{
			Assert("Base Testing Only.", true);
		}

		public override void TestRequiresClient()
		{
			Assert("Base Testing Only.", true);
		}

		public override void TestRequiresBranch()
		{
			Assert("Base Testing Only.", true);
		}

		public override void TestRequiresDepartment()
		{
			Assert("Base Testing Only.", true);
		}

		#endregion

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			return new IWorkflowProvider[] { Factory.New<DummyWithWorkflow>() };
		}

		protected override bool ShouldRunBufferManagementSupportedJobTypeTests => false;
	}
}
