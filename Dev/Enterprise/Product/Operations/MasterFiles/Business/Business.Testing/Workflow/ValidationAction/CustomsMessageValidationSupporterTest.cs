using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.MasterFiles.Business.Workflow.ValidationAction.Testing
{
	public abstract class CustomsMessageValidationSupporterTest<T> : TestCaseWithFactory where T : BusinessObject, IWorkflowProvider, IValidateForCustomsMessagingSupporter
	{
		public void TestSupportValidateForCustomsMessaging()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCode))
			{
				var parent = GetValidateForCustomsMessagingSupporter();
				Factory.Save();
				var trigger = parent.WorkflowItems.Triggers.AddNew();
				trigger.P9_Description = "Validate for Customs Messaging";
				trigger.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;
				trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;

				foreach (var code in trigger.WorkflowDescriptor.GetSupportsValidateForCustomsMessagingTriggerActions(trigger, parent))
				{
					var triggerAction = trigger.ProcessTaskNotifications.AddNew();
					triggerAction.PQ_TriggerType = code;
					triggerAction.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
					triggerAction.PQ_EmailAddr = "someone@therightplace.com";

					parent.GetLogs().AddNew(Events.Authorised);

					var workflowProcessor = trigger.WorkflowDescriptor.GetWorkflowTriggerAction(triggerAction, new QueuedLogForTesting(Factory));
					workflowProcessor.Process(new NotificationBuffer());

					Assert(parent.GetEntityToValidate(code).GetLogs().HasLogWith((x) => x.SL_SE_NKEvent == Events.MessageValidationFailedCode || x.SL_SE_NKEvent == Events.MessageValidationPassedCode));
				}
			}
		}

		public void TestTriggerTypes()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCode))
			{
				var parent = GetValidateForCustomsMessagingSupporter();
				var trigger = parent.WorkflowItems.Triggers.AddNew();
				var actualTriggerTypes = trigger.WorkflowDescriptor.GetSupportsValidateForCustomsMessagingTriggerActions(trigger, parent);
				AssertContainsExactElementsInAnyOrder(ExpectedTriggerTypes, actualTriggerTypes);
			}
		}

		public virtual void TestMessageValidationWithCreditCheck()
		{
			using (Globals.SetIsUserInteractiveForTest(false))
			using (FreightDataRegistry.Instance.FreightEnableComplianceWise.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty,
				ComplianceWiseRegistryHelper.SetValue(false)))
			using (RawDataRegistry.Instance.EnableComplianceRisk.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (OrganisationsDataRegistry.Instance.DPSFreightMovementRestrictions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DPSFreightMovementRestrictionsOptions.Codes.All))
			{
				var shipment = Factory.New<IForwardingShipment>();
				shipment.JS_ScreeningStatus = "MAT";

				var declaration = Factory.New<IBaseJobDeclaration>();
				declaration.JE_ScreeningStatus = "CLR";
				declaration.JE_JS = shipment.PK;

				if (declaration is IValidateForCustomsMessagingSupporter supporter && supporter.SupportValidateCustomsMessaging)
				{
					var workflowProvider = declaration as IWorkflowProvider;
					var trigger = workflowProvider.WorkflowItems.Triggers.AddNew();
					trigger.P9_Description = "Validate for Customs Messaging";
					trigger.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;
					trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;

					var triggerAction = trigger.ProcessTaskNotifications.AddNew();
					triggerAction.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.ValidateForCustomsMessaging;
					triggerAction.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
					triggerAction.PQ_EmailAddr = "someone@therightplace.com";

					var logs = workflowProvider.Logs;
					logs.AddNew(Events.Authorised);
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
					AssertContains("Screening Status is not Clear.", declaration.GetCreditCheckMessage());

					var validationLogQuery = new ZQuery(StmALogSchema.SL_Parent, declaration.PK);
					validationLogQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.MessageValidationFailedCode);
					var validationLogs = Factory.Load<StmALog>(validationLogQuery);
					AssertEquals("Should be an MVF log against the declaration now the trigger has fired.", 1, validationLogs.Length);

					var creditCheckLogQuery = new ZQuery(StmALogSchema.SL_Parent, declaration.PK);
					creditCheckLogQuery.AddToFilter(StmALogSchema.SL_SE_NKEvent, Events.CreditCheckFailedCode);
					var creditCheckLogs = Factory.Load<StmALog>(creditCheckLogQuery);
					AssertEquals("Should be an CTF log against the declaration now the trigger has fired.", 1, creditCheckLogs.Length);
				}
				Assert(true);
			}
		}

		protected virtual T GetValidateForCustomsMessagingSupporter()
		{
			return Factory.New<T>();
		}

		protected abstract IEnumerable<string> ExpectedTriggerTypes { get; }

		protected virtual ZString CountryCode { get { return GlbCompany.CurrentCompany.GC_RN_NKCountryCode; } }
	}
}
