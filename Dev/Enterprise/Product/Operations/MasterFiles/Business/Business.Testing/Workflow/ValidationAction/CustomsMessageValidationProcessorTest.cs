using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Workflow.ValidationAction.Testing
{
	sealed class CustomsMessageValidationProcessorTest : TestCaseWithFactory
	{
		public void TestMessageValidationWithOutturnReconciliation()
		{
			var cusMAWB = (BusinessObject)Factory.New<Enterprise.Integration.Customs.AU.ICusMAWB>();
			var logs = cusMAWB.GetLogs();
			var trigger = ((IWorkflowProvider)cusMAWB).WorkflowItems.Triggers.AddNew();
			var triggerAction = trigger.ProcessTaskNotifications.AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ReconcileOutturn;

			var processor = new CustomsMessageValidationProcessor(triggerAction, cusMAWB);
			processor.Process(new NotificationBuffer());
			AssertEquals("Outturn Reconciliation is triggered when TriggerType = ROT", true, HasOutturnReconciliationEvents());
			AssertEquals("Message Validation is not triggered when TriggerTYpe = ROT", false, HasMessageValidationEvents());

			logs.RemoveAndDeleteAll();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ValidateForCustomsMessaging;
			processor.Process(new NotificationBuffer());
			AssertEquals("Outturn Reconciliation is not triggered when TriggerType <> ROT", false, HasOutturnReconciliationEvents());
			AssertEquals("Message Validation is triggered when TriggerTYpe <> ROT", true, HasMessageValidationEvents());

			bool HasOutturnReconciliationEvents()
			{
				return logs.HasLogWith((x) => x.SL_SE_NKEvent == Events.ReconcileOutturnPassedCode || x.SL_SE_NKEvent == Events.ReconcileOutturnFailedCode);
			}

			bool HasMessageValidationEvents()
			{
				return logs.HasLogWith((x) => x.SL_SE_NKEvent == Events.MessageValidationPassedCode || x.SL_SE_NKEvent == Events.MessageValidationFailedCode);
			}
		}

		public void TestMessageValidationWithInvalidData()
		{
			var dummyBO = Factory.New<DummyWithWorkflow>();
			dummyBO.ValidationForTesting = new DummyCustomsMessageValidation(dummyBO);
			dummyBO.Z0_Code = "";
			dummyBO.Z0_Description = "D00001000";

			var dummyChildBO = Factory.New<DummyDependantBusinessObject>();
			dummyChildBO.ZD1_Z0 = dummyBO.PK;
			dummyChildBO.ZD1_Code = "error";
			dummyBO.RegisterEditableChildObject(dummyChildBO);

			dummyBO.Validation.ValidateAll();

			var trigger = dummyBO.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Validate for Customs Messaging";
			trigger.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;
			trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;

			var triggerAction = trigger.ProcessTaskNotifications.AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ValidateForCustomsMessaging;
			triggerAction.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			triggerAction.PQ_EmailAddr = "someone@therightplace.com";

			dummyBO.Logs.AddNew(Events.Authorised);

			Factory.Save();

			var query = new ZQuery(StmALogSchema.SL_Parent, trigger.PK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);
			var triggerWTELogs = Factory.Load<StmALog>(query);
			AssertEquals("Should be a WTE log against the trigger now so WorkFlow will fire.", 1, triggerWTELogs.Length);
			var triggerWTELog = triggerWTELogs[0];

			var workflowProcessor = DummyWorkflowDescriptor.Instance.GetWorkflowTriggerAction(triggerAction, new QueuedLogForTesting(triggerWTELog, trigger));
			AssertNotNull("workFlowDescriptor.GetWorkflowTriggerAction()", workflowProcessor);

			var notifications = new NotificationBuffer();
			workflowProcessor.Process(notifications);
			AssertEquals("Service Task Log", "", notifications.AsString);

			var validationLogQuery = new ZQuery(StmALogSchema.SL_Parent, dummyBO.PK);
			validationLogQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.MessageValidationFailedCode);
			var validationLogs = Factory.Load<StmALog>(validationLogQuery);
			AssertEquals("Should be an MVF log against the DummyBO now the trigger has fired.", 1, validationLogs.Length);

			AssertEquals("Emails created during processing", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			CombineAssertions(delegate
			{
				AssertEquals("email.Subject", "Validation Failure on Dummy Business Object D00001000.", email.Subject);
				AssertEquals("email.Recipients", "someone@therightplace.com", email.Recipients.RecipientsAsDelimitedString());
				AssertContains("email.Body", "DummyDependentBizo\tError - Code: Error", email.Body);
				AssertContains("email.Body", "Error - N Var Char: You must have at least 3 characters of description.", email.Body);
				AssertContains("email.Body", "Message Error - N Var Char Max: You should have at least 10 characters of explanatory notes.", email.Body);
				AssertNotContains("email.Body", "Warning - Code: You have not entered a Code.", email.Body);
			});
		}

		public void TestMessageValidationWithValidData()
		{
			var dummyBO = Factory.New<DummyWithWorkflow>();
			dummyBO.ValidationForTesting = new DummyCustomsMessageValidation(dummyBO);

			dummyBO.Z0_Description = "D00001000";
			dummyBO.Z0_Code = "123";
			dummyBO.Z0_NVarChar = "Over 3 characters.";
			dummyBO.Z0_NVarCharMax = "WAY over 5 characters. It's all good!";

			var trigger = dummyBO.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Validate for Customs Messaging";
			trigger.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;
			trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;

			var triggerAction = trigger.ProcessTaskNotifications.AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ValidateForCustomsMessaging;
			triggerAction.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			triggerAction.PQ_EmailAddr = "someone@therightplace.com";

			dummyBO.Logs.AddNew(Events.Authorised);

			Factory.Save();

			var query = new ZQuery(StmALogSchema.SL_Parent, trigger.PK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);
			var triggerWTELogs = Factory.Load<StmALog>(query);
			AssertEquals("Should be a WTE log against the trigger now so WorkFlow will fire.", 1, triggerWTELogs.Length);
			var triggerWTELog = triggerWTELogs[0];

			var workflowProcessor = DummyWorkflowDescriptor.Instance.GetWorkflowTriggerAction(triggerAction, new QueuedLogForTesting(triggerWTELog, trigger));
			AssertNotNull("workFlowDescriptor.GetWorkflowTriggerAction()", workflowProcessor);

			var notifications = new NotificationBuffer();
			workflowProcessor.Process(notifications);
			AssertEquals("Service Task Log", "", notifications.AsString);

			ZQuery validationLogQuery = new ZQuery(StmALogSchema.SL_Parent, dummyBO.PK);
			validationLogQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.MessageValidationPassedCode);
			var validationLogs = Factory.Load<StmALog>(validationLogQuery);
			AssertEquals("Should be an MVP log against the DummyBO now the trigger has fired.", 1, validationLogs.Length);

			AssertEquals("No email created during processing", "", string.Join(", ", Env.OutgoingMailManager.EmailsCreated.Select(o => "To: [" + o.Recipients.RecipientsAsDelimitedString() + "] Subject: [" + o.Subject + "]\r\nBody: [" + o.Body + "]")));
		}

		public void TestMessageValidationWithAdditionalValidation()
		{
			var dummyBO = Factory.New<DummyWithWorkflow>();
			dummyBO.SetAdditionalValidationErrorMessages(["Test Additional Validation Message"]);
			dummyBO.ValidationForTesting = new DummyCustomsMessageValidation(dummyBO);

			dummyBO.Z0_Description = "D00001000";
			dummyBO.Z0_Code = "123";
			dummyBO.Z0_NVarChar = "Over 3 characters.";
			dummyBO.Z0_NVarCharMax = "WAY over 5 characters. It's all good!";

			var trigger = dummyBO.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Validate for Customs Messaging";
			trigger.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;
			trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;

			var triggerAction = trigger.ProcessTaskNotifications.AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ValidateForCustomsMessaging;
			triggerAction.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			triggerAction.PQ_EmailAddr = "someone@therightplace.com";

			dummyBO.Logs.AddNew(Events.Authorised);

			Factory.Save();

			var query = new ZQuery(StmALogSchema.SL_Parent, trigger.PK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);
			var triggerWTELogs = Factory.Load<StmALog>(query);
			AssertEquals("Should be a WTE log against the trigger now so WorkFlow will fire.", 1, triggerWTELogs.Length);
			var triggerWTELog = triggerWTELogs[0];

			var workflowProcessor = DummyWorkflowDescriptor.Instance.GetWorkflowTriggerAction(triggerAction, new QueuedLogForTesting(triggerWTELog, trigger));
			AssertNotNull("workFlowDescriptor.GetWorkflowTriggerAction()", workflowProcessor);

			var notifications = new NotificationBuffer();
			workflowProcessor.Process(notifications);
			AssertEquals("Service Task Log", "", notifications.AsString);

			var validationLogQuery = new ZQuery(StmALogSchema.SL_Parent, dummyBO.PK);
			validationLogQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.MessageValidationFailedCode);
			var validationLogs = Factory.Load<StmALog>(validationLogQuery);
			AssertEquals("Should be an MVF log against the DummyBO now the trigger has fired.", 1, validationLogs.Length);

			AssertEquals("Emails created during processing", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			CombineAssertions(delegate
			{
				AssertEquals("email.Subject", "Validation Failure on Dummy Business Object D00001000.", email.Subject);
				AssertContains("email.Body", "Test Additional Validation Message", email.Body);
			});
		}

		public void TestMessageValidationLoadsChildren()
		{
			var cusMAWB = Factory.New<Enterprise.Integration.Customs.AU.ICusMAWB>();

			cusMAWB.CM_ArrivalDate = ZDateTime.Today;
			cusMAWB.CM_FlightNo = "QF123";
			cusMAWB.CM_MAWB = "08199999992";
			cusMAWB.CM_RL_NKLoadPort = "USCHI";
			cusMAWB.CM_RL_NKDischargePort = "AUSYD";

			var trigger = ((IWorkflowProvider)cusMAWB).WorkflowItems.Triggers.AddNew();
			trigger.P9_ParentID = cusMAWB.PK;
			trigger.P9_ParentTableCode = CusMAWBSchema.Constants.Prefix;
			trigger.P9_Description = "Validate for Customs Messaging";
			trigger.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;
			trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;

			var triggerAction = trigger.ProcessTaskNotifications.AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ValidateForCustomsMessaging;
			triggerAction.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			triggerAction.PQ_EmailAddr = "someone@therightplace.com";

			var athLog = Factory.New<StmALog>();
			using (athLog.LockForUpdatingKeyFieldsForTesting())
			{
				athLog.SL_Parent = cusMAWB.PK;
				athLog.SL_Table = CusMAWBSchema.Constants.TableName;
				athLog.SL_SE_NKEvent = Events.AuthorisedCode;
				athLog.SL_EventTime = ZDateTime.Now;
			}

			cusMAWB.RunPreSaveValidation();
			AssertEquals(0, cusMAWB.Notifications.Count());
			Factory.Save(); // Now CM_IsValid is 'Y' (cannot be asserted, but believe me) and full validation won't run next time

			var cusHAWB = Factory.New<Enterprise.Integration.Customs.AU.ICusHAWB>();
			cusHAWB.CS_CM = cusMAWB.PK;
			Factory.Save(); // Now CusMAWB has a CusHAWB with lots of message errors

			var newFactory = new BusinessObjectFactory();
			var triggerCopy = newFactory.Load<ProcessTask>(trigger.PK);
			var triggerActionCopy = newFactory.Load<ProcessTaskNotification>(triggerAction.PK);

			var query = new ZQuery(StmALogSchema.SL_Parent, triggerCopy.PK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);
			var triggerWTELogs = newFactory.Load<StmALog>(query);
			AssertEquals("Should be a WTE log against the trigger now so WorkFlow will fire.", 1, triggerWTELogs.Length);
			var triggerWTELog = triggerWTELogs[0];

			var workflowDescriptor = WorkflowDescriptors.Instance.TryGetValueSafe("ACR");
			var workflowProcessor = workflowDescriptor.GetWorkflowTriggerAction(triggerActionCopy, new QueuedLogForTesting(triggerWTELog, triggerCopy));
			AssertNotNull("workFlowDescriptor.GetWorkflowTriggerAction()", workflowProcessor);

			var notifications = new NotificationBuffer();
			workflowProcessor.Process(notifications);
			AssertEquals("Service Task Log", "", notifications.AsString);

			var validationLogQuery = new ZQuery(StmALogSchema.SL_Parent, cusMAWB.PK);
			validationLogQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.MessageValidationFailedCode);
			var validationLogs = newFactory.Load<StmALog>(validationLogQuery);
			AssertEquals("Should be an MVF log against CusMAWB now the trigger has fired.", 1, validationLogs.Length);

			AssertEquals("Emails created during processing", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			CombineAssertions(delegate
			{
				AssertEquals("email.Subject", "Validation Failure on AirCargo Report (MAWB: 081-99999992).", email.Subject);
				AssertEquals("email.Recipients", "someone@therightplace.com", email.Recipients.RecipientsAsDelimitedString());
				AssertContains("email.Body", "Message Error - HAWB: Housebill number is required for air cargo messaging.", email.Body);
				AssertContains("email.Body", "Message Error - Weight: Weight is required for air cargo messaging.", email.Body);
			});
		}

		public void TestCustomsMessageHasNoInvalidFactories()
		{
			//Setup
			var dummyBO = Factory.New<DummyWithWorkflow>();
			dummyBO.ValidationForTesting = new DummyCustomsMessageValidation(dummyBO);
			dummyBO.Z0_Code = "";
			dummyBO.Z0_Description = "D00001000";

			var dummyChildBO = Factory.New<DummyDependantBusinessObject>();
			dummyChildBO.ZD1_Z0 = dummyBO.PK;
			dummyChildBO.ZD1_Code = "error";
			dummyBO.RegisterEditableChildObject(dummyChildBO);

			dummyBO.Validation.ValidateAll();

			var trigger = dummyBO.WorkflowItems.Triggers.AddNew();
			trigger.P9_Description = "Validate for Customs Messaging";
			trigger.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;
			trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;

			var triggerAction = trigger.ProcessTaskNotifications.AddNew();
			triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ValidateForCustomsMessaging;
			triggerAction.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			triggerAction.PQ_EmailAddr = "someone@therightplace.com";

			dummyBO.Logs.AddNew(Events.Authorised);

			Factory.Save();

			var query = new ZQuery(StmALogSchema.SL_Parent, trigger.PK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);
			var triggerWTELogs = Factory.Load<StmALog>(query);
			AssertEquals("Should be a WTE log against the trigger now so WorkFlow will fire.", 1, triggerWTELogs.Length);
			var triggerWTELog = triggerWTELogs[0];

			var workflowProcessor = DummyWorkflowDescriptor.Instance.GetWorkflowTriggerAction(triggerAction, new QueuedLogForTesting(triggerWTELog, trigger));
			AssertNotNull("workFlowDescriptor.GetWorkflowTriggerAction()", workflowProcessor);

			var notifications = new NotificationBuffer();

			int factoryCount = 0;
			BusinessObjectFactory.SetOnFactorySaveHookForTest((thefactory) => factoryCount++);

			//Act
			workflowProcessor.Process(notifications);

			//Assert
			AssertEquals("No Factory Saves", 0, factoryCount);
		}

		#region Implementation

		class DummyCustomsMessageValidation : DummyBizoValidation
		{
			internal DummyCustomsMessageValidation(DummyWithWorkflow parent)
				: base(parent)
			{
			}

			protected override void CheckZ0_Code()
			{
				MandatoryValidation.WarnIfNotEntered(Parent.Z0_CodeInfo);

				base.CheckZ0_Code();
			}

			protected override void CheckZ0_NVarChar()
			{
				if (Parent.Z0_NVarChar.Length < 3)
				{
					Parent.Z0_NVarCharInfo.AddError("You must have at least 3 characters of description.");
				}

				base.CheckZ0_NVarChar();
			}

			protected override void CheckZ0_NVarCharMax()
			{
				if (Parent.Z0_NVarCharMax.Length < 10)
				{
					Parent.Z0_NVarCharMaxInfo.AddMessageError("You should have at least 10 characters of explanatory notes.");
				}

				base.CheckZ0_NVarCharMax();
			}
		}

		#endregion
	}
}
