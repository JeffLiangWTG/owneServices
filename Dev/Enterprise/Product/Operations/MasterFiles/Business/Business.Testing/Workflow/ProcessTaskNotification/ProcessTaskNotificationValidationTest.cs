using System;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Workflow.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Integration.Forwarding;
using static Enterprise.MasterFiles.Business.Testing.MasterFilesTestHelper;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ProcessTaskNotificationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestSecurityCheckpointIsAllowed()
		{
			var error = $"You do not have the appropriate security rights to add this Trigger Action.\r\nIf you require access to this Trigger Action, ask your system administrator to change either your Staff or Group Security Rights to allow access to: {Env.Security.ImporterSecurityFilingMessaging.DisplayTextPathToSecurityRight}.";
			Env.Security.ImporterSecurityFilingMessaging.IsAllowed = false;
			var parentBO = Factory.New<DummyWithWorkflow>();
			var trigger = parentBO.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;
			var triggerAction = trigger.ProcessTaskNotifications.AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ScheduleB3Message;
			AssertHasError(triggerAction.PQ_TriggerTypeInfo, error);

			Env.Security.ImporterSecurityFilingMessaging.IsAllowed = true;
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ScheduleB3Message;
			AssertNoError(triggerAction.PQ_TriggerTypeInfo, error);

			Env.Security.ImporterSecurityFilingMessaging.IsAllowed = true;
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "DUM";
			var templatetrigger = template.WorkflowItems.Triggers.AddNew();
			var templateTriggerAction = templatetrigger.ProcessTaskNotifications.AddNew();
			templateTriggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ScheduleB3Message;
			Assert(templatetrigger is TemplateProcessTask);
			AssertNoError(templateTriggerAction.PQ_TriggerTypeInfo, error);

			Env.Security.ImporterSecurityFilingMessaging.IsAllowed = false;
			templateTriggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ScheduleB3Message;
			AssertNoError(templateTriggerAction.PQ_TriggerTypeInfo, error);
		}

		#region CommunicationModeWarnings

		void DummyUniversalSetup(OrgHeader org, GlbBranch branch) => AddCommunicationMode(org, EDICommunicationsModeFileFormatList.Codes.All, null, EDICommunicationsModeCommunicationsTransportList.Codes.EHubService);

		void ShipmentUniversalSetup(OrgHeader org, GlbBranch branch) => AddCommunicationMode(org, EDICommunicationsModeFileFormatList.Codes.All, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode, EDICommunicationsModeCommunicationsTransportList.Codes.EHubService);

		void NTFSetup(OrgHeader org, GlbBranch branch) => AddCommunicationMode(org, EDICommunicationsModeFileFormatList.Codes.NotificationEmail, transport: EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText);

		void NoMatchingOrgSetup(OrgHeader org, GlbBranch branch) => branch.Company.GC_OH_OrgProxy = ZGuid.Empty;

		string NoWarnings(OrgProxyEnv orgp) => null;

		string NoCommunicationModesWarning(OrgProxyEnv orgp) => $"Organization [{orgp.Org.OH_Code}] for Company [{orgp.Branch.Company.GC_Code}] has no matching Communication Modes.";

		string NoMatchingOrgWarning(OrgProxyEnv orgp) => $"Company [{orgp.Branch.Company.GC_Code}] has no Organization Proxy";

		public void TestCommunicationModeWarning_NTF_Success()
			=> AssertCommunicationModeWarning<DummyWithWorkflow>(WorkflowTriggerActionTypeConstants.Codes.NotificationEmail, NoWarnings, NTFSetup);

		public void TestCommunicationModeWarning_NTF_EML()
			=> AssertCommunicationModeWarning<DummyWithWorkflow>(WorkflowTriggerActionTypeConstants.Codes.NotificationEmail, NoWarnings, NTFSetup, MessageRecipientPartyTypeList.Codes.Email);

		public void TestCommunicationModeWarning_NTF_NoCommunicationModes()
			=> AssertCommunicationModeWarning<DummyWithWorkflow>(WorkflowTriggerActionTypeConstants.Codes.NotificationEmail, NoCommunicationModesWarning);

		public void TestCommunicationModeWarning_NTF_NoMatchingOrg()
			=> AssertCommunicationModeWarning<DummyWithWorkflow>(WorkflowTriggerActionTypeConstants.Codes.NotificationEmail, NoMatchingOrgWarning, NoMatchingOrgSetup);

		public void TestCommunicationModeWarning_XUS_Success()
			=> AssertCommunicationModeWarning<IForwardingShipment>(WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML, NoWarnings, ShipmentUniversalSetup);

		public void TestCommunicationModeWarning_XUS_NoCommunicationModes()
			=> AssertCommunicationModeWarning<IForwardingShipment>(WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML, NoCommunicationModesWarning);

		public void TestCommunicationModeWarning_XUS_NoMatchingOrg()
			=> AssertCommunicationModeWarning<IForwardingShipment>(WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML, NoMatchingOrgWarning, NoMatchingOrgSetup);

		public void TestCommunicationModeWarning_XUE_Success()
			=> AssertCommunicationModeWarning<IForwardingShipment>(WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML, NoWarnings, ShipmentUniversalSetup);

		public void TestCommunicationModeWarning_XUE_NoCommunicationModes()
			=> AssertCommunicationModeWarning<IForwardingShipment>(WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML, NoCommunicationModesWarning);

		public void TestCommunicationModeWarning_XUE_NoMatchingOrg()
			=> AssertCommunicationModeWarning<IForwardingShipment>(WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML, NoMatchingOrgWarning, NoMatchingOrgSetup);

		public void TestCommunicationModeWarning_XUT_Success()
			=> AssertCommunicationModeWarning<DummyWithWorkflow>(WorkflowTriggerActionTypeConstants.Codes.SendUniversalTransactionXML, NoWarnings, DummyUniversalSetup);

		public void TestCommunicationModeWarning_XUT_NoCommunicationModes()
			=> AssertCommunicationModeWarning<DummyWithWorkflow>(WorkflowTriggerActionTypeConstants.Codes.SendUniversalTransactionXML, NoCommunicationModesWarning);

		public void TestCommunicationModeWarning_XUT_NoMatchingOrg()
			=> AssertCommunicationModeWarning<DummyWithWorkflow>(WorkflowTriggerActionTypeConstants.Codes.SendUniversalTransactionXML, NoMatchingOrgWarning, NoMatchingOrgSetup);

		public void TestCommunicationModeWarning_Native_Success()
			=> AssertCommunicationModeWarning<DummyWithWorkflow>(WorkflowTriggerActionTypeConstants.Codes.SendNativeXML, NoWarnings, DummyUniversalSetup);

		public void TestCommunicationModeWarning_Native_NoCommunicationModes()
			=> AssertCommunicationModeWarning<DummyWithWorkflow>(WorkflowTriggerActionTypeConstants.Codes.SendNativeXML, NoCommunicationModesWarning);

		public void TestCommunicationModeWarning_Native_NoMatchingOrg()
			=> AssertCommunicationModeWarning<DummyWithWorkflow>(WorkflowTriggerActionTypeConstants.Codes.SendNativeXML, NoMatchingOrgWarning, NoMatchingOrgSetup);

		void AssertCommunicationModeWarning<BizoT>(string triggerType, Func<OrgProxyEnv, string> expectedWarning, Action<OrgHeader, GlbBranch> orgSetup = null, string recipientPartyType = MessageRecipientPartyTypeList.Codes.OrgProxy) where BizoT : class
		{
			ProcessTaskNotification triggerAction;
			string expectedWarningString;
			var bizo = (IWorkflowProvider)Factory.New<BizoT>();
			using (var orgp = SetupOrgProxy(Factory, orgSetup))
			{
				var trigger = bizo.WorkflowItems.Triggers.AddNew();
				trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;

				triggerAction = trigger.CompletionTriggerActionsCollection().AddNew();
				triggerAction.PQ_TriggerType = triggerType;

				triggerAction.PQ_Calc_TriggerParty = recipientPartyType;
				expectedWarningString = expectedWarning(orgp);
			}

			EDICommunicationRulesValidation.ValidateEDICommunicationRules(bizo.WorkflowItems);

			if (expectedWarningString != null)
			{
				AssertHasWarning(triggerAction.PQ_Calc_TriggerPartyInfo, expectedWarningString);
			}
			else
			{
				AssertNoWarnings(triggerAction.PQ_Calc_TriggerPartyInfo);
			}
		}

		public void TestCommunicationModeWarning_ClearedOnChange()
		{
			var dummyBO = Factory.New<DummyWithWorkflow>();
			var trigger = dummyBO.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Hap";
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;

			var triggerAction = trigger.CompletionTriggerActionsCollection().AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			triggerAction.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;

			string orgName = triggerAction.WorkflowDescriptor
				.GetMessageRecipientPartyWithFailureReason(triggerAction, MessageRecipientPartyTypeList.Codes.OrgProxy)
				.recipients.ToArray()[0]
				.Party.OH_Code;

			EDICommunicationRulesValidation.ValidateEDICommunicationRules(dummyBO.WorkflowItems);
			AssertHasWarning(triggerAction.PQ_Calc_TriggerPartyInfo, $"Organization [{orgName}] for Company [{GlbCompany.CurrentCompany.GC_Code}] has no matching Communication Modes.");

			triggerAction.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			EDICommunicationRulesValidation.ValidateEDICommunicationRules(dummyBO.WorkflowItems);

			AssertNoWarnings(triggerAction.PQ_Calc_TriggerPartyInfo);
		}

		public void TestNTFActionRequiresEMLCommunicationmode()
		{
			using (var orgp = SetupOrgProxy(Factory))
			{
				var dummyBO = Factory.New<DummyWithWorkflow>();
				var trigger = dummyBO.WorkflowItems.Triggers.AddNew();
				trigger.P9_Description = "Hap";
				trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
				AddCommunicationMode(orgp.Org, EDICommunicationsModeFileFormatList.Codes.All, null, EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsAttachment);
				Factory.Save();

				var triggerAction = trigger.CompletionTriggerActionsCollection().AddNew();
				triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
				triggerAction.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;

				var expectedWarningString = NoCommunicationModesWarning(orgp);
				EDICommunicationRulesValidation.ValidateEDICommunicationRules(dummyBO.WorkflowItems);
				AssertHasWarning(triggerAction.PQ_Calc_TriggerPartyInfo, expectedWarningString);
			}
		}

		public void TestCommunicationModeWarning_SPK()
		{
			ProcessTaskNotification triggerAction;
			using (var orgp = SetupOrgProxy(Factory))
			{
				var bizo = (IWorkflowProvider)Factory.New<DummyWithWorkflow>();

				var trigger = bizo.WorkflowItems.Triggers.AddNew();
				trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
				trigger.TriggerConditions.TriggerContextCode = TriggerUserContextList.Codes.Event;

				triggerAction = trigger.CompletionTriggerActionsCollection().AddNew();
				triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendNativeXML;

				triggerAction.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;
				var expectedWarningString = NoCommunicationModesWarning(orgp);
				EDICommunicationRulesValidation.ValidateEDICommunicationRules(bizo.WorkflowItems);

				AssertHasWarning(triggerAction.PQ_Calc_TriggerPartyInfo, expectedWarningString);
			}
		}

		#endregion
	}

	public class ProcessTask_ProcessTaskNotificationValidationTest : ProcessTaskNotificationValidationTestCase<ProcessTask>
	{
		protected override ProcessTask CreateWorkflowItemForJob(IWorkflowProvider job, string workflowItemType = Constants.Workflow.WorkflowTriggerType)
		{
			return (ProcessTask)CreateWorkflowItemForJobCore(job, workflowItemType);
		}

		protected override ProcessTask CreateWorkflowItemForTemplate(ProcessTaskTemplate template, string workflowItemType = Constants.Workflow.WorkflowTriggerType)
		{
			return CreateWorkflowItemForJob(template, workflowItemType);
		}

		public void TestShouldNotAllowDLYAction_ForJobTriggers()
		{
			var dummyBO = Factory.New<DummyWithWorkflow>();
			var trigger = CreateWorkflowItemForJob(dummyBO);
			Assert("Precondition", trigger is IWorkflowTrigger);
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;

			var triggerAction = trigger.CompletionTriggerActionsCollection().AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ScheduleDelayedEvent;
			AssertHasError(triggerAction.PQ_TriggerTypeInfo, "DLY completion trigger actions are only allowed for universal triggers.");
		}

		public void TestErrorOnClientSideTriggerActionWithDelayedTrigger()
		{
			var dummyBO = Factory.New<DummyWithWorkflow>();
			var trigger = CreateWorkflowItemForJob(dummyBO);
			Assert("Precondition", trigger is IWorkflowTrigger);
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			trigger.P9_DelayDurationSeconds = 10;

			var triggerAction = trigger.CompletionTriggerActionsCollection().AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;

			AssertHasError(triggerAction.PQ_TriggerTypeInfo, "Cannot have immediate trigger action on a trigger with a delay duration.");
		}

		public void TestCheckPQ_TriggerType_TRA_Validation()
		{
			const string message = "Trigger action Type ‘TRA’ requires Line Trigger Type ‘CXR’ in its Trigger";
			var dummy = Factory.New<DummyWithWorkflow>();
			var trigger = CreateWorkflowItemForJob(dummy);
			var triggerAction = trigger.CompletionTriggerActionsCollection().AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendExitReportTransferMessage;
			AssertHasErrorContaining(triggerAction.PQ_TriggerTypeInfo, message);

			trigger.P9_LineTriggerType = TriggerLineTypes.Codes.CusExitReport;
			triggerAction.Validation.ValidatePQ_TriggerType();
			AssertNoErrorContaining(triggerAction.PQ_TriggerTypeInfo, message);
		}
	}

	sealed class ProcessTemplateTrigger_ProcessTaskNotificationValidationTest : ProcessTaskNotificationValidationTestCase<ITemplateTrigger>
	{
		protected override ITemplateTrigger CreateWorkflowItemForJob(IWorkflowProvider job, string workflowItemType = Constants.Workflow.WorkflowTriggerType)
		{
			if (job == null)
			{
				var trigger = Factory.New<IUniversalTemplateTrigger>();
				((BusinessObject)trigger).FillWithValidTestData();

				return trigger;
			}

			if (workflowItemType != Constants.Workflow.WorkflowTriggerType) // Only triggers are supported for Universal Templates.
			{
				return (ITemplateTrigger)CreateWorkflowItemForJobCore(job, workflowItemType);
			}
			else
			{
				return CreateWorkflowItemForTemplateType(job.WorkflowType);
			}
		}

		protected override ITemplateTrigger CreateWorkflowItemForTemplate(ProcessTaskTemplate template, string workflowItemType = Constants.Workflow.WorkflowTriggerType)
		{
			var trigger = template.TemplateTriggers.AddNew();
			trigger.FillWithValidTestData();

			return (ITemplateTrigger)trigger;
		}

		public void TestShouldAllowDLYAction_ForUniversalTriggers()
		{
			var dummyBO = Factory.New<DummyWithWorkflow>();
			var trigger = CreateWorkflowItemForJob(dummyBO);
			Assert("Precondition", trigger is IUniversalTemplateTrigger);
			trigger.TriggerConditions_ForBinding.TriggerEventCode = Events.CustomisableEvent00Code;

			var triggerAction = trigger.CompletionTriggerActionsCollection().AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ScheduleDelayedEvent;
			AssertNoNotifications(triggerAction.PQ_TriggerTypeInfo);
		}

		public void TestShouldAllowDLYAction_ForJobRepresentationsOfUniversalTriggers()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "DUM";
			template.P0_IsUniversal = true;
			template.P0_TriggerFallbackMethod = FallbackTypeList.Codes.AlwaysFallback;
			var trigger = CreateWorkflowItemForTemplate(template);
			Assert("Precondition", trigger is IUniversalTemplateTrigger);
			trigger.TriggerConditions_ForBinding.TriggerEventCode = Events.CustomisableEvent00Code;

			var triggerAction = trigger.CompletionTriggerActionsCollection().AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ScheduleDelayedEvent;

			Factory.Save();

			var dummyBO = Factory.New<DummyWithWorkflow>();
			var jobTriggerBO = dummyBO.WorkflowItems.TriggersIncludingRelated.SingleOrDefault();
			AssertNotNull(jobTriggerBO);
			var jobTrigger = (ProcessTask)jobTriggerBO;
			Assert("Precondition: representation of a universal trigger", jobTrigger.IsNonPersistedRepresentationOfTemplateTrigger);
			Assert(!(jobTrigger is IUniversalTemplateTrigger));

			var jobTriggerAction = jobTrigger.ProcessTaskNotifications.SingleOrDefault();
			AssertNotNull(jobTriggerAction);
			AssertEquals(WorkflowTriggerActionTypeConstants.Codes.ScheduleDelayedEvent, jobTriggerAction.PQ_TriggerType);

			dummyBO.RunPreSaveValidation();
			AssertNoNotifications(jobTriggerAction.PQ_TriggerTypeInfo);
		}
	}

	public abstract class ProcessTaskNotificationValidationTestCase<TWorkflowItem> : BusinessObjectValidationTestCase
		where TWorkflowItem : IBaseTrigger
	{
		public void TestCustomsValidationIsAllowedAndRequiresAnEmailAddress()
		{
			var workFlowDescriptor = DummyWorkflowDescriptor.Instance;
			{
				var dummyBO = Factory.New<DummyWithWorkflow>();
				var trigger = CreateWorkflowItemForJob(dummyBO);
				trigger.TriggerConditions_ForBinding.TriggerEventCode = Events.AuthorisedCode;

				foreach (var code in workFlowDescriptor.GetSupportsValidateForCustomsMessagingTriggerActions(trigger, trigger.GetJob()))
				{
					var triggerAction = trigger.CompletionTriggerActionsCollection().AddNew();
					triggerAction.PQ_TriggerType = code;
					AssertNoErrors(triggerAction.PQ_TriggerTypeInfo);

					AssertEquals("triggerAction.PQ_Calc_TriggerPartyInfo.ReadOnly", false, triggerAction.PQ_Calc_TriggerPartyInfo.ReadOnly);
					triggerAction.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;
					AssertNoErrors(triggerAction.PQ_TriggerTypeInfo);
					AssertHasErrors(triggerAction.PQ_Calc_TriggerPartyInfo);

					triggerAction.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
					AssertNoErrors(triggerAction.PQ_TriggerTypeInfo);
					AssertNoErrors(triggerAction.PQ_Calc_TriggerPartyInfo);

					AssertEquals("triggerAction.PQ_EmailAddrInfo.ReadOnly", false, triggerAction.PQ_EmailAddrInfo.ReadOnly);
					triggerAction.PQ_EmailAddr = "roger@ramjet.com";
					AssertNoErrors(triggerAction.PQ_EmailAddrInfo);

					triggerAction.PQ_EmailAddr = "";
					AssertHasErrors(triggerAction.PQ_EmailAddrInfo);

					triggerAction.PQ_EmailAddr = "bill@ted.nom";
					AssertNoErrors(triggerAction.PQ_TriggerTypeInfo);
					AssertNoErrors(triggerAction.PQ_Calc_TriggerPartyInfo);
					AssertNoErrors(triggerAction.PQ_EmailAddrInfo);

					triggerAction.Validation.ValidateAll();
					AssertNoErrors(triggerAction);
				}
			}
		}

		public void TestCannotSendAUniversalEventWithoutAnEventCodeOnTheMilestone()
		{
			var shipment = (IWorkflowProvider)Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingShipment>());
			var milestone = CreateWorkflowItemForJob(shipment, Constants.Workflow.MilestoneType);
			milestone.TriggerConditions_ForBinding.TriggerEventCode = "";

			const string expectedErrorMessage = "Cannot have a Universal Event trigger type where there is no Event Code for the Milestone.";

			var triggerAction = milestone.CompletionTriggerActionsCollection().AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML;
			AssertHasError(triggerAction.PQ_TriggerTypeInfo, expectedErrorMessage);

			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
			AssertNoError(triggerAction.PQ_TriggerTypeInfo, expectedErrorMessage);

			milestone.TriggerConditions_ForBinding.TriggerEventCode = Events.AuthorisedCode;
			AssertNoError(triggerAction.PQ_TriggerTypeInfo, expectedErrorMessage);

			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML;
			AssertNoError(triggerAction.PQ_TriggerTypeInfo, expectedErrorMessage);

			milestone.TriggerConditions_ForBinding.TriggerEventCode = "";
			AssertHasError(triggerAction.PQ_TriggerTypeInfo, expectedErrorMessage);

			milestone.TriggerConditions_ForBinding.TriggerEventCode = Events.AuthorisedCode;
			AssertNoError(triggerAction.PQ_TriggerTypeInfo, expectedErrorMessage);
		}

		public void TestValidationSuspendionOnProperties()
		{
			#region incorrectEmailHtml
			const string incorrectEmailHtml = @"<p>This html doesn't have the closing tr tag on the tableheadings class</p>
<table class=''table''>
<tr class=''tableheadings''><th><b>Shipment Number</b></th>
<tr><td>S00000001</td></tr>
</table>";
			#endregion
			AssertValidationSuspendedOnProperty("PQ_EmailTextFallbackToTemplateInfo",
				incorrectEmailHtml, WorkflowTriggerActionTypeConstants.Codes.NotificationEmail);

			AssertValidationSuspendedOnProperty("PQ_FieldNameInfo",
				"<JS_WrongField>", WorkflowTriggerActionTypeConstants.Codes.SetField);

			var isTemplate = typeof(TWorkflowItem) == typeof(ITemplateTrigger);
			if (!isTemplate)
			{
				AssertValidationSuspendedOnProperty("PQ_FieldValueInfo",
					"<__DataSource>", WorkflowTriggerActionTypeConstants.Codes.SetField);
			}
		}

		public void AssertValidationSuspendedOnProperty(string propertyName, string invalidPropertyValue, string triggerType)
		{
			AssertSuspendedIfNoChanges(propertyName, invalidPropertyValue, triggerType, true, true);
			AssertSuspendedIfNoChanges(propertyName, invalidPropertyValue, triggerType, true, false);
			AssertSuspendedIfNoChanges(propertyName, invalidPropertyValue, triggerType, false, true);
			AssertSuspendedIfNoChanges(propertyName, invalidPropertyValue, triggerType, false, false);
		}

		public void AssertSuspendedIfNoChanges(string propertyName, string propertyValue, string triggerType, bool hasChanges, bool isInDatabase)
		{
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingShipment>());
			var milestone = CreateWorkflowItemForJob((IWorkflowProvider)shipment);
			milestone.TriggerConditions_ForBinding.TriggerEventCode = Events.CustomisableEvent00Code;

			Factory.SuspendValidation();

			var triggerAction = milestone.CompletionTriggerActionsCollection().AddNew();
			triggerAction.PQ_TriggerType = triggerType;

			var propertyInfo = triggerAction.GetPropertyValue(propertyName);
			var zPropertyInfo = (ZPropertyInfo)propertyInfo;
			zPropertyInfo.SetValueFromString(propertyValue);

			Factory.ResumeValidation();

			var assertionMessageCase = $"HasChanges = {hasChanges} & IsInDatabase = {isInDatabase}";

			AssertNoNotifications($"Pre-validation: {assertionMessageCase}",
				zPropertyInfo);

			if (isInDatabase)
			{
				Factory.Save();
			}
			triggerAction.HasChanges = hasChanges;

			triggerAction.Validation.ValidateAll();

			if (hasChanges || !isInDatabase)
			{
				AssertHasNotifications($"Post-validation: {assertionMessageCase}",
					zPropertyInfo);
			}
			else
			{
				AssertNoNotifications($"Post-validation: {assertionMessageCase}",
					zPropertyInfo);
			}
		}

		public void TestCannotPairDDATriggerWithEDCAction()
		{
			var shipment = (IWorkflowProvider)Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingShipment>());
			var trigger = CreateWorkflowItemForJob(shipment);
			trigger.TriggerConditions_ForBinding.TriggerEventCode = Events.DocumentAllocatedCode;

			const string expectedErrorMessage = "Pairing document-allocated(DDA) trigger-type with add-document-to-eDoc(EDC) completion-trigger-action may cause unnecessary documents to be added.";

			var triggerAction = trigger.CompletionTriggerActionsCollection().AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.AddDocumentToEDocs;
			AssertHasWarning(triggerAction.PQ_TriggerTypeInfo, expectedErrorMessage);

			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
			AssertNoWarning(triggerAction.PQ_TriggerTypeInfo, expectedErrorMessage);
		}

		public void TestCannotSendAUniversalEventCollectionWithoutAnEventCodeOnTheMilestone()
		{
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingShipment>());
			var milestone = CreateWorkflowItemForJob((IWorkflowProvider)shipment);
			milestone.TriggerConditions_ForBinding.TriggerEventCode = "";

			const string expectedErrorMessage = "Cannot have a Universal Event Collection trigger type where there is no Event Code for the Milestone.";

			var triggerAction = milestone.CompletionTriggerActionsCollection().AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventCollectionXML;
			AssertHasError(triggerAction.PQ_TriggerTypeInfo, expectedErrorMessage);

			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
			AssertNoError(triggerAction.PQ_TriggerTypeInfo, expectedErrorMessage);

			milestone.TriggerConditions_ForBinding.TriggerEventCode = Events.AuthorisedCode;
			AssertNoError(triggerAction.PQ_TriggerTypeInfo, expectedErrorMessage);

			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventCollectionXML;
			AssertNoError(triggerAction.PQ_TriggerTypeInfo, expectedErrorMessage);

			milestone.TriggerConditions_ForBinding.TriggerEventCode = "";
			AssertHasError(triggerAction.PQ_TriggerTypeInfo, expectedErrorMessage);

			milestone.TriggerConditions_ForBinding.TriggerEventCode = Events.AuthorisedCode;
			AssertNoError(triggerAction.PQ_TriggerTypeInfo, expectedErrorMessage);
		}

		public void TestEDIMessageDeliveryContextSelectors()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			var action = trigger.CompletionTriggerActionsCollection().AddNew();

			var contextSelector = Factory.New<IEDIMessageDeliveryContextSelector>();
			contextSelector.ECS_Code = "123";
			contextSelector.ECS_Description = "123";
			contextSelector.ECS_ProcessType = WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode;

			var contextSelector2 = Factory.New<IEDIMessageDeliveryContextSelector>();
			contextSelector2.ECS_Code = "124";
			contextSelector2.ECS_Description = "124";
			contextSelector2.ECS_ProcessType = WorkflowDescriptors.DummyWorkflowDescriptorCode;

			action.PQ_ECS_MessageDeliveryContextSelector = ZGuid.Empty;
			AssertNoErrors(action.PQ_ECS_MessageDeliveryContextSelectorInfo);

			action.PQ_ECS_MessageDeliveryContextSelector = (contextSelector as BusinessObject).PK;
			AssertListValidationInvalidCodeError(action.PQ_ECS_MessageDeliveryContextSelectorInfo, true);

			action.PQ_ECS_MessageDeliveryContextSelector = (contextSelector2 as BusinessObject).PK;
			AssertNoErrors(action.PQ_ECS_MessageDeliveryContextSelectorInfo);
		}

		public void TestEDIMessageDeliveryContextSelectors_Template()
		{
			var template = Factory.New<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.DummyWorkflowDescriptorCode;
			var trigger = template.WorkflowItems.Triggers.AddNew();
			var action = trigger.CompletionTriggerActionsCollection().AddNew();

			var contextSelector = Factory.New<IEDIMessageDeliveryContextSelector>();
			contextSelector.ECS_Code = "123";
			contextSelector.ECS_Description = "123";
			contextSelector.ECS_ProcessType = WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode;

			var contextSelector2 = Factory.New<IEDIMessageDeliveryContextSelector>();
			contextSelector2.ECS_Code = "124";
			contextSelector2.ECS_Description = "124";
			contextSelector2.ECS_ProcessType = WorkflowDescriptors.DummyWorkflowDescriptorCode;

			action.PQ_ECS_MessageDeliveryContextSelector = ZGuid.Empty;
			AssertNoErrors(action.PQ_ECS_MessageDeliveryContextSelectorInfo);

			action.PQ_ECS_MessageDeliveryContextSelector = (contextSelector as BusinessObject).PK;
			AssertListValidationInvalidCodeError(action.PQ_ECS_MessageDeliveryContextSelectorInfo, true);

			action.PQ_ECS_MessageDeliveryContextSelector = (contextSelector2 as BusinessObject).PK;
			AssertNoErrors(action.PQ_ECS_MessageDeliveryContextSelectorInfo);

			template.P0_ProcessType = WorkflowDescriptors.OrgHeaderWorkflowDescriptorCode;
			action.PQ_ECS_MessageDeliveryContextSelector = (contextSelector as BusinessObject).PK;
			AssertNoErrors(action.PQ_ECS_MessageDeliveryContextSelectorInfo);
		}

		public void TestCheckPQ_SQ()
		{
			ProcessTaskNotification ptNotification = NewProcessTaskNotification();
			ptNotification.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Print;
			Enterprise.Integration.DocumentEngine.IStmPrintQueue printQueue = (Enterprise.Integration.DocumentEngine.IStmPrintQueue)ptNotification.Lookups.PrintQueues.AddNew();
			printQueue.QueueName = "TEST";

			ptNotification.PQ_SQ = ZGuid.NewZGuid();
			AssertEquals(true, ptNotification.PQ_SQInfo.HasNotifications());

			ptNotification.PQ_SQ = printQueue.PK;
			AssertNoError(ptNotification.PQ_SQInfo, ProcessTaskNotificationValidation.InvalidPrinter);

			ptNotification.PQ_SQ = ZGuid.Empty;
			AssertHasError(ptNotification.PQ_SQInfo, ProcessTaskNotificationValidation.InvalidPrinter);

			ptNotification.PQ_Calc_TriggerParty = "";
			ptNotification.Validation.ValidatePQ_SQ();
			AssertNoError(ptNotification.PQ_SQInfo, ProcessTaskNotificationValidation.InvalidPrinter);
		}

		public void TestPQ_MessagePurpose()
		{
			ProcessTaskNotification ptNotification = NewProcessTaskNotification();
			ptNotification.PQ_MessagePurpose = "ZUB";
			AssertHasErrors(ptNotification.PQ_MessagePurposeInfo);

			ptNotification.PQ_MessagePurpose = ptNotification.Lookups.ProcessTaskTriggerPurposeList[0].Code;
			AssertNoErrors(ptNotification.PQ_MessagePurposeInfo);

			ptNotification.PQ_MessagePurpose = "";
			AssertNoErrors(ptNotification.PQ_MessagePurposeInfo);
		}

		public void TestPQ_MacroTypeCode()
		{
			using (WorkflowDataRegistry.Instance.FeatureFlagMacroEnhancements.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				ProcessTaskNotification ptNotification = NewProcessTaskNotification();
				ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;

				ptNotification.PQ_MacroTypeCode = "ZUB";
				AssertHasErrors(ptNotification.PQ_MacroTypeCodeInfo);

				ptNotification.PQ_MacroTypeCode = ptNotification.Lookups.MacroTypeCodeList[0].Code;
				AssertNoErrors(ptNotification.PQ_MacroTypeCodeInfo);

				ptNotification.PQ_MacroTypeCode = "";
				AssertNoErrors(ptNotification.PQ_MacroTypeCodeInfo);
			}
		}

		public void TestPQ_SU_Document()
		{
			ProcessTaskNotification ptNotification = NewProcessTaskNotification();
			ptNotification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendDocument;
			AssertEquals("Precondition", false, ptNotification.PQ_SU_DocumentInfo.ReadOnly);
			ptNotification.Validation.ValidatePQ_SU_Document();
			AssertHasErrors(ptNotification.PQ_SU_DocumentInfo);

			StmMenuItem menuItemWithAutoDeliver = Factory.New<StmMenuItem>();
			menuItemWithAutoDeliver.SU_MenuName = "TEST";
			menuItemWithAutoDeliver.SU_BusinessContext = "TEST";
			menuItemWithAutoDeliver.SU_PreventAutoDelivery = false;
			menuItemWithAutoDeliver.SU_ContactType = ContactType.Consignee.Code;

			ptNotification.PQ_SU_Document = menuItemWithAutoDeliver.PK;
			AssertHasError(ptNotification.PQ_SU_DocumentInfo, "Enter a valid Document.");

			ptNotification.PQ_TriggerType = "foo";
			ptNotification.PQ_SU_Document = ZGuid.Empty;
			ptNotification.Validation.ValidatePQ_SU_Document();
			AssertNoErrors("Should not prohibit leaving PQ_SU_Document blank when it is not required", ptNotification.PQ_SU_DocumentInfo);
		}

		[ExpectNoExceptions()]
		public void TestPQ_SU_Document_DoesNotBlowUpWhenDocumentIsNull()
		{
			var bizO = Factory.New<DummyWithWorkflowDocumentSupportable>();
			var trigger = bizO.WorkflowItems.Triggers.AddNew();

			var notification = trigger.CompletionTriggerActionsCollection().AddNew();

			notification.PQ_P9 = trigger.PK;
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendDocument;
			notification.PQ_SU_Document = ZGuid.Empty;

			notification.Validation.ValidatePQ_SU_Document();
		}

		public void TestCheckDocumentDataState()
		{
			var bizo = Factory.New<CreditControlledWorkflowProviderBizo>();
			var processTask = bizo.WorkflowItems.AddNew();

			var creditControlledDocument = Factory.NewWithValidTestData<StmMenuItem>();
			creditControlledDocument.SU_MenuName = "TEST";
			creditControlledDocument.SU_BusinessContext = "TEST";
			creditControlledDocument.SU_ContactType = ContactType.Consignee.Code;

			var notification = Factory.NewWithValidTestData<ProcessTaskNotificationForTest>();
			notification.PQ_P9 = processTask.PK;
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.AddDocumentToEDocs;
			notification.PQ_SU_Document = creditControlledDocument.PK;

			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(CreditControlledWorkflowProviderBizo);
			DummyWorkflowDescriptor.Instance.OverriddenWorkflowProviderType = typeof(CreditControlledWorkflowProviderBizo);
			DummyWorkflowDescriptor.Instance.SetDocumentBusinessContext(Array.Empty<BusinessContext>());
			notification.Validation.ValidatePQ_SU_Document();
			AssertNoErrors(notification.PQ_SU_DocumentInfo);
			AssertNoWarnings(notification.PQ_SU_DocumentInfo);

			var orgHeader = CreditControlledBizo.CreateOrganization(Factory);
			CreditControlledBizo.SetOrganizationCreditOnHold(orgHeader, true);
			bizo.OrganisationsForCreditChecksForTest = new[] { orgHeader };

			creditControlledDocument.SU_DeliveryRestrictionType = nameof(DeliveryRestrictionType.CNH);
			notification.Validation.ValidatePQ_SU_Document();
			AssertNoErrors(notification.PQ_SU_DocumentInfo);
			AssertHasWarning(notification.PQ_SU_DocumentInfo, @"Delivery of this document is restricted because:
       The Consignee, Consignor or Local Client for Billing
	  a) Have at least one or more outstanding transactions that have fulfilled the restriction
	      set in the Credit Controlled Documents Check Registry, OR
	  b) Is at or above their Credit Limit, OR
	  c) Has been put on Credit Hold.");
		}

		[ExpectNoExceptions()]
		public void TestCheckDocumentDataState_DoesNotBlowUpWhenDocumentSupporterDataStateIsNull()
		{
			CreditControlledWorkflowProviderBizo bizo = Factory.New<CreditControlledWorkflowProviderBizo>();
			bizo.DocumentSupporter = new DocumentSupporterDodgyThatReturnsNullForGetDataStateBeforeRun(bizo);

			ProcessTask processTask = bizo.WorkflowItems.AddNew();

			StmMenuItem creditControlledDocument = Factory.NewWithValidTestData<StmMenuItem>();
			creditControlledDocument.SU_MenuName = "TEST";
			creditControlledDocument.SU_BusinessContext = "TEST";
			creditControlledDocument.SU_ContactType = ContactType.Consignee.Code;

			ProcessTaskNotificationForTest notification = Factory.NewWithValidTestData<ProcessTaskNotificationForTest>();
			notification.PQ_P9 = processTask.PK;
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendDocument;
			notification.PQ_SU_Document = creditControlledDocument.PK;

			notification.Validation.ValidatePQ_SU_Document();
		}

		public void TestCheckDocumentGroup()
		{
			const string expectedWarningMessage = "Can't get Document Delivery Contact for Document Group \"CNE\", auto delivery will not run for this document.";

			var bizo = Factory.New<CreditControlledWorkflowProviderBizo>();
			var trigger = bizo.WorkflowItems.Triggers.AddNew();

			var document = Factory.NewWithValidTestData<StmMenuItem>();
			document.SU_MenuName = "TEST";
			document.SU_BusinessContext = "TEST";
			document.SU_ContactType = ContactType.Consignee.Code;
			document.SU_DocumentDirection = "DEP";

			var triggerAction = trigger.CompletionTriggerActionsCollection().AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendDocument;
			triggerAction.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.AutoDocumentDelivery;
			triggerAction.PQ_SU_Document = document.PK;

			AssertHasWarning(triggerAction.PQ_SU_DocumentInfo, expectedWarningMessage);

			var triggerAction2 = trigger.CompletionTriggerActionsCollection().AddNew();
			triggerAction2.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.AddDocumentToEDocs;
			triggerAction2.PQ_SU_Document = document.PK;

			AssertNoWarning(triggerAction2.PQ_SU_DocumentInfo, expectedWarningMessage);

			bizo.ConsigneeDocumentaryAddress.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;

			triggerAction.Validation.ValidatePQ_SU_Document();
			AssertNoWarning(triggerAction.PQ_SU_DocumentInfo, expectedWarningMessage);
		}

		public void TestCheckDocumentIsApplicable()
		{
			const string expectedWarningMessage = "The document may not be applicable, auto delivery may not run for this document. Filter: \"<CurrentCompany.Country.Code>\" == \"DE\" && \"<Z0_Code>\" == \"ZCD\"";

			var bizo = Factory.New<CreditControlledWorkflowProviderBizo>();
			var trigger = bizo.WorkflowItems.Triggers.AddNew();

			var document = Factory.NewWithValidTestData<StmMenuItem>();
			document.SU_MenuName = "TEST";
			document.SU_BusinessContext = "TEST";
			document.SU_FilterList = "";
			document.SU_ContactType = ContactType.Consignee.Code;
			document.SU_DocumentDirection = "DEP";

			var triggerAction = trigger.CompletionTriggerActionsCollection().AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendDocument;
			triggerAction.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			triggerAction.PQ_SU_Document = document.PK;

			AssertNoWarning(triggerAction.PQ_SU_DocumentInfo, expectedWarningMessage);

			document.SU_FilterList = "\"<CurrentCompany.Country.Code>\" == \"DE\" && \"<Z0_Code>\" == \"ZCD\"";
			triggerAction.Validation.ValidatePQ_SU_Document();
			AssertHasWarning(triggerAction.PQ_SU_DocumentInfo, expectedWarningMessage);

			bizo.Z0_Code = "ZCD";
			triggerAction.Validation.ValidatePQ_SU_Document();
			AssertHasWarning(triggerAction.PQ_SU_DocumentInfo, expectedWarningMessage);

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = "DE";
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = company.PK;
			branch.GB_RN_NKCountryCode = "DE";
			Factory.Save();

			var baseTrigger = triggerAction.Parent;
			baseTrigger.TriggerContextCode = TriggerUserContextList.Codes.Specified;
			baseTrigger.TriggerBranch = branch.PK;

			bizo.Z0_Code = "";
			triggerAction.Validation.ValidatePQ_SU_Document();
			AssertHasWarning(triggerAction.PQ_SU_DocumentInfo, expectedWarningMessage);

			bizo.Z0_Code = "ZCD";
			triggerAction.Validation.ValidatePQ_SU_Document();
			AssertNoWarning(triggerAction.PQ_SU_DocumentInfo, expectedWarningMessage);
		}

		public void TestCheckDocumentGroupWarningMessageOnlyAppearWhenRecipientIsAutoDelivery()
		{
			const string expectedWarningMessage = "Can't get Document Delivery Contact for Document Group \"CNE\", auto delivery will not run for this document.";

			var bizo = Factory.New<CreditControlledWorkflowProviderBizo>();
			var trigger = bizo.WorkflowItems.Triggers.AddNew();

			var document = Factory.NewWithValidTestData<StmMenuItem>();
			document.SU_MenuName = "TEST_MenuName";
			document.SU_BusinessContext = "TEST_BusinessContext";
			document.SU_ContactType = ContactType.Consignee.Code;
			document.SU_DocumentDirection = "DEP";

			var triggerAction = trigger.CompletionTriggerActionsCollection().AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendDocument;
			triggerAction.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.AutoDocumentDelivery;
			triggerAction.PQ_SU_Document = document.PK;

			AssertHasWarning(triggerAction.PQ_SU_DocumentInfo, expectedWarningMessage);

			var triggerAction2 = trigger.CompletionTriggerActionsCollection().AddNew();
			triggerAction2.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendDocument;
			triggerAction.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			triggerAction2.PQ_SU_Document = document.PK;

			AssertNoWarning(triggerAction2.PQ_SU_DocumentInfo, expectedWarningMessage);

			bizo.ConsigneeDocumentaryAddress.OrganisationPK = Factory.NewWithValidTestData<OrgHeader>().PK;

			triggerAction.Validation.ValidatePQ_SU_Document();
			AssertNoWarning(triggerAction.PQ_SU_DocumentInfo, expectedWarningMessage);
		}

		public void TestPQ_Calc_TriggerParty()
		{
			ProcessTaskNotification ptNotification = NewProcessTaskNotificationWithOrgOpportunity();
			Assert("Precondition: PQ_Calc_TriggerParty should not be read-only to be validated", !ptNotification.PQ_Calc_TriggerPartyInfo.ReadOnly);
			ptNotification.PQ_Calc_TriggerParty = "ZUB";
			AssertHasErrors(ptNotification.PQ_Calc_TriggerPartyInfo);

			ptNotification.PQ_Calc_TriggerParty = ptNotification.Lookups.MessagingTriggerPartiesList[0].Code;
			AssertNoErrors(ptNotification.PQ_Calc_TriggerPartyInfo);

			ptNotification.PQ_Calc_TriggerParty = "";
			AssertHasErrors(ptNotification.PQ_Calc_TriggerPartyInfo);
		}

		public void TestPQ_Calc_TriggerParty_WhenJobLevelWorkflowGroupSelected_AndBufferManagementNotEnabledForWorkflowType_ShouldHaveWarning()
		{
			var notification = NewProcessTaskNotificationWithOrgOpportunity();
			AssertTriggerParty_WhenJobLevelWorkflowGroupSelected_AndBufferManagementNotEnabledForWorkflowType_ShouldHaveWarning(notification);
		}

		public void TestPQ_Calc_TriggerParty_ForTemplate_WhenJobLevelWorkflowGroupSelected_AndBufferManagementNotEnabledForWorkflowType_ShouldHaveWarning()
		{
			var template = Factory.New<ProcessTaskTemplate>();
			template.P0_ProcessType = "OPP";
			var trigger = MasterFilesTestHelper.CreateTrigger(template, string.Empty);
			var notification = trigger.ProcessTaskNotifications.AddNew();
			AssertTriggerParty_WhenJobLevelWorkflowGroupSelected_AndBufferManagementNotEnabledForWorkflowType_ShouldHaveWarning(notification);
		}

		void AssertTriggerParty_WhenJobLevelWorkflowGroupSelected_AndBufferManagementNotEnabledForWorkflowType_ShouldHaveWarning(ProcessTaskNotification notification)
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.DisableBMSInRegistry(); // Should be by default, but just in case.
			const string expectedWarning = "Buffer Management is not enabled for this job type. The Job-level Workflow Group option is only valid for jobs associated with a Buffer Management System. Fallback logic will be used to determine an alternative recipient.";

			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			notification.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.JobLevelWorkflowGroup;
			AssertHasWarning("Buffer Management is not enabled, so the warning should be shown. SAD!", notification.PQ_Calc_TriggerPartyInfo, expectedWarning);

			helper.EnableBMSInRegistry();
			notification.Validation.ValidatePQ_Calc_TriggerParty();
			AssertHasWarning("Buffer Management is enabled, but there is no system for the parent workflow type, so the warning should still be shown. SAD!", notification.PQ_Calc_TriggerPartyInfo, expectedWarning);

			helper.CreateSystem(Factory, "OPP");
			notification.Validation.ValidatePQ_Calc_TriggerParty();
			AssertNoWarning("The warning should not be shown because Buffer Management is enabled and the workflow type is associated with a Buffer Management System. SAD!", notification.PQ_Calc_TriggerPartyInfo, expectedWarning);
		}

		public void TestCheckPQ_TriggerParty()
		{
			var ptNotification = NewProcessTaskNotificationWithOrgOpportunity();
			ptNotification.PQ_TriggerParty = ZString.Empty;
			AssertNoErrors(ptNotification.PQ_TriggerPartyInfo);
			ptNotification.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.SpecialCodes.Other;
			AssertHasErrorContaining(ptNotification.PQ_TriggerPartyInfo, MandatoryValidation.MustBeEntered);
			ptNotification.PQ_TriggerParty = "!@";
			AssertNoErrorContaining(ptNotification.PQ_TriggerPartyInfo, MandatoryValidation.MustBeEntered);
			AssertHasErrorContaining(ptNotification.PQ_TriggerPartyInfo, ListValidation.InvalidCodeError);
			ptNotification.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;
			AssertNoErrorContaining(ptNotification.PQ_TriggerPartyInfo, MandatoryValidation.MustBeEntered);
			AssertNoErrorContaining(ptNotification.PQ_TriggerPartyInfo, ListValidation.InvalidCodeError);
		}

		public void TestCheckPQ_TriggerPartyService_Template()
		{
			DummyWorkflowDescriptor.Instance.SupportedTriggerPartyServicesExposed = new ZString[] { "ABC" };

			var dummyTemplate = Factory.New<ProcessTaskTemplate>();
			dummyTemplate.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
			dummyTemplate.P0_GC = GlbCompany.CurrentCompany.PK;
			var trigger = dummyTemplate.WorkflowItems.Triggers.AddNew();
			var triggerAction = trigger.CompletionTriggerActionsCollection().AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
			triggerAction.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;
			triggerAction.PQ_TriggerPartyService = "";
			AssertHasErrorContaining(triggerAction.PQ_TriggerPartyServiceInfo, MandatoryValidation.MustBeEntered);

			DummyWorkflowDescriptor.Instance.SupportedTriggerPartyServicesExposed = new ZString[] { "", "ABC" };
			triggerAction.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.NotifyParty; // new cached list
			triggerAction.Validation.ValidatePQ_TriggerPartyService();
			AssertNoErrors("A TriggerPartyService is not mandatory if it supports BLANK.", triggerAction.PQ_TriggerPartyServiceInfo);

			triggerAction.PQ_TriggerPartyService = "!@";
			AssertHasErrorContaining(triggerAction.PQ_TriggerPartyServiceInfo, ListValidation.InvalidCodeError);

			triggerAction.PQ_TriggerPartyService = "ABC";
			AssertNoErrors("ABC is a valid TriggerPartyService.", triggerAction.PQ_TriggerPartyServiceInfo);
		}

		public void TestCheckPQ_TriggerPartyService_Job()
		{
			DummyWorkflowDescriptor.Instance.SupportedTriggerPartyServicesExposed = new ZString[] { "ABC" };

			var dummy = Factory.New<DummyWithWorkflow>();
			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			var triggerAction = trigger.CompletionTriggerActionsCollection().AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
			triggerAction.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;
			triggerAction.PQ_TriggerPartyService = "";
			AssertHasErrorContaining(triggerAction.PQ_TriggerPartyServiceInfo, MandatoryValidation.MustBeEntered);

			DummyWorkflowDescriptor.Instance.SupportedTriggerPartyServicesExposed = new ZString[] { "", "ABC" };
			triggerAction.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.NotifyParty; // new cached list
			triggerAction.Validation.ValidatePQ_TriggerPartyService();
			AssertNoErrors("A TriggerPartyService is not mandatory if it supports BLANK.", triggerAction.PQ_TriggerPartyServiceInfo);

			triggerAction.PQ_TriggerPartyService = "!@";
			AssertHasErrorContaining(triggerAction.PQ_TriggerPartyServiceInfo, ListValidation.InvalidCodeError);

			triggerAction.PQ_TriggerPartyService = "ABC";
			AssertNoErrors("ABC is a valid TriggerPartyService.", triggerAction.PQ_TriggerPartyServiceInfo);
		}

		public void TestCheckPQ_TriggerPartyWarningOnSpecialPartyType()
		{
			var dummy = Factory.New<DummyWithWorkflow>();

			var trigger = dummy.WorkflowItems.Triggers.AddNew();

			var triggerAction = trigger.CompletionTriggerActionsCollection().AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
			triggerAction.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.SpecialCodes.Other;

			triggerAction.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;
			triggerAction.Validation.ValidatePQ_TriggerParty();
			AssertNoErrors("OrgProxy is valid", triggerAction.PQ_TriggerPartyInfo);

			foreach (var code in MessageRecipientPartyTypeList.SpecialPartyTypes.GetAllCodes())
			{
				triggerAction.PQ_TriggerParty = code;
				triggerAction.Validation.ValidatePQ_TriggerParty();
				AssertHasWarning(triggerAction.PQ_TriggerPartyInfo, $"{code} is not a valid Recipient.");
			}

			triggerAction.PQ_TriggerParty = "!@";
			triggerAction.Validation.ValidatePQ_TriggerParty();
			AssertHasErrorContaining(triggerAction.PQ_TriggerPartyInfo, ListValidation.InvalidCodeError);
		}

		public void TestCheckPQ_TriggerPartyCannotBeSpecialPartyTypeIfTemplate()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			var trigger = (ITemplateTrigger)template.TemplateTriggers.AddNew();

			var triggerAction = trigger.CompletionTriggerActionsCollection().AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
			triggerAction.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.SpecialCodes.Other;

			triggerAction.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;
			triggerAction.Validation.ValidatePQ_TriggerParty();
			AssertNoErrors("OrgProxy is valid", triggerAction.PQ_TriggerPartyInfo);

			foreach (var code in MessageRecipientPartyTypeList.SpecialPartyTypes.GetAllCodes())
			{
				triggerAction.PQ_TriggerParty = code;
				triggerAction.Validation.ValidatePQ_TriggerParty();
				AssertHasErrorContaining(triggerAction.PQ_TriggerPartyInfo, ListValidation.InvalidCodeError);
			}

			triggerAction.PQ_TriggerParty = "!@";
			triggerAction.Validation.ValidatePQ_TriggerParty();
			AssertHasErrorContaining(triggerAction.PQ_TriggerPartyInfo, ListValidation.InvalidCodeError);
		}

		public void TestCheckPQ_OH_Recipient()
		{
			var ptNotification = NewProcessTaskNotificationWithOrgOpportunity();
			AssertNoErrorContaining(ptNotification.PQ_OH_RecipientInfo, MandatoryValidation.MustBeEntered);
			ptNotification.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;
			AssertNoErrorContaining(ptNotification.PQ_OH_RecipientInfo, MandatoryValidation.MustBeEntered);
			ptNotification.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.SpecialCodes.Other;
			AssertHasErrorContaining(ptNotification.PQ_OH_RecipientInfo, MandatoryValidation.MustBeEntered);
			ptNotification.PQ_OH_Recipient = Factory.NewWithValidTestData<OrgHeader>().PK;
			AssertNoErrorContaining(ptNotification.PQ_OH_RecipientInfo, MandatoryValidation.MustBeEntered);
		}

		public void TestPQ_TriggerType()
		{
			ProcessTaskNotification ptNotification = NewProcessTaskNotificationWithOrgOpportunity();
			ptNotification.PQ_TriggerType = "ZUB";
			AssertHasErrors(ptNotification.PQ_TriggerTypeInfo);

			ptNotification.PQ_TriggerType = ((CodeDescriptionPairList)ptNotification.Lookups.WorkflowTriggerActionTypes)[0].Code;
			AssertNoErrors(ptNotification.PQ_TriggerTypeInfo);

			ptNotification.PQ_TriggerType = "";
			AssertHasErrors(ptNotification.PQ_TriggerTypeInfo);
		}

		public void TestCheckPQ_EmailAddr()
		{
			var dummyBO = Factory.New<DummyWithWorkflow>();
			var trigger = dummyBO.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Hap";
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			var triggerAction = trigger.CompletionTriggerActionsCollection().AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendDocument;

			triggerAction.Parent.Description = "BingBong";
			triggerAction.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			triggerAction.PQ_EmailAddr = "dongdong@dong.com";
			AssertNoError(triggerAction.PQ_EmailAddrInfo, GetEmailAddressIsRequiredMessage(MessageRecipientPartyTypeList.Codes.Email));
			AssertNoError(triggerAction.PQ_EmailAddrInfo, ProcessTaskNotificationValidation.EmailAddressIsInvalid);

			triggerAction.PQ_EmailAddr += ", dingding@ding.com,dingdong@dang.com";
			AssertNoError(triggerAction.PQ_EmailAddrInfo, GetEmailAddressIsRequiredMessage(MessageRecipientPartyTypeList.Codes.Email));
			AssertNoError(triggerAction.PQ_EmailAddrInfo, ProcessTaskNotificationValidation.EmailAddressIsInvalid);

			triggerAction.PQ_EmailAddr = "(*GetUserEmailWhoRaisedEvent(\"IFL\", \"Something Weird\")*)";
			AssertNoError(triggerAction.PQ_EmailAddrInfo, GetEmailAddressIsRequiredMessage(MessageRecipientPartyTypeList.Codes.Email));
			AssertNoError(triggerAction.PQ_EmailAddrInfo, ProcessTaskNotificationValidation.EmailAddressIsInvalid);

			triggerAction.PQ_EmailAddr = "(*P9_Description*)@mingmingming.ming";
			AssertNoError(triggerAction.PQ_EmailAddrInfo, GetEmailAddressIsRequiredMessage(MessageRecipientPartyTypeList.Codes.Email));
			AssertNoError(triggerAction.PQ_EmailAddrInfo, ProcessTaskNotificationValidation.EmailAddressIsInvalid);

			triggerAction.PQ_EmailAddr = "(*P9_Description*)";
			AssertNoError(triggerAction.PQ_EmailAddrInfo, GetEmailAddressIsRequiredMessage(MessageRecipientPartyTypeList.Codes.Email));
			AssertNoError(triggerAction.PQ_EmailAddrInfo, ProcessTaskNotificationValidation.EmailAddressIsInvalid);
			AssertHasWarning(triggerAction.PQ_EmailAddrInfo, ProcessTaskNotificationValidation.EmailAddressIsInvalidAfterMacroEvaluation);

			triggerAction.PQ_EmailAddr = "dongdong";
			AssertNoError(triggerAction.PQ_EmailAddrInfo, GetEmailAddressIsRequiredMessage(MessageRecipientPartyTypeList.Codes.Email));
			AssertHasError(triggerAction.PQ_EmailAddrInfo, ProcessTaskNotificationValidation.EmailAddressIsInvalid);
			AssertNoWarning(triggerAction.PQ_EmailAddrInfo, ProcessTaskNotificationValidation.EmailAddressIsInvalidAfterMacroEvaluation);

			triggerAction.PQ_EmailAddr = ZString.Empty;
			AssertHasError(triggerAction.PQ_EmailAddrInfo, GetEmailAddressIsRequiredMessage(MessageRecipientPartyTypeList.Codes.Email));
			AssertNoError(triggerAction.PQ_EmailAddrInfo, ProcessTaskNotificationValidation.EmailAddressIsInvalid);

			triggerAction.PQ_EmailAddr = ",";
			AssertHasError(triggerAction.PQ_EmailAddrInfo, ProcessTaskNotificationValidation.EmailAddressIsInvalid);

			triggerAction.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;
			AssertNoError(triggerAction.PQ_EmailAddrInfo, GetEmailAddressIsRequiredMessage(MessageRecipientPartyTypeList.Codes.Email));
			AssertNoError(triggerAction.PQ_EmailAddrInfo, ProcessTaskNotificationValidation.EmailAddressIsInvalid);
		}

		public void TestCheckPQ_EmailAddrWithEmailAddressRetrievable()
		{
			var person = Factory.NewWithValidTestData<GlbPerson>();
			person.PER_EmailAddress = "personal@email.com";
			var notification = person.WorkflowItems.Triggers.AddNew().ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendDocument;
			notification.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.PersonalEmail;
			notification.PQ_EmailAddr = "";
			AssertHasWarning(notification.PQ_EmailAddrInfo, GetEmailAddressIsRequiredMessage(MessageRecipientPartyTypeList.Codes.PersonalEmail));

			notification.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.PersonPrimaryWorkEmail;
			notification.PQ_EmailAddr = "";
			AssertHasWarning(notification.PQ_EmailAddrInfo, GetEmailAddressIsRequiredMessage(MessageRecipientPartyTypeList.Codes.PersonPrimaryWorkEmail));

			notification.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.PersonalFallbackPrimaryWorkEmail;
			notification.PQ_EmailAddr = "";
			AssertHasWarning(notification.PQ_EmailAddrInfo, GetEmailAddressIsRequiredMessage(MessageRecipientPartyTypeList.Codes.PersonalFallbackPrimaryWorkEmail));
		}

		string GetEmailAddressIsRequiredMessage(string triggerParty)
		{
			return $"Please enter an Email Address; an Email Address is required when the recipient is '{triggerParty}'.";
		}

		public void TestCheckPQ_EmailAddr_Template()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			var trigger = (ITemplateTrigger)template.TemplateTriggers.AddNew();
			var triggerAction = trigger.CompletionTriggerActionsCollection().AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXML;
			triggerAction.Parent.Description = "BingBong";
			triggerAction.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;

			// Needs to have data
			triggerAction.PQ_EmailAddr = ZString.Empty;
			AssertHasError(triggerAction.PQ_EmailAddrInfo, GetEmailAddressIsRequiredMessage(MessageRecipientPartyTypeList.Codes.Email));

			// No validation on the contents of an email adress in the template
			triggerAction.PQ_EmailAddr = "XXX";
			AssertNoError(triggerAction.PQ_EmailAddrInfo, GetEmailAddressIsRequiredMessage(MessageRecipientPartyTypeList.Codes.Email));
		}

		public void TestCheckPQ_FieldName()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var trigger = CreateWorkflowItemForJob(dummy);
			var triggerAction = trigger.CompletionTriggerActionsCollection().AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;

			triggerAction.PQ_FieldName = "Z0_Number";
			AssertNoErrors(triggerAction.PQ_FieldNameInfo);

			triggerAction.PQ_FieldName = "Collection.Z0_Number";
			AssertNoErrors(triggerAction.PQ_FieldNameInfo);

			triggerAction.PQ_FieldName = "<Z0_Number>";
			AssertNoErrors(triggerAction.PQ_FieldNameInfo);

			triggerAction.PQ_FieldName = "Z0_WrongField";
			AssertHasWarning(triggerAction.PQ_FieldNameInfo, "Cannot find property Z0_WrongField on Enterprise.MasterFiles.Business.Testing.DummyWithWorkflow.");

			triggerAction.PQ_FieldName = "GetCustomField(Field 1)";
			if (trigger.IsTemplate)
			{
				AssertNoErrors("No validation for GetCustomField in template as there's no job to confirm the field is existed or not at the time", triggerAction.PQ_FieldNameInfo);
			}
			else
			{
				AssertHasWarning(triggerAction.PQ_FieldNameInfo, "Cannot find custom field 'Field 1' on Enterprise.MasterFiles.Business.Testing.DummyWithWorkflow by Set Field trigger action.");
			}

			triggerAction.PQ_FieldName = "OtherMethod(Argument 1)";
			AssertHasWarning(triggerAction.PQ_FieldNameInfo, "Cannot find property OtherMethod(Argument 1) on Enterprise.MasterFiles.Business.Testing.DummyWithWorkflow.");
		}

		public void TestCheckPQ_FieldName_MandatoryValidation()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var trigger = CreateWorkflowItemForJob(dummy);
			var triggerAction = trigger.CompletionTriggerActionsCollection().AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;

			triggerAction.PQ_FieldName = ZString.Empty;
			triggerAction.Validation.ValidatePQ_FieldName();
			AssertHasError(triggerAction.PQ_FieldNameInfo, "Please enter a Field Name.");

			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyTag;
			triggerAction.Validation.ValidatePQ_FieldName();
			AssertNoErrors(triggerAction.PQ_FieldNameInfo);
		}

		public void TestCheckPQ_FieldName_Where()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var trigger = CreateWorkflowItemForJob(dummy);
			var triggerAction = trigger.CompletionTriggerActionsCollection().AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;

			triggerAction.PQ_FieldName = "Collection.Z0_Number";
			AssertNoErrors(triggerAction.PQ_FieldNameInfo);

			triggerAction.PQ_FieldName = "Collection.Where(\"somefield\" == \"value\").Z0_Number";
			AssertNoErrors(triggerAction.PQ_FieldNameInfo);

			triggerAction.PQ_FieldName = "Collection.Where(\"somefield\" == \"value\").RelatedDummy.Collection.Where(\"otherfield\" == \"othervalue\").Z0_Number";
			AssertNoErrors(triggerAction.PQ_FieldNameInfo);

			triggerAction.PQ_FieldName = "Collection.Where().Z0_Number";
			AssertHasError(triggerAction.PQ_FieldNameInfo, "Where clause must be properly formatted.");

			triggerAction.PQ_FieldName = "Collection.Where(\"1\").Z0_Number";
			AssertNoErrors("GIVEN invalid macro 'Result of \"1\" is not a True/False expression' WHEN validating THEN should not error because we validate on running-trigger-action", triggerAction.PQ_FieldNameInfo);
		}

		public void TestCheckPQ_FieldName_Where_TAG()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var jobHeader = BMSTestHelper.CreateJobHeader<DummyWithWorkflow>(Factory);
			jobHeader.FH_ParentId = dummy.PK;
			jobHeader.ProcessHeaders.AddNew();

			Factory.Save();

			var trigger = CreateWorkflowItemForJob(dummy);
			var triggerAction = trigger.CompletionTriggerActionsCollection().AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyTag;

			triggerAction.PQ_FieldName = "<Workflows>";
			AssertNoErrors(triggerAction.PQ_FieldNameInfo);

			triggerAction.PQ_FieldName = "Workflows";
			AssertNoErrors(triggerAction.PQ_FieldNameInfo);

			triggerAction.PQ_FieldName = "Workflows.Where()";
			AssertHasError(triggerAction.PQ_FieldNameInfo, "Where clause must be properly formatted.");

			triggerAction.PQ_FieldName = "Workflows.Where(\"FH_CompletionStatement\"==\"Workflow1\")";
			AssertNoErrors(triggerAction.PQ_FieldNameInfo);

			triggerAction.PQ_FieldName = "Workflows.Where(\"somefield\"==\"somevalue\")";
			AssertNoErrors(triggerAction.PQ_FieldNameInfo);

			triggerAction.PQ_FieldName = "Workflows.Where(\"somefield\"==\"somevalue\").FirstOrDefault(\"somefield\"==\"somevalue\")";
			AssertNoErrors(triggerAction.PQ_FieldNameInfo);

			triggerAction.PQ_FieldName = "Workflows.Where(\"somefield\"==\"somevalue\").FirstOrDefault(\"somefield\"==\"somevalue\")";
			AssertNoErrors(triggerAction.PQ_FieldNameInfo);

			triggerAction.PQ_FieldName = "WorkflowItems";
			AssertHasError(triggerAction.PQ_FieldNameInfo, "Property Enterprise.MasterFiles.Business.Testing.DummyWithWorkflow.WorkflowItems of type DummyProcessTaskCollection cannot be used in 'Apply Tag' trigger action - only IProcessHeaderCollection type is supported.");

			triggerAction.PQ_FieldName = "Workflows.Where(\"somefield\"==\"somevalue\").FH_CompletionStatement";
			AssertHasErrorContaining(triggerAction.PQ_FieldNameInfo, "FH_CompletionStatement of type ZString cannot be used in 'Apply Tag' trigger action - only IProcessHeaderCollection type is supported.");
		}

		public void TestCheckPQ_FieldName_MultiTypes()
		{
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingShipment>());
			var shipmentWorkflow = (IWorkflowProvider)shipment;
			var shipmentTask = shipmentWorkflow.WorkflowItems.Triggers.AddNew();
			var action = Factory.New<ProcessTaskNotification>();
			action.PQ_P9 = shipmentTask.PK;
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			var rootTypeProvider = action as IRootTypeProvider;
			AssertSequencesEqual("Precondition: RootTypes", new[] { shipment.GetType(), shipmentTask.GetType(), action.GetType(), typeof(StmALog) }, rootTypeProvider.RootTypes);

			CombineAssertions(() =>
			{
				action.PQ_FieldName = "_DataSource";
				AssertHasWarning("Field name is always a macro even if no <>", action.PQ_FieldNameInfo, MacroDataSource.EmptyDataSourceTypeError);

				action.PQ_FieldName = "<_DataSource>";
				AssertHasWarning("No data source after _dataSource prefix", action.PQ_FieldNameInfo, MacroDataSource.EmptyDataSourceTypeError);

				action.PQ_FieldName = "<_DataSource.XXX>";
				AssertHasWarning("Data source XXX is not support", action.PQ_FieldNameInfo, MacroDataSource.GetDataSourceTypeNotFoundMessage("XXX"));

				action.PQ_FieldName = "<_DataSource.JobDeclaration.JE_HouseBill>";
				AssertHasWarning("No JobDeclaration type in parent types", action.PQ_FieldNameInfo, MacroDataSource.GetDataSourceTypeNotFoundMessage("JobDeclaration"));

				action.PQ_FieldName = "<JS_HouseBill>";
				AssertNoNotifications("Valid property", action.PQ_FieldNameInfo);

				var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
				declaration.JE_JS = shipment.PK;
				AssertSequencesEqual("Precondition: RootTypes", new[] { shipment.GetType(), shipmentTask.GetType(), declaration.GetType(), action.GetType(), typeof(StmALog) }, rootTypeProvider.RootTypes);

				action.PQ_FieldName = "<_DataSource.JobDeclaration.>";
				AssertHasWarning("empty property", action.PQ_FieldNameInfo, prepertyNotfoundOnDeclarationError(""));

				action.PQ_FieldName = "<_DataSource.JobDeclaration.JE_HouseBill>";
				AssertNoNotifications("Valid property JE_HouseBill on JobDeclaration", action.PQ_FieldNameInfo);

				action.PQ_FieldName = "<JE_HouseBill>";
				AssertNoNotifications("Valid property JE_HouseBill", action.PQ_FieldNameInfo);

				action.PQ_FieldName = "<JS_HouseBill>";
				AssertNoNotifications("Valid property JS_HouseBill", action.PQ_FieldNameInfo);

				action.PQ_FieldName = "<XXX>";
				AssertHasWarning("invalid property", action.PQ_FieldNameInfo, prepertyNotfoundOnShipmentError("XXX") + "\n" + prepertyNotfoundOnDeclarationError("XXX"));
			});
		}

		public void TestCheckPQ_FieldValue_FieldNameError()
		{
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingShipment>());
			var shipmentWorkflow = (IWorkflowProvider)shipment;
			var shipmentTask = shipmentWorkflow.WorkflowItems.Triggers.AddNew();
			var action = Factory.New<ProcessTaskNotification>();
			action.PQ_P9 = shipmentTask.PK;
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			var warningMsg = "Field Name has errors.";

			action.PQ_FieldName = "<_DataSource.JobDeclaration.JE_HouseBill>";
			action.PQ_FieldValue = "value1";
			AssertHasWarning("Parent types not include JobDeclaration data source", action.PQ_FieldValueInfo, warningMsg);

			action.PQ_FieldName = "<Z0_WrongField>";
			action.PQ_FieldValue = "value2";
			AssertHasWarning("Parent types not include Z0_WrongField property", action.PQ_FieldValueInfo, warningMsg);

			var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;

			action.PQ_FieldName = "<_DataSource.JobDeclaration.JE_HouseBill>";
			action.PQ_FieldValue = "value3";
			AssertNoNotifications("Parent types include JobDeclaration data source", action.PQ_FieldValueInfo);

			action.PQ_FieldName = "<_DataSource.JobDeclaration.Z0_WrongField>";
			action.PQ_FieldValue = "value4";
			AssertHasWarning("JobDeclaration data source not include Z0_WrongField property", action.PQ_FieldValueInfo, warningMsg);
		}

		public void TestCheckPQ_FieldValue_MultiTypes()
		{
			var shipment = Factory.NewWithValidTestData(ObjectFactory.GetType<IForwardingShipment>());
			var shipmentWorkflow = (IWorkflowProvider)shipment;
			var shipmentTask = shipmentWorkflow.WorkflowItems.Triggers.AddNew();
			var action = Factory.New<ProcessTaskNotification>();
			action.PQ_P9 = shipmentTask.PK;
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
			var rootTypeProvider = action as IRootTypeProvider;
			AssertSequencesEqual("Precondition: RootTypes", new[] { shipment.GetType(), shipmentTask.GetType(), action.GetType(), typeof(StmALog) }, rootTypeProvider.RootTypes);

			CombineAssertions(() =>
			{
				action.PQ_FieldName = "<JS_HouseBill>";
				action.PQ_FieldValue = "BILL123";
				AssertNoNotifications("value is a string", action.PQ_FieldValueInfo);

				action.PQ_FieldValue = "<BILL123>";
				AssertHasWarning("value is a macro and has no BILL123 property", action.PQ_FieldValueInfo, valueNotfoundError("<BILL123>", "[ForwardingShipment], [ForwardingShipmentProcessTask], [ProcessTaskNotification]"));

				action.PQ_FieldValue = "_DataSource";
				AssertNoNotifications("_DataSource is avalid string", action.PQ_FieldValueInfo);

				action.PQ_FieldValue = "<_DataSource>";
				AssertHasWarning("_DataSource means prefix in macro", action.PQ_FieldValueInfo, MacroDataSource.EmptyDataSourceTypeError);

				action.PQ_FieldValue = "<_DataSource.XXX.>";
				AssertHasWarning("XXX data source prefix is not support", action.PQ_FieldValueInfo, MacroDataSource.GetDataSourceTypeNotFoundMessage("XXX"));

				action.PQ_FieldValue = "<_DataSource.JobDeclaration.JE_HouseBill>";
				AssertHasWarning("Parent DataSources has no JobDeclaration data source", action.PQ_FieldValueInfo, MacroDataSource.GetDataSourceTypeNotFoundMessage("JobDeclaration"));

				var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
				declaration.JE_JS = shipment.PK;
				AssertSequencesEqual("Precondition: RootTypes", new[] { shipment.GetType(), shipmentTask.GetType(), declaration.GetType(), action.GetType(), typeof(StmALog) }, rootTypeProvider.RootTypes);

				action.PQ_FieldValue = "<_DataSource.JobDeclaration.Z0_WrongField>";
				AssertHasWarning("JobDeclaration data source has no Z0_WrongField property", action.PQ_FieldValueInfo, "Field <Z0_WrongField> not found on DataSource Type [JobDeclaration].");

				action.PQ_FieldValue = "<Z0_WrongField>";
				AssertHasWarning("All data source have no Z0_WrongField property", action.PQ_FieldValueInfo, valueNotfoundError("<Z0_WrongField>", "[ForwardingShipment], [ForwardingShipmentProcessTask], [ProcessTaskNotification], [JobDeclaration]"));

				action.PQ_FieldValue = "<_DataSource.JobDeclaration.JE_HouseBill>";
				AssertNoNotifications("Parent DataSources has JobDeclaration data source", action.PQ_FieldValueInfo);

				action.PQ_FieldValue = "<_DataSource.JobDeclaration.JE_HouseBill><JS_HouseBill>";
				AssertNoNotifications("Multi macros", action.PQ_FieldValueInfo);
			});
		}

		string prepertyNotfoundOnDeclarationError(ZString propertyName) => $"Cannot find property {propertyName} on Enterprise.Customs.AU.Declaration.Business.JobDeclaration.";

		string prepertyNotfoundOnShipmentError(ZString propertyName) => $"Cannot find property {propertyName} on Enterprise.Freight.Forwarding.Business.ForwardingShipment.";

		string valueNotfoundError(ZString value, ZString types) => $"Field {value} not found on any of the DataSource Types: {types}.";

		public void TestCheckPQ_FieldValue()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			dummy.Z0_Code = "XYZ";

			var trigger = CreateWorkflowItemForJob(dummy);

			if (!(trigger is IWorkflowTrigger))
			{
				Assert("It's not possible to test the value produced by a template trigger since a job is required.", true);
			}
			else
			{
				var triggerAction = trigger.CompletionTriggerActionsCollection().AddNew();

				triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
				triggerAction.PQ_FieldName = "Z0_Number";

				triggerAction.PQ_FieldValue = "123";
				AssertNoWarnings(triggerAction.PQ_FieldValueInfo);

				triggerAction.PQ_FieldValue = "";
				AssertNoWarnings(triggerAction.PQ_FieldValueInfo);

				triggerAction.PQ_FieldValue = "<>";
#if NETFRAMEWORK
				var warningMessage = @"Property Enterprise.MasterFiles.Business.Testing.DummyWithWorkflow.Z0_Number of type ZInt cannot be set with value 'Enterprise.MasterFiles.Business.Testing.DummyWithWorkflow' in 'Set Field' trigger action.
Input string was not in a correct format. (in ConvertFrom, value = 'Enterprise.MasterFiles.Business.Testing.DummyWithWorkflow')";
#else
				var warningMessage = @"Property Enterprise.MasterFiles.Business.Testing.DummyWithWorkflow.Z0_Number of type ZInt cannot be set with value 'Enterprise.MasterFiles.Business.Testing.DummyWithWorkflow' in 'Set Field' trigger action.
The input string 'Enterprise.MasterFiles.Business.Testing.DummyWithWorkflow' was not in a correct format. (in ConvertFrom, value = 'Enterprise.MasterFiles.Business.Testing.DummyWithWorkflow')";
#endif
				AssertHasWarning(triggerAction.PQ_FieldValueInfo, warningMessage);

				triggerAction.PQ_FieldValue = "<Z0_WrongField>";
				AssertHasWarning(triggerAction.PQ_FieldValueInfo, "Field <Z0_WrongField> not found on any of the DataSource Types: [DummyWithWorkflow], [DummyProcessTask], [ProcessTaskNotification].");

#if NETFRAMEWORK
				warningMessage = @"Property Enterprise.MasterFiles.Business.Testing.DummyWithWorkflow.Z0_Number of type ZInt cannot be set with value 'XYZ' in 'Set Field' trigger action.
Input string was not in a correct format. (in ConvertFrom, value = 'XYZ')";
#else
				warningMessage = @"Property Enterprise.MasterFiles.Business.Testing.DummyWithWorkflow.Z0_Number of type ZInt cannot be set with value 'XYZ' in 'Set Field' trigger action.
The input string 'XYZ' was not in a correct format. (in ConvertFrom, value = 'XYZ')";
#endif
				triggerAction.PQ_FieldValue = "<Z0_Code>";
				AssertHasWarning(triggerAction.PQ_FieldValueInfo, warningMessage);
			}
		}

		public void TestCheckPQ_FieldValue_StmNote()
		{
			using (new WorkflowDescriptorsForTest())
			{
				var dummy = Factory.New<DummyWithWorkflowWithStmNote>();
				dummy.Z0_IsSystem = false;
				dummy.StmNoteProperty = Factory.New<StmNote>();
				dummy.StmNoteProperty.ST_Description = PredefinedNoteTypes.Instance.BookingNotes.Code;
				dummy.StmNoteProperty.Master = dummy;
				var trigger = dummy.WorkflowItems.Triggers.AddNew();

				if (!(trigger is IWorkflowTrigger))
				{
					Assert("It's not possible to test the value produced by a template trigger since a job is required.", true);
				}
				else
				{
					var triggerAction = trigger.CompletionTriggerActionsCollection().AddNew();

					triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
					triggerAction.PQ_FieldName = "Z0_StmNote";

					triggerAction.PQ_FieldValue = "123";
					AssertNoErrors(triggerAction.PQ_FieldNameInfo);

					var notifications = new NotificationsForTest();
					FireTriggerAction(dummy, trigger, triggerAction, notifications);

					var notificationText = notifications.ToString();
					AssertContains("DummyWithWorkflowWithStmNote", notificationText);
					AssertContains("Z0_StmNote", notificationText);
				}
			}
		}

		[TestDate(2020, 1, 1)]
		public void TestCheckPQ_FieldValue_TriggeringEvent()
		{
			using (new WorkflowDescriptorsForTest())
			{
				var dummy = Factory.New<DummyWithWorkflow>();
				dummy.Z0_IsSystem = false;

				var trigger = dummy.WorkflowItems.Triggers.AddNew();
				trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;

				var action = trigger.ProcessTaskNotifications.AddNew();
				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;
				action.PQ_FieldName = "Z0_Date";
				action.PQ_FieldValue = "<TriggeringEvent.SL_EventTime>";

				Factory.Save();

				var log = dummy.Logs.AddNew();
				using (log.LockForUpdatingKeyFieldsForTesting())
				{
					log.SL_EventTime = ZDateTime.Now;
					log.SL_SE_NKEvent = Events.CustomisableEvent00Code;
				}

				AssertEquals("Precondition: Action fired once (triggering event will be set)", ZDateTime.Now, dummy.Z0_Date);

				log.Delete();//Delete the triggering event
				action.PQ_FieldValue = "<TriggeringEvent.SL_Reference>";//This will cause validation
				AssertNoErrors("The triggering event was deleted but should be set back to null", action.PQ_FieldValueInfo);
			}
		}

		public void TestPQ_EmailTextFallbackToTemplate()
		{
			const string incorrectEmailHtml = @"<p>This html doesn't have the closing tr tag on the tableheadings class</p>
<table class=''table''>
<tr class=''tableheadings''><th><b>Shipment Number</b></th>
<tr><td>S00000001</td></tr>
</table>";

			const string correctEmailHtml = @"<p>This html has the closing tr tag on the tableheadings class</p>
<table class=''table''>
<tr class=''tableheadings''><th><b>Shipment Number</b></th></tr>
<tr><td>S00000001</td></tr>
</table>";

			var processTaskNotification = NewProcessTaskNotification();

			processTaskNotification.PQ_EmailTextFallbackToTemplate = string.Empty;
			processTaskNotification.Validation.ValidateAll();
			AssertNoErrors(processTaskNotification.PQ_EmailTextFallbackToTemplateInfo);

			processTaskNotification.PQ_EmailTextFallbackToTemplate = incorrectEmailHtml;
			processTaskNotification.Validation.ValidateAll();
			AssertHasErrors(processTaskNotification.PQ_EmailTextFallbackToTemplateInfo);

			processTaskNotification.PQ_EmailTextFallbackToTemplate = correctEmailHtml;
			processTaskNotification.Validation.ValidateAll();
			AssertNoErrors(processTaskNotification.PQ_EmailTextFallbackToTemplateInfo);
		}

		public void TestPQ_RelatedEntityID_MandatoryValidation()
		{
			var tagDefinition = BMSTestHelper.CreateTagDefinition(Factory, "A1", usageScope: TagUsageScopeList.Codes.All, scope: TagScopeList.Codes.All);
			var tag = BMSTestHelper.CreateTagMagnitude(tagDefinition, "TG1", "TAG 1");
			Factory.Save();

			var trigger = CreateWorkflowItemForTemplateType(DummyWorkflowDescriptor.Instance.Code);
			var triggerAction = trigger.CompletionTriggerActionsCollection().AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyTag;

			var expectedError = "Please enter a Related Entity.";
			triggerAction.PQ_RelatedEntityId = ZGuid.Empty;
			AssertHasError(triggerAction.PQ_RelatedEntityIdInfo, expectedError);

			triggerAction.PQ_RelatedEntityId = tag.PK;
			AssertNoErrors(triggerAction.PQ_RelatedEntityIdInfo);

			triggerAction.PQ_RelatedEntityId = ZGuid.Empty;
			AssertHasError(triggerAction.PQ_RelatedEntityIdInfo, expectedError);

			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendReleaseMessage;
			triggerAction.Validation.ValidateAll();
			AssertNoErrors(triggerAction.PQ_RelatedEntityIdInfo);
		}

		public void TestPQ_RelatedEntityID_ListValidation()
		{
			var tagDef1 = BMSTestHelper.CreateTagDefinition(Factory, "A1", usageScope: TagUsageScopeList.Codes.All, scope: TagScopeList.Codes.All);
			var tagDef2 = BMSTestHelper.CreateTagDefinition(Factory, "A2", usageScope: TagUsageScopeList.Codes.All, scope: TagScopeList.Codes.Task);
			var tagDef3 = BMSTestHelper.CreateTagDefinition(Factory, "A3", usageScope: TagUsageScopeList.Codes.All, scope: TagScopeList.Codes.Workflow);

			var tagDef4 = BMSTestHelper.CreateTagDefinition(Factory, "R1", usageScope: TagUsageScopeList.Codes.Rule, scope: TagScopeList.Codes.All);
			var tagDef5 = BMSTestHelper.CreateTagDefinition(Factory, "R2", usageScope: TagUsageScopeList.Codes.Rule, scope: TagScopeList.Codes.Task);
			var tagDef6 = BMSTestHelper.CreateTagDefinition(Factory, "R3", usageScope: TagUsageScopeList.Codes.Rule, scope: TagScopeList.Codes.Workflow);

			var tagDef7 = BMSTestHelper.CreateTagDefinition(Factory, "U1", usageScope: TagUsageScopeList.Codes.User, scope: TagScopeList.Codes.All);
			var tagDef8 = BMSTestHelper.CreateTagDefinition(Factory, "U2", usageScope: TagUsageScopeList.Codes.User, scope: TagScopeList.Codes.Task);
			var tagDef9 = BMSTestHelper.CreateTagDefinition(Factory, "U3", usageScope: TagUsageScopeList.Codes.User, scope: TagScopeList.Codes.Workflow);

			var tag1 = BMSTestHelper.CreateTagMagnitude(tagDef1, "TG1", "TAG 1");
			var tag2 = BMSTestHelper.CreateTagMagnitude(tagDef2, "TG2", "TAG 2");
			var tag3 = BMSTestHelper.CreateTagMagnitude(tagDef3, "TG3", "TAG 3");
			var tag4 = BMSTestHelper.CreateTagMagnitude(tagDef4, "TG4", "TAG 4");
			var tag5 = BMSTestHelper.CreateTagMagnitude(tagDef5, "TG5", "TAG 5");
			var tag6 = BMSTestHelper.CreateTagMagnitude(tagDef6, "TG6", "TAG 6");
			var tag7 = BMSTestHelper.CreateTagMagnitude(tagDef7, "TG7", "TAG 7");
			var tag8 = BMSTestHelper.CreateTagMagnitude(tagDef8, "TG8", "TAG 8");
			var tag9 = BMSTestHelper.CreateTagMagnitude(tagDef9, "TG9", "TAG 9");

			Factory.Save();

			var trigger = CreateWorkflowItemForTemplateType(DummyWorkflowDescriptor.Instance.Code);
			var triggerAction = trigger.CompletionTriggerActionsCollection().AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyTag;

			var expectedError = "Enter a valid Related Entity.";
			triggerAction.PQ_RelatedEntityId = tag1.PK;
			AssertNoErrors("tag1 is a valid tag", triggerAction.PQ_RelatedEntityIdInfo);

			triggerAction.PQ_RelatedEntityId = tag2.PK;
			AssertHasError("tag2 is not a valid tag", triggerAction.PQ_RelatedEntityIdInfo, expectedError);

			triggerAction.PQ_RelatedEntityId = tag3.PK;
			AssertNoErrors("tag3 is a valid tag", triggerAction.PQ_RelatedEntityIdInfo);

			triggerAction.PQ_RelatedEntityId = tag4.PK;
			AssertNoErrors("tag4 is a valid tag", triggerAction.PQ_RelatedEntityIdInfo);

			triggerAction.PQ_RelatedEntityId = tag5.PK;
			AssertHasError("tag5 is not a valid tag", triggerAction.PQ_RelatedEntityIdInfo, expectedError);

			triggerAction.PQ_RelatedEntityId = tag6.PK;
			AssertNoErrors("tag6 is a valid tag", triggerAction.PQ_RelatedEntityIdInfo);

			triggerAction.PQ_RelatedEntityId = tag7.PK;
			AssertHasError("tag7 is not a valid tag", triggerAction.PQ_RelatedEntityIdInfo, expectedError);

			triggerAction.PQ_RelatedEntityId = tag8.PK;
			AssertHasError("tag8 is not a valid tag", triggerAction.PQ_RelatedEntityIdInfo, expectedError);

			triggerAction.PQ_RelatedEntityId = tag9.PK;
			AssertHasError("tag9 is not a valid tag", triggerAction.PQ_RelatedEntityIdInfo, expectedError);
		}

		public void TestPQ_RelatedEntityID_NoErrorWhenMultipleTriggerActionsLinkedToDifferentEventsAndSameExclusiveTriggerAction()
		{
			var tagDef1 = BMSTestHelper.CreateTagDefinition(Factory, "A1", isExclusive: true, usageScope: TagUsageScopeList.Codes.All, scope: TagScopeList.Codes.All);

			var tag1 = BMSTestHelper.CreateTagMagnitude(tagDef1, "TG1", "TAG 1");
			var tag2 = BMSTestHelper.CreateTagMagnitude(tagDef1, "TG2", "TAG 2");

			Factory.Save();

			var dummyBO = Factory.New<DummyWithWorkflow>();

			var trigger1 = dummyBO.WorkflowItems.Triggers.AddNew();
			trigger1.P9_Description = "Hap1";
			trigger1.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;

			var triggerAction1 = trigger1.CompletionTriggerActionsCollection().AddNew();
			triggerAction1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyTag;
			triggerAction1.PQ_RelatedEntityId = tag1.PK;

			var trigger2 = dummyBO.WorkflowItems.Triggers.AddNew();
			trigger2.P9_Description = "Hap2";
			trigger2.TriggerConditions.TriggerEventCode = Events.CustomisableEvent01Code;

			var triggerAction2 = trigger2.CompletionTriggerActionsCollection().AddNew();
			triggerAction2.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyTag;
			triggerAction2.PQ_RelatedEntityId = tag2.PK;

			AssertNoErrors("No error should be found since both trigger actions are linked to separate events though linked to the same exclusive tag group", triggerAction1.PQ_RelatedEntityIdInfo);
			AssertNoErrors("No error should be found since both trigger actions are linked to separate events though linked to the same exclusive tag group", triggerAction2.PQ_RelatedEntityIdInfo);

			trigger2.TriggerConditions.TriggerEventCode = trigger1.TriggerConditions.TriggerEventCode;

			triggerAction1.Validation.ValidateAll();
			triggerAction2.Validation.ValidateAll();

			AssertNoErrors("No error should be found since both trigger actions are linked to separate events though linked to the same exclusive tag group", triggerAction1.PQ_RelatedEntityIdInfo);
			AssertNoErrors("No error should be found since both trigger actions are linked to separate events though linked to the same exclusive tag group", triggerAction2.PQ_RelatedEntityIdInfo);

			triggerAction2.PQ_P9 = trigger1.PK;

			Factory.Save();

			triggerAction1.Validation.ValidateAll();
			triggerAction2.Validation.ValidateAll();

			var expectedError = "Another TAG completion trigger action is already configured to apply a different tag from the same exclusive group. Please update or remove the existing TAG completion trigger action to apply this tag.";
			AssertHasError("Two different trigger actions linked to the same trigger and the same exclusive group, this is not permitted", triggerAction1.PQ_RelatedEntityIdInfo, expectedError);
			AssertHasError("Two different trigger actions linked to the same trigger and the same exclusive group, this is not permitted", triggerAction2.PQ_RelatedEntityIdInfo, expectedError);
		}

		public void TestPQ_RelatedEntityID_NoErrorWhenMultipleTriggerActionsLinkedToTheSameEventAndTwoDifferentExclusiveTriggerActions()
		{
			var tagDef1 = BMSTestHelper.CreateTagDefinition(Factory, "A1", isExclusive: true, usageScope: TagUsageScopeList.Codes.All, scope: TagScopeList.Codes.All);
			var tagDef2 = BMSTestHelper.CreateTagDefinition(Factory, "A2", isExclusive: true, usageScope: TagUsageScopeList.Codes.All, scope: TagScopeList.Codes.All);

			var tag1 = BMSTestHelper.CreateTagMagnitude(tagDef1, "TG1", "TAG 1");
			var tag2 = BMSTestHelper.CreateTagMagnitude(tagDef2, "TG2", "TAG 2");

			Factory.Save();

			var dummyBO = Factory.New<DummyWithWorkflow>();

			var trigger1 = dummyBO.WorkflowItems.Triggers.AddNew();
			trigger1.P9_Description = "Hap1";
			trigger1.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;

			var triggerAction1 = trigger1.CompletionTriggerActionsCollection().AddNew();
			triggerAction1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyTag;
			triggerAction1.PQ_RelatedEntityId = tag1.PK;

			var trigger2 = dummyBO.WorkflowItems.Triggers.AddNew();
			trigger2.P9_Description = "Hap2";
			trigger2.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;

			var triggerAction2 = trigger2.CompletionTriggerActionsCollection().AddNew();
			triggerAction2.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyTag;
			triggerAction2.PQ_RelatedEntityId = tag2.PK;

			AssertNoErrors("No error should be found since both trigger actions are linked to the same event and two different exclusive tag groups", triggerAction1.PQ_RelatedEntityIdInfo);
			AssertNoErrors("No error should be found since both trigger actions are linked to the same event and two different exclusive tag groups", triggerAction2.PQ_RelatedEntityIdInfo);

			triggerAction2.PQ_RelatedEntityId = triggerAction1.PQ_RelatedEntityId;

			triggerAction1.Validation.ValidateAll();
			triggerAction2.Validation.ValidateAll();

			AssertNoErrors("No error should be found since both trigger actions are linked to the same event and two different exclusive tag groups", triggerAction1.PQ_RelatedEntityIdInfo);
			AssertNoErrors("No error should be found since both trigger actions are linked to the same event and two different exclusive tag groups", triggerAction2.PQ_RelatedEntityIdInfo);

			triggerAction2.PQ_P9 = trigger1.PK;

			Factory.Save();

			triggerAction1.Validation.ValidateAll();
			triggerAction2.Validation.ValidateAll();

			var expectedError = "Another TAG completion trigger action is already configured to apply a different tag from the same exclusive group. Please update or remove the existing TAG completion trigger action to apply this tag.";

			AssertHasError("Two different trigger actions linked to the same trigger and the same exclusive group, this is not permitted", triggerAction1.PQ_RelatedEntityIdInfo, expectedError);
			AssertHasError("Two different trigger actions linked to the same trigger and the same exclusive group, this is not permitted", triggerAction2.PQ_RelatedEntityIdInfo, expectedError);
		}

		public void TestHasValidationErrorForTriggerWithTriggerFieldAndImmediateFieldChangeAction()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			dummy.Z0_IsSystem = false;

			var trigger = CreateWorkflowItemForJob(dummy);
			trigger.TriggerConditions_ForBinding.TriggerFieldName = DummyBizoSchema.Z0_Description.Name;

			var action = trigger.CompletionTriggerActionsCollection().AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange;

			string error = "Immediate Field Change cannot be used on Completion Trigger Actions for Milestones and Triggers that can trigger via the Trigger Field.";
			AssertHasError(action.PQ_TriggerTypeInfo, error);

			// Changing to event code should remove the error
			trigger.TriggerConditions_ForBinding.TriggerFieldName = null;
			trigger.TriggerConditions_ForBinding.TriggerEventCode = Events.CustomisableEvent01.Code;

			AssertNoError(action.PQ_TriggerTypeInfo, error);
		}

		public void TestCheckPQ_Offset_ShouldBeSetForDLYActions()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var trigger = CreateWorkflowItemForJob(dummy);
			trigger.TriggerConditions_ForBinding.TriggerEventCode = Events.CustomisableEvent00.Code;

			var action = trigger.CompletionTriggerActionsCollection().AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;

			action.Validation.ValidateAll();
			string error = "Please enter a value.";
			AssertNoError(action.PQ_OffsetInfo, error);

			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ScheduleDelayedEvent;

			action.Validation.ValidateAll();
			AssertHasError(action.PQ_OffsetInfo, error);

			action.PQ_Offset = new ZDateTime(2022, 06, 05);
			action.Validation.ValidateAll();
			AssertNoError(action.PQ_OffsetInfo, error);
		}

		public void TestCheckPQ_Offset_ShouldNotBeNegativeForActualTriggers()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var trigger = CreateWorkflowItemForJob(dummy);
			trigger.TriggerConditions_ForBinding.TriggerEventCode = Events.CustomisableEvent00.Code;
			trigger.SetShouldTriggerOnEstimateEvents_ForTests(false);
			AssertEquals("Precondition: actual trigger", false, trigger.ShouldTriggerOnEstimateEvents);

			var action = trigger.CompletionTriggerActionsCollection().AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ScheduleDelayedEvent;

			action.Validation.ValidateAll();
			string error = "Cannot define a negative offset for a trigger that responds to actual events. Negative offsets are allowed for triggers responding to estimate events only.";
			AssertNoError(action.PQ_OffsetInfo, error);

			var zeroOffset = new ZDateTime(2022, 1, 1);
			var positiveOffset = zeroOffset.AddHours(1);
			var negativeOffset = zeroOffset.AddHours(-1);

			action.PQ_Offset = zeroOffset;
			action.Validation.ValidateAll();
			AssertNoError(action.PQ_OffsetInfo, error);

			action.PQ_Offset = negativeOffset;
			action.Validation.ValidateAll();
			AssertHasError("Should not allow negative offset for actual triggers", action.PQ_OffsetInfo, error);

			action.PQ_Offset = positiveOffset;
			action.Validation.ValidateAll();
			AssertNoError(action.PQ_OffsetInfo, error);

			trigger.SetShouldTriggerOnEstimateEvents_ForTests(true);
			AssertEquals("Precondition: estimate trigger", true, trigger.ShouldTriggerOnEstimateEvents);

			action.PQ_Offset = zeroOffset;
			action.Validation.ValidateAll();
			AssertNoError(action.PQ_OffsetInfo, error);

			action.PQ_Offset = negativeOffset;
			action.Validation.ValidateAll();
			AssertNoError(action.PQ_OffsetInfo, error);

			action.PQ_Offset = positiveOffset;
			action.Validation.ValidateAll();
			AssertNoError(action.PQ_OffsetInfo, error);
		}

		public void TestCheckPQ_ActionReference_ShoudNotContainVerticalLines()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var trigger = CreateWorkflowItemForJob(dummy);
			trigger.TriggerConditions_ForBinding.TriggerEventCode = Events.CustomisableEvent00.Code;

			var action = trigger.CompletionTriggerActionsCollection().AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ScheduleDelayedEvent;

			action.Validation.ValidateAll();
			string error = "Action reference should not contain pipe (\"|\") symbols.";
			AssertNoError(action.PQ_ActionReferenceInfo, error);

			action.PQ_ActionReference = "Improper|reference";
			action.Validation.ValidateAll();
			AssertHasError(action.PQ_ActionReferenceInfo, error);

			action.PQ_ActionReference = "Proper reference";
			action.Validation.ValidateAll();
			AssertNoError(action.PQ_ActionReferenceInfo, error);
		}

		internal IBMTestHelper BMSTestHelper
		{
			get { return ObjectFactory.Get<IBMTestHelper>(); }
		}

		sealed class DummyWithWorkflowWithStmNote : DummyWithWorkflow
		{
			public DummyWithWorkflowWithStmNote(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{ }

			public StmNote StmNoteProperty { get; set; }

			public ZString Z0_StmNote
			{
				get
				{
					return StmNoteProperty.ST_NoteText;
				}
				set
				{
					StmNoteProperty.ST_NoteText = value;
				}
			}

			protected override string GetWorkflowType()
			{
				return DummyWithWorkflowWithStmNoteType;
			}
		}

		public const string DummyWithWorkflowWithStmNoteType = "SNT";

		sealed class WorkflowDescriptorsForTest : WorkflowDescriptors, IDisposable
		{
			public WorkflowDescriptorsForTest()
			{
				AddDescriptor(new DummyWithWorkflowWithStmNoteDescriptor());
				overrideDescriptorsDelegate = OverrideWorkflowDescriptorsDelegate(() => this);
			}

			public void Dispose()
			{
				overrideDescriptorsDelegate.Dispose();
			}

			readonly IDisposable overrideDescriptorsDelegate;
		}

		sealed class DummyWithWorkflowWithStmNoteDescriptor : WorkflowDescriptor
		{
			public DummyWithWorkflowWithStmNoteDescriptor()
			{
				DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithWorkflowWithStmNote);
			}

			public override string Code => DummyWithWorkflowWithStmNoteType;
			public override IMultilingualString Description => (NoResString)"DummyWithWorkflow With StmNote Type";
			public override ControllerID ControllerID => DummyControllerIDs.Dummy;
			public override Type WorkflowProviderType => typeof(DummyWithWorkflowWithStmNote);
			protected override ZString[] SupportedTriggerPartyServicesCore(ZString recipient) => new ZString[] { "ABC", "DEF" };
		}

		public void TestCheckPQ_FieldValue_Macro_DateTimeAsString()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var trigger = CreateWorkflowItemForJob(dummy);

			var triggerAction = trigger.CompletionTriggerActionsCollection().AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;

			triggerAction.PQ_FieldName = "Z0_Date";
			triggerAction.PQ_FieldValue = "<DateTimeAsString('<Now>', 'dd-MMM-yy')>";
			AssertNoWarnings("GIVEN processTaskNotification with FieldValue contains DateTimeAsString macro WHEN validating THEN should not show warning", triggerAction.PQ_FieldValueInfo);
		}

		public void TestCheckPQ_FieldValue_First()
		{
			SetupAndAssert_CheckPQ_FieldValue_First_FirstOrDefault("First");
		}

		public void TestCheckPQ_FieldValue_FirstOrDefault()
		{
			SetupAndAssert_CheckPQ_FieldValue_First_FirstOrDefault("FirstOrDefault");
		}

		public void TestCheckPQ_FieldValue_First_WithoutCollection()
		{
			SetupAndAssert_CheckPQ_FieldValue_WithoutCollection("First");
		}

		public void TestCheckPQ_FieldValue_FirstOrDefault_WithoutCollection()
		{
			SetupAndAssert_CheckPQ_FieldValue_WithoutCollection("FirstOrDefault");
		}

		public void TestCheckPQ_P0_WorkflowTemplate()
		{
			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			var template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = "DUM";
			template2.P0_ProcessType = "DUM";
			var trigger = CreateWorkflowItemForTemplate(template1);
			var triggerAction = trigger.CompletionTriggerActionsCollection().AddNew();

			triggerAction.Validation.ValidateAll();
			AssertNoErrors("Trigger type not set to TMP, so no relevant errors should be present, and yet...", triggerAction.PQ_P0_WorkflowTemplateInfo);

			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateOnce;
			triggerAction.Validation.ValidateAll();
			AssertHasError(triggerAction.PQ_P0_WorkflowTemplateInfo, "Please enter a Workflow Template.");

			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateAlways;
			triggerAction.Validation.ValidateAll();
			AssertHasError(triggerAction.PQ_P0_WorkflowTemplateInfo, "Please enter a Workflow Template.");

			triggerAction.PQ_P0_WorkflowTemplate = ZGuid.NewZGuid();
			AssertNoError(triggerAction.PQ_P0_WorkflowTemplateInfo, "Please enter a Workflow Template.");
			AssertHasError(triggerAction.PQ_P0_WorkflowTemplateInfo, "Enter a valid Workflow Template.");

			triggerAction.PQ_P0_WorkflowTemplate = template2.PK;
			AssertNoErrors(triggerAction.PQ_P0_WorkflowTemplateInfo);

			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateOnce;
			triggerAction.Validation.ValidateAll();
			AssertNoErrors(triggerAction.PQ_P0_WorkflowTemplateInfo);

			triggerAction.PQ_P0_WorkflowTemplate = template1.PK;
			AssertHasError(triggerAction.PQ_P0_WorkflowTemplateInfo, "Please select a workflow template that is not the current workflow template.");

			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateAlways;
			triggerAction.Validation.ValidateAll();
			AssertHasError(triggerAction.PQ_P0_WorkflowTemplateInfo, "Please select a workflow template that is not the current workflow template.");
		}

		public void TestCheckPQ_P0_WorkflowTemplate_StartAndEndDates()
		{
			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			var template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template2.P0_EffectiveStartDateUtc = ZDateTime.UtcNow.AddDays(10);
			template2.P0_EffectiveEndDateUtc = ZDateTime.UtcNow.AddDays(-10);
			template1.P0_ProcessType = "DUM";
			template2.P0_ProcessType = "DUM";
			var trigger = CreateWorkflowItemForTemplate(template1);
			var triggerAction = trigger.CompletionTriggerActionsCollection().AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateOnce;
			triggerAction.PQ_P0_WorkflowTemplate = template2.PK;

			AssertHasWarning(triggerAction.PQ_P0_WorkflowTemplateInfo, "The effective start date of the selected template has not yet been reached.");
			AssertHasWarning(triggerAction.PQ_P0_WorkflowTemplateInfo, "The effective end date of the selected template has been exceeded.");
		}

		public void TestCheckPQ_P0_WorkflowTemplate_ApplyWorkflowTemplateAlways_ShouldNotAddDuplicateTemplate()
		{
			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			var template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			var template3 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_ProcessType = "DUM";
			template2.P0_ProcessType = "DUM";
			template3.P0_ProcessType = "DUM";
			var trigger = CreateWorkflowItemForTemplate(template1);

			var triggerAction1 = trigger.CompletionTriggerActionsCollection().AddNew();
			triggerAction1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateAlways;
			triggerAction1.PQ_P0_WorkflowTemplate = template2.PK;
			var triggerAction2 = trigger.CompletionTriggerActionsCollection().AddNew();
			triggerAction2.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateAlways;
			triggerAction2.PQ_P0_WorkflowTemplate = template2.PK;

			AssertHasError(triggerAction2.PQ_P0_WorkflowTemplateInfo, "Please select a workflow template that is not already applied to this trigger.");

			triggerAction2.PQ_P0_WorkflowTemplate = template3.PK;
			AssertNoErrors(triggerAction2.PQ_P0_WorkflowTemplateInfo);
		}

		public void TestCheckPQ_TriggerTypeForAutoSendingActions()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var trigger = CreateWorkflowItemForJob(dummy);
			var triggerAction = trigger.CompletionTriggerActionsCollection().AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendEntryDeclarationMessage;
			AssertHasErrorContaining(triggerAction.PQ_TriggerTypeInfo, ListValidation.InvalidCodeError);
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendReleaseMessage;
			AssertHasErrorContaining(triggerAction.PQ_TriggerTypeInfo, ListValidation.InvalidCodeError);

			var notification = Factory.New<ProcessTaskNotification>();
			triggerAction.PQ_SourceTemplateNotification = notification.PK;
			triggerAction.Validation.ValidatePQ_TriggerType();
			AssertNoErrorContaining(triggerAction.PQ_TriggerTypeInfo, ListValidation.InvalidCodeError);
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendEntryDeclarationMessage;
			AssertNoErrorContaining(triggerAction.PQ_TriggerTypeInfo, ListValidation.InvalidCodeError);
		}

		public void TestCheckPQ_TriggerType_TRI_Validation()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var trigger = CreateWorkflowItemForJob(dummy);
			var triggerAction = trigger.CompletionTriggerActionsCollection().AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendImportDemandDeTracing;
			AssertHasErrorContaining(triggerAction.PQ_TriggerTypeInfo, ListValidation.InvalidCodeError);

			Factory.Save();
			triggerAction.Validation.ValidatePQ_TriggerType();
			AssertNoErrorContaining(triggerAction.PQ_TriggerTypeInfo, ListValidation.InvalidCodeError);
		}

		public void TestCheckPQ_TriggerType_TRE_Validation()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var trigger = CreateWorkflowItemForJob(dummy);
			var triggerAction = trigger.CompletionTriggerActionsCollection().AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendExportDemandDeTracing;
			AssertHasErrorContaining(triggerAction.PQ_TriggerTypeInfo, ListValidation.InvalidCodeError);

			Factory.Save();
			triggerAction.Validation.ValidatePQ_TriggerType();
			AssertNoErrorContaining(triggerAction.PQ_TriggerTypeInfo, ListValidation.InvalidCodeError);
		}

		public void TestCheckPQ_TriggerType_CIN_Validation()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var trigger = CreateWorkflowItemForJob(dummy);
			var triggerAction = trigger.CompletionTriggerActionsCollection().AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.CINExportNotification;
			AssertHasErrorContaining(triggerAction.PQ_TriggerTypeInfo, ListValidation.InvalidCodeError);

			Factory.Save();
			triggerAction.Validation.ValidatePQ_TriggerType();
			AssertNoErrorContaining(triggerAction.PQ_TriggerTypeInfo, ListValidation.InvalidCodeError);
		}

		#region Implementation

		ProcessTaskNotification NewProcessTaskNotification()
		{
			var trigger = CreateWorkflowItemForTemplateType(DummyWorkflowDescriptor.Instance.Code);
			var triggerAction = trigger.CompletionTriggerActionsCollection().AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXML;

			Factory.Save();

			return triggerAction;
		}

		ProcessTaskNotification NewProcessTaskNotificationWithOrgOpportunity()
		{
			var opp = Factory.NewWithValidTestData<OrgOpportunity>();
			var trigger = CreateWorkflowItemForJob(opp);
			var triggerAction = trigger.CompletionTriggerActionsCollection().AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXML;

			Factory.Save();

			return triggerAction;
		}

		void SetupAndAssert_CheckPQ_FieldValue_First_FirstOrDefault(string clause)
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var trigger = CreateWorkflowItemForJob(dummy);

			if (!(trigger is IWorkflowTrigger))
			{
				Assert("It's not possible to test the value produced by a template trigger since a job is required.", true);
			}
			else
			{
				var triggerAction = trigger.CompletionTriggerActionsCollection().AddNew();
				triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
				triggerAction.PQ_FieldName = "Collection.Z0_Number";

				var child1 = dummy.Collection.AddNew();
				child1.Z0_Code = "A";
				child1.Z0_Number = 111;

				var child2 = dummy.Collection.AddNew();
				child2.Z0_Code = "B";
				child2.Z0_Number = 222;

				triggerAction.PQ_FieldValue = "<Collection.Z0_Number>";
				AssertNoWarnings("WHEN argument is collection without First/FirstOrDefault, should not error because it will get the FIRST value by default", triggerAction.PQ_FieldValueInfo);

				triggerAction.PQ_FieldValue = $"<Collection.{clause}(\"<Z0_Code>\" == \"A\").Z0_Number>";
				AssertNoWarnings("WHEN argument is valid, should not error", triggerAction.PQ_FieldValueInfo);

				triggerAction.PQ_FieldValue = $"<Collection.{clause}(\"<Z0_Code>\" == \"A\").RelatedDummy.Collection.{clause}(\"<Z0_Code>\" == \"A1\").Z0_Number>";
				AssertNoWarnings("WHEN argument has RelatedDummy is NULL, should not error instead return null and not set any value", triggerAction.PQ_FieldValueInfo);

				AssertNull(child1.RelatedDummy);

				var relatedDummy = Factory.New<DummyWithWorkflow>();
				relatedDummy.Z0_Number = 30;
				child1.Z0_Guid = relatedDummy.PK;

				AssertNotNull(child1.RelatedDummy);

				var child1related1 = relatedDummy.Collection.AddNew();
				child1related1.Z0_Code = "A1";
				child1related1.Z0_Number = 999;

				triggerAction.PQ_FieldValue = $"<Collection.{clause}(\"<Z0_Code>\" == \"A\").RelatedDummy.Collection.{clause}(\"<Z0_Code>\" == \"A1\").Z0_Number>";
				AssertNoWarnings("WHEN argument is valid and return result, should not error", triggerAction.PQ_FieldValueInfo);

				triggerAction.PQ_FieldValue = $"<Collection.{clause}().Z0_Number>";
				AssertNoWarnings("Empty argument is valid", triggerAction.PQ_FieldValueInfo);

				CombineAssertions("GIVEN invalid macro WHEN validating THEN should have no error because validation happen on running trigger action", () =>
				{
					triggerAction.PQ_FieldValue = $"<Collection.{clause}(\"1\").Z0_Number>";
					AssertNoWarnings("Result of \"1\" is not a True/False expression",
						triggerAction.PQ_FieldValueInfo);

					triggerAction.PQ_FieldValue = $"<Collection.{clause}(\"abc\").Z0_Number>";
					AssertNoWarnings("Result of \"abc\" is not a True/False expression",
						triggerAction.PQ_FieldValueInfo);

					triggerAction.PQ_FieldValue = $"<Collection.{clause}(\"<Z0_Code>\" == \"A\").Z0_Code>";
					AssertNoWarnings("Property Enterprise.MasterFiles.Business.Testing.DummyWithWorkflow.Z0_Number of type ZInt cannot be set with value 'A' in 'Set Field' trigger action.\r\nInput string was not in a correct format. (in ConvertFrom, value = 'A')",
							triggerAction.PQ_FieldValueInfo);

					triggerAction.PQ_FieldValue = $"<Collection.{clause}(\"<Z0_Code>\" == \"A\").RelatedDummy.Collection.{clause}(\"???\").Z0_Number>";
					AssertNoWarnings("Result of \"???\" is not a True/False expression",
						triggerAction.PQ_FieldValueInfo);
				});
			}
		}

		void SetupAndAssert_CheckPQ_FieldValue_WithoutCollection(string clause)
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var trigger = CreateWorkflowItemForJob(dummy);

			if (!(trigger is IWorkflowTrigger))
			{
				Assert("It's not possible to test the value produced by a template trigger since a job is required.", true);
			}
			else
			{
				var triggerAction = trigger.CompletionTriggerActionsCollection().AddNew();
				triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SetField;
				triggerAction.PQ_FieldName = "Collection.Z0_Number";
				triggerAction.PQ_FieldValue = string.Format("<Z0_Code.{0}(\"???\").Z0_Number>", clause);

				AssertNoWarnings("GIVEN FieldValue has First but no collection, WHEN validationg THEN should not error",
					triggerAction.PQ_FieldValueInfo);

				var notifications = new NotificationsForTest();
				FireTriggerAction(dummy, trigger, triggerAction, notifications);

				AssertContains(
					"WHEN processing trigger action THEN should show error",
					$@"Set field (FLD) macro evaluated to null. Macro: [<Z0_Code.{clause}(""???"").Z0_Number>]",
					notifications.ToString().Trim());
			}
		}

		protected override void SetUp()
		{
			AssertNotNull(DummyWorkflowDescriptor.Instance);
			base.SetUp();
		}

		#region DocumentSupportable

		sealed class DummyWithWorkflowDocumentSupportable : DummyWithWorkflow, IDocumentSupportable
		{
			public DummyWithWorkflowDocumentSupportable(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public override DocumentSupporter DocumentSupporter
			{
				get
				{
					return documentSupporter ?? (documentSupporter = new DocumentSupporterDodgy(this));
				}
			}
			DocumentSupporter documentSupporter;
		}

		class DocumentSupporterDodgy : DocumentSupporter
		{
			public DocumentSupporterDodgy(BusinessObject parentBusinessObject)
				: base(parentBusinessObject)
			{
				if (object.ReferenceEquals(parentBusinessObject, null))
				{
					throw new ArgumentNullException(nameof(parentBusinessObject));
				}
			}

			public override DocumentSupporterDataState GetDataStateBeforeRun(IStmMenuItem commandAboutToBeRun)
			{
				throw new NullReferenceException();
			}

			public override BusinessContext BusinessContext
			{
				get { throw new NotImplementedException(); }
			}

			public override ISecurityCheckpoint CustomisationSecurityCheckpoint
			{
				get { throw new NotImplementedException(); }
			}

			protected override DocumentEngineCore.DocWrappers.DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
			{
				throw new NotImplementedException();
			}

			protected override Constants.DataContext[] GetSupportedDataContexts()
			{
				throw new NotImplementedException();
			}
		}

		sealed class DocumentSupporterDodgyThatReturnsNullForGetDataStateBeforeRun : DocumentSupporterDodgy
		{
			public DocumentSupporterDodgyThatReturnsNullForGetDataStateBeforeRun(BusinessObject parentBusinessObject)
				: base(parentBusinessObject)
			{
			}

			public override DocumentSupporterDataState GetDataStateBeforeRun(IStmMenuItem commandAboutToBeRun)
			{
				return null;
			}
		}

		#endregion

		#endregion

		void FireTriggerAction(BusinessObject triggerJob, IBaseTrigger trigger, ProcessTaskNotification action, INotifications notifications = null)
		{
			using (WorkflowTriggerActionTracker.TrackTriggerActions(trigger.Factory))
			{
				ObjectFactory.Get<IWorkflowTriggerActionProcessorCreator>().GetSetFieldProcessor(new WorkflowTriggerActionSource(triggerJob, trigger, action, new ExampleLog(action), null)).Process(notifications);
				WorkflowTriggerActionTracker.OnAllActionsRun(trigger.Factory);
			}
		}

		protected TWorkflowItem CreateWorkflowItemForTemplateType(string workflowType)
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = workflowType;

			return CreateWorkflowItemForTemplate(template);
		}

		internal static IBaseTrigger CreateWorkflowItemForJobCore(IWorkflowProvider job, string workflowItemType = Constants.Workflow.WorkflowTriggerType)
		{
			switch (workflowItemType)
			{
				case Constants.Workflow.MilestoneType:
					return job.WorkflowItems.Milestones.AddNew();

				case Constants.Workflow.WorkflowTriggerType:
					return job.WorkflowItems.Triggers.AddNew();

				default:
					return job.WorkflowItems.AddNew();
			}
		}

		protected abstract TWorkflowItem CreateWorkflowItemForTemplate(ProcessTaskTemplate template, string workflowItemType = Constants.Workflow.WorkflowTriggerType);
		protected abstract TWorkflowItem CreateWorkflowItemForJob(IWorkflowProvider job, string workflowItemType = Constants.Workflow.WorkflowTriggerType);
	}

	#region Test Classes

	public class ProcessTaskNotificationForTest : ProcessTaskNotification
	{
		public ProcessTaskNotificationForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZBool Get_overrideEmail()
		{
			return overrideEmail;
		}
	}

	#endregion
}
