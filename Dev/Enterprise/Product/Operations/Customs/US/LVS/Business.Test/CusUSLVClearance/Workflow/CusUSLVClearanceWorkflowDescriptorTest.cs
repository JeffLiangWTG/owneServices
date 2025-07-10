using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
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

namespace Enterprise.Customs.US.LVS.Business.Testing
{
	[TestedType(typeof(CusUSLVClearanceWorkflowDescriptor))]
	public class CusUSLVClearanceWorkflowDescriptorTest : WorkflowDescriptorTestCase<CusUSLVClearanceWorkflowDescriptor>
	{
		public void TestCodeDescriptionControllerIDAndWorkflowProviderType()
		{
			var descriptor = new CusUSLVClearanceWorkflowDescriptor();
			CombineAssertions(() =>
			{
				AssertEquals(WorkflowDescriptors.CusUSLVClearanceWorkflowDescriptorCode, descriptor.Code);
				AssertEquals("US Customs Low Value Entries", descriptor.Description);
				AssertEquals(ControllerIDs.Customs.US.USLowValueEntries, descriptor.ControllerID);
				AssertEquals(typeof(CusUSLVClearance), descriptor.WorkflowProviderType);
			});
		}

		public void TestAdditionalWorkflowTriggerAction()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			var trigger = clearance.WorkflowItems.Triggers.AddNew();
			var triggerAction = trigger.ProcessTaskNotifications.AddNew();
			Assert(triggerAction.Lookups.WorkflowTriggerActionTypes.ContainsCode(WorkflowTriggerActionTypeConstants.Codes.SendReleaseMessage));
		}

		#region Overrides

		public override void TestDescription() => AssertEquals("US Customs Low Value Entries", WorkflowDescriptor.Description);

		public override void TestID() => AssertEquals("CUL", WorkflowDescriptor.Code);

		public override void TestRequiresBranch()
		{
			Assert(!WorkflowDescriptor.RequiresBranch);
		}

		public override void TestRequiresClient()
		{
			Assert(WorkflowDescriptor.RequiresClient);
		}

		public override void TestRequiresDepartment()
		{
			Assert(!WorkflowDescriptor.RequiresDepartment);
		}

		public override void TestRequiresPorts()
		{
			Assert(!WorkflowDescriptor.RequiresPort1);
			Assert(!WorkflowDescriptor.RequiresPort2);
		}

		public override void TestSubTypes()
		{
			Assert(true);
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties =>
			MessageRecipientPartyType.OrgProxy |
			MessageRecipientPartyType.Email |
			MessageRecipientPartyType.HVLVForwarder;

		public void TestMessageRecipientParty_ForHVLVForwarder_UsesOrgProxy()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			var hvlRecipient = WorkflowDescriptor.GetMessageRecipientParty(clearance, MessageRecipientPartyTypeList.Codes.HVLVForwarder).SingleOrDefault();

			AssertEquals("Because we're publishing internally, the recipient party for HVL should be the Org Proxy", GlbCompany.CurrentCompany.OrgProxy, hvlRecipient.Party);
		}

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			return new IWorkflowProvider[] { Factory.New<CusUSLVClearance>() };
		}

		public override void TestSupportsEventTracking()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsEventTracking);
		}

		protected override CodeDescriptionPair[] ExpectedAdditionalWorkflowTriggerActionTypes
		{
			get
			{
				return new[]
				{
					new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.SendReleaseMessage, WorkflowTriggerActionTypeConstants.Descriptions.SendReleaseMessage)
				};
			}
		}

		#endregion
	}

	[TestedType(typeof(CusUSLVClearance))]
	class CusUSLVClearenceWorkflowProviderTest : WorkflowProviderTest<CusUSLVClearance, CusUSLVClearanceProcessTaskCollection>
	{
		#region Implementation

		protected override ZString ExpectedWorkflowType
		{
			get { return WorkflowDescriptors.CusUSLVClearanceWorkflowDescriptorCode; }
		}

		#endregion
	}

	class CusUSLVClearenceCustomsMessageValidationSupporterTest : CustomsMessageValidationSupporterTest<CusUSLVClearance>
	{
		protected override IEnumerable<string> ExpectedTriggerTypes
		{
			get { yield return WorkflowTriggerActionTypeConstants.Codes.ValidateForCustomsMessaging; }
		}
	}
}
