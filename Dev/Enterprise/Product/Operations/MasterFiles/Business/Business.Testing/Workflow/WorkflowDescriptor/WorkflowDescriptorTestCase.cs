using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.DataTransfer.Native.Integration;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.EConversation.Business;
using Enterprise.Environment;
using Enterprise.Freight.Integration;
using Enterprise.Integration;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business.UniversalData;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using WTG.WiseTechAcademy;
using WTG.WiseTechAcademy.TestFramework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestsSubclassesOf(typeof(WorkflowDescriptor), ExcludePrivate = true)]
	public abstract class WorkflowDescriptorTestCase<T> : NonPersistentBusinessObjectTestCase where T : WorkflowDescriptor, new()
	{
		public void TestPersonRelatedRecipientPartiesOnlyInIPersonalEmailAddressGetterForTriggerProvider()
		{
			if (!WorkflowDescriptor.WorkflowProviderType.IsAbstract && !(WorkflowDescriptor.GetBizOForTest(Factory) is IEmailAddressGetterForTrigger))
			{
				var sendDocumentTriggerParties = WorkflowDescriptor.GetMessagingTriggerPartiesList(WorkflowTriggerActionTypeConstants.Codes.SendDocument, null, null);

				AssertNull(sendDocumentTriggerParties.FindByPartyType(MessageRecipientPartyType.PersonalEmail));
				AssertNull(sendDocumentTriggerParties.FindByPartyType(MessageRecipientPartyType.PersonPrimaryWorkEmail));
				AssertNull(sendDocumentTriggerParties.FindByPartyType(MessageRecipientPartyType.PersonalFallbackPrimaryWorkEmail));
			}
			else
			{
				Assert(true);
			}
		}

		public void TestSupportsReleaseGroupRulesOnlyIfSupportsTasks()
		{
			if (!WorkflowDescriptor.SupportsTasks)
			{
				AssertEquals(WorkflowDescriptor.SupportsReleaseGroupRules, false);
			}
			else
			{
				Assert(true);
			}
		}

		#region WiseTech Academy Enrolment Trigger Action

		public void TestSupportsWtaEnrolmentTriggerAction()
		{
			if (ClientHookLoader.Instance?.Client == Clients.EDI)
			{
				Assert("WTA trigger action can optionally be supported in EDI client extension", true);
			}
			else
			{
				AssertEquals("Outside of EDI client extension, this trigger should never be supported.", expected: false, WorkflowDescriptor.SupportsWtaEnrolmentTriggerAction);
			}
		}

		public void TestWtaEnrolmentTriggerAction()
		{
			if (!WorkflowDescriptor.SupportsWtaEnrolmentTriggerAction)
			{
				Assert(true);
				return;
			}

			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				const string courseID = "123";
				var staff = MasterFilesTestHelper.CreateStaff(Factory, "DW", "Davey Wavey", "davey@wavey.com");
				var template = MasterFilesTestHelper.CreateUniversalTemplate(Factory, WorkflowDescriptors.GlbStaffDescriptorCode);
				var trigger = CreateTemplateTriggerForWtaEnrolment(template, courseID);
				var job = GetWorkflowProviderForWtaEnrolmentTrigger(staff);

				Factory.Save();

				var helper = new WiseTechAcademyApiTestHelper().SetupEnsureEnrolled(courseID, staff.GS_PER.ToGuid(), "Davey Wavey", "davey@wavey.com", HttpStatusCode.OK, null, sendEmailNotification: true, verifiable: true);
				var wtaClient = helper.CreateMockClient();

				using (ObjectFactory.Substitute<IWiseTechAcademyApiClient>(wtaClient))
				{
					MasterFilesTestHelper.AssertUniversalTriggerFired(trigger, job, expectingTriggerToHaveFired: false);
					RaiseEventForWtaEnrolmentTrigger(job);
					MasterFilesTestHelper.AssertUniversalTriggerFired(trigger, job, expectingTriggerToHaveFired: true);

					var logWalkerOutput = MasterFilesTestHelper.RunLogWalker();
					AssertContains(trigger.TriggerConditionValue, logWalkerOutput);

					helper.HandlerMock.VerifyAll();
				}
			}
		}

		public void TestWtaEnrolmentTriggerAction_WhenInvalidReferenceSpecified()
		{
			if (!WorkflowDescriptor.SupportsWtaEnrolmentTriggerAction)
			{
				Assert(true);
				return;
			}

			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				var staff = MasterFilesTestHelper.CreateStaff(Factory, "DW", "Davey Wavey", "davey@wavey.com");
				var template = MasterFilesTestHelper.CreateUniversalTemplate(Factory, WorkflowDescriptors.GlbStaffDescriptorCode);
				var trigger = CreateTemplateTriggerForWtaEnrolment(template, string.Empty);
				var job = GetWorkflowProviderForWtaEnrolmentTrigger(staff);

				Factory.Save();

				var helper = new WiseTechAcademyApiTestHelper().SetupEnsureEnrolled("123", staff.GS_PER.ToGuid(), "Davey Wavey", "davey@wavey.com", HttpStatusCode.OK, null, sendEmailNotification: true, verifiable: true);
				var wtaClient = helper.CreateMockClient();

				using (ObjectFactory.Substitute<IWiseTechAcademyApiClient>(wtaClient))
				{
					MasterFilesTestHelper.AssertUniversalTriggerFired(trigger, job, expectingTriggerToHaveFired: false);
					RaiseEventForWtaEnrolmentTrigger(job);
					MasterFilesTestHelper.AssertUniversalTriggerFired(trigger, job, expectingTriggerToHaveFired: true);

					var logWalkerOutput = MasterFilesTestHelper.RunLogWalker();
					AssertContains("Empty unitId not permitted", logWalkerOutput);
				}
			}

			ErrorReporter.Clear();
		}

		public void TestWtaEnrolmentTriggerAction_WhenUnsuccessfulResponseReceived()
		{
			if (!WorkflowDescriptor.SupportsWtaEnrolmentTriggerAction)
			{
				Assert(true);
				return;
			}

			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				const string courseID = "123";
				var staff = MasterFilesTestHelper.CreateStaff(Factory, "DW", "Davey Wavey", "davey@wavey.com");
				var template = MasterFilesTestHelper.CreateUniversalTemplate(Factory, WorkflowDescriptors.GlbStaffDescriptorCode);
				var trigger = CreateTemplateTriggerForWtaEnrolment(template, courseID);
				var job = GetWorkflowProviderForWtaEnrolmentTrigger(staff);

				Factory.Save();

				var helper = new WiseTechAcademyApiTestHelper().SetupEnsureEnrolled(courseID, staff.GS_PER.ToGuid(), "Davey Wavey", "davey@wavey.com", HttpStatusCode.InternalServerError, "Invalid response from WTA", sendEmailNotification: true, verifiable: true);
				var wtaClient = helper.CreateMockClient();

				using (ObjectFactory.Substitute<IWiseTechAcademyApiClient>(wtaClient))
				{
					MasterFilesTestHelper.AssertUniversalTriggerFired(trigger, job, expectingTriggerToHaveFired: false);
					RaiseEventForWtaEnrolmentTrigger(job);
					MasterFilesTestHelper.AssertUniversalTriggerFired(trigger, job, expectingTriggerToHaveFired: true);

					var logWalkerOutput = MasterFilesTestHelper.RunLogWalker();
					AssertContains("Invalid response from WTA", logWalkerOutput);

					helper.HandlerMock.VerifyAll();
				}
			}
		}

		public void TestWtaEnrolmentTriggerAction_WhenTriggerActionReferenceUnset()
		{
			if (!WorkflowDescriptor.SupportsWtaEnrolmentTriggerAction)
			{
				Assert(true);
				return;
			}

			using (ObjectFactory.Substitute(Mock.Of<INudgingController>()))
			{
				const string courseID = "123";
				var staff = MasterFilesTestHelper.CreateStaff(Factory, "DW", "Davey Wavey", "davey@wavey.com");
				var template = MasterFilesTestHelper.CreateUniversalTemplate(Factory, WorkflowDescriptors.GlbStaffDescriptorCode);
				var trigger = CreateTemplateTriggerForWtaEnrolment(template, courseID);
				var triggerAction = trigger.TriggerActions.Cast<ProcessTaskNotification>().Single();

				AssertEquals("123", triggerAction.PQ_ActionReference);

				triggerAction.PQ_ActionReference = ZString.Empty;
				AssertEquals(ZString.Empty, triggerAction.PQ_ActionReference);

				triggerAction.PQ_ActionReference = "456";
				AssertEquals("456", triggerAction.PQ_ActionReference);
			}
		}

		protected virtual ITemplateTrigger CreateTemplateTriggerForWtaEnrolment(ProcessTaskTemplate template, string unitId)
		{
			throw MustOverrideForWtaTriggerAction();
		}

		protected virtual IWorkflowProvider GetWorkflowProviderForWtaEnrolmentTrigger(GlbStaff staffToBeEnrolled)
		{
			throw MustOverrideForWtaTriggerAction();
		}

		protected virtual void RaiseEventForWtaEnrolmentTrigger(IWorkflowProvider job)
		{
			throw MustOverrideForWtaTriggerAction();
		}

		Exception MustOverrideForWtaTriggerAction()
			=> new NotImplementedException($"This method must be implemented if your WorkflowDescriptor returns true for {nameof(WorkflowDescriptor.SupportsWtaEnrolmentTriggerAction)}");

		#endregion

		#region CO2e

		public void TestOverridesCO2eWorkflowDescriptorTestMethods()
		{
			if (DoesWorkflowDescriptorSupportCO2e)
			{
				CombineAssertions($"Should override all CO2e virtual test methods when WorkflowDescriptor supports CO2e functionality.", () =>
				{
					AssertNoExceptionThrown(nameof(CreateTaskTemplateForCO2eTests), () => CreateTaskTemplateForCO2eTests());
					AssertNoExceptionThrown(nameof(CreateWithValidDataForCO2eTests), () => CreateWithValidDataForCO2eTests());
					AssertNoExceptionThrown(nameof(CreateWithInvalidDataForCO2eTests), () => CreateWithInvalidDataForCO2eTests());
				});
			}
			else
			{
				Assert(true);
			}
		}

		public void TestGetWorkflowTriggerActionTypesIncludeGHG()
		{
			// Pre-condition
			if (!DoesWorkflowDescriptorSupportCO2e)
			{
				Assert(true);
				return;
			}

			// Arrange
			var template = CreateTaskTemplateForCO2eTests();
			var trigger = template.WorkflowItems.Triggers.AddNew();

			SetupAndRunCO2eTest(greenhouseGasRegistryValue: true, () =>
			{
				// Act
				var triggerActionTypes = WorkflowDescriptor.GetWorkflowTriggerActionTypes(trigger, template);

				// Assert
				Assert("Should contain GHG action when registry is enabled", triggerActionTypes.ContainsCode("GHG"));
			});

			SetupAndRunCO2eTest(greenhouseGasRegistryValue: false, () =>
			{
				// Act
				var triggerActionTypes = WorkflowDescriptor.GetWorkflowTriggerActionTypes(trigger, template);

				// Assert
				Assert("Should not contain GHG action when registry is disabled", !triggerActionTypes.ContainsCode("GHG"));
			});
		}

		public void TestGetWorkflowTriggerAction_ForGHG_FailedWhenMissingMandatoryData()
		{
			// Pre-condition
			if (!DoesWorkflowDescriptorSupportCO2e)
			{
				Assert(true);
				return;
			}

			SetupAndRunCO2eTest(greenhouseGasRegistryValue: true, () =>
			{
				// Arrange
				var co2eCalculationSupporter = CreateWithInvalidDataForCO2eTests();
				var triggerType = WorkflowTriggerActionTypeConstants.Codes.SendCalculateCO2EmissionRequest;

				// Act
				AssertRequestForCO2eSupporter(co2eCalculationSupporter as IWorkflowProvider, triggerType, out var logger);

				// Assert
				AssertStartsWith("logger results from processor.Process()",
					"The greenhouse gas emissions calculation cannot be requested because the following mandatory input is missing or invalid:",
					logger.AsString);
			});
		}

		public void TestGetWorkflowTriggerAction_ForGHG_RequestSent()
		{
			// Pre-condition
			if (!DoesWorkflowDescriptorSupportCO2e)
			{
				Assert(true);
				return;
			}

			SetupAndRunCO2eTest(greenhouseGasRegistryValue: true, () =>
			{
				// Arrange
				var co2eCalculationSupporter = CreateWithValidDataForCO2eTests();
				var triggerType = WorkflowTriggerActionTypeConstants.Codes.SendCalculateCO2EmissionRequest;

				// Act
				AssertRequestForCO2eSupporter(co2eCalculationSupporter as IWorkflowProvider, triggerType, out var logger);

				// Assert
				AssertMultilineASCIIEquals("logger results from processor.Process()", string.Empty, logger.AsString);

				var newFactory = new BusinessObjectFactory();
				var messages = newFactory.Load<IEDIMessage>(new ZQuery());
				AssertEquals(1, messages.Length);
				var message = messages[0];
				Assert(message.IsInDatabase);
				CombineAssertions("EDI Message sent", () =>
				{
					AssertEquals("message.EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
					AssertEquals("message.EM_MessageSubType", EDIMessageSubTypeList.Codes.XmlUniversalShipment, message.EM_MessageSubType);
					AssertEquals("message.EM_ReceiveTransmit", EDICommunicationsModeCommsDirectionList.Codes.Transmit, message.EM_ReceiveTransmit);
					AssertEquals("message.EM_Status", EDIMessageStatusList.Codes.Sent, message.EM_Status);
					AssertContains("message.EM_MessageText", "UniversalShipment", message.EM_MessageText);
				});

				var interchange = newFactory.Load<IEDIInterchange>(message.EM_EI);
				Assert(interchange.IsInDatabase);
				CombineAssertions("EDI Interchange", () =>
				{
					AssertNotNull(interchange);
					AssertEquals("EMISSION_CALCULATOR", interchange.EI_To);
				});
			});
		}

		void AssertRequestForCO2eSupporter(IWorkflowProvider co2eCalculationSupporter, ZString triggerType, out NotificationBuffer logger)
		{
			var trigger = co2eCalculationSupporter.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Validate for Customs Messaging";
			trigger.TriggerConditions.TriggerEventCode = AutoEvents.CustomisableEvent00Code;
			trigger.TriggerConditions.TriggerFiredCountdown = 1;

			var triggerAction = trigger.ProcessTaskNotifications.AddNew();
			triggerAction.PQ_TriggerType = triggerType;
			co2eCalculationSupporter.Logs.AddNew(AutoEvents.CustomisableEvent00);
			Factory.Save();

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.WorkflowTriggerEventCode);
			query.AddToFilter(StmALogSchema.SL_Parent, trigger.PK);
			var triggerLogs = Factory.Load<StmALog>(query);
			AssertEquals("Precondition: Customisable Events linked to our Trigger", 1, triggerLogs.Length);

			var triggerLog = triggerLogs[0];
			var queuedLog = new QueuedLogForTesting(triggerLog, trigger);
			var processor = WorkflowDescriptor.GetWorkflowTriggerAction(triggerAction, queuedLog);
			logger = new NotificationBuffer();
			using (Factory.AddDisposableService())
			{
				processor.Process(logger);
				Factory.Save();
			}

			AssertEquals("Trigger Count Reduced to 0", (short)0, trigger.P9_TriggerFiredCountdown);
		}

		protected virtual void SetupAndRunCO2eTest(bool greenhouseGasRegistryValue, Action action)
		{
			var mockCO2eFeatureControl = new Mock<ICO2eFeatureControlHelper>();
			mockCO2eFeatureControl.Setup(m => m.Enabled).Returns(greenhouseGasRegistryValue);

			using (ObjectFactory.Substitute(mockCO2eFeatureControl.Object))
			{
				action();
			}
		}

		protected virtual ProcessTaskTemplate CreateTaskTemplateForCO2eTests() => throw MustOverrideForGHGTriggerAction();

		protected virtual ICO2eCalculationSupporter CreateWithValidDataForCO2eTests() => throw MustOverrideForGHGTriggerAction();

		protected virtual ICO2eCalculationSupporter CreateWithInvalidDataForCO2eTests() => throw MustOverrideForGHGTriggerAction();

		bool DoesWorkflowDescriptorSupportCO2e => WorkflowDescriptor.WorkflowProviderType.GetInterface(nameof(ICO2eCalculationSupporter)) != null;

		Exception MustOverrideForGHGTriggerAction()
			=> new NotImplementedException($"This method must be implemented if your WorkflowDescriptor returns true for {nameof(WorkflowDescriptor.SupportsSendCalculateCO2EmissionRequest)}");

		#endregion

		#region eConversation Trigger Actions

		public void TestSupportsAddingEConversationMessages()
		{
			if (WorkflowDescriptor.SupportsAddEConversationMessages)
			{
				var workflowProvider = GetParentsWithConfiguredOrganisationPartiesForTest().First();
				var message = FormattableString.Invariant($"In order to add eConversation messages with the {WorkflowTriggerActionTypeConstants.Codes.AddEConversationMessage} and {WorkflowTriggerActionTypeConstants.Codes.AddInternalEConversationMessage} trigger actions, the parent business object needs to implement {nameof(IConversationProvider)}.");

				Assert(message, workflowProvider is IConversationProvider);

				AssertNoExceptionThrown("It's important to be able to save the factory with no exception because accessing eConversation on a job before it's saved will throw an exception.", Factory.Save);
			}
			else
			{
				Assert(true);
			}
		}

		public void TestAddEConversationMessageTriggerAction_WhenMessageTextIsEmpty_ShouldLogWarning()
		{
			if (WorkflowDescriptor.SupportsAddEConversationMessages)
			{
				MasterFilesTestHelper.ClearWorkflowTables();
				var participantStaff1 = Factory.NewWithValidTestData<GlbStaff>();
				participantStaff1.GS_EmailAddress = "TapeABunchOfCats@together.com";

				var workflowProvider = GetParentsWithConfiguredOrganisationPartiesForTest().First();
				var bizo = (BusinessObject)workflowProvider;
				var conversationProvider = (IConversationProvider)workflowProvider;

				var trigger = MasterFilesTestHelper.CreateTrigger(workflowProvider, Events.TagWasAddedOrRemovedCode);
				var triggerAction = MasterFilesTestHelper.CreateTriggerAction(trigger, WorkflowTriggerActionTypeConstants.Codes.AddEConversationMessage, emailText: "");

				Factory.Save();

				conversationProvider.eConversation.Participants.AddNewParticipant(participantStaff1);
				bizo.GetLogs().AddNew(Events.TagWasAddedOrRemoved);

				Factory.Save();

				AssertMatch(new Regex("Warning:.*The eConversation message to be sent was empty. This is not supported."), MasterFilesTestHelper.RunLogWalker());
			}
			else
			{
				Assert(true);
			}
		}

		public void TestAddEConversationMessageTriggerAction()
		{
			AssertAddEConversationMessageTriggerAction(WorkflowTriggerActionTypeConstants.Codes.AddEConversationMessage, eConversationMessageShouldBeInternal: false);
		}

		public void TestAddEConversationMessageTriggerAction_ForInternalMessage()
		{
			AssertAddEConversationMessageTriggerAction(WorkflowTriggerActionTypeConstants.Codes.AddInternalEConversationMessage, eConversationMessageShouldBeInternal: true);
		}

		public void TestAddEConversationMessageTriggerAction_WhenTriggerDefinedOnWorkflowTemplate_ShouldSendCorrectMessage()
		{
			AssertAddEConversationMessageTriggerAction(WorkflowTriggerActionTypeConstants.Codes.AddEConversationMessage, eConversationMessageShouldBeInternal: false, createTriggerOnWorkflowTemplate: true);
		}

		public void TestAddEConversationMessageTriggerAction_ForInternalMessage_WhenTriggerDefinedOnWorkflowTemplate_ShouldSendCorrectMessage()
		{
			AssertAddEConversationMessageTriggerAction(WorkflowTriggerActionTypeConstants.Codes.AddInternalEConversationMessage, eConversationMessageShouldBeInternal: true, createTriggerOnWorkflowTemplate: true);
		}

		public void TestAddEConversationMessageTriggerAction_WhenTriggerDefinedOnUniversalWorkflowTemplate_ShouldSendCorrectMessage()
		{
			AssertAddEConversationMessageTriggerAction(WorkflowTriggerActionTypeConstants.Codes.AddEConversationMessage, eConversationMessageShouldBeInternal: false, createTriggerOnUniversalWorkflowTemplate: true);
		}

		public void TestAddEConversationMessageTriggerAction_ForInternalMessage_WhenTriggerDefinedOnUniversalWorkflowTemplate_ShouldSendCorrectMessage()
		{
			AssertAddEConversationMessageTriggerAction(WorkflowTriggerActionTypeConstants.Codes.AddInternalEConversationMessage, eConversationMessageShouldBeInternal: true, createTriggerOnUniversalWorkflowTemplate: true);
		}

		void AssertAddEConversationMessageTriggerAction(string triggerActionType, bool eConversationMessageShouldBeInternal, bool createTriggerOnWorkflowTemplate = false, bool createTriggerOnUniversalWorkflowTemplate = false)
		{
			if (!WorkflowDescriptor.SupportsAddEConversationMessages)
			{
				Assert(true);
				return;
			}

			MasterFilesTestHelper.ClearWorkflowTables();

			AssertNotNull(FormattableString.Invariant($"Please override {nameof(TextPropertyWhichCanBeRepresentedByAMacro)} so a macro can be defined for testing."), TextPropertyWhichCanBeRepresentedByAMacro);

			var loginStaff = Factory.NewWithValidTestData<GlbStaff>();
			loginStaff.GS_Code = "TRP";
			loginStaff.GS_FullName = "I just tripled my productivity!";

			var participantStaff1 = Factory.NewWithValidTestData<GlbStaff>();
			var participantStaff2 = Factory.NewWithValidTestData<GlbStaff>();

			participantStaff1.GS_EmailAddress = "we@usually.just";
			participantStaff2.GS_EmailAddress = "TapeABunchOfCats@together.com";

			var workflowProvider = GetParentsWithConfiguredOrganisationPartiesForTest().First();
			var bizo = (BusinessObject)workflowProvider;
			var conversationProvider = (IConversationProvider)workflowProvider;

			bizo[TextPropertyWhichCanBeRepresentedByAMacro] = "Keep sliding";

			void CreateTrigger(IWorkflowProvider provider)
			{
				var trigger = MasterFilesTestHelper.CreateTrigger(provider, Events.TagWasAddedOrRemovedCode);

				CreateTriggerAction(trigger);
			}

			void CreateTriggerAction(IBaseTrigger trigger)
			{
				var triggerAction = MasterFilesTestHelper.CreateTriggerAction(trigger, triggerActionType, emailText: $@"
""I work on a sliding scale. I can go as low as $30 an hour.""
""(*{TextPropertyWhichCanBeRepresentedByAMacro.Name}*)""
");
			}

			if (createTriggerOnWorkflowTemplate)
			{
				var existingTemplate = Factory.LoadTop1<ProcessTaskTemplate>(new ZQuery(ProcessTaskTemplateSchema.P0_ProcessType, WorkflowDescriptor.Code).AddToFilter(ProcessTaskTemplateSchema.P0_IsUniversal, false));

				if (existingTemplate != null)
				{
					existingTemplate.Delete();
				}

				var template = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, WorkflowDescriptor.Code, name: existingTemplate == null ? "Two" : "Three!!");

				CreateTrigger(template);
			}
			else if (createTriggerOnUniversalWorkflowTemplate)
			{
				var existingTemplate = Factory.LoadTop1<ProcessTaskTemplate>(new ZQuery(ProcessTaskTemplateSchema.P0_ProcessType, WorkflowDescriptor.Code).AddToFilter(ProcessTaskTemplateSchema.P0_IsUniversal, true));

				if (existingTemplate != null)
				{
					existingTemplate.P0_IsActive = false;
				}

				var template = MasterFilesTestHelper.CreateUniversalTemplate(Factory, WorkflowDescriptor.Code, name: existingTemplate == null ? "Two" : "Three!!");
				var trigger = MasterFilesTestHelper.CreateTemplateTrigger(template, Events.TagWasAddedOrRemovedCode);

				CreateTriggerAction(trigger);
			}
			else
			{
				CreateTrigger(workflowProvider);
			}

			Factory.Save();

			if (createTriggerOnWorkflowTemplate || createTriggerOnUniversalWorkflowTemplate)
			{
				workflowProvider.ApplyWorkflowTemplates();

				AssertContainsExactElementsInAnyOrder("Defined template should be applicable to this workflow provider", new[] { Events.TagWasAddedOrRemovedCode }, workflowProvider.WorkflowItems.TriggersIncludingRelated.Cast<ProcessTask>().Select(t => t.P9_SE_NKMilestoneEvent));
			}

			conversationProvider.eConversation.Participants.AddNewParticipant(participantStaff1);
			conversationProvider.eConversation.Participants.AddNewParticipant(participantStaff2);

			Factory.Save();

			using (Env.SetTemporaryUserContext(loginStaff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
			{
				bizo.GetLogs().AddNew(Events.TagWasAddedOrRemoved);
			}

			Factory.Save();

			AssertEquals(0, Env.AllEmailsCreated.Count());

			MasterFilesTestHelper.RunLogWalker();

			const string messageWithEvaluatedMacros = @"
""I work on a sliding scale. I can go as low as $30 an hour.""
""Keep sliding""
";

			var newFactory = bizo.Factory.CreateNewFactory();
			bizo = newFactory.Load(bizo.GetType(), bizo.PK);

			var message = ((IConversationProvider)bizo).eConversation.Messages.SingleOrDefault(m => m.SenderCode == loginStaff.GS_Code); // Messages marked as 'IsSystemMessage' are added when a participant is added to the conversation.
			var expectedDisplayNote = eConversationMessageShouldBeInternal ? "[Internal System Message]" : "[System Message]";

			AssertNotNull("An eConversation message should have been added to the job by Log Walker", message);

			CombineAssertions("eConversation message details", () =>
			{
				AssertEquals("SenderDisplayName", loginStaff.GS_FullName, message.SenderDisplayName);
				AssertMultilineASCIIEquals("Body", messageWithEvaluatedMacros, message.Body);
				AssertEquals("AdditionalNoteForDisplay", expectedDisplayNote, message.AdditionalNoteForDisplay);
			});

			var missingEmailsMessage = $@"If emails aren't getting created here, you probably haven't poked your eConversation instance to send emails when they should be.
This really should happen inside the business layer of eConversation but for the moment it isn't. Therefore you need to do this yourself, sadly. :( TODO in WI00198145.
See uses of {nameof(EConversationEmailBuilder)}.{nameof(EConversationEmailBuilder.GenerateAndQueueEmailNotifications)} outside Enterprise.EConversation assembly as an example of how to do this.";

			var participantEmailsAddedByDefault = GetAdditionalEConversationParticipantEmailAddressesAddedByDefault(workflowProvider, eConversationMessageShouldBeInternal);
			var expectedParticipantEmailAddresses = new[] { "we@usually.just", "TapeABunchOfCats@together.com" }.Concat(participantEmailsAddedByDefault).ToArray();
			var emails = Env.AllEmailsCreated.ToArray();

			CombineAssertions(missingEmailsMessage, () =>
			{
				AssertContainsExactElementsInAnyOrder(expectedParticipantEmailAddresses, emails.SelectMany(email => email.Recipients.Cast<RecipientDef>()).Select(r => r.Email));
				AssertEquals("Should send an email to each participant separately", expectedParticipantEmailAddresses.Length, emails.Length);

				foreach (var email in emails)
				{
					AssertContains("I work on a sliding scale. I can go as low as $30 an hour.", email.Body);
				}
			});

			Env.ClearAllEmailsCreated();
		}

		protected virtual SchemaStringColumn TextPropertyWhichCanBeRepresentedByAMacro => null;

		protected virtual IEnumerable<string> GetAdditionalEConversationParticipantEmailAddressesAddedByDefault(IWorkflowProvider workflowProvider, bool isForInternalEConversationMessage) => Enumerable.Empty<string>();

		#endregion

		#region GetDateTimeFromParent

		[TestDate(2019, 1, 1)]
		[TestUtcOffset(12, 0, 0)]
		public void TestGetDateTimeOffsetFromParent_ShouldReturnTimeInUTCAccordingToRelevantContext()
		{
			if (WorkflowDescriptor.EstimateDefaultedFromList.Count == 0)
			{
				Assert(true);
				return;
			}

			var branch = GlbCompany.GetCurrentCompany(Factory).Branches.AddNew();
			branch.FillWithValidTestData();
			branch.GB_RL_NKHomePort = "NZAKL";

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				CombineAssertions($"For the various properties that this workflow type supports through {nameof(WorkflowDescriptor.EstimateDefaultedFromList)}, it should be possible to determine the correct UTC time. This means that if the local time is in a specific timezone, like shipment ETA, the UTC conversion shouldn't be based on logged-in branch UNLOCO, but rather should be based on the actual timezone where the local time is relevant. Please override {nameof(GetUtcOffsetForDateTimeSourceType)} to provide the expected UTC offset for each date/time source.", () =>
				{
					foreach (ICodeDescription cdp in WorkflowDescriptor.EstimateDefaultedFromList)
					{
						var workflowProvider = GetParentForGettingDateTimeOffset();
						var workflowItem = workflowProvider.WorkflowItems.Triggers.AddNew();
						workflowItem.P9_Description = "FOR TEST";

						var localTime = GetLocalTimeForDateTimeOffsetTest(workflowProvider);
						var utcOffset = GetUtcOffsetForDateTimeSourceType(workflowProvider, cdp.Code);
						var expectedValue = utcOffset != TimeSpan.Zero ? new ZDateTimeOffset(localTime, DateTimeKind.Local, utcOffset) : new ZDateTimeOffset(localTime, DateTimeKind.Utc);

						SetDateTimeSourcePropertyValue(workflowProvider, cdp.Code, localTime);

						var result = WorkflowDescriptor.GetDateTimeFromParent(workflowItem, cdp.Code, ZDateTime.Empty);
						var message = $"EstimateDefaultedFromList value {cdp.Code} ({cdp.Description}): ";

						AssertEquals(message + "UTC time", expectedValue.ToUtcDateTime(), result.ToUtcDateTime());
						AssertEquals(message + "local time", expectedValue.ToDateTime(), result.ToDateTime());
					}
				});
			}
		}

		[TestDate(2019, 02, 01)]
		[TestTimeZoneUNLOCO("AUBNE")]
		public virtual void TestEstimateDates_DifferentTimezones()
		{
			TestDateAttribute.UseUNLOCO = true;
			if (WorkflowDescriptor.EstimateDefaultedFromList.Count == 0)
			{
				Assert(true);
				return;
			}

			var bneBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_Code, "BNE"));
			var sinBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_Code, "SIN"));
			var sinUtcOffset = new RefUNLOCO.Loader(Factory).Load("SGSIN").StandardZoneUTCOffset;
			var bneUtcOffset = new RefUNLOCO.Loader(Factory).Load("AUBNE").StandardZoneUTCOffset;
			AssertNotEquals("Precondition, SIN and BNE should have a different UTC offset", sinUtcOffset, bneUtcOffset);

			foreach (ICodeDescription cdp in WorkflowDescriptor.EstimateDefaultedFromList)
			{
				ZGuid taskPK = ZGuid.Empty;
				var expectedDateUtc = ZDateTime.Empty;

				using (Env.SetTemporaryUserContext(new UserContext(Env.CurrentUser.LoginName, bneBranch.PK.ToGuid(), Env.CurrentDepartment.PK)))
				{
					var workflowProvider = GetParentForGettingDateTimeOffset();
					var utcOffset = GetUtcOffsetForDateTimeSourceType(workflowProvider, cdp.Code);

					var task = workflowProvider.WorkflowItems.Tasks.AddNew();
					task.P9_Type = "UDF";
					task.P9_Description = "FOR TEST";
					task.P9_EstimatedDefaultedFrom = cdp.Code;
					task.P9_EstimatedDefaultTimeDelta = TimeSpan.FromHours(0);
					task.P9_RecalculateScheduledDate = true;
					taskPK = task.PK;
					var localDate = GetLocalTimeForDateTimeOffsetTest(workflowProvider);
					SetDateTimeSourcePropertyValue(workflowProvider, cdp.Code, localDate);

					Factory.Save();
					expectedDateUtc = localDate.Add(-utcOffset);
					CombineAssertions($"{cdp.Code}", () =>
					{
						AssertEquals("In a different timezone, UTC date should be a same", expectedDateUtc, task.P9_ScheduledDateUtc);
						AssertEquals("Scheduled date doesn't change", localDate, task.P9_ScheduledDate);
						AssertEquals("Schedule date depends on the location returned by GetScheduleDateTimeAndLocationForTimeZone", expectedDateUtc.ToLocalBranchTime(), task.P9_ScheduledDateLocalForBinding);
					});
				}

				using (Env.SetTemporaryUserContext(new UserContext(Env.CurrentUser.LoginName, sinBranch.PK.ToGuid(), Env.CurrentDepartment.PK)))
				{
					var newFactory = new BusinessObjectFactory();
					var task = newFactory.Load<ProcessTask>(taskPK);
					task.P9_Description = "NEW DESCRIPTION";
					newFactory.Save();

					CombineAssertions($"{cdp.Code}", () =>
					{
						AssertEquals("In a different timezone, UTC date should be a same", expectedDateUtc, task.P9_ScheduledDateUtc);
						AssertEquals("Schedule date depends on the location returned by GetScheduleDateTimeAndLocationForTimeZone", expectedDateUtc.ToLocalBranchTime(), task.P9_ScheduledDateLocalForBinding);
					});
				}

				var query = new ZQuery(StmALogSchema.SL_Parent, taskPK)
					.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.CalculatedEstimateCode);
				var logs = Factory.Load<StmALog>(query);
				AssertEquals("Only expecting 1 CEE event", 1, logs.Length);
			}
		}

		protected virtual IWorkflowProvider GetParentForGettingDateTimeOffset() => GetParentsWithConfiguredOrganisationPartiesForTest().First();

		protected virtual ZDateTime GetLocalTimeForDateTimeOffsetTest(IWorkflowProvider workflowProvider) => new ZDateTime(2019, 2, 1);

		protected virtual TimeSpan GetUtcOffsetForDateTimeSourceType(IWorkflowProvider workflowProvider, string dateTimeSourceType)
		{
			return TimeSpan.FromHours(12); // NZAKL timezone by default since that's the currently logged-in branch.
		}

		protected virtual void SetDateTimeSourcePropertyValue(IWorkflowProvider workflowProvider, string dateTimeSourceType, ZDateTime localTime)
		{
			var message = $@"For workflow types which include values in the {nameof(WorkflowDescriptor.EstimateDefaultedFromList)} property, this method must be overridden, and the value in {nameof(localTime)} parameter should be set on the property that corresponds to {dateTimeSourceType} ({WorkflowDescriptor.EstimateDefaultedFromList.GetDescriptionFromCode(dateTimeSourceType)}).";

			throw new NotImplementedException(message);
		}

		#endregion

		#region Buffer Management

		[GuiTest]
		public void TestAllSupportedJobTypes()
		{
			RunBufferManagementSupportedJobTypeTest((runner, descriptor) => runner.RunTestAllSupportedJobTypes(descriptor));
		}

		[GuiTest]
		public void TestAllSupportedJobTypes_ShouldDisplayOnVisualBoard()
		{
			RunBufferManagementSupportedJobTypeTest((runner, descriptor) => runner.RunTestAllSupportedJobTypes_ShouldDisplayOnVisualBoard(descriptor));
		}

		[GuiTest]
		public void TestAllSupportedJobTypes_ShouldPerformFilterOnVisualBoard()
		{
			RunBufferManagementSupportedJobTypeTest((runner, descriptor) => runner.RunTestAllSupportedJobTypes_ShouldPerformFilterOnVisualBoard(descriptor));
		}

		[GuiTest]
		public void TestAllSupportedJobTypes_ShouldDisplayManagementTab()
		{
			RunBufferManagementSupportedJobTypeTest((runner, descriptor) => runner.RunTestAllSupportedJobTypes_ShouldDisplayManagementTab(descriptor));
		}

		[GuiTest]
		public void TestInactiveRelatedJobType_ShouldNotDisplayManagementTab()
		{
			RunBufferManagementSupportedJobTypeTest((runner, descriptor) => runner.RunTestInactiveRelatedJobType_ShouldNotDisplayManagementTab(descriptor));
		}

		[GuiTest]
		public void TestActiveRelatedJobType_ShouldDisplayManagementTab()
		{
			RunBufferManagementSupportedJobTypeTest((runner, descriptor) => runner.RunTestActiveRelatedJobType_ShouldDisplayManagementTab(descriptor));
		}

		public void TestAllSupportedJobTypes_ShouldSafelyCreateFetchHints()
		{
			RunBufferManagementSupportedJobTypeTest((runner, descriptor) => runner.RunTestAllSupportedJobTypes_ShouldSafelyCreateFetchHints(descriptor));
		}

		public void TestAllSupportedJobTypes_ShouldBeAbleToSaveJobNetworks()
		{
			RunBufferManagementSupportedJobTypeTest((runner, descriptor) => runner.RunTestAllSupportedJobTypes_ShouldBeAbleToSaveJobNetworks(descriptor));
		}

		[GuiTest]
		public void TestAllSupportedJobTypes_ShouldBeWorkQueuable_ShouldNotDieHorribly()
		{
			RunBufferManagementSupportedJobTypeTest((runner, descriptor) => runner.RunTestAllSupportedJobTypes_ShouldBeWorkQueuable_ShouldNotDieHorribly(descriptor));
		}

		void RunBufferManagementSupportedJobTypeTest(Action<ISupportedWorkflowTypesForAllDescriptorsTestRunner, IWorkflowDescriptor> testAction)
		{
			ObjectFactory.Get<IBMTestHelper>().EnableBMSInRegistry();

			if (ShouldRunBufferManagementSupportedJobTypeTests)
			{
				var runner = ObjectFactory.Get<ISupportedWorkflowTypesForAllDescriptorsTestRunner>();
				testAction.Invoke(runner, WorkflowDescriptor);
			}
			else
			{
				Assert(true);
			}
		}

		protected virtual bool ShouldRunBufferManagementSupportedJobTypeTests => WorkflowDescriptor.SupportsBufferManagement && ObjectFactory.Get<IWorkflowDescriptorList>().ContainsCode(WorkflowDescriptor.Code);

		#endregion

		public virtual void TestIEventPublisherPerformance()
		{
			Assert(@"This test has detected your descriptor implements IEventPublisher.
You must override this test in your testcase to prove you have considered performance of your SQL Query.
Please ensure that you test with a large representative data set. See workitem WI00116587 for an example of getting it wrong.",
!typeof(IEventPublisher).IsAssignableFrom(typeof(T)));
		}

		public void TestGetWorkflowTriggerActionForUniversalShipmentXML()
		{
			var descriptor = new T();
			var actionTypes = descriptor.GetWorkflowTriggerActionTypes();

			if (!descriptor.SupportsWorkflowTriggerActionUniversalShipmentXML)
			{
				AssertEquals("actionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML)"
					, false
					, actionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML));
			}
			else
			{
				AssertEquals("actionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML)"
					, true
					, actionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML));

				var action = GetProcessTaskNotificationWithRealParent();
				action.Item1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
				AssertNotNull("You must implement GetUniversalShipmentDataObjectWriter(action) when you override SupportsWorkflowTriggerActionUniversalShipmentXML to 'true'."
					, descriptor.GetUniversalShipmentDataObjectWriter(ObjectFactory.New<IDataWritingManager>(new ActionWrapper(action.Item1, action.Item2, null), null, null, null)));
			}
		}

		public virtual void TestGetWorkflowTriggerActionForUniversalEventXML()
		{
			var descriptor = new T();
			var actionTypes = descriptor.GetWorkflowTriggerActionTypes();

			if (!descriptor.SupportsWorkflowTriggerActionUniversalEventXML)
			{
				AssertEquals("actionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML)"
					, false
					, actionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML));
			}
			else
			{
				AssertEquals("actionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML)"
					, true
					, actionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML));

				var (action, parent) = GetProcessTaskNotificationWithRealParent();
				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML;
				AssertNotNull("You must implement GetUniversalEventDataObjectWriter(action) when you override SupportsWorkflowTriggerActionUniversalEventXML to 'true'."
					, descriptor.GetUniversalEventDataObjectWriter(ObjectFactory.New<IDataWritingManager>(new ActionWrapper(action, parent, null), null, null, null)));
			}
		}

		public void TestGetWorkflowTriggerActionForUniversalEventCollectionXML()
		{
			var descriptor = new T();
			var actionTypes = descriptor.GetWorkflowTriggerActionTypes();

			if (!descriptor.SupportsWorkflowTriggerActionUniversalEventCollectionXML)
			{
				Assert(
					"actionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventCollectionXML)",
					!actionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventCollectionXML));
			}
			else
			{
				Assert(
					"actionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventCollectionXML)",
					actionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventCollectionXML));

				var (action, biz) = GetProcessTaskNotificationWithRealParent();
				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventCollectionXML;
				AssertNotNull(
					"You must implement GetUniversalEventCollectionDataObjectWriter(IDataWritingManager) when you override SupportsWorkflowTriggerActionUniversalEventCollectionXML to 'true'.",
					descriptor.GetUniversalEventCollectionDataObjectWriter(ObjectFactory.New<IDataWritingManager>(new ActionWrapper(action, biz, null), null, null, null)));
			}
		}

		public ITopLevelDataObjectWriter GetUniversalEventDataObjectWriter(WorkflowDescriptor descriptor, IDataWritingManager manager)
		{
			return descriptor.GetUniversalEventDataObjectWriter(manager);
		}

		public void TestGetWorkflowTriggerActionForUniversalActivityXML()
		{
			var descriptor = new T();
			var actionTypes = descriptor.GetWorkflowTriggerActionTypes();
			var isActivitySupported = descriptor.SupportsWorkflowTriggerActionUniversalActivityXML;

			AssertEquals("The XUA action type should only be visible if SendUniversalActivityXML is true. SAD!", isActivitySupported,
				actionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.SendUniversalActivityXML));

			if (isActivitySupported)
			{
				var (action, biz) = GetProcessTaskNotificationWithRealParent();
				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalActivityXML;
				AssertNotNull("You must implement GetUniversalActivityDataObjectWriter(action) when you override SupportsWorkflowTriggerActionUniversalActivityXML to 'true'."
					, descriptor.GetUniversalActivityDataObjectWriter(ObjectFactory.New<IDataWritingManager>(new ActionWrapper(action, biz, null), null, null, null)));

				var trigger = (ProcessTask)action.Parent;
				trigger.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;
				var action1 = trigger.ProcessTaskNotifications.AddNew();
				action1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalActivityXML;
				var log = trigger.Parent.Logs.AddNew(Events.Authorised);

				Factory.Save();
				var triggerLogBO = trigger.Logs.Find(aLog => aLog.SL_SE_NKEvent == Events.WorkflowTriggerEventCode).First();
				var result = descriptor.GetWorkflowTriggerAction(action1, new QueuedLogForTesting(triggerLogBO, trigger));
				AssertNotNull("workFlowDescriptor.GetWorkflowTriggerAction() result", result);
				AssertEquals("workFlowDescriptor.GetWorkflowTriggerAction() result Type", "Enterprise.MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWorkflowProcessor", result.GetType().FullName);
			}
		}

		public void TestGetWorkflowTriggerActionForApplyTag()
		{
			var parentBO = Factory.New<DummyWithWorkflow>();
			var trigger = parentBO.WorkflowItems.Triggers.AddNew();

			BMTestHelper.DisableBMSInRegistry();

			var descriptor = new T();
			var actionTypes = descriptor.GetWorkflowTriggerActionTypes(trigger, parentBO);

			Assert(!descriptor.SupportsWorkflowTriggerActionApplyTag(parentBO));

			AssertEquals("actionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.ApplyTag)"
					, false
					, actionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.ApplyTag));

			BMTestHelper.EnableBMSInRegistry();
			BMTestHelper.CreateSystem(Factory, DummyWorkflowDescriptor.Instance.Code);

			Assert(descriptor.SupportsWorkflowTriggerActionApplyTag(parentBO));

			actionTypes = descriptor.GetWorkflowTriggerActionTypes(trigger, parentBO);

			AssertEquals("actionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.ApplyTag)"
					, true
					, actionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.ApplyTag));

			trigger.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;
			var action1 = trigger.ProcessTaskNotifications.AddNew();
			action1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ApplyTag;
			var logBO = parentBO.Logs.AddNew(Events.Authorised);

			Factory.Save();

			var triggerLogBO = trigger.Logs.Find(aLog => aLog.SL_SE_NKEvent == Events.WorkflowTriggerEventCode).First();
			var workFlowDescriptor = new DummyEnterpriseBusinessObjectWorkflowDescriptor();

			var result = workFlowDescriptor.GetWorkflowTriggerAction(action1, new QueuedLogForTesting(triggerLogBO, trigger));
			AssertNotNull("workFlowDescriptor.GetWorkflowTriggerAction() result", result);
			AssertEquals("workFlowDescriptor.GetWorkflowTriggerAction() result Type", "Enterprise.MasterFiles.Business.WorkflowApplyTagProcessor", result.GetType().FullName);
		}

		public void TestGetWorkflowTriggerActionForApplyTag_WorkflowTemplate()
		{
			var template = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, WorkflowDescriptors.DummyWorkflowDescriptorCode);
			var trigger = template.WorkflowItems.Triggers.AddNew();

			BMTestHelper.DisableBMSInRegistry();

			var descriptor = new T();
			var actionTypes = descriptor.GetWorkflowTriggerActionTypes(trigger, trigger.GetJob());

			Assert(!descriptor.SupportsWorkflowTriggerActionApplyTag(trigger.GetJob()));

			AssertEquals("actionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.ApplyTag)"
					, false
					, actionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.ApplyTag));

			BMTestHelper.EnableBMSInRegistry();
			BMTestHelper.CreateSystem(Factory, DummyWorkflowDescriptor.Instance.Code);

			Assert(descriptor.SupportsWorkflowTriggerActionApplyTag(trigger.GetJob()));

			actionTypes = descriptor.GetWorkflowTriggerActionTypes(trigger, trigger.GetJob());

			AssertEquals("actionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.ApplyTag)"
					, true
					, actionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.ApplyTag));
		}

		public void TestGetWorkflowTriggerActionForUniversalTransactionXML()
		{
			var descriptor = new T();
			var actionTypes = descriptor.GetWorkflowTriggerActionTypes();

			if (!descriptor.SupportsWorkflowTriggerActionUniversalTransactionXML)
			{
				AssertEquals("actionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.SendUniversalTransactionXML)"
					, false
					, actionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.SendUniversalTransactionXML));
			}
			else
			{
				AssertEquals("actionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.SendUniversalTransactionXML)"
					, true
					, actionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.SendUniversalTransactionXML));

				var (action, biz) = GetProcessTaskNotificationWithRealParent();
				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalTransactionXML;
				AssertNotNull("You must implement GetUniversalTransactionDataObjectWriter(action) when you override SupportsWorkflowTriggerActionUniversalTransactionXML to 'true'."
					, descriptor.GetUniversalTransactionDataObjectWriter(ObjectFactory.New<IDataWritingManager>(new ActionWrapper(action, biz, null), null, null, null)));
			}

			var parentBO = Factory.New<DummyWithWorkflow>();
			var trigger = parentBO.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;
			var action1 = trigger.ProcessTaskNotifications.AddNew();
			action1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalTransactionXML;
			var logBO = parentBO.Logs.AddNew(Events.Authorised);

			Factory.Save();

			var triggerLogBO = trigger.Logs.Find(aLog => aLog.SL_SE_NKEvent == Events.WorkflowTriggerEventCode).First();
			var workFlowDescriptor = new DummyEnterpriseBusinessObjectWorkflowDescriptor();

			var result = workFlowDescriptor.GetWorkflowTriggerAction(action1, new QueuedLogForTesting(triggerLogBO, trigger));
			AssertNotNull("workFlowDescriptor.GetWorkflowTriggerAction() result", result);
			AssertEquals("workFlowDescriptor.GetWorkflowTriggerAction() result Type", "Enterprise.MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWorkflowProcessor", result.GetType().FullName);
		}

		public void TestGetWorkflowTriggerActionForUniversalScheduleXML()
		{
			var descriptor = new T();
			var actionTypes = descriptor.GetWorkflowTriggerActionTypes();

			if (!descriptor.SupportsWorkflowTriggerActionUniversalScheduleXML)
			{
				AssertEquals("actionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.SendUniversalScheduleXML)"
					, false
					, actionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.SendUniversalScheduleXML));
			}
			else
			{
				AssertEquals("actionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.SendUniversalScheduleXML)"
					, true
					, actionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.SendUniversalScheduleXML));

				var (action, biz) = GetProcessTaskNotificationWithRealParent();
				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalScheduleXML;
				AssertNotNull("You must implement GetUniversalScheduleDataObjectWriter(action) when you override SupportsWorkflowTriggerActionUniversalScheduleXML to 'true'."
					, descriptor.GetUniversalScheduleDataObjectWriter(ObjectFactory.New<IDataWritingManager>(new ActionWrapper(action, biz, null), null, null, null)));
			}

			var parentBO = Factory.New<DummyWithWorkflow>();
			var trigger = parentBO.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;
			var action1 = trigger.ProcessTaskNotifications.AddNew();
			action1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalScheduleXML;
			var logBO = parentBO.Logs.AddNew(Events.Authorised);

			Factory.Save();

			var triggerLogBO = trigger.Logs.Find(aLog => aLog.SL_SE_NKEvent == Events.WorkflowTriggerEventCode).First();
			var workFlowDescriptor = new DummyEnterpriseBusinessObjectWorkflowDescriptor();

			var result = workFlowDescriptor.GetWorkflowTriggerAction(action1, new QueuedLogForTesting(triggerLogBO, trigger));
			AssertNotNull("workFlowDescriptor.GetWorkflowTriggerAction() result", result);
			AssertEquals("workFlowDescriptor.GetWorkflowTriggerAction() result Type", "Enterprise.MasterFiles.DataTransfer.Universal.Workflow.UniversalXmlWorkflowProcessor", result.GetType().FullName);
		}

		public void TestGetWorkflowTriggerActionForScheduleDelayedEvent()
		{
			var descriptor = new T();
			var actionTypes = descriptor.GetWorkflowTriggerActionTypes();

			if (!descriptor.SupportsDelayedEvents)
			{
				AssertEquals("actionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.ScheduleDelayedEvent)"
					, false
					, actionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.ScheduleDelayedEvent));
			}
			else
			{
				AssertEquals("actionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.ScheduleDelayedEvent)"
					, true
					, actionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.ScheduleDelayedEvent));
			}

			var parentBO = Factory.New<DummyWithWorkflow>();
			var trigger = parentBO.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;
			var action1 = trigger.ProcessTaskNotifications.AddNew();
			action1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ScheduleDelayedEvent;
			var logBO = parentBO.Logs.AddNew(Events.Authorised);

			Factory.Save();

			var triggerLogBO = trigger.Logs.Find(aLog => aLog.SL_SE_NKEvent == Events.WorkflowTriggerEventCode).First();
			var workFlowDescriptor = new DummyEnterpriseBusinessObjectWorkflowDescriptor();

			var result = workFlowDescriptor.GetWorkflowTriggerAction(action1, new QueuedLogForTesting(triggerLogBO, trigger));
			AssertNotNull("workFlowDescriptor.GetWorkflowTriggerAction() result", result);
			AssertEquals("workFlowDescriptor.GetWorkflowTriggerAction() result Type", "Enterprise.MasterFiles.Business.WorkflowDelayedEventScheduler", result.GetType().FullName);
		}

		(ProcessTaskNotification, BusinessObject) GetProcessTaskNotificationWithRealParent()
		{
			var testProviders = GetParentsWithConfiguredOrganisationPartiesForTest();

			Assert("WorkflowDescriptor supports XML message delivering.\r\n"
				+ "GetParentsWithConfiguredOrganisationPartiesForTest() must be implemented to return at least 1 IWorkflowProvider",
				testProviders != null && testProviders.Length > 0);

			var job = testProviders.First();
			var processTask = job.WorkflowItems.Triggers.AddNew();
			processTask.P9_Description = "Start Work";
			processTask.ReferenceCode = "REF";
			var action = processTask.ProcessTaskNotifications.AddNew();
			return (action, processTask.GetJob());
		}

		IBMTestHelper BMTestHelper
		{
			get { return bmTestHelper ?? (bmTestHelper = ObjectFactory.Get<IBMTestHelper>()); }
		}

		IBMTestHelper bmTestHelper;

		public abstract void TestID();
		public abstract void TestDescription();
		public abstract void TestSubTypes();
		public abstract void TestRequiresPorts();
		public abstract void TestRequiresClient();
		public abstract void TestRequiresBranch();
		public abstract void TestRequiresDepartment();

		#region TestSupportedMessageRecipientParties

		public void TestSupportedMessageRecipientParties()
		{
			var processTaskNotification = GetProcessTaskNotificationWithRealParent();
			var actual = WorkflowDescriptor.SupportedMessageRecipientParties(processTaskNotification.Item1.Parent, processTaskNotification.Item2);

			var diffs = MessageRecipientPartyType.GetFlagDifferences(ExpectedSupportedMessageRecipientParties, actual);
			var strDiffs = diffs.Any() ? diffs.Select(a => a.ToString()).Aggregate((a, b) => $"{a}, {b}") : "<none>";

			AssertEquals($"MessageRecipientPartyTypes were different for the following flags: {strDiffs}",
				ExpectedSupportedMessageRecipientParties,
				actual);
		}

		protected virtual MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get { return MessageRecipientPartyType.None; }
		}

		#endregion

		#region TestSupportedTriggerPartyServices

		public void TestSupportedTriggerPartyServices()
		{
			if (!WorkflowDescriptor.SupportsWorkflowTriggerActionUniversalShipmentXML)
			{
				Assert("This test is only for workflows that support 'XUS - Send XML Universal Shipment'.", true);
				return;
			}

			var triggerAction = GetProcessTaskNotificationWithRealParent();
			var supportedRecipients = WorkflowDescriptor.GetMessagingTriggerPartiesList(WorkflowTriggerActionTypeConstants.Codes.SendUniversalScheduleXML, triggerAction.Item1.Parent, triggerAction.Item2);
			foreach (var recipientPair in supportedRecipients)
			{
				var recipientCode = ((PartyTypeDescriptionPair)recipientPair).Code;
				AssertContainsExactElementsInAnyOrder("SupportedTriggerPartyServices", ExpectedSupportedTriggerPartyServices(recipientCode), WorkflowDescriptor.SupportedTriggerPartyServices(recipientCode));
			}
		}

		protected virtual ZString[] ExpectedSupportedTriggerPartyServices(ZString recipient)
		{
			return Array.Empty<ZString>();
		}

		#endregion

		#region TestUnsupportedControllerIDs

		public void TestUnsupportedControllerIDs()
		{
			var descriptor = new T();

			if (workflowDescriptorCodesWithNoControllerId.Value.Contains(descriptor.Code))
			{
				AssertNull($"ControllerID should be null in {typeof(T)} (code {descriptor.Code}) as it has not been implemented yet.", descriptor.ControllerID);
			}
			else
			{
				AssertNotNull($"ControllerID should not be null in {typeof(T)} (code {descriptor.Code})", descriptor.ControllerID);
			}
		}

		readonly Lazy<ImmutableHashSet<string>> workflowDescriptorCodesWithNoControllerId = new(() => new HashSet<string>(
		[
			WorkflowDescriptors.ContainerLoadListLineWorkflowDescriptorCode,
			WorkflowDescriptors.CargoLoadPlanLineWorkflowDescriptorCode,
			WorkflowDescriptors.JobSupplierBookingLineWorkflowDescriptorCode,
			WorkflowDescriptors.DtbBookingInstructionWorkflowDescriptorCode,
			WorkflowDescriptors.DtbBookingConfirmationWorkflowDescriptorCode,
			WorkflowDescriptors.DtbConsignmentRunSheetInstructionWorkflowDescriptorCode,
			WorkflowDescriptors.PkgPackageWorkflowDecriptorCode,
			WorkflowDescriptors.AgencyShipmentWorkflowDescriptorCode,
			WorkflowDescriptors.CommericalInvoiceLineWorkflowDescriptorCode,
			WorkflowDescriptors.DummyWorkflowDescriptorCode,
			WorkflowDescriptors.ContainerYardContainerWorkflowDescriptorCode,
			WorkflowDescriptors.FacilityGateWorkflowDescriptorCode,
			WorkflowDescriptors.GateTransportCFSWorkflowDescriptorCode,
			WorkflowDescriptors.CusEntryHeaderWorkflowDescriptorCode,
			WorkflowDescriptors.CYDPickupHeaderWorkflowDescriptorCode,
			WorkflowDescriptors.CusExitReportWorkflowDescriptorCode,
			WorkflowDescriptors.ServiceWorkflowDescriptorCode,
			WorkflowDescriptors.CarrierShipmentCargoWorkflowDescriptorCode,
			WorkflowDescriptors.CrmOpportunityWorkflowDescriptorCode,
			WorkflowDescriptors.NctsArrivalMovementHeaderWorkflowDescriptor,
			WorkflowDescriptors.NctsDepartureMovementHeaderWorkflowDescriptor,
			WorkflowDescriptors.AccDraftInvoiceCode,
			WorkflowDescriptors.TransitPackage,
			WorkflowDescriptors.CarrierVoyageWorkflowDescriptorCode,
			WorkflowDescriptors.CarrierVoyagePortCallWorkflowDescriptorCode,
			WorkflowDescriptors.CusNOEmmaMessageGeneratorWorkflowDescriptorCode,
			// TODO: Fix customs issue - Same process task type used for different process tasks (one PT per country but only one PT type)
			WorkflowDescriptors.CusSCAOceanBillWorkflowDescriptorCode,
			WorkflowDescriptors.CusSCAHouseWorkflowDescriptorCode,
			WorkflowDescriptors.CYDDeliveryWorkflowDescriptorCode,
			WorkflowDescriptors.CYDPickupWorkflowDescriptorCode
		]).ToImmutableHashSet());

		#endregion

		#region TestWorkflowTriggerActionTypes

		public void TestWorkflowTriggerActionTypes()
		{
			var processTaskNotification = GetProcessTaskNotificationWithRealParent();
			var task = processTaskNotification.Item1.Parent;
			var parent = task.GetJob();
			var expected = new CodeDescriptionPairList();
			if (WorkflowDescriptor.SupportedMessageRecipientParties(task, parent) != MessageRecipientPartyType.None)
			{
				expected.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.NotificationEmail, WorkflowTriggerActionTypeConstants.Descriptions.NotificationEmail));
				expected.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.NotificationBodyEmail, WorkflowTriggerActionTypeConstants.Descriptions.NotificationBodyEmail));

				if (WorkflowDescriptor.DocumentBusinessContext[0] != BusinessContext.INVALID)
				{
					expected.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.SendDocument, WorkflowTriggerActionTypeConstants.Descriptions.SendDocument));
					expected.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.AddDocumentToEDocs, WorkflowTriggerActionTypeConstants.Descriptions.AddDocumentToEDocs));
				}
			}
			else
			{
				if (WorkflowDescriptor.SupportedMessageRecipientPartiesForSpecificAction(WorkflowTriggerActionTypeConstants.Codes.NotificationEmail) != MessageRecipientPartyType.None)
				{
					expected.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.NotificationEmail, WorkflowTriggerActionTypeConstants.Descriptions.NotificationEmail));
				}
				if (WorkflowDescriptor.SupportedMessageRecipientPartiesForSpecificAction(WorkflowTriggerActionTypeConstants.Codes.NotificationBodyEmail) != MessageRecipientPartyType.None)
				{
					expected.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.NotificationBodyEmail, WorkflowTriggerActionTypeConstants.Descriptions.NotificationBodyEmail));
				}
			}
			if (WorkflowDescriptor.SupportsWorkflowTriggerActionXML)
			{
				expected.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.SendXML, WorkflowTriggerActionTypeConstants.Descriptions.SendXML));
				expected.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.SendXMLSimplified, WorkflowTriggerActionTypeConstants.Descriptions.SendXMLSimplified));

				if (WorkflowDescriptor.IncludeWorkflowTriggerActionXMLDebtorBalance)
				{
					expected.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.SendXMLDebtorBalance, WorkflowTriggerActionTypeConstants.Descriptions.SendXMLDebtorBalance));
				}
			}

			if (WorkflowDescriptor.SupportsWorkflowTriggerActionNativeXML)
			{
				expected.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.SendNativeXML, WorkflowTriggerActionTypeConstants.Descriptions.SendNativeXML));
			}

			if (WorkflowDescriptor.SupportsWorkflowTriggerActionXMLWithJobFallback)
			{
				expected.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.SendXMLWithJobFallback, WorkflowTriggerActionTypeConstants.Descriptions.SendXMLWithJobFallback));
				expected.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.SendXMLWithJobFallbackSimplified, WorkflowTriggerActionTypeConstants.Descriptions.SendXMLWithJobFallbackSimplified));
			}

			if (WorkflowDescriptor.SupportsWorkflowTriggerActionXMLWithAWB)
			{
				expected.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.SendXMLWithAWB, WorkflowTriggerActionTypeConstants.Descriptions.SendXMLWithAWB));
			}

			if (workflowDescriptor.SupportsWorkflowTriggerActionApplyTag(parent))
			{
				expected.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.ApplyTag, WorkflowTriggerActionTypeConstants.Descriptions.ApplyTag));
			}

			if (WorkflowDescriptor.SupportsOtherCompanyAPInvoiceImport)
			{
				expected.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.ImportAPInvoicesFromOtherCompanies, WorkflowTriggerActionTypeConstants.Descriptions.ImportAPInvoicesFromOtherCompanies));
			}

			if (WorkflowDescriptor.SupportsRecognizeRevenue)
			{
				expected.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.RecognizeRevenue, WorkflowTriggerActionTypeConstants.Descriptions.RecognizeRevenue));
			}

			if (WorkflowDescriptor.SupportsAutoRateCostsAndRevenue)
			{
				expected.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.AutoRateCostsAndRevenue, WorkflowTriggerActionTypeConstants.Descriptions.AutoRateCostsAndRevenue));
				expected.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.AutoRateCosts, WorkflowTriggerActionTypeConstants.Descriptions.AutoRateCosts));
				expected.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.AutoRateRevenue, WorkflowTriggerActionTypeConstants.Descriptions.AutoRateRevenue));
				expected.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.AutoRateNonConsolLevelCosts, WorkflowTriggerActionTypeConstants.Descriptions.AutoRateNonConsolLevelCosts));
				expected.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.AutoRateNonConsolLevelCostsAndRevenue, WorkflowTriggerActionTypeConstants.Descriptions.AutoRateNonConsolLevelCostsAndRevenue));
			}

			if (WorkflowDescriptor.SupportsAutoPack)
			{
				expected.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.AutoPack, WorkflowTriggerActionTypeConstants.Descriptions.AutoPack));
			}

			if (WorkflowDescriptor.SupportsCreateTransportBooking)
			{
				expected.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.CreateTransportBooking, WorkflowTriggerActionTypeConstants.Descriptions.CreateTransportBooking));
				expected.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.CreateTransportBookingContainer, WorkflowTriggerActionTypeConstants.Descriptions.CreateTransportBookingContainer));
			}

			if (WorkflowDescriptor.SupportsPostConsolCostOnly)
			{
				expected.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.PostConsolCostOnly, WorkflowTriggerActionTypeConstants.Descriptions.PostConsolCostOnly));
			}

			if (WorkflowDescriptor.SupportsTransactionAllocationAndPost)
			{
				expected.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.TransactionAllocationAndPost, WorkflowTriggerActionTypeConstants.Descriptions.TransactionAllocationAndPost));
			}

			if (WorkflowDescriptor.SupportsPostAllRevenue)
			{
				expected.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.PostAllRevenue, WorkflowTriggerActionTypeConstants.Descriptions.PostAllRevenue));
			}

			if (WorkflowDescriptor.SupportsCreateJobInvoiceHeader)
			{
				expected.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.CreateJobInvoiceHeader, WorkflowTriggerActionTypeConstants.Descriptions.CreateJobInvoiceHeader));
			}

			if (WorkflowDescriptor.SupportsCreateProfitShareCharges)
			{
				expected.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.CreateProfitShareCharges, WorkflowTriggerActionTypeConstants.Descriptions.CreateProfitShareCharges));
			}

			if (WorkflowDescriptor.SupportsPostAllSisterCompanyCharges)
			{
				expected.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.PostAllSisterCompanyCharges, WorkflowTriggerActionTypeConstants.Descriptions.PostAllSisterCompanyCharges));
			}

			if (WorkflowDescriptor.SupportsPostLocalSisterCompanyChargesOnly)
			{
				expected.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.PostLocalSisterCompanyChargesOnly, WorkflowTriggerActionTypeConstants.Descriptions.PostLocalSisterCompanyChargesOnly));
			}

			if (WorkflowDescriptor.SupportsPostAllCosts)
			{
				expected.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.PostAllCosts, WorkflowTriggerActionTypeConstants.Descriptions.PostAllCosts));
			}

			foreach (var code in WorkflowDescriptor.GetSupportsValidateForCustomsMessagingTriggerActions(task, parent))
			{
				expected.AddPair(code, new WorkflowTriggerActionTypeConstants().GetDescriptionFromCode(code));
			}

			if (WorkflowDescriptor.SupportsSetFieldTriggerAction(task, parent))
			{
				expected.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.SetField, WorkflowTriggerActionTypeConstants.Descriptions.SetField));
				expected.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.ImmediateFieldChange, WorkflowTriggerActionTypeConstants.Descriptions.ImmediateFieldChange));
			}

			if (WorkflowDescriptor.SupportsWorkflowTriggerActionUniversalShipmentXML || WorkflowDescriptor.ParentSupportsWorkflowTriggerActionUniversalShipmentXML(parent))
			{
				expected.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML, WorkflowTriggerActionTypeConstants.Descriptions.SendUniversalShipmentXML));
			}

			if (WorkflowDescriptor.SupportsWorkflowTriggerActionUniversalEventXML)
			{
				expected.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML, WorkflowTriggerActionTypeConstants.Descriptions.SendUniversalEventXML));
			}

			if (WorkflowDescriptor.SupportsWorkflowTriggerActionUniversalEventCollectionXML)
			{
				expected.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventCollectionXML, WorkflowTriggerActionTypeConstants.Descriptions.SendUniversalEventCollectionXML));
			}

			if (WorkflowDescriptor.SupportsWorkflowTriggerActionUniversalTransactionXML)
			{
				expected.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.SendUniversalTransactionXML, WorkflowTriggerActionTypeConstants.Descriptions.SendUniversalTransactionXML));
			}

			if (WorkflowDescriptor.SupportsWorkflowTriggerActionUniversalTransactionBatchXML)
			{
				expected.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.SendUniversalTransactionBatchXML, WorkflowTriggerActionTypeConstants.Descriptions.SendUniversalTransactionBatchXML));
			}

			if (WorkflowDescriptor.SupportsWorkflowTriggerActionUniversalScheduleXML)
			{
				expected.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.SendUniversalScheduleXML, WorkflowTriggerActionTypeConstants.Descriptions.SendUniversalScheduleXML));
			}

			if (WorkflowDescriptor.SupportsWorkflowTriggerActionUniversalActivityXML)
			{
				expected.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.SendUniversalActivityXML, WorkflowTriggerActionTypeConstants.Descriptions.SendUniversalActivityXML));
			}

			if (WorkflowDescriptor.SupportsPostOverseasAgentCharges)
			{
				expected.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.PostOverseasAgentCharges, WorkflowTriggerActionTypeConstants.Descriptions.PostOverseasAgentCharges));
			}

			if (WorkflowDescriptor.SupportsIncludeChargeInProfitShare)
			{
				expected.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.IncludeChargeInProfitShare, WorkflowTriggerActionTypeConstants.Descriptions.IncludeChargeInProfitShare));
			}

			if (WorkflowDescriptor.SupportsAutoFinalisation)
			{
				expected.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.AutoFinalisation, WorkflowTriggerActionTypeConstants.Descriptions.AutoFinalisation));
			}

			if (WorkflowDescriptor.SupportsPrintAllPackageLabels)
			{
				expected.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.PrintAllPackageLabels, WorkflowTriggerActionTypeConstants.Descriptions.PrintAllPackageLabels));
				expected.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.PrintAllCarrierLabels, WorkflowTriggerActionTypeConstants.Descriptions.PrintAllCarrierLabels));
			}

			if (WorkflowDescriptor.SupportsApplyWorkflowTemplate)
			{
				expected.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateOnce, WorkflowTriggerActionTypeConstants.Descriptions.ApplyWorkflowTemplateOnce));
				expected.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.ApplyWorkflowTemplateAlways, WorkflowTriggerActionTypeConstants.Descriptions.ApplyWorkflowTemplateAlways));
			}

			if (WorkflowDescriptor.SupportsConvertToShipment)
			{
				expected.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.ConvertToShipment, WorkflowTriggerActionTypeConstants.Descriptions.ConvertToShipment));
			}

			foreach (var code in workflowDescriptor.GetScheduleDeferredMessageSendTriggerActions(task, parent))
			{
				expected.Add(new CodeDescriptionPair(code, new WorkflowTriggerActionTypeConstants().GetDescriptionFromCode(code)));
			}

			if (WorkflowDescriptor.SupportsHVLVPreScreening)
			{
				expected.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.RunHVLVPreScreening, WorkflowTriggerActionTypeConstants.Descriptions.RunHVLVPreScreening));
			}

			if (WorkflowDescriptor.SupportsDelayedEvents)
			{
				expected.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.ScheduleDelayedEvent, WorkflowTriggerActionTypeConstants.Descriptions.ScheduleDelayedEvent));
			}

			if (WorkflowDescriptor.SupportsAutomatedExitReportTransferMessageSending(parent, task))
			{
				expected.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.SendExitReportTransferMessage, WorkflowTriggerActionTypeConstants.Descriptions.SendExitReportTransferMessage));
			}

			if (WorkflowDescriptor.SupportsSendCalculateCO2EmissionRequest)
			{
				expected.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.SendCalculateCO2EmissionRequest, WorkflowTriggerActionTypeConstants.Descriptions.SendCalculateCO2EmissionRequest));
			}

			if (WorkflowDescriptor.SupportEmmaMessageGeneration)
			{
				expected.Add(new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.CusNOEmmaMessageGenerator, WorkflowTriggerActionTypeConstants.Descriptions.CusNOEmmaMessageGenerator));
			}

			expected.AddRange(ExpectedAdditionalWorkflowTriggerActionTypes);
			expected.Sort();

			var actual = new CodeDescriptionPairList();
			actual.AddRange(WorkflowDescriptor.GetWorkflowTriggerActionTypes(task, parent));
			actual.Sort();

			AssertEquals("ExpectedWorkflowTriggerActionTypes do not match", expected.ElementsAsString, actual.ElementsAsString);
		}

		public void TestWorkflowThatSupportsWorkflowTriggerActionNativeXMLDoesNotThrowExceptions()
		{
			if (!WorkflowDescriptor.SupportsWorkflowTriggerActionNativeXML)
			{
				Assert("This test is only for workflows that support native.", true);
				return;
			}
			var workflowParentBO = (BusinessObject)Factory.GetType().GetMethods().Single(mi => mi.Name == "NewWithValidTestData" && mi.GetParameters().Length == 0)
				.MakeGenericMethod(WorkflowDescriptor.WorkflowProviderType).Invoke(Factory, Array.Empty<object>());

			var workflowParent = workflowParentBO as IWorkflowProvider;
			var trigger = workflowParent.WorkflowItems.Triggers.AddNew();
			var action = trigger.ProcessTaskNotifications.AddNew();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendNativeXML;
			action.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.OrgProxy;
			Factory.Save();

			var communicationsMode = Factory.New<EDICommunicationsMode>();
			communicationsMode.EK_Module = workflowParent.WorkflowType;
			communicationsMode.EK_CommsDirection = EDICommunicationsModeCommsDirectionList.Codes.Transmit;
			communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EAdaptorInterface;
			communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XML;
			communicationsMode.EK_Destination = "SOMEONE";
			communicationsMode.EK_Filename = "(*JobNumber*).xml";
			communicationsMode.EK_ServerAddressSubject = "Organisation [(*JobNumber*)]";

			var stmALog = workflowParentBO.GetLogs().AddNew(new EventValue(Events.Authorised, eventTime: new ZDateTimeOffset(2010, 12, 25), reference: "Test Ref"));
			var queuedLog = new QueuedLogForTesting(stmALog, trigger)
			{
				SJ_SE_NKEvent = Events.WorkflowTriggerEventCode,
				SJ_Reference = $"{stmALog.PK}|{GlbBranch.CurrentBranch.GB_Code}|{GlbDepartment.CurrentDepartment.GE_Code}"
			};

			var actionWrapper = new ActionWrapper(action, workflowParentBO, null);
			var eventInfoProvider = new EventInfoProvider(queuedLog, action.Parent, workflowParentBO);

			var processor = ObjectFactory.New<INativeXmlWorkflowProcessor>(actionWrapper, Lazy.Create(() => new MessageProcessorCommunicationModesResult(new[] { communicationsMode }, null)), eventInfoProvider);
			try
			{
				using (Factory.AddDisposableService())
				{
					processor.Process(new NotificationBuffer());
					Factory.Save();
				}
			}
			catch
			{
				Fail(
$@"The business object {WorkflowDescriptor.WorkflowProviderType} has not been properly setup to handle Native XML.
In order to properly implement Native XML in this module, you will need to:
1). Add the data type to Enterprise.UniversalDataBuss.Integration.DataContextType (if necessary)
2.) Add the attribute UniversalDataContext to the class {WorkflowDescriptor.WorkflowProviderType} using the data type created above.
3.) Create an EventDataContextManager for the DataContext you have created.
4.) Add your manager to $\Dev\Enterprise\Architecture\Core\Core\Configuration\UniversalDataContextManagersConfiguration.xml
5.) Go have a coffee
6.) Pat yourself on the back for doing a good job.");
			}
			CombineAssertions(delegate
			{
				var messages = Factory.Load<IEDIMessage>(new ZQuery());
				AssertEquals("messages.Length", 1, messages.Length);
				var message = messages[0];
				AssertEquals("message.EM_ApplicationCode", ApplicationCodeList.Codes.NativeDataMessaging, message.EM_ApplicationCode);
				AssertEquals("message.EM_MessageType", EDIMessageTypeList.Codes.XDC, message.EM_MessageType);
				AssertEquals("message.EM_MessageSubType", EDIMessageSubType, message.EM_MessageSubType);
			});
		}

		protected virtual string EDIMessageSubType => null;

		protected virtual CodeDescriptionPair[] ExpectedAdditionalWorkflowTriggerActionTypes
		{
			get { return Array.Empty<CodeDescriptionPair>(); }
		}

		#endregion

		public virtual void TestIsMessagingOrEmailNotificationTriggerAction()
		{
			var boolTrueValues = new HashSet<string>();

			boolTrueValues.Add(WorkflowTriggerActionTypeConstants.Codes.NotificationEmail);
			boolTrueValues.Add(WorkflowTriggerActionTypeConstants.Codes.NotificationBodyEmail);
			boolTrueValues.Add(WorkflowTriggerActionTypeConstants.Codes.SendXML);
			boolTrueValues.Add(WorkflowTriggerActionTypeConstants.Codes.SendXMLWithJobFallback);
			boolTrueValues.Add(WorkflowTriggerActionTypeConstants.Codes.SendXMLSimplified);
			boolTrueValues.Add(WorkflowTriggerActionTypeConstants.Codes.SendXMLWithJobFallbackSimplified);
			boolTrueValues.Add(WorkflowTriggerActionTypeConstants.Codes.SendXMLDebtorBalance);
			boolTrueValues.Add(WorkflowTriggerActionTypeConstants.Codes.SendNativeXML);
			boolTrueValues.Add(WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML);
			boolTrueValues.Add(WorkflowTriggerActionTypeConstants.Codes.SendUniversalActivityXML);
			boolTrueValues.Add(WorkflowTriggerActionTypeConstants.Codes.SendUniversalTransactionBatchXML);
			boolTrueValues.Add(WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML);
			boolTrueValues.Add(WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventCollectionXML);
			boolTrueValues.Add(WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXMLWithEDoc);
			boolTrueValues.Add(WorkflowTriggerActionTypeConstants.Codes.SendUniversalTransactionXML);
			boolTrueValues.Add(WorkflowTriggerActionTypeConstants.Codes.SendUniversalScheduleXML);
			boolTrueValues.Add(WorkflowTriggerActionTypeConstants.Codes.SendDescartesXml);
			boolTrueValues.Add(WorkflowTriggerActionTypeConstants.Codes.SendEDocXml);
			boolTrueValues.Add(WorkflowTriggerActionTypeConstants.Codes.SendXMLWithAWB);
			boolTrueValues.Add(WorkflowTriggerActionTypeConstants.Codes.ValidateForCustomsMessaging);
			boolTrueValues.Add(WorkflowTriggerActionTypeConstants.Codes.ValidateForAUCargoMessaging);
			boolTrueValues.Add(WorkflowTriggerActionTypeConstants.Codes.CreateTransportBooking);
			boolTrueValues.Add(WorkflowTriggerActionTypeConstants.Codes.CreateTransportBookingContainer);
			boolTrueValues.Add(WorkflowTriggerActionTypeConstants.Codes.CreateTransportJob);

			FieldInfo[] thisObjectProperties = typeof(WorkflowTriggerActionTypeConstants.Codes).GetFields();
			foreach (FieldInfo info in thisObjectProperties)
			{
				var code = info.GetValue(null) as string;
				var isMessagingOrEmailNotificationTriggerAction = WorkflowDescriptor.IsMessagingOrEmailNotificationTriggerAction(code);

				AssertEquals("The bool value should match the result from IsMessagingOrEmailNotificationTriggerAction(). code:" + code, isMessagingOrEmailNotificationTriggerAction, boolTrueValues.Contains(code));
			}
		}

		public virtual void TestIsManifestMessagingTriggerAction()
		{
			var boolTrueValues = new HashSet<string>();

			boolTrueValues.Add(WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentManifestXML);

			FieldInfo[] thisObjectProperties = typeof(WorkflowTriggerActionTypeConstants.Codes).GetFields();
			foreach (FieldInfo info in thisObjectProperties)
			{
				var code = info.GetValue(null) as string;
				var isManifestMessagingTriggerAction = WorkflowDescriptor.IsManifestMessagingTriggerAction(code);

				AssertEquals("The bool value should match the result from IsManifestMessagingTriggerAction(). code:" + code, isManifestMessagingTriggerAction, boolTrueValues.Contains(code));
			}
		}

		public void TestIsPrintingTriggerAction()
		{
			var boolTrueValues = new HashSet<string>(IsPrintingTriggerActionsCore);
			boolTrueValues.Add(WorkflowTriggerActionTypeConstants.Codes.SendDocument);

			FieldInfo[] thisObjectProperties = typeof(WorkflowTriggerActionTypeConstants.Codes).GetFields();
			foreach (FieldInfo info in thisObjectProperties)
			{
				var code = info.GetValue(null) as string;
				var isSendDocumentTriggerAction = WorkflowDescriptor.IsPrintingTriggerAction(code);

				AssertEquals("The bool value should match the result from isSendDocumentTriggerAction(). code:" + code, isSendDocumentTriggerAction, boolTrueValues.Contains(code));
			}
		}

		protected virtual string[] IsPrintingTriggerActionsCore
		{
			get { return Array.Empty<string>(); }
		}

		public void TestIsPrintOnlyTriggerAction()
		{
			var boolTrueValues = new HashSet<string>(IsPrintOnlyTriggerActionsCore);

			FieldInfo[] thisObjectProperties = typeof(WorkflowTriggerActionTypeConstants.Codes).GetFields();
			foreach (FieldInfo info in thisObjectProperties)
			{
				var code = info.GetValue(null) as string;
				var isSendDocumentTriggerAction = WorkflowDescriptor.IsPrintOnlyTriggerAction(code);

				AssertEquals("The bool value should match the result from IsPrintOnlyTriggerAction(). code:" + code, isSendDocumentTriggerAction, boolTrueValues.Contains(code));
			}
		}

		protected virtual string[] IsPrintOnlyTriggerActionsCore
		{
			get { return Array.Empty<string>(); }
		}

		public virtual void TestSupportsEventTracking()
		{
			AssertEquals(false, WorkflowDescriptor.SupportsEventTracking);
		}

		public virtual void TestOnlySupportEventTrackingForUniversalTemplates()
		{
			AssertEquals(false, WorkflowDescriptor.OnlySupportEventTrackingForUniversalTemplates);
		}

		public virtual void TestSupportsTasks()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsTasks);
		}

		public virtual void TestSupportsWorkflowTemplates()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsWorkflowTemplates);
		}

		public virtual void TestSupportsScreenLayout()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsScreenLayout);
		}

		public virtual void TestSupportsCustomFields()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsCustomFields);
		}

		public virtual void TestIncludeWorkflowTriggerActionXMLDebtorBalance()
		{
			AssertEquals(true, WorkflowDescriptor.IncludeWorkflowTriggerActionXMLDebtorBalance);
		}

		#region TestRequiresWarehouse

		public void TestRequiresWarehouse()
		{
			AssertEquals(RequiresWarehouseExpectedResult, WorkflowDescriptor.RequiresWarehouse);
		}

		protected virtual bool RequiresWarehouseExpectedResult
		{
			get { return false; }
		}

		#endregion

		#region TestRequiresWarehouse

		public void TestWarehouseType()
		{
			AssertEquals(WarehouseTypeExpectedResult, WorkflowDescriptor.WarehouseType);
		}

		protected virtual WarehouseCollectionType WarehouseTypeExpectedResult
		{
			get { return WarehouseCollectionType.ProductWarehouse; }
		}

		public virtual void TestSupportsAssignStaffAndEmail()
		{
			AssertEquals(WorkflowDescriptor.SupportsAssignStaffAndEmail(null, null), false);
		}

		#endregion

		public void TestSubTypeInformationNotNull()
		{
			AssertNotNull(WorkflowDescriptor.SubTypeInformation);
		}

		[StressTest]
		public void TestSubTypeInformation_MaxLengthsInDatabaseCorrect()
		{
			for (int i = 1; i <= WorkflowDescriptor.SubTypeInformation.Length; i++)
			{
				var requiredMaxLength = 0;

				if (WorkflowDescriptor.SubTypeInformation[i - 1].UseCollection)
				{
					foreach (ICodeDescription item in WorkflowDescriptor.SubTypeInformation[i - 1].Collection)
					{
						requiredMaxLength = Math.Max(requiredMaxLength, item.Code.Length);
					}
				}
				else
				{
					var list = WorkflowDescriptor.SubTypeInformation[i - 1].List;
					foreach (ICodeDescription item in WorkflowDescriptor.SubTypeInformation[i - 1].List)
					{
						requiredMaxLength = Math.Max(requiredMaxLength, item.Code.Length);
					}
				}

				switch (i)
				{
					case 1:
						AssertEquals("P0_SubType1 MaxLength needs to be increased from " + ProcessTaskTemplateSchema.P0_SubType1.MaxLength + " to " + requiredMaxLength, true, ProcessTaskTemplateSchema.P0_SubType1.MaxLength >= requiredMaxLength);
						break;
					case 2:
						AssertEquals("P0_SubType2 MaxLength needs to be increased from " + ProcessTaskTemplateSchema.P0_SubType2.MaxLength + " to " + requiredMaxLength, true, ProcessTaskTemplateSchema.P0_SubType2.MaxLength >= requiredMaxLength);
						break;
					case 3:
						AssertEquals("P0_SubType3 MaxLength needs to be increased from " + ProcessTaskTemplateSchema.P0_SubType3.MaxLength + " to " + requiredMaxLength, true, ProcessTaskTemplateSchema.P0_SubType3.MaxLength >= requiredMaxLength);
						break;
					case 4:
						AssertEquals("P0_SubType4 MaxLength needs to be increased from " + ProcessTaskTemplateSchema.P0_SubType4.MaxLength + " to " + requiredMaxLength, true, ProcessTaskTemplateSchema.P0_SubType4.MaxLength >= requiredMaxLength);
						break;
					case 5:
						AssertEquals("P0_SubType5 MaxLength needs to be increased from " + ProcessTaskTemplateSchema.P0_SubType5.MaxLength + " to " + requiredMaxLength, true, ProcessTaskTemplateSchema.P0_SubType5.MaxLength >= requiredMaxLength);
						break;
				}
			}
			Assert(true);
		}

		#region Workflow Triggers

		public void TestWorkflowTriggerFieldColumns()
		{
			AssertContainsExactElementsInAnyOrder(FormattableString.Invariant($"Field change columns need to be declared in {nameof(ExpectedWorkflowTriggerFieldColumns)}."),
				ExpectedWorkflowTriggerFieldColumns.Select(c => c.Name), WorkflowDescriptor.GetWorkflowTriggerFieldColumns().Select(c => c.Name));
		}

		protected virtual SchemaColumn[] ExpectedWorkflowTriggerFieldColumns
		{
			get { return Array.Empty<SchemaColumn>(); }
		}

		public void TestWorkflowTriggerFieldColumns_AlsoRegisteredInPropertyChangeSubscriptionList()
		{
			AssertNotNull(WorkflowDescriptor.GetWorkflowTriggerFieldColumns());

			CombineAssertions("Any fields listed as able to fire workflow triggers must also be listed in " + typeof(PropertyChangeSubscriptionList).FullName + "." + nameof(PropertyChangeSubscriptionList.AllPropertyNamesThatMayBeLogged), () =>
			{
				foreach (var column in WorkflowDescriptor.GetWorkflowTriggerFieldColumns())
				{
					AssertEquals(column.Name, true, PropertyChangeSubscriptionList.AllPropertyNamesThatMayBeLogged.Contains(column.Name));
				}
			});
		}

		public void TestWorkflowTriggerFieldColumns_ChangeLogParentKnownAndInterfacesImplemented()
		{
			AssertNotNull(WorkflowDescriptor.GetWorkflowTriggerFieldColumns());
			foreach (var fieldColumn in WorkflowDescriptor.GetWorkflowTriggerFieldColumns())
			{
				StmChangeLog log = GetChangeLogForField(fieldColumn);
				BusinessObject parent = log.Parent;

				AssertNotNull("Parent type must be known from dbo.StmChangeLog", parent);
				AssertEquals("Parent table missmatch", GetPrefixFromColumnName(fieldColumn.Name), parent.TablePrefix);

				var isWorkflowProvider = parent is IWorkflowProvider;
				var isWorkflowFieldChangeSource = parent is IWorkflowTriggerFieldChangeSource;

				AssertEquals(
					string.Format("Field change property business object ({0}) must be either a IWorkflowProvider or a IWorkflowTriggerFieldChangeSource", parent.GetType().Name),
					true, isWorkflowProvider || isWorkflowFieldChangeSource);
			}
		}

		public virtual string GetPrefixFromColumnName(string fieldColumnName)
		{
			return Schema.GetPrefixFromColumnName(fieldColumnName);
		}

		public void TestGetFieldColumnDescription()
		{
			CombineAssertions("Field change triggers need resource strings specified for all relevant columns. These should be specified in the *ResourceStrings.xml file so they are accessible here.", () =>
			{
				foreach (var fieldColumn in WorkflowDescriptor.GetWorkflowTriggerFieldColumns())
				{
					var description = WorkflowDescriptor.GetFieldColumnDescription(Factory, fieldColumn);
					AssertEquals(FormattableString.Invariant($"A description for table '{fieldColumn.TableName}' and column '{fieldColumn.Name}' must be specified (from resource strings). Detected description: [{description}]"), true, description.StartsWith(DataBoundResourceStrings.GetTableDescriptiveName(fieldColumn.TableName) + " - "));
					AssertEquals(FormattableString.Invariant($"A description for field column '{fieldColumn.Name}' must be specified (from resource strings). Detected description: [{description}]"), false, description.Contains("_"));
				}
			});
			Assert(true);
		}

		#endregion

		public void TestGetWorkflowTriggerAction_ForImportAPInvoicesFromOtherCompanies()
		{
			if (WorkflowDescriptor.GetWorkflowTriggerActionTypes().ContainsCode(WorkflowTriggerActionTypeConstants.Codes.ImportAPInvoicesFromOtherCompanies))
			{
				SetupOrgCommunicationModes(GlbCompany.CurrentCompany.OrgProxy);
				var testProviders = GetParentsWithConfiguredOrganisationPartiesForTest();
				const string message = "You must override method GetParentsWithConfiguredOrganisationPartiesForTest on this test class when there are SupportedMessageRecipientParties";
				Assert(message, testProviders.Length > 0);
				AssertNotNull(message, testProviders[0]);

				var trigger = testProviders[0].WorkflowItems.Triggers.AddNew();
				trigger.P9_Description = "DUMMY TASK";
				trigger.ReferenceCode = "REF";
				var notification = trigger.ProcessTaskNotifications.AddNew();
				notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ImportAPInvoicesFromOtherCompanies;

				var notificationAction = WorkflowDescriptor.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(Factory));
				AssertNotNull("Workflow notification action run for 'ISI' trigger action type", notificationAction);
				AssertEquals("The IProcessor type should be as expected", "Enterprise.Accounting.Business.TriggerActionWithOptionalFactorySave.TriggerActionWithOptionalFactorySaveProcessor", notificationAction.GetType().FullName);
			}
			Assert(true);
		}

		public void TestGetWorkflowTriggerAction_ForRevenueRecognition()
		{
			AssertGetWorkflowTriggerActionWithAccountingTriggerActionWithOptionalFactorySaveProcessorCreator(WorkflowTriggerActionTypeConstants.Codes.RecognizeRevenue);
		}

		public void TestGetWorkflowTriggerAction_ForAutoRateCostsAndRevenue()
		{
			if (WorkflowDescriptor.GetWorkflowTriggerActionTypes().ContainsCode(WorkflowTriggerActionTypeConstants.Codes.AutoRateCostsAndRevenue))
			{
				SetupOrgCommunicationModes(GlbCompany.CurrentCompany.OrgProxy);
				var testProviders = GetParentsWithConfiguredOrganisationPartiesForTest();
				const string message = "You must override method GetParentsWithConfiguredOrganisationPartiesForTest on this test class when there are SupportedMessageRecipientParties";
				Assert(message, testProviders.Length > 0);
				AssertNotNull(message, testProviders[0]);

				var trigger = testProviders[0].WorkflowItems.Triggers.AddNew();
				trigger.P9_Description = "DUMMY TASK";
				trigger.ReferenceCode = "REF";
				var notification = trigger.ProcessTaskNotifications.AddNew();
				notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.AutoRateCostsAndRevenue;

				var notificationAction = WorkflowDescriptor.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(Factory));
				AssertNotNull("Workflow notification action run for 'CAR' trigger action type", notificationAction);
				if (notificationAction is IWrappingProcessor actionWithWrappedProcessor)
				{
					AssertEquals("The IProcessor type of the WrappedProcessor should be as expected", "Enterprise.Accounting.Business.WorkflowAutoRater", actionWithWrappedProcessor.WrappedProcessor.GetType().FullName);
				}
				else
				{
					AssertEquals("The IProcessor type should be as expected", "Enterprise.Accounting.Business.WorkflowAutoRater", notificationAction.GetType().FullName);
				}
			}
			Assert(true);
		}

		public void TestGetWorkflowTriggerAction_ForAutoRateRevenue()
		{
			if (WorkflowDescriptor.GetWorkflowTriggerActionTypes().ContainsCode(WorkflowTriggerActionTypeConstants.Codes.AutoRateRevenue))
			{
				SetupOrgCommunicationModes(GlbCompany.CurrentCompany.OrgProxy);
				var testProviders = GetParentsWithConfiguredOrganisationPartiesForTest();
				const string message = "You must override method GetParentsWithConfiguredOrganisationPartiesForTest on this test class when there are SupportedMessageRecipientParties";
				Assert(message, testProviders.Length > 0);
				AssertNotNull(message, testProviders[0]);

				var trigger = testProviders[0].WorkflowItems.Triggers.AddNew();
				trigger.P9_Description = "DUMMY TASK";
				trigger.ReferenceCode = "REF";
				var notification = trigger.ProcessTaskNotifications.AddNew();
				notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.AutoRateRevenue;

				var notificationAction = WorkflowDescriptor.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(Factory));
				AssertNotNull("Workflow notification action run for 'REV' trigger action type", notificationAction);
				if (notificationAction is IWrappingProcessor actionWithWrappedProcessor)
				{
					AssertEquals("The IProcessor type of the WrappedProcessor should be as expected", "Enterprise.Accounting.Business.WorkflowAutoRater", actionWithWrappedProcessor.WrappedProcessor.GetType().FullName);
				}
				else
				{
					AssertEquals("The IProcessor type should be as expected", "Enterprise.Accounting.Business.WorkflowAutoRater", notificationAction.GetType().FullName);
				}
			}
			Assert(true);
		}

		public void TestGetWorkflowTriggerAction_ForAutoRateCosts()
		{
			if (WorkflowDescriptor.GetWorkflowTriggerActionTypes().ContainsCode(WorkflowTriggerActionTypeConstants.Codes.AutoRateCosts))
			{
				SetupOrgCommunicationModes(GlbCompany.CurrentCompany.OrgProxy);
				var testProviders = GetParentsWithConfiguredOrganisationPartiesForTest();
				const string message = "You must override method GetParentsWithConfiguredOrganisationPartiesForTest on this test class when there are SupportedMessageRecipientParties";
				Assert(message, testProviders.Length > 0);
				AssertNotNull(message, testProviders[0]);

				var trigger = testProviders[0].WorkflowItems.Triggers.AddNew();
				trigger.P9_Description = "DUMMY TASK";
				trigger.ReferenceCode = "REF";
				var notification = trigger.ProcessTaskNotifications.AddNew();
				notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.AutoRateCosts;

				var notificationAction = WorkflowDescriptor.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(Factory));
				AssertNotNull("Workflow notification action run for 'COS' trigger action type", notificationAction);

				if (notificationAction is IWrappingProcessor actionWithWrappedProcessor)
				{
					AssertEquals("The IProcessor type of the WrappedProcessor should be as expected", "Enterprise.Accounting.Business.WorkflowAutoRater", actionWithWrappedProcessor.WrappedProcessor.GetType().FullName);
				}
				else
				{
					AssertEquals("The IProcessor type should be as expected", "Enterprise.Accounting.Business.WorkflowAutoRater", notificationAction.GetType().FullName);
				}
			}
			Assert(true);
		}

		public void TestGetWorkflowTriggerAction_ForAutoRateNonConsolLevelCosts()
		{
			if (WorkflowDescriptor.GetWorkflowTriggerActionTypes().ContainsCode(WorkflowTriggerActionTypeConstants.Codes.AutoRateNonConsolLevelCosts))
			{
				SetupOrgCommunicationModes(GlbCompany.CurrentCompany.OrgProxy);
				var testProviders = GetParentsWithConfiguredOrganisationPartiesForTest();
				const string message = "You must override method GetParentsWithConfiguredOrganisationPartiesForTest on this test class when there are SupportedMessageRecipientParties";
				Assert(message, testProviders.Length > 0);
				AssertNotNull(message, testProviders[0]);

				var trigger = testProviders[0].WorkflowItems.Triggers.AddNew();
				trigger.P9_Description = "DUMMY TASK";
				trigger.ReferenceCode = "REF";
				var notification = trigger.ProcessTaskNotifications.AddNew();
				notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.AutoRateNonConsolLevelCosts;

				var notificationAction = WorkflowDescriptor.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(Factory));
				AssertNotNull("Workflow notification action run for 'CNC' trigger action type", notificationAction);
				if (notificationAction is IWrappingProcessor actionWithWrappedProcessor)
				{
					AssertEquals("The IProcessor type of the WrappedProcessor should be as expected", "Enterprise.Accounting.Business.WorkflowAutoRater", actionWithWrappedProcessor.WrappedProcessor.GetType().FullName);
				}
				else
				{
					AssertEquals("The IProcessor type should be as expected", "Enterprise.Accounting.Business.WorkflowAutoRater", notificationAction.GetType().FullName);
				}
			}
			Assert(true);
		}

		public void TestGetWorkflowTriggerAction_ForAutoRateNonConsolLevelCostsAndRevenue()
		{
			if (WorkflowDescriptor.GetWorkflowTriggerActionTypes().ContainsCode(WorkflowTriggerActionTypeConstants.Codes.AutoRateNonConsolLevelCostsAndRevenue))
			{
				SetupOrgCommunicationModes(GlbCompany.CurrentCompany.OrgProxy);
				var testProviders = GetParentsWithConfiguredOrganisationPartiesForTest();
				const string message = "You must override method GetParentsWithConfiguredOrganisationPartiesForTest on this test class when there are SupportedMessageRecipientParties";
				Assert(message, testProviders.Length > 0);
				AssertNotNull(message, testProviders[0]);

				var trigger = testProviders[0].WorkflowItems.Triggers.AddNew();
				trigger.P9_Description = "DUMMY TASK";
				trigger.ReferenceCode = "REF";
				var notification = trigger.ProcessTaskNotifications.AddNew();
				notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.AutoRateNonConsolLevelCostsAndRevenue;

				var notificationAction = WorkflowDescriptor.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(Factory));
				AssertNotNull("Workflow notification action run for 'NAR' trigger action type", notificationAction);
				if (notificationAction is IWrappingProcessor actionWithWrappedProcessor)
				{
					AssertEquals("The IProcessor type of the WrappedProcessor should be as expected", "Enterprise.Accounting.Business.WorkflowAutoRater", actionWithWrappedProcessor.WrappedProcessor.GetType().FullName);
				}
				else
				{
					AssertEquals("The IProcessor type should be as expected", "Enterprise.Accounting.Business.WorkflowAutoRater", notificationAction.GetType().FullName);
				}
			}
			Assert(true);
		}

		public void TestGetWorkflowTriggerAction_ForAutoPack()
		{
			if (WorkflowDescriptor.GetWorkflowTriggerActionTypes().ContainsCode(WorkflowTriggerActionTypeConstants.Codes.AutoPack))
			{
				SetupOrgCommunicationModes(GlbCompany.CurrentCompany.OrgProxy);
				var testProviders = GetParentsWithConfiguredOrganisationPartiesForTest();
				const string message = "You must override method GetParentsWithConfiguredOrganisationPartiesForTest on this test class when there are SupportedMessageRecipientParties";
				Assert(message, testProviders.Length > 0);
				AssertNotNull(message, testProviders[0]);

				var trigger = testProviders[0].WorkflowItems.Triggers.AddNew();
				trigger.P9_Description = "DUMMY TASK";
				trigger.ReferenceCode = "REF";
				var notification = trigger.ProcessTaskNotifications.AddNew();
				notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.AutoPack;

				var notificationAction = WorkflowDescriptor.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(Factory));
				AssertNotNull("Workflow notification action run for 'APK' trigger action type", notificationAction);
				AssertEquals("The IProcessor type should be as expected", "Enterprise.Warehouse.Transactions.Business.WhsOrderAutoPackProcessor", notificationAction.GetType().FullName);
			}
			Assert(true);
		}

		public void TestGetWorkflowTriggerAction_ForCreateTransportBooking()
		{
			if (WorkflowDescriptor.GetWorkflowTriggerActionTypes().ContainsCode(WorkflowTriggerActionTypeConstants.Codes.CreateTransportBooking))
			{
				SetupOrgCommunicationModes(GlbCompany.CurrentCompany.OrgProxy);
				var testProviders = GetParentsWithConfiguredOrganisationPartiesForTest();
				const string message = "You must override method GetParentsWithConfiguredOrganisationPartiesForTest on this test class when there are SupportedMessageRecipientParties";
				Assert(message, testProviders.Length > 0);
				AssertNotNull(message, testProviders[0]);

				var trigger = testProviders[0].WorkflowItems.Triggers.AddNew();
				trigger.P9_Description = "DUMMY TASK";
				trigger.ReferenceCode = "REF";
				var notification = trigger.ProcessTaskNotifications.AddNew();
				notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.CreateTransportBooking;

				var notificationAction = WorkflowDescriptor.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(Factory));
				AssertNotNull("Workflow notification action run for 'CTB' trigger action type", notificationAction);
				AssertEquals("The IProcessor type should be as expected", "Enterprise.TransportBookings.Business.DtbBookingProcessor", notificationAction.GetType().FullName);
			}
			Assert(true);
		}

		public void TestGetWorkflowTriggerAction_ForCreateTransportBookingContainer()
		{
			if (WorkflowDescriptor.GetWorkflowTriggerActionTypes().ContainsCode(WorkflowTriggerActionTypeConstants.Codes.CreateTransportBookingContainer))
			{
				SetupOrgCommunicationModes(GlbCompany.CurrentCompany.OrgProxy);
				var testProviders = GetParentsWithConfiguredOrganisationPartiesForTest();
				const string message = "You must override method GetParentsWithConfiguredOrganisationPartiesForTest on this test class when there are SupportedMessageRecipientParties";
				Assert(message, testProviders.Length > 0);
				AssertNotNull(message, testProviders[0]);

				var trigger = testProviders[0].WorkflowItems.Triggers.AddNew();
				trigger.P9_Description = "DUMMY TASK";
				trigger.ReferenceCode = "REF";
				var notification = trigger.ProcessTaskNotifications.AddNew();
				notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.CreateTransportBookingContainer;

				var notificationAction = WorkflowDescriptor.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(Factory));
				AssertNotNull("Workflow notification action run for 'CTC' trigger action type", notificationAction);
				AssertEquals("The IProcessor type should be as expected", "Enterprise.TransportBookings.Business.DtbBookingProcessor", notificationAction.GetType().FullName);
			}
			Assert(true);
		}

		public void TestGetWorkflowTriggerAction_ForPostConsolCostOnly()
		{
			AssertGetWorkflowTriggerActionWithAccountingTriggerActionWithOptionalFactorySaveProcessorCreator(WorkflowTriggerActionTypeConstants.Codes.PostConsolCostOnly);
		}

		public void TestGetWorkflowTriggerAction_ForPostAllRevenue()
		{
			AssertGetWorkflowTriggerActionWithAccountingTriggerActionWithOptionalFactorySaveProcessorCreator(WorkflowTriggerActionTypeConstants.Codes.PostAllRevenue);
		}

		public void TestGetWorkflowTriggerAction_ForPostAllCost()
		{
			AssertGetWorkflowTriggerActionWithAccountingTriggerActionWithOptionalFactorySaveProcessorCreator(WorkflowTriggerActionTypeConstants.Codes.PostAllCosts);
		}

		void AssertGetWorkflowTriggerActionWithAccountingTriggerActionWithOptionalFactorySaveProcessorCreator(string workflowTriggerActionType)
		{
			if (WorkflowDescriptor.GetWorkflowTriggerActionTypes().ContainsCode(workflowTriggerActionType)) // The tests that use this method should be run for a particular descriptor test class, like ForwardingShipmentWorkflowDescriptor, to make this IF to be true.
			{
				SetupOrgCommunicationModes(GlbCompany.CurrentCompany.OrgProxy);
				var testProviders = GetParentsWithConfiguredOrganisationPartiesForTest();
				const string message = "You must override method GetParentsWithConfiguredOrganisationPartiesForTest on this test class when there are SupportedMessageRecipientParties";
				Assert(message, testProviders.Length > 0);
				AssertNotNull(message, testProviders[0]);

				var trigger = testProviders[0].WorkflowItems.Triggers.AddNew();
				trigger.P9_Description = "DUMMY TASK";
				trigger.ReferenceCode = "REF";
				var notification = trigger.ProcessTaskNotifications.AddNew();
				notification.PQ_TriggerType = workflowTriggerActionType;

				AssertAccountingTriggerActionWithOptionalFactorySaveProcessorCreator(testProviders, notification);
			}
			Assert(true);

			void AssertAccountingTriggerActionWithOptionalFactorySaveProcessorCreator(IWorkflowProvider[] testProviders, ProcessTaskNotification notification)
			{
				var notificationAction = WorkflowDescriptor.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(Factory));
				AssertNotNull("Workflow notification action run for the trigger action type", notificationAction);
				if (notificationAction is IWrappingProcessor actionWithWrappedProcessor)
				{
					AssertEquals("The IProcessor type of the WrappedProcessor should be as expected", "Enterprise.Accounting.Business.TriggerActionWithOptionalFactorySave.TriggerActionWithOptionalFactorySaveProcessor", actionWithWrappedProcessor.WrappedProcessor.GetType().FullName);
				}
				else
				{
					AssertEquals("The IProcessor type should be as expected", "Enterprise.Accounting.Business.TriggerActionWithOptionalFactorySave.TriggerActionWithOptionalFactorySaveProcessor", notificationAction.GetType().FullName);
				}

				var optionalSavePRocessorMock = new Mock<IAccountingTriggerActionWithOptionalFactorySaveProcessorCreator>();
				ObjectFactory.Substitute(optionalSavePRocessorMock.Object);
				var processorMock = new Mock<IProcessor>();
				var queuedLog = new QueuedLogForTesting(Factory);
				optionalSavePRocessorMock.Setup(x => x.Create(notification, testProviders[0].WorkflowItems.Parent, queuedLog)).Returns(processorMock.Object);
				notificationAction = WorkflowDescriptor.GetWorkflowTriggerAction(notification, queuedLog);
				if (notificationAction is IWrappingProcessor actionWithWrappedProcessor2)
				{
					AssertNotNull("Workflow notification action run for the trigger action type", actionWithWrappedProcessor2.WrappedProcessor);
					AssertEquals("The IProcessor type of the WrappedProcessor should be as expected", processorMock.Object, actionWithWrappedProcessor2.WrappedProcessor);
				}
				else
				{
					AssertNotNull("Workflow notification action run for the trigger action type", notificationAction);
					AssertEquals("The IProcessor type should be as expected", processorMock.Object, notificationAction);
				}
			}
		}

		public void TestGetWorkflowTriggerAction_ForCreateJobInvoiceHeader()
		{
			AssertGetWorkflowTriggerActionWithAccountingTriggerActionWithOptionalFactorySaveProcessorCreator(WorkflowTriggerActionTypeConstants.Codes.CreateJobInvoiceHeader);
		}

		public void TestGetWorkflowTriggerAction_CreateProfitShareCharges()
		{
			AssertGetWorkflowTriggerActionWithAccountingTriggerActionWithOptionalFactorySaveProcessorCreator(WorkflowTriggerActionTypeConstants.Codes.CreateProfitShareCharges);
		}

		public void TestGetWorkflowTriggerAction_ForPostAllSisterCompanyCharges()
		{
			AssertGetWorkflowTriggerActionWithAccountingTriggerActionWithOptionalFactorySaveProcessorCreator(WorkflowTriggerActionTypeConstants.Codes.PostAllSisterCompanyCharges);
		}

		public void TestGetWorkflowTriggerAction_ForPostLocalSisterCompanyChargesOnly()
		{
			AssertGetWorkflowTriggerActionWithAccountingTriggerActionWithOptionalFactorySaveProcessorCreator(WorkflowTriggerActionTypeConstants.Codes.PostLocalSisterCompanyChargesOnly);
		}

		public void TestGetWorkflowTriggerAction_ForPostOverseasAgentCharges()
		{
			AssertGetWorkflowTriggerActionWithAccountingTriggerActionWithOptionalFactorySaveProcessorCreator(WorkflowTriggerActionTypeConstants.Codes.PostOverseasAgentCharges);
		}

		public void TestGetWorkflowTriggerAction_ForSendARInvoice()
		{
			if (WorkflowDescriptor.GetWorkflowTriggerActionTypes().ContainsCode(WorkflowTriggerActionTypeConstants.Codes.SendARInvoice))
			{
				SetupOrgCommunicationModes(GlbCompany.CurrentCompany.OrgProxy);
				var testProviders = GetParentsWithConfiguredOrganisationPartiesForTest();
				const string message = "You must override method GetParentsWithConfiguredOrganisationPartiesForTest on this test class when there are SupportedMessageRecipientParties";
				Assert(message, testProviders.Length > 0);
				AssertNotNull(message, testProviders[0]);

				var trigger = testProviders[0].WorkflowItems.Triggers.AddNew();
				trigger.P9_Description = "DUMMY TASK";
				trigger.ReferenceCode = "REF";
				var notification = trigger.ProcessTaskNotifications.AddNew();
				notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendARInvoice;

				var notificationAction = WorkflowDescriptor.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(Factory));
				AssertNotNull("Workflow notification action run for 'SNR' trigger action type", notificationAction);
				AssertEquals("The IProcessor type should be as expected", "Enterprise.Accounting.Business.ARAP.Invoicing.Printing.SendARInvoiceProcessor", notificationAction.GetType().FullName);
			}
			Assert(true);
		}

		public void TestGetWorkflowTriggerAction_ForGenerateARInvoiceToEdocs()
		{
			if (WorkflowDescriptor.GetWorkflowTriggerActionTypes().ContainsCode(WorkflowTriggerActionTypeConstants.Codes.GenerateARInvoiceToEdocs))
			{
				SetupOrgCommunicationModes(GlbCompany.CurrentCompany.OrgProxy);
				var testProviders = GetParentsWithConfiguredOrganisationPartiesForTest();
				const string message = "You must override method GetParentsWithConfiguredOrganisationPartiesForTest on this test class when there are SupportedMessageRecipientParties";
				Assert(message, testProviders.Length > 0);
				AssertNotNull(message, testProviders[0]);

				var trigger = testProviders[0].WorkflowItems.Triggers.AddNew();
				trigger.P9_Description = "DUMMY TASK";
				trigger.ReferenceCode = "REF";
				var notification = trigger.ProcessTaskNotifications.AddNew();
				notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.GenerateARInvoiceToEdocs;

				var notificationAction = WorkflowDescriptor.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(Factory));
				AssertNotNull("Workflow notification action run for 'GAR' trigger action type", notificationAction);
				AssertEquals("The IProcessor type should be as expected", "Enterprise.Accounting.Business.ARAP.Invoicing.Printing.GenerateARInvoiceToEdocsProcessor", notificationAction.GetType().FullName);
			}
			Assert(true);
		}

		public void TestUniversalXmlWorkflowProcessorHelper_NoNullReference()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			var log = new QueuedLogForTesting(trigger.Logs.AddNew(Events.WorkflowTriggerEvent), trigger);
			var dataExport = new ManualDataExport(Factory, dummy, UniversalDataType.UniversalEvent);
			dataExport.RecipientPK = org.PK;
			var helper = new UniversalShipmentTriggerActionBuilder(DummyWorkflowDescriptor.Instance, new ActionWrapper(dataExport, dummy), new EventInfoProvider(log, trigger, dummy));
			AssertNoExceptionThrown(() => helper.GetUniversalWorkflowProcessor());
		}

		public void TestUniversalXmlWorkflowProcessorHelper_NoStmALog()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithWorkflow>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();
			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			var log = new QueuedLogForTesting(trigger.Logs.AddNew(Events.WorkflowTriggerEvent), trigger);
			var dataExport = new ManualDataExport(Factory, dummy, UniversalDataType.UniversalEvent);
			dataExport.EventCode = Events.CustomisableEvent03Code;
			dataExport.RecipientPK = org.PK;
			var helper = new UniversalShipmentTriggerActionBuilder(DummyWorkflowDescriptor.Instance, new ActionWrapper(dataExport, dummy), new EventInfoProvider(log, trigger, dummy));
			var processor = helper.GetUniversalWorkflowProcessor();
			AssertEquals(0, dataExport.Factory.Load<StmALog>(new ZQuery(StmALogSchema.SL_SE_NKEvent, dataExport.EventCode) { FetchOnlyFromLocalCache = true }).Length);
		}

		public void TestGetWorkflowTriggerAction_ForIncludeChargeInProfitShare()
		{
			AssertGetWorkflowTriggerActionWithAccountingTriggerActionWithOptionalFactorySaveProcessorCreator(WorkflowTriggerActionTypeConstants.Codes.IncludeChargeInProfitShare);
		}

		#region Email Notification Delivery Trigger

		public void TestGetWorkflowTriggerAction_ForNotificationEmailDelivery()
		{
			if (WorkflowDescriptor.GetWorkflowTriggerActionTypes().ContainsCode(WorkflowTriggerActionTypeConstants.Codes.NotificationEmail))
			{
				SetupOrgCommunicationModes(GlbCompany.CurrentCompany.OrgProxy);
				var testProviders = GetParentsWithConfiguredOrganisationPartiesForTest();
				const string message = "You must override method GetParentsWithConfiguredOrganisationPartiesForTest on this test class when there are SupportedMessageRecipientParties";
				Assert(message, testProviders.Length > 0);
				AssertNotNull(message, testProviders[0]);

				foreach (var partyTypeDescriptionPair in new MessageRecipientPartyTypeList(WorkflowDescriptor.SupportedMessageRecipientParties(null, null)))
				{
					var trigger = testProviders[0].WorkflowItems.Triggers.AddNew();
					trigger.P9_Description = "DUMMY TASK";
					trigger.ReferenceCode = "REF";
					var line = SetupLineTriggerIfRequired(testProviders[0], trigger) ?? (BusinessObject)testProviders[0];

					var notification = trigger.ProcessTaskNotifications.AddNew();
					notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
					notification.PQ_TriggerParty = MessageRecipientPartyTypeList.AllPossiblePartyTypes.GetCodeFromDescription(partyTypeDescriptionPair.Description);
					SetNotificationEmailAddress(line, notification, "dummy@test.com");

					SetAdditionalPropertiesForTestGetWorkflowTriggerAction_ForNotificationEmailDelivery(testProviders[0], notification.PQ_TriggerParty);
					var log = new QueuedLogForTesting(Factory);
					WorkflowTriggerNotification notificationAction;
					var intermediateAction = WorkflowDescriptor.GetWorkflowTriggerAction(new WorkflowTriggerActionSource(line, trigger, notification, log, null), log);
					if (intermediateAction is IWrappingProcessor actionWithWrappedProcessor)
					{
						notificationAction = actionWithWrappedProcessor.WrappedProcessor as WorkflowTriggerNotification;
					}
					else
					{
						notificationAction = intermediateAction as WorkflowTriggerNotification;
					}
					AssertNotNull("Workflow notification action run for 'NTF' trigger action type", notificationAction);
					var errorMessage = string.Format("Override AddToMessageRecipientPartyList and add '{0}' to the list", notification.PQ_TriggerParty);
					AssertEquals(errorMessage, 1, notificationAction.Modes.CommunicationModes.Count);
					AssertEquals(EDICommunicationsModeFileFormatList.Codes.NotificationEmail, notificationAction.Modes.CommunicationModes[0].EK_FileFormat);
					if (WorkflowDescriptor.IsDirectEmailRecipient(notification.PQ_TriggerParty))
					{
						AssertEquals(notification.PQ_TriggerType == WorkflowTriggerActionTypeConstants.Codes.NotificationEmail ? EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText : EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsAttachment, notificationAction.Modes.CommunicationModes[0].EK_CommunicationsTransport);
						AssertEquals("dummy@test.com", notificationAction.Modes.CommunicationModes[0].EK_Destination);
					}
					else
					{
						AssertEquals(EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText, notificationAction.Modes.CommunicationModes[0].EK_CommunicationsTransport);
						AssertEquals(notificationAction.Modes.CommunicationModes[0].Organisation.OH_FullName + "@notificationemail.cargowise.com", notificationAction.Modes.CommunicationModes[0].EK_Destination);
					}
				}
			}
			Assert(true);
		}

		protected virtual BusinessObject SetupLineTriggerIfRequired(IWorkflowProvider parent, ProcessTask trigger)
		{
			return null;
		}

		protected virtual void SetNotificationEmailAddress(BusinessObject line, ProcessTaskNotification notification, string emailAddress)
		{
			notification.PQ_EmailAddr = emailAddress;
		}

		protected virtual void SetAdditionalPropertiesForTestGetWorkflowTriggerAction_ForNotificationEmailDelivery(IWorkflowProvider workflowProvider, string partyTypeCode)
		{
		}

		#endregion

		#region Xml Message Delivery Trigger

		public void TestGetAndRunWorkflowTriggerAction_ForXmlMessageDelivery_ForEachPartyType()
		{
			if (!CheckIfXmlDeliveryTestRequired())
			{
				return;
			}

			var testProviders = GetParentsWithConfiguredOrganisationPartiesForTest();

			Assert(
				"WorkflowDescriptor supports XML message delivering.\r\n" +
				"GetTestParentsWithConfiguredOrganisationParties() must be implemented to return at least 1 IWorkflowProvider",
				testProviders.Length > 0);

			foreach (MessageRecipientPartyType partyType in MessageRecipientPartyType.GetValues())
			{
				foreach (var provider in testProviders)
				{
					var expectedRecipientParties = GetExpectedOrganisationsForPartyType(provider.WorkflowItems.Triggers.AddNew(), partyType);
					RunDeliveryWorkflowTriggerAction_AndAssertXmlMessageDelivery(provider, partyType, expectedRecipientParties, WorkflowTriggerActionTypeConstants.Codes.SendXML, shouldSave: false);
				}
			}
			try
			{
				Factory.Saving += (s) => throw new Exception("Crash fast");
				Factory.Save();
			}
			catch (Exception)
			{
				// Just making sure that every DeliveryContext gets a chance to save.
			}
		}

		public void TestGetAndRunWorkflowTriggerAction_ForXmlMessageDelivery_ForAllParties()
		{
			if (!CheckIfXmlDeliveryTestRequired())
			{
				return;
			}

			var testProviders = GetParentsWithConfiguredOrganisationPartiesForTest();

			Assert(
				"WorkflowDescriptor supports XML message delivering.\r\n" +
				"GetTestParentsWithConfiguredOrganisationParties() must be implemented to return at least 1 IWorkflowProvider",
				testProviders.Length > 0);

			var allPartyTypes = GetAllPartyTypes();
			foreach (var provider in testProviders)
			{
				var task = provider.WorkflowItems.Triggers.AddNew();
				var expectedRecipientParties = GetExpectedOrganisationsForPartyType(task, allPartyTypes);
				RunDeliveryWorkflowTriggerAction_AndAssertXmlMessageDelivery(provider, allPartyTypes, expectedRecipientParties, WorkflowTriggerActionTypeConstants.Codes.SendXML);
			}
		}

		public void TestGetAndRunWorkflowTriggerAction_ForXmlMessageDelivery_DoesNotAddDuplicatedParties()
		{
			if (!CheckIfXmlDeliveryTestRequired())
			{
				return;
			}

			var onlyOrgParty = GetOrgWithCommunicationModes("TheOnlyOrgParty");
			SetAllOrgPartiesAsTheSameForDoesNotAddDuplicatedPartiesTest(onlyOrgParty);
			var testProviders = GetParentsWithConfiguredOrganisationPartiesForTest();

			Assert(
				"WorkflowDescriptor supports XML message delivering.\r\n" +
				"GetTestParentsWithConfiguredOrganisationParties() must be implemented to return at least 1 IWorkflowProvider",
				testProviders.Length > 0);

			foreach (var provider in testProviders)
			{
				var task = provider.WorkflowItems.Triggers.AddNew();
				var partyType = WorkflowDescriptor.SupportedMessageRecipientParties(task, (IBusiness)provider);
				var expectedRecipientParties = GetExpectedOrganisationsForPartyType(task, partyType);
				Assert("All Orgs are the same. Number of expected parties should be <= 1, but was " + expectedRecipientParties.Length.ToString(), expectedRecipientParties.Length <= 1);
				RunDeliveryWorkflowTriggerAction_AndAssertXmlMessageDelivery(provider, partyType, expectedRecipientParties, WorkflowTriggerActionTypeConstants.Codes.SendXML);
			}
		}

		public void TestGetAndRunWorkflowTriggerAction_ForXmlMessageDeliveryWithFallback_ForAllParties()
		{
			if (!CheckIfXmlDeliveryWithFallbackTestRequired())
			{
				return;
			}

			var testProvider = GetParentWithConfiguredOrganisationPartiesForXmlFallbackTest();

			Assert(
				"WorkflowDescriptor supports XML message delivering with fallback.\r\n" +
				"GetTestParentWithConfiguredOrganisationPartiesForXmlFallbackTest() must be implemented",
				testProvider != null);

			if (testProvider == null)
			{
				return;
			}

			var allPartyTypes = GetAllPartyTypes();
			var task = testProvider.WorkflowItems.Triggers.AddNew();
			var expectedRecipientParties = GetExpectedOrganisationsForPartyType(task, allPartyTypes);
			RunDeliveryWorkflowTriggerAction_AndAssertXmlMessageDelivery(testProvider, allPartyTypes, expectedRecipientParties, WorkflowTriggerActionTypeConstants.Codes.SendXMLWithJobFallback);
		}

		public void TestEventTriggeredXmlExportWillContainTriggeredByElement1()
		{
			TestEventOrFieldTriggeredXmlExport(null);
		}

		public void TestEventTriggeredXmlExportWillContainTriggeredByElement2()
		{
			TestEventOrFieldTriggeredXmlExport(Array.Empty<StmChangeLog>());
		}

		public void TestFieldTriggeredXmlExportWillNotContainTriggeredByElement()
		{
			var log1 = Factory.New<StmChangeLog>();
			log1.SY_Changes = "test changes";

			TestEventOrFieldTriggeredXmlExport(new[] { log1 });
		}

		void TestEventOrFieldTriggeredXmlExport(StmChangeLog[] triggeringChangeLogs)
		{
			if (!CheckIfXmlDeliveryTestRequired())
			{
				return;
			}

			var testProviders = GetParentsWithConfiguredOrganisationPartiesForTest();
			Assert(
				"WorkflowDescriptor supports XML message delivering.\r\n" +
				"GetTestParentsWithConfiguredOrganisationParties() must be implemented to return at least 1 IWorkflowProvider",
				testProviders.Length > 0);

			var allPartyTypes = GetAllPartyTypes();

			Env.OutgoingMailManager.EmailsCreated.Clear();

			using (Factory.AddDisposableService())
			{
				foreach (var provider in testProviders)
				{
					var providerAsBizO = provider as BusinessObject;
					if (providerAsBizO == null)
					{
						continue;
					}

					providerAsBizO.GetLogs().AddNew(Events.Booked);
					providerAsBizO.GetLogs().AddNew(Events.Arrival);
					providerAsBizO.GetLogs().AddNew(Events.Departure);

					var recipientPartyTypeList = new MessageRecipientPartyTypeList(WorkflowDescriptor.SupportedMessageRecipientParties(null, null));
					var triggerParty = MessageRecipientPartyTypeList.AllPossiblePartyTypes.GetCodeFromDescription(recipientPartyTypeList[0].Description);
					var triggerParty2 = MessageRecipientPartyTypeList.AllPossiblePartyTypes.GetCodeFromDescription(recipientPartyTypeList[1].Description);
					var milestone1 = provider.WorkflowItems.Milestones.AddNew();
					milestone1.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
					var notification1 = milestone1.ProcessTaskNotifications.AddNew();
					notification1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXML;
					notification1.PQ_TriggerParty = triggerParty;

					var milestone2 = provider.WorkflowItems.Milestones.AddNew();
					milestone2.TriggerConditions.TriggerEventCode = Events.Booked.Code;
					var notification2 = milestone2.ProcessTaskNotifications.AddNew();
					notification2.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXML;
					notification2.PQ_TriggerParty = triggerParty;

					var milestone3 = provider.WorkflowItems.Milestones.AddNew();
					milestone3.TriggerConditions.TriggerEventCode = Events.Departure.Code;
					var notification3 = milestone3.ProcessTaskNotifications.AddNew();
					notification3.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXML;
					notification3.PQ_EmailAddr = "test@gmail.com";
					notification3.PQ_TriggerParty = triggerParty2;

					var processor = WorkflowDescriptor.GetWorkflowTriggerAction(notification1, new QueuedLogForTesting(Factory) { ChangeLogs = triggeringChangeLogs });
					processor.Process(new NotificationBuffer());

					processor = WorkflowDescriptor.GetWorkflowTriggerAction(notification2, new QueuedLogForTesting(Factory) { ChangeLogs = triggeringChangeLogs });
					processor.Process(new NotificationBuffer());

					processor = WorkflowDescriptor.GetWorkflowTriggerAction(notification3, new QueuedLogForTesting(Factory) { ChangeLogs = triggeringChangeLogs });
					processor.Process(new NotificationBuffer());
					try
					{
						Factory.Save();
					}
					catch (Exception)
					{
						// IDC about your weird duplicate index entry issue right now. JobShipmentPreplanning
					}
					Assert("At least 1 Email created", Env.OutgoingMailManager.EmailsCreated.Count > 0);

					var email = Env.OutgoingMailManager.EmailsCreated.Count > 1 ? Env.OutgoingMailManager.EmailsCreated[1] : Env.OutgoingMailManager.EmailsCreated[0];
					var xmlText = Encoding.UTF8.GetString(email.Attachments[0].Data);

					if (!xmlText.Contains("<Event>") || (!xmlText.Contains("<Code>BKD</Code>") && !xmlText.Contains("<Code>ARV</Code>") && !xmlText.Contains("<Code>DEP</Code>")))
					{
						continue;
					}

					var containsTriggeredBy = xmlText.Contains("<TriggeredBy>true</TriggeredBy>");
					if (triggeringChangeLogs == null || triggeringChangeLogs.Length == 0)
					{
						if (!containsTriggeredBy)
						{
							Fail("Xml generated below should contain <TriggeredBy>true</TriggeredBy>." + System.Environment.NewLine + System.Environment.NewLine + xmlText);
						}
					}
					else
					{
						if (containsTriggeredBy)
						{
							Fail("Xml generated below should NOT contain <TriggeredBy>true</TriggeredBy>." + System.Environment.NewLine + System.Environment.NewLine + xmlText);
						}
					}
				}
			}
		}

		protected virtual IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			return Array.Empty<IWorkflowProvider>();
		}

		protected virtual IWorkflowProvider GetParentWithConfiguredOrganisationPartiesForXmlFallbackTest()
		{
			return null;
		}

		protected void RunDeliveryWorkflowTriggerAction_AndAssertXmlMessageDelivery(IWorkflowProvider provider, MessageRecipientPartyType partyType, OrgHeader[] expectedPartiesWithDeliveredModes, string triggerActionType, bool shouldSave = true)
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();
			using (Factory.AddDisposableService())
			{
				var count = RunDeliveryWorkflowTriggerActionForParent(provider, partyType, triggerActionType, shouldSave);
				if (shouldSave)
				{
					try
					{
						Factory.Save();
					}
					catch (Exception)
					{
						// Just making sure that every DeliveryContext gets a chance to save.
					}
				}
				AssertXmlMessageDelivery(expectedPartiesWithDeliveredModes, count);
			}
		}

		protected int RunDeliveryWorkflowTriggerActionForParent(IWorkflowProvider provider, MessageRecipientPartyType partyTypes, string triggerActionType, bool shouldSave = true)
		{
			using (provider.LogsFactory.AddDisposableService())
			{
				int result = 0;
				var milestone = provider.WorkflowItems.Milestones.AddNew();
				milestone.TriggerConditions.TriggerEventCode = Events.Arrival.Code;
				for (var i = 0; i < MessageRecipientPartyTypeList.AllPossiblePartyTypes.Count; i++)
				{
					if (partyTypes == (partyTypes | ProcessTask.GetMessageRecipientPartyTypeFromCode(MessageRecipientPartyTypeList.AllPossiblePartyTypes[i].Code)))
					{
						ProcessTaskNotification notification = milestone.ProcessTaskNotifications.AddNew();
						notification.PQ_TriggerType = triggerActionType;
						notification.PQ_TriggerParty = MessageRecipientPartyTypeList.AllPossiblePartyTypes[i].Code;
						notification.PQ_P9 = milestone.PK;
						notification.PQ_EmailAddr = "dummy@test.com";
						milestone.P9_Description = "MILESTONE";
						milestone.ReferenceCode = "REF";
						var processor = WorkflowDescriptor.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(provider.WorkflowItems.Factory));
						processor.Process(new NotificationBuffer());
						if (processor is IMessageProcessor messageProcessor)
						{
							if (messageProcessor.GetDestinations().Destinations.Any())
							{
								result++;
							}
						}
						else
						{
							result++;
						}
					}
				}
				if (shouldSave)
				{
					try
					{
						provider.WorkflowItems.Factory.Save();
					}
					catch (Exception)
					{
						// NOM NOM NOM I DONT CARE ABOUT YOUR EXCEPTION! This is just about proving that all of the DeliveryContext objects get saved.
					}
				}
				return result;
			}
		}

		protected void AssertXmlMessageDelivery(OrgHeader[] expectedPartiesWithDeliveredModes, int numberOfActions)
		{
			if (numberOfActions != Env.OutgoingMailManager.EmailsCreated.Count)
			{
				// Messages Expected
				var expectedOrderedMessages = new SortedDictionary<string, object>();
				foreach (var orgParty in expectedPartiesWithDeliveredModes)
				{
					expectedOrderedMessages.Add(orgParty.OH_FullName, null);
				}

				var expectedMessages = new StringBuilder();
				expectedMessages.AppendLine();
				expectedMessages.AppendLine(ZString.Format("Messages Expected ({0}):", expectedOrderedMessages.Keys.Count));

				foreach (string expectedMessage in expectedOrderedMessages.Keys)
				{
					expectedMessages.AppendLine("  " + expectedMessage);
				}

				// Messages Created
				var createdOrderedMessages = new SortedDictionary<string, object>();
				foreach (var message in Env.OutgoingMailManager.EmailsCreated)
				{
					createdOrderedMessages.Add(message.Attachments[0].DisplayName, null);
				}

				var createdMessages = new StringBuilder();
				createdMessages.AppendLine();
				createdMessages.AppendLine(ZString.Format("Messages Created ({0}):", createdOrderedMessages.Keys.Count));

				foreach (string createdMessage in createdOrderedMessages.Keys)
				{
					createdMessages.AppendLine("  " + createdMessage);
				}

				Fail(expectedMessages.ToString() + createdMessages);
			}

			AssertEquals("Number of messages created", numberOfActions, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		MessageRecipientPartyType GetAllPartyTypes()
		{
			var result = MessageRecipientPartyType.None;

			foreach (MessageRecipientPartyType partyType in MessageRecipientPartyType.GetValues())
			{
				result = result | partyType;
			}

			return result;
		}

		void SetAllOrgPartiesAsTheSameForDoesNotAddDuplicatedPartiesTest(OrgHeader onlyOrgParty)
		{
			consigneeOrg = onlyOrgParty;
			consignorOrg = onlyOrgParty;
			billToPartyOrg = onlyOrgParty;
			brokerOrg = onlyOrgParty;
			pickupCartageOrg = onlyOrgParty;
			deliveryCartageOrg = onlyOrgParty;
			pickupAgentOrg = onlyOrgParty;
			deliveryAgentOrg = onlyOrgParty;
			receivingAgentOrg = onlyOrgParty;
			sendingAgentOrg = onlyOrgParty;
			controllingAgentOrg = onlyOrgParty;
			controllingCustomerOrg = onlyOrgParty;
			orgProxyOrg = onlyOrgParty;
			clientOrg = onlyOrgParty;
			carrierOrg = onlyOrgParty;
			notifyPartyOrg = onlyOrgParty;
			pickupFromOrg = onlyOrgParty;
			deliverToOrg = onlyOrgParty;
			departureCFSOrg = onlyOrgParty;
			arrivalCFSOrg = onlyOrgParty;
			forwarder = onlyOrgParty;
			arrivalCarrierOrg = onlyOrgParty;
			departureCarrierOrg = onlyOrgParty;
			principalOrg = onlyOrgParty;
			departureCTOOrg = onlyOrgParty;
			arrivalCTOOrg = onlyOrgParty;
			departureContainerYardOrg = onlyOrgParty;
			arrivalContainerYardOrg = onlyOrgParty;
			warehouseInwardsOrg = onlyOrgParty;
			warehouseOutwardsOrg = onlyOrgParty;
			warehouseOrg = onlyOrgParty;
			hvlvAirClearanceAgentOrg = onlyOrgParty;
			hvlvSeaClearanceAgentOrg = onlyOrgParty;
			shippingManagerOrg = onlyOrgParty;
			bookingPartyOrg = onlyOrgParty;
			destinationDepotOrg = onlyOrgParty;
			exportReceivingDepotOrg = onlyOrgParty;
			importReleaseDepotOrg = onlyOrgParty;
			externalBrokerOrg = onlyOrgParty;
		}

		protected virtual OrgHeader[] GetExpectedOrganisationsForPartyType(ProcessTask task, MessageRecipientPartyType partyType)
		{
			var result = new MessageRecipientPartyCollection();

			if (IsExpectingOrganisationForPartyType(partyType, MessageRecipientPartyType.Consignee))
			{
				result.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(ConsigneeOrg, ZString.Empty));
			}

			if (IsExpectingOrganisationForPartyType(partyType, MessageRecipientPartyType.Consignor))
			{
				result.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(ConsignorOrg, ZString.Empty));
			}

			if (IsExpectingOrganisationForPartyType(partyType, MessageRecipientPartyType.BillToParty))
			{
				result.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(BillToPartyOrg, ZString.Empty));
			}

			if (IsExpectingOrganisationForPartyType(partyType, MessageRecipientPartyType.Broker))
			{
				result.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(BrokerOrg, ZString.Empty));
			}

			if (IsExpectingOrganisationForPartyType(partyType, MessageRecipientPartyType.ImportBroker))
			{
				result.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(BrokerOrg, ZString.Empty));
			}

			if (IsExpectingOrganisationForPartyType(partyType, MessageRecipientPartyType.ExportBroker))
			{
				result.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(BrokerOrg, ZString.Empty));
			}

			if (IsExpectingOrganisationForPartyType(partyType, MessageRecipientPartyType.PickupCartage))
			{
				result.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(PickupCartageOrg, ZString.Empty));
			}

			if (IsExpectingOrganisationForPartyType(partyType, MessageRecipientPartyType.DeliveryCartage))
			{
				result.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(DeliveryCartageOrg, ZString.Empty));
			}

			if (IsExpectingOrganisationForPartyType(partyType, MessageRecipientPartyType.ReceivingAgent))
			{
				result.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(ReceivingAgentOrg, ZString.Empty));
			}

			if (IsExpectingOrganisationForPartyType(partyType, MessageRecipientPartyType.SendingAgent))
			{
				result.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(SendingAgentOrg, ZString.Empty));
			}

			if (IsExpectingOrganisationForPartyType(partyType, MessageRecipientPartyType.OrgProxy))
			{
				result.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(OrgProxyOrg, ZString.Empty));
			}

			if (IsExpectingOrganisationForPartyType(partyType, MessageRecipientPartyType.ControllingAgent))
			{
				result.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(ControllingAgentOrg, ZString.Empty));
			}

			if (IsExpectingOrganisationForPartyType(partyType, MessageRecipientPartyType.ControllingCustomer))
			{
				result.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(ControllingCustomerOrg, ZString.Empty));
			}

			if (IsExpectingOrganisationForPartyType(partyType, MessageRecipientPartyType.Client))
			{
				result.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(ClientOrg, ZString.Empty));
			}

			if (IsExpectingOrganisationForPartyType(partyType, MessageRecipientPartyType.Carrier))
			{
				result.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(CarrierOrg, ZString.Empty));
			}

			if (IsExpectingOrganisationForPartyType(partyType, MessageRecipientPartyType.NotifyParty))
			{
				result.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(NotifyPartyOrg, ZString.Empty));
			}

			if (IsExpectingOrganisationForPartyType(partyType, MessageRecipientPartyType.PickupParty))
			{
				result.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(PickupFromOrg, ZString.Empty));
			}

			if (IsExpectingOrganisationForPartyType(partyType, MessageRecipientPartyType.DeliveryToParty))
			{
				result.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(DeliverToOrg, ZString.Empty));
			}

			if (IsExpectingOrganisationForPartyType(partyType, MessageRecipientPartyType.DepartureCFS))
			{
				result.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(DepartureCFSOrg, ZString.Empty));
			}

			if (IsExpectingOrganisationForPartyType(partyType, MessageRecipientPartyType.ArrivalCFS))
			{
				result.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(ArrivalCFSOrg, ZString.Empty));
			}

			if (IsExpectingOrganisationForPartyType(partyType, MessageRecipientPartyType.Forwarder))
			{
				result.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(Forwarder, ZString.Empty));
			}

			if (IsExpectingOrganisationForPartyType(partyType, MessageRecipientPartyType.ArrivalCarrier))
			{
				result.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(ArrivalCarrierOrg, ZString.Empty));
			}

			if (IsExpectingOrganisationForPartyType(partyType, MessageRecipientPartyType.DepartureCarrier))
			{
				result.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(DepartureCarrierOrg, ZString.Empty));
			}

			if (IsExpectingOrganisationForPartyType(partyType, MessageRecipientPartyType.Principal))
			{
				result.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(PrincipalOrg, ZString.Empty));
			}

			if (IsExpectingOrganisationForPartyType(partyType, MessageRecipientPartyType.DepartureCTO))
			{
				result.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(DepartureCTOOrg, ZString.Empty));
			}

			if (IsExpectingOrganisationForPartyType(partyType, MessageRecipientPartyType.ArrivalCTO))
			{
				result.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(ArrivalCTOOrg, ZString.Empty));
			}

			if (IsExpectingOrganisationForPartyType(partyType, MessageRecipientPartyType.DepartureContainerYard))
			{
				result.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(DepartureContainerYardOrg, ZString.Empty));
			}

			if (IsExpectingOrganisationForPartyType(partyType, MessageRecipientPartyType.ArrivalContainerYard))
			{
				result.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(ArrivalContainerYardOrg, ZString.Empty));
			}

			if (IsExpectingOrganisationForPartyType(partyType, MessageRecipientPartyType.WarehouseInwards))
			{
				result.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(WarehouseInwardsOrg, ZString.Empty));
			}

			if (IsExpectingOrganisationForPartyType(partyType, MessageRecipientPartyType.WarehouseOutwards))
			{
				result.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(WarehouseOutwardsOrg, ZString.Empty));
			}

			if (IsExpectingOrganisationForPartyType(partyType, MessageRecipientPartyType.Warehouse))
			{
				result.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(WarehouseOrg, ZString.Empty));
			}

			if (IsExpectingOrganisationForPartyType(partyType, MessageRecipientPartyType.HVLVAirClearanceAgent))
			{
				result.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(HVLVAirClearanceAgentOrg, ZString.Empty));
			}

			if (IsExpectingOrganisationForPartyType(partyType, MessageRecipientPartyType.HVLVSeaClearanceAgent))
			{
				result.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(HVLVSeaClearanceAgentOrg, ZString.Empty));
			}

			if (IsExpectingOrganisationForPartyType(partyType, MessageRecipientPartyType.ShippingManager))
			{
				result.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(ShippingManagerOrg, ZString.Empty));
			}

			if (IsExpectingOrganisationForPartyType(partyType, MessageRecipientPartyType.BookingParty))
			{
				result.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(BookingPartyOrg, ZString.Empty));
			}

			if (IsExpectingOrganisationForPartyType(partyType, MessageRecipientPartyType.DeConsolidator))
			{
				result.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(DestinationDepotOrg, ZString.Empty));
			}

			if (IsExpectingOrganisationForPartyType(partyType, MessageRecipientPartyType.PickupAgent))
			{
				result.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(PickupAgentOrg, ZString.Empty));
			}

			if (IsExpectingOrganisationForPartyType(partyType, MessageRecipientPartyType.DeliveryAgent))
			{
				result.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(DeliveryAgentOrg, ZString.Empty));
			}

			if (IsExpectingOrganisationForPartyType(partyType, MessageRecipientPartyType.DepartureTransitWarehouse))
			{
				result.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(ExportReceivingDepotOrg, ZString.Empty));
			}

			if (IsExpectingOrganisationForPartyType(partyType, MessageRecipientPartyType.ArrivalTransitWarehouse))
			{
				result.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(ImportReleaseDepotOrg, ZString.Empty));
			}

			if (IsExpectingOrganisationForPartyType(partyType, MessageRecipientPartyType.ExternalBroker))
			{
				result.AddNotNullAndNotDuplicatedItem(new MessageRecipientParty(ExternalBrokerOrg, ZString.Empty));
			}

			return result.Select(recipient => recipient.Party).ToArray();
		}

		bool IsExpectingOrganisationForPartyType(MessageRecipientPartyType partyTypes, MessageRecipientPartyType partyType)
		{
			return (partyType & partyTypes) == partyType;
		}

		#region Message Recipient Parties

		protected OrgHeader GetOrgWithCommunicationModes(string organisationName)
		{
			OrgHeader result;
			if (organisationName == "OrgProxyOrg")
			{
				result = GlbCompany.CurrentCompany.OrgProxy;
			}
			else
			{
				result = Factory.NewWithValidTestData<OrgHeader>();
				result.OH_FullName = organisationName;
				SetupOrg(result);
			}
			SetupOrgCommunicationModes(result, organisationName);

			return result;
		}

		protected virtual void SetupOrg(OrgHeader org)
		{
		}

		void SetupOrgCommunicationModes(OrgHeader org)
		{
			SetupOrgCommunicationModes(org, org.OH_FullName);
		}

		void SetupOrgCommunicationModes(OrgHeader org, string organisationName)
		{
			// XML Communication Mode
			var xmlMode = org.EDICommunicationsModes.Cast<EDICommunicationsMode>().FirstOrDefault(x => x.EK_FileFormat == EDICommunicationsModeFileFormatList.Codes.XML && x.EK_Module == WorkflowDescriptor.Code
				&& x.EK_Destination == "test@test.com");
			if (xmlMode == null)
			{
				xmlMode = org.EDICommunicationsModes.AddNew();
				xmlMode.EK_Module = WorkflowDescriptor.Code;
				xmlMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XML;
				xmlMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsAttachment;
				xmlMode.EK_Destination = "test@test.com";
				xmlMode.EK_Filename = organisationName + " - (*JobNumber*)";
			}

			// Email Notification
			var emailNotificationMode = org.EDICommunicationsModes.Cast<EDICommunicationsMode>().FirstOrDefault(x => x.EK_Module == WorkflowDescriptor.Code && x.EK_FileFormat == EDICommunicationsModeFileFormatList.Codes.NotificationEmail
				&& x.EK_Destination == organisationName + "@notificationemail.cargowise.com");
			if (emailNotificationMode == null)
			{
				emailNotificationMode = org.EDICommunicationsModes.AddNew();
				emailNotificationMode.EK_Module = WorkflowDescriptor.Code;
				emailNotificationMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.NotificationEmail;
				emailNotificationMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EmailAsText;
				emailNotificationMode.EK_Destination = organisationName + "@notificationemail.cargowise.com";
			}

			// Some Communication mode not applicable in this case
			var anotherMode = org.EDICommunicationsModes.Cast<EDICommunicationsMode>().FirstOrDefault(x => x.EK_FileFormat == "@@@");
			if (anotherMode == null)
			{
				anotherMode = org.EDICommunicationsModes.AddNew();
				anotherMode.EK_FileFormat = "@@@";
			}
		}

		protected OrgHeader ConsigneeOrg
		{
			get { return consigneeOrg ?? (consigneeOrg = GetOrgWithCommunicationModes("ConsigneeOrg")); }
		}
		OrgHeader consigneeOrg;

		protected OrgHeader ConsignorOrg
		{
			get { return consignorOrg ?? (consignorOrg = GetOrgWithCommunicationModes("ConsignorOrg")); }
		}
		OrgHeader consignorOrg;

		protected OrgHeader BillToPartyOrg
		{
			get { return billToPartyOrg ?? (billToPartyOrg = GetOrgWithCommunicationModes("BillToPartyOrg")); }
		}
		OrgHeader billToPartyOrg;

		protected OrgHeader BrokerOrg
		{
			get { return brokerOrg ?? (brokerOrg = GetOrgWithCommunicationModes("BrokerOrg")); }
		}
		OrgHeader brokerOrg;

		protected OrgHeader PickupCartageOrg
		{
			get
			{
				if (pickupCartageOrg == null)
				{
					pickupCartageOrg = GetOrgWithCommunicationModes("PickupCartageOrg");
					pickupCartageOrg.OH_IsShippingProvider = true;
					pickupCartageOrg.OH_IsLocalTransport = true;
				}
				return pickupCartageOrg;
			}
		}
		OrgHeader pickupCartageOrg;

		protected OrgHeader DeliveryCartageOrg
		{
			get
			{
				if (deliveryCartageOrg == null)
				{
					deliveryCartageOrg = GetOrgWithCommunicationModes("DeliveryCartageOrg");
					deliveryCartageOrg.OH_IsShippingProvider = true;
					deliveryCartageOrg.OH_IsLocalTransport = true;
				}
				return deliveryCartageOrg;
			}
		}
		OrgHeader deliveryCartageOrg;

		protected OrgHeader ReceivingAgentOrg
		{
			get { return receivingAgentOrg ?? (receivingAgentOrg = GetOrgWithCommunicationModes("ReceivingAgentOrg")); }
		}
		OrgHeader receivingAgentOrg;

		protected OrgHeader SendingAgentOrg
		{
			get { return sendingAgentOrg ?? (sendingAgentOrg = GetOrgWithCommunicationModes("SendingAgentOrg")); }
		}
		OrgHeader sendingAgentOrg;

		protected OrgHeader ControllingAgentOrg
		{
			get { return controllingAgentOrg ?? (controllingAgentOrg = this.GetOrgWithCommunicationModes("ControllingAgentOrg")); }
		}
		OrgHeader controllingAgentOrg;

		protected OrgHeader ControllingCustomerOrg
		{
			get { return controllingCustomerOrg ?? (controllingCustomerOrg = this.GetOrgWithCommunicationModes("ControllingCustomerOrg")); }
		}
		OrgHeader controllingCustomerOrg;

		protected OrgHeader OrgProxyOrg
		{
			get { return orgProxyOrg ?? (orgProxyOrg = GetOrgWithCommunicationModes("OrgProxyOrg")); }
		}
		OrgHeader orgProxyOrg;

		protected OrgHeader ClientOrg
		{
			get { return clientOrg ?? (clientOrg = GetOrgWithCommunicationModes("ClientOrg")); }
		}
		OrgHeader clientOrg;

		protected OrgHeader CarrierOrg
		{
			get { return carrierOrg ?? (carrierOrg = GetOrgWithCommunicationModes("CarrierOrg")); }
		}
		OrgHeader carrierOrg;

		protected OrgHeader NotifyPartyOrg
		{
			get { return notifyPartyOrg ?? (notifyPartyOrg = GetOrgWithCommunicationModes("NotifyPartyOrg")); }
		}
		OrgHeader notifyPartyOrg;

		protected OrgHeader DeliverToOrg
		{
			get { return deliverToOrg ?? (deliverToOrg = GetOrgWithCommunicationModes("DeliverToOrg")); }
		}
		OrgHeader deliverToOrg;

		protected OrgHeader PickupFromOrg
		{
			get { return pickupFromOrg ?? (pickupFromOrg = GetOrgWithCommunicationModes("PickupFromOrg")); }
		}
		OrgHeader pickupFromOrg;

		protected OrgHeader DepartureCFSOrg
		{
			get { return departureCFSOrg ?? (departureCFSOrg = GetOrgWithCommunicationModes("DepartureCFSOrg")); }
		}
		OrgHeader departureCFSOrg;

		protected OrgHeader ArrivalCFSOrg
		{
			get { return arrivalCFSOrg ?? (arrivalCFSOrg = GetOrgWithCommunicationModes("ArrivalCFSOrg")); }
		}
		OrgHeader arrivalCFSOrg;

		protected OrgHeader Forwarder
		{
			get { return forwarder ?? (forwarder = GetOrgWithCommunicationModes("Forwarder")); }
		}
		OrgHeader forwarder;

		protected OrgHeader ArrivalCarrierOrg
		{
			get { return arrivalCarrierOrg ?? (arrivalCarrierOrg = GetOrgWithCommunicationModes("ArrivalCarrierOrg")); }
		}
		OrgHeader arrivalCarrierOrg;

		protected OrgHeader DepartureCarrierOrg
		{
			get { return departureCarrierOrg ?? (departureCarrierOrg = GetOrgWithCommunicationModes("DepartureCarrierOrg")); }
		}
		OrgHeader departureCarrierOrg;

		protected OrgHeader PrincipalOrg
		{
			get { return principalOrg ?? (principalOrg = GetOrgWithCommunicationModes("PrincipalOrg")); }
		}
		OrgHeader principalOrg;

		protected OrgHeader DepartureCTOOrg
		{
			get { return departureCTOOrg ?? (departureCTOOrg = GetOrgWithCommunicationModes("DepartureCTOOrg")); }
		}
		OrgHeader departureCTOOrg;

		protected OrgHeader ArrivalCTOOrg
		{
			get { return arrivalCTOOrg ?? (arrivalCTOOrg = GetOrgWithCommunicationModes("ArrivalCTOOrg")); }
		}
		OrgHeader arrivalCTOOrg;

		protected OrgHeader DepartureContainerYardOrg
		{
			get { return departureContainerYardOrg ?? (departureContainerYardOrg = GetOrgWithCommunicationModes("DepartureContainerYardOrg")); }
		}
		OrgHeader departureContainerYardOrg;

		protected OrgHeader ArrivalContainerYardOrg
		{
			get { return arrivalContainerYardOrg ?? (arrivalContainerYardOrg = GetOrgWithCommunicationModes("ArrivalContainerYardOrg")); }
		}
		OrgHeader arrivalContainerYardOrg;

		protected OrgHeader WarehouseInwardsOrg
		{
			get { return warehouseInwardsOrg ?? (warehouseInwardsOrg = GetOrgWithCommunicationModes("WarehouseInwards")); }
		}
		OrgHeader warehouseInwardsOrg;

		protected OrgHeader WarehouseOutwardsOrg
		{
			get { return warehouseOutwardsOrg ?? (warehouseOutwardsOrg = GetOrgWithCommunicationModes("WarehouseOutwards")); }
		}
		OrgHeader warehouseOutwardsOrg;

		protected OrgHeader WarehouseOrg
		{
			get { return warehouseOrg ?? (warehouseOrg = GetOrgWithCommunicationModes("Warehouse")); }
		}
		OrgHeader warehouseOrg;

		protected OrgHeader HVLVAirClearanceAgentOrg
		{
			get { return hvlvAirClearanceAgentOrg ?? (hvlvAirClearanceAgentOrg = GetOrgWithCommunicationModes("HVLVAirClearanceAgent")); }
		}
		OrgHeader hvlvAirClearanceAgentOrg;

		protected OrgHeader HVLVSeaClearanceAgentOrg
		{
			get { return hvlvSeaClearanceAgentOrg ?? (hvlvSeaClearanceAgentOrg = GetOrgWithCommunicationModes("HVLVSeaClearanceAgent")); }
		}
		OrgHeader hvlvSeaClearanceAgentOrg;

		protected OrgHeader ShippingManagerOrg
		{
			get { return shippingManagerOrg ?? (shippingManagerOrg = GetOrgWithCommunicationModes("ShippingManager")); }
		}
		OrgHeader shippingManagerOrg;

		protected OrgHeader BookingPartyOrg
		{
			get { return bookingPartyOrg ?? (bookingPartyOrg = GetOrgWithCommunicationModes("BookingParty")); }
		}
		OrgHeader bookingPartyOrg;

		protected OrgHeader DestinationDepotOrg
		{
			get
			{
				return destinationDepotOrg ?? (destinationDepotOrg = GetOrgWithCommunicationModes("DestinationDepot"));
			}
		}
		OrgHeader destinationDepotOrg;

		protected OrgHeader ImportReleaseDepotOrg
		{
			get { return importReleaseDepotOrg ?? (importReleaseDepotOrg = GetOrgWithCommunicationModes("ImportReleaseDepotOrg")); }
		}
		OrgHeader importReleaseDepotOrg;

		protected OrgHeader ExportReceivingDepotOrg
		{
			get
			{
				return exportReceivingDepotOrg ?? (exportReceivingDepotOrg = GetOrgWithCommunicationModes("ExportReceivingDepotOrg"));
			}
		}
		OrgHeader exportReceivingDepotOrg;

		protected OrgHeader PickupAgentOrg
		{
			get { return pickupAgentOrg ?? (pickupAgentOrg = GetOrgWithCommunicationModes("PickupAgent")); }
		}
		OrgHeader pickupAgentOrg;

		protected OrgHeader DeliveryAgentOrg
		{
			get { return deliveryAgentOrg ?? (deliveryAgentOrg = GetOrgWithCommunicationModes("DeliveryAgent")); }
		}
		OrgHeader deliveryAgentOrg;

		protected OrgHeader ExternalBrokerOrg
		{
			get { return externalBrokerOrg ?? (externalBrokerOrg = GetOrgWithCommunicationModes("ExternalBroker")); }
		}
		OrgHeader externalBrokerOrg;

		#endregion

		bool CheckIfXmlDeliveryTestRequired()
		{
			bool result = SupportsXmlDeliveryActions;
			if (!result)
			{
				Assert("This workflow does not support Xml Message Delivery, this test is not needed", true);
			}
			return result;
		}

		bool CheckIfXmlDeliveryWithFallbackTestRequired()
		{
			bool result = CheckIfXmlDeliveryTestRequired();
			if (result)
			{
				result = WorkflowDescriptor.GetWorkflowTriggerActionTypes().ContainsCode(WorkflowTriggerActionTypeConstants.Codes.SendXMLWithJobFallback);
				if (!result)
				{
					Assert("This workflow does not support Xml Message Delivery with fallback, this test is not needed", true);
				}
			}
			return result;
		}

		bool SupportsXmlDeliveryActions
		{
			get
			{
				var actionTypes = WorkflowDescriptor.GetWorkflowTriggerActionTypes();
				return
					actionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.SendXML) ||
					actionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.SendXMLWithJobFallback);
			}
		}

		#endregion

		public void TestValidDocumentActionTypes()
		{
			var actionTypes = WorkflowDescriptor.GetWorkflowTriggerActionTypes();
			var hasDocumentActionTypes =
				actionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.SendDocument) ||
				actionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.AddDocumentToEDocs);

			if (hasDocumentActionTypes)
			{
				var providers = GetParentsWithConfiguredOrganisationPartiesForTest();
				Assert(
				"WorkflowDescriptor supports Documents.\r\n" +
					"GetTestParentsWithConfiguredOrganisationParties() must be implemented to return at least 1 IWorkflowProvider",
					providers.Length > 0);

				foreach (var provider in providers)
				{
					var docSupportable = provider as IDocumentSupportable;
					AssertNotNull("To be able to produce documents, WorkflowProvider has to be IDocumentSupportable. Override WorkflowTriggerActionTypes to exclude Document action types.", docSupportable);
					AssertEquals("Specify a valid BusinessContext for the document", docSupportable.DocumentSupporter.BusinessContext, WorkflowDescriptor.DocumentBusinessContext[0]);
				}
			}
			else
			{
				Assert("This workflow does not support Documents, this test is not needed", !hasDocumentActionTypes);
			}
		}

		public void TestAreTasksCompanySpecific()
		{
			AssertEquals(ExpectingTasksToBeCompanySpecific, WorkflowDescriptor.AreTasksCompanySpecific);
		}

		public void TestCondition1List()
		{
			var task = Factory.New<ProcessTask>();
			var list = WorkflowDescriptor.GetConditionList1(task);
			foreach (CodeDescriptionPair pair in new EventReferenceConditionList())
			{
				var errorMessage = string.Format("Should not contain '{0}' as this is used in base", pair.Code);
				Assert(errorMessage, !list.ContainsCode(pair.Code));
			}
		}

		public virtual void TestWorkflowProviderType()
		{
			var provider = (IWorkflowProvider)Factory.New(WorkflowDescriptor.WorkflowProviderType);
			Assert(WorkflowDescriptor.WorkflowProviderType.IsAssignableFrom(provider.GetType()));
			AssertEquals(WorkflowDescriptor.Code, provider.WorkflowType);
		}

		public void TestWorkflowProviderTypeWithTemplatesHasFullAuditContext()
		{
			if (WorkflowDescriptor.SupportsWorkflowTemplates)
			{
				Assert("Workflow Desscriptors that support templates should have AuditContext", typeof(IAuditDetailsWithContext).IsAssignableFrom(WorkflowDescriptor.WorkflowProviderType));
			}
			else
			{
				Assert(true);
			}
		}

		public void TestRootTypeProvider_RootTypesForTaskLineTrigger_ShouldReturnSameAsTaskSubclass()
		{
			if (WorkflowDescriptor.SupportsTaskLineTriggers)
			{
				var job = GetParentsWithConfiguredOrganisationPartiesForTest().First();
				var task = job.WorkflowItems.Tasks.AddNew();
				var trigger = job.WorkflowItems.Triggers.AddNew();

				trigger.P9_LineTriggerType = ProcessTasksLookups.TaskLineTriggerCode;
				var roots = ((IRootTypeProvider)trigger).RootTypes;
				AssertSequencesEqual(new[] { typeof(ProcessTask), task.GetType(), trigger.GetJob().GetType() }, roots);
			}
			else
			{
				Assert(true);
			}
		}

		public void TestPersistentWorkflowProviderType()
		{
			Assert("This property must never return a type which is non persistent. If this test fails, you'll need to override WorkflowProviderTypeForNonPersistentBusinessObjects to return something persistent.", !typeof(NonPersistentBusinessObject).IsAssignableFrom(WorkflowDescriptor.WorkflowProviderTypeForNonPersistentBusinessObjects));
			AssertEquals(WorkflowProviderTypeForNonPersistentBusinessObjects, WorkflowDescriptor.WorkflowProviderTypeForNonPersistentBusinessObjects);
		}

		protected virtual Type WorkflowProviderTypeForNonPersistentBusinessObjects => WorkflowDescriptor.WorkflowProviderType;

		public void TestWorkflowDescriptors_TryGetValueSafe()
		{
			var descriptor = WorkflowDescriptors.Instance.TryGetValueSafe(WorkflowDescriptor.Code);
			if (descriptor != null)
			{
				AssertEquals(WorkflowDescriptor.GetType(), descriptor.GetType());
			}
			else
			{
				Assert(true);
			}
		}

		protected virtual bool ExpectingTasksToBeCompanySpecific
		{
			get { return true; }
		}

		public void TestPropertiesThatAffectWorkflowProvider()
		{
			var arr1 = WorkflowDescriptor.FormCustomisationSettings != null ? WorkflowDescriptor.FormCustomisationSettings.PropertiesThatAffectWorkflow : Array.Empty<string>();
			var arr2 = new WorkflowDescriptor.PropertiesThatAffectWorkflowProvider().GetPropertiesThatAffectWorkflow(WorkflowDescriptor.Code);
			AssertEquals(arr1.Length, arr2.Length);
			var list2 = new List<string>(arr2);
			foreach (var item in arr1)
			{
				Assert(list2.Contains(item));
			}
		}

		public void TestGetCommunicationModesForRecipient_ThrowsWithNullRecipient()
		{
			var workFlowDescriptor = new DummyEnterpriseBusinessObjectWorkflowDescriptor();
			AssertExceptionThrown<ArgumentNullException>("ArgumentNullException should be thrown if GetCommunicationModesForRecipient is called with null recipient",
				() => workFlowDescriptor.GetCommunicationModesForRecipient(null, new EDICommunicationModeQuery(null, null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty)));
		}

		public void TestCommonPropertyBetweenBusinessObjectAndEventDataModelNotAllowed()
		{
			var processTaskNotification = GetProcessTaskNotificationWithRealParent();
			var task = processTaskNotification.Item1.Parent;
			var businessObject = task.GetJob();

			if (businessObject == null)
			{
				Assert(true);
				return;
			}

			var descriptor = new T();
			var eventDataModel = descriptor.GetEventDataModel(businessObject);

			if (eventDataModel.GetType() == typeof(BusinessObjectEventDataModel))
			{
				Assert(true);
				return;
			}

			var commonProperties = businessObject.GetType().GetProperties().Select(p => p.Name)
				.Intersect(eventDataModel.GetType().GetProperties().Select(p => p.Name))
				.ToList();

			if (commonProperties.Count == 0)
			{
				Assert(true);
				return;
			}

			var whiteList = WorkflowDescriptorCommonPropertyWhiteListHelper.WhiteList;
			var collisions = commonProperties
				.Where(item => !whiteList.Any(kvp => kvp.Key == eventDataModel.GetType().Name && kvp.Value == item))
				.ToList();

			if (collisions.Count == 0)
			{
				Assert(true);
			}
			else
			{
				Fail($"Using the same property in both the BusinessObject and BusinessObjectEventDataModel classes is not permitted. Common property/properties include: {string.Join(", ", collisions)}");
			}
		}

		#region Implementation

		protected virtual string TestingCountry => null;

		protected override void SetUp()
		{
			base.SetUp();
			ObjectFactory.Get<IBMSRegistry>().AlwaysViewWorkflowManagementTab = true;

			if (!string.IsNullOrEmpty(TestingCountry))
			{
				StoredCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
				GlbCompany.CurrentCompany.SetCountry(TestingCountry);
			}
			OutboundAdapterServiceUrlTemporaryValue = eAdaptorRegistry.Instance.OutboundAdapterServiceUrl.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "http://ftp://https://edinet://");
		}
		IDisposable OutboundAdapterServiceUrlTemporaryValue;
		protected string StoredCountry;

		protected override void TearDown()
		{
			if (!string.IsNullOrEmpty(StoredCountry) && StoredCountry != GlbCompany.CurrentCompany.GC_RN_NKCountryCode)
			{
				GlbCompany.CurrentCompany.SetCountry(StoredCountry);
			}

			GlbCompany.CurrentCompany.OrgProxy.EDICommunicationsModes.RemoveAndDeleteAll();

			OutboundAdapterServiceUrlTemporaryValue.Dispose();
			base.TearDown();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new T();
		}

		protected WorkflowDescriptor WorkflowDescriptor
		{
			get { return workflowDescriptor ?? (workflowDescriptor = new T()); }
		}
		WorkflowDescriptor workflowDescriptor;

		StmChangeLog GetChangeLogForField(SchemaColumn column)
		{
			BusinessObject bo = NewBusinessObjectInTable(column.TableSchema);

			StmChangeLog log = Factory.New<StmChangeLog>();
			log.SY_ParentID = bo.PK;
			log.SY_ParentTableCode = bo.TablePrefix;
			log.SY_Changes = column.Name + "||";
			return log;
		}

		protected virtual BusinessObject NewBusinessObjectInTable(ITableSchema table)
		{
			return Factory.New(BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(Schema.GetPrefixFromColumnName(table.PK.Name)));
		}

		#endregion
	}
}
