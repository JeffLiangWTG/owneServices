using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Registry.Business.WorkflowManager;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.DataTransfer;
using Enterprise.Warehouse.Transactions.DataTransfer.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public abstract class WhsDocketWorkflowDescriptorTest<DescriptorObject> : WorkflowDescriptorTestCase<DescriptorObject>
		where DescriptorObject : WorkflowDescriptor, new()
	{
		#region Overrides

		protected override bool ExpectingTasksToBeCompanySpecific => false;

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
			=> MessageRecipientPartyType.Client
			| MessageRecipientPartyType.OrgProxy
			| MessageRecipientPartyType.Email
			| MessageRecipientPartyType.Warehouse;

		#endregion

		#region TestID

		public override void TestID()
		{
			AssertEquals("Correct Code", GetExpectedCode(), WorkflowDescriptor.Code);
		}

		#endregion

		#region TestDescription

		public override void TestDescription()
		{
			AssertEquals("Correct Desc", GetExpectedDescription(), WorkflowDescriptor.Description);
		}
		#endregion

		#region TestRequiresClient

		public override void TestRequiresClient()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresClient);
		}

		#endregion

		#region TestRequiresWarehouse

		protected override bool RequiresWarehouseExpectedResult => true;

		#endregion

		#region TestSupportsEventTracking

		public override void TestSupportsEventTracking()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsEventTracking);
		}

		#endregion

		#region TestRequiresBranch

		public override void TestRequiresBranch()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresBranch);
		}

		#endregion

		#region TestRequiresDepartment

		public override void TestRequiresDepartment()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresDepartment);
		}

		#endregion

		#region TestRequiresPorts

		public override void TestRequiresPorts()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresPort1);
			AssertEquals(false, WorkflowDescriptor.RequiresPort2);
		}

		#endregion

		#region TestSubTypes

		public override void TestSubTypes()
		{
			AssertEquals("No sub type", 0, WorkflowDescriptor.SubTypeInformation.Length);
		}

		#endregion

		#region PropertiesThatAffectWorkflow

		public void TestPropertiesThatAffectWorkflow() => TestPropertiesThatAffectWorkflowCore();

		protected virtual void TestPropertiesThatAffectWorkflowCore()
		{
			AssertEquals(true, ((IList<string>)WorkflowDescriptor.FormCustomisationSettings.PropertiesThatAffectWorkflow).Contains(WhsDocketSchema.WD_OH_Client.Name));
		}

		#endregion

		#region Workflow Triggers

		public void TestWorkflowTriggerActionTypes_EnableTaskManagement()
		{
			using (WarehouseDataRegistry.Instance.ExposeWarehouseTaskManagement.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				TestWorkflowTriggerActionTypes();
			}
		}

		public void TestWorkflowTriggerActionCore_DocketSpecific()
		{
			var docket = GetNewDocket();
			var task = GetNewProcessTask(docket, ActionTypes.Codes.SetTaskPlanningStatusToReadyForPlanning);
			var processor = task.WorkflowDescriptor.GetWorkflowTriggerAction(docket.WorkflowItems[0].ProcessTaskNotifications[0], new QueuedLogForTesting(Factory));
			Assert(processor is SetTaskPlanningStatusToReadyForPlanningProcessor);
		}

		protected override CodeDescriptionPair[] ExpectedAdditionalWorkflowTriggerActionTypes
		{
			get
			{
				var result = new List<CodeDescriptionPair>();
				result.Add(new CodeDescriptionPair(ActionTypes.Codes.Confirmation, ActionTypes.Descriptions.Confirmation));
				result.AddRange(ExpectedAdditionalWorkflowTriggerActionTypesCore);
				return result.ToArray();
			}
		}

		protected virtual CodeDescriptionPair[] ExpectedAdditionalWorkflowTriggerActionTypesCore => Array.Empty<CodeDescriptionPair>();

		public override void TestIsMessagingOrEmailNotificationTriggerAction()
		{
			base.TestIsMessagingOrEmailNotificationTriggerAction();
			AssertEquals(true, WorkflowDescriptor.IsMessagingOrEmailNotificationTriggerAction(ActionTypes.Codes.Confirmation));
		}

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			return new IWorkflowProvider[]
			{
				Docket
			};
		}

		#endregion

		#region TestCanSendUniversalXMLToWarehouse

		protected override void SetupOrg(OrgHeader org)
		{
			base.SetupOrg(org);

			var communicationsMode = org.EDICommunicationsModes.AddNew();
			communicationsMode.EK_Module = GetExpectedCode();
			communicationsMode.EK_FileFormat = EDICommunicationsModeFileFormatList.Codes.XmlUniversalEvent;
			communicationsMode.EK_CommunicationsTransport = EDICommunicationsModeCommunicationsTransportList.Codes.EHubService;
			communicationsMode.EK_Destination = "9CHARCODE";
		}

		public void TestCanSendUniversalXMLToWarehouse()
		{
			if (!ShouldRunTestCanSendUniversalXMLToWarehouse())
			{
				Assert(true);
			}
			else
			{
				var docket = GetNewDocket();
				var trigger = docket.WorkflowItems.AddNew();
				trigger.P9_Type = Constants.Workflow.WorkflowTriggerType;
				trigger.TriggerConditions.TriggerEventCode = Events.AuthorisedCode;

				var action = trigger.ProcessTaskNotifications.AddNew();
				action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalEventXML;
				action.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Warehouse;
				action.PQ_MessagePurpose = ProcessTaskTriggerPurposeList.Codes.Event;

				var logBO = docket.GetLogs().AddNew(Events.Authorised, ZDateTimeOffset.UtcNow);
				Factory.Save();
				AssertNotEquals("Precondition: Trigger should now have an actual date / time recorded.", ZDateTime.Empty, trigger.P9_ActualDate);

				var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.WorkflowTriggerEventCode);
				query.AddToFilter(StmALogSchema.SL_Parent, trigger.PK);
				var triggerLogs = Factory.Load<StmALog>(query);
				AssertEquals("Precondition: WorkFlowTrigger Events linked to our Trigger", 1, triggerLogs.Length);

				var triggerLog = triggerLogs[0];
				var queuedLog = new QueuedLogForTesting(triggerLog, trigger);

				var processor = WorkflowDescriptor.GetWorkflowTriggerAction(action, queuedLog);

				var logger = new NotificationsForTesting();
				using (Factory.AddDisposableService())
				{
					processor.Process(logger);
					Factory.Save();
				}

				AssertMultilineASCIIEquals("loggger results from processor.Process()", "", logger.ToString());

				var messages = Factory.Load<EDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, docket.PK));
				AssertEquals("EDIMessages linked to Docket", 1, messages.Length);
				var message = messages[0];
				AssertContains("message.EM_MessageText", @"
      <RecipientRoleCollection>
        <RecipientRole>
          <Code>WHS</Code>
          <Description>Warehouse</Description>
        </RecipientRole>
      </RecipientRoleCollection>".Trim()
					, message.EM_MessageText);
			}
		}

		protected virtual bool ShouldRunTestCanSendUniversalXMLToWarehouse()
		{
			return true;
		}

		class NotificationsForTesting : INotifications
		{
			public void Add(INotification notification)
			{
				notifications.Add(notification.Message);
			}

			readonly List<string> notifications = new List<string>();

			public override string ToString()
			{
				return string.Join("\r\n", notifications.ToArray());
			}
		}

		#endregion

		#region Implementation

		protected virtual WhsDocketProcessTasks GetNewProcessTask(WhsDocket docket, ZString eDIActionType)
		{
			var task = docket.WorkflowItems.Triggers.AddNew();
			var notification = task.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = eDIActionType;
			return (WhsDocketProcessTasks)task;
		}

		protected abstract ZString GetExpectedCode();
		protected abstract ZString GetExpectedDescription();
		protected abstract WhsDocket GetNewDocket();

		WhsDocket docket;

		protected WhsDocket Docket => docket ?? (docket = GetNewDocket());

		protected WhsWarehouse Warehouse => warehouse ?? (warehouse = GetNewWarehouse());
		WhsWarehouse warehouse;

		protected WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;

		protected virtual WhsWarehouse GetNewWarehouse()
		{
			return Helper.CreateWarehouse("TST WHS", "ROW", 2, 2);
		}

		#endregion
	}

	[TestedType(typeof(WhsDocketWorkflowDescriptor.WhsDocketFormCustomisationSettingsProvider))]
	public class WhsDocketFormCustomisationSettingsProviderTest : FormCustomisationSettingsProviderTest<WhsDocketWorkflowDescriptor.WhsDocketFormCustomisationSettingsProvider>
	{
		public override void TestDisplayTabs()
		{
			var provider = GetNewProvider();
			AssertEquals(0, provider.DisplayTabs.Count);
		}

		public override void TestPropertiesThatAffectWorkflow()
		{
			var expected = new string[]
			{
				WhsDocketSchema.WD_OH_Client.Name
			};

			var provider = GetNewProvider();
			AssertContainsExactElementsInAnyOrder(expected, provider.PropertiesThatAffectWorkflow);
		}

		public override void TestTabPlacementProhibitions()
		{
			var provider = GetNewProvider();
			AssertEquals(0, provider.TabPlacementProhibitions.Length);
		}

		public override WhsDocketWorkflowDescriptor.WhsDocketFormCustomisationSettingsProvider GetNewProvider()
		{
			return new WhsDocketWorkflowDescriptor.WhsDocketFormCustomisationSettingsProvider();
		}
	}
}
