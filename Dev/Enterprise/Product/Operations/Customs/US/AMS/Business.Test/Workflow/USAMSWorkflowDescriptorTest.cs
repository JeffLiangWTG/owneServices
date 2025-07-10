using System.Collections.Generic;
using CargoWise.Definitions;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Business.Workflow.ValidationAction.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.AMS.Business.Testing
{
	[TestedType(typeof(USAMSWorkflowDescriptor))]
	class CusInBondHeaderWorkflowDescriptorTest : WorkflowDescriptorTestCase<USAMSWorkflowDescriptor>
	{
		#region Overrides of WorkflowDescriptorTestCase<eManifestWorkflowDescriptor>

		public override void TestSupportsEventTracking()
		{
			AssertEquals("SupportsEventTracking", true, WorkflowDescriptor.SupportsEventTracking);
		}

		public override void TestID()
		{
			AssertEquals("Code", WorkflowDescriptors.USAMSWorkflowDescriptorCode, WorkflowDescriptor.Code);
		}

		public override void TestDescription()
		{
			AssertEquals("Description", "US AMS", WorkflowDescriptor.Description);
		}

		public override void TestSubTypes()
		{
			AssertEquals("0 sub type", 0, WorkflowDescriptor.SubTypeInformation.Length);
		}

		public override void TestRequiresPorts()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresPort1);
			AssertEquals(true, WorkflowDescriptor.RequiresPort2);
		}

		public override void TestRequiresClient()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresClient);
		}

		public override void TestRequiresBranch()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresBranch);
		}

		public override void TestRequiresDepartment()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresDepartment);
		}

		public void TestDocumentBusinessContext()
		{
			AssertEquals(BusinessContext.CusInBondHeader, WorkflowDescriptor.DocumentBusinessContext[0]);
		}

		public void TestControllerID()
		{
			AssertEquals(ControllerIDs.Customs.US.AMS, WorkflowDescriptor.ControllerID);
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get { return MessageRecipientPartyType.Email | MessageRecipientPartyType.OrgProxy; }
		}

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			return new IWorkflowProvider[] { Factory.NewWithValidTestData<CusInBondHeader>() };
		}

		public void TestGetWorkflowTriggerActionCore()
		{
			var descriptor = new USAMSWorkflowDescriptor();

			var header = Factory.New<CusInBondHeader>();
			var processTask = header.WorkflowItems.Triggers.AddNew();
			var action = Factory.NewWithValidTestData<ProcessTaskNotification>();

			processTask.ProcessTaskNotifications.Add(action);
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendManifestMessage;
			var resultProcessor = descriptor.GetWorkflowTriggerAction(action, new QueuedLogForTesting(Factory));

			AssertEquals("Processor for Send Manifest message should be CustomsStmProcessQueueCreatorProcessor", "CustomsStmProcessQueueCreatorProcessor", resultProcessor.GetType().Name);
		}

		protected override CodeDescriptionPair[] ExpectedAdditionalWorkflowTriggerActionTypes
		{
			get
			{
				return new CodeDescriptionPair[] { new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.SendManifestMessage, WorkflowTriggerActionTypeConstants.Descriptions.SendManifestMessage) };
			}
		}

		#endregion
	}

	class CusInBondHeaderCustomsMessageValidationSupporterTest : CustomsMessageValidationSupporterTest<CusInBondHeader>
	{
		protected override IEnumerable<string> ExpectedTriggerTypes
		{
			get { yield return WorkflowTriggerActionTypeConstants.Codes.ValidateForCustomsMessaging; }
		}
	}
}
