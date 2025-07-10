using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(ContainerLoadPlanWorkflowDescriptor))]
	public class ContainerLoadPlanWorkflowDescriptorTest : WorkflowDescriptorTestCase<ContainerLoadPlanWorkflowDescriptor>
	{
		public override void TestDescription()
		{
			AssertEquals("Correct Desc", "Container Load Plan", WorkflowDescriptor.Description);
		}

		public override void TestID()
		{
			AssertEquals("Correct Code", WorkflowDescriptors.ContainerLoadPlanWorkflowDescriptorCode, WorkflowDescriptor.Code);
		}

		public override void TestRequiresBranch()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresBranch);
		}

		public override void TestRequiresClient()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresClient);
		}

		public override void TestRequiresDepartment()
		{
			AssertEquals(false, WorkflowDescriptor.RequiresDepartment);
		}

		public override void TestSupportsEventTracking()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsEventTracking);
		}

		public override void TestRequiresPorts()
		{
			AssertEquals(true, WorkflowDescriptor.RequiresPort1);
			AssertEquals(true, WorkflowDescriptor.RequiresPort2);
		}

		public override void TestSupportsScreenLayout()
		{
			AssertEquals(false, WorkflowDescriptor.SupportsScreenLayout);
		}

		protected override IWorkflowProvider[] GetParentsWithConfiguredOrganisationPartiesForTest()
		{
			return new IWorkflowProvider[]
			{
				ContainerLoadPlanHeaderWithOrganisationParties,
			};
		}

		protected CFSContainerLoadList ContainerLoadPlanHeaderWithOrganisationParties
		{
			get { return containerLoadPlanHeaderWithOrganisationParties ?? (containerLoadPlanHeaderWithOrganisationParties = GetOrgWithCommunicationModes()); }
		}
		CFSContainerLoadList containerLoadPlanHeaderWithOrganisationParties;

		CFSContainerLoadList GetOrgWithCommunicationModes()
		{
			var entity = Factory.NewWithValidTestData<CFSContainerLoadList>();
			entity.CLH_OH_LoadListParty = BookingPartyOrg.PK;

			return entity;
		}

		public override void TestSubTypes()
		{
			AssertEquals("1 sub types", 1, WorkflowDescriptor.SubTypeInformation.Length);
			AssertEquals("Sub Type 1 is Transport Mode", "Transport Mode", WorkflowDescriptor.SubTypeInformation[0].Description);
		}

		public override void TestSupportsWorkflowTemplates()
		{
			AssertEquals(true, WorkflowDescriptor.SupportsWorkflowTemplates);
		}

		#region TestSupportedMessageRecipientParties

		protected override CodeDescriptionPair[] ExpectedAdditionalWorkflowTriggerActionTypes
		{
			get
			{
				return new CodeDescriptionPair[] {
					new CodeDescriptionPair(WorkflowTriggerActionTypeConstants.Codes.SetField, WorkflowTriggerActionTypeConstants.Descriptions.SetField) };
			}
		}

		protected override MessageRecipientPartyType ExpectedSupportedMessageRecipientParties
		{
			get
			{
				return
					MessageRecipientPartyType.BookingParty |
					MessageRecipientPartyType.Email;
			}
		}

		#endregion

		public void TestValidationToolSettings()
		{
			AssertType<ContainerLoadPlanValidationToolSettings>(WorkflowDescriptor.ValidationToolSettings);
		}
	}
}
